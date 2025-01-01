namespace RedisClient.DataModels
{
    public class Data6
    {
        public int Id { get; set; }
        public DateTime SignalRTimeStamp { get; set; } = DateTime.MinValue;
        public long SignalREpochTimeStamp { get => new DateTimeOffset(SignalRTimeStamp == DateTime.MinValue ? SignalRTimeStamp.AddDays(1) : SignalRTimeStamp).ToUnixTimeSeconds(); }
    }
}
