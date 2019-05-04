using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsMawbcontainerService : RepositoryBase<CsMawbcontainer, CsMawbcontainerModel>, ICsMawbcontainerService
    {
        public CsMawbcontainerService(IContextBase<CsMawbcontainer> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public List<object> ListContOfHB(Guid JobId)
        {
            var houseBills = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.JobId == JobId).ToList();
            List<object> returnList = new List<object>();
            foreach(var item in houseBills)
            {
                var conts = ((eFMSDataContext)DataContext.DC).CsMawbcontainer.Where(x => x.Hblid == item.Id).ToList();
                foreach(var c in conts)
                {
                    var obj = new { c.ContainerTypeId, c.Quantity,hblid=item.Id };
                    returnList.Add(obj);
                }
            }
            return returnList;
        }

        public IQueryable<CsMawbcontainerModel> Query(CsMawbcontainerCriteria criteria)
        {
            var data = GetView();
            var results = data.Where(x => (x.MBLID == criteria.Mblid || criteria.Mblid == null)
                                && (x.HBLID == criteria.Hblid || criteria.Hblid == null)
                                 ).Select(x => new CsMawbcontainerModel { Id = x.ID,
                                 Mblid = x.MBLID,
                                 Hblid = x.HBLID,
                                 ContainerTypeId = x.ContainerTypeID,
                                 ContainerTypeName = x.ContainerTypeName,
                                 Quantity = x.Quantity,
                                 ContainerNo = x.ContainerNo,
                                 SealNo = x.SealNo,
                                 MarkNo = x.MarkNo,
                                 UnitOfMeasureId = x.UnitOfMeasureID,
                                 UnitOfMeasureName = x.UnitOfMeasureName,
                                 CommodityId = x.CommodityId,
                                 CommodityName = x.CommodityName,
                                 PackageTypeId = x.PackageTypeId,
                                 PackageTypeName = x.PackageTypeName,
                                 PackageQuantity = x.PackageQuantity,
                                 Description = x.Description,
                                 Gw = x.GW,
                                 Nw = x.NW,
                                 Cbm = x.CBM,
                                 ChargeAbleWeight = x.ChargeAbleWeight,
                                 UserModified = x.UserModified,
                                 DatetimeModified = x.DatetimeModified
                                 }).AsQueryable();
            
            return results;
        }

        public HandleState Update(List<CsMawbcontainerModel> list, Guid? masterId, Guid? housebillId)
        {
            List<CsMawbcontainer> oldList = null;
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                if (masterId != null)
                {
                    oldList = ((eFMSDataContext)DataContext.DC).CsMawbcontainer.Where(x => x.Mblid == masterId).ToList();
                    foreach (var item in oldList)
                    {
                        if (list.FirstOrDefault(x => x.Id == item.Id) == null)
                        {
                            dc.CsMawbcontainer.Remove(item);
                        }
                    }
                    //dc.SaveChanges();
                }
                foreach (var item in list)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.UserModified = "01";
                        item.Mblid = (Guid)masterId;
                        item.DatetimeModified = DateTime.Now;
                        var hs = Add(item);
                    }
                    else
                    {
                        if (((eFMSDataContext)DataContext.DC).CsMawbcontainer.Count(x => x.Id == item.Id) == 1)
                        {
                            item.UserModified = "01";
                            item.DatetimeModified = DateTime.Now;
                            var hs = Update(item, x => x.Id == item.Id);
                        }
                    }
                }
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        private List<vw_csMAWBContainer> GetView(){
            
            List<vw_csMAWBContainer> results = ((eFMSDataContext)DataContext.DC).GetViewData<vw_csMAWBContainer>();
            return results;
        }
    }
}
