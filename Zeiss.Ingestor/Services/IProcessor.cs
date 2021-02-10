using System.Threading;
using System.Threading.Tasks;
using Zeiss.Ingestor.Models;

namespace Zeiss.Ingestor.Services
{
	interface IProcessor
	{
		Task ProcessMessageAsync(MachineStatusMessage message, CancellationToken cancellationToken = default);
	}
}