using System;
using System.Windows;
using System.Windows.Input;
using SSLVerifier.API.ModelObjects;
using SSLVerifier.API.ViewModels;

namespace SSLVerifier.Views.UserControls {
	/// <summary>
	/// Interaction logic for CertTreeView.xaml
	/// </summary>
	public partial class CertTreeView {
		public CertTreeView() {
			InitializeComponent();
		}
		void TreeView_OnSelectedItemChanged(Object Sender, RoutedPropertyChangedEventArgs<Object> E) {
			if (E.NewValue == null) { return; }
			((MainWindowVM)Application.Current.MainWindow.DataContext).SelectedTreeItem = ((TreeNode<ChainElement>)E.NewValue).Value;
		}
		void TreeMouseDoubleClick(Object Sender, MouseButtonEventArgs e) {
			if (tree.SelectedItem == null || ((TreeNode<ChainElement>)tree.SelectedItem).Value.Certificate == null) { return;}
			((MainWindowVM)DataContext).ViewChainCertificate(null);
		}
	}
}
