using System;
using System.Security;

namespace SSLVerifier.Core.Default {
    public class ServerEntryProxy : IServerProxy {
        public Boolean UseProxy { get; set; }
        public String Server { get; set; }
        public Int32 Port { get; set; }
        public Boolean UseAuthentication { get; set; }
        public String UserName { get; set; }
        public SecureString SecurePassword { get; set; }
    }
}