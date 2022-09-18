using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Services
{
    public class ScopedProcessingAlertATDService: IScopedProcessingAlertATDService
    {
        private IContextBase<CsTransaction> csTransactionRepository;
        private IContextBase<SysUser> userRepository;
        public ScopedProcessingAlertATDService(IContextBase<CsTransaction> csTransaction) 
        {
            csTransactionRepository = csTransaction;
        }

        public async Task AlertATD()
        {
            var transactionTypeExport = new List<string> {
                DocumentConstants.AE_SHIPMENT,
                DocumentConstants.SEC_SHIPMENT,
                DocumentConstants.SEF_SHIPMENT,
                DocumentConstants.SEL_SHIPMENT,
            };

            Expression<Func<CsTransaction, bool>> query = x => (
                    transactionTypeExport.Contains(x.TransactionType)
                    && x.Etd.Value.Date >= DateTime.Now.Date
                    && x.Atd == null
                    && x.ServiceDate.Value.Year == 2022
                    && x.ServiceDate.Value.Month >= 9
            );

            var jobs = csTransactionRepository.Get(query);

            var grpPic = jobs.GroupBy(x => new { x.PersonIncharge }).ToList();

            Console.WriteLine(JsonConvert.SerializeObject(grpPic));
            await Task.Delay(3000);
        }
    }
}
