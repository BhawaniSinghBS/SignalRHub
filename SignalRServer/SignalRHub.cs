using Microsoft.AspNetCore.SignalR;
using PromethusClient.Instruments;
using ServiceStack;
using SignalRClient.ClientSettings;
using SignalRClient.DataModals;
using SignalRClient.DataModals.DataModels;
using SignalRClient.Helpers.DerectoryFileHelper;
using SignalRClient.SerilizingDeserilizing;
using SignalRServer.Settings;
using System.Collections.Concurrent;

namespace SignalRServer
{
    public class SignalRHub : Hub
    {
        public static ConcurrentDictionary<string, HashSet<string>> TagSubscriptions { get; private set; } = new(); //tags and conetionids
        // private static ConcurrentDictionary<string, object> TagLocks { get; set; } = new();

        public async override Task OnConnectedAsync()
        {
            try
            {
                await base.OnConnectedAsync();

                List<string> allConnectionIdsHUB = TagSubscriptions?.SelectMany(kv => kv.Value)?.Where(s => !string.IsNullOrEmpty(s))?.ToList() ?? new();

                if (!allConnectionIdsHUB.Contains(Context.ConnectionId))
                {
                    allConnectionIdsHUB.Add(Context.ConnectionId);
                }
                ClientGaugeMetrics.UpdateConnectedClientsGauge(allConnectionIdsHUB ?? new());
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();
                string excetpion = $"Class name : {nameof(SignalRHub)}  -- Function Name : {nameof(OnConnectedAsync)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                      appName: ServerSettings.ThisAppName,
                                                      tagsOnWhichToSend: new List<string>() { tag },
                                                      nonSerialezedDataToSend: excetpion,
                                                      jwtToken: ServerSettings.ThisAppToken);
            }
        }

        //public async override Task OnConnectedAsync()
        //{
        //    try
        //    {
        //        await base.OnConnectedAsync();

        //        var allConnectionIdsHUB = new List<string>();

        //        // Collect all connection IDs in a thread-safe manner
        //        Parallel.ForEach(TagSubscriptions, keyValuePair =>
        //        {
        //            var lockObject = TagLocks.GetOrAdd(keyValuePair.Key, new object());

        //            lock (lockObject)
        //            {
        //                allConnectionIdsHUB.AddRange(keyValuePair.Value);
        //            }
        //        });

        //        // Ensure the Context.ConnectionId is included in the list
        //        if (!allConnectionIdsHUB.Contains(Context.ConnectionId))
        //        {
        //            allConnectionIdsHUB.Add(Context.ConnectionId);
        //        }

        //        // Update the connected clients gauge
        //        ClientGaugeMetrics.UpdateConnectedClientsGauge(allConnectionIdsHUB);
        //    }
        //    catch (Exception ex)
        //    {
        //        string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();
        //        string excetpion = $"Class name : {nameof(SignalRHub)}  -- Function Name : {nameof(OnConnectedAsync)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

        //        _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
        //                                              appName: ServerSettings.ThisAppName,
        //                                              tagsOnWhichToSend: new List<string>() { tag },
        //                                              nonSerialezedDataToSend: excetpion,
        //                                              jwtToken: ServerSettings.ThisAppToken);
        //    }
        //}
        //public override async Task OnDisconnectedAsync(Exception? exception)
        //{
        //    try
        //    {
        //        await base.OnDisconnectedAsync(exception);

        //        ////Parallel.ForEach(TagSubscriptions, keyValuePair =>
        //        ////{
        //        ////    HashSet<string> hashSet = keyValuePair.Value;
        //        ////    hashSet.Remove(Context.ConnectionId);
        //        ////});
        //        bool IsCloseRadis = true;
        //        foreach (var keyValuePair in TagSubscriptions)
        //        {
        //            string key = keyValuePair.Key;
        //            HashSet<string> frontEndConnectionIdsList = keyValuePair.Value;
        //            frontEndConnectionIdsList.Remove(Context.ConnectionId);
        //            // check if any client data from redis or not
        //            if (IsCloseRadis && key.Contains("Data5Message1") || key.Contains("Data5Message2") || key.Contains("Data5Message3"))
        //            {
        //                IsCloseRadis = false;
        //            }
        //        }

        //        #region no tags register to get data from redis close redis connection
        //        //// this logic moved to above loop to avoid second loop
        //        ////foreach (var allTags in TagSubscriptions?.Keys) 
        //        ////{
        //        ////    if (allTags.Contains("Data5Message1") || allTags.Contains("Data5Message2") || allTags.Contains("Data5Message3"))
        //        ////    {
        //        ////        IsCloseRadis = false; break;
        //        ////    }
        //        ////}
        //        if (IsCloseRadis)
        //        {
        //            SignalRServer.Redis.Redis.CloseRedisConnection();
        //        }

        //        #endregion no tags register to get data from redis close redis connection
        //        _ = Task.Run(() =>
        //         {
        //             List<string> allConnectionIdsHUB = TagSubscriptions?.SelectMany(kv => kv.Value)?.Where(s => !string.IsNullOrEmpty(s))?.ToList();
        //             if (allConnectionIdsHUB != null && allConnectionIdsHUB.Count() > 0)
        //             {
        //                 ClientGaugeMetrics.UpdateConnectedClientsGauge(allConnectionIdsHUB);
        //             }
        //         });
        //        //string remoteIpAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();
        //        //_ = ClientGaugeMetrics.UpdateCurrentAuthenticatedSessions(new() { remoteIpAddress }, isAuthenticated: false);

