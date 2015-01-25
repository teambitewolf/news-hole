using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NewsHole.Account.Infrastructure;
using NewsHole.Account.Messages;
using NewsHole.Account.Services;
using NewsHole.Data.Entities;
using NewsHole.Data.Repositories;
using System;

namespace NewsHole.Tests.Account.Services
{
    [TestClass]
    public class AccountServiceTests
    {
        private Mock<IUserRepository> _userRepo;
        private Mock<ICrypt> _cryptServices;
        private Mock<IRepository<ResetPasswordEntry, string>> _resetPassRepo;

        private LoginRequest _testLoginRequest;
        private AccountCreateRequest _testAccountCreateRequest;
        private ResetPasswordRequest _testResetPasswordRequest;
        private ChangePasswordRequest _testChangePasswordRequest;
        private const string TestToken = "12345";
        private const string TestEmail = "test@test.com";

        private ResetPasswordEntry _testResetPassEntry;

        private User _testUser;

        private const string TestSalt = "Salt";

        [TestInitialize]
        public void TestInitialize()
        {
            _resetPassRepo = new Mock<IRepository<ResetPasswordEntry, string>>();
            _userRepo = new Mock<IUserRepository>();
            _cryptServices = new Mock<ICrypt>();

            _testLoginRequest = new LoginRequest
            {
                Password = "Test",
                Email = TestEmail
            };

            _testUser = new User
            {
                Email = TestEmail,
                FirstName = "Test",
                PasswordHash = "ImAHash",
                LastName = "User"
            };

            _testAccountCreateRequest = new AccountCreateRequest
            {
                Email = "new-test@test.com",
                FirstName = "Tester",
                Password = "NewPassword",
                LastName = "User"
            };

            _testChangePasswordRequest = new ChangePasswordRequest
            {
                NewPassword = "TestNewPass",
                Token = "12345"
            };

            _testResetPasswordRequest = new ResetPasswordRequest
            {
                Email = TestEmail
            };

            _testResetPassEntry = new ResetPasswordEntry
            {
                EntryDateTime = DateTime.Now,
                Token = "12345",
                User = new User
                {
                    Email = TestEmail
                }
            };
        }

        [TestMethod]
        public void Test_Login_Returns_Account_Does_Not_Exist_If_User_Does_Not_Exist()
        {
            _userRepo.Setup(x => x.UserExists(_testLoginRequest.Email)).Returns(false).Verifiable();

            var accountService = InitializeObject();
            var response = accountService.Login(_testLoginRequest);

            _userRepo.Verify();
            Assert.AreEqual(LoginResponseCode.AccountDoesNotExist, response.ResponseCode);
        }

        [TestMethod]
        public void Test_Login_Returns_Password_Does_Not_Match_If_Password_Does_Not_Match()
        {
            _userRepo.Setup(x => x.UserExists(_testLoginRequest.Email)).Returns(true);
            _userRepo.Setup(x => x.GetByEmailAddress(_testLoginRequest.Email)).Returns(_testUser);
            _cryptServices.Setup(x => x.CheckPassword(_testLoginRequest.Password, _testUser.PasswordHash)).Returns(false).Verifiable();

            var accountService = InitializeObject();
            var response = accountService.Login(_testLoginRequest);

            _cryptServices.Verify();
            Assert.AreEqual(LoginResponseCode.PasswordDoesNotMatch, response.ResponseCode);
        }

        [TestMethod]
        public void Test_Login_Returns_Success_On_Successful_Login()
        {
            _userRepo.Setup(x => x.UserExists(_testLoginRequest.Email)).Returns(true);
            _userRepo.Setup(x => x.GetByEmailAddress(_testLoginRequest.Email)).Returns(_testUser);
            _cryptServices.Setup(x => x.CheckPassword(_testLoginRequest.Password, _testUser.PasswordHash)).Returns(true).Verifiable();

            var accountService = InitializeObject();
            var response = accountService.Login(_testLoginRequest);

            _cryptServices.Verify();
            Assert.AreEqual(LoginResponseCode.Success, response.ResponseCode);
        }

