using System.Net.Mail;

namespace NewsHole.Email.Infrastructure
{
    public interface ISmtpService
    {
        void SendMail(string from, string recipients, string subject, string body);
    }

    public class SmtpService : ISmtpService
    {
        public void SendMail(string from, string recipients, string subject, string body)
        {
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Send(from, recipients, subject, body);
            }
        }
    }
}
