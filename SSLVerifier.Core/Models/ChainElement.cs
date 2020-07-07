using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using SSLVerifier.Core.Processor;

namespace SSLVerifier.Core.Models {
    public class ChainElement : IChainElement {
        public String Name { get; set; }
        public X509Certificate2 Certificate { get; set; }
        public IChainElement Parent { get; set; }
        public String NativeErrorString { get; }
        public String PropagatedErrorString { get; }
        public ObservableCollection<IChainElement> Child { get; }
            = new ObservableCollection<IChainElement>();
        public Boolean IsRoot { get; set; }
        public Boolean HasErrors { get; set; }
        public Boolean HasWarnings { get; set; }
        public X509ChainStatusFlags2 NativeErrors { get; set; }
        public X509ChainStatusFlags2 PropagatedErrors { get; set; }
    }
}