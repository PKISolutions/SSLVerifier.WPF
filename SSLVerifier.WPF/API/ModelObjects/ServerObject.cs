using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using SSLVerifier.Core;
using SSLVerifier.Core.Models;
using SSLVerifier.Core.Processor;
using SysadminsLV.WPF.OfficeTheme.Toolkit.ViewModels;

namespace SSLVerifier.API.ModelObjects {
    public class ServerObject : ViewModelBase, IServerObject {
        X509Certificate2 cert;
        String name;
        Int32 port;
        ServerStatusEnum status;

        public ServerObject() {
            status = ServerStatusEnum.Unknown;
            SAN = new ObservableCollection<String>();
            Log = new ServerLogWriter();
            Tree = new ObservableCollection<TreeNode<IChainElement>>();
            Proxy = new ProxyObject();
        }
        public ServerObject(ProxyObject proxy) : this() {
            Proxy = proxy;
        }

        public String ServerAddress {
            get => name;
            set {
                name = value;
                OnPropertyChanged(nameof(ServerAddress));
            }
        }
        public Int32 Port {
            get => port;
            set {
                port = value;
                OnPropertyChanged(nameof(Port));
            }
        }
        public IServerProxy Proxy { get; set; }
        [JsonIgnore]
        public ServerStatusEnum ItemStatus {
            get => status;
            set {
                status = value;
                OnPropertyChanged(nameof(ItemStatus));
            }
        }
        [JsonIgnore]
        public IServerLogWriter Log { get; }
        [JsonIgnore]
        public String ValidFrom => Certificate?.NotBefore.ToShortDateString();

        [JsonIgnore]
        public String ValidTo => Certificate?.NotAfter.ToShortDateString();

        [JsonIgnore]
        public Int32 DaysLeft => Certificate == null ? 0 : (Certificate.NotAfter - DateTime.Now).Days;

        [JsonIgnore]
        public ObservableCollection<String> SAN { get; }
        [JsonIgnore]
        public X509ChainStatusFlags2 ChainStatus { get; set; }
        [JsonIgnore]
        public X509Certificate2 Certificate {
            get => cert;
            set {
                cert = value;
                OnPropertyChanged(nameof(Certificate));
                OnPropertyChanged(nameof(ValidFrom));
                OnPropertyChanged(nameof(ValidTo));
                OnPropertyChanged(nameof(DaysLeft));
            }
        }
        [JsonIgnore]
        public ObservableCollection<TreeNode<IChainElement>> Tree { get; }
        [JsonIgnore]
        public Boolean CanProcess { get; set; }
        
        public override String ToString() {
            return Certificate == null
                ? "\"" + ServerAddress + "\"," + "\"" + Port + "\",,,,,"
                : "\"" + ServerAddress + "\"," + "\"" + Port + "\"," + "\"" + Certificate.Subject + "\"," + "\"" +
                Certificate.NotBefore + "\"," + "\"" + Certificate.NotAfter + "\"," + "\"" + ItemStatus + "\"";
        }

        public override Boolean Equals(Object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((ServerObject)obj);
        }
        public override Int32 GetHashCode() {
            unchecked {
                return (name.GetHashCode() * 397) ^ port;
            }
        }
        protected Boolean Equals(ServerObject other) {
            return String.Equals(name, other.name, StringComparison.OrdinalIgnoreCase) && port == other.port;
        }
    }
}
