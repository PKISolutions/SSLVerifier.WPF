using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography;

namespace SSLVerifier.Core.Default {
    public class CertProcessorConfig : ICertProcessorConfig {
        readonly ISet<String> _sigAlgorithms = new HashSet<String>(StringComparer.OrdinalIgnoreCase);

        public CertProcessorConfig() {
            _sigAlgorithms.Add("1.2.840.113549.1.1.2");
            _sigAlgorithms.Add("1.2.840.113549.1.1.3");
            _sigAlgorithms.Add("1.2.840.113549.1.1.4");
            _sigAlgorithms.Add("1.2.840.113549.1.1.5");

            CheckWeakPubKey = true;
            MinimumRsaPubKeyLength = 2048;
            SslProtocolsToUse = SslProtocols.Tls11 | SslProtocols.Tls12;
            Threshold = 30;
        }

        public Boolean AllowUserTrust { get; set; }
        public Boolean StrictUsage { get; set; }
        public OidCollection WeakAlgorithms {
            get {
                var col = new OidCollection();
                foreach (String alg in _sigAlgorithms) {
                    col.Add(new Oid(alg));
                }

                return col;
            }
        }
        public Boolean CheckWeakPubKey { get; set; }
        public Int32 MinimumRsaPubKeyLength { get; set; }
        public SslProtocols SslProtocolsToUse { get; set; }
        public Int32 Threshold { get; set; }
        public Boolean SearchCT { get; set; }

        public Boolean AddWeakAlgorithm(String algID) {
            return _sigAlgorithms.Add(new Oid(algID).Value);
        }
        public Boolean RemoveWeakAlgorithm(String algID) {
            return _sigAlgorithms.Remove(new Oid(algID).Value);
        }
    }
}