using System;
using System.IO;
using Newtonsoft.Json;
using SSLVerifier.API.ModelObjects;

namespace SSLVerifier.API.MainLogic {
    static class JsonRoutine {
        public static RootExportDto Deserialize(String file) {
            return JsonConvert.DeserializeObject<RootExportDto>(File.ReadAllText(file));
        }
    }
}
