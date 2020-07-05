using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using SSLVerifier.API.ModelObjects;

namespace SSLVerifier.API.MainLogic {
    class ServerObjectWrapper : IDisposable {

        public ServerObjectWrapper(ServerObject serverObject) {
            ServerObject = serverObject;
        }

        public ServerObject ServerObject { get; }
        public HttpWebRequest Request { get; set; }
        public HttpWebResponse Response { get; set; }
        public X509Chain InternalChain { get; set; }
        public void Dispose() {
            Response?.Dispose();
            InternalChain.Reset();
        }
    }
}
