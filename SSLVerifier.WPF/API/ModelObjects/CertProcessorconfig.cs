using System;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using SSLVerifier.Core;
using SSLVerifier.Properties;

namespace SSLVerifier.API.ModelObjects {
    class CertProcessorConfig : ICertProcessorConfig {

        public CertProcessorConfig() {
            AllowUserTrust = Settings.Default.AllowUserTrust;
            StrictUsage = Settings.Default.StrictUsage;
            if (Settings.Default.WeakAlgs != null) {
                foreach (String algID in Settings.Default.WeakAlgs.OfType<String>()) {
                    WeakAlgorithms.Add(new Oid(algID));
                }
            }
            
            CheckWeakPubKey = Settings.Default.CheckWeakPubKey;
            MinimumRsaPubKeyLength = Settings.Default.MinimumRsaPubKeyLength;
            SslProtocolsToUse = (SslProtocols)Settings.Default.SslProtocolsToUse;
            SearchCT = Settings.Default.SearchCT;
        }

        public Boolean AllowUserTrust { get; }
        public Boolean StrictUsage { get; }
        public OidCollection WeakAlgorithms { get; } = new OidCollection();
        public Boolean CheckWeakPubKey { get; }
        public Int32 MinimumRsaPubKeyLength { get; }
        public SslProtocols SslProtocolsToUse { get; }
        public Int32 Threshold { get; set; }
        public Boolean SearchCT { get; set; }
    }
}