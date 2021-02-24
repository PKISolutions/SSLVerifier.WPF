using System;
using System.Security.Cryptography.X509Certificates;
using SSLVerifier.Core.Processor;

namespace SSLVerifier.Core.Models {
    class ChainElement : IChainElement {
        public String Name { get; internal set; }
        public X509Certificate2 Certificate { get; internal set; }
        public String NativeErrorString => NativeErrors.ToString();
        public String PropagatedErrorString => PropagatedErrors.ToString();
        public Boolean IsRoot { get; internal set; }
        public Boolean HasErrors { get; internal set; }
        public Boolean HasWarnings { get; internal set; }
        public X509ChainStatusFlags2 NativeErrors { get; internal set; }
        public X509ChainStatusFlags2 PropagatedErrors { get; internal set; }
    }
}