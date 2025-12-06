using Keymaker.Model;

namespace Keymaker.Api.Mapping;

public static class Mapper
{
    extension(Service.Model.ChallengeInfo challengeInfo)
    {
        public ChallengeInfo ToPublic()
        {
            return new ChallengeInfo
            {
                CertificateName = challengeInfo.CertificateName,
                ChallengeMode = challengeInfo.ChallengeMode,
                Contact = challengeInfo.Contact,
                DnsMode = challengeInfo.DnsMode,
                Domain = challengeInfo.Domain,
                Expiry = challengeInfo.Expiry,
                IsAutoRenewalEnabled = challengeInfo.IsAutoRenewalEnabled,
                IsChallengeTriggerEnabled = challengeInfo.IsChallengeTriggerEnabled,
                Issuer = challengeInfo.Issuer,
                NextRenewal = challengeInfo.NextRenewal,
                Obtained = challengeInfo.Obtained,
                Organization = challengeInfo.Organization,
                RenewEveryHours = challengeInfo.RenewEveryHours,
                Server = challengeInfo.Server,
                Status = challengeInfo.Status,
                StoreMode = challengeInfo.StoreMode,
                StoreTarget = challengeInfo.StoreTarget
            };
        }
    }
}