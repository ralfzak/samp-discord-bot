using main.Core;
using main.Services;
using Moq;
using System;
using Xunit;

namespace test.Services
{
    public class UserServiceTest
    {
        [Fact]
        public void Test_SetUserCooldown_WithGivenCommandAndCooldown_SetsCorrectUserCooldown()
        {
            var subject = GetFakeTimeStubbedSubject(10);
            var command = "testCommand";
            ulong userId = 1;
            
            subject.SetUserCooldown(userId, command, 60);

            var result = subject.IsUserOnCooldown(userId, command);
            Assert.True(result);
        }
        
        [Fact]
        public void Test_SetUserCooldown_WithGivenCommandAndCooldown_DoesNotSetWrongUserCooldown()
        {
            var subject = GetFakeTimeStubbedSubject(10);
            var command = "testCommand";
            ulong userId = 1;
            
            subject.SetUserCooldown(userId, command, 60);

            var result = subject.IsUserOnCooldown(2, command);
            Assert.False(result);
        }
        
        [Fact]
        public void Test_IsUserOnCooldown_WithGivenCommandAndCooldownAndExpiredTime_ReturnsFalse()
        {
            var subject = GetFakeTimeStubbedSubject(0);
            var command = "testCommand";
            ulong userId = 1;
            subject.SetUserCooldown(userId, command, 60);

            var result = subject.IsUserOnCooldown(userId, command);
            
            Assert.False(result);
        }
        
        [Fact]
        public void Test_IsUserOnCooldown_WithGivenCommandAndCooldownWithValidTime_ReturnsTrue()
        {
            var subject = GetFakeTimeStubbedSubject(10);
            var command = "testCommand";
            ulong userId = 1;
            subject.SetUserCooldown(userId, command, 60);

            var result = subject.IsUserOnCooldown(userId, command);
            
            Assert.True(result);
        }
        
        private UserService GetFakeTimeStubbedSubject(int elapsedTime)
        {
            var subject = new Mock<ITimeProvider>();
            subject.Setup(mk => mk.GetElapsedFromEpoch(It.IsAny<long>())).Returns(elapsedTime);

            return new UserService(subject.Object);
        }
    }
}
 