using App.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public class LogConfiguration : IEntityTypeConfiguration<Log>
{
    public void Configure(EntityTypeBuilder<Log> builder)
    {
        builder.ToTable("Log", "dbo");

        builder.HasKey(log => log.Id);

        builder.Property(log => log.CreatedDate)
            .IsRequired();

        builder.Property(log => log.Error)
            .IsRequired();

        builder.Property(log => log.StackTrace)
            .IsRequired();

        builder.Property(log => log.Request)
            .IsRequired();

        builder.HasOne(log => log.LogType)
            .WithMany(logType => logType.Logs)
            .HasForeignKey(log => log.LogTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
