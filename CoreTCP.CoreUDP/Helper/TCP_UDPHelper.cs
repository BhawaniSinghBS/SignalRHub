using SignalRClient.DataModals;
using SignalRClient.SerilizingDeserilizing;

namespace CoreTCP.CoreUDP.Helper
{
    public class TCP_UDPHelper
    {
        public static List<TCP_UDPFrameModal> CollectFramesFrames(byte[] byteArray)
        {
            List<TCP_UDPFrameModal> framesToReturn = new List<TCP_UDPFrameModal>();
            try
            {
                byte[] frameToReturn = new byte[] { };
                byte nextByteValue = 0;
                //foreach (byte eachByte in byteArray)
                for (int i = 0; i < byteArray.Length; i++)
                {

                    byte eachByte = byteArray[i];
                    Array.Resize(ref frameToReturn, frameToReturn.Length + 1);// increasing size or return array by 1 cant decide at what leght of this frame will be because byteArray will have many frames , and { 200,20} = byte.MaxValue and {200,19}=244. these adjacent combinations of byte will result in one byte with value i.e. 244 or byte.MaxValue

                    if (i < byteArray.Length - 2)// till second last index of the frame
                    {
                        nextByteValue = byteArray[i + 1];

                        if (eachByte == 200)// current byte
                        {
                            switch (nextByteValue)
                            {
                                case 20: frameToReturn[frameToReturn.Length - 1] = byte.MaxValue; i++; break;
                                case 19: frameToReturn[frameToReturn.Length - 1] = 244; i++; break;
                                default: frameToReturn[frameToReturn.Length - 1] = eachByte; break;// nomal add to frame
                            }
                        }
                        else
                        {
                            frameToReturn[frameToReturn.Length - 1] = eachByte;
                        }
                    }
                    else
                    {
                        // last byte of the data
                        frameToReturn[frameToReturn.Length - 1] = eachByte;
                    }


                    #region handle current frame completion on byte.MaxValue and start new frame if data is there
                    if (eachByte == byte.MaxValue)// either start or stop
                    {
                        if ((frameToReturn[0] == byte.MaxValue && frameToReturn.Length > 1))
                        {
                            framesToReturn.Add(new TCP_UDPFrameModal() { FrameBytesWithStartStopBits = frameToReturn });
                            frameToReturn = new byte[] { };
                        }
                        else if (frameToReturn.Length > 1 ? frameToReturn[frameToReturn.Length - 2] == byte.MaxValue : false)
                        {
                            byte[] newArray = new byte[frameToReturn.Length - 1]; 
                            Array.Copy(frameToReturn, 0, newArray, 0, newArray.Length);

                            framesToReturn.Add(new TCP_UDPFrameModal() { FrameBytesWithStartStopBits = newArray });
                            frameToReturn = new byte[] { eachByte };
                        }
                    }
                    if (i >= byteArray.Length - 1) 
                    {
                        framesToReturn.Add(new TCP_UDPFrameModal() { FrameBytesWithStartStopBits = frameToReturn });
                        frameToReturn = new byte[] { };
                    }
                    #endregion handle current frame completion on byte.MaxValue and start new frame if data is there
                }
            }
            catch (Exception ex)
            {
                string errorLogtag = SignalRClient.SignalRClient.GetEncruptedErrorLogTag();

                string excetpion = $"Class name : {nameof(TCP_UDPHelper)}  -- Function Name : {nameof(CollectFramesFrames)}----------" + SerilizingDeserilizing.JSONSerializeOBJ(ex);

                _ = SignalRClient.SignalRClient.SendMessage(hubCompleteurl: SignalRClient.ClientSettings.ClientSettings.SignalRHubUrl,
                                              appName: "signalrhub", // this app name should be removed if this library is used in other than signalR hub project
                                              tagsOnWhichToSend: new List<string>() { errorLogtag },
                                              nonSerialezedDataToSend: excetpion,
                                              jwtToken: "sdfghj456789sdfghj45678sdfghj45678sdfghj45678dfghj34567sdfgh34567dfghj45678sdfghj345678sdfghj3456");
                // this tokent should be removed if this library is used in other than signalR hub project
            }
            return framesToReturn;
        }
    }
}
