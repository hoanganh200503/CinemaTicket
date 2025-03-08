
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
namespace CinemaTicketApp.Service
{
    public class EmailService
    {
        private readonly string _fromEmail = "huyntgce182398@fpt.edu.vn"; 
        private readonly string _appPassword = "qaah ggsj ainj ocgl";  

        public void SendEmail(string toEmail, string subject, string body)
        {
            using (var client = new SmtpClient("smtp.gmail.com", 587))
            {
                client.Credentials = new NetworkCredential(_fromEmail, _appPassword);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(toEmail);

                client.Send(mailMessage);
            }
        }
    }
}
