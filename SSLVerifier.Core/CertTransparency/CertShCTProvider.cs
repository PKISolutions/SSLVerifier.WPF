using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;

namespace SSLVerifier.Core.CertTransparency {
    public class CertShCTProvider : ICTProvider {
        public IList<ICertLogEntry> GetLogCertificates(String hostName) {
            using (var wc = new WebClient()) {
                try {
                    String json = wc.DownloadString($"https://crt.sh/?q={hostName}&output=json");
                    List<CertShLogEntry> obj = JsonConvert.DeserializeObject<List<CertShLogEntry>>(json);
                    return obj.Cast<ICertLogEntry>().ToList();
                } catch {
                    // we were unable to connect to this provider for whatever reason, so simply return null instead of exception
                    return null;
                }
            }
        }
        public X509Certificate2 GetCertificate(ICertLogEntry logEntry) {
            using (var wc = new WebClient()) {
                try {
                    String base64 = wc.DownloadString($"https://crt.sh/?d={logEntry.EntryID}")
                        .Replace("-----BEGIN CERTIFICATE-----", String.Empty)
                        .Replace("-----END CERTIFICATE-----", String.Empty);
                    var cert = new X509Certificate2(Convert.FromBase64String(base64));
                    return new X509Certificate2(Convert.FromBase64String(base64));
                } catch {
                    return null;
                }
            }
        }
        public Boolean CertExist(String hostName, String thumbprint) {
            if (String.IsNullOrEmpty(hostName)) {
                throw new ArgumentNullException(nameof(hostName));
            }
            if (String.IsNullOrEmpty(thumbprint)) {
                throw new ArgumentNullException(nameof(thumbprint));
            }

            IList<ICertLogEntry> entries = GetLogCertificates(hostName);
            if (entries == null) {
                return false;
            }

            return entries.Select(GetCertificate)
                .Where(x => x != null)
                .Any(x => thumbprint.Equals(x.Thumbprint, StringComparison.OrdinalIgnoreCase));
        }
    }
}