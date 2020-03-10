using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Linq;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsAirWayBillService : RepositoryBase<CsAirWayBill, CsAirWayBillModel>, ICsAirWayBillService
    {
        private readonly ICsDimensionDetailService dimensionDetailService;
        private readonly ICsShipmentOtherChargeService shipmentOtherChargeService;
        private readonly ICurrentUser currentUser;
        public CsAirWayBillService(IContextBase<CsAirWayBill> repository, 
            IMapper mapper,
            ICsDimensionDetailService dimensionService,
            ICsShipmentOtherChargeService otherChargeService,
            ICurrentUser currUser) : base(repository, mapper)
        {
            dimensionDetailService = dimensionService;
            shipmentOtherChargeService = otherChargeService;
            currentUser = currUser;
        }

        public CsAirWayBillModel GetBy(Guid jobId)
        {
            var result = Get(x => x.JobId == jobId).FirstOrDefault();
            if (result == null) return null;
            result.DimensionDetails = dimensionDetailService.Get(x => x.AirWayBillId == result.Id).ToList();
            result.OtherCharges = shipmentOtherChargeService.Get(x => x.JobId == jobId).ToList();
            return result;
        }

        public override HandleState Add(CsAirWayBillModel entity)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var model = mapper.Map<CsAirWayBill>(entity);
                    model.Id = Guid.NewGuid();
                    model.DatetimeModified = DateTime.Now;
                    model.UserModified = currentUser.UserID;
                    var hs = DataContext.Add(model);
                    if (hs.Success)
                    {
                        if(entity.DimensionDetails != null)
                        {
                            entity.DimensionDetails.ForEach(x => {
                                x.UserCreated = currentUser.UserID;
                                x.DatetimeCreated = DateTime.Now;
                                x.Id = Guid.NewGuid();
                                x.AirWayBillId = model.Id;
                            });
                            var hsDimensions = dimensionDetailService.Add(entity.DimensionDetails);
                        }
                        if(entity.OtherCharges != null)
                        {
                            entity.OtherCharges.ForEach(x => {
                                x.UserModified = currentUser.UserID;
                                x.DatetimeModified = DateTime.Now;
                                x.Id = Guid.NewGuid();
                                x.JobId = model.JobId;
                            });
                            var hsOtherCharges = shipmentOtherChargeService.Add(entity.OtherCharges);
                        }
                    }
                    trans.Commit();
                    return hs;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        public HandleState Update(CsAirWayBillModel model)
        {
            var bill = mapper.Map<CsAirWayBill>(model);
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Update(bill, x => x.Id == model.Id);
                    if (hs.Success)
                    {
                        if(model.DimensionDetails != null)
                        {
                            var hsdimensions = dimensionDetailService.UpdateAirWayBill(model.DimensionDetails, model.Id);
                        }
                        if(model.OtherCharges != null)
                        {
                            var hsOtherCharges = shipmentOtherChargeService.UpdateOtherChargeMasterBill(model.OtherCharges, model.Id);
                        }
                    }
                    trans.Commit();
                    return hs;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }
    }
}
