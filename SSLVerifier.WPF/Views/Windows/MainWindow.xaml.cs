using System;
using System.ComponentModel;

namespace SSLVerifier.Views.Windows {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow {
		public MainWindow() {
			InitializeComponent();
		}
		void MainWindow_OnClosing(Object Sender, CancelEventArgs e) {
			exitMenu.Command.Execute(e);
		}
	}
}
