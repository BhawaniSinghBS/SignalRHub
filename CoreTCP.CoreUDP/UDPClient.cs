using CoreTCP.CoreUDP.Helper;
using SignalRClient.SerilizingDeserilizing;
using System.Net;
using System.Net.Sockets;

namespace CoreTCP.CoreUDP
{
    public static class UDPClient
    {

        public static event Action<(int, byte[])> MessageReceivedFromUDP = UDPClient.HandleMessageReceived;// this handles is called in hub timer

        public static UdpClient UDPClientApp { get; set; } = new UdpClient(CoreTCP.CoreUDP.Settings.TCPUDPSettings.UDPPort);// 

        // data length and Datas from udp
        private static void HandleMessageReceived((int, byte[]) udpDatas)
        {
            // if there is nobody who has not subcribed this event then it might throw exception to subcribing the event with this fucntion to avoid exception and doing nothing here
            return;
        }

        public static async Task ReceiveDataFromUDPClient(string expectedIPWhereDataWillBeReceived = "0.0.0.0",// allos any ip to send to this udp receiver
                                                                int expectedPortWhereDataWillBeReceived = 302,
                                                                bool receiveContinueslyOnSubcribedEvent = true
                                                         )
        {
            try
            {
                _ = Task.Run(() =>
                    {

                        IPEndPoint serverEndPoint = (IPEndPoint)UDPClient.UDPClientApp.Client.LocalEndPoint; // // 0.0.0.0:CoreTCP.CoreUDP.Settings.TCPUDPSettings.UDPPort // will accept packets from any ip
                        UDPClientApp.Client.ReceiveTimeout = int.MaxValue;

                        if (
                            !string.IsNullOrWhiteSpace(expectedIPWhereDataWillBeReceived) &&
                            expectedIPWhereDataWillBeReceived.Length > 6 &&
                            !(expectedIPWhereDataWillBeReceived == "0.0.0.0" &&
                            expectedPortWhereDataWillBeReceived == CoreTCP.CoreUDP.Settings.TCPUDPSettings.UDPPort) &&
                            expectedPortWhereDataWillBeReceived != 0
                           )
                        {
                            serverEndPoint = new IPEndPoint(IPAddress.Parse(expectedIPWhereDataWillBeReceived), expectedPortWhereDataWillBeReceived);
                        }
                        if (receiveContinueslyOnSubcribedEvent)
                        {

                            while (true)
                            {
                                var responseData = new byte[] { };
                                int udpDataLengthNotIndex = 0;
                                try
                                {

                                    byte[] receiveBuffer = UDPClientApp.Receive(ref serverEndPoint);
                                    responseData = new byte[receiveBuffer.Length];
                                    responseData = receiveBuffer;
                                    udpDataLengthNotIndex = responseData.Length;

                                }
                                catch (SocketException ex)
                                {
                                    // Handle socket exceptions (e.g., timeout)
                                    // nothig do if want to receive data further
                                    MessageReceivedFromUDP?.Invoke((int.MinValue, new byte []{ 1,2,3,4,5,6,7,8,9}));// to restart reading againg from udp passed to hub timer
                                }
                                catch (Exception ex)
                                {
                                    MessageReceivedFromUDP?.Invoke((int.MinValue, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }));// to restart reading againg from udp passed to hub timer
                                    // Handle other exceptions
                                    // nothig do if want to receive data further
                                }
                                if (responseData.Length > 10 && !(responseData[0] == 0 &&
                                        responseData[1] == 0 &&
                                        responseData[2] == 0 &&
                                        responseData[3] == 0 &&
                                        responseData[4] == 0 &&
                                        responseData[5] == 0 &&
                                        responseData[6] == 0 &&
                                        responseData[7] == 0 &&
                                        responseData[8] == 0 &&
                                        responseData[10] == 0))
                                {
                                    MessageReceivedFromUDP?.Invoke((udpDataLengthNotIndex, responseData));
                                }
                            }

                        }
                        else
                        {

                            var responseData = new byte[] { };
                            int udpDataLengthNotIndex = 0;

                            try
                            {

                                byte[] receiveBuffer = UDPClientApp.Receive(ref serverEndPoint);
                                responseData = new byte[receiveBuffer.Length];
                                responseData = receiveBuffer;
                                udpDataLengthNotIndex = responseData.Length;

                            }
                            catch (SocketException ex)
                            {
                                // Handle socket exceptions (e.g., timeout)
                                //UDPClientApp.Connect(expectedIPWhereDataWillBeReceived, expectedPortWhereDataWillBeReceived);
                            }
                            catch (Exception ex)
                            {
                                // Handle other exceptions
                                //UDPClientApp.Connect(expectedIPWhereDataWillBeReceived, expectedPortWhereDataWillBeReceived);
                            }
                            if (responseData.Length > 10 && !(responseData[0] == 0 &&
                                responseData[1] == 0 &&
                                responseData[2] == 0 &&
                                responseData[3] == 0 &&
                                responseData[4] == 0 &&
                                responseData[5] == 0 &&
                                responseData[6] == 0 &&
                                responseData[7] == 0 &&
                                responseData[8] == 0 &&
                                responseData[10] == 0))
                            {
                                MessageReceivedFromUDP?.Invoke((udpDataLengthNotIndex, responseData));
                            }

                        }
                    });
            }
            catch (Exception ex)
            {
                string errorLogtag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();

                string excetpion = $"Class name : {nameof(TCP_UDPHelper)}  -- Function Name : {nameof(ReceiveDataFromUDPClient)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

               _ =  SignalRClient.SignalRClient.SendMessage(hubCompleteurl: SignalRClient.ClientSettings.ClientSettings.SignalRHubUrl,
                                              appName: "signalrhub", // this app name should be removed if this library is used in other than signalR hub project
                                              tagsOnWhichToSend: new List<string>() { errorLogtag },
                                              nonSerialezedDataToSend: excetpion,
                                              jwtToken: "sdfghj456789sdfghj45678sdfghj45678sdfghj45678dfghj34567sdfgh34567dfghj45678sdfghj345678sdfghj3456");
                // this tokent should be removed if this library is used in other than signalR hub project
            }
        }


        public static async Task<bool> SendDataOnUDPClient(byte[] dataToSend, string expectedIPWhereDataWillBeReceived = "0.0.0.0", int expectedPortWhereDataWillBeReceived = 302)
        {
            try
            {
                IPEndPoint serverEndPoint = (IPEndPoint)UDPClient.UDPClientApp.Client.LocalEndPoint; // // 0.0.0.0:CoreTCP.CoreUDP.Settings.TCPUDPSettings.UDPPort // will accept packets from any ip
                UDPClientApp.Client.ReceiveTimeout = int.MinValue; // Set a 5-second timeout

                if (
                    !string.IsNullOrWhiteSpace(expectedIPWhereDataWillBeReceived) &&
                    expectedIPWhereDataWillBeReceived.Length > 6 &&
                    !(expectedIPWhereDataWillBeReceived == "0.0.0.0" &&
                    expectedPortWhereDataWillBeReceived == CoreTCP.CoreUDP.Settings.TCPUDPSettings.UDPPort) &&
                    expectedPortWhereDataWillBeReceived != 0
                   )
                {
                    serverEndPoint = new IPEndPoint(IPAddress.Parse(expectedIPWhereDataWillBeReceived), expectedPortWhereDataWillBeReceived);
                }
                await UDPClientApp.SendAsync(dataToSend, dataToSend.Length, serverEndPoint);
                return true;
            }
            catch (Exception ex)
            {
                string errorLogtag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();

                string excetpion = $"Class name : {nameof(TCP_UDPHelper)}  -- Function Name : {nameof(SendDataOnUDPClient)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: SignalRClient.ClientSettings.ClientSettings.SignalRHubUrl,
                                              appName: "signalrhub", // this app name should be removed if this library is used in other than signalR hub project
                                              tagsOnWhichToSend: new List<string>() { errorLogtag },
                                              nonSerialezedDataToSend: excetpion,
                                              jwtToken: "sdfghj456789sdfghj45678sdfghj45678sdfghj45678dfghj34567sdfgh34567dfghj45678sdfghj345678sdfghj3456");
                // this tokent should be removed if this library is used in other than signalR hub project
                return false;
                //throw;
            }
        }
    }
}
