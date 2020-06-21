using System;
using System.Collections.Generic;
using System.Linq;
using main.Core;
using main.Core.Database;
using main.Core.Domain.Models;
using Moq;
using Xunit;

namespace test.Core.Database
{
    public class MysqlVerificationsRepositoryTest: FakeDatabaseContext<Verifications>
    {
        [Fact]
        public void Test_Create_CreatesCorrectRecord()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var verification = new Verifications
                {
                    Userid = 1,
                    ForumId = 1,
                    ForumName = "any",
                    VerifiedBy = "any",
                    VerifiedOn = DateTime.UtcNow,
                    DeletedOn = null
                };
                var subject = SubjectWithData(context, now);

                subject.Create(verification);

                var result = GetAll(context);
                Assert.Equal(result.First().Userid, verification.Userid);
            }
        }
        
        [Fact]
        public void Test_DeleteByUserId_WithExistingRecords_DoesNotDeleteWrongRecord()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var verification = new Verifications
                {
                    Userid = 1,
                    ForumId = 1,
                    ForumName = "any1",
                    VerifiedBy = "any1",
                    VerifiedOn = DateTime.UtcNow,
                    DeletedOn = null
                };
                var noise = new Verifications
                {
                    Userid = 2,
                    ForumId = 2,
                    ForumName = "any2",
                    VerifiedBy = "any2",
                    VerifiedOn = DateTime.UtcNow,
                    DeletedOn = null
                };
                var subject = SubjectWithData(context, now, new List<Verifications>{ verification, noise });

                subject.DeleteByUserId(verification.Userid);

                var result = GetAll(context).Select(v => v.Userid);
                Assert.Contains(noise.Userid, result);
            }
        }
        
        [Fact]
        public void Test_DeleteByUserId_WithExistingRecords_DeletesCorrectRecord()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var verification = new Verifications
                {
                    Userid = 1,
                    ForumId = 1,
                    ForumName = "any1",
                    VerifiedBy = "any1",
                    VerifiedOn = DateTime.UtcNow,
                    DeletedOn = null
                };
                var noise = new Verifications
                {
                    Userid = 2,
                    ForumId = 2,
                    ForumName = "any2",
                    VerifiedBy = "any2",
                    VerifiedOn = DateTime.UtcNow,
                    DeletedOn = null
                };
                var subject = SubjectWithData(context, now, new List<Verifications>{ verification, noise });
                
                subject.DeleteByUserId(verification.Userid);

                var result = GetAll(context).Select(v => v.Userid);
                Assert.DoesNotContain(verification.Userid, result);
            }
        }

        [Fact]
        void Test_DeleteByUserId_WithNoExistingMatchingRecords_SilentlyIgnores()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var subject = SubjectWithData(context, now);
                
                subject.DeleteByUserId(1);

                var result = GetAll(context);
                Assert.Empty(result);
            }
        }

        [Fact]
        public void Test_FindByForumInfo_WithNoRecords_ReturnsEmptyList()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var subject = SubjectWithData(context, now);
                
                subject.FindByForumInfo("any");

                var result = GetAll(context);
                Assert.Empty(result);
            }
        }

        [Fact]
        public void Test_FindByForumInfo_WithExistingMatchingRecord_ReturnsCorrectRecordBForumId()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var verification = new Verifications
                {
                    Userid = 1,
                    ForumId = 1,
                    ForumName = "any1",
                    VerifiedBy = "any1",
                    VerifiedOn = DateTime.UtcNow,
                    DeletedOn = null
                };
                var noise = new Verifications
                {
                    Userid = 2,
                    ForumId = 2,
                    ForumName = "any2",
                    VerifiedBy = "any2",
                    VerifiedOn = DateTime.UtcNow,
                    DeletedOn = null
                };
                var subject = SubjectWithData(context, now, new List<Verifications>{ verification, noise });
                
                var result  = subject.FindByForumInfo(verification.ForumId.ToString());

                Assert.Single(result);
                Assert.Equal(verification.Userid, result.First().Userid);
            }
        }

        [Fact]
        public void Test_FindByForumInfo_WithExistingMatchingRecord_ReturnsCorrectRecordByForumName()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var verification = new Verifications
                {
                    Userid = 1,
                    ForumId = 1,
                    ForumName = "any1",
                    VerifiedBy = "any1",
                    VerifiedOn = DateTime.UtcNow,
                    DeletedOn = null
                };
                var noise = new Verifications
                {
                    Userid = 2,
                    ForumId = 2,
                    ForumName = "any2",
                    VerifiedBy = "any2",
                    VerifiedOn = DateTime.UtcNow,
                    DeletedOn = null
                };
                var subject = SubjectWithData(context, now, new List<Verifications>{ verification, noise });
                
                var result  = subject.FindByForumInfo(verification.ForumName);

                Assert.Single(result);
                Assert.Equal(verification.Userid, result.First().Userid);
            }
        }

        [Fact]
        public void Test_FindByForumId_WithNoExistingRecords_ReturnsNull()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var subject = SubjectWithData(context, now);
                
                var result  = subject.FindByForumId(1);

                Assert.Null(result);
            }
        }

        [Fact]
        public void Test_FindByForumId_WithExistingMatchingRecord_ReturnsRecord()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var verification = new Verifications
                {
                    Userid = 1,
                    ForumId = 1,
                    ForumName = "any1",
                    VerifiedBy = "any1",
                    VerifiedOn = DateTime.UtcNow,
                    DeletedOn = null
                };
                var subject = SubjectWithData(context, now, new List<Verifications>{ verification });
                
                var result  = subject.FindByForumId(verification.ForumId.Value);

                Assert.Equal(verification.ForumId, result.ForumId);
            }
        }

        [Fact]
        public void Test_FindByForumId_WithNoExistingMatchingRecord_ReturnsNull()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var noise = new Verifications
                {
                    Userid = 1,
                    ForumId = 1,
                    ForumName = "any1",
                    VerifiedBy = "any1",
                    VerifiedOn = DateTime.UtcNow,
                    DeletedOn = null
                };
                var subject = SubjectWithData(context, now, new List<Verifications>{ noise });
                
                var result  = subject.FindByForumId(0);

                Assert.Null(result);
            }
        }
        
        [Fact]
        public void Test_FindByForumId_WithExistingRecords_ReturnsOnlyMatchingRecord()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var verification = new Verifications
                {
                    Userid = 1,
                    ForumId = 1,
                    ForumName = "any1",
                    VerifiedBy = "any1",
                    VerifiedOn = DateTime.UtcNow,
                    DeletedOn = null
                };
                var noise = new Verifications
                {
                    Userid = 2,
                    ForumId = 2,
                    ForumName = "any2",
                    VerifiedBy = "any2",
                    VerifiedOn = DateTime.UtcNow,
                    DeletedOn = null
                };
                var subject = SubjectWithData(context, now, new List<Verifications>{ verification, noise });
                
                var result  = subject.FindByForumId(verification.ForumId.Value);

                Assert.Equal(verification.ForumId, result.ForumId);
            }
        }

        [Fact]
        public void Test_FindByUserId_WithNoExistingRecords_ReturnsNull()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var subject = SubjectWithData(context, now);
                
                var result  = subject.FindByUserId(1);

                Assert.Null(result);
            }
        }

        [Fact]
        public void Test_FindByUserId_WithExistingMatchingRecord_ReturnsRecord()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var verification = new Verifications
                {
                    Userid = 1,
                    ForumId = 1,
                    ForumName = "any1",
                    VerifiedBy = "any1",
                    VerifiedOn = DateTime.UtcNow,
                    DeletedOn = null
                };
                var subject = SubjectWithData(context, now, new List<Verifications>{ verification });
                
                var result  = subject.FindByUserId(verification.Userid);

                Assert.Equal(verification.Userid, result.Userid);
            }
        }

        [Fact]
        public void Test_FindByUserId_WithNoExistingMatchingRecord_ReturnsNull()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var noise = new Verifications
                {
                    Userid = 1,
                    ForumId = 1,
                    ForumName = "any1",
                    VerifiedBy = "any1",
                    VerifiedOn = DateTime.UtcNow,
                    DeletedOn = null
                };
                var subject = SubjectWithData(context, now, new List<Verifications>{ noise });
                
                var result  = subject.FindByUserId(0);

                Assert.Null(result);
            }
        }
        
        [Fact]
        public void Test_FindByUserId_WithExistingRecords_ReturnsOnlyMatchingRecord()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var verification = new Verifications
                {
                    Userid = 1,
                    ForumId = 1,
                    ForumName = "any1",
                    VerifiedBy = "any1",
                    VerifiedOn = DateTime.UtcNow,
                    DeletedOn = null
                };
                var noise = new Verifications
                {
                    Userid = 2,
                    ForumId = 2,
                    ForumName = "any2",
                    VerifiedBy = "any2",
                    VerifiedOn = DateTime.UtcNow,
                    DeletedOn = null
                };
                var subject = SubjectWithData(context, now, new List<Verifications>{ verification, noise });
                
                var result  = subject.FindByUserId(verification.Userid);

                Assert.Equal(verification.Userid, result.Userid);
            }
        }

        private MysqlVerificationsRepository SubjectWithData(
            DatabaseContext context, 
            DateTime time, 
            List<Verifications> verifications = null
            )
        {
            Provision(context, verifications);
            return new MysqlVerificationsRepository(context, MockTime(time).Object);
        }

        private Mock<ITimeProvider> MockTime(DateTime time)
        {
            var subject = new Mock<ITimeProvider>();
            subject.Setup(s => s.UtcNow).Returns(time);
            return subject;
        }
    }
}
