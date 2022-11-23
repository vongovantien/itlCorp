using AutoMapper;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.SettlementPayment;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Helpers;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Remotion.Linq.Clauses.ResultOperators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.Services
{
    public class EDocService : RepositoryBase<SysImageDetail, SysImageModel>, IEDocService
    {
        private readonly IContextBase<AcctApproveSettlement> acctApproveSettlementRepo;
        private readonly IContextBase<CsShipmentSurcharge> surchargetRepo;
        private readonly IContextBase<OpsTransaction> opsTranRepo;
        private readonly IContextBase<CsTransactionDetail> cstranDeRepo;
        private readonly IContextBase<CsTransaction> _csTranRepo;
        private readonly IContextBase<SysImage> sysImageRepo;
        public EDocService(
            IContextBase<SysImageDetail> repository,
            IContextBase<AcctApproveSettlement> acctApproveSettlementRepository,
            IContextBase<CsShipmentSurcharge> surchargetRepository,
            IContextBase<OpsTransaction> opsTranRepoitory,
            IContextBase<CsTransactionDetail> cstranDeRepository,
            IContextBase<SysImage> sysImageRepository,
            IContextBase<CsTransaction> tranrepository,
            IMapper mapper) : base(repository, mapper)
        {
            acctApproveSettlementRepo = acctApproveSettlementRepository;
            surchargetRepo = surchargetRepository;
            opsTranRepo = opsTranRepoitory;
            cstranDeRepo = cstranDeRepository;
            sysImageRepo = sysImageRepository;
            _csTranRepo = tranrepository;
        }

        public async Task<HandleState> GenerateEdoc(CreateUpdateSettlementModel model)
        {
            try
            {
                var surcharge = surchargetRepo.GetAsync(x => x.SettlementCode == model.Settlement.SettlementNo);
                var jobCharge = new List<Guid?>();
                surcharge.Result.ToList().ForEach(x =>
                {
                    if (x.TransactionType == "CL")
                    {
                        jobCharge.Add(opsTranRepo.Get(z => z.JobNo == x.JobNo).FirstOrDefault().Id);
                    }
                    else
                    {
                        jobCharge.Add(_csTranRepo.Get(z => z.JobNo == x.JobNo).FirstOrDefault().Id);
                    }

                });
                jobCharge.Distinct();
                var jobModel = model.ShipmentCharge.Select(x => x.ShipmentId).Distinct().ToList();
                var jobDel = jobCharge.Where(x => !jobModel.Contains((Guid)x)).Distinct().ToList();
                var jobAddHBLs = model.ShipmentCharge.Where(x => x.Id == Guid.Empty).Select(x=>x.Hblid);
                var jobIds = surchargetRepo.Get(x => jobAddHBLs.Contains(x.Hblid)).Select(x => new { x.JobNo,x.TransactionType }).GroupBy(x=>x.JobNo).ToList();
                var jobAdd = new List<Guid>();
                jobIds.ForEach(x =>
                {
                    if (x.FirstOrDefault()?.TransactionType == "CL")
                    {
                        jobAdd.Add(opsTranRepo.Get(z => z.JobNo == x.FirstOrDefault().JobNo).FirstOrDefault().Id);
                    }
                    else
                    {
                        jobAdd.Add(_csTranRepo.Get(z => z.JobNo == x.FirstOrDefault().JobNo).FirstOrDefault().Id);
                    }
                });
                if (jobDel.Count() > 0)
                {
                    await DelEdocForJobDel(model.Settlement.SettlementNo, jobDel);
                }
                if (jobAdd.Count() > 0)
                {
                    await AddEdocForJobDel(model.Settlement.SettlementNo, jobAdd);
                }
                return new HandleState();
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_LOG_GenEdoc", ex.ToString());
                return new HandleState(ex.Message);
            }
        }


        private async Task DelEdocForJobDel(string BillingNo,List<Guid?> JobIds)
        {
            await DataContext.DeleteAsync(x => x.BillingNo == BillingNo && JobIds.Contains(x.JobId));
        }
        private async Task AddEdocForJobDel(string BillingNo, List<Guid> JobIds)
        {
            var EDocAdd = await DataContext.GetAsync(x => x.BillingNo == BillingNo);
            var EDocAdds = EDocAdd.GroupBy(x => x.SysImageId).Select(x => x.FirstOrDefault()).ToList();
            JobIds.ForEach(z =>
            {
                EDocAdds.ForEach(x =>
                {
                    var edoc = x;
                    edoc.Id = Guid.NewGuid();
                    edoc.JobId = z;
                    edoc.BillingNo = BillingNo;
                    DataContext.Add(edoc,false);
                });
            });
            DataContext.SubmitChanges();
        }
    }
}
