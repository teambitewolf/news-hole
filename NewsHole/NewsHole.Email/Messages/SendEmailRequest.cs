namespace NewsHole.Email.Messages
{
    public class SendEmailRequest
    {
        public Sender Sender { get; set; }
        public string ToAddress { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}
