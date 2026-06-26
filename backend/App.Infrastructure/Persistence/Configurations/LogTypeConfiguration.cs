using App.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public class LogTypeConfiguration : IEntityTypeConfiguration<LogType>
{
    public void Configure(EntityTypeBuilder<LogType> builder)
    {
        builder.ToTable("LogType", "dbo");

        builder.HasKey(logType => logType.Id);

        builder.Property(logType => logType.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasData(
            new LogType { Id = 1, Name = "HandledError" },
            new LogType { Id = 2, Name = "UnhandledError" }
        );
    }
}
