using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace eFMSWindowService.Helpers
{
    public static class SendMailHelper
    {

        public static bool Send(string subject, string body, string email)
        {
            string description = "";
            bool result = true;

            MailAddress emailFrom = new MailAddress("info.fms@itlvn.com");
            MailMessage message = new MailMessage();

            message.From = emailFrom;
            message.To.Add(email);

            message.IsBodyHtml = true;
            message.Subject = subject;
            message.Body = body;

            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;

            client.Host = "webmail.itlvn.com";
            client.Port = 587;
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials =
                new System.Net.NetworkCredential("info.fms",
                    "ITPr0No1!");
            client.Timeout = 300000;

            // send message
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                client.Send(message);
                message.Attachments.Dispose();
                client.Dispose();
            }
            catch (Exception ex)
            {
                result = false;
                description = ex.Message;
            }
            finally
            {
                // InsertEmailHistory(SentUser, Receivers, CCs, "", Subject, result, Description);
            }
            return result;
        }
    }
}
