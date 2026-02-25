using TimescaleProcessor.Application.DTOs;

namespace TimescaleProcessor.Application.Interfaces;

public interface ICsvParserService
{
	Task<CsvParseResult> ParseCsvAsync(Stream fileStream, string fileName);
}