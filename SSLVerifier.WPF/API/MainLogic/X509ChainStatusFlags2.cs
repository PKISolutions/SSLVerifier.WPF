using System;

namespace SSLVerifier.API.MainLogic {
    [Flags]
    public enum X509ChainStatusFlags2 : long {
        NoError                       = 0x00000000, // 0
        NotTimeValid                  = 0x00000001, // 1
        NotTimeNested                 = 0x00000002, // 2
        Revoked                       = 0x00000004, // 4
        NotSignatureValid             = 0x00000008, // 8
        NotValidForUsage              = 0x00000010, // 16,
        UntrustedRoot                 = 0x00000020, // 32,
        RevocationStatusUnknown       = 0x00000040, // 64,
        Cyclic                        = 0x00000080, // 128,
        InvalidExtension              = 0x00000100, // 256,
        InvalidPolicyConstraints      = 0x00000200, // 512,
        InvalidBasicConstraints       = 0x00000400, // 1024,
        InvalidNameConstraints        = 0x00000800, // 2048
        HasNotSupportedNameConstraint = 0x00001000, // 4096,
        HasNotDefinedNameConstraint   = 0x00002000, // 8192,
        HasNotPermittedNameConstraint = 0x00004000, // 16384,
        HasExcludedNameConstraint     = 0x00008000, // 32768,
        PartialChain                  = 0x00010000, // 65536
        CtlNotTimeValid               = 0x00020000, // 131072,
        CtlNotSignatureValid          = 0x00040000, // 262144,
        CtlNotValidForUsage           = 0x00080000, // 524288,
        OfflineRevocation             = 0x01000000, // 16777216,
        NoIssuanceChainPolicy         = 0x02000000, // 33554432,
        NameMismatch                  = 0x04000000, // 67108864
        CertificateNotFound           = 0x08000000, // 134217728
        AboutExpire                   = 0x10000000, // 268435456
        WeakSignatureAlgorithm        = 0x20000000, // 536870912
        WeakRsaPublicKey              = 0x40000000, // 1073741824
    }
}