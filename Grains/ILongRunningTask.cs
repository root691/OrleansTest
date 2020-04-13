using System.Threading.Tasks;
using Orleans;

namespace Grains
{
    public interface ILongRunningTask : IGrainWithGuidKey
    {
        Task<string> Execute(string message);
    }
}
