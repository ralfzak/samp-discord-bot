using main.Core;
using main.Services;
using Moq;
using Xunit;

namespace test.Services
{
    public class StatsServiceTest
    {
        [Fact]
        public void Test_GetSampPlayerServerCount_WithBadPage_ReturnsZeros()
        {
            var subject = Subject(MockHttpClient("bad page"));

            var result = subject.GetSampPlayerServerCount();
            
            Assert.Equal((0, 0), result);
        }
        
        [Fact]
        public void Test_GetSampPlayerServerCount_WithCorrectPlayersCountAndBadServersCount_ReturnsCorrectValueForPlayersAndZeroForServers()
        {
            var html =
                "anything" +
                "<td>" +
                "<font size=\"2\">Players Online: </font><font size=\"2\" color=\"#BBBBBB\"><b>100</b></font>" +
                "</td>" +
                "anything";
            var subject = Subject(MockHttpClient(html));

            var result = subject.GetSampPlayerServerCount();
            
            Assert.Equal((100, 0), result);
        }
        
        [Fact]
        public void Test_GetSampPlayerServerCount_WithCorrectServersCountAndBadPlayersCount_ReturnsCorrectValueForServersAndZeroForPlayers()
        {
            var html =
                "anything" +
                "<td>" +
                "<font size=\"2\">Servers Online: </font><font size=\"2\" color=\"#BBBBBB\"><b>100</b></font>" +
                "</td>" +
                "anything";
            var subject = Subject(MockHttpClient(html));

            var result = subject.GetSampPlayerServerCount();
            
            Assert.Equal((0, 100), result);
        }
        
        [Fact]
        public void Test_GetSampPlayerServerCount_WithCorrectCounts_ReturnsCorrectValues()
        {
            var html =
                "anything" +
                "<td>" +
                "<font size=\"2\">Players Online: </font><font size=\"2\" color=\"#BBBBBB\"><b>100</b></font>" +
                "</td>" +
                "anything" +
                "<td>" +
                "<font size=\"2\">Servers Online: </font><font size=\"2\" color=\"#BBBBBB\"><b>200</b></font>" +
                "</td>" +
                "anything";
            var subject = Subject(MockHttpClient(html));

            var result = subject.GetSampPlayerServerCount();
            
            Assert.Equal((100, 200), result);
        }

        private StatsService Subject(Mock<IHttpClient> httpMock) =>
            new StatsService(httpMock.Object);

        private Mock<IHttpClient> MockHttpClient(string content)
        {
            var subject = new Mock<IHttpClient>();
            subject.Setup(s => s.GetContent(It.IsAny<string>())).Returns(content);
            return subject;
        }
    }
}
