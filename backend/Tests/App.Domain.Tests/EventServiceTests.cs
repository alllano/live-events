using App.Common.Constants;
using App.Common.DTOs.Events;
using App.Common.DTOs.Reports;
using App.Domain.Exceptions;
using App.Domain.Services;
using App.Infrastructure.Entities;
using App.Infrastructure.Persistence;
using App.Infrastructure.Repositories;
using App.Infrastructure.Validators;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ValidationException = App.Domain.Exceptions.ValidationException;

namespace App.Domain.Tests;

public class EventServiceTests
{
    [Fact]
    public async Task CreateEventAsync_WhenMaxCapacityExceedsVenueCapacity_ThrowsBusinessRuleException()
    {
        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock.Setup(repo => repo.GetVenueCapacityAsync(It.IsAny<int>())).ReturnsAsync(50);

        EventService eventService = CreateService(eventRepositoryMock, new Mock<IReservationRepository>(), new Mock<IUnitOfWork>(), CreateAlwaysValidValidatorMock().Object);
        CreateEventRequest request = CreateValidEventRequest(maxCapacity: 100);

        await Assert.ThrowsAsync<BusinessRuleException>(() => eventService.CreateEventAsync(request));
    }

    [Fact]
    public async Task CreateEventAsync_WhenMaxCapacityEqualsVenueCapacity_CreatesEventSuccessfully()
    {
        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock.Setup(repo => repo.GetVenueCapacityAsync(It.IsAny<int>())).ReturnsAsync(100);
        eventRepositoryMock.Setup(repo => repo.GetOverlappingEventsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>())).ReturnsAsync(new List<Event>());
        eventRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Event>())).Returns(Task.CompletedTask);
        eventRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CreateActiveEvent(maxCapacity: 100));

        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetTicketsSummaryByEventIdAsync(It.IsAny<int>())).ReturnsAsync(new TicketsSummary(0, 0, 0));

        Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

        EventService eventService = CreateService(eventRepositoryMock, reservationRepositoryMock, unitOfWorkMock, CreateAlwaysValidValidatorMock().Object);
        CreateEventRequest request = CreateValidEventRequest(maxCapacity: 100, startDate: NextWeekdayAt(10));

        EventDetailResponse response = await eventService.CreateEventAsync(request);

        Assert.Equal("Active", response.EventStatusName);
        Assert.Equal(100, response.AvailableTickets);
    }

    [Fact]
    public async Task CreateEventAsync_WhenVenueHasOverlappingActiveEvent_ThrowsBusinessRuleException()
    {
        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock.Setup(repo => repo.GetVenueCapacityAsync(It.IsAny<int>())).ReturnsAsync(200);
        eventRepositoryMock.Setup(repo => repo.GetOverlappingEventsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
            .ReturnsAsync(new List<Event> { CreateActiveEvent(id: 99) });

        EventService eventService = CreateService(eventRepositoryMock, new Mock<IReservationRepository>(), new Mock<IUnitOfWork>(), CreateAlwaysValidValidatorMock().Object);
        CreateEventRequest request = CreateValidEventRequest(maxCapacity: 100, startDate: NextWeekdayAt(10));

        await Assert.ThrowsAsync<BusinessRuleException>(() => eventService.CreateEventAsync(request));
    }

    [Fact]
    public async Task CreateEventAsync_WhenVenueHasNoOverlappingEvents_CreatesEventSuccessfully()
    {
        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock.Setup(repo => repo.GetVenueCapacityAsync(It.IsAny<int>())).ReturnsAsync(200);
        eventRepositoryMock.Setup(repo => repo.GetOverlappingEventsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>())).ReturnsAsync(new List<Event>());
        eventRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Event>())).Returns(Task.CompletedTask);
        eventRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CreateActiveEvent(maxCapacity: 100));

        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetTicketsSummaryByEventIdAsync(It.IsAny<int>())).ReturnsAsync(new TicketsSummary(0, 0, 0));

        Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

        EventService eventService = CreateService(eventRepositoryMock, reservationRepositoryMock, unitOfWorkMock, CreateAlwaysValidValidatorMock().Object);
        CreateEventRequest request = CreateValidEventRequest(maxCapacity: 100, startDate: NextWeekdayAt(10));

        EventDetailResponse response = await eventService.CreateEventAsync(request);

        Assert.NotNull(response);
        eventRepositoryMock.Verify(repo => repo.GetOverlappingEventsAsync(request.VenueId, request.StartDate, request.EndDate, null), Times.Once);
    }

    [Fact]
    public async Task CreateEventAsync_WhenStartDateIsWeekendAfter10PM_ThrowsBusinessRuleException()
    {
        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock.Setup(repo => repo.GetVenueCapacityAsync(It.IsAny<int>())).ReturnsAsync(200);
        eventRepositoryMock.Setup(repo => repo.GetOverlappingEventsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>())).ReturnsAsync(new List<Event>());

        EventService eventService = CreateService(eventRepositoryMock, new Mock<IReservationRepository>(), new Mock<IUnitOfWork>(), CreateAlwaysValidValidatorMock().Object);
        CreateEventRequest request = CreateValidEventRequest(maxCapacity: 100, startDate: NextSaturdayAt(23));

        await Assert.ThrowsAsync<BusinessRuleException>(() => eventService.CreateEventAsync(request));
    }

    [Fact]
    public async Task CreateEventAsync_WhenStartDateIsWeekendBefore10PM_CreatesEventSuccessfully()
    {
        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock.Setup(repo => repo.GetVenueCapacityAsync(It.IsAny<int>())).ReturnsAsync(200);
        eventRepositoryMock.Setup(repo => repo.GetOverlappingEventsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>())).ReturnsAsync(new List<Event>());
        eventRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Event>())).Returns(Task.CompletedTask);
        eventRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CreateActiveEvent(maxCapacity: 100));

        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetTicketsSummaryByEventIdAsync(It.IsAny<int>())).ReturnsAsync(new TicketsSummary(0, 0, 0));

        Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

        EventService eventService = CreateService(eventRepositoryMock, reservationRepositoryMock, unitOfWorkMock, CreateAlwaysValidValidatorMock().Object);
        CreateEventRequest request = CreateValidEventRequest(maxCapacity: 100, startDate: NextSaturdayAt(18));

        EventDetailResponse response = await eventService.CreateEventAsync(request);

        Assert.NotNull(response);
    }

    [Fact]
    public async Task CreateEventAsync_WhenNameIsTooShort_ThrowsValidationException()
    {
        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        CreateEventRequestValidator realValidator = new CreateEventRequestValidator();

        EventService eventService = CreateService(eventRepositoryMock, new Mock<IReservationRepository>(), new Mock<IUnitOfWork>(), realValidator);
        CreateEventRequest request = CreateValidEventRequest();
        request.Name = "Ab";

        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(() => eventService.CreateEventAsync(request));

        Assert.Contains("Name", exception.Message);
        eventRepositoryMock.Verify(repo => repo.GetVenueCapacityAsync(It.IsAny<int>()), Times.Never);
    }

    // ----- GetOccupancyReportAsync -----

    [Fact]
    public async Task GetOccupancyReportAsync_WhenEventDoesNotExist_ThrowsNotFoundException()
    {
        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Event?)null);

        EventService eventService = CreateService(eventRepositoryMock, new Mock<IReservationRepository>(), new Mock<IUnitOfWork>(), CreateAlwaysValidValidatorMock().Object);

        await Assert.ThrowsAsync<NotFoundException>(() => eventService.GetOccupancyReportAsync(1));
    }

    [Fact]
    public async Task GetOccupancyReportAsync_WhenEventHasConfirmedPendingAndLostReservations_CalculatesReportCorrectly()
    {
        Event existingEvent = CreateActiveEvent(maxCapacity: 100, price: 50m);

        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(existingEvent);

        // 20 Confirmed (sold), 5 PendingPayment, 5 Lost: all three block availability, but only Confirmed counts as sold/revenue.
        Mock<IReservationRepository> reservationRepositoryMock = new Mock<IReservationRepository>();
        reservationRepositoryMock.Setup(repo => repo.GetTicketsSummaryByEventIdAsync(existingEvent.Id)).ReturnsAsync(new TicketsSummary(20, 5, 5));

        EventService eventService = CreateService(eventRepositoryMock, reservationRepositoryMock, new Mock<IUnitOfWork>(), CreateAlwaysValidValidatorMock().Object);

        OccupancyReportResponse report = await eventService.GetOccupancyReportAsync(existingEvent.Id);

        Assert.Equal(20, report.TicketsSold);
        Assert.Equal(70, report.TicketsAvailable);
        Assert.Equal(20m, report.OccupancyPercentage);
        Assert.Equal(1000m, report.TotalRevenue);
        Assert.Equal("Active", report.EventStatusName);
    }

    private static EventService CreateService(
        Mock<IEventRepository> eventRepositoryMock,
        Mock<IReservationRepository> reservationRepositoryMock,
        Mock<IUnitOfWork> unitOfWorkMock,
        IValidator<CreateEventRequest> validator)
    {
        return new EventService(
            eventRepositoryMock.Object,
            reservationRepositoryMock.Object,
            TestMapperFactory.Create(),
            unitOfWorkMock.Object,
            validator);
    }

    private static Mock<IValidator<CreateEventRequest>> CreateAlwaysValidValidatorMock()
    {
        Mock<IValidator<CreateEventRequest>> validatorMock = new Mock<IValidator<CreateEventRequest>>();
        validatorMock.Setup(validator => validator.Validate(It.IsAny<CreateEventRequest>())).Returns(new ValidationResult());
        return validatorMock;
    }

    private static CreateEventRequest CreateValidEventRequest(int venueId = 1, int maxCapacity = 100, DateTime? startDate = null, decimal price = 50m, int eventTypeId = 1)
    {
        DateTime resolvedStartDate = startDate ?? NextWeekdayAt(10);

        return new CreateEventRequest
        {
            Name = "Sample Conference Event",
            Description = "A sample event description used for testing purposes only.",
            VenueId = venueId,
            MaxCapacity = maxCapacity,
            StartDate = resolvedStartDate,
            EndDate = resolvedStartDate.AddHours(2),
            Price = price,
            EventTypeId = eventTypeId
        };
    }

    private static Event CreateActiveEvent(int id = 1, int venueId = 1, DateTime? startDate = null, decimal price = 50m, int maxCapacity = 100)
    {
        DateTime resolvedStartDate = startDate ?? NextWeekdayAt(10);

        return new Event
        {
            Id = id,
            Name = "Sample Conference Event",
            Description = "A sample event description used for testing purposes only.",
            EventTypeId = 1,
            EventStatusId = EventStatusIds.Active,
            VenueId = venueId,
            CreatedDate = DateTime.Now,
            UpdatedDate = DateTime.Now,
            StartDate = resolvedStartDate,
            EndDate = resolvedStartDate.AddHours(2),
            MaxCapacity = maxCapacity,
            Price = price,
            EventType = new EventType { Id = 1, Name = "Conference" },
            EventStatus = new EventStatus { Id = EventStatusIds.Active, Name = "Active" },
            Venue = new Venue { Id = venueId, Name = "Auditorio Central", Capacity = 200, CityId = 1 }
        };
    }

    private static DateTime NextWeekdayAt(int hour)
    {
        DateTime candidate = DateTime.Now.Date.AddDays(7).AddHours(hour);
        while (candidate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            candidate = candidate.AddDays(1);
        }

        return candidate;
    }

    private static DateTime NextSaturdayAt(int hour)
    {
        DateTime candidate = DateTime.Now.Date.AddDays(7);
        while (candidate.DayOfWeek != DayOfWeek.Saturday)
        {
            candidate = candidate.AddDays(1);
        }

        return candidate.AddHours(hour);
    }
}
