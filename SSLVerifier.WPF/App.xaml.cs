using SSLVerifier.Properties;
using System.Windows;

namespace SSLVerifier {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App {
		protected override void OnStartup(StartupEventArgs e) {
			Settings.Default.Reload();
			base.OnStartup(e);
		}
	}
}
