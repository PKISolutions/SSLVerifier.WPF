using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using SSLVerifier.Core;
using SSLVerifier.Core.Processor;
using SysadminsLV.WPF.OfficeTheme.Toolkit.ViewModels;

namespace SSLVerifier.API.ModelObjects {
    public class ServerListContainer : ViewModelBase {
        

        public ServerListContainer() {
            Servers.CollectionChanged += onServersCollectionChanged;
        }
        void onServersCollectionChanged(Object sender, NotifyCollectionChangedEventArgs e) {

            if (e.OldItems != null) {
                foreach (INotifyPropertyChanged item in e.OldItems)
                    item.PropertyChanged -= onItemPropertyChanged;
            }
            if (e.NewItems != null) {
                foreach (INotifyPropertyChanged item in e.NewItems)
                    item.PropertyChanged += onItemPropertyChanged;
            }

            OnPropertyChanged(nameof(Unknown));
            OnPropertyChanged(nameof(Valid));
            OnPropertyChanged(nameof(Pending));
            OnPropertyChanged(nameof(Failed));
        }
        void onItemPropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnPropertyChanged(nameof(Unknown));
            OnPropertyChanged(nameof(Valid));
            OnPropertyChanged(nameof(Pending));
            OnPropertyChanged(nameof(Failed));
        }

        public ObservableCollection<ServerObject> Servers { get; }
            = new ObservableCollection<ServerObject>();

        public Int32 Unknown => Servers.Count(x => x.ItemStatus == ServerStatusEnum.Unknown);
        public Int32 Valid => Servers.Count(x => x.ItemStatus == ServerStatusEnum.Valid);
        public Int32 Pending => Servers.Count(x => x.ItemStatus == ServerStatusEnum.Pending);
        public Int32 Failed => Servers.Count(x => x.ItemStatus == ServerStatusEnum.Failed);
    }
}