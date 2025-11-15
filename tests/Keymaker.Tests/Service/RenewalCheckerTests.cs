using System;
using System.Threading.Tasks;
using Keymaker.Model;
using Keymaker.Service.Configuration;
using Keymaker.Service.Expiration;
using Keymaker.Service.Store;
using Moq;
using Xunit;
using Shouldly;

namespace Keymaker.Tests.Service;

public sealed class RenewalCheckerTests
{
    [Fact]
    public async Task ShouldTrigger_WhenNoCertificateExists_ReturnsTriggerAndRaisesEvent()
    {
        var store = new Mock<ICertStoreService>();
        var start = DateTime.Now;
        var captured = default(NextChallenge?);
        var capturedSender = default(object?);
        var configuration = new KeyMakerConfiguration
        {
            RenewEveryHours = 24,
            CheckForExpirationEveryMinutes = 60,
            ChallengeMode = default,
            DnsMode = default,
            StorageMode = default,
            IsAutoRenewalEnabled = false,
            IsWebHookEnabled = false,
            WebHookUrl = string.Empty
        };
        
        store.Setup(s => s.GetMostRecentCertificateInfoAsync()).ReturnsAsync(CertificateInfo.Empty).Verifiable();

        var checker = new RenewalChecker(store.Object, configuration);

        checker.NextChallengeChecked += (s, nc) =>
        {
            capturedSender = s;
            captured = nc;
        };

        var result = await checker.ShouldTriggerChallengeAsync();

        result.ShouldNotBeNull();
        result.ShouldTrigger.ShouldBeTrue();
        result.HoursLeft.ShouldBe(0);
        result.NextNegotiation.ShouldBeGreaterThanOrEqualTo(start);
        captured.ShouldNotBeNull();
        captured.ShouldBe(result);
        capturedSender.ShouldBe(checker);
        store.Verify();
    }

    [Fact]
    public async Task ShouldTrigger_WhenCertificateOlderThanRenewHours_ReturnsTriggerAndNegativeOrZeroHoursLeft()
    {
        var store = new Mock<ICertStoreService>();
        var obtained = DateTime.Now.AddHours(-25);
        var captured = default(NextChallenge?);
        var cert = new CertificateInfo
        {
            Domain = "example.com",
            Issuer = "issuer",
            Expiry = DateTime.Now.AddMonths(1),
            Obtained = obtained
        };
        var configuration = new KeyMakerConfiguration
        {
            RenewEveryHours = 24,
            CheckForExpirationEveryMinutes = 60,
            ChallengeMode = default,
            DnsMode = default,
            StorageMode = default,
            IsAutoRenewalEnabled = false,
            IsWebHookEnabled = false,
            WebHookUrl = string.Empty
        };

        store .Setup(s => s.GetMostRecentCertificateInfoAsync()) .ReturnsAsync(cert).Verifiable();

        var checker = new RenewalChecker(store.Object, configuration);
        
        checker.NextChallengeChecked += (_, nc) => captured = nc;

        var result = await checker.ShouldTriggerChallengeAsync();
        
        result.ShouldNotBeNull();
        result.ShouldTrigger.ShouldBeTrue();
        result.HoursLeft.ShouldBeLessThanOrEqualTo(0);
        captured.ShouldNotBeNull();
        captured.ShouldBe(result);
        store.Verify();
    }

    [Fact]
    public async Task ShouldTrigger_WhenCertificateIsRecent_ReturnsFalseAndPositiveHoursLeft()
    {
        var store = new Mock<ICertStoreService>();
        var obtained = DateTime.Now.AddHours(-1);
        var captured = default( NextChallenge?);
        var cert = new CertificateInfo
        {
            Domain = "example.com",
            Issuer = "issuer",
            Expiry = DateTime.Now.AddMonths(1),
            Obtained = obtained
        };
        var configuration = new KeyMakerConfiguration
        {
            RenewEveryHours = 24,
            CheckForExpirationEveryMinutes = 60,
            ChallengeMode = default,
            DnsMode = default,
            StorageMode = default,
            IsAutoRenewalEnabled = false,
            IsWebHookEnabled = false,
            WebHookUrl = string.Empty
        };
        
        store.Setup(s => s.GetMostRecentCertificateInfoAsync()).ReturnsAsync(cert).Verifiable();
        
        var checker = new RenewalChecker(store.Object, configuration);
       
        checker.NextChallengeChecked += (_, nc) => captured = nc;
        
        var result = await checker.ShouldTriggerChallengeAsync();
        
        result.ShouldNotBeNull();
        result.ShouldTrigger.ShouldBeFalse();
        result.HoursLeft.ShouldBeInRange(22, 24);
        captured.ShouldNotBeNull();
        captured!.ShouldBe(result);
        store.Verify();
    }
}