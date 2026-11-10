using FluentValidation;
using GymBase.Base.Exceptions;

namespace GymBase.Base.Validation;

public static class ValidatorExtensions
{
    /// <summary>Chạy validator; không hợp lệ thì ném lỗi 400 có <c>errors</c> theo từng trường.</summary>
    /// <exception cref="ValidationFailedException">Dữ liệu không qua được validator.</exception>
    public static async Task ValidateOrThrowAsync<T>(this IValidator<T> validator, T dto, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(dto, cancellationToken);
        if (!result.IsValid)
        {
            throw new ValidationFailedException(result.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()));
        }
    }
}
