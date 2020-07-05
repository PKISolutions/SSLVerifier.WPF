using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SSLVerifier.API.ModelObjects;
using SSLVerifier.Core;
using SSLVerifier.Core.Extensions;
using SSLVerifier.Core.Models;
using SSLVerifier.Core.Processor;

namespace SSLVerifier.API.MainLogic {
    class CertProcessor {
        const String SERVER_AUTHENTICATION = "1.3.6.1.5.5.7.3.1";
        const String RSA_ALG_ID            = "1.2.840.113549.1.1.1";
        const String EXT_EKU               = "2.5.29.37";
        const String EXT_SAN               = "2.5.29.17";

        readonly List<String> _visitedNames = new List<String>();
        readonly X509ChainStatusFlags2 _warningStatuses;
        static Int32 currentIndex;
        Boolean redirected, globalErrors, globalWarnings, shouldProceed = true;
        BackgroundWorker bworker;

        public CertProcessor() {
            _warningStatuses = X509ChainStatusFlags2.AboutExpire
                               | X509ChainStatusFlags2.WeakRsaPublicKey
                               | X509ChainStatusFlags2.HasWeakSignature;
        }

        ServerObjectWrapper Entry { get; set; }
        ServerObject NativeEntry => Entry.ServerObject;
        public ICertProcessorConfig Config { get; set; }

        void createRequest() {
            NativeEntry.Log.Progress = 10;
            Uri preUrl = new Uri("https://" + NativeEntry.ServerAddress);
            NativeEntry.Log.AppendLine("Generating connection string...");
            String preString = "https://" +
                preUrl.DnsSafeHost + ":" +
                NativeEntry.Port +
                preUrl.PathAndQuery;
            NativeEntry.Log.AppendLine($"Connection string is: {preString}");
            Entry.Request = (HttpWebRequest)WebRequest.Create(preString);
            Entry.Request.UserAgent = "SSL Certificate Verifier v1.3";
            if (NativeEntry.Proxy.UseProxy) {
                Entry.Request.Proxy = new WebProxy();
                if (NativeEntry.Proxy.UseAuthentication) {
                    var creds = new NetworkCredential(NativeEntry.Proxy.UserName, NativeEntry.Proxy.SecurePassword);
                    Entry.Request.Proxy.Credentials = creds;
                }
            }
            _visitedNames.Add(Entry.Request.Host.ToLower());
            Entry.Request.Timeout = 30000;
            Entry.Request.AllowAutoRedirect = true;
            getResponse();
        }
        void getResponse() {
            NativeEntry.Log.Progress = 15;
            NativeEntry.Log.AppendLine("Entering certificate validation callback function...");
            ServicePointManager.MaxServicePointIdleTime = 0;
            ServicePointManager.ServerCertificateValidationCallback = serverCertificateValidationCallback;
            if (!shouldProceed) {
                NativeEntry.Log.Progress = 100;
                ServicePointManager.ServerCertificateValidationCallback = null;
                return;
            }
            NativeEntry.Log.Progress = 40;
            try {
                Entry.Response = (HttpWebResponse)Entry.Request.GetResponse();
                NativeEntry.Log.Progress = 70;
            } catch (WebException e) {
                if (e.Message.Contains("(401)")) {
                    NativeEntry.Log.AppendLine("Server requested authorization (HTTP/401), cannot proceed.");
                } else if (e.Message.Contains("403")) {
                    NativeEntry.Log.AppendLine("Server requested authorization (HTTP/403), cannot proceed.");
                } else {
                    NativeEntry.Log.AppendLine($"An exception occured while attempting to connect to server: {e.Message}");
                    if (e.InnerException != null) {
                        NativeEntry.Log.AppendLine($"Details: {e.InnerException.Message}");
                    }
                    globalErrors = true;
                }
                NativeEntry.ChainStatus |= X509ChainStatusFlags2.CertificateNotFound;
            } finally {
                ServicePointManager.ServerCertificateValidationCallback = null;
            }
        }
        Boolean serverCertificateValidationCallback(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            Entry.InternalChain = new X509Chain(Config.AllowUserTrust);
            if (redirected) {
                if (_visitedNames.Contains(((HttpWebRequest)sender).Address.Host.ToLower())) {
                    NativeEntry.Log.AppendLine(
                        $"We are redirected to an already visited host: {((HttpWebRequest)sender).Address.Host}. Stop execution.");
                    shouldProceed = false;
                    ((HttpWebRequest)sender).AllowAutoRedirect = false;
                    return true;
                }
                _visitedNames.Add(((HttpWebRequest)sender).Address.Host);
                NativeEntry.Log.AppendLine("We are redirected. Entering the certificate validation callback function again.");
                NativeEntry.Log.AppendLine($"Redirected URL: {((HttpWebRequest)sender).Address.AbsoluteUri}");
                NativeEntry.ChainStatus = 0;
            } else {
                NativeEntry.Log.AppendLine($"Server returned {chain.ChainElements.Count} certificates.");
            }
            if (chain.ChainElements.Count > 1) {
                NativeEntry.Log.AppendLine("Dumping certificates:");
                for (Int32 index = 0; index < chain.ChainElements.Count; index++) {
                    NativeEntry.Log.AppendLine($"=============================== Certificate {index} ===============================");
                    NativeEntry.Log.AppendLine(chain.ChainElements[index].Certificate.ToString(true));
                    Entry.InternalChain.ChainPolicy.ExtraStore.Add(chain.ChainElements[index].Certificate);
                }
            }
            if (((Int32)sslPolicyErrors & (Int32)SslPolicyErrors.RemoteCertificateNameMismatch) == 0) {
                NativeEntry.ChainStatus |= X509ChainStatusFlags2.NameMismatch;
            }
            executeChain(chain);
            Entry.InternalChain.Reset();
            redirected = true;
            return true;
        }

