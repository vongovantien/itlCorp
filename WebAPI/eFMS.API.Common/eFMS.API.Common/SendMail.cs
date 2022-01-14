using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net.Mime;
using eFMS.API.Common.Helpers;
using System.IO;

namespace eFMS.API.Common
{
    public class SendMail
    {
        public const string _emailFrom = "info.fms@itlvn.com"; //"noreply-efms@itlvn.com";
        private const string _smtpHost = "webmail.itlvn.com"; //"email-smtp.ap-southeast-2.amazonaws.com";
        private const string _smptUser = "info.fms"; //"AKIA2AI6JMUOVFIQJQXN";
        private const string _smtpPassword = "ITPr0No1!"; //"BPHb4U8b6yCmJ7W4QB095djPHL75tQUfcXLOCGL99WKP";

        public static bool Send(string subject, string body, List<string> toEmails, List<string> attachments, List<string> emailCCs, List<string> emailBCC = null)
        {
            string receivers = "";
            string CCs = "";
            string BCCs = "";

            string description = "";
            bool result = true;

            MailAddress emailFrom = new MailAddress(_emailFrom);
            MailMessage message = new MailMessage();

            message.From = emailFrom;
            try
            {
                if (toEmails != null && toEmails.Count() > 0)
                {
                    toEmails = GetListEmailValid(toEmails);
                    foreach (string ToEmail in toEmails)
                    {
                        if (IsValidEmail(ToEmail))
                        {
                            MailAddress EmailTo = new MailAddress(ToEmail);
                            receivers += EmailTo.Address + ", ";
                            message.To.Add(EmailTo.Address);
                        }
                    }
                }

                message.IsBodyHtml = true;
                message.Subject = subject;
                message.Body = body;

                // Add a carbon copy recipient.
                if (emailCCs != null)
                {
                    emailCCs = GetListEmailValid(emailCCs);
                    foreach (string EmailCC in emailCCs)
                    {
                        if (IsValidEmail(EmailCC))
                        {
                            MailAddress CC = new MailAddress(EmailCC);
                            CCs += CC.Address + ", ";
                            message.CC.Add(CC.Address);
                        }
                    }
                }

                // Add a carbon copy recipient.
                if (emailBCC != null)
                {
                    emailBCC = GetListEmailValid(emailBCC);
                    foreach (string EmailBCC in emailBCC)
                    {
                        if (IsValidEmail(EmailBCC))
                        {
                            MailAddress BCC = new MailAddress(EmailBCC);
                            BCCs += BCC.Address + ", ";
                            message.Bcc.Add(BCC.Address);
                        }
                    }
                }

                //now attached the file
                if (attachments != null)
                {
                    var webClient = new WebClient();
                    string fileName = string.Empty;
                    foreach (string attachment in attachments)
                    {
                        fileName = Path.GetFileName(attachment);
                        webClient.DownloadFile(attachment, fileName);

                        Attachment attached = new Attachment(fileName, MediaTypeNames.Application.Octet);
                        message.Attachments.Add(attached);
                    }
                }

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

                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                client.Send(message);
                message.Attachments.Dispose();
                client.Dispose();
            }
            catch (Exception ex)
            {
                result = false;
                new LogHelper("LOG_SEND_MAIl", ex.ToString());
                description = ex.Message;
            }
            finally
            {
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
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static DateTime GetDateTime()
        {
            return DateTime.Now;
        }

        /// <summary>
        /// Return valid email list
        /// </summary>
        /// <param name="emails"></param>
        /// <returns></returns>
        private static List<string> GetListEmailValid(List<string> emails)
        {
            var emailsReturn = new List<string>();
            if (emails == null || emails.Count == 0)
            {
                return emailsReturn;
            }
            foreach (var item in emails)
            {
                var email = item.Split(new char[] { ';', '\n' });
                emailsReturn.AddRange(email.Where(x => !string.IsNullOrEmpty(x)));
            }
            if (emailsReturn.Count > 0)
            {
                emailsReturn = emailsReturn.Distinct().ToList();
            }
            return emailsReturn;
        }
    }
}
