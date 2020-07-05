using System;
using System.Collections.Generic;
using System.IO;
using SSLVerifier.API.ModelObjects;

namespace SSLVerifier.API.Extensions {
    static class IEnumerableExtensions {
        public static void SaveAsCSV(this IEnumerable<ServerObject> list, String path) {
            using (StreamWriter file = new StreamWriter(path)) {
                file.WriteLine("Name,Port,Subject,NotBefore,NotAfter,Status");
                foreach (ServerObject server in list) {
                    file.WriteLine(server.ToString());
                }
            }
        }
    }
}
