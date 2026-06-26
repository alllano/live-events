using App.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservation", "Users");

        builder.HasKey(reservation => reservation.Id);

        builder.Property(reservation => reservation.TicketQuantity)
            .IsRequired();

        builder.Property(reservation => reservation.TotalPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(reservation => reservation.ReservationDate)
            .IsRequired();

        builder.Property(reservation => reservation.CancelledDate)
            .IsRequired(false);

        builder.Property(reservation => reservation.ReservationCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasOne(reservation => reservation.Customer)
            .WithMany(customer => customer.Reservations)
            .HasForeignKey(reservation => reservation.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(reservation => reservation.Event)
            .WithMany(eventEntity => eventEntity.Reservations)
            .HasForeignKey(reservation => reservation.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(reservation => reservation.ReservationStatus)
            .WithMany(reservationStatus => reservationStatus.Reservations)
            .HasForeignKey(reservation => reservation.ReservationStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
