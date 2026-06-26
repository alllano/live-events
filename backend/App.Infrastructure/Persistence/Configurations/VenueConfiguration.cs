using App.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("Venue", "Locations");

        builder.HasKey(venue => venue.Id);

        builder.Property(venue => venue.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(venue => venue.Capacity)
            .IsRequired();

        builder.HasOne(venue => venue.City)
            .WithMany(city => city.Venues)
            .HasForeignKey(venue => venue.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Venue { Id = 1, Name = "Auditorio Central", Capacity = 200, CityId = 1 },
            new Venue { Id = 2, Name = "Sala Norte", Capacity = 50, CityId = 1 },
            new Venue { Id = 3, Name = "Arena Sur", Capacity = 500, CityId = 2 }
        );
    }
}
