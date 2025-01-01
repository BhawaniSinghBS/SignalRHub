using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace SignalRClient
{
    public static class SignalRClient
    {
        private static HubConnection? hubConnection;
        public static string SignalRConnectionID { get; set; }
        private static List<string> _tagsSubscribed = new List<string>();//// needs to put in local storage
        private static List<string> tagsSubscribed
        {
            get => _tagsSubscribed;
            set
            {
                _tagsSubscribed = value ?? new();
                TagsSubscribed = TagsSubscribed;// invoke state has changed to notify frontend to check values in the property
            }
        }
        public static List<string> TagsSubscribed
        {
            get => tagsSubscribed;
            set {/* for updating fron end invoke chage has happen but no set because get is from private property of this*/ }
        }  // needs to put in local storage
        public static bool IsConnected => hubConnection?.State == HubConnectionState.Connected;
        public static bool IsConnecting => hubConnection?.State == HubConnectionState.Connecting;
        public static bool IsDisconnected => hubConnection?.State == HubConnectionState.Disconnected;
        //public static List<object> messages = new();
        public static event Action<string, string> MessageReceived;
        private static void HandleMessageReceived(string tag, string jsonData)
        {
            return;
        }
        public static async Task<bool> ConnectToHub(string signalRHubURL, string appName, string jwtToken, List<string> tagsToSubsCribe = null, Action<string, string> functionToHandleDatasAtClientApplication = null)
        {

            if (SignalRClient.IsConnected && !SignalRClient.IsConnecting)
            {
                return true;
            }
            if (SignalRClient.IsConnecting)
            {
                return false;
            }
            MessageReceived = HandleMessageReceived;
            if (functionToHandleDatasAtClientApplication != null)
            {
                MessageReceived += functionToHandleDatasAtClientApplication;
            }
            List<string> tagsToSubsCribeInHub = tagsToSubsCribe?.ToList() ?? new();
            if (tagsToSubsCribeInHub == null) // adding default tags to subscribe
            {
                tagsToSubsCribeInHub = new() { "defaulttag", "SendSubscribedTagsByThisConnectionToClient", GetPingTag() };
            }
            else if (!tagsToSubsCribeInHub.Contains("SendSubscribedTagsByThisConnectionToClient"))// it will get the subcribed tag by my app from server in this connection
            {
                tagsToSubsCribeInHub.Add("SendSubscribedTagsByThisConnectionToClient");// add
            }

            if (!tagsToSubsCribeInHub.Contains("defaulttag"))
            {
                tagsToSubsCribeInHub.Add("defaulttag");//add
            }

            if (!tagsToSubsCribeInHub.Contains(GetPingTag()))
            {
                tagsToSubsCribeInHub.Add(GetPingTag());//add
            }

            if (!tagsToSubsCribeInHub.Contains(GetDebugTag()))
            {
                tagsToSubsCribeInHub.Add(GetDebugTag());//add
            }

            try
            {
                hubConnection = new HubConnectionBuilder()
                .WithUrl(signalRHubURL, options =>
                {
                    options.Headers.Add("Authorization", $"Bearer {jwtToken}");
                    options.Headers.Add("AppName", $"{appName}");
                    options.CloseTimeout = TimeSpan.FromMinutes(8);
                })
                .WithAutomaticReconnect()
                .Build();
                hubConnection.ServerTimeout = TimeSpan.FromMinutes(8);
                //}

                hubConnection.Closed += async (error) =>
                {
                    List<string> linesToLog = new List<string>()
                    {
                    $"------------------ {DateTime.Now:dd-MM-yyyy HH-mm-ss} Automatic----------------",
                    $"Controller hub Connection closed.Trying to automatic reconnect connectionId =  {SignalRConnectionID}",

                    };

                    // Add a delay before attempting to reconnect (optional)
                    await Task.Delay(1000);

                    // Try to reconnect
                    await hubConnection.StartAsync();

                    if (ClientSettings.ClientSettings.IsSignalRLoggingOn)
                    {
                        lock (ClientSettings.ClientSettings.SignalRLoggingFileLockObject)
                        {
                            linesToLog.Add($"------------------ /{DateTime.Now:dd-MM-yyyy HH-mm-ss} Automatic----------------");
                            System.IO.File.WriteAllLines(ClientSettings.ClientSettings.SignalRLogFilePath, linesToLog);
                        }
                    }
                };

                hubConnection.Reconnecting += async (exception) =>
                {
                    List<string> linesToLog = new List<string>()
                    {
                        $"------------------ {DateTime.Now:dd-MM-yyyy HH-mm-ss} Automatic----------------",
                        $"Controller hub trying to automatic reconnect .",
                        $" Exception :",
                        $"",
                        $"------------------ {SerilizingDeserilizing.SerilizingDeserilizing.JSONSerializeOBJ(exception)}----------------",
                        $"",
                        $"Controller hub Connection closed. Trying to reconnect connectionId = {SignalRConnectionID}"
                    };

                    if (ClientSettings.ClientSettings.IsSignalRLoggingOn)
                    {
                        lock (ClientSettings.ClientSettings.SignalRLoggingFileLockObject)
                        {
                            linesToLog.Add($"------------------ /{DateTime.Now:dd-MM-yyyy HH-mm-ss} Automatic----------------");
                            System.IO.File.WriteAllLines(ClientSettings.ClientSettings.SignalRLogFilePath, linesToLog);
                        }
                    }
                };
                hubConnection.Reconnected += async (connectionId) =>
                {
                    await SubscribeToTagsIfHubIsAlreadyConnected(tagsSubscribed);


                    List<string> linesToLog = new List<string>()
                    {
                        $"------------------ {DateTime.Now:dd-MM-yyyy HH-mm-ss}  Automatic----------------",
                        $"Controller hub when Connection closed connectionId = {SignalRConnectionID}",
                        $"Subcribed to all older tags again  .",

                    };
                    SignalRClient.SignalRConnectionID = connectionId;
                    linesToLog.Add($"Controller automatic reconnected with connection id : {connectionId}.");
                    linesToLog.Add($"");

                    if (ClientSettings.ClientSettings.IsSignalRLoggingOn)
                    {
                        lock (ClientSettings.ClientSettings.SignalRLoggingFileLockObject)
                        {
                            linesToLog.Add($"------------------ /{DateTime.Now:dd-MM-yyyy HH-mm-ss} Automatic----------------");
                            System.IO.File.WriteAllLines(ClientSettings.ClientSettings.SignalRLogFilePath, linesToLog);
                        }
                    }
                };

                if (!IsConnected && !IsConnecting && IsDisconnected)
                {
                    await hubConnection.StartAsync();
                }
            }
            catch (Exception ex)
            {
                try
                {
                    // retry
                    if (!IsConnected && !IsConnecting && IsDisconnected)
                    {
                        await hubConnection.StartAsync();
                    }
                }
                catch (Exception ex2)
                {
                    try
                    {
                        // retry
                        if (!IsConnected && !IsConnecting && IsDisconnected)
                        {
                            await hubConnection.StartAsync();
                        }
                    }
                    catch (Exception ex3)
                    {
                        //
                    }
                }
            }
            var isconnected = false;

            try
            {
                isconnected = IsConnected && !string.IsNullOrEmpty(hubConnection?.ConnectionId);
            }
            catch (Exception ex)
            {
                isconnected = false;
            }

            SignalRConnectionID = IsConnected && !string.IsNullOrEmpty(hubConnection?.ConnectionId) ? hubConnection.ConnectionId : "";
            await SubscribeToTagsIfHubIsAlreadyConnected(tagsToSubsCribeInHub);
            return isconnected;
        }
        public static async Task<bool> UnSubscribeSelectedTagsIfConnected(List<string> tagstoUnSubCribe)
        {
            try
            {
                if (IsConnected)
                {
                    await hubConnection?.InvokeAsync("UnSubscribeFromTags", tagstoUnSubCribe);

                    if (tagsSubscribed?.Count() > 0)
                    {
                        tagsSubscribed.RemoveAll(tag => tagstoUnSubCribe.Contains(tag));
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                // log
                //throw;
                return false;
            }
        }
        private static async Task<bool> SubscribeToTagsInHub(List<string> tagsToSubsCribe)
        {

            try
            {
                if (IsConnected)
                {
                    await hubConnection?.InvokeAsync("SubscribeToTagsInHub", tagsToSubsCribe);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                // log
                //throw;
                return false;
            }
        }
        public static async Task<bool> SubscribeToTagsIfHubIsAlreadyConnected(List<string> tagsToSubsCribe)
        {
            if (IsConnected && tagsToSubsCribe?.Count > 0)
            {
                try
                {

                    // register handler first then tell hub it will imediately end registered tags to subcribed tag by client
                    //List<string> tagsToSubsCribeInHub = new();
                    List<string> tagsToSubsCribeInHub = tagsToSubsCribe;
                    foreach (var tag in tagsToSubsCribe)
                    {
                        //if (!tagsSubscribed.Contains(tag))
                        //{
                        // already subcribed
                        hubConnection?.On<string, string>(tag, (tag, jsonData) =>
                        {
                            #region handle SendSubscribedTagsByThisConnectionToClient if implemented in future in some other way
                            if (!string.IsNullOrEmpty(tag) && tag == "SendSubscribedTagsByThisConnectionToClient")
                            {
                                // this function needs to implemented in server side or client side to get total tags subcribed by the app
                                //can be stored in local storage also

                                // curretly server sends messages to the cliets who have subcribed the tag for which the server is sending message 
                                List<string> SendSubscribedTagsByThisConnectionToClient = GetDataObject<List<string>>(jsonData) ?? new List<string>();
                                if (tagsSubscribed == null)
                                {
                                    tagsSubscribed = new List<string>() { "SendSubscribedTagsByThisConnectionToClient" };
                                }
                                //tagsSubscribed.AddRange(SendSubscribedTagsByThisConnectionToClient.Where(tagRecevied => !tagsSubscribed.Contains(tagRecevied)).Select(tagRecevied => tagRecevied));// adding the tags to global subcribed tag list if some subcribed tag is not in that global static list
                                tagsSubscribed = SendSubscribedTagsByThisConnectionToClient;
                                #region comment

                                //if (SendSubscribedTagsByThisConnectionToClient != null && SendSubscribedTagsByThisConnectionToClient.Count()>0)
                                //{
                                //    foreach (var subscribedTagByThisConnection in SendSubscribedTagsByThisConnectionToClient)
                                //    {
                                //        if (tagsSubscribed != null && !tagsSubscribed.Contains(subscribedTagByThisConnection))
                                //        {
                                //            //tagsSubscribed 
                                //        }
                                //    }
                                //}
                                #endregion comment
                                #endregion handle SendSubscribedTagsByThisConnectionToClient if implemented in future in some other way
                            }
                            else
                            {
                                SignalRClient.MessageReceived.Invoke(tag, jsonData);
                            }
                        });

                        //tagsToSubsCribeInHub.Add(tag);
                        //}
                        //else
                        //{
                        //    //NOT already subcribed
                        //    //return false;
                        //}

                    }
                    //if (tagsToSubsCribeInHub?.Count > 0)
                    //{
                    await SubscribeToTagsInHub(tagsToSubsCribeInHub);
                    //}
                    return true;
                    //ServicePointManager.DefaultConnectionLimit = int.MaxValue;
                }
                catch (Exception ex)
                {
                    // logger
                    return false;
                }
            }
            return IsConnected;
        }
        //  calls SignalR Hub function named as send message to send signalR message
        public async static Task<string> SendMessage(string hubCompleteurl, string appName, List<string> tagsOnWhichToSend, object nonSerialezedDataToSend, string jwtToken, List<string> specificConnectionIds_ToSend = null)
        {

            if (nonSerialezedDataToSend != null && tagsOnWhichToSend != null && tagsOnWhichToSend.Count() > 0)
            {
                foreach (var tagOnWhichToSend in tagsOnWhichToSend)
                {
                    string serializedStringObjectData = Newtonsoft.Json.JsonConvert.SerializeObject(nonSerialezedDataToSend);

                    if (hubConnection != null && !string.IsNullOrEmpty(hubConnection.ConnectionId))
                    {
                        if (specificConnectionIds_ToSend?.Count > 0)
                        {

                            await hubConnection?.SendAsync("SendMessageToGivenConnectionIdsIfSubscribedGivenTags_ViaSignalRHub", tagOnWhichToSend, serializedStringObjectData, specificConnectionIds_ToSend);
                        }
                        else
                        {
                            await hubConnection?.SendAsync("SendMessageToClientsViaSignalRHub", tagOnWhichToSend, serializedStringObjectData);
                        }
                    }
                    else
                    {
                        if (await SignalRClient.ConnectToHub(signalRHubURL: hubCompleteurl,
                                                             appName: appName,
                                                             jwtToken: jwtToken,
                                                             tagsToSubsCribe: tagsOnWhichToSend))
                        {
                            //need to remove
                            try
                            {
                                if (specificConnectionIds_ToSend?.Count > 0)
                                {

                                    await hubConnection?.SendAsync("SendMessageToGivenConnectionIdsIfSubscribedGivenTags_ViaSignalRHub", tagOnWhichToSend, serializedStringObjectData, specificConnectionIds_ToSend);
                                }
                                else
                                {
                                    await hubConnection?.SendAsync("SendMessageToClientsViaSignalRHub", tagOnWhichToSend, serializedStringObjectData);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (await SignalRClient.ConnectToHub(signalRHubURL: hubCompleteurl,
                                                             appName: appName,
                                                             jwtToken: jwtToken,
                                                             tagsToSubsCribe: tagsOnWhichToSend))
                                {
                                    if (specificConnectionIds_ToSend?.Count > 0)
                                    {

                                        await hubConnection?.SendAsync("SendMessageToGivenConnectionIdsIfSubscribedGivenTags_ViaSignalRHub", tagOnWhichToSend, serializedStringObjectData, specificConnectionIds_ToSend);
                                    }
                                    else
                                    {
                                        await hubConnection?.SendAsync("SendMessageToClientsViaSignalRHub", tagOnWhichToSend, serializedStringObjectData);
                                    }
                                }
                            }
                        }
                    }
                }
                return "message send successfully.  ";
            }
            else
            {
                return "Not connected to hub or no tag to subcribed";
            }
        }
        public async static Task<string> SendMessageToClientsViaSignalRHubToAllClients(string hubCompleteurl, string appName, List<string> tagsOnWhichToSend, object nonSerialezedDataToSend, string jwtToken)
        {

            if (nonSerialezedDataToSend != null && tagsOnWhichToSend != null && tagsOnWhichToSend.Count() > 0)
            {
                foreach (var tagOnWhichToSend in tagsOnWhichToSend)
                {
                    string serializedStringObjectData = Newtonsoft.Json.JsonConvert.SerializeObject(nonSerialezedDataToSend);

                    if (hubConnection != null && !string.IsNullOrEmpty(hubConnection.ConnectionId))
                    {
                        await hubConnection?.SendAsync("SendMessageToClientsViaSignalRHubToAllClients", tagOnWhichToSend, serializedStringObjectData);
                    }
                    else
                    {
                        if (await SignalRClient.ConnectToHub(signalRHubURL: hubCompleteurl,
                                                             appName: appName,
                                                             jwtToken: jwtToken,
                                                             tagsToSubsCribe: tagsOnWhichToSend))
                        {
                            //need to remove
                            try
                            {
                                await hubConnection?.SendAsync("SendMessageToClientsViaSignalRHubToAllClients", tagOnWhichToSend, serializedStringObjectData);
                            }
                            catch (Exception ex)
                            {
                                if (await SignalRClient.ConnectToHub(signalRHubURL: hubCompleteurl,
                                                             appName: appName,
                                                             jwtToken: jwtToken,
                                                             tagsToSubsCribe: tagsOnWhichToSend))
                                {
                                    await hubConnection?.SendAsync("SendMessageToClientsViaSignalRHubToAllClients", tagOnWhichToSend, serializedStringObjectData);
                                }
                            }
                        }
                    }
                }
                return "message send successfully. to clients ";
            }
            else
            {
                return "Not connected to hub or no tag to subcribed";
            }
        }
        public static async Task<bool> Disconnect()
        {
            try
            {
                SignalRConnectionID = "";
                if (hubConnection is not null)
                {
                    await hubConnection.DisposeAsync();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //log 
                return false;
                throw;
            }

        }
        public static expectedType GetDataObject<expectedType>(string json)
        {
            try
            {
                Type expectedTypeType = typeof(expectedType);
                if (json != null)
                {
                    // Deserialize object to JSON
                    var expectedResultobj = JsonConvert.DeserializeObject<expectedType>(json);
                    //expectedType expectedObject = (expectedType)rawObject;
                    return expectedResultobj ?? (expectedType)new object();
                }
                else
                {
                    return (expectedType)new object();
                }
            }
            catch (Exception ex)
            {
                return (expectedType)new object();
            }
        }
        public static string GetEncryptedTag(Enums.SignalRReceiveType receiveType, string etityId, out string nonEncruptedTagOnlyToDebug, string encruptionKey = ClientSettings.ClientSettings.TagEncruptionKey, int timeInterValForRecivingData_inMS = (int)Enums.SignalRDataReciveTimeInterval.Milliseconds200, bool getRadisTagWithStar = false)
        {
            var tag = string.Empty;
            if (receiveType != Enums.SignalRReceiveType.DataType6 &&// redis data types
                receiveType != Enums.SignalRReceiveType.DataType5&&
                receiveType != Enums.SignalRReceiveType.DataType4)
            {

                // created string tag
                tag = $"{timeInterValForRecivingData_inMS}__{receiveType}__{etityId}";
                nonEncruptedTagOnlyToDebug = tag;
                // encripty tag with sha255
                //tag = SignalRClient.EncriptionAndDecritption.EcryptionAndDcryption.EncryptString(tag, encruptionKey);
                tag = EncriptionAndDecritption.EcryptionAndDcryption.EncryptStringWithSHA_OneSidedEncruption(tag);
            }
            else
            {
                if (receiveType == Enums.SignalRReceiveType.DataType5)
                {
                    if (getRadisTagWithStar)
                    {
                        tag = $"*:Redis_Data5Message-{etityId}";
                    }
                    else
                    {
                        tag = $"Redis_Data5Message-{etityId}";
                    }
                }
                else if (receiveType == Enums.SignalRReceiveType.DataType6)
                {
                    if (getRadisTagWithStar)
                    {
                        tag = $"*:Redis_Data6Message2-{etityId}";
                    }
                    else
                    {
                        tag = $"Redis_Data6Message2-{etityId}";
                    }

                }
                else if (receiveType == Enums.SignalRReceiveType.DataType4)
                {
                    //tag = $"*:Redis_Data5Message3-{etityId}";
                    if (getRadisTagWithStar)
                    {
                        tag = $"*:Redis_Data4Message3-{etityId}";
                    }
                    else
                    {
                        tag = $"Redis_Data4Message3-{etityId}";
                    }
                }
                nonEncruptedTagOnlyToDebug = tag;
            }
            return tag?.Trim() ?? "Invalid tag";
        }
        public static string GetEncruptedErrorLogTag()
        {
            // created string tag
            var tag = $"{(int)Enums.SignalRDataReciveTimeInterval.Milliseconds200}__{Enums.SignalRReceiveType.Logs}__{0}";
            //tag = SignalRClient.EncriptionAndDecritption.EcryptionAndDcryption.EncryptString(tag, encruptionKey);
            tag = EncriptionAndDecritption.EcryptionAndDcryption.EncryptStringWithSHA_OneSidedEncruption(tag);
            return tag?.Trim() ?? "Invalid tag";
        }
        public static string GetPingTag()
        {
            return "ping";
        }

        public static string GetDebugTag()
        {
            return "Debug";
        }
        private static async Task<bool> SubscribeToPingMessage(List<string> tagsToSubsCribe)
        {

            try
            {
                if (IsConnected)
                {
                    _ = SubscribeToTagsIfHubIsAlreadyConnected(new() { "ping" });
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                // log
                //throw;
                return false;
            }
        }
        public async static ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }
    }
}