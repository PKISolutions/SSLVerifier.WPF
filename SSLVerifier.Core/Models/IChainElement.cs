using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using SSLVerifier.Core.Processor;

namespace SSLVerifier.Core.Models {
    public interface IChainElement {
        String Name { get; set; }
        X509Certificate2 Certificate { get; set; }
        IChainElement Parent { get; set; }
        String NativeErrorString { get; }
        String PropagatedErrorString { get; }
        ObservableCollection<IChainElement> Child { get; }
        Boolean IsRoot { get; set; }
        Boolean HasErrors { get; set; }
        Boolean HasWarnings { get; set; }
        X509ChainStatusFlags2 NativeErrors { get; set; }
        X509ChainStatusFlags2 PropagatedErrors { get; set; }
    }
}