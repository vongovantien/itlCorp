﻿using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Models;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Models.Exports;
using eFMS.API.Documentation.DL.Models.ReportResults;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsTransactionDetailService : RepositoryBase<CsTransactionDetail, CsTransactionDetailModel>, ICsTransactionDetailService
    {
        readonly IContextBase<CsTransaction> csTransactionRepo;
        readonly IContextBase<CsMawbcontainer> csMawbcontainerRepo;
        readonly IContextBase<CatPartner> catPartnerRepo;
        readonly IContextBase<CatPlace> catPlaceRepo;
        readonly IContextBase<SysUser> sysUserRepo;
        readonly IContextBase<CatUnit> catUnitRepo;
        readonly IContextBase<CatCommodity> catCommodityRepo;
        readonly IContextBase<CatSaleman> catSalemanRepo;
        readonly ICsMawbcontainerService containerService;
        readonly IContextBase<CsTransactionDetail> csTransactionDetailRepo;
        readonly IContextBase<CsShipmentSurcharge> surchareRepository;
        readonly IContextBase<CatCountry> countryRepository;
        readonly ICsDimensionDetailService dimensionDetailService;
        readonly ICsShipmentOtherChargeService shipmentOtherChargeService;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysAuthorization> authorizationRepository;
        readonly IUserPermissionService permissionService;
        readonly IContextBase<SysOffice> sysOfficeRepo;


        public CsTransactionDetailService(IContextBase<CsTransactionDetail> repository,
            IMapper mapper,
            IContextBase<CsTransaction> csTransaction,
            IContextBase<CsMawbcontainer> csMawbcontainer,
            IContextBase<CatPartner> catPartner,
            IContextBase<CatPlace> catPlace,
            IContextBase<SysUser> sysUser,
            IContextBase<CatUnit> catUnit,
            IContextBase<CatCommodity> catCommodity,
            IContextBase<CatSaleman> catSaleman,
            IContextBase<CsShipmentSurcharge> surchareRepo,
            IContextBase<CatCountry> countryRepo,
            IContextBase<CsTransactionDetail> csTransactiondetail,
            ICsMawbcontainerService contService, 
            ICurrentUser user,
            ICsDimensionDetailService dimensionService,
            IContextBase<SysAuthorization> authorizationRepo,
            ICsShipmentOtherChargeService oChargeService,
            IUserPermissionService perService,
            IContextBase<SysOffice> sysOffice) : base(repository, mapper)
        {
            csTransactionRepo = csTransaction;
            csMawbcontainerRepo = csMawbcontainer;
            catPartnerRepo = catPartner;
            catPlaceRepo = catPlace;
            sysUserRepo = sysUser;
            catUnitRepo = catUnit;
            catCommodityRepo = catCommodity;
            surchareRepository = surchareRepo;
            catSalemanRepo = catSaleman;
            containerService = contService;
            shipmentOtherChargeService = oChargeService;
            csTransactionDetailRepo = csTransactiondetail;
            currentUser = user;
            countryRepository = countryRepo;
            dimensionDetailService = dimensionService;
            authorizationRepository = authorizationRepo;
            permissionService = perService;
            sysOfficeRepo = sysOffice;
        }

        #region -- INSERT & UPDATE HOUSEBILLS --
        public HandleState AddTransactionDetail(CsTransactionDetailModel model)
        {
            var job = csTransactionRepo.Get(x => x.Id == model.JobId).FirstOrDefault();
            ICurrentUser _currentUser = PermissionEx.GetUserMenuPermissionTransaction(job.TransactionType, currentUser);
            var permissionRangeWrite = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Write);
            if (permissionRangeWrite == PermissionRange.None) return new HandleState(403,"");

            if (model.CsMawbcontainers?.Count > 0)
            {
                var checkDuplicateCont = containerService.ValidateContainerList(model.CsMawbcontainers, null, model.Id);
                if (checkDuplicateCont.Success == false)
                {
                    return checkDuplicateCont;
                }
            }
            model.UserCreated = currentUser.UserID;
            model.DatetimeCreated = DateTime.Now;
            model.Id = Guid.NewGuid();
            model.UserModified = model.UserCreated = currentUser.UserID;
            model.DatetimeModified = model.DatetimeCreated = DateTime.Now;
            model.Active = true;
            model.GroupId = _currentUser.GroupId;
            model.DepartmentId = _currentUser.DepartmentId;
            model.OfficeId = _currentUser.OfficeID;
            model.CompanyId = _currentUser.CompanyID;

            string contSealNo = string.Empty;

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = Add(model);
                    if (hs.Success)
                    {
                        if (model.CsMawbcontainers != null)
                        {
                            model.CsMawbcontainers.ForEach(x =>
                            {
                                if (!string.IsNullOrEmpty(x.ContainerNo))
                                {
                                    contSealNo = contSealNo + x.ContainerNo;
                                }
                                if (!string.IsNullOrEmpty(x.SealNo))
                                {
                                    contSealNo = contSealNo + "/ " + x.SealNo + "; ";
                                }
                                x.Id = Guid.NewGuid();
                                x.Hblid = model.Id;
                                x.UserModified = model.UserModified;
                                x.DatetimeModified = DateTime.Now;
                            });

                            var t = containerService.Add(model.CsMawbcontainers);
                            if (t.Success)
                            {
                                model.ContSealNo = contSealNo;
                                var updateDetail = Update(model, x => x.Id == model.Id);
                            }
                        }
                        if (model.DimensionDetails != null)
                        {
                            model.DimensionDetails.ForEach(x =>
                            {
                                x.Id = Guid.NewGuid();
                                x.Hblid = model.Id;
                            });
                            var d = dimensionDetailService.Add(model.DimensionDetails);
                        }
                        if (model.OtherCharges != null)
                        {
                            model.OtherCharges.ForEach(x =>
                            {
                                x.Id = Guid.NewGuid();
                                x.Hblid = model.Id;
                                dimensionDetailService.Add(model.DimensionDetails);
                            });
                            shipmentOtherChargeService.Add(model.OtherCharges);
                        }

                    }
                    DataContext.SubmitChanges();
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

        public HandleState UpdateTransactionDetail(CsTransactionDetailModel model)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hb = DataContext.Where(x => x.Id == model.Id).FirstOrDefault();
                    if (hb == null)
                    {
                        return new HandleState("Housebill not found !");
                    }
                    ICurrentUser _currentUser = PermissionEx.GetUserMenuPermissionTransaction(model.TransactionType, currentUser);
                    var permissionRange = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Write);
                    int code = GetPermissionToUpdate(new ModelUpdate { SaleManId = model.SaleManId, UserCreated = model.UserCreated, CompanyId = model.CompanyId, OfficeId = model.OfficeId, DepartmentId = model.DepartmentId, GroupId = model.GroupId }, permissionRange, model.TransactionType);
                    if (code == 403) return new HandleState(403,"");
                    
                    // TODO validate follow service
                    //if (model.CsMawbcontainers?.Count > 0)
                    //{
                    //    var checkDuplicateCont = containerService.ValidateContainerList(model.CsMawbcontainers, null, model.Id);
                    //    if (checkDuplicateCont.Success == false)
                    //    {
                    //        return checkDuplicateCont;
                    //    }
                    //}

                    model.UserModified = currentUser.UserID;
                    model.DatetimeModified = DateTime.Now;

                    model.UserCreated = hb.UserCreated;
                    model.DatetimeCreated = hb.DatetimeCreated;
                    model.GroupId = hb.GroupId;
                    model.DepartmentId = hb.DepartmentId;
                    model.OfficeId = hb.OfficeId;
                    model.CompanyId = hb.CompanyId;

                    var isUpdateDone = Update(model, x => x.Id == hb.Id);
                    if (isUpdateDone.Success)
                    {
                        if (model.CsMawbcontainers != null)
                        {
                            var hscontainers = containerService.UpdateHouseBill(model.CsMawbcontainers, model.Id);
                        }
                        else
                        {
                            var hsContainerDetele = csMawbcontainerRepo.Delete(x => x.Hblid == hb.Id);
                        }
                        if (model.DimensionDetails != null)
                        {
                            var hsDimension = dimensionDetailService.UpdateHouseBill(model.DimensionDetails, model.Id);
                        }
                        else
                        {
                            var hsDimensionDelete = dimensionDetailService.Delete(x => x.Hblid == model.Id);
                        }
                        if(model.OtherCharges != null)
                        {
                            var otherCharges = shipmentOtherChargeService.UpdateOtherChargeHouseBill(model.OtherCharges,model.Id);
                        }
                    }
                    trans.Commit();
                    return isUpdateDone;
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

        public string GenerateHBLNo(TransactionTypeEnum transactionTypeEnum)
        {
            string hblNo = string.Empty;
            var transactionType = DataTypeEx.GetType(transactionTypeEnum);
            if (transactionType == TermData.AirImport || transactionType == TermData.AirExport)
            {
                var hblNos = Get(x => x.Hwbno.Contains(DocumentConstants.CODE_ITL)).ToArray()
                    .OrderByDescending(o => o.DatetimeCreated)
                    .ThenByDescending(o => o.Hwbno)
                    .Select(s => s.Hwbno);
                int count = 0;
                if (hblNos != null && hblNos.Count() > 0)
                {
                    foreach (var hbl in hblNos)
                    {
                        string _hbl = hbl;
                        _hbl = _hbl.Substring(DocumentConstants.CODE_ITL.Length, _hbl.Length - DocumentConstants.CODE_ITL.Length);
                        Int32.TryParse(_hbl, out count);
                        if (count > 0)
                        {
                            break;
                        }
                    }
                }
                //Reset về 0
                if (count == 9999)
                {
                    count = 0;
                }
                hblNo = GenerateID.GenerateHBLNo(count);
            }
            return hblNo;
        }

        #endregion -- INSERT & UPDATE HOUSEBILLS --

        public List<CsTransactionDetailModel> GetByJob(CsTransactionDetailCriteria criteria)
        {
            var results = QueryDetail(criteria).ToList();
            var containers = (from container in ((eFMSDataContext)DataContext.DC).CsMawbcontainer
                              join unit in ((eFMSDataContext)DataContext.DC).CatUnit on container.ContainerTypeId equals unit.Id
                              join unitMeasure in ((eFMSDataContext)DataContext.DC).CatUnit on container.UnitOfMeasureId equals unitMeasure.Id into grMeasure
                              from measure in grMeasure.DefaultIfEmpty()
                              join packageType in ((eFMSDataContext)DataContext.DC).CatUnit on container.PackageTypeId equals packageType.Id into grPackage
                              from packType in grPackage.DefaultIfEmpty()
                              join commodity in ((eFMSDataContext)DataContext.DC).CatCommodity on container.CommodityId equals commodity.Id into grCommodity
                              from commo in grCommodity.DefaultIfEmpty()
                              select new
                              {
                                  container,
                                  unit,
                                  UnitOfMeasureName = measure.UnitNameEn,
                                  PackageTypeName = packType.UnitNameEn,
                                  CommodityName = commo.CommodityNameEn
                              });

            if (containers.Count() == 0) return results;
            results.ForEach(detail =>
            {
                detail.PackageTypes = string.Empty;
                detail.CBM = 0;
                var containerHouses = containers.Where(x => x.container.Hblid == detail.Id);
                if (containerHouses != null)
                {
                    detail.CsMawbcontainers = new List<CsMawbcontainerModel>();
                    foreach (var item in containerHouses)
                    {
                        if (item.container.PackageQuantity != null && item.container.PackageTypeId != null)
                        {
                            detail.PackageTypes += item.container.PackageQuantity + "x" + item.PackageTypeName + ", ";

                        }
                        detail.GW = detail.GW + item.container.Gw != null ? item.container.Gw : 0;
                        detail.CBM = detail.CBM + item.container.Cbm != null ? item.container.Cbm : 0;
                        detail.CW = detail.CW + item.container.ChargeAbleWeight != null ? item.container.ChargeAbleWeight : 0;

                        var container = mapper.Map<CsMawbcontainerModel>(item.container);
                        container.CommodityName = item.CommodityName;
                        container.PackageTypeName = item.PackageTypeName;
                        container.ContainerTypeName = item.unit.UnitNameEn;
                        container.UnitOfMeasureName = item.UnitOfMeasureName;
                        detail.CsMawbcontainers.Add(container);
                    }
                }
                if (detail.PackageTypes.Length > 0 && detail.PackageTypes.ElementAt(detail.PackageTypes.Length - 1) == ';')
                {
                    detail.PackageTypes = detail.PackageTypes.Substring(0, detail.PackageTypes.Length - 1);
                }
            });
            return results;
        }

        public IQueryable<CsTransactionDetailModel> QueryDetail(CsTransactionDetailCriteria criteria)
        {
            List<CsTransactionDetailModel> results = new List<CsTransactionDetailModel>();
            var details = DataContext.Get(x => x.JobId == criteria.JobId);
            var partners = catPartnerRepo.Get();
            var places = catPlaceRepo.Get();
            var salemans = catSalemanRepo.Get();
            var query = (from detail in details
                         join customer in partners on detail.CustomerId equals customer.Id into detailCustomers
                         from y in detailCustomers.DefaultIfEmpty()
                         join noti in partners on detail.NotifyPartyId equals noti.Id into detailNotis
                         from noti in detailNotis.DefaultIfEmpty()
                         join port in places on detail.Pod equals port.Id into portDetail
                         from pod in portDetail.DefaultIfEmpty()
                         join fwd in partners on detail.ForwardingAgentId equals fwd.Id into forwarding
                         from f in forwarding.DefaultIfEmpty()
                         join saleman in salemans on detail.SaleManId equals saleman.Id.ToString() into prods
                         from x in prods.DefaultIfEmpty()
                         select new { detail, customer = y, notiParty = noti, saleman = x, agent = f, pod });
            if (query == null) return null;
            foreach (var item in query)
            {
                var detail = mapper.Map<CsTransactionDetailModel>(item.detail);
                detail.CustomerName = item.customer?.PartnerNameEn;
                detail.CustomerNameVn = item.customer?.PartnerNameVn;
                detail.SaleManName = item.saleman?.SaleManId;
                detail.NotifyParty = item.notiParty?.PartnerNameEn;
                detail.ForwardingAgentName = item.agent?.PartnerNameEn;
                detail.PODName = item.pod?.NameEn;
                results.Add(detail);
            }
            return results.AsQueryable();
        }

        public CsTransactionDetailModel GetById(Guid Id)
        {
            try
            {
                var queryDetail = csTransactionDetailRepo.Get(x => x.Id == Id).FirstOrDefault();
                
                var detail = mapper.Map<CsTransactionDetailModel>(queryDetail);
                if (detail != null)
                {
                    var resultPartner = catPartnerRepo.Get(x => x.Id == detail.CustomerId).FirstOrDefault();
                    var resultNoti = catPartnerRepo.Get(x => x.Id == detail.NotifyPartyId).FirstOrDefault();
                    var resultSaleman = sysUserRepo.Get(x => x.Id == detail.SaleManId).FirstOrDefault();
                    var pol = catPlaceRepo.Get(x => x.Id == detail.Pol).FirstOrDefault();
                    var pod = catPlaceRepo.Get(x => x.Id == detail.Pod).FirstOrDefault();
                    var shipment = csTransactionRepo.Get(x => x.Id == queryDetail.JobId).First();
                    detail.CustomerName = resultPartner?.PartnerNameEn;
                    detail.CustomerNameVn = resultPartner?.PartnerNameVn;
                    detail.SaleManId = resultSaleman?.Id;
                    detail.NotifyParty = resultNoti?.PartnerNameEn;
                    detail.POLName = pol?.NameEn;
                    detail.PODName = pod?.NameEn;
                    detail.ShipmentEta = shipment.Eta;
                    detail.TransactionType = shipment.TransactionType;
                    return detail;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public int CheckDetailPermission(Guid id)
        {
            var detail = GetById(id);
            ICurrentUser _currentUser = PermissionEx.GetUserMenuPermissionTransaction(detail.TransactionType, currentUser);
            var permissionRange = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Detail);
            int code = GetPermissionToUpdate(new ModelUpdate { SaleManId = detail.SaleManId, UserCreated = detail.UserCreated, CompanyId = detail.CompanyId, OfficeId = detail.OfficeId, DepartmentId = detail.DepartmentId, GroupId = detail.GroupId }, permissionRange, detail.TransactionType);
            return code;
        }

        public CsTransactionDetailModel GetDetails(Guid id)
        {
            var detail = GetById(id);
            if (detail == null) return null;
            List<string> authorizeUserIds = permissionService.GetAuthorizedIds(detail.TransactionType, currentUser);


            ICurrentUser _currentUser = PermissionEx.GetUserMenuPermissionTransaction(detail.TransactionType, currentUser);

            var permissionRangeWrite = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Write);
            detail.Permission = new PermissionAllowBase
            {
                AllowUpdate = GetPermissionDetail(permissionRangeWrite, authorizeUserIds, detail)
            };
            var specialActions = _currentUser.UserMenuPermission.SpecialActions;
            if (specialActions.Count > 0)
            {
                if (specialActions.Any(x => x.Action.Contains("Lock")))
                {
                    detail.Permission.AllowLock = true;
                }
                if (specialActions.Any(x => x.Action.Contains("Update Charge")))
                {
                    detail.Permission.AllowUpdateCharge = true;
                }
            }
            return detail;
        }



        private int GetPermissionToUpdate(ModelUpdate model, PermissionRange permissionRange, string transactionType)
        {
            List<string> authorizeUserIds = authorizationRepository.Get(x => x.Active == true
                                                                 && x.AssignTo == currentUser.UserID
                                                                 && (x.EndDate.HasValue ? x.EndDate.Value : DateTime.Now.Date) >= DateTime.Now.Date
                                                                 && x.Services.Contains(transactionType)
                                                                 )?.Select(x => x.UserId).ToList();
            int code = PermissionEx.GetPermissionToUpdateHbl(model, permissionRange, currentUser, authorizeUserIds);
            return code;
        }



        private bool GetPermissionDetail(PermissionRange permissionRangeWrite, List<string> authorizeUserIds, CsTransactionDetailModel detail)
        {
            bool result = false;
            switch (permissionRangeWrite)
            {
                case PermissionRange.None:
                    result = false;
                    break;
                case PermissionRange.All:
                    result = true;
                    break;
                case PermissionRange.Owner:
                    if (detail.SaleManId == currentUser.UserID || authorizeUserIds.Contains(detail.SaleManId) || detail.UserCreated == currentUser.UserID)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                case PermissionRange.Group:
                    if ((detail.GroupId == currentUser.GroupId && detail.DepartmentId == currentUser.DepartmentId && detail.OfficeId == currentUser.OfficeID && detail.CompanyId == currentUser.CompanyID || detail.UserCreated == currentUser.UserID)
                        || authorizeUserIds.Contains(detail.SaleManId))
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                case PermissionRange.Department:
                    if ((detail.DepartmentId == currentUser.DepartmentId && detail.OfficeId == currentUser.OfficeID && detail.CompanyId == currentUser.CompanyID) || authorizeUserIds.Contains(detail.SaleManId) || detail.UserCreated == currentUser.UserID)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                case PermissionRange.Office:
                    if ((detail.OfficeId == currentUser.OfficeID && detail.CompanyId == currentUser.CompanyID) || authorizeUserIds.Contains(detail.SaleManId) || detail.UserCreated == currentUser.UserID)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                case PermissionRange.Company:
                    if (detail.CompanyId == currentUser.CompanyID || authorizeUserIds.Contains(detail.SaleManId) || detail.UserCreated == currentUser.UserID)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
            }
            return result;
        }

        private int GetPermissionToDelete(ModelUpdate model, PermissionRange permissionRange)
        {
            int code = PermissionEx.GetPermissionToDeleteHbl(model, permissionRange, currentUser);
            return code;
        }


        public CsTransactionDetailModel GetSeparateByHblid(Guid hbId)
        {
            try
            {
                var queryDetail = csTransactionDetailRepo.Get(x => x.ParentId == hbId).FirstOrDefault();
                var detail = mapper.Map<CsTransactionDetailModel>(queryDetail);
                if (detail != null)
                {
                    var resultPartner = catPartnerRepo.Get(x => x.Id == detail.CustomerId).FirstOrDefault();
                    var resultNoti = catPartnerRepo.Get(x => x.Id == detail.NotifyPartyId).FirstOrDefault();
                    var resultSaleman = sysUserRepo.Get(x => x.Id == detail.SaleManId).FirstOrDefault();
                    var pol = catPlaceRepo.Get(x => x.Id == detail.Pol).FirstOrDefault();
                    var pod = catPlaceRepo.Get(x => x.Id == detail.Pod).FirstOrDefault();
                    var shipment = csTransactionRepo.Get(x => x.Id == queryDetail.JobId).First();
                    detail.CustomerName = resultPartner?.PartnerNameEn;
                    detail.CustomerNameVn = resultPartner?.PartnerNameVn;
                    detail.SaleManId = resultSaleman?.Id;
                    detail.NotifyParty = resultNoti?.PartnerNameEn;
                    detail.POLName = pol?.NameEn;
                    detail.PODName = pod?.NameEn;
                    detail.ShipmentEta = shipment.Eta;
                    return detail;
                }
            }
            catch (Exception ex)
            {

            }
            return null;

        }

        #region -- LIST & PAGING HOUSEBILLS --
        public List<CsTransactionDetailModel> Query(CsTransactionDetailCriteria criteria)
        {
            var shipment = csTransactionRepo.Get(x => x.Id == criteria.JobId).FirstOrDefault();
            var transactionType = DataTypeEx.GetType(criteria.TransactionType);
            if (shipment == null) return null;
            var houseBills = GetHouseBill(shipment.TransactionType);

            var query = (from detail in houseBills//DataContext.Get()
                         join tran in csTransactionRepo.Get() on detail.JobId equals tran.Id
                         join customer in catPartnerRepo.Get() on detail.CustomerId equals customer.Id into customers
                         from cus in customers.DefaultIfEmpty()
                         join saleman in sysUserRepo.Get() on detail.SaleManId equals saleman.Id.ToString() into salemans
                         from sale in salemans.DefaultIfEmpty()
                         select new { detail, tran, cus, sale });
            //var transactionType = DataTypeEx.GetType(criteria.TransactionType);
            if (criteria.All == null)
            {
                if (criteria.TypeFCL == "Export")
                {
                    query = query.Where(x => (x.tran.Mawb ?? "").IndexOf(criteria.Mawb ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                 && (x.detail.Hwbno.IndexOf(criteria.Hwbno ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                 && (x.cus.ShortName.IndexOf(criteria.CustomerName ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                 && (x.tran.Etd >= criteria.FromDate || criteria.FromDate == null)
                 && (x.tran.Etd <= criteria.ToDate || criteria.ToDate == null)
                 && (x.sale.Username.IndexOf(criteria.SaleManName ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                 && (x.tran.TransactionType == transactionType || string.IsNullOrEmpty(transactionType))
                 );
                }
                else if(criteria.TypeFCL == "Import")
                {
                        query = query.Where(x => (x.tran.Mawb ?? "").IndexOf(criteria.Mawb ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                  && (x.detail.Hwbno.IndexOf(criteria.Hwbno ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                  && (x.cus.ShortName.IndexOf(criteria.CustomerName ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                  && (x.tran.Eta >= criteria.FromDate || criteria.FromDate == null)
                  && (x.tran.Eta <= criteria.ToDate || criteria.ToDate == null)
                  && (x.sale.Username.IndexOf(criteria.SaleManName ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                  && (x.tran.TransactionType == transactionType || string.IsNullOrEmpty(transactionType))
                  );
                }
                else
                {
                    query = query.Where(x => criteria.JobId != Guid.Empty && criteria.JobId != null ? x.detail.JobId == criteria.JobId : true
                                         && ((x.detail.Mawb ?? "").IndexOf(criteria.Mawb ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                         && (x.detail.Hwbno.IndexOf(criteria.Hwbno ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                         && (x.cus.ShortName.IndexOf(criteria.CustomerName ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                         && (x.tran.Eta >= criteria.FromDate || criteria.FromDate == null)
                                         && (x.tran.Eta <= criteria.ToDate || criteria.ToDate == null)
                                         && (x.sale.Id.IndexOf(criteria.SaleManName ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                         && (x.tran.TransactionType == transactionType || string.IsNullOrEmpty(transactionType)
                                         ));
                }
            }
            else
            {
                query = query.Where(x => criteria.JobId != Guid.Empty && criteria.JobId != null ? x.detail.JobId == criteria.JobId : true
                                      || (x.tran.Mawb.IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                      || (x.detail.Hwbno.IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                      || (x.cus.ShortName.IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                      || (x.sale.Id.IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0))
                                      && ((x.tran.Etd ?? null) >= (criteria.FromDate ?? null) && (x.tran.Etd ?? null) <= (criteria.ToDate ?? null))
                                      && (x.tran.TransactionType == transactionType || string.IsNullOrEmpty(transactionType))
                                      );
            }
            var res = from detail in query.Select(s => s.detail)
                      join tran in csTransactionRepo.Get() on detail.JobId equals tran.Id
                      join customer in catPartnerRepo.Get() on detail.CustomerId equals customer.Id into customers
                      from cus in customers.DefaultIfEmpty()
                      join shipper in catPartnerRepo.Get() on detail.ShipperId equals shipper.Id into shippers
                      from shipper in shippers.DefaultIfEmpty()
                      join consignee in catPartnerRepo.Get() on detail.ConsigneeId equals consignee.Id into consignees
                      from consignee in consignees.DefaultIfEmpty()
                      join saleman in sysUserRepo.Get() on detail.SaleManId equals saleman.Id into salemans
                      from sale in salemans.DefaultIfEmpty()
                      join notify in catPartnerRepo.Get() on detail.NotifyPartyId equals notify.Id into notifys
                      from notify in notifys.DefaultIfEmpty()
                      join port in catPlaceRepo.Get() on detail.Pod equals port.Id into portDetail
                      from pod in portDetail.DefaultIfEmpty()
                      select new CsTransactionDetailModel
                      {
                          Id = detail.Id,
                          JobId = detail.JobId,
                          Hwbno = detail.Hwbno,
                          Mawb = detail.Mawb,
                          SaleManId = detail.SaleManId,
                          SaleManName = sale.Username,
                          CustomerId = detail.CustomerId,
                          CustomerName = cus.ShortName,
                          NotifyPartyId = detail.NotifyPartyId,
                          NotifyParty = notify.ShortName,
                          FinalDestinationPlace = detail.FinalDestinationPlace,
                          Eta = detail.Eta,
                          Etd = detail.Etd,
                          ConsigneeId = detail.ConsigneeId,
                          ConsigneeDescription = detail.ConsigneeDescription,
                          ShipperDescription = detail.ShipperDescription,
                          ShipperId = detail.ShipperId,
                          NotifyPartyDescription = detail.NotifyPartyDescription,
                          Pod = detail.Pod,
                          Pol = detail.Pol,
                          AlsoNotifyPartyId = detail.AlsoNotifyPartyId,
                          AlsoNotifyPartyDescription = detail.AlsoNotifyPartyDescription,
                          Hbltype = detail.Hbltype,
                          ReferenceNo = detail.ReferenceNo,
                          ColoaderId = detail.ColoaderId,
                          LocalVoyNo = detail.LocalVoyNo,
                          LocalVessel = detail.LocalVessel,
                          OceanVessel = detail.OceanVessel,
                          OceanVoyNo = detail.OceanVoyNo,
                          OriginBlnumber = detail.OriginBlnumber,
                          ShipperName = shipper.ShortName,
                          ConsigneeName = consignee.ShortName,
                          DesOfGoods = detail.DesOfGoods,
                          PODName = pod.NameEn,
                          ManifestRefNo = detail.ManifestRefNo,
                          ServiceType = detail.ServiceType,
                          ContSealNo = detail.ContSealNo,
                          SailingDate = detail.SailingDate,
                          FreightPayment = detail.FreightPayment,
                          PickupPlace = detail.PickupPlace,
                          DeliveryPlace = detail.DeliveryPlace,
                          GoodsDeliveryDescription = detail.GoodsDeliveryDescription,
                          GoodsDeliveryId = detail.GoodsDeliveryId,
                          ForwardingAgentDescription = detail.ForwardingAgentDescription,
                          ForwardingAgentId = detail.ForwardingAgentId,
                          MoveType = detail.MoveType,
                          ShippingMark = detail.ShippingMark,
                          InWord = detail.InWord,
                          OnBoardStatus = detail.OnBoardStatus,
                          Remark = detail.Remark,
                          PurchaseOrderNo = detail.PurchaseOrderNo,
                          OriginCountryId = detail.OriginCountryId,
                          CBM = detail.Cbm,
                          GW = detail.GrossWeight,
                          PackageContainer = detail.PackageContainer,
                          PackageQty = detail.PackageQty,
                          PackageType = detail.PackageType,
                          CW = detail.ChargeWeight,
                          DatetimeCreated = detail.DatetimeCreated,
                          DatetimeModified = detail.DatetimeModified,
                          ParentId = detail.ParentId,
                          ShipmentEta = tran.Eta,
                          ShipmentEtd = tran.Etd,
                          ShipmentMawb = tran.Mawb,
                          UserCreated = detail.UserCreated
                      };
            List<CsTransactionDetailModel> results = new List<CsTransactionDetailModel>();
            if (res.Count() > 0)
            {
                results = res.Where(x => x.ParentId == null).OrderByDescending(o => o.DatetimeModified).ToList();
            }
            results.ForEach(fe =>
            {
                //var packages = csMawbcontainerRepo.Get(x => x.Hblid == fe.Id).Select(s => s.PackageQuantity != null ? (s.PackageQuantity + " x " + GetUnitNameById(s.PackageTypeId)) : string.Empty);
                var packages = fe.PackageQty != null ? (fe.PackageQty + "x" + (fe.PackageType != null? catUnitRepo.Get(x => x.Id == fe.PackageType)?.FirstOrDefault()?.UnitNameEn: "PKGS")) : string.Empty;
                fe.Packages = string.Join(", ", packages);
            });
            return results;
        }

        public IQueryable<CsTransactionDetail> GetHouseBill(string transactionType)
        {
            ICurrentUser _user = PermissionEx.GetUserMenuPermissionTransaction(transactionType, currentUser);
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            var houseBills = DataContext.Get();

            List<string> authorizeUserIds = authorizationRepository.Get(x => x.Active == true
                                                                 && x.AssignTo == currentUser.UserID
                                                                 && (x.EndDate.HasValue ? x.EndDate.Value : DateTime.Now.Date) >= DateTime.Now.Date
                                                                 && x.Services.Contains(transactionType)
                                                                 )?.Select(x => x.UserId).ToList();

            switch (rangeSearch)
            {
                case PermissionRange.All:
                    break;
                case PermissionRange.Owner:
                    houseBills = houseBills.Where(x => x.SaleManId == currentUser.UserID
                                                || authorizeUserIds.Contains(x.SaleManId)
                                                || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Group:
                    houseBills = houseBills.Where(x => (x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || authorizeUserIds.Contains(x.SaleManId)
                                                || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Department:
                    houseBills = houseBills.Where(x => (x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || authorizeUserIds.Contains(x.SaleManId)
                                                || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Office:
                    houseBills = houseBills.Where(x => (x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || authorizeUserIds.Contains(x.SaleManId)
                                                || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Company:
                    houseBills = houseBills.Where(x => x.CompanyId == currentUser.CompanyID
                                                || authorizeUserIds.Contains(x.SaleManId)
                                                || x.UserCreated == currentUser.UserID);
                    break;
            }
            if (houseBills == null)
                return null;
            return houseBills;
        }

        public IQueryable<CsTransactionDetailModel> GetListHouseBillAscHBL(CsTransactionDetailCriteria criteria)
        {
            var transactionType = DataTypeEx.GetType(criteria.TransactionType);
            var houseBills = GetHouseBill(transactionType);
            var query = from detail in houseBills.Where(x => x.ParentId == null)
                        join surcharge in surchareRepository.Get() on detail.Id equals surcharge.Hblid into surchargeTrans
                        from surcharge in surchargeTrans.DefaultIfEmpty()
                        select new { detail, surcharge };

            if (criteria.All == null)
            {
                query = query.Where(x =>
                        x.detail.JobId == criteria.JobId
                    &&
                        (x.detail.Hwbno ?? "").IndexOf(criteria.Hwbno ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    &&
                        ((x.detail.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                    &&
                        ((x.detail.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    &&
                    (
                        (x.surcharge.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.surcharge.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    &&
                    (
                        (x.surcharge.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.surcharge.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                );
            }
            else
            {
                query = query.Where(x =>
                        x.detail.JobId == criteria.JobId
                    ||
                        (x.detail.Hwbno ?? "").IndexOf(criteria.Hwbno ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    ||
                        ((x.detail.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                    ||
                        ((x.detail.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    ||
                    (
                        (x.surcharge.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.surcharge.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    ||
                    (
                        (x.surcharge.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.surcharge.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                );
            }
            var houseBillData = query.Select(s => s.detail).GroupBy(g => g.Id).Select(s => s.FirstOrDefault());
            var res = from detail in houseBillData//DataContext.Get()
                                                  //join tran in csTransactionRepo.Get() on detail.JobId equals tran.Id
                      join customer in catPartnerRepo.Get() on detail.CustomerId equals customer.Id into customers
                      from cus in customers.DefaultIfEmpty()
                      join shipper in catPartnerRepo.Get() on detail.ShipperId equals shipper.Id into shippers
                      from shipper in shippers.DefaultIfEmpty()
                      join consignee in catPartnerRepo.Get() on detail.ConsigneeId equals consignee.Id into consignees
                      from consignee in consignees.DefaultIfEmpty()
                      join saleman in sysUserRepo.Get() on detail.SaleManId equals saleman.Id into salemans
                      from sale in salemans.DefaultIfEmpty()
                      join notify in catPartnerRepo.Get() on detail.NotifyPartyId equals notify.Id into notifys
                      from notify in notifys.DefaultIfEmpty()
                      join port in catPlaceRepo.Get() on detail.Pod equals port.Id into portDetail
                      from pod in portDetail.DefaultIfEmpty()
                          //where detail.JobId == criteria.JobId
                      select new CsTransactionDetailModel
                      {
                          Id = detail.Id,
                          JobId = detail.JobId,
                          Hwbno = detail.Hwbno,
                          Mawb = detail.Mawb,
                          SaleManId = detail.SaleManId,
                          SaleManName = sale.Username,
                          CustomerId = detail.CustomerId,
                          CustomerName = cus.ShortName,
                          NotifyPartyId = detail.NotifyPartyId,
                          NotifyParty = notify.ShortName,
                          FinalDestinationPlace = detail.FinalDestinationPlace,
                          Eta = detail.Eta,
                          Etd = detail.Etd,
                          ConsigneeId = detail.ConsigneeId,
                          ConsigneeDescription = detail.ConsigneeDescription,
                          ShipperDescription = detail.ShipperDescription,
                          ShipperId = detail.ShipperId,
                          NotifyPartyDescription = detail.NotifyPartyDescription,
                          Pod = detail.Pod,
                          Pol = detail.Pol,
                          AlsoNotifyPartyId = detail.AlsoNotifyPartyId,
                          AlsoNotifyPartyDescription = detail.AlsoNotifyPartyDescription,
                          Hbltype = detail.Hbltype,
                          ReferenceNo = detail.ReferenceNo,
                          ColoaderId = detail.ColoaderId,
                          LocalVoyNo = detail.LocalVoyNo,
                          LocalVessel = detail.LocalVessel,
                          OceanVessel = detail.OceanVessel,
                          OceanVoyNo = detail.OceanVoyNo,
                          OriginBlnumber = detail.OriginBlnumber,
                          ShipperName = shipper.ShortName,
                          ConsigneeName = consignee.ShortName,
                          DesOfGoods = detail.DesOfGoods,
                          PODName = pod.NameEn,
                          ManifestRefNo = detail.ManifestRefNo,
                          ServiceType = detail.ServiceType,
                          ContSealNo = detail.ContSealNo,
                          SailingDate = detail.SailingDate,
                          FreightPayment = detail.FreightPayment,
                          PickupPlace = detail.PickupPlace,
                          DeliveryPlace = detail.DeliveryPlace,
                          GoodsDeliveryDescription = detail.GoodsDeliveryDescription,
                          GoodsDeliveryId = detail.GoodsDeliveryId,
                          ForwardingAgentDescription = detail.ForwardingAgentDescription,
                          ForwardingAgentId = detail.ForwardingAgentId,
                          MoveType = detail.MoveType,
                          ShippingMark = detail.ShippingMark,
                          InWord = detail.InWord,
                          OnBoardStatus = detail.OnBoardStatus,
                          Remark = detail.Remark,
                          PurchaseOrderNo = detail.PurchaseOrderNo,
                          OriginCountryId = detail.OriginCountryId,
                          CBM = detail.Cbm,
                          GW = detail.GrossWeight,
                          PackageContainer = detail.PackageContainer,
                          PackageQty = detail.PackageQty,
                          PackageType = detail.PackageType,
                          CW = detail.ChargeWeight,
                          DatetimeCreated = detail.DatetimeCreated,
                      };
            //Order tăng dần theo số House
            var results = res.ToArray().OrderBy(o => o.Hwbno).ToList();
            results.ForEach(fe =>
            {
                //Qty*Unit Cont of list Container HBL
                fe.Containers = string.Join(",", csMawbcontainerRepo.Get(x => x.Hblid == fe.Id)
                                                                        .Select(s => (s.ContainerTypeId != null || s.Quantity != null) ? (s.Quantity + "x" + GetUnitNameById(s.ContainerTypeId)) : string.Empty));
                //Qty*Unit Package of list Container HBL
                //fe.Packages = string.Join(",", csMawbcontainerRepo.Get(x => x.Hblid == fe.Id)
                //                                                        .Select(s => (s.PackageTypeId != null || s.PackageQuantity != null) ? (s.PackageQuantity + "x" + GetUnitNameById(s.PackageTypeId)) : string.Empty));
                var packages = fe.PackageQty != null ? (fe.PackageQty + "x" + (fe.PackageType != null ? catUnitRepo.Get(x => x.Id == fe.PackageType)?.FirstOrDefault()?.UnitNameEn : "PKGS")) : string.Empty;
                fe.Packages = string.Join(", ", packages);
            });
            return results.AsQueryable();
        }

        public List<CsTransactionDetailModel> Paging(CsTransactionDetailCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = Query(criteria);
            if(data == null)
            {
                rowsCount = 0;
                return null;
            }
            rowsCount = data.Count();
            List<CsTransactionDetailModel> results = new List<CsTransactionDetailModel>();
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = data.Skip((page - 1) * size).Take(size).ToList();
            }

            return results;
        }

        public object GetGoodSummaryOfAllHBLByJobId(Guid JobId)
        {
            var shipment = csTransactionRepo.Get(x => x.Id == JobId).FirstOrDefault();
            var houseBills = GetHouseBill(shipment.TransactionType);
            var houserbills = houseBills.Where(x => x.JobId == JobId && x.ParentId == null); ;//DataContext.Get(x => x.JobId == JobId);
            decimal? totalGW = 0;
            decimal? totalNW = 0;
            decimal? totalCW = 0;
            decimal? totalCbm = 0;
            var containers = string.Empty;
            var commodities = string.Empty;

            if (houserbills.Any())
            {
                foreach (var item in houserbills)
                {
                    var containersHbl = csMawbcontainerRepo.Get(x => x.Hblid == item.Id);
                    totalGW += containersHbl.Sum(s => s.Gw);
                    totalNW += containersHbl.Sum(s => s.Nw);
                    totalCW += containersHbl.Sum(s => s.ChargeAbleWeight);
                    totalCbm += containersHbl.Sum(s => s.Cbm);
                    containers += string.Join(",", containersHbl.OrderByDescending(o => o.ContainerTypeId).Select(s => (s.ContainerTypeId != null || s.Quantity != null) ? (s.Quantity + "x" + GetUnitNameById(s.ContainerTypeId)) : string.Empty));
                    commodities += string.Join(",", containersHbl.Select(x => GetCommodityNameById(x.CommodityId)));
                }
            }
            return new { totalGW, totalNW, totalCW, totalCbm, containers, commodities };
        }

        private string GetUnitNameById(short? id)
        {
            var result = string.Empty;
            var data = catUnitRepo.Get(g => g.Id == id).FirstOrDefault();
            result = (data != null) ? data.UnitNameEn : "PKGS";
            return result;
        }

        private string GetCommodityNameById(int? id)
        {
            var result = string.Empty;
            var data = catCommodityRepo.Get(x => x.Id == id).FirstOrDefault();
            result = (data != null) ? data.CommodityNameEn : string.Empty;
            return result;
        }
        #endregion -- LIST & PAGING HOUSEBILLS --

        public object ImportCSTransactionDetail(CsTransactionDetailModel model)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var detail = mapper.Map<CsTransactionDetail>(model);
                detail.Id = Guid.NewGuid();
                detail.Active = true;
                detail.UserCreated = model.UserCreated;
                detail.DatetimeCreated = DateTime.Now;
                detail.DesOfGoods = null;
                detail.Commodity = null;
                detail.PackageContainer = null;
                dc.CsTransactionDetail.Add(detail);

                List<CsMawbcontainer> containers = null;
                if (model.CsMawbcontainers.Count > 0)
                {
                    containers = mapper.Map<List<CsMawbcontainer>>(model.CsMawbcontainers);
                }
                else
                {
                    containers = dc.CsMawbcontainer.Where(y => y.Hblid == model.Id).ToList();
                }
                if (containers != null)
                {
                    foreach (var x in containers)
                    {
                        if (x.Id != Guid.Empty)
                        {
                            x.ContainerNo = string.Empty;
                            x.SealNo = string.Empty;
                            x.MarkNo = string.Empty;
                        }
                        x.Id = Guid.NewGuid();
                        x.Hblid = detail.Id;
                        x.Mblid = Guid.Empty;
                        x.UserModified = model.UserCreated;
                        x.DatetimeModified = DateTime.Now;
                        dc.CsMawbcontainer.Add(x);
                    }
                }
                var charges = dc.CsShipmentSurcharge.Where(x => x.Hblid == model.Id);
                if (charges != null)
                {
                    foreach (var charge in charges)
                    {
                        charge.Id = Guid.NewGuid();
                        charge.UserCreated = model.UserCreated;
                        charge.DatetimeCreated = DateTime.Now;
                        charge.Hblid = detail.Id;
                        charge.Soano = null;
                        charge.Soaclosed = null;
                        dc.CsShipmentSurcharge.Add(charge);
                    }
                }
                dc.SaveChanges();
                var result = new HandleState();
                return new { model = detail, result };
            }
            catch (Exception ex)
            {
                var result = new HandleState(ex.Message);
                return new { model = new object { }, result };
            }
        }

        public HandleState DeleteTransactionDetail(Guid hbId)
        {
            var hs = new HandleState();
            try
            {

                var hbl = DataContext.Where(x => x.Id == hbId).FirstOrDefault();
                if (hbl == null)
                {
                    hs = new HandleState("House Bill not found !");
                }
                else
                {
                    var job = csTransactionRepo.Get(x => x.Id == hbl.JobId).FirstOrDefault();
                    ICurrentUser _currentUser = PermissionEx.GetUserMenuPermissionTransaction(job.TransactionType, currentUser);
                    var permissionRange = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Delete);

                    int code = GetPermissionToDelete(new ModelUpdate { SaleManId = hbl.SaleManId, UserCreated = hbl.UserCreated, CompanyId = hbl.CompanyId, OfficeId = hbl.OfficeId, DepartmentId = hbl.DepartmentId, GroupId = hbl.GroupId }, permissionRange);
                    if (code == 403) return new HandleState(403,"");

                    var charges = surchareRepository.Get(x => x.Hblid == hbl.Id).ToList();
                    var isSOA = false;
                    foreach (var item in charges)
                    {
                        if (item.CreditNo != null || item.DebitNo != null || item.Soano != null)
                        {
                            isSOA = true;
                        }
                    }
                    if (isSOA == true)
                    {
                        hs = new HandleState("Cannot delete, this house bill is containing at least one charge have Credit Debit Note/SOA no!");
                    }
                    else
                    {
                        foreach (var item in charges)
                        {
                            surchareRepository.Delete(x => x.Id == item.Id, false);
                        }
                        DataContext.Delete(x => x.Id == hbl.Id);
                        DataContext.SubmitChanges();
                        surchareRepository.SubmitChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                hs = new HandleState(ex.Message);
            }

            return hs;

        }

        #region --- PREVIEW ---
        public Crystal Preview(CsTransactionDetailModel model)
        {
            if (model == null)
            {
                return null;
            }
            Crystal result = null;
            var parameter = new SeaHBillofLadingITLFrameParameter
            {
                Packages = model.PackageContainer,
                GrossWeight = model.GW == null ? (decimal)model.GW : 0,
                Measurement = string.Empty
            };
            result = new Crystal
            {
                ReportName = "SeaHBillofLadingITLFrame.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            var housebills = new List<SeaHBillofLadingITLFrame>();
            //continue
            var freightCharges = new List<FreightCharge>();
            if (freightCharges.Count == 0)
                return null;
            result.AddDataSource(housebills);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.AddSubReport("Freightcharges", freightCharges);
            result.SetParameter(parameter);
            return result;
        }
        public Crystal PreviewProofOfDelivery(Guid Id)
        {
            var data = GetById(Id);
            var listProof = new List<ProofOfDeliveryReport>();
            Crystal result = null;
            var _currentUser = currentUser.UserName;
            if (data != null)
            {
                var dataPOD = catPlaceRepo.First(x => x.Id == data.Pod);
                var dataPOL = catPlaceRepo.First(x => x.Id == data.Pol);
                var dataATTN = catPartnerRepo.First(x => x.Id == data.AlsoNotifyPartyId);
                var dataConsignee = catPartnerRepo.First(x => x.Id == data.ConsigneeId);

                var proofOfDelivery = new ProofOfDeliveryReport();
                proofOfDelivery.MAWB = data.Mawb?.ToUpper();
                proofOfDelivery.HWBNO = data.Hwbno?.ToUpper();
                proofOfDelivery.PortofDischarge = dataPOD?.NameEn?.ToUpper();
                proofOfDelivery.DepartureAirport = dataPOL?.NameEn?.ToUpper();
                proofOfDelivery.ShippingMarkImport = data.ShippingMark?.ToUpper();
                proofOfDelivery.Consignee = dataConsignee.PartnerNameEn?.ToUpper();
                proofOfDelivery.ATTN = dataATTN?.PartnerNameEn?.ToUpper();
                proofOfDelivery.TotalValue = 0;
                var csMawbcontainers = csMawbcontainerRepo.Get(x => x.Hblid == data.Id);
                foreach (var item in csMawbcontainers)
                {
                    proofOfDelivery.Description += item.Description + string.Join(",", data.Commodity);
                }
                proofOfDelivery.Description = proofOfDelivery.Description?.ToUpper();
                if (csMawbcontainers.Count() > 0)
                {
                    proofOfDelivery.NoPieces = csMawbcontainers.Sum(x => x.Quantity) ?? 0;
                    proofOfDelivery.GW = csMawbcontainers.Sum(x => x.Gw) ?? 0;
                    proofOfDelivery.NW = csMawbcontainers.Sum(x => x.Nw) ?? 0;
                }
                listProof.Add(proofOfDelivery);
            }

            var parameter = new ProofOfDeliveryReportParams();
            parameter.CompanyName = DocumentConstants.COMPANY_NAME;
            parameter.CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1;
            parameter.CompanyDescription = string.Empty;
            parameter.CompanyAddress2 = DocumentConstants.COMPANY_CONTACT;
            parameter.Website = DocumentConstants.COMPANY_WEBSITE;
            parameter.Contact = _currentUser;//Get user name login
            parameter.DecimalNo = 0; // set 0  temporary
            parameter.CurrDecimalNo = 0; //set 0 temporary
            result = new Crystal
            {
                ReportName = "SeaImpProofofDelivery.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listProof);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;

        }

        public Crystal PreviewAirProofOfDelivery(Guid Id)
        {
            var data = GetById(Id);
            var listProof = new List<AirProofOfDeliveryReport>();
            Crystal result = null;
            var _currentUser = currentUser.UserName;
            if (data != null)
            {
                var dataPOD = catPlaceRepo.First(x => x.Id == data.Pod);
                var dataPOL = catPlaceRepo.First(x => x.Id == data.Pol);
                var dataATTN = catPartnerRepo.First(x => x.Id == data.AlsoNotifyPartyId);
                var dataConsignee = catPartnerRepo.First(x => x.Id == data.ConsigneeId);
                var dataShipper = catPartnerRepo.First(x => x.Id == data.ShipperId);


                var proofOfDelivery = new AirProofOfDeliveryReport();
                proofOfDelivery.MAWB = data.Mawb?.ToUpper();
                proofOfDelivery.HWBNO = data.Hwbno?.ToUpper();
                proofOfDelivery.LastDestination = dataPOD?.NameEn?.ToUpper();
                proofOfDelivery.DepartureAirport = dataPOL?.NameEn?.ToUpper();
                proofOfDelivery.ShippingMarkImport = data.ShippingMark?.ToUpper();
                proofOfDelivery.Consignee = dataConsignee.PartnerNameEn?.ToUpper();
                proofOfDelivery.ATTN = dataATTN?.PartnerNameEn?.ToUpper();
                proofOfDelivery.TotalValue = 0;
                proofOfDelivery.Shipper = dataShipper.PartnerNameEn?.ToUpper();
                proofOfDelivery.WChargeable = data.ChargeWeight ?? 0;
                proofOfDelivery.Description = data.DesOfGoods?.ToUpper();
                proofOfDelivery.NoPieces = data.PackageQty ?? 0;
                proofOfDelivery.GW = data.GrossWeight ?? 0;
                proofOfDelivery.NW = data.NetWeight ?? 0;
                listProof.Add(proofOfDelivery);
            }

            var parameter = new ProofOfDeliveryReportParams();
            parameter.CompanyName = DocumentConstants.COMPANY_NAME;
            parameter.CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1;
            parameter.CompanyDescription = string.Empty;
            parameter.CompanyAddress2 = DocumentConstants.COMPANY_CONTACT;
            parameter.Website = DocumentConstants.COMPANY_WEBSITE;
            parameter.Contact = _currentUser;//Get user name login
            parameter.DecimalNo = 0; // set 0  temporary
            parameter.CurrDecimalNo = 0; //set 0 temporary
            result = new Crystal
            {
                ReportName = "AirImpProofofDelivery.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listProof);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }

        public Crystal PreviewAirDocumentRelease(Guid Id)
        {
            var data = GetById(Id);
            var listDocument = new List<AirDocumentReleaseReport>();
            Crystal result = null;
            var _currentUser = currentUser.UserName;
            // var _currentUser = string.Empty;
            if (data != null)
            {
                var dataPOD = catPlaceRepo.First(x => x.Id == data.Pod);
                var dataPOL = catPlaceRepo.First(x => x.Id == data.Pol);
                var dataATTN = catPartnerRepo.First(x => x.Id == data.AlsoNotifyPartyId);
                var dataConsignee = catPartnerRepo.First(x => x.Id == data.ConsigneeId);
                var dataShipper = catPartnerRepo.First(x => x.Id == data.ShipperId);

                var documentRelease = new AirDocumentReleaseReport();
                documentRelease.Consignee = dataConsignee.PartnerNameEn?.ToUpper();
                documentRelease.HWBNO = data.Hwbno?.ToUpper();
                documentRelease.FlightNo = data.FlightNo?.ToUpper();
                documentRelease.CussignedDate = data.FlightDate;
                documentRelease.DepartureAirport = dataPOL?.NameEn?.ToUpper();
                documentRelease.LastDestination = dataPOD?.NameEn?.ToUpper();
                documentRelease.NoPieces = data.PackageQty != null ? data.PackageQty.ToString() : "";
                documentRelease.WChargeable = data.ChargeWeight ?? 0;
                listDocument.Add(documentRelease);
            }

            var parameter = new AirDocumentReleaseReportParams();
            parameter.MAWB = data?.Mawb?.ToUpper();
            parameter.CompanyName = DocumentConstants.COMPANY_NAME;
            parameter.CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1;
            parameter.CompanyDescription = string.Empty;
            parameter.CompanyAddress2 = DocumentConstants.COMPANY_CONTACT;
            parameter.Website = DocumentConstants.COMPANY_WEBSITE;
            parameter.Contact = _currentUser;//Get user name login
            parameter.DecimalNo = 0; // set 0  temporary

            result = new Crystal
            {
                ReportName = "AirImptDocumentRelease.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listDocument);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }

        public Crystal PreviewSeaHBLofLading(Guid hblId, string reportType)
        {
            Crystal result = null;
            var data = GetById(hblId);
            var housebills = new List<SeaHBillofLadingReport>();
            if (data != null)
            {
                var dataPOD = catPlaceRepo.Get(x => x.Id == data.Pod).FirstOrDefault();
                var dataPOL = catPlaceRepo.Get(x => x.Id == data.Pol).FirstOrDefault();
                var dataATTN = catPartnerRepo.Get(x => x.Id == data.AlsoNotifyPartyId).FirstOrDefault();
                var dataConsignee = catPartnerRepo.Get(x => x.Id == data.ConsigneeId).FirstOrDefault();
                var dataShipper = catPartnerRepo.Get(x => x.Id == data.ShipperId).FirstOrDefault();

                var housebill = new SeaHBillofLadingReport();
                housebill.HWBNO = data.Hwbno?.ToUpper(); //HouseBill No
                housebill.OSI = string.Empty; //Để trống
                housebill.CheckNullAttach = string.Empty; //Để trống
                housebill.ReferrenceNo = data.ReferenceNo?.ToUpper(); //ReferenceNo
                housebill.Shipper = ReportUltity.ReplaceNullAddressDescription(data.ShipperDescription)?.ToUpper();//dataShipper?.PartnerNameEn; //Shipper name
                housebill.ConsigneeID = data.ConsigneeId; //NOT USE
                housebill.Consignee = ReportUltity.ReplaceNullAddressDescription(data.ConsigneeDescription)?.ToUpper();//dataConsignee?.PartnerNameEn;
                housebill.Notify = ReportUltity.ReplaceNullAddressDescription(data.NotifyPartyDescription)?.ToUpper();
                housebill.PlaceAtReceipt = data.PickupPlace?.ToUpper();// Place of receipt
                housebill.PlaceDelivery = data.DeliveryPlace?.ToUpper();// Place of Delivery
                housebill.LocalVessel = data.LocalVoyNo?.ToUpper();
                housebill.FromSea = string.Empty; //NOT USE
                housebill.OceanVessel = data.OceanVoyNo?.ToUpper();
                if (dataPOL != null)
                {
                    housebill.DepartureAirport = dataPOL?.NameEn?.ToUpper(); //POL
                    housebill.DepartureAirport = housebill.DepartureAirport?.ToUpper();
                }
                if (dataPOD != null)
                {
                    housebill.PortofDischarge = dataPOD?.NameEn?.ToUpper(); //POD
                    housebill.PortofDischarge = housebill.PortofDischarge?.ToUpper();
                }
                housebill.TranShipmentTo = string.Empty; //NOT USE
                housebill.GoodsDelivery = data.GoodsDeliveryDescription?.ToUpper(); //Good delivery
                housebill.CleanOnBoard = data.OnBoardStatus?.ToUpper(); //On board status                
                housebill.NoPieces = string.Empty; //Tạm thời để trống
                //var conts = csMawbcontainerRepo.Get(x => x.Hblid == data.Id);
                //if (conts != null && conts.Count() > 0)
                //{
                //    string hbConstainers = string.Empty;
                //    var contLast = conts.Last();
                //    foreach (var cont in conts)
                //    {
                //        var contUnit = catUnitRepo.Get(x => x.Id == cont.ContainerTypeId).FirstOrDefault();
                //        if (contUnit != null)
                //        {
                //            hbConstainers += (cont.Quantity + " x " + contUnit.UnitNameEn + (!cont.Equals(contLast) ? ", " : string.Empty));
                //        }
                //    }
                //    housebill.Qty = hbConstainers?.ToUpper(); //Qty Container (Số Lượng container + Cont Type)
                //    housebill.MaskNos = string.Join("\r\n", conts.Select(x => !string.IsNullOrEmpty(x.ContainerNo) || !string.IsNullOrEmpty(x.SealNo) ? x.ContainerNo + "-" + x.SealNo : string.Empty));
                //    housebill.MaskNos = housebill.MaskNos?.ToUpper();
                //}         
                housebill.MaskNos = data.ContSealNo?.ToUpper(); //Update lại MarkNo: lấy theo field ]Container No/ Cont Type/ Seal No]
                housebill.Description = data.DesOfGoods?.ToUpper();//Description of goods
                housebill.GrossWeight = data.GrossWeight ?? 0;
                housebill.GrwDecimal = 2;
                housebill.Unit = "KGS"; //Đang gán cứng (PKS update thành KGS)
                housebill.CBM = data.Cbm != null ? data.Cbm.Value : 0;
                housebill.CBMDecimal = 2;
                housebill.SpecialNote = data.ShippingMark; //Shipping Mark
                housebill.TotalPackages = string.Empty; //NOT USE
                housebill.OriginCode = string.Empty; //NOT USE
                housebill.ICASNC = string.Empty; //NOT USE
                housebill.Movement = data.MoveType?.ToUpper(); //Type of move
                housebill.AccountingInfo = string.Empty; //NOT USE
                housebill.SayWord = "SAY: " + data.InWord?.ToUpper(); //Inword
                housebill.strOriginLandPP = string.Empty; //NOT USE
                housebill.strOriginLandCC = string.Empty; //NOT USE
                housebill.strOriginTHCPP = string.Empty; //NOT USE
                housebill.strOriginTHCCC = string.Empty; //NOT USE
                housebill.strSeafreightPP = string.Empty; //NOT USE
                housebill.strSeafreightCC = string.Empty; //NOT USE
                housebill.strDesTHCPP = string.Empty; //NOT USE
                housebill.strDesTHCCC = string.Empty; //NOT USE
                housebill.strDesLandPP = string.Empty; //NOT USE
                housebill.strDesLandCC = string.Empty; //NOT USE
                housebill.FreightPayAt = data.PlaceFreightPay?.ToUpper(); //Freight Payable at
                housebill.ExecutedAt = data.IssueHblplace?.ToUpper(); //Place of Issue HBL
                housebill.ExecutedOn = data.DatetimeCreated != null ? data.DatetimeCreated.Value.ToString("dd MMM, yyyy").ToUpper() : string.Empty; //Created Date
                housebill.NoofOriginBL = data.OriginBlnumber != null ? API.Common.Globals.CustomData.NumberOfOriginBls.Where(x => x.Key == data.OriginBlnumber).Select(s => s.Value).FirstOrDefault() : string.Empty; //Number of Origin B/L
                housebill.ForCarrier = string.Empty; //Để trống
                housebill.SeaLCL = false; //NOT USE
                housebill.SeaFCL = false; //NOT USE
                housebill.ExportReferences = data.ExportReferenceNo; //NOT USE
                housebill.AlsoNotify = dataATTN?.PartnerNameEn; //NOT USE
                housebill.Signature = data.Hbltype ?? string.Empty; //HBL Type
                if (data.SailingDate != null)
                {
                    housebill.SailingDate = data.SailingDate.Value; //NOT USE
                }
                housebill.ShipPicture = null; //Để trống
                housebill.PicMarks = string.Empty; //Để trống

                housebills.Add(housebill);
            }

            string _reportName = string.Empty;
            switch (reportType)
            {
                case DocumentConstants.HBLOFLANDING_ITL:
                    _reportName = "SeaHBillofLadingITL.rpt";
                    break;
                case DocumentConstants.HBLOFLANDING_ITL_FRAME:
                    _reportName = "SeaHBillofLadingITLFrame.rpt";
                    break;
                case DocumentConstants.HBLOFLANDING_FBL_FRAME:
                    _reportName = "SeaHBillofLadingFBLFrame.rpt";
                    break;
                case DocumentConstants.HBLOFLANDING_ITL_KESCO:
                    _reportName = "SeaHBillofLadingITL_KESCO.rpt";
                    break;
                case DocumentConstants.HBLOFLANDING_ITL_FRAME_KESCO:
                    _reportName = "SeaHBillofLadingITLFrame_Kesco.rpt";
                    break;
                case DocumentConstants.HBLOFLANDING_ITL_SEKO:
                    _reportName = "SeaHBillofLadingITL_Seko.rpt";
                    break;
                case DocumentConstants.HBLOFLANDING_ITL_FRAME_SAMKIP:
                    _reportName = "SeaHBillofLadingITLFrame_SAMKIP.rpt";
                    break;
            }

            var freightCharges = new List<FreightCharge>() {
                new FreightCharge(){ FreightCharges = "FREIGHT " + data.FreightPayment?.ToUpper() }
            };

            result = new Crystal
            {
                ReportName = _reportName,
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(housebills);
            result.AddSubReport("Freightcharges", freightCharges);
            result.FormatType = ExportFormatType.PortableDocFormat;
            if (reportType == DocumentConstants.HBLOFLANDING_ITL
                || reportType == DocumentConstants.HBLOFLANDING_ITL_SEKO)
            {
                var parameter = new SeaHBillofLadingReportParams1()
                {
                    Packages = data.PackageQty != null ? data.PackageQty.ToString() : string.Empty, //field Package
                    GrossWeight = data?.GrossWeight.ToString(),
                    Measurement = data?.Cbm.ToString(),
                };
                result.SetParameter(parameter);
            }

            if (reportType == DocumentConstants.HBLOFLANDING_ITL_FRAME)
            {
                var parameter = new SeaHBillofLadingReportITLFRAMEParams()
                {
                    Packages = data.PackageQty != null ? data.PackageQty.ToString() : string.Empty, //field Package
                    GrossWeight = data?.GrossWeight.ToString(),
                    Measurement = data?.Cbm.ToString(),
                    TextInfo = "RECEIVED in apparent good order and condition except as otherwise noted the total number of Containers of other packages or units enumerated below for transportation from the place of receipt to the place of delivery subject to the terms detailed on the reverse side of this Bill of Lading. One of the signed bill of lading must be surrendered duly endorsed in exchange for the goods or delivery orther. On presentation of this document( duly endorsed) to the Carrier by or on behalf of the Holder the rights and liabilities arising in accordance with the terms here of shall( without prejudice to any rule of common law or statute rendering them biding on the Merchant) become binding hereby had been made between them.\r\nIN WITHNESS where of the stated number or original bills of lading all this tenor and date have been signed, one of which being accomplished, the other(s) to be void."
                };                
                result.SetParameter(parameter);
            }

            if (reportType == DocumentConstants.HBLOFLANDING_ITL_KESCO
                || reportType == DocumentConstants.HBLOFLANDING_ITL_FRAME_SAMKIP
                || reportType == DocumentConstants.HBLOFLANDING_ITL_FRAME_KESCO)
            {
                var parameter = new SeaHBillofLadingReportParams2()
                {
                    Packages = data.PackageQty != null ? data.PackageQty.ToString() : string.Empty, //field Package
                    GrossWeight = data?.GrossWeight.ToString(),
                    Measurement = data?.Cbm.ToString(),
                    DocumentNo = string.Empty //Tạm thời để trống
                };
                result.SetParameter(parameter);
            }
            
            return result;
        }

        public Crystal PreviewHouseAirwayBillLastest(Guid hblId, string reportType)
        {
            Crystal result = null;
            var data = GetById(hblId);
            var housebills = new List<HouseAirwayBillLastestReport>();
            if (data != null)
            {
                var dataPOD = catPlaceRepo.Get(x => x.Id == data.Pod).FirstOrDefault();
                var dataPOL = catPlaceRepo.Get(x => x.Id == data.Pol).FirstOrDefault();

                var housebill = new HouseAirwayBillLastestReport();
                housebill.MAWB = data.Mawb; //NOT USE
                housebill.HWBNO = data.Hwbno?.ToUpper(); //Housebill No
                housebill.ATTN = ReportUltity.ReplaceNullAddressDescription(data.ShipperDescription)?.ToUpper(); //ShipperName & Address
                housebill.ISSUED = string.Empty; //NOT USE
                housebill.ConsigneeID = data.ConsigneeId; //NOT USE
                housebill.Consignee = ReportUltity.ReplaceNullAddressDescription(data.ConsigneeDescription)?.ToUpper(); //Consignee & Address
                housebill.ICASNC = string.Empty; //NOT USE
                housebill.AccountingInfo = "FREIGHT " + data.FreightPayment?.ToUpper(); //'FREIGHT ' + Air Freight
                housebill.AgentIATACode = string.Empty; //Gán rỗng
                housebill.AccountNo = string.Empty; //NOT USE
                if (dataPOL != null)
                {
                    var polCountry = countryRepository.Get(x => x.Id == dataPOL.CountryId).FirstOrDefault()?.NameEn;
                    housebill.DepartureAirport = dataPOL?.NameEn + (!string.IsNullOrEmpty(polCountry) ? ", " + polCountry : string.Empty); //AOL - Departure
                    housebill.DepartureAirport = housebill.DepartureAirport?.ToUpper();
                }
                housebill.ReferrenceNo = string.Empty; //NOT USE
                housebill.OSI = string.Empty; //NOT USE
                housebill.FirstDestination = data.FirstCarrierTo?.ToUpper();
                housebill.FirstCarrier = data.FirstCarrierBy?.ToUpper();
                housebill.SecondDestination = data.TransitPlaceTo1?.ToUpper();
                housebill.SecondCarrier = data.TransitPlaceBy1?.ToUpper();
                housebill.ThirdDestination = data.TransitPlaceTo2?.ToUpper();
                housebill.ThirdCarrier = data.TransitPlaceBy2?.ToUpper();
                housebill.Currency = data.CurrencyId?.ToUpper(); //Currency
                housebill.CHGSCode = data.Chgs?.ToUpper(); //CHGS
                housebill.WTPP = data.WtorValpayment?.ToUpper(); //WT/VAL là PP
                housebill.WTCLL = data.WtorValpayment?.ToUpper(); //WT/VAL là CLL
                housebill.ORPP = data.OtherPayment?.ToUpper(); //Other Là PP
                housebill.ORCLL = data.OtherPayment?.ToUpper(); //Other Là CLL
                housebill.DlvCarriage = data.Dclrca?.ToUpper(); //DCLR-CA
                housebill.DlvCustoms = data.Dclrcus?.ToUpper(); //DCLR-CUS
                if (dataPOD != null)
                {
                    var podCountry = countryRepository.Get(x => x.Id == dataPOD.CountryId).FirstOrDefault()?.NameEn;
                    housebill.LastDestination = dataPOL?.NameEn + (!string.IsNullOrEmpty(podCountry) ? ", " + podCountry : string.Empty); //AOD - DestinationAirport
                    housebill.LastDestination = housebill.LastDestination?.ToUpper();
                }
                housebill.FlightNo = data.FlightNo?.ToUpper(); //Flight No
                housebill.FlightDate = data.FlightDate; //Flight Date
                housebill.ConnectingFlight = string.Empty; //Để rỗng
                housebill.ConnectingFlightDate = null; //Gán null
                housebill.insurAmount = data.IssuranceAmount?.ToUpper(); //Issurance Amount
                housebill.HandlingInfo = data.HandingInformation?.ToUpper(); //Handing Information
                housebill.Notify = data.Notify?.ToUpper(); //Notify
                housebill.SCI = string.Empty; //NOT USE
                housebill.NoPieces = data.PackageQty != null ? data.PackageQty.ToString() : string.Empty; //Số kiện (Pieces)
                housebill.GrossWeight = data.GrossWeight ?? 0; //GrossWeight
                housebill.GrwDecimal = 2; //NOT USE
                housebill.Wlbs = data.KgIb?.ToUpper(); //KgIb
                housebill.RateClass = string.Empty; //NOT USE
                housebill.ItemNo = data.ComItemNo?.ToUpper(); //ComItemNo - Commodity Item no
                housebill.WChargeable = data.ChargeWeight ?? 0; //CW
                housebill.ChWDecimal = 2; //NOT USE
                housebill.Rchge = data.RateCharge != null ? data.RateCharge.ToString() : string.Empty; //RateCharge
                housebill.Ttal = data.Total != null ? data.Total.ToString() : string.Empty;
                housebill.Description = data.DesOfGoods?.ToUpper(); //Natural and Quality Goods
                housebill.WghtPP = data.Wtpp?.ToUpper(); //WT (prepaid)
                housebill.WghtCC = data.Wtcll?.ToUpper(); //WT (Collect)
                housebill.ValChPP = string.Empty; //NOT USE
                housebill.ValChCC = string.Empty; //NOT USE
                housebill.TxPP = string.Empty; //NOT USE
                housebill.TxCC = string.Empty; //NOT USE
                housebill.OrchW = data.OtherCharge?.ToUpper(); //Other Charge
                housebill.OChrVal = string.Empty; //NOT USE
                housebill.TTChgAgntPP = data.DueAgentPp?.ToUpper(); //Due to agent (prepaid)
                housebill.TTChgAgntCC = data.DueAgentCll?.ToUpper(); //Due to agent (Collect)
                housebill.TTCarrPP = string.Empty; //NOT USE
                housebill.TTCarrCC = string.Empty; //NOT USE
                housebill.TtalPP = data.TotalPp?.ToUpper(); //Total (prepaid)
                housebill.TtalCC = data.TotalCll?.ToUpper(); //Total (Collect)
                housebill.CurConvRate = string.Empty; //NOT USE
                housebill.CCChgDes = string.Empty; //NOT USE
                housebill.SpecialNote = data.ShippingMark?.ToUpper(); //Shipping Mark
                housebill.ShipperCertf = string.Empty; //NOT USE
                housebill.ExecutedOn = data.IssueHblplace?.ToUpper(); //Issued On
                housebill.ExecutedAt = data.IssueHbldate != null ? data.IssueHbldate.Value.ToString("dd MMM, yyyy")?.ToUpper() : string.Empty; //Issue At
                housebill.Signature = string.Empty; //NOT USE
                var dimHbl = dimensionDetailService.Get(x => x.Hblid == hblId);
                string _dimensions = string.Join("\r\n", dimHbl.Select(s => (int)s.Length + "*" + (int)s.Width + "*" + (int)s.Height + "*" + (int)s.Package));
                housebill.Dimensions = _dimensions; //Dim (Cộng chuỗi theo Format L*W*H*PCS, mỗi dòng cách nhau bằng enter)
                housebill.ShipPicture = null; //NOT USE
                housebill.PicMarks = string.Empty; //Gán rỗng
                housebill.GoodsDelivery = string.Empty; //Chưa biết

                housebills.Add(housebill);
            }
            string _reportName = string.Empty;
            switch (reportType)
            {
                case DocumentConstants.HOUSEAIRWAYBILLLASTEST_ITL:
                    _reportName = "HouseAirwayBillLastestITL.rpt";
                    break;
                case DocumentConstants.HOUSEAIRWAYBILLLASTEST_ITL_FRAME:
                    _reportName = "HouseAirwayBillLastestITLFrame.rpt";
                    break;
            }
            result = new Crystal
            {
                ReportName = _reportName,
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(housebills);
            result.FormatType = ExportFormatType.PortableDocFormat;
            var parameter = new HouseAirwayBillLastestReportParams()
            {
                MAWBN = data != null ? data.Mawb?.ToUpper() : string.Empty
            };
            result.SetParameter(parameter);
            return result;
        }

        public Crystal PreviewAirAttachList(Guid hblId)
        {
            Crystal result = null;
            var data = GetById(hblId);
            var housebills = new List<AirAttachedListReport>();
            if (data != null)
            {
                var housebill = new AirAttachedListReport();
                housebill.HBLNo = data.Hwbno?.ToUpper();
                housebill.IssuedDate = data.Etd;//ETD of Housebill
                housebill.AttachedList = ReportUltity.ReplaceHtmlBaseForPreviewReport(data.AttachList); // (Không Upper Case)
                housebills.Add(housebill);
            }
            result = new Crystal
            {
                ReportName = "AirAttachedList.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(housebills);
            result.FormatType = ExportFormatType.PortableDocFormat;
            return result;
        }

        public Crystal PreviewAirImptAuthorisedLetter(Guid housbillId)
        {
            Crystal result = null;
            var data = GetById(housbillId);
            var authorizeLetters = new List<AirImptAuthorisedLetterReport>();
            if (data != null)
            {
                var authorizeLetter = new AirImptAuthorisedLetterReport
                {
                    HWBNO = data.Hwbno?.ToUpper(),
                    DONo = data.DeliveryOrderNo?.ToUpper(),
                    Consignee = data.ConsigneeDescription?.ToUpper(),
                    FlightNo = data.FlightNo?.ToUpper(),
                    FlightDate = data.FlightDate,
                    DepartureAirport = data.Route,//data.PODName?.ToUpper(), (change: tuyến sẽ lấy Route)
                    NoPieces = data.PackageQty?.ToString() + " " + catUnitRepo.Get(x => x.Id == data.PackageType).FirstOrDefault()?.UnitNameEn?.ToUpper(),
                    Description = data.DesOfGoods?.ToUpper(),
                    WChargeable = data.GrossWeight,//data.ChargeWeight, (change: trọng lượng sẽ lấy Gross Weight)
                    DeliveryOrderNote = string.Empty,//data.DeliveryOrderNo?.ToUpper(),
                    FirstDestination = data.DosentTo1?.ToUpper(),//data.FirstCarrierTo?.ToUpper(),
                    SecondDestination = data.SubAbbr?.ToUpper(),//data.TransitPlaceTo1?.ToUpper(),
                    Notify = data.NotifyPartyDescription?.ToUpper()
                };
                authorizeLetters.Add(authorizeLetter);
            }
            var parameter = new AirImptAuthorisedLetterReportParameter
            {
                MAWB = data.Mawb?.ToUpper(),
                CompanyName = DocumentConstants.COMPANY_NAME,
                CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1,
                CompanyAddress2 = DocumentConstants.COMPANY_CONTACT,
                Website = DocumentConstants.COMPANY_TAXCODE,//DocumentConstants.COMPANY_WEBSITE, (Sửa lại thành MST)
                DecimalNo = 2,
                PrintDay = string.Empty,
                PrintMonth = string.Empty,
                PrintYear = string.Empty
            };
            if(data.DeliveryOrderPrintedDate != null)
            {
                parameter.PrintDay = data.DeliveryOrderPrintedDate.Value.Day.ToString();
                parameter.PrintMonth = data.DeliveryOrderPrintedDate.Value.Month.ToString();
                parameter.PrintYear = data.DeliveryOrderPrintedDate.Value.Year.ToString();
            }
            result = new Crystal
            {
                ReportName = "AirImptAuthorisedLetter.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(authorizeLetters);
            result.SetParameter(parameter);
            result.FormatType = ExportFormatType.PortableDocFormat;
            return result;
        }

        public Crystal PreviewAirImptAuthorisedLetterConsign(Guid housbillId)
        {
            Crystal result = null;
            var data = GetById(housbillId);
            var authorizeLetters = new List<AirImptAuthorisedLetterReport>();
            if (data != null)
            {
                var authorizeLetter = new AirImptAuthorisedLetterReport
                {
                    HWBNO = data.Hwbno?.ToUpper(),
                    DONo = data.DeliveryOrderNo?.ToUpper(),
                    Consignee = data.ConsigneeDescription?.ToUpper(),
                    FlightNo = data.FlightNo?.ToUpper(),
                    FlightDate = data.FlightDate,
                    NoPieces = data.PackageQty?.ToString() + " " + catUnitRepo.Get(x => x.Id == data.PackageType).FirstOrDefault()?.UnitNameEn?.ToUpper(),
                    Description = data.DesOfGoods?.ToUpper(),
                    WChargeable = data.GrossWeight,//data.ChargeWeight, (change: trọng lượng sẽ lấy Gross Weight)
                    DeliveryOrderNote = string.Empty,//data.DeliveryOrderNo?.ToUpper(),
                    FirstDestination = data.DosentTo1?.ToUpper(),//data.FirstCarrierTo?.ToUpper(),
                    SecondDestination = data.SubAbbr?.ToUpper(),//data.TransitPlaceTo1?.ToUpper(),
                    CBM = data.ChargeWeight,//data.Cbm, (change: khối lượng sẽ lấy Charge Weight)
                    Notify = data.NotifyPartyDescription?.ToUpper()
                };
                authorizeLetters.Add(authorizeLetter);
            }
            var parameter = new AirImptAuthorisedLetterReportParameter
            {
                MAWB = data.Mawb?.ToUpper(),
                CompanyName = DocumentConstants.COMPANY_NAME,
                CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1,
                CompanyAddress2 = DocumentConstants.COMPANY_CONTACT,
                Website = DocumentConstants.COMPANY_TAXCODE,//DocumentConstants.COMPANY_WEBSITE, (Sửa lại thành MST)
                DecimalNo = 2,
                PrintDay = string.Empty,
                PrintMonth = string.Empty,
                PrintYear = string.Empty
            };
            if (data.DeliveryOrderPrintedDate != null)
            {
                parameter.PrintDay = data.DeliveryOrderPrintedDate.Value.Day.ToString();
                parameter.PrintMonth = data.DeliveryOrderPrintedDate.Value.Month.ToString();
                parameter.PrintYear = data.DeliveryOrderPrintedDate.Value.Year.ToString();
            }
            result = new Crystal
            {
                ReportName = "AirImptAuthorisedLetter_Consign.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(authorizeLetters);
            result.SetParameter(parameter);
            result.FormatType = ExportFormatType.PortableDocFormat;
            return result;
        }
        
        public Crystal PreviewBookingNote(BookingNoteCriteria criteria)
        {
            Crystal result = null;
            var data = GetById(criteria.HblId);
            var bookingNotes = new List<BookingNoteReport>();
            if(data != null)
            {
                var bookingNote = new BookingNoteReport();
                bookingNote.FlexId = criteria.FlexId?.ToUpper();
                bookingNote.Shipper = catPartnerRepo.Get(x => x.Id == data.ShipperId).FirstOrDefault()?.ShortName?.ToUpper();
                bookingNote.Consignee = catPartnerRepo.Get(x => x.Id == data.ConsigneeId).FirstOrDefault()?.ShortName?.ToUpper();
                bookingNote.HawbNo = data.Hwbno?.ToUpper();
                bookingNote.MawbNo = data.Mawb?.ToUpper();
                var _pol = catPlaceRepo.Get(x => x.Id == data.Pol).FirstOrDefault();
                var _pod = catPlaceRepo.Get(x => x.Id == data.Pod).FirstOrDefault();
                var _airportOfDischarge = !string.IsNullOrEmpty(data.FirstCarrierTo) ? data.FirstCarrierTo : _pod?.Code;
                var _flightNo = _pol?.Code + "-" + _airportOfDischarge + ":" +data.FlightNo + "/" + (data.Etd != null ? data.Etd.Value.ToString("dd MMM") : string.Empty);
                bookingNote.FlightNo1 = _flightNo?.ToUpper();
                bookingNote.FlightNo2 = criteria.FlightNo2?.ToUpper();
                bookingNote.DepartureAirport = _pol?.Code?.ToUpper(); //Lấy Code
                bookingNote.PlaceOfReceipt = data.PickupPlace?.ToUpper();
                bookingNote.AirportOfDischarge = _airportOfDischarge?.ToUpper();
                bookingNote.DestinationAirport = _pod != null ? (_pod?.NameEn + " - " + _pod?.Code)?.ToUpper() : string.Empty; //Lấy Name - Code
                bookingNote.TotalCollect = data.TotalCll?.ToUpper();
                bookingNote.TotalPrepaid = data.TotalPp?.ToUpper();
                bookingNote.ShippingMark = data.ShippingMark?.ToUpper();
                bookingNote.Pieces = data.PackageQty;
                bookingNote.DesOfGood = data.DesOfGoods?.ToUpper();
                bookingNote.Gw = data.GrossWeight;
                bookingNote.Cbm = data.Cbm;
                bookingNote.ContactPerson = criteria.ContactPerson;
                bookingNote.ClosingTime = criteria.ClosingTime?.ToUpper();
                bookingNote.CurrentUser = currentUser.UserName?.ToUpper();
                bookingNote.CurrentDate = DateTime.Now.ToString("dd MMM yyyy").ToUpper();

                bookingNotes.Add(bookingNote);
            }
            result = new Crystal
            {
                ReportName = criteria.ReportType == "BN_SCSC" ? "BookingNoteAir_SCSC.rpt" : "BookingNoteAir_TCS.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(bookingNotes);
            result.FormatType = ExportFormatType.PortableDocFormat;
            return result;

        }
        public IQueryable<CsTransactionDetailModel> GetDataHawbToCheckExisted()
        {
            var transactionData = csTransactionRepo.Get();
            var transactionDetailData = csTransactionDetailRepo.Get();
            var data = from t in transactionData
                       join d in transactionDetailData on t.Id equals d.JobId
                       select new CsTransactionDetailModel
                       {
                           Id = d.Id,
                           TransactionType = t.TransactionType,
                           Hwbno = d.Hwbno,
                           JobId = d.JobId
                       };
            return data;
        }
        #endregion --- PREVIEW ---

        #region --- Export ---
        public AirwayBillExportResult NeutralHawbExport(Guid housebillId, Guid officeId)
        {
            var hbDetail = GetById(housebillId);
            if (hbDetail == null) return null;

            var office = sysOfficeRepo.Get(x => x.Id == officeId).FirstOrDefault();
            var result = new AirwayBillExportResult();
            result.MawbNo = hbDetail.Mawb;
            var pol = catPlaceRepo.Get(x => x.Id == hbDetail.Pol).FirstOrDefault();
            var pod = catPlaceRepo.Get(x => x.Id == hbDetail.Pod).FirstOrDefault();
            result.AolCode = pol?.Code;
            result.HawbNo = hbDetail.Hwbno;
            result.Shipper = hbDetail.ShipperDescription;
            result.OfficeUserCurrent = office?.BranchNameEn;
            result.Consignee = hbDetail.ConsigneeDescription;

            var _airFrieghtDa = string.Empty;
            if (!string.IsNullOrEmpty(hbDetail.FreightPayment))
            {
                if (hbDetail.FreightPayment == "Sea - Air Difference" || hbDetail.FreightPayment == "Prepaid")
                {
                    _airFrieghtDa = "PP IN " + pol?.Code;
                }
                else
                {
                    _airFrieghtDa = "CLL IN " + pod?.Code;
                }
            }            
            result.AirFrieghtDa = _airFrieghtDa;

            result.DepartureAirport = pol?.NameEn;
            result.FirstTo = hbDetail.FirstCarrierTo;
            result.FirstCarrier = hbDetail.FirstCarrierBy;
            result.SecondTo = hbDetail.TransitPlaceTo2;
            result.SecondBy = hbDetail.TransitPlaceBy2;
            result.Currency = hbDetail.CurrencyId;
            result.Dclrca = hbDetail.Dclrca;
            result.Dclrcus = hbDetail.Dclrcus;
            result.DestinationAirport = pod?.NameEn;
            result.FlightNo = hbDetail.FlightNo;
            result.FlightDate = hbDetail.FlightDate;
            result.IssuranceAmount = hbDetail.IssuranceAmount;
            result.HandingInfo = hbDetail.HandingInformation;
            result.Pieces = hbDetail.PackageQty;
            result.Gw = hbDetail.GrossWeight;
            result.Cw = hbDetail.ChargeWeight;
            result.RateCharge = hbDetail.RateCharge;
            result.Total = hbDetail.Total;
            result.DesOfGood = hbDetail.DesOfGoods;
            var dimHbl = dimensionDetailService.Get(x => x.Hblid == housebillId);
            string _dimensions = string.Join("\r\n", dimHbl.Select(s => Math.Round(s.Length.Value, 2) + "*" + Math.Round(s.Width.Value, 2) + "*" + Math.Round(s.Height.Value, 2) + "*" + Math.Round(s.Package.Value, 2)));
            result.VolumeField = _dimensions;
            result.PrepaidTotal = hbDetail.TotalPp;
            result.CollectTotal = hbDetail.TotalCll;
            result.IssueOn = hbDetail.IssueHblplace;
            result.IssueDate = hbDetail.IssueHbldate;
            return result;
        }
        #endregion --- Export ---
    }
}
