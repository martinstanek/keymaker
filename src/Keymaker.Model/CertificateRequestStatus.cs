namespace Keymaker.Model;

public enum CertificateRequestStatus
{
    Started,
    WaitingForDnsPropagation,
    WaitingForHttpVerification,
    WaitingForDnsVerification,
    TimeOut,
    Success,
    Failed,
    Idle
}