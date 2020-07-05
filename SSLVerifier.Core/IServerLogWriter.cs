using System;

namespace SSLVerifier.Core {
    public interface IServerLogWriter {
        Int32 Progress { get; set; }
        String Text { get; }
        void AppendLine(String text);
        void Clear();
    }
}