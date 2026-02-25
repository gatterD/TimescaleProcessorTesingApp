using TimescaleProcessor.Domain.Entities;

namespace TimescaleProcessor.Application.DTOs;

public class CsvParseResult
{
	public bool IsValid { get; set; }
	public List<ValueRecord> Records { get; set; } = new();
	public List<string> Errors { get; set; } = new();
	public int TotalRows { get; set; }
	public string? FileName { get; set; }
}