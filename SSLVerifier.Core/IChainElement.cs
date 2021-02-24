using System;
using System.Security.Cryptography.X509Certificates;
using SSLVerifier.Core.Processor;

namespace SSLVerifier.Core {
    public interface IChainElement {
        String Name { get; }
        X509Certificate2 Certificate { get; }
        String NativeErrorString { get; }
        String PropagatedErrorString { get; }
        Boolean HasErrors { get; }
        Boolean HasWarnings { get; }
        X509ChainStatusFlags2 NativeErrors { get; }
        X509ChainStatusFlags2 PropagatedErrors { get; }
    }
}