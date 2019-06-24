using AutoMapper;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Service.Contexts;
using eFMS.API.Setting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using eFMS.API.Setting.DL.Models.Criteria;
using System.Linq.Expressions;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Setting.DL.Common;
using eFMS.API.Setting.DL.Models.Ecus;
using eFMS.API.Provider.Services.IService;
using Microsoft.Extensions.Caching.Distributed;

namespace eFMS.API.Setting.DL.Services
{
    public class CustomsDeclarationService : RepositoryBase<CustomsDeclaration, CustomsDeclarationModel>, ICustomsDeclarationService
    {
        private readonly ICatPartnerApiService catPartnerApi;
        private readonly ICatPlaceApiService catPlaceApi;
        private readonly IEcusConnectionService ecusCconnectionService;
        private readonly ICatCountryApiService countryApi;
        private readonly IDistributedCache cache;

        public CustomsDeclarationService(IContextBase<CustomsDeclaration> repository, IMapper mapper, 
            IEcusConnectionService ecusCconnection
            , ICatPartnerApiService catPartner
            , ICatPlaceApiService catPlace
            , ICatCountryApiService country
            , IDistributedCache distributedCache) : base(repository, mapper)
        {
            ecusCconnectionService = ecusCconnection;
            catPartnerApi = catPartner;
            catPlaceApi = catPlace;
            countryApi = country;
            cache = distributedCache;
        }

        public IQueryable<CustomsDeclaration> Get()
        {
            var clearanceCaching = RedisCacheHelper.GetObject<List<CustomsDeclaration>>(cache, Templates.CustomDeclaration.NameCaching.ListName);
            IQueryable<CustomsDeclaration> customClearances = null;
            if (clearanceCaching == null)
            {
                customClearances = DataContext.Get();
                RedisCacheHelper.SetObject(cache, Templates.CustomDeclaration.NameCaching.ListName, customClearances);
            }
            else
            {
                customClearances = clearanceCaching.AsQueryable();
            }
            return customClearances;
        }

        public HandleState ImportClearancesFromEcus()
        {
            string userId = "admin";
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var connections = dc.SetEcusconnection.Where(x => x.UserId == userId);
            var result = new HandleState();
            foreach (var item in connections)
            {
                var clearanceEcus = ecusCconnectionService.GetDataEcusByUser(item.UserId, item.ServerName, item.Dbusername, item.Dbpassword, item.Dbname);
                if(clearanceEcus == null)
                {
                    return new HandleState("Not connect data");
                }
                var clearances = dc.CustomsDeclaration.ToList();
                foreach (var clearance in clearanceEcus)
                {
                    var clearanceNo = clearance.SOTK.ToString();
                    var itemExisted = clearances.FirstOrDefault(x => x.ClearanceNo == clearanceNo && x.ClearanceDate == clearance.NGAY_DK);
                    if (itemExisted == null && clearance.SOTK != null)
                    {
                        var newClearance = MapEcusClearanceToCustom(clearance, clearanceNo);
                        newClearance.Source = Constants.FromEFMS;
                        dc.CustomsDeclaration.Add(newClearance);
                    }
                }
            }
            try
            {
                dc.SaveChanges();
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
            var serviceType = GetServiceType(clearance);
            var route = GetRouteType(clearance.PLUONG);
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
                PortCodeCk = clearance.MA_CK,
                PortCodeNn = clearance.MA_CANGNN,
                ExportCountryCode = clearance.NUOC_XK,
                ImportcountryCode = clearance.NUOC_NK,
                Pcs = clearance.SO_KIEN == null ? (int?)clearance.SO_KIEN : null,
                UnitId = clearance.DVT_KIEN,
                QtyCont = clearance.SO_CONTAINER == null ? (int?)clearance.SO_CONTAINER : null,
                GrossWeight = clearance.TR_LUONG,
                Route = clearance.PLUONG,
                Type = type,
                ServiceType = serviceType,
            };
            return newItem;
        }

