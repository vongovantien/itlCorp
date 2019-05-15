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
    public class SendMailCC
    {
        public static bool Send(string Subject, string Body, List<string> ToEmails, List<string> Attachments, List<string> EmailCCs)
        {
            string SentUser = "";
            string Receivers = "";
            string CCs = "";
            string Description = "";
            bool result = true;

            MailAddress EmailFrom = new MailAddress("tms.ftl@itlvn.com");
            MailMessage message = new MailMessage();

            message.From = EmailFrom;
            if (ToEmails != null && ToEmails.Count() > 0)
            {
                foreach (string ToEmail in ToEmails)
                {
                    MailAddress EmailTo = new MailAddress(ToEmail);
                    Receivers += ToEmail + ", ";
                    message.To.Add(EmailTo);
                }
            }

            message.IsBodyHtml = true;
            message.Subject = Subject;
            message.Body = Body;

            // Add a carbon copy recipient.
            if (EmailCCs != null)
            {
                foreach (string EmailCC in EmailCCs)
                {
                    MailAddress CC = new MailAddress(EmailCC);
                    CCs += EmailCC + ", ";
                    message.CC.Add(CC);
                }
            }

            //now attached the file
            if (Attachments != null)
            {
                foreach (string attachment in Attachments)
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
                new System.Net.NetworkCredential("tms.ftl@itlvn.com",
                "P@ssw0rd","itl");
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
                Description = ex.Message;
            }
            finally
            {
               // InsertEmailHistory(SentUser, Receivers, CCs, "", Subject, result, Description);
            }
            return result;
        }

        //public static bool Send(string Subject, string Body, List<string> ToEmails, List<string> Attachments)
        //{
        //    MailMessage message = new MailMessage();
        //    MailAddress EmailFrom = new MailAddress("tms.ftl@itlvn.com");

        //    message.From = EmailFrom;

        //    if (ToEmails != null)
        //    {
        //        foreach (string ToEmail in ToEmails)
        //        {
        //            MailAddress EmailTo = new MailAddress(ToEmail);
        //            message.To.Add(EmailTo);
        //        }
        //    }

        //    message.IsBodyHtml = true;
        //    message.Subject = Subject;
        //    message.Body = Body;

        //    //now attached the file
        //    if (Attachments != null)
        //    {
        //        foreach (string attachment in Attachments)
        //        {
        //            Attachment attached = new Attachment(attachment, MediaTypeNames.Application.Octet);
        //            message.Attachments.Add(attached);
        //        }
        //    }

        //    SmtpClient client = new SmtpClient();
        //    client.UseDefaultCredentials = false;

        //    client.Host = "webmail.itlvn.com";
        //    client.Port = 587;
        //    client.EnableSsl = true;
        //    client.DeliveryMethod = SmtpDeliveryMethod.Network;
        //    client.Credentials =
        //        new System.Net.NetworkCredential("tms.ftl@itlvn.com",
        //        "P@ssw0rd");
        //    client.Timeout = 20000;

        //    // send message
        //    try
        //    {
        //        ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
        //        client.Send(message);
        //        return true;
        //    }
        //    catch { return false; }
        //}

        public static bool Send(string Subject, string Body, string ToEmail, List<string> Attachments, List<string> EmailCCs)
        {
            List<string> ToEmails = new List<string>(){ ToEmail };
            return Send(Subject, Body, ToEmails, Attachments, EmailCCs); 
        }

        //private static int InsertEmailHistory(string SentUser, string Receivers, string CCs, string BCCs, string Subject, bool Sent, string Description)
        //{
        //    DataService DA = new DataService();
        //    DateTime getDate = GetDateTime();
        //    string sql = string.Format("INSERT INTO sysSentEmailHistory (SentUser, Receivers, CCs, BCCs, "
        //        + " Subject, Sent, SentDateTime, Description) "
        //        + " VALUES (N'{0}', N'{1}', N'{2}', N'{3}', N'{4}', '{5}', '{6}', N'{7}')"
        //        , SentUser
        //        , Receivers
        //        , CCs
        //        , BCCs
        //        , Subject
        //        , Sent.ToString()
        //        , string.Format("{0:yyyy-MM-dd HH:mm:ss}", getDate)
        //        , Description);

        //    return DA.ExcuteQuery(sql, CommandType.Text);
        //}

        private static DateTime GetDateTime()
        {
           
            return DateTime.Now;
        }
    }
}
