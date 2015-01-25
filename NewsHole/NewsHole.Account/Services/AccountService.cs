using NewsHole.Account.Infrastructure;
using NewsHole.Account.Messages;
using NewsHole.Data.Entities;
using NewsHole.Data.Repositories;
using System;

namespace NewsHole.Account.Services
{
    public interface IAccountService
    {
        AccountCreateResponse CreateAccount(AccountCreateRequest request);
        LoginResponse Login(LoginRequest request);
        ChangePasswordResponse ChangePassword(ChangePasswordRequest request);
        ResetPasswordResponse ResetPassword(ResetPasswordRequest request);
        AccountInfoResponse GetAccountInfo(string email);
        bool IsChangePasswordTokenValid(string token);
        void DeleteChangePasswordToken(string token);
    }

    public class AccountService : IAccountService
    {
        private int NumberOfRounds = 10;

        private ICrypt _cryptServices;
        private IUserRepository _userRepo;
        private IRepository<ResetPasswordEntry, string> _resetPassRepo;

        public AccountService(ICrypt cryptServices, IRepository<ResetPasswordEntry, string> resetPassRepo, IUserRepository userRepo)
        {
            _cryptServices = cryptServices;
            _resetPassRepo = resetPassRepo;
            _userRepo = userRepo;
        }

        public AccountCreateResponse CreateAccount(AccountCreateRequest request)
        {
            string message = "Success!";
            var responseCode = AccountResponseCode.Success;

            if (_userRepo.UserExists(request.Email))
            {
                message = "User Exists";
                responseCode = AccountResponseCode.UserAlreadyExists;
            }
            else
            {
                try
                {
                    _userRepo.Add(new User
                    {
                        Email = request.Email,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        PasswordHash = HashPassword(request.Password)
                    });
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    responseCode = AccountResponseCode.FailedToCreate;
                }
            }

            return new AccountCreateResponse
            {
                Message = message,
                ResponseCode = responseCode
            };
        }

        public LoginResponse Login(LoginRequest request)
        {
            var message = "Success!";
            var responseCode = LoginResponseCode.Success;
            UserLogin userLogin = null;

            if (!_userRepo.UserExists(request.Email))
            {
                message = "Account Does Not Exist.";
                responseCode = LoginResponseCode.AccountDoesNotExist;
            }
            else
            {
                var user = _userRepo.GetByEmailAddress(request.Email);

                if (_cryptServices.CheckPassword(request.Password, user.PasswordHash))
                {
                    userLogin = new UserLogin
                    {
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    };
                }
                else
                {
                    message = "Password Does Not Match.";
                    responseCode = LoginResponseCode.PasswordDoesNotMatch;
                }
            }

            return new LoginResponse
            {
                Message = message,
                ResponseCode = responseCode,
                UserLogin = userLogin
            };
        }

        public ChangePasswordResponse ChangePassword(ChangePasswordRequest request)
        {
            var message = "Success!";
            var responseCode = ChangePasswordResponseCode.Success;

            try
            {
                var resetPass = _resetPassRepo.Get(request.Token);

                var user = _userRepo.GetByEmailAddress(resetPass.User.Email);
                user.PasswordHash = HashPassword(request.NewPassword);

                _userRepo.Update(user);
                _resetPassRepo.Delete(resetPass);
            }
            catch (Exception ex)
            {
                message = ex.Message;
                responseCode = ChangePasswordResponseCode.FailedToChangePassword;
            }

            return new ChangePasswordResponse
            {
                Message = message,
                ResponseCode = responseCode
            };
        }

        public ResetPasswordResponse ResetPassword(ResetPasswordRequest request)
        {
            var message = "Success!";
            string resetPasswordToken = null;
            var responseCode = ResetPasswordResponseCode.Success;

            //get user by email
            if (_userRepo.UserExists(request.Email))
            {
                try
                {
                    var user = _userRepo.GetByEmailAddress(request.Email);

                    var resetPassEntry = new ResetPasswordEntry
                    {
                        EntryDateTime = DateTime.UtcNow,
                        Token = Guid.NewGuid().ToString(),
                        User = user
                    };

                    _resetPassRepo.Add(resetPassEntry);

                    resetPasswordToken = resetPassEntry.Token;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    responseCode = ResetPasswordResponseCode.FailedToResetPassword;
                    resetPasswordToken = null;
                }
            }
            else
            {
                message = "No user with provided email found.";
                responseCode = ResetPasswordResponseCode.EmailNotFound;
            }

            return new ResetPasswordResponse
            {
                Message = message,
                ChangePasswordToken = resetPasswordToken,
                ResponseCode = responseCode
            };
        }

        public AccountInfoResponse GetAccountInfo(string email)
        {
            var message = "Success";
            var responseCode = AccountInfoResponseCode.Success;
            UserLogin login = null;

            var user = _userRepo.GetByEmailAddress(email);

            if (user != null)
            {
                login = new UserLogin
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
            }
            else
            {
                message = "Account Does Not Exist";
                responseCode = AccountInfoResponseCode.AccountDoesNotExist;
            }

            return new AccountInfoResponse
            {
                Message = message,
                ResponseCode = responseCode,
                UserLogin = login
            };
        }

        public bool IsChangePasswordTokenValid(string token)
        {
            var resetPass = _resetPassRepo.Get(token);

            if ((resetPass == null) || (DateTime.UtcNow.Subtract(resetPass.EntryDateTime) > TimeSpan.FromHours(1)))
            {
                return false;
            }

            return true;
        }

        public void DeleteChangePasswordToken(string token)
        {
            var resetPass = _resetPassRepo.Get(token);

            if (resetPass != null)
            {
                _resetPassRepo.Delete(resetPass);
            }
        }

        private string HashPassword(string password)
        {
            var salt = _cryptServices.GenerateSalt(NumberOfRounds);
            return _cryptServices.HashPassword(password, salt);
        }
    }
}
