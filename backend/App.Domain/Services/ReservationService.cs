using App.Common.Constants;
using App.Common.DTOs.Reservations;
using App.Domain.Exceptions;
using App.Infrastructure.Entities;
using App.Infrastructure.Persistence;
using App.Infrastructure.Repositories;
using AutoMapper;
using FluentValidation;

namespace App.Domain.Services;

public class ReservationService : IReservationService
{
    private readonly IEventRepository _eventRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateReservationRequest> _createReservationRequestValidator;

    public ReservationService(
        IEventRepository eventRepository,
        IReservationRepository reservationRepository,
        ICustomerRepository customerRepository,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IValidator<CreateReservationRequest> createReservationRequestValidator)
    {
        _eventRepository = eventRepository;
        _reservationRepository = reservationRepository;
        _customerRepository = customerRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _createReservationRequestValidator = createReservationRequestValidator;
    }

    public async Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request)
    {
        RequestValidationHelper.ValidateOrThrow(_createReservationRequestValidator, request);

        Event? existingEvent = await _eventRepository.GetByIdAsync(request.EventId);
        if (existingEvent is null)
        {
            throw new NotFoundException($"Event with id {request.EventId} was not found.");
        }

        if (existingEvent.EventStatusId != EventStatusIds.Active)
        {
            throw new BusinessRuleException($"Cannot reserve tickets for event {request.EventId}: the event is not Active.");
        }

        if (existingEvent.StartDate < DateTime.Now.AddHours(1))
        {
            throw new BusinessRuleException("Reservations are not allowed when the event starts in less than 1 hour.");
        }

        TicketsSummary ticketsSummary = await _reservationRepository.GetTicketsSummaryByEventIdAsync(existingEvent.Id);
        int availableTickets = TicketAvailabilityCalculator.CalculateAvailableTickets(existingEvent.MaxCapacity, ticketsSummary);
        if (request.TicketQuantity > availableTickets)
        {
            throw new BusinessRuleException($"TicketQuantity ({request.TicketQuantity}) exceeds the {availableTickets} ticket(s) available for this event.");
        }

        (int? maxTicketsPerTransaction, string? limitReason) = DetermineMaxTicketsPerTransaction(existingEvent);
        if (maxTicketsPerTransaction.HasValue && request.TicketQuantity > maxTicketsPerTransaction.Value)
        {
            throw new BusinessRuleException($"TicketQuantity ({request.TicketQuantity}) exceeds the maximum of {maxTicketsPerTransaction.Value} tickets per transaction: {limitReason}.");
        }

        Customer? existingCustomer = await _customerRepository.GetByEmailAsync(request.CustomerEmail);
        Customer customer;
        if (existingCustomer is not null)
        {
            customer = existingCustomer;
        }
        else
        {
            customer = new Customer
            {
                Name = request.CustomerName,
                Email = request.CustomerEmail,
                Phone = request.CustomerPhone ?? string.Empty
            };
            await _customerRepository.AddAsync(customer);
        }

        Reservation newReservation = _mapper.Map<Reservation>(request);
        newReservation.Customer = customer;
        newReservation.ReservationStatusId = ReservationStatusIds.PendingPayment;
        newReservation.TotalPrice = request.TicketQuantity * existingEvent.Price;
        newReservation.ReservationDate = DateTime.Now;
        newReservation.ReservationCode = string.Empty;

        await _reservationRepository.AddAsync(newReservation);
        await _unitOfWork.SaveChangesAsync();

        Reservation createdReservation = await GetReservationOrThrowAsync(newReservation.Id);
        return _mapper.Map<ReservationResponse>(createdReservation);
    }

    public async Task<ReservationResponse> ConfirmPaymentAsync(int reservationId)
    {
        Reservation existingReservation = await GetReservationOrThrowAsync(reservationId);

        if (existingReservation.ReservationStatusId != ReservationStatusIds.PendingPayment)
        {
            throw new InvalidStateTransitionException($"Cannot confirm payment for reservation {reservationId}: current status is '{existingReservation.ReservationStatus.Name}', expected 'PendingPayment'.");
        }

        existingReservation.ReservationCode = await GenerateUniqueReservationCodeAsync();
        existingReservation.ReservationStatusId = ReservationStatusIds.Confirmed;

        await _reservationRepository.UpdateAsync(existingReservation);
        await _unitOfWork.SaveChangesAsync();

        return BuildResponseWithResolvedStatus(existingReservation);
    }

    public async Task<ReservationResponse> CancelReservationAsync(int reservationId)
    {
        Reservation existingReservation = await GetReservationOrThrowAsync(reservationId);

        if (existingReservation.ReservationStatusId != ReservationStatusIds.Confirmed)
        {
            throw new InvalidStateTransitionException($"Cannot cancel reservation {reservationId}: current status is '{existingReservation.ReservationStatus.Name}', expected 'Confirmed'.");
        }

        bool appliesLatePenalty = existingReservation.Event.StartDate < DateTime.Now.AddHours(48);
        existingReservation.ReservationStatusId = appliesLatePenalty ? ReservationStatusIds.Lost : ReservationStatusIds.Cancelled;
        existingReservation.CancelledDate = DateTime.Now;

        await _reservationRepository.UpdateAsync(existingReservation);
        await _unitOfWork.SaveChangesAsync();

        return BuildResponseWithResolvedStatus(existingReservation);
    }

    public async Task<ReservationResponse> ReleaseLostReservationAsync(int reservationId)
    {
        Reservation existingReservation = await GetReservationOrThrowAsync(reservationId);

        if (existingReservation.ReservationStatusId != ReservationStatusIds.Lost)
        {
            throw new InvalidStateTransitionException($"Cannot release reservation {reservationId}: current status is '{existingReservation.ReservationStatus.Name}', expected 'Lost'.");
        }

        existingReservation.ReservationStatusId = ReservationStatusIds.Cancelled;

        await _reservationRepository.UpdateAsync(existingReservation);
        await _unitOfWork.SaveChangesAsync();

        return BuildResponseWithResolvedStatus(existingReservation);
    }

    public async Task<List<ReservationResponse>> GetReservationsByEventIdAsync(int eventId)
    {
        List<Reservation> reservations = await _reservationRepository.GetByEventIdAsync(eventId);
        return _mapper.Map<List<ReservationResponse>>(reservations);
    }

    private async Task<Reservation> GetReservationOrThrowAsync(int reservationId)
    {
        Reservation? reservation = await _reservationRepository.GetByIdAsync(reservationId);
        if (reservation is null)
        {
            throw new NotFoundException($"Reservation with id {reservationId} was not found.");
        }

        return reservation;
    }

    // The ReservationStatus navigation loaded via Include() still points to the pre-update status object after an
    // in-memory ReservationStatusId change, so the resolved name is set explicitly instead of trusting AutoMapper's MapFrom here.
    private ReservationResponse BuildResponseWithResolvedStatus(Reservation reservation)
    {
        ReservationResponse response = _mapper.Map<ReservationResponse>(reservation);
        response.ReservationStatusName = ResolveReservationStatusName(reservation.ReservationStatusId);
        return response;
    }

    private static string ResolveReservationStatusName(int reservationStatusId)
    {
        if (reservationStatusId == ReservationStatusIds.PendingPayment)
        {
            return "PendingPayment";
        }

        if (reservationStatusId == ReservationStatusIds.Confirmed)
        {
            return "Confirmed";
        }

        if (reservationStatusId == ReservationStatusIds.Cancelled)
        {
            return "Cancelled";
        }

        if (reservationStatusId == ReservationStatusIds.Lost)
        {
            return "Lost";
        }

        throw new InvalidOperationException($"Unknown ReservationStatusId: {reservationStatusId}.");
    }

    // FR-03 takes priority over BR-05 when both would apply simultaneously.
    private static (int? Limit, string? Reason) DetermineMaxTicketsPerTransaction(Event eventEntity)
    {
        if (eventEntity.StartDate < DateTime.Now.AddHours(24))
        {
            return (5, "the event starts in less than 24 hours, which takes priority over the price-based limit (FR-03 over BR-05)");
        }

        if (eventEntity.Price > 100m)
        {
            return (10, "the event price exceeds $100 (BR-05)");
        }

        return (null, null);
    }

    private async Task<string> GenerateUniqueReservationCodeAsync()
    {
        string candidateCode;
        bool codeAlreadyExists;

        do
        {
            candidateCode = $"EV-{Random.Shared.Next(0, 1_000_000):D6}";
            codeAlreadyExists = await _reservationRepository.ExistsByReservationCodeAsync(candidateCode);
        } while (codeAlreadyExists);

        return candidateCode;
    }
}
