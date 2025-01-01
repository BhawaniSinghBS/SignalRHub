using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TCP.UDP
{
    public static class UDPClient
    {

        public static event Action<(int, byte[])> MessageReceivedFromUDP = UDP.UDPClient.HandleMessageReceived;

        public static UdpClient UDPClientApp { get; set; } = new UdpClient(302);// 302 this port can receive data from distributer app on port 302 in quarck city indian server 

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
           _= Task.Run(() =>
            {

                IPEndPoint serverEndPoint = (IPEndPoint)UDP.UDPClient.UDPClientApp.Client.LocalEndPoint; // // 0.0.0.0:302// will accept packets from any ip
                UDPClientApp.Client.ReceiveTimeout = 100; // Set a 5-second timeout

                if (
                    !string.IsNullOrWhiteSpace(expectedIPWhereDataWillBeReceived) &&
                    expectedIPWhereDataWillBeReceived.Length > 6 &&
                    !(expectedIPWhereDataWillBeReceived == "0.0.0.0" &&
                    expectedPortWhereDataWillBeReceived == 302) &&
                    expectedPortWhereDataWillBeReceived != 0
                   )
                {
                    serverEndPoint = new IPEndPoint(IPAddress.Parse(expectedIPWhereDataWillBeReceived), expectedPortWhereDataWillBeReceived);
                }
                if (receiveContinueslyOnSubcribedEvent)
                {

                    while (true)
                    {
                        var responseData = new byte[1024];
                        int udpDataLengthNotIndex = 0;
                        try
                        {

                            byte[] receiveBuffer = UDPClientApp.Receive(ref serverEndPoint);
                            responseData = receiveBuffer;
                            udpDataLengthNotIndex = responseData.Length;

                        }
                        catch (SocketException ex)
                        {
                            // Handle socket exceptions (e.g., timeout)
                            // nothig do if want to receive data further
                        }
                        catch (Exception ex)
                        {
                            // Handle other exceptions
                            // nothig do if want to receive data further
                        }
                        if (!(responseData[0] == 0 &&
                                responseData[1] == 0 &&
                                responseData[2] == 0 &&
                                responseData[3] == 0 &&
                                responseData[4] == 0 &&
                                responseData[5] == 0 &&
                                responseData[6] == 0 &&
                                responseData[7] == 0 &&
                                responseData[8] == 0))
                        {
                            MessageReceivedFromUDP.Invoke((udpDataLengthNotIndex, responseData));
                        }
                    }

                }
                else
                {

                    var responseData = new byte[1024];
                    int udpDataLengthNotIndex = 0;

                    try
                    {

                        byte[] receiveBuffer = UDPClientApp.Receive(ref serverEndPoint);
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
                    if (!(responseData[0] == 0 &&
                        responseData[1] == 0 &&
                        responseData[2] == 0 &&
                        responseData[3] == 0 &&
                        responseData[4] == 0 &&
                        responseData[5] == 0 &&
                        responseData[6] == 0 &&
                        responseData[7] == 0 &&
                        responseData[8] == 0))
                    {
                        MessageReceivedFromUDP.Invoke((udpDataLengthNotIndex, responseData));
                    }

                }
            });
        }

        public static async Task<bool> SendDataOnUDPClient(byte[] dataToSend, string expectedIPWhereDataWillBeReceived = "0.0.0.0", int expectedPortWhereDataWillBeReceived = 302)
        {
            try
            {
                IPEndPoint serverEndPoint = (IPEndPoint)UDP.UDPClient.UDPClientApp.Client.LocalEndPoint; // // 0.0.0.0:302// will accept packets from any ip
                UDPClientApp.Client.ReceiveTimeout = 5000; // Set a 5-second timeout

                if (
                    !string.IsNullOrWhiteSpace(expectedIPWhereDataWillBeReceived) &&
                    expectedIPWhereDataWillBeReceived.Length > 6 &&
                    !(expectedIPWhereDataWillBeReceived == "0.0.0.0" &&
                    expectedPortWhereDataWillBeReceived == 302) &&
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
                return false;
                //throw;
            }
        }
    }
}
