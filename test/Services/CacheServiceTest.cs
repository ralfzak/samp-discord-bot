using main.Core.Domain;
using main.Services;
using Xunit;

namespace test.Services
{
    public class CacheServiceTest
    {
        [Fact]
        public void Test_GetUserForumId_WithDifferentCacheEntries_ReturnsCorrectForumId()
        {
            var subject = Subject();
            subject.SetUserForumId(1, 1);
            subject.SetUserForumId(2, 2);

            var result = subject.GetUserForumId(1);
            
            Assert.Equal(1, result);
        }        
        
        [Fact]
        public void Test_GetUserForumId_WithNoCacheEntries_ReturnsMinusOne()
        {
            var subject = Subject();
            subject.SetUserForumId(1, 1);

            var result = subject.GetUserForumId(2);
            
            Assert.Equal(-1, result);
        }        
        
        [Fact]
        public void Test_SetUserForumId_WithNoCacheEntries_AddsACacheValue()
        {
            var subject = Subject();
            subject.SetUserForumId(1, 1);

            var result = subject.GetUserForumId(1);
            
            Assert.Equal(1, result);
        }        
        
        [Fact]
        public void Test_SetUserForumId_WithExistingCacheEntry_UpdatesCacheValue()
        {
            var subject = Subject();
            subject.SetUserForumId(1, 1);
            subject.SetUserForumId(1, 2);

            var result = subject.GetUserForumId(1);
            
            Assert.Equal(2, result);
        }
        
        [Fact]
        public void Test_GetUserVerificationState_WithDifferentCacheEntries_ReturnsCorrectState()
        {
            var subject = Subject();
            subject.SetUserVerificationState(1, VerificationStates.WaitingConfirm);
            subject.SetUserVerificationState(2, VerificationStates.WaitingConfirm);

            var result = subject.GetUserVerificationState(1);
            
            Assert.Equal(VerificationStates.WaitingConfirm, result);
        }        
        
        [Fact]
        public void Test_GetUserVerificationState_WithNoCacheEntries_ReturnsStateNone()
        {
            var subject = Subject();
            subject.SetUserVerificationState(1, VerificationStates.WaitingConfirm);

            var result = subject.GetUserVerificationState(2);
            
            Assert.Equal(VerificationStates.None, result);
        }        
        
        [Fact]
        public void Test_SetUserVerificationState_WithNoCacheEntries_AddsACacheValue()
        {
            var subject = Subject();
            subject.SetUserVerificationState(1, VerificationStates.WaitingConfirm);

            var result = subject.GetUserVerificationState(1);
            
            Assert.Equal(VerificationStates.WaitingConfirm, result);
        }        
        
        [Fact]
        public void Test_SetUserVerificationState_WithExistingCacheEntry_UpdatesCacheValue()
        {
            var subject = Subject();
            subject.SetUserVerificationState(1, VerificationStates.None);
            subject.SetUserVerificationState(1, VerificationStates.WaitingConfirm);

            var result = subject.GetUserVerificationState(1);
            
            Assert.Equal(VerificationStates.WaitingConfirm, result);
        }
        
        [Fact]
        public void Test_GetUserToken_WithDifferentCacheEntries_ReturnsCorrectToken()
        {
            var subject = Subject();
            subject.SetUserToken(1, "1");
            subject.SetUserToken(2, "2");

            var result = subject.GetUserToken(1);
            
            Assert.Equal("1", result);
        }        
        
        [Fact]
        public void Test_GetUserToken_WithNoCacheEntries_ReturnsEmptyString()
        {
            var subject = Subject();
            subject.SetUserToken(1, "1");

            var result = subject.GetUserToken(2);
            
            Assert.Equal("", result);
        }        
        
        [Fact]
        public void Test_SetUserToken_WithNoCacheEntries_AddsACacheValue()
        {
            var subject = Subject();
            subject.SetUserToken(1, "1");

            var result = subject.GetUserToken(1);
            
            Assert.Equal("1", result);
        }        
        
        [Fact]
        public void Test_SetUserToken_WithExistingCacheEntry_UpdatesCacheValue()
        {
            var subject = Subject();
            subject.SetUserToken(1, "1");
            subject.SetUserToken(1, "2");

            var result = subject.GetUserToken(1);
            
            Assert.Equal("2", result);
        }

        [Fact]
        public void Test_ClearCache_WithGivenUserId_ClearsAllCacheOnlyForUserId()
        {
            var subject = Subject();
            subject.SetUserForumId(1, 1);
            subject.SetUserToken(1, "1");
            subject.SetUserVerificationState(1, VerificationStates.WaitingConfirm);
            subject.SetUserForumId(2, 2);
            subject.SetUserToken(2, "2");
            subject.SetUserVerificationState(2, VerificationStates.WaitingConfirm);
            
            subject.ClearCache(1);
            
            Assert.Equal(-1, subject.GetUserForumId(1));
            Assert.Equal("", subject.GetUserToken(1));            
            Assert.Equal(VerificationStates.None, subject.GetUserVerificationState(1));            
            Assert.Equal(2, subject.GetUserForumId(2));
            Assert.Equal("2", subject.GetUserToken(2));
            Assert.Equal(VerificationStates.WaitingConfirm, subject.GetUserVerificationState(2));
        }
        
        private CacheService Subject()
        {
            return new CacheService();
        }
    }
}
