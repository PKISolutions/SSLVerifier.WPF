using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Windows.Input;
using SSLVerifier.API.ModelObjects;
using SSLVerifier.Properties;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SSLVerifier.API.ViewModels {
    class SettingsVM : ViewModelBase {
        Boolean strictEKU, userTrust, checkWeakAlgs, checkWeakPubKeyLength;
        Boolean? dialogResult;
        String algId;
        Int32 minRsaKeyLength, selectedIndex;
        Oid selectedAlgId;

        public SettingsVM() {
            Settings.Default.Reload();
            WeakSigAlgs = new ObservableCollection<Oid>();
            SslProtocolsToUse = new ObservableCollection<SslProtocolEnablerModel>();
            initCommands();
            initProps();
        }

        void initCommands() {
            AddWeakAlgCommand = new RelayCommand(addAlgId, canAddAlgId);
            RemoveWeakAlgCommand = new RelayCommand(removeAlgId, canRemoveAlgId);
            SaveCommand = new RelayCommand(save);
        }
        void initProps() {
            StrictEKU = Settings.Default.StrictUsage;
            AllowUserTrustStore = Settings.Default.AllowUserTrust;
            CheckWeakRsaPubKey = Settings.Default.CheckWeakPubKey;
            CheckWeakSignatureAlgorithms = Settings.Default.CheckWeakAlgs;
            MinimumRsaPublicKeyLength = Settings.Default.MinimumRsaPubKeyLength;
            initProtos();
            initAlgs();
        }
        void initProtos() {
            Boolean enabled = (Settings.Default.SslProtocolsToUse & (Int32)SslProtocols.Ssl2) > 0;
            SslProtocolsToUse.Add(new SslProtocolEnablerModel(SslProtocols.Ssl2, enabled));
            enabled = (Settings.Default.SslProtocolsToUse & (Int32)SslProtocols.Ssl3) > 0;
            SslProtocolsToUse.Add(new SslProtocolEnablerModel(SslProtocols.Ssl3, enabled));
            enabled = (Settings.Default.SslProtocolsToUse & (Int32)SslProtocols.Tls) > 0;
            SslProtocolsToUse.Add(new SslProtocolEnablerModel(SslProtocols.Tls, enabled));
            enabled = (Settings.Default.SslProtocolsToUse & (Int32)SslProtocols.Tls11) > 0;
            SslProtocolsToUse.Add(new SslProtocolEnablerModel(SslProtocols.Tls11, enabled));
            enabled = (Settings.Default.SslProtocolsToUse & (Int32)SslProtocols.Tls12) > 0;
            SslProtocolsToUse.Add(new SslProtocolEnablerModel(SslProtocols.Tls12, enabled));
        }
        void initAlgs() {
            foreach (String alg in Settings.Default.WeakAlgs) {
                WeakSigAlgs.Add(new Oid(alg));
            }
        }

        public ICommand AddWeakAlgCommand { get; set; }
        public ICommand RemoveWeakAlgCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public ObservableCollection<Oid> WeakSigAlgs { get; }
        public ObservableCollection<SslProtocolEnablerModel> SslProtocolsToUse { get; }

        public Boolean StrictEKU {
            get => strictEKU;
            set {
                strictEKU = value;
                OnPropertyChanged(nameof(StrictEKU));
            }
        }
        public Boolean AllowUserTrustStore {
            get => userTrust;
            set {
                userTrust = value;
                OnPropertyChanged(nameof(AllowUserTrustStore));
            }
        }
        public Boolean CheckWeakSignatureAlgorithms {
            get => checkWeakAlgs;
            set {
                checkWeakAlgs = value;
                OnPropertyChanged(nameof(CheckWeakSignatureAlgorithms));
            }
        }
        public Boolean CheckWeakRsaPubKey {
            get => checkWeakPubKeyLength;
            set {
                checkWeakPubKeyLength = value;
                OnPropertyChanged(nameof(CheckWeakRsaPubKey));
            }
        }
        public Int32 MinimumRsaPublicKeyLength {
            get => minRsaKeyLength;
            set {
                minRsaKeyLength = value;
                OnPropertyChanged(nameof(MinimumRsaPublicKeyLength));
            }
        }
        public String AlgId {
            get => algId;
            set {
                algId = value;
                OnPropertyChanged(nameof(AlgId));
            }
        }
        public Oid SelectedAlgId {
            get => selectedAlgId;
            set {
                selectedAlgId = value;
                OnPropertyChanged(nameof(SelectedAlgId));
            }
        }
        public Int32 SelectedIndex {
            get => selectedIndex;
            set {
                selectedIndex = value;
                OnPropertyChanged(nameof(SelectedIndex));
            }
        }
        public Boolean? DialogResult {
            get => dialogResult;
            set {
                dialogResult = value;
                OnPropertyChanged(nameof(DialogResult));
            }
        }

        void addAlgId(Object obj) {
            WeakSigAlgs.Add(new Oid(AlgId));
        }
        Boolean canAddAlgId(Object obj) {
            return !String.IsNullOrEmpty(AlgId);
        }
        void removeAlgId(Object obj) {
            WeakSigAlgs.RemoveAt(SelectedIndex);
        }
        Boolean canRemoveAlgId(Object obj) {
            return SelectedIndex > 0;
        }
        void save(Object obj) {
            Settings.Default.StrictUsage = StrictEKU;
            Settings.Default.AllowUserTrust = AllowUserTrustStore;
            Settings.Default.CheckWeakPubKey = CheckWeakRsaPubKey;
            Settings.Default.CheckWeakAlgs = CheckWeakSignatureAlgorithms;
            Settings.Default.MinimumRsaPubKeyLength = MinimumRsaPublicKeyLength;
            Int32 newValue = SslProtocolsToUse
                .Where(x => x.Enabled)
                .Sum(protocol => (Int32)protocol.ProtocolValue);
            Settings.Default.SslProtocolsToUse = newValue;
            Settings.Default.WeakAlgs.Clear();
            foreach (Oid weakAlg in WeakSigAlgs) {
                Settings.Default.WeakAlgs.Add(weakAlg.Value);
            }
            Settings.Default.Save();
            DialogResult = true;
        }
    }
}
