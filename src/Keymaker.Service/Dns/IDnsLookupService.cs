using System.Threading.Tasks;

namespace Keymaker.Service.Dns;

public interface IDnsLookupService
{
    Task<string> GetTxtEntryAsync(string checkDomain);
}