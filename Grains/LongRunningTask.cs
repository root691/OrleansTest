using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Grains
{
    public class LongRunningTask : Grain, ILongRunningTask
    {
        private readonly ILogger<LongRunningTask> _logger;

        public LongRunningTask(ILogger<LongRunningTask> logger)
        {
            _logger = logger;
        }

        public async Task<string> Execute(string message)
        {
            _logger.LogInformation($"Get message from client in {DateTime.Now:HH:mm:ss.fff}: {message}");
            _logger.LogInformation($"Start long task in {DateTime.Now:HH:mm:ss.fff}");
            await Task.Run(() => Thread.Sleep(10000));
            _logger.LogInformation($"End long task in {DateTime.Now:HH:mm:ss.fff}");
            return "Task completed";
        }
    }
}
