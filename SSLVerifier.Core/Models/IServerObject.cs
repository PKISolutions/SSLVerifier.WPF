using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using SSLVerifier.Core.Processor;

namespace SSLVerifier.Core.Models {
    public interface IServerObject {
        String ServerAddress { get; }
        Int32 Port { get; }
        IServerProxy Proxy { get; }
        ServerStatusEnum ItemStatus { set; }
        IServerLogWriter Log { get; }
        ObservableCollection<String> SAN { get; }
        X509ChainStatusFlags2 ChainStatus { get; set; }
        X509Certificate2 Certificate { get; set; }
        ObservableCollection<TreeNode<IChainElement>> Tree { get; }
    }
}