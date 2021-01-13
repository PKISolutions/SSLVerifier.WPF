using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SSLVerifier.API.ModelObjects;

namespace SSLVerifier.API.Extensions {
    public static class IEnumerableExtensions {
        public static void SaveAsCSV(this IEnumerable<ServerObject> list, String path) {
            using (StreamWriter file = new StreamWriter(path)) {
                file.WriteLine("Name,Port,Subject,NotBefore,NotAfter,Status");
                foreach (ServerObject server in list) {
                    file.WriteLine(server.ToString());
                }
            }
        }
        public static void SaveAsJson(this IEnumerable<ServerObject> servers, String file) {
            if (servers == null) {
                servers = new ServerObject[0];
            }
            RootExportDto root = new RootExportDto { ServerObjects = servers.ToArray() };
            File.WriteAllText(file, JsonConvert.SerializeObject(root));
        }
        public static Task ForEachAsync<T>(
            this IEnumerable<T> source, Int32 dop, Func<T, Task> body) {
            return Task.WhenAll(Partitioner.Create(source)
                .GetPartitions(dop)
                .Select(partition => Task.Run(async delegate {
                    using (partition) {
                        while (partition.MoveNext()) {
                            await body(partition.Current)
                                .ContinueWith(x => { });
                        }
                    }
                }))
            );
        }
    }
}