        void executeChain(X509Chain chain) {
            NativeEntry.Log.AppendLine("Entering server certificate chain validation function...");
            if (Entry.Request.ServicePoint.Certificate == null) {
                Entry.InternalChain.Reset();
                NativeEntry.ItemStatus = ServerStatusEnum.Failed;
                NativeEntry.ChainStatus |= X509ChainStatusFlags2.CertificateNotFound;
                return;
            }
            X509Certificate2 cert = new X509Certificate2(Entry.Request.ServicePoint.Certificate);
            NativeEntry.Log.AppendLine("Leaf certificate issued to: " + cert.Subject);
            NativeEntry.Certificate = cert;
            processCert();
            // configure chaining engine
            if (Config.StrictUsage) {
                Entry.InternalChain.ChainPolicy.ApplicationPolicy.Add(new Oid(SERVER_AUTHENTICATION));
            }
            Boolean status = Entry.InternalChain.Build(cert);
            if (status) {
                NativeEntry.Log.AppendLine("Certificate chain successfully passed all checks.");
            } else {
                NativeEntry.Log.AppendLine("Certificate chaining engine reported some issues with the certificate.");
                foreach (X509ChainStatus chainStatus in Entry.InternalChain.ChainStatus) {
                    NativeEntry.Log.AppendLine(chainStatus.Status.ToString());
                }
            }
            grabInternalChain(chain);
            NativeEntry.Log.Progress += 3;
        }
        void extendedCertValidation(TreeNode<ChainElement> tree, X509ChainElement chainElement, Int32 index) {
            if (Config.CheckWeakPubKey) {
                if (chainElement.Certificate.PublicKey.Oid.Value == RSA_ALG_ID) {
                    if (chainElement.Certificate.PublicKey.Key.KeySize < Config.MinimumRsaPubKeyLength) {
                        addStatus(tree.Value, new X509ChainStatus2 { Status = X509ChainStatusFlags2.WeakRsaPublicKey });
                    }
                }
            }
            if (Config.CheckWeakPubKey) {
                Boolean isRoot = chainElement.Certificate.SubjectName.RawData.SequenceEqual(chainElement.Certificate.IssuerName.RawData);
                if (!isRoot) {
                    if (Config.WeakAlgorithms.Contains(chainElement.Certificate.SignatureAlgorithm.Value)) {
                        addStatus(tree.Value, new X509ChainStatus2 { Status = X509ChainStatusFlags2.HasWeakSignature });
                    }
                }
                if (chainElement.Certificate.PublicKey.Oid.Value == RSA_ALG_ID) {
                    if (chainElement.Certificate.PublicKey.Key.KeySize < Config.MinimumRsaPubKeyLength) {
                        addStatus(tree.Value, new X509ChainStatus2 { Status = X509ChainStatusFlags2.WeakRsaPublicKey });
                    }
                }
            }
            if ((chainElement.Certificate.NotAfter - DateTime.Now).Days <= Config.Threshold) {
                addStatus(tree.Value, new X509ChainStatus2 { Status = X509ChainStatusFlags2.AboutExpire });
            }
            if (index == 0) {
                if (!hasValidEKU(chainElement.Certificate)) {
                    if ((chainElement.Certificate.NotAfter - DateTime.Now).Days <= Config.Threshold) {
                        addStatus(tree.Value, new X509ChainStatus2 { Status = X509ChainStatusFlags2.NotValidForUsage });
                    }
                }
                X509Extension san = chainElement.Certificate.Extensions[EXT_SAN];
                if (san == null) {
                    addStatus(tree.Value, new X509ChainStatus2 {Status = X509ChainStatusFlags2.MissingAltNameExtension });
                }
            }
        }
        void processCert() {
            X509Extension san = NativeEntry.Certificate.Extensions[EXT_SAN];
            if (san != null) {
                NativeEntry.Log.AppendLine("Found Subject Alternative Names extension in the certificate.");
                NativeEntry.Log.AppendLine("Fetching SAN values:");
                NativeEntry.Log.AppendLine(san.Format(true));
                foreach (String name in san.Format(false)
                    .Split(new[] {", "}, StringSplitOptions.RemoveEmptyEntries)) {
                    NativeEntry.SAN.Add(name);
                }
            }
        }
        void grabInternalChain(X509Chain chain) {
            TreeNode<ChainElement> tree = Entry.Response == null
                ? new TreeNode<ChainElement>(new ChainElement { Name = Entry.Request.Address.AbsoluteUri, IsRoot = true })
                : new TreeNode<ChainElement>(new ChainElement { Name = Entry.Response.ResponseUri.AbsoluteUri, IsRoot = true });
            List<TreeNode<ChainElement>> tempList = new List<TreeNode<ChainElement>> { tree };
            for (Int32 index = chain.ChainElements.Count - 1; index >= 0; index--) {
                ChainElement temp = new ChainElement {
                    Certificate = Entry.InternalChain.ChainElements[index].Certificate,
                    Name = Entry.InternalChain.ChainElements[index].Certificate.GetNameInfo(X509NameType.SimpleName, false)
                };
                tree.AddChild(temp);
                temp.PropagatedErrors = tree.Value.NativeErrors;
                tree = tree.Children[0];
                addStatus(tree.Value, chain.ChainStatus.Select(x => new X509ChainStatus2(x)).ToArray());
                addStatus(tree.Value, Entry.InternalChain.ChainElements.Item(chain.ChainElements[index])
                    .ChainElementStatus.Select(x => new X509ChainStatus2(x)).ToArray());
                extendedCertValidation(tree, chain.ChainElements[index], index);
            }
            bworker.ReportProgress(0, new ReportObject { Action = "Add", Index = currentIndex, NewTree = tempList[0] });
        }
        static Boolean hasValidEKU(X509Certificate2 cert) {
            X509EnhancedKeyUsageExtension eku = (X509EnhancedKeyUsageExtension)cert.Extensions[EXT_EKU];
            return eku?.EnhancedKeyUsages[SERVER_AUTHENTICATION] != null;
        }
        void addStatus(ChainElement temp, params X509ChainStatus2[] status) {
            if (status == null) { return; }
            foreach (X509ChainStatus2 flag in status) {
                if (flag.Status != X509ChainStatusFlags2.NoError && (_warningStatuses & flag.Status) == 0) {
                    temp.HasErrors = globalErrors = true;
                }
                if ((_warningStatuses & flag.Status) > 0) {
                    temp.HasWarnings = globalWarnings = true;
                }
                temp.NativeErrors |= flag.Status;
            }
        }
        void changeStatCounter(IStatusCounter counters) {
            switch (NativeEntry.ItemStatus) {
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
                NativeEntry.ItemStatus = ServerStatusEnum.Pending;
            }
            if (globalErrors) {
                NativeEntry.ItemStatus = ServerStatusEnum.Failed;
            }
            // end routine
            if (!globalErrors && !globalWarnings) {
                NativeEntry.ItemStatus = ServerStatusEnum.Valid;
            }
        }
        public void StartScan(Object sender, DoWorkEventArgs EventArgs) {
            if (!(sender is BackgroundWorker worker)) { return; }

            bworker = worker;
            worker.ReportProgress(0);
            BackgroundObject background = (BackgroundObject)EventArgs.Argument;
            Int32 duration = 100 / background.Servers.Count;
            for (Int32 index = 0; index < background.Servers.Count; index++) {
                // if single scan is selected, process entries that are marked to process.
                if (background.SingleScan && !background.Servers[index].CanProcess) { continue; }
                currentIndex = index;
                worker.ReportProgress(duration * index);

                globalErrors = globalWarnings = false;
                ServerObject nativeObject = background.Servers[index];
                nativeObject.Log.Clear();
                nativeObject.Log.AppendLine("**************************************************************************");
                nativeObject.Log.AppendLine($"Processing '{nativeObject.ServerAddress}'");
                nativeObject.Log.AppendLine("**************************************************************************");
                nativeObject.Log.AppendLine($"Scan started: {DateTime.Now:dd-MM-yyyy HH:mm:ss}");
                // prepare properties
                worker.ReportProgress(0, new ReportObject { Action = "Clear", Index = index });

                using (Entry = new ServerObjectWrapper(nativeObject)) {
                    Entry.InternalChain = new X509Chain(!Config.AllowUserTrust);
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)Config.SslProtocolsToUse;
                    // execute main routine
                    createRequest();
                }

                // calculate resulting status of the object
                getEffectiveStatus();
                changeStatCounter(background.Counters);
                // release resources and close connections
                NativeEntry.Log.AppendLine("Finished!");
                NativeEntry.Log.Progress = 100;
                NativeEntry.Log.AppendLine("Scan ended: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                redirected = false;
            }
            worker.ReportProgress(100);
        }
    }
}
