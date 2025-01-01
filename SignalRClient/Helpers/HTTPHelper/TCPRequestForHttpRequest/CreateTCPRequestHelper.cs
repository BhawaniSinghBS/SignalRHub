using System.Text;

namespace SignalRClient.Helpers.HTTPHelper.TCPRequestForHttpRequest
{
    public class CreateTCPRequestHelper
    {
        public static byte[] GetIdBytes(int Id)
        {
            string text = Id.ToString();// should be Bit converted string
            var bytes = Encoding.BigEndianUnicode.GetBytes(text.ToString());
            return bytes;
        }


        static byte messageCountToSend = 1;
        public static async Task<byte[]> GetDataMessage(
            byte messageTyepebyte = (byte)Enums.FrameMessageType.DataType5,
            double property1 = 3,
            double property2 = 2,
            int Id = 1, 
            bool reverseBytes = true// keep it true
            )
        {

            byte[] arrayOfBytes = new byte[39];

            //arrayOfBytes.Add(BitConverter.GetBytes((byte)Enums.MessageType.enDataMessage).Reverse().ToArray());

            arrayOfBytes[0] = messageTyepebyte;


            byte[] messageCountToSendBytes;
            if (reverseBytes)
            {
                messageCountToSendBytes = BitConverter.GetBytes(messageCountToSend).Reverse().ToArray();
            }
            else
            {
                messageCountToSendBytes = BitConverter.GetBytes(messageCountToSend).ToArray();
            }


            arrayOfBytes[1] = messageCountToSendBytes[1]; // msg id
          
            byte[] id;
            if (reverseBytes)
            {
                id = BitConverter.GetBytes(Id).ToArray().Reverse().ToArray();
            }
            else
            {
                id = BitConverter.GetBytes(Id).ToArray().ToArray();
            }

            arrayOfBytes[2] = id[0];
            arrayOfBytes[3] = id[1];
            arrayOfBytes[4] = id[2];
            arrayOfBytes[5] = id[3];


            byte[] property1Array;
            if (reverseBytes)
            {
                property1Array = BitConverter.GetBytes(property1).Reverse().ToArray();
            }
            else
            {
                property1Array = BitConverter.GetBytes(property1).ToArray();
            }
            arrayOfBytes[6] = property1Array[0]; 
            arrayOfBytes[7] = property1Array[1];  

            byte[] property2Array;

            if (reverseBytes)
            {
                property2Array = BitConverter.GetBytes(property2).Reverse().ToArray();
            }
            else
            {
                property2Array = BitConverter.GetBytes(property2).ToArray();
            }
            arrayOfBytes[16] = property2Array[0]; // altitude
            arrayOfBytes[17] = property2Array[1]; // altitude

             
            if (messageCountToSend == 255)//Message Count: 0 to 255.  Increments for every message, wraps around back to 0 once reach 255. 
            {
                messageCountToSend = 0; // will be incremented in next line
            }
            messageCountToSend++;
            return AddStartStopBit(arrayOfBytes).ToArray();
        }


        private static List<byte> AddStartStopBit(byte[] arrayOfBytes)
        {
            List<byte> result = new List<byte>() { byte.MaxValue};
            foreach (var item in arrayOfBytes)
            {
                result.Add(item);
            }
            result.Add(byte.MaxValue);
            return result;
        }
    }
}

