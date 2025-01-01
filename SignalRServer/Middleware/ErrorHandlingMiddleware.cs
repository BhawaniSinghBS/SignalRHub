using Microsoft.AspNetCore.Http;
using SignalRServer.Secuirity;
using System.Net;

namespace SignalRServer.Middleware
{


    namespace MapUtility.Infrastructure.ErrorHanding
    {
        public class ErrorHandlingMiddleware
        {
            private readonly RequestDelegate next;

            public ErrorHandlingMiddleware(RequestDelegate next)
            {
                this.next = next;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                try
                {
                    var token = JWTSecurity.GetJwtTokenFromContext(context);

                    if (JWTSecurity.IsValidJwtToken(token))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        return;
                    }
                    else
                    {
                        // Call the next middleware in the pipeline
                        await next(context);
                    }
                }
                catch (Exception ex)
                {
                    // Handle the exception
                    await HandleExceptionAsync(context, ex);
                }
            }

            private async static Task HandleExceptionAsync(HttpContext context, Exception ex)
            {
                // Customize the error response and log the exception
                // Here you can implement your own logic for error handling, such as returning a specific error message or redirecting to an error page

                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("An error occurred. Please try again later.");

                return;
            }

        }
    }

}
