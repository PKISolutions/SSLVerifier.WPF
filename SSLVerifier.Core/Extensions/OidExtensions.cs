using System;
using System.Linq;
using System.Security.Cryptography;

namespace SSLVerifier.Core.Extensions {
    static class OidExtensions {
        public static String Format(this Oid oid) {
            return String.IsNullOrEmpty(oid.FriendlyName)
                ? oid.Value
                : $"{oid.FriendlyName} ({oid.Value})";
        }

        public static Boolean Contains2(this OidCollection collection, Oid value) {
            return collection.Cast<Oid>().Any(x => x.Value == value.Value);
        }
    }
}