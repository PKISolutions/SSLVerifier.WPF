using System;
using System.ComponentModel;

namespace SSLVerifier.API.ViewModels {
	public class ViewModelBase : INotifyPropertyChanged {
		protected void OnPropertyChanged(String propName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
