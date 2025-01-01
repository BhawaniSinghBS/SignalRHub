namespace SignalRClient.DataModals.DataModels
{
    public class DataModal2
    {
        public DataModal2(byte[] receivedByteFramesFromTCPorUDPWithStartStopBits)
        {
            byte[] result = receivedByteFramesFromTCPorUDPWithStartStopBits
                      .Skip(1) // Skip the first byte
                      .Take(receivedByteFramesFromTCPorUDPWithStartStopBits.Length - 2) // Exclude the last byte
                      .ToArray();
            // procecess received bytes
            Id = result[2];// id on second index
        }
        public int Id { get; set; }
        public DateTime SignalRTimeStamp { get; set; } = DateTime.MinValue;
        public long SignalREpochTimeStamp { get => new DateTimeOffset(SignalRTimeStamp == DateTime.MinValue ? SignalRTimeStamp.AddDays(1) : SignalRTimeStamp).ToUnixTimeSeconds(); }
        public bool IsRealTimeDataNotCached { get => SignalRTimeStamp == DateTime.MinValue; }
    }
}
