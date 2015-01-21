using System;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcContrib.TestHelper;
using MvcContrib.TestHelper.Fakes;
using NewsHole.Account.Messages;
using NewsHole.Account.Services;
using NewsHole.Email.Messages;
using NewsHole.Email.Services;
using NewsHole.Web.Controllers;
using NewsHole.Web.Infrastructure;
using NewsHole.Web.Models.Account;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NewsHole.Tests.Web.Controllers
{
    [TestClass]
    public class AccountControllerTests
    {
        private const string TestToken = "1234567890";

        private Mock<IAccountService> _accountService;
        private Mock<IAuthenticationHelper> _authHelper;
        private Mock<IEmailService> _emailService;

        private AccountCreateResponse _testAccountCreateResponse;
        private AccountInfoResponse _testAccountInfoResponse;
        private LoginResponse _testLoginResponse;
        private ChangePasswordResponse _testChangePasswordResponse;
        private ResetPasswordResponse _testResetPasswordResponse;

        private SendEmailResponse _testSendEmailResponse;

        private CreateUserModel _testCreateUserModel;
        private LoginModel _testLoginModel;
        private NewPasswordModel _testNewPasswordModel;
        private ResetPasswordModel _testResetPasswordModel;

        [TestInitialize]
        public void Initialize()
        {
            _accountService = new Mock<IAccountService>();
            _authHelper = new Mock<IAuthenticationHelper>();
            _emailService = new Mock<IEmailService>();

            _testCreateUserModel = new CreateUserModel
            {
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User",
                Password = "thisisatest",
                ConfirmPassword = "thisisatest"
            };

            _testAccountCreateResponse = new AccountCreateResponse
            {
                Message = "Success",
                ResponseCode = AccountResponseCode.Success
            };

            _testLoginModel = new LoginModel
            {
                Email = "test@test.com",
                Password = "thisisatest",
                RememberUser = false
            };

            _testLoginResponse = new LoginResponse
            {
                Message = "Success",
                ResponseCode = LoginResponseCode.Success,
                UserLogin = new UserLogin
                {
                    Email = _testLoginModel.Email,
                    FirstName = "Test",
                    LastName = "User"
                }
            };

            _testNewPasswordModel = new NewPasswordModel
            {
                Token = TestToken,
                NewPassword = "newpass",
                ConfirmNewPassword = "newpass"
            };

            _testChangePasswordResponse = new ChangePasswordResponse
            {
                Message = "Success",
                ResponseCode = ChangePasswordResponseCode.Success
            };

            _testResetPasswordModel = new ResetPasswordModel
            {
                Email = "test@test.com"
            };

            _testResetPasswordResponse = new ResetPasswordResponse
            {
                Message = "Success",
                ChangePasswordToken = TestToken,
                ResponseCode = ResetPasswordResponseCode.Success
            };

            _testSendEmailResponse = new SendEmailResponse
            {
                Message = "Success",
                Success = true
            };

            _testAccountInfoResponse = new AccountInfoResponse
            {
                Message = "Success",
                ResponseCode = AccountInfoResponseCode.Success,
                UserLogin = new UserLogin
                {
                    Email = _testLoginModel.Email,
                    FirstName = "Test",
                    LastName = "User"
                }
            };
        }

        [TestMethod]
        public void Test_Create_Get_Returns_View()
        {
            var controller = InitializeController();
            var result = controller.Create();

            result.AssertViewRendered(); 
        }

        [TestMethod]
        public void Test_Login_Get_Returns_View()
        {
            var controller = InitializeController();
            var result = controller.Login();

            result.AssertViewRendered();
        }

        [TestMethod]
        public void Test_Info_Get_Redirects_To_Home_If_Service_Fails_To_Get_User_Info()
        {
            _testAccountInfoResponse.Message = "No Account";
            _testAccountInfoResponse.ResponseCode = AccountInfoResponseCode.AccountDoesNotExist;

            _accountService.Setup(x => x.GetAccountInfo("fake@fake.com")).Returns(_testAccountInfoResponse).Verifiable();

            var controller = InitializeController();
            AddUserInfoToController(controller);
            var result = controller.Info();

            _accountService.Verify();
            result.AssertActionRedirect().ToAction("Index").ToController("Home");
        }

        [TestMethod]
        public void Test_Info_Get_Renders_View_If_Service_Gets_User_Info()
        {
            _accountService.Setup(x => x.GetAccountInfo("fake@fake.com")).Returns(_testAccountInfoResponse).Verifiable();

            var controller = InitializeController();
            AddUserInfoToController(controller);
            var result = controller.Info();

            _accountService.Verify();
            result.AssertViewRendered();
        }

        [TestMethod]
        public void Test_NewPassword_Get_Returns_View_If_Token_Is_Valid()
        {
            _accountService.Setup(x => x.IsChangePasswordTokenValid(TestToken)).Returns(true).Verifiable();

            var controller = InitializeController();
            var result = controller.NewPassword(TestToken);

            _accountService.Verify();
            result.AssertViewRendered();
        }

        [TestMethod]
        public void Test_NewPassword_Get_Redirects_To_PasswordTokenExpired_If_Token_Is_Not_Valid()
        {
            _accountService.Setup(x => x.IsChangePasswordTokenValid(TestToken)).Returns(false).Verifiable();

            var controller = InitializeController();
            var result = controller.NewPassword(TestToken);

            _accountService.Verify();
            result.AssertActionRedirect().ToAction("PasswordTokenExpired");
        }

        [TestMethod]
        public void Test_NewPassword_Get_Redirects_To_PasswordTokenExpired_If_No_Token_Is_Provided()
        {
            _accountService.Setup(x => x.IsChangePasswordTokenValid("")).Returns(false).Verifiable();

            var controller = InitializeController();
            var result = controller.NewPassword();

            _accountService.Verify();
            result.AssertActionRedirect().ToAction("PasswordTokenExpired");
        }

        [TestMethod]
        public void Test_PasswordToken_Expired_Get_Returns_View()
        {
            var controller = InitializeController();
            var result = controller.PasswordTokenExpired();

            result.AssertViewRendered();
        }

        [TestMethod]
        public void Test_ResetPassword_Get_Returns_View()
        {
            var controller = InitializeController();
            var result = controller.ResetPassword();

            result.AssertViewRendered();
        }

        [TestMethod]
        public void Test_Logout_Returns_View()
        {
            var controller = InitializeController();
            var result = controller.Logout();

            result.AssertViewRendered();
        }

        [TestMethod]
        public void Test_Logout_Signs_User_Out()
        {
            _authHelper.Setup(x => x.SignOut()).Verifiable();

            var controller = InitializeController();
            var result = controller.Logout();

            _authHelper.Verify();
        }

        [TestMethod]
        public void Test_Create_Post_Adds_Model_Error_When_Account_Already_Exists()
        {
            _testAccountCreateResponse.Message = "Failure - User";
            _testAccountCreateResponse.ResponseCode = AccountResponseCode.UserAlreadyExists;

            _accountService.Setup(x => x.CreateAccount(It.Is<AccountCreateRequest>(
                r => r.Email == _testCreateUserModel.Email
                && r.Password == _testCreateUserModel.Password
                && r.FirstName == _testCreateUserModel.FirstName
                && r.LastName == _testCreateUserModel.LastName))).Returns(_testAccountCreateResponse).Verifiable();

            var controller = InitializeController();
            var result = (ViewResult)controller.Create(_testCreateUserModel);

            _accountService.Verify();
            result.AssertViewRendered();
            Assert.AreEqual(_testAccountCreateResponse.Message, result.ViewData.ModelState["CreateError"].Errors.Single().ErrorMessage);
        }

        [TestMethod]
        public void Test_Create_Post_Adds_Model_Error_When_Account_Service_Fails_To_Create_Account()
        {
            _testAccountCreateResponse.Message = "Failure - Account Fail";
            _testAccountCreateResponse.ResponseCode = AccountResponseCode.FailedToCreate;

            _accountService.Setup(x => x.CreateAccount(It.Is<AccountCreateRequest>(
                r => r.Email == _testCreateUserModel.Email
                && r.Password == _testCreateUserModel.Password
                && r.FirstName == _testCreateUserModel.FirstName
                && r.LastName == _testCreateUserModel.LastName))).Returns(_testAccountCreateResponse).Verifiable();
            var controller = InitializeController();
            var result = (ViewResult)controller.Create(_testCreateUserModel);

            _accountService.Verify();
            result.AssertViewRendered();
            Assert.AreEqual(_testAccountCreateResponse.Message, result.ViewData.ModelState["CreateError"].Errors.Single().ErrorMessage);
        }

        [TestMethod]
        public void Test_Create_Post_Redirects_To_Index_On_Success()
        {
            _accountService.Setup(x => x.CreateAccount(It.Is<AccountCreateRequest>(
                r => r.Email == _testCreateUserModel.Email
                && r.Password == _testCreateUserModel.Password
                && r.FirstName == _testCreateUserModel.FirstName
                && r.LastName == _testCreateUserModel.LastName))).Returns(_testAccountCreateResponse).Verifiable();
            var controller = InitializeController();
            var result = controller.Create(_testCreateUserModel);

            _authHelper.Setup(x => x.Authenticate(It.IsAny<string>(), It.IsAny<bool>()));

            _accountService.Verify();
            result.AssertActionRedirect().ToController("Home").ToAction("Index");
        }

        [TestMethod]
        public void Test_Create_Post_Authenticates_User_On_Success()
        {
            _accountService.Setup(x => x.CreateAccount(It.Is<AccountCreateRequest>(
                r => r.Email == _testCreateUserModel.Email
                && r.Password == _testCreateUserModel.Password
                && r.FirstName == _testCreateUserModel.FirstName
                && r.LastName == _testCreateUserModel.LastName))).Returns(_testAccountCreateResponse).Verifiable();
            var controller = InitializeController();

            _authHelper.Setup(x => x.Authenticate(It.Is<string>(s => s == _testCreateUserModel.Email), false)).Verifiable();

            var result = controller.Create(_testCreateUserModel);

            _accountService.Verify();
            _authHelper.Verify();
        }

        [TestMethod]
        public void Test_Login_Post_Adds_Model_Error_When_Account_Does_Not_Exist()
        {
            _testLoginResponse.Message = "Failure";
            _testLoginResponse.ResponseCode = LoginResponseCode.AccountDoesNotExist;

            _accountService.Setup(x => x.Login(It.Is<LoginRequest>(
                l => l.Email == _testLoginModel.Email
                && l.Password == _testLoginModel.Password))).Returns(_testLoginResponse).Verifiable();

            var controller = InitializeController();
            var result = (ViewResult)controller.Login(_testLoginModel);

            _accountService.Verify();
            result.AssertViewRendered();
            Assert.AreEqual(_testLoginResponse.Message, result.ViewData.ModelState["LoginError"].Errors.Single().ErrorMessage);
        }

        [TestMethod]
        public void Test_Login_Post_Adds_Model_Error_When_Passwords_Do_Not_Match()
        {
            _testLoginResponse.Message = "Failure - Pass";
            _testLoginResponse.ResponseCode = LoginResponseCode.PasswordDoesNotMatch;

            _accountService.Setup(x => x.Login(It.Is<LoginRequest>(
                l => l.Email == _testLoginModel.Email
                && l.Password == _testLoginModel.Password))).Returns(_testLoginResponse).Verifiable();

            var controller = InitializeController();
            var result = (ViewResult)controller.Login(_testLoginModel);

            _accountService.Verify();
            result.AssertViewRendered();
            Assert.AreEqual(_testLoginResponse.Message, result.ViewData.ModelState["LoginError"].Errors.Single().ErrorMessage);
        }

        [TestMethod]
        public void Test_Login_Post_Authenticates_User_On_Success()
        {
            _accountService.Setup(x => x.Login(It.Is<LoginRequest>(
                l => l.Email == _testLoginModel.Email
                && l.Password == _testLoginModel.Password))).Returns(_testLoginResponse).Verifiable();

            _authHelper.Setup(x => x.Authenticate(It.Is<string>(s => s == _testLoginModel.Email), _testLoginModel.RememberUser)).Verifiable();

            var controller = InitializeController();
            var result = controller.Login(_testLoginModel);

            _accountService.Verify();
            _authHelper.Verify();
        }

        [TestMethod]
        public void Test_Login_Post_Redirects_To_Index_On_Success()
        {
            _accountService.Setup(x => x.Login(It.Is<LoginRequest>(
                l => l.Email == _testLoginModel.Email
                && l.Password == _testLoginModel.Password))).Returns(_testLoginResponse).Verifiable();

            _authHelper.Setup(x => x.Authenticate(It.Is<string>(s => s == _testLoginModel.Email), _testLoginModel.RememberUser)).Verifiable();

            var controller = InitializeController();
            var result = controller.Login(_testLoginModel);

            _accountService.Verify();
            result.AssertActionRedirect().ToController("Home").ToAction("Index");
        }

        //TODO: Should there be a test specifically for Remember User?

        [TestMethod]
        public void Test_NewPassword_Post_Adds_Model_Error_If_Token_Expired()
        {
            _testChangePasswordResponse.Message = "Token Expired";
            _testChangePasswordResponse.ResponseCode = ChangePasswordResponseCode.TokenExpired;

            _accountService.Setup(x => x.ChangePassword(It.Is<ChangePasswordRequest>(
                c => c.Token == TestToken
                && c.NewPassword == _testNewPasswordModel.NewPassword))).Returns(_testChangePasswordResponse).Verifiable();

            var controller = InitializeController();
            var result = (ViewResult)controller.NewPassword(_testNewPasswordModel);

            _accountService.Verify();
            result.AssertViewRendered();
            Assert.AreEqual(_testChangePasswordResponse.Message, result.ViewData.ModelState["NewPassError"].Errors.Single().ErrorMessage);
        }

        [TestMethod]
        public void Test_NewPassword_Post_Adds_Model_Error_If_Password_Change_Fails()
        {
            _testChangePasswordResponse.Message = "Password Change Fail";
            _testChangePasswordResponse.ResponseCode = ChangePasswordResponseCode.FailedToChangePassword;

            _accountService.Setup(x => x.ChangePassword(It.Is<ChangePasswordRequest>(
                c => c.Token == TestToken
                && c.NewPassword == _testNewPasswordModel.NewPassword))).Returns(_testChangePasswordResponse).Verifiable();

            var controller = InitializeController();
            var result = (ViewResult)controller.NewPassword(_testNewPasswordModel);

            _accountService.Verify();
            result.AssertViewRendered();
            Assert.AreEqual(_testChangePasswordResponse.Message, result.ViewData.ModelState["NewPassError"].Errors.Single().ErrorMessage);
        }

        [TestMethod]
        public void Test_NewPassword_Post_Redirects_To_Login_On_Success()
        {
            _accountService.Setup(x => x.ChangePassword(It.Is<ChangePasswordRequest>(
                c => c.Token == TestToken
                && c.NewPassword == _testNewPasswordModel.NewPassword))).Returns(_testChangePasswordResponse).Verifiable();

            var controller = InitializeController();
            var result = controller.NewPassword(_testNewPasswordModel);

            _accountService.Verify();
            result.AssertActionRedirect().ToAction("Login");
        }
        
        [TestMethod]
        public void Test_ResetPassword_Post_Adds_Model_Error_If_Email_Not_Found()
        {
            _testResetPasswordResponse.Message = "Email Not Found";
            _testResetPasswordResponse.ResponseCode = ResetPasswordResponseCode.EmailNotFound;

            _accountService.Setup(x => x.ResetPassword(It.Is<ResetPasswordRequest>(
                r => r.Email == _testResetPasswordModel.Email))).Returns(_testResetPasswordResponse).Verifiable();

            var controller = InitializeController();
            var result = (ViewResult)controller.ResetPassword(_testResetPasswordModel);

            _accountService.Verify();
            result.AssertViewRendered();
            Assert.AreEqual(_testResetPasswordResponse.Message, result.ViewData.ModelState["ResetPassError"].Errors.Single().ErrorMessage);
        }

        [TestMethod]
        public void Test_ResetPassword_Post_Does_Not_Send_Email_If_Email_Not_Found()
        {
            _testResetPasswordResponse.Message = "Email Not Found";
            _testResetPasswordResponse.ResponseCode = ResetPasswordResponseCode.EmailNotFound;

            _accountService.Setup(x => x.ResetPassword(It.Is<ResetPasswordRequest>(
                r => r.Email == _testResetPasswordModel.Email))).Returns(_testResetPasswordResponse).Verifiable();
            _emailService.Setup(x => x.SendEmail(It.IsAny<SendEmailRequest>())).Verifiable();

            var controller = InitializeController();
            var result = (ViewResult)controller.ResetPassword(_testResetPasswordModel);

            _accountService.Verify();
            _emailService.Verify(x => x.SendEmail(It.IsAny<SendEmailRequest>()), Times.Never);
        }

        [TestMethod]
        public void Test_ResetPassword_Post_Adds_Model_Error_If_Service_Fails_To_Reset_Password()
        {
            _testResetPasswordResponse.Message = "Failed To Reset Password";
            _testResetPasswordResponse.ResponseCode = ResetPasswordResponseCode.FailedToResetPassword;

            _accountService.Setup(x => x.ResetPassword(It.Is<ResetPasswordRequest>(
                r => r.Email == _testResetPasswordModel.Email))).Returns(_testResetPasswordResponse).Verifiable();

            var controller = InitializeController();
            var result = (ViewResult)controller.ResetPassword(_testResetPasswordModel);

            _accountService.Verify();
            result.AssertViewRendered();
            Assert.AreEqual(_testResetPasswordResponse.Message, result.ViewData.ModelState["ResetPassError"].Errors.Single().ErrorMessage);
        }

        [TestMethod]
        public void Test_ResetPassword_Post_Does_Not_Send_Email_If_Service_Fails_To_Reset_Password()
        {
            _testResetPasswordResponse.Message = "Failed To Reset Password";
            _testResetPasswordResponse.ResponseCode = ResetPasswordResponseCode.FailedToResetPassword;

            _accountService.Setup(x => x.ResetPassword(It.Is<ResetPasswordRequest>(
                r => r.Email == _testResetPasswordModel.Email))).Returns(_testResetPasswordResponse).Verifiable();
            _emailService.Setup(x => x.SendEmail(It.IsAny<SendEmailRequest>())).Returns(_testSendEmailResponse).Verifiable();

            var controller = InitializeController();
            var result = (ViewResult)controller.ResetPassword(_testResetPasswordModel);

            _accountService.Verify();
            _emailService.Verify(x => x.SendEmail(It.IsAny<SendEmailRequest>()), Times.Never);
        }

        [TestMethod]
        public void Test_ResetPassword_Post_Deletes_Token_If_Email_Fails_To_Send()
        {
            _testSendEmailResponse.Message = "Failed To Send";
            _testSendEmailResponse.Success = false;

            _accountService.Setup(x => x.ResetPassword(It.Is<ResetPasswordRequest>(
                r => r.Email == _testResetPasswordModel.Email))).Returns(_testResetPasswordResponse).Verifiable();
            _accountService.Setup(x => x.DeleteChangePasswordToken(TestToken)).Verifiable();

            _emailService.Setup(x => x.SendEmail(It.Is<SendEmailRequest>(
                s => s.Sender == Sender.PasswordReset))).Returns(_testSendEmailResponse).Verifiable();

            var controller = InitializeController();
            AddUrlInfoToController(controller);
            var result = controller.ResetPassword(_testResetPasswordModel);

            _accountService.Verify();
            _emailService.Verify();
        }

        [TestMethod]
        public void Test_ResetPassword_Post_Adds_Model_Error_If_Email_Fails_To_Send()
        {
            _testSendEmailResponse.Message = "Failed To Send";
            _testSendEmailResponse.Success = false;

            _accountService.Setup(x => x.ResetPassword(It.Is<ResetPasswordRequest>(
                r => r.Email == _testResetPasswordModel.Email))).Returns(_testResetPasswordResponse).Verifiable();

            _emailService.Setup(x => x.SendEmail(It.Is<SendEmailRequest>(
                s => s.Sender == Sender.PasswordReset))).Returns(_testSendEmailResponse).Verifiable();

            var controller = InitializeController();
            AddUrlInfoToController(controller);
            var result = (ViewResult)controller.ResetPassword(_testResetPasswordModel);

            _accountService.Verify();
            _emailService.Verify();
            Assert.AreEqual(_testSendEmailResponse.Message, result.ViewData.ModelState["ResetPassError"].Errors.Single().ErrorMessage);
        }

        [TestMethod]
        public void Test_ResetPassword_Post_Redirects_To_Index_On_Success()
        {
            _accountService.Setup(x => x.ResetPassword(It.Is<ResetPasswordRequest>(
                r => r.Email == _testResetPasswordModel.Email))).Returns(_testResetPasswordResponse).Verifiable();

            _emailService.Setup(x => x.SendEmail(It.Is<SendEmailRequest>(
                s => s.Sender == Sender.PasswordReset))).Returns(_testSendEmailResponse);

            var controller = InitializeController();
            AddUrlInfoToController(controller);
            var result = controller.ResetPassword(_testResetPasswordModel);

            _accountService.Verify();
            result.AssertActionRedirect().ToController("Home").ToAction("Index");
        }

        [TestMethod]
        public void Test_ResetPassword_Post_Sends_Email_On_Success()
        {
            _accountService.Setup(x => x.ResetPassword(It.Is<ResetPasswordRequest>(
                r => r.Email == _testResetPasswordModel.Email))).Returns(_testResetPasswordResponse).Verifiable();

            _emailService.Setup(x => x.SendEmail(It.Is<SendEmailRequest>(
                s => s.Sender == Sender.PasswordReset))).Returns(_testSendEmailResponse).Verifiable();

            var controller = InitializeController();
            AddUrlInfoToController(controller);
            var result = controller.ResetPassword(_testResetPasswordModel);

            _accountService.Verify();
            _emailService.Verify();
        }

        private AccountController InitializeController()
        {
            return new AccountController(_accountService.Object, _authHelper.Object, _emailService.Object);
        }

        /// <summary>
        /// Provides a controller context with a fake http context given a controller.
        /// Needed for any controller using UrlHelper.
        /// 
        /// For reference (to part of it): http://stackoverflow.com/a/3249559
        /// </summary>
        private void AddUrlInfoToController(Controller controller)
        {
            var uri = new Uri("http://localhost");
            var httpContext = new Mock<HttpContextBase>();
            var httpRequest = new Mock<HttpRequestBase>();

            httpRequest.Setup(x => x.Headers).Returns(new System.Net.WebHeaderCollection()
            {
                {"X-Requested-With", "XMLHttpRequest"}
            });
            httpRequest.Setup(x => x.Url).Returns(uri);

            httpContext.Setup(x => x.Request).Returns(httpRequest.Object);

            controller.ControllerContext = new ControllerContext(httpContext.Object, new RouteData(), controller);
            controller.Url = new UrlHelper(new RequestContext(httpContext.Object, new RouteData()));
        }

        /// <summary>
        /// Provides a controller with necessary fake controller context for any
        /// controller that requires the User variable.
        /// </summary>
        private void AddUserInfoToController(Controller controller)
        {
            //need to generate fake http context and fake principal to utilize 'User' variable.
            var user = new FakePrincipal(new FakeIdentity("fake@fake.com"), null);

            var builder = new TestControllerBuilder();
            builder.HttpContext.User = user;

            builder.InitializeController(controller);
        }
    }
}
