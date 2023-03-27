using DoggoShopClient.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using RestSharp.Authenticators;
using RestSharp;
using System.Text;
using System.Net.Mail;
using System.Net;
using MimeKit;

namespace DoggoShopClient.Services
{
    public class EmailSenderService
    {
        public EmailSenderService()
        {
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            /*var options = new RestClientOptions
            {
                BaseUrl = new Uri("https://api.mailgun.net/v3"),
                Authenticator = new HttpBasicAuthenticator("api", "8f9db088a1747b62598eb12e20c21583-30344472-47c53993")
            };
            var client = new RestClient(options);
            client.AddDefaultHeader("Content-Type", "application/json");
            RestRequest request = new RestRequest();
            request.AddParameter("domain", "sandbox34706dcde3b54ad0bdcc362f2809ebd2.mailgun.org", ParameterType.UrlSegment);
            request.Resource = "{sandbox34706dcde3b54ad0bdcc362f2809ebd2.mailgun.org}/messages";
            request.AddParameter("from", "hailhhe153224@fpt.edu.vn");
            request.AddParameter("to", email);
            request.AddParameter("subject", subject);
            request.AddParameter("text", htmlMessage);
            request.Method = Method.Post;
            var response = await client.ExecuteAsync(request);
            return response;*/
            using(var mail = new MailMessage("rey.hilll93@ethereal.email", email, subject, htmlMessage))
            {
                using (var smtp = new SmtpClient("smtp.ethereal.email", 587))
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("rey.hilll93@ethereal.email", "RYTN6nCkByD1xrTA3g");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
            
        }
    }
}