        [TestMethod]
        public void Test_Login_Returns_Correct_User_Info_On_Login()
        {
            _userRepo.Setup(x => x.UserExists(_testLoginRequest.Email)).Returns(true);
            _userRepo.Setup(x => x.GetByEmailAddress(_testLoginRequest.Email)).Returns(_testUser);
            _cryptServices.Setup(x => x.CheckPassword(_testLoginRequest.Password, _testUser.PasswordHash)).Returns(true);

            var accountService = InitializeObject();
            var response = accountService.Login(_testLoginRequest);

            Assert.AreEqual(_testUser.Email, response.UserLogin.Email);
            Assert.AreEqual(_testUser.FirstName, response.UserLogin.FirstName);
            Assert.AreEqual(_testUser.LastName, response.UserLogin.LastName);
        }

        [TestMethod]
        public void Test_Create_Account_Returns_User_Exists_If_Email_Is_Taken()
        {
            _userRepo.Setup(x => x.UserExists(_testAccountCreateRequest.Email)).Returns(true).Verifiable();

            var accountService = InitializeObject();
            var response = accountService.CreateAccount(_testAccountCreateRequest);

            _userRepo.Verify();
            Assert.AreEqual(AccountResponseCode.UserAlreadyExists, response.ResponseCode);
        }

        [TestMethod]
        public void Test_Create_Account_Returns_Failed_To_Create_If_UserRepo_Throws_Exception()
        {
            _userRepo.Setup(x => x.UserExists(_testAccountCreateRequest.Email)).Returns(false);
            _userRepo.Setup(x => x.Add(It.IsAny<User>())).Throws(new Exception("Test Exception")).Verifiable();

            var accountService = InitializeObject();
            var response = accountService.CreateAccount(_testAccountCreateRequest);

            _userRepo.Verify();
            Assert.AreEqual(AccountResponseCode.FailedToCreate, response.ResponseCode);
        }

        [TestMethod]
        public void Test_Create_Account_Returns_Success_On_Successful_Create()
        {
            _userRepo.Setup(x => x.UserExists(_testAccountCreateRequest.Email)).Returns(false);
            _userRepo.Setup(x => x.Add(It.Is<User>(
                u => u.Email == _testAccountCreateRequest.Email
                && u.FirstName == _testAccountCreateRequest.FirstName
                && u.LastName == _testAccountCreateRequest.LastName))).Verifiable();

            var accountService = InitializeObject();
            var response = accountService.CreateAccount(_testAccountCreateRequest);

            _userRepo.Verify();
            Assert.AreEqual(AccountResponseCode.Success, response.ResponseCode);
        }

        [TestMethod]
        public void Test_Create_Account_Hashes_Password_On_Create()
        {
            _cryptServices.Setup(x => x.GenerateSalt(It.IsAny<int>())).Returns(TestSalt).Verifiable();
            _cryptServices.Setup(x => x.HashPassword(_testAccountCreateRequest.Password, TestSalt)).Returns("TestHash").Verifiable();

            _userRepo.Setup(x => x.UserExists(_testAccountCreateRequest.Email)).Returns(false);
            _userRepo.Setup(x => x.Add(It.Is<User>(u => u.PasswordHash == "TestHash"))).Verifiable();

            var accountService = InitializeObject();
            var response = accountService.CreateAccount(_testAccountCreateRequest);

            _cryptServices.Verify();
            _userRepo.Verify();
        }

        [TestMethod]
        public void Test_ResetPassword_Returns_Email_Not_Found_If_Email_Does_Not_Exist()
        {
            _userRepo.Setup(x => x.UserExists(_testResetPasswordRequest.Email)).Returns(false).Verifiable();

            var accountService = InitializeObject();
            var response = accountService.ResetPassword(_testResetPasswordRequest);

            _userRepo.Verify();
            Assert.AreEqual(ResetPasswordResponseCode.EmailNotFound, response.ResponseCode);
        }

        //Ran into this when Test_ResetPassword_Returns_Email_Not_Found_If_Email_Does_Not_Exist would return correct
        //responses under certain conditions, but not in others when it was trying to generate reset pass token
        //regardless of if the email existed.
        [TestMethod]
        public void Test_ResetPassword_Does_Not_Attempt_To_Create_Reset_Pass_Token_If_Email_Does_Not_Exist()
        {
            _userRepo.Setup(x => x.UserExists(_testResetPasswordRequest.Email)).Returns(false).Verifiable();
            _userRepo.Setup(x => x.GetByEmailAddress(It.IsAny<string>())).Verifiable();

            _resetPassRepo.Setup(x => x.Add(It.IsAny<ResetPasswordEntry>()));

            var accountService = InitializeObject();
            var response = accountService.ResetPassword(_testResetPasswordRequest);

            _userRepo.Verify(x => x.UserExists(_testResetPasswordRequest.Email));
            _userRepo.Verify(x => x.GetByEmailAddress(It.IsAny<string>()), Times.Never);
            _resetPassRepo.Verify(x => x.Add(It.IsAny<ResetPasswordEntry>()), Times.Never);
        }

