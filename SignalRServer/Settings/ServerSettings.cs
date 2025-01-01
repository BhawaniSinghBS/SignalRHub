using SignalRClient.Enums;

namespace SignalRServer.Settings
{
    public class ServerSettings
    {
        public static string TagEncruptionKey { get; } = "sdfghj5678dfghj45678dfghj456789";
        public static string JWTTokenIssuer { get; } = @"dfgh45678dfghj456789o";
        public static string JWTTokenSecretKey { get; } = @"sdfghj3456789dfgh34567vn";

        public static List<string> AuthorizedAppNames { get; } = new List<string>() { "redis", "signalrhub","promethureceiver"};
        public static string ThisAppName { get; } = "signalrhub";
        public static string ThisAppToken { get; } = "sdfghj456789sdfghj45678sdfghj45678sdfghj45678dfghj34567sdfgh34567dfghj45678sdfghj345678sdfghj3456";
        public static string SignalRServerUrl { get; } = @"100.101.102.103";
        public static bool IsSendAllDataOnZeroTagAsWell { get; set; } = false;
       
        public static bool IsSendOnlyRealTime { get; set; } = false;
        public static bool IsSendCashedDataAtHalfSecond { get; set; } = true;
        public static bool IsSendCachedDataAt200MilliSecond { get; set; } = false;
        public static bool IsRedisDeligatesSubscribed { get; set; } = false;
        public static bool IsRedisClientCreating { get; set; } = false;
        public static short SendData1InSecondsOnSub { get; set; } = 1;
        public static short SendData1ForSecondsOnSub { get; set; } = 5;
        public static bool IsSendOtherThanData1OnSubscribe { get; set; } = true;
        public static bool IsSendDataOnSubscribe { get; set; } = true;
        public static short ClearRedisIdTrackerInHours { get; set; } = 6;
        public static bool IsSubscribeForRedisItemspecificData { get; set; } = true;
        public static bool IsSendDataOnDebugTag { get; set; } = false;
        public static bool IsCnonnectToRedis { get; set; } = false;
        public static bool IsDisconnectCnonnectToRedisWhenNoClient { get; set; } = false;
        public static bool IsEnableJWTAndAppAuthenticationForPromethus { get; set; } = false;
        public static string AllowedHosts { get; set; } = "localhost";
        public static ushort PromethusMetricsServerPort { get; set; } = 304;
        public static int SendData3GreatedThanId { get; set; } = 123456;
    }
}
