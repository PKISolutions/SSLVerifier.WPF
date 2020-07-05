using System;
using SSLVerifier.API.ViewModels;
using SSLVerifier.Core;

namespace SSLVerifier.API.ModelObjects {
    class StatusCounter : ViewModelBase, IStatusCounter {
        Int32 unknown, valid, pending, failed;

        public Int32 Unknown {
            get => unknown;
            set {
                unknown = value;
                OnPropertyChanged(nameof(Unknown));
            }
        }
        public Int32 Valid {
            get => valid;
            set {
                valid = value;
                OnPropertyChanged(nameof(Valid));
            }
        }
        public Int32 Pending {
            get => pending;
            set {
                pending = value;
                OnPropertyChanged(nameof(Pending));
            }
        }
        public Int32 Failed {
            get => failed;
            set {
                failed = value;
                OnPropertyChanged(nameof(Failed));
            }
        }

        public void Refresh() {
            Valid = Pending = Failed = 0;
        }
        public void Refresh(Int32 count) {
            Unknown = count;
            Refresh();
        }
    }
}
