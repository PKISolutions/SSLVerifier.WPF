using System;
using System.Security.Authentication;
using SysadminsLV.WPF.OfficeTheme.Toolkit.ViewModels;

namespace SSLVerifier.API.ModelObjects {
    class SslProtocolEnablerModel : ViewModelBase {
        Boolean m_enabled;
        SslProtocols protoValue;

        public SslProtocolEnablerModel(SslProtocols proto, Boolean enabled) {
            Enabled = enabled;
            ProtocolValue = proto;
        }

        public Boolean Enabled {
            get => m_enabled;
            set {
                m_enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }
        public String ProtocolName { get; private set; }

        public SslProtocols ProtocolValue {
            get => protoValue;
            set {
                protoValue = value;
                switch (protoValue) {
                    case SslProtocols.Ssl2:
                        ProtocolName = "SSL 2.0";
                        break;
                    case SslProtocols.Ssl3:
                        ProtocolName = "SSL 3.0";
                        break;
                    case SslProtocols.Tls:
                        ProtocolName = "TLS 1.0";
                        break;
                    case SslProtocols.Tls11:
                        ProtocolName = "TLS 1.1";
                        break;
                    case SslProtocols.Tls12:
                        ProtocolName = "TLS 1.2";
                        break;
                    case SslProtocols.Tls13:
                        ProtocolName = "TLS 1.3";
                        break;
                }
            }
        }
    }
}
