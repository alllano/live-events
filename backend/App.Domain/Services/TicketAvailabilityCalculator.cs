using App.Infrastructure.Repositories;

namespace App.Domain.Services;

public static class TicketAvailabilityCalculator
{
    public static int CalculateAvailableTickets(int maxCapacity, TicketsSummary ticketsSummary)
    {
        return maxCapacity - ticketsSummary.ConfirmedQuantity - ticketsSummary.PendingPaymentQuantity - ticketsSummary.LostQuantity;
    }
}
