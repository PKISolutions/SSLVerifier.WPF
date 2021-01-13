using Newtonsoft.Json;

namespace SSLVerifier.API.ModelObjects {
    public class RootExportDto {
        [JsonProperty("SERVER_ENTRY")]
        public ServerObject[] ServerObjects { get; set; }
    }
}
