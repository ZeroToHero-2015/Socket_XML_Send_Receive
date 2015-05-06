using System;
using System.Net;
using System.Text;

namespace Socket_XML_Send_Receive
{
    public class BufferCreator
    {
        public byte[] GetBufferToSendFromString(string inputMessage, Encoding encoding, bool shouldAddLengthToMessage)
        {
            var messageArray = ConvertStringToByteArrayUsingEncoding(inputMessage, encoding);

            var bufferToSend = GetBufferToSend(messageArray, shouldAddLengthToMessage);
            return bufferToSend;
        }

        private byte[] GetBufferToSend(byte[] messageArray, bool shouldAddLengthToMessage)
        {
            if (shouldAddLengthToMessage)
            {
                int bufferLengthInNetworkOrder = IPAddress.HostToNetworkOrder(messageArray.Length);
                byte[] reqLenArray = BitConverter.GetBytes(bufferLengthInNetworkOrder);
                return ConcatenateArrays(reqLenArray, messageArray);
            }
            return messageArray;
        }

        private byte[] ConcatenateArrays(byte[] firstArray, byte[] secondArray)
        {
            var resultArray = new byte[firstArray.Length + secondArray.Length];
            firstArray.CopyTo(resultArray, 0);
            secondArray.CopyTo(resultArray, firstArray.Length);
            return resultArray;
        }

        private byte[] ConvertStringToByteArrayUsingEncoding(string inputMessage, Encoding encoding)
        {
            return encoding.GetBytes(inputMessage);
        }
    }
}