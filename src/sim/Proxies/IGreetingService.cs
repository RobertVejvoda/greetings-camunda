using System.Threading;
using System.Threading.Tasks;

namespace sim.Proxies
{
	public interface IGreetingService
	{
		Task GreetAsync(GreetingRequest request, CancellationToken cancellationToken);
	}
}