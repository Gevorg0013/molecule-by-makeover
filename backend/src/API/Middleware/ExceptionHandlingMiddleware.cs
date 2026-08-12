using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            ValidationException => (HttpStatusCode.BadRequest, "One or more validation errors occurred."),
            NotFoundException => (HttpStatusCode.NotFound, "The requested resource was not found."),
            ConflictException => (HttpStatusCode.Conflict, "The request conflicts with the current state."),
            ForbiddenAccessException => (HttpStatusCode.Forbidden, "You do not have permission to perform this action."),
            UnauthorizedException => (HttpStatusCode.Unauthorized, "Authentication failed."),
            BadRequestException => (HttpStatusCode.BadRequest, "The request could not be processed."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            logger.LogWarning(exception, "{ExceptionType} processing {Method} {Path}", exception.GetType().Name, context.Request.Method, context.Request.Path);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        if (exception is ValidationException validationException)
        {
            // A single property can break several rules at once (an empty slug fails both
            // NotEmpty and the format regex), so errors are grouped rather than added one by
            // one - Dictionary.Add on a repeated property name would throw and turn the 400
            // into a 500.
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

            var problem = new ValidationProblemDetails(errors)
            {
                Title = title,
                Status = (int)statusCode,
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problem);
            return;
        }

        var details = new ProblemDetails
        {
            Title = title,
            Status = (int)statusCode,
            Detail = statusCode == HttpStatusCode.InternalServerError ? null : exception.Message,
            Instance = context.Request.Path
        };
        await context.Response.WriteAsJsonAsync(details);
    }
}
