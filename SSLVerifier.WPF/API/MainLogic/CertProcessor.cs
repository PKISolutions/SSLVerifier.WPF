using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using SSLVerifier.API.Extensions;
using SSLVerifier.API.ModelObjects;
using SSLVerifier.Properties;

namespace SSLVerifier.API.MainLogic {
    class CertProcessor {
        readonly List<String> _visitedNames = new List<String>();
        readonly List<Int64> _warningStatuses;
        ServerObject srv;
        static Int32 currentIndex, threshold;
        readonly String _nl = Environment.NewLine;
        Boolean redirected, globalErrors, globalWarnings, shouldProceed = true;
        BackgroundWorker bworker;

        public CertProcessor() {
            _warningStatuses = new List<Int64> {
                (Int64)X509ChainStatusFlags2.AboutExpire,
                (Int64)X509ChainStatusFlags2.WeakRsaPublicKey,
                (Int64)X509ChainStatusFlags2.WeakSignatureAlgorithm
            };
        }

        void createRequest() {
            srv.ItemProgress = 10;
            Uri preUrl = new Uri("https://" + srv.ServerAddress);
            srv.Log.Add("Generating connection string..." + _nl);
            String preString = "https://" +
                preUrl.DnsSafeHost + ":" +
                srv.Port +
                preUrl.PathAndQuery;
            srv.Log.Add($"Connection string is: {preString}{_nl}");
            srv.TempRequest = (HttpWebRequest)WebRequest.Create(preString);
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)Settings.Default.SslProtocolsToUse;
            srv.TempRequest.UserAgent = "SSL Certificate Verifier v1.3";
            if (srv.Proxy.UseProxy) {
                srv.TempRequest.Proxy = new WebProxy();
                if (srv.Proxy.UseAuthentication) {
                    var creds = new NetworkCredential(srv.Proxy.UserName, srv.Proxy.SecurePassword);
                    srv.TempRequest.Proxy.Credentials = creds;
                }
            }
            _visitedNames.Add(srv.TempRequest.Host.ToLower());
            srv.TempRequest.Timeout = 30000;
            srv.TempRequest.AllowAutoRedirect = true;
            getResponse();
        }
        void getResponse() {
            srv.ItemProgress = 15;
            srv.Log.Add("Entering certificate validation callback function..." + _nl);
            ServicePointManager.MaxServicePointIdleTime = 0;
            ServicePointManager.ServerCertificateValidationCallback = serverCertificateValidationCallback;
            if (!shouldProceed) {
                srv.ItemProgress = 100;
                ServicePointManager.ServerCertificateValidationCallback = null;
                return;
            }
            srv.ItemProgress = 40;
            try {
                srv.TempResponse = (HttpWebResponse)srv.TempRequest.GetResponse();
                srv.ItemProgress = 70;
            } catch (WebException e) {
                if (e.Message.Contains("(401)")) {
                    srv.Log.Add($"Server requested authorization (HTTP/401), cannot proceed.{_nl}");
                } else if (e.Message.Contains("403")) {
                    srv.Log.Add($"Server requested authorization (HTTP/403), cannot proceed.{_nl}");
                } else {
                    srv.Log.Add($"An exception occured while attempting to connect to server: {e.Message}{_nl}");
                    globalErrors = true;
                }
                srv.ChainStatus |= X509ChainStatusFlags2.CertificateNotFound;
            } finally {
                ServicePointManager.ServerCertificateValidationCallback = null;
            }
        }
        Boolean serverCertificateValidationCallback(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            srv.TempChain = new X509Chain(Settings.Default.AllowUserTrust);
            if (redirected) {
                if (_visitedNames.Contains(((HttpWebRequest)sender).Address.Host.ToLower())) {
                    srv.Log.Add(
                        $"We are redirected to a already visited host: {((HttpWebRequest)sender).Address.Host}. Stop execution.{_nl}");
                    shouldProceed = false;
                    ((HttpWebRequest)sender).AllowAutoRedirect = false;
                    return true;
                }
                _visitedNames.Add(((HttpWebRequest)sender).Address.Host);
                srv.Log.Add("We are redirected. Entering the certificate validation callback function again." + _nl);
                srv.Log.Add($"Redirected URL: {((HttpWebRequest)sender).Address.AbsoluteUri} {_nl}");
                srv.ChainStatus = 0;
            } else {
                srv.Log.Add($"Server returned {chain.ChainElements.Count} certificates.{_nl}");
            }
            if (chain.ChainElements.Count > 1) {
                srv.Log.Add("Dumping certificates:" + _nl);
                for (Int32 index = 0; index < chain.ChainElements.Count; index++) {
                    srv.Log.Add($"=============================== Certificate {index} ==============================={_nl}");
                    srv.Log.Add(chain.ChainElements[index].Certificate.ToString(true) + _nl);
                    srv.TempChain.ChainPolicy.ExtraStore.Add(chain.ChainElements[index].Certificate);
                }
            }
            if (((Int32)sslPolicyErrors & (Int32)SslPolicyErrors.RemoteCertificateNameMismatch) == 0) {
                srv.ChainStatus |= X509ChainStatusFlags2.NameMismatch;
            }
            executeChain(chain);
            srv.TempChain.Reset();
            redirected = true;
            return true;
        }

