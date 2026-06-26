using App.Common.DTOs.Common;
using App.Common.DTOs.Reservations;
using App.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    /// <summary>
    /// Retrieves reservations filtered by event id or customer email (at least one is required).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ResponseDTO<List<ReservationResponse>>>> GetReservationsAsync([FromQuery] int? eventId, [FromQuery] string? customerEmail)
    {
        List<ReservationResponse> reservations = await _reservationService.GetReservationsAsync(eventId, customerEmail);
        ResponseDTO<List<ReservationResponse>> response = ResponseDTO<List<ReservationResponse>>.AsResponseDTO(reservations, StatusCodes.Status200OK);
        return Ok(response);
    }

    /// <summary>
    /// Creates a reservation in PendingPayment status for the given event and customer (FR-03).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ResponseDTO<ReservationResponse>>> CreateReservationAsync([FromBody] CreateReservationRequest request)
    {
        ReservationResponse createdReservation = await _reservationService.CreateReservationAsync(request);
        ResponseDTO<ReservationResponse> response = ResponseDTO<ReservationResponse>.AsResponseDTO(createdReservation, StatusCodes.Status201Created);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Confirms payment for a PendingPayment reservation, generating its unique ReservationCode (FR-04).
    /// </summary>
    [HttpPatch("{id}/confirm")]
    public async Task<ActionResult<ResponseDTO<ReservationResponse>>> ConfirmPaymentAsync([FromRoute] int id)
    {
        ReservationResponse confirmedReservation = await _reservationService.ConfirmPaymentAsync(id);
        ResponseDTO<ReservationResponse> response = ResponseDTO<ReservationResponse>.AsResponseDTO(confirmedReservation, StatusCodes.Status200OK);
        return Ok(response);
    }

    /// <summary>
    /// Cancels a Confirmed reservation (FR-05), applying the late-cancellation penalty (BR-07) when the event starts in less than 48 hours.
    /// </summary>
    [HttpPatch("{id}/cancel")]
    public async Task<ActionResult<ResponseDTO<ReservationResponse>>> CancelReservationAsync([FromRoute] int id)
    {
        ReservationResponse cancelledReservation = await _reservationService.CancelReservationAsync(id);
        ResponseDTO<ReservationResponse> response = ResponseDTO<ReservationResponse>.AsResponseDTO(cancelledReservation, StatusCodes.Status200OK);
        return Ok(response);
    }

    /// <summary>
    /// Administrative action that releases a Lost reservation's tickets by transitioning it to Cancelled.
    /// </summary>
    [HttpPatch("{id}/release")]
    public async Task<ActionResult<ResponseDTO<ReservationResponse>>> ReleaseLostReservationAsync([FromRoute] int id)
    {
        ReservationResponse releasedReservation = await _reservationService.ReleaseLostReservationAsync(id);
        ResponseDTO<ReservationResponse> response = ResponseDTO<ReservationResponse>.AsResponseDTO(releasedReservation, StatusCodes.Status200OK);
        return Ok(response);
    }
}