        [TestMethod]
        public void Test_ResetPassword_Returns_Reset_Pass_Fail_If_Exception_Is_Thrown()
        {
            _userRepo.Setup(x => x.UserExists(_testResetPasswordRequest.Email)).Returns(true);
            _resetPassRepo.Setup(x => x.Add(It.IsAny<ResetPasswordEntry>())).Throws(new Exception("Test Exception")).Verifiable();

            var accountService = InitializeObject();
            var response = accountService.ResetPassword(_testResetPasswordRequest);

            _resetPassRepo.Verify();
            Assert.AreEqual(ResetPasswordResponseCode.FailedToResetPassword, response.ResponseCode);
        }

        [TestMethod]
        public void Test_ResetPassword_Returns_Success_On_Successful_Call()
        {
            _userRepo.Setup(x => x.UserExists(_testResetPasswordRequest.Email)).Returns(true);
            _resetPassRepo.Setup(x => x.Add(It.IsAny<ResetPasswordEntry>()));

            var accountService = InitializeObject();
            var response = accountService.ResetPassword(_testResetPasswordRequest);

            _resetPassRepo.Verify();
            Assert.AreEqual(ResetPasswordResponseCode.Success, response.ResponseCode);
        }

        [TestMethod]
        public void Test_ResetPassword_Adds_Correct_ResetPasswordEntry()
        {
            _userRepo.Setup(x => x.UserExists(_testResetPasswordRequest.Email)).Returns(true);
            _resetPassRepo.Setup(x => x.Add(It.Is<ResetPasswordEntry>(
                r => r.User.Id == _testUser.Id)));

            var accountService = InitializeObject();
            var response = accountService.ResetPassword(_testResetPasswordRequest);

            _resetPassRepo.Verify();
        }

        [TestMethod]
        public void Test_ChangePassword_Returns_FailedToChangePassword_If_Exception_Occurs()
        {
            _resetPassRepo.Setup(x => x.Get(It.IsAny<string>())).Throws(new Exception("Test")).Verifiable();

            var accountService = InitializeObject();
            var response = accountService.ChangePassword(_testChangePasswordRequest);

            Assert.AreEqual(ChangePasswordResponseCode.FailedToChangePassword, response.ResponseCode);
            _resetPassRepo.Verify();
        }

        [TestMethod]
        public void Test_ChangePassword_Changes_Hash_For_Correct_User()
        {
            _resetPassRepo.Setup(x => x.Get(_testChangePasswordRequest.Token)).Returns(_testResetPassEntry);

            _userRepo.Setup(x => x.GetByEmailAddress(_testResetPassEntry.User.Email)).Returns(_testUser).Verifiable();

            _cryptServices.Setup(x => x.GenerateSalt(It.IsAny<int>())).Returns("Salt");
            _cryptServices.Setup(x => x.HashPassword(_testChangePasswordRequest.NewPassword, "Salt")).Returns("TestHash").Verifiable();

            _userRepo.Setup(x => x.Update(It.Is<User>(
                u => u.Email == _testUser.Email
                && u.PasswordHash == "TestHash"))).Verifiable();

            var accountService = InitializeObject();
            var response = accountService.ChangePassword(_testChangePasswordRequest);

            _userRepo.Verify();
            _cryptServices.Verify();
        }

        [TestMethod]
        public void Test_ChangePassword_Deletes_ResetPassEntry_After_Use()
        {
            _resetPassRepo.Setup(x => x.Get(_testChangePasswordRequest.Token)).Returns(_testResetPassEntry);

            _userRepo.Setup(x => x.GetByEmailAddress(_testResetPassEntry.User.Email)).Returns(_testUser);

            _cryptServices.Setup(x => x.GenerateSalt(It.IsAny<int>())).Returns("Salt");
            _cryptServices.Setup(x => x.HashPassword(_testChangePasswordRequest.NewPassword, "Salt")).Returns("TestHash");

            _resetPassRepo.Setup(x => x.Delete(It.Is<ResetPasswordEntry>(
                r => r.Token == _testResetPassEntry.Token))).Verifiable();

            var accountService = InitializeObject();
            var response = accountService.ChangePassword(_testChangePasswordRequest);

            _resetPassRepo.Verify();
        }

