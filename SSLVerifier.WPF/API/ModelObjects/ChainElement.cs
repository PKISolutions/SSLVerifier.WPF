using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using SSLVerifier.Core;
using SSLVerifier.Core.Models;
using SSLVerifier.Core.Processor;
using SysadminsLV.WPF.OfficeTheme.Toolkit.ViewModels;

namespace SSLVerifier.API.ModelObjects {
    public sealed class ChainElement : ViewModelBase, IChainElement {
        String name;
        X509Certificate2 cert;
        IChainElement parent;

        public ChainElement() {
            Child = new ObservableCollection<IChainElement>();
        }

        public String Name {
            get => name;
            set {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public X509Certificate2 Certificate {
            get => cert;
            set {
                cert = value;
                OnPropertyChanged(nameof(Certificate));
            }
        }
        public IChainElement Parent {
            get => parent;
            set {
                parent = value;
                OnPropertyChanged(nameof(Parent));
            }
        }
        public String NativeErrorString => NativeErrors.ToString().Replace(", ", Environment.NewLine);
        public String PropagatedErrorString => PropagatedErrors.ToString().Replace(", ", Environment.NewLine);
        public ObservableCollection<IChainElement> Child { get; set; }
        public Boolean IsRoot { get; set; }
        public Boolean HasErrors { get; set; }
        public Boolean HasWarnings { get; set; }
        public X509ChainStatusFlags2 NativeErrors { get; set; }
        public X509ChainStatusFlags2 PropagatedErrors { get; set; }
    }
}
