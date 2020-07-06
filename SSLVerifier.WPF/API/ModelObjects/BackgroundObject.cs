using System;
using System.Collections.Generic;

namespace SSLVerifier.API.ModelObjects {
    class BackgroundObject {

        public BackgroundObject() { }
        public BackgroundObject(IEnumerable<ServerObject> collection) {
            if (collection != null) {
                foreach (ServerObject item in collection) {
                    Servers.Add(item);
                }
            }
        }

        public IList<ServerObject> Servers { get; } = new List<ServerObject>();
        public Boolean SingleScan { get; set; }
    }
}
