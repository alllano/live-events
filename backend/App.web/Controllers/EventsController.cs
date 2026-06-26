using App.Common.DTOs.Common;
using App.Common.DTOs.Events;
using App.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>
    /// Creates a new event after validating venue capacity (BR-01), venue schedule overlap (BR-02),
    /// and the weekend night-time restriction (BR-03).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ResponseDTO<EventDetailResponse>>> CreateEventAsync([FromBody] CreateEventRequest request)
    {
        EventDetailResponse createdEvent = await _eventService.CreateEventAsync(request);
        ResponseDTO<EventDetailResponse> response = ResponseDTO<EventDetailResponse>.AsResponseDTO(createdEvent, StatusCodes.Status201Created);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Retrieves a single event by id, including its effective status (BR-06) and available tickets.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ResponseDTO<EventDetailResponse>>> GetEventByIdAsync([FromRoute] int id)
    {
        EventDetailResponse existingEvent = await _eventService.GetEventByIdAsync(id);
        ResponseDTO<EventDetailResponse> response = ResponseDTO<EventDetailResponse>.AsResponseDTO(existingEvent, StatusCodes.Status200OK);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves events matching the given optional filters (event type, date range, venue, status, title search).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ResponseDTO<List<EventListItemResponse>>>> GetEventsAsync([FromQuery] EventFilterRequest filter)
    {
        List<EventListItemResponse> events = await _eventService.GetEventsAsync(filter);
        ResponseDTO<List<EventListItemResponse>> response = ResponseDTO<List<EventListItemResponse>>.AsResponseDTO(events, StatusCodes.Status200OK);
        return Ok(response);
    }
}
