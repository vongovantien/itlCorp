using AutoMapper;
using eFMS.API.Operation.Service.Contexts;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Operation.DL.Common;
using eFMS.API.Operation.DL.IService;
using eFMS.API.Operation.DL.Models;
using eFMS.API.Operation.DL.Models.Criteria;
using eFMS.API.Operation.DL.Models.Ecus;
using eFMS.API.Operation.Service.Models;
using eFMS.API.Provider.Services.IService;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using eFMS.API.Operation.Service.ViewModels;
using System.Data.Common;
using ITL.NetCore.Connection;

namespace eFMS.API.Operation.DL.Services
{
    public class CustomsDeclarationService : RepositoryBase<CustomsDeclaration, CustomsDeclarationModel>, ICustomsDeclarationService
    {
        private readonly ICatPartnerApiService catPartnerApi;
        private readonly ICatPlaceApiService catPlaceApi;
        private readonly IEcusConnectionService ecusCconnectionService;
        private readonly ICatCountryApiService countryApi;
        private readonly ICatCommodityApiService commodityApi;
        private readonly IDistributedCache cache;
        private readonly ICurrentUser currentUser;
        private readonly IStringLocalizer stringLocalizer;

        public CustomsDeclarationService(IContextBase<CustomsDeclaration> repository, IMapper mapper,
            IEcusConnectionService ecusCconnection
            , ICatPartnerApiService catPartner
            , ICatPlaceApiService catPlace
            , ICatCountryApiService country
            , IDistributedCache distributedCache
            , ICatCommodityApiService commodity
            , ICurrentUser user
            , IStringLocalizer<LanguageSub> localizer) : base(repository, mapper)
        {
            ecusCconnectionService = ecusCconnection;
            catPartnerApi = catPartner;
            catPlaceApi = catPlace;
            countryApi = country;
            cache = distributedCache;
            commodityApi = commodity;
            currentUser = user;
            stringLocalizer = localizer;
        }

        public IQueryable<CustomsDeclarationModel> GetAll()
        {
            return GetAllData().AsQueryable();
        }

