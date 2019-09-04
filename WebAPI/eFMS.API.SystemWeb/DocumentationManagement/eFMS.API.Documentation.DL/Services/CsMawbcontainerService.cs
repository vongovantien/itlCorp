using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using eFMS.IdentityServer.DL.UserManager;
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
        private readonly ICurrentUser currentUser;
        public CsMawbcontainerService(IContextBase<CsMawbcontainer> repository, IMapper mapper, ICurrentUser user) : base(repository, mapper)
        {
            currentUser = user;
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
        public HandleState Importcontainer(List<CsMawbcontainerImportModel> data)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                foreach (var item in data)
                {
                    var container = mapper.Map<CsMawbcontainer>(item);
                    container.DatetimeModified = DateTime.Now;
                    container.UserModified = currentUser.UserID;
                    dc.CsMawbcontainer.Add(container);
                }
                dc.SaveChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
        public List<CsMawbcontainerImportModel> CheckValidContainerImport(List<CsMawbcontainerImportModel> list)
        {
            var units = ((eFMSDataContext)DataContext.DC).CatUnit.ToList();
            var commpdities = ((eFMSDataContext)DataContext.DC).CatCommodity.ToList();
            var containers = units.Where(x => x.UnitType == "Container");
            var packages = units.Where(x => x.UnitType == "Package");
            var unitOfMeasures = units.Where(x => x.UnitType == "Weight Measurement");
            list.ForEach(item => {
                if (string.IsNullOrEmpty(item.ContainerTypeName))
                {
                    item.IsValid = false;
                    item.ContainerTypeNameError = "Container name is empty";
                }
                else
                {
                    var container = containers.FirstOrDefault(x => x.UnitNameEn == item.ContainerTypeName);
                    if (container == null)
                    {
                        item.IsValid = false;
                        item.ContainerTypeNameError = "Container name not found";
                    }
                    else
                    {
                        item.ContainerTypeId = container.Id;
                    }
                }
                if (item.QuantityError == null)
                {
                    item.QuantityError = "Quantity is empty";
                    item.IsValid = false;
                }
                else
                {
                    if (Int32.TryParse(item.QuantityError, out int x))
                    {
                        item.Quantity = x;
                        item.QuantityError = null;
                    }
                    else
                    {
                        item.QuantityError = "Quantity must be a number";
                        item.IsValid = false;
                    }
                }
                if(item.PackageQuantityError == null)
                {
                    item.PackageQuantityError = "Package quantity is empty";
                }
                else
                {
                    if (Int64.TryParse(item.PackageQuantityError, out long x))
                    {
                        item.PackageQuantity = (short?)x;
                        item.PackageQuantityError = null;
                    }
                    else
                    {
                        item.PackageQuantityError = "Package quantity must be a number";
                        item.IsValid = false;
                    }
                }
                if(item.NwError != null)
                {
                    if (Int64.TryParse(item.NwError, out long x))
                    {
                        item.Nw = x;
                        item.NwError = null;
                    }
                    else
                    {
                        item.NwError = "Nw must be a number";
                        item.IsValid = false;
                    }
                }
                if (item.GwError != null)
                {
                    if (Int64.TryParse(item.GwError, out long x))
                    {
                        item.Gw = x;
                        item.GwError = null;
                    }
                    else
                    {
                        item.NwError = "Gw must be a number";
                        item.IsValid = false;
                    }
                }
                if (item.CbmError != null)
                {
                    if (Int64.TryParse(item.CbmError, out long x))
                    {
                        item.Cbm = x;
                    }
                    else
                    {
                        item.CbmError = "Cbm must be a number";
                        item.IsValid = false;
                    }
                }
                if (!string.IsNullOrEmpty(item.PackageTypeName))
                {
                    var packageType = packages.FirstOrDefault(x => x.UnitNameEn == item.PackageTypeName);
                    if (packageType == null)
                    {
                        item.IsValid = false;
                        item.PackageTypeNameError = "Package type name is not found";
                    }
                    else
                    {
                        item.PackageTypeId = packageType.Id;
                        if (!string.IsNullOrEmpty(item.ContainerNo))
                        {
                            var existedItems = list.Where(x => x.ContainerTypeId == item.ContainerTypeId && x.Quantity == item.Quantity && x.ContainerNo == item.ContainerNo && x.PackageTypeId == item.PackageTypeId);
                            if (existedItems.Count() > 1)
                            {
                                list.Where(x => x.ContainerTypeId == item.ContainerTypeId && x.Quantity == item.Quantity && x.ContainerNo == item.ContainerNo && x.PackageTypeId == item.PackageTypeId).ToList().ForEach(x =>
                                {
                                    x.IsValid = false;
                                    item.ContainerTypeNameError = "duplicate(Cont Type && Cont Q'ty && Container No && Package Type)";
                                });
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(item.CommodityName))
                {
                    var commodity = commpdities.FirstOrDefault(x => x.CommodityNameEn == item.CommodityName);
                    if (commodity == null)
                    {
                        item.IsValid = false;
                        item.CommodityNameError = "Commodity name is not found";
                    }
                    else
                    {
                        item.CommodityId = commodity.Id;
                    }
                }
                if (!string.IsNullOrEmpty(item.UnitOfMeasureName))
                {
                    var unitOfMeasure = unitOfMeasures.FirstOrDefault(x => x.UnitNameEn == item.UnitOfMeasureName);
                    if (unitOfMeasure == null)
                    {
                        item.IsValid = false;
                        item.UnitOfMeasureNameError = "Unit of measure name is not found";
                    }
                    else
                    {
                        item.UnitOfMeasureId = unitOfMeasure.Id;
                    }
                }
                if (!string.IsNullOrEmpty(item.ContainerNo) || !string.IsNullOrEmpty(item.MarkNo) || !string.IsNullOrEmpty(item.SealNo))
                {
                    item.IsValid = false;
                    item.QuantityError = "Quantity must be 1";
                }
            });
            return list;
        }
    }
}
