using Microsoft.AspNetCore.Mvc;
using TimescaleProcessor.API.Models;
using TimescaleProcessor.Application.DTOs;
using TimescaleProcessor.Application.Interfaces;
using TimescaleProcessor.Domain.Entities;
using TimescaleProcessor.Domain.Interfaces;

namespace TimescaleProcessor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FilesController : ControllerBase
{
	private readonly IFileProcessingService _fileProcessingService;
	private readonly IDataRepository _repository;
	private readonly ILogger<FilesController> _logger;

	public FilesController(
		IFileProcessingService fileProcessingService,
		IDataRepository repository,
		ILogger<FilesController> logger)
	{
		_fileProcessingService = fileProcessingService;
		_repository = repository;
		_logger = logger;
	}

	[HttpPost("upload")]
	[ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	[RequestSizeLimit(10_000_000)]
	public async Task<IActionResult> UploadCsv(IFormFile file)
	{
		if (file == null || file.Length == 0)
		{
			return BadRequest(new UploadFileResponse
			{
				Success = false,
				Message = "Файл не выбран или пуст"
			});
		}

		if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
		{
			return BadRequest(new UploadFileResponse
			{
				Success = false,
				Message = "Допустимы только CSV файлы"
			});
		}

		try
		{
			using var stream = new MemoryStream();
			await file.CopyToAsync(stream);
			stream.Position = 0;

			var result = await _fileProcessingService.ProcessFileAsync(stream, file.FileName);

			if (!result.Success)
			{
				return BadRequest(result);
			}

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Ошибка при обработке файла {FileName}", file.FileName);
			return StatusCode(500, new UploadFileResponse
			{
				Success = false,
				Message = "Внутренняя ошибка сервера"
			});
		}
	}

	[HttpGet("results")]
	[ProducesResponseType(typeof(List<ResultDto>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetResults([FromQuery] ResultFilterRequest filter)
	{
		var results = await _repository.GetFilteredResultsAsync(
			fileName: filter.FileName,
			minDateFrom: filter.MinDateFrom,
			minDateTo: filter.MinDateTo,
			avgValueFrom: filter.AvgValueFrom,
			avgValueTo: filter.AvgValueTo,
			avgExecutionTimeFrom: filter.AvgExecutionTimeFrom,
			avgExecutionTimeTo: filter.AvgExecutionTimeTo);

		// Преобразуем в DTO
		var resultDtos = results.Select(r => new ResultDto
		{
			FileName = r.FileName,
			TimeDeltaSeconds = r.TimeDeltaSeconds,
			MinDate = r.MinDate,
			AvgExecutionTime = r.AvgExecutionTime,
			AvgValue = r.AvgValue,
			MedianValue = r.MedianValue,
			MaxValue = r.MaxValue,
			MinValue = r.MinValue,
			ProcessedAt = r.ProcessedAt
		}).ToList();

		return Ok(resultDtos);
	}

	[HttpGet("{fileName}/last-values")]
	[ProducesResponseType(typeof(List<ValueRecordDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetLastValues(string fileName)
	{
		if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
		{
			fileName = fileName + ".csv";
		}

		var values = await _repository.GetLastValuesByFileNameAsync(fileName, 10);

		if (values == null || values.Count == 0)
		{
			return NotFound($"Файл с именем '{fileName}' не найден");
		}

		// Преобразуем в DTO
		var valueDtos = values.Select(v => new ValueRecordDto
		{
			Date = v.Date,
			ExecutionTime = v.ExecutionTime,
			Value = v.Value
		}).ToList();

		return Ok(valueDtos);
	}
}