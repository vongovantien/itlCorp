using AutoMapper;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.SettlementPayment;
using eFMS.API.Accounting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.Services
{
    public class EDocService : RepositoryBase<SysImageDetail, AcctCombineBillingModel>, IEdocService
    {
        readonly IContextBase<AcctAdvanceRequest> _advRequest;
        readonly IContextBase<OpsTransaction> _opsTran;
        readonly IContextBase<CsTransaction> _csTran;
        readonly IContextBase<AcctSettlementPayment> _settle;
        readonly IContextBase<CsShipmentSurcharge> _surcharge;
        readonly IContextBase<SysOffice> _office;
        readonly ICurrentUser _currentUser;

        public EDocService(
            IContextBase<SysImageDetail> repository, 
            IContextBase<AcctAdvanceRequest> advRequest,
            IContextBase<OpsTransaction> opsTran,
            IContextBase<CsTransaction> csTran,
            IContextBase<AcctSettlementPayment> settle,
            IContextBase<CsShipmentSurcharge> surcharge,
            IContextBase<SysOffice> office,
            ICurrentUser currentUser,
            IMapper mapper
            ) : base(repository, mapper)
        {
            _advRequest= advRequest;
            _opsTran= opsTran;
            _csTran= csTran;
            _settle= settle;
            _surcharge= surcharge;
            _office = office;
            _currentUser = currentUser;
        }
        public async Task<List<Guid?>> getListRep(List<Guid?> lstJobId)
        {
            var fromRep = await _office.AnyAsync(x => x.Id == _currentUser.OfficeID && x.OfficeType == "OutSource");
            if (fromRep)
            {
                var result = new List<Guid?>();
                lstJobId.ForEach(async x =>
                {
                    if (await _opsTran.AnyAsync(z => z.ReplicatedId == x))
                    {
                        result.Add(x);
                    }
                });
                return result;
            }
            return lstJobId;
        }

        public async Task<List<Guid?>> filterListDel(List<Guid?> lstJobId, List<string> lstCurrentJobNo)
        {
            var lstCurrentJob = await _opsTran.GetAsync(x => lstCurrentJobNo.Contains(x.JobNo));
            var lstCurrenJobId=lstCurrentJob.Select(x => x.Id).ToList();
            var fromRep = await _office.AnyAsync(x => x.Id == _currentUser.OfficeID && x.OfficeType == "OutSource");
            var result = new List<Guid?>();
            if (fromRep)
            {
                var lstDelRep = await _opsTran.GetAsync(x => lstJobId.Contains(x.Id));
                result = lstJobId.ToList();
                lstJobId.ForEach( x =>
                {
                        if ( _opsTran.Any(z =>lstCurrenJobId.Contains((Guid)z.ReplicatedId)&&z.Id==x))
                        {
                        result.Remove(x);
                        }
                });
            }
            return result;
        }

        public async Task<EdocAccUpdateModel> MapAdvanceRequest(string AdvNo)
        {
            var fromRep = await _office.AnyAsync(x => x.Id == _currentUser.OfficeID && x.OfficeType == "OutSource");
            var adv = await _advRequest.GetAsync(x => x.AdvanceNo == AdvNo);
            var lstCurrADV = adv.GroupBy(x => x.JobId).Select(x => x.FirstOrDefault().JobId).ToList();
            var edoc = await DataContext.GetAsync(x => x.BillingNo == AdvNo);
            var lstCurrEDoc = edoc.GroupBy(x => x.JobId).Select(x => x.FirstOrDefault().JobId).ToList();
            var lstAdvId= ConvertJobdetail(lstCurrADV);
            var lstAdd = lstAdvId.Where(x => !lstCurrEDoc.Contains(x)).ToList();
            var lstDel = lstCurrEDoc.Where(x => !lstAdvId.Contains(x)).ToList();
            var lstCurrJobNo=new List<string>();
            var edocModel = new EdocAccUpdateModel()
            {
                BillingNo = AdvNo,
                BillingType = "Advance",
                ListAdd = lstAdd,
                ListDel= fromRep ? await filterListDel(lstDel, lstCurrADV) : await getListRep(lstDel),
                FromRep = fromRep
            };
            return edocModel;
        }

        public async Task<EdocAccUpdateModel> MapSettleCharge(string settleNo)
        {
            var fromRep = await _office.AnyAsync(x => x.Id == _currentUser.OfficeID && x.OfficeType == "OutSource");
            var surcharge = await _surcharge.GetAsync(x => x.SettlementCode == settleNo);
            var lstCurrSetNo = surcharge.GroupBy(x => x.JobNo).Select(x => x.FirstOrDefault().JobNo).ToList();
            var lstCurrSet = ConvertJobdetail(lstCurrSetNo);
            var edoc=await DataContext.GetAsync(x => x.BillingNo == settleNo);
            var lstCurrEDoc = edoc.GroupBy(x => x.JobId).Select(x => x.FirstOrDefault().JobId).ToList();
            var lstDel = lstCurrEDoc.Where(x => !lstCurrSet.Contains(x)).ToList();
            var lstAdd = lstCurrSet.Where(x => !lstCurrEDoc.Contains(x)).ToList();
            var edocModel = new EdocAccUpdateModel()
            {
                BillingNo = settleNo,
                BillingType = "Settlement",
                ListAdd = lstAdd,
                ListDel = fromRep? await filterListDel(lstDel, lstCurrSetNo) :lstDel,
                FromRep= fromRep
            };
            return edocModel;
        }

        public async Task<EdocAccUpdateModel> MapSOACharge(string soaNo)
        {
            var fromRep = await _office.AnyAsync(x => x.Id == _currentUser.OfficeID && x.OfficeType == "OutSource");
            var surcharge=await _surcharge.GetAsync(x => x.Soano == soaNo||x.PaySoano==soaNo);
            var lstCurrSOANo = surcharge.GroupBy(x => x.JobNo).Select(x => x.FirstOrDefault().JobNo).ToList();
            var lstCurrSOA = ConvertJobdetail(lstCurrSOANo);
            var edoc = await DataContext.GetAsync(x => x.BillingNo == soaNo);
            var lstCurrEDoc = edoc.GroupBy(x => x.JobId).Select(x => x.FirstOrDefault().JobId).ToList();
            var lstDel = lstCurrEDoc.Where(x => !lstCurrSOA.Contains(x)).ToList();
            var lstAdd = lstCurrSOA.Where(x => !lstCurrEDoc.Contains(x)).ToList();
            var edocModel = new EdocAccUpdateModel()
            {
                BillingNo = soaNo,
                BillingType = "SOA",
                ListAdd = lstAdd,
                ListDel = fromRep ? await filterListDel(lstDel, lstCurrSOANo) : lstDel,
                FromRep = fromRep
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
                    var opsId =  _opsTran.Get(x => x.JobNo == jobNo);
                    result.Add(opsId.FirstOrDefault().Id);
                }
                else
                {
                    var csId =  _csTran.Get(x => x.JobNo == jobNo);
                    result.Add(csId.FirstOrDefault().Id);
                }

            });
            return result;
        }
    }
}
