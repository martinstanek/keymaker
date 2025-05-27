namespace Keymaker.Model;

public enum CertificateRequestStatus
{
    WaitingForDnsPropagation,
    WaitingForHttpVerification,
    WaitingForDnsVerification,
    TimeOut,
    Success,
    NotRequested
}