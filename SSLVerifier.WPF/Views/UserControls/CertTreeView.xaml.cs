using System;
using System.Windows;
using System.Windows.Input;
using SSLVerifier.API.ViewModels;
using SSLVerifier.Core.Models;
using ChainElement = SSLVerifier.API.ModelObjects.ChainElement;

namespace SSLVerifier.Views.UserControls {
    /// <summary>
    /// Interaction logic for CertTreeView.xaml
    /// </summary>
    public partial class CertTreeView {
        public CertTreeView() {
            InitializeComponent();
        }
        void onTreeViewSelectedItemChanged(Object s, RoutedPropertyChangedEventArgs<Object> e) {
            if (e.NewValue == null) { return; }
            ((MainWindowVM)Application.Current.MainWindow.DataContext).SelectedTreeItem = ((TreeNode<IChainElement>)e.NewValue).Value;
        }
        void onTreeMouseDoubleClick(Object s, MouseButtonEventArgs e) {
            if (((TreeNode<IChainElement>) tree.SelectedItem)?.Value.Certificate == null) {
                return;
            }
            ((MainWindowVM)DataContext).ViewChainCertificate(null);
        }
    }
}
