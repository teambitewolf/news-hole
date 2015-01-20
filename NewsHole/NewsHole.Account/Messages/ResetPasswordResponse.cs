namespace NewsHole.Account.Messages
{
    public class ResetPasswordResponse
    {
        public string ChangePasswordToken { get; set; }
        public string Message { get; set; }
        public ResetPasswordResponseCode ResponseCode { get; set; }
    }

    public enum ResetPasswordResponseCode
    {
        EmailNotFound,
        FailedToResetPassword,
        Success
    }
}
