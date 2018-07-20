using System;
using System.Windows;

namespace SSLVerifier.Views.Windows {
	/// <summary>
	/// Interaction logic for AddServerWindow.xaml
	/// </summary>
	public partial class AddServerWindow {
		public AddServerWindow() {
			InitializeComponent();
		}
		void CloseButton(Object Sender, RoutedEventArgs E) {
			Close();
		}
	}
}
