using App.Infrastructure.Entities;

namespace App.Infrastructure.Repositories;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(int id);
    Task<List<Reservation>> GetByEventIdAsync(int eventId);
    Task<List<Reservation>> GetByCustomerEmailAsync(string customerEmail);
    Task AddAsync(Reservation newReservation);
    Task UpdateAsync(Reservation reservation);
    Task<TicketsSummary> GetTicketsSummaryByEventIdAsync(int eventId);
    Task<bool> ExistsByReservationCodeAsync(string reservationCode);
}
