using NewsHole.Account.Messages;
using NewsHole.Account.Services;
using NewsHole.Email.Messages;
using NewsHole.Email.Services;
using NewsHole.Web.Infrastructure;
using NewsHole.Web.Models.Account;
using System.Web.Mvc;

namespace NewsHole.Web.Controllers
{
    public class AccountController : Controller
    {
        private IAccountService _accountService;
        private IAuthenticationHelper _authenticationHelper;
        private IEmailService _emailService;

        public AccountController(IAccountService accountService, IAuthenticationHelper authenticationHelper, IEmailService emailService)
        {
            _accountService = accountService;
            _authenticationHelper = authenticationHelper;
            _emailService = emailService;
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(CreateUserModel createUserModel)
        {
            var result = _accountService.CreateAccount(new AccountCreateRequest
            {
                Email = createUserModel.Email,
                FirstName = createUserModel.FirstName,
                LastName = createUserModel.LastName,
                Password = createUserModel.Password
            });

            if (result.ResponseCode != AccountResponseCode.Success)
            {
                ModelState.AddModelError("CreateError", result.Message);
                return View();
            }

            _authenticationHelper.Authenticate(createUserModel.Email, false);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Info()
        {
            var result = _accountService.GetAccountInfo(User.Identity.Name);

            if (result.ResponseCode != AccountInfoResponseCode.Success)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new UserModel
            {
                Email = result.UserLogin.Email,
                FirstName = result.UserLogin.FirstName,
                LastName = result.UserLogin.LastName
            });
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel loginModel)
        {
            var result = _accountService.Login(new LoginRequest
            {
                Email = loginModel.Email,
                Password = loginModel.Password
            });

            if (result.ResponseCode != LoginResponseCode.Success)
            {
                ModelState.AddModelError("LoginError", result.Message);
                return View();
            }
            _authenticationHelper.Authenticate(loginModel.Email, loginModel.RememberUser);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            _authenticationHelper.SignOut();

            return View();
        }

        public ActionResult NewPassword(string token = "")
        {
            if (!_accountService.IsChangePasswordTokenValid(token))
            {
                return RedirectToAction("PasswordTokenExpired");
            }

            return View(new NewPasswordModel
            {
                Token = token
            });
        }

        [HttpPost]
        public ActionResult NewPassword(NewPasswordModel newPassModel)
        {
            var result = _accountService.ChangePassword(new ChangePasswordRequest
            {
                NewPassword = newPassModel.NewPassword,
                Token = newPassModel.Token
            });

            if (result.ResponseCode != ChangePasswordResponseCode.Success)
            {
                //add model error and return view 
                ModelState.AddModelError("NewPassError", result.Message);
                return View();
            }

            return RedirectToAction("Login");
        }

        public ActionResult PasswordTokenExpired()
        {
            return View();
        }

        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordModel resetPassModel)
        {
            var resetPassResult = _accountService.ResetPassword(new ResetPasswordRequest
            {
                Email = resetPassModel.Email
            });

            if (resetPassResult.ResponseCode != ResetPasswordResponseCode.Success)
            {
                //add model error and return view 
                ModelState.AddModelError("ResetPassError", resetPassResult.Message);
                return View();
            }

            var emailResult = _emailService.SendEmail(new SendEmailRequest
            {
                Message = "Reset password. Follow link: " + CreateResetPassLink(resetPassResult.ChangePasswordToken),
                Subject = "Password Reset Request",
                ToAddress = resetPassModel.Email,
                Sender = Sender.PasswordReset
            });

            if (!emailResult.Success)
            {
                //delete reset pass token
                _accountService.DeleteChangePasswordToken(resetPassResult.ChangePasswordToken);

                ModelState.AddModelError("ResetPassError", emailResult.Message);
                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        private string CreateResetPassLink(string passToken)
        {
            //need to append token at the end
            return Url.Action("NewPassword", "Account",
                routeValues: new { token = passToken },
                protocol: Request.Url.Scheme);
        }
    }
}