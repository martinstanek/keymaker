using System;
using System.Text.Json.Serialization;

namespace Keymaker.Model;

public sealed record ChallengeStatus
{
    public required uint PerformedChecks { get; init; }

    public required string DnsRecordValue { get; init; }

    public required DateTime Requested { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required CertificateRequestStatus Status { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ChallengeMode Challenge { get; init; }

    public static ChallengeStatus Empty => new()
    {
        PerformedChecks = 0,
        DnsRecordValue = string.Empty,
        Requested = DateTime.MinValue,
        Status = CertificateRequestStatus.Idle,
        Challenge = ChallengeMode.Http
    };
}