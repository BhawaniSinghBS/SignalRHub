using CoreTCP.CoreUDP;
using CoreTCP.CoreUDP.Helper;
using Microsoft.Extensions.DependencyInjection;
using PromethusClient.Instruments;
using ServiceStack;
using SignalRClient.ClientSettings;
using SignalRClient.DataModals;
using SignalRClient.DataModals.DataModels;
using SignalRClient.Helpers.DerectoryFileHelper;
using SignalRClient.Helpers.HTTPHelper.ApiRequests;
using SignalRClient.SerilizingDeserilizing;
using SignalRServer.Settings;
using SihnalRHub.DAL.Repositories;
using System.Collections.Concurrent;

namespace SignalRServer
{
    public static class HubTimers
    {
        static IServiceProvider _serviceProvider { get; set; }
        public static void InitializeServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        // this class takes request through http client and can run on time interval and can execute tcp request also on tcp client send as wrapped in http request body and also processes udp data received

        // time inter val , request datat
        static ConcurrentDictionary<string, int> NewDataTagsToBeSendToClientsExceptData1 { get; set; } = new();

        static ConcurrentDictionary<string, int> NewDataTagsToBeSendToClientsOnlyData1 { get; set; } = new();

        public static ConcurrentDictionary<int, ConcurrentBag<object>> TimePeriodAndRequestObj { get; private set; } = new();
        public static ConcurrentDictionary<string, SignalrClientDataModal> ProcessedModelBufferExceptData1 { get; private set; } = new();
        public static ConcurrentDictionary<string, SignalrClientDataModal> ProcessedModelBufferOnlyData1 { get; private set; } = new();
        //( tagTosend,datatoSend,connectionIdTosend  ) (TimeIntervalToSendAt,TimerFrom,TimeTill)
        //  if (keyValue.Value.Item3 - keyValue.Value.Item2 > keyValue.Value.Item1)   // this way
        public static ConcurrentDictionary<(string, object, string), (TimeSpan, DateTime, DateTime)> RecordsToBeProcessedForSpecifiedTime { get; private set; } = new(); //tags and conetionids

        //        public static ConcurrentDictionary<int, Data3> Data3sInDB { get; private set; } = new();
        public static ConcurrentDictionary<int, object> DBData { get; private set; } = new();
        public static Timer HalfSecondTimer = new Timer(HandleTimerCallBack, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
        public static byte TimerTriggerCountTillByteMax = 0; // does not go above 254 // if needed declare another counter// it will be increcremented 10 times by one in 1 second if timmer is triggring at 100 ms
        public static int TimerTriggerCountTillIntMax { get; set; } = 0;//like this
        //public static bool isExecuting = false;// for debug
        public static bool IsUDPSubcribedInHubTimer { get; set; } = false;
        private static int elapsedMilliseconds = 0;
        public static DateTime LastTimeBuffersCleared { get; set; } = DateTime.UtcNow;
        public static DateTime LastDateTimeLogFileCreated { get; set; } = DateTime.UtcNow;
        public static DateTime LastTimeDatSendToClients { get; set; } = DateTime.UtcNow;
        public static DateTime LastTimeRedisDictionaryCleared = DateTime.UtcNow;
        #region promethus
        //public static DateTime LastTimeTotalNotAuthenticatedSessionsCountIsRest = DateTime.UtcNow;
        //public static DateTime LastTimeSendMessagesCountIsReset = DateTime.UtcNow;
        #endregion promethus

        public static async void HandleTimerCallBack(object data)
        {
            try
            {

                // Increment the elapsed milliseconds
                if (elapsedMilliseconds >= 100)
                {
                    elapsedMilliseconds = 0; // Reset the elapsed milliseconds counter
                }
                else
                {
                    elapsedMilliseconds += 100; // timer does trigger before 100 ms so halding the trigger for 100 ms
                    return;
                }
                DateTime currentDateTime = DateTime.Now;
                if (!IsUDPSubcribedInHubTimer)
                {
                    IsUDPSubcribedInHubTimer = true;
                    UDPClient.MessageReceivedFromUDP -= HandleUDPMessageReceived; // this is subscribed at program.cs so removving earlier subscription
                    UDPClient.MessageReceivedFromUDP += HandleUDPMessageReceived;

                    //************************ start udp client ************************
                    // starting udp to start listning to data throwen from distributer app on the ip of indian server currently , if want tho get data on other ip where this app is running , make entry in distrbuter app table with the ip adress of the server where this app is running
                    _ = UDPClient.ReceiveDataFromUDPClient(expectedIPWhereDataWillBeReceived: "0.0.0.0",// allos any ip to send to this udp receiver
                                                                expectedPortWhereDataWillBeReceived: CoreTCP.CoreUDP.Settings.TCPUDPSettings.UDPPort,
                                                                receiveContinueslyOnSubcribedEvent: true
                                                              );
                    //************************ /start udp client ************************
                }



                // because we need to put value till 254 (byte)in tcp message max
                HubTimers.TimerTriggerCountTillByteMax = HubTimers.TimerTriggerCountTillByteMax < 0 || HubTimers.TimerTriggerCountTillByteMax == byte.MaxValue ? (byte)1 : HubTimers.TimerTriggerCountTillByteMax++;

                HubTimers.TimerTriggerCountTillIntMax = HubTimers.TimerTriggerCountTillIntMax == int.MaxValue ? 1 : HubTimers.TimerTriggerCountTillIntMax++;

                bool is100msElapsed = TimerTriggerCountTillIntMax % 1 == 0;
                bool is200msElapsed = TimerTriggerCountTillIntMax % 2 == 0;
                bool isHalfSecondElapsed = TimerTriggerCountTillIntMax % 5 == 0 && (currentDateTime.Millisecond <= 201 || (currentDateTime.Millisecond >= 401 && currentDateTime.Millisecond <= 601));
                bool isOneSecondElapsed = currentDateTime.Second % 1 == 0 && currentDateTime.Millisecond <= 201;
                bool is2SecondsElapsed = currentDateTime.Second % 2 == 0 && currentDateTime.Millisecond <= 201;
                bool is3SecondsElapsed = currentDateTime.Second % 3 == 0 && currentDateTime.Millisecond <= 201;
                bool isfiveSecondElapsed = currentDateTime.Second % 5 == 0 && currentDateTime.Millisecond <= 201;
                bool is10SecondElapsed = currentDateTime.Second % 10 == 0 && currentDateTime.Millisecond <= 201;
                bool is59minutesElapsed = currentDateTime.Minute % 59 == 0 && currentDateTime.Second == 0 && currentDateTime.Millisecond <= 201;
                bool is10minutesElapsed = currentDateTime.Minute % 10 == 0 && currentDateTime.Second == 0 && currentDateTime.Millisecond <= 201;

                //if (is100msElapsed)
                //{
                //}

                //if (isHalfSecondElapsed && ServerSettings.IsSendCashedDataAtHalfSecond)// sending all Datas at half second
                //{
                //    Task.Run(() => SendCachedDataToClients(!ServerSettings.IsSendRealTimeData1Data, isHalfSecondElapsed, isSendOnlyNewData: true));
                //}

                if (is200msElapsed)
                {
                    // argumented    and    Data1
                    if (ServerSettings.IsSendCachedDataAt200MilliSecond)
                    {
                        _ = Task.Run(() => SendCachedDataToClients(!ServerSettings.IsSendOnlyRealTime, is200msElapsed, isSendOnlyNewData: true));
                    }

                    if (TimePeriodAndRequestObj?.TryGetValue((int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200, out ConcurrentBag<object> requetsToBeExecutedIn1Second) ?? false)// requests to be executed in 1 second
                    {// for 1 second calls
                        if (requetsToBeExecutedIn1Second?.Count() > 0)
                        {
                            // there are some requests that needs to be executed in 1 second
                            Parallel.ForEach(requetsToBeExecutedIn1Second, oneSecondRequest =>
                            {
                                Type typeOfDataObject = oneSecondRequest?.GetType() ?? typeof(object);
                                if (oneSecondRequest != null && typeOfDataObject == typeof(TCPRequestDTO))// for Datas reuest
                                {

                                    Task.Run(() => CallTcpAndSendRecivedBytesToSubscribers(oneSecondRequest));
                                }
                                // handle any other type of request here
                            });
                        }
                    }
                    if (ClientSettings.IsSignalRLoggingOn && (DateTime.UtcNow - LastDateTimeLogFileCreated).TotalMinutes >= 3)
                    {
                        ClientSettings.SignalRLogFilePath =
                       Path.Combine(ClientSettings.LogDirectoryPath, $"{DateTime.UtcNow:dd-MM-yyyy} {HubTimers.TimerTriggerCountTillIntMax}.txt");
                        DirectoryFileHelper.CreateFileAtGivenPathIfNotPresent(ClientSettings.SignalRLogFilePath);
                        LastDateTimeLogFileCreated = DateTime.UtcNow;
                    }
                }

                if (RecordsToBeProcessedForSpecifiedTime.Count > 0)
                {

                    // (tagTosend,datatoSend,connectionIdTosend  ) (TimeIntervalToSendAt,TimerFrom,TimeTill)
                    // there are some requests that needs to be executed in 1 second
                    _ = Task.Run(() =>
                     {
                         Parallel.ForEach(RecordsToBeProcessedForSpecifiedTime, keyValue =>
                     {
                         if (is200msElapsed && keyValue.Value.Item1.TotalMilliseconds == 200)
                         {
                             if (keyValue.Value.Item3 - keyValue.Value.Item2 > keyValue.Value.Item1)
                             {
                                 // sed till item 3 i.e. specified time
                                 SendProcessedMessgesToSignalRClientsWithRetry(keyValue.Key.Item2, keyValue.Key.Item1, specificConnectionIds_ToSend: new() { keyValue.Key.Item3 });

                                 var updaeteValue = (keyValue.Value.Item1, DateTime.UtcNow, keyValue.Value.Item3);
                                 RecordsToBeProcessedForSpecifiedTime[keyValue.Key] = updaeteValue;
                             }
                             else
                             {
                                 RecordsToBeProcessedForSpecifiedTime.TryRemove(keyValue.Key, out _);
                             }
                         }
                         else if (isfiveSecondElapsed && keyValue.Value.Item1.TotalSeconds == 5)
                         {
                             if (keyValue.Value.Item3 - keyValue.Value.Item2 > keyValue.Value.Item1)
                             {
                                 // sed till item 3 i.e. specified time
                                 SendProcessedMessgesToSignalRClientsWithRetry(keyValue.Key.Item2, keyValue.Key.Item1, specificConnectionIds_ToSend: new() { keyValue.Key.Item3 });

                                 var updaeteValue = (keyValue.Value.Item1, DateTime.UtcNow, keyValue.Value.Item3);
                                 RecordsToBeProcessedForSpecifiedTime[keyValue.Key] = updaeteValue;
                             }
                             else
                             {
                                 RecordsToBeProcessedForSpecifiedTime.TryRemove(keyValue.Key, out _);
                             }
                         }
                         else if (is3SecondsElapsed && keyValue.Value.Item1.TotalSeconds == 3)
                         {
                             if (keyValue.Value.Item3 - keyValue.Value.Item2 > keyValue.Value.Item1)
                             {
                                 // sed till item 3 i.e. specified time
                                 SendProcessedMessgesToSignalRClientsWithRetry(keyValue.Key.Item2, keyValue.Key.Item1, specificConnectionIds_ToSend: new() { keyValue.Key.Item3 });

                                 var updaeteValue = (keyValue.Value.Item1, DateTime.UtcNow, keyValue.Value.Item3);
                                 RecordsToBeProcessedForSpecifiedTime[keyValue.Key] = updaeteValue;
                             }
                             else
                             {
                                 RecordsToBeProcessedForSpecifiedTime.TryRemove(keyValue.Key, out _);
                             }
                         }
                         else if (is2SecondsElapsed && keyValue.Value.Item1.TotalSeconds == 2)
                         {
                             if (keyValue.Value.Item3 - keyValue.Value.Item2 > keyValue.Value.Item1)
                             {
                                 // sed till item 3 i.e. specified time
                                 SendProcessedMessgesToSignalRClientsWithRetry(keyValue.Key.Item2, keyValue.Key.Item1, specificConnectionIds_ToSend: new() { keyValue.Key.Item3 });

                                 var updaeteValue = (keyValue.Value.Item1, DateTime.UtcNow, keyValue.Value.Item3);
                                 RecordsToBeProcessedForSpecifiedTime[keyValue.Key] = updaeteValue;
                             }
                             else
                             {
                                 RecordsToBeProcessedForSpecifiedTime.TryRemove(keyValue.Key, out _);
                             }
                         }
                         else if (isOneSecondElapsed && keyValue.Value.Item1.TotalSeconds == 1)
                         {
                             if (keyValue.Value.Item3 - keyValue.Value.Item2 > keyValue.Value.Item1)
                             {
                                 // sed till item 3 i.e. specified time
                                 SendProcessedMessgesToSignalRClientsWithRetry(keyValue.Key.Item2, keyValue.Key.Item1, specificConnectionIds_ToSend: new() { keyValue.Key.Item3 });

                                 var updaeteValue = (keyValue.Value.Item1, DateTime.UtcNow, keyValue.Value.Item3);
                                 RecordsToBeProcessedForSpecifiedTime[keyValue.Key] = updaeteValue;
                             }
                             else
                             {
                                 RecordsToBeProcessedForSpecifiedTime.TryRemove(keyValue.Key, out _);
                             }
                         }

                     });
                     });
                }

                if (is10SecondElapsed)
                {
                    // ping messages to client to matain the connection

                    SendMessageToClientsViaSignalRHubToAllClients(DateTime.UtcNow, SignalRClient.SignalRClient.GetPingTag());
                    if (DBData?.Count() < 1)
                    {
                        _ = FillFromDB();
                    }
                }
                if (is10minutesElapsed)
                {
                    // other than Data1 can be cleared earlier
                    if ((DateTime.UtcNow - LastTimeBuffersCleared).TotalMinutes >= 10)
                    {
                        ProcessedModelBufferOnlyData1.Clear();
                        ProcessedModelBufferExceptData1.Clear();
                        LastTimeBuffersCleared = DateTime.UtcNow;
                    }
                }

                if (is59minutesElapsed)// whole fucntion is running in .5 sec
                {
                    List<int> timePeriodsRequested = TimePeriodAndRequestObj?.Keys?.ToList() ?? new();
                    Parallel.ForEach(timePeriodsRequested, timePeriod =>
                    {
                        Task.Run(() => RemoveAllUnsubcribedRequestForThisTimePeriod(timePeriod));
                    });
                    _ = Task.Run(() =>
                    {
                        #region no tags register to get data from redis close redis connection
                        bool IsCloseRadis = true;
                        foreach (var allTags in SignalRHub.TagSubscriptions?.Keys)
                        {
                            if (allTags.Contains("Data5Message1") || allTags.Contains("Data5Message2") || allTags.Contains("Data5Message3"))
                            {
                                IsCloseRadis = false; break;
                            }
                        }
                        if (IsCloseRadis)
                        {
                            SignalRServer.Redis.Redis.CloseRedisConnection();
                        }
                        #endregion no tags register to get data from redis close redis connection
                    });



                    // clear redis send status time tracker dictionary
                    if ((DateTime.UtcNow - LastTimeRedisDictionaryCleared).Hours > ServerSettings.ClearRedisIdTrackerInHours)
                    {
                        Redis.Redis.RedisIdAndTimeStamp.Clear();
                        LastTimeRedisDictionaryCleared = DateTime.UtcNow;
                    }
                    if (LastTimeBuffersCleared.Date < DateTime.UtcNow.Date)
                    {
                        _ = FillFromDB();
                    }
                }


                //HalfSecondTimer.Change(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100)); // not required
                //Thread.Sleep(20); // not required
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();

                string excetpion = $"Class name : {nameof(HubTimers)}  -- Function Name : {nameof(HandleTimerCallBack)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                               appName: ServerSettings.ThisAppName,
                                               tagsOnWhichToSend: new List<string>() { tag },
                                               nonSerialezedDataToSend: excetpion,
                                               jwtToken: ServerSettings.ThisAppToken);
            }
        }
        public static void HandleUDPMessageReceived((int, byte[]) udpDatas)
        {
            if (udpDatas.Item1 > 3 && udpDatas.Item2.Length == 9)// udp port exception has occured so start reading udp port again
            {
                LastTimeDatSendToClients = LastTimeDatSendToClients.AddMinutes(-60);// it will trigger the loginc to start udp reading again below 
                return;
            }
            _ = BufferReceivedFramesFromNetworkClientOrSendRealTime(udpDatas);
        }

        public async static void CallTcpAndSendRecivedBytesToSubscribers(object requestData)
        {
            var request = (TCPRequestDTO)requestData;
            // do not wait send only
            try
            {
                if (request.TCPRequest[2] == 0)// message count can not be zero
                {
                    request.TCPRequest[2] = 1;
                }

                if (request.TCPRequest == null || (request.TCPRequest[1] != 0 && request.TCPRequest[2] != 0))
                {
                    using (var client = new CoreTCP.CoreUDP.TCPClient(request.TCPServerPort, request.TCPServerIP))
                    {
                        request.TCPRequest[2] = Decimal.Divide(HubTimers.TimerTriggerCountTillByteMax, 2) >= 254 ? (byte)1 : (byte)Decimal.Divide(HubTimers.TimerTriggerCountTillByteMax, 2);

                        // (tcpData Length , tcpData)


                        (int, byte[]) safetyRawDataFromTCP = await client.SendRequestToClientAndGetRawData(request.TCPRequest); // the data received here is on this call where as using udp client then it geves data on subcrined event of message received we do not nedd this call or request to get data
                        // as soon as the udp recevies data it triggers event HandleUDPMessageReceived 
                        await BufferReceivedFramesFromNetworkClientOrSendRealTime(safetyRawDataFromTCP, request);
                    }
                }
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();

                string excetpion = $"Class name : {nameof(HubTimers)}  -- Function Name : {nameof(CallTcpAndSendRecivedBytesToSubscribers)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                              appName: ServerSettings.ThisAppName,
                                              tagsOnWhichToSend: new List<string>() { tag },
                                              nonSerialezedDataToSend: excetpion,
                                              jwtToken: ServerSettings.ThisAppToken);
            }

        }
        public static async Task BufferReceivedFramesFromNetworkClientOrSendRealTime((int, byte[]) safetyRawDataFromNetworkClient, TCPRequestDTO request = null)
        {
            try
            {
                if (safetyRawDataFromNetworkClient.Item2 != null && safetyRawDataFromNetworkClient.Item2.Length > 2)// 2 byte.MaxValue,byte.MaxValue start stop only = 2
                {
                    List<TCP_UDPFrameModal> messageFramesFromDeviceWithMetaData = TCP_UDPHelper.CollectFramesFrames(safetyRawDataFromNetworkClient.Item2);
                    if (messageFramesFromDeviceWithMetaData?.Count > 0)
                    {
                        ClientGaugeMetrics.UpdateUDPMessageCount(messageFramesFromDeviceWithMetaData.Count());
                        _ = Parallel.ForEachAsync(messageFramesFromDeviceWithMetaData, async (messageFrame, CancellationToken) =>
                         {
                             if (messageFrame?.IsCompleteFrame ?? false)
                             {
                                 /****************************for  buffer prcessed model starts *******************/
                                 if (messageFrame.MessageTypeReceivedId == (int)CoreTCP.CoreUDP.Enums.FrameMessageType.DataType1)
                                 {
                                     await ProcessAndSendOrBufferData1ToClients(messageFrame);
                                 }
                                 else
                                 {
                                     await ProcessAndSendNonData1ToClients(messageFrame);
                                 }
                             }
                         });
                    }
                }
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();

                string excetpion = $"Class name : {nameof(HubTimers)}  -- Function Name : {nameof(BufferReceivedFramesFromNetworkClientOrSendRealTime)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                              appName: ServerSettings.ThisAppName,
                                              tagsOnWhichToSend: new List<string>() { tag },
                                              nonSerialezedDataToSend: excetpion,
                                              jwtToken: ServerSettings.ThisAppToken);
            }
        }

