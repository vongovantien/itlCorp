using eFMS.API.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Services
{
    public class ScopedProcessingAlertService: IScopedProcessingAlertService
    {
        private IContextBase<CsTransaction> csTransactionRepository;
        private eFMSDataContextDefault DC => (eFMSDataContextDefault)csTransactionRepository.DC;
        public ScopedProcessingAlertService(IContextBase<CsTransaction> csTransaction) 
        {
            csTransactionRepository = csTransaction;
        }

        public void AlertATD()
        {
            var dtData = DC.GetViewData<vw_GetShipmentAlertATD>();
            if (dtData.Count > 0)
            {
                var emailTemplate = DC.SysEmailTemplate.Where(x => x.Code == "OPEX-ALERT-ATD").FirstOrDefault();

                var mailTo = new List<string> { };
                var mailCC = new List<string> { };
                List<string> emailBCCs = new List<string>();
                var emailBcc = DC.ExecuteFuncScalar("[dbo].[fn_GetEmailBcc]");
                if (emailBcc != null)
                {
                    emailBCCs = emailBcc.ToString().Split(";").ToList();
                }
                string subject = emailTemplate.Subject;
                string footer = emailTemplate.Footer;

                var grpPic = dtData.GroupBy(x => new { x.PIC }).ToList();
                foreach (var grp in grpPic)
                {
                    string body = emailTemplate.Body;
                    body = body.Replace("{{PIC}}", grp.FirstOrDefault().PIC);

                    string tBody = string.Empty;

                    var listData = grp.Select(x => x).ToList();
                    int number = 0;
                    foreach (var item in listData)
                    {
                        string tr = emailTemplate.Content;
                        tr = tr.Replace("{{STT}}", (number + 1).ToString());
                        tr = tr.Replace("{{JOBNO}}", item.JobNo);
                        tr = tr.Replace("{{ETD}}", item.ETD.ToString("dd/MM/yyyy"));

                        tBody += tr;
                        number++;
                    }
                    mailTo = new List<string> { grp.FirstOrDefault().Email };
                    mailCC = grp.FirstOrDefault().EmailCC.Split(";").ToList();
                    body = body.Replace("{{CONTENT}}", tBody);

                    string email = body + footer;
                    var sendMailResult = SendMail.Send(emailTemplate.Subject, email, mailTo, null, mailCC, emailBCCs);
                    #region --- Ghi Log Send Mail ---
                    var logSendMail = new SysSentEmailHistory
                    {
                        SentUser = SendMail._emailFrom,
                        Receivers = string.Join("; ", mailTo),
                        Ccs = string.Join("; ", mailCC),
                        Subject = emailTemplate.Subject,
                        Sent = sendMailResult,
                        SentDateTime = DateTime.Now,
                        Body = email
                    };
                    var hsLogSendMail = DC.SysSentEmailHistory.Add(logSendMail);
                    #endregion --- Ghi Log Send Mail ---
                }
            }
        }

        public List<vw_GetShipmentAlertATD> GetAlertATDData()
        {
            var dtData = DC.GetViewData<vw_GetShipmentAlertATD>();
            return dtData;
        }

        public void AlertATA()
        {
            var dtData = DC.GetViewData<vw_GetShipmentAlertATA>();
            if (dtData.Count > 0)
            {
                var emailTemplate = DC.SysEmailTemplate.Where(x => x.Code == "OPEX-ALERT-ATA").FirstOrDefault();

                var mailTo = new List<string> { };
                var mailCC = new List<string> { };
                List<string> emailBCCs = new List<string>();
                var emailBcc = DC.ExecuteFuncScalar("[dbo].[fn_GetEmailBcc]");
                if (emailBcc != null)
                {
                    emailBCCs = emailBcc.ToString().Split(";").ToList();
                }
                string subject = emailTemplate.Subject;
                string footer = emailTemplate.Footer;

                var grpPic = dtData.GroupBy(x => new { x.PIC }).ToList();
                foreach (var grp in grpPic)
                {
                    string body = emailTemplate.Body;
                    body = body.Replace("{{PIC}}", grp.FirstOrDefault().PIC);

                    string tBody = string.Empty;

                    var listData = grp.Select(x => x).ToList();
                    int number = 0;
                    foreach (var item in listData)
                    {
                        string tr = emailTemplate.Content;
                        tr = tr.Replace("{{STT}}", (number + 1).ToString());
                        tr = tr.Replace("{{JOBNO}}", item.JobNo);
                        tr = tr.Replace("{{ETA}}", item.ETA.ToString("dd/MM/yyyy"));

                        tBody += tr;
                        number++;
                    }
                    mailTo = new List<string> { grp.FirstOrDefault().Email };
                    mailCC = grp.FirstOrDefault().EmailCC.Split(";").ToList();
                    body = body.Replace("{{CONTENT}}", tBody);

                    string email = body + footer;
                    var sendMailResult = SendMail.Send(emailTemplate.Subject, email, mailTo, null, mailCC, emailBCCs);
                    #region --- Ghi Log Send Mail ---
                    var logSendMail = new SysSentEmailHistory
                    {
                        SentUser = SendMail._emailFrom,
                        Receivers = string.Join("; ", mailTo),
                        Ccs = string.Join("; ", mailCC),
                        Subject = emailTemplate.Subject,
                        Sent = sendMailResult,
                        SentDateTime = DateTime.Now,
                        Body = email
                    };
                    var hsLogSendMail = DC.SysSentEmailHistory.Add(logSendMail);
                    #endregion --- Ghi Log Send Mail ---
                }
            }
        }

        public List<vw_GetShipmentAlertATA> GetAlertATAData()
        {
            var dtData = DC.GetViewData<vw_GetShipmentAlertATA>();
            return dtData;
        }
    }
}
