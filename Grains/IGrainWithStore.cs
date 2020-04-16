using System.Threading.Tasks;
using Orleans;

namespace Grains
{
    public interface IGrainWithStore : IGrainWithStringKey
    {
        public Task<int> IncrementAndReturn();
    }
}
