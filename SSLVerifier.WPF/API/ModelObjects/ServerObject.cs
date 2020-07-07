using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using SSLVerifier.Core;
using SSLVerifier.Core.Models;
using SSLVerifier.Core.Processor;
using SysadminsLV.WPF.OfficeTheme.Toolkit.ViewModels;

namespace SSLVerifier.API.ModelObjects {
    [XmlType(AnonymousType = true)]
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

        [XmlElement]
        public String ServerAddress {
            get => name;
            set {
                name = value;
                OnPropertyChanged(nameof(ServerAddress));
            }
        }
        [XmlElement]
        public Int32 Port {
            get => port;
            set {
                port = value;
                OnPropertyChanged(nameof(Port));
            }
        }
        public IServerProxy Proxy { get; set; }
        [XmlIgnore]
        public ServerStatusEnum ItemStatus {
            get => status;
            set {
                status = value;
                OnPropertyChanged(nameof(ItemStatus));
            }
        }
        [XmlIgnore]
        public IServerLogWriter Log { get; }
        [XmlIgnore]
        public String ValidFrom => Certificate?.NotBefore.ToShortDateString();

        [XmlIgnore]
        public String ValidTo => Certificate?.NotAfter.ToShortDateString();

        [XmlIgnore]
        public Int32 DaysLeft => Certificate == null ? 0 : (Certificate.NotAfter - DateTime.Now).Days;

        [XmlIgnore]
        public ObservableCollection<String> SAN { get; }
        [XmlIgnore]
        public X509ChainStatusFlags2 ChainStatus { get; set; }
        [XmlIgnore]
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
        [XmlIgnore]
        public ObservableCollection<TreeNode<IChainElement>> Tree { get; }
        [XmlIgnore]
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
