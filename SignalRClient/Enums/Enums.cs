namespace SignalRClient.Enums
{
    public enum SignalRReceiveType
    {
        Unknown = -1,
        Ping = 10,
        DataType1 = 20,
        DataType2 = 30,
        DataType3 = 40,
        DataType4 = 50,
        DataType5 = 60,
        DataType6 = 70,
        Logs = 80,
    }
    public enum FrameMessageType
    {
        Unknown = -1,
        DataType1 = 20,
        DataType2 = 30,
        DataType3 = 40,
        DataType4 = 50,
        DataType5 = 60,
        DataType6 = 70,
    }
    public enum SignalRDataReciveTimeInterval
    {
        Unknown = -1,// get only once
        GetResponceOnlyOnce = 0,// get only once
        Default = 200,
        Milliseconds10 = 10,
        Milliseconds50 = 50,
        Milliseconds100 = 100,
        Milliseconds200 = 200,
        Milliseconds300 = 300,
        Milliseconds400 = 400,
        Milliseconds500 = 500,
        Milliseconds600 = 600,
        Milliseconds700 = 700,
        Milliseconds800 = 800,
        Milliseconds900 = 900,
        Seconds1 = 1000,// 1 second
        Seconds1andHalf = 1500,// 1.5 second
        Seconds2 = 2000,// 2 second
        Seconds3 = 3000,// 3 second
        Seconds5 = 5000,// 5 second
        Seconds10 = 10000,// 10 second
        Minute1 = 60000,// 1 Minute
        Minute2 = 120000,// 2 Minute 
        Minute5 = 600000,// 5 Minute 
    } 
}
