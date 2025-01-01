using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PromethusClient;
using PromethusClient.Settings;
using Serilog;
using SignalRServer;
using SignalRServer.Middleware;
using SignalRServer.Settings;
using SihnalRHub.DAL;
using SihnalRHub.DAL.Enums;


namespace SignalRHub.Server
{

    public class Program
    {
        private static string hostURL { get; set; }
        public static string HostURL { get => hostURL; }
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            #region appsettings 
            var configuration = builder.Services.BuildServiceProvider().GetService<IConfiguration>();
            // Read initial settings
            UpdateAppSettings(configuration);
            // Listen for changes in the configuration
            ChangeToken.OnChange(() => configuration.GetReloadToken(), () =>
            {
                UpdateAppSettings(configuration);
            });
            #endregion appsettings 

            #region logFileSetUp
            //*******************************setting log file*****************
            SignalRClient.Helpers.DerectoryFileHelper.DirectoryFileHelper.CreateFileAtGivenPathIfNotPresent(SignalRClient.ClientSettings.ClientSettings.SignalRLogFilePath);
            //*******************************/setting log file*****************
            #region addSerilog
            Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(builder.Configuration)  // Read settings from appsettings.json
                        .CreateLogger();

            // Replace the default logger with Serilog
            builder.Host.UseSerilog();
            #endregion addSerilog
            #endregion
            // Add services to the container.
            builder.Services.AddHttpContextAccessor(); // add http contex to access
            builder.Services.AddControllersWithViews();

            builder.Services.AddRazorPages();

            builder.Services.AddResponseCompression(opts =>
            {
                opts.EnableForHttps = true;
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
                opts.Providers.Add<BrotliCompressionProvider>();
                opts.Providers.Add<GzipCompressionProvider>();
            });

            builder.Services.AddSignalR(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(10);
                options.ClientTimeoutInterval = TimeSpan.FromMinutes(3);
                options.MaximumReceiveMessageSize = long.MaxValue;
                options.MaximumParallelInvocationsPerClient = int.MaxValue;
                options.HandshakeTimeout = TimeSpan.FromMinutes(5);
            });
             
