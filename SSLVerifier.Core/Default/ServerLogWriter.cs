using System;
using System.Text;

namespace SSLVerifier.Core.Default {
    public class ServerLogWriter : IServerLogWriter {
        readonly StringBuilder _sb = new StringBuilder();
        public Int32 Progress { get; set; }
        public String Text => _sb.ToString();
        public void AppendLine(String text) {
            _sb.AppendLine(text);
        }
        public void Clear() {
            _sb.Clear();
        }
    }
}