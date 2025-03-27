using System;

namespace Awitec.Framework.Acme.Callback
{
    public interface IAcmeCallback
    {
        string Token { get; set; }

        string Thumbprint { get; set; }

        string Location { get; set; }

        DateTime? Hit { get; set; }
    }
}