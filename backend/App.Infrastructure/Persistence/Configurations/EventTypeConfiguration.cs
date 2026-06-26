using App.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public class EventTypeConfiguration : IEntityTypeConfiguration<EventType>
{
    public void Configure(EntityTypeBuilder<EventType> builder)
    {
        builder.ToTable("EventType", "Events");

        builder.HasKey(eventType => eventType.Id);

        builder.Property(eventType => eventType.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasData(
            new EventType { Id = 1, Name = "Conference" },
            new EventType { Id = 2, Name = "Workshop" },
            new EventType { Id = 3, Name = "Concert" }
        );
    }
}
