using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Html;
using Org.BouncyCastle.Utilities;

namespace WebRazor.Materials
{
    public class Mail: IEmailSender
    {

        private static string gmail= "hrmanagementswp@gmail.com";
        private static string password= "nxawkhmencnndqys";

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            throw new NotImplementedException();
        }

        public Task SendEmailResetAsync(string email, string password)
        {
            var emailToSend = new MimeMessage();
            emailToSend.From.Add(MailboxAddress.Parse(gmail));
            emailToSend.To.Add(MailboxAddress.Parse(email));
            emailToSend.Subject = "Reset Password Request was sended";
            var emailBody = new MimeKit.BodyBuilder
            {
                HtmlBody =
                    "Need to reset your password?<br>" +
                    "   Use your new password!<br>" +
                    "   Password: " + password + "<br>" +
                    "You must use new password to login system."
            };

            emailToSend.Body = emailBody.ToMessageBody();

            using (var emailClient = new SmtpClient())
            {
                emailClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                emailClient.Authenticate(gmail, password);
                emailClient.Send(emailToSend);
                emailClient.Disconnect(true);
            }
            return Task.CompletedTask;
        }

        public Task SendEmailOrderAsync(string email, Byte[] filedata)
        {
            String subject = "Confirm Order";

            var emailToSend = new MimeMessage();
            emailToSend.From.Add(MailboxAddress.Parse(gmail));
            emailToSend.To.Add(MailboxAddress.Parse(email));
            emailToSend.Subject = subject;

            var emailBody = new MimeKit.BodyBuilder
            {
                HtmlBody =
                "Hello " + email + @",<br>
                    We noticed that you have just placed an order on our system. Here is your bill.<br>
                    Best Regards,<br>
                    Dat<br>"
            };
            emailBody.Attachments.Add("YourOrder.pdf", filedata);

            emailToSend.Body = emailBody.ToMessageBody();

            using (var emailClient = new SmtpClient())
            {
                emailClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                emailClient.Authenticate(gmail, password);
                emailClient.Send(emailToSend);
                emailClient.Disconnect(true);
            }
            return Task.CompletedTask;
        }


    }
}
