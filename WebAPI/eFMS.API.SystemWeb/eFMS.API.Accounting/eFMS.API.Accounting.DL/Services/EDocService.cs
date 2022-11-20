using AutoMapper;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.SettlementPayment;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.EntityFrameworkCore;
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
        private readonly IContextBase<CsTransaction> csTranRepo;
        private readonly IContextBase<SysImage> sysImageRepo;
        public EDocService(
            IContextBase<SysImageDetail> repository,
            IContextBase<AcctApproveSettlement> acctApproveSettlementRepository,
            IContextBase<CsShipmentSurcharge> surchargetRepository,
            IContextBase<OpsTransaction> opsTranRepoitory,
            IContextBase<CsTransactionDetail> cstranDeRepository,
            IContextBase<SysImage> sysImageRepository,
            IMapper mapper) : base(repository, mapper)
        {
            acctApproveSettlementRepo = acctApproveSettlementRepository;
            surchargetRepo = surchargetRepository;
            opsTranRepo = opsTranRepoitory;
            cstranDeRepo = cstranDeRepository;
            sysImageRepo = sysImageRepository;
        }

        public async Task<HandleState> GenerateEdoc(CreateUpdateSettlementModel model)
        {
            var surcharge = await surchargetRepo.GetAsync(x => x.SettlementCode == model.Settlement.SettlementNo);
            var jobCharge = new List<string>();
            surcharge.ForEach(x =>
            {
                if (x.TransactionType == "CL")
                {
                    jobCharge.Add(opsTranRepo.Get(z => z.JobNo == x.JobNo).FirstOrDefault().Id.ToString());
                };
                jobCharge.Add(csTranRepo.Get(z => z.JobNo == x.JobNo).FirstOrDefault().Id.ToString());
            });
            jobCharge.Distinct();
            var jobModel  = model.ShipmentCharge.Select(x => x.JobId).Distinct().ToList();
            var jobDel = jobCharge.Where(x => !jobModel.Contains(x)).Distinct().ToList();
            var jobAdd = jobModel.Where(y => !jobCharge.Contains(y)).Distinct().ToList();
            if (jobDel.Count()>0)
            {
                await DelEdocForJobDel(model.Settlement.SettlementNo, jobDel);
            }
            if (jobAdd.Count() > 0)
            {
                await AddEdocForJobDel(model.Settlement.SettlementNo,jobAdd);
            }
            return new HandleState();
        }


        private async Task DelEdocForJobDel(string BillingNo,List<string> JobIds)
        {
            await DataContext.DeleteAsync(x => x.BillingNo == BillingNo && JobIds.Contains(x.JobId.ToString()));
        }
        private async Task AddEdocForJobDel(string BillingNo, List<string> JobIds)
        {
            var EDocAdd = await DataContext.GetAsync(x => x.BillingNo == BillingNo && !JobIds.Contains(x.JobId.ToString()));
            var EDocAdds = EDocAdd.GroupBy(x => x.SysImageId).Select(x => x.FirstOrDefault()).ToList();
            JobIds.ForEach(z =>
            {
                EDocAdds.ForEach(x =>
                {
                    var edoc = x;
                    edoc.JobId = Guid.Parse(z);
                    edoc.BillingNo = BillingNo;
                    DataContext.AddAsync(edoc);
                });
            });
        }
    }
}