        public HandleState ImportClearancesFromEcus()
        {
            string userId = currentUser.UserID;
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var connections = dc.SetEcusconnection.Where(x => x.UserId == userId);
            var result = new HandleState();
            var lists = new List<CustomsDeclaration>();
            foreach (var item in connections)
            {
                var clearanceEcus = ecusCconnectionService.GetDataEcusByUser(item.UserId, item.ServerName, item.Dbusername, item.Dbpassword, item.Dbname);
               if (clearanceEcus == null)
                {
                    continue;
                }
                else
                {
                    var clearances = dc.CustomsDeclaration.ToList();
                    foreach (var clearance in clearanceEcus)
                    {
                        var clearanceNo = clearance.SOTK?.ToString().Trim();
                        var itemExisted = clearances.FirstOrDefault(x => x.ClearanceNo == clearanceNo && x.ClearanceDate == clearance.NGAY_DK);
                        var countDuplicated = lists.Count(x => x.ClearanceNo == clearanceNo && x.ClearanceDate == clearance.NGAY_DK);
                        if (itemExisted == null && clearanceNo != null && countDuplicated < 2)
                        {
                            var newClearance = MapEcusClearanceToCustom(clearance, clearanceNo);
                            newClearance.Source = Constants.FromEcus;
                            lists.Add(newClearance);
                        }
                    }
                }
            }
            try
            {
                if (lists.Count > 0)
                {
                    dc.CustomsDeclaration.AddRange(lists);
                    dc.SaveChanges();
                    result = new HandleState(true, lists.Count + stringLocalizer[LanguageSub.MSG_CUSTOM_CLEARANCE_ECUS_CONVERT_SUCCESS]);
                    cache.Remove(Templates.CustomDeclaration.NameCaching.ListName);
                }
                else
                {
                    result = new HandleState(true, stringLocalizer[LanguageSub.MSG_CUSTOM_CLEARANCE_ECUS_CONVERT_NO_DATA]);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        private CustomsDeclaration MapEcusClearanceToCustom(DTOKHAIMD clearance, string clearanceNo)
        {
            var type = ClearanceConstants.Export_Type_Value;
            if (clearance.XorN.Contains(ClearanceConstants.Import_Type))
            {
                type = ClearanceConstants.Import_Type_Value;
            }
            var serviceType = GetServiceType(clearance, out string cargoType);
            var route = clearance.PLUONG!= null?GetRouteType(clearance.PLUONG): string.Empty;
            var partnerTaxCode = clearance.MA_DV;
            if (clearance.MA_DV != null)
            {
                Regex pattern = new Regex("[-_ ]");
                pattern.Replace(clearance.MA_DV, "");
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
                Pcs = clearance.SO_KIEN != null ? (int?)clearance.SO_KIEN : null,
                UnitCode = clearance.DVT_KIEN,
                QtyCont = clearance.SO_CONTAINER == null ? (int?)clearance.SO_CONTAINER : null,
                GrossWeight = clearance.TR_LUONG,
                Route = route,
                Type = type,
                ServiceType = serviceType,
                UserCreated = currentUser.UserID,
                DatetimeCreated = DateTime.Now,
                DatetimeModified = DateTime.Now
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

        private string GetServiceType(DTOKHAIMD clearance, out string cargoType)
        {
            cargoType = string.Empty;
            var serviceType = string.Empty;
            switch (clearance.MA_HIEU_PTVC)
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
            Expression<Func<CustomsDeclaration, bool>> query = x => (x.ClearanceNo.IndexOf(criteria.ClearanceNo ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                                                                    && (x.UserCreated == criteria.PersonHandle || string.IsNullOrEmpty(criteria.PersonHandle))
                                                                                    && (x.Type == criteria.Type || string.IsNullOrEmpty(criteria.Type))
                                                                                    && (x.ClearanceDate >= criteria.FromClearanceDate || criteria.FromClearanceDate == null)
                                                                                    && (x.ClearanceDate <= criteria.ToClearanceDate || criteria.ToClearanceDate == null)
                                                                                    && (x.DatetimeCreated >= criteria.FromImportDate || criteria.FromImportDate == null)
                                                                                    && (x.DatetimeCreated <= criteria.ToImportDate || criteria.ToImportDate == null);
            //var data = GetCustomClearanceViewList(string.Empty);
            if (criteria.ImPorted == true)
            {
                query = query.And(x => x.JobNo != null);
            }
            else if (criteria.ImPorted == false)
            {
                query = query.And(x => x.JobNo == null);
            }
            Expression<Func<CustomsDeclaration, object>> orderByProperty = x => x.DatetimeModified;
            var list = DataContext.Paging(query, page, size, orderByProperty, false, out rowsCount);
            if (rowsCount == 0) return new List<CustomsDeclarationModel>();
            var results = MapClearancesToClearanceModels(list);
            return results;
        }


        public List<CustomsDeclarationModel> GetCustomDeclaration(string keySearch, string customerNo, bool Imported, int pageNumber, int pageSize, out int rowsCount)
        {
            List<CustomsDeclarationModel> returnList = null;
            var foos = keySearch;
            string[] fooArray = null;
            if (foos != null)
            {
                fooArray = foos.Split(',');
            }
      


            Func<CustomsDeclarationModel, bool> query = x => (x.PartnerTaxCode == customerNo)
            && (keySearch != null ? fooArray.Contains(x.ClearanceNo) :  x.ClearanceNo == keySearch || x.Hblid == keySearch
            || x.ExportCountryCode == keySearch || x.ImportCountryCode == keySearch || x.CommodityCode == keySearch
            || x.Note == keySearch || x.FirstClearanceNo == keySearch || string.IsNullOrEmpty(keySearch));
            var data = GetAllData().Where(query);
            //if (Imported == true)
            //{
            //    data = data.Where(x => x.JobNo != null);
            //}
            //else if (Imported == false)
            //{
            //    data = data.Where(x => x.JobNo == null);
            //}
            rowsCount = data.Count();
            if (rowsCount == 0) return returnList;
            else data = data.OrderByDescending(x => x.DatetimeModified);

            if (pageSize > 1)
            {
                if (pageNumber < 1)
                {
                    pageNumber = 1;
                }
                returnList = data.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            }
            return returnList;
        }

        private List<CustomsDeclarationModel> MapClearancesToClearanceModels(IQueryable<CustomsDeclaration> list)
        {
            List<CustomsDeclarationModel> results = new List<CustomsDeclarationModel>();
            var countryCache = countryApi.Getcountries().Result;
            var portCache = catPlaceApi.GetPlaces().Result;
            var customerCache = catPartnerApi.GetPartners().Result;
            var countries = countryCache != null ? countryCache.ToList() : new List<Provider.Models.CatCountryApiModel>(); //dc.CatCountry;
            var portIndexs = portCache != null ? portCache.Where(x => x.PlaceTypeId == GetTypeFromData.GetPlaceType(CatPlaceTypeEnum.Port)).ToList() : new List<Provider.Models.CatPlaceApiModel>(); //dc.CatPlace.Where(x => x.PlaceTypeId == GetTypeFromData.GetPlaceType(CatPlaceTypeEnum.Port));
            var customers = customerCache != null ? customerCache.Where(x => x.PartnerGroup.IndexOf(GetTypeFromData.GetPartnerGroup(CatPartnerGroupEnum.CUSTOMER), StringComparison.OrdinalIgnoreCase) > -1).ToList() : new List<Provider.Models.CatPartnerApiModel>(); //dc.CatPartner.Where(x => x.PartnerGroup == GetTypeFromData.GetPartnerGroup(CatPartnerGroupEnum.CUSTOMER));
            var clearances = (from clearance in list
                              join importCountry in countries on clearance.ImportCountryCode equals importCountry.Code into grpImports
                              from imCountry in grpImports.DefaultIfEmpty()
                              join exportCountry in countries on clearance.ExportCountryCode equals exportCountry.Code into grpExports
                              from exCountry in grpExports.DefaultIfEmpty()
                              join portIndex in portIndexs on clearance.Gateway equals portIndex.Code into grpPorts
                              from port in grpPorts.DefaultIfEmpty()
                              join partner in customers on clearance.PartnerTaxCode equals partner.TaxCode into grpCustomers
                              from customer in grpCustomers.DefaultIfEmpty()
                              select new { clearance, ImportCountryName = imCountry.NameEn, ExportCountryName = exCountry.NameEn, GatewayName = port.NameEn, CustomerName = customer.PartnerNameEn }
                       );
            if (clearances == null) return results;
            foreach (var item in clearances)
            {
                var clearance = mapper.Map<CustomsDeclarationModel>(item.clearance);
                clearance.ImportCountryName = item.ImportCountryName;
                clearance.ExportCountryName = item.ExportCountryName;
                clearance.CustomerName = item.CustomerName;
                clearance.GatewayName = item.GatewayName;
                results.Add(clearance);
            }
            return results;
        }

        public List<CustomsDeclarationModel> GetBy(string jobNo)
        {
            List<CustomsDeclarationModel> results = null;
            //var data = DataContext.Get(x => x.JobNo == jobNo);
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

        public HandleState UpdateJobToClearances(List<CustomsDeclarationModel> clearances)
        {
            var result = new HandleState();
            try
            {
                foreach (var item in clearances)
                {
                    var clearance = mapper.Map<CustomsDeclaration>(item);
                    DataContext.Update(clearance, x => x.Id == item.Id, false);
                }
                DataContext.DC.SaveChanges();
            }
            catch (Exception ex)
            {
                result = new HandleState(ex.Message);
            }
            return result;
        }
        public CustomsDeclaration GetById(int id)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var result = dc.CustomsDeclaration.Where(x => x.Id == id).FirstOrDefault();
            return result;
        }
        public IQueryable<CustomsDeclarationModel> Query(CustomsDeclarationCriteria criteria)
        {
            Func<CustomsDeclarationModel, bool> query = x => (x.ClearanceNo.IndexOf(criteria.ClearanceNo ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                                                                       && (x.UserCreated == criteria.PersonHandle || string.IsNullOrEmpty(criteria.PersonHandle))
                                                                                       && (x.Type == criteria.Type || string.IsNullOrEmpty(criteria.Type))
                                                                                       && (x.ClearanceDate >= criteria.FromClearanceDate || criteria.FromClearanceDate == null)
                                                                                       && (x.ClearanceDate <= criteria.ToClearanceDate || criteria.ToClearanceDate == null)
                                                                                       && (x.DatetimeCreated >= criteria.FromImportDate || criteria.FromImportDate == null)
                                                                                       && (x.DatetimeCreated <= criteria.ToImportDate || criteria.ToImportDate == null);
            var results = GetAllData().Where(query);
            if (criteria.ImPorted == true)
            {
                results = results.Where(x => x.JobNo != null);
            }
            else if (criteria.ImPorted == false)
            {
                results = results.Where(x => x.JobNo == null);
            }
            return results?.AsQueryable();
        }

        public List<CustomsDeclarationModel> GetCustomDeclaration(string customNo, string taxCode, bool? ImPorted, int pageNumber, int  pageSize, out int rowsCount)
        {
            Func<CustomsDeclarationModel, bool> query = x => x.PartnerTaxCode == taxCode && (x.ClearanceNo == customNo || string.IsNullOrEmpty(customNo));

            Expression<Func<CustomsDeclarationModel, object>> orderByProperty = x => x.DatetimeModified;
            List <CustomsDeclarationModel> returnList = null;
            var results = GetAllData().Where(query);
            rowsCount = results.Count();
            if (rowsCount == 0) return returnList;
            else results = results.OrderByDescending(x => x.DatetimeModified);
            if (ImPorted == true)
            {
                results = results.Where(x => x.JobNo != null);
            }
            else if (ImPorted == false)
            {
                results = results.Where(x => x.JobNo == null);
            }
            if (pageSize > 1)
            {
                if (pageNumber < 1)
                {
                    pageNumber = 1;
                }
                returnList = results.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            }
            return returnList;
        }


        private List<CustomsDeclarationModel> GetAllData()
        {
            //get from cache
            var clearanceCaching = RedisCacheHelper.GetObject<List<CustomsDeclarationModel>>(cache, Templates.CustomDeclaration.NameCaching.ListName);
            List<CustomsDeclarationModel> customClearances = null;
            if (clearanceCaching == null)
            {
                //get from view data
                var list = GetCustomClearanceViewList(string.Empty);
                customClearances = mapper.Map<List<CustomsDeclarationModel>>(list);
                RedisCacheHelper.SetObject(cache, Templates.CustomDeclaration.NameCaching.ListName, customClearances);
            }
            else
            {
                customClearances = clearanceCaching;
            }
            return customClearances;
        }
        private List<sp_GetCustomDeclaration> GetCustomClearanceViewList(string jobNo)
        {
            DbParameter[] parameters =
            {
                SqlParam.GetParameter("jobNo", jobNo)
            };
            return ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetCustomDeclaration>(parameters);
        }

        public HandleState DeleteMultiple(List<CustomsDeclarationModel> customs)
        {
            var result = new HandleState();
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                dc.CustomsDeclaration.RemoveRange(customs);
                dc.SaveChanges();
            }
            catch (Exception ex)
            {
                result = new HandleState(ex.Message);
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

            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            list.ForEach(item =>
            {
                //Check empty ClearanceNo
                string _clearanceNo = item.ClearanceNo;
                item.ClearanceNoValid = true;
                if (string.IsNullOrEmpty(_clearanceNo))
                {
                    item.ClearanceNo = stringLocalizer[LanguageSub.MSG_CUSTOM_CLEARANCE_NO_EMPTY];
                    item.IsValid = false;
                    item.ClearanceNoValid = false;
                }
                else
                {
                    //Check valid maxlength for Clearance No
                    if (_clearanceNo.Length > 100)
                    {
                        item.ClearanceNo = string.Format(stringLocalizer[LanguageSub.MSG_INVALID_MAX_LENGTH], 50);
                        item.IsValid = false;
                        item.ClearanceNoValid = false;
                    }

                    //Check input character special, number
                    Regex pattern = new Regex(@"^[a-zA-Z0-9 ./_-]*$");
                    if (!pattern.IsMatch(_clearanceNo))
                    {
                        item.ClearanceNo = stringLocalizer[LanguageSub.MSG_CUSTOM_INVALID_CHARACTER_SPECIAL];
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
                    item.ClearanceDateStr = stringLocalizer[LanguageSub.MSG_CUSTOM_CLEARANCE_DATE_EMPTY];
                    item.IsValid = false;
                    item.ClearanceDateValid = false;
                }
                else
                {
                    isDate = DateTime.TryParse(_clearanceDate, out dateTimeDefault);
                    if (!isDate)
                    {
                        item.ClearanceDateStr = stringLocalizer[LanguageSub.MSG_INVALID_DATE];
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
                string _partnerTaxCode = item.PartnerTaxCode;
                item.PartnerTaxCodeValid = true;
                if (string.IsNullOrEmpty(_partnerTaxCode))
                {
                    item.CustomerName = stringLocalizer[LanguageSub.MSG_CUSTOM_CLEARANCE_CUSTOMER_CODE_EMPTY];
                    item.IsValid = false;
                    item.PartnerTaxCodeValid = false;
                }
                else
                {
                    var isFound = catPartnerApi.GetPartners().Result.Any(x => x.TaxCode == _partnerTaxCode);
                    if (!isFound)
                    {
                        item.CustomerName = stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND];
                        item.IsValid = false;
                        item.PartnerTaxCodeValid = false;
                    }
                    else
                    {
                        item.CustomerName = catPartnerApi.GetPartners().Result.Where(x => x.TaxCode == _partnerTaxCode).First().PartnerNameEn;
                    }
                }

                //Check empty for MBL
                string _mbl = item.Mblid;
                item.MblidValid = true;
                if (string.IsNullOrEmpty(_mbl))
                {
                    item.Mblid = stringLocalizer[LanguageSub.MSG_CUSTOM_CLEARANCE_MBL_EMPTY];
                    item.IsValid = false;
                    item.MblidValid = false;
                }
                else
                {
                    //Check valid maxlength for MblId
                    if (_mbl.Length > 250)
                    {
                        item.Mblid = string.Format(stringLocalizer[LanguageSub.MSG_INVALID_MAX_LENGTH], 50);
                        item.IsValid = false;
                        item.MblidValid = false;
                    }

                    //Check input character special, number
                    Regex pattern = new Regex(@"^[a-zA-Z0-9 ./_-]*$");
                    if(!pattern.IsMatch(_mbl))
                    {
                        item.Mblid = stringLocalizer[LanguageSub.MSG_CUSTOM_INVALID_CHARACTER_SPECIAL];
                        item.IsValid = false;
                        item.MblidValid = false;
                    }
                }

                //Check empty for HBL
                string _hbl = item.Hblid;
                item.HblidValid = true;
                if (string.IsNullOrEmpty(_hbl))
                {
                    item.Hblid = stringLocalizer[LanguageSub.MSG_CUSTOM_CLEARANCE_HBL_EMPTY];
                    item.IsValid = false;
                    item.HblidValid = false;
                }
                else
                {
                    //Check valid maxlength for HblId
                    if (_hbl.Length > 250)
                    {
                        item.Hblid = string.Format(stringLocalizer[LanguageSub.MSG_INVALID_MAX_LENGTH], 50);
                        item.IsValid = false;
                        item.HblidValid = false;
                    }

                    //Check input character special, number
                    Regex pattern = new Regex(@"^[a-zA-Z0-9 ./_-]*$");
                    if (!pattern.IsMatch(_hbl))
                    {
                        item.Hblid = stringLocalizer[LanguageSub.MSG_CUSTOM_INVALID_CHARACTER_SPECIAL];
                        item.IsValid = false;
                        item.HblidValid = false;
                    }
                }

                //Check empty & exist data for Gateway
                string _gateway = item.Gateway;
                item.GatewayValid = true;
                if (string.IsNullOrEmpty(_gateway))
                {
                    item.GatewayName = stringLocalizer[LanguageSub.MSG_CUSTOM_CLEARANCE_GATEWAY_EMPTY];
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
                        item.GrossWeightStr = stringLocalizer[LanguageSub.MSG_INVALID_NUMBER];
                        item.IsValid = false;
                        item.GrossWeightValid = false;
                    }
                    else
                    {
                        var valueConvert = Convert.ToDecimal(_grossWeight);
                        if (valueConvert < 0)
                        {                           
                            item.GrossWeightStr = stringLocalizer[LanguageSub.MSG_INVALID_NEGATIVE];
                            item.IsValid = false;
                            item.GrossWeightValid = false;
                        }
                        else
                        {
                            item.GrossWeight = Math.Round(valueConvert, 2);
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
                        item.NetWeightStr = stringLocalizer[LanguageSub.MSG_INVALID_NUMBER];
                        item.IsValid = false;
                        item.NetWeightValid = false;
                    }
                    else
                    {
                        var valueConvert = Convert.ToDecimal(_netWeight);
                        if (valueConvert < 0)
                        {                            
                            item.NetWeightStr = stringLocalizer[LanguageSub.MSG_INVALID_NEGATIVE];
                            item.IsValid = false;
                            item.NetWeightValid = false;
                        }
                        else
                        {
                            item.NetWeight = Math.Round(valueConvert, 2);
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
                        item.CbmStr = stringLocalizer[LanguageSub.MSG_INVALID_NUMBER];
                        item.IsValid = false;
                        item.CbmValid = false;
                    }
                    else
                    {
                        var valueConvert = Convert.ToDecimal(_cbm);
                        if (valueConvert < 0)
                        {                            
                            item.CbmStr = stringLocalizer[LanguageSub.MSG_INVALID_NEGATIVE];
                            item.IsValid = false;
                            item.CbmValid = false;
                        }
                        else
                        {
                            item.Cbm = Math.Round(valueConvert, 2);
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
                        item.QtyContStr = stringLocalizer[LanguageSub.MSG_INVALID_NUMBER];
                        item.IsValid = false;
                        item.QtyContValid = false;
                    }
                    else
                    {
                        var valueConvert = Convert.ToInt32(_qtyCont);
                        if (valueConvert < 0)
                        {                          
                            item.QtyContStr = stringLocalizer[LanguageSub.MSG_INVALID_NEGATIVE];
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
                        item.PcsStr = stringLocalizer[LanguageSub.MSG_INVALID_NUMBER];
                        item.IsValid = false;
                        item.PcsValid = false;
                    }
                    else
                    {
                        var valueConvert = Convert.ToInt32(_pcs);
                        if (valueConvert < 0)
                        {                           
                            item.PcsStr = stringLocalizer[LanguageSub.MSG_INVALID_NEGATIVE];
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
                    var isFound = dc.CatCommodity.Any(x => x.Code == _commodity);
                    if (!isFound)
                    {
                        item.CommodityName = stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND];
                        item.IsValid = false;
                        item.CommodityValid = false;
                    }
                    else
                    {
                        item.CommodityName = dc.CatCommodity.Where(x => x.Code == _commodity).First().CommodityNameEn;
                    }
                }

                //Check empty & exist data for ServiceType
                string _serviceType = item.ServiceType;
                item.ServiceTypeValid = true;
                if (string.IsNullOrEmpty(_serviceType))
                {
                    item.ServiceType = stringLocalizer[LanguageSub.MSG_CUSTOM_CLEARANCE_SERVICE_TYPE_EMPTY];
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
                    item.CargoType = stringLocalizer[LanguageSub.MSG_CUSTOM_CLEARANCE_CARGO_TYPE_EMPTY];
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
                    item.Route = stringLocalizer[LanguageSub.MSG_CUSTOM_CLEARANCE_ROUTE_EMPTY];
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
                    item.ClearanceNo = string.Format(stringLocalizer[LanguageSub.MSG_DUPLICATE_DATA].Value, item.ClearanceNo);
                    item.ClearanceDateStr = string.Format(stringLocalizer[LanguageSub.MSG_DUPLICATE_DATA].Value, item.ClearanceDateStr);
                    item.IsValid = item.ClearanceNoValid = item.ClearanceDateValid = false;
                }

                //Check exist ClearanceNo & ClearanceDate
                if (dc.CustomsDeclaration.Any(x => x.ClearanceNo == item.ClearanceNo && x.ClearanceDate == item.ClearanceDate))
                {
                    item.ClearanceNo = item.ClearanceDateStr = string.Format(stringLocalizer[LanguageSub.MSG_CLEARANCENO_EXISTED].Value, item.ClearanceNo);
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
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                dc.CustomsDeclaration.AddRange(data);
                cache.Remove(Templates.CustomDeclaration.NameCaching.ListName);
                dc.SaveChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
    }
}
