using Microsoft.Extensions.Logging;
using TimescaleProcessor.Application.DTOs;
using TimescaleProcessor.Application.Interfaces;
using TimescaleProcessor.Domain.Entities;
using TimescaleProcessor.Domain.Interfaces;


namespace TimescaleProcessor.Application.Services;

public class FileProcessingService : IFileProcessingService
{
	private readonly ICsvParserService _csvParserService;
	private readonly IResultCalculatorService _resultCalculatorService;
	private readonly IDataRepository _repository;
	private readonly ILogger<FileProcessingService> _logger;

	public FileProcessingService(
		ICsvParserService csvParserService,
		IResultCalculatorService resultCalculatorService,
		IDataRepository repository,
		ILogger<FileProcessingService> logger)
	{
		_csvParserService = csvParserService;
		_resultCalculatorService = resultCalculatorService;
		_repository = repository;
		_logger = logger;
	}

	public async Task<UploadFileResponse> ProcessFileAsync(Stream fileStream, string fileName)
	{
		try
		{
			var parseResult = await _csvParserService.ParseCsvAsync(fileStream, fileName);

			if (!parseResult.IsValid)
			{
				return new UploadFileResponse
				{
					Success = false,
					Message = "Ошибка валидации файла",
					Errors = parseResult.Errors,
					FileName = fileName
				};
			}

			foreach (var record in parseResult.Records)
			{
				record.FileName = fileName;
			}

			// Вычисляем результаты
			var resultDto = _resultCalculatorService.CalculateIntegralResults(parseResult.Records, fileName);

			// Сохраняем в БД (в транзакции)
			await _repository.BeginTransactionAsync();

			try
			{
				await _repository.DeleteValueRecordsByFileNameAsync(fileName);

				await _repository.AddValueRecordsAsync(parseResult.Records);

				var resultRecord = new ResultRecord
				{
					Id = Guid.NewGuid(),
					FileName = fileName,
					TimeDeltaSeconds = resultDto.TimeDeltaSeconds,
					MinDate = resultDto.MinDate,
					AvgExecutionTime = resultDto.AvgExecutionTime,
					AvgValue = resultDto.AvgValue,
					MedianValue = resultDto.MedianValue,
					MaxValue = resultDto.MaxValue,
					MinValue = resultDto.MinValue,
					ProcessedAt = DateTime.UtcNow
				};

				await _repository.AddOrUpdateResultAsync(resultRecord);
				await _repository.CommitTransactionAsync();

				_logger.LogInformation("Файл {FileName} успешно обработан. Добавлено записей: {Count}",
					fileName, parseResult.Records.Count);

				return new UploadFileResponse
				{
					Success = true,
					Message = "Файл успешно обработан",
					FileName = fileName,
					RecordsProcessed = parseResult.Records.Count,
					Results = resultDto
				};
			}
			catch (Exception ex)
			{
				await _repository.RollbackTransactionAsync();
				_logger.LogError(ex, "Ошибка при сохранении файла {FileName} в БД", fileName);
				throw;
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Ошибка при обработке файла {FileName}", fileName);
			throw;
		}
	}
}