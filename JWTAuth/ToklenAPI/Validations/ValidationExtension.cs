using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace ToklenAPI.Validations
{
    public static class ValidationExtension
    {
        public static ValidationProblemDetails ToProblemDetails(this ValidationException ex)
        {
            var error = new ValidationProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Status = StatusCodes.Status400BadRequest
            };

            foreach (var validationFailure in ex.Errors)
            {
                var containsKey = error.Errors.ContainsKey(validationFailure.PropertyName);
                var propertyName = validationFailure.PropertyName[0].ToString().ToLower() +
                                   validationFailure.PropertyName[1..];

                if (containsKey)
                {
                    error.Errors[propertyName] = error.Errors[propertyName]
                        .Concat(new[] { validationFailure.ErrorMessage }).ToArray();

                    continue;
                }

                error.Errors.Add(new KeyValuePair<string, string[]>(propertyName,
                    new[] { validationFailure.ErrorMessage }));
            }

            return error;
        }

        public static ValidationProblemDetails ToProblemDetails(
            this Domain.Exceptions.ValidationException ex)
        {
            var error = new ValidationProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message,
                Title = ex.Message
            };

            if (ex?.Errors == null) return error;

            foreach (var validationFailure in ex.Errors)
            {
                var containsKey = error.Errors.ContainsKey(validationFailure.Key);
                var propertyName = validationFailure.Key[0].ToString().ToLower() +
                                   validationFailure.Key[1..];

                if (containsKey)
                {
                    error.Errors[propertyName] = error.Errors[propertyName]
                        .Concat(validationFailure.Value).ToArray();

                    continue;
                }

                error.Errors.Add(new KeyValuePair<string, string[]>(propertyName,
                    validationFailure.Value.ToArray()));
            }

            return error;
        }

        public static ValidationProblemDetails ToProblemDetails(
            this Domain.Exceptions.UnAuthorizedException ex)
        {
            var error = new ValidationProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Status = StatusCodes.Status401Unauthorized,
                Detail = ex.Message,
                Title = ex.Message
            };

            return error;
        }

        public static ValidationProblemDetails ToProblemDetails(
            this Domain.Exceptions.BadRequestException ex)
        {
            var error = new ValidationProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message,
                Title = ex.Message
            };

            if (ex?.Errors == null) return error;

            foreach (var validationFailure in ex.Errors)
            {
                var containsKey = error.Errors.ContainsKey(validationFailure.Key);
                var propertyName = validationFailure.Key[0].ToString().ToLower() +
                                   validationFailure.Key[1..];

                if (containsKey)
                {
                    error.Errors[propertyName] = error.Errors[propertyName]
                        .Concat(validationFailure.Value).ToArray();

                    continue;
                }

                error.Errors.Add(new KeyValuePair<string, string[]>(propertyName,
                    validationFailure.Value.ToArray()));
            }

            return error;
        }

        public static ValidationProblemDetails ToProblemDetails(
         this Domain.Exceptions.ApiException ex)
        {
            var error = new ValidationProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message,
                Title = ex.Message
            };

            return error;
        }
    }
}
