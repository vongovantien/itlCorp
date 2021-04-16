using AutoMapper;
using eFMS.API.Common.Globals;
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsMawbcontainerService : RepositoryBase<CsMawbcontainer, CsMawbcontainerModel>, ICsMawbcontainerService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<OpsTransaction> opsTransRepository;
        private readonly IContextBase<CsTransaction> csTransRepository;
        private readonly IContextBase<CsTransactionDetail> detailRepository;
        private readonly IContextBase<CatUnit> catUnitRepository;
        private readonly IContextBase<CatCommodity> catCommodityRepository;

        public CsMawbcontainerService(IContextBase<CsMawbcontainer> repository, 
            IContextBase<OpsTransaction> opsTransRepo,
            IContextBase<CsTransaction> csTransRepo,
            IMapper mapper, ICurrentUser user, 
            IStringLocalizer<LanguageSub> localize,
            IContextBase<CsTransactionDetail> detailRepo,
            IContextBase<CatUnit> unitRepo,
            IContextBase<CatCommodity> catCommodityRepo) : base(repository, mapper)
        {
            stringLocalizer = localize;
            currentUser = user;
            opsTransRepository = opsTransRepo;
            csTransRepository = csTransRepo;
            detailRepository = detailRepo;
            catUnitRepository = unitRepo;
            catCommodityRepository = catCommodityRepo;
        }

        public List<object> ListContOfHB(Guid JobId)
        {
            var houseBills = detailRepository.Get(x => x.JobId == JobId).ToList();
            List<object> returnList = new List<object>();
            foreach(var item in houseBills)
            {
                var conts = DataContext.Get(x => x.Hblid == item.Id).ToList();
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
            var containers = DataContext.Get(x => (x.Mblid == criteria.Mblid || criteria.Mblid == null)
                                               && (x.Hblid == criteria.Hblid || criteria.Hblid == null));
            var containerTypes = catUnitRepository.Get(x => x.UnitType == "Container");
            var unitOfMeasures = catUnitRepository.Get(x => x.UnitType == "WeightMeasurement");
            var packageTypes = catUnitRepository.Get(x => x.UnitType == "Package");
            var commodities = catCommodityRepository.Get();
            var results = (from container in containers
                           join unitOfMeasure in unitOfMeasures on container.UnitOfMeasureId equals unitOfMeasure.Id into grpUnitOfMeasures
                           from uOfMeasure in grpUnitOfMeasures.DefaultIfEmpty()
                           join containerType in containerTypes on container.ContainerTypeId equals containerType.Id into grpContainerTypes
                           from conType in grpContainerTypes.DefaultIfEmpty()
                           join packageType in packageTypes on container.PackageTypeId equals packageType.Id into grpPackageTypes
                           from packType in grpPackageTypes.DefaultIfEmpty()
                           join commodity in commodities on container.CommodityId equals commodity.Id into grpCommodities
                           from com in grpCommodities.DefaultIfEmpty()
                           select new CsMawbcontainerModel {
                               Id = container.Id,
                               Mblid = container.Mblid,
                               Hblid = container.Hblid,
                               ContainerTypeId = container.ContainerTypeId,
                               ContainerTypeName = conType.UnitNameEn,
                               Quantity = container.Quantity,
                               ContainerNo = container.ContainerNo,
                               SealNo = container.SealNo,
                               MarkNo = container.MarkNo,
                               UnitOfMeasureId = container.UnitOfMeasureId,
                               UnitOfMeasureName = uOfMeasure.UnitNameEn,
                               CommodityId = container.CommodityId,
                               CommodityName = com.CommodityNameEn,
                               PackageTypeId = container.PackageTypeId,
                               PackageTypeName = packType.UnitNameEn,
                               PackageQuantity = container.PackageQuantity,
                               Description = container.Description,
                               Gw = container.Gw,
                               Nw = container.Nw,
                               Cbm = container.Cbm,
                               ChargeAbleWeight = container.ChargeAbleWeight,
                               UserModified = container.UserModified,
                               DatetimeModified = container.DatetimeModified,
                               IsPartOfContainer = container.IsPartOfContainer
                           });
            if(results.Count() > 0)
            {
                results = results.OrderBy(x => x.ContainerNo);
            }
            return results;
        }

        public HandleState Update(List<CsMawbcontainerModel> list, Guid? masterId, Guid? housebillId)
        {
            try
            {
                if (masterId != null)
                {
                    List<CsMawbcontainer> oldList = null;
                    oldList = DataContext.Where(x => x.Mblid == masterId).ToList();
                    foreach (var item in oldList)
                    {
                        if (list.FirstOrDefault(x => x.Id == item.Id) == null)
                        {
                            DataContext.Delete(x => x.Id == item.Id, false);
                        }
                    }

                    if (housebillId != null)
                    {
                        List<CsMawbcontainer> oldHouseList = null;
                        oldHouseList = DataContext.Where(x => x.Hblid == housebillId && x.Mblid == masterId).ToList();
                        foreach (var item in oldHouseList)
                        {
                            if (list.FirstOrDefault(x => x.Id == item.Id) == null)
                            {
                                DataContext.Delete(x => x.Id == item.Id, false);
                            }
                        }
                    }
                }
                Hashtable ht = new Hashtable();
                int sumCont = 0;decimal sumGW = 0; decimal sumNW = 0; decimal sumCW = 0; decimal sumCBM = 0; int sumPackages = 0;
                foreach (var item in list)
                {
                    sumCont = sumCont + (int)item.Quantity;
                    sumGW = sumGW + (item.Gw != null?(long)item.Gw: 0);
                    sumNW = sumNW + (item.Nw != null?(long)item.Nw: 0);
                    sumCW = sumCW + (item.ChargeAbleWeight != null?(long)item.ChargeAbleWeight: 0);
                    sumCBM = sumCBM + (item.Cbm != null? (long)item.Cbm: 0);
                    sumPackages = sumPackages + (item.PackageQuantity != null? (int)item.PackageQuantity: 0);
                    if (ht.ContainsKey(item.ContainerTypeName))
                    {
                        var sumContDes = Convert.ToInt32(ht[item.ContainerTypeName]) + item.Quantity;
                        ht[item.ContainerTypeName] = sumContDes;
                    }
                    else
                    {
                        ht.Add(item.ContainerTypeName, item.Quantity);
                    }
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.UserModified = currentUser.UserID;
                        item.Mblid = (Guid)masterId;
                        item.DatetimeModified = DateTime.Now;
                        var hs = Add(item, false);
                    }
                    else
                    {
                        if (DataContext.Count(x => x.Id == item.Id) == 1)
                        {
                            item.UserModified = currentUser.UserID;
                            item.DatetimeModified = DateTime.Now;
                            var hs = Update(item, x => x.Id == item.Id, false);
                        }
                    }
                }
                var opstrans = opsTransRepository.First(x => x.Id == masterId);

                if (ht.Count > 0)
                {
                    var containerDes = string.Empty;
                    ICollection keys = ht.Keys;
                    foreach (var key in keys)
                    {
                        containerDes = containerDes + ht[key] + "x" + key + "; ";
                    }
                    containerDes = containerDes.Substring(0, containerDes.Length - 2);
                    opstrans.SumCbm = sumCBM != 0? (decimal?)sumCBM: null;
                    opstrans.SumChargeWeight = sumCW != 0 ? (decimal?)sumCW : null;
                    opstrans.SumGrossWeight = sumGW != 0 ? (decimal?)sumGW : null;
                    opstrans.SumNetWeight = sumNW != 0 ? (decimal?)sumNW : null;
                    opstrans.SumPackages = sumPackages != 0 ? (int?)sumPackages : null;
                    opstrans.SumContainers = sumCont != 0 ? (int?)sumCont : null ;
                    opstrans.ContainerDescription = containerDes;
                }
                else
                {
                    opstrans.SumCbm = opstrans.SumChargeWeight = opstrans.SumGrossWeight = opstrans.SumNetWeight = opstrans.SumPackages = opstrans.SumContainers = null;
                    opstrans.ContainerDescription = null;
                }
                opstrans.DatetimeModified = DateTime.Now;
                opstrans.UserModified = currentUser.UserID;
                opsTransRepository.Update(opstrans, x => x.Id == masterId, false);
                DataContext.SubmitChanges();
                opsTransRepository.SubmitChanges();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
            return new HandleState();
        }

        public HandleState UpdateMasterBill(List<CsMawbcontainerModel> containers, Guid masterId)
        {
            try
            {
                var listIdOfCont = containers.Where(x => x.Id != Guid.Empty).Select(s => s.Id);
                var idContainersNeedRemove = DataContext.Get(x => x.Mblid == masterId && !listIdOfCont.Contains(x.Id)).Select(s => s.Id);
                //Delete item of List Container MBL
                if (idContainersNeedRemove != null && idContainersNeedRemove.Count() > 0)
                {
                    var hsDelContHBL = DataContext.Delete(x => idContainersNeedRemove.Contains(x.Id));
                }

                foreach (var container in containers)
                {
                    //Insert & Update List Container MBL
                    if (container.Id == Guid.Empty)
                    {
                        container.Id = Guid.NewGuid();
                        container.Mblid = masterId;
                        container.UserModified = currentUser.UserID;
                        container.DatetimeModified = DateTime.Now;
                        var hsAddContMBL = Add(container);
                    }
                    else
                    {
                        container.Mblid = masterId;
                        container.UserModified = currentUser.UserID;
                        container.DatetimeModified = DateTime.Now;
                        var hsUpdateContMBL = Update(container, x => x.Id == container.Id);
                    }
                }
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
        
        public HandleState Importcontainer(List<CsMawbcontainerImportModel> data)
        {
            try
            {
                foreach (var item in data)
                {
                    var container = mapper.Map<CsMawbcontainer>(item);
                    container.DatetimeModified = DateTime.Now;
                    container.UserModified = currentUser.UserID;
                    container.Id = Guid.NewGuid();
                    DataContext.Add(container, false);
                }
                DataContext.SubmitChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
        public List<CsMawbcontainerImportModel> CheckValidContainerImport(List<CsMawbcontainerImportModel> list, Guid? mblid, Guid? hblid)
        {
            var units = catUnitRepository.Get().ToList();
            var commodities = catCommodityRepository.Get();
            var containers = units.Where(x => x.UnitType == "Container");
            var packages = units.Where(x => x.UnitType == "Package");
            var unitOfMeasures = units.Where(x => x.UnitType == "WeightMeasurement");
            var containerShipments = DataContext.Get().ToList();
            list.ForEach(item => {
                if (string.IsNullOrEmpty(item.ContainerTypeName))
                {
                    item.IsValid = false;
                    item.ContainerTypeNameError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_CONTAINERTYPE_EMPTY].Value;
                }
                else
                {
                    var container = containers.FirstOrDefault(x => x.UnitNameEn == item.ContainerTypeName);
                    if (container == null)
                    {
                        item.IsValid = false;
                        item.ContainerTypeNameError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_CONTAINERTYPE_NOT_FOUND, item.ContainerTypeName].Value;
                    }
                    else
                    {
                        item.ContainerTypeId = container.Id;
                    }
                }

                if (!string.IsNullOrEmpty(item.PackageTypeName))
                {
                    var packageType = packages.FirstOrDefault(x => x.UnitNameEn == item.PackageTypeName);
                    if (packageType == null)
                    {
                        item.IsValid = false;
                        item.PackageTypeNameError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_PACKAGE_TYPE_NOT_FOUND, item.PackageTypeName].Value;
                    }
                    else
                    {
                        item.PackageTypeId = packageType.Id;
                        item.PackageTypeNameError = null;
                    }
                }
                if (item.QuantityError == null)
                {
                    item.QuantityError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_QUANTITY_EMPTY].Value;
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
                            item.QuantityError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_QUANTITY_MUST_BE_1].Value;
                        }


                        if (!string.IsNullOrEmpty(item.ContainerNo) && item.PackageTypeId != null)
                        {
                            //var index = list.IndexOf(item);
                            var duplicateItems = list.Where(cont => cont.ContainerTypeId == item.ContainerTypeId && cont.Quantity == item.Quantity && cont.ContainerNo == item.ContainerNo && cont.PackageTypeId == item.PackageTypeId);
                            if (duplicateItems.Count() > 1)
                            {
                                list.Where(cont => cont.ContainerTypeId == item.ContainerTypeId && cont.Quantity == item.Quantity && cont.ContainerNo == item.ContainerNo && cont.PackageTypeId == item.PackageTypeId).ToList().ForEach(cont =>
                                {
                                    cont.IsValid = false;
                                    cont.DuplicateError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_DUPLICATE, item.ContainerTypeName, item.Quantity.ToString(), item.ContainerNo, item.PackageTypeName].Value;
                                    cont.ContainerNoError = item.ContainerNo;
                                    cont.QuantityError = cont.Quantity + string.Empty;
                                    cont.ContainerTypeNameError = cont.ContainerTypeName;
                                    cont.PackageTypeNameError = cont.PackageTypeName;
                                });
                            }
                            IEnumerable<CsMawbcontainer> existedItems = null;
                            if (mblid != null)
                            {
                                existedItems = containerShipments.Where(cont => cont.ContainerTypeId == item.ContainerTypeId && cont.Quantity == item.Quantity && cont.ContainerNo == item.ContainerNo && cont.PackageTypeId == item.PackageTypeId && cont.Mblid == mblid);
                            }
                            else
                            {
                                existedItems = containerShipments.Where(cont => cont.ContainerTypeId == item.ContainerTypeId && cont.Quantity == item.Quantity && cont.ContainerNo == item.ContainerNo && cont.PackageTypeId == item.PackageTypeId && cont.Mblid == hblid);
                            }
                            if (existedItems.Any())
                            {
                                item.IsValid = false;
                                item.ExistedError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_EXISTED, item.ContainerTypeName, item.Quantity.ToString(), item.ContainerNo, item.PackageTypeName].Value;
                                item.ContainerNoError = item.ContainerNo;
                                item.QuantityError = item.Quantity + string.Empty;
                                item.ContainerTypeNameError = item.ContainerTypeName;
                                item.PackageTypeNameError = item.PackageTypeName;
                            }
                        }
                    }
                    else
                    {
                        item.QuantityError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_QUANTITY_MUST_BE_NUMBER].Value;
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
                        item.PackageQuantityError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_PACKAGE_QUANTITY_MUST_BE_NUMBER].Value;
                        item.IsValid = false;
                    }
                }
                if(item.NwError != null)
                {
                    if (decimal.TryParse(item.NwError, out decimal x))
                    {
                        item.Nw = x;
                        item.NwError = null;
                    }
                    else
                    {
                        item.NwError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_NW_MUST_BE_NUMBER].Value;
                        item.IsValid = false;
                    }
                }
                if (item.GwError != null)
                {
                    if (decimal.TryParse(item.GwError, out decimal x))
                    {
                        item.Gw = x;
                        item.GwError = null;
                    }
                    else
                    {
                        item.GwError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_GW_MUST_BE_NUMBER].Value;
                        item.IsValid = false;
                    }
                }
                if (item.CbmError != null)
                {
                    if (decimal.TryParse(item.CbmError, out decimal x))
                    {
                        item.Cbm = x;
                        item.CbmError = null;
                    }
                    else
                    {
                        item.CbmError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_CBM_MUST_BE_NUMBER].Value;
                        item.IsValid = false;
                    }
                }
                if (!string.IsNullOrEmpty(item.CommodityName))
                {
                    var commodity = commodities.FirstOrDefault(x => x.CommodityNameEn == item.CommodityName);
                    if (commodity == null)
                    {
                        item.IsValid = false;
                        item.CommodityNameError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_COMMODITY_NAME_NOT_FOUND].Value;
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
                        item.UnitOfMeasureNameError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_UNIT_OF_MEASURE_NOT_FOUND].Value;
                    }
                    else
                    {
                        item.UnitOfMeasureId = unitOfMeasure.Id;
                    }
                }
            });
            return list;
        }
        public HandleState ValidateContainerList(List<CsMawbcontainerModel> csMawbcontainers, Guid? mblId, Guid? hblId)
        {
            var groups = csMawbcontainers.Where(x => !string.IsNullOrEmpty(x.ContainerNo) && x.PackageTypeId != null)
                .GroupBy(x => new { x.ContainerTypeId, x.Quantity, x.ContainerNo, x.PackageTypeId });
            if (groups.Any(x => x.Count() > 1))
            {
                return new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_DUPLICATE].Value);
            }
            if (hblId != null)
            {
                return ValidateContainerListInHousebill(csMawbcontainers, (Guid)hblId);
            }
            if (mblId != null)
            {
                return ValidateContainerListInMasterBill(csMawbcontainers, (Guid)mblId);
            }
            return new HandleState();
        }

        private HandleState ValidateContainerListInMasterBill(List<CsMawbcontainerModel> csMawbcontainers, Guid mblId)
        {
            var containerShipments = DataContext.Get(x => x.Mblid == mblId);

            foreach (var item in csMawbcontainers)
            {
                var existedItems = containerShipments.Count(cont => cont.ContainerTypeId == item.ContainerTypeId
                    && cont.Quantity == item.Quantity
                    && cont.ContainerNo == item.ContainerNo
                    && cont.PackageTypeId == item.PackageTypeId
                    && item.PackageTypeId != null
                    && !string.IsNullOrEmpty(item.ContainerNo)
                    );
                if (existedItems > 1)
                {
                    return new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_EXISTED].Value);
                }
            }
            return new HandleState();
        }

        private HandleState ValidateContainerListInHousebill(List<CsMawbcontainerModel> csMawbcontainers, Guid hblId)
        {
            var containerShipments = DataContext.Get(x => x.Hblid == hblId);

            foreach (var item in csMawbcontainers)
            {
                bool existedItems = containerShipments.Any(cont => cont.ContainerTypeId == item.ContainerTypeId
                && cont.Quantity == item.Quantity
                && cont.ContainerNo == item.ContainerNo
                && cont.PackageTypeId == item.PackageTypeId
                && item.PackageTypeId != null
                && !string.IsNullOrEmpty(item.ContainerNo));
                if (existedItems)
                {
                    return new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_EXISTED].Value);
                }
            }
            return new HandleState();
        }

        public HandleState UpdateHouseBill(List<CsMawbcontainerModel> containers, Guid housebillId)
        {
            try
            {
                var listIdOfCont = containers.Where(x => x.Id != Guid.Empty).Select(s => s.Id);
                var idContainersNeedRemove = DataContext.Get(x => x.Hblid == housebillId && !listIdOfCont.Contains(x.Id)).Select(s => s.Id);
                //Delete item of List Container MBL
                if (idContainersNeedRemove != null && idContainersNeedRemove.Count() > 0)
                {
                    var hsDelContHBL = DataContext.Delete(x => idContainersNeedRemove.Contains(x.Id));
                }

                foreach (var container in containers)
                {
                    //Insert & Update List Container MBL
                    if (container.Id == Guid.Empty)
                    {
                        container.Id = Guid.NewGuid();
                        container.Hblid = housebillId;
                        container.UserModified = currentUser.UserID;
                        container.DatetimeModified = DateTime.Now;
                        var hsAddContMBL = Add(container);
                    }
                    else
                    {
                        container.Hblid = housebillId;
                        container.UserModified = currentUser.UserID;
                        container.DatetimeModified = DateTime.Now;
                        var hsUpdateContMBL = Update(container, x => x.Id == container.Id);
                    }
                }
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public List<CsMawbcontainerImportModel> CheckValidGoodsImport(List<CsMawbcontainerImportModel> list, Guid? mblid, Guid? hblid)
        {
            var units = catUnitRepository.Get().ToList();
            var commodities = catCommodityRepository.Get();
            var containers = units.Where(x => x.UnitType == "Container");
            var packages = units.Where(x => x.UnitType == "Package");
            var unitOfMeasures = units.Where(x => x.UnitType == "WeightMeasurement");
            var containerShipments = DataContext.Get().ToList();
            list.ForEach(item => {
                var container = containers.FirstOrDefault(x => x.UnitNameEn == item.ContainerTypeName);
                if (container == null)
                {
                    item.IsValid = false;
                    item.ContainerTypeNameError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_CONTAINERTYPE_NOT_FOUND, item.ContainerTypeName].Value;
                }
                else
                {
                    item.ContainerTypeId = container.Id;
                }

                if (!string.IsNullOrEmpty(item.PackageTypeName))
                {
                    var packageType = packages.FirstOrDefault(x => x.UnitNameEn == item.PackageTypeName);
                    if (packageType == null)
                    {
                        item.IsValid = false;
                        item.PackageTypeNameError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_PACKAGE_TYPE_NOT_FOUND, item.PackageTypeName].Value;
                    }
                    else
                    {
                        item.PackageTypeId = packageType.Id;
                        item.PackageTypeNameError = null;
                    }
                }
                else {
                    item.IsValid = false;
                    item.PackageTypeNameError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_PACKAGE_TYPE_EMPTY].Value;
                }
                if (Int32.TryParse(item.QuantityError, out int quant))
                {
                    item.Quantity = quant;
                    item.QuantityError = null;

                    if ((!string.IsNullOrEmpty(item.ContainerNo) || !string.IsNullOrEmpty(item.MarkNo) || !string.IsNullOrEmpty(item.SealNo)) && item.Quantity > 1)
                    {
                        item.IsValid = false;
                        item.QuantityError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_QUANTITY_MUST_BE_1].Value;
                    }


                    if (!string.IsNullOrEmpty(item.ContainerNo) && item.PackageTypeId != null)
                    {
                        //var index = list.IndexOf(item);
                        var duplicateItems = list.Where(cont => cont.ContainerTypeId == item.ContainerTypeId && cont.Quantity == item.Quantity && cont.ContainerNo == item.ContainerNo && cont.PackageTypeId == item.PackageTypeId);
                        if (duplicateItems.Count() > 1)
                        {
                            list.Where(cont => cont.ContainerTypeId == item.ContainerTypeId && cont.Quantity == item.Quantity && cont.ContainerNo == item.ContainerNo && cont.PackageTypeId == item.PackageTypeId).ToList().ForEach(cont =>
                            {
                                cont.IsValid = false;
                                cont.DuplicateError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_DUPLICATE, item.ContainerTypeName, item.Quantity.ToString(), item.ContainerNo, item.PackageTypeName].Value;
                                cont.ContainerNoError = item.ContainerNo;
                                cont.QuantityError = cont.Quantity + string.Empty;
                                cont.ContainerTypeNameError = cont.ContainerTypeName;
                                cont.PackageTypeNameError = cont.PackageTypeName;
                            });
                        }
                        IEnumerable<CsMawbcontainer> existedItems = null;
                        if (mblid != null)
                        {
                            existedItems = containerShipments.Where(cont => cont.ContainerTypeId == item.ContainerTypeId && cont.Quantity == item.Quantity && cont.ContainerNo == item.ContainerNo && cont.PackageTypeId == item.PackageTypeId && cont.Mblid == mblid);
                        }
                        else
                        {
                            existedItems = containerShipments.Where(cont => cont.ContainerTypeId == item.ContainerTypeId && cont.Quantity == item.Quantity && cont.ContainerNo == item.ContainerNo && cont.PackageTypeId == item.PackageTypeId && cont.Mblid == hblid);
                        }
                        if (existedItems.Any())
                        {
                            item.IsValid = false;
                            item.ExistedError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_EXISTED, item.ContainerTypeName, item.Quantity.ToString(), item.ContainerNo, item.PackageTypeName].Value;
                            item.ContainerNoError = item.ContainerNo;
                            item.QuantityError = item.Quantity + string.Empty;
                            item.ContainerTypeNameError = item.ContainerTypeName;
                            item.PackageTypeNameError = item.PackageTypeName;
                        }
                    }
                }
                else
                {
                    item.QuantityError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_QUANTITY_MUST_BE_NUMBER].Value;
                    item.IsValid = false;
                }
                if (item.PackageQuantityError != null)
                {
                    if (Int64.TryParse(item.PackageQuantityError, out long x))
                    {
                        item.PackageQuantity = (short?)x;
                        item.PackageQuantityError = null;
                    }
                    else
                    {
                        item.PackageQuantityError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_PACKAGE_QUANTITY_MUST_BE_NUMBER].Value;
                        item.IsValid = false;
                    }
                }
                else
                {
                    item.PackageQuantityError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_PACKAGE_QUANTITY_EMPTY].Value;
                    item.IsValid = false;
                }
                if (item.NwError != null)
                {
                    if (decimal.TryParse(item.NwError, out decimal x))
                    {
                        item.Nw = x;
                        item.NwError = null;
                    }
                    else
                    {
                        item.NwError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_NW_MUST_BE_NUMBER].Value;
                        item.IsValid = false;
                    }
                }
                if (item.GwError != null)
                {
                    if (decimal.TryParse(item.GwError, out decimal x))
                    {
                        item.Gw = x;
                        item.GwError = null;
                    }
                    else
                    {
                        item.GwError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_GW_MUST_BE_NUMBER].Value;
                        item.IsValid = false;
                    }
                }
                if (item.CbmError != null)
                {
                    if (decimal.TryParse(item.CbmError, out decimal x))
                    {
                        item.Cbm = x;
                        item.CbmError = null;
                    }
                    else
                    {
                        item.CbmError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_CBM_MUST_BE_NUMBER].Value;
                        item.IsValid = false;
                    }
                }
                if (!string.IsNullOrEmpty(item.CommodityName))
                {
                    var commodity = commodities.FirstOrDefault(x => x.CommodityNameEn == item.CommodityName);
                    if (commodity == null)
                    {
                        item.IsValid = false;
                        item.CommodityNameError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_COMMODITY_NAME_NOT_FOUND].Value;
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
                        item.UnitOfMeasureNameError = stringLocalizer[DocumentationLanguageSub.MSG_MAWBCONTAINER_UNIT_OF_MEASURE_NOT_FOUND].Value;
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