        [TestMethod]
        public void Test_IsChangePasswordTokenValid_Returns_False_If_Token_Does_Not_Exist()
        {
            _resetPassRepo.Setup(x => x.Get(TestToken)).Returns<ResetPasswordEntry>(null);

            var accountService = InitializeObject();
            var response = accountService.IsChangePasswordTokenValid(TestToken);

            Assert.IsFalse(response);
        }

        [TestMethod]
        public void Test_IsChangePasswordTokenValid_Returns_False_If_Token_Is_Older_Than_One_Hour()
        {
            _resetPassRepo.Setup(x => x.Get(TestToken)).Returns(new ResetPasswordEntry
            {
                EntryDateTime = DateTime.UtcNow.Subtract(new TimeSpan(3, 0, 0)),
                Token = TestToken,
                User = null
            });

            var accountService = InitializeObject();
            var response = accountService.IsChangePasswordTokenValid(TestToken);

            Assert.IsFalse(response);
        }

        [TestMethod]
        public void Test_IsChangePasswordTokenValid_Returns_True_If_Token_Is_Within_One_Hour()
        {
            _resetPassRepo.Setup(x => x.Get(TestToken)).Returns(new ResetPasswordEntry
            {
                EntryDateTime = DateTime.UtcNow.Subtract(new TimeSpan(0, 5, 0)),
                Token = TestToken,
                User = null
            });

            var accountService = InitializeObject();
            var response = accountService.IsChangePasswordTokenValid(TestToken);

            Assert.IsTrue(response);
        }

        [TestMethod]
        public void Test_DeleteChangePasswordToken_Safely_Returns_If_No_Token_Is_Present()
        {
            _resetPassRepo.Setup(x => x.Get(TestToken)).Returns<ResetPasswordEntry>(null);

            var accountService = InitializeObject();
            accountService.DeleteChangePasswordToken(TestToken);
        }

        [TestMethod]
        public void Test_DeleteChangePasswordToken_Deletes_Token_If_Present()
        {
            _resetPassRepo.Setup(x => x.Get(TestToken)).Returns(new ResetPasswordEntry
            {
                EntryDateTime = DateTime.UtcNow.Subtract(new TimeSpan(0, 5, 0)),
                Token = TestToken,
                User = null
            }).Verifiable();

            _resetPassRepo.Setup(x => x.Delete(It.Is<ResetPasswordEntry>(
                r => r.Token == TestToken))).Verifiable();

            var accountService = InitializeObject();
            accountService.DeleteChangePasswordToken(TestToken);

            _resetPassRepo.Verify();
        }

        [TestMethod]
        public void Test_GetAccountInfo_Returns_Account_Does_Not_Exist_If_User_Not_Found()
        {
            _userRepo.Setup(x => x.GetByEmailAddress(TestEmail)).Returns<User>(null).Verifiable();

            var accountService = InitializeObject();
            var response = accountService.GetAccountInfo(TestEmail);

            _userRepo.Verify();
            Assert.AreEqual(AccountInfoResponseCode.AccountDoesNotExist, response.ResponseCode);
        }

        [TestMethod]
        public void Test_GetAccountInfo_Returns_Success_On_Successful_Account_Retrieval()
        {
            _userRepo.Setup(x => x.GetByEmailAddress(TestEmail)).Returns(_testUser).Verifiable();

            var accountService = InitializeObject();
            var response = accountService.GetAccountInfo(TestEmail);

            _userRepo.Verify();
            Assert.AreEqual(AccountInfoResponseCode.Success, response.ResponseCode);
        }

        [TestMethod]
        public void Test_GetAccountInfo_Returns_Correct_User_Info_On_Successful_Account_Retrieval()
        {
            _userRepo.Setup(x => x.GetByEmailAddress(TestEmail)).Returns(_testUser).Verifiable();

            var accountService = InitializeObject();
            var response = accountService.GetAccountInfo(TestEmail);

            _userRepo.Verify();
            Assert.AreEqual(_testUser.Email, response.UserLogin.Email);
            Assert.AreEqual(_testUser.FirstName, response.UserLogin.FirstName);
            Assert.AreEqual(_testUser.LastName, response.UserLogin.LastName);
        }

        private AccountService InitializeObject()
        {
            return new AccountService(_cryptServices.Object, _resetPassRepo.Object, _userRepo.Object);
        }
    }
}
