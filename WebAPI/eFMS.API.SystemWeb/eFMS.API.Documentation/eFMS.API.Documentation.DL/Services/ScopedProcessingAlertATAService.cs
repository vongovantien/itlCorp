using eFMS.API.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.EF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Services
{
    public class ScopedProcessingAlertATDService: IScopedProcessingAlertATDService
    {
        private IContextBase<CsTransaction> csTransactionRepository;
        private IContextBase<SysUser> userRepository;
        private eFMSDataContextDefault DC => (eFMSDataContextDefault)csTransactionRepository.DC;
        public ScopedProcessingAlertATDService(IContextBase<CsTransaction> csTransaction) 
        {
            csTransactionRepository = csTransaction;
        }

        public async Task AlertATD()
        {
            var dtData = DC.GetViewData<vw_GetShipmentAlertATD>();
            if(dtData.Count > 0)
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
                int number = 0;
                foreach (var item in grpPic)
                {
                    string body = emailTemplate.Body;
                    body = body.Replace("{{PIC}}", item.FirstOrDefault().PIC);

                    string content = string.Empty;

                    string table = emailTemplate.Content;
                    table = table.Replace("{{STT}}", (number + 1).ToString());
                    table = table.Replace("{{JOBNO}}", item.FirstOrDefault().JobNo);
                    table = table.Replace("{{ETD}}", item.FirstOrDefault().ETD.ToString("dd/MM/yyyy"));

                    content += table;
                    number++;

                    body = body.Replace("{{CONTENT}}", content);

                    mailTo = new List<string> { item.FirstOrDefault().Email };
                    mailCC = item.FirstOrDefault().EmailCC.Split(";").ToList();
                    

                    string email = body + footer;
                    var s = SendMail.Send(emailTemplate.Subject, email, mailTo, null, mailCC, emailBCCs);
                }
            }

            Console.WriteLine(JsonConvert.SerializeObject(dtData));
            await Task.Delay(10000);
        }
    }
}
