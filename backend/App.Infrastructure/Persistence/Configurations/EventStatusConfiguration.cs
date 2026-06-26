using App.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public class EventStatusConfiguration : IEntityTypeConfiguration<EventStatus>
{
    public void Configure(EntityTypeBuilder<EventStatus> builder)
    {
        builder.ToTable("EventStatus", "Events");

        builder.HasKey(eventStatus => eventStatus.Id);

        builder.Property(eventStatus => eventStatus.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasData(
            new EventStatus { Id = 1, Name = "Active" },
            new EventStatus { Id = 2, Name = "Cancelled" }
        );
    }
}