            if (!ServerSettings.AllowedHosts?.Contains("*") ?? false)
            {
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowSpecificOrigin",
                        builder =>// if cliet address is static and known then to them only*/
                        builder.WithOrigins(ServerSettings.AllowedHosts.Split(';'))
                               .SetIsOriginAllowedToAllowWildcardSubdomains()
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowCredentials());
                });
            } 
            else
            {
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAnyOrigin", builder =>
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader());
                });
            }
            RegisterPromethus.ConfigureServicesForPromethus(builder.Services, ServerSettings.PromethusMetricsServerPort);
            //************************************************
            Dictionary<string, int> dbConnectionStringNames = EnumsExtenshion.ToDictionary<ConnectionStringName>();

            builder.Services.AddDALServices(dbConnectionStringNames, configuration);
            var app = builder.Build();
            var serviceProvider = app.Services;
            // Initialize your static class or method with the service provider
            HubTimers.InitializeServiceProvider(serviceProvider);
            app.UseMiddleware<ErrorHandlingMiddlewareJWTMiddleware>();
            // *********************setting Content root path and web ruoute path in constants// *********************
            SignalRClient.ClientSettings.ClientSettings.AppContentRootPath = app.Environment.ContentRootPath;
            SignalRClient.ClientSettings.ClientSettings.AppWebRootPath = app.Environment.WebRootPath;
            SignalRClient.ClientSettings.ClientSettings.LogDirectoryPath = Path.Combine(app.Environment.ContentRootPath, @"logs\Logs");
            SignalRClient.ClientSettings.ClientSettings.SignalRLogFilePath = Path.Combine(Path.Combine(app.Environment.ContentRootPath, @"logs\Logs"), $"{DateTime.UtcNow:dd-MM-yyyy}{SignalRServer.HubTimers.TimerTriggerCountTillIntMax}.txt");
            // *********************/setting Content root path and web ruoute path in constants// *********************

            // app.UseResponseCompression();// should be here
           
            if (!ServerSettings.AllowedHosts?.Contains("*") ?? false)
            {
                app.UseCors("AllowSpecificOrigin");
            } 
            else
            {
                app.UseCors("AllowAnyOrigin");
            }
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())

            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //hostURL = app.Environment.WebRootPath;
            //app.UseHttpsRedirection();

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            //******************security*********************
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRouting();
            //******************/security*********************
            app.MapRazorPages();

            //*****************call matrics**********************
            RegisterPromethus.ConfigureAppForPromethus(app);
            //*****************/call matrics**********************
            //************************************************
            app.MapControllers();
            app.MapHub<SignalRServer.SignalRHub>("/SignalRHub");//should be here
            app.MapFallbackToFile("index.html");
            //************************************************
            ////************************ start udp client ************************
            //// starting udp to start listning to data throwen from distributer app on the ip of indian server currently , if want tho get data on other ip where this app is running , make entry in distrbuter app table with the ip adress of the server where this app is running
            ///
            #region Data4Subscription
            RedisClient.MyRedisClient.Data4MessageReceived += SignalRServer.Redis.Redis.HandleData4MessageReceived;
            RedisClient.MyRedisClient.Data5MessageReceived += SignalRServer.Redis.Redis.HandleData5Message3Received;
            RedisClient.MyRedisClient.Data6MessageReceived += SignalRServer.Redis.Redis.HandleData5EventMessageReceived;
            #endregion Data4Subscription
            if (!HubTimers.IsUDPSubcribedInHubTimer)
            {
                HubTimers.IsUDPSubcribedInHubTimer = true;
                Task.Run(() =>
                {
                    CoreTCP.CoreUDP.UDPClient.MessageReceivedFromUDP += HubTimers.HandleUDPMessageReceived;
                  
                    _ = CoreTCP.CoreUDP.UDPClient.ReceiveDataFromUDPClient(
                                                                            expectedIPWhereDataWillBeReceived: "0.0.0.0",// allos any ip to send to this udp receiver
                                                                            expectedPortWhereDataWillBeReceived: 302,
                                                                            receiveContinueslyOnSubcribedEvent: true
                                                                         );
                });
            }
            //************************ /start udp client ************************
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
            app.Run();
        }

        private static void UpdateAppSettings(IConfiguration configuration)
        {
            string allowedHosts = configuration.GetValue<string>("AllowedHosts");
            ServerSettings.AllowedHosts = !string.IsNullOrEmpty(allowedHosts) ? allowedHosts : ServerSettings.AllowedHosts;



            IConfigurationSection settingsSection = configuration.GetSection("SettingsSeciton");
            // Update the static variables when configuration changes
            string isLoggingOnSet = settingsSection.GetValue<string>("IsLoggingOn");
            SignalRClient.ClientSettings.ClientSettings.IsSignalRLoggingOn = !string.IsNullOrEmpty(isLoggingOnSet) && isLoggingOnSet == "true";

            string isSendAllDataOnZeroTagAsWell = settingsSection.GetValue<string>("IsSendAllDataOnZeroTagAsWell");
            ServerSettings.IsSendAllDataOnZeroTagAsWell = !string.IsNullOrEmpty(isSendAllDataOnZeroTagAsWell) && isSendAllDataOnZeroTagAsWell == "true";
             
            string IsSendOnlyRealTime = settingsSection.GetValue<string>("IsSendOnlyRealTime");
            ServerSettings.IsSendOnlyRealTime = !string.IsNullOrEmpty(IsSendOnlyRealTime) && IsSendOnlyRealTime == "true";

            string IsSendCachedDataAt200MilliSecond = settingsSection.GetValue<string>("IsSendCachedDataAt200MilliSecond");
            ServerSettings.IsSendCachedDataAt200MilliSecond = !string.IsNullOrEmpty(IsSendCachedDataAt200MilliSecond) && IsSendCachedDataAt200MilliSecond == "true";

            string IsSendCashedDataAtHalfSecond = settingsSection.GetValue<string>("IsSendCashedDataAtHalfSecond");
            ServerSettings.IsSendCashedDataAtHalfSecond = !string.IsNullOrEmpty(IsSendCashedDataAtHalfSecond) && IsSendCashedDataAtHalfSecond == "true";

            string IsSendOtherThanData1OnSubscribe = settingsSection.GetValue<string>("IsSendOtherThanData1OnSubscribe");
            ServerSettings.IsSendOtherThanData1OnSubscribe = !string.IsNullOrEmpty(IsSendOtherThanData1OnSubscribe) && IsSendOtherThanData1OnSubscribe == "true";

            string IsSendDataOnSubscribe = settingsSection.GetValue<string>("IsSendDataOnSubscribe");
            ServerSettings.IsSendDataOnSubscribe = !string.IsNullOrEmpty(IsSendDataOnSubscribe) && IsSendDataOnSubscribe == "true";

            string redisConnectionString = settingsSection.GetValue<string>("RedisConnectionString");
            RedisClient.Cosntants.RedisConstants.RedisConnectionString = !string.IsNullOrEmpty(redisConnectionString) ? redisConnectionString : "default:userName@103.102.101.111:305";

            string isSubscribeForRedisItemspecificData = settingsSection.GetValue<string>("IsSubscribeForRedisItemspecificData");
            ServerSettings.IsSubscribeForRedisItemspecificData = !string.IsNullOrEmpty(isSubscribeForRedisItemspecificData) && isSubscribeForRedisItemspecificData == "true";

            string isSendDataOnDebugTag = settingsSection.GetValue<string>("IsSendDataOnDebugTag");
            ServerSettings.IsSendDataOnDebugTag = !string.IsNullOrEmpty(isSendDataOnDebugTag) && isSendDataOnDebugTag == "true";

            string isCnonnectToRedis = settingsSection.GetValue<string>("IsCnonnectToRedis");
            ServerSettings.IsCnonnectToRedis = !string.IsNullOrEmpty(isCnonnectToRedis) && isCnonnectToRedis == "true";
            
            string isDisconnectCnonnectToRedisWhenNoClient = settingsSection.GetValue<string>("IsDisconnectCnonnectToRedisWhenNoClient");
            ServerSettings.IsDisconnectCnonnectToRedisWhenNoClient = !string.IsNullOrEmpty(isDisconnectCnonnectToRedisWhenNoClient) && isDisconnectCnonnectToRedisWhenNoClient == "true";


            string isEnableJWTAndAppAuthenticationForPromethus = settingsSection.GetValue<string>("IsEnableJWTAndAppAuthenticationForPromethus");
            ServerSettings.IsEnableJWTAndAppAuthenticationForPromethus = !string.IsNullOrEmpty(isEnableJWTAndAppAuthenticationForPromethus) && isEnableJWTAndAppAuthenticationForPromethus == "true";

            string SendData3GreatedThanId = settingsSection.GetValue<string>("SendData3GreatedThanId");
            if (int.TryParse(SendData3GreatedThanId, out int SendData3GreatedThanIdInt))
            {
                ServerSettings.SendData3GreatedThanId = SendData3GreatedThanIdInt;
            }
            else
            {
                ServerSettings.SendData3GreatedThanId = 123456;
            }
            string isAddConnectionIdsToMatrics = settingsSection.GetValue<string>("IsAddConnectionIdsToMatrics");
            PromethusSettings.IsAddConnectionIdsToMatrics = !string.IsNullOrEmpty(isAddConnectionIdsToMatrics) && isAddConnectionIdsToMatrics == "true";

            string clearRedisStatusTimeTrackerInHours = settingsSection.GetValue<string>("ClearRedisStatusTimeTrackerInHours");
            if (short.TryParse(clearRedisStatusTimeTrackerInHours, out short clearRedisStatusTimeTrackerInHoursParsed))
            {
                ServerSettings.ClearRedisIdTrackerInHours = clearRedisStatusTimeTrackerInHoursParsed;
            }
            else
            {
                ServerSettings.ClearRedisIdTrackerInHours = 6;
            }

            string SendData1InSecondsOnSub = settingsSection.GetValue<string>("SendData1InSecondsOnSub");
            if (short.TryParse(SendData1InSecondsOnSub, out short SendData1InSecondsOnSubResult))
            {
                ServerSettings.SendData1InSecondsOnSub = SendData1InSecondsOnSubResult;
            }
            else
            {
                ServerSettings.SendData1InSecondsOnSub = 1;
            }

            string SendData1ForSecondsOnSub = settingsSection.GetValue<string>("SendData1ForSecondsOnSub");
            if (short.TryParse(SendData1ForSecondsOnSub, out short SendData1ForSecondsOnSubResult))
            {
                ServerSettings.SendData1ForSecondsOnSub = SendData1ForSecondsOnSubResult;
            }
            else
            {
                ServerSettings.SendData1ForSecondsOnSub = 5;
            }

            string uDPPort = settingsSection.GetValue<string>("UDPPort");
            if (short.TryParse(uDPPort, out short UDPPort))
            {
                if(CoreTCP.CoreUDP.Settings.TCPUDPSettings.UDPPort != UDPPort)
                {
                    CoreTCP.CoreUDP.Settings.TCPUDPSettings.UDPPort = UDPPort;
                    HubTimers.IsUDPSubcribedInHubTimer = false;
                }
            }
            else
            {
                CoreTCP.CoreUDP.Settings.TCPUDPSettings.UDPPort = 302;
                HubTimers.IsUDPSubcribedInHubTimer = false;
            }

            string promethusMetricsServerPort = settingsSection.GetValue<string>("PromethusMetricsServerPort");
            if (ushort.TryParse(promethusMetricsServerPort, out ushort promethusMetricsServerPortShort))
            {
                ServerSettings.PromethusMetricsServerPort = promethusMetricsServerPortShort;
            }
            else
            {
                ServerSettings.PromethusMetricsServerPort = 304;
            }

            string allowCollectMatricsInSeconds = settingsSection.GetValue<string>("AllowCollectMatricsInSeconds");
            if (short.TryParse(allowCollectMatricsInSeconds, out short allowCollectMatricsInSecondsShort))
            {
                PromethusSettings.AllowCollectMatricsInSeconds = allowCollectMatricsInSecondsShort;
            }
            else
            {
                PromethusSettings.AllowCollectMatricsInSeconds = 60 * 5;
            }
            
            string signalRServerUrl = settingsSection.GetValue<string>("SignalRServerUrl");
            SignalRClient.ClientSettings.ClientSettings.SignalRServerUrl = !string.IsNullOrEmpty(signalRServerUrl) && signalRServerUrl.Length > 7 ? signalRServerUrl : SignalRClient.ClientSettings.ClientSettings.SignalRServerUrl;

            string signalRHubUrl = settingsSection.GetValue<string>("SignalRHubUrl");
            SignalRClient.ClientSettings.ClientSettings.SignalRHubUrl = !string.IsNullOrEmpty(signalRHubUrl) && signalRHubUrl.Length > 7 ? signalRHubUrl : SignalRClient.ClientSettings.ClientSettings.SignalRHubUrl;
            HubTimers.IsUDPSubcribedInHubTimer = false; // connect to udp on updated port in appsettings
        }
    }
}