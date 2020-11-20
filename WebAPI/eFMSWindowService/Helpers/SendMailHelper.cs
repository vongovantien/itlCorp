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
        private const string _emailFrom = "info.fms@itlvn.com"; // "noreply-efms@itlvn.com"
        private const string _smtpHost = "webmail.itlvn.com"; // "email-smtp.ap-southeast-2.amazonaws.com"
        private const string _smptUser = "info.fms"; //"AKIA2AI6JMUOVFIQJQXN"
        private const string _smtpPassword = "ITPr0No1!"; // "BPHb4U8b6yCmJ7W4QB095djPHL75tQUfcXLOCGL99WKP"

        public static bool Send(string subject, string body, List<string> toEmails)
        {
            string description = "";
            bool result = true;

            MailAddress emailFrom = new MailAddress(_emailFrom);
            MailMessage message = new MailMessage();

            message.From = emailFrom;
            string receivers = "";
            if (toEmails != null && toEmails.Count() > 0)
            {
                foreach (string ToEmail in toEmails)
                {
                    MailAddress EmailTo = new MailAddress(ToEmail);
                    receivers += ToEmail + ", ";
                    message.To.Add(EmailTo);
                }
            }

            message.IsBodyHtml = true;
            message.Subject = subject;
            message.Body = body;

            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;

            client.Host = _smtpHost;
            client.Port = 587;
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials =
                new System.Net.NetworkCredential(_smptUser,
                    _smtpPassword);
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
