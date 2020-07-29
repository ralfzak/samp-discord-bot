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
    public class MysqlUserRoleRepositoryTest: FakeDatabaseContext<UserRole>
    {
        [Fact]
        public void Test_Create_CreatesCorrectRecord()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var userRole = new UserRole
                {
                    UserId = 1,
                    RoleId = 1,
                    AssignedBy = 1,
                    AssignedOn = now
                };
                var subject = SubjectWithData(context, now);

                subject.Create(userRole.UserId, userRole.RoleId, userRole.AssignedBy);

                var result = GetAll(context).First();
                Assert.Equal(result.UserId, userRole.UserId);
                Assert.Equal(result.RoleId, userRole.RoleId);
                Assert.Equal(result.AssignedBy, userRole.AssignedBy);
                Assert.Equal(result.AssignedOn, userRole.AssignedOn);
            }
        }
        
        [Fact]
        public void Test_Create_GivenTwoRecordsWithSameUserIdAndRoleId_SilentlyIgnoresSecondRequest()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var userRole = new UserRole
                {
                    UserId = 1,
                    RoleId = 1,
                    AssignedBy = 1,
                    AssignedOn = now
                };
                var subject = SubjectWithData(context, now);
                
                subject.Create(userRole.UserId, userRole.RoleId, userRole.AssignedBy); 
                subject.Create(userRole.UserId, userRole.RoleId, userRole.AssignedBy);

                var result = GetAll(context);
                Assert.Single(result);
            }
        }
        
        [Fact]
        public void Test_DeleteByUserId_WithExistingRecords_DoesNotDeleteWrongRecord()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var userRole = new UserRole
                {
                    UserId = 1,
                    RoleId = 1,
                    AssignedBy = 1,
                    AssignedOn = now
                };
                var noise = new UserRole
                {
                    UserId = 2,
                    RoleId = 1,
                    AssignedBy = 1,
                    AssignedOn = now
                };
                var subject = SubjectWithData(context, now, new List<UserRole>{ userRole, noise });

                subject.Delete(userRole.UserId, userRole.RoleId);

                var result = GetAll(context).First(r => r.UserId == noise.UserId);
                Assert.Equal(result.UserId, noise.UserId);
            }
        }
        
        [Fact]
        public void Test_DeleteByUserId_WithExistingRecords_DeletesCorrectRecord()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var userRole = new UserRole
                {
                    UserId = 1,
                    RoleId = 1,
                    AssignedBy = 1,
                    AssignedOn = now
                };
                var noise = new UserRole
                {
                    UserId = 2,
                    RoleId = 1,
                    AssignedBy = 1,
                    AssignedOn = now
                };
                var subject = SubjectWithData(context, now, new List<UserRole>{ userRole, noise });

                subject.Delete(userRole.UserId, userRole.RoleId);

                var result = GetAll(context).Select(r => r.UserId);
                Assert.DoesNotContain(userRole.UserId, result);
            }
        }

        [Fact]
        public void Test_DeleteByUserId_WithNoExistingMatchingRecords_SilentlyIgnores()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var subject = SubjectWithData(context, now);
                
                subject.Delete(1, 1);

                var result = GetAll(context);
                Assert.Empty(result);
            }
        }

        [Fact]
        public void Test_FindByUserId_WithNoExistingRecords_ReturnsEmptyList()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var subject = SubjectWithData(context, now);
                
                var result  = subject.GetByUserId(1);

                Assert.Empty(result);
            }
        }

        [Fact]
        public void Test_FindByUserId_WithExistingMatchingRecord_ReturnsRecord()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var userRole = new UserRole
                {
                    UserId = 1,
                    RoleId = 1,
                    AssignedBy = 1,
                    AssignedOn = now
                };
                var subject = SubjectWithData(context, now, new List<UserRole>{ userRole });
                
                var result  = subject.GetByUserId(userRole.UserId);

                Assert.Single(result);
                Assert.Equal(userRole.UserId, result.First().UserId);
            }
        }
        
        [Fact]
        public void Test_FindByUserId_WithNoExistingMatchingRecord_ReturnsEmptyList()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var userRole = new UserRole
                {
                    UserId = 1,
                    RoleId = 1,
                    AssignedBy = 1,
                    AssignedOn = now
                };
                var subject = SubjectWithData(context, now, new List<UserRole>{ userRole });
                
                var result  = subject.GetByUserId(0);

                Assert.Empty(result);
            }
        }
        
        [Fact]
        public void Test_FindByUserId_WithExistingRecords_ReturnsOnlyMatchingRecord()
        {
            var now = DateTime.UtcNow;
            using (var context = Context())
            {
                var userRole = new UserRole
                {
                    UserId = 1,
                    RoleId = 1,
                    AssignedBy = 1,
                    AssignedOn = now
                };
                var noise = new UserRole
                {
                    UserId = 2,
                    RoleId = 1,
                    AssignedBy = 1,
                    AssignedOn = now
                };
                var subject = SubjectWithData(context, now, new List<UserRole>{ userRole, noise });
                
                var result  = subject.GetByUserId(userRole.UserId);

                Assert.Single(result);
                Assert.Equal(userRole.UserId, result.First().UserId);
            }
        }
        
        private MysqlUserRoleRepository SubjectWithData(
            DatabaseContext context, 
            DateTime time, 
            List<UserRole> userRoles = null
            )
        {
            Provision(context, userRoles);
            return new MysqlUserRoleRepository(context, MockTime(time).Object);
        }

        private Mock<ITimeProvider> MockTime(DateTime time)
        {
            var subject = new Mock<ITimeProvider>();
            subject.Setup(s => s.UtcNow).Returns(time);
            return subject;
        }
    }
}
