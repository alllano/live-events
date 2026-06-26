using System.Text.Json;
using App.Common.Constants;
using App.Common.DTOs.Common;
using App.Domain.Exceptions;
using App.Infrastructure.Entities;
using App.Infrastructure.Persistence;
using App.Infrastructure.Repositories;

namespace App.web.Middleware;

public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonResponseOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogRepository logRepository, IUnitOfWork unitOfWork)
    {
        try
        {
            await _next(context);
        }
        catch (AppException appException)
        {
            await LogHandledExceptionAsync(context, appException, logRepository, unitOfWork);
            await WriteErrorResponseAsync(context, appException.StatusCode, appException.Message);
        }
        catch (Exception unhandledException)
        {
            await LogUnhandledExceptionAsync(context, unhandledException, logRepository, unitOfWork);
            await WriteErrorResponseAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, int statusCode, string message)
    {
        ResponseDTO<object> errorResponse = ResponseDTO<object>.AsResponseDTO(null, statusCode, success: false, message: message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, JsonResponseOptions));
    }

    // Expected business rejections are also recorded in dbo.Log (as HandledError) for traceability, even though the
    // client-facing response (built separately by WriteErrorResponseAsync) is unaffected by this.
    private static async Task LogHandledExceptionAsync(HttpContext context, AppException exception, ILogRepository logRepository, IUnitOfWork unitOfWork)
    {
        Log newLog = new Log
        {
            LogTypeId = LogTypeIds.HandledError,
            CreatedDate = DateTime.Now,
            Error = exception.Message,
            StackTrace = exception.StackTrace ?? string.Empty,
            Request = context.Request.Path.ToString()
        };

        await logRepository.AddAsync(newLog);
        await unitOfWork.SaveChangesAsync();
    }

    // Unhandled exceptions are recorded in dbo.Log for diagnostics, but the client only ever sees the generic message above.
    private static async Task LogUnhandledExceptionAsync(HttpContext context, Exception exception, ILogRepository logRepository, IUnitOfWork unitOfWork)
    {
        Log newLog = new Log
        {
            LogTypeId = LogTypeIds.UnhandledError,
            CreatedDate = DateTime.Now,
            Error = exception.Message,
            StackTrace = exception.StackTrace ?? string.Empty,
            Request = context.Request.Path.ToString()
        };

        await logRepository.AddAsync(newLog);
        await unitOfWork.SaveChangesAsync();
    }
}
