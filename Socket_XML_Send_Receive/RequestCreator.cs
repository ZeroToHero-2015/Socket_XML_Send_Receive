using System;
using System.Net;
using System.Text;

namespace Socket_XML_Send_Receive
{
    public class RequestCreator
    {

        public byte[] GetByteArrayToSend(string message, Encoding encoding, bool shouldAddMessageLength)
        {
            var textBytes = ConvertStringToBytes(message, encoding);
            return CreateByteArrayToSend(textBytes, shouldAddMessageLength);
        }

        private byte[] CreateByteArrayToSend(byte[] textBytes, bool shouldAddMessageLength)
        {
            if (shouldAddMessageLength)
            {
                var requestPrefix = GetRequestPrefix(textBytes.Length);
                return ConcatenateArrays(requestPrefix, textBytes);
            }

            return textBytes;
        }

        private byte[] ConcatenateArrays(byte[] firstArray, byte[] secondArray)
        {
            var bigArray = new byte[firstArray.Length + secondArray.Length];
            firstArray.CopyTo(bigArray, 0);
            secondArray.CopyTo(bigArray, firstArray.Length);
            return bigArray;
        }

        private byte[] GetRequestPrefix(int byteArrayLength)
        {
            int lengthInNetworkOrder = IPAddress.HostToNetworkOrder(byteArrayLength);
            return BitConverter.GetBytes(lengthInNetworkOrder);
        }

        private byte[] ConvertStringToBytes(string text, Encoding encoding)
        {
            byte[] stringBytes = encoding.GetBytes(text);

            return stringBytes;
        }
    }
}