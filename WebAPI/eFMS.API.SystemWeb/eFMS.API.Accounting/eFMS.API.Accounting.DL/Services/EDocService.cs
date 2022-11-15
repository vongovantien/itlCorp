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

        public HandleState RegenEDocSettle(CreateUpdateSettlementModel model)
        {
            var surcharge = surchargetRepo.Get(x => x.SettlementCode == model.Settlement.SettlementNo).ToList();
            var edocExist = DataContext.Get(x => x.BillingNo == model.Settlement.SettlementNo).GroupBy(x=>x.JobId).ToList();
            //var imageExist = await sysImageRepo.GetAsync(x => x.ObjectId == model.Settlement.Id.ToString() && x.Folder == "Settlement");
            var jobOPSSMExist = opsTranRepo.Get(ops => surcharge.Select(x => x.Hblid).Contains(ops.Hblid)).ToList();
            var jobCSSMExist = cstranDeRepo.Get(cs => surcharge.Select(x => x.Hblid).Contains(cs.Id)).ToList();
            // get Job Don't have Edoc
            var jobOPSNotEdoc = jobOPSSMExist.Where(x => !(edocExist.Select(y => (Guid)y.FirstOrDefault().JobId).Contains(x.Id))).Select(x=>x.Id).ToList();
            var jobCsNotEdoc = jobCSSMExist.Where(x => !(edocExist.Select(y => (Guid)y.FirstOrDefault().JobId).Contains(x.Id))).Select(x => x.JobId).ToList();
            // update Edoc OPS deon't have
            jobOPSNotEdoc.ForEach(x =>
            {
                edocExist.Where(y => y.FirstOrDefault().JobId != x).ToList().ForEach(z =>
                {
                    z.FirstOrDefault().JobId = x;
                    DataContext.Add(z, false);
                });

            });
            // update Edoc CS deon't have
            jobCsNotEdoc.ForEach(x =>
            {
                edocExist.Where(y => y.FirstOrDefault().JobId != x).ToList().ForEach(z =>
                {
                    z.FirstOrDefault().JobId = x;
                    DataContext.Add(z, false);
                });

            });
            //get Job Del Edoc
            var jobOPSDelEdoc = edocExist.Where(x => !(jobOPSSMExist.Select(y=>y.Id).Contains((Guid)x.FirstOrDefault().JobId))).ToList();
            var jobCsDelEdoc = edocExist.Where(x => !(jobCSSMExist.Select(y => y.JobId).Contains((Guid)x.FirstOrDefault().JobId))).ToList();
            //delete edoc
            //jobOPSDelEdoc.ForEach(x =>
            //{
            //        DataContext.Delete(z=>z.JobId==x.FirstOrDefault().Id, false);
            //});
            //jobCsDelEdoc.ForEach(x =>
            //{
            //    DataContext.Delete(z => z.JobId == x.FirstOrDefault().JobId, false);
            //});

            var result= DataContext.SubmitChanges();

            if (result.Success)
            {
                return new HandleState("Can't Gen Edoc");
            }
            return new HandleState();
        }
    }
}
