using App.Common.DTOs.Reservations;

namespace App.Domain.Services;

public interface IReservationService
{
    /// <summary>
    /// Creates a reservation in PendingPayment status after validating event existence, that the event is Active
    /// (Design decision #10), the 1-hour cutoff (BR-04), ticket availability, and the per-transaction quantity
    /// limit (FR-03 priority over BR-05). Resolves the Customer by email, creating one if needed, and persists
    /// both atomically.
    /// </summary>
    Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request);

    /// <summary>
    /// Confirms payment for a PendingPayment reservation, generating a unique ReservationCode and transitioning it to Confirmed.
    /// </summary>
    Task<ReservationResponse> ConfirmPaymentAsync(int reservationId);

    /// <summary>
    /// Cancels a Confirmed reservation, applying the late-cancellation penalty (BR-07) when the event starts in less than 48 hours.
    /// </summary>
    Task<ReservationResponse> CancelReservationAsync(int reservationId);

    /// <summary>
    /// Administrative action that releases a Lost reservation's tickets by transitioning it to Cancelled.
    /// </summary>
    Task<ReservationResponse> ReleaseLostReservationAsync(int reservationId);

    /// <summary>
    /// Retrieves all reservations for a given event.
    /// </summary>
    Task<List<ReservationResponse>> GetReservationsByEventIdAsync(int eventId);
}
