using App.Common.Constants;
using App.Common.DTOs.Reservations;
using App.Domain.Exceptions;
using App.Domain.Services;
using App.Infrastructure.Entities;
using App.Infrastructure.Persistence;
using App.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace App.Domain.Tests;

public class ReservationServiceTests
{
    // ----- CreateReservationAsync -----

    [Fact]
    public async Task CreateReservationAsync_WhenEventDoesNotExist_ThrowsNotFoundException()
    {
        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Event?)null);

        ReservationService reservationService = CreateService(eventRepositoryMock, new Mock<IReservationRepository>(), new Mock<ICustomerRepository>(), new Mock<IUnitOfWork>(), CreateAlwaysValidValidatorMock().Object);
        CreateReservationRequest request = CreateValidReservationRequest();

        await Assert.ThrowsAsync<NotFoundException>(() => reservationService.CreateReservationAsync(request));
    }

    [Fact]
    public async Task CreateReservationAsync_WhenEventIsNotActive_ThrowsBusinessRuleException()
    {
        Event cancelledEvent = CreateEvent(eventStatusId: EventStatusIds.Cancelled, startDate: DateTime.Now.AddDays(10));

        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(cancelledEvent);

        ReservationService reservationService = CreateService(eventRepositoryMock, new Mock<IReservationRepository>(), new Mock<ICustomerRepository>(), new Mock<IUnitOfWork>(), CreateAlwaysValidValidatorMock().Object);
        CreateReservationRequest request = CreateValidReservationRequest();

        await Assert.ThrowsAsync<BusinessRuleException>(() => reservationService.CreateReservationAsync(request));
    }

    [Fact]
    public async Task CreateReservationAsync_WhenEventStartsInLessThanOneHour_ThrowsBusinessRuleException()
    {
        Event soonEvent = CreateEvent(startDate: DateTime.Now.AddMinutes(30));

        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(soonEvent);

        ReservationService reservationService = CreateService(eventRepositoryMock, new Mock<IReservationRepository>(), new Mock<ICustomerRepository>(), new Mock<IUnitOfWork>(), CreateAlwaysValidValidatorMock().Object);
        CreateReservationRequest request = CreateValidReservationRequest();

        await Assert.ThrowsAsync<BusinessRuleException>(() => reservationService.CreateReservationAsync(request));
    }

    [Fact]
    public async Task CreateReservationAsync_WhenTicketQuantityExceedsAvailability_ThrowsBusinessRuleException()
    {
        Event futureEvent = CreateEvent(startDate: DateTime.Now.AddDays(10), maxCapacity: 10);

        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(futureEvent);

        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetTicketsSummaryByEventIdAsync(It.IsAny<int>())).ReturnsAsync(new TicketsSummary(8, 0, 0));

        ReservationService reservationService = CreateService(eventRepositoryMock, reservationRepositoryMock, new Mock<ICustomerRepository>(), new Mock<IUnitOfWork>(), CreateAlwaysValidValidatorMock().Object);
        CreateReservationRequest request = CreateValidReservationRequest(ticketQuantity: 5);

        await Assert.ThrowsAsync<BusinessRuleException>(() => reservationService.CreateReservationAsync(request));
    }

    [Fact]
    public async Task CreateReservationAsync_WhenEventStartsWithin24HoursAndPriceExceeds100_LimitsToFiveNotTen()
    {
        Event soonExpensiveEvent = CreateEvent(startDate: DateTime.Now.AddHours(20), price: 150m, maxCapacity: 100);

        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(soonExpensiveEvent);

        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetTicketsSummaryByEventIdAsync(It.IsAny<int>())).ReturnsAsync(new TicketsSummary(0, 0, 0));

        ReservationService reservationService = CreateService(eventRepositoryMock, reservationRepositoryMock, new Mock<ICustomerRepository>(), new Mock<IUnitOfWork>(), CreateAlwaysValidValidatorMock().Object);
        CreateReservationRequest request = CreateValidReservationRequest(ticketQuantity: 6);

        BusinessRuleException exception = await Assert.ThrowsAsync<BusinessRuleException>(() => reservationService.CreateReservationAsync(request));

        Assert.Contains("24 hours", exception.Message);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenEventStartsWithin24HoursAndPriceExceeds100_AllowsExactlyFiveTickets()
    {
        Event soonExpensiveEvent = CreateEvent(startDate: DateTime.Now.AddHours(20), price: 150m, maxCapacity: 100);
        Customer customer = CreateCustomer();

        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(soonExpensiveEvent);

        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetTicketsSummaryByEventIdAsync(It.IsAny<int>())).ReturnsAsync(new TicketsSummary(0, 0, 0));
        reservationRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Reservation>())).Returns(Task.CompletedTask);
        reservationRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(CreateReservation(1, soonExpensiveEvent, customer, ReservationStatusIds.PendingPayment, ticketQuantity: 5));

        Mock<ICustomerRepository> customerRepositoryMock = new Mock<ICustomerRepository>();
        customerRepositoryMock.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(customer);

        Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

        ReservationService reservationService = CreateService(eventRepositoryMock, reservationRepositoryMock, customerRepositoryMock, unitOfWorkMock, CreateAlwaysValidValidatorMock().Object);
        CreateReservationRequest request = CreateValidReservationRequest(ticketQuantity: 5);

        ReservationResponse response = await reservationService.CreateReservationAsync(request);

        Assert.Equal(5, response.TicketQuantity);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenCustomerEmailAlreadyExists_ReusesExistingCustomer()
    {
        Event futureEvent = CreateEvent(startDate: DateTime.Now.AddDays(10));
        Customer existingCustomer = CreateCustomer(id: 42, email: "existing@example.com");

        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(futureEvent);

        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetTicketsSummaryByEventIdAsync(It.IsAny<int>())).ReturnsAsync(new TicketsSummary(0, 0, 0));
        reservationRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Reservation>())).Returns(Task.CompletedTask);
        reservationRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(CreateReservation(1, futureEvent, existingCustomer, ReservationStatusIds.PendingPayment));

        Mock<ICustomerRepository> customerRepositoryMock = new Mock<ICustomerRepository>();
        customerRepositoryMock.Setup(repo => repo.GetByEmailAsync("existing@example.com")).ReturnsAsync(existingCustomer);

        Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

        ReservationService reservationService = CreateService(eventRepositoryMock, reservationRepositoryMock, customerRepositoryMock, unitOfWorkMock, CreateAlwaysValidValidatorMock().Object);
        CreateReservationRequest request = CreateValidReservationRequest(customerEmail: "existing@example.com");

        await reservationService.CreateReservationAsync(request);

        customerRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenCustomerEmailDoesNotExist_CreatesNewCustomer()
    {
        Event futureEvent = CreateEvent(startDate: DateTime.Now.AddDays(10));

        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(futureEvent);

        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetTicketsSummaryByEventIdAsync(It.IsAny<int>())).ReturnsAsync(new TicketsSummary(0, 0, 0));
        reservationRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Reservation>())).Returns(Task.CompletedTask);
        reservationRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(CreateReservation(1, futureEvent, CreateCustomer(email: "new@example.com"), ReservationStatusIds.PendingPayment));

        Mock<ICustomerRepository> customerRepositoryMock = new Mock<ICustomerRepository>();
        customerRepositoryMock.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((Customer?)null);
        customerRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Customer>())).Returns(Task.CompletedTask);

        Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

        ReservationService reservationService = CreateService(eventRepositoryMock, reservationRepositoryMock, customerRepositoryMock, unitOfWorkMock, CreateAlwaysValidValidatorMock().Object);
        CreateReservationRequest request = CreateValidReservationRequest(customerEmail: "new@example.com");

        await reservationService.CreateReservationAsync(request);

        customerRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Customer>(customer => customer.Email == "new@example.com")), Times.Once);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenAllValidationsPass_CreatesReservationSuccessfully()
    {
        Event futureEvent = CreateEvent(startDate: DateTime.Now.AddDays(10), price: 50m, maxCapacity: 100);
        Customer customer = CreateCustomer();

        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(futureEvent);

        Reservation? capturedReservation = null;
        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetTicketsSummaryByEventIdAsync(It.IsAny<int>())).ReturnsAsync(new TicketsSummary(0, 0, 0));
        reservationRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(reservation => capturedReservation = reservation)
            .Returns(Task.CompletedTask);
        reservationRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(CreateReservation(1, futureEvent, customer, ReservationStatusIds.PendingPayment, ticketQuantity: 3));

        Mock<ICustomerRepository> customerRepositoryMock = new Mock<ICustomerRepository>();
        customerRepositoryMock.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(customer);

        Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

        ReservationService reservationService = CreateService(eventRepositoryMock, reservationRepositoryMock, customerRepositoryMock, unitOfWorkMock, CreateAlwaysValidValidatorMock().Object);
        CreateReservationRequest request = CreateValidReservationRequest(ticketQuantity: 3);

        ReservationResponse response = await reservationService.CreateReservationAsync(request);

        Assert.Equal("PendingPayment", response.ReservationStatusName);

        Assert.NotNull(capturedReservation);
        Assert.Equal(ReservationStatusIds.PendingPayment, capturedReservation.ReservationStatusId);
        Assert.Equal(150m, capturedReservation.TotalPrice);
        Assert.Equal(string.Empty, capturedReservation.ReservationCode);
        Assert.Equal(customer.Email, capturedReservation.Customer.Email);
    }

    // ----- ConfirmPaymentAsync -----

    [Fact]
    public async Task ConfirmPaymentAsync_WhenReservationDoesNotExist_ThrowsNotFoundException()
    {
        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Reservation?)null);

        ReservationService reservationService = CreateService(new Mock<IEventRepository>(), reservationRepositoryMock, new Mock<ICustomerRepository>(), new Mock<IUnitOfWork>(), CreateAlwaysValidValidatorMock().Object);

        await Assert.ThrowsAsync<NotFoundException>(() => reservationService.ConfirmPaymentAsync(1));
    }

    [Fact]
    public async Task ConfirmPaymentAsync_WhenReservationIsAlreadyConfirmed_ThrowsInvalidStateTransitionException()
    {
        Reservation confirmedReservation = CreateReservation(1, CreateEvent(), CreateCustomer(), ReservationStatusIds.Confirmed);

        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(confirmedReservation);

        ReservationService reservationService = CreateService(new Mock<IEventRepository>(), reservationRepositoryMock, new Mock<ICustomerRepository>(), new Mock<IUnitOfWork>(), CreateAlwaysValidValidatorMock().Object);

        await Assert.ThrowsAsync<InvalidStateTransitionException>(() => reservationService.ConfirmPaymentAsync(1));
    }

    [Fact]
    public async Task ConfirmPaymentAsync_WhenReservationIsPendingPayment_ConfirmsSuccessfully()
    {
        Reservation pendingReservation = CreateReservation(1, CreateEvent(), CreateCustomer(), ReservationStatusIds.PendingPayment);

        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(pendingReservation);
        reservationRepositoryMock.Setup(repo => repo.ExistsByReservationCodeAsync(It.IsAny<string>())).ReturnsAsync(false);
        reservationRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Reservation>())).Returns(Task.CompletedTask);

        Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

        ReservationService reservationService = CreateService(new Mock<IEventRepository>(), reservationRepositoryMock, new Mock<ICustomerRepository>(), unitOfWorkMock, CreateAlwaysValidValidatorMock().Object);

        ReservationResponse response = await reservationService.ConfirmPaymentAsync(1);

        Assert.Equal("Confirmed", response.ReservationStatusName);
        Assert.Matches(@"^EV-\d{6}$", response.ReservationCode);
        Assert.Equal(ReservationStatusIds.Confirmed, pendingReservation.ReservationStatusId);
    }

    // ----- CancelReservationAsync -----

    [Fact]
    public async Task CancelReservationAsync_WhenReservationIsPendingPayment_ThrowsInvalidStateTransitionException()
    {
        Reservation pendingReservation = CreateReservation(1, CreateEvent(), CreateCustomer(), ReservationStatusIds.PendingPayment);

        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(pendingReservation);

        ReservationService reservationService = CreateService(new Mock<IEventRepository>(), reservationRepositoryMock, new Mock<ICustomerRepository>(), new Mock<IUnitOfWork>(), CreateAlwaysValidValidatorMock().Object);

        await Assert.ThrowsAsync<InvalidStateTransitionException>(() => reservationService.CancelReservationAsync(1));
    }

    [Fact]
    public async Task CancelReservationAsync_WhenEventStartsInLessThan48Hours_TransitionsToLost()
    {
        Event soonEvent = CreateEvent(startDate: DateTime.Now.AddHours(20));
        Reservation confirmedReservation = CreateReservation(1, soonEvent, CreateCustomer(), ReservationStatusIds.Confirmed);

        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(confirmedReservation);
        reservationRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Reservation>())).Returns(Task.CompletedTask);

        Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

        ReservationService reservationService = CreateService(new Mock<IEventRepository>(), reservationRepositoryMock, new Mock<ICustomerRepository>(), unitOfWorkMock, CreateAlwaysValidValidatorMock().Object);

        ReservationResponse response = await reservationService.CancelReservationAsync(1);

        Assert.Equal("Lost", response.ReservationStatusName);
        Assert.Equal(ReservationStatusIds.Lost, confirmedReservation.ReservationStatusId);
        Assert.NotNull(confirmedReservation.CancelledDate);
    }

    [Fact]
    public async Task CancelReservationAsync_WhenEventStartsInMoreThan48Hours_TransitionsToCancelled()
    {
        Event laterEvent = CreateEvent(startDate: DateTime.Now.AddDays(10));
        Reservation confirmedReservation = CreateReservation(1, laterEvent, CreateCustomer(), ReservationStatusIds.Confirmed);

        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(confirmedReservation);
        reservationRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Reservation>())).Returns(Task.CompletedTask);

        Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

        ReservationService reservationService = CreateService(new Mock<IEventRepository>(), reservationRepositoryMock, new Mock<ICustomerRepository>(), unitOfWorkMock, CreateAlwaysValidValidatorMock().Object);

        ReservationResponse response = await reservationService.CancelReservationAsync(1);

        Assert.Equal("Cancelled", response.ReservationStatusName);
        Assert.Equal(ReservationStatusIds.Cancelled, confirmedReservation.ReservationStatusId);
    }

    // ----- ReleaseLostReservationAsync -----

    [Fact]
    public async Task ReleaseLostReservationAsync_WhenReservationIsNotLost_ThrowsInvalidStateTransitionException()
    {
        Reservation confirmedReservation = CreateReservation(1, CreateEvent(), CreateCustomer(), ReservationStatusIds.Confirmed);

        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(confirmedReservation);

        ReservationService reservationService = CreateService(new Mock<IEventRepository>(), reservationRepositoryMock, new Mock<ICustomerRepository>(), new Mock<IUnitOfWork>(), CreateAlwaysValidValidatorMock().Object);

        await Assert.ThrowsAsync<InvalidStateTransitionException>(() => reservationService.ReleaseLostReservationAsync(1));
    }

    [Fact]
    public async Task ReleaseLostReservationAsync_WhenReservationIsLost_ReleasesToCancelledSuccessfully()
    {
        Reservation lostReservation = CreateReservation(1, CreateEvent(), CreateCustomer(), ReservationStatusIds.Lost);

        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(lostReservation);
        reservationRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Reservation>())).Returns(Task.CompletedTask);

        Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

        ReservationService reservationService = CreateService(new Mock<IEventRepository>(), reservationRepositoryMock, new Mock<ICustomerRepository>(), unitOfWorkMock, CreateAlwaysValidValidatorMock().Object);

        ReservationResponse response = await reservationService.ReleaseLostReservationAsync(1);

        Assert.Equal("Cancelled", response.ReservationStatusName);
        Assert.Equal(ReservationStatusIds.Cancelled, lostReservation.ReservationStatusId);
    }

    // ----- GetReservationsByEventIdAsync -----

    [Fact]
    public async Task GetReservationsByEventIdAsync_WhenEventHasReservations_MapsResponsesWithResolvedStatusName()
    {
        Event eventEntity = CreateEvent();
        Customer customer = CreateCustomer();
        Reservation confirmedReservation = CreateReservation(1, eventEntity, customer, ReservationStatusIds.Confirmed, ticketQuantity: 2, reservationCode: "EV-123456");

        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetByEventIdAsync(eventEntity.Id)).ReturnsAsync(new List<Reservation> { confirmedReservation });

        ReservationService reservationService = CreateService(new Mock<IEventRepository>(), reservationRepositoryMock, new Mock<ICustomerRepository>(), new Mock<IUnitOfWork>(), CreateAlwaysValidValidatorMock().Object);

        List<ReservationResponse> responses = await reservationService.GetReservationsByEventIdAsync(eventEntity.Id);

        Assert.Single(responses);
        Assert.Equal("Confirmed", responses[0].ReservationStatusName);
        Assert.Equal("EV-123456", responses[0].ReservationCode);
        Assert.Equal(eventEntity.Name, responses[0].EventName);
        Assert.Equal(customer.Name, responses[0].CustomerName);
    }

    // ----- Shared helpers -----

    private static ReservationService CreateService(
        Mock<IEventRepository> eventRepositoryMock,
        Mock<IReservationRepository> reservationRepositoryMock,
        Mock<ICustomerRepository> customerRepositoryMock,
        Mock<IUnitOfWork> unitOfWorkMock,
        IValidator<CreateReservationRequest> validator)
    {
        return new ReservationService(
            eventRepositoryMock.Object,
            reservationRepositoryMock.Object,
            customerRepositoryMock.Object,
            TestMapperFactory.Create(),
            unitOfWorkMock.Object,
            validator);
    }

    private static Mock<IValidator<CreateReservationRequest>> CreateAlwaysValidValidatorMock()
    {
        Mock<IValidator<CreateReservationRequest>> validatorMock = new Mock<IValidator<CreateReservationRequest>>();
        validatorMock.Setup(validator => validator.Validate(It.IsAny<CreateReservationRequest>())).Returns(new ValidationResult());
        return validatorMock;
    }

    private static CreateReservationRequest CreateValidReservationRequest(int eventId = 1, int ticketQuantity = 2, string customerEmail = "john.doe@example.com", string customerName = "John Doe")
    {
        return new CreateReservationRequest
        {
            EventId = eventId,
            TicketQuantity = ticketQuantity,
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            CustomerPhone = "3000000000"
        };
    }

    private static Event CreateEvent(int id = 1, DateTime? startDate = null, decimal price = 50m, int maxCapacity = 100, int eventStatusId = EventStatusIds.Active)
    {
        DateTime resolvedStartDate = startDate ?? DateTime.Now.AddDays(10);

        return new Event
        {
            Id = id,
            Name = "Sample Event",
            Description = "Sample event description used for reservation tests.",
            EventTypeId = 1,
            EventStatusId = eventStatusId,
            VenueId = 1,
            CreatedDate = DateTime.Now,
            UpdatedDate = DateTime.Now,
            StartDate = resolvedStartDate,
            EndDate = resolvedStartDate.AddHours(2),
            MaxCapacity = maxCapacity,
            Price = price,
            EventType = new EventType { Id = 1, Name = "Conference" },
            EventStatus = new EventStatus { Id = eventStatusId, Name = eventStatusId == EventStatusIds.Active ? "Active" : "Cancelled" },
            Venue = new Venue { Id = 1, Name = "Auditorio Central", Capacity = 200, CityId = 1 }
        };
    }

    private static Customer CreateCustomer(int id = 1, string name = "John Doe", string email = "john.doe@example.com")
    {
        return new Customer { Id = id, Name = name, Email = email, Phone = "3000000000" };
    }

    private static ReservationStatus CreateReservationStatusEntity(int reservationStatusId)
    {
        string name = reservationStatusId switch
        {
            ReservationStatusIds.PendingPayment => "PendingPayment",
            ReservationStatusIds.Confirmed => "Confirmed",
            ReservationStatusIds.Cancelled => "Cancelled",
            ReservationStatusIds.Lost => "Lost",
            _ => throw new ArgumentOutOfRangeException(nameof(reservationStatusId))
        };

        return new ReservationStatus { Id = reservationStatusId, Name = name };
    }

    private static Reservation CreateReservation(int id, Event eventEntity, Customer customer, int reservationStatusId, int ticketQuantity = 2, string reservationCode = "")
    {
        return new Reservation
        {
            Id = id,
            EventId = eventEntity.Id,
            CustomerId = customer.Id,
            ReservationStatusId = reservationStatusId,
            TicketQuantity = ticketQuantity,
            TotalPrice = ticketQuantity * eventEntity.Price,
            ReservationDate = DateTime.Now,
            ReservationCode = reservationCode,
            Event = eventEntity,
            Customer = customer,
            ReservationStatus = CreateReservationStatusEntity(reservationStatusId)
        };
    }
}
