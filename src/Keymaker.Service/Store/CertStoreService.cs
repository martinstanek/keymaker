using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Keymaker.Model;

namespace Keymaker.Service.Store;

public sealed class CertStoreService : ICertStoreService
{
    private const string DefaultFolderTimeFormat = "yyyyMMddHHddss";
    private const string TopLevelFolderName = "/data";
    private const string PrivateKeyFileName = "privkey.pem";
    private const string FullChainFileName = "fullchain.pem";
    private const string PfxFileName = "base64.pfx.txt";

    private readonly ILogger<CertStoreService> _logger;

    public CertStoreService(ILogger<CertStoreService> logger)
    {
        _logger = logger;
    }

    public async Task PersistCertificatesAsync(string domain, string fullChainPem, string privateKeyPem, string base64Pfx)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullChainPem);
        ArgumentException.ThrowIfNullOrWhiteSpace(privateKeyPem);
        ArgumentException.ThrowIfNullOrWhiteSpace(base64Pfx);

        var folder = DateTime.Now.ToString(DefaultFolderTimeFormat);
        var path = Path.Combine(TopLevelFolderName, domain, folder);

        _logger.LogDebug($"Persisting certificates into: {path}");

        try
        {
            Directory.CreateDirectory(path);

            await File.WriteAllTextAsync(Path.Combine(path, FullChainFileName), fullChainPem);
            await File.WriteAllTextAsync(Path.Combine(path, PrivateKeyFileName), privateKeyPem);
            await File.WriteAllTextAsync(Path.Combine(path, PfxFileName), base64Pfx);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }

    public async Task<CertificateInfo> GetMostRecentCertificateInfoAsync()
    {
        if (!Directory.Exists(TopLevelFolderName))
        {
            return CertificateInfo.Empty;
        }

        var result = new List<CertificateInfo>();
        var domains = Directory.GetDirectories(TopLevelFolderName);

        foreach (var domain in domains)
        {
            var path = Path.Combine(TopLevelFolderName, domain);
            var times = Directory.GetDirectories(path);

            foreach (var time in times)
            {
                var base64 = await File.ReadAllTextAsync(Path.Combine(path, time, PfxFileName));
                var cert = FromBase64(base64, time);

                result.Add(cert);
            }
        }

        return result.MaxBy(c => c.Obtained) ?? CertificateInfo.Empty;
    }

    private static CertificateInfo FromBase64(string base64, string obtainedTime)
    {
        var certificateBytes = Convert.FromBase64String(base64);
        var certificate = X509CertificateLoader.LoadCertificate(certificateBytes);
        var obtained = DateTime.ParseExact(obtainedTime, DefaultFolderTimeFormat, provider: null);

        return new CertificateInfo
        {
            Domain = certificate.Subject,
            Issuer = certificate.Issuer,
            Expiry = certificate.NotAfter,
            Obtained = obtained
        };
    }
}