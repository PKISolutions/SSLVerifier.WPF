using System;
using System.Security.Authentication;
using System.Security.Cryptography;

namespace SSLVerifier.Core {
    public interface ICertProcessorConfig {
        Boolean AllowUserTrust { get; }
        Boolean StrictUsage { get; }
        OidCollection WeakAlgorithms { get; }
        Boolean CheckWeakPubKey { get; }
        Int32 MinimumRsaPubKeyLength { get; }
        SslProtocols SslProtocolsToUse { get; }
        Int32 Threshold { get; }
        Boolean SearchCT { get; }
    }
}