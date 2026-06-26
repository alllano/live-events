using App.Common.Constants;
using App.Common.DTOs.Events;
using App.Domain.Exceptions;
using App.Infrastructure.Entities;
using App.Infrastructure.Persistence;
using App.Infrastructure.Repositories;
using AutoMapper;

namespace App.Domain.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public EventService(
        IEventRepository eventRepository,
        IReservationRepository reservationRepository,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _reservationRepository = reservationRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventDetailResponse> CreateEventAsync(CreateEventRequest request)
    {
        int? venueCapacity = await _eventRepository.GetVenueCapacityAsync(request.VenueId);
        if (venueCapacity is null)
        {
            throw new NotFoundException($"Venue with id {request.VenueId} was not found.");
        }

        if (request.MaxCapacity > venueCapacity.Value)
        {
            throw new BusinessRuleException($"MaxCapacity ({request.MaxCapacity}) cannot exceed the venue's capacity ({venueCapacity.Value}).");
        }

        List<Event> overlappingEvents = await _eventRepository.GetOverlappingEventsAsync(request.VenueId, request.StartDate, request.EndDate, excludeEventId: null);
        if (overlappingEvents.Count > 0)
        {
            throw new BusinessRuleException("The venue already has an active event scheduled in an overlapping time range.");
        }

        if (ViolatesWeekendNightRestriction(request.StartDate))
        {
            throw new BusinessRuleException("Weekend events cannot start after 22:00.");
        }

        Event newEvent = _mapper.Map<Event>(request);
        newEvent.EventStatusId = EventStatusIds.Active;
        newEvent.CreatedDate = DateTime.Now;
        newEvent.UpdatedDate = DateTime.Now;

        await _eventRepository.AddAsync(newEvent);
        await _unitOfWork.SaveChangesAsync();

        return await GetEventByIdAsync(newEvent.Id);
    }

    public async Task<EventDetailResponse> GetEventByIdAsync(int id)
    {
        Event? existingEvent = await _eventRepository.GetByIdAsync(id);
        if (existingEvent is null)
        {
            throw new NotFoundException($"Event with id {id} was not found.");
        }

        EventDetailResponse response = _mapper.Map<EventDetailResponse>(existingEvent);
        response.EventStatusName = ResolveEffectiveEventStatusName(existingEvent);

        TicketsSummary ticketsSummary = await _reservationRepository.GetTicketsSummaryByEventIdAsync(existingEvent.Id);
        response.AvailableTickets = TicketAvailabilityCalculator.CalculateAvailableTickets(existingEvent.MaxCapacity, ticketsSummary);

        return response;
    }

    public async Task<List<EventListItemResponse>> GetEventsAsync(EventFilterRequest filter)
    {
        List<Event> events = await _eventRepository.GetAllAsync(filter);

        List<EventListItemResponse> responses = new List<EventListItemResponse>();
        foreach (Event eventEntity in events)
        {
            EventListItemResponse response = _mapper.Map<EventListItemResponse>(eventEntity);
            response.EventStatusName = ResolveEffectiveEventStatusName(eventEntity);
            responses.Add(response);
        }

        return responses;
    }

    // BR-06: an Active event is reported as "Completed" once its EndDate has elapsed, without persisting the change; any other persisted status (e.g. Cancelled) is reported as-is.
    private static string ResolveEffectiveEventStatusName(Event eventEntity)
    {
        if (eventEntity.EventStatus.Name != "Active")
        {
            return eventEntity.EventStatus.Name;
        }

        return DateTime.Now > eventEntity.EndDate ? "Completed" : "Active";
    }

    private static bool ViolatesWeekendNightRestriction(DateTime startDate)
    {
        bool isWeekend = startDate.DayOfWeek == DayOfWeek.Saturday || startDate.DayOfWeek == DayOfWeek.Sunday;
        return isWeekend && startDate.TimeOfDay > new TimeSpan(22, 0, 0);
    }
}
