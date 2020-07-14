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
    public class CsDimensionDetailService : RepositoryBase<CsDimensionDetail, CsDimensionDetailModel>, ICsDimensionDetailService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CsTransactionDetail> detailRepository;
        public CsDimensionDetailService(IContextBase<CsDimensionDetail> repository, IMapper mapper,
            ICurrentUser currUser,
            IContextBase<CsTransactionDetail> detailRepo) : base(repository, mapper)
        {
            currentUser = currUser;
            detailRepository = detailRepo;
        }

        public List<CsDimensionDetailModel> GetDIMFromHouseByJob(Guid id)
        {
            var houseBills = detailRepository.Get(x => x.JobId == id);
            List<CsDimensionDetailModel> results = null;
            if (houseBills != null)
            {
                results = new List<CsDimensionDetailModel>();
                foreach (var item in houseBills)
                {
                    var dimensions = Get(x => x.Hblid == item.Id);
                    foreach(var dimension in dimensions)
                    {
                        dimension.Id = Guid.Empty;
                        dimension.Hblid = null;
                        dimension.Mblid = id;
                        results.Add(dimension);
                    }
                }
            }
            return results;
        }

        public HandleState UpdateAirWayBill(List<CsDimensionDetailModel> dimensionDetails, Guid airbillId)
        {
            try
            {
                var listIdOfDimension = dimensionDetails.Where(x => x.Id != Guid.Empty).Select(s => s.Id);
                var idContainersNeedRemove = DataContext.Get(x => x.AirWayBillId == airbillId && !listIdOfDimension.Contains(x.Id)).Select(s => s.Id);
                //Delete item of List Container MBL
                if (idContainersNeedRemove != null && idContainersNeedRemove.Count() > 0)
                {
                    var hsDelContHBL = DataContext.Delete(x => idContainersNeedRemove.Contains(x.Id));
                }

                foreach (var dimesion in dimensionDetails)
                {
                    //Insert & Update List Container MBL
                    if (dimesion.Id == Guid.Empty)
                    {
                        dimesion.Id = Guid.NewGuid();
                        dimesion.AirWayBillId = airbillId;
                        dimesion.UserModified = currentUser.UserID;
                        dimesion.DatetimeModified = DateTime.Now;
                        var hsAddDemension = Add(dimesion, false);
                    }
                    else
                    {
                        dimesion.AirWayBillId = airbillId;
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

        public HandleState UpdateHouseBill(List<CsDimensionDetailModel> dimensionDetails, Guid housebillId)
        {
            try
            {
                var listIdOfDimension = dimensionDetails.Where(x => x.Id != Guid.Empty).Select(s => s.Id);
                var idContainersNeedRemove = DataContext.Get(x => x.Hblid == housebillId && !listIdOfDimension.Contains(x.Id)).Select(s => s.Id);
                //Delete item of List Container MBL
                if (idContainersNeedRemove != null && idContainersNeedRemove.Count() > 0)
                {
                    var hsDelContHBL = DataContext.Delete(x => idContainersNeedRemove.Contains(x.Id));
                }

                foreach (var dimesion in dimensionDetails)
                {
                    //Insert & Update List Container MBL
                    if (dimesion.Id == Guid.Empty)
                    {
                        dimesion.Id = Guid.NewGuid();
                        dimesion.Hblid = housebillId;
                        dimesion.UserModified = currentUser.UserID;
                        dimesion.DatetimeModified = DateTime.Now;
                        var hsAddDemension = Add(dimesion);
                    }
                    else
                    {
                        dimesion.Hblid = housebillId;
                        dimesion.UserModified = currentUser.UserID;
                        dimesion.DatetimeModified = DateTime.Now;
                        var hsUpdateContMBL = Update(dimesion, x => x.Id == dimesion.Id);
                    }
                }
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public HandleState UpdateMasterBill(List<CsDimensionDetailModel> dimensionDetails, Guid masterId)
        {
            try
            {
                var listIdOfDimension = dimensionDetails.Where(x => x.Id != Guid.Empty).Select(s => s.Id);
                var idContainersNeedRemove = DataContext.Get(x => x.Mblid == masterId && !listIdOfDimension.Contains(x.Id)).Select(s => s.Id);
                //Delete item of List Container MBL
                if (idContainersNeedRemove != null && idContainersNeedRemove.Count() > 0)
                {
                    var hsDelContHBL = DataContext.Delete(x => idContainersNeedRemove.Contains(x.Id));
                }

                foreach (var dimesion in dimensionDetails)
                {
                    //Insert & Update List Container MBL
                    if (dimesion.Id == Guid.Empty)
                    {
                        dimesion.Id = Guid.NewGuid();
                        dimesion.Mblid = masterId;
                        dimesion.UserModified = currentUser.UserID;
                        dimesion.DatetimeModified = DateTime.Now;
                        var hsAddDemension = Add(dimesion, false);
                    }
                    else
                    {
                        dimesion.Mblid = masterId;
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
