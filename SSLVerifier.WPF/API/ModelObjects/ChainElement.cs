using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using SSLVerifier.API.MainLogic;
using SSLVerifier.API.ViewModels;

namespace SSLVerifier.API.ModelObjects {
    public sealed class ChainElement : ViewModelBase {
        String name;
        X509Certificate2 cert;
        ChainElement parent;

        public ChainElement() {
            Child = new ObservableCollection<ChainElement>();
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
        public ChainElement Parent {
            get => parent;
            set {
                parent = value;
                OnPropertyChanged(nameof(Parent));
            }
        }
        public String NativeErrorString => NativeErrors.ToString().Replace(", ", Environment.NewLine);
        public String PropagatedErrorString => PropagatedErrors.ToString().Replace(", ", Environment.NewLine);
        public ObservableCollection<ChainElement> Child { get; set; }
        public Boolean IsRoot { get; set; }
        public Boolean HasErrors { get; set; }
        public Boolean HasWarnings { get; set; }
        public X509ChainStatusFlags2 NativeErrors { get; set; }
        public X509ChainStatusFlags2 PropagatedErrors { get; set; }
    }
}
