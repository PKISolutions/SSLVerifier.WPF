using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SSLVerifier.API.Functions;
using SSLVerifier.API.MainLogic;
using SSLVerifier.API.ModelObjects;
using SSLVerifier.Views.Windows;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SSLVerifier.API.ViewModels {
    class MainWindowVM : ViewModelBase {
        Boolean alreadyExitRaised, singleScan, isSaved = true, running;
        String lastSavedFile, tbServer, statusText = "Ready";
        ServerObject selectedItem;
        Int32 selectedIndex, threshold = 30, tbPort = 443;
        Double progress, progressWidth;
        ChainElement selectedTreeElement, chain;
        Visibility progressVisible;

        public MainWindowVM() {
            initCommands();
            Servers = new ObservableCollection<ServerObject>();
            StatCounter = new StatusCounter();
            StatCounter.Refresh(0);
        }

        void initCommands() {
            NewListCommand = new RelayCommand(newList);
            OpenListCommand = new RelayCommand(openList);
            SaveListCommand = new RelayCommand(saveList, canSaveList);
            SaveAsListCommand = new RelayCommand(saveAsList, canSaveList);
            CloseCommand = new RelayCommand<CancelEventArgs>(close);
            ViewCertificateCommand = new RelayCommand(viewCertificate, canViewCertificate);
            ViewChainCertificateCommand = new RelayCommand(ViewChainCertificate, canViewChainCertificate);
            AddServerCommand = new RelayCommand(addServer, canAddServer);
            RemoveServerCommand = new RelayCommand(removeServer, CanRemoveServer);
            StartSingleScanCommand = new RelayCommand(StartSingleScan, canStartSingleScan);
            StartSingleScan2Command = new RelayCommand(addServerAndScan, canAddServerAndScan);
            StartScanCommand = new RelayCommand(startScan, canStartScan);
            ShowAboutCommand = new RelayCommand(ShowAbout);
            ShowEntryProperties = new RelayCommand(showProperties, canShowProperties);
            ShowSettings = new RelayCommand(showSettings);
            SaveCsvCommand = new RelayCommand(saveAsCsv, canSaveCsv);
            ShowLicenseCommand = new RelayCommand(ShowLicense);
        }

        #region Commands
        public ICommand NewListCommand { get; set; }
        public ICommand OpenListCommand { get; set; }
        public ICommand SaveListCommand { get; set; }
        public ICommand SaveAsListCommand { get; set; }
        public ICommand SaveCsvCommand { get; set; }
        public ICommand CloseCommand { get; set; }
        public ICommand ViewCertificateCommand { get; set; }
        public ICommand ViewChainCertificateCommand { get; set; }
        public ICommand AddServerCommand { get; set; }
        public ICommand RemoveServerCommand { get; set; }
        public ICommand StartSingleScanCommand { get; set; }
        public ICommand StartSingleScan2Command { get; set; }
        public ICommand StartScanCommand { get; set; }
        public ICommand ShowAboutCommand { get; set; }
        public ICommand ShowLicenseCommand { get; set; }
        public ICommand ShowEntryProperties { get; set; }
        public ICommand ShowSettings { get; set; }
        #endregion

        public ObservableCollection<ServerObject> Servers { get; set; }


        public Boolean IsSaved {
            get => isSaved;
            set {
                isSaved = value;
                OnPropertyChanged(nameof(IsSaved));
            }
        }
        public String LastSavedFile {
            get => lastSavedFile;
            set {
                lastSavedFile = value;
                OnPropertyChanged(nameof(LastSavedFile));
            }
        }
        public String ServerName2 {
            get => tbServer;
            set {
                tbServer = value;
                OnPropertyChanged(nameof(ServerName2));
            }
        }
        public Int32 Port2 {
            get => tbPort;
            set {
                tbPort = value;
                OnPropertyChanged(nameof(Port2));
            }
        }

        public Int32 Threshold {
            get => threshold;
            set {
                threshold = value;
                OnPropertyChanged(nameof(Threshold));
            }
        }
        public Boolean Running {
            get => running;
            set {
                running = value;
                OnPropertyChanged(nameof(Running));
            }
        }
        public String StatusText {
            get => statusText;
            set {
                statusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }
        public ChainElement Chain {
            get => chain;
            set {
                chain = value;
                OnPropertyChanged(nameof(Chain));
            }
        }
        public Visibility ProgressVisible {
            get => progressVisible;
            set {
                progressVisible = value;
                OnPropertyChanged(nameof(ProgressVisible));
            }
        }
        public Double Progress {
            get => progress;
            set {
                progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }
        public Double ProgressWidth {
            get => progressWidth;
            set {
                progressWidth = value;
                OnPropertyChanged(nameof(ProgressWidth));
            }
        }
        public StatusCounter StatCounter { get; set; }
        public ServerObject SelectedItem {
            get => selectedItem;
            set {
                selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }
        public Int32 SelectedIndex {
            get => selectedIndex;
            set {
                selectedIndex = value;
                OnPropertyChanged(nameof(SelectedIndex));
            }
        }
        public ChainElement SelectedTreeItem {
            get => selectedTreeElement;
            set {
                selectedTreeElement = value;
                OnPropertyChanged(nameof(SelectedTreeItem));
            }
        }

        void newList(Object obj) {
            if (testSaved()) {
                LastSavedFile = String.Empty;
                IsSaved = true;
                Servers.Clear();
                StatCounter.Refresh(Servers.Count);
            }
        }
        void openList(Object obj) {
            if (testSaved()) {
                OpenFileDialog dlg = new OpenFileDialog {
                    FileName = "ServerList.xml",
                    DefaultExt = ".xml",
                    Filter = "Server list files (.xml)|*.xml"
                };
                Boolean? result = dlg.ShowDialog();
                if (result != true) { return; }
                try {
                    XmlObject list = XmlRoutine.Deserialize(dlg.FileName);
                    if (list == null) { return; }
                    Servers.Clear();
                    foreach (ServerObject item in list.ServerObjects) {
                        Servers.Add(item);
                    }
                    StatCounter.Refresh(Servers.Count);
                    IsSaved = true;
                } catch (Exception e) {
                    Tools.MsgBox("XML Read error", e.Message);
                }
            }
        }
        void saveList(Object obj) {
            if (String.IsNullOrEmpty(LastSavedFile)) { saveAsList(null); }
            try {
                XmlRoutine.Serialize(Servers, LastSavedFile);
                IsSaved = true;
            } catch (Exception e) {
                Tools.MsgBox("XML Write error", e.Message);
            }
        }
        void saveAsList(Object obj) {
            SaveFileDialog dlg = new SaveFileDialog {
                FileName = "ServerList.xml",
                DefaultExt = ".xml",
                Filter = "Server list files (.xml)|*.xml"
            };
            Boolean? result = dlg.ShowDialog();
            if (result == true) {
                LastSavedFile = dlg.FileName;
                try {
                    XmlRoutine.Serialize(Servers, LastSavedFile);
                    IsSaved = true;
                } catch (Exception e) {
                    Tools.MsgBox("XML Write error", e.Message);
                }
            }
        }
        void saveAsCsv(Object obj) {
            SaveFileDialog dlg = new SaveFileDialog {
                FileName = "ServerList.csv",
                DefaultExt = ".xml",
                Filter = "Comma separated value file  (.csv)|*.csv"
            };
            Boolean? result = dlg.ShowDialog();
            if (result == true) {
                LastSavedFile = dlg.FileName;
                try {
                    Tools.SaveArrayAsCSV(Servers, dlg.FileName);
                } catch (Exception e) {
                    Tools.MsgBox("CSV Write error", e.Message);
                }
            }
        }
        void close(CancelEventArgs e) {
            if (e == null) {
                // exit button pressed
                if (IsSaved) {
                    Application.Current.Shutdown();
                } else {
                    MessageBoxResult mbxResult = Tools.MsgBox(
                        "SSL Certificate Verifier",
                        "Current server was modified. Save changes?",
                        MessageBoxImage.Warning,
                        MessageBoxButton.YesNoCancel);
                    switch (mbxResult) {
                        case MessageBoxResult.Yes:
                            saveList(null);
                            if (IsSaved) { Application.Current.MainWindow.Close(); }
                            break;
                        case MessageBoxResult.No:
                            alreadyExitRaised = true;
                            Application.Current.MainWindow.Close();
                            break;
                    }
                }
            } else {
                // fired from X button or Alt+F4 gesture
                if (IsSaved) {
                    e.Cancel = false;
                } else {
                    // stub statement to determine whether the event was raised from button.
                    if (alreadyExitRaised) {
                        e.Cancel = false;
                    } else {
                        MessageBoxResult mbxResult = Tools.MsgBox(
                            "SSL Certificate Verifier",
                            "Current server was modified. Save changes?",
                            MessageBoxImage.Warning,
                            MessageBoxButton.YesNoCancel);
                        switch (mbxResult) {
                            case MessageBoxResult.Yes:
                                saveList(null);
                                e.Cancel = !IsSaved;
                                break;
                            case MessageBoxResult.No:
                                e.Cancel = false;
                                break;
                            case MessageBoxResult.Cancel:
                                e.Cancel = true;
                                break;
                        }
                    }
                }
            }
        }
        void showProperties(Object obj) {
            ProxyObject old = SelectedItem.Proxy;
            var dlg = WindowsUI.ShowWindowDialog<ServerEntryProperties>(SelectedItem);
            if (dlg.MustSave) {
                SelectedItem.Proxy = old;
            }
        }
        Boolean canShowProperties(Object obj) {
            return SelectedItem != null;
        }
        Boolean canSaveCsv(Object obj) {
            return Servers.Count > 0;
        }
        Boolean canSaveList(Object obj) {
            return Servers.Count > 0;
        }

        void viewCertificate(Object obj) {
            X509Certificate2UI.DisplayCertificate(SelectedItem.Certificate);
        }
        Boolean canViewCertificate(Object obj) {
            if (SelectedItem == null) { return false; }
            return SelectedItem.Certificate != null && !IntPtr.Zero.Equals(SelectedItem.Certificate.Handle);
        }
        public void ViewChainCertificate(Object obj) {
            X509Certificate2UI.DisplayCertificate(SelectedTreeItem.Certificate);
        }
        Boolean canViewChainCertificate(Object obj) {
            return SelectedTreeItem?.Certificate != null;
        }

        public void StartSingleScan(Object obj) {
            singleScan = true;
            startScan(null);
        }
        Boolean canStartSingleScan(Object obj) {
            return SelectedIndex >= 0;
        }
        void addServerAndScan(Object obj) {
            ServerObject server = new ServerObject { ServerAddress = ServerName2.Trim().ToLower().Replace("https://", null), Port = Port2 };
            Servers.Add(server);
            StatCounter.Unknown++;
            IsSaved = false;
            ServerName2 = String.Empty;
            SelectedIndex = Servers.Count - 1;
            StartSingleScan(null);
        }
        Boolean canAddServerAndScan(Object obj) {
            return !String.IsNullOrEmpty(ServerName2) &&
                !String.IsNullOrWhiteSpace(ServerName2) &&
                !Servers.Contains(new ServerObject { ServerAddress = ServerName2.Trim().ToLower().Replace("https://", null), Port = Port2 });
        }
        void prepareScan() {
            foreach (ServerObject server in Servers) {
                if (singleScan) {
                    server.CanProcess = false;
                } else {
                    server.ItemStatus = ServerStatusEnum.Unknown;
                }
            }
            if (singleScan) {
                switch (Servers[SelectedIndex].ItemStatus) {
                    case ServerStatusEnum.Valid:
                        StatCounter.Valid--;
                        StatCounter.Unknown++;
                        break;
                    case ServerStatusEnum.Pending:
                        StatCounter.Pending--;
                        StatCounter.Unknown++;
                        break;
                    case ServerStatusEnum.Failed:
                        StatCounter.Failed--;
                        StatCounter.Unknown++;
                        break;
                }
                Servers[SelectedIndex].CanProcess = true;
                Servers[SelectedIndex].ItemStatus = ServerStatusEnum.Unknown;
            } else {
                StatCounter.Refresh();
            }
        }
        void startScan(Object obj) {
            Running = true;
            ProgressVisible = Visibility.Visible;
            ProgressWidth = 100;
            StatusText = "working...";
            prepareScan();
            BackgroundWorker worker = new BackgroundWorker { WorkerReportsProgress = true };
            var certVerifier = new CertProcessor();
            worker.DoWork += certVerifier.StartScan;
            worker.RunWorkerCompleted += endScan;
            worker.ProgressChanged += scanReportChanged;
            // reset statuses
            BackgroundObject arg = new BackgroundObject {
                Servers = Servers,
                Counters = StatCounter,
                Treshold = Threshold,
                SingleScan = singleScan
            };
            worker.RunWorkerAsync(arg);
        }
        void scanReportChanged(Object sender, ProgressChangedEventArgs e) {
            if (e.UserState == null) {
                Progress = e.ProgressPercentage;
            } else {
                switch (((ReportObject)e.UserState).Action) {
                    case "Clear":
                        Servers[((ReportObject)e.UserState).Index].Tree.Clear();
                        break;
                    case "Add":
                        Servers[((ReportObject)e.UserState).Index].Tree.Add(((ReportObject)e.UserState).NewTree);
                        break;
                }
            }
        }
        void endScan(Object Sender, RunWorkerCompletedEventArgs e) {
            Running = singleScan = false;
            ProgressVisible = Visibility.Hidden;
            ProgressWidth = 0;
            foreach (ServerObject server in Servers) {
                server.ItemProgress = 0;
                server.CanProcess = false;
            }
            ((BackgroundWorker)Sender).Dispose();
            StatusText = "ready";

        }
        Boolean canStartScan(Object obj) {
            return !Running && Servers.Count > 0;
        }

        static void addServer(Object obj) {
            WindowsUI.ShowWindow<AddServerWindow>();
        }
        Boolean canAddServer(Object obj) {
            return !Running;
        }
        void removeServer(Object obj) {
            switch (SelectedItem.ItemStatus) {
                case ServerStatusEnum.Unknown:
                    StatCounter.Unknown--;
                    break;
                case ServerStatusEnum.Valid:
                    StatCounter.Valid--;
                    break;
                case ServerStatusEnum.Pending:
                    StatCounter.Pending--;
                    break;
                case ServerStatusEnum.Failed:
                    StatCounter.Failed--;
                    break;
            }
            Servers.RemoveAt(SelectedIndex);
            IsSaved = false;
        }
        Boolean CanRemoveServer(Object obj) {
            return SelectedIndex > -1;
        }

        static void ShowAbout(Object obj) {
            WindowsUI.ShowWindow<About>();
        }
        static void ShowLicense(Object obj) {
            WindowsUI.ShowWindow<LicenseWindow>();
        }
        void showSettings(Object obj) {
            WindowsUI.ShowWindowDialog<SettingsWindow>();
        }

        Boolean testSaved() {
            if (!IsSaved) {
                MessageBoxResult mbxResult = Tools.MsgBox(
                    "SSL Certificate Verifier",
                    "Current server list was modified. Save changes?",
                    MessageBoxImage.Warning,
                    MessageBoxButton.YesNoCancel);
                switch (mbxResult) {
                    case MessageBoxResult.Yes:
                        saveList(null);
                        break;
                    case MessageBoxResult.No:
                        return true;
                    case MessageBoxResult.Cancel:
                        return false;
                }
            }
            return IsSaved;
        }
        public Boolean AddServerItem(String name, Int32 port) {
            ServerObject obj = new ServerObject { ServerAddress = name.Trim().ToLower().Replace("https://", null), Port = port };
            if (Servers.Contains(obj)) {
                Tools.MsgBox("Error", "Entered server name already in the list.");
                return false;
            }
            Servers.Add(obj);
            StatCounter.Unknown++;
            IsSaved = false;
            return true;
        }
    }
}