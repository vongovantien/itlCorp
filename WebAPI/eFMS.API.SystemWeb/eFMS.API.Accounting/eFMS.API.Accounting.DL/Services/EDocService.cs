using AutoMapper;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace eFMS.API.Accounting.DL.Services
{
    public class EDocService : RepositoryBase<SysImageDetail, AcctCombineBillingModel>, IEdocService
    {
        readonly IContextBase<AcctAdvanceRequest> _advRequest;
        readonly IContextBase<OpsTransaction> _opsTran;
        readonly IContextBase<CsTransaction> _csTran;
        public EDocService(
            IContextBase<SysImageDetail> repository, 
            IContextBase<AcctAdvanceRequest> advRequest,
            IContextBase<OpsTransaction> opsTran,
            IContextBase<CsTransaction> csTran,
            IMapper mapper
            ) : base(repository, mapper)
        {
            _advRequest= advRequest;
            _opsTran= opsTran;
            _csTran= csTran;
        }

        public EdocAccUpdateModel MapAdvanceRequest(AcctAdvancePaymentModel model)
        {
            var lstAdd = model.AdvanceRequests.Where(x => x.Id == Guid.Empty).Select(x => x.JobId).ToList();
            var requestUpdate = model.AdvanceRequests.Where(x => x.Id != Guid.Empty).Select(x => x.JobId).ToList();
            var lstCurrADV = _advRequest.Get(x => x.AdvanceNo == model.AdvanceNo).Select(x => x.JobId).ToList();
            var lstCurrId=ConvertJobdetail(lstCurrADV).Select(x=>x.JobId).ToList();
            var lstDel = DataContext.Get(x=>x.BillingNo==model.AdvanceNo && !lstCurrId.Contains(x.JobId)).Select(x=>x.Id).ToList();
            var edocModel = new EdocAccUpdateModel()
            {
                BillingNo = model.AdvanceNo,
                BillingType = "Advance",
                ListAdd = ConvertJobdetail(lstAdd),
                ListDel = lstDel,
            };
            return edocModel;
        }

        private List<EdocJobModel> ConvertJobdetail(List<string> jobNos)
        {
            var result=new List<EdocJobModel>();
            jobNos.ForEach(jobNo =>
            {
                if (jobNo.Contains("LOG"))
                {
                    var opsId = _opsTran.Get(x => x.JobNo == jobNo).FirstOrDefault().Id;
                    result.Add(new EdocJobModel()
                    {
                        JobId = opsId,
                        TransactionType = "CL"
                    });
                }
                else
                {
                    var csJob = _csTran.Get(x => x.JobNo == jobNo).FirstOrDefault();
                    result.Add(new EdocJobModel()
                    {
                        JobId = csJob.Id,
                        TransactionType = csJob.TransactionType,
                    });
                }
                
            });
            return result;
        }
    }
}
