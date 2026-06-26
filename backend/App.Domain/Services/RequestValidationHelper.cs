using FluentValidation;
using FluentValidation.Results;
using ValidationException = App.Domain.Exceptions.ValidationException;

namespace App.Domain.Services;

public static class RequestValidationHelper
{
    public static void ValidateOrThrow<T>(IValidator<T> validator, T request)
    {
        ValidationResult result = validator.Validate(request);
        if (!result.IsValid)
        {
            string aggregatedMessage = string.Join("; ", result.Errors.Select(error => error.ErrorMessage));
            throw new ValidationException(aggregatedMessage);
        }
    }
}
