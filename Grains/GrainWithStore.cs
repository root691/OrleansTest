using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;

namespace Grains
{
    public class GrainWithStore : Grain, IGrainWithStore
    {
        private readonly IPersistentState<int> _number;
        private readonly ILogger<GrainWithStore> _logger;

        public GrainWithStore([PersistentState("number")] IPersistentState<int> number, ILogger<GrainWithStore> logger)
        {
            _number = number;
            _logger = logger;
        }

        public async Task<int> IncrementAndReturn()
        {
            _number.State += 1;
            await _number.WriteStateAsync();

            _logger.LogInformation($"Get message from client in {DateTime.Now:HH:mm:ss.fff}");
            _logger.LogInformation($"Start long task in {DateTime.Now:HH:mm:ss.fff}");

            await Task.Run(() => Thread.Sleep(10000));

            _logger.LogInformation($"End long task in {DateTime.Now:HH:mm:ss.fff}");

            if (_number.State == 3) 
                throw new Exception("OH NO, FATAL ERROR");

            return _number.State;
        }
    }
}
