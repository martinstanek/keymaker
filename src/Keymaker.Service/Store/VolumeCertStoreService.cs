using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using Keymaker.Model;
using Keymaker.Service.Configuration;
using Keymaker.Service.Model;

namespace Keymaker.Service.Store;

public sealed class VolumeCertStoreService : ICertStoreService
{
    private const string DefaultFolderTimeFormat = "yyyyMMddHHddss";
    private const string TopLevelFolderName = "/data";
    private const string PrivateKeyFileName = "privkey.pem";
    private const string FullChainFileName = "fullchain.pem";
    private const string CertificateInfoFileName = "info.json";
    private const string PfxFileName = "base64.pfx.txt";

    private readonly VolumeStoreConfiguration _configuration;
    private readonly ILogger<VolumeCertStoreService> _logger;

    public VolumeCertStoreService(VolumeStoreConfiguration configuration, ILogger<VolumeCertStoreService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task PersistCertificatesAsync(CertificatePersistenceInfo persistenceInfo)
    {
        var topLevel = string.IsNullOrWhiteSpace(_configuration.ToplevelFolder)
            ? TopLevelFolderName
            : _configuration.ToplevelFolder;
        var folder = DateTime.Now.ToString(DefaultFolderTimeFormat);
        var path = Path.Combine(topLevel, persistenceInfo.Domain, folder);

        _logger.LogDebug($"Persisting certificates into: {path}");

        try
        {
            Directory.CreateDirectory(path);

            await File.WriteAllTextAsync(Path.Combine(path, FullChainFileName), persistenceInfo.FullChainPem);
            await File.WriteAllTextAsync(Path.Combine(path, PrivateKeyFileName), persistenceInfo.PrivateKeyPem);
            await File.WriteAllTextAsync(Path.Combine(path, PfxFileName), persistenceInfo.Base64Pfx);
            await File.WriteAllTextAsync(Path.Combine(path, CertificateInfoFileName), JsonSerializer.Serialize(persistenceInfo));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }

    public async Task<CertificateInfo> GetMostRecentCertificateInfoAsync()
    {
        var topLevel = string.IsNullOrWhiteSpace(_configuration.ToplevelFolder)
            ? TopLevelFolderName
            : _configuration.ToplevelFolder;

        if (!Directory.Exists(topLevel))
        {
            return CertificateInfo.Empty;
        }

        var result = new List<CertificateInfo>();
        var domains = Directory.GetDirectories(topLevel);

        foreach (var domain in domains)
        {
            var path = Path.Combine(topLevel, domain);
            var times = Directory.GetDirectories(path);

            foreach (var time in times)
            {
                var infoPath = Path.Combine(path, time, CertificateInfoFileName);
                var cert = await FromInfoAsync(infoPath);

                result.Add(cert);
            }
        }

        return result.MaxBy(c => c.Obtained) ?? CertificateInfo.Empty;
    }

    private static async Task<CertificateInfo> FromInfoAsync(string infoPath)
    {
        var infoContent = await File.ReadAllTextAsync(infoPath);
        var info = JsonSerializer.Deserialize<CertificatePersistenceInfo>(infoContent);

        if (info is null)
        {
            throw new SerializationException();
        }

        return new CertificateInfo
        {
            Obtained = info.Obtained,
            Domain =  info.Domain,
            Issuer = info.Issuer,
            Expiry = info.Expiry
        };
    }
}