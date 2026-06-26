using App.Common.DTOs.Events;
using FluentValidation;

namespace App.Infrastructure.Validators;

public class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
{
    public CreateEventRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Length(5, 100).WithMessage("Name must be between 5 and 100 characters long.");

        RuleFor(request => request.Description)
            .NotEmpty().WithMessage("Description is required.")
            .Length(10, 500).WithMessage("Description must be between 10 and 500 characters long.");

        RuleFor(request => request.VenueId)
            .GreaterThan(0).WithMessage("VenueId must be a positive integer.");

        RuleFor(request => request.MaxCapacity)
            .GreaterThan(0).WithMessage("MaxCapacity must be a positive integer.");

        RuleFor(request => request.StartDate)
            .GreaterThan(DateTime.Now).WithMessage("StartDate must be a future date.");

        RuleFor(request => request.EndDate)
            .GreaterThan(request => request.StartDate).WithMessage("EndDate must be later than StartDate.");

        RuleFor(request => request.Price)
            .GreaterThan(0).WithMessage("Price must be a positive value.");

        RuleFor(request => request.EventTypeId)
            .GreaterThan(0).WithMessage("EventTypeId must be a positive integer.");
    }
}
