using main.Core;
using main.Services;
using Moq;
using System;
using Xunit;

namespace test.Services
{
    public class MessageServiceTest
    {
        [Fact]
        public void Test_LogCommand_WithGivenCommandAndResponse_AddsCorrectEntry()
        {
            var (subject, timeMock) = GetFakeTimeStubbedSubject(DateTime.UtcNow);

            subject.LogCommand(1, 1);

            var result = subject.GetResponseFromCommandLogEntry(1);
            Assert.Equal<ulong>(1, result);
        }

        [Fact]
        public void Test_GetResponseFromCommandLogEntry_WithExistingCommandAndResponse_RetrievesCorrectEntry()
        {
            var (subject, timeMock) = GetFakeTimeStubbedSubject(DateTime.UtcNow);

            subject.LogCommand(1, 1);
            subject.LogCommand(2, 2);

            var result = subject.GetResponseFromCommandLogEntry(1);
            Assert.Equal<ulong>(1, result);
        }   
        
        [Fact]
        public void Test_GetResponseFromCommandLogEntry_WhenFetched_RemovesFetchedEntry()
        {
            var (subject, timeMock) = GetFakeTimeStubbedSubject(DateTime.UtcNow);

            subject.LogCommand(1, 1);

            var result = subject.GetResponseFromCommandLogEntry(1);
            Assert.Equal<ulong>(1, result);

            var poppedResult = subject.GetResponseFromCommandLogEntry(1);
            Assert.Equal<ulong>(0, poppedResult);
        }      
        
        [Fact]
        public void Test_GetResponseFromCommandLogEntry_WithNoCommandAndResponse_ReturnsZero()
        {
            var (subject, timeMock) = GetFakeTimeStubbedSubject(DateTime.UtcNow);

            var result = subject.GetResponseFromCommandLogEntry(1);
            Assert.Equal<ulong>(0, result);
        }
        
        [Fact]
        public void Test_DropCommandLogEntry_WithEntriesInLessThanFiveHours_DoesNotDropEntries()
        {
            var (subject, timeMock) = GetFakeTimeStubbedSubject(DateTime.UtcNow);
            subject.LogCommand(1, 1);

            subject.DropCommandLogEntry();

            var result = subject.GetResponseFromCommandLogEntry(1);
            Assert.Equal<ulong>(1, result);
        }
        
        [Fact]
        public void Test_DropCommandLogEntry_WithEntriesMoreThanFiveHours_DoesNotDropEntries()
        {
            var (subject, timeMock) = GetFakeTimeStubbedSubject(DateTime.UtcNow);
            subject.LogCommand(1, 1);

            timeMock.SetupGet(mk => mk.UtcNow).Returns(DateTime.UtcNow.AddHours(6));

            subject.DropCommandLogEntry();

            var result = subject.GetResponseFromCommandLogEntry(1);
            Assert.Equal<ulong>(0, result);
        }

        private (MessageService, Mock<ITimeProvider>) GetFakeTimeStubbedSubject(DateTime dateTime)
        {
            var subject = new Mock<ITimeProvider>();
            subject.Setup(mk => mk.UtcNow).Returns(dateTime);

            return ( new MessageService(subject.Object), subject );
        }
    }
}
