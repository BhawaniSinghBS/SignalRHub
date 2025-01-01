using Prometheus;

namespace PromethusClient.Instruments
{
    public static class ClientCounterMetrics
    {
        public static Counter TotalAuthenticatedSessionCount { get; } = Metrics.CreateCounter(
                                                                            "signalr_Authenticated_Session_Count_Total_From_Start_Of_Application",
                                                                            "signalr Authenticated Session Count Total From Start Of Application"
                                                                           );
    }
}