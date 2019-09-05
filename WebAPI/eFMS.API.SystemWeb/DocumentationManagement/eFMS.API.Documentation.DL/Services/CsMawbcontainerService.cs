using AutoMapper;
using eFMS.API.Documentation.DL.Common;
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
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsMawbcontainerService : RepositoryBase<CsMawbcontainer, CsMawbcontainerModel>, ICsMawbcontainerService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        public CsMawbcontainerService(IContextBase<CsMawbcontainer> repository, IMapper mapper, ICurrentUser user, IStringLocalizer<LanguageSub> localize) : base(repository, mapper)
        {
            stringLocalizer = localize;
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
                        item.UserModified = currentUser.UserID;
                        item.Mblid = (Guid)masterId;
                        item.DatetimeModified = DateTime.Now;
                        var hs = Add(item);
                    }
                    else
                    {
                        if (((eFMSDataContext)DataContext.DC).CsMawbcontainer.Count(x => x.Id == item.Id) == 1)
                        {
                            item.UserModified = currentUser.UserID;
                            item.DatetimeModified = DateTime.Now;
                            var hs = Update(item, x => x.Id == item.Id);
                        }
                    }
                }
                dc.SaveChanges();
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
                    container.Id = Guid.NewGuid();
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
            var containerShipments = ((eFMSDataContext)DataContext.DC).CsMawbcontainer.ToList();
            list.ForEach(item => {
                if (string.IsNullOrEmpty(item.ContainerTypeName))
                {
                    item.IsValid = false;
                    item.ContainerTypeNameError = stringLocalizer[LanguageSub.MSG_MAWBCONTAINER_CONTAINERTYPE_EMPTY].Value;
                }
                else
                {
                    var container = containers.FirstOrDefault(x => x.UnitNameEn == item.ContainerTypeName);
                    if (container == null)
                    {
                        item.IsValid = false;
                        item.ContainerTypeNameError = stringLocalizer[LanguageSub.MSG_MAWBCONTAINER_CONTAINERTYPE_NOT_FOUND, item.ContainerTypeName].Value;
                    }
                    else
                    {
                        item.ContainerTypeId = container.Id;
                    }
                }
                if (item.QuantityError == null)
                {
                    item.QuantityError = stringLocalizer[LanguageSub.MSG_MAWBCONTAINER_QUANTITY_EMPTY].Value;
                    item.IsValid = false;
                }
                else
                {
                    if (Int32.TryParse(item.QuantityError, out int x))
                    {
                        item.Quantity = x;
                        item.QuantityError = null;

                        if ((!string.IsNullOrEmpty(item.ContainerNo) || !string.IsNullOrEmpty(item.MarkNo) || !string.IsNullOrEmpty(item.SealNo)) && item.Quantity > 1)
                        {
                            item.IsValid = false;
                            item.QuantityError = stringLocalizer[LanguageSub.MSG_MAWBCONTAINER_QUANTITY_MUST_BE_1].Value;
                        }
                    }
                    else
                    {
                        item.QuantityError = stringLocalizer[LanguageSub.MSG_MAWBCONTAINER_QUANTITY_MUST_BE_NUMBER].Value;
                        item.IsValid = false;
                    }
                }
                if(item.PackageQuantityError != null)
                {
                    if (Int64.TryParse(item.PackageQuantityError, out long x))
                    {
                        item.PackageQuantity = (short?)x;
                        item.PackageQuantityError = null;
                    }
                    else
                    {
                        item.PackageQuantityError = stringLocalizer[LanguageSub.MSG_MAWBCONTAINER_PACKAGE_QUANTITY_MUST_BE_NUMBER].Value;
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
                        item.NwError = stringLocalizer[LanguageSub.MSG_MAWBCONTAINER_NW_MUST_BE_NUMBER].Value;
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
                        item.GwError = stringLocalizer[LanguageSub.MSG_MAWBCONTAINER_GW_MUST_BE_NUMBER].Value;
                        item.IsValid = false;
                    }
                }
                if (item.CbmError != null)
                {
                    if (Int64.TryParse(item.CbmError, out long x))
                    {
                        item.Cbm = x;
                        item.CbmError = null;
                    }
                    else
                    {
                        item.CbmError = stringLocalizer[LanguageSub.MSG_MAWBCONTAINER_CBM_MUST_BE_NUMBER].Value;
                        item.IsValid = false;
                    }
                }
                if (!string.IsNullOrEmpty(item.PackageTypeName))
                {
                    var packageType = packages.FirstOrDefault(x => x.UnitNameEn == item.PackageTypeName);
                    if (packageType == null)
                    {
                        item.IsValid = false;
                        item.PackageTypeNameError = stringLocalizer[LanguageSub.MSG_MAWBCONTAINER_PACKAGE_TYPE_NOT_FOUND, item.PackageTypeName].Value;
                    }
                    else
                    {
                        item.PackageTypeId = packageType.Id;
                        item.PackageQuantityError = null;
                    }
                }

                if (!string.IsNullOrEmpty(item.ContainerNo) && item.PackageTypeId != null)
                {
                    var duplicateItems = list.Where(x => x.ContainerTypeId == item.ContainerTypeId && x.Quantity == item.Quantity && x.ContainerNo == item.ContainerNo && x.PackageTypeId == item.PackageTypeId);
                    if (duplicateItems.Count() > 1)
                    {
                        list.Where(x => x.ContainerTypeId == item.ContainerTypeId && x.Quantity == item.Quantity && x.ContainerNo == item.ContainerNo && x.PackageTypeId == item.PackageTypeId).ToList().ForEach(x =>
                        {
                            x.IsValid = false;
                            x.ContainerTypeNameError = stringLocalizer[LanguageSub.MSG_MAWBCONTAINER_DUPLICATE, item.ContainerTypeName, item.Quantity.ToString(), item.ContainerNo, item.PackageTypeName].Value;
                        });
                    }
                    var existedItems = containerShipments.Where(x => x.ContainerTypeId == item.ContainerTypeId && x.Quantity == item.Quantity && x.ContainerNo == item.ContainerNo && x.PackageTypeId == item.PackageTypeId);
                    if(existedItems.Count() > 0)
                    {
                        item.IsValid = false;
                        item.ContainerTypeNameError = stringLocalizer[LanguageSub.MSG_MAWBCONTAINER_EXISTED, item.ContainerTypeName, item.Quantity.ToString(), item.ContainerNo, item.PackageTypeName].Value;
                    }
                }
                if (!string.IsNullOrEmpty(item.CommodityName))
                {
                    var commodity = commpdities.FirstOrDefault(x => x.CommodityNameEn == item.CommodityName);
                    if (commodity == null)
                    {
                        item.IsValid = false;
                        item.CommodityNameError = stringLocalizer[LanguageSub.MSG_MAWBCONTAINER_COMMODITY_NAME_NOT_FOUND].Value;
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
                        item.UnitOfMeasureNameError = stringLocalizer[LanguageSub.MSG_MAWBCONTAINER_UNIT_OF_MEASURE_NOT_FOUND].Value;
                    }
                    else
                    {
                        item.UnitOfMeasureId = unitOfMeasure.Id;
                    }
                }
            });
            return list;
        }
    }
}
