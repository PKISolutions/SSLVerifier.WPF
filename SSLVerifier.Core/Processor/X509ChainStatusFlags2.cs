using System;

namespace SSLVerifier.Core.Processor {
    [Flags]
    public enum X509ChainStatusFlags2 : long {
        NoError                          = 0x00000000,
        NotTimeValid                     = 0x00000001,
        NotTimeNested                    = 0x00000002,
        Revoked                          = 0x00000004,
        NotSignatureValid                = 0x00000008,
        NotValidForUsage                 = 0x00000010,
        UntrustedRoot                    = 0x00000020,
        RevocationStatusUnknown          = 0x00000040,
        Cyclic                           = 0x00000080,
        InvalidExtension                 = 0x00000100,
        InvalidPolicyConstraints         = 0x00000200,
        InvalidBasicConstraints          = 0x00000400,
        InvalidNameConstraints           = 0x00000800,
        HasNotSupportedNameConstraint    = 0x00001000,
        HasNotDefinedNameConstraint      = 0x00002000,
        HasNotPermittedNameConstraint    = 0x00004000,
        HasExcludedNameConstraint        = 0x00008000,
        PartialChain                     = 0x00010000,
        CtlNotTimeValid                  = 0x00020000,
        CtlNotSignatureValid             = 0x00040000,
        CtlNotValidForUsage              = 0x00080000,
        HasWeakSignature                 = 0x00100000,
        OfflineRevocation                = 0x01000000,
        NoIssuanceChainPolicy            = 0x02000000,
        HasNotSupportedCriticalExtension = 0x08000000,
        ExplicitDistrust                 = 0x04000000,
        CertificateNotFound              = 0x100000000,
        AboutExpire                      = 0x200000000,
        NameMismatch                     = 0x400000000,
        WeakRsaPublicKey                 = 0x800000000,
        MissingAltNameExtension          = 0x1000000000,
    }
}