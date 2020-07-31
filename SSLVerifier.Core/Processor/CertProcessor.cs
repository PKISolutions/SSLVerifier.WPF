using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using SSLVerifier.Core.CertTransparency;
using SSLVerifier.Core.Extensions;
using SSLVerifier.Core.Models; //using SSLVerifier.API.ModelObjects;

namespace SSLVerifier.Core.Processor {
    public class CertProcessor {
        const String SERVER_AUTHENTICATION = "1.3.6.1.5.5.7.3.1";
        const String RSA_ALG_ID = "1.2.840.113549.1.1.1";
        const String EXT_EKU = "2.5.29.37";
        const String EXT_SAN = "2.5.29.17";

        static readonly Object _lock = new Object();
        static readonly IDictionary<String, CertProcessor> _syncTable = new Dictionary<String, CertProcessor>(StringComparer.OrdinalIgnoreCase);
        static SynchronizationContext syncContext;
        readonly ISet<String> _visitedNames = new HashSet<String>(StringComparer.OrdinalIgnoreCase);
        readonly X509ChainStatusFlags2 _warningStatuses;
        Boolean redirected, globalErrors, globalWarnings, shouldProceed = true;

        public CertProcessor(ICertProcessorConfig config) {
            Config = config;
            _warningStatuses = X509ChainStatusFlags2.AboutExpire
                               | X509ChainStatusFlags2.WeakRsaPublicKey
                               | X509ChainStatusFlags2.HasWeakSignature;
        }

        ServerObjectWrapper Entry { get; set; }
        IServerObject NativeEntry => Entry.ServerObject;
        public ICertProcessorConfig Config { get; }

        async Task createRequest() {
            NativeEntry.Log.Progress = 10;
            Uri preUrl = new Uri("https://" + NativeEntry.ServerAddress);
            NativeEntry.Log.AppendLine("Generating connection string...");
            String preString = "https://" +
                preUrl.DnsSafeHost + ":" +
                NativeEntry.Port +
                preUrl.PathAndQuery;
            NativeEntry.Log.AppendLine($"Connection string is: {preString}");
            Entry.Request = (HttpWebRequest)WebRequest.Create(preString);
            lock (_lock) {
                _syncTable.Add(Entry.Request.RequestUri.ToString(), this);
            }
            Entry.Request.UserAgent = "SSL Certificate Verifier v1.3";
            if (NativeEntry.Proxy.UseProxy) {
                Entry.Request.Proxy = new WebProxy();
                if (NativeEntry.Proxy.UseAuthentication) {
                    var creds = new NetworkCredential(NativeEntry.Proxy.UserName, NativeEntry.Proxy.SecurePassword);
                    Entry.Request.Proxy.Credentials = creds;
                }
            }
            _visitedNames.Add(Entry.Request.Host);
            Entry.Request.Timeout = 30000;
            Entry.Request.AllowAutoRedirect = true;
            await getResponse();
            lock (_lock) {
                _syncTable.Remove(Entry.Request.RequestUri.ToString());
            }
        }
        async Task getResponse() {
            NativeEntry.Log.Progress = 15;
            NativeEntry.Log.AppendLine("Entering certificate validation callback function...");
            ServicePointManager.MaxServicePointIdleTime = 0;
            if (ServicePointManager.ServerCertificateValidationCallback == null) {
                ServicePointManager.ServerCertificateValidationCallback = serverCertificateValidationCallback;
            }

            if (!shouldProceed) {
                NativeEntry.Log.Progress = 100;
                ServicePointManager.ServerCertificateValidationCallback = null;
                return;
            }
            NativeEntry.Log.Progress = 40;
            try {
                Entry.Response = (HttpWebResponse)await Entry.Request.GetResponseAsync();
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
        static Boolean serverCertificateValidationCallback(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            lock (_lock) {
                var request = (HttpWebRequest)sender;
                // normally, the key is presented, but the call to CT may mess this, so silently skip it.
                if (!_syncTable.ContainsKey(request.RequestUri.ToString())) {
                    return sslPolicyErrors == SslPolicyErrors.None && chain.ChainStatus.All(x => x.Status == X509ChainStatusFlags.NoError);
                }
                CertProcessor processor = _syncTable[request.RequestUri.ToString()];
                ServerObjectWrapper entry = processor.Entry;
                ICertProcessorConfig config = processor.Config;

                entry.InternalChain = new X509Chain(!config.AllowUserTrust);
                if (processor.redirected) {
                    if (!processor._visitedNames.Add(request.Address.Host)) {
                        entry.ServerObject.Log.AppendLine(
                            $"We are redirected to an already visited host: {request.Address.Host}. Stop execution.");
                        processor.shouldProceed = false;
                        request.AllowAutoRedirect = false;
                        return true;
                    }
                    entry.ServerObject.Log.AppendLine("We are redirected. Entering the certificate validation callback function again.");
                    entry.ServerObject.Log.AppendLine($"Redirected URL: {((HttpWebRequest)sender).Address.AbsoluteUri}");
                    entry.ServerObject.ChainStatus = 0;
                } else {
                    entry.ServerObject.Log.AppendLine($"Server returned {chain.ChainElements.Count} certificates.");
                }
                if (chain.ChainElements.Count > 1) {
                    entry.ServerObject.Log.AppendLine("Dumping certificates:");
                    for (Int32 index = 0; index < chain.ChainElements.Count; index++) {
                        entry.ServerObject.Log.AppendLine($"=============================== Certificate {index} ===============================");
                        try {
                            // there is a bug in .NET 4.5 on Windows 7 when calling ToString(true) on ECC cert
                            entry.ServerObject.Log.AppendLine(chain.ChainElements[index].Certificate.ToString(true));
                        } catch {
                            // fallback to brief dump
                            entry.ServerObject.Log.AppendLine(chain.ChainElements[index].Certificate.ToString(false));
                        }
                        entry.InternalChain.ChainPolicy.ExtraStore.Add(chain.ChainElements[index].Certificate);
                    }
                }
                Boolean hasNameMismatch = ((Int32)sslPolicyErrors & (Int32)SslPolicyErrors.RemoteCertificateNameMismatch) > 0;
                processor.executeChain(chain);
                if (hasNameMismatch) {
                    processor.addStatus(entry.ServerObject.Tree.Last().Flatten().Last(), new X509ChainStatus2 { Status = X509ChainStatusFlags2.NameMismatch });
                }
                entry.InternalChain.Reset();
                processor.redirected = true;
            }
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
            readAltNames();
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
        void grabInternalChain(X509Chain chain) {
            TreeNode<IChainElement> tree = Entry.Response == null
                ? new TreeNode<IChainElement>(new ChainElement { Name = Entry.Request.Address.AbsoluteUri, IsRoot = true })
                : new TreeNode<IChainElement>(new ChainElement { Name = Entry.Response.ResponseUri.AbsoluteUri, IsRoot = true });
            List<TreeNode<IChainElement>> tempList = new List<TreeNode<IChainElement>> { tree };
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

            // this is necessary for stuff like WPF, because of an attempt to update CollectionView in background thread.
            if (syncContext == null) {
                NativeEntry.Tree.Add(tempList[0]);
            } else {
                syncContext.Send(x => NativeEntry.Tree.Add(tempList[0]), null);
            }
        }
        void extendedCertValidation(TreeNode<IChainElement> tree, X509ChainElement chainElement, Int32 index) {
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
                    if (Config.WeakAlgorithms.Contains2(chainElement.Certificate.SignatureAlgorithm)) {
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
                    addStatus(tree.Value, new X509ChainStatus2 { Status = X509ChainStatusFlags2.MissingAltNameExtension });
                }

                // Apple policy: up to 398 days after Sep 1 2020
                var dt = new DateTime(2020, 9, 1);
                if (chainElement.Certificate.NotBefore > dt && (chainElement.Certificate.NotAfter - chainElement.Certificate.NotBefore).Days > 398) {
                    addStatus(tree.Value, new X509ChainStatus2 { Status = X509ChainStatusFlags2.TooLongValidity });
                }

                if (Config.SearchCT) {
                    var prov = new CertShCTProvider();
                    Boolean status = prov.CertExist(NativeEntry.ServerAddress, chainElement.Certificate);
                    if (!status) {
                        addStatus(tree.Value, new X509ChainStatus2 { Status = X509ChainStatusFlags2.NotInTransparencyLog });
                    }
                }
            }
        }
        void readAltNames() {
            X509Extension san = NativeEntry.Certificate.Extensions[EXT_SAN];
            if (san != null) {
                NativeEntry.Log.AppendLine("Found Subject Alternative Names extension in the certificate.");
                NativeEntry.Log.AppendLine("Fetching SAN values:");
                NativeEntry.Log.AppendLine(san.Format(true));
                foreach (String name in san.Format(false)
                    .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)) {
                    NativeEntry.SAN.Add(name);
                }
            }
        }
        static Boolean hasValidEKU(X509Certificate2 cert) {
            X509EnhancedKeyUsageExtension eku = (X509EnhancedKeyUsageExtension)cert.Extensions[EXT_EKU];
            return eku?.EnhancedKeyUsages[SERVER_AUTHENTICATION] != null;
        }
        void addStatus(IChainElement temp, params X509ChainStatus2[] status) {
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
        public void StartScan(IServerObject server) {
            StartScanAsync(server, null).Wait();
        }
        public async Task StartScanAsync(IServerObject server, SynchronizationContext ctx) {
            if (server == null) {
                return;
            }
            syncContext = ctx;
            lock (_lock) {
                _syncTable.Clear();
            }
            _visitedNames.Clear();
            redirected = globalErrors = globalWarnings = false;
            server.Log.Clear();
            server.Log.AppendLine("**************************************************************************");
            server.Log.AppendLine($"Processing '{server.ServerAddress}'");
            server.Log.AppendLine("**************************************************************************");
            server.Log.AppendLine($"Scan started: {DateTime.Now:dd-MM-yyyy HH:mm:ss}");

            using (Entry = new ServerObjectWrapper(server)) {
                Entry.InternalChain = new X509Chain(!Config.AllowUserTrust);
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)Config.SslProtocolsToUse;
                // execute main routine
                await createRequest();
            }

            // calculate resulting status of the object
            getEffectiveStatus();
            // release resources and close connections
            NativeEntry.Log.AppendLine("Finished!");
            NativeEntry.Log.Progress = 100;
            NativeEntry.Log.AppendLine("Scan ended: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
            redirected = false;
        }
    }
}
