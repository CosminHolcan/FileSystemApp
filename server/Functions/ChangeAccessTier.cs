using System;
using System.Threading.Tasks;
using BLL;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions;

public class ChangeAccessTier
{
    private readonly ILogger _logger;
    private readonly AppFilesBLL _appFilesBLL;

    public ChangeAccessTier(ILoggerFactory loggerFactory, AppFilesBLL appFilesBLL)
    {
        _logger = loggerFactory.CreateLogger<ChangeAccessTier>();
        _appFilesBLL = appFilesBLL;
    }

    // Sundays at 3:00 AM
    [Function("ChangeAccessTier")]
    public async Task Run([TimerTrigger("0 0 3 * * 0")] TimerInfo myTimer)
    {
        _logger.LogInformation("ChangeAccessTier executed at: {executionTime}", DateTime.Now);

        try
        {
            await _appFilesBLL.ChangeAccessTiers();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while changing access tiers");
        }
    }
}
