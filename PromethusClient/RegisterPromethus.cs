using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using PromethusClient.Instruments;

namespace PromethusClient
{
    public class RegisterPromethus
    {
        public static DateTime LastMetricsCollectionTime { get; set; } = DateTime.UtcNow; // used in middleware
        public static void ConfigureServicesForPromethus(IServiceCollection services,ushort port)
        {
            try
            {
                // Add Prometheus metrics services
                services.AddEndpointsApiExplorer();
                services.UseHttpClientMetrics();
                services.AddMetricServer(options =>
                {
                    options.Port = port;
                });


                Metrics.SuppressDefaultMetrics();
                Metrics.DefaultRegistry.AddBeforeCollectCallback(() =>
                {
                    ClientGaugeMetrics.ConnectedClientsGauge.Publish();
                    ClientGaugeMetrics.MessageSendForSetTimeSpanGauge.Publish();
                    ClientGaugeMetrics.UDPReceiveFameCount.Publish();
                    //ClientGaugeMetrics.CurrentAuthenticatedSessionsGauge.Publish();
                    //ClientGaugeMetrics.TotalNotAuthenticatedSessionGauge.Publish();
                    ClientCounterMetrics.TotalAuthenticatedSessionCount.Publish();
                });

                // Add health checks
                services.AddHealthChecks().ForwardToPrometheus();
            }
            catch (Exception ex)
            {
 
            }
        }
        // call it under use routing    
        public static void ConfigureAppForPromethus(IApplicationBuilder app)
        {
            // Use health checks middleware and configure health endpoint
           // app.UseHttpMetrics();
            app.UseHttpMetrics(options =>
            {
                // This will preserve only the first digit of the status code.
                // For example: 200, 201, 203 -> 2xx
                options.ReduceStatusCodeCardinality();
                //options.ConfigureMeasurements(measurementOptions =>
                //{
                //    // Only measure exemplar if the HTTP response status code is not "OK".
                //    measurementOptions.ExemplarPredicate = context => context.Response.StatusCode != HttpStatusCode.Ok;
                //});
            });
            app.UseHealthChecks("/health");
            app.UseEndpoints(endpoints =>
            { 
                endpoints.MapMetrics();
                //endpoints.MapMetrics().RequireAuthorization("AllowSpecificOrigin");
            });
        }
    }
}
