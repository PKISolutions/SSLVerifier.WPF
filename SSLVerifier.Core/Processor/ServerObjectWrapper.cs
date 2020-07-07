using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using SSLVerifier.Core.Models;

namespace SSLVerifier.Core.Processor {
    public class ServerObjectWrapper : IDisposable {

        public ServerObjectWrapper(IServerObject serverObject) {
            ServerObject = serverObject;
        }

        public IServerObject ServerObject { get; }
        public HttpWebRequest Request { get; set; }
        public HttpWebResponse Response { get; set; }
        public X509Chain InternalChain { get; set; }
        public void Dispose() {
            Response?.Dispose();
            InternalChain.Reset();
        }
    }
}
