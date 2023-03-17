using AutoMapper;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.SettlementPayment;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Common;
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
        readonly IContextBase<AcctSettlementPayment> _settle;
        readonly IContextBase<CsShipmentSurcharge> _surcharge;
        public EDocService(
            IContextBase<SysImageDetail> repository, 
            IContextBase<AcctAdvanceRequest> advRequest,
            IContextBase<OpsTransaction> opsTran,
            IContextBase<CsTransaction> csTran,
            IContextBase<AcctSettlementPayment> settle,
            IContextBase<CsShipmentSurcharge> surcharge,
            IMapper mapper
            ) : base(repository, mapper)
        {
            _advRequest= advRequest;
            _opsTran= opsTran;
            _csTran= csTran;
            _settle= settle;
            _surcharge= surcharge;
        }

        public EdocAccUpdateModel MapAdvanceRequest(string AdvNo)
        {
            var lstCurrADV = _advRequest.Get(x => x.AdvanceNo == AdvNo).GroupBy(x => x.JobId).Select(x => x.FirstOrDefault().JobId).ToList();
            var lstCurrEDoc = DataContext.Get(x => x.BillingNo == AdvNo).GroupBy(x => x.JobId).Select(x => x.FirstOrDefault().JobId).ToList();
            var lstAdvId= ConvertJobdetail(lstCurrADV);
            var lstAdd = lstAdvId.Where(x => !lstCurrEDoc.Contains(x)).ToList();
            var lstDel = lstCurrEDoc.Where(x => !lstAdvId.Contains(x)).ToList();
            var edocModel = new EdocAccUpdateModel()
            {
                BillingNo = AdvNo,
                BillingType = "Advance",
                ListAdd = lstAdd,
                ListDel = lstDel,
            };
            return edocModel;
        }

        public EdocAccUpdateModel MapSettleCharge(string settleNo)
        {
            var lstCurrSetNo = _surcharge.Get(x => x.SettlementCode == settleNo).GroupBy(x => x.JobNo).Select(x => x.FirstOrDefault().JobNo).ToList();
            var lstCurrSet = ConvertJobdetail(lstCurrSetNo);
            var lstCurrEDoc = DataContext.Get(x => x.BillingNo == settleNo).GroupBy(x => x.JobId).Select(x => x.FirstOrDefault().JobId).ToList();
            var lstDel = lstCurrEDoc.Where(x => !lstCurrSet.Contains(x)).ToList();
            var lstAdd = lstCurrSet.Where(x => !lstCurrEDoc.Contains(x)).ToList();
            var edocModel = new EdocAccUpdateModel()
            {
                BillingNo = settleNo,
                BillingType = "Settlement",
                ListAdd = lstAdd,
                ListDel = lstDel,
            };
            return edocModel;
        }

        public EdocAccUpdateModel MapSOACharge(string soaNo)
         {
            var lstCurrSOANo = _surcharge.Get(x => x.Soano == soaNo||x.PaySoano==soaNo).GroupBy(x => x.JobNo).Select(x => x.FirstOrDefault().JobNo).ToList();
            var lstCurrSOA = ConvertJobdetail(lstCurrSOANo).ToList();
            var lstCurrEDoc = DataContext.Get(x => x.BillingNo == soaNo).GroupBy(x => x.JobId).Select(x => x.FirstOrDefault().JobId).ToList();
            var lstDel = lstCurrEDoc.Where(x => !lstCurrSOA.Contains(x)).ToList();
            var lstAdd = lstCurrSOA.Where(x => !lstCurrEDoc.Contains(x)).ToList();
            var edocModel = new EdocAccUpdateModel()
            {
                BillingNo = soaNo,
                BillingType = "SOA",
                ListAdd = lstAdd,
                ListDel = lstDel,
            };
            return edocModel;
        }

        private List<Guid?> ConvertJobdetail(List<string> jobNos)
        {
            var result = new List<Guid?>();
            jobNos.ForEach(jobNo =>
            {
                if (jobNo.Contains("LOG"))
                {
                    var opsId = _opsTran.Get(x => x.JobNo == jobNo).FirstOrDefault().Id;
                    result.Add(opsId);
                }
                else
                {
                    var csId = _csTran.Get(x => x.JobNo == jobNo).FirstOrDefault().Id;
                    result.Add(csId);
                }

            });
            return result;
        }
    }
}
