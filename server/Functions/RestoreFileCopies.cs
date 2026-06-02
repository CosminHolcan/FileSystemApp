using BLL;
using DAL.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions;

public class RestoreFileCopies
{
    private readonly ILogger _logger;
    private readonly AppFilesBLL _appFilesBLL;

    public RestoreFileCopies(ILoggerFactory loggerFactory, AppFilesBLL appFilesBLL)
    {
        _logger = loggerFactory.CreateLogger<RestoreFileCopies>();
        _appFilesBLL = appFilesBLL;
    }

    // every minute
    [Function("RestoreFileCopies")]
    public async Task Run([TimerTrigger("0 * * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("RestoreFileCopies executed at: {executionTime}", DateTime.Now);

        try
        {
            List<(AppFile Original, AppFile Replica)> pairs = await _appFilesBLL.GetFilesWithReplicasGrouped();

            foreach (var pair in pairs)
            {
                try
                {
                    AppFile original = pair.Original;
                    AppFile replica = pair.Replica;

                    AppFile newer = original.LastUpdate >= replica.LastUpdate ? original : replica;
                    AppFile older = newer.Id == original.Id ? replica : original;

                    await _appFilesBLL.RestorePair(newer, older);
                }
                catch (Exception exPair)
                {
                    _logger.LogError(exPair, "Error while processing pair for restore");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RestoreFileCopies top-level error");
        }
    }
}
