using System;
using main.Core;
using main.Exceptions;
using main.Services;
using Moq;
using Xunit;

namespace test.Services
{
    public class SampServerServiceTest
    {
        [Theory]
        [InlineData("127.0.0.1:7777:7777", "Wrong format")]
        [InlineData("127.0.0.1:port", "Invalid port")]
        [InlineData("127.0.0.1:", "Invalid port")]
        [InlineData("-", "Invalid IP")]
        [InlineData("7777", "Invalid IP")]
        [InlineData("any.host.name", "Failed to find DNS entry")]
        [InlineData("any.host.com", "Failed to find DNS entry")]
        [InlineData("ip:7777", "Failed to find DNS entry")]
        public void Test_ParseIpPort_WithInvalidIpPort_ThrowsExceptionWithCorrectMessage(string ipPort, string message)
        {
            var subject = Subject(MockHttpClient(""));

            var exception = Assert.Throws<InvalidIpParseException>(() => subject.ParseIpPort(ipPort));
            Assert.Equal($"Unable to parse Ip address: {message}", exception.Message);
        }
        
        [Theory]
        [InlineData("127.0.0.1:7777", "127.0.0.1", 7777)]
        [InlineData("127.0.0.1", "127.0.0.1", 7777)]
        [InlineData("127.0.0.1:8888", "127.0.0.1", 8888)]
        [InlineData("127.0.0.1:9999", "127.0.0.1", 9999)]
        public void Test_ParseIpPort_WithValidIpPort_ParsesIpAndPort(string ipPort, string ip, ushort port)
        {
            var subject = Subject(MockHttpClient(""));

            var result = subject.ParseIpPort(ipPort);
            
            Assert.Equal(ip, result.ip);
            Assert.Equal(port, result.port);
        }
        
        private SampServerService Subject(Mock<IHttpClient> httpMock) =>
            new SampServerService(httpMock.Object);

        private Mock<IHttpClient> MockHttpClient(string content)
        {
            var subject = new Mock<IHttpClient>();
            subject.Setup(s => s.GetContent(It.IsAny<string>())).Returns(content);
            return subject;
        }
    }
}
