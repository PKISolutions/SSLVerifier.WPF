using System;
using System.Text;
using SSLVerifier.API.ViewModels;
using SSLVerifier.Core;

namespace SSLVerifier.API.ModelObjects {
    public class ServerLogWriter : ViewModelBase, IServerLogWriter {
        readonly StringBuilder _builder = new StringBuilder();
        Int32 progress;

        public Int32 Progress {
            get => progress;
            set {
                progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }
        public String Text => _builder.ToString();

        public void AppendLine(String text) {
            _builder.AppendLine(text);
            OnPropertyChanged(nameof(Text));
        }
        public void Clear() {
            progress = 0;
            _builder.Clear();
        }
    }
}
