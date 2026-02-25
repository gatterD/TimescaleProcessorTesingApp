using TimescaleProcessor.Application.DTOs;
using TimescaleProcessor.Application.Interfaces;
using TimescaleProcessor.Domain.Entities;

namespace TimescaleProcessor.Application.Services;

public class ResultCalculatorService : IResultCalculatorService
{
	public ResultDto CalculateIntegralResults(List<ValueRecord> records, string fileName)
	{
		if (records == null || records.Count == 0)
		{
			throw new ArgumentException("Нет записей для вычисления результатов");
		}

		foreach (var record in records)
		{
			if (record.Date.Kind != DateTimeKind.Utc)
			{
				record.Date = DateTime.SpecifyKind(record.Date, DateTimeKind.Utc);
			}
		}

		var minDate = records.Min(r => r.Date);
		var maxDate = records.Max(r => r.Date);

		// Дельта времени в секундах
		var timeDelta = (maxDate - minDate).TotalSeconds;

		// Средние значения
		var avgExecutionTime = records.Average(r => r.ExecutionTime);
		var avgValue = records.Average(r => r.Value);

		// Медиана значения
		var medianValue = CalculateMedian(records.Select(r => r.Value).ToList());

		// Максимум и минимум
		var maxValue = records.Max(r => r.Value);
		var minValue = records.Min(r => r.Value);

		return new ResultDto
		{
			FileName = fileName,
			TimeDeltaSeconds = timeDelta,
			MinDate = DateTime.SpecifyKind(minDate, DateTimeKind.Utc),
			AvgExecutionTime = avgExecutionTime,
			AvgValue = avgValue,
			MedianValue = medianValue,
			MaxValue = maxValue,
			MinValue = minValue,
			ProcessedAt = DateTime.UtcNow
		};
	}
	private double CalculateMedian(List<double> values)
	{
		var sorted = values.OrderBy(v => v).ToList();
		int count = sorted.Count;

		if (count % 2 == 0)
		{
			return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
		}
		else
		{
			return sorted[count / 2];
		}
	}
}