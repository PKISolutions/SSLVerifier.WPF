using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using SSLVerifier.Core.Models;
using SSLVerifier.Core.Processor;

namespace SSLVerifier.Core.Default {
    public class ServerEntry : IServerObject {
        public ServerEntry(String serverAddress, Int32 port = 443) {
            ServerAddress = serverAddress;
            Port = port;
            Log = new ServerLogWriter();
        }
        public String ServerAddress { get; }
        public Int32 Port { get; }
        public IServerProxy Proxy { get; } = new ServerEntryProxy();
        public ServerStatusEnum ItemStatus { get; set; }
        public IServerLogWriter Log { get; set; }
        public ObservableCollection<String> SAN { get; } = new ObservableCollection<String>();
        public X509ChainStatusFlags2 ChainStatus { get; set; }
        public X509Certificate2 Certificate { get; set; }
        public ObservableCollection<TreeNode<IChainElement>> Tree { get; } = new ObservableCollection<TreeNode<IChainElement>>();
    }
}