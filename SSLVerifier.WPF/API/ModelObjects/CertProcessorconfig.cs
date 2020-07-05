using System;
using System.Linq;
using System.Security.Authentication;
using SSLVerifier.Core;
using SSLVerifier.Properties;

namespace SSLVerifier.API.ModelObjects {
    class CertProcessorConfig : ICertProcessorConfig {

        public CertProcessorConfig() {
            AllowUserTrust = Settings.Default.AllowUserTrust;
            StrictUsage = Settings.Default.StrictUsage;
            WeakAlgorithms = Settings.Default.WeakAlgs?.OfType<String>().ToArray();
            CheckWeakPubKey = Settings.Default.CheckWeakPubKey;
            MinimumRsaPubKeyLength = Settings.Default.MinimumRsaPubKeyLength;
            SslProtocolsToUse = (SslProtocols)Settings.Default.SslProtocolsToUse;
        }

        public Boolean AllowUserTrust { get; }
        public Boolean StrictUsage { get; }
        public String[] WeakAlgorithms { get; }
        public Boolean CheckWeakPubKey { get; }
        public Int32 MinimumRsaPubKeyLength { get; }
        public SslProtocols SslProtocolsToUse { get; }
        public Int32 Threshold { get; set; }
    }
}