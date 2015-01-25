namespace NewsHole.Account.Messages
{
    public class LoginResponse
    {
        public string Message { get; set; }
        public LoginResponseCode ResponseCode { get; set; }
        public UserLogin UserLogin { get; set; }
    }

    public enum LoginResponseCode
    {
        Success,
        AccountDoesNotExist,
        PasswordDoesNotMatch
    }
}
