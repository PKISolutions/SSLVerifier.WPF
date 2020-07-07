using System;

namespace SSLVerifier.Core.Extensions {
    static class BoolExtensions {
        public static String ToYesNo(this Boolean value) {
            return value ? "Yes" : "No";
        }
    }
}
