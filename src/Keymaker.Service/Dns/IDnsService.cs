using System.Threading.Tasks;

namespace Keymaker.Service.Dns;

public interface IDnsService
{
    Task AddTxtEntryAsync(string value);

    Task<string> GetTxtEntryAsync();

    string SetDomain { get; }

    string CheckDomain { get; }
}