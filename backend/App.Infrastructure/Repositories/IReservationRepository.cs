using App.Infrastructure.Entities;

namespace App.Infrastructure.Repositories;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(int id);
    Task<List<Reservation>> GetByEventIdAsync(int eventId);
    Task AddAsync(Reservation newReservation);
    Task UpdateAsync(Reservation reservation);
    Task<TicketsSummary> GetTicketsSummaryByEventIdAsync(int eventId);
}
