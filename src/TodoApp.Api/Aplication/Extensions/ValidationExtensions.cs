using FluentValidation.Results;

namespace TodoApp.Api.Aplication.Extensions;

public static class ValidationExtensions
{
    public static IEnumerable<string> ToFormattedErrorMessages(this ValidationResult validation)
    {
        ArgumentNullException.ThrowIfNull(validation);

        return validation
                .Errors
                .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                .ToList();
    }
}
