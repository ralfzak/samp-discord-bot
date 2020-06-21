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
    public class MysqlBansRepositoryTest: FakeDatabaseContext<Bans>
    {
        [Fact]
        public void Test_Create_CreatesCorrectRecord()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var ban = new Bans
                {
                    Id = 1,
                    Userid = 1,
                    Name = "any",
                    ByUserid = 1,
                    ByName = "any",
                    ExpiresOn = null,
                    Reason = "any",
                    BannedOn = now
                };
                var subject = SubjectWithData(context, now);

                subject.Create(ban);

                var result = GetAll(context);
                Assert.Equal(result.First().Id, ban.Id);
            }
        }
        
        [Fact]
        public void Test_DeleteByUserId_WithExistingRecords_DoesNotDeleteWrongRecord()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var ban = new Bans
                {
                    Id = 1,
                    Userid = 1,
                    Name = "any",
                    ByUserid = 1,
                    ByName = "any",
                    ExpiresOn = null,
                    Reason = "any",
                    BannedOn = now
                };
                var noise = new Bans
                {
                    Id = 2,
                    Userid = 2,
                    Name = "any",
                    ByUserid = 1,
                    ByName = "any",
                    ExpiresOn = null,
                    Reason = "any",
                    BannedOn = now
                };
                var subject = SubjectWithData(context, now, new List<Bans>{ ban, noise });

                subject.DeleteByUserId(ban.Userid);

                var result = GetAll(context).Select(b => b.Userid);
                Assert.Contains(noise.Userid, result);
            }
        }
        
        [Fact]
        public void Test_DeleteByUserId_WithExistingRecords_DeletesCorrectRecord()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var ban = new Bans
                {
                    Id = 1,
                    Userid = 1,
                    Name = "any",
                    ByUserid = 1,
                    ByName = "any",
                    ExpiresOn = null,
                    Reason = "any",
                    BannedOn = now
                };
                var noise = new Bans
                {
                    Id = 2,
                    Userid = 2,
                    Name = "any",
                    ByUserid = 1,
                    ByName = "any",
                    ExpiresOn = null,
                    Reason = "any",
                    BannedOn = now
                };
                var subject = SubjectWithData(context, now, new List<Bans>{ ban, noise });
                
                subject.DeleteByUserId(ban.Userid);

                var result = GetAll(context).Select(b => b.Userid);
                Assert.Contains(ban.Userid, result);
            }
        }

        [Fact]
        public void Test_DeleteByUserId_WithNoExistingMatchingRecords_SilentlyIgnores()
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
        public void Test_GetBans_WithNoRecords_ReturnsEmptyList()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var subject = SubjectWithData(context, now);
                
                subject.GetBans("any");

                var result = GetAll(context);
                Assert.Empty(result);
            }
        }

        [Fact]
        public void Test_GetBans_WithExistingMatchingRecord_ReturnsCorrectRecordByUserId()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var ban = new Bans
                {
                    Id = 1,
                    Userid = 1,
                    Name = "any",
                    ByUserid = 1,
                    ByName = "any",
                    ExpiresOn = null,
                    Reason = "any",
                    BannedOn = now
                };
                var noise = new Bans
                {
                    Id = 2,
                    Userid = 2,
                    Name = "any",
                    ByUserid = 1,
                    ByName = "any",
                    ExpiresOn = null,
                    Reason = "any",
                    BannedOn = now
                };
                var subject = SubjectWithData(context, now, new List<Bans>{ ban, noise });
                
                var result  = subject.GetBans(ban.Userid.ToString());

                Assert.Single(result);
                Assert.Equal(ban.Userid, result.First().Userid);
            }
        }

        [Fact]
        public void Test_GetBans_WithExistingMatchingRecord_ReturnsCorrectRecordByName()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var ban = new Bans
                {
                    Id = 1,
                    Userid = 1,
                    Name = "any1",
                    ByUserid = 1,
                    ByName = "any1",
                    ExpiresOn = null,
                    Reason = "any1",
                    BannedOn = now
                };
                var noise = new Bans
                {
                    Id = 2,
                    Userid = 2,
                    Name = "any2",
                    ByUserid = 1,
                    ByName = "any2",
                    ExpiresOn = null,
                    Reason = "any2",
                    BannedOn = now
                };
                var subject = SubjectWithData(context, now, new List<Bans>{ ban, noise });
                
                var result  = subject.GetBans(ban.Name);

                Assert.Single(result);
                Assert.Equal(ban.Userid, result.First().Userid);
            }
        }

        [Fact]
        public void Test_GetBans_WithExistingMatchingRecords_ReturnsAllRecordsByCriteria()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var ban = new Bans
                {
                    Id = 1,
                    Userid = 1,
                    Name = "any",
                    ByUserid = 1,
                    ByName = "any1",
                    ExpiresOn = null,
                    Reason = "any1",
                    BannedOn = now
                };
                var anotherBan = new Bans
                {
                    Id = 2,
                    Userid = 2,
                    Name = "any",
                    ByUserid = 1,
                    ByName = "any2",
                    ExpiresOn = null,
                    Reason = "any2",
                    BannedOn = now
                };
                var records = new List<Bans>{ ban, anotherBan };
                var subject = SubjectWithData(context, now, records);
                
                var result  = subject.GetBans("any");

                Assert.Equal(records.Count, result.Count);
            }
        }

        [Fact]
        public void Test_GetExpiredBans_WithNoExistingRecords_ReturnsEmptyList()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var subject = SubjectWithData(context, now);
                
                var result  = subject.GetExpiredBans();

                Assert.Empty(result);
            }
        }

        [Fact]
        public void Test_GetExpiredBans_WithNoExpiringRecords_ReturnsEmptyList()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var ban = new Bans
                {
                    Id = 1,
                    Userid = 1,
                    Name = "any",
                    ByUserid = 1,
                    ByName = "any",
                    ExpiresOn = now.AddDays(1),
                    Reason = "any",
                    BannedOn = now
                };
                var subject = SubjectWithData(context, now, new List<Bans>{ ban });
                
                var result  = subject.GetExpiredBans();

                Assert.Empty(result);
            }
        }
        
        [Fact]
        public void Test_GetExpiredBans_WithExpiredRecords_ReturnsCorrectRecords()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var expiredBan = new Bans
                {
                    Id = 1,
                    Userid = 1,
                    Name = "any",
                    ByUserid = 1,
                    ByName = "any",
                    ExpiresOn = now.AddDays(-1),
                    Reason = "any",
                    BannedOn = now
                };
                var subject = SubjectWithData(context, now, new List<Bans>{ expiredBan });
                
                var result  = subject.GetExpiredBans().First();

                Assert.Equal(expiredBan, result);
            }
        }
        
        [Fact]
        public void Test_GetExpiredBans_WithExpiredAndLiftedRecords_ReturnsEmptyList()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var expiredBan = new Bans
                {
                    Id = 1,
                    Userid = 1,
                    Name = "any",
                    ByUserid = 1,
                    ByName = "any",
                    ExpiresOn = now.AddDays(-1),
                    Reason = "any",
                    BannedOn = now,
                    Lifted = 1
                };
                var subject = SubjectWithData(context, now, new List<Bans>{ expiredBan });
                
                var result  = subject.GetExpiredBans();

                Assert.Empty(result);
            }
        }

        private MysqlBansRepository SubjectWithData(DatabaseContext context, DateTime time, List<Bans> bans = null)
        {
            Provision(context, bans);
            return new MysqlBansRepository(context, MockTime(time).Object);
        }

        private Mock<ITimeProvider> MockTime(DateTime time)
        {
            var subject = new Mock<ITimeProvider>();
            subject.Setup(s => s.UtcNow).Returns(time);
            return subject;
        }
    }
}