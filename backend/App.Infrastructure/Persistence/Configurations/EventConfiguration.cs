using App.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Event", "Events");

        builder.HasKey(eventEntity => eventEntity.Id);

        builder.Property(eventEntity => eventEntity.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(eventEntity => eventEntity.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(eventEntity => eventEntity.MaxCapacity)
            .IsRequired();

        builder.Property(eventEntity => eventEntity.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(eventEntity => eventEntity.CreatedDate)
            .IsRequired();

        builder.Property(eventEntity => eventEntity.UpdatedDate)
            .IsRequired();

        builder.Property(eventEntity => eventEntity.StartDate)
            .IsRequired();

        builder.Property(eventEntity => eventEntity.EndDate)
            .IsRequired();

        builder.HasOne(eventEntity => eventEntity.EventType)
            .WithMany(eventType => eventType.Events)
            .HasForeignKey(eventEntity => eventEntity.EventTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(eventEntity => eventEntity.EventStatus)
            .WithMany(eventStatus => eventStatus.Events)
            .HasForeignKey(eventEntity => eventEntity.EventStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(eventEntity => eventEntity.Venue)
            .WithMany(venue => venue.Events)
            .HasForeignKey(eventEntity => eventEntity.VenueId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
