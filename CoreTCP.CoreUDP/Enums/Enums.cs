namespace CoreTCP.CoreUDP.Enums
{
    public enum ConnectionType
    {
        Biderectional = 10,
        Transmit = 20,
        Receive = 30,
    }
    public enum FrameMessageType
    {
        Unknown = -1,
        Ping = 10,
        DataType1 = 20,
        DataType2 = 30,
        DataType3 = 40,//4,5,6 will come from redis not frames
    }
}

