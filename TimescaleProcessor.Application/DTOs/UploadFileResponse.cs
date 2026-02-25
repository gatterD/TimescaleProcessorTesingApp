namespace TimescaleProcessor.Application.DTOs;

public class UploadFileResponse
{
	public bool Success { get; set; }
	public string? Message { get; set; }
	public List<string>? Errors { get; set; }
	public string? FileName { get; set; }
	public int RecordsProcessed { get; set; }
	public ResultDto? Results { get; set; }
}