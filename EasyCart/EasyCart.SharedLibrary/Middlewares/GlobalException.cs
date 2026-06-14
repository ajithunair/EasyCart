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
            string message = $"An unexpected error occurred while processing the request: {context.Request.Path}";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string title = "Error";
            try
            {
                await next(context);

                if(context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    message = $"Too many requests. Please try again later: {context.Request.Path}";
                    statusCode = context.Response.StatusCode;
                    title = "Warning";

                    await ModifyHeader(context, title, message, statusCode);
                }
                
                if(context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    message = $"Unauthorized access. Please check your credentials: {context.Request.Path}";
                    statusCode = context.Response.StatusCode;
                    title = "Unauthorized";
                    await ModifyHeader(context, title, message, statusCode);
                }

                if(context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    message = $"Forbidden access. You do not have permission to access this resource: {context.Request.Path}";
                    statusCode = context.Response.StatusCode;
                    title = "Forbidden";
                    await ModifyHeader(context, title, message, statusCode);
                }
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);

                if(ex is TaskCanceledException || ex is TimeoutException)
                {
                    message = $"Request timed out. Please try again later: {context.Request.Path}";
                    statusCode = (int)HttpStatusCode.RequestTimeout;
                    title = "Request Timeout";
                }

                await ModifyHeader(context, title, message, statusCode);
            }
        }

        private async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails
            {
                Title = title,
                Detail = message,
                Status = statusCode
            }), CancellationToken.None);

            return;
        }
    }
}
