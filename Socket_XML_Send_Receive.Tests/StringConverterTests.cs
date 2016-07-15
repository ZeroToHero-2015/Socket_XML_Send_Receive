using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Socket_XML_Send_Receive.Tests
{
    [TestFixture]
    public class StringConverterTests
    {
        [Test]
        public void ConvertsToASCIIWithoutPrefix()
        {
            var result = StringConverter.GetBytesToSend("ASCII","asds",false);
            byte[] array = { 97, 115, 100, 115 };
            Assert.That(result, Is.EqualTo(array));
        }
    }
}
