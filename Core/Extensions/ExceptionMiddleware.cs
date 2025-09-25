using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace Core.Extensions
{
    public sealed class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next,
                                   ILogger<ExceptionMiddleware> logger,
                                   IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Yapısal log + exception (stack trace log’da kalır)
                _logger.LogError(ex,
                    "Unhandled exception. Path: {Path}, TraceId: {TraceId}",
                    context.Request.Path,
                    context.TraceIdentifier);

                await HandleAsync(context, ex);
            }
        }


        private async Task HandleAsync(HttpContext ctx, Exception ex)
        {
            if (ctx.Response.HasStarted)
            {
                _logger.LogWarning("Response already started. Can't write error body. TraceId: {TraceId}",
                    ctx.TraceIdentifier);
                return;
            }

            ctx.Response.ContentType = "application/json; charset=utf-8";

            var code = ex switch
            {
                ValidationException => HttpStatusCode.BadRequest,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                KeyNotFoundException => HttpStatusCode.NotFound,
                NotImplementedException => HttpStatusCode.NotImplemented,
                _ => HttpStatusCode.InternalServerError
            };
            ctx.Response.StatusCode = (int)code;

            if (!ctx.Response.Headers.ContainsKey("X-Trace-Id"))
                ctx.Response.Headers.Add("X-Trace-Id", ctx.TraceIdentifier);

            var safeMessage = code == HttpStatusCode.InternalServerError
                ? "An unexpected error occurred."
                : ex.Message;

            var problem = new
            {
                type = $"https://httpstatuses.com/{(int)code}",
                title = code.ToString(),
                status = (int)code,
                traceId = ctx.TraceIdentifier,
                path = ctx.Request.Path.Value,
                errors = ex is ValidationException ve
                    ? ve.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                    : null,
                detail = _env.IsDevelopment() ? ex.ToString() : safeMessage
            };

            var json = JsonSerializer.Serialize(problem);
            await ctx.Response.WriteAsync(json);
        }

    }

    //public class ExceptionMiddleware
    //{
    //    private readonly RequestDelegate _next;
    //    public ExceptionMiddleware(RequestDelegate next) { _next = next; }

    //    public async Task Invoke(HttpContext context)
    //    {
    //        try { await _next(context); }
    //        catch (Exception e) { await HandleException(context, e); }
    //    }

    //    private Task HandleException(HttpContext ctx, Exception e)
    //    {
    //        ctx.Response.ContentType = "application/json";
    //        var code = HttpStatusCode.InternalServerError;
    //        var message = e.Message;

    //        if (e is ValidationException) code = HttpStatusCode.BadRequest;
    //        else if (e is UnauthorizedAccessException) code = HttpStatusCode.Unauthorized;

    //        ctx.Response.StatusCode = (int)code;

    //        var result = System.Text.Json.JsonSerializer.Serialize(new { error = message });
    //        return ctx.Response.WriteAsync(result);
    //    }

    //}
}
