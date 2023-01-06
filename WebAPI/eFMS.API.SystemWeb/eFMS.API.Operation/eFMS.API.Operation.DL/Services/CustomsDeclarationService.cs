﻿using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.API.Operation.DL.Common;
using eFMS.API.Operation.DL.IService;
using eFMS.API.Operation.DL.Models;
using eFMS.API.Operation.DL.Models.Criteria;
using eFMS.API.Operation.DL.Models.Ecus;
using eFMS.API.Operation.Service.Contexts;
using eFMS.API.Operation.Service.Models;
using eFMS.API.Operation.Service.ViewModels;
using eFMS.API.Provider.Services.IService;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eFMS.API.Operation.DL.Services
{
    public class CustomsDeclarationService : RepositoryBase<CustomsDeclaration, CustomsDeclarationModel>, ICustomsDeclarationService
    {
        private readonly ICatPlaceApiService catPlaceApi;
        private readonly IEcusConnectionService ecusCconnectionService;
        private readonly ICatCountryApiService countryApi;
        private readonly ICatCommodityApiService commodityApi;
        private readonly ICurrentUser currentUser;
        private readonly IStringLocalizer stringLocalizer;
        private readonly IContextBase<CatCommodity> commodityRepository;
        private readonly IContextBase<OpsTransaction> opsTransactionRepo;
        private readonly IContextBase<OpsStageAssigned> opsStageAssignedRepo;
        private readonly IContextBase<SysUser> userRepository;
        private readonly IContextBase<CatPartner> customerRepository;
        private readonly IContextBase<AcctAdvanceRequest> accAdvanceRequestRepository;
        private readonly IContextBase<AcctAdvancePayment> accAdvancePaymentRepository;

        readonly IContextBase<CsShipmentSurcharge> csShipmentSurchargeRepo;

        public CustomsDeclarationService(IContextBase<CustomsDeclaration> repository, IMapper mapper,
            IEcusConnectionService ecusCconnection
            , ICatPlaceApiService catPlace
            , ICatCountryApiService country
            , ICatCommodityApiService commodity
            , ICurrentUser user,
            IStringLocalizer<LanguageSub> localizer,
            IContextBase<CatCommodity> commodityRepo,
            IContextBase<OpsTransaction> opsTransaction,
            IContextBase<OpsStageAssigned> opsStageAssigned,
            IContextBase<SysUser> userRepo,
            IContextBase<CsShipmentSurcharge> csShipmentSurcharge,
            IContextBase<AcctAdvanceRequest> accAdvanceRequestRepo,
            IContextBase<AcctAdvancePayment> accAdvancePaymentRepo,
        IContextBase<CatPartner> customerRepo) : base(repository, mapper)
        {
            ecusCconnectionService = ecusCconnection;
            catPlaceApi = catPlace;
            countryApi = country;
            commodityApi = commodity;
            currentUser = user;
            stringLocalizer = localizer;
            commodityRepository = commodityRepo;
            opsTransactionRepo = opsTransaction;
            opsStageAssignedRepo = opsStageAssigned;
            userRepository = userRepo;
            customerRepository = customerRepo;
            csShipmentSurchargeRepo = csShipmentSurcharge;
            accAdvanceRequestRepository = accAdvanceRequestRepo;
            accAdvancePaymentRepository = accAdvancePaymentRepo;
        }

        public IQueryable<CustomsDeclarationModel> GetAll()
        {
            return Get();
        }

        public HandleState ImportClearancesFromEcus()
        {
            string userId = currentUser.UserID;
            var connections = ecusCconnectionService.Get(x => x.UserId == userId && x.Active == true);
            var result = new HandleState();
            var lists = new List<CustomsDeclaration>();
            try
            {
                foreach (var item in connections)
                {
                    var clearanceEcus = ecusCconnectionService.GetDataEcusByUser(item.UserId, item.ServerName, item.Dbusername, item.Dbpassword, item.Dbname);
                    if (clearanceEcus == null)
                    {
                        continue;
                    }
                    else
                    {
                        var clearances = DataContext.Get();
                        var cleancesNotExsitInFMS = clearanceEcus.Where(x => !checkExistEcusInEFMS(x.SOTK.ToString()));
                        if (cleancesNotExsitInFMS.Count() > 0)
                        {
                            foreach (var d in cleancesNotExsitInFMS)
                            {
                                var newClearance = MapEcusClearanceToCustom(d, d.SOTK?.ToString().Trim());
                                newClearance.Source = OperationConstants.FromEcus;
                                lists.Add(newClearance);
                            }
                        }
                        //foreach (var clearance in clearanceEcus)
                        //{
                        //    var clearanceNo = clearance.SOTK?.ToString().Trim();
                        //    var itemExisted = clearances.FirstOrDefault(x => x.ClearanceNo == clearanceNo && x.ClearanceDate == clearance.NGAY_DK);
                        //    var countDuplicated = lists.Count(x => x.ClearanceNo == clearanceNo && x.ClearanceDate == clearance.NGAY_DK);
                        //    if (itemExisted == null && clearanceNo != null && countDuplicated < 1)
                        //    {
                        //        var newClearance = MapEcusClearanceToCustom(clearance, clearanceNo);
                        //        newClearance.Source = OperationConstants.FromEcus;
                        //        lists.Add(newClearance);
                        //    }
                        //}
                    }
                }
                if (lists.Count > 0)
                {
                    HandleState hs = DataContext.Add(lists);
                    if (hs.Success)
                    {
                        result = new HandleState(true, stringLocalizer[OperationLanguageSub.MSG_CUSTOM_CLEARANCE_ECUS_CONVERT_SUCCESS, lists.Count]);

                        string logErr = String.Format("Import Ecus thành công {0} \n {1} Tờ khai {2}", currentUser.UserName, lists.Count, DateTime.Now);
                        new LogHelper("ECUS", logErr);
                    }
                    else
                    {
                        result = new HandleState(true, stringLocalizer[OperationLanguageSub.MSG_CUSTOM_CLEARANCE_ECUS_CONVERT_SUCCESS, 0]);
                        string logErr = String.Format("{0} Import thất bại {1} Tờ khai do {2} at {3}", currentUser.UserName, lists.Count, hs.Message, DateTime.Now);
                        new LogHelper("ECUS", logErr);
                    }
                }
                else
                {
                    result = new HandleState(true, stringLocalizer[OperationLanguageSub.MSG_CUSTOM_CLEARANCE_ECUS_CONVERT_NO_DATA]);
                    string logErr = String.Format("Import thất bại {0} \n {1} Tờ khai {2}", currentUser.UserName, lists.Count, DateTime.Now);
                    new LogHelper("ECUS", logErr);
                }
            }
            catch (Exception ex)
            {
                string logErr = String.Format("Lỗi import Ecus {0} \n {1} {2}", currentUser.UserID, ex.ToString(), DateTime.Now);
                new LogHelper("ECUS", logErr);
                result = new HandleState(ex.Message);
            }
            return result;
        }

        private bool checkExistEcusInEFMS(string tokhai)
        {
            var result = false;
            var clearances = DataContext.Get();

            var data = clearances.Where(x => x.ClearanceNo == tokhai).ToList();
            if (data.Count > 0)
            {
                result = true;
            }

            return result;
        }

        private CustomsDeclaration MapEcusClearanceToCustom(DTOKHAIMD clearance, string clearanceNo)
        {
            var type = ClearanceConstants.Export_Type_Value;
            if (clearance.XorN != null)
            {
                if (clearance.XorN.Contains(ClearanceConstants.Import_Type))
                {
                    type = ClearanceConstants.Import_Type_Value;
                }
            }
            var serviceType = GetServiceType(clearance.MA_HIEU_PTVC, out string cargoType);
            var route = clearance.PLUONG != null ? GetRouteType(clearance.PLUONG) : string.Empty;
            //var partnerTaxCode = clearance.MA_DV;
            if (clearance.MA_DV != null)
            {
                Regex pattern = new Regex("[-_ ]");
                pattern.Replace(clearance.MA_DV, "");
            }
            string _accountNo = clearance.MA_DV;
            var partner = customerRepository.Get(x => x.Active == true && x.TaxCode.Trim() == clearance.MA_DV.Trim())?.FirstOrDefault();
            if (partner != null)
            {
                _accountNo = partner.AccountNo;
            }
            var newItem = new CustomsDeclaration
            {
                IdfromEcus = clearance.DToKhaiMDID,
                ClearanceNo = clearanceNo,
                FirstClearanceNo = clearance.SOTK_DAU_TIEN,
                ClearanceDate = clearance.NGAY_DK,
                PartnerTaxCode = clearance.MA_DV,
                Mblid = clearance.VAN_DON,
                Hblid = clearance.VAN_DON,
                Gateway = clearance.MA_CK,
                PortCodeNn = clearance.MA_CANGNN,
                ExportCountryCode = clearance.NUOC_XK,
                ImportCountryCode = clearance.NUOC_NK,
                Pcs = (int?)clearance.SO_KIEN ?? null,
                UnitCode = clearance.DVT_KIEN,
                QtyCont = (int?)clearance.SO_CONTAINER ?? null,
                GrossWeight = clearance.TR_LUONG,
                Route = route,
                Type = type,
                CargoType = cargoType,
                ServiceType = serviceType,
                Shipper = clearance.DV_DT,
                Consignee = clearance._Ten_DV_L1,
                UserCreated = currentUser.UserID,
                DatetimeCreated = DateTime.Now,
                DatetimeModified = DateTime.Now,
                GroupId = currentUser.GroupId,
                DepartmentId = currentUser.DepartmentId,
                OfficeId = currentUser.OfficeID,
                CompanyId = currentUser.CompanyID,
                AccountNo = _accountNo,
                Eta = clearance.NGAYDEN
            };
            return newItem;
        }

        private string GetRouteType(string luong)
        {
            var route = string.Empty;
            luong = luong.ToLower().Trim();
            switch (luong)
            {
                case ClearanceConstants.Route_Type_Vang:
                    route = ClearanceConstants.Route_Type_Yellow;
                    break;
                case ClearanceConstants.Route_Type_Xanh:
                    route = ClearanceConstants.Route_Type_Green;
                    break;
                case ClearanceConstants.Route_Type_Do:
                    route = ClearanceConstants.Route_Type_Red;
                    break;
            }
            return route;
        }

        private string GetServiceType(string MA_HIEU_PTVC, out string cargoType)
        {
            cargoType = string.Empty;
            var serviceType = string.Empty;
            switch (MA_HIEU_PTVC)
            {
                case ClearanceConstants.Air_Service_Type:
                    serviceType = ClearanceConstants.Air_Service;
                    break;
                case ClearanceConstants.Sea_FCL_Service_Type:
                    cargoType = ClearanceConstants.CargoTypeFCL;
                    serviceType = ClearanceConstants.Sea_Service;
                    break;
                case ClearanceConstants.Sea_LCL_Service_Type:
                    cargoType = ClearanceConstants.CargoTypeLCL;
                    serviceType = ClearanceConstants.Sea_Service;
                    break;
                case ClearanceConstants.Trucking_Inland_Service_Type:
                    serviceType = ClearanceConstants.Trucking_Inland_Service;
                    break;
                case ClearanceConstants.Rail_Service_Type:
                    serviceType = ClearanceConstants.Rail_Service;
                    break;
                case ClearanceConstants.Warehouse_Service_Type6:
                    serviceType = ClearanceConstants.Warehouse_Service;
                    break;
                case ClearanceConstants.Warehouse_Service_Type9:
                    serviceType = ClearanceConstants.Warehouse_Service;
                    break;
            }
            return serviceType;
        }

        public List<CustomsDeclarationModel> Paging(CustomsDeclarationCriteria criteria, int page, int size, out int rowsCount)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.opsCustomClearance);
            var rangeSearch = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.List);
            if (rangeSearch == PermissionRange.None)
            {
                rowsCount = 0;
                return null;
            }
            Expression<Func<CustomsDeclaration, bool>> query = x => x.Source != OperationConstants.FROM_REPLICATE && (criteria.ClearanceNo.Contains(x.ClearanceNo) || string.IsNullOrEmpty(criteria.ClearanceNo))
                                                                                    && (x.UserCreated == criteria.PersonHandle || string.IsNullOrEmpty(criteria.PersonHandle))
                                                                                    && (x.Type == criteria.CusType || string.IsNullOrEmpty(criteria.CusType))
                                                                                    && (x.ClearanceDate >= criteria.FromClearanceDate || criteria.FromClearanceDate == null)
                                                                                    && (x.ClearanceDate <= criteria.ToClearanceDate || criteria.ToClearanceDate == null)
                                                                                    && ((x.DatetimeCreated >= criteria.FromImportDate && x.DatetimeCreated <= criteria.ToImportDate) || criteria.FromImportDate == null)
                                                                                    && ((x.AccountNo ?? x.PartnerTaxCode) == criteria.CustomerNo || string.IsNullOrEmpty(criteria.CustomerNo));

            if (criteria.ImPorted == true)
            {
                query = query.And(x => x.JobNo != null);
            }
            else if (criteria.ImPorted == false)
            {
                query = query.And(x => x.JobNo == null);
            }

            // Query with Permission Range.
            switch (rangeSearch)
            {
                case PermissionRange.Owner:
                    query = query.And(x => x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Group:
                    query = query.And(x => (x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                        || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Department:
                    query = query.And(x => (x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                        || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Office:
                    query = query.And(x => (x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID) || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Company:
                    query = query.And(x => x.CompanyId == currentUser.CompanyID || x.UserCreated == currentUser.UserID);
                    break;
                default:
                    break;
            }

            Expression<Func<CustomsDeclaration, object>> orderByProperty = x => x.DatetimeModified;

            var list = DataContext.Paging(query, page, size, orderByProperty, false, out rowsCount);
            var results = new List<CustomsDeclarationModel>();
            if (rowsCount == 0) return results;
            results = MapClearancesToClearanceModels(list);
            return results;
        }


        public IQueryable<CustomsDeclarationModel> GetCustomDeclaration(string keySearch, string customerNo, bool Imported, int pageNumber, int pageSize, out int rowsCount)
        {
            IQueryable<CustomsDeclarationModel> returnList = null;
            string[] clearanceNoArray = null;
            string autocompleteKey = string.Empty;
            if (keySearch != null)
            {
                keySearch = keySearch.ToLower().Trim();
                var replaceString = keySearch.Split(',');
                autocompleteKey = replaceString.Length > 0 ? replaceString[0] : string.Empty;
                if (replaceString.Length > 1)
                {
                    clearanceNoArray = replaceString.Length > 1 ? replaceString[1].Split('\n') : null;
                }
                else
                {
                    clearanceNoArray = replaceString.Length > 0 ? replaceString[0].Split('\n') : null;
                }
            }
            else
            {
                keySearch = String.Empty;
            }

            Func<CustomsDeclarationModel, bool> query = x => (x.AccountNo == customerNo)
                                && ((clearanceNoArray != null ? clearanceNoArray.Any(val => x.ClearanceNo.Contains(val.Trim(), StringComparison.OrdinalIgnoreCase))
                                                                : (x.ClearanceNo.Contains(autocompleteKey, StringComparison.OrdinalIgnoreCase)))
                                     || (x.Hblid != null && x.Hblid.Contains(autocompleteKey, StringComparison.OrdinalIgnoreCase))
                                     || (x.ExportCountryCode != null && x.ExportCountryCode.Contains(autocompleteKey, StringComparison.OrdinalIgnoreCase))
                                     || (x.ImportCountryCode != null && x.ImportCountryCode.Contains(autocompleteKey, StringComparison.OrdinalIgnoreCase))
                                     || (x.CommodityCode != null && x.CommodityCode.Contains(autocompleteKey, StringComparison.OrdinalIgnoreCase))
                                     || (x.FirstClearanceNo != null && x.FirstClearanceNo.Contains(autocompleteKey, StringComparison.OrdinalIgnoreCase))
                                     || (x.QtyCont.HasValue && x.QtyCont.ToString().Contains(autocompleteKey))
                                     || (x.Note != null && x.Note.Contains(autocompleteKey, StringComparison.OrdinalIgnoreCase))
                                     || string.IsNullOrEmpty(autocompleteKey)
                                );


            var data = Get().Where(query);
            if (Imported == true)
            {
                data = data.Where(x => x.JobNo != null);
            }
            else if (Imported == false)
            {
                data = data.Where(x => x.JobNo == null);
            }
            rowsCount = data.Count();
            if (rowsCount == 0) return returnList;
            else data = data.OrderByDescending(x => x.DatetimeModified);

            if (pageSize > 1)
            {
                if (pageNumber < 1)
                {
                    pageNumber = 1;
                }
                returnList = data.Skip((pageNumber - 1) * pageSize).Take(pageSize)?.AsQueryable();
            }
            return returnList;

        }

        private List<CustomsDeclarationModel> MapClearancesToClearanceModels(IQueryable<CustomsDeclaration> list)
        {
            List<CustomsDeclarationModel> results = new List<CustomsDeclarationModel>();
            var countryCache = countryApi.Getcountries().Result;
            var portCache = catPlaceApi.GetPlaces().Result;
            //var customerCache = catPartnerApi.GetPartners().Result;
            var countries = countryCache != null ? countryCache.ToList() : new List<Provider.Models.CatCountryApiModel>(); //dc.CatCountry;
            var portIndexs = portCache != null ? portCache.Where(x => x.PlaceTypeId == GetTypeFromData.GetPlaceType(CatPlaceTypeEnum.Port)).ToList() : new List<Provider.Models.CatPlaceApiModel>();
            // var customers = customerCache != null ? customerCache.Where(x => x.PartnerGroup.IndexOf(GetTypeFromData.GetPartnerGroup(CatPartnerGroupEnum.CUSTOMER), StringComparison.OrdinalIgnoreCase) > -1).ToList() : new List<Provider.Models.CatPartnerApiModel>();
            // var customers = customerRepository.Get(x => x.PartnerGroup.Contains("CUSTOMER"));
            foreach (CustomsDeclaration item in list)
            {
                var clearance = mapper.Map<CustomsDeclarationModel>(item);
                var imCountryCode = item.ImportCountryCode;
                var exCountryCode = item.ExportCountryCode;
                clearance.ImportCountryName = countries.FirstOrDefault(x => x.Code == imCountryCode)?.NameEn;
                clearance.ExportCountryName = countries.FirstOrDefault(x => x.Code == exCountryCode)?.NameEn;
                if (item.AccountNo == null)
                {
                    var customer = customerRepository.Get(x => x.TaxCode == item.PartnerTaxCode)?.FirstOrDefault();
                    clearance.CustomerName = customer?.ShortName;
                    clearance.CustomerId = customer.Id;
                }
                else
                {
                    var customer = customerRepository.Get(x => x.AccountNo == item.AccountNo)?.FirstOrDefault();
                    clearance.CustomerName = customer?.ShortName;
                    clearance.CustomerId = customer?.Id;

                }
                clearance.GatewayName = portIndexs.FirstOrDefault(x => x.Code == item.Gateway)?.NameEn;
                clearance.UserCreatedName = userRepository.Get(x => x.Id == item.UserCreated).FirstOrDefault()?.Username;
                clearance.UserModifieddName = userRepository.Get(x => x.Id == item.UserModified).FirstOrDefault()?.Username;

                results.Add(clearance);
            }
            return results;
        }

        public List<CustomsDeclarationModel> GetBy(string jobNo)
        {
            List<CustomsDeclarationModel> results = null;
            var data = GetCustomClearanceViewList(jobNo);
            if (data.Count == 0) return results;
            results = mapper.Map<List<CustomsDeclarationModel>>(data);
            return results;
        }

        public object GetClearanceTypeData()
        {
            var types = Common.CustomData.Types;
            var cargoTypes = Common.CustomData.CargoTypes;
            var routes = Common.CustomData.Routes;
            var serviceTypes = Common.CustomData.ServiceTypes;
            var results = new { types, cargoTypes, routes, serviceTypes };
            return results;
        }

        public async Task<HandleState> UpdateJobToClearances(List<CustomsDeclarationModel> clearances)
        {
            var result = new HandleState();
            try
            {
                var clearanceAdd = clearances.OrderBy(x => x.ClearanceDate).ThenByDescending(x => x.DatetimeModified);
                foreach (var item in clearanceAdd)
                {
                    var clearance = DataContext.Get(x => x.Id == item.Id).FirstOrDefault();
                    if (clearance != null)
                    {
                        clearance.JobNo = item.JobNo;
                        clearance.ConvertTime = item.ConvertTime;
                        clearance.DatetimeModified = DateTime.Now;
                        clearance.UserModified = currentUser.UserID;
                    }
                    result = DataContext.Update(clearance, x => x.Id == item.Id, false);
                }

                var jobNo = clearanceAdd.FirstOrDefault()?.JobNo;
                if (result.Success && jobNo != null)
                {
                    var query = from advRequest in accAdvanceRequestRepository.Get(x => x.JobId == jobNo)
                                join advPayment in accAdvancePaymentRepository.Get()
                                on advRequest.AdvanceNo equals advPayment.AdvanceNo
                                where advPayment.SyncStatus == null
                                select advRequest;

                    await query.ForEachAsync(x => x.CustomNo = clearanceAdd.FirstOrDefault()?.ClearanceNo);
                    result = accAdvanceRequestRepository.SubmitChanges();
                }
                result = DataContext.SubmitChanges();
            }
            catch (Exception ex)
            {
                result = new HandleState(ex.Message);
            }
            return result;
        }

        public CustomsDeclaration GetById(int id)
        {
            var detail = DataContext.Get(x => x.Id == id).FirstOrDefault();
            return detail;

        }
        public IQueryable<CustomsDeclarationModel> Query(CustomsDeclarationCriteria criteria)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.opsCustomClearance);
            var rangeSearch = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.List);
            if (rangeSearch == PermissionRange.None)
            {
                return null;
            }

            Expression<Func<CustomsDeclarationModel, bool>> query = x => (x.ClearanceNo.IndexOf(criteria.ClearanceNo ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                                                                    && (x.UserCreated == criteria.PersonHandle || string.IsNullOrEmpty(criteria.PersonHandle))
                                                                                    && (x.Type == criteria.CusType || string.IsNullOrEmpty(criteria.CusType))
                                                                                    && (x.ClearanceDate >= criteria.FromClearanceDate || criteria.FromClearanceDate == null)
                                                                                    && (x.ClearanceDate <= criteria.ToClearanceDate || criteria.ToClearanceDate == null)
                                                                                    && ((x.DatetimeCreated >= criteria.FromImportDate && x.DatetimeCreated <= criteria.ToImportDate) || criteria.FromImportDate == null);

            if (criteria.ImPorted == true)
            {
                query = query.And(x => x.JobNo != null);
            }
            else if (criteria.ImPorted == false)
            {
                query = query.And(x => x.JobNo == null);
            }

            // Query with Permission Range.
            switch (rangeSearch)
            {
                case PermissionRange.Owner:
                    query = query.And(x => x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Group:
                    query = query.And(x => (x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                        || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Department:
                    query = query.And(x => (x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                        || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Office:
                    query = query.And(x => (x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID) || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Company:
                    query = query.And(x => x.CompanyId == currentUser.CompanyID || x.UserCreated == currentUser.UserID);
                    break;
                default:
                    break;
            }
            var results = Get().Where(query).AsEnumerable();
            results = MapClearancesToClearanceModels(results.AsQueryable());
            return results?.AsQueryable();
        }

        private List<sp_GetCustomDeclaration> GetCustomClearanceViewList(string jobNo)
        {
            DbParameter[] parameters =
            {
                SqlParam.GetParameter("jobNo", jobNo)
            };
            return ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetCustomDeclaration>(parameters);
        }

        public HandleState CheckAllowDelete(List<CustomsDeclarationModel> customs)
        {
            var result = new HandleState();
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.opsCustomClearance);
            var permissionRangeDelete = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            switch (permissionRangeDelete)
            {
                case PermissionRange.None:
                    result = new HandleState(403, "You don't have permission to delete!");
                    break;
                case PermissionRange.Owner:
                    if (customs.Any(x => x.UserCreated != currentUser.UserID))
                    {
                        var ItemForbidDelete = customs.Where(i => i.UserCreated != currentUser.UserID).Select(x => x.ClearanceNo);
                        result = new HandleState(false, "Items: " + String.Join(", ", ItemForbidDelete.Select(s => s).Distinct()) + " . You don't have permission to delete");
                    }
                    break;
                case PermissionRange.Group:
                    if (customs.Any(x => x.GroupId != currentUser.GroupId
                                     && x.DepartmentId != currentUser.DepartmentId
                                     && x.OfficeId != currentUser.OfficeID
                                     && x.CompanyId != currentUser.CompanyID)
                       )
                    {
                        var ItemForbidDelete = customs.Where(x => x.GroupId != currentUser.GroupId
                                     && x.DepartmentId != currentUser.DepartmentId
                                     && x.OfficeId != currentUser.OfficeID
                                     && x.CompanyId != currentUser.CompanyID);
                        result = new HandleState(false, "Items: " + String.Join(", ", ItemForbidDelete.Select(s => s.Id).Distinct()) + " . You don't have permission to delete!");
                    }
                    break;
                case PermissionRange.Department:
                    if (customs.Any(x => x.DepartmentId != currentUser.DepartmentId
                                                         && x.OfficeId != currentUser.OfficeID
                                                         && x.CompanyId != currentUser.CompanyID)
                                           )
                    {
                        var ItemForbidDelete = customs.Where(x => x.DepartmentId != currentUser.DepartmentId
                                     && x.OfficeId != currentUser.OfficeID
                                     && x.CompanyId != currentUser.CompanyID);
                        result = new HandleState(false, "Items: " + String.Join(", ", ItemForbidDelete.Select(s => s.Id).Distinct()) + " . You don't have permission to delete!");
                    }
                    break;
                case PermissionRange.Office:
                    if (customs.Any(x => x.OfficeId != currentUser.OfficeID && x.CompanyId != currentUser.CompanyID))
                    {
                        var ItemForbidDelete = customs.Where(x => x.OfficeId != currentUser.OfficeID && x.CompanyId != currentUser.CompanyID);
                        result = new HandleState(false, "Items: " + String.Join(", ", ItemForbidDelete.Select(s => s.Id).Distinct()) + " . You don't have permission to delete!");
                    }
                    break;
                case PermissionRange.Company:
                    if (customs.Any(x => x.CompanyId != currentUser.CompanyID))
                    {
                        var ItemForbidDelete = customs.Where(x => x.CompanyId != currentUser.CompanyID);
                        result = new HandleState(new { Message = "Items: " + String.Join(", ", ItemForbidDelete.Select(s => s.Id).Distinct()) + " . You don't have permission to delete" });
                    }
                    break;
            }

            return result;
        }
        public HandleState DeleteMultiple(List<CustomsDeclarationModel> customs)
        {
            var result = new HandleState();
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.opsCustomClearance);
            var permissionRangeDelete = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);

            switch (permissionRangeDelete)
            {
                case PermissionRange.None:
                    result = new HandleState(403, "");
                    break;
                case PermissionRange.Owner:
                    if (customs.Any(x => x.UserCreated != currentUser.UserID))
                    {
                        var ItemForbidDelete = customs.Where(i => i.UserCreated != currentUser.UserID);
                        return new HandleState(false, "Items: " + String.Join(", ", ItemForbidDelete.Select(s => s.Id).Distinct()) + " Invalid");
                    }
                    result = deleteMultipleModel(customs);
                    break;
                case PermissionRange.Group:
                    if (customs.Any(x => x.GroupId != currentUser.GroupId
                                     && x.DepartmentId != currentUser.DepartmentId
                                     && x.OfficeId != currentUser.OfficeID
                                     && x.CompanyId != currentUser.CompanyID)
                       )
                    {
                        var ItemForbidDelete = customs.Where(x => x.GroupId != currentUser.GroupId
                                     && x.DepartmentId != currentUser.DepartmentId
                                     && x.OfficeId != currentUser.OfficeID
                                     && x.CompanyId != currentUser.CompanyID);
                        return new HandleState(false, "Items: " + String.Join(", ", ItemForbidDelete.Select(s => s.Id).Distinct()) + " Invalid");
                    }
                    result = deleteMultipleModel(customs);
                    break;
                case PermissionRange.Department:
                    if (customs.Any(x => x.DepartmentId != currentUser.DepartmentId
                                                         && x.OfficeId != currentUser.OfficeID
                                                         && x.CompanyId != currentUser.CompanyID)
                                           )
                    {
                        var ItemForbidDelete = customs.Where(x => x.DepartmentId != currentUser.DepartmentId
                                     && x.OfficeId != currentUser.OfficeID
                                     && x.CompanyId != currentUser.CompanyID);
                        return new HandleState(false, "Items: " + String.Join(", ", ItemForbidDelete.Select(s => s.Id).Distinct()) + " Invalid");
                    }
                    result = deleteMultipleModel(customs);
                    break;
                case PermissionRange.Office:
                    if (customs.Any(x => x.OfficeId != currentUser.OfficeID && x.CompanyId != currentUser.CompanyID))
                    {
                        var ItemForbidDelete = customs.Where(x => x.OfficeId != currentUser.OfficeID && x.CompanyId != currentUser.CompanyID);
                        return new HandleState(false, "Items: " + String.Join(", ", ItemForbidDelete.Select(s => s.Id).Distinct()) + " Invalid");
                    }
                    result = deleteMultipleModel(customs);
                    break;
                case PermissionRange.Company:
                    if (customs.Any(x => x.CompanyId != currentUser.CompanyID))
                    {
                        var ItemForbidDelete = customs.Where(x => x.CompanyId != currentUser.CompanyID);
                        return new HandleState(new { Message = "Items: " + String.Join(", ", ItemForbidDelete.Select(s => s.Id).Distinct()) + " Invalid" });
                    }
                    result = deleteMultipleModel(customs);
                    break;
                case PermissionRange.All:
                    result = deleteMultipleModel(customs);
                    break;
                default:
                    return new HandleState(403, "");
            }

            return result;

        }

        public List<CustomClearanceImportModel> CheckValidImport(List<CustomClearanceImportModel> list)
        {
            DateTime dateTimeDefault;
            decimal decimalDefault;
            int intDefault;
            bool isDecimal = false;
            bool isInt = false;
            bool isDate = false;

            //eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            list.ForEach(item =>
            {
                //Check empty ClearanceNo
                string _clearanceNo = item.ClearanceNo;
                item.ClearanceNoValid = true;
                if (string.IsNullOrEmpty(_clearanceNo))
                {
                    item.ClearanceNo = stringLocalizer[OperationLanguageSub.MSG_CUSTOM_CLEARANCE_NO_EMPTY];
                    item.IsValid = false;
                    item.ClearanceNoValid = false;
                }
                else
                {
                    //Check valid maxlength for Clearance No
                    if (_clearanceNo.Length > 100)
                    {
                        item.ClearanceNo = string.Format(stringLocalizer[OperationLanguageSub.MSG_INVALID_MAX_LENGTH], 50);
                        item.IsValid = false;
                        item.ClearanceNoValid = false;
                    }

                    //Check input character special, number
                    Regex pattern = new Regex(@"^[a-zA-Z0-9 ./_-]*$");
                    if (!pattern.IsMatch(_clearanceNo))
                    {
                        item.ClearanceNo = stringLocalizer[OperationLanguageSub.MSG_CUSTOM_INVALID_CHARACTER_SPECIAL];
                        item.IsValid = false;
                        item.ClearanceNoValid = false;
                    }
                }

                //Check empty & exist data for Type
                string _type = item.Type;
                item.TypeValid = true;
                if (!string.IsNullOrEmpty(_type))
                {
                    var isFound = Common.CustomData.Types.Any(x => x.Value == _type);
                    if (!isFound)
                    {
                        item.Type = stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND];
                        item.IsValid = false;
                        item.TypeValid = false;
                    }
                    else
                    {
                        item.Type = Common.CustomData.Types.Where(x => x.Value == _type).First().Value;
                    }
                }

                //Check empty & valid format date for ClearanceDate
                string _clearanceDate = item.ClearanceDateStr;
                item.ClearanceDateValid = true;
                if (string.IsNullOrEmpty(_clearanceDate))
                {
                    item.ClearanceDateStr = stringLocalizer[OperationLanguageSub.MSG_CUSTOM_CLEARANCE_DATE_EMPTY];
                    item.IsValid = false;
                    item.ClearanceDateValid = false;
                }
                else
                {
                    isDate = DateTime.TryParse(_clearanceDate, out dateTimeDefault);
                    if (!isDate)
                    {
                        item.ClearanceDateStr = stringLocalizer[OperationLanguageSub.MSG_INVALID_DATE];
                        item.IsValid = false;
                        item.ClearanceDateValid = false;
                    }
                    else
                    {
                        item.ClearanceDate = dateTimeDefault;
                        item.ClearanceDateStr = dateTimeDefault.ToString("dd/MM/yyyy");
                    }
                }

                //Check empty & exist data for PartnerTaxCode
                string _partnerTaxCode = item.AccountNo;
                item.PartnerTaxCodeValid = true;
                if (string.IsNullOrEmpty(_partnerTaxCode))
                {
                    item.CustomerName = stringLocalizer[OperationLanguageSub.MSG_CUSTOM_CLEARANCE_CUSTOMER_CODE_EMPTY];
                    item.IsValid = false;
                    item.PartnerTaxCodeValid = false;
                }
                else
                {
                    var isFound = customerRepository.Any(x => x.AccountNo == _partnerTaxCode);
                    if (!isFound)
                    {
                        item.CustomerName = stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND];
                        item.IsValid = false;
                        item.PartnerTaxCodeValid = false;
                    }
                    else
                    {
                        var customer = customerRepository.Where(x => x.AccountNo == _partnerTaxCode).First();
                        item.CustomerName = customer.PartnerNameEn;
                        item.PartnerTaxCode = customer.TaxCode;
                    }
                }

                //Check empty for MBL
                string _mbl = item.Mblid;
                item.MblidValid = true;
                if (string.IsNullOrEmpty(_mbl))
                {
                    item.Mblid = stringLocalizer[OperationLanguageSub.MSG_CUSTOM_CLEARANCE_MBL_EMPTY];
                    item.IsValid = false;
                    item.MblidValid = false;
                }
                else
                {
                    //Check valid maxlength for MblId
                    if (_mbl.Length > 250)
                    {
                        item.Mblid = string.Format(stringLocalizer[OperationLanguageSub.MSG_INVALID_MAX_LENGTH], 50);
                        item.IsValid = false;
                        item.MblidValid = false;
                    }

                    //Check input character special, number
                    Regex pattern = new Regex(@"^[a-zA-Z0-9 ./_-]*$");
                    if (!pattern.IsMatch(_mbl))
                    {
                        item.Mblid = stringLocalizer[OperationLanguageSub.MSG_CUSTOM_INVALID_CHARACTER_SPECIAL];
                        item.IsValid = false;
                        item.MblidValid = false;
                    }
                }

                //Check empty for HBL
                string _hbl = item.Hblid;
                item.HblidValid = true;
                if (string.IsNullOrEmpty(_hbl))
                {
                    item.Hblid = stringLocalizer[OperationLanguageSub.MSG_CUSTOM_CLEARANCE_HBL_EMPTY];
                    item.IsValid = false;
                    item.HblidValid = false;
                }
                else
                {
                    //Check valid maxlength for HblId
                    if (_hbl.Length > 250)
                    {
                        item.Hblid = string.Format(stringLocalizer[OperationLanguageSub.MSG_INVALID_MAX_LENGTH], 50);
                        item.IsValid = false;
                        item.HblidValid = false;
                    }

                    //Check input character special, number
                    Regex pattern = new Regex(@"^[a-zA-Z0-9 ./_-]*$");
                    if (!pattern.IsMatch(_hbl))
                    {
                        item.Hblid = stringLocalizer[OperationLanguageSub.MSG_CUSTOM_INVALID_CHARACTER_SPECIAL];
                        item.IsValid = false;
                        item.HblidValid = false;
                    }
                }

                //Check empty & exist data for Gateway
                string _gateway = item.Gateway;
                item.GatewayValid = true;
                if (string.IsNullOrEmpty(_gateway))
                {
                    item.GatewayName = stringLocalizer[OperationLanguageSub.MSG_CUSTOM_CLEARANCE_GATEWAY_EMPTY];
                    item.IsValid = false;
                    item.GatewayValid = false;
                }
                else
                {
                    var gatewayList = catPlaceApi.GetPlaces().Result.Where(x => x.PlaceTypeId == GetTypeFromData.GetPlaceType(CatPlaceTypeEnum.Port));
                    var isFound = gatewayList.Any(x => x.Code == _gateway);
                    if (!isFound)
                    {
                        item.GatewayName = stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND];
                        item.IsValid = false;
                        item.GatewayValid = false;
                    }
                    else
                    {
                        item.GatewayName = gatewayList.Where(x => x.Code == _gateway).First().NameEn;
                    }
                }

                //Check valid format for GrossWeight
                string _grossWeight = item.GrossWeightStr;
                item.GrossWeightValid = true;
                if (!string.IsNullOrEmpty(_grossWeight))
                {
                    isDecimal = decimal.TryParse(_grossWeight, out decimalDefault);
                    if (!isDecimal || _grossWeight.IndexOf(",") > -1)
                    {
                        item.GrossWeightStr = stringLocalizer[OperationLanguageSub.MSG_INVALID_NUMBER];
                        item.IsValid = false;
                        item.GrossWeightValid = false;
                    }
                    else
                    {
                        var valueConvert = Convert.ToDecimal(_grossWeight);
                        if (valueConvert < 0)
                        {
                            item.GrossWeightStr = stringLocalizer[OperationLanguageSub.MSG_INVALID_NEGATIVE];
                            item.IsValid = false;
                            item.GrossWeightValid = false;
                        }
                        else
                        {
                            item.GrossWeight = NumberHelper.RoundNumber(valueConvert, 2);
                            item.GrossWeightStr = item.GrossWeight.ToString();
                        }
                    }
                }

                //Check valid format number for NetWeight
                string _netWeight = item.NetWeightStr;
                item.NetWeightValid = true;
                if (!string.IsNullOrEmpty(_netWeight))
                {
                    isDecimal = decimal.TryParse(_netWeight, out decimalDefault);
                    if (!isDecimal || _netWeight.IndexOf(",") > -1)
                    {
                        item.NetWeightStr = stringLocalizer[OperationLanguageSub.MSG_INVALID_NUMBER];
                        item.IsValid = false;
                        item.NetWeightValid = false;
                    }
                    else
                    {
                        var valueConvert = Convert.ToDecimal(_netWeight);
                        if (valueConvert < 0)
                        {
                            item.NetWeightStr = stringLocalizer[OperationLanguageSub.MSG_INVALID_NEGATIVE];
                            item.IsValid = false;
                            item.NetWeightValid = false;
                        }
                        else
                        {
                            item.NetWeight = NumberHelper.RoundNumber(valueConvert, 2);
                            item.NetWeightStr = item.NetWeight.ToString();
                        }
                    }
                }

                //Check valid format number for CBM
                string _cbm = item.CbmStr;
                item.CbmValid = true;
                if (!string.IsNullOrEmpty(_cbm))
                {
                    isDecimal = decimal.TryParse(_cbm, out decimalDefault);
                    if (!isDecimal || _cbm.IndexOf(",") > -1)
                    {
                        item.CbmStr = stringLocalizer[OperationLanguageSub.MSG_INVALID_NUMBER];
                        item.IsValid = false;
                        item.CbmValid = false;
                    }
                    else
                    {
                        var valueConvert = Convert.ToDecimal(_cbm);
                        if (valueConvert < 0)
                        {
                            item.CbmStr = stringLocalizer[OperationLanguageSub.MSG_INVALID_NEGATIVE];
                            item.IsValid = false;
                            item.CbmValid = false;
                        }
                        else
                        {
                            item.Cbm = NumberHelper.RoundNumber(valueConvert, 2);
                            item.CbmStr = item.Cbm.ToString();
                        }
                    }
                }

                //Check valid format number for QtyCont
                string _qtyCont = item.QtyContStr;
                item.QtyContValid = true;
                if (!string.IsNullOrEmpty(_qtyCont))
                {
                    isInt = int.TryParse(_qtyCont, out intDefault);
                    if (!isInt)
                    {
                        item.QtyContStr = stringLocalizer[OperationLanguageSub.MSG_INVALID_NUMBER];
                        item.IsValid = false;
                        item.QtyContValid = false;
                    }
                    else
                    {
                        var valueConvert = Convert.ToInt32(_qtyCont);
                        if (valueConvert < 0)
                        {
                            item.QtyContStr = stringLocalizer[OperationLanguageSub.MSG_INVALID_NEGATIVE];
                            item.IsValid = false;
                            item.QtyContValid = false;
                        }
                        else
                        {
                            item.QtyCont = valueConvert;
                            item.QtyContStr = _qtyCont;
                        }
                    }
                }

                //Check valid format number for PCS
                string _pcs = item.PcsStr;
                item.PcsValid = true;
                if (!string.IsNullOrEmpty(_pcs))
                {
                    isInt = int.TryParse(_pcs, out intDefault);
                    if (!isInt)
                    {
                        item.PcsStr = stringLocalizer[OperationLanguageSub.MSG_INVALID_NUMBER];
                        item.IsValid = false;
                        item.PcsValid = false;
                    }
                    else
                    {
                        var valueConvert = Convert.ToInt32(_pcs);
                        if (valueConvert < 0)
                        {
                            item.PcsStr = stringLocalizer[OperationLanguageSub.MSG_INVALID_NEGATIVE];
                            item.IsValid = false;
                            item.PcsValid = false;
                        }
                        else
                        {
                            item.Pcs = valueConvert;
                            item.PcsStr = _pcs;
                        }
                    }
                }

                //Check exist data for CommodityCode
                string _commodity = item.CommodityCode;
                item.CommodityValid = true;
                if (!string.IsNullOrEmpty(_commodity))
                {
                    var isFound = commodityRepository.Any(x => x.Code == _commodity);
                    if (!isFound)
                    {
                        item.CommodityName = stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND];
                        item.IsValid = false;
                        item.CommodityValid = false;
                    }
                    else
                    {
                        item.CommodityName = commodityRepository.Get(x => x.Code == _commodity)?.First().CommodityNameEn;
                    }
                }

                //Check empty & exist data for ServiceType
                string _serviceType = item.ServiceType;
                item.ServiceTypeValid = true;
                if (string.IsNullOrEmpty(_serviceType))
                {
                    item.ServiceType = stringLocalizer[OperationLanguageSub.MSG_CUSTOM_CLEARANCE_SERVICE_TYPE_EMPTY];
                    item.IsValid = false;
                    item.ServiceTypeValid = false;
                }
                else
                {
                    var isFound = Common.CustomData.ServiceTypes.Any(x => x.Value == _serviceType);
                    if (!isFound)
                    {
                        item.ServiceType = stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND];
                        item.IsValid = false;
                        item.ServiceTypeValid = false;
                    }
                    else
                    {
                        item.ServiceType = Common.CustomData.ServiceTypes.Where(x => x.Value == _serviceType).First().Value;
                    }
                }

                //Check empty & exist data for CargoType
                string _cargoType = item.CargoType;
                item.CargoTypeValid = true;
                if (!item.ServiceType.Equals("Air") && !item.ServiceType.Equals("Express") && string.IsNullOrEmpty(_cargoType))
                {
                    item.CargoType = stringLocalizer[OperationLanguageSub.MSG_CUSTOM_CLEARANCE_CARGO_TYPE_EMPTY];
                    item.IsValid = false;
                    item.CargoTypeValid = false;
                }
                else if (item.ServiceType.Equals("Air") || item.ServiceType.Equals("Express"))
                {
                    item.CargoType = null;
                }
                else
                {
                    var isFound = Common.CustomData.CargoTypes.Any(x => x.Value == _cargoType);
                    if (!isFound)
                    {
                        item.CargoType = stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND];
                        item.IsValid = false;
                        item.CargoTypeValid = false;
                    }
                    else
                    {
                        item.CargoType = Common.CustomData.CargoTypes.Where(x => x.Value == _cargoType).First().Value;
                    }
                }

                //Check empty & exist data for Route
                string _route = item.Route;
                item.RouteValid = true;
                if (string.IsNullOrEmpty(_route))
                {
                    item.Route = stringLocalizer[OperationLanguageSub.MSG_CUSTOM_CLEARANCE_ROUTE_EMPTY];
                    item.IsValid = false;
                    item.RouteValid = false;
                }
                else
                {
                    var isFound = Common.CustomData.Routes.Any(x => x.Value == _route);
                    if (!isFound)
                    {
                        item.Route = stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND];
                        item.IsValid = false;
                        item.RouteValid = false;
                    }
                    else
                    {
                        item.Route = Common.CustomData.Routes.Where(x => x.Value == _route).First().Value;
                    }
                }

                //Check exist data for ImportCoutryCode
                var countryList = countryApi.Getcountries().Result;
                string _importCountryCode = item.ImportCountryCode;
                item.ImportCountryCodeValid = true;
                if (!string.IsNullOrEmpty(_importCountryCode))
                {
                    var isFound = countryList.Any(x => x.Code == _importCountryCode);
                    if (!isFound)
                    {
                        item.ImportCountryName = stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND];
                        item.IsValid = false;
                        item.ImportCountryCodeValid = false;
                    }
                    else
                    {
                        item.ImportCountryName = countryList.Where(x => x.Code == _importCountryCode).First().NameEn;
                    }
                }

                //Check exist data for ExportCoutryCode
                string _exportCountryCode = item.ExportCountryCode;
                item.ExportCountryCodeValid = true;
                if (!string.IsNullOrEmpty(_exportCountryCode))
                {
                    var isFound = countryList.Any(x => x.Code == _exportCountryCode);
                    if (!isFound)
                    {
                        item.ExportCountryName = stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND];
                        item.IsValid = false;
                        item.ExportCountryCodeValid = false;
                    }
                    else
                    {
                        item.ExportCountryName = countryList.Where(x => x.Code == _exportCountryCode).First().NameEn;
                    }
                }

                //Check double ClearanceNo & ClearanceDate
                if (list.Where(x => !string.IsNullOrEmpty(item.ClearanceNo) && item.ClearanceDate != null && x.ClearanceNo == item.ClearanceNo && x.ClearanceDate == item.ClearanceDate).Count() > 1)
                {
                    item.ClearanceNo = string.Format(stringLocalizer[OperationLanguageSub.MSG_DUPLICATE_DATA].Value, item.ClearanceNo);
                    item.ClearanceDateStr = string.Format(stringLocalizer[OperationLanguageSub.MSG_DUPLICATE_DATA].Value, item.ClearanceDateStr);
                    item.IsValid = item.ClearanceNoValid = item.ClearanceDateValid = false;
                }

                //Check exist ClearanceNo & ClearanceDate
                if (DataContext.Any(x => x.ClearanceNo == item.ClearanceNo && x.ClearanceDate == item.ClearanceDate))
                {
                    item.ClearanceNo = item.ClearanceDateStr = string.Format(stringLocalizer[OperationLanguageSub.MSG_CLEARANCENO_EXISTED].Value, item.ClearanceNo);
                    item.IsValid = false;
                    item.ClearanceNoValid = item.ClearanceDateValid = false;
                }

            });

            return list;
        }

        public HandleState Import(List<CustomsDeclarationModel> data)
        {
            try
            {
                foreach (var item in data)
                {
                    item.DatetimeCreated = DateTime.Now;
                    item.DatetimeModified = DateTime.Now;
                    item.UserCreated = item.UserModified = currentUser.UserID;
                    item.Source = OperationConstants.FromEFMS;
                    item.GroupId = currentUser.GroupId;
                    item.DepartmentId = currentUser.DepartmentId;
                    item.OfficeId = currentUser.OfficeID;
                    item.CompanyId = currentUser.CompanyID;
                }

                var hs = Add(data);
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public List<CustomsDeclarationModel> GetCustomsShipmentNotLocked()
        {
            //Get list custom có shipment operation chưa bị lock & list shipment đã được assign cho current user
            var userCurrent = currentUser.UserID;
            var customs = DataContext.Get();
            var shipmentsOperation = from ops in opsTransactionRepo.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != "Canceled" && x.IsLocked == false)
                                     join osa in opsStageAssignedRepo.Get() on ops.Id equals osa.JobId //into osa2
                                     //from osa in osa2.DefaultIfEmpty()
                                     where osa.MainPersonInCharge == userCurrent
                                     select ops;

            //Join theo số HBL
            var query = from cus in customs
                        join ope in shipmentsOperation on cus.Hblid equals ope.Hwbno into ope2
                        from ope in ope2
                        select cus;

            var data = mapper.Map<List<CustomsDeclarationModel>>(query);
            return data;
        }

        public int CheckDetailPermission(int id)
        {
            int code = 0;
            var detail = GetById(id);
            var permissionRange = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Detail);
            var model = new BaseUpdateModel { UserCreated = detail.UserCreated, GroupId = detail.GroupId, DepartmentId = detail.DepartmentId, OfficeId = detail.OfficeId, CompanyId = detail.CompanyId };
            code = PermissionExtention.GetPermissionCommonItem(model, permissionRange, currentUser);
            return code;
        }

        private bool GetPermissionDetail(PermissionRange permissionRange, CustomsDeclarationModel detail)
        {
            bool result = false;

            switch (permissionRange)
            {
                case PermissionRange.All:
                    result = true;
                    break;
                case PermissionRange.Owner:
                    if (detail.UserCreated == currentUser.UserID)
                    {
                        result = true;
                    }
                    break;
                case PermissionRange.Group:
                    if (detail.GroupId == currentUser.GroupId
                        && detail.DepartmentId == currentUser.DepartmentId
                        && detail.OfficeId == currentUser.OfficeID
                        && detail.CompanyId == currentUser.CompanyID
                        )
                    {
                        result = true;
                    }
                    break;
                case PermissionRange.Department:
                    if (detail.DepartmentId == currentUser.DepartmentId
                        && detail.OfficeId == currentUser.OfficeID
                        && detail.CompanyId == currentUser.CompanyID)
                    {
                        result = true;
                    }
                    break;
                case PermissionRange.Office:
                    if (detail.OfficeId == currentUser.OfficeID
                        && detail.CompanyId == currentUser.CompanyID)
                    {
                        result = true;
                    }
                    break;
                case PermissionRange.Company:
                    if (detail.CompanyId == currentUser.CompanyID)
                    {
                        result = true;
                    }
                    break;
            }
            return result;
        }

        public CustomsDeclarationModel GetDetail(int id)
        {
            CustomsDeclaration clearance = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if (clearance == null) return null;
            if (clearance.AccountNo == null)
            {
                clearance.AccountNo = clearance.PartnerTaxCode;
            }
            var result = mapper.Map<CustomsDeclarationModel>(clearance);
            var customer = customerRepository.Get(x => x.AccountNo == result.AccountNo)?.FirstOrDefault();

            result.CustomerName = customer?.ShortName;
            result.CustomerId = customer?.Id;

            result.UserCreatedName = userRepository.Get(x => x.Id == result.UserCreated).FirstOrDefault()?.Username;
            result.UserModifieddName = userRepository.Get(x => x.Id == result.UserModified).FirstOrDefault()?.Username;
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.opsCustomClearance);
            var permissionRangeWrite = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);

            result.Permission = new PermissionAllowBase
            {
                AllowUpdate = GetPermissionDetail(permissionRangeWrite, result),
            };
            return result;
        }

        private HandleState deleteMultipleModel(List<CustomsDeclarationModel> customs)
        {
            var result = new HandleState();

            try
            {
                foreach (var item in customs)
                {
                    var hs = Delete(x => x.Id == item.Id, false);

                    var hasReplicate = DataContext.Get(x => x.ClearanceNo == item.ClearanceNo && x.Id != item.Id
                    && x.Source == OperationConstants.FROM_REPLICATE
                    && x.ClearanceDate == item.ClearanceDate)?.FirstOrDefault(); // do đang check trùng clearance theo ngày
                    if (hasReplicate != null)
                    {
                        Delete(x => x.Id == hasReplicate.Id, false);
                    }
                }
                DataContext.SubmitChanges();
            }
            catch (Exception ex)
            {
                result = new HandleState(ex.Message);
            }
            return result;
        }

        public List<CustomsDeclarationModel> GetListCustomNoAsignPIC()
        {
            //Get list custom có shipment operation chưa bị lock, list shipment đã được assign cho current user hoặc shipment có PIC là current user
            var userCurrent = currentUser.UserID;
            var customs = DataContext.Get(x => !string.IsNullOrEmpty(x.JobNo));
            var shipments = opsTransactionRepo.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != "Canceled" && x.IsLocked == false && x.OfficeId == currentUser.OfficeID);  // Lấy theo office current user
            var shipmentsOperation = from ops in shipments
                                     join osa in opsStageAssignedRepo.Get() on ops.Id equals osa.JobId
                                     where osa.MainPersonInCharge == userCurrent
                                     select ops;
            var shipmentPIC = shipments.Where(x => x.BillingOpsId == userCurrent);

            var shipmentMerge = shipmentsOperation.Union(shipmentPIC);

            //Join theo số JobNo
            var query = from cus in customs
                        join ope in shipmentMerge on cus.JobNo equals ope.JobNo
                        select new { cus, ope };
            IQueryable<CustomsDeclarationModel> d = null;
            if (query != null)
            {
                d = query.ToList().Select(x =>
                {
                    x.cus.Hblid = x.ope.Hblid.ToString();
                    return mapper.Map<CustomsDeclarationModel>(x.cus);
                }).AsQueryable();

            }

            if (d != null && d.Count() > 0)
            {
                var data = d.ToArray().OrderBy(o => o.ClearanceDate).ToList();
                return data;
            }

            return new List<CustomsDeclarationModel>();
        }

        public bool CheckAllowUpdate(Guid? jobId, List<string> clearanceNos)
        {
            var detail = opsTransactionRepo.Get(x => x.Id == jobId && x.CurrentStatus != "Canceled")?.FirstOrDefault();
            var query = csShipmentSurchargeRepo.Get(x => x.Hblid == detail.Hblid &&
            (!string.IsNullOrEmpty(x.CreditNo)
                          || !string.IsNullOrEmpty(x.DebitNo)
                          || !string.IsNullOrEmpty(x.Soano)
                          || !string.IsNullOrEmpty(x.PaymentRefNo)
                          || !string.IsNullOrEmpty(x.AdvanceNo)
                          || !string.IsNullOrEmpty(x.VoucherId)

                          || !string.IsNullOrEmpty(x.PaySoano)
                          || !string.IsNullOrEmpty(x.SettlementCode)
                          || !string.IsNullOrEmpty(x.SyncedFrom))).Select(x => x.ClearanceNo);
            var cleanceNoRequet = accAdvanceRequestRepository.Get(y => y.JobId == detail.JobNo && !string.IsNullOrEmpty(y.CustomNo)).Select(x => x.CustomNo).ToList();
            if (clearanceNos.Any(x => query.Any(y => y == x) || cleanceNoRequet.Any(z => z == x)))
            {
                return false;
            }

            return true;
        }

        public HandleState ImportClearancesOlaFromEcus()
        {
            string userId = currentUser.UserID;
            var connections = ecusCconnectionService.Get(x => x.UserId == userId && x.Active == true);
            var result = new HandleState();
            var lists = new List<CustomsDeclaration>();
            try
            {
                foreach (var item in connections)
                {
                    var clearanceEcus = ecusCconnectionService.GetDataOlaEcusByUser(item.UserId, item.ServerName, item.Dbusername, item.Dbpassword, item.Dbname);
                    if (clearanceEcus == null)
                    {
                        continue;
                    }
                    else
                    {
                        var clearances = DataContext.Get();
                        var cleancesNotExsitInFMS = clearanceEcus.Where(x => !checkExistEcusInEFMS(x.SOTK.ToString()));
                        if (cleancesNotExsitInFMS.Count() > 0)
                        {
                            foreach (var d in cleancesNotExsitInFMS)
                            {
                                var newClearance = MapOlaEcusClearanceToCustom(d, d.SOTK?.ToString().Trim());
                                newClearance.Source = OperationConstants.FromEcus;
                                lists.Add(newClearance);
                            }
                        }
                    }
                }
                if (lists.Count > 0)
                {
                    HandleState hs = DataContext.Add(lists);
                    if (hs.Success)
                    {
                        result = new HandleState(true, stringLocalizer[OperationLanguageSub.MSG_CUSTOM_CLEARANCE_ECUS_CONVERT_SUCCESS, lists.Count]);

                        string logErr = String.Format("Import Ecus thành công {0} \n {1} Tờ khai {2}", currentUser.UserName, lists.Count, DateTime.Now);
                        new LogHelper("ECUS", logErr);
                    }
                    else
                    {
                        result = new HandleState(true, stringLocalizer[OperationLanguageSub.MSG_CUSTOM_CLEARANCE_ECUS_CONVERT_SUCCESS, 0]);
                        string logErr = String.Format("{0} Import thất bại {1} Tờ khai do {2} at {3}", currentUser.UserName, lists.Count, hs.Message, DateTime.Now);
                        new LogHelper("ECUS", logErr);
                    }
                }
                else
                {
                    result = new HandleState(true, stringLocalizer[OperationLanguageSub.MSG_CUSTOM_CLEARANCE_ECUS_CONVERT_NO_DATA]);
                    string logErr = String.Format("Import thất bại {0} \n {1} Tờ khai {2}", currentUser.UserName, lists.Count, DateTime.Now);
                    new LogHelper("ECUS", logErr);
                }
            }
            catch (Exception ex)
            {
                string logErr = String.Format("Lỗi import Ecus {0} \n {1} {2}", currentUser.UserID, ex.ToString(), DateTime.Now);
                new LogHelper("ECUS", logErr);
                result = new HandleState(ex.Message);
            }
            return result;
        }

        private CustomsDeclaration MapOlaEcusClearanceToCustom(D_OLA clearance, string clearanceNo)
        {
            var type = ClearanceConstants.Export_Type_Value;
            if (clearance._XorN != null)
            {
                if (clearance._XorN.Contains(ClearanceConstants.Import_Type))
                {
                    type = ClearanceConstants.Import_Type_Value;
                }
            }
            var serviceType = GetServiceTypeOla(clearance.MA_HIEU_PTVC, out string cargoType);
            var route = clearance.PLUONG != null ? GetRouteType(clearance.PLUONG) : string.Empty;

            if (clearance.MA_DV != null)
            {
                Regex pattern = new Regex("[-_ ]");
                pattern.Replace(clearance.MA_DV, "");
            }

            var newItem = new CustomsDeclaration
            {
                IdfromEcus = clearance.D_OLAID,
                ClearanceNo = clearanceNo,
                FirstClearanceNo = clearance.SOTK?.ToString(),
                ClearanceDate = clearance.NGAY_KB,
                PartnerTaxCode = clearance.MA_DV,
                Mblid = clearance.SO_VAN_DON,
                Hblid = clearance.SO_VAN_DON,
                Gateway = clearance.MA_CANGNN,
                PortCodeNn = clearance.MA_CANGNN,
                UnitCode = clearance.MA_DVT,
                QtyCont = (int?)clearance.SO_CONTAINER ?? null,
                GrossWeight = clearance.TRLUONG,
                NetWeight = clearance.TRLUONG,
                Route = route,
                Type = type,
                CargoType = cargoType,
                ServiceType = serviceType,
                Shipper = clearance.TEN_DVXK,
                UserCreated = currentUser.UserID,
                DatetimeCreated = DateTime.Now,
                DatetimeModified = DateTime.Now,
                GroupId = currentUser.GroupId,
                DepartmentId = currentUser.DepartmentId,
                OfficeId = currentUser.OfficeID,
                CompanyId = currentUser.CompanyID,
                AccountNo = clearance.MA_DV,
                Cbm = clearance.THE_TICH,
                Pcs = (int?)clearance.LUONG,
            };

            return newItem;
        }

        private string GetServiceTypeOla(string MA_HIEU_PTVC, out string cargoType)
        {
            cargoType = string.Empty;
            var serviceType = string.Empty;
            switch (MA_HIEU_PTVC)
            {
                case "MAY BAY":
                    serviceType = ClearanceConstants.Air_Service;
                    break;
                default:
                    serviceType = ClearanceConstants.Sea_Service;
                    cargoType = ClearanceConstants.CargoTypeFCL;
                    break;
            }
            return serviceType;
        }

        public async Task<HandleState> ReplicateCustomClearance(int Id)
        {
            HandleState hs = new HandleState();
            CustomsDeclaration cd = DataContext.Get(x => x.Id == Id)?.FirstOrDefault();
            try
            {
                if (cd != null)
                {
                    if (string.IsNullOrEmpty(cd.JobNo))
                    {
                        throw new NullReferenceException();
                    }

                    OpsTransaction opsJob = opsTransactionRepo.Get(x => x.JobNo == cd.JobNo)?.FirstOrDefault();
                    if (opsJob == null)
                    {
                        throw new NullReferenceException();
                    }

                    if (opsJob.ReplicatedId == null)
                    {
                        return new HandleState((object)string.Format("Không tìm thấy thông tin lô replicate của lô {0}", cd.JobNo));
                    }

                    var opsJobReplicate = opsTransactionRepo.Get(x => x.Id == opsJob.ReplicatedId)?.FirstOrDefault();
                    if (opsJobReplicate == null)
                    {
                        return new HandleState((object)string.Format("Không tìm thấy thông tin lô replicate của lô {0}", cd.JobNo));
                    }
                    var existedClearance = DataContext.Any(x => x.ClearanceNo == cd.ClearanceNo && x.Id != cd.Id && opsJobReplicate.JobNo == x.JobNo);
                    if (existedClearance)
                    {
                        return new HandleState((object)string.Format("Tờ khai {0} đã được thêm vào lô replicate", cd.ClearanceNo));
                    }

                    CustomsDeclaration replicateCd = new CustomsDeclaration();

                    replicateCd.JobNo = opsJobReplicate.JobNo;
                    replicateCd.Source = OperationConstants.FROM_REPLICATE;
                    replicateCd.DatetimeCreated = DateTime.Now;
                    replicateCd.DatetimeModified = DateTime.Now;
                    replicateCd.GroupId = currentUser.GroupId;
                    replicateCd.DepartmentId = currentUser.DepartmentId;
                    replicateCd.OfficeId = currentUser.OfficeID;
                    replicateCd.CompanyId = currentUser.CompanyID;
                    replicateCd.AccountNo = cd.AccountNo;
                    replicateCd.PartnerTaxCode = cd.PartnerTaxCode;
                    replicateCd.Mblid = cd.Mblid;
                    replicateCd.Hblid = cd.Hblid;
                    replicateCd.ClearanceNo = cd.ClearanceNo;
                    replicateCd.ClearanceDate = cd.ClearanceDate;
                    replicateCd.ServiceType = cd.ServiceType;
                    replicateCd.PortCodeNn = cd.PortCodeNn;
                    replicateCd.UnitCode = cd.UnitCode;
                    replicateCd.QtyCont = cd.QtyCont;
                    replicateCd.Pcs = cd.Pcs;
                    replicateCd.ImportCountryCode = cd.ImportCountryCode;
                    replicateCd.ExportCountryCode = cd.ExportCountryCode;
                    replicateCd.GrossWeight = cd.GrossWeight;
                    replicateCd.Cbm = cd.Cbm;
                    replicateCd.Gateway = cd.Gateway;
                    replicateCd.Type = cd.Type;
                    replicateCd.CargoType = cd.CargoType;
                    replicateCd.Route = cd.Route;
                    replicateCd.Shipper = cd.Shipper;
                    replicateCd.Consignee = cd.Consignee;

                    hs = await DataContext.AddAsync(replicateCd);
                }
            }
            catch (NullReferenceException)
            {
                return new HandleState((object)"Không tìm thấy thông tin job trong tờ khai!");
            }

            return hs;
        }

        public async Task<HandleState> AddNewCustomsDeclaration(CustomsDeclarationModel model)
        {
            bool isUpdateCharge = false;
            var listCustom = await DataContext.Get(x => x.JobNo == model.JobNo).OrderBy(x => x.ClearanceDate).ThenByDescending(x => x.DatetimeModified)?.FirstOrDefaultAsync();
            if (listCustom == null)
            {
                isUpdateCharge = true;
            }
            var hs = DataContext.Add(model, false);
            if (isUpdateCharge && hs.Success)
            {
                var listSurcharge = csShipmentSurchargeRepo.Get(x => x.JobNo == model.JobNo && string.IsNullOrEmpty(x.SyncedFrom) && string.IsNullOrEmpty(x.PaySyncedFrom));
                var query = from surcharge in listSurcharge.Where(x => x.AdvanceNoFor != null)
                            join advances in accAdvancePaymentRepository.Get()
                            on surcharge.AdvanceNoFor equals advances.AdvanceNo
                            where advances.SyncStatus == null
                            select surcharge;

                //update custom clearance for advRequest
                var query2 = from advRequest in accAdvanceRequestRepository.Get(x => x.JobId == model.JobNo)
                             join advPayment in accAdvancePaymentRepository.Get()
                             on advRequest.AdvanceNo equals advPayment.AdvanceNo
                             where advPayment.SyncStatus == null
                             select advRequest;
                await query2.ForEachAsync(x => x.CustomNo = model.ClearanceNo);
                hs = accAdvanceRequestRepository.SubmitChanges();

                //update custom surcharge
                var listUpdateItem = listSurcharge.Where(x => string.IsNullOrEmpty(x.AdvanceNoFor)).Union(query);
                await listUpdateItem.ForEachAsync(x => x.ClearanceNo = model.ClearanceNo);
                hs = csShipmentSurchargeRepo.SubmitChanges();
            }
            DataContext.SubmitChanges();
            return hs;
        }
    }
}
