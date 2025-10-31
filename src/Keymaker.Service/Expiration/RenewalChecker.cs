using System;
using System.Threading.Tasks;
using Keymaker.Service.Configuration;

namespace Keymaker.Service.Expiration;

public sealed class RenewalChecker : IRenewalChecker
{
    private readonly IKeymakerService _keymakerService;
    private readonly KeyMakerConfiguration _configuration;

    public RenewalChecker(IKeymakerService keymakerService, KeyMakerConfiguration configuration)
    {
        _keymakerService = keymakerService;
        _configuration = configuration;
    }

    public async Task<NextChallenge> ShouldTriggerChallengeAsync()
    {
        var certInfo = await _keymakerService.GetMostRecentCertificateInfoAsync();
        var difference = DateTime.Now.Subtract(certInfo.Obtained).TotalHours;
        var nextChallenge = certInfo.IsEmpty()
            ? new NextChallenge
            {
                HoursLeft = 0,
                NextNegotiation = DateTime.Now,
                ShouldTrigger = true
            }
            : new NextChallenge
            {
                HoursLeft = Convert.ToInt32(Math.Round(difference)),
                NextNegotiation = DateTime.Now.AddHours(difference),
                ShouldTrigger = difference > _configuration.RenewEveryHours
            };

        OnNextChallengeCheck.Invoke(this, nextChallenge);

        return nextChallenge;
    }

    public event EventHandler<NextChallenge> OnNextChallengeCheck = (_, _) => { };
}