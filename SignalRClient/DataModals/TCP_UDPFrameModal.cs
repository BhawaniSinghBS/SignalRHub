namespace SignalRClient.DataModals
{
    public class TCP_UDPFrameModal
    {
        public int Id
        {
            get
            {
                if (IsCompleteFrame)// valid message will be greater than 10
                {
                    int messageId = BitConverter.ToInt32(FrameBytesWithStartStopBits, 2);// contains start and stop bits i.e byte.MaxValue
                    return messageId;
                }
                return -1;
            }
        }
        public int MessageTypeReceivedId
        {
            get
            {
                if (IsCompleteFrame)// valid message will be greater than 10
                {
                    int messageId = FrameBytesWithStartStopBits[1];// contains start and stop bits i.e byte.MaxValue at 0 at 1 type of message
                    return messageId;
                }
                return -1;
            }
        }
        public bool IsCompleteFrame
        {
            get
            {
                bool iscompleteFrame = false;
                if (FrameBytesWithStartStopBits?.Length > 2)
                {
                    iscompleteFrame = FrameBytesWithStartStopBits[0] == byte.MaxValue && //byte.MaxValue start stop bit
                                        FrameBytesWithStartStopBits[2] != byte.MaxValue &&
                                        FrameBytesWithStartStopBits[FrameBytesWithStartStopBits.Length - 1] == byte.MaxValue;
                }
                return iscompleteFrame;
            }
        }
        public bool IsStartingPartialFrame
        {
            get
            {
                bool isStartingPartialFrame = false;
                if (!IsCompleteFrame)
                {
                    if (FrameBytesWithStartStopBits?.Length > 0)
                    {
                        isStartingPartialFrame =
                            FrameBytesWithStartStopBits[0] == byte.MaxValue && FrameBytesWithStartStopBits[FrameBytesWithStartStopBits.Length - 1] != byte.MaxValue;
                    }
                }
                return isStartingPartialFrame;
            }
        }
        public bool IsEndPartialFrame
        {
            get
            {
                bool isEndPartialFrame = false;
                if (!IsCompleteFrame)
                {
                    if (FrameBytesWithStartStopBits?.Length > 0)
                    {
                        isEndPartialFrame =
                            FrameBytesWithStartStopBits[FrameBytesWithStartStopBits.Length - 1] == byte.MaxValue && FrameBytesWithStartStopBits[0] != byte.MaxValue;
                    }
                }
                return isEndPartialFrame;
            }
        }
        public byte[] FrameBytesWithStartStopBits { get; set; }
    }
}