        //    }
        //    catch (Exception ex)
        //    {
        //        string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();
        //        string excetpion = $"Class name : {nameof(SignalRHub)}  -- Function Name : {nameof(OnDisconnectedAsync)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

        //        _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
        //                                              appName: ServerSettings.ThisAppName,
        //                                              tagsOnWhichToSend: new List<string>() { tag },
        //                                              nonSerialezedDataToSend: excetpion,
        //                                              jwtToken: ServerSettings.ThisAppToken);
        //        if (ClientSettings.IsSignalRLoggingOn)
        //        {
        //            List<string> linesToLog = new List<string>();
        //            linesToLog.Add($"-------------------{DateTime.Now:dd-MM-yyyy HH-mm-ss}----------------------");
        //            linesToLog.Add($"EXCEPTION : {excetpion}");


        //            lock (ClientSettings.SignalRLoggingFileLockObject)
        //            {
        //                using (FileStream fileStream = new FileStream(ClientSettings.SignalRLogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
        //                {
        //                    // Create a StreamWriter to write to the file
        //                    using (StreamWriter streamWriter = new StreamWriter(fileStream))
        //                    {
        //                        // If the file is newly created, write a log message
        //                        //if (fileStream.Length == 0)
        //                        //{
        //                        linesToLog.Add($"-------------------/{DateTime.Now:dd-MM-yyyy HH-mm-ss}----------------------");
        //                        foreach (string s in linesToLog)
        //                        {
        //                            streamWriter.WriteLine(s);
        //                        }
        //                        // }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                await base.OnDisconnectedAsync(exception);

                bool isCloseRedis = true;

                var allConnectionIdsHUB = new List<string>();

                foreach (var keyValuePair in TagSubscriptions)
                {
                    HashSet<string> frontEndConnectionIdsList = keyValuePair.Value;
                    lock (frontEndConnectionIdsList)
                    {
                        frontEndConnectionIdsList.Remove(Context.ConnectionId);
                    }

                    // Check if any client data from Redis or not
                    if (isCloseRedis && (keyValuePair.Key.Contains("Data5Message1") || keyValuePair.Key.Contains("Data5Message2") || keyValuePair.Key.Contains("Data5Message3")))
                    {
                        isCloseRedis = false;
                    }
                    allConnectionIdsHUB.AddRange(keyValuePair.Value);
                }

                if (allConnectionIdsHUB.Count > 0)
                {
                    ClientGaugeMetrics.UpdateConnectedClientsGauge(allConnectionIdsHUB);
                }

                //Parallel.ForEach(TagSubscriptions, keyValuePair =>
                //{
                //    var lockObject = TagLocks.GetOrAdd(keyValuePair.Key, new object());

                //    lock (lockObject)
                //    {
                //        HashSet<string> frontEndConnectionIdsList = keyValuePair.Value;
                //        frontEndConnectionIdsList.Remove(Context.ConnectionId);

                //        // Check if any client data from Redis or not
                //        if (isCloseRedis && (keyValuePair.Key.Contains("Data5Message1") || keyValuePair.Key.Contains("Data5Message2") || keyValuePair.Key.Contains("Data5Message3")))
                //        {
                //            isCloseRedis = false;
                //        }
                //    }
                //});

                if (isCloseRedis)
                {
                    SignalRServer.Redis.Redis.CloseRedisConnection();
                }

                //_ = Task.Run(() =>
                //{
                //    var allConnectionIdsHUB = new List<string>();
                //    Parallel.ForEach(TagSubscriptions, keyValuePair =>
                //    {
                //        var lockObject = TagLocks.GetOrAdd(keyValuePair.Key, new object());

                //        lock (lockObject)
                //        {
                //            allConnectionIdsHUB.AddRange(keyValuePair.Value);
                //        }
                //    });

                //    if (allConnectionIdsHUB.Count > 0)
                //    {
                //        ClientGaugeMetrics.UpdateConnectedClientsGauge(allConnectionIdsHUB);
                //    }
                //});
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();
                string excetpion = $"Class name : {nameof(SignalRHub)}  -- Function Name : {nameof(OnDisconnectedAsync)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                      appName: ServerSettings.ThisAppName,
                                                      tagsOnWhichToSend: new List<string>() { tag },
                                                      nonSerialezedDataToSend: excetpion,
                                                      jwtToken: ServerSettings.ThisAppToken);
                if (ClientSettings.IsSignalRLoggingOn)
                {
                    List<string> linesToLog = new List<string>();
                    linesToLog.Add($"-------------------{DateTime.Now:dd-MM-yyyy HH-mm-ss}----------------------");
                    linesToLog.Add($"EXCEPTION : {excetpion}");


                    lock (ClientSettings.SignalRLoggingFileLockObject)
                    {
                        using (FileStream fileStream = new FileStream(ClientSettings.SignalRLogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                        {
                            // Create a StreamWriter to write to the file
                            using (StreamWriter streamWriter = new StreamWriter(fileStream))
                            {
                                // If the file is newly created, write a log message
                                //if (fileStream.Length == 0)
                                //{
                                linesToLog.Add($"-------------------/{DateTime.Now:dd-MM-yyyy HH-mm-ss}----------------------");
                                foreach (string s in linesToLog)
                                {
                                    streamWriter.WriteLine(s);
                                }
                                // }
                            }
                        }
                    }
                }
            }
        }
        #region functanility of hub
        public async Task SendMessageToClientsViaSignalRHubToAllClients(string clientTagReceivingThisMessage, string data)
        {

            try
            {
                //sends to all clients
                //await Clients.All.SendAsync(clientTagReceivingThisMessage, clientTagReceivingThisMessage, data);
                _ = Clients.All.SendAsync(clientTagReceivingThisMessage, clientTagReceivingThisMessage, data);
                _ = Task.Run(() =>
                {
                    List<string> allStrings = TagSubscriptions?.SelectMany(kv => kv.Value)?.Where(s => !string.IsNullOrEmpty(s))?.ToList();
                    if (allStrings != null && allStrings.Count() > 0)
                    {
                        ClientGaugeMetrics.UpdateMessageSendForSetTimeSpan(allStrings);
                    }
                });
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();
                string excetpion = $"Class name : {nameof(SignalRHub)}  -- Function Name : {nameof(SendMessageToClientsViaSignalRHubToAllClients)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                      appName: ServerSettings.ThisAppName,
                                                      tagsOnWhichToSend: new List<string>() { tag },
                                                      nonSerialezedDataToSend: excetpion,
                                                      jwtToken: ServerSettings.ThisAppToken);
            }
        }

        // hubs sends the messages to those clients only who have subcribed the tag
        public async Task SendMessageToClientsViaSignalRHub(string clientTagReceivingThisMessage, string data)
        {
            try
            {
                if (TagSubscriptions.TryGetValue(clientTagReceivingThisMessage, out HashSet<string> value))
                {
                    //string[] connectionIds = TagSubscriptions[clientTagReceivingThisMessage].ToArray();
                    string[] connectionIds = value?.ToArray() ?? new string[0];
                    _ = Clients.Clients(connectionIds).SendAsync(clientTagReceivingThisMessage, clientTagReceivingThisMessage, data);
                    // update prometheus matrics
                    _ = Task.Run(() => ClientGaugeMetrics.UpdateMessageSendForSetTimeSpan(connectionIds.ToList()));
                    // convert below conde to a function as it is used at many places with taking file path to write as in parameter
                    if (ClientSettings.IsSignalRLoggingOn && ClientSettings.LastTimeLoggedInFile <= DateTime.UtcNow.AddSeconds(-1))
                    {
                        if (DirectoryFileHelper.CreateFileAtGivenPathIfNotPresent(ClientSettings.SignalRLogFilePath))
                        {
                            List<string> linesToLog = new List<string> { $"-------------------{DateTime.Now:dd-MM-yyyy HH-mm-ss}----------------------" };
                            linesToLog.Add($"sending message to clients if found subcribed for tag : {clientTagReceivingThisMessage}");
                            linesToLog.Add($"message send to clients : {string.Join(" | ", connectionIds)}");
                            lock (ClientSettings.SignalRLoggingFileLockObject)
                            {
                                linesToLog.Add($"-------------------/{DateTime.Now:dd-MM-yyyy HH-mm-ss}----------------------");
                                File.AppendAllLines(ClientSettings.SignalRLogFilePath, linesToLog);
                            }
                        }
                        ClientSettings.LastTimeLoggedInFile = DateTime.UtcNow;
                    }
                }
                else
                {
                    if (ClientSettings.IsSignalRLoggingOn && ClientSettings.LastTimeLoggedInFile <= DateTime.UtcNow.AddSeconds(-1))
                    {
                        if (DirectoryFileHelper.CreateFileAtGivenPathIfNotPresent(ClientSettings.SignalRLogFilePath))
                        {
                            List<string> linesToLog = new List<string> { $"-------------------{DateTime.Now:dd-MM-yyyy HH-mm-ss}----------------------" };
                            linesToLog.Add($"tag not found with any client : {clientTagReceivingThisMessage}");
                            linesToLog.Add($"-------------------/{DateTime.Now:dd-MM-yyyy HH-mm-ss}----------------------");
                            lock (ClientSettings.SignalRLoggingFileLockObject)
                            {
                                File.AppendAllLines(ClientSettings.SignalRLogFilePath, linesToLog);
                            }
                        }
                        ClientSettings.LastTimeLoggedInFile = DateTime.UtcNow.AddSeconds(-1);
                    }
                }
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();
                string excetpion = $"Class name : {nameof(SignalRHub)}  -- Function Name : {nameof(SendMessageToClientsViaSignalRHub)} -----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                      appName: ServerSettings.ThisAppName,
                                                      tagsOnWhichToSend: new List<string>() { tag },
                                                      nonSerialezedDataToSend: excetpion,
                                                      jwtToken: ServerSettings.ThisAppToken);



                lock (ClientSettings.SignalRLoggingFileLockObject)
                {
                    File.WriteAllText(ClientSettings.SignalRLogFilePath, excetpion + Environment.NewLine);
                }
            }
        }
        public async Task SendMessageToCurrentContextClientViaSignalRHub_DoNotCallInBacgroud(string clientTagReceivingThisMessage, string data)
        {
            try
            {
                List<string> linesToLog = new List<string> { $"-------------------{DateTime.Now:dd-MM-yyyy HH-mm-ss}----------------------" };
                if (ClientSettings.IsSignalRLoggingOn)
                {
                    linesToLog.Add($"sending message to client with connection id : {Context.ConnectionId} | if found subcribed for tag : {clientTagReceivingThisMessage}");
                }

                if (TagSubscriptions.TryGetValue(clientTagReceivingThisMessage, out HashSet<string> value))
                {
                    if (value?.Count > 0 && value.Contains(Context.ConnectionId))
                    {
                        //string[] connectionIds = TagSubscriptions[clientTagReceivingThisMessage].ToArray();
                        string[] connectionIds = new string[1] { Context.ConnectionId };
                        if (ClientSettings.IsSignalRLoggingOn)
                        {
                            linesToLog.Add($"message send to clients : {string.Join(" | ", connectionIds)}");
                        }

                        var clients = Clients.Clients(connectionIds).SendAsync(clientTagReceivingThisMessage, clientTagReceivingThisMessage, data);
                        // update prometheus matrics
                        _ = Task.Run(() => ClientGaugeMetrics.UpdateMessageSendForSetTimeSpan(connectionIds.ToList()));
                        // convert below conde to a function as it is used at many places with taking file path to write as in parameter
                        if (ClientSettings.IsSignalRLoggingOn)
                        {
                            if (clients == null)
                            {
                                linesToLog.Add($"tried to send to connection | {Context.ConnectionId} | but client not found but found this id registered for tag : | {clientTagReceivingThisMessage} | in registered dictionary.");
                            }
                            else
                            {
                                linesToLog.Add($"send succesfully to connection | {Context.ConnectionId} | for tag : | {clientTagReceivingThisMessage} |.");
                            }
                        }
                    }
                    else if (ClientSettings.IsSignalRLoggingOn)
                    {
                        linesToLog.Add($"current connection | {Context.ConnectionId} | is not registered for tag : {clientTagReceivingThisMessage}");
                    }
                }
                else if (ClientSettings.IsSignalRLoggingOn)
                {
                    linesToLog.Add($"tag not found with any client : {clientTagReceivingThisMessage}");
                }

                if (ClientSettings.IsSignalRLoggingOn && ClientSettings.LastTimeLoggedInFile <= DateTime.UtcNow.AddSeconds(-1))
                {
                    if (DirectoryFileHelper.CreateFileAtGivenPathIfNotPresent(ClientSettings.SignalRLogFilePath))
                    {
                        lock (ClientSettings.SignalRLoggingFileLockObject)
                        {
                            linesToLog.Add($"-------------------/{DateTime.Now:dd-MM-yyyy HH-mm-ss}----------------------");
                            File.AppendAllLines(ClientSettings.SignalRLogFilePath, linesToLog);
                        }
                    }
                    ClientSettings.LastTimeLoggedInFile = DateTime.UtcNow.AddSeconds(-1);
                }
            }
            catch (Exception ex)
            {
                string excetpion = $"Class name : {nameof(SignalRHub)}  -- Function Name : {nameof(SendMessageToCurrentContextClientViaSignalRHub_DoNotCallInBacgroud)} -----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                lock (ClientSettings.SignalRLoggingFileLockObject)
                {
                    File.WriteAllText(ClientSettings.SignalRLogFilePath, excetpion + Environment.NewLine);
                }
            }
        }

        public async Task SendMessageToGivenConnectionIdsIfSubscribedGivenTags_ViaSignalRHub(string clientTagReceivingThisMessage, string data, List<string> connectionIds_ToSendThisDataIfTheseConnectionIdsSubscribedThisTag)
        {
            try
            {
                List<string> linesToLog = new List<string> { $"-------------------{DateTime.Now:dd-MM-yyyy HH-mm-ss}----------------------" };


                if (connectionIds_ToSendThisDataIfTheseConnectionIdsSubscribedThisTag?.Count() > 0 &&
                    TagSubscriptions.TryGetValue(clientTagReceivingThisMessage, out HashSet<string> value))
                {

                    if (value?.Count > 0)
                    {
                        // some connection ids are registered for this tag
                        string[] registeredConnectionIdsInGivenConnectionIdsList = value.Intersect(connectionIds_ToSendThisDataIfTheseConnectionIdsSubscribedThisTag)?.ToArray() ?? new string[0];
                        if (registeredConnectionIdsInGivenConnectionIdsList.Length > 0)
                        {
                            // found registered connection ids for this tag within the given connection ids
                            var clients = Clients.Clients(registeredConnectionIdsInGivenConnectionIdsList).SendAsync(clientTagReceivingThisMessage, clientTagReceivingThisMessage, data);

                            _ = Task.Run(() => ClientGaugeMetrics.UpdateMessageSendForSetTimeSpan(registeredConnectionIdsInGivenConnectionIdsList.ToList()));

                            // convert below conde to a function as it is used at many places with taking file path to write as in parameter
                            if (ClientSettings.IsSignalRLoggingOn)
                            {
                                if (clients == null)
                                {
                                    linesToLog.Add($"sending message to client with connection id : {connectionIds_ToSendThisDataIfTheseConnectionIdsSubscribedThisTag?.Join(" | ")} | if found subcribed for tag : {clientTagReceivingThisMessage}");
                                    linesToLog.Add($"tried to send to connections found regiestered  | {string.Join(" | ", registeredConnectionIdsInGivenConnectionIdsList)} |" +
                                        $"{Environment.NewLine} out of given connection ids| {string.Join(" , ", connectionIds_ToSendThisDataIfTheseConnectionIdsSubscribedThisTag)} | " +
                                        $"{Environment.NewLine} but client not found but found registered for tag : | {clientTagReceivingThisMessage} | in registered dictionary.");
                                }
                                else
                                {
                                    linesToLog.Add($"message send to clients : {string.Join(" | ", registeredConnectionIdsInGivenConnectionIdsList)}");
                                }
                            }
                        }
                        else
                        {

                            if (ClientSettings.IsSignalRLoggingOn && ClientSettings.LastTimeLoggedInFile <= DateTime.UtcNow.AddSeconds(-1))
                            {
                                linesToLog.Add($"sending message to client with connection id : {connectionIds_ToSendThisDataIfTheseConnectionIdsSubscribedThisTag?.Join(" | ")} | if found subcribed for tag : {clientTagReceivingThisMessage}");
                                //given connection ids does not contain any registered connection ids
                                linesToLog.Add($"no connection id out of these connection id was registered for tag {clientTagReceivingThisMessage} out of given connection ids : {string.Join(" | ", connectionIds_ToSendThisDataIfTheseConnectionIdsSubscribedThisTag)}");

                                if (DirectoryFileHelper.CreateFileAtGivenPathIfNotPresent(ClientSettings.SignalRLogFilePath))
                                {
                                    lock (ClientSettings.SignalRLoggingFileLockObject)
                                    {
                                        linesToLog.Add($"-------------------/{DateTime.Now:dd-MM-yyyy HH-mm-ss}----------------------");
                                        File.AppendAllLines(ClientSettings.SignalRLogFilePath, linesToLog);
                                    }
                                }
                                ClientSettings.LastTimeLoggedInFile = DateTime.UtcNow.AddSeconds(-1);
                            }
                        }
                    }
                }
                else
                {
                    // this tag is not registered for any connection id in hub
                    if (ClientSettings.IsSignalRLoggingOn && ClientSettings.LastTimeLoggedInFile <= DateTime.UtcNow.AddSeconds(-1))
                    {
                        if (DirectoryFileHelper.CreateFileAtGivenPathIfNotPresent(ClientSettings.SignalRLogFilePath))
                        {
                            lock (ClientSettings.SignalRLoggingFileLockObject)
                            {
                                linesToLog.Add($"tConnection ids given to send this data count = {connectionIds_ToSendThisDataIfTheseConnectionIdsSubscribedThisTag.Count()} | or ag not found with any client : {clientTagReceivingThisMessage}");
                                linesToLog.Add($"Given connection ids to send this tag data : {clientTagReceivingThisMessage}");
                                linesToLog.Add($"-------------------/{DateTime.Now:dd-MM-yyyy HH-mm-ss}----------------------");
                                File.AppendAllLines(ClientSettings.SignalRLogFilePath, linesToLog);
                            }
                        }
                        ClientSettings.LastTimeLoggedInFile = DateTime.UtcNow.AddSeconds(-1);
                    }
                }
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();
                string excetpion = $"Class name : {nameof(SignalRHub)}  -- Function Name : {nameof(SendMessageToGivenConnectionIdsIfSubscribedGivenTags_ViaSignalRHub)} -----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                      appName: ServerSettings.ThisAppName,
                                                      tagsOnWhichToSend: new List<string>() { tag },
                                                      nonSerialezedDataToSend: excetpion,
                                                      jwtToken: ServerSettings.ThisAppToken);



                lock (ClientSettings.SignalRLoggingFileLockObject)
                {
                    File.WriteAllText(ClientSettings.SignalRLogFilePath, excetpion + Environment.NewLine);
                }
            }
        }

        //public async Task SendSubscribedTagsByThisConnectionToClient()
        //{
        //    try
        //    {
        //        List<string> tagsSubscribed = GetSubscribedTagsByConnectionId(Context.ConnectionId);

        //        if (tagsSubscribed != null && tagsSubscribed.Count() > 0)
        //        {
        //            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tagsSubscribed);//function name is same as tag for reciving subcribed tags
        //            var clietsThatHaveSubscribedthisTag = Clients.Clients(new List<string> { Context.ConnectionId });
        //            await clietsThatHaveSubscribedthisTag.SendAsync(nameof(SendSubscribedTagsByThisConnectionToClient), nameof(SendSubscribedTagsByThisConnectionToClienst), json);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();
        //        string excetpion = $"Class name : {nameof(SignalRHub)}  -- Function Name : {nameof(SendSubscribedTagsByThisConnectionToClient)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

        //        SignalRClient.SignalRClient.SendMessage(hubCompleteurl: Constants.SignalRHubUrl,
        //                                      appName: Constants.Constants.ThisAppName,
        //                                      tagsOnWhichToSend: new List<string>() { tag },
        //                                      nonSerialezedDataToSend: excetpion,
        //                                      jwtToken: SignalRServer.Constants.Constants.ThisAppToken);
        //    }
        //}

        private List<string> GetSubscribedTagsByConnectionId(string connectionId)
        {
            List<string> subscribedTags = new List<string>();
            try
            {
                Parallel.ForEach(TagSubscriptions, entry =>
                {
                    if (entry.Value?.Contains(connectionId) ?? false)
                    {
                        subscribedTags.Add(entry.Key);
                    }
                });

                //foreach (var entry in TagSubscriptions)
                //{
                //    if (entry.Value?.Contains(connectionId) ?? false)
                //    {
                //        subscribedTags.Add(entry.Key);
                //    }
                //}
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();

                string excetpion = $"Class name : {nameof(SignalRHub)}  -- Function Name : {nameof(GetSubscribedTagsByConnectionId)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                              appName: ServerSettings.ThisAppName,
                                              tagsOnWhichToSend: new List<string>() { tag },
                                              nonSerialezedDataToSend: excetpion,
                                              jwtToken: ServerSettings.ThisAppToken);
            }
            return subscribedTags;
        }
        public async Task SubscribeToTagsInHub(List<string> tagsToSubsCribe)
        {
            try
            {
                if (tagsToSubsCribe != null && tagsToSubsCribe.Count() > 0)
                {
                    List<string> redisTagsToSubscribe = new();
                    tagsToSubsCribe.ForEach(async (tagName) =>
                    {
                        if (string.IsNullOrEmpty(tagName))
                        {
                            return;
                        }

                        if (TagSubscriptions == null)
                        {
                            TagSubscriptions = new();
                        }


                        // add key value pair

                        TagSubscriptions.AddOrUpdate(
                                    tagName,
                                    key =>
                                    {
                                        // This function is called when the key is not present in the dictionary
                                        // Create a new HashSet and add the connection ID
                                        var newHashSet = new HashSet<string>();
                                        newHashSet.Add(Context.ConnectionId);
                                        return newHashSet;
                                    },
                                    (key, existingHashSet) =>
                                    {
                                        // This function is called when the key is already present in the dictionary
                                        // Update the existing HashSet by adding the connection ID
                                        lock (existingHashSet)
                                        {
                                            existingHashSet.Add(Context.ConnectionId);
                                            return existingHashSet;
                                        }
                                    }
                                );

                        await Groups.AddToGroupAsync(Context.ConnectionId, tagName);

                        if (HubTimers.ProcessedModelBufferExceptData1.TryGetValue(tagName, out SignalrClientDataModal value))
                        {
                            // The key exists, and 'value' now contains the corresponding value
                            //HubTimers.SendProcessedMessgesToSignalRClientsWithRetry(value, tagName);
                            //await SendMessageToClientsViaSignalRHub(tagName, serilizedJSonData);
                            try
                            {

                                if (value != null)
                                {
                                    if (ServerSettings.IsSendDataOnSubscribe && value.ModalTypeName == nameof(DataModal))
                                    {
                                        HubTimers.RecordsToBeProcessedForSpecifiedTime.AddOrUpdate(
                                                             (tagName, value, Context.ConnectionId),
                                                             (TimeSpan.FromSeconds(ServerSettings.SendData1InSecondsOnSub), DateTime.UtcNow, DateTime.UtcNow.AddSeconds(ServerSettings.SendData1ForSecondsOnSub)),// sending only onece after 2 second of reciving
                                                             (key, existingModel) =>
                                                             {
                                                                 // Update the existing Processed modal if needed
                                                                 existingModel = (TimeSpan.FromSeconds(1), DateTime.UtcNow, DateTime.UtcNow.AddSeconds(7));
                                                                 return existingModel;
                                                             });

                                        string serilizedJSonData = SerilizingDeserilizing.JSONSerializeOBJ(value);
                                        await SendMessageToGivenConnectionIdsIfSubscribedGivenTags_ViaSignalRHub(tagName, serilizedJSonData, new() { Context.ConnectionId });
                                    }
                                    else if (ServerSettings.IsSendOtherThanData1OnSubscribe)
                                    {
                                        string serilizedJSonData = SerilizingDeserilizing.JSONSerializeOBJ(value);
                                        await SendMessageToGivenConnectionIdsIfSubscribedGivenTags_ViaSignalRHub(tagName, serilizedJSonData, new() { Context.ConnectionId });
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }

                        if (ClientSettings.IsSignalRLoggingOn && ClientSettings.LastTimeLoggedInFile <= DateTime.UtcNow.AddSeconds(-1))
                        {
                            if (DirectoryFileHelper.CreateFileAtGivenPathIfNotPresent(ClientSettings.SignalRLogFilePath))
                            {
                                List<string> linesToLog = new List<string>
                                {
                                    $"-------------------{DateTime.Now:dd-MM-yyyy HH-mm-ss}----------------------",
                                    $"Tags came to subscribe on hub : {string.Join(" | ", tagsToSubsCribe)}",
                                    $"-------------------/{DateTime.Now:dd-MM-yyyy HH-mm-ss}----------------------"
                                };
                                lock (ClientSettings.SignalRLoggingFileLockObject)
                                {
                                    File.AppendAllLines(ClientSettings.SignalRLogFilePath, linesToLog);
                                }
                            }
                            ClientSettings.LastTimeLoggedInFile = DateTime.UtcNow;
                        }
                        if (tagName.Contains("Data5Message1") || tagName.Contains("Data5Message2") || tagName.Contains("Data5Message3"))
                        {
                            // create list of redis tags to subcribe
                            redisTagsToSubscribe.Add(tagName);
                        }

                    });
                    if (ClientSettings.IsSignalRLoggingOn)
                    {
                        List<string> linesToLog = new List<string>
                        {
                            $"-------------------{DateTime.Now:dd-MM-yyyy HH-mm-ss}----------------------",
                            $"Tags came to subscribe on hub : {string.Join(" | ", tagsToSubsCribe)}",
                            $"All these tags subcribed on hub and send message instantly from buffer except Data1 Data Data1 Data will be send in every 3 seconds",
                            $"-------------------/{DateTime.Now:dd-MM-yyyy HH-mm-ss}----------------------"
                        };
                        if (DirectoryFileHelper.CreateFileAtGivenPathIfNotPresent(ClientSettings.SignalRLogFilePath))
                        {
                            lock (ClientSettings.SignalRLoggingFileLockObject)
                            {
                                File.AppendAllLines(ClientSettings.SignalRLogFilePath, linesToLog);
                            }
                        }
                    }


                    _ = Task.Run(async () =>
                        {
                            try
                            {
                                if (ServerSettings.IsCnonnectToRedis && redisTagsToSubscribe?.Count > 0)
                                {
                                    // START REDIS        
                                    //if (SignalRServer.Redis.Redis.RedisClient == null || (!SignalRServer.Redis.Redis.RedisClient.IsRedisConnected()))
                                    if (!ServerSettings.IsRedisClientCreating && !RedisClient.MyRedisClient.IsRedisConnected && !RedisClient.MyRedisClient.IsConnecting)
                                    {
                                        ServerSettings.IsRedisClientCreating = true;
                                        RedisClient.MyRedisClient.GetRedisClientAndSubcriber();
                                        ServerSettings.IsRedisClientCreating = false;

                                        if (ServerSettings.IsSendDataOnDebugTag)
                                        {
                                            string tag = SignalRClient.SignalRClient.GetDebugTag();
                                            string message = "created redis connection sussesfull";

                                            _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                                                  appName: ServerSettings.ThisAppName,
                                                                                  tagsOnWhichToSend: new List<string>() { tag },
                                                                                  nonSerialezedDataToSend: message,
                                                                                  jwtToken: ServerSettings.ThisAppToken);
                                        }
                                    }
                                    if (RedisClient.MyRedisClient.IsRedisConnected && !ServerSettings.IsRedisDeligatesSubscribed)
                                    {
                                        ServerSettings.IsRedisDeligatesSubscribed = true;

                                        //#region noUSubsCribe
                                        //RedisClient.RedisClient.Data4MessageReceived += SignalRServer.Redis.Redis.HandleData4MessageReceived;
                                        //RedisClient.RedisClient.Data5Message3Received += SignalRServer.Redis.Redis.HandleData5Message3Received;
                                        //RedisClient.RedisClient.Data5EventMessageReceived += Redis.Redis.HandleData6MessageReceived;
                                        //#endregion noUSubsCribe
                                        if (!ServerSettings.IsSubscribeForRedisItemspecificData)
                                        {
                                            #region use either this or for individual device subcripition region below
                                            // subcribe to all Items only onece 
                                            if (redisTagsToSubscribe.Count > 0)
                                            {
                                                // tag for all Items data
                                                redisTagsToSubscribe = new List<string>() { "Redis_Data5Message-*" };
                                                if (RedisClient.MyRedisClient.IsRedisConnected)
                                                {
                                                    _ = RedisClient.MyRedisClient.SubscribeToChannelsAndGetAvailabilityAsync(redisTagsToSubscribe);
                                                }
                                                else
                                                {
                                                    // add to dictionary which will subcribe to redis as connection is restored
                                                    RedisClient.MyRedisClient.SubscribeWhenRestoredConnection(redisTagsToSubscribe);
                                                }
                                            }
                                            #endregion use either this or for individual device subcripition region below
                                        }
                                    }
                                    if (ServerSettings.IsSubscribeForRedisItemspecificData)
                                    {
                                        #region for individual device subscription as per request from client
                                        if (redisTagsToSubscribe.Count > 0)
                                        {
                                            if (RedisClient.MyRedisClient.IsRedisConnected)
                                            {
                                                _ = RedisClient.MyRedisClient.SubscribeToChannelsAndGetAvailabilityAsync(redisTagsToSubscribe);
                                            }
                                            else
                                            {
                                                // add to dictionary which will subcribe to redis as connection is restored
                                                RedisClient.MyRedisClient.SubscribeWhenRestoredConnection(redisTagsToSubscribe);
                                            }

                                            if (ServerSettings.IsSendDataOnDebugTag)
                                            {
                                                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();
                                                string message = "Redis Tags came to subcribe and send to redis if redis is connction is succesul received on debug tag , do on from app setting send on debug tag = true" + SerilizingDeserilizing.JSONSerializeOBJ(redisTagsToSubscribe);

                                                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                                                      appName: ServerSettings.ThisAppName,
                                                                                      tagsOnWhichToSend: new List<string>() { tag },
                                                                                      nonSerialezedDataToSend: message,
                                                                                      jwtToken: ServerSettings.ThisAppToken);
                                            }
                                        }
                                        #endregion for individual device subscription as per request from client
                                    }
                                }
                                //can not send in the background thread signalr hub context is disposed
                                //SendSubscribedTagsByThisConnectionToClient();

                            }
                            catch (Exception ex)
                            {
                                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();

                                string excetpion = $"Class name : {nameof(SignalRHub)}  -- Function Name : {nameof(SubscribeToTagsInHub)}" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                               _= SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                                                  appName: ServerSettings.ThisAppName,
                                                                                  tagsOnWhichToSend: new List<string>() { tag
                            },
                                                                                  nonSerialezedDataToSend: excetpion,
                                                                                  jwtToken: ServerSettings.ThisAppToken);
                            }
                        });

                    //SendSubscribedTagsByThisConnectionToClient();
                }
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();

                string excetpion = $"Class name : {nameof(SignalRHub)}  -- Function Name : {nameof(SubscribeToTagsInHub)}" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

               _= SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                                  appName: ServerSettings.ThisAppName,
                                                                  tagsOnWhichToSend: new List<string>() { tag
            },
                                                                  nonSerialezedDataToSend: excetpion,
                                                                  jwtToken: ServerSettings.ThisAppToken);
            }
        }
        public async Task UnSubscribeFromTags(List<string> tagsToUnSubscribe)
        {
            try
            {
                if (tagsToUnSubscribe != null && tagsToUnSubscribe.Count() > 0 && true)// if this connection id has subscribed to this tag 
                {
                    List<string> tagsToUnSubcribeInRedis = new();
                    // then unsubcribe this tag from this connection 
                    tagsToUnSubscribe.ForEach(async (tagName) =>
                    {
                        if (!string.IsNullOrEmpty(tagName))
                        {
                            await Groups.RemoveFromGroupAsync(Context.ConnectionId, tagName);
                            if (TagSubscriptions.ContainsKey(tagName))
                            {
                                if (TagSubscriptions.TryGetValue(tagName, out var existingHashSet))
                                {
                                    lock (existingHashSet)
                                    {
                                        existingHashSet.Remove(Context.ConnectionId);
                                    }
                                }
#warning this sould be uncommented but with good testing for the case when unsubcribed then unsubcribed and then unsubcribed with one or more  clients
                                if (TagSubscriptions[tagName]?.Count() < 1)
                                {
                                    TagSubscriptions.Remove(tagName, out HashSet<string> itsValueShouldBeEmpty);
                                    if (tagName.Contains("Data5Message1") || tagName.Contains("Data5Message2") || tagName.Contains("Data5Message3"))
                                    {
                                        tagsToUnSubcribeInRedis.Add(tagName);
                                    }
                                }
                            }

                            #region removeTimePeriodRequestIfNoOnewatsThatDataAlsoClearedHOurly
                            //HubTimers.TimePeriodAndRequestObj.AddOrUpdate(
                            //        DataRequest.RunAfterSeconds,//for 1 second requests (key)
                            //        key =>
                            //        {
                            //            // This function is called when the key is not present in the dictionary
                            //            // Create a new HashSet and add the connection ID
                            //            var newRequestListFor1Second = new List<object>() { DataRequest };
                            //            return newRequestListFor1Second;
                            //        },
                            //        (key, existingHashSet) =>
                            //        {
                            //            // This function is called when the key is already present in the dictionary
                            //            // Update the existing HashSet by adding the connection ID
                            //            if (existingHashSet != null)
                            //            {
                            //                existingHashSet.Add(DataRequest);
                            //                return existingHashSet;
                            //            }
                            //            else
                            //            {
                            //                // if null
                            //                existingHashSet = new List<object>() { DataRequest };
                            //                return existingHashSet;
                            //            }
                            //        }
                            //    );


                            //if (SignalRServer.HubTimers.TimePeriodAndRequestObj.ContainsKey(tagName))
                            //{
                            //    TimePeriodAndRequestobj[tagName].Remove(Context.ConnectionId);
                            //    if (TagSubscriptions[tagName]?.Count() < 1)
                            //    {
                            //        TagSubscriptions.Remove(tagName, out HashSet<string> itsValueShouldBeEmpty);
                            //    }
                            //}
                            #endregion removeTimePeriodRequestIfNoOnewatsThatDataAlsoClearedHOurly
                        }
                    });
                    if (ServerSettings.IsSubscribeForRedisItemspecificData)
                    {

                        #region withsingleClientCodeNotAbleToDoSubcriptionAndGetDataWithSingleClient
                        if (tagsToUnSubcribeInRedis.Count > 0 /*&& SignalRServer.Redis.Redis.RedisClient.IsRedisConnected*/)
                        {
                            RedisClient.MyRedisClient.UnSubscribeToChannelsAndGetAvailabilityAsync(tagsToUnSubcribeInRedis);
                        }
                        #endregion withsingleClientCodeNotAbleToDoSubcriptionAndGetDataWithSingleClient
                    }

                    // can not send in background thread objec  will be disposed  
                    //SendSubscribedTagsByThisConnectionToClient();

                }
                #region no tags register to get data from redis close redis connection
                bool isCloseRadis = true;
                foreach (var allTags in TagSubscriptions?.Keys)
                {
                    if (allTags.Contains("Data5Message1") || allTags.Contains("Data5Message2") || allTags.Contains("Data5Message3"))
                    {
                        isCloseRadis = false; break;
                    }
                }
                if (isCloseRadis)
                {
                    SignalRServer.Redis.Redis.CloseRedisConnection();
                }
                #endregion no tags register to get data from redis close redis connection
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();

                string excetpion = $"Class name : {nameof(SignalRHub)}  -- Function Name : {nameof(UnSubscribeFromTags)}------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

               _= SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                              appName: ServerSettings.ThisAppName,
                                              tagsOnWhichToSend: new List<string>() { tag },
                                              nonSerialezedDataToSend: excetpion,
                                              jwtToken: ServerSettings.ThisAppToken);
            }
        } 
        #endregion
    }

}