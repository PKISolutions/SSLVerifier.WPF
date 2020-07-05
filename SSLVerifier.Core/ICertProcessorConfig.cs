using System;
using System.Security.Authentication;

namespace SSLVerifier.Core {
    public interface ICertProcessorConfig {
        Boolean AllowUserTrust { get; }
        Boolean StrictUsage { get; }
        String[] WeakAlgorithms { get; }
        Boolean CheckWeakPubKey { get; }
        Int32 MinimumRsaPubKeyLength { get; }
        SslProtocols SslProtocolsToUse { get; }
        Int32 Threshold { get; }
    }
}