using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using SSLVerifier.Core.Models;
using SSLVerifier.Core.Processor;

namespace SSLVerifier.Core {
    public interface IServerObject {
        String ServerAddress { get; }
        Int32 Port { get; }
        IServerProxy Proxy { get; }
        ServerStatusEnum ItemStatus { get; set; }
        IServerLogWriter Log { get; }
        ObservableCollection<String> SAN { get; }
        X509ChainStatusFlags2 ChainStatus { get; set; }
        X509Certificate2 Certificate { get; set; }
        ObservableCollection<TreeNode<IChainElement>> Tree { get; }
    }
}