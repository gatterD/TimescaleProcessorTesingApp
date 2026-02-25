using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TimescaleProcessor.Domain.Entities;
using TimescaleProcessor.Infrastructure.Converters;

namespace TimescaleProcessor.Infrastructure.Data;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	public DbSet<ValueRecord> Values { get; set; }
	public DbSet<ResultRecord> Results { get; set; }

	protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
	{
		configurationBuilder
			.Properties<DateTime>()
			.HaveConversion<DateTimeUtcConverter>();

		configurationBuilder
			.Properties<DateTime?>()
			.HaveConversion<DateTimeUtcConverter>();
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<ValueRecord>(entity =>
		{
			entity.ToTable("Values");
			entity.HasKey(e => e.Id);

			entity.Property(e => e.FileName)
				.IsRequired()
				.HasMaxLength(255);

			entity.Property(e => e.Date)
				.IsRequired();

			entity.Property(e => e.ExecutionTime)
				.IsRequired();

			entity.Property(e => e.Value)
				.IsRequired();

			entity.HasIndex(e => new { e.FileName, e.Date })
				.HasDatabaseName("IX_Values_FileName_Date");

			entity.HasIndex(e => e.FileName)
				.HasDatabaseName("IX_Values_FileName");
		});

		modelBuilder.Entity<ResultRecord>(entity =>
		{
			entity.ToTable("Results");
			entity.HasKey(e => e.Id);

			entity.Property(e => e.FileName)
				.IsRequired()
				.HasMaxLength(255);

			entity.Property(e => e.TimeDeltaSeconds)
				.IsRequired();

			entity.Property(e => e.MinDate)
				.IsRequired();

			entity.Property(e => e.AvgExecutionTime)
				.IsRequired();

			entity.Property(e => e.AvgValue)
				.IsRequired();

			entity.Property(e => e.MedianValue)
				.IsRequired();

			entity.Property(e => e.MaxValue)
				.IsRequired();

			entity.Property(e => e.MinValue)
				.IsRequired();

			entity.Property(e => e.ProcessedAt)
				.IsRequired();

			entity.HasIndex(e => e.FileName)
				.IsUnique()
				.HasDatabaseName("IX_Results_FileName");
		});
	}
}