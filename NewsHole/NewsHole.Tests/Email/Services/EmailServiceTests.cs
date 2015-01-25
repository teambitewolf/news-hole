using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NewsHole.Email.Infrastructure;
using NewsHole.Email.Messages;
using NewsHole.Email.Services;
using System;

namespace NewsHole.Tests.Email.Services
{
    [TestClass]
    public class EmailServiceTests
    {
        private Mock<ISmtpService> _smtpService;
        private Mock<IEmailConfiguration> _emailConfig;

        private SendEmailRequest _testSendEmailRequest;

        private const string _testResetSender = "test@reset.com";
        private const string _testToAddress = "fake@fake.com";
        private const string _testSubject = "Test";
        private const string _testMessage = "Message Test";

        [TestInitialize]
        public void Initialize()
        {
            _smtpService = new Mock<ISmtpService>();
            _emailConfig = new Mock<IEmailConfiguration>();

            _emailConfig.Setup(x => x.GetSender(It.IsAny<Sender>())).Returns(_testResetSender);
            _smtpService.Setup(x => x.SendMail(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()));

            _testSendEmailRequest = new SendEmailRequest
            {
                Sender = Sender.PasswordReset,
                ToAddress = _testToAddress,
                Subject = _testSubject,
                Message = _testMessage
            };
        }

        [TestMethod]
        public void Test_SendMail_Sends_Correct_Email()
        {
            _smtpService.Setup(x => x.SendMail(
                _testResetSender,
                _testToAddress,
                _testSubject,
                _testMessage)).Verifiable();

            var emailService = new EmailService(_smtpService.Object, _emailConfig.Object);
            var result = emailService.SendEmail(_testSendEmailRequest);

            _smtpService.Verify();
        }

        [TestMethod]
        public void Test_SendMail_Returns_Successful_Response_On_Email_Success()
        {
            var emailService = new EmailService(_smtpService.Object, _emailConfig.Object);
            var result = emailService.SendEmail(_testSendEmailRequest);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("Success!", result.Message);
        }

        [TestMethod]
        public void Test_SendMail_Returns_Error_On_Send_Failure()
        {
            _smtpService.Setup(x => x.SendMail(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>())).Throws(new Exception("Test Error"));

            var emailService = new EmailService(_smtpService.Object, _emailConfig.Object);
            var result = emailService.SendEmail(_testSendEmailRequest);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Test Error", result.Message);
        }
    }
}
