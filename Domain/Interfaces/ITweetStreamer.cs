using Domain.Models.Application;

namespace Domain.Interfaces
{
    public interface ITweetStreamer
	{
		string GetCurrentStatistics();

		Task<OperationResult> ReadAsync(CancellationToken cancellationToken);
	}
}