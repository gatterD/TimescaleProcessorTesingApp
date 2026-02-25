using TimescaleProcessor.Application.DTOs;
using TimescaleProcessor.Application.Interfaces;
using TimescaleProcessor.Domain.Entities;
using System.Globalization;

namespace TimescaleProcessor.Application.Services;

public class CsvParserService : ICsvParserService
{
	private const int MAX_ROWS = 10000;
	private const int MIN_ROWS = 1;
	private static readonly DateTime MinAllowedDate = new DateTime(2000, 1, 1);

	public async Task<CsvParseResult> ParseCsvAsync(Stream fileStream, string fileName)
	{
		var result = new CsvParseResult
		{
			FileName = fileName,
			Records = new List<ValueRecord>(),
			Errors = new List<string>()
		};

		using var reader = new StreamReader(fileStream);
		string? line;
		int rowNumber = 0;

		line = await reader.ReadLineAsync();
		if (string.IsNullOrEmpty(line) || !line.StartsWith("Date;ExecutionTime;Value"))
		{
			result.Errors.Add("Неверный формат CSV. Ожидается заголовок: Date;ExecutionTime;Value");
			result.IsValid = false;
			return result;
		}

		while ((line = await reader.ReadLineAsync()) != null)
		{
			rowNumber++;

			if (rowNumber > MAX_ROWS)
			{
				result.Errors.Add($"Превышено максимальное количество строк ({MAX_ROWS})");
				break;
			}

			if (string.IsNullOrWhiteSpace(line))
				continue;

			var parseError = TryParseLine(line, rowNumber, out var record);
			if (!string.IsNullOrEmpty(parseError))
			{
				result.Errors.Add($"Строка {rowNumber}: {parseError}");
				continue;
			}

			if (record != null)
			{
				record.FileName = fileName;
				result.Records.Add(record);
			}
		}

		if (rowNumber < MIN_ROWS)
		{
			result.Errors.Add($"Файл должен содержать минимум {MIN_ROWS} строку данных");
		}

		result.TotalRows = rowNumber;
		result.IsValid = result.Errors.Count == 0 && result.Records.Count > 0;

		return result;
	}

	private string? TryParseLine(string line, int rowNumber, out ValueRecord? record)
	{
		record = null;
		var parts = line.Split(';');

		if (parts.Length != 3)
		{
			return "Строка должна содержать 3 поля, разделенных точкой с запятой";
		}

		if (!DateTime.TryParseExact(parts[0], "yyyy-MM-ddTHH-mm-ss.ffffZ",
			CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
		{
			return "Неверный формат даты. Ожидается: ГГГГ-ММ-ДДTчч-мм-сс.ммммZ";
		}

		date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

		if (date > DateTime.UtcNow)
		{
			return "Дата не может быть позже текущей";
		}

		if (date < MinAllowedDate)  // MinAllowedDate = 2000-01-01
		{
			return $"Дата не может быть раньше {MinAllowedDate:yyyy-MM-dd}";
		}

		if (!double.TryParse(parts[1], CultureInfo.InvariantCulture, out var executionTime))
		{
			return "Неверный формат времени выполнения";
		}

		if (executionTime < 0)
		{
			return "Время выполнения не может быть отрицательным";
		}

		if (!double.TryParse(parts[2], CultureInfo.InvariantCulture, out var value))
		{
			return "Неверный формат значения";
		}

		if (value < 0)
		{
			return "Значение не может быть отрицательным";
		}

		record = new ValueRecord
		{
			Id = Guid.NewGuid(),
			Date = date,
			ExecutionTime = executionTime,
			Value = value
		};

		return null;
	}
}