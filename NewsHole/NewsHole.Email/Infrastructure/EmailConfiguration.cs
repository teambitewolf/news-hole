using NewsHole.Email.Messages;

namespace NewsHole.Email.Infrastructure
{
    public interface IEmailConfiguration
    {
        string GetSender(Sender sender);
    }

    public class EmailConfiguration : IEmailConfiguration
    {
        public string GetSender(Sender sender)
        {
            switch (sender)
            {
                case Sender.PasswordReset:
                    return Properties.Settings.Default.ResetPasswordSender;
                default:
                    return null;
            }
        }
    }
}
