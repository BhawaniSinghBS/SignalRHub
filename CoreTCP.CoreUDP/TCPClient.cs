using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CoreTCP.CoreUDP
{
    public class TCPClient : IDisposable
    {
        private bool disposedValue;
        public TcpClient TCPClientApp { get; }
        public bool TCPClientIsConnected { get => TCPClientApp.Connected; }
        public IPAddress UDPServerIp { get; set; }
        public int UDPServerPort { get; }
        public TCPClient(int TCPServerPort = 303, string TCPServerIp = "")
        {   //208.91.197.27 tcp // 2.travel safety
            //"192.168.1.91" tcp // // //
            try
            {
                if (!string.IsNullOrEmpty(TCPServerIp) && TCPServerIp.Length > 5)
                {
                    // TryParse returns true when IP is parsed successfully
                    UDPServerIp = IPAddress.Parse(TCPServerIp);
                }
                else
                {
                    UDPServerIp = IPAddress.Any;
                    IPAddress udpServerIpFromUrl;
                    if (!IPAddress.TryParse("tcpserver.com", out udpServerIpFromUrl))
                    {
                        // in case user input is not an IP, assume it's a hostname
                        UDPServerIp = Dns.GetHostEntry("tcpserver.com").AddressList[0];
                    }
                }

                UDPServerPort = TCPServerPort == 0 ? 303 : TCPServerPort;
                TCPClientApp = new TcpClient();
                TCPClientApp.Connect(UDPServerIp, UDPServerPort);
            }
            catch (Exception ex)
            {
                string exs = JsonConvert.SerializeObject(ex);
                //lock (Constants.Constants.fileLockObject) // Lock the critical section to ensure thread safety
                //{
                //    try
                //    {
                //        System.IO.File.AppendAllText(Constants.Constants.LogFilePath, $"Reached  - {nameof(TCPClient)} Constructor : {nameof(TCPClient)} {DateTime.UtcNow} {Environment.NewLine}  Exception : \n {exs}");
                //    }
                //    catch (Exception ex2)
                //    {
                //       // Console.WriteLine($"An error occurred while writing to the file: {ex.Message}");
                //    }
                //}
                //log // throw;
            }
        }
        public async Task<(int, byte[])> SendRequestToClientAndGetRawData(byte[] requestByteArray)
        {

            //lock (Constants.Constants.fileLockObject) // Lock the critical section to ensure thread safety
            //{
            //    try
            //    {
            //        System.IO.File.AppendAllText(Constants.Constants.LogFilePath, $"Reached Class- {nameof(TcpClient)} Merthod  : {nameof(SendRequestToClientAndGetRawData)} {DateTime.UtcNow} \n");
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"An error occurred while writing to the file: {ex.Message}");
            //    }
            //}

            // Receive the TcpServer.response.
            // Buffer to store the response bytes.
            //var bufferData = new byte[1048576];
            var bufferData = new byte[1024];
            int tcpDataLengthNotIndex = 0;
            try
            {

                if (TCPClientIsConnected)
                {
                    // Get the network stream from the client
                    using (NetworkStream stream = TCPClientApp.GetStream())
                    {
                        stream.ReadTimeout = 10;
                        if (stream.CanRead)
                        {
                            StringBuilder sb = new StringBuilder();
                            //lock (Constants.Constants.fileLockObject) // Lock the critical section to ensure thread safety
                            //{
                            //    try
                            //    {
                            //        System.IO.File.AppendAllText(Constants.Constants.LogFilePath, $"Reached Class- {nameof(TcpClient)} Merthod  : {nameof(SendRequestToClientAndGetRawData)} \n wrote on tcp stream{DateTime.UtcNow} \n");
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        Console.WriteLine($"An error occurred while writing to the file: {ex.Message}");
                            //    }
                            //}

                            await stream.WriteAsync(requestByteArray, 0, requestByteArray.Length);

                            // String to store the response ASCII representation.
                            int bytes = 0;


                            //lock (Constants.Constants.fileLockObject) // Lock the critical section to ensure thread safety
                            //{
                            //    try
                            //    {
                            //        System.IO.File.AppendAllText(Constants.Constants.LogFilePath, $"Reached Class- {nameof(TcpClient)} Merthod  : {nameof(SendRequestToClientAndGetRawData)} \n  started reading the tcp data   {DateTime.UtcNow} \n");
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        Console.WriteLine($"An error occurred while writing to the file: {ex.Message}");
                            //    }
                            //}

                            do
                            {
                                tcpDataLengthNotIndex++;
                                bytes = await stream.ReadAsync(bufferData, 0, bufferData.Length);
                                sb.AppendFormat("{0}", Encoding.Unicode.GetString(bufferData, 0, bytes));

                                //lock (Constants.Constants.fileLockObject) // Lock the critical section to ensure thread safety
                                //{
                                //    try
                                //    {
                                //        System.IO.File.AppendAllText(Constants.Constants.LogFilePath, $"Reached Class- {nameof(TcpClient)} Merthod  : {nameof(SendRequestToClientAndGetRawData)} \n  tcp data  read successfully {DateTime.UtcNow} \n");
                                //    }
                                //    catch (Exception ex)
                                //    {
                                //        Console.WriteLine($"An error occurred while writing to the file: {ex.Message}");
                                //    }
                                //}
                            }
                            while (stream.DataAvailable);

                            TCPClientApp.Close();
                            stream.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //throw;
                //lock (Constants.Constants.fileLockObject) // Lock the critical section to ensure thread safety
                //{
                //    try
                //    {
                //        string exceptionText = Newtonsoft.Json.JsonConvert.SerializeObject(ex);
                //    }
                //    catch (Exception ex2)
                //    {
                //        // Console.WriteLine($"An error occurred while writing to the file: {ex.Message}");
                //    }
                //}
            }

            //lock (Constants.Constants.fileLockObject) // Lock the critical section to ensure thread safety
            //{
            //    try
            //    {
            //        System.IO.File.AppendAllText(Constants.Constants.LogFilePath, $"Reached Class- {nameof(TcpClient)} Merthod  : {nameof(SendRequestToClientAndGetRawData)} \n  going back from tcp with data {DateTime.UtcNow} \n");
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"An error occurred while writing to the file: {ex.Message}");
            //    }
            //}


            return (tcpDataLengthNotIndex, bufferData);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TCPClientSignalR()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

