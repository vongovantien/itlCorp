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
using System.Diagnostics;
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
        private readonly IContextBase<AcctSettlementPayment> settleRepo;
        private readonly IContextBase<SysAttachFileTemplate> attachRepo;
        public EDocService(
            IContextBase<SysImageDetail> repository,
            IContextBase<AcctApproveSettlement> acctApproveSettlementRepository,
            IContextBase<CsShipmentSurcharge> surchargetRepository,
            IContextBase<OpsTransaction> opsTranRepoitory,
            IContextBase<CsTransactionDetail> cstranDeRepository,
            IContextBase<SysImage> sysImageRepository,
            IContextBase<CsTransaction> tranrepository,
            IContextBase<AcctSettlementPayment> settleRepository,
            IContextBase<SysAttachFileTemplate> attachRepository,
        IMapper mapper) : base(repository, mapper)
        {
            acctApproveSettlementRepo = acctApproveSettlementRepository;
            surchargetRepo = surchargetRepository;
            opsTranRepo = opsTranRepoitory;
            cstranDeRepo = cstranDeRepository;
            sysImageRepo = sysImageRepository;
            _csTranRepo = tranrepository;
            settleRepo = settleRepository;
            attachRepo = attachRepository;
        }

        public async Task<HandleState> GenerateEdocSettlement(CreateUpdateSettlementModel model)
        {
            try
            {
                var job = new { };
                var surcharge = await surchargetRepo.GetAsync(x => x.SettlementCode == model.Settlement.SettlementNo);
                var jobCharge = new List<Guid?>();
                surcharge.ToList().ForEach(x =>
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
                if (jobDel.Count() > 0)
                {
                    await DelEdocForJob(model.Settlement.SettlementNo, jobDel);
                }
                if (jobAdd.Count() > 0)
                {
                    await AddEdocForJob(model.Settlement.SettlementNo, jobAdd);
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

        private async Task DelEdocForJob(string BillingNo, List<Guid?> JobIds)
        {
            await DataContext.DeleteAsync(x => x.BillingNo == BillingNo && JobIds.Contains(x.JobId));
        }
        private async Task AddEdocForJob(string BillingNo, List<Guid> JobIds)
        {
            try
            {
                var EDocAdd = await DataContext.GetAsync(x => x.BillingNo == BillingNo);
                var EDocOTH = new List<SysImage>();
                var EDocAdds = EDocAdd.GroupBy(x => x.SysImageId).Select(x => x.FirstOrDefault()).ToList();
                if (BillingNo.Substring(0, 2) == "SM")
                {
                    var BillingId = await settleRepo.GetAsync(x => x.SettlementNo == BillingNo);
                    var EDocAddIds = EDocAdds.Select(z => z.SysImageId.ToString());
                    var ImageOTH = await sysImageRepo.GetAsync(x => x.ObjectId == BillingId.FirstOrDefault().Id.ToString() && !EDocAddIds.Contains(x.Id.ToString()));
                    ImageOTH.ForEach(x =>
                    {
                        EDocOTH.Add(x);
                    });
                }
                JobIds.ForEach(async z =>
                {
                    EDocAdds.ForEach(x =>
                    {
                        var edoc = x;
                        edoc.Id = Guid.NewGuid();
                        edoc.JobId = z;
                        edoc.BillingNo = BillingNo;
                        if (!HaveEdoc((Guid)edoc.SysImageId, z).Result)
                        {
                            DataContext.AddAsync(edoc);
                        }
                    });
                    EDocOTH.ForEach(x =>
                    {
                        var edoc = new SysImageDetail();
                        edoc.Id = Guid.NewGuid();
                        edoc.JobId = z;
                        edoc.BillingNo = BillingNo;
                        edoc.SysImageId = x.Id;
                        edoc.SystemFileName = "OTH_" + x.Name;
                        edoc.UserFileName = x.Name;
                        edoc.UserCreated = x.UserCreated;
                        edoc.UserModified = x.UserModified;
                        edoc.Source = "Shipment";
                        edoc.BillingType = BillingNo.Substring(0, 2);
                        edoc.DocumentTypeId = getDocType(z).Result;
                        if (!HaveEdoc((Guid)edoc.Id, z).Result)
                        {
                            DataContext.AddAsync(edoc);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<int> getDocType(Guid z)
        {
            if (opsTranRepo.Get(x => x.Id == z).FirstOrDefault() != null)
            {
                var opsDoc = await attachRepo.GetAsync(x => x.Code == "OT" && x.TransactionType == "CL" && x.Type == "Accountant");
                return opsDoc.FirstOrDefault().Id;
            }
            else
            {
                var cs = _csTranRepo.Get(x => x.Id == z).FirstOrDefault();
                var csDoc = await attachRepo.GetAsync(x => x.Code == "OT" && x.TransactionType == cs.TransactionType && x.Type == "Accountant");
                return csDoc.FirstOrDefault().Id;
            }
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
