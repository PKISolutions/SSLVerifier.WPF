using System;
using Newtonsoft.Json;

namespace SSLVerifier.Core.CertTransparency {
    class CertShLogEntry : ICertLogEntry {
        [JsonProperty("id")]
        public Int64 EntryID { get; set; }
        [JsonProperty("issuer_ca_id")]
        public Int64 IssuerID { get; set; }
        [JsonProperty("issuer_name")]
        public String IssuerName { get; set; }
        [JsonProperty("name_value")]
        public String HostName { get; set; }
        [JsonProperty("entry_timestamp")]
        public DateTime Timestamp { get; set; }
        [JsonProperty("not_before")]
        public DateTime NotBefore { get; set; }
        [JsonProperty("not_after")]
        public DateTime NotAfter { get; set; }
    }
}
