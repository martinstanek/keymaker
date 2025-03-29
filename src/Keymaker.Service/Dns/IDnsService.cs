using System.Threading.Tasks;

namespace Keymaker.Service.Dns;

public interface IDnsService
{
    Task AddTxtEntryAsync(string domain, string prefix, string value);

    Task RemoveTxtEntryAsync(string domain, string prefix);
}