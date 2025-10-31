using System;

namespace Keymaker.Service.Expiration;

public sealed record NextChallenge
{
    public required DateTime NextNegotiation { get; init; }

    public required bool ShouldTrigger { get; init; }

    public required int HoursLeft { get; init; }

    public bool IsEmpty()
    {
        return NextNegotiation == DateTime.MinValue;
    }

    public static NextChallenge Empty => new()
    {
        HoursLeft = 0,
        NextNegotiation = DateTime.MinValue,
        ShouldTrigger = false
    };
}