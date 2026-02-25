namespace TimescaleProcessor.Domain.Entities;

public class ResultRecord
{
	public Guid Id { get; set; }
	public string FileName { get; set; } = string.Empty;

	private DateTime _minDate;
	public DateTime MinDate
	{
		get => _minDate;
		set => _minDate = EnsureUtc(value);
	}

	private DateTime _processedAt;
	public DateTime ProcessedAt
	{
		get => _processedAt;
		set => _processedAt = EnsureUtc(value);
	}

	public double TimeDeltaSeconds { get; set; }
	public double AvgExecutionTime { get; set; }
	public double AvgValue { get; set; }
	public double MedianValue { get; set; }
	public double MaxValue { get; set; }
	public double MinValue { get; set; }

	private static DateTime EnsureUtc(DateTime date)
	{
		if (date.Kind == DateTimeKind.Utc)
			return date;

		if (date.Kind == DateTimeKind.Local)
			return date.ToUniversalTime();

		// Kind = Unspecified - предполагаем, что это UTC
		return DateTime.SpecifyKind(date, DateTimeKind.Utc);
	}
}