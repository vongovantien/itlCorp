﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace eFMSWindowService.Helpers
{
    public static class SendMailHelper
    {
        public const string _emailFrom = "noreply-efms@itlvn.com"; //"info.fms@itlvn.com";
        private const string _smtpHost = "email-smtp.ap-southeast-2.amazonaws.com"; //"webmail.itlvn.com";
        private const string _smptUser = "AKIA2AI6JMUOVFIQJQXN"; //"info.fms";
        private const string _smtpPassword = "BPHb4U8b6yCmJ7W4QB095djPHL75tQUfcXLOCGL99WKP"; //"ITPr0No1!";

        public static bool Send(string subject, string body, List<string> toEmails, List<string> attachments = null, List<string> emailCCs = null, List<string> emailBCC = null)
        {
            string receivers = "";
            string CCs = "";
            string BCCs = "";

            string description = "";
            bool result = true;

            MailAddress emailFrom = new MailAddress(_emailFrom);
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

            client.Host = _smtpHost;
            client.Port = 587;
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials =
                new NetworkCredential(_smptUser,
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
            }
            return result;
        }
    }
}
