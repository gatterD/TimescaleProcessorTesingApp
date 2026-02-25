using TimescaleProcessor.Domain.Entities;
using TimescaleProcessor.Application.DTOs;

namespace TimescaleProcessor.Application.Interfaces;

public interface IResultCalculatorService
{
	ResultDto CalculateIntegralResults(List<ValueRecord> records, string fileName);
}