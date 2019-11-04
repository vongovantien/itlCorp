using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsArrivalFrieghtChargeService : RepositoryBase<CsArrivalFrieghtCharge, CsArrivalFrieghtChargeModel>, ICsArrivalFrieghtChargeService
    {
        private readonly IContextBase<CsTransactionDetail> detailTransactionRepository;
        private readonly IContextBase<CsArrivalFrieghtChargeDefault> freightChargeDefaultRepository;
        public CsArrivalFrieghtChargeService(IContextBase<CsArrivalFrieghtCharge> repository, 
            IMapper mapper,
            IContextBase<CsTransactionDetail> detailTransaction,
            IContextBase<CsArrivalFrieghtChargeDefault> freightChargeDefault
            ) : base(repository, mapper)
        {
            detailTransactionRepository = detailTransaction;
            freightChargeDefaultRepository = freightChargeDefault;
        }

        public HandleState SetArrivalChargeDefault(CsArrivalFrieghtChargeDefaultModel model)
        {
            throw new NotImplementedException();
        }

        public HandleState UpdateArrival(CsArrivalFrieghtChargeEditModel model)
        {
            var detailTransaction = detailTransactionRepository.First(x => x.Id == model.HBLID);
            if (detailTransaction == null) return new HandleState("Not found");
            detailTransaction.ArrivalNo = model.ArrivalNo;
            detailTransaction.ArrivalFirstNotice = model.ArrivalFirstNotice;
            detailTransaction.ArrivalSecondNotice = model.ArrivalSecondNotice;
            detailTransaction.ArrivalHeader = model.ArrivalHeader;
            detailTransaction.ArrivalFooter = model.ArrivalFooter;
            var hs = detailTransactionRepository.Update(detailTransaction, x => x.Id == model.HBLID, false);
            if (hs.Success)
            {
                var oldCharges = DataContext.Get(x => x.Hblid == model.HBLID);
                foreach(var item in oldCharges)
                {
                    DataContext.Delete(x => x.Id == item.Id, false);
                }
                foreach (var item in model.CsArrivalFrieghtCharges)
                {
                    item.Id = Guid.NewGuid();
                    item.Hblid = model.HBLID;
                    item.UserCreated = item.UserModified = "admin";
                    item.DatetimeCreated = item.DatetimeModified = DateTime.Now;
                    DataContext.Add(item, false);
                }
                detailTransactionRepository.SubmitChanges();
                DataContext.SubmitChanges();
            }
            return hs;
        }
    }
}
