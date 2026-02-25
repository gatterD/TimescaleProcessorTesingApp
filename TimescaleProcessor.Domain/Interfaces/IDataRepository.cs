using TimescaleProcessor.Domain.Entities;

namespace TimescaleProcessor.Domain.Interfaces;

public interface IDataRepository
{
	Task AddValueRecordsAsync(IEnumerable<ValueRecord> records);
	Task<List<ValueRecord>> GetLastValuesByFileNameAsync(string fileName, int count);
	Task DeleteValueRecordsByFileNameAsync(string FileName);


	Task AddOrUpdateResultAsync(ResultRecord result);
	Task<ResultRecord?> GetResultByFileNameAsync(string fileName);
	Task<List<ResultRecord>> GetFilteredResultsAsync(
		string? fileName = null,
		DateTime? minDateFrom = null,
		DateTime? minDateTo = null,
		double? avgValueFrom = null,
		double? avgValueTo = null,
		double? avgExecutionTimeFrom = null,
		double? avgExecutionTimeTo = null);

	Task BeginTransactionAsync();
	Task CommitTransactionAsync();
	Task RollbackTransactionAsync();
	Task SaveChangesAsync();
}

