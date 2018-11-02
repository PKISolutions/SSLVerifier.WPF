using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
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
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            while ((dep != null) && !(dep is TextBlock))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            // We have only TextBlock elements in our "GridView"
            if (dep == null || !(dep is TextBlock))
            {
                return;
            }

            if (listView.SelectedIndex >= 0 && !((MainWindowVM)DataContext).Running) {
				((MainWindowVM)DataContext).StartSingleScan(null);
			}
		}
	}
}
