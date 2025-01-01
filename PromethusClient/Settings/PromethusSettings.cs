using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace PromethusClient.Settings
{
    public class PromethusSettings
    {
        public static short AllowCollectMatricsInSeconds { get; set; } = 60 * 5;
        //public static int TimeSpanToResetSendMessagesCountInSeconds { get; set; } = 60 * 5;
        //public static int TimeSpanToResetTotalNotAuthenticatedSessionsInSeconds { get; set; } = 60 * 5;
        public static bool IsEnabledJWTAndAppAuthenticationForPromethus { get; set; } = false;
        public static bool IsAddConnectionIdsToMatrics { get; set; } = false;
    }
}
