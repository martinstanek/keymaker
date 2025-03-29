using System;

namespace Keymaker.Api.Services.Callback;

public interface IAcmeCallback
{
    string Token { get; set; }

    string Thumbprint { get; set; }

    string Location { get; set; }

    DateTime? Hit { get; set; }
}