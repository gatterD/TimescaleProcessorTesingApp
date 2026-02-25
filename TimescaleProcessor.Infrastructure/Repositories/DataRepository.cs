using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TimescaleProcessor.Domain.Entities;
using TimescaleProcessor.Domain.Interfaces;
using TimescaleProcessor.Infrastructure.Data;

namespace TimescaleProcessor.Infrastructure.Repositories;

public class DataRepository : IDataRepository
{
	private readonly AppDbContext _context;
	private IDbContextTransaction? _currentTransaction;

	public DataRepository(AppDbContext context)
	{
		_context = context;
	}


	public async Task AddValueRecordsAsync(IEnumerable<ValueRecord> records)
	{
		await _context.Values.AddRangeAsync(records);
	}

	public async Task<List<ValueRecord>> GetLastValuesByFileNameAsync(string fileName, int count)
	{
		return await _context.Values
			.Where(v => v.FileName == fileName)
			.OrderByDescending(v => v.Date)
			.Take(count)
			.ToListAsync();
	}

	public async Task DeleteValueRecordsByFileNameAsync(string fileName)
	{
		var records = await _context.Values
			.Where(v => v.FileName == fileName)
			.ToListAsync();

		_context.Values.RemoveRange(records);
	}


	public async Task AddOrUpdateResultAsync(ResultRecord result)
	{
		var existing = await _context.Results
			.FirstOrDefaultAsync(r => r.FileName == result.FileName);

		if (existing != null)
		{
			existing.TimeDeltaSeconds = result.TimeDeltaSeconds;
			existing.MinDate = result.MinDate;
			existing.AvgExecutionTime = result.AvgExecutionTime;
			existing.AvgValue = result.AvgValue;
			existing.MedianValue = result.MedianValue;
			existing.MaxValue = result.MaxValue;
			existing.MinValue = result.MinValue;
			existing.ProcessedAt = result.ProcessedAt;
		}
		else
		{
			await _context.Results.AddAsync(result);
		}
	}

	public async Task<ResultRecord?> GetResultByFileNameAsync(string fileName)
	{
		return await _context.Results
			.FirstOrDefaultAsync(r => r.FileName == fileName);
	}

	public async Task<List<ResultRecord>> GetFilteredResultsAsync(
		string? fileName = null,
		DateTime? minDateFrom = null,
		DateTime? minDateTo = null,
		double? avgValueFrom = null,
		double? avgValueTo = null,
		double? avgExecutionTimeFrom = null,
		double? avgExecutionTimeTo = null)
	{
		var query = _context.Results.AsNoTracking();

		if (!string.IsNullOrWhiteSpace(fileName))
		{
			query = query.Where(r => r.FileName.Contains(fileName));
		}

		if (minDateFrom.HasValue)
		{
			var fromDate = minDateFrom.Value.Date;
			query = query.Where(r => r.MinDate >= fromDate);
		}

		if (minDateTo.HasValue)
		{
			var toDate = minDateTo.Value.Date.AddDays(1).AddTicks(-1);
			query = query.Where(r => r.MinDate <= toDate);
		}

		if (avgValueFrom.HasValue)
		{
			query = query.Where(r => r.AvgValue >= avgValueFrom.Value);
		}

		if (avgValueTo.HasValue)
		{
			query = query.Where(r => r.AvgValue <= avgValueTo.Value);
		}

		if (avgExecutionTimeFrom.HasValue)
		{
			query = query.Where(r => r.AvgExecutionTime >= avgExecutionTimeFrom.Value);
		}

		if (avgExecutionTimeTo.HasValue)
		{
			query = query.Where(r => r.AvgExecutionTime <= avgExecutionTimeTo.Value);
		}

		return await query.OrderByDescending(r => r.ProcessedAt).ToListAsync();
	}


	public async Task BeginTransactionAsync()
	{
		_currentTransaction = await _context.Database.BeginTransactionAsync();
	}

	public async Task CommitTransactionAsync()
	{
		try
		{
			await SaveChangesAsync();
			if (_currentTransaction != null)
			{
				await _currentTransaction.CommitAsync();
			}
		}
		catch
		{
			await RollbackTransactionAsync();
			throw;
		}
		finally
		{
			_currentTransaction?.Dispose();
			_currentTransaction = null;
		}
	}

	public async Task RollbackTransactionAsync()
	{
		if (_currentTransaction != null)
		{
			await _currentTransaction.RollbackAsync();
			_currentTransaction.Dispose();
			_currentTransaction = null;
		}
	}

	public async Task SaveChangesAsync()
	{
		await _context.SaveChangesAsync();
	}
}