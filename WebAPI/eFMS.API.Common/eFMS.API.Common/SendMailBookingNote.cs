using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net.Mime;
using eFMS.API.Common.Globals;

namespace eFMS.API.Common
{
    public class SendMailBookingNote
    {
        //public static void Send(string Subject, string Body, List<string> Attachments, string EmailCustomer)
        //{
        //    // Create the Outlook application.
        //    Outlook.Application oApp = new Outlook.Application();
        //    // Create a new mail item.
        //    Outlook.MailItem oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
        //    // Set HTMLBody. 
        //    //add the body of the email
        //    oMsg.HTMLBody = Body;
        //    //Add an attachment.
        //    String sDisplayName = "MyAttachment";
        //    int iPosition = (int)oMsg.Body.Length + 1;
        //    int iAttachType = (int)Outlook.OlAttachmentType.olByValue;
        //    //now attached the file
        //    foreach (string attachment in Attachments)
        //    {
        //        Outlook.Attachment oAttach = oMsg.Attachments.Add(attachment, iAttachType, iPosition, sDisplayName);
        //    }
        //    //Subject line
        //    oMsg.Subject = Subject;
        //    // Add a recipient.
        //    Outlook.Recipients oRecips = (Outlook.Recipients)oMsg.Recipients;
        //    // Change the recipient in the next line if necessary.
        //    Outlook.Recipient oRecip = (Outlook.Recipient)oRecips.Add(EmailCustomer);
            
        //    oRecip.Resolve();
        //    // Send.
        //    oMsg.Send();
        //    // Clean up.
        //    oRecip = null;
        //    oRecips = null;
        //    oMsg = null;
        //    oApp = null;
        //}

        //public static bool SendReplyTo(string Subject, string Body, string ReplyToEmail, List<string> Attachments, string EmailCustomer)
        //{
        //    MailAddress EmailFrom = new MailAddress("tms.ftl@itlvn.com");
        //    MailAddress EmailTo = new MailAddress(EmailCustomer);
        //    MailMessage message = new MailMessage(EmailFrom, EmailTo);

        //    message.IsBodyHtml = true;
        //    message.Subject = Subject;
        //    message.Body = Body;
        //    // Add a carbon copy recipient.
        //    MailAddress copy = new MailAddress(ReplyToEmail);
        //    message.ReplyToList.Add(copy);

        //    //now attached the file
        //    foreach (string attachment in Attachments)
        //    {
        //        Attachment attached = new Attachment(attachment, MediaTypeNames.Application.Octet);
        //        message.Attachments.Add(attached);
        //    }

        //    SmtpClient client = new SmtpClient();
        //    client.UseDefaultCredentials = false;

        //    client.Host = "192.168.2.3";
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

        public static bool SendReplyTo(string Subject, string Body, string ReplyToEmail, List<string> Attachments, string EmailCustomer, List<string> EmailCCs)
        {
            MailAddress EmailFrom = new MailAddress("tms.ftl@itlvn.com");
            MailAddress EmailTo = new MailAddress(EmailCustomer);
            MailMessage message = new MailMessage(EmailFrom, EmailTo);

            message.IsBodyHtml = true;
            message.Subject = Subject;
            message.Body = Body;
            // Add a carbon copy recipient.
            MailAddress copy = new MailAddress(ReplyToEmail);
            message.ReplyToList.Add(copy);

            //now attached the file
            foreach (string attachment in Attachments)
            {
                Attachment attached = new Attachment(attachment, MediaTypeNames.Application.Octet);
                message.Attachments.Add(attached);
            }

            // Add a carbon copy recipient.
            if (EmailCCs != null)
            {
                foreach (string EmailCC in EmailCCs)
                {
                    MailAddress CC = new MailAddress(EmailCC);
                    message.CC.Add(CC);
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
                "P@ssw0rd", "itl");
            client.Timeout = 20000;

            // send message
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                client.Send(message);
                message.Attachments.Dispose();
                client.Dispose();

                return true;
            }
            catch { return false; }
        }
    }
}
