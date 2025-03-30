using System.Threading.Tasks;

namespace Keymaker.Service.Dns;

public interface IDnsService
{
    Task AddTxtEntryAsync(string domain, string value);

    Task<string> GetTxtEntryAsync(string domain);
}