using System;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using main.Core;
using main.Core.Domain.Models;
using main.Core.Domain.Repo;
using main.Services;
using Moq;
using Xunit;

namespace test.Services
{
    public class UserServiceTest
    {
        [Fact]
        public void Test_SetUserCooldown_WithGivenCommandAndCooldown_SetsCorrectUserCooldown()
        {
            var subject = GetFakeTimeStubbedSubjectWithFakeRoles(10);
            var command = "testCommand";
            ulong userId = 1;
            
            subject.SetUserCooldown(userId, 60, command);

            var result = subject.IsUserOnCooldown(userId, command);
            Assert.True(result);
        }
        
        [Fact]
        public void Test_SetUserCooldown_WithGivenCommandAndCooldown_DoesNotSetWrongUserCooldown()
        {
            var subject = GetFakeTimeStubbedSubjectWithFakeRoles(10);
            var command = "testCommand";
            ulong userId = 1;
            
            subject.SetUserCooldown(userId,60, command);

            var result = subject.IsUserOnCooldown(2, command);
            Assert.False(result);
        }
        
        [Fact]
        public void Test_IsUserOnCooldown_WithGivenCommandAndCooldownAndExpiredTime_ReturnsFalse()
        {
            var subject = GetFakeTimeStubbedSubjectWithFakeRoles(0);
            var command = "testCommand";
            ulong userId = 1;
            subject.SetUserCooldown(userId, 60, command);

            var result = subject.IsUserOnCooldown(userId, command);
            
            Assert.False(result);
        }
        
        [Fact]
        public void Test_IsUserOnCooldown_WithGivenCommandAndCooldownWithValidTime_ReturnsTrue()
        {
            var subject = GetFakeTimeStubbedSubjectWithFakeRoles(10);
            var command = "testCommand";
            ulong userId = 1;
            subject.SetUserCooldown(userId, 60, command);

            var result = subject.IsUserOnCooldown(userId, command);
            
            Assert.True(result);
        }
        
        [Fact]
        public void Test_GetUserRolesIds_WithNoExistingData_ReturnsEmptyList()
        {
            ulong userId = 1;
            var userRoleRepo = MockUserRoleRepository();
            var subject = GetFakeTimeStubbedSubjectWithFakeRoles(10, userRoleRepo.Object);

            var result = subject.GetUserRolesIds(userId);
            
            Assert.Empty(result);
        }
        
        [Fact]
        public void Test_GetUserRolesIds_WithExistingData_ReturnsCorrectRoleIds()
        {
            ulong userId = 1;
            ulong roleId = 1;
            var userRoleRepo = MockUserRoleRepository(userId, roleId);
            var subject = GetFakeTimeStubbedSubjectWithFakeRoles(10, userRoleRepo.Object);

            var result = subject.GetUserRolesIds(userId);
            
            Assert.Contains(roleId, result);
        }
        
        [Fact]
        public void Test_AssignUserRole_CallsRepositoryCreate()
        {
            ulong userId = 1;
            ulong roleId = 1;
            var userRoleRepo = MockUserRoleRepository(userId, roleId);
            var subject = GetFakeTimeStubbedSubjectWithFakeRoles(10, userRoleRepo.Object);

            subject.AssignUserRole(userId, roleId, userId);
            
            userRoleRepo.Verify(r => r.Create(userId, roleId, userId), Times.Once);
        }
        
        [Fact]
        public void Test_DeleteUserRole_CallsRepositoryDeleteByUserId()
        {
            ulong userId = 1;
            ulong roleId = 1;
            var userRoleRepo = MockUserRoleRepository(userId, roleId);
            var subject = GetFakeTimeStubbedSubjectWithFakeRoles(10, userRoleRepo.Object);

            subject.DeleteUserRole(userId, roleId);
            
            userRoleRepo.Verify(r => r.Delete(userId, roleId), Times.Once);
        }

        private Mock<IUserRoleRepository> MockUserRoleRepository(ulong? userId = null, ulong? roleId = null)
        {
            var subject = new Mock<IUserRoleRepository>();
            if (userId.HasValue && roleId.HasValue)
            {
                subject.Setup(mk => mk.GetByUserId(userId.Value)).Returns(new List<UserRole>
                {
                    new UserRole
                    {
                        UserId = userId.Value,
                        AssignedBy = userId.Value,
                        AssignedOn = DateTime.Now,
                        RoleId = roleId.Value
                    }
                });
            }
            else
            {
                subject.Setup(mk => mk.GetByUserId(It.IsAny<ulong>())).Returns(new List<UserRole>());
            }

            return subject;  
        }
        
        private UserService GetFakeTimeStubbedSubjectWithFakeRoles(
            int elapsedTime, 
            IUserRoleRepository userRoleRepository = null
            ) 
        {
            var subject = new Mock<ITimeProvider>();
            subject.Setup(mk => mk.GetElapsedFromEpoch(It.IsAny<long>())).Returns(elapsedTime);
            if (userRoleRepository is null)
            {
                userRoleRepository = MockUserRoleRepository().Object;
            }
            
            return new UserService(subject.Object, userRoleRepository);
        }
    }
}
 