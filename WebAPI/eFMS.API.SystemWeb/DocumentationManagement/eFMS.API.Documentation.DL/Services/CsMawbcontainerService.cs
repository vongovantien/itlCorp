using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
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

        private List<vw_csMAWBContainer> GetView(){
            
            List<vw_csMAWBContainer> results = ((eFMSDataContext)DataContext.DC).GetViewData<vw_csMAWBContainer>();
            return results;
        }
    }
}
