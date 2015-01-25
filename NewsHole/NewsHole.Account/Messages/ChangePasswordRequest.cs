namespace NewsHole.Account.Messages
{
    public class ChangePasswordRequest
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