        void executeChain(X509Chain chain) {
            srv.Log.Add($"Entering server certificate chain validation function...{_nl}");
            if (srv.TempRequest.ServicePoint.Certificate == null) {
                srv.TempChain.Reset();
                srv.ItemStatus = ServerStatusEnum.Failed;
                srv.ChainStatus |= X509ChainStatusFlags2.CertificateNotFound;
                return;
            }
            X509Certificate2 cert = new X509Certificate2(srv.TempRequest.ServicePoint.Certificate);
            srv.Log.Add("Leaf certificate issued to: " + cert.Subject + _nl);
            srv.Certificate = cert;
            processCert();
            // configure chaining engine
            if (Settings.Default.StrictUsage) {
                srv.TempChain.ChainPolicy.ApplicationPolicy.Add(new Oid("1.3.6.1.5.5.7.3.1"));
            }
            Boolean status = srv.TempChain.Build(cert);
            if (status) {
                srv.Log.Add("Certificate chain successfully passed all checks." + _nl);
            } else {
                srv.Log.Add($"Certificate chaining engine reported some issues with the certificate.{_nl}");
                foreach (X509ChainStatus chainStatus in srv.TempChain.ChainStatus) {
                    srv.Log.Add(chainStatus.Status + _nl);
                }
            }
            grabInternalChain(chain);
            srv.ItemProgress += 3;
        }
        void extendedCertValidation(TreeNode<ChainElement> tree, X509ChainElement chainElement, Int32 index) {
            if (Settings.Default.CheckWeakPubKey) {
                if (chainElement.Certificate.PublicKey.Oid.Value == "1.2.840.113549.1.1.1") {
                    if (chainElement.Certificate.PublicKey.Key.KeySize < Settings.Default.MinimumPubKeyLength) {
                        addStatus(tree.Value, new[] { new X509ChainStatus { Status = (X509ChainStatusFlags)X509ChainStatusFlags2.WeakRsaPublicKey } });
                    }
                }
            }
            if (Settings.Default.CheckWeakPubKey) {
                Boolean isRoot = chainElement.Certificate.SubjectName.RawData.SequenceEqual(chainElement.Certificate.IssuerName.RawData);
                if (!isRoot) {
                    if (Settings.Default.WeakAlgs.Contains(chainElement.Certificate.SignatureAlgorithm.Value)) {
                        addStatus(tree.Value, new[] { new X509ChainStatus { Status = (X509ChainStatusFlags)X509ChainStatusFlags2.WeakSignatureAlgorithm } });
                    }
                }
                if (chainElement.Certificate.PublicKey.Oid.Value == "1.2.840.113549.1.1.1") {
                    if (chainElement.Certificate.PublicKey.Key.KeySize < Settings.Default.MinimumPubKeyLength) {
                        addStatus(tree.Value, new[] { new X509ChainStatus { Status = (X509ChainStatusFlags)X509ChainStatusFlags2.WeakRsaPublicKey } });
                    }
                }
            }
            if ((chainElement.Certificate.NotAfter - DateTime.Now).Days <= threshold) {
                addStatus(tree.Value, new[] { new X509ChainStatus { Status = (X509ChainStatusFlags)X509ChainStatusFlags2.AboutExpire } });
            }
            if (index == 0) {
                if (!hasValidEKU(chainElement.Certificate)) {
                    if ((chainElement.Certificate.NotAfter - DateTime.Now).Days <= threshold) {
                        addStatus(tree.Value, new[] { new X509ChainStatus { Status = (X509ChainStatusFlags)X509ChainStatusFlags2.NotValidForUsage } });
                    }
                }
            }
        }
        void processCert() {
            X509Extension san = srv.Certificate.Extensions["2.5.29.17"];
            if (san != null) {
                srv.Log.Add($"Found Subject Alternative Names extension in the certificate.{_nl}");
                srv.Log.Add($"Fetching SAN values:{_nl}");
                srv.Log.Add(san.Format(true) + _nl);
                foreach (String name in san.Format(false).Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)) {
                    srv.SAN.Add(name);
                }
            }
        }
        void grabInternalChain(X509Chain chain) {
            TreeNode<ChainElement> tree = srv.TempResponse == null
                ? new TreeNode<ChainElement>(new ChainElement { Name = srv.TempRequest.Address.AbsoluteUri, IsRoot = true })
                : new TreeNode<ChainElement>(new ChainElement { Name = srv.TempResponse.ResponseUri.AbsoluteUri, IsRoot = true });
            List<TreeNode<ChainElement>> tempList = new List<TreeNode<ChainElement>> { tree };
            for (Int32 index = chain.ChainElements.Count - 1; index >= 0; index--) {
                ChainElement temp = new ChainElement {
                    Certificate = srv.TempChain.ChainElements[index].Certificate,
                    Name = stripName(srv.TempChain.ChainElements[index].Certificate.Subject)
                };
                tree.AddChild(temp);
                temp.PropagatedErrors = tree.Value.NativeErrors;
                tree = tree.Children[0];
                addStatus(tree.Value, chain.ChainStatus);
                addStatus(tree.Value, srv.TempChain.ChainElements.Item(chain.ChainElements[index]).ChainElementStatus);
                extendedCertValidation(tree, chain.ChainElements[index], index);
            }
            bworker.ReportProgress(0, new ReportObject { Action = "Add", Index = currentIndex, NewTree = tempList[0] });
        }
        static Boolean hasValidEKU(X509Certificate2 cert) {
            X509EnhancedKeyUsageExtension eku = (X509EnhancedKeyUsageExtension)cert.Extensions["2.5.29.37"];
            return eku?.EnhancedKeyUsages["1.3.6.1.5.5.7.3.1"] != null;
        }
        void addStatus(ChainElement temp, IEnumerable<X509ChainStatus> status) {
            if (status == null) { return; }
            foreach (X509ChainStatus flag in status) {
                if (flag.Status != X509ChainStatusFlags.NoError && !_warningStatuses.Contains((Int64)flag.Status)) {
                    temp.HasErrors = globalErrors = true;
                }
                if (_warningStatuses.Contains((Int64)flag.Status)) {
                    temp.HasWarnings = globalWarnings = true;
                }
                temp.NativeErrors |= (X509ChainStatusFlags2)flag.Status;
            }
        }
        static String stripName(String name) {
            Regex regex = new Regex("CN=([^,]+)");
            return regex.Match(name).Value.Replace("CN=", null);
        }
        void changeStatCounter(StatusCounter counters) {
            switch (srv.ItemStatus) {
                case ServerStatusEnum.Valid:
                    counters.Valid++;
                    break;
                case ServerStatusEnum.Pending:
                    counters.Pending++;
                    break;
                case ServerStatusEnum.Failed:
                    counters.Failed++;
                    break;
            }
            counters.Unknown--;
        }
        void getEffectiveStatus() {
            if (globalWarnings) {
                srv.ItemStatus = ServerStatusEnum.Pending;
            }
            if (globalErrors) {
                srv.ItemStatus = ServerStatusEnum.Failed;
            }
            // end routine
            if (!globalErrors && !globalWarnings) {
                srv.ItemStatus = ServerStatusEnum.Valid;
            }
        }
        public void StartScan(Object sender, DoWorkEventArgs EventArgs) {
            BackgroundWorker worker = sender as BackgroundWorker;
            if (worker == null) { return; }
            bworker = worker;
            worker.ReportProgress(0);
            BackgroundObject background = (BackgroundObject)EventArgs.Argument;
            threshold = background.Treshold;
            Int32 duration = 100 / background.Servers.Count;
            for (Int32 index = 0; index < background.Servers.Count; index++) {
                // if single scan is selected, process entries that are marked to process.
                if (background.SingleScan && !background.Servers[index].CanProcess) { continue; }
                currentIndex = index;
                worker.ReportProgress(duration * index);
                srv = background.Servers[index];
                globalErrors = globalWarnings = false;
                srv.Log.Clear();
                srv.Log.Add($"**************************************************************************{_nl}");
                srv.Log.Add($"Processing '{srv.ServerAddress}'{_nl}");
                srv.Log.Add($"**************************************************************************{_nl}");
                srv.Log.Add($"Scan started: {DateTime.Now:dd-MM-yyyy HH:mm:ss} {_nl}");
                // prepare properties
                srv.ItemProgress = 0;
                worker.ReportProgress(0, new ReportObject { Action = "Clear", Index = index });
                // execute main routine
                createRequest();
                // calculate resulting status of the object
                getEffectiveStatus();
                changeStatCounter(background.Counters);
                // release resources and close connections
                srv.Dispose();
                srv.Log.Add("Finished!" + _nl);
                srv.ItemProgress = 100;
                srv.Log.Add("Scan ended: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                redirected = false;
            }
            worker.ReportProgress(100);
        }
    }
}
