using System;
using domain.Models;
using domain.Repo;
using main.Core;
using main.Services;
using Moq;
using Xunit;

namespace test.Services
{
    public class BanningServiceTest
    {
        [Fact]
        public void Test_StoreBan_WithNoSecondsToExpire_InsertsNullExpiresOn()
        {
            var repoMock = new Mock<IBansRepository>();
            var time = new DateTime(2020, 1, 1);
            var timeMock = MockTime(time);
            var subject = Subject(repoMock, timeMock);
            var ban = new Bans
            {
                Id = 1,
                Userid = 1,
                Name = "any",
                ByUserid = 1,
                ByName = "any",
                ExpiresOn = null,
                Reason = "any",
                BannedOn = time
            };
            
            subject.StoreBan(ban.Userid, ban.Name, ban.ByUserid, ban.ByName, 0, ban.Reason);
            
            repoMock.Verify(s => s.Create(
                It.Is<Bans>(b => b.ExpiresOn == ban.ExpiresOn)
                ), Times.Once);
        }
        
        [Fact]
        public void Test_StoreBan_WithSecondsToExpire_InsertsCorrectExpiresOn()
        {
            var repoMock = new Mock<IBansRepository>();
            var time = new DateTime(2020, 1, 1);
            var timeMock = MockTime(time);
            var subject = Subject(repoMock, timeMock);
            var secondsToAdd = 60;
            var ban = new Bans
            {
                Id = 1,
                Userid = 1,
                Name = "any",
                ByUserid = 1,
                ByName = "any",
                ExpiresOn = time.AddSeconds(secondsToAdd),
                Reason = "any",
                BannedOn = time
            };
            
            subject.StoreBan(ban.Userid, ban.Name, ban.ByUserid, ban.ByName, secondsToAdd, ban.Reason);
            
            repoMock.Verify(s => s.Create(
                It.Is<Bans>(b => b.ExpiresOn == ban.ExpiresOn)
                ), Times.Once);
        }
        
        [Fact]
        public void Test_RemoveBan_WithUserId_CallsRepoMethodWithCorrectUserId()
        {
            var repoMock = new Mock<IBansRepository>();
            var time = new DateTime(2020, 1, 1);
            var timeMock = MockTime(time);
            var subject = Subject(repoMock, timeMock);
            ulong userId = 1;
            
            subject.RemoveBan(userId);
            
            repoMock.Verify(s => s.DeleteByUserId(userId), Times.Once);
        }
        
        [Fact]
        public void Test_GetBans_WithSearchCriteria_CallsRepoMethodWithCorrectCriteria()
        {
            var repoMock = new Mock<IBansRepository>();
            var time = new DateTime(2020, 1, 1);
            var timeMock = MockTime(time);
            var subject = Subject(repoMock, timeMock);
            var search = "any";
            
            subject.GetBans(search);
            
            repoMock.Verify(s => s.GetBans(search), Times.Once);
        }
        
        [Fact]
        public void Test_GetExpiredBans_CallsRepoMethod()
        {
            var repoMock = new Mock<IBansRepository>();
            var time = new DateTime(2020, 1, 1);
            var timeMock = MockTime(time);
            var subject = Subject(repoMock, timeMock);
            var search = "any";
            
            subject.GetBans(search);
            
            repoMock.Verify(s => s.GetBans(search), Times.Once);
        }
        
        private BanningService Subject(Mock<IBansRepository> repoMock, Mock<ITimeProvider> timeMock) =>
            new BanningService(timeMock.Object, repoMock.Object);

        private Mock<ITimeProvider> MockTime(DateTime time)
        {
            var subject = new Mock<ITimeProvider>();
            subject.Setup(s => s.UtcNow).Returns(time);
            return subject;
        }
    }
}