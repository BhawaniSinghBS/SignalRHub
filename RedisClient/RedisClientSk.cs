#region noUSubsCribe
using Newtonsoft.Json;
using RedisClient.Cosntants;
using RedisClient.DataModels;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace RedisClient
{ 
    public static class MyRedisClient
    {
        //public static ConcurrentDictionary<string, string> RedisTagSubscrbed { get; private set; } = new(); //tags and conetionids
        public static bool IsRedisConnected { get => RedisClient?.IsConnected ?? false; }
        public static bool IsConnecting { get => RedisClient?.IsConnecting ?? false; }
        private static ConnectionMultiplexer RedisClient { get; set; }
        private static ISubscriber Subscriber { get; set; }
        public static event Action<int, Data4> Data4MessageReceived;
        public static event Action<int, Data6> Data5MessageReceived;
        public static event Action<int, Data5> Data6MessageReceived;

        public static ConcurrentDictionary<string, string> RedisChannelsSubscribed = new();
        public static ConcurrentDictionary<string, string> RedisChannelsPedingToSubscribe = new();

        public static ConnectionMultiplexer GetRedisClientAndSubcriber()
        {
            Dispose();// clear if earlier garbage client is there
            string[] parts = RedisConstants.RedisConnectionString.Split('@');
            string usernamePassword = parts[0]; 
            string[] credentials = usernamePassword.Split(':');
            // Extract IP address and port from the second part of the input string
            string[] ipAddressPort = parts[1].Split(':');

            string username = credentials[0]; 
            string password = credentials[1];
            string ipAddress = ipAddressPort[0]; 
            string port = ipAddressPort[1];

            var redisConfiguration = new ConfigurationOptions()
            {
                EndPoints = new EndPointCollection() { { ipAddress, short.Parse(port) } },
                Password = password,
                AbortOnConnectFail = false,
                SyncTimeout = (1000 * 10),
                AsyncTimeout = (1000 * 10),
                IncludeDetailInExceptions = true,
                ReconnectRetryPolicy = new ExponentialRetry(1000 * 10),
                //ReconnectRetryPolicy = new ExponentialRetry(1000, 1000 * 2),
                HeartbeatInterval = TimeSpan.FromSeconds(10),
                ConnectRetry = 3,
                ConnectTimeout = 1000 * 10 * 5,
            };

            try
            {
                if (RedisClient != null)
                {
                    return RedisClient;
                }
                else
                {
                    RedisClient = ConnectionMultiplexer.Connect(redisConfiguration);
                    GetSubcriber();
                    RedisClient.ConnectionRestored += (sender, args) =>
                    {
                        // subscribe tags which came to subcribe while redis was not connected or was in connecting state or reconnecting state
                        List<string> tagsPendignToSubscribe = new();

                        Parallel.ForEach(RedisChannelsPedingToSubscribe, toBeSubscribedTag =>
                        {
                            string channel = toBeSubscribedTag.Key;

                            if (
                                   string.IsNullOrEmpty(channel) ||
                                   RedisChannelsSubscribed.TryGetValue(channel, out string channelAlreadySubscribed)
                               )
                            {
                                return;
                            }
                            else
                            {
                                tagsPendignToSubscribe.Add(channel);
                            }
                        });

                        if (tagsPendignToSubscribe.Count > 0)
                        {
                            _ = SubscribeToChannelsAndGetAvailabilityAsync(tagsPendignToSubscribe);
                        }
                        RedisChannelsPedingToSubscribe.Clear();

                    };
                    RedisClient.ConnectionFailed += (sender, args) =>
                    {
                        Parallel.ForEach(RedisChannelsSubscribed, toBeSubscribedTag =>
                        {
                            string channel = toBeSubscribedTag.Key;

                            if (
                                   string.IsNullOrEmpty(channel) ||
                                   RedisChannelsPedingToSubscribe.TryGetValue(channel, out string channelAlreadySubscribed)
                               )
                            {
                                return;
                            }
                            else
                            {
                                RedisChannelsPedingToSubscribe.AddOrUpdate(channel, channel, (key, oldValue) => channel);
                            }
                            RedisChannelsSubscribed.Clear();
                        });
                    };
                }

                return RedisClient;
            }
            catch (Exception ex)
            {

                try
                {
                    RedisClient = ConnectionMultiplexer.Connect(redisConfiguration);
                    GetSubcriber();
                    return RedisClient;
                }
                catch (Exception ex2)
                {
                    try
                    {
                        RedisClient = ConnectionMultiplexer.Connect(redisConfiguration);
                        GetSubcriber();
                        return RedisClient;
                    }
                    catch (Exception ex3)
                    {
                        RedisClient = ConnectionMultiplexer.Connect(redisConfiguration);
                        GetSubcriber();
                        return RedisClient;
                    }
                }
            }
        }

        private static ISubscriber GetSubcriber()
        {
            try
            {
                if (Subscriber != null && Subscriber.IsConnected())
                {
                    return Subscriber;
                }
                else
                {
                    if (RedisClient != null && (RedisClient.IsConnected || RedisClient.IsConnecting))
                    {
                        if (RedisClient.IsConnecting)
                        {
                            return Subscriber;
                        }

                        if (RedisClient.IsConnected && !RedisClient.IsConnecting)
                        {
                            if (Subscriber != null && Subscriber.IsConnected())
                            {
                                return Subscriber;
                            }
                            else
                            {
                                return Subscriber = RedisClient.GetSubscriber();
                            }
                        }
                        else
                        {
                            GetRedisClientAndSubcriber();
                            return Subscriber;
                        }
                    }
                    else
                    {
                        GetRedisClientAndSubcriber();
                        return Subscriber;
                    }
                }
            }
            catch (Exception)
            {
                return GetSubcriber();
            }
        }


        private static void HandleChannelMessageReceivedMdal(string channel, string serilazedDataModal)
        {

            try
            {
                if (channel.Contains("Data4"))
                {
                    var data = JsonConvert.DeserializeObject<Data4>(serilazedDataModal);
                    data.SignalRTimeStamp = DateTime.UtcNow;
                    if (data != null)
                    {
                        Data4MessageReceived?.Invoke(data.Id, data);
                    }
                }
                else if (channel.Contains("Data6"))
                {
                    var data = JsonConvert.DeserializeObject<Data6>(serilazedDataModal);
                    data.SignalRTimeStamp = DateTime.UtcNow;
                    if (data != null)
                    {
                        Data5MessageReceived?.Invoke(data.Id, data);
                    }
                }
                else if (channel.Contains("Data5"))
                {
                    var data = JsonConvert.DeserializeObject<Data5>(serilazedDataModal);
                    data.SignalRTimeStamp = DateTime.UtcNow;
                    if (data != null)
                    {
                        Data6MessageReceived?.Invoke(data.Id, data);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static bool SubscribeWhenRestoredConnection(List<string> channelsToSubscribe)
        {
            bool isScucces = false;
            foreach (var channel in channelsToSubscribe)
            {
                if (string.IsNullOrEmpty(channel) ||
                    RedisChannelsSubscribed.TryGetValue(channel, out string channelAlreadySubscribed) ||
                    RedisChannelsPedingToSubscribe.TryGetValue(channel, out string channelAlreadyAddedIntoBeSubscribedList)
                   )
                {
                    continue;
                }

                RedisChannelsPedingToSubscribe.AddOrUpdate(channel, channel, (key, oldValue) => channel);
                isScucces = true;
            }
            return isScucces;
        }



        public static async Task SubscribeToChannelsAndGetAvailabilityAsync(List<string> channelsToSubscribe)
        {
            try
            {
                _ = Task.Run(() =>
                {
                    try
                    {
                        if (Subscriber == null || !Subscriber.IsConnected())
                        {
                            GetRedisClientAndSubcriber();
                        }

                        if (Subscriber?.IsConnected() ?? false)
                        {
                            foreach (var channel in channelsToSubscribe)
                            {
                                if (string.IsNullOrEmpty(channel) || RedisChannelsSubscribed.TryGetValue(channel, out string channelAlreadySubscribed))
                                {
                                    continue;
                                }
                                Subscriber?.Subscribe(channel, (channel, message) =>
                                {
                                    HandleChannelMessageReceivedMdal(channel, message);
                                });
                                RedisChannelsSubscribed.AddOrUpdate(channel, channel, (key, oldValue) => channel);
                            }
                        }
                        else
                        {
                            SubscribeWhenRestoredConnection(channelsToSubscribe);
                        }
                    }
                    catch (Exception ex)
                    {

                        //List<string> subsciribedtags = RedisChannelsSubscribed?.Values?.ToList();
                        //channelsToSubscribe = channelsToSubscribe ?? new();
                        //foreach (var item in channelsToSubscribe)
                        //{
                        //    if (string.IsNullOrEmpty(item) || subsciribedtags.Contains(item))
                        //    {
                        //        continue;
                        //    }
                        //    else
                        //    {
                        //        subsciribedtags.Add(item);
                        //    }
                        //}
                        //Dispose();
                        //GetRedisClientAndSubcriber();
                        //SubscribeToChannelsAndGetAvailabilityAsync(subsciribedtags);

                        //Parallel.ForEach(RedisChannelsSubscribed, toBeSubscribedTag =>
                        //{
                        //    string channel = toBeSubscribedTag.Key;
                        //    if (
                        //           string.IsNullOrEmpty(channel) ||
                        //           RedisChannelsPedingToSubscribe.TryGetValue(channel, out string channelAlreadySubscribed)
                        //       )
                        //    {
                        //        return;
                        //    }
                        //    else
                        //    {
                        //        RedisChannelsPedingToSubscribe.AddOrUpdate(channel, channel, (key, oldValue) => channel);
                        //    }
                        //});
                    }
                });
            }
            catch (Exception ex)
            {

            }
        }
        public static async Task UnSubscribeToChannelsAndGetAvailabilityAsync(List<string> channelsToUnSubscribe)
        {

            _ = Task.Run(() =>
             {
                 try
                 {
                     if (Subscriber == null || !Subscriber.IsConnected())
                     {
                         foreach (var channel in channelsToUnSubscribe)
                         {
                             RedisChannelsSubscribed.TryRemove(channel, out string channelAlreadySubscribed);
                             RedisChannelsPedingToSubscribe.TryRemove(channel, out string channelPendingSubscribed);
                         }
                         return;
                     }

                     foreach (var channel in channelsToUnSubscribe)
                     {
                         Subscriber?.Unsubscribe(channel);
                         RedisChannelsSubscribed.TryRemove(channel, out string channelAlreadySubscribed);
                         RedisChannelsPedingToSubscribe.TryRemove(channel, out string channelPendingSubscribed);
                     }
                 }
                 catch (Exception ex)
                 {

                 }
             });
            return;
        }

        public static void Dispose()
        {
            try
            {
                Subscriber?.UnsubscribeAll();
                RedisClient?.Dispose();
                Subscriber = null;
                RedisClient = null;
                RedisChannelsSubscribed.Clear();
                RedisChannelsPedingToSubscribe.Clear();
                GC.Collect();
            }
            catch (Exception ex)
            {
                try
                {
                    RedisClient?.Dispose();
                    Subscriber = null;
                    RedisClient = null;

                    RedisChannelsSubscribed.Clear();
                    RedisChannelsPedingToSubscribe.Clear();
                    GC.Collect();
                }
                catch (Exception)
                {
                    Subscriber = null;
                    RedisClient = null;
                    RedisChannelsSubscribed.Clear();
                    RedisChannelsPedingToSubscribe.Clear();
                    GC.Collect();
                }
            }
            // RedisTagSubscrbed?.Clear();----------------------------------------
        }
    }
}
#endregion noUSubsCribe