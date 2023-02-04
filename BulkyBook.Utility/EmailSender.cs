using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BulkyBook.Utility
{
	public class EmailSender : IEmailSender
	{
		public string SendGridSecret { get; set; }
		public EmailSender(IConfiguration _config)
		{
			SendGridSecret = _config.GetValue<string>("SendGrid:SecretKey");
		}
		public Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			#region use For gmail, yahoo, hotmail 

			//var emailToSend = new MimeMessage();
			//emailToSend.From.Add(MailboxAddress.Parse("Hello@dotnetmastery.com"));
			//emailToSend.To.Add(MailboxAddress.Parse(email));
			//emailToSend.Subject = subject;
			//emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html) {Text = htmlMessage};

			//// SEND email
			//using(var emailClient = new SmtpClient())
			//{
			//	emailClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
			//	emailClient.Authenticate("Hello@dotnetmastery.com", "DotNet213$");
			//	emailClient.Send(emailToSend);
			//	emailClient.Disconnect(true);
			//}
			//return Task.CompletedTask; 
			#endregion

			#region for my domain
			var client = new SendGridClient(SendGridSecret);
			var from = new EmailAddress("Hello@dotnetmastery.com", "Bulky Book");
			var to = new EmailAddress(email);
			var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);
			return client.SendEmailAsync(msg);
			#endregion
		}
	}
}
