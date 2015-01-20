namespace NewsHole.Account.Messages
{
    public class AccountCreateResponse
    {
        public string Message { get; set; }
        public AccountResponseCode ResponseCode { get; set; }
    }

    public enum AccountResponseCode
    {
        Success,
        FailedToCreate,
        UserAlreadyExists
    }
}
