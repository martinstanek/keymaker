using System;
using System.Text.Json.Serialization;

namespace Keymaker.Model;

public sealed record ChallengeStatus
{
    public required DateTime Requested { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required CertificateRequestStatus Status { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ChallengeMode Challenge { get; init; }

    public static ChallengeStatus Empty => new()
    {
        Requested = DateTime.MinValue,
        Status = CertificateRequestStatus.Idle,
        Challenge = ChallengeMode.Http
    };
}