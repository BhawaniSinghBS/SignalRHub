using TCP.UDP.TCP_UDPModals;
using System;
using System.Collections.Generic;

namespace TCP.UDP.Helper
{
    public class TCP_UDPHelper
    {
        public static List<TCP_UDPFrameModal> CollectFramesFrames(byte[] byteArray)
        {

            // isCompleteFrame,IdofDatas,isStartHalfmessage,frame/message
            //isStartHalfmessage = true => this will be true if this half frame does not have start bit
            //isStartHalfmessage = false => this will be true if this half frame does not have end bit
            List<TCP_UDPFrameModal> framesToReturn = new List<TCP_UDPFrameModal>();
            byte[] frameToReturn = new byte[] { };
            byte previousByteValue = 0;
            //foreach (byte eachByte in byteArray)
            for (int i = 0; i < byteArray.Length; i++)
            {
                byte eachByte = byteArray[i];
                Array.Resize(ref frameToReturn, frameToReturn.Length + 1);
                frameToReturn[frameToReturn.Length - 1] = eachByte;

                if (eachByte == byte.MaxValue)// either start or stop
                {
                    if ((frameToReturn[0] == byte.MaxValue && frameToReturn.Length > 1))
                    {
                        framesToReturn.Add(new TCP_UDPFrameModal() { FrameBytesWithStartStopBits = frameToReturn });
                        frameToReturn = new byte[] { };
                    }
                    else if (frameToReturn.Length > 1 ? frameToReturn[frameToReturn.Length - 2] == byte.MaxValue && frameToReturn.Length > 1 : false)
                    {
                        byte[] newArray = new byte[frameToReturn.Length - 1];
                        Array.Copy(frameToReturn, 0, newArray, 0, newArray.Length);

                        framesToReturn.Add(new TCP_UDPFrameModal() { FrameBytesWithStartStopBits = newArray });
                        frameToReturn = new byte[] { eachByte };
                    }
                }
                // byte.MaxValue was not found till last byte so make its frame  
                if (i == byteArray.Length - 1)
                {
                    framesToReturn.Add(new TCP_UDPFrameModal() { FrameBytesWithStartStopBits = frameToReturn });
                    frameToReturn = new byte[] { };
                }
                previousByteValue = eachByte;

            }
            return framesToReturn;
        }
    }
}
