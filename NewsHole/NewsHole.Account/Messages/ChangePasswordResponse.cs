namespace NewsHole.Account.Messages
{
    public class ChangePasswordResponse
    {
        public string Message { get; set; }
        public ChangePasswordResponseCode ResponseCode { get; set; }
    }

    public enum ChangePasswordResponseCode
    {
        TokenExpired,
        FailedToChangePassword,
        Success
    }
}
