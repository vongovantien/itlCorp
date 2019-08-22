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
        public static bool Send(string subject, string body, List<string> toEmails, List<string> attachments, List<string> emailCCs)
        {
            //string SentUser = "";
            string receivers = "";
            string CCs = "";
            string description = "";
            bool result = true;

            MailAddress EmailFrom = new MailAddress("dntonetms@itlvn.com");
            MailMessage message = new MailMessage();

            message.From = EmailFrom;
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
                new System.Net.NetworkCredential("itl\\dntonetms",
                    "0TMS@ne2");
            client.Timeout = 300000;

            // send message
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
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
        public static bool Send(string Subject, string Body, string ToEmail, List<string> Attachments, List<string> EmailCCs)
        {
            List<string> ToEmails = new List<string>(){ ToEmail };
            return Send(Subject, Body, ToEmails, Attachments, EmailCCs); 
        }
        private static DateTime GetDateTime()
        {
           
            return DateTime.Now;
        }
    }
}
