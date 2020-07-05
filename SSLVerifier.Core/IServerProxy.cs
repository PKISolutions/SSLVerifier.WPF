using System;
using System.Security;

namespace SSLVerifier.Core {
    public interface IServerProxy {
        Boolean UseProxy { get; }
        String Server { get; }
        Int32 Port { get; }
        Boolean UseAuthentication { get; }
        String UserName { get; }
        SecureString SecurePassword { get; set; }
    }
}