using System;

namespace SSLVerifier.Core.Default {
    public class ServerLogWriter : IServerLogWriter {
        public Int32 Progress { get; set; }
        public String Text { get; set; }
        public void AppendLine(String text) { }
        public void Clear() { }
    }
}