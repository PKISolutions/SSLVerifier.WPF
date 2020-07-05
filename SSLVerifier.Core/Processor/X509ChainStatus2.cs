using System;
using System.Security.Cryptography.X509Certificates;

namespace SSLVerifier.Core.Processor {
    public class X509ChainStatus2 {

        public X509ChainStatus2() { }
        public X509ChainStatus2(X509ChainStatus nativeStatus) {
            Status = (X509ChainStatusFlags2)nativeStatus.Status;
            StatusInformation = nativeStatus.StatusInformation;
        }

        public X509ChainStatusFlags2 Status { get; set; }
        public String StatusInformation { get; set; }
    }
}
