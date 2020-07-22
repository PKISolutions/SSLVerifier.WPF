using System;

namespace SSLVerifier.Core.CertTransparency {
    public interface ICertLogEntry {
        Int64 EntryID { get; }
        Int64 IssuerID { get; }
        String IssuerName { get; }
        String HostName { get; }
        DateTime Timestamp { get; }
        DateTime NotBefore { get; }
        DateTime NotAfter { get; }
    }
}