namespace NewsHole.Account.Messages
{
    public class AccountInfoResponse
    {
        public UserLogin UserLogin { get; set; }
        public string Message { get; set; }
        public AccountInfoResponseCode ResponseCode { get; set; }
    }

    public enum AccountInfoResponseCode
    {
        AccountDoesNotExist,
        Success
    }
}
