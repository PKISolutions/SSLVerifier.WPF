using System;
using System.Security.Cryptography;

namespace SSLVerifier.Core.Extensions {
    static class OidExtensions {
        public static String Format(this Oid oid) {
            return String.IsNullOrEmpty(oid.FriendlyName)
                ? oid.Value
                : $"{oid.FriendlyName} ({oid.Value})";
        }
    }
}