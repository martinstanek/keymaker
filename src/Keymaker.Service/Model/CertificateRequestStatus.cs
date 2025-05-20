namespace Keymaker.Service.Model;

public enum CertificateRequestStatus
{
    WaitingForDnsPropagation,
    WaitingForHttpVerification,
    WaitingForDnsVerification,
    TimeOut,
    Success
}