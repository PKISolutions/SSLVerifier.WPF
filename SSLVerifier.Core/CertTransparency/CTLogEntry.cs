using System;

namespace SSLVerifier.Core.CertTransparency {
    public interface ICertLogEntry {
        Int32 EntryID { get; }
        Int32 IssuerID { get; }
        String IssuerName { get; }
        String HostName { get; }
        DateTime Timestamp { get; }
        DateTime NotBefore { get; }
        DateTime NotAfter { get; }
    }
}