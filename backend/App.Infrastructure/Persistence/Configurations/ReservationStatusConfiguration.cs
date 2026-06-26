using App.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public class ReservationStatusConfiguration : IEntityTypeConfiguration<ReservationStatus>
{
    public void Configure(EntityTypeBuilder<ReservationStatus> builder)
    {
        builder.ToTable("ReservationStatus", "Reservations");

        builder.HasKey(reservationStatus => reservationStatus.Id);

        builder.Property(reservationStatus => reservationStatus.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasData(
            new ReservationStatus { Id = 1, Name = "PendingPayment" },
            new ReservationStatus { Id = 2, Name = "Confirmed" },
            new ReservationStatus { Id = 3, Name = "Cancelled" },
            new ReservationStatus { Id = 4, Name = "Lost" }
        );
    }
}
