using App.Common.DTOs.Reservations;
using FluentValidation;

namespace App.Infrastructure.Validators;

public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(request => request.EventId)
            .GreaterThan(0).WithMessage("EventId must be a positive integer.");

        RuleFor(request => request.TicketQuantity)
            .GreaterThanOrEqualTo(1).WithMessage("TicketQuantity must be at least 1.");

        RuleFor(request => request.CustomerName)
            .NotEmpty().WithMessage("CustomerName is required.");

        RuleFor(request => request.CustomerEmail)
            .NotEmpty().WithMessage("CustomerEmail is required.")
            .EmailAddress().WithMessage("CustomerEmail must be a valid email address.");

        RuleFor(request => request.CustomerPhone)
            .NotEmpty().WithMessage("CustomerPhone must not be empty when provided.")
            .When(request => request.CustomerPhone is not null);
    }
}
