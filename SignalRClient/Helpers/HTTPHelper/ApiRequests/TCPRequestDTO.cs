namespace SignalRClient.Helpers.HTTPHelper.ApiRequests
{
    public class TCPRequestDTO : SignalRRequestBaseDTO
    {
        public string TCPServerIP { get; set; }

        public int TCPServerPort { get; set; }
        public byte[] TCPRequest { get; set; }
    }
}