        private string GetRouteType(string luong)
        {
            var route = string.Empty;
            switch(luong)
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

        private string GetServiceType(DTOKHAIMD clearance)
        {
            var serviceType = string.Empty;
            switch (clearance.MA_HIEU_PTVC)
            {
                case ClearanceConstants.Air_Service_Type:
                    serviceType = ClearanceConstants.Air_Service;
                    break;
                case ClearanceConstants.Sea_FCL_Service_Type:
                    serviceType = ClearanceConstants.Sea_FCL_Service;
                    break;
                case ClearanceConstants.Sea_LCL_Service_Type:
                    serviceType = ClearanceConstants.Sea_LCL_Service;
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
            if (criteria.ImPorted == true)
            {
                query = query.And(x => x.JobNo != null);
            }
            else if(criteria.ImPorted == false)
            {
                query = query.And(x => x.JobNo == null);
            }
            var list = DataContext.Paging(query, page, size, out rowsCount);
            if (rowsCount == 0) return new List<CustomsDeclarationModel>();
            var results = MapClearancesToClearanceModels(list);
            return results;
        }

        private List<CustomsDeclarationModel> MapClearancesToClearanceModels(IQueryable<CustomsDeclaration> list)
        {
            List<CustomsDeclarationModel> results = new List<CustomsDeclarationModel>();
            //eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var countryCache = countryApi.Getcountries().Result;
            var portCache = catPlaceApi.GetPlaces().Result;
            var customerCache = catPartnerApi.GetPartners().Result;
            var countries = countryCache != null ? countryCache.ToList() : new List<Provider.Models.CatCountryApiModel>(); //dc.CatCountry;
            var portIndexs = portCache != null ? portCache.Where(x => x.PlaceTypeId == GetTypeFromData.GetPlaceType(CatPlaceTypeEnum.Port)).ToList() : new List<Provider.Models.CatPlaceApiModel>(); //dc.CatPlace.Where(x => x.PlaceTypeId == GetTypeFromData.GetPlaceType(CatPlaceTypeEnum.Port));
            var customers = customerCache != null ? customerCache.Where(x => x.PartnerGroup == GetTypeFromData.GetPartnerGroup(CatPartnerGroupEnum.CUSTOMER)).ToList() : new List<Provider.Models.CatPartnerApiModel>(); //dc.CatPartner.Where(x => x.PartnerGroup == GetTypeFromData.GetPartnerGroup(CatPartnerGroupEnum.CUSTOMER));
            //var countries = dc.CatCountry;
            //var portIndexs = dc.CatPlace.Where(x => x.PlaceTypeId == GetTypeFromData.GetPlaceType(CatPlaceTypeEnum.Port));
            //var customers = dc.CatPartner.Where(x => x.PartnerGroup == GetTypeFromData.GetPartnerGroup(CatPartnerGroupEnum.CUSTOMER));
            var clearances = (from clearance in list
                              join importCountry in countries on clearance.ImportcountryCode equals importCountry.Code into grpImports
                              from imCountry in grpImports.DefaultIfEmpty()
                              join exportCountry in countries on clearance.ExportCountryCode equals exportCountry.Code into grpExports
                              from exCountry in grpExports.DefaultIfEmpty()
                              join portIndex in portIndexs on clearance.Gateway equals portIndex.Code into grpPorts
                              from port in grpPorts.DefaultIfEmpty()
                              join partner in customers on clearance.PartnerTaxCode equals partner.TaxCode into grpCustomers
                              from customer in grpCustomers.DefaultIfEmpty()
                              select new { clearance, ImportCountryName = imCountry.NameEn, ExportCountryName = exCountry.NameEn, GatewayName = port.NameEn, CustomerName = customer.PartnerNameEn }
                       );
            if(clearances == null) return results;
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

        public List<CustomsDeclarationModel> GetBy(Guid jobId)
        {
            List<CustomsDeclarationModel> results = null;
            var data = DataContext.Get(x => x.JobId == jobId);
            if (data == null) return results;
            results = new List<CustomsDeclarationModel>();
            foreach (var item in data)
            {
                var clearance = mapper.Map<CustomsDeclarationModel>(item);
                results.Add(clearance);
            }
            return results;
        }

        public object GetClearanceTypeData() {
            var types = CustomData.Types;
            var cargoTypes = CustomData.CargoTypes;
            var routes = CustomData.Routes;
            var serviceTypes = CustomData.ServiceTypes;
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
                    clearance.JobId = item.JobId;
                    DataContext.Update(clearance, x => x.Id == item.Id);
                }
            }
            catch (Exception ex)
            {
                result = new HandleState(ex.Message);
            }
            return result;
        }
 		public CustomsDeclaration GetById(string id)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var result = dc.CustomsDeclaration.Where(x => x.Id == Int32.Parse(id)).FirstOrDefault();
            return result;
        }
        public List<CustomsDeclarationModel> Query(CustomsDeclarationCriteria criteria)
        {
            Expression<Func<CustomsDeclaration, bool>> query = x => (x.ClearanceNo.IndexOf(criteria.ClearanceNo ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                                                                       && (x.UserCreated == criteria.PersonHandle || string.IsNullOrEmpty(criteria.PersonHandle))
                                                                                       && (x.Type == criteria.Type || string.IsNullOrEmpty(criteria.Type))
                                                                                       && (x.ClearanceDate >= criteria.FromClearanceDate || criteria.FromClearanceDate == null)
                                                                                       && (x.ClearanceDate <= criteria.ToClearanceDate || criteria.ToClearanceDate == null)
                                                                                       && (x.DatetimeCreated >= criteria.FromImportDate || criteria.FromImportDate == null)
                                                                                       && (x.DatetimeCreated <= criteria.ToImportDate || criteria.ToImportDate == null);
            if (criteria.ImPorted == true)
            {
                query = query.And(x => x.JobNo != null);
            }
            else if (criteria.ImPorted == false)
            {
                query = query.And(x => x.JobNo == null);
            }
            var list = DataContext.Get(query);
            if (list == null) return new List<CustomsDeclarationModel>();
            var results = MapClearancesToClearanceModels(list);
            return results;
        }
    }
}
