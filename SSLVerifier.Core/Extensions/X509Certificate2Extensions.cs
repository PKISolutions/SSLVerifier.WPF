using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SSLVerifier.Core.Extensions {
    static class X509Certificate2Extensions {
        public static String ToPEM(this X509Certificate2 certificate) {
            var sb = new StringBuilder(Convert.ToBase64String(certificate.RawData));
            for (Int32 i = 64; i < sb.Length; i += 66) { // 64 + "\r\n"
                sb.Insert(i, Environment.NewLine);
            }
            sb.Insert(0, "-----BEGIN CERTIFICATE-----" + Environment.NewLine);
            sb.Append($"{Environment.NewLine}-----End CERTIFICATE-----");

            return sb.ToString();
        }
    }
}
