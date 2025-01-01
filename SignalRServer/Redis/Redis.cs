using RedisClient.DataModels;
using SignalRClient.DataModals;
using SignalRClient.SerilizingDeserilizing;
using SignalRServer.Settings;
using System.Collections.Concurrent;

namespace SignalRServer.Redis
{
    public class Redis
    {
        public static ConcurrentDictionary<int, long> RedisIdAndTimeStamp { get; set; } = new();
        public static void HandleData4MessageReceived(int Id, Data4 data)
        {
            try
            {
                if (data.SignalRTimeStamp < DateTime.UtcNow)
                {
                    return;
                }

                SignalrClientDataModal dataToSend = new SignalrClientDataModal();
                dataToSend.EntityId = Id;
                dataToSend.ModalTypeName = data.GetType().Name;
                dataToSend.DataModal = data;

                string expectedTagToBroadCastDataAsModal = SignalRClient.SignalRClient.GetEncryptedTag(
                         receiveType: SignalRClient.Enums.SignalRReceiveType.DataType1,
                         etityId: Id.ToString(),
                         out string nonEncruptedTagOnlyToDebugForModal,
                         encruptionKey: SignalRClient.ClientSettings.ClientSettings.TagEncruptionKey,
                         timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                         ).Trim();

                string tagOnWhichSendAllModelsOfThisType = SignalRClient.SignalRClient.GetEncryptedTag(
                      receiveType: SignalRClient.Enums.SignalRReceiveType.DataType1,
                      etityId: 0.ToString(),
                      out string nonEncruptedTagOnlyToDebugForData1,
                      encruptionKey: SignalRClient.ClientSettings.ClientSettings.TagEncruptionKey,
                      timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                      ).Trim();

                if (ServerSettings.IsSendDataOnDebugTag)
                {

                    _ = HubTimers.SendProcessedMessgesToSignalRClientsWithRetry(dataToSend, SignalRClient.SignalRClient.GetDebugTag(), tagOnWhichSendAllModelsOfThisType);
                    if (RedisIdAndTimeStamp.TryGetValue(Id, out long timeStamp))
                    {
                        // compare last status time send is smaller that current if ye send to clients and update else do nothing
                        if (timeStamp == 0 || data.SignalRTimeStamp.ToFileTimeUtc() > timeStamp)
                        {
                            RedisIdAndTimeStamp.AddOrUpdate(data.Id, data.SignalRTimeStamp.ToFileTimeUtc(), (key, oldValue) => data.SignalRTimeStamp.ToFileTimeUtc());
                            _ = HubTimers.SendProcessedMessgesToSignalRClientsWithRetry(dataToSend, expectedTagToBroadCastDataAsModal);
                        }
                    }
                    else
                    {
                        RedisIdAndTimeStamp.AddOrUpdate(Id, data.SignalRTimeStamp.ToFileTimeUtc(), (key, oldValue) => data.SignalRTimeStamp.ToFileTimeUtc());
                    }
                }
                else
                {

                    if (RedisIdAndTimeStamp.TryGetValue(Id, out long statuTime))
                    {
                        // compare last status time send is smaller that current if ye send to clients and update else do nothing
                        if (statuTime == 0 || data.SignalRTimeStamp.ToFileTimeUtc() > statuTime)
                        {
                            RedisIdAndTimeStamp.AddOrUpdate(data.Id, data.SignalRTimeStamp.ToFileTimeUtc(), (key, oldValue) => data.SignalRTimeStamp.ToFileTimeUtc());
                            _ = HubTimers.SendProcessedMessgesToSignalRClientsWithRetry(dataToSend, expectedTagToBroadCastDataAsModal, tagOnWhichSendAllModelsOfThisType);
                        }
                    }
                    else
                    {
                        RedisIdAndTimeStamp.AddOrUpdate(Id, data.SignalRTimeStamp.ToFileTimeUtc(), (key, oldValue) => data.SignalRTimeStamp.ToFileTimeUtc());
                    }
                }
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();
                string message = SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: SignalRClient.ClientSettings.ClientSettings.SignalRHubUrl,
                                                      appName: ServerSettings.ThisAppName,
                                                      tagsOnWhichToSend: new List<string>() { tag },
                                                      nonSerialezedDataToSend: message,
                                                      jwtToken: ServerSettings.ThisAppToken);
            }
        }
        public static void HandleData5Message3Received(int Id, Data6 data)
        {
            try
            {
                SignalrClientDataModal dataToSend = new SignalrClientDataModal();
                dataToSend.EntityId = data.Id;
                dataToSend.ModalTypeName = data.GetType().Name;
                dataToSend.DataModal = data;

                string expectedTagToBroadCastDataAsModal = SignalRClient.SignalRClient.GetEncryptedTag(
                              receiveType: SignalRClient.Enums.SignalRReceiveType.DataType6,
                              etityId: Id.ToString(),
                              out string nonEncruptedTagOnlyToDebugForModal,
                              encruptionKey: SignalRClient.ClientSettings.ClientSettings.TagEncruptionKey,
                              timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                              ).Trim();

                string tagOnWhichSendAllModelsOfThisType = SignalRClient.SignalRClient.GetEncryptedTag(
                      receiveType: SignalRClient.Enums.SignalRReceiveType.DataType6,
                      etityId: 0.ToString(),
                      out string nonEncruptedTagOnlyToDebugForData1,
                      encruptionKey: SignalRClient.ClientSettings.ClientSettings.TagEncruptionKey,
                      timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                      ).Trim();
                _ = HubTimers.SendProcessedMessgesToSignalRClientsWithRetry(dataToSend, expectedTagToBroadCastDataAsModal, tagOnWhichSendAllModelsOfThisType);
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();
                string message = SerilizingDeserilizing.JSONSerializeOBJ(ex);
                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: SignalRClient.ClientSettings.ClientSettings.SignalRHubUrl,
                                                      appName: ServerSettings.ThisAppName,
                                                      tagsOnWhichToSend: new List<string>() { tag },
                                                      nonSerialezedDataToSend: message,
                                                      jwtToken: ServerSettings.ThisAppToken);
            }
        }
        public static void HandleData5EventMessageReceived(int Id, Data5 data)
        {

            try
            {
                SignalrClientDataModal dataToSend = new SignalrClientDataModal();
                dataToSend.EntityId = data.Id;
                dataToSend.ModalTypeName = data.GetType().Name;
                dataToSend.DataModal = data;

                string expectedTagToBroadCastDataAsModal = SignalRClient.SignalRClient.GetEncryptedTag(
                           receiveType: SignalRClient.Enums.SignalRReceiveType.DataType5,
                           etityId: Id.ToString(),
                           out string nonEncruptedTagOnlyToDebugForModal,
                           encruptionKey: SignalRClient.ClientSettings.ClientSettings.TagEncruptionKey,
                           timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                           ).Trim();

                string tagOnWhichSendAllModelsOfThisType = SignalRClient.SignalRClient.GetEncryptedTag(
                      receiveType: SignalRClient.Enums.SignalRReceiveType.DataType5,
                      etityId: 0.ToString(),
                      out string nonEncruptedTagOnlyToDebugForData1,
                      encruptionKey: SignalRClient.ClientSettings.ClientSettings.TagEncruptionKey,
                      timeInterValForRecivingData_inMS: (int)SignalRClient.Enums.SignalRDataReciveTimeInterval.Milliseconds200
                      ).Trim();
                _ = HubTimers.SendProcessedMessgesToSignalRClientsWithRetry(dataToSend, expectedTagToBroadCastDataAsModal, tagOnWhichSendAllModelsOfThisType);
            }
            catch (Exception ex)
            {
                string tag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();
                string message = SerilizingDeserilizing.JSONSerializeOBJ(ex);
                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: SignalRClient.ClientSettings.ClientSettings.SignalRHubUrl,
                                                      appName: ServerSettings.ThisAppName,
                                                      tagsOnWhichToSend: new List<string>() { tag },
                                                      nonSerialezedDataToSend: message,
                                                      jwtToken: ServerSettings.ThisAppToken);
            }
        }
        public static void CloseRedisConnection()
        {
            if (ServerSettings.IsDisconnectCnonnectToRedisWhenNoClient)
            {

                //RedisClient.RedisClient.Data4MessageReceived -= HandleData4MessageReceived;
                //RedisClient.RedisClient.Data5Message3Received -= HandleData5MessageReceived;
                //RedisClient.RedisClient.Data5EventMessageReceived -= HandleData5EventMessageReceived;
                RedisClient.MyRedisClient.Dispose();
                ServerSettings.IsRedisDeligatesSubscribed = false;
            }
        }
    }
}
