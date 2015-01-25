using NewsHole.Email.Infrastructure;
using NewsHole.Email.Services;
using Ninject.Modules;

namespace NewsHole.Dependencies
{
    public class EmailModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ISmtpService>().To<SmtpService>();
            Bind<IEmailService>().To<EmailService>();
            Bind<IEmailConfiguration>().To<EmailConfiguration>();
        }
    }
}
