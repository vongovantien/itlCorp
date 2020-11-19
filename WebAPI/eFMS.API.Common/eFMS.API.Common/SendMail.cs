using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net.Mime;

namespace eFMS.API.Common
{
    public class SendMail
    {
        public static bool Send(string subject, string body, List<string> toEmails, List<string> attachments, List<string> emailCCs, List<string> emailBCC = null)
        {
            //string SentUser = "";
            string receivers = "";
            string CCs = "";
            string BCCs = "";

            string description = "";
            bool result = true;

            MailAddress emailFrom = new MailAddress("info.fms@itlvn.com");
            MailMessage message = new MailMessage();

            message.From = emailFrom;
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

            // Add a carbon copy recipient.
            if (emailCCs != null)
            {
                foreach (string EmailCC in emailCCs)
                {
                    MailAddress CC = new MailAddress(EmailCC);
                    CCs += EmailCC + ", ";
                    message.CC.Add(CC);
                }
            }

            // Add a carbon copy recipient.
            if (emailBCC != null)
            {
                foreach (string EmailBCC in emailBCC)
                {
                    MailAddress BCC = new MailAddress(EmailBCC);
                    BCCs += emailBCC + ", ";
                    message.Bcc.Add(BCC);
                }
            }

            //now attached the file
            if (attachments != null)
            {
                foreach (string attachment in attachments)
                {
                    Attachment attached = new Attachment(attachment, MediaTypeNames.Application.Octet);
                    message.Attachments.Add(attached);
                }
            }

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
        public static bool Send(string Subject, string Body, string ToEmail, List<string> Attachments, List<string> EmailCCs, List<string> emailBCC = null)
        {
            List<string> ToEmails = new List<string>() { ToEmail };
            return Send(Subject, Body, ToEmails, Attachments, EmailCCs, emailBCC);
        }        
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static DateTime GetDateTime()
        {

            return DateTime.Now;
        }
    }
}
