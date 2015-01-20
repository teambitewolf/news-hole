using System;

using NewsHole.Email.Infrastructure;
using NewsHole.Email.Messages;

namespace NewsHole.Email.Services
{
    public interface IEmailService
    {
        SendEmailResponse SendEmail(SendEmailRequest request);
    }

    public class EmailService : IEmailService
    {
        private ISmtpService _smtpService;
        private IEmailConfiguration _emailConfig;

        public EmailService(ISmtpService smtpService, IEmailConfiguration config)
        {
            _smtpService = smtpService;
            _emailConfig = config;
        }

        public SendEmailResponse SendEmail(SendEmailRequest request)
        {
            try
            {
                _smtpService.SendMail(
                    _emailConfig.GetSender(request.Sender),
                    request.ToAddress,
                    request.Subject,
                    request.Message);
            }
            catch (Exception ex)
            {
                return new SendEmailResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }

            return new SendEmailResponse
            {
                Success = true,
                Message = "Success!"
            };
        }
    }
}
