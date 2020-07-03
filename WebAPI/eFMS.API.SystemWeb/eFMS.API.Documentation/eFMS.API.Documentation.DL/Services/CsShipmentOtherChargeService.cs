using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsShipmentOtherChargeService : RepositoryBase<CsShipmentOtherCharge, CsShipmentOtherChargeModel>, ICsShipmentOtherChargeService
    {
        private readonly ICurrentUser currentUser;
        public CsShipmentOtherChargeService(IContextBase<CsShipmentOtherCharge> repository, 
            IMapper mapper,
            ICurrentUser currUser) : base(repository, mapper)
        {
            currentUser = currUser;
        }
        
        public HandleState UpdateOtherChargeHouseBill(List<CsShipmentOtherChargeModel> otherCharges, Guid hblId)
        {
            try
            {
                var charges = otherCharges.Where(x => x.Id != Guid.Empty).Select(s => s.Id);
                var idContainersNeedRemove = DataContext.Get(x => x.Hblid == hblId && !charges.Contains(x.Id)).Select(s => s.Id);
                //Delete item of List Container MBL
                if (idContainersNeedRemove != null && idContainersNeedRemove.Count() > 0)
                {
                    var hsDelContHBL = DataContext.Delete(x => idContainersNeedRemove.Contains(x.Id));
                }

                foreach (var charge in otherCharges)
                {
                    //Insert & Update List Container MBL
                    if (charge.Id == Guid.Empty)
                    {
                        charge.Id = Guid.NewGuid();
                        charge.Hblid = hblId;

                        charge.UserModified = currentUser.UserID;
                        charge.DatetimeModified = DateTime.Now;

                        var hsAddDemension = Add(charge, false);
                    }
                    else
                    {
                        charge.Hblid = hblId;
                       
                        charge.UserModified = currentUser.UserID;
                        charge.DatetimeModified = DateTime.Now;
                        var hsUpdateContMBL = Update(charge, x => x.Id == charge.Id, false);
                    }
                }
                DataContext.SubmitChanges();

                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public HandleState UpdateOtherChargeMasterBill(List<CsShipmentOtherChargeModel> otherCharges, Guid jobId)
        {
            try
            {
                var charges = otherCharges.Where(x => x.Id != Guid.Empty).Select(s => s.Id);
                var idContainersNeedRemove = DataContext.Get(x => x.JobId == jobId && !charges.Contains(x.Id)).Select(s => s.Id);
                //Delete item of List Container MBL
                if (idContainersNeedRemove != null && idContainersNeedRemove.Count() > 0)
                {
                    var hsDelContHBL = DataContext.Delete(x => idContainersNeedRemove.Contains(x.Id));
                }

                foreach (var dimesion in otherCharges)
                {
                    //Insert & Update List Container MBL
                    if (dimesion.Id == Guid.Empty)
                    {
                        dimesion.Id = Guid.NewGuid();
                        dimesion.JobId = jobId;

                        dimesion.UserModified = currentUser.UserID;
                        dimesion.DatetimeModified = DateTime.Now;

                        var hsAddDemension = Add(dimesion, false);
                    }
                    else
                    {
                        dimesion.JobId = jobId;
                        dimesion.UserModified = currentUser.UserID;
                        dimesion.DatetimeModified = DateTime.Now;
                        var hsUpdateContMBL = Update(dimesion, x => x.Id == dimesion.Id, false);
                    }
                }
                DataContext.SubmitChanges();

                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
    }
}
