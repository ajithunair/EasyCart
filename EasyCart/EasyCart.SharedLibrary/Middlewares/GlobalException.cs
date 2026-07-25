using EasyCart.SharedLibrary.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace EasyCart.SharedLibrary.Middlewares
{
    public class GlobalException(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var defaultTitle = "Error";
            var defaultMessage = $"An unexpected error occurred while processing the request: {context.Request.Path}";
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var title = defaultTitle;
            var message = defaultMessage;

            try
            {
                await next(context);

                // If the pipeline already started streaming a response, we cannot safely rewrite it here.
                if (context.Response.HasStarted)
                {
                    return;
                }

                if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    title = "Warning";
                    message = $"Too many requests. Please try again later: {context.Request.Path}";
                    statusCode = context.Response.StatusCode;
                    await WriteProblemDetailsAsync(context, title, message, statusCode);
                }
                else if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    title = "Unauthorized";
                    message = $"Unauthorized access. Please check your credentials: {context.Request.Path}";
                    statusCode = context.Response.StatusCode;
                    await WriteProblemDetailsAsync(context, title, message, statusCode);
                }
                else if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    title = "Forbidden";
                    message = $"Forbidden access. You do not have permission to access this resource: {context.Request.Path}";
                    statusCode = context.Response.StatusCode;
                    await WriteProblemDetailsAsync(context, title, message, statusCode);
                }
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);

                if (ex is TaskCanceledException || ex is TimeoutException)
                {
                    title = "Request Timeout";
                    message = $"Request timed out. Please try again later: {context.Request.Path}";
                    statusCode = StatusCodes.Status408RequestTimeout;
                }

                if (!context.Response.HasStarted)
                {
                    await WriteProblemDetailsAsync(context, title, message, statusCode);
                }
            }
        }

        private static async Task WriteProblemDetailsAsync(HttpContext context, string title, string message, int statusCode)
        {
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            // ProblemDetails gives every service a consistent JSON error envelope.
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails
            {
                Title = title,
                Detail = message,
                Status = statusCode
            }), CancellationToken.None);
        }
    }
}
