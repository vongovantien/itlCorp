using AutoMapper;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.SettlementPayment;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Helpers;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<HandleState> GenerateEdocSettlement(CreateUpdateSettlementModel model)
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
                var jobIds = model.ShipmentCharge.Where(x => x.Id == Guid.Empty).ToList();
                var jobAdd = new List<Guid>();
                jobIds.ForEach(x =>
                {
                    if (x.JobId.Contains("LOG"))
                    {
                        jobAdd.Add(opsTranRepo.Get(z => z.JobNo == x.JobId).FirstOrDefault().Id);
                    }
                    else
                    {
                        jobAdd.Add(_csTranRepo.Get(z => z.JobNo == x.JobId).FirstOrDefault().Id);
                    }
                });
                var jobExist = new List<Guid>();
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

        private async Task<bool> HaveEdoc(Guid imageId, Guid jobId)
        {
            var edocExist = await DataContext.GetAsync(x => x.JobId == jobId && x.SysImageId == imageId);
            if (edocExist.FirstOrDefault() != null)
            {
                return true;
            }
            return false;
        }

        private async Task DelEdocForJobDel(string BillingNo, List<Guid?> JobIds)
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
                    if (!HaveEdoc((Guid)edoc.SysImageId, z).Result)
                    {
                        DataContext.Add(edoc, false);
                    }
                });
            });
            DataContext.SubmitChanges();
        }

        public async Task DeleteEdocByBillingNo(string billingNo)
        {
            await DataContext.DeleteAsync(x => x.BillingNo == billingNo);
        }

        public Task<HandleState> GenerateEdocSOA(AcctSoaModel model)
        {
            throw new NotImplementedException();
        }

        public Task<HandleState> GenerateEdocAdvance(AcctAdvancePaymentModel model)
        {
            throw new NotImplementedException();
        }
    }
}
