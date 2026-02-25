using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TimescaleProcessor.Infrastructure.Converters;

public class DateTimeUtcConverter : ValueConverter<DateTime, DateTime>
{
	public DateTimeUtcConverter()
		: base(
			v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
			v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
	{
	}
}