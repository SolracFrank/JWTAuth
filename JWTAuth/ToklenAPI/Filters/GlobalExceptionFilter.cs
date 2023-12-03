using Domain.Exceptions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ToklenAPI.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var problemDetails = new ProblemDetails { Instance = context.HttpContext.Request.GetDisplayUrl() };
            var statusCode = 0;
            var isInfrastructureException = context.Exception.GetType() == typeof(InfrastructureException);
            var isValidationException = context.Exception.GetType() == typeof(ValidationException);
            var isNotFoundException = context.Exception.GetType() == typeof(NotFoundException);
            var isBadRequestException = context.Exception.GetType() == typeof(BadRequestException);
            var isApiException = context.Exception.GetType() == typeof(ApiException);
            var isUnAuthorizedException = context.Exception.GetType() == typeof(UnAuthorizedException);

            if (isInfrastructureException)
            {

                var exception = (InfrastructureException)context.Exception;
                statusCode = StatusCodes.Status500InternalServerError;

                problemDetails.Detail = exception.Message;
                problemDetails.Title = "Application infrastructure has failed";
            }
            if (isValidationException)
            {
                const string propertyValidationErrorsName = "errors";
                var exception = (ValidationException)context.Exception;
                statusCode = StatusCodes.Status400BadRequest;
                var existsErrors = exception.Errors != null && exception.Errors.Any();

                if (existsErrors)
                {
                    var errorMessages = exception.Errors.Select(x => $"{x.Key} : {string.Join(". ", x.Value)}").ToList();
                    var message = string.Join(". ", errorMessages);
                }

                problemDetails.Title = "Validation problem.";
                problemDetails.Extensions.Add(propertyValidationErrorsName, exception.Errors);
                problemDetails.Detail = exception.Message;
            }

            if (isNotFoundException)
            {
                var exception = (NotFoundException)context.Exception;
                statusCode = StatusCodes.Status404NotFound;

                problemDetails.Title = "Not found.";
                problemDetails.Detail = exception.Message;
            }

            if (isBadRequestException)
            {
                const string propertyValidationErrorsName = "errors";
                var exception = (BadRequestException)context.Exception;
                statusCode = StatusCodes.Status400BadRequest;

                problemDetails.Title = "Validation problem.";
                problemDetails.Extensions.Add(propertyValidationErrorsName, exception.Errors);
                problemDetails.Detail = exception.Message;
            }
            if (isApiException)
            {
                var exception = (ApiException)context.Exception;
                statusCode = StatusCodes.Status400BadRequest;

                problemDetails.Title = "Validation problem.";
                problemDetails.Detail = exception.Message;
            }
            if (isUnAuthorizedException)
            {
                var exception = (UnauthorizedAccessException)context.Exception;
                statusCode = StatusCodes.Status401Unauthorized;

                problemDetails.Title = "Unauthorized";
                problemDetails.Detail = exception.Message;

            }

            if (isNotFoundException)
            {
                var exception = (NotFoundException)context.Exception;
                statusCode = StatusCodes.Status404NotFound;

                problemDetails.Title = "Not found.";
                problemDetails.Detail = exception.Message;
            }

            var notExcpectedException = statusCode == 0;

            if (notExcpectedException)
            {
                statusCode = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Problema no identificado.";
                problemDetails.Detail = "Existe un problema critico.";
            }

            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = statusCode
            };
        }
    }
}
