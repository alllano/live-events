using App.Infrastructure.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _context;

    public ReservationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Reservation?> GetByIdAsync(int id)
    {
        return await _context.Reservations
            .Include(reservation => reservation.Event)
            .Include(reservation => reservation.Customer)
            .Include(reservation => reservation.ReservationStatus)
            .FirstOrDefaultAsync(reservation => reservation.Id == id);
    }

    public async Task<List<Reservation>> GetByEventIdAsync(int eventId)
    {
        return await _context.Reservations
            .Include(reservation => reservation.Event)
            .Include(reservation => reservation.Customer)
            .Include(reservation => reservation.ReservationStatus)
            .Where(reservation => reservation.EventId == eventId)
            .ToListAsync();
    }

    public async Task<List<Reservation>> GetByCustomerEmailAsync(string customerEmail)
    {
        string customerEmailLowered = customerEmail.ToLower();

        return await _context.Reservations
            .Include(reservation => reservation.Event)
            .Include(reservation => reservation.Customer)
            .Include(reservation => reservation.ReservationStatus)
            .Where(reservation => reservation.Customer.Email.ToLower() == customerEmailLowered)
            .ToListAsync();
    }

    public async Task AddAsync(Reservation newReservation)
    {
        await _context.Reservations.AddAsync(newReservation);
    }

    public Task UpdateAsync(Reservation reservation)
    {
        _context.Reservations.Update(reservation);
        return Task.CompletedTask;
    }

    public async Task<TicketsSummary> GetTicketsSummaryByEventIdAsync(int eventId)
    {
        TicketsSummary? summary = await _context.Reservations
            .Where(reservation => reservation.EventId == eventId)
            .GroupBy(reservation => reservation.EventId)
            .Select(group => new TicketsSummary(
                group.Where(reservation => reservation.ReservationStatus.Name == "Confirmed").Sum(reservation => (int?)reservation.TicketQuantity) ?? 0,
                group.Where(reservation => reservation.ReservationStatus.Name == "PendingPayment").Sum(reservation => (int?)reservation.TicketQuantity) ?? 0,
                group.Where(reservation => reservation.ReservationStatus.Name == "Lost").Sum(reservation => (int?)reservation.TicketQuantity) ?? 0
            ))
            .FirstOrDefaultAsync();

        return summary ?? new TicketsSummary(0, 0, 0);
    }

    public async Task<bool> ExistsByReservationCodeAsync(string reservationCode)
    {
        return await _context.Reservations.AnyAsync(reservation => reservation.ReservationCode == reservationCode);
    }
}
