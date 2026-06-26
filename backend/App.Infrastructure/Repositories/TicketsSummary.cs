namespace App.Infrastructure.Repositories;

public record TicketsSummary(int ConfirmedQuantity, int PendingPaymentQuantity, int LostQuantity);
