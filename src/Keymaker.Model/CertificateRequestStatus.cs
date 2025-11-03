namespace Keymaker.Model;

public enum CertificateRequestStatus
{
    Started,
    WaitingForDnsPropagation,
    WaitingForHttpVerification,
    WaitingForDnsVerification,
    TimedOut,
    Success,
    Failed,
    Idle
}