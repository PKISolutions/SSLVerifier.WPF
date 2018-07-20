using System;
using System.Windows;

namespace SSLVerifier.Views.Windows {
	/// <summary>
	/// Interaction logic for Settings.xaml
	/// </summary>
	public partial class SettingsWindow {
		public SettingsWindow() {
			InitializeComponent();
		}

		void CloseClick(Object sender, RoutedEventArgs e) {
			Close();
		}
	}
}