        private static async Task SendCachedDataToClients(bool isSendData1Datas, bool isHalfSecondTimerElapsed, bool isSendOnlyNewData = true)
        {
            try
            {
                if (isSendOnlyNewData)
                {
                    SendDataReceiveToClients(isSendData1Datas, isHalfSecondTimerElapsed);
                }
                else
                {

                    _ = Parallel.ForEachAsync(ProcessedModelBufferExceptData1, async (tagAnddeviceProcessedModel, cancellationToken) =>
                      {
                          SignalrClientDataModal ClientDataModal = tagAnddeviceProcessedModel.Value;
                          if (ClientDataModal != null)
                          {
                              _ = SendProcessedDataMessagesToClients(ClientDataModal, isSendData1Datas, isHalfSecondTimerElapsed);
                          }
                      });


                    _ = Parallel.ForEachAsync(ProcessedModelBufferOnlyData1, async (tagAnddeviceProcessedModel, cancellationToken) =>
                          {
                              SignalrClientDataModal ClientDataModal = tagAnddeviceProcessedModel.Value;
                              if (ClientDataModal != null)
                              {
                                  _ = SendProcessedDataMessagesToClients(ClientDataModal, isSendData1Datas, isHalfSecondTimerElapsed);
                              }
                          });
                }
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();

                string excetpion = $"Class name : {nameof(HubTimers)}  -- Function Name : {nameof(SendCachedDataToClients)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                               appName: ServerSettings.ThisAppName,
                                               tagsOnWhichToSend: new List<string>() { tag },
                                               nonSerialezedDataToSend: excetpion,
                                               jwtToken: ServerSettings.ThisAppToken);
            }
        }
        static void SendDataReceiveToClients(bool isSendData1Datas, bool isHalfSecondTimerElapsed)
        {
            Task.Run(() =>//for non Data1
            {
                // Dequeue all items from the queue
                Parallel.ForEachAsync(NewDataTagsToBeSendToClientsExceptData1, async (tagAnddeviceProcessedModel, cancellationToken) =>
                {
                    string tagOfDataToForward = tagAnddeviceProcessedModel.Key;
                    if (!string.IsNullOrEmpty(tagOfDataToForward))
                    {

                        if (ProcessedModelBufferExceptData1?.TryGetValue(tagOfDataToForward, out SignalrClientDataModal tagAnddeviceProcessedModelNonData1) ?? false)
                        {
                            if (tagAnddeviceProcessedModelNonData1 != null)
                            {
                                SendProcessedDataMessagesToClients(tagAnddeviceProcessedModelNonData1, isSendData1Datas, isHalfSecondTimerElapsed);
                            }
                        }
                    }
                }).ContinueWith(_ =>
                {
                    if (NewDataTagsToBeSendToClientsExceptData1.Count > 0)
                    {
                        LastTimeDatSendToClients = DateTime.UtcNow;
                    }
                    // Clear the collection after the completion of the loop
                    NewDataTagsToBeSendToClientsExceptData1.Clear();

                });
            });
            Task.Run(() =>//for non Data1
            {
                Parallel.ForEachAsync(NewDataTagsToBeSendToClientsOnlyData1, async (tagAnddeviceProcessedModel, cancellationToken) =>
                {
                    string tagOfDataToForward = tagAnddeviceProcessedModel.Key;
                    if (!string.IsNullOrEmpty(tagOfDataToForward))
                    {

                        if (ProcessedModelBufferOnlyData1?.TryGetValue(tagOfDataToForward, out SignalrClientDataModal tagAnddeviceProcessedModelData1) ?? false)
                        {
                            if (tagAnddeviceProcessedModelData1 != null)
                            {
                                SendProcessedDataMessagesToClients(tagAnddeviceProcessedModelData1, isSendData1Datas, isHalfSecondTimerElapsed);
                            }
                        }
                    }
                }).ContinueWith(_ =>
                {
                    NewDataTagsToBeSendToClientsOnlyData1.Clear();
                });

            });
        }
        private static async Task ProcessAndSendOrBufferData1ToClients(TCP_UDPFrameModal receivedDataFrameFromUDP, string ulpdatedDataBase64 = null)
        {
            // convert raw bytes from udp to Data 16 model
            // check
            try
            {
                if ((receivedDataFrameFromUDP?.FrameBytesWithStartStopBits == null || receivedDataFrameFromUDP.FrameBytesWithStartStopBits?.Length < 10) && receivedDataFrameFromUDP.MessageTypeReceivedId < 0)
                {
                    return;
                }

                SignalrClientDataModal processedModal = new();

                //string expectedTagToBroadCastDataAsData = "";
                string expectedTagToBroadCastDataAsModal = "";
                string tagOnWhichSendAllModelsOfThisType = "";

                if (receivedDataFrameFromUDP.MessageTypeReceivedId == (int)CoreTCP.CoreUDP.Enums.FrameMessageType.DataType2 || receivedDataFrameFromUDP.MessageTypeReceivedId == (int)CoreTCP.CoreUDP.Enums.FrameMessageType.DataType1)
                {
                    // device id tag
                    string tagToBroadCastData2AsModal = SignalRClient.SignalRClient.GetEncryptedTag(
                                      receiveType: SignalRClient.Enums.SignalRReceiveType.DataType2,
                                      etityId: receivedDataFrameFromUDP.Id.ToString(),
                                      out string nonEncruptedTagOnlyToDebugForModalS,
                                      encruptionKey: ServerSettings.TagEncruptionKey,
                                      timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                                      ).Trim();
                    string tagToBroadCastData1AsModal = SignalRClient.SignalRClient.GetEncryptedTag(
                                   receiveType: SignalRClient.Enums.SignalRReceiveType.DataType1,
                                   etityId: receivedDataFrameFromUDP.Id.ToString(),
                                   out string nonEncruptedTagOnlyToDebugForModalL,
                                   encruptionKey: ServerSettings.TagEncruptionKey,
                                   timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                                   ).Trim();
                    // either data1 or either data2 one will be sent at a time so keep one out of both in buffer 
                    if (receivedDataFrameFromUDP.MessageTypeReceivedId == (int)CoreTCP.CoreUDP.Enums.FrameMessageType.DataType2)
                    {
                        DataModal dataModel2 = new DataModal(receivedDataFrameFromUDP.FrameBytesWithStartStopBits);
                        SignalrClientDataModal dataToSend = new SignalrClientDataModal();
                        dataToSend.EntityId = dataModel2.Id;
                        dataToSend.ModalTypeName = dataModel2.GetType().Name;
                        dataToSend.DataModal = dataModel2;

                        processedModal = dataToSend;

                        //if data type 1 is comming for this device clear long Data
                        ProcessedModelBufferExceptData1.TryRemove(tagToBroadCastData1AsModal, out SignalrClientDataModal value);

                        expectedTagToBroadCastDataAsModal = tagToBroadCastData2AsModal;
                        // 0 tag
                        string tagOnWhichSendAllDataType2 = SignalRClient.SignalRClient.GetEncryptedTag(
                                  receiveType: SignalRClient.Enums.SignalRReceiveType.DataType2,
                                  etityId: 0.ToString(),
                                  out string nonEncruptedTagOnlyToDebugForData1,
                                  encruptionKey: ServerSettings.TagEncruptionKey,
                                  timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                                  ).Trim();
                        tagOnWhichSendAllModelsOfThisType = tagOnWhichSendAllDataType2;

                    }
                    else if (receivedDataFrameFromUDP.MessageTypeReceivedId == (int)CoreTCP.CoreUDP.Enums.FrameMessageType.DataType1)
                    {
                        DataModal dataModel1 = new DataModal(receivedDataFrameFromUDP.FrameBytesWithStartStopBits);
                        SignalrClientDataModal dataToSend = new SignalrClientDataModal();
                        dataToSend.EntityId = dataModel1.Id;
                        dataToSend.ModalTypeName = dataModel1.GetType().Name;
                        dataToSend.DataModal = dataModel1;

                        processedModal = dataToSend;
                        ProcessedModelBufferExceptData1.TryRemove(tagToBroadCastData1AsModal, out SignalrClientDataModal value);

                        expectedTagToBroadCastDataAsModal = tagToBroadCastData1AsModal;

                        string tagOnWhichSendAllDataType1 = SignalRClient.SignalRClient.GetEncryptedTag(
                            receiveType: SignalRClient.Enums.SignalRReceiveType.DataType1,
                            etityId: 0.ToString(),
                            out string nonEncruptedTagOnlyToDebugForData2,
                            encruptionKey: ServerSettings.TagEncruptionKey,
                            timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                            ).Trim();

                        tagOnWhichSendAllModelsOfThisType = tagOnWhichSendAllDataType1;
                    }

                    if (ServerSettings.IsSendOnlyRealTime)
                    {
                        _ = SendProcessedMessgesToSignalRClientsWithRetry(processedModal, expectedTagToBroadCastDataAsModal, tagOnWhichSendAllModelsOfThisType);
                        // add of realtime from appsettings
                        LastTimeDatSendToClients = DateTime.UtcNow;
                        return;
                    }
                    else
                    {
                        AddTagToQueOfFreshDataToForWardDataToClient(expectedTagToBroadCastDataAsModal, processedModal.EntityId, isData1: false);
                    }
                }

                // only sendin Data1 Data from here others will be send on timeer from buffer ealier all were send from here
                //SendProcessedMessgesToSignalRClientsWithRetry(DataModalFromBytes, expectedTagToBroadCastDataAsModal, tagOnWhichSendAllModelsOfThisType);

                /***********adding processed buffer starts *******************/
                ProcessedModelBufferExceptData1.AddOrUpdate(
                                     expectedTagToBroadCastDataAsModal,
                                     processedModal,
                                     (key, existingModel) =>
                                     {
                                         // Update the existing Processed modal if needed
                                         existingModel = processedModal;
                                         return existingModel;
                                     });
                /***********adding processed buffer ends*******************/
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();
                string excetpion = $"Class name : {nameof(HubTimers)}  -- Function Name : {nameof(ProcessAndSendOrBufferData1ToClients)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                               appName: ServerSettings.ThisAppName,
                                               tagsOnWhichToSend: new List<string>() { tag },
                                               nonSerialezedDataToSend: excetpion,
                                               jwtToken: ServerSettings.ThisAppToken);
            }
        }

        public static async Task SendProcessedMessgesToSignalRClientsWithRetry(object DataModalFromBytes, string expectedTagToBroadCastDataAsModal, string tagOnWhichSendAllModelsOfThisType = "", List<string> specificConnectionIds_ToSend = null)
        {

            try
            {
                List<string> tagToSendData = new();
                if (ServerSettings.IsSendAllDataOnZeroTagAsWell && !string.IsNullOrEmpty(tagOnWhichSendAllModelsOfThisType))
                {
                    if (!string.IsNullOrEmpty(expectedTagToBroadCastDataAsModal))
                    {
                        tagToSendData = new() { expectedTagToBroadCastDataAsModal, tagOnWhichSendAllModelsOfThisType };
                    }
                    else
                    {
                        tagToSendData = new() { tagOnWhichSendAllModelsOfThisType };
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(expectedTagToBroadCastDataAsModal))
                    {
                        tagToSendData = new() { expectedTagToBroadCastDataAsModal };
                    }
                }

                if (DataModalFromBytes != null && tagToSendData.Count > 0)
                {

                    if (SignalRClient.SignalRClient.IsConnected)
                    {
                        // sending modal on modal request type tag
                        await SignalRClient.SignalRClient.SendMessage(
                                                  hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                  appName: ServerSettings.ThisAppName,
                                                  tagsOnWhichToSend: tagToSendData,
                                                  nonSerialezedDataToSend: DataModalFromBytes,
                                                  jwtToken: ServerSettings.ThisAppToken,
                                                  specificConnectionIds_ToSend
                                                  );
                    }
                    else
                    {
                        if (await SignalRClient.SignalRClient.ConnectToHub(signalRHubURL: ClientSettings.SignalRHubUrl,
                            appName: ServerSettings.ThisAppName,
                            jwtToken: ServerSettings.ThisAppToken))
                        {
                            if (SignalRClient.SignalRClient.IsConnected)
                            {
                                // sending modal on modal request type tag
                                await SignalRClient.SignalRClient.SendMessage(
                                                          hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                          appName: ServerSettings.ThisAppName,
                                                          tagsOnWhichToSend: tagToSendData,
                                                          nonSerialezedDataToSend: DataModalFromBytes,
                                                          jwtToken: ServerSettings.ThisAppToken,
                                                          specificConnectionIds_ToSend
                                                          );
                            }
                            else
                            {
                                //retry
                                if (await SignalRClient.SignalRClient.ConnectToHub(signalRHubURL: ClientSettings.SignalRHubUrl,
                                    appName: ServerSettings.ThisAppName,
                                    jwtToken: ServerSettings.ThisAppToken))
                                {
                                    if (SignalRClient.SignalRClient.IsConnected)
                                    {
                                        // sending modal on modal request type tag
                                        await SignalRClient.SignalRClient.SendMessage(
                                                                  hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                                  appName: ServerSettings.ThisAppName,
                                                                  tagsOnWhichToSend: tagToSendData,
                                                                  nonSerialezedDataToSend: DataModalFromBytes,
                                                                  jwtToken: ServerSettings.ThisAppToken,
                                                                  specificConnectionIds_ToSend
                                                                  );
                                    }
                                }
                            }
                        }
                        else
                        {
                            //retry
                            if (await SignalRClient.SignalRClient.ConnectToHub(signalRHubURL: ClientSettings.SignalRHubUrl,
                            appName: ServerSettings.ThisAppName,
                            jwtToken: ServerSettings.ThisAppToken))
                            {
                                await SignalRClient.SignalRClient.SendMessage(
                                                      hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                      appName: ServerSettings.ThisAppName,
                                                      tagsOnWhichToSend: tagToSendData,
                                                      nonSerialezedDataToSend: DataModalFromBytes,
                                                      jwtToken: ServerSettings.ThisAppToken,
                                                      specificConnectionIds_ToSend
                                                      );
                            }
                            else
                            {
                                if (await SignalRClient.SignalRClient.ConnectToHub(signalRHubURL: ClientSettings.SignalRHubUrl,
                                    appName: ServerSettings.ThisAppName,
                                    jwtToken: ServerSettings.ThisAppToken))
                                {
                                    if (SignalRClient.SignalRClient.IsConnected)
                                    {
                                        // sending modal on modal request type tag
                                        await SignalRClient.SignalRClient.SendMessage(
                                                                  hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                                  appName: ServerSettings.ThisAppName,
                                                                  tagsOnWhichToSend: tagToSendData,
                                                                  nonSerialezedDataToSend: DataModalFromBytes,
                                                                  jwtToken: ServerSettings.ThisAppToken,
                                                                  specificConnectionIds_ToSend
                                                                  );
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();

                string excetpion = $"Class name : {nameof(HubTimers)}  -- Function Name : {nameof(SendProcessedMessgesToSignalRClientsWithRetry)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                              appName: ServerSettings.ThisAppName,
                                                              tagsOnWhichToSend: new List<string>() { tag },
                                                              nonSerialezedDataToSend: excetpion,
                                                              jwtToken: ServerSettings.ThisAppToken);
            }
        }

        public static async Task SendMessageToClientsViaSignalRHubToAllClients(object DataModalFromBytes, string expectedTagToBroadCastDataAsModal)
        {

            try
            {

                List<string> tagToSendData = new() { expectedTagToBroadCastDataAsModal };
                if (DataModalFromBytes != null && tagToSendData.Count > 0)
                {

                    if (SignalRClient.SignalRClient.IsConnected)
                    {
                        // sending modal on modal request type tag
                        await SignalRClient.SignalRClient.SendMessageToClientsViaSignalRHubToAllClients(
                                                  hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                  appName: ServerSettings.ThisAppName,
                                                  tagsOnWhichToSend: tagToSendData,
                                                  nonSerialezedDataToSend: DataModalFromBytes,
                                                  jwtToken: ServerSettings.ThisAppToken
                                                  );
                    }
                    else
                    {
                        if (await SignalRClient.SignalRClient.ConnectToHub(signalRHubURL: ClientSettings.SignalRHubUrl,
                            appName: ServerSettings.ThisAppName,
                            jwtToken: ServerSettings.ThisAppToken))
                        {
                            if (SignalRClient.SignalRClient.IsConnected)
                            {
                                // sending modal on modal request type tag
                                await SignalRClient.SignalRClient.SendMessageToClientsViaSignalRHubToAllClients(
                                                  hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                  appName: ServerSettings.ThisAppName,
                                                  tagsOnWhichToSend: tagToSendData,
                                                  nonSerialezedDataToSend: DataModalFromBytes,
                                                  jwtToken: ServerSettings.ThisAppToken
                                                  );
                            }
                            else
                            {
                                //retry
                                if (await SignalRClient.SignalRClient.ConnectToHub(signalRHubURL: ClientSettings.SignalRHubUrl,
                                    appName: ServerSettings.ThisAppName,
                                    jwtToken: ServerSettings.ThisAppToken))
                                {
                                    if (SignalRClient.SignalRClient.IsConnected)
                                    {
                                        // sending modal on modal request type tag
                                        await SignalRClient.SignalRClient.SendMessageToClientsViaSignalRHubToAllClients(
                                                  hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                  appName: ServerSettings.ThisAppName,
                                                  tagsOnWhichToSend: tagToSendData,
                                                  nonSerialezedDataToSend: DataModalFromBytes,
                                                  jwtToken: ServerSettings.ThisAppToken
                                                  );
                                    }
                                }
                            }
                        }
                        else
                        {
                            //retry
                            if (await SignalRClient.SignalRClient.ConnectToHub(signalRHubURL: ClientSettings.SignalRHubUrl,
                            appName: ServerSettings.ThisAppName,
                            jwtToken: ServerSettings.ThisAppToken))
                            {
                                await SignalRClient.SignalRClient.SendMessageToClientsViaSignalRHubToAllClients(
                                                  hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                  appName: ServerSettings.ThisAppName,
                                                  tagsOnWhichToSend: tagToSendData,
                                                  nonSerialezedDataToSend: DataModalFromBytes,
                                                  jwtToken: ServerSettings.ThisAppToken
                                                  );
                            }
                            else
                            {
                                if (await SignalRClient.SignalRClient.ConnectToHub(signalRHubURL: ClientSettings.SignalRHubUrl,
                                    appName: ServerSettings.ThisAppName,
                                    jwtToken: ServerSettings.ThisAppToken))
                                {
                                    if (SignalRClient.SignalRClient.IsConnected)
                                    {
                                        // sending modal on modal request type tag
                                        await SignalRClient.SignalRClient.SendMessageToClientsViaSignalRHubToAllClients(
                                                  hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                  appName: ServerSettings.ThisAppName,
                                                  tagsOnWhichToSend: tagToSendData,
                                                  nonSerialezedDataToSend: DataModalFromBytes,
                                                  jwtToken: ServerSettings.ThisAppToken
                                                  );
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();

                string excetpion = $"Class name : {nameof(HubTimers)}  -- Function Name : {nameof(SendMessageToClientsViaSignalRHubToAllClients)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);
                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                               appName: ServerSettings.ThisAppName,
                                               tagsOnWhichToSend: new List<string>() { tag },
                                               nonSerialezedDataToSend: excetpion,
                                               jwtToken: ServerSettings.ThisAppToken);
            }
        }

        private static async Task ProcessAndSendNonData1ToClients(TCP_UDPFrameModal receivedDataFrameFromUDP)
        {
            try
            {
                if ((receivedDataFrameFromUDP?.FrameBytesWithStartStopBits == null || receivedDataFrameFromUDP.FrameBytesWithStartStopBits?.Length < 10) && receivedDataFrameFromUDP.MessageTypeReceivedId < 0)
                {
                    return;
                }

                SignalrClientDataModal processedModal = new();
                string expectedTagToBroadCastDataAsModal = "";
                string tagOnWhichSendAllModelsOfThisType = "";

                if (receivedDataFrameFromUDP.MessageTypeReceivedId == (int)CoreTCP.CoreUDP.Enums.FrameMessageType.DataType1 ||
                    receivedDataFrameFromUDP.MessageTypeReceivedId == (int)CoreTCP.CoreUDP.Enums.FrameMessageType.DataType3)
                {
                    DataModal DataMessage = new DataModal(receivedDataFrameFromUDP.FrameBytesWithStartStopBits);
                    SignalrClientDataModal dataToSend = new SignalrClientDataModal();
                    dataToSend.EntityId = DataMessage.Id;
                    dataToSend.ModalTypeName = DataMessage.GetType().Name;
                    dataToSend.DataModal = DataMessage;

                    processedModal = dataToSend;

                    tagOnWhichSendAllModelsOfThisType = SignalRClient.SignalRClient.GetEncryptedTag(
                              receiveType: SignalRClient.Enums.SignalRReceiveType.DataType1,
                              etityId: 0.ToString(),
                              out string nonEncruptedtag,
                              encruptionKey: ClientSettings.TagEncruptionKey,
                              timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                              ).Trim();

                    expectedTagToBroadCastDataAsModal = SignalRClient.SignalRClient.GetEncryptedTag(
                                  receiveType: SignalRClient.Enums.SignalRReceiveType.DataType1,
                                  etityId: DataMessage.Id.ToString(),
                                  out string nonEncruptedTagOnlyToDebugForModal,
                                  encruptionKey: ClientSettings.TagEncruptionKey,
                                  timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                                  ).Trim();


                    if (ServerSettings.IsSendOnlyRealTime)
                    {
                        _ = HubTimers.SendProcessedMessgesToSignalRClientsWithRetry(processedModal, expectedTagToBroadCastDataAsModal, tagOnWhichSendAllModelsOfThisType);
                        _ = Task.Run(async () =>
                        {
                            string tagOnWhichSendAllDataType3 = SignalRClient.SignalRClient.GetEncryptedTag(
                                      receiveType: SignalRClient.Enums.SignalRReceiveType.DataType3,
                                      etityId: 0.ToString(),
                                      out string nonEncruptedTagOnlyToDebugtagOnWhichSendAllData3Modal,
                                      encruptionKey: ClientSettings.TagEncruptionKey,
                                      timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                                      ).Trim();

                            _ = HubTimers.SendProcessedMessgesToSignalRClientsWithRetry(processedModal, null, tagOnWhichSendAllDataType3);// sendig all Data 3 data on 0 tag

                            List<int> dbRulesForThisUDPdata = await GetData3sIdOfThisModal(DataMessage) ?? new();

                            short count = 1;
                            foreach (var dbRule in dbRulesForThisUDPdata)
                            {
                                string data3TagOnWhichSendData3 = SignalRClient.SignalRClient.GetEncryptedTag(
                                          receiveType: SignalRClient.Enums.SignalRReceiveType.DataType3,
                                          etityId: dbRule.ToString(),
                                          out string nonEncruptedTagOnlyToDebugData3Data,
                                          encruptionKey: ClientSettings.TagEncruptionKey,
                                          timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                                          ).Trim();
                                _ = HubTimers.SendProcessedMessgesToSignalRClientsWithRetry(processedModal, data3TagOnWhichSendData3);
                            }
                        });
                        // add of realtime from appsettings
                        LastTimeDatSendToClients = DateTime.UtcNow;
                        return;
                    }
                    else
                    {
                        AddTagToQueOfFreshDataToForWardDataToClient(expectedTagToBroadCastDataAsModal, processedModal.EntityId, isData1: false);
                    }
                }
                // HubTimers.SendProcessedMessgesToSignalRClientsWithRetry(processedModal, expectedTagToBroadCastDataAsModal, tagOnWhichSendAllModelsOfThisType);
                /***********adding processed buffer starts *******************/
                ProcessedModelBufferExceptData1.AddOrUpdate(
                                     expectedTagToBroadCastDataAsModal,
                                     processedModal,
                                     (key, existingModel) =>
                                     {
                                         // Update the existing Processed modal if needed
                                         existingModel = processedModal;
                                         return existingModel;
                                     });
                /***********adding processed buffer ends*******************/

            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();

                string excetpion = $"Class name : {nameof(HubTimers)}  -- Function Name : {nameof(ProcessAndSendNonData1ToClients)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                              appName: ServerSettings.ThisAppName,
                                              tagsOnWhichToSend: new List<string>() { tag },
                                              nonSerialezedDataToSend: excetpion,
                                              jwtToken: ServerSettings.ThisAppToken);
            }
        }
        private static async Task SendProcessedDataMessagesToClients(SignalrClientDataModal clientDataModal, bool isSendAugumentDatas = false, bool isHalfSecondTimerElapsed = false)
        {
            try
            {
                if (clientDataModal == null)
                {
                    return;
                }
                SignalrClientDataModal clientDataModalObject = clientDataModal;
                string expectedTagToBroadCastDataAsModal = "";
                string tagOnWhichSendAllDatatype1Model = "";
                string modalType = clientDataModal.ModalTypeName;

                if (isHalfSecondTimerElapsed && modalType == nameof(DataModal))
                {
                    tagOnWhichSendAllDatatype1Model = SignalRClient.SignalRClient.GetEncryptedTag(
                                receiveType: SignalRClient.Enums.SignalRReceiveType.DataType2,
                                 etityId: 0.ToString(),
                                 out string nonEncruptedTagOnlyToDebugForData,
                                 encruptionKey: ClientSettings.TagEncruptionKey,
                                 timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                                 ).Trim();

                    expectedTagToBroadCastDataAsModal = SignalRClient.SignalRClient.GetEncryptedTag(
                                  receiveType: SignalRClient.Enums.SignalRReceiveType.DataType2,
                                  etityId: clientDataModalObject.EntityId.ToString(),
                                  out string nonEncruptedTagOnlyToDebugForModal,
                                  encruptionKey: ClientSettings.TagEncruptionKey,
                                  timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                                  ).Trim();
                }
                else if (modalType == nameof(DataModal2))
                {
                    tagOnWhichSendAllDatatype1Model = SignalRClient.SignalRClient.GetEncryptedTag(
                              receiveType: SignalRClient.Enums.SignalRReceiveType.DataType1,
                              etityId: 0.ToString(),
                              out string nonEncruptedTagOnlyToDebugForData,
                              encruptionKey: ClientSettings.TagEncruptionKey,
                              timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                              ).Trim();

                    expectedTagToBroadCastDataAsModal = SignalRClient.SignalRClient.GetEncryptedTag(
                                  receiveType: SignalRClient.Enums.SignalRReceiveType.DataType1,
                                  etityId: clientDataModalObject.EntityId.ToString(),
                                  out string nonEncruptedTagOnlyToDebugForModal,
                                  encruptionKey: ClientSettings.TagEncruptionKey,
                                  timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                                  ).Trim();
                    _ = Task.Run(async () =>
                    {

                        //sending on DataType3 by checking db rules area tag
                        var data3Medel = (DataModal3)clientDataModalObject.DataModal;

                        string tagOnWhichSendAllData3Data3Medel3 = SignalRClient.SignalRClient.GetEncryptedTag(
                                  receiveType: SignalRClient.Enums.SignalRReceiveType.DataType3,
                                  etityId: 0.ToString(),
                                  out string nonEncruptedTagOnlyToDebugtagOnWhichSendAllData3Modal,
                                  encruptionKey: ClientSettings.TagEncruptionKey,
                                  timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                                  ).Trim();

                        _ = HubTimers.SendProcessedMessgesToSignalRClientsWithRetry(clientDataModalObject, null, tagOnWhichSendAllData3Data3Medel3);// sendig all Data 3 data on 0 tag
                        List<int> dbRulesForThisModel = await GetData3sIdOfThisModal(data3Medel) ?? new();

                        foreach (var dbRule in dbRulesForThisModel)
                        {
                            string Data3TagOnWhichSendData3Medel3 = SignalRClient.SignalRClient.GetEncryptedTag(
                                      receiveType: SignalRClient.Enums.SignalRReceiveType.DataType3,
                                      etityId: dbRule.ToString(),
                                      out string nonEncruptedTagOnlyToDebugData3Data,
                                      encruptionKey: ClientSettings.TagEncruptionKey,
                                      timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                                      ).Trim();
                            _ = HubTimers.SendProcessedMessgesToSignalRClientsWithRetry(clientDataModalObject, Data3TagOnWhichSendData3Medel3);
                        }
                    });
                }
                else
                {
                    return;
                }
                _ = SendProcessedMessgesToSignalRClientsWithRetry(clientDataModalObject, expectedTagToBroadCastDataAsModal, tagOnWhichSendAllDatatype1Model);
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();

                string excetpion = $"Class name : {nameof(HubTimers)}  -- Function Name : {nameof(ProcessAndSendNonData1ToClients)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                              appName: ServerSettings.ThisAppName,
                                              tagsOnWhichToSend: new List<string>() { tag },
                                              nonSerialezedDataToSend: excetpion,
                                              jwtToken: ServerSettings.ThisAppToken);
            }
        }
        private async static void RemoveAllUnsubcribedRequestForThisTimePeriod(int timeDelayOfRequest)
        {
            try
            {
                if (TimePeriodAndRequestObj?.ContainsKey(timeDelayOfRequest) ?? false)
                {
                    ConcurrentBag<object> allRequestsInQueForRequestTimeDelayPeriod = TimePeriodAndRequestObj[timeDelayOfRequest]; // geeting all requests for this time delay period
                    if (allRequestsInQueForRequestTimeDelayPeriod?.Count() > 0)
                    {
                        Parallel.ForEach(allRequestsInQueForRequestTimeDelayPeriod, eachRequest =>
                        {
                            SignalRRequestBaseDTO tagAndTimeDelayObj = (SignalRRequestBaseDTO)eachRequest;
                            List<string> tags = tagAndTimeDelayObj?.SignalRTagSubscribedForThisDataAtClient ?? new();

                            foreach (string tag in tags)
                            {
                                //tags and conetionids
                                if (SignalRHub.TagSubscriptions?.ContainsKey(tag) ?? false) // chek if tag is subcribed by any client 
                                {
                                    var noOfClietsSubscribedThisTag = 0;
                                    SignalRHub.TagSubscriptions[tag]?.TryGetNonEnumeratedCount(out noOfClietsSubscribedThisTag);//remove tag one by one if necessary
                                    if (noOfClietsSubscribedThisTag < 1) // in no client is subcribed this tag remove this tag with key
                                    {
                                        // no cilent is subcribed to this tag so remove remove this request from request que(Dictionay)
                                        //TimePeriodAndRequestObj.Remove();
                                        RemoveRequestFromRequestQueueIfNoSubcribedClilentForThisRequest(tag, timeDelayOfRequest);
                                    }
                                }
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();

                string excetpion = $"Class name : {nameof(HubTimers)}  -- Function Name : {nameof(RemoveAllUnsubcribedRequestForThisTimePeriod)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                              appName: ServerSettings.ThisAppName,
                                              tagsOnWhichToSend: new List<string>() { tag },
                                              nonSerialezedDataToSend: excetpion,
                                              jwtToken: ServerSettings.ThisAppToken);
            }
        }
        private static void RemoveRequestFromRequestQueueIfNoSubcribedClilentForThisRequest(string tag, int timeDelay) // get all request tags one by one 
        {
            try
            {
                if (TimePeriodAndRequestObj?.ContainsKey(timeDelay) ?? false)
                {
                    // TimePeriodAndRequestObj => null check is in parent function
                    bool isSuccess = (TimePeriodAndRequestObj[timeDelay]).TryTake(out object reomovedItemRequest); // no method to remove directly

                    var removedIdtemTimeDelayAndTag = (SignalRRequestBaseDTO)reomovedItemRequest; // request obj any for this time key

                    List<string> tagsOfRemovedItem = removedIdtemTimeDelayAndTag?.SignalRTagSubscribedForThisDataAtClient ?? new();

                    if (isSuccess && tagsOfRemovedItem?.Count() == 1 && !tagsOfRemovedItem.Contains(tag)) // the item is removed for this tag inside if it will add back to que
                    {
                        // this item needs not to be removed so add back
                        (TimePeriodAndRequestObj[timeDelay]).Add(reomovedItemRequest); // Add back the item if it's not the one to remove
                    }

                    if (TimePeriodAndRequestObj[timeDelay]?.Count() < 1)
                    {// removing key value pair from dictionay because no request at this time interval entry in cuncurrent dictionary
                        TimePeriodAndRequestObj.Remove(timeDelay, out ConcurrentBag<object> valueRemoved);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorLogtag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();
                string excetpion = $"Class name : {nameof(HubTimers)}  -- Function Name : {nameof(RemoveRequestFromRequestQueueIfNoSubcribedClilentForThisRequest)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                               appName: ServerSettings.ThisAppName,
                                               tagsOnWhichToSend: new List<string>() { errorLogtag },
                                               nonSerialezedDataToSend: excetpion,
                                               jwtToken: ServerSettings.ThisAppToken);
            }
        }
        private static void AddTagToQueOfFreshDataToForWardDataToClient(string tag, int Id, bool isData1)
        {
            if (string.IsNullOrEmpty(tag)) { return; }
            if (isData1)
            {
                //NewDataTagsToBeSendToClientsOnlyData1.Enqueue(tag);
                NewDataTagsToBeSendToClientsOnlyData1.AddOrUpdate(tag, Id, (key, oldValue) => Id);
            }
            else
            {
                //NewDataTagsToBeSendToClientsExceptData2.Enqueue(tag);
                NewDataTagsToBeSendToClientsExceptData1.AddOrUpdate(tag, Id, (key, oldValue) => Id);
            }
        }

        private async static Task<List<int>> GetData3sIdOfThisModal(object dataProperty)
        {
            if (dataProperty == null) { return await Task.FromResult(new List<int>()); }

            List<int> dbRulesOfThisRecivedData = new List<int>();
            Parallel.ForEach(DBData, kvp =>
            {
                var Data3 = kvp.Value;
                if (Data3 != null)
                {
                    if ("check for contion with db to check recived data and validate it with db rules"
                         ==
                        "check for contion with db to check recived data and validate it with db rules"
                    )
                    {
                        dbRulesOfThisRecivedData.Add(kvp.Key);
                    }
                }
            });
            return dbRulesOfThisRecivedData ?? new();
        }

        private async static Task<bool> FillFromDB()
        {
            try
            {
                using (var scope = _serviceProvider?.CreateScope())
                {
                    var DBRulesService = scope?.ServiceProvider?.GetRequiredService<IRepository>();
                    if (DBRulesService != null)
                    {
                        IEnumerable<object> DBRules = await DBRulesService?.GetAllDBRules();
                        if (DBRules != null && DBRules.Count() > 0)
                        {
                            DBData.Clear();
                            foreach (var dbRule in DBRules)
                            {
                                DBData.AddOrUpdate(1, dbRule, (key, oldValue) => dbRule);
                                //DBData.AddOrUpdate(dbRule.Id, dbRule, (key, oldValue) => dbRule);
                            }
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                string errorLogtag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();

                string excetpion = $"Class name : {nameof(TCP_UDPHelper)}  -- Function Name : {nameof(FillFromDB)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: SignalRClient.ClientSettings.ClientSettings.SignalRHubUrl,
                                              appName: "signalrhub", // this app name should be removed if this library is used in other than signalR hub project
                                              tagsOnWhichToSend: new List<string>() { errorLogtag },
                                              nonSerialezedDataToSend: excetpion,
                                              jwtToken: "sdfghj456789sdfghj45678sdfghj45678sdfghj45678dfghj34567sdfgh34567dfghj45678sdfghj345678sdfghj3456");
                return false;
            }
            return false;
        }
    }
}
