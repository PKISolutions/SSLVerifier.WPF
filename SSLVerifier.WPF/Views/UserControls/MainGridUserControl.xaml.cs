using System;
using System.Windows.Input;
using SSLVerifier.API.ViewModels;

namespace SSLVerifier.Views.UserControls {
	/// <summary>
	/// Interaction logic for MainGridUserControl.xaml
	/// </summary>
	public partial class MainGridUserControl {
		public MainGridUserControl() {
			InitializeComponent();
		}
		void lvModulesMouseDoubleClick(Object Sender, MouseButtonEventArgs e) {
			if (listView.SelectedIndex >= 0 && !((MainWindowVM)DataContext).Running) {
				((MainWindowVM)DataContext).StartSingleScan(null);
			}
		}
	}
}
