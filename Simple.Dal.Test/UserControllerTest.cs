using Crpm.Dal.Services;
using Crpm.Infrastructure.Core;
using Crpm.Model.Data;
using Crpm.Model.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Crpm.Dal.UnitTest
{
    public class UserControllerTest : TestWebHost
    {
        private readonly UserService _userService;
        private UserDetails _curentUser;

        public UserControllerTest()
        {
            _userService = GeneralContext.GetService<UserService>();
        }

        [Fact]
        public async void GetUserDetails()
        {
            string userName = "4cast"; string password = "123456";
            var result = await _userService.GetUserDetails(userName, password);
            Assert.NotNull(result);
        }

        [Fact]
        public async void GetUsers()
        {
            string userGuid = null;
            var results = await _userService.GetUsers(userGuid);
            Assert.NotNull(results);
        }

        [Fact]
        public async void DeactivateUsers()
        {
            string[] userGuids = new string[] { "00001111222233334444555566667777" }; //UserName = "4cast"
            _userService.IsTransactionEnabled = true;
            await _userService.ActivateUsers(userGuids, false);
            await _userService.OkResult();
            var result = (await _userService.GetUsers(userGuids[0])).FirstOrDefault();
            Assert.Equal("", result.UserStatus);
        }

        [Fact]
        public async void ActivateUsers()
        {
            string[] userGuids = new string[] { "00001111222233334444555566667777" }; //UserName = "4cast"
            _userService.IsTransactionEnabled = true;
            await _userService.ActivateUsers(userGuids, true);
            await _userService.OkResult();
            var result = (await _userService.GetUsers(userGuids[0])).FirstOrDefault();
            Assert.Equal("activate", result.UserStatus);
        }

        [Fact]
        public async void SaveUser()
        {
            _curentUser = CreateTestUser();
            bool createNewUSer = true;
            _userService.IsTransactionEnabled = true;
            await _userService.SaveUser(_curentUser, createNewUSer);
            await _userService.OkResult();
            var result = (await _userService.GetUsers(_curentUser.UserGuid)).FirstOrDefault();
            Assert.Equal("activate", result.UserStatus);
        }

        [Fact]
        public async void DeleteUsers()
        {
            var curentUsers = (await _userService.GetUsers()).Where(x => x.UserName.Contains("user_unit_test"));
            IEnumerable<string> userGuids = curentUsers.Select(x => x.UserGuid);  //new string[] { _curentUser.UserGuid };
            var result = await _userService.DeleteUsers(userGuids);
            Assert.True(result);
        }

        [Fact(Skip = "temporary")]
        public void SendEmails()
        {
            string[] userGuids = new string[] { "00001111222233334444555566667777" }; //UserName = "4cast"
            var result = _userService.SendEmails(userGuids);
            Assert.True(result);
        }

        [Fact]
        public async void GetPermissions()
        {
            var result = await _userService.GetPermissions();
            Assert.NotNull(result);
        }

        [Fact]
        public async void GetRolePermissions()
        {
            int? roleId = 8;
            var result = await _userService.GetRolePermissions(roleId);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(8)]
        [InlineData(null)]
        public async void GetRoles(int? roleId)
        {
            var result = await _userService.GetRoles(roleId);
            Assert.NotNull(result);
        }

        [Fact]
        public async void GetUsersTypes()
        {
            var result = await _userService.GetUsersTypes();
            Assert.NotNull(result);
        }

        [Fact]
        public async void SaveUsersByInterFace()
        {
            List<UserDetails> userDetails = new List<UserDetails>();
            userDetails.Add(CreateTestUser());
            userDetails.Add(CreateTestUser());
            bool createNewUSer = false;
            _userService.IsTransactionEnabled = true;
            var result = await _userService.SaveUserByInterFace(userDetails, createNewUSer);
            Assert.NotNull(result);
        }

        [Fact(Skip = "temporary")]
        public async void SaveCandidateUser()
        {
            List<Candidate> candidates = new List<Candidate>() { 
                new Candidate() { UserGuid = "00001111222233334444555566667777", PersonalNumber=-1, IdValue = "-1",  } 
            };

            var result = await _userService.SaveCandidates(candidates);
            Assert.True(result);
        }

        private void DeleteCandidateUser()
        {
            var oldCandidates = _userService.DbContext.Candidate.Where(x => x.PersonalNumber == -1);
            if (oldCandidates != null)
                _userService.DbContext.Candidate.RemoveRange(oldCandidates);
            Assert.True(true);
        }

        private UserDetails CreateTestUser()
        {
            var newUserIdSuffix = new Random().Next(int.MinValue, 0);
            return new UserDetails() 
            {
                UserGuid = null,
                UserId = newUserIdSuffix.ToString(),
                UserName = "only_for_test" + newUserIdSuffix.ToString(),
                Password = "123456",
                UserFirstName = "userFirstName_" + newUserIdSuffix.ToString(),
                UserLastName = "userLastName_" + newUserIdSuffix.ToString(),
                UserBusinessPhone = "123456",
                UserMobilePhone = "123456",
                UserNotes = "",
                UserCreateDate = "20200722152536",
                UserStatus = "activate",
                OrgGuid = "aaaabbbbccccddddeeeeffffgggghhhh",
                UserAdminPermission = 8,
                RoleId = 8,
                UserType = 2,
                JobTitle = "",
                JobTitleGuid = "",
                Language = 2,
                UnitGuid = "aaaabbbbccccddddeeeeffffgggghhhh",
                OrgName = "",
                RoleName = "",
                UserMail = "test@test.com"
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    using var tempServiceScope = GeneralContext.CreateServiceScope();
                    var userService = GeneralContext.GetService<UserService>();
                    userService.DbContext = tempServiceScope.ServiceProvider.GetService<CRPMContext>();
                    // remove testusers
                    var results = userService.DbContext.User.AsEnumerable()
                        .Where(x => int.Parse(x.UserId) < 0 && x.UserName.Contains("only_for_test"));
                    if (results != null && results.Any()) 
                        userService.DbContext.User.RemoveRange(results);
                    
                    userService.OkResult().Wait();

                    base.Dispose(disposing);
                }
                disposedValue = true;
            }
        }
    }
}
