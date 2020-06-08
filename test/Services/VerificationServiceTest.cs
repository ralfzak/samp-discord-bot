using System;
using System.Collections.Generic;
using System.Linq;
using domain;
using domain.Models;
using domain.Repo;
using main.Services;
using Moq;
using Xunit;

namespace test.Services
{
    public class VerificationServiceTest
    {
        [Fact]
        public void Test_GetUserIDsFromForumInfo_WithValidVerifications_ReturnsCorrectUserIds()
        {
            var repoMock = new Mock<IVerificationsRepository>();
            var subject = Subject(repoMock, MockHttpClient());
            var verifications = new List<Verifications>
            {
                new Verifications
                {
                    Userid = 1,
                    ForumId = 1,
                    ForumName = "any",
                    VerifiedBy = "any",
                    VerifiedOn = DateTime.UtcNow,
                    DeletedOn = null
                }
            };
            repoMock.Setup(m => m.FindByForumInfo(It.IsAny<string>())).Returns(verifications);

            var result = subject.GetUserIDsFromForumInfo("any");

            Assert.Contains(verifications.First().Userid, result);
        }
        
        [Fact]
        public void Test_GetUserIDsFromForumInfo_WithNoVerifications_ReturnsEmptyList()
        {
            var repoMock = new Mock<IVerificationsRepository>();
            var subject = Subject(repoMock, MockHttpClient());
            var verifications = new List<Verifications>();
            repoMock.Setup(m => m.FindByForumInfo(It.IsAny<string>())).Returns(verifications);

            var result = subject.GetUserIDsFromForumInfo("any");

            Assert.Empty(result);
        }
        
        [Fact]
        public void Test_GetUserForumProfileId_WithValidVerification_ReturnsCorrectForumIdAndForumName()
        {
            var repoMock = new Mock<IVerificationsRepository>();
            var subject = Subject(repoMock, MockHttpClient());
            var verification = new Verifications
            {
                Userid = 1,
                ForumId = 1,
                ForumName = "any",
                VerifiedBy = "any",
                VerifiedOn = DateTime.UtcNow,
                DeletedOn = null
            };
            repoMock.Setup(m => m.FindByUserId(It.IsAny<ulong>())).Returns(verification);

            subject.GetUserForumProfileId(verification.Userid, out int forumId, out string forumName);

            Assert.Equal(verification.ForumId, forumId);
            Assert.Equal(verification.ForumName, forumName);
        }
        
        [Fact]
        public void Test_GetUserForumProfileId_WithNoVerification_ReturnsInvalidForumIdAndEmptyForumName()
        {
            var repoMock = new Mock<IVerificationsRepository>();
            var subject = Subject(repoMock, MockHttpClient());
            repoMock.Setup(m => m.FindByUserId(It.IsAny<ulong>())).Returns(null as Verifications);

            subject.GetUserForumProfileId(1, out int forumId, out string forumName);

            Assert.Equal(-1, forumId);
            Assert.Equal(string.Empty, forumName);
        }
        
        [Fact]
        public void Test_IsForumProfileLinked_WithValidVerification_ReturnsTrue()
        {
            var repoMock = new Mock<IVerificationsRepository>();
            var subject = Subject(repoMock, MockHttpClient());
            var verification = new Verifications
            {
                Userid = 1,
                ForumId = 1,
                ForumName = "any",
                VerifiedBy = "any",
                VerifiedOn = DateTime.UtcNow,
                DeletedOn = null
            };
            repoMock.Setup(m => m.FindByForumId(It.IsAny<int>())).Returns(verification);

            var result = subject.IsForumProfileLinked(verification.ForumId ?? 0);

            Assert.True(result);
        }
        
        [Fact]
        public void Test_IsForumProfileLinked_WithNoVerification_ReturnsFalse()
        {
            var repoMock = new Mock<IVerificationsRepository>();
            var subject = Subject(repoMock, MockHttpClient());
            repoMock.Setup(m => m.FindByForumId(It.IsAny<int>())).Returns(null as Verifications);

            var result = subject.IsForumProfileLinked(1);

            Assert.False(result);
        }
        
        [Fact]
        public void Test_IsUserVerified_WithValidVerification_ReturnsTrue()
        {
            var repoMock = new Mock<IVerificationsRepository>();
            var subject = Subject(repoMock, MockHttpClient());
            var verification = new Verifications
            {
                Userid = 1,
                ForumId = 1,
                ForumName = "any",
                VerifiedBy = "any",
                VerifiedOn = DateTime.UtcNow,
                DeletedOn = null
            };
            repoMock.Setup(m => m.FindByUserId(It.IsAny<ulong>())).Returns(verification);

            var result = subject.IsUserVerified(verification.Userid);

            Assert.True(result);
        }
        
        [Fact]
        public void Test_IsUserVerified_WithNoVerification_ReturnsFalse()
        {
            var repoMock = new Mock<IVerificationsRepository>();
            var subject = Subject(repoMock, MockHttpClient());
            repoMock.Setup(m => m.FindByUserId(It.IsAny<ulong>())).Returns(null as Verifications);

            var result = subject.IsUserVerified(1);

            Assert.False(result);
        }
        
        [Fact]
        public void Test_StoreUserVerification_WithData_StoresCorrectValidation()
        {
            var repoMock = new Mock<IVerificationsRepository>();
            var subject = Subject(repoMock, MockHttpClient());
            var verification = new Verifications
            {
                Userid = 1,
                ForumId = 1,
                ForumName = "any",
                VerifiedBy = "any"
            };

            subject.StoreUserVerification(
                verification.Userid,
                verification.ForumId ?? 1, 
                verification.ForumName, 
                verification.VerifiedBy
                );

            repoMock.Verify(m => m.Create(
                It.Is<Verifications>(v => 
                    v.Userid == verification.Userid &&
                    v.ForumId == verification.ForumId &&
                    v.ForumName == verification.ForumName &&
                    v.VerifiedBy == verification.VerifiedBy)
                ), Times.Once);
        }
        
        [Fact]
        public void Test_DeleteUserVerification_WithGivenUserId_DeletesCorrectUserId()
        {
            var repoMock = new Mock<IVerificationsRepository>();
            var subject = Subject(repoMock, MockHttpClient());
            ulong userId = 1;
            
            subject.DeleteUserVerification(userId);

            repoMock.Verify(m => m.DeleteByUserId(userId), Times.Once);
        }
        
        [Fact]
        public void Test_GetForumProfileNameIfContainsToken_WithProfileContainingToken_ReturnsCorrectForumName()
        {
            var profileName = "profileName";
            var profileContent = 
                $"any<title>SA-MP Forums - View Profile: {profileName}</title>anyTOKENany";
            var subject = Subject(new Mock<IVerificationsRepository>(), MockHttpClient(profileContent));
            ulong userId = 1;
            
            var result = subject.GetForumProfileNameIfContainsToken(1, "TOKEN");
            
            Assert.Equal(profileName, result);
        }
        
        [Fact]
        public void Test_GetForumProfileNameIfContainsToken_WithProfileMissingToken_ReturnsEmptyForumName()
        {
            var profileName = "profileName";
            var profileContent = 
                $"any<title>SA-MP Forums - View Profile: {profileName}</title>any";
            var subject = Subject(new Mock<IVerificationsRepository>(), MockHttpClient(profileContent));
            ulong userId = 1;
            
            var result = subject.GetForumProfileNameIfContainsToken(1, "TOKEN");
            
            Assert.Empty(result);
        }
        
        [Fact]
        public void Test_GetForumProfileName_WithValidProfileContent_ReturnsCorrectForumName()
        {
            var profileName = "profileName";
            var profileContent = 
                $"any<title>SA-MP Forums - View Profile: {profileName}</title>any";
            var subject = Subject(new Mock<IVerificationsRepository>(), MockHttpClient(profileContent));
            
            var result = subject.GetForumProfileName(1);
            
            Assert.Equal(profileName, result);
        }
        
        [Fact]
        public void Test_GetForumProfileName_WithInvalidProfileContent_ReturnsEmptyForumName()
        {
            var profileContent = "any";
            var subject = Subject(new Mock<IVerificationsRepository>(), MockHttpClient(profileContent));
            
            var result = subject.GetForumProfileName(1);
            
            Assert.Empty(result);
        }
        
        private VerificationService Subject(Mock<IVerificationsRepository> repoMock, Mock<IHttpClient> httpMock) =>
            new VerificationService(repoMock.Object, httpMock.Object);

        private Mock<IHttpClient> MockHttpClient(string content = "")
        {
            var subject = new Mock<IHttpClient>();
            subject.Setup(s => s.GetContent(It.IsAny<string>())).Returns(content);
            return subject;
        }
    }
}