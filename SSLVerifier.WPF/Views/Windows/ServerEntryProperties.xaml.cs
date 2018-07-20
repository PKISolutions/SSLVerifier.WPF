using System;
using System.Windows;
using SSLVerifier.API.ModelObjects;

namespace SSLVerifier.Views.Windows {
	/// <summary>
	/// Interaction logic for ServerEntryProperties.xaml
	/// </summary>
	public partial class ServerEntryProperties {
		public ServerEntryProperties(ServerObject item) {
			InitializeComponent();
			DataContext = item;
		}

		void CloseClick(Object sender, RoutedEventArgs e) {
			MustSave = false;
			Close();
		}
		void SaveClick(Object sender, RoutedEventArgs e) {
			MustSave = true;
			((ServerObject)DataContext).Proxy.SecurePassword = passbox.SecurePassword;
			Close();
		}
		public Boolean MustSave { get; private set; }
	}
}
