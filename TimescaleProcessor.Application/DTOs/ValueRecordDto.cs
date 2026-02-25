namespace TimescaleProcessor.Application.DTOs;

public class ValueRecordDto
{
	public DateTime Date { get; set; }
	public double ExecutionTime { get; set; }
	public double Value { get; set; }
}