using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Socket_XML_Send_Receive
{
    public static class StringConverter
    {
        public static byte[] GetBytesToSend(string encoding
                                    , string content
                                    , bool shouldAddLengthPrefix)
        {
            byte[] bytesToSend = GetContentBytes(encoding, content);
            if (shouldAddLengthPrefix)
            {
                int reqLen = content.Length;
                int reqLenH2N = IPAddress.HostToNetworkOrder(reqLen * 2);
                byte[] reqLenArray = BitConverter.GetBytes(reqLenH2N);

                byte[] buff_intermediar = new byte[reqLen * 2 + 4];
                reqLenArray.CopyTo(buff_intermediar, 0);
                bytesToSend.CopyTo(buff_intermediar, 4);
                bytesToSend = buff_intermediar;
            }

            return bytesToSend;
        }

        private static byte[] GetContentBytes(string enconding, string content)
        {
            byte[] contentBytes = null;
            switch (enconding)
            {
                case "ASCII":
                    contentBytes = Encoding.ASCII.GetBytes(content);
                    break;
                case "UTF7":
                    contentBytes = Encoding.UTF7.GetBytes(content);
                    break;
                case "UTF8":
                    contentBytes = Encoding.UTF8.GetBytes(content);
                    break;
                case "Unicode":
                    contentBytes = Encoding.Unicode.GetBytes(content);
                    break;
                default:
                    //
                    break;
            };
            return contentBytes;
        }
    }
}
