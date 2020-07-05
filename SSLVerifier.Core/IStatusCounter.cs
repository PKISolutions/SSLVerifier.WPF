using System;

namespace SSLVerifier.Core {
    public interface IStatusCounter {
        Int32 Unknown { get; set; }
        Int32 Valid { get; set; }
        Int32 Pending { get; set; }
        Int32 Failed { get; set; }
        void Refresh();
        void Refresh(Int32 count);
    }
}