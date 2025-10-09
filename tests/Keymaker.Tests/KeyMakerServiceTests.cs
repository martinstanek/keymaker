using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Keymaker.Tests;

public sealed class KeyMakerServiceTests
{
    [Fact]
    public Task Test()
    {
        true.ShouldBeTrue();

        return Task.CompletedTask;
    }
}