namespace SignalRClient.DataModals
{
    public class SignalrClientDataModal
    {
        public int EntityId { get; set; } 
        public string? ModalTypeName { get; set; }= string.Empty;
        public string? Message { get; set; }=string.Empty;
        public object DataModal { get; set; } = new object();
    }

}
