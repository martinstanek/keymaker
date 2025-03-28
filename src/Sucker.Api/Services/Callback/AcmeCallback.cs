using System;

namespace Sucker.Api.Services.Callback;

public sealed record AcmeCallback : IAcmeCallback
{
    public string Token { get; set; } = string.Empty;

    public string Thumbprint { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public DateTime? Hit { get; set; }
}