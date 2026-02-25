using TimescaleProcessor.Application.DTOs;

namespace TimescaleProcessor.Application.Interfaces;

public interface IFileProcessingService
{
	Task<UploadFileResponse> ProcessFileAsync(Stream fileStream, string fileName);
}