using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TimescaleProcessor.Application.Interfaces;
using TimescaleProcessor.Application.Services;
using TimescaleProcessor.Domain.Interfaces;
using TimescaleProcessor.Infrastructure.Data;
using TimescaleProcessor.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "TimescaleProcessor API",
		Version = "v1",
		Description = "API для обработки CSV файлов с timescale данными"
	});

	// Безопасно добавляем XML комментарии, если файл существует
	var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
	if (File.Exists(xmlPath))
	{
		c.IncludeXmlComments(xmlPath);
	}
});


builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<ICsvParserService, CsvParserService>();
builder.Services.AddScoped<IResultCalculatorService, ResultCalculatorService>();
builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();
builder.Services.AddScoped<IDataRepository, DataRepository>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	dbContext.Database.Migrate();
}

app.Run();