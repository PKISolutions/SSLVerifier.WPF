using SSLVerifier.API.ViewModels;
using System;
using System.Text;

namespace SSLVerifier.API.ModelObjects {
	public class StringBuilderWrapper : ViewModelBase {
		readonly StringBuilder _builder = new StringBuilder();

		public String Text => _builder.ToString();

		public void Add(String text) {
			_builder.Append(text);
			OnPropertyChanged(nameof(Text));
		}
		public void Clear() {
			_builder.Clear();
		}
	}
}
