using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Keymaker.Service.Configuration.Service;

namespace Keymaker.Api.Health;

public sealed class StartupHealthCheck : IHealthCheck
{
    private readonly ServiceState _serviceState;

    public StartupHealthCheck(ServiceState serviceState)
    {
        _serviceState = serviceState;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        return _serviceState.ValidationPassed
            ? Task.FromResult(HealthCheckResult.Healthy())
            : Task.FromResult(HealthCheckResult.Unhealthy());
    }
}