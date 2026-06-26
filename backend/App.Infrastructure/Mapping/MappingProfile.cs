using App.Common.DTOs.Events;
using App.Common.DTOs.Reservations;
using App.Infrastructure.Entities;
using AutoMapper;

namespace App.Infrastructure.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Event, EventListItemResponse>()
            .ForMember(destination => destination.EventTypeName, options => options.MapFrom(source => source.EventType.Name))
            .ForMember(destination => destination.VenueName, options => options.MapFrom(source => source.Venue.Name))
            // EventStatusName is the effective status (persisted EventStatus + EndDate < Now check, per DATA-MODEL.md "Design decisions" #2) — filled by the Service after mapping.
            .ForMember(destination => destination.EventStatusName, options => options.Ignore());

        CreateMap<Event, EventDetailResponse>()
            .ForMember(destination => destination.EventTypeName, options => options.MapFrom(source => source.EventType.Name))
            .ForMember(destination => destination.VenueName, options => options.MapFrom(source => source.Venue.Name))
            // EventStatusName (effective status) and AvailableTickets (derived from reservations, never persisted, per DATA-MODEL.md "Design decisions" #1 and #2) are filled by the Service after mapping.
            .ForMember(destination => destination.EventStatusName, options => options.Ignore())
            .ForMember(destination => destination.AvailableTickets, options => options.Ignore());

        CreateMap<Reservation, ReservationResponse>()
            .ForMember(destination => destination.EventName, options => options.MapFrom(source => source.Event.Name))
            .ForMember(destination => destination.CustomerName, options => options.MapFrom(source => source.Customer.Name))
            .ForMember(destination => destination.ReservationStatusName, options => options.MapFrom(source => source.ReservationStatus.Name));

        CreateMap<CreateEventRequest, Event>()
            // Assigned by the Service after mapping: Id is DB-generated, EventStatusId defaults to Active per FR-01, CreatedDate/UpdatedDate are set to the current timestamp.
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.EventStatusId, options => options.Ignore())
            .ForMember(destination => destination.CreatedDate, options => options.Ignore())
            .ForMember(destination => destination.UpdatedDate, options => options.Ignore());

        CreateMap<CreateReservationRequest, Reservation>()
            // Assigned by the Service after mapping: CustomerId requires resolving/creating the Customer (CustomerName/Email/Phone are not mapped here), ReservationStatusId defaults to PendingPayment per FR-03, TotalPrice is calculated from the event price, ReservationDate is set to the current timestamp, ReservationCode is only generated on payment confirmation per FR-04, Id is DB-generated.
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CustomerId, options => options.Ignore())
            .ForMember(destination => destination.ReservationStatusId, options => options.Ignore())
            .ForMember(destination => destination.TotalPrice, options => options.Ignore())
            .ForMember(destination => destination.ReservationDate, options => options.Ignore())
            .ForMember(destination => destination.ReservationCode, options => options.Ignore())
            .ForMember(destination => destination.CancelledDate, options => options.Ignore());
    }
}
