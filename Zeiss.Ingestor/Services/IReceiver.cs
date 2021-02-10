using System.Threading;
using System.Threading.Tasks;

namespace Zeiss.Ingestor.Services
{
	interface IReceiver
	{
		Task StartAsync(CancellationToken cancellationToken = default);
		Task StopAsync(CancellationToken cancellationToken = default);
	}
}