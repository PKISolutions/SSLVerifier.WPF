using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace SSLVerifier.Core.CertTransparency {
    public interface ICTProvider {
        IList<ICertLogEntry> GetLogCertificates(String hostName);
        X509Certificate2 GetCertificate(ICertLogEntry logEntry);
        Boolean CertExist(String hostName, String thumbprint);
    }
}