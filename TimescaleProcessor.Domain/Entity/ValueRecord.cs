namespace TimescaleProcessor.Domain.Entities;

public class ValueRecord
{
	public Guid Id { get; set; }
	public string FileName { get; set; } = string.Empty;

	private DateTime _date;
	public DateTime Date
	{
		get => _date;
		set => _date = EnsureUtc(value);
	}

	public double ExecutionTime { get; set; }
	public double Value { get; set; }

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