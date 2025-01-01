using Microsoft.AspNetCore.Http;
using PromethusClient;
using PromethusClient.Instruments;
using PromethusClient.Settings;
using SignalRClient.SerilizingDeserilizing;
using SignalRServer.Secuirity;
using SignalRServer.Settings;
using System.Net;

namespace SignalRServer.Middleware
{
    public class ErrorHandlingMiddlewareJWTMiddleware
    {
        private readonly RequestDelegate next;

        public ErrorHandlingMiddlewareJWTMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                List<string> request = PromethusSettings.IsEnabledJWTAndAppAuthenticationForPromethus ? new List<string>() { "signalrh", "api", "health", "metrics" } : new List<string>() { "signalrh", "api" };

                if (request.Any(r => context.Request.Path.ToString().Contains(r, StringComparison.OrdinalIgnoreCase)))
                {


                    var callerappName = JWTSecurity.GetAppName(context);
                    bool isValidAppRequest = JWTSecurity.IsValidAppRequesting(callerappName, ServerSettings.AuthorizedAppNames);

                    var token = JWTSecurity.GetJwtTokenFromContext(context);
                    bool isValidJWTToken = JWTSecurity.IsValidJwtToken(token, callerappName);

                    if (isValidAppRequest && isValidJWTToken)
                    {
                        await next(context);
                            ClientCounterMetrics.TotalAuthenticatedSessionCount.Inc(1);
                        //_ = ClientGaugeMetrics.UpdateCurrentAuthenticatedSessions(new() { context.Connection.RemoteIpAddress?.ToString() }, isAuthenticated: true);
                        //context.Response.OnCompleted(() =>
                        //{
                        //    _ = ClientGaugeMetrics.UpdateCurrentAuthenticatedSessions(new() { context.Connection.RemoteIpAddress?.ToString() }, isAuthenticated: false);
                        //    return Task.CompletedTask;
                        //});
                    }
                    else
                    {
                        //Call the next middleware in the pipeline
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                       // _ = ClientGaugeMetrics.UpdateTotalNotAuthenticatedSessionGauge(new() { context.Connection.RemoteIpAddress?.ToString() }, isAuthenticated: false);
                        return;
                    }
                }
                else
                {
                    if (new List<string>() { "health", "metrics" }.Any(r => context.Request.Path.ToString().Contains(r, StringComparison.OrdinalIgnoreCase))  // check minimum time elapsed to give matrics
                      )
                    {
                        if ((DateTime.UtcNow - RegisterPromethus.LastMetricsCollectionTime).TotalSeconds < PromethusSettings.AllowCollectMatricsInSeconds)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.AlreadyReported;
                            return;
                        }
                        RegisterPromethus.LastMetricsCollectionTime = DateTime.UtcNow;
                    }
                    // internal calls
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

            string errorLogtag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();
            string excetpion = $"Class name : {nameof(HubTimers)}  -- Function Name : {nameof(HandleExceptionAsync)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);
            _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: SignalRClient.ClientSettings.ClientSettings.SignalRHubUrl,
                                          appName: ServerSettings.ThisAppName,
                                          tagsOnWhichToSend: new List<string>() { errorLogtag },
                                          nonSerialezedDataToSend: excetpion,
                                          jwtToken: ServerSettings.ThisAppToken);
            return;
        }

    }
}
