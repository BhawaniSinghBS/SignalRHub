namespace SignalRClient.Helpers.HTTPHelper.ApiRequests
{
    public class SignalRRequestBaseDTO
    {
        public int EntityId { get; set; }
        public List<string> SignalRTagSubscribedForThisDataAtClient // used for removing request if not subscribed by any client on hub
        {
            get
            {
                List<string> encruptedTags = new List<string>();
                string encriptedTag = SignalRClient.GetEncryptedTag(
                        receiveType: SignalRRequestType,
                        etityId: EntityId.ToString(),
                        out string nonEncruptedTagOnlyToDebug,
                        timeInterValForRecivingData_inMS: (int)SignalRRequestType);
                encruptedTags.Add(encriptedTag);
                return encruptedTags;
            }
        } 
        public Enums.SignalRDataReciveTimeInterval RunAfterMilliconds { get; set; } = Enums.SignalRDataReciveTimeInterval.Default;
        public Enums.SignalRReceiveType SignalRRequestType { get; set; } = Enums.SignalRReceiveType.DataType1;
        public bool UseClientTimer { get; set; } = false;
    }
}
