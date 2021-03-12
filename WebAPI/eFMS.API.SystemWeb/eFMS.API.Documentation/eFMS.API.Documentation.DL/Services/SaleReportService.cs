using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models.ReportResults.Sales;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.ViewModels;
using AutoMapper;
using ITL.NetCore.Connection.BL;
using eFMS.API.Documentation.DL.Models;
using ITL.NetCore.Connection;

namespace eFMS.API.Documentation.DL.Services
{
    public class SaleReportService : RepositoryBase<OpsTransaction, OpsTransactionModel> ,ISaleReportService
    {
        readonly IContextBase<OpsTransaction> opsRepository;
        readonly IContextBase<CsTransaction> csRepository;
        readonly IContextBase<CsTransactionDetail> detailRepository;
        readonly IContextBase<CatDepartment> departmentRepository;
        readonly IContextBase<CsMawbcontainer> containerRepository;
        readonly IContextBase<CatPartner> catPartnerRepository;
        readonly IContextBase<CatPlace> catPlaceRepository;
        readonly IContextBase<CatUnit> uniRepository;
        readonly IContextBase<CsShipmentSurcharge> surchargeRepository;
        readonly IContextBase<CatCurrencyExchange> exchangeRepository;
        readonly IContextBase<SysUser> userRepository;
        readonly IContextBase<SysEmployee> employeeRepository;
        readonly IContextBase<SysCompany> companyRepository;
        readonly ICurrentUser currentUser;
        readonly IContextBase<SysOffice> officeRepository;
        readonly IContextBase<SysUserLevel> userLevelRepository;
        private readonly ICurrencyExchangeService currencyExchangeService;
        readonly IContextBase<CatChargeGroup> catChargeGroupRepo;
        private decimal _decimalNumber = Constants.DecimalNumber;

        public SaleReportService(IContextBase<OpsTransaction> opsRepo,
            IMapper mapper,
            IContextBase<CsTransaction> csRepo,
            IContextBase<CsTransactionDetail> detailRepo,
            IContextBase<CatDepartment> departmentRepo,
            IContextBase<CsMawbcontainer> containerRepo,
            IContextBase<CatPartner> catPartnerRepo,
            IContextBase<CatPlace> placeRepo,
            IContextBase<CatUnit> uniRepo,
            IContextBase<CsShipmentSurcharge> surchargeRepo,
            IContextBase<CatCurrencyExchange> exchangeRepo,
            IContextBase<SysUser> userRepo,
            IContextBase<SysEmployee> employeeRepo,
            ICurrentUser currUser,
            IContextBase<SysCompany> companyRepo,
            IContextBase<SysOffice> officeRepo,
            IContextBase<SysUserLevel> userLevelRepo,
            ICurrencyExchangeService currencyExchange,
            IContextBase<CatChargeGroup> catChargeGroup) : base(opsRepo, mapper)
        {
            opsRepository = opsRepo;
            csRepository = csRepo;
            detailRepository = detailRepo;
            departmentRepository = departmentRepo;
            containerRepository = containerRepo;
            catPartnerRepository = catPartnerRepo;
            catPlaceRepository = placeRepo;
            uniRepository = uniRepo;
            surchargeRepository = surchargeRepo;
            exchangeRepository = exchangeRepo;
            userRepository = userRepo;
            employeeRepository = employeeRepo;
            currentUser = currUser;
            companyRepository = companyRepo;
            officeRepository = officeRepo;
            userLevelRepository = userLevelRepo;
            currencyExchangeService = currencyExchange;
            catChargeGroupRepo = catChargeGroup;
        }
        private IQueryable<MonthlySaleReportResult> GetMonthlySaleReport(SaleReportCriteria criteria)
        {
            IQueryable<MonthlySaleReportResult> csShipments = null;
            csShipments = GetCSSaleReport(criteria);
            return csShipments;
        }

        private List<sp_GetDataSaleReport> GetDataSaleReport(SaleReportCriteria criteria)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@serviceDateFrom", Value = criteria.ServiceDateFrom },
                new SqlParameter(){ ParameterName = "@serviceDateTo", Value = criteria.ServiceDateTo },
                new SqlParameter(){ ParameterName = "@createdDateFrom", Value = criteria.CreatedDateFrom },
                new SqlParameter(){ ParameterName = "@createdDateTo", Value = criteria.CreatedDateTo },
                new SqlParameter(){ ParameterName = "@customerId", Value = criteria.CustomerId },
                new SqlParameter(){ ParameterName = "@service", Value = criteria.Service },
                new SqlParameter(){ ParameterName = "@currency", Value = criteria.Currency },
                new SqlParameter(){ ParameterName = "@jobId", Value = criteria.JobId },
                new SqlParameter(){ ParameterName = "@mawb", Value = criteria.Mawb },
                new SqlParameter(){ ParameterName = "@hawb", Value = criteria.Hawb },
                new SqlParameter(){ ParameterName = "@officeId", Value = criteria.OfficeId },
                new SqlParameter(){ ParameterName = "@departmentId", Value = criteria.DepartmentId },
                new SqlParameter(){ ParameterName = "@groupId", Value = criteria.GroupId },
                new SqlParameter(){ ParameterName = "@personalInCharge", Value = criteria.PersonInCharge },
                new SqlParameter(){ ParameterName = "@salesMan", Value = criteria.SalesMan },
                new SqlParameter(){ ParameterName = "@creator", Value = criteria.Creator },
                new SqlParameter(){ ParameterName = "@carrierId", Value = criteria.CarrierId },
                new SqlParameter(){ ParameterName = "@agentId", Value = criteria.AgentId },
                new SqlParameter(){ ParameterName = "@pol", Value = criteria.Pol },
                new SqlParameter(){ ParameterName = "@pod", Value = criteria.Pod }
            };
            var list = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetDataSaleReport>(parameters);
            return list;
        }

        private IQueryable<MonthlySaleReportResult> GetOpsSaleReport(SaleReportCriteria criteria)
        {
            List<MonthlySaleReportResult> results = null;
            IQueryable<OpsTransaction> data = QueryOpsSaleReport(criteria);
            if (data == null) return null;
            var containerData = uniRepository.Get(x => x.UnitType == "Container");
            results = new List<MonthlySaleReportResult>();
            foreach (var item in data)
            {
                var report = new MonthlySaleReportResult
                {
                    Department = departmentRepository.Get(x => x.Id == item.DepartmentId).FirstOrDefault()?.DeptNameEn,
                    //ContactName = userRepository.Get(x => x.Id == item.SalemanId).FirstOrDefault()?.Username,
                    SalesManager = string.Empty,
                    PartnerName = catPartnerRepository.Get(x => x.Id == item.CustomerId).FirstOrDefault()?.PartnerNameEn,
                    Description = "Logistics",
                    Area = string.Empty,
                    POL = item.Pol != null ? catPlaceRepository.Get(x => x.Id == item.Pol).FirstOrDefault()?.NameEn : string.Empty,
                    POD = item.Pod != null ? catPlaceRepository.Get(x => x.Id == item.Pod).FirstOrDefault()?.NameEn : string.Empty,
                    Lines = catPartnerRepository.Get(x => x.Id == item.SupplierId).FirstOrDefault()?.PartnerNameEn,
                    Agent = item.AgentId != null ? catPartnerRepository.Get(x => x.Id == item.AgentId).FirstOrDefault()?.PartnerNameEn : string.Empty,
                    NominationParty = string.Empty,
                    assigned = false,
                    TransID = item.JobNo,
                    HWBNO = item.Hwbno,
                    KGS = (item.SumGrossWeight ?? 0) + _decimalNumber, //Cộng thêm phần thập phân
                    CBM = (item.SumCbm ?? 0) + _decimalNumber, //Cộng thêm phần thập phân
                    SharedProfit = 0,
                    OtherCharges = 0,
                    SalesTarget = 0,
                    Bonus = 0,
                    TypeOfService = "CL",
                    Shipper = item.Shipper ?? string.Empty,
                    Consignee = item.Consignee ?? string.Empty,
                    LoadingDate = item.ServiceDate
                };
                string employeeId = userRepository.Get(x => x.Id == item.SalemanId).FirstOrDefault()?.EmployeeId;
                if (employeeId != null)
                {
                    report.ContactName = employeeRepository.Get(x => x.Id == employeeId).FirstOrDefault()?.EmployeeNameVn;
                }
                //Tổng amount trước thuế selling của HBL
                report.SellingRate = GetSellingRate(item.Hblid, criteria.Currency) + _decimalNumber; //Cộng thêm phần thập phân
                //Tổng amount trước thuế Buying của HBL (ko lấy phí có tick KB)
                report.BuyingRate = GetBuyingRate(item.Hblid, criteria.Currency) + _decimalNumber; //Cộng thêm phần thập phân
                //Tổng amount Trước thuế của phí tick chon Kick Back
                report.SharedProfit = GetShareProfit(item.Hblid, criteria.Currency) + _decimalNumber; //Cộng thêm phần thập phân
                var contInfo = GetContainer(containerData, null, item.Hblid);
                if (contInfo != null)
                {
                    report.Cont40HC = (decimal)contInfo?.Cont40HC + _decimalNumber;
                    report.Qty20 = (decimal)contInfo?.Qty20 + _decimalNumber;
                    report.Qty40 = (decimal)contInfo?.Qty40 + _decimalNumber;
                }
                results.Add(report);
            }
            return results.AsQueryable();
        }
        private IQueryable<OpsTransaction> QueryOpsSaleReport(SaleReportCriteria criteria)
        {
            var hasSalesman = criteria.SalesMan != null; // Check if Type=Salesman
            Expression<Func<OpsTransaction, bool>> queryOpsTrans = x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED
                                                                         && (x.AgentId == criteria.AgentId || criteria.AgentId == null)
                                                                         && (x.Mblno.IndexOf(criteria.Mawb, StringComparison.OrdinalIgnoreCase) > -1 || string.IsNullOrEmpty(criteria.Mawb))
                                                                         && (x.JobNo.IndexOf(criteria.JobId, StringComparison.OrdinalIgnoreCase) > -1 || string.IsNullOrEmpty(criteria.JobId))
                                                                         && (x.Hwbno.IndexOf(criteria.Hawb, StringComparison.OrdinalIgnoreCase) > -1 || string.IsNullOrEmpty(criteria.Hawb))
                                                                         && (x.Pod == criteria.Pod || criteria.Pod == null)
                                                                         && (x.Pol == criteria.Pol || criteria.Pol == null)
                                                                         && (x.SupplierId == criteria.CarrierId || criteria.CarrierId == null)
                                                                         && (criteria.PersonInCharge.Contains(x.BillingOpsId) || string.IsNullOrEmpty(criteria.PersonInCharge))
                                                                         && (criteria.Creator.Contains(x.UserCreated) || string.IsNullOrEmpty(criteria.Creator))
                                                                         && (criteria.SalesMan.Contains(x.SalemanId) || string.IsNullOrEmpty(criteria.SalesMan));
            if (hasSalesman)
            {
                queryOpsTrans = queryOpsTrans.And(x => (!string.IsNullOrEmpty(x.SalesOfficeId) || string.IsNullOrEmpty(criteria.OfficeId))
                                                && (!string.IsNullOrEmpty(x.SalesDepartmentId) || string.IsNullOrEmpty(criteria.DepartmentId))
                                                && (!string.IsNullOrEmpty(x.SalesGroupId) || string.IsNullOrEmpty(criteria.GroupId)));
            }
            else
            {
                queryOpsTrans = queryOpsTrans.And(x => (criteria.OfficeId.Contains(x.OfficeId.ToString()) || string.IsNullOrEmpty(criteria.OfficeId))
                                                && (criteria.DepartmentId.Contains(x.DepartmentId.ToString()) || string.IsNullOrEmpty(criteria.DepartmentId))
                                                && (criteria.GroupId.Contains(x.GroupId.ToString()) || string.IsNullOrEmpty(criteria.GroupId)));
            }
            if (criteria.ServiceDateFrom != null && criteria.ServiceDateTo != null)
            {
                queryOpsTrans = queryOpsTrans.And(x => x.ServiceDate.Value.Date >= criteria.ServiceDateFrom.Value.Date && x.ServiceDate.Value.Date <= criteria.ServiceDateTo.Value.Date);
            }
            if (criteria.CreatedDateFrom != null && criteria.CreatedDateTo != null)
            {
                queryOpsTrans = queryOpsTrans.And(x => x.DatetimeCreated.Value.Date >= criteria.CreatedDateFrom.Value.Date && x.DatetimeCreated.Value.Date <= criteria.CreatedDateTo.Value.Date);
            }
            // Search Customer
            if (!string.IsNullOrEmpty(criteria.CustomerId))
            {
                queryOpsTrans = queryOpsTrans.And(x => !string.IsNullOrEmpty(x.CustomerId) && criteria.CustomerId.Contains(x.CustomerId));
            }
            var data = opsRepository.Get(queryOpsTrans);
            var shipmentList = new List<OpsTransaction>();
            if (hasSalesman)
            {
                foreach (var shipment in data)
                {
                    if (string.IsNullOrEmpty(criteria.OfficeId) || shipment.SalesOfficeId.Split(';').Intersect(criteria.OfficeId.Split(';')).Any())
                    {
                        if (string.IsNullOrEmpty(criteria.DepartmentId) || shipment.SalesDepartmentId.Split(';').Intersect(criteria.DepartmentId.Split(';')).Any())
                        {
                            if (string.IsNullOrEmpty(criteria.GroupId) || shipment.SalesGroupId.Split(';').Intersect(criteria.GroupId.Split(';')).Any())
                            {
                                shipmentList.Add(shipment);
                            }
                        }
                    }
                }
                data = shipmentList.AsQueryable();
            }
            return data;
        }

        private IQueryable<MonthlySaleReportResult> GetCSSaleReport(SaleReportCriteria criteria)
        {
            var list = GetDataSaleReport(criteria);
            if (list == null) return null;
            var listPartner = catPartnerRepository.Get();
            var partnerLookup = listPartner.ToLookup(q => q.Id);
            var listPlace = catPlaceRepository.Get();
            var placeLookup = listPlace.ToLookup(q => q.Id);
            var listEmployee = employeeRepository.Get();
            var lookupEmployee = listEmployee.ToLookup(q => q.Id);
            var listUser = userRepository.Get();
            var lookupUser = listUser.ToLookup(q => q.Id);
            var listCharges = surchargeRepository.Get();
            var lookupSellingCharges = listCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE).ToLookup(q => q.Hblid);
            var lookupBuying = listCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && (x.KickBack == false || x.KickBack == null)).ToLookup(q => q.Hblid);
            var lookupShareProfit = listCharges.Where(x => x.KickBack == true).ToLookup(q => q.Hblid);
            var listDepartment = departmentRepository.Get();
            var departmentLookup = listDepartment.ToLookup(q => q.Id);
            var containerList = containerRepository.Get();
            var containerLookup = containerList.ToLookup(q => q.Hblid);
            var results = new List<MonthlySaleReportResult>();
            foreach (var item in list)
            {
                var report = new MonthlySaleReportResult
                {
                    Department = item.DepartmentId != null ? departmentLookup[(int)item.DepartmentId].FirstOrDefault()?.DeptNameEn : string.Empty,
                    SalesManager = string.Empty,
                    PartnerName = partnerLookup[item.CustomerId].FirstOrDefault()?.PartnerNameEn,
                    Description = item.TransactionType == "CL"? "Logistics" : API.Common.Globals.CustomData.Services.FirstOrDefault(x => x.Value == item.TransactionType)?.DisplayName,
                    Area = string.Empty,
                    POL = item.Pol != null ? placeLookup[(Guid)item.Pol].FirstOrDefault()?.NameEn : string.Empty,
                    POD = item.Pod != null ? placeLookup[(Guid)item.Pod].FirstOrDefault()?.NameEn : string.Empty,
                    Lines = item.TransactionType == "CL" ? partnerLookup[item.ColoaderId].FirstOrDefault()?.PartnerNameEn :  item.ColoaderId != null ? partnerLookup[item.ColoaderId].FirstOrDefault()?.PartnerNameEn : string.Empty,
                    Agent = item.AgentId != null ? partnerLookup[item.AgentId].FirstOrDefault()?.PartnerNameEn : string.Empty,
                    NominationParty = item.NominationParty ?? string.Empty,
                    assigned = item.TransactionType == "CL" ? false: item.ShipmentType == "Nominated",
                    TransID = item.JobNo,
                    HWBNO = item.HwbNo,
                    KGS = item.TransactionType == "CL" ? (item.GrossWeight ?? 0) + _decimalNumber : (item.TransactionType.Contains("A") ? (item.ChargeWeight ?? 0) : (item.GrossWeight ?? 0)) + _decimalNumber, //CR: CW đối với hàng Air, GW các hàng còn lại [25-09-2020]
                    CBM = (item.CBM ?? 0) + _decimalNumber, //Cộng thêm phần thập phân
                    SharedProfit = 0,
                    OtherCharges = 0,
                    SalesTarget = 0,
                    Bonus = 0,
                    TypeOfService = item.TransactionType == "CL" ? "CL"  : (item.TypeOfService != null) ? (item.TypeOfService.Contains("LCL") ? "LCL" : string.Empty) : string.Empty,//item.ShipmentType.Contains("I") ? "IMP" : "EXP",
                    Shipper = item.TransactionType == "CL" ? item.ShipperId : partnerLookup[item.ShipperId].FirstOrDefault()?.PartnerNameEn, //CR: Get Shipper Name En [25-09-2020]
                    Consignee = item.TransactionType == "CL" ? item.ConsigneeId : partnerLookup[item.ConsigneeId].FirstOrDefault()?.PartnerNameEn, //CR: Get Consignee Name En [25-09-2020]
                    LoadingDate = item.TransactionType == "CL" ? item.Etd : item.TransactionType.Contains("I") ? item.Eta : item.Etd
                };
                string employeeId = lookupUser[item.SalemanId].FirstOrDefault()?.EmployeeId;
                if (employeeId != null)
                {
                    report.ContactName = lookupEmployee[employeeId].FirstOrDefault()?.EmployeeNameVn;
                }
                //Tổng amount trước thuế selling của HBL
                report.SellingRate = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? (decimal)lookupSellingCharges[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupSellingCharges[item.HblId].Sum(x => x.AmountUsd ?? 0) + _decimalNumber;//GetSellingRate(item.HblId, criteria.Currency) + _decimalNumber; //Cộng thêm phần thập phân
                //Tổng amount trước thuế Buying của HBL (ko lấy phí có tick KB)
                report.BuyingRate = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? (decimal)lookupBuying[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupBuying[item.HblId].Sum(x => x.AmountUsd ?? 0) + _decimalNumber; //Cộng thêm phần thập phân
                //Tổng Amount Trước thuế của phí tick chon Kick Back
                report.SharedProfit = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? (decimal)lookupShareProfit[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupShareProfit[item.HblId].Sum(x => x.AmountUsd ?? 0) + _decimalNumber; //Cộng thêm phần thập phân

                report.Cont40HC = item.Cont40HC ?? 0 + _decimalNumber;
                report.Qty20 = item.Qty20 ?? 0 + _decimalNumber;
                report.Qty40 = item.Qty40 ?? 0 + _decimalNumber;
                results.Add(report);
            }
            return results.AsQueryable();
        }
        private IQueryable<CsTransactionDetail> QueryHouseBills(SaleReportCriteria criteria)
        {
            Expression<Func<CsTransactionDetail, bool>> queryTranDetail = null;
            // Search Customer
            if (!string.IsNullOrEmpty(criteria.CustomerId))
            {
                queryTranDetail = x => !string.IsNullOrEmpty(x.CustomerId) ? criteria.CustomerId.Contains(x.CustomerId) : true;
            }
            // Search Hawb
            if (!string.IsNullOrEmpty(criteria.Hawb))
            {
                queryTranDetail = queryTranDetail == null ? (x => !string.IsNullOrEmpty(x.Hwbno) && x.Hwbno.IndexOf(criteria.Hawb, StringComparison.OrdinalIgnoreCase) > -1)
                                                        : queryTranDetail.And(x => !string.IsNullOrEmpty(x.Hwbno) && x.Hwbno.IndexOf(criteria.Hawb, StringComparison.OrdinalIgnoreCase) > -1);
            }
            var hasSalesman = criteria.SalesMan != null; // Check if Type = Salesman
            if (hasSalesman)
            {
                // Search SalesOffice
                if (!string.IsNullOrEmpty(criteria.OfficeId))
                {
                    queryTranDetail = queryTranDetail == null ? (x => !string.IsNullOrEmpty(x.SalesOfficeId))
                                                              : queryTranDetail.And(x => !string.IsNullOrEmpty(x.SalesOfficeId));
                }
                // Search SalesDepartment
                if (!string.IsNullOrEmpty(criteria.DepartmentId))
                {
                    queryTranDetail = queryTranDetail == null ? (x => !string.IsNullOrEmpty(x.SalesDepartmentId))
                                                              : queryTranDetail.And(x => !string.IsNullOrEmpty(x.SalesDepartmentId));
                }
                // Search SalesGroup
                if (!string.IsNullOrEmpty(criteria.GroupId))
                {
                    queryTranDetail = queryTranDetail == null ? (x => !string.IsNullOrEmpty(x.SalesGroupId))
                                                              : queryTranDetail.And(x => !string.IsNullOrEmpty(x.SalesGroupId));
                }
                // Search SaleMan
                if (!string.IsNullOrEmpty(criteria.SalesMan))
                {
                    queryTranDetail = queryTranDetail == null ? (x => criteria.SalesMan.Contains(x.SaleManId))
                                                          : queryTranDetail.And(x => criteria.SalesMan.Contains(x.SaleManId));
                }
            }
            var housebills = queryTranDetail == null ? detailRepository.Get() : detailRepository.Get(queryTranDetail);
            var houseBillList = new List<CsTransactionDetail>();
            if (hasSalesman)
            {
                foreach (var house in housebills)
                {
                    if (string.IsNullOrEmpty(criteria.OfficeId) || house.SalesOfficeId.Split(';').Intersect(criteria.OfficeId.Split(';')).Any())
                    {
                        if (string.IsNullOrEmpty(criteria.DepartmentId) || house.SalesDepartmentId.Split(';').Intersect(criteria.DepartmentId.Split(';')).Any())
                        {
                            if (string.IsNullOrEmpty(criteria.GroupId) || house.SalesGroupId.Split(';').Intersect(criteria.GroupId.Split(';')).Any())
                            {
                                houseBillList.Add(house);
                            }
                        }
                    }
                }
                housebills = houseBillList.AsQueryable();
            }
            return housebills;
        }

        private IQueryable<CsTransaction> QueryCsTransaction(SaleReportCriteria criteria)
        {
            var hasSalesman = criteria.SalesMan != null; // Check if Type = Salesman
            Expression<Func<CsTransaction, bool>> queryTrans = x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED
                                                                 && (x.AgentId == criteria.AgentId || criteria.AgentId == null)
                                                                 && (x.Mawb.IndexOf(criteria.Mawb, StringComparison.OrdinalIgnoreCase) > -1 || string.IsNullOrEmpty(criteria.Mawb))
                                                                 && (x.JobNo.IndexOf(criteria.JobId, StringComparison.OrdinalIgnoreCase) > -1 || string.IsNullOrEmpty(criteria.JobId))
                                                                 && (criteria.Service.Contains(x.TransactionType) || string.IsNullOrEmpty(criteria.Service))
                                                                 && (x.Pod == criteria.Pod || criteria.Pod == null)
                                                                 && (x.Pol == criteria.Pol || criteria.Pol == null)
                                                                 && (x.ColoaderId == criteria.CarrierId || criteria.CarrierId == null)
                                                                 && (criteria.PersonInCharge.Contains(x.PersonIncharge) || string.IsNullOrEmpty(criteria.PersonInCharge))
                                                                 && (criteria.Creator.Contains(x.UserCreated) || string.IsNullOrEmpty(criteria.Creator));
            if (!hasSalesman)
            {
                queryTrans = queryTrans.And(x => criteria.OfficeId.Contains(x.OfficeId.ToString()) || string.IsNullOrEmpty(criteria.OfficeId)
                                            && criteria.DepartmentId.Contains(x.DepartmentId.ToString()) || string.IsNullOrEmpty(criteria.DepartmentId)
                                            && criteria.GroupId.Contains(x.GroupId.ToString()) || string.IsNullOrEmpty(criteria.GroupId));
            }
            if (criteria.ServiceDateFrom != null && criteria.ServiceDateTo != null)
            {
                queryTrans = queryTrans.And(x => x.TransactionType.Contains("E") ?
                                    (!x.Etd.HasValue || (x.Etd.Value >= criteria.ServiceDateFrom.Value.Date && x.Etd.Value.Date <= criteria.ServiceDateTo.Value.Date))
                                   : (!x.Eta.HasValue || (x.Eta.Value >= criteria.ServiceDateFrom.Value.Date && x.Eta.Value.Date <= criteria.ServiceDateTo.Value.Date))
                                   );
            }
            if (criteria.CreatedDateFrom != null && criteria.CreatedDateTo != null)
            {
                queryTrans = queryTrans.And(x => x.DatetimeCreated.Value.Date >= criteria.CreatedDateFrom.Value.Date && x.DatetimeCreated.Value.Date <= criteria.CreatedDateTo.Value.Date);
            }

            var data = csRepository.Get(queryTrans);
            return data;
        }

        private MonthlySaleReportResult GetContainer(IQueryable<CatUnit> containerData, Guid? mblid, Guid? hblid = null)
        {
            MonthlySaleReportResult report = null;
            IQueryable<CsMawbcontainer> containers = null;
            if (mblid != null)
            {
                containers = containerRepository.Get(x => x.Mblid == mblid);
            }
            else
            {
                containers = containerRepository.Get(x => x.Hblid == hblid);
            }

            if (containers != null)
            {
                report = new MonthlySaleReportResult();
                var conts = containers.Join(containerData, x => x.ContainerTypeId, y => y.Id, (x, y) => new { x, y.Code });
                report.Cont40HC = (decimal)conts.Where(x => x.Code.Contains("HQ")).Sum(x => x.x.Quantity);
                report.Qty20 = (decimal)conts.Where(x => x.Code.Contains("20") && !x.Code.Contains("HQ")).Sum(x => x.x.Quantity);
                report.Qty40 = (decimal)conts.Where(x => x.Code.Contains("40") && !x.Code.Contains("HQ")).Sum(x => x.x.Quantity);
            }
            return report;
        }

        private decimal GetBuyingRate(Guid hblid, string toCurrency)
        {
            decimal cost = 0;
            var buyingCharges = surchargeRepository.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE
                                                          && x.Hblid == hblid
                                                          && (x.KickBack == false || x.KickBack == null));
            if (buyingCharges != null)
            {
                foreach (var charge in buyingCharges)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, toCurrency);//charge.FinalExchangeRate;
                    /*if (rate == null)
                    {
                        rate = GetCurrencyExchangeRate(charge, toCurrency);
                    }*/
                    cost += charge.Quantity * charge.UnitPrice * rate ?? 0; // Phí Selling trước thuế
                }
            }
            return cost;
        }
        private decimal GetShareProfit(Guid hblid, string currencyTo)
        {
            decimal cost = 0;
            var shareProfits = surchargeRepository.Get(x => x.Hblid == hblid && x.KickBack == true);
            if (shareProfits != null)
            {
                foreach (var charge in shareProfits)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, currencyTo);//charge.FinalExchangeRate;
                    /*if (rate == null)
                    {
                        rate = GetCurrencyExchangeRate(charge, currencyTo);
                    }*/
                    cost += charge.Quantity * charge.UnitPrice * rate ?? 0; // Phí Selling trước thuế
                }
            }
            return cost;
        }
        private decimal GetSellingRate(Guid hblid, string toCurrency)
        {
            decimal revenue = 0;
            var sellingCharges = surchargeRepository.Get(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE && x.Hblid == hblid);
            if (sellingCharges != null)
            {
                foreach (var charge in sellingCharges)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, toCurrency);//charge.FinalExchangeRate;
                    /*if (rate == null)
                    {
                        rate = GetCurrencyExchangeRate(charge, toCurrency);
                    }*/
                    revenue += charge.Quantity * charge.UnitPrice * rate ?? 0;
                }
            }
            return revenue;
        }
        private decimal GetChargeFee(Guid hblid, string currency, string type)
        {
            decimal revenue = 0;
            IQueryable<CsShipmentSurcharge> charges = null;
            switch (type)
            {
                case "SELL":
                    charges = surchargeRepository.Get(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE && x.Hblid == hblid);
                    break;
                case "BUY":
                    charges = surchargeRepository.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE
                                                          && x.Hblid == hblid
                                                          && (x.KickBack == false || x.KickBack == null));
                    break;
                case "SHARE":
                    charges = surchargeRepository.Get(x => x.Hblid == hblid && x.KickBack == true);
                    break;
                default:
                    break;
            }
            if (charges != null)
            {
                foreach (var charge in charges)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, currency);//charge.FinalExchangeRate;

                    revenue += charge.Quantity * charge.UnitPrice * rate ?? 0;
                }
            }
            return revenue;
        }
        //private decimal GetCurrencyExchangeRate(CsShipmentSurcharge charge, string toCurrency)
        //{
        //    if (charge.CurrencyId == toCurrency) return 1;
        //    var currencyExchange = exchangeRepository.Get(x => x.DatetimeModified.Value.Date == charge.ExchangeDate.Value.Date
        //                                                    && x.CurrencyFromId == charge.CurrencyId && x.CurrencyToId == toCurrency)?.OrderByDescending(x => x.DatetimeModified)?.FirstOrDefault();
        //    if (currencyExchange != null) return currencyExchange.Rate;
        //    else
        //    {
        //        currencyExchange = exchangeRepository.Get(x => x.DatetimeModified.Value.Date == charge.ExchangeDate.Value.Date
        //                                                    && x.CurrencyFromId == charge.CurrencyId && x.CurrencyToId == toCurrency)?.OrderByDescending(x => x.DatetimeModified)?.FirstOrDefault();
        //        if (currencyExchange != null)
        //        {
        //            return (1 / currencyExchange.Rate);
        //        }
        //        return 0;
        //    }
        //}

        public Crystal PreviewGetMonthlySaleReport(SaleReportCriteria criteria)
        {
            var data = GetMonthlySaleReport(criteria);
            var listEmployee = employeeRepository.Get();
            var lookupEmployee = listEmployee.ToLookup(q => q.Id);
            var listUser = userRepository.Get();
            var lookupUser = listUser.ToLookup(q => q.Id);
            Crystal result = new Crystal();
            if (data == null) result = null;
            else
            {
                var list = data.ToList();
                var employeeId = lookupUser[currentUser.UserID].FirstOrDefault()?.EmployeeId;
                string employeeContact = currentUser.UserName;
                if (employeeId == null)
                {
                    var employee = lookupEmployee[employeeId].FirstOrDefault();
                    employeeContact = employee != null ? employee.EmployeeNameVn ?? string.Empty : string.Empty;
                }
                var company = companyRepository.Get(x => x.Id == currentUser.CompanyID).FirstOrDefault();
                DateTime? dateFrom = null;
                DateTime? dateTo = null;
                if (criteria.CreatedDateFrom != null && criteria.CreatedDateTo != null)
                {
                    dateFrom = criteria.CreatedDateFrom;
                    dateTo = criteria.CreatedDateTo;
                }
                else if (criteria.ServiceDateFrom != null && criteria.ServiceDateTo != null)
                {
                    dateFrom = criteria.ServiceDateFrom;
                    dateTo = criteria.ServiceDateTo;
                }
                var parameter = new MonthlySaleReportParameter
                {
                    FromDate = (DateTime)dateFrom,
                    ToDate = (DateTime)dateTo,
                    Contact = employeeContact,
                    CompanyName = company != null ? (company.BunameVn ?? string.Empty) : string.Empty,
                    CompanyAddress1 = company != null ? (company.AddressVn ?? string.Empty) : string.Empty,
                    CurrDecimalNo = 2,
                    ReportBy = employeeContact,
                    Director = string.Empty,
                    ChiefAccountant = string.Empty,
                };
                result = new Crystal
                {
                    ReportName = "Monthly Sale Report.rpt",
                    AllowPrint = true,
                    AllowExport = true,
                    IsLandscape = true
                };
                result.AddDataSource(list);
                result.FormatType = ExportFormatType.PortableDocFormat;
                result.SetParameter(parameter);
            }
            return result;
        }

        private List<string> GetDeptManager(Guid? companyId, Guid? officeId, int? departmentId)
        {
            var managers = userLevelRepository.Get(x => x.GroupId == 11
                                                    && x.Position == "Manager-Leader"
                                                    && x.DepartmentId == departmentId
                                                    && x.DepartmentId != null
                                                    && x.OfficeId == officeId
                                                    && x.CompanyId == companyId).Select(s => s.UserId).ToList();
            return managers;
        }

        private List<string> GetAccoutantManager(Guid? companyId, Guid? officeId)
        {
            var deptAccountants = departmentRepository.Get(s => s.DeptType == "ACCOUNTANT").Select(s => s.Id).ToList();
            var accountants = userLevelRepository.Get(x => x.GroupId == 11
                                                    && x.OfficeId == officeId
                                                    && x.DepartmentId != null
                                                    && x.CompanyId == companyId
                                                    && x.Position == "Manager-Leader")
                                                    .Where(x => deptAccountants.Contains(x.DepartmentId.Value))
                                                    .Select(s => s.UserId).ToList();
            return accountants;
        }

        private List<string> GetOfficeManager(Guid? companyId, Guid? officeId)
        {
            var officeManager = userLevelRepository.Get(x => x.GroupId == 11
                                                    && x.DepartmentId == null
                                                    && x.OfficeId == officeId
                                                    && x.CompanyId == companyId
                                                    && x.Position == "Manager-Leader")
                                                    .Select(s => s.UserId).ToList();
            return officeManager;
        }

        private List<string> GetCompanyManager(Guid? companyId)
        {
            var companyManager = userLevelRepository.Get(x => x.GroupId == 11
                                                    && x.DepartmentId == null
                                                    && x.OfficeId == null
                                                    && x.CompanyId == companyId
                                                    && x.Position == "Manager-Leader")
                                                    .Select(s => s.UserId).ToList();
            return companyManager;
        }

        #region -- SALE REPORT BY QUATER --    
        // comment improve
        //private IQueryable<QuaterSaleReportResult> GetQuaterOpsSaleReport(SaleReportCriteria criteria)
        //{
        //    List<QuaterSaleReportResult> results = null;
        //    IQueryable<OpsTransaction> data = QueryOpsSaleReport(criteria);
        //    if (data == null) return null;
        //    results = new List<QuaterSaleReportResult>();
        //    var hasSalesman = criteria.SalesMan != null; // Check if Type = Salesman
        //    foreach (var item in data)
        //    {
        //        string employeeId = userRepository.Get(x => x.Id == item.SalemanId).FirstOrDefault()?.EmployeeId;
        //        string _contactName = string.Empty;
        //        if (employeeId != null)
        //        {
        //            _contactName = employeeRepository.Get(x => x.Id == employeeId).FirstOrDefault()?.EmployeeNameVn;
        //        }

        //        string _deptManagerSale = GetDeptManager(item.CompanyId, item.OfficeId, item.DepartmentId).FirstOrDefault();
        //        string _employeeIdDeptManagerSale = userRepository.Get(x => x.Id == _deptManagerSale).FirstOrDefault()?.EmployeeId;
        //        string _saleManager = string.Empty;
        //        if (_employeeIdDeptManagerSale != null)
        //        {
        //            _saleManager = employeeRepository.Get(x => x.Id == _employeeIdDeptManagerSale).FirstOrDefault()?.EmployeeNameVn;
        //        }

        //        #region -- Tổng amount trước thuế selling của HBL --               
        //        decimal _sellingRate = 0;
        //        var _chargeSell = surchargeRepository.Get(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE && x.Hblid == item.Hblid);
        //        foreach (var charge in _chargeSell)
        //        {
        //            //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
        //            var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
        //            _sellingRate += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
        //        }
        //        #endregion -- Tổng amount trước thuế selling của HBL --

        //        #region -- Tổng amount trước thuế Buying của HBL (ko lấy phí có tick KB) --
        //        decimal _buyingRate = 0;
        //        var _chargeBuy = surchargeRepository.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE
        //                                                    && x.Hblid == item.Hblid
        //                                                    && (x.KickBack == false || x.KickBack == null));
        //        foreach (var charge in _chargeBuy)
        //        {
        //            //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
        //            var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
        //            _buyingRate += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
        //        }
        //        #endregion -- Tổng amount trước thuế Buying của HBL (ko lấy phí có tick KB) --

        //        #region -- Tổng amount trước thuế Buying của HBL (lấy phí có tick Kick Back) --
        //        decimal _sharedProfit = 0;
        //        var _chargeBuyKB = surchargeRepository.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE
        //                                                    && x.Hblid == item.Hblid
        //                                                    && x.KickBack == true);
        //        foreach (var charge in _chargeBuyKB)
        //        {
        //            //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
        //            var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
        //            _sharedProfit += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
        //        }
        //        #endregion -- Tổng amount trước thuế Buying của HBL (lấy phí có tick Kick Back) --
        //        string Depart = hasSalesman == true ? item.SalesDepartmentId : item.DepartmentId.ToString();
        //        var report = new QuaterSaleReportResult
        //        {
        //            Department = departmentRepository.Get(x => Depart.Contains(x.Id.ToString())).FirstOrDefault()?.DeptNameEn, // Department của Sale lô hàng đó
        //            ContactName = _contactName, // Sale Full Name
        //            SalesManager = _saleManager, // Sale Manager
        //            PartnerName = string.Empty,
        //            Description = string.Empty,
        //            Area = string.Empty,
        //            POL = string.Empty,
        //            POD = string.Empty,
        //            Lines = string.Empty,
        //            Agent = string.Empty,
        //            NominationParty = string.Empty,
        //            assigned = false,
        //            TransID = string.Empty,
        //            LoadingDate = item.ServiceDate,
        //            HWBNO = string.Empty,
        //            Volumne = string.Empty,
        //            Qty20 = 0,
        //            Qty40 = 0,
        //            Cont40HC = 0,
        //            KGS = 0,
        //            CBM = 0,
        //            SellingRate = _sellingRate + _decimalNumber, // Total Amount Selling
        //            SharedProfit = _sharedProfit + _decimalNumber, // Total Amount Buying có Check Kick Back
        //            BuyingRate = _buyingRate + _decimalNumber, // Total Amount Buying ko có Check Kick Back
        //            OtherCharges = 0, // Default bằng 0
        //            SalesTarget = 0, // Default bằng 0
        //            Bonus = 0, // Default bằng 0
        //            DptSalesTarget = 0, // Default bằng 0
        //            DptBonus = 0, // Default bằng 0
        //            KeyContact = string.Empty,
        //            MBLNO = string.Empty,
        //            Vessel = string.Empty,
        //            TpyeofService = string.Empty,
        //        };

        //        results.Add(report);
        //    }
        //    return results.AsQueryable();
        //}

        private IQueryable<QuaterSaleReportResult> GetDataQuaterCSSaleReport(SaleReportCriteria criteria)
        {
            var data = GetDataSaleReport(criteria);
            if (data == null) return null;
            var listEmployee = employeeRepository.Get();
            var lookupEmployee = listEmployee.ToLookup(q => q.Id);
            var listUser = userRepository.Get();
            var lookupUser = listUser.ToLookup(q => q.Id);
            var listCharges = surchargeRepository.Get();
            var lookupSellingCharges = listCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE).ToLookup(q => q.Hblid);
            var lookupBuying = listCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && (x.KickBack == false || x.KickBack == null)).ToLookup(q => q.Hblid);
            var lookupBuyKB = listCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && x.KickBack == true).ToLookup(q => q.Hblid);
            bool hasSalesman = criteria.SalesMan != null;
            var results = new List<QuaterSaleReportResult>();
            foreach (var item in data)
            {
                string employeeId = lookupUser[item.SalemanId].FirstOrDefault()?.EmployeeId;
                string _contactName = string.Empty;
                if (employeeId != null)
                {
                    _contactName = lookupEmployee[employeeId].FirstOrDefault()?.EmployeeNameVn;
                }

                string _employeeIdDeptManagerSale = lookupUser[item.DepartSaleManager].FirstOrDefault()?.EmployeeId;
                string _saleManager = string.Empty;
                if (_employeeIdDeptManagerSale != null)
                {
                    _saleManager = lookupEmployee[_employeeIdDeptManagerSale].FirstOrDefault()?.EmployeeNameVn;
                }

                #region -- Tổng amount trước thuế selling của HBL --               
                decimal _sellingRate = 0;
                _sellingRate = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? (decimal)lookupSellingCharges[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupSellingCharges[item.HblId].Sum(x => x.AmountUsd ?? 0);
                #endregion -- Tổng amount trước thuế selling của HBL --

                #region -- Tổng amount trước thuế Buying của HBL (ko lấy phí có tick KB) --
                decimal _buyingRate = 0;
                _buyingRate = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? (decimal)lookupBuying[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupBuying[item.HblId].Sum(x => x.AmountUsd ?? 0);

                #endregion -- Tổng amount trước thuế Buying của HBL (ko lấy phí có tick KB) --

                #region -- Tổng amount trước thuế Buying của HBL (lấy phí có tick Kick Back) --
                decimal _sharedProfit = 0;
                _sharedProfit = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? (decimal)lookupBuyKB[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupBuyKB[item.HblId].Sum(x => x.AmountUsd ?? 0 );
                #endregion -- Tổng amount trước thuế Buying của HBL (lấy phí có tick Kick Back) --
                string Depart = hasSalesman == true ? item.SalesDepartmentId : item.DepartmentId.ToString();
                var report = new QuaterSaleReportResult
                {
                    Department = item.DepartmentSale,
                    ContactName = _contactName, // Sale Full Name
                    SalesManager = _saleManager, // Sale Manager
                    PartnerName = string.Empty,
                    Description = string.Empty,
                    Area = string.Empty,
                    POL = string.Empty,
                    POD = string.Empty,
                    Lines = string.Empty,
                    Agent = string.Empty,
                    NominationParty = string.Empty,
                    assigned = false,
                    TransID = string.Empty,
                    LoadingDate = item.TransactionType == "CL" ? item.Etd : ( item.TransactionType.Contains("I") ? item.Eta : item.Etd),
                    HWBNO = string.Empty,
                    Volumne = string.Empty,
                    Qty20 = 0,
                    Qty40 = 0,
                    Cont40HC = 0,
                    KGS = 0,
                    CBM = 0,
                    SellingRate = _sellingRate + _decimalNumber, // Total Amount Selling
                    SharedProfit = _sharedProfit + _decimalNumber, // Total Amount Buying có Check Kick Back
                    BuyingRate = _buyingRate + _decimalNumber, // Total Amount Buying ko có Check Kick Back
                    OtherCharges = 0, // Default bằng 0
                    SalesTarget = 0, // Default bằng 0
                    Bonus = 0, // Default bằng 0
                    DptSalesTarget = 0, // Default bằng 0
                    DptBonus = 0, // Default bằng 0
                    KeyContact = string.Empty,
                    MBLNO = string.Empty,
                    Vessel = string.Empty,
                    TpyeofService = string.Empty,
                };

                results.Add(report);
            }
            return results.AsQueryable();
        }
        // Comment improve
        //private IQueryable<QuaterSaleReportResult> GetQuaterCSSaleReport(SaleReportCriteria criteria)
        //{
        //    IQueryable<CsTransaction> shipments = QueryCsTransaction(criteria);
        //    if (shipments == null) return null;
        //    IQueryable<CsTransactionDetail> housebills = QueryHouseBills(criteria);
        //    var hasSalesman = criteria.SalesMan != null; // Check if Type = Salesman
        //    var data = (from shipment in shipments
        //                join housebill in housebills on shipment.Id equals housebill.JobId
        //                select new
        //                {
        //                    shipment.DepartmentId,
        //                    shipment.TransactionType,
        //                    shipment.JobNo,
        //                    shipment.ShipmentType,
        //                    shipment.Pol,
        //                    shipment.Pod,
        //                    shipment.ColoaderId,
        //                    shipment.AgentId,
        //                    housebill.CustomerId,
        //                    housebill.NotifyPartyDescription,
        //                    HBLID = housebill.Id,
        //                    housebill.Hwbno,
        //                    housebill.NetWeight,
        //                    housebill.Cbm,
        //                    housebill.ShipperDescription,
        //                    housebill.ConsigneeDescription,
        //                    housebill.SaleManId,
        //                    shipment.Eta,
        //                    shipment.Etd,
        //                    housebill.OfficeId,
        //                    housebill.CompanyId,
        //                    DepartmentIdHbl = housebill.DepartmentId,
        //                    housebill.SalesDepartmentId
        //                });
        //    if (data == null) return null;
        //    var results = new List<QuaterSaleReportResult>();
        //    foreach (var item in data)
        //    {
        //        string employeeId = userRepository.Get(x => x.Id == item.SaleManId).FirstOrDefault()?.EmployeeId;
        //        string _contactName = string.Empty;
        //        if (employeeId != null)
        //        {
        //            _contactName = employeeRepository.Get(x => x.Id == employeeId).FirstOrDefault()?.EmployeeNameVn;
        //        }

        //        string _deptManagerSale = GetDeptManager(item.CompanyId, item.OfficeId, item.DepartmentIdHbl).FirstOrDefault();
        //        string _employeeIdDeptManagerSale = userRepository.Get(x => x.Id == _deptManagerSale).FirstOrDefault()?.EmployeeId;
        //        string _saleManager = string.Empty;
        //        if (_employeeIdDeptManagerSale != null)
        //        {
        //            _saleManager = employeeRepository.Get(x => x.Id == _employeeIdDeptManagerSale).FirstOrDefault()?.EmployeeNameVn;
        //        }

        //        #region -- Tổng amount trước thuế selling của HBL --               
        //        decimal _sellingRate = 0;
        //        var _chargeSell = surchargeRepository.Get(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE && x.Hblid == item.HBLID);
        //        foreach (var charge in _chargeSell)
        //        {
        //            //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
        //            var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
        //            _sellingRate += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
        //        }
        //        #endregion -- Tổng amount trước thuế selling của HBL --

        //        #region -- Tổng amount trước thuế Buying của HBL (ko lấy phí có tick KB) --
        //        decimal _buyingRate = 0;
        //        var _chargeBuy = surchargeRepository.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE
        //                                                    && x.Hblid == item.HBLID
        //                                                    && (x.KickBack == false || x.KickBack == null));
        //        foreach (var charge in _chargeBuy)
        //        {
        //            //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
        //            var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
        //            _buyingRate += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
        //        }
        //        #endregion -- Tổng amount trước thuế Buying của HBL (ko lấy phí có tick KB) --

        //        #region -- Tổng amount trước thuế Buying của HBL (lấy phí có tick Kick Back) --
        //        decimal _sharedProfit = 0;
        //        var _chargeBuyKB = surchargeRepository.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE
        //                                                    && x.Hblid == item.HBLID
        //                                                    && x.KickBack == true);
        //        foreach (var charge in _chargeBuyKB)
        //        {
        //            //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
        //            var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
        //            _sharedProfit += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
        //        }
        //        #endregion -- Tổng amount trước thuế Buying của HBL (lấy phí có tick Kick Back) --
        //        string Depart = hasSalesman == true ? item.SalesDepartmentId : item.DepartmentId.ToString();
        //        var report = new QuaterSaleReportResult
        //        {
        //            Department = departmentRepository.Get(x => Depart.Contains(x.Id.ToString())).FirstOrDefault()?.DeptNameEn, // Department của Sale lô hàng đó
        //            ContactName = _contactName, // Sale Full Name
        //            SalesManager = _saleManager, // Sale Manager
        //            PartnerName = string.Empty,
        //            Description = string.Empty,
        //            Area = string.Empty,
        //            POL = string.Empty,
        //            POD = string.Empty,
        //            Lines = string.Empty,
        //            Agent = string.Empty,
        //            NominationParty = string.Empty,
        //            assigned = false,
        //            TransID = string.Empty,
        //            LoadingDate = item.TransactionType.Contains("I") ? item.Eta : item.Etd,
        //            HWBNO = string.Empty,
        //            Volumne = string.Empty,
        //            Qty20 = 0,
        //            Qty40 = 0,
        //            Cont40HC = 0,
        //            KGS = 0,
        //            CBM = 0,
        //            SellingRate = _sellingRate + _decimalNumber, // Total Amount Selling
        //            SharedProfit = _sharedProfit + _decimalNumber, // Total Amount Buying có Check Kick Back
        //            BuyingRate = _buyingRate + _decimalNumber, // Total Amount Buying ko có Check Kick Back
        //            OtherCharges = 0, // Default bằng 0
        //            SalesTarget = 0, // Default bằng 0
        //            Bonus = 0, // Default bằng 0
        //            DptSalesTarget = 0, // Default bằng 0
        //            DptBonus = 0, // Default bằng 0
        //            KeyContact = string.Empty,
        //            MBLNO = string.Empty,
        //            Vessel = string.Empty,
        //            TpyeofService = string.Empty,
        //        };

        //        results.Add(report);
        //    }
        //    return results.AsQueryable();
        //}

        #endregion -- SALE REPORT BY QUATER -- 

        public Crystal PreviewGetDepartSaleReport(SaleReportCriteria criteria)
        {
            var data = GetQuaterSaleReport(criteria);
            Crystal result = null;

            DateTime _fromDate, _toDate = DateTime.Now;
            if (criteria.CreatedDateFrom != null && criteria.CreatedDateTo != null)
            {
                _fromDate = criteria.CreatedDateFrom.Value;
                _toDate = criteria.CreatedDateTo.Value;
            }
            else
            {
                var list = data.ToList();
                _fromDate = criteria.ServiceDateFrom.Value;
                _toDate = criteria.ServiceDateTo.Value;

                var _officeCurrentUser = officeRepository.Get(x => x.Id == currentUser.OfficeID).FirstOrDefault();
                string _officeNameEn = _officeCurrentUser?.BranchNameEn ?? string.Empty;
                string _addressOffice = _officeCurrentUser?.AddressEn ?? string.Empty;

                string _userIdAccountant = GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault();
                var _employeeIdAcountant = userRepository.Get(x => x.Id == _userIdAccountant).FirstOrDefault()?.EmployeeId;
                string _accountantName = employeeRepository.Get(x => x.Id == _employeeIdAcountant).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;

                string _managerName = string.Empty;
                string _userIdOfficeManager = GetOfficeManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault();
                var _employeeIdOfficeManager = userRepository.Get(x => x.Id == _userIdOfficeManager).FirstOrDefault()?.EmployeeId;
                _managerName = employeeRepository.Get(x => x.Id == _employeeIdOfficeManager).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;
                if (string.IsNullOrEmpty(_managerName))
                {
                    string _userIdComManager = GetCompanyManager(currentUser.CompanyID).FirstOrDefault();
                    var _employeeIdComManager = userRepository.Get(x => x.Id == _userIdComManager).FirstOrDefault()?.EmployeeId;
                    _managerName = employeeRepository.Get(x => x.Id == _employeeIdComManager).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;
                }

                var parameter = new DepartSaleReportParameter
                {
                    FromDate = _fromDate, //
                    ToDate = _toDate, //
                    Contact = currentUser.UserName, // Current User Name
                    CompanyName = _officeNameEn, // Office Name En của Current User
                    CompanyDescription = string.Empty,
                    CompanyAddress1 = _addressOffice, // Address En của Current User
                    CompanyAddress2 = string.Empty,
                    Website = string.Empty,
                    CurrDecimalNo = 2, //
                    ReportBy = string.Empty,
                    SalesManager = string.Empty,
                    Director = _managerName, // Office/Company Manager
                    ChiefAccountant = _accountantName // Accountant Manager của Current User
                };
                result = new Crystal
                {
                    ReportName = "SalesReportByDepartment.rpt",
                    AllowPrint = true,
                    AllowExport = true
                };
                result.AddDataSource(list);
                result.FormatType = ExportFormatType.PortableDocFormat;
                result.SetParameter(parameter);
            }

            return result;
        }

        private IQueryable<QuaterSaleReportResult> GetQuaterSaleReport(SaleReportCriteria criteria)
        {
            IQueryable<QuaterSaleReportResult> csShipments = null;
            csShipments = GetDataQuaterCSSaleReport(criteria);
            return csShipments;
        }

        public Crystal PreviewGetQuaterSaleReport(SaleReportCriteria criteria)
        {
            var data = GetQuaterSaleReport(criteria);
            Crystal result = null;

            if (data == null)
            {
                result = null;
            }
            else
            {
                var list = data.ToList();

                DateTime _fromDate, _toDate = DateTime.Now;
                if (criteria.CreatedDateFrom != null && criteria.CreatedDateTo != null)
                {
                    _fromDate = criteria.CreatedDateFrom.Value;
                    _toDate = criteria.CreatedDateTo.Value;
                }
                else
                {
                    _fromDate = criteria.ServiceDateFrom.Value;
                    _toDate = criteria.ServiceDateTo.Value;
                }

                var _officeCurrentUser = officeRepository.Get(x => x.Id == currentUser.OfficeID).FirstOrDefault();
                string _officeNameEn = _officeCurrentUser?.BranchNameEn ?? string.Empty;
                string _addressOffice = _officeCurrentUser?.AddressEn ?? string.Empty;

                string _userIdAccountant = GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault();
                var _employeeIdAcountant = userRepository.Get(x => x.Id == _userIdAccountant).FirstOrDefault()?.EmployeeId;
                string _accountantName = employeeRepository.Get(x => x.Id == _employeeIdAcountant).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;

                string _managerName = string.Empty;
                string _userIdOfficeManager = GetOfficeManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault();
                var _employeeIdOfficeManager = userRepository.Get(x => x.Id == _userIdOfficeManager).FirstOrDefault()?.EmployeeId;
                _managerName = employeeRepository.Get(x => x.Id == _employeeIdOfficeManager).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;
                if (string.IsNullOrEmpty(_managerName))
                {
                    string _userIdComManager = GetCompanyManager(currentUser.CompanyID).FirstOrDefault();
                    var _employeeIdComManager = userRepository.Get(x => x.Id == _userIdComManager).FirstOrDefault()?.EmployeeId;
                    _managerName = employeeRepository.Get(x => x.Id == _employeeIdComManager).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;
                }

                var parameter = new QuaterSaleReportParameter
                {
                    FromDate = _fromDate, //
                    ToDate = _toDate, //
                    Contact = currentUser.UserName, // Current User Name
                    CompanyName = _officeNameEn, // Office Name En của Current User
                    CompanyDescription = string.Empty,
                    CompanyAddress1 = _addressOffice, // Address En của Current User
                    CompanyAddress2 = string.Empty,
                    Website = string.Empty,
                    CurrDecimalNo = 2, //
                    ReportBy = string.Empty,
                    SalesManager = string.Empty,
                    Director = _managerName, // Office/Company Manager
                    ChiefAccountant = _accountantName // Accountant Manager của Current User
                };
                result = new Crystal
                {
                    ReportName = "SalesReportByQuater.rpt",
                    AllowPrint = true,
                    AllowExport = true
                };
                result.AddDataSource(list);
                result.FormatType = ExportFormatType.PortableDocFormat;
                result.SetParameter(parameter);
            }
            return result;
        }

        #region -- SALE REPORT SUMMARY --        

        private IQueryable<SummarySaleReportResult> GetSummarySaleReport(SaleReportCriteria criteria)
        {

            IQueryable<SummarySaleReportResult> csShipments = null;
            csShipments = GetDataCsShipmentReport(criteria);
            return csShipments;
        }

        private IQueryable<SummarySaleReportResult> GetDataCsShipmentReport(SaleReportCriteria criteria)
        {
            var data = GetDataSaleReport(criteria);
            if (data == null) return null;
            var listEmployee = employeeRepository.Get();
            var lookupEmployee = listEmployee.ToLookup(q => q.Id);
            var listUser = userRepository.Get();
            var lookupUser = listUser.ToLookup(q => q.Id);
            var listCharges = surchargeRepository.Get();
            var lookupSellingCharges = listCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE).ToLookup(q => q.Hblid);
            var lookupBuying = listCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && (x.KickBack == false || x.KickBack == null)).ToLookup(q => q.Hblid);
            var lookupShareProfit= listCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && x.KickBack == true).ToLookup(q => q.Hblid);
            var listPartner = catPartnerRepository.Get();
            var partnerLookup = listPartner.ToLookup(q => q.Id);
            bool hasSalesman = criteria.SalesMan != null;
            var results = new List<SummarySaleReportResult>();
            foreach (var item in data)
            {
                var report = new SummarySaleReportResult
                {
                    Department = item.DepartmentSale,
                    PartnerName = partnerLookup[item.CustomerId].FirstOrDefault()?.PartnerNameEn,
                    Description = GetShipmentTypeForPreviewPL(item.TransactionType),
                    Assigned = item.ShipmentType == "Nominated" ? true : false,
                    TransID = item.JobNo,
                    KGS = (item.NetWeight ?? 0) + _decimalNumber,
                    CBM = (item.Cbm ?? 0) + _decimalNumber,
                    SharedProfit = 0,
                    OtherCharges = 0,
                    TpyeofService = item.TypeOfService
                };
                string employeeId = lookupUser[item.SalemanId].FirstOrDefault()?.EmployeeId;
                if (employeeId != null)
                {
                    report.ContactName = lookupEmployee[employeeId].FirstOrDefault()?.EmployeeNameVn;
                }
                // Selling
                report.SellingRate = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? (decimal)lookupSellingCharges[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupSellingCharges[item.HblId].Sum(x => x.AmountUsd ?? 0) + _decimalNumber;//GetSellingRate(item.HblId, criteria.Currency) + _decimalNumber; //Cộng thêm phần thập phân
                //Buying without kickBack.
                report.BuyingRate = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? (decimal)lookupBuying[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupBuying[item.HblId].Sum(x => x.AmountUsd ?? 0) + _decimalNumber; //Cộng thêm phần thập phân
                //KickBack
                report.SharedProfit = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? (decimal)lookupShareProfit[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupShareProfit[item.HblId].Sum(x => x.AmountUsd ?? 0) + _decimalNumber; //Cộng thêm phần thập phân

                report.Cont40HC = item.Cont40HC != null ?(decimal)item.Cont40HC + _decimalNumber : 0 ;
                report.Qty20 = item.Qty20 != null ? (decimal)item.Qty20 + _decimalNumber : 0;
                report.Qty40 = item.Qty40 != null ? (decimal)item.Qty40 + _decimalNumber : 0;
                results.Add(report);
            }
            return results.AsQueryable();
        }

        //private IQueryable<SummarySaleReportResult> GetCsShipmentReport(SaleReportCriteria criteria)
        //{
        //    IQueryable<CsTransaction> shipments = QueryCsTransaction(criteria);
        //    if (shipments == null) return null;
        //    IQueryable<CsTransactionDetail> housebills = QueryHouseBills(criteria);

        //    var data = (from shipment in shipments
        //                join housebill in housebills on shipment.Id equals housebill.JobId
        //                select new
        //                {
        //                    shipment.DepartmentId,
        //                    shipment.TransactionType,
        //                    shipment.JobNo,
        //                    shipment.ShipmentType,
        //                    housebill.CustomerId,
        //                    HBLID = housebill.Id,
        //                    housebill.NetWeight,
        //                    housebill.Cbm,
        //                    housebill.SaleManId,
        //                    shipment.TypeOfService

        //                });
        //    if (data == null) return null;

        //    var results = new List<SummarySaleReportResult>();
        //    var containerData = uniRepository.Get(x => x.UnitType == "Container");
        //    foreach (var item in data)
        //    {
        //        var report = new SummarySaleReportResult
        //        {
        //            Department = departmentRepository.Get(x => x.Id == item.DepartmentId).FirstOrDefault()?.DeptNameEn,

        //            PartnerName = catPartnerRepository.Get(x => x.Id == item.CustomerId).FirstOrDefault()?.PartnerNameEn,
        //            Description = GetShipmentTypeForPreviewPL(item.TransactionType),
        //            Assigned = item.ShipmentType == "Nominated" ? true : false,
        //            TransID = item.JobNo,
        //            KGS = (item.NetWeight ?? 0) + _decimalNumber,
        //            CBM = (item.Cbm ?? 0) + _decimalNumber,
        //            SharedProfit = 0,
        //            OtherCharges = 0,
        //            TpyeofService = item.TypeOfService
        //        };
        //        string employeeId = userRepository.Get(x => x.Id == item.SaleManId).FirstOrDefault()?.EmployeeId;
        //        if (employeeId != null)
        //        {
        //            report.ContactName = employeeRepository.Get(x => x.Id == employeeId).FirstOrDefault()?.EmployeeNameVn;
        //        }
        //        // Selling
        //        report.SellingRate = GetChargeFee(item.HBLID, criteria.Currency, "SELL") + _decimalNumber; //Cộng thêm phần thập phân
        //        //Buying without kickBack.
        //        report.BuyingRate = GetChargeFee(item.HBLID, criteria.Currency, "BUY") + _decimalNumber; //Cộng thêm phần thập phân
        //        //KickBack
        //        report.SharedProfit = GetChargeFee(item.HBLID, criteria.Currency, "SHARE") + _decimalNumber; //Cộng thêm phần thập phân
        //        // Share-Profit (Sử dụng hàm trên 28092020)
        //        // report.SharedProfit = GetShareProfit(item.HBLID, criteria.Currency) + _decimalNumber; //Cộng thêm phần thập phân

        //        var contInfo = GetContainer(containerData, null, item.HBLID);

        //        report.Cont40HC = (decimal)contInfo?.Cont40HC + _decimalNumber;
        //        report.Qty20 = (decimal)contInfo?.Qty20 + _decimalNumber;
        //        report.Qty40 = (decimal)contInfo?.Qty40 + _decimalNumber;
        //        results.Add(report);
        //    }
        //    return results.AsQueryable();
        //}

        //private IQueryable<SummarySaleReportResult> GetOpsShipmentReport(SaleReportCriteria criteria)
        //{
        //    List<SummarySaleReportResult> results = new List<SummarySaleReportResult>();

        //    IQueryable<OpsTransaction> shipment = QueryOpsSaleReport(criteria);
        //    if (shipment == null) return null;

        //    var containerData = uniRepository.Get(x => x.UnitType == "Container");

        //    foreach (var item in shipment)
        //    {
        //        SummarySaleReportResult report = new SummarySaleReportResult
        //        {
        //            Description = "Logistics",
        //            TransID = item.JobNo,
        //            KGS = (item.SumNetWeight ?? 0) + _decimalNumber,
        //            CBM = (item.SumCbm ?? 0) + _decimalNumber,
        //            SharedProfit = 0,
        //            OtherCharges = 0,
        //            TpyeofService = API.Common.Globals.CustomData.Services.FirstOrDefault(c => c.Value == DocumentConstants.LG_SHIPMENT)?.Value,
        //            Assigned = true // TODO
        //        };
        //        string employeeId = userRepository.Get(x => x.Id == item.SalemanId).FirstOrDefault()?.EmployeeId;
        //        if (employeeId != null)
        //        {
        //            report.ContactName = employeeRepository.Get(x => x.Id == employeeId).FirstOrDefault()?.EmployeeNameVn;
        //        }

        //        // Selling
        //        report.SellingRate = GetChargeFee(item.Hblid, criteria.Currency, "SELL") + _decimalNumber; //Cộng thêm phần thập phân
        //        //Buying without kickBack.
        //        report.BuyingRate = GetChargeFee(item.Hblid, criteria.Currency, "BUY") + _decimalNumber; //Cộng thêm phần thập phân
        //        //KickBack
        //        report.SharedProfit = GetChargeFee(item.Hblid, criteria.Currency, "SHARE") + _decimalNumber; //Cộng thêm phần thập phân
        //        // Container
        //        IQueryable<CsMawbcontainer> containers = null;
        //        if (item != null)
        //        {
        //            containers = containerRepository.Get(x => x.Mblid == item.Id);
        //        }
        //        else
        //        {
        //            containers = containerRepository.Get(x => x.Hblid == item.Hblid);
        //        }

        //        if (containers != null)
        //        {
        //            var conts = containers.Join(containerData, x => x.ContainerTypeId, y => y.Id, (x, y) => new { x, y.Code });

        //            report.Cont40HC = (decimal)conts.Where(x => x.Code.ToLower() == "Cont40HC".ToLower()).Sum(x => x.x.Quantity) + _decimalNumber;
        //            report.Qty20 = (decimal)conts.Where(x => x.Code.ToLower() == "Cont20DC".ToLower()).Sum(x => x.x.Quantity) + _decimalNumber;
        //            report.Qty40 = (decimal)conts.Where(x => x.Code.ToLower() == "Cont40DC".ToLower()).Sum(x => x.x.Quantity) + _decimalNumber;
        //        }

        //        results.Add(report);

        //    }

        //    return results.AsQueryable();
        //}

        public Crystal PreviewSummarySaleReport(SaleReportCriteria criteria)
        {
            IQueryable<SummarySaleReportResult> data = GetSummarySaleReport(criteria);

            Crystal crystal = null;

            DateTime _fromDate, _toDate = DateTime.Now;

            if (criteria.CreatedDateFrom != null && criteria.CreatedDateTo != null)
            {
                _fromDate = criteria.CreatedDateFrom.Value;
                _toDate = criteria.CreatedDateTo.Value;
            }
            else
            {
                _fromDate = criteria.ServiceDateFrom.Value;
                _toDate = criteria.ServiceDateTo.Value;

                var dataSource = data.ToList();

                // get current user's office 
                var _officeCurrentUser = officeRepository.Get(x => x.Id == currentUser.OfficeID).FirstOrDefault();
                string _officeNameEn = _officeCurrentUser?.BranchNameEn ?? string.Empty;
                string _addressOffice = _officeCurrentUser?.AddressEn ?? string.Empty;

                // get current user's accountant 
                string _userIdAccountant = GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault();
                var _employeeIdAcountant = userRepository.Get(x => x.Id == _userIdAccountant).FirstOrDefault()?.EmployeeId;
                string _accountantName = employeeRepository.Get(x => x.Id == _employeeIdAcountant).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;

                // get current user's company manager 
                string _userIdComManager = GetCompanyManager(currentUser.CompanyID).FirstOrDefault();
                var _employeeIdComManager = userRepository.Get(x => x.Id == _userIdComManager).FirstOrDefault()?.EmployeeId;
                string _comManagerName = employeeRepository.Get(x => x.Id == _employeeIdAcountant).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;

                var parameter = new SummarySaleReportParams
                {
                    FromDate = _fromDate, //
                    ToDate = _toDate, //
                    Contact = currentUser.UserName, // Current User Name
                    CompanyName = "ITL", //_officeNameEn, // Office Name En của Current User
                    CompanyDescription = "Conpany Description", //string.Empty,
                    CompanyAddress1 = "Company Address", // _addressOffice, // Address En của Current User
                    CompanyAddress2 = string.Empty,
                    Website = "Wesite", //string.Empty,
                    CurrDecimalNo = 2, //
                    ReportBy = string.Empty,
                    SalesManager = string.Empty,
                    Director = "Director",//_comManagerName, // Company Manager
                    ChiefAccountant = "Accountant Name", //_accountantName // Accountant Manager của Current User
                };
                crystal = new Crystal
                {
                    ReportName = "TotalSalesReport.rpt",
                    AllowPrint = true,
                    AllowExport = true
                };
                crystal.AddDataSource(dataSource);
                crystal.FormatType = ExportFormatType.PortableDocFormat;
                crystal.SetParameter(parameter);
            }

            return crystal;
        }

        //private IQueryable<CombinationSaleReportResult> GetOpsShipmentCombinationReport(SaleReportCriteria criteria)
        //{
        //    List<CombinationSaleReportResult> results = new List<CombinationSaleReportResult>();

        //    IQueryable<OpsTransaction> shipment = QueryOpsSaleReport(criteria);
        //    if (shipment == null) return null;

        //    IQueryable<CatUnit> containerData = uniRepository.Get(x => x.UnitType == "Container");

        //    foreach (var item in shipment)
        //    {
        //        CombinationSaleReportResult report = new CombinationSaleReportResult
        //        {
        //            ShipmentSource = string.IsNullOrEmpty(item.ShipmentType) ? null : item.ShipmentType.ToUpper(),
        //            Department = departmentRepository.Get(x => x.Id == item.DepartmentId).FirstOrDefault()?.DeptNameAbbr,
        //            POD = catPlaceRepository.Get(x => x.Id == item.Pod)?.FirstOrDefault()?.Code,
        //            POL = catPlaceRepository.Get(x => x.Id == item.Pol)?.FirstOrDefault()?.Code,
        //            Description = "Logistics",
        //            TransID = item?.JobNo,
        //            KGS = (item.SumNetWeight ?? 0) + _decimalNumber,
        //            CBM = (item.SumCbm ?? 0) + _decimalNumber,
        //            SharedProfit = 0,
        //            OtherCharges = 0,
        //            NominationParty = string.Empty,
        //            Area = string.Empty,
        //            Consignee = item.Consignee,
        //            Lines = catPartnerRepository.Get(x => x.Id == item.SupplierId).FirstOrDefault()?.PartnerNameEn,
        //            Agent = item.AgentId != null ? catPartnerRepository.Get(x => x.Id == item.AgentId).FirstOrDefault()?.PartnerNameEn : string.Empty,
        //            Shipper = item.Shipper,
        //            LoadingDate = item.ServiceDate,
        //            TpyeofService = API.Common.Globals.CustomData.Services.FirstOrDefault(c => c.Value == DocumentConstants.LG_SHIPMENT)?.Value,
        //            Assigned = false,
        //            HAWBNO = item.Hwbno,
        //            PartnerName = catPartnerRepository.Get(x => x.Id == item.CustomerId).FirstOrDefault()?.PartnerNameEn,
        //        };
        //        string employeeId = userRepository.Get(x => x.Id == item.SalemanId).FirstOrDefault()?.EmployeeId;
        //        if (employeeId != null)
        //        {
        //            report.ContactName = employeeRepository.Get(x => x.Id == employeeId).FirstOrDefault()?.EmployeeNameVn;
        //        }

        //        // Selling
        //        report.SellingRate = GetChargeFee(item.Hblid, criteria.Currency, "SELL") + _decimalNumber; //Cộng thêm phần thập phân
        //        //Buying without kickBack.
        //        report.BuyingRate = GetChargeFee(item.Hblid, criteria.Currency, "BUY") + _decimalNumber; //Cộng thêm phần thập phân
        //        //KickBack
        //        report.SharedProfit = GetChargeFee(item.Hblid, criteria.Currency, "SHARE") + _decimalNumber; //Cộng thêm phần thập phân
        //        // Container
        //        IQueryable<CsMawbcontainer> containers = null;
        //        if (item != null)
        //        {
        //            containers = containerRepository.Get(x => x.Mblid == item.Id);
        //        }
        //        else
        //        {
        //            containers = containerRepository.Get(x => x.Hblid == item.Hblid);
        //        }

        //        if (containers != null)
        //        {
        //            var conts = containers.Join(containerData, x => x.ContainerTypeId, y => y.Id, (x, y) => new { x, y.Code });

        //            report.Cont40HC = (decimal)conts.Where(x => x.Code.ToLower() == "Cont40HC".ToLower()).Sum(x => x.x.Quantity) + _decimalNumber;
        //            report.Qty20 = (decimal)conts.Where(x => x.Code.ToLower() == "Cont20DC".ToLower()).Sum(x => x.x.Quantity) + _decimalNumber;
        //            report.Qty40 = (decimal)conts.Where(x => x.Code.ToLower() == "Cont40DC".ToLower()).Sum(x => x.x.Quantity) + _decimalNumber;
        //        }

        //        results.Add(report);

        //    }

        //    return results.AsQueryable();
        //}

        private IQueryable<CombinationSaleReportResult> GetDataCSShipmentCombinationReport(SaleReportCriteria criteria)
        {
            var data = GetDataSaleReport(criteria);
            if (data == null) return null;
            var listEmployee = employeeRepository.Get();
            var lookupEmployee = listEmployee.ToLookup(q => q.Id);
            var listUser = userRepository.Get();
            var lookupUser = listUser.ToLookup(q => q.Id);
            var listCharges = surchargeRepository.Get();
            var lookupSellingCharges = listCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE).ToLookup(q => q.Hblid);
            var lookupBuying = listCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && (x.KickBack == false || x.KickBack == null)).ToLookup(q => q.Hblid);
            var lookupShareProfit = listCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && x.KickBack == true).ToLookup(q => q.Hblid);
            var listPartner = catPartnerRepository.Get();
            var partnerLookup = listPartner.ToLookup(q => q.Id);
            var listPlace = catPlaceRepository.Get();
            var lookupPlace = listPlace.ToLookup(q => q.Id);
            var listDepartment = departmentRepository.Get();
            var lookupDepartment = listDepartment.ToLookup(q => q.Id);
            var results = new List<CombinationSaleReportResult>();
            foreach (var item in data)
            {
                var report = new CombinationSaleReportResult
                {
                    Department = item.DepartmentId != null ? lookupDepartment[(int)item.DepartmentId].FirstOrDefault()?.DeptNameAbbr : string.Empty,
                    POD = item.Pod != null? lookupPlace[(Guid)item.Pod]?.FirstOrDefault()?.Code : string.Empty,
                    POL = item.Pol != null ? lookupPlace[(Guid)item.Pol]?.FirstOrDefault()?.Code : string.Empty,
                    PartnerName = partnerLookup[item.CustomerId].FirstOrDefault()?.PartnerNameEn,
                    Description = GetShipmentTypeForPreviewPL(item.TransactionType),
                    Assigned = item.TransactionType == "CL" ? false : item.ShipmentType == "Nominated" ? true : false,
                    TransID = item.JobNo,
                    KGS = (item.NetWeight ?? 0) + _decimalNumber,
                    CBM = (item.Cbm ?? 0) + _decimalNumber,
                    SharedProfit = 0,
                    OtherCharges = 0,
                    ShipmentSource = string.IsNullOrEmpty(item.ShipmentType) ? null : item.ShipmentType.ToUpper(),
                    Lines =  item.ColoaderId != null ? partnerLookup[item.ColoaderId].FirstOrDefault()?.PartnerNameEn : string.Empty,
                    Agent = item.AgentId != null ? partnerLookup[item.AgentId].FirstOrDefault()?.PartnerNameEn : string.Empty,
                    NominationParty = item.NominationParty ?? string.Empty,
                    HAWBNO = item.HwbNo,
                    TpyeofService = item.TransactionType == "CL" ? API.Common.Globals.CustomData.Services.FirstOrDefault(c => c.Value == DocumentConstants.LG_SHIPMENT)?.Value : item.TypeOfService != null ? (item.TypeOfService.Contains("LCL") ? "LCL" : string.Empty) : string.Empty,//item.ShipmentType.Contains("I") ? "IMP" : "EXP",
                    Shipper = item.ShipperDescription,
                    Consignee = item.ConsigneeDescription,
                    LoadingDate = item.TransactionType == "CL" ? item.Etd : item.TransactionType.Contains("I") ? item.Eta : item.Etd

                };
                string employeeId = lookupUser[item.SalemanId].FirstOrDefault()?.EmployeeId;
                if (employeeId != null)
                {
                    report.ContactName = lookupEmployee[employeeId].FirstOrDefault()?.EmployeeNameVn;
                }
                // Selling
                report.SellingRate = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? (decimal)lookupSellingCharges[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupSellingCharges[item.HblId].Sum(x => x.AmountUsd ?? 0) + _decimalNumber;//GetSellingRate(item.HblId, criteria.Currency) + _decimalNumber; //Cộng thêm phần thập phân
                report.BuyingRate = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? (decimal)lookupBuying[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupBuying[item.HblId].Sum(x => x.AmountUsd ?? 0) + _decimalNumber; //Cộng thêm phần thập phân
                report.SharedProfit = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? (decimal)lookupShareProfit[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupShareProfit[item.HblId].Sum(x => x.AmountUsd) + _decimalNumber; //Cộng thêm phần thập phân

                report.Cont40HC = item.Cont40HC != null ? item.Cont40HC + _decimalNumber : 0;
                report.Qty20 = item.Qty20 != null ? item.Qty20 + _decimalNumber : 0;
                report.Qty40 = item.Qty40 != null ? item.Qty40 + _decimalNumber: 0;
                results.Add(report);
            }
            return results.AsQueryable();
        }

        //private IQueryable<CombinationSaleReportResult> GetCSShipmentCombinationReport(SaleReportCriteria criteria)
        //{
        //    IQueryable<CsTransaction> shipments = QueryCsTransaction(criteria);
        //    if (shipments == null) return null;
        //    IQueryable<CsTransactionDetail> housebills = QueryHouseBills(criteria);

        //    var data = (from shipment in shipments
        //                join housebill in housebills on shipment.Id equals housebill.JobId
        //                select new
        //                {
        //                    shipment.DepartmentId,
        //                    shipment.TransactionType,
        //                    shipment.JobNo,
        //                    shipment.ShipmentType,
        //                    housebill.CustomerId,
        //                    HBLID = housebill.Id,
        //                    housebill.NetWeight,
        //                    housebill.Cbm,
        //                    housebill.SaleManId,
        //                    shipment.TypeOfService,
        //                    shipment.Pod,
        //                    shipment.Pol,
        //                    housebill.ShipperId,
        //                    housebill.ConsigneeId,
        //                    shipment.ColoaderId,
        //                    shipment.AgentId,
        //                    housebill.NotifyPartyDescription,
        //                    housebill.Hwbno,
        //                    housebill.GrossWeight,
        //                    housebill.ShipperDescription,
        //                    housebill.ConsigneeDescription,
        //                    shipment.Eta,
        //                    shipment.Etd,
        //                });
        //    if (data == null) return null;

        //    var results = new List<CombinationSaleReportResult>();
        //    var containerData = uniRepository.Get(x => x.UnitType == "Container");
        //    foreach (var item in data)
        //    {
        //        var report = new CombinationSaleReportResult
        //        {
        //            Department = departmentRepository.Get(x => x.Id == item.DepartmentId).FirstOrDefault()?.DeptNameAbbr,
        //            POD = catPlaceRepository.Get(x => x.Id == item.Pod)?.FirstOrDefault()?.Code,
        //            POL = catPlaceRepository.Get(x => x.Id == item.Pol)?.FirstOrDefault()?.Code,

        //            PartnerName = catPartnerRepository.Get(x => x.Id == item.CustomerId).FirstOrDefault()?.PartnerNameEn,
        //            Description = GetShipmentTypeForPreviewPL(item.TransactionType),
        //            Assigned = item.ShipmentType == "Nominated" ? true : false,
        //            TransID = item.JobNo,
        //            KGS = (item.NetWeight ?? 0) + _decimalNumber,
        //            CBM = (item.Cbm ?? 0) + _decimalNumber,
        //            SharedProfit = 0,
        //            OtherCharges = 0,
        //            ShipmentSource = string.IsNullOrEmpty(item.ShipmentType) ? null : item.ShipmentType.ToUpper(),

        //            Lines = item.ColoaderId != null ? catPartnerRepository.Get(x => x.Id == item.ColoaderId).FirstOrDefault()?.PartnerNameEn : string.Empty,
        //            Agent = item.AgentId != null ? catPartnerRepository.Get(x => x.Id == item.AgentId).FirstOrDefault()?.PartnerNameEn : string.Empty,
        //            NominationParty = item.NotifyPartyDescription ?? string.Empty,
        //            HAWBNO = item.Hwbno,

        //            TpyeofService = item.TypeOfService != null ? (item.TypeOfService.Contains("LCL") ? "LCL" : string.Empty) : string.Empty,//item.ShipmentType.Contains("I") ? "IMP" : "EXP",
        //            Shipper = item.ShipperDescription,
        //            Consignee = item.ConsigneeDescription,
        //            LoadingDate = item.TransactionType.Contains("I") ? item.Eta : item.Etd

        //        };
        //        string employeeId = userRepository.Get(x => x.Id == item.SaleManId).FirstOrDefault()?.EmployeeId;
        //        if (employeeId != null)
        //        {
        //            report.ContactName = employeeRepository.Get(x => x.Id == employeeId).FirstOrDefault()?.EmployeeNameVn;
        //        }
        //        // Selling
        //        report.SellingRate = GetChargeFee(item.HBLID, criteria.Currency, "SELL") + _decimalNumber;
        //        //Buying without kickBack.
        //        report.BuyingRate = GetChargeFee(item.HBLID, criteria.Currency, "BUY") + _decimalNumber;
        //        //KickBack
        //        report.SharedProfit = GetChargeFee(item.HBLID, criteria.Currency, "SHARE") + _decimalNumber;
        //        // Share-Profit (Sử dụng hàm trên)
        //        // report.SharedProfit = GetShareProfit(item.HBLID, criteria.Currency);

        //        var contInfo = GetContainer(containerData, null, item.HBLID);

        //        report.Cont40HC = (decimal)contInfo?.Cont40HC + _decimalNumber;
        //        report.Qty20 = (decimal)contInfo?.Qty20 + _decimalNumber;
        //        report.Qty40 = (decimal)contInfo?.Qty40 + _decimalNumber;
        //        results.Add(report);
        //    }
        //    return results.AsQueryable();
        //}

        private IQueryable<CombinationSaleReportResult> GetCombinationReport(SaleReportCriteria criteria)
        {
            IQueryable<CombinationSaleReportResult> csShipments = null;
            csShipments = GetDataCSShipmentCombinationReport(criteria);
            return csShipments;
        }

        public Crystal PreviewCombinationSaleReport(SaleReportCriteria criteria)
        {

            Crystal crystal = null;
            IQueryable<CombinationSaleReportResult> data = GetCombinationReport(criteria);

            DateTime _fromDate, _toDate = DateTime.Now;

            if (criteria.CreatedDateFrom != null && criteria.CreatedDateTo != null)
            {
                _fromDate = criteria.CreatedDateFrom.Value;
                _toDate = criteria.CreatedDateTo.Value;
            }
            else
            {
                _fromDate = criteria.ServiceDateFrom.Value;
                _toDate = criteria.ServiceDateTo.Value;

                var dataSource = data.ToList();

                // get current user's office 
                var _officeCurrentUser = officeRepository.Get(x => x.Id == currentUser.OfficeID).FirstOrDefault();
                string _officeNameEn = _officeCurrentUser?.BranchNameEn ?? string.Empty;
                string _addressOffice = _officeCurrentUser?.AddressEn ?? string.Empty;

                // get current user's accountant 
                string _userIdAccountant = GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault();
                var _employeeIdAcountant = userRepository.Get(x => x.Id == _userIdAccountant).FirstOrDefault()?.EmployeeId;
                string _accountantName = employeeRepository.Get(x => x.Id == _employeeIdAcountant).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;

                // get current user's company manager 
                string _userIdComManager = GetCompanyManager(currentUser.CompanyID).FirstOrDefault();
                var _employeeIdComManager = userRepository.Get(x => x.Id == _userIdComManager).FirstOrDefault()?.EmployeeId;
                string _comManagerName = employeeRepository.Get(x => x.Id == _employeeIdAcountant).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;

                var parameter = new SummarySaleReportParams
                {
                    FromDate = _fromDate, //
                    ToDate = _toDate, //
                    Contact = currentUser.UserName, // Current User Name
                    CompanyName = "ITL", //_officeNameEn, // Office Name En của Current User
                    CompanyDescription = "Conpany Description", //string.Empty,
                    CompanyAddress1 = "Company Address", // _addressOffice, // Address En của Current User
                    CompanyAddress2 = string.Empty,
                    Website = "Wesite", //string.Empty,
                    CurrDecimalNo = 2, //
                    ReportBy = string.Empty,
                    SalesManager = string.Empty,
                    Director = "Director",//_comManagerName, // Company Manager
                    ChiefAccountant = "Accountant Name", //_accountantName // Accountant Manager của Current User
                };
                crystal = new Crystal
                {
                    ReportName = "CombinationSalesReport.rpt",
                    AllowPrint = true,
                    AllowExport = true
                };
                crystal.AddDataSource(dataSource);
                crystal.FormatType = ExportFormatType.PortableDocFormat;
                crystal.SetParameter(parameter);
            }
            return crystal;

        }


        private IQueryable<SaleKickBackReportResult> GetSaleKickBackReport(SaleReportCriteria criteria)
        {
            IQueryable<SaleKickBackReportResult> csShipments = null;
            csShipments = GetCSKickBackReport(criteria);
            return csShipments;
        }

        //private IQueryable<SaleKickBackReportResult> GetOpsKBReport(SaleReportCriteria criteria)
        //{
        //    List<SaleKickBackReportResult> results = null;
        //    IQueryable<OpsTransaction> dataShipment = QueryOpsSaleReport(criteria);
        //    if (!dataShipment.Any()) return null;
        //    results = new List<SaleKickBackReportResult>();
        //    var detailLookupSur = surchargeRepository.Get().ToLookup(q => q.Hblid);
        //    foreach (var item in dataShipment)
        //    {
        //        if (item.Hblid != null && item.Hblid != Guid.Empty)
        //        {
        //            var comChargeGroup = catChargeGroupRepo.Get(x => x.Name == "Com").FirstOrDefault();
        //            var surcharge = detailLookupSur[item.Hblid].Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && (x.KickBack == true || x.ChargeGroup == comChargeGroup?.Id));
        //            var partner = catPartnerRepository.Get(x => x.Id == item.CustomerId).FirstOrDefault();
        //            foreach (var charge in surcharge)
        //            {
        //                var report = new SaleKickBackReportResult
        //                {
        //                    TransID = item.JobNo,
        //                    HBLID = item.Hblid,
        //                    HAWBNO = item.Hwbno,
        //                    LoadingDate = item.ServiceDate,
        //                    PartnerName = partner?.PartnerNameEn,
        //                    Description = "Logistics",
        //                    Quantity = charge.Quantity,
        //                    Unit = uniRepository.Get(x => x.Id == charge.UnitId).Select(x => x.Code).FirstOrDefault() ?? string.Empty,
        //                    UnitPrice = charge.UnitPrice ?? 0,
        //                    TotalValue = 0,
        //                    Currency = charge.CurrencyId,
        //                    Status = string.Empty,
        //                    // Total Amount phí buying kickback
        //                    UsdExt = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? (NumberHelper.RoundNumber(charge.VatAmountVnd ?? 0, 0) + NumberHelper.RoundNumber(charge.AmountVnd ?? 0, 0))
        //                                                                                   : (NumberHelper.RoundNumber(charge.VatAmountUsd ?? 0, 2) + NumberHelper.RoundNumber(charge.AmountUsd ?? 0, 2)),
        //                    MAWB = item.Mblno ?? string.Empty,
        //                    Shipper = item.Shipper ?? string.Empty,
        //                    Consignee = item.Consignee ?? string.Empty,
        //                    PartnerID = item.CustomerId,
        //                    Category = partner?.PartnerType,
        //                };
        //                // Lấy tất cả Buying charge
        //                var chargeBuy = detailLookupSur[(Guid)item.Hblid].Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE);
        //                report.Costs = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? chargeBuy.Sum(x => x.AmountVnd ?? 0) + _decimalNumber : chargeBuy.Sum(x => x.AmountUsd ?? 0) + _decimalNumber; //Cộng thêm phần thập phân
        //                // Lấy tất cả Selling charge
        //                var chargeSell = detailLookupSur[(Guid)item.Hblid].Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE);
        //                report.Incomes = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? chargeSell.Sum(x => x.AmountVnd ?? 0) + _decimalNumber : chargeSell.Sum(x => x.AmountUsd ?? 0) + _decimalNumber; //Cộng thêm phần thập phân
        //                results.Add(report);
        //            }
        //        }
        //    }
        //    return results.AsQueryable();
        //}

        private IQueryable<SaleKickBackReportResult> GetCSKickBackReport(SaleReportCriteria criteria)
        {
            var data = GetDataSaleReport(criteria);
            if (!data.Any()) return null;
            var results = new List<SaleKickBackReportResult>();
            var comChargeGroup = catChargeGroupRepo.Get(x => x.Name == "Com").FirstOrDefault();
            var detailLookupSur = surchargeRepository.Get().ToLookup(q => q.Hblid);
            var listUnit = uniRepository.Get();
            var unitLookup = listUnit.ToLookup(q => q.Id);
            var listPartner = catPartnerRepository.Get();
            var lookupPartner = listPartner.ToLookup(q => q.Id);
            foreach (var item in data)
            {
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
<<<<<<< HEAD
                    var comChargeGroup = catChargeGroupRepo.Get(x => x.Name == "Com").FirstOrDefault();
                    var surcharge = detailLookupSur[item.HBLID].Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && (x.KickBack == true || x.ChargeGroup == comChargeGroup?.Id));                    
=======
                    var surcharge = detailLookupSur[item.HblId].Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && (x.KickBack == true || x.ChargeGroup == comChargeGroup?.Id));
>>>>>>> origin/uat/improve-sale-report
                    foreach (var charge in surcharge)
                    {
                        var partner = catPartnerRepository.Get(x => x.Id == charge.PaymentObjectId).FirstOrDefault();
                        var report = new SaleKickBackReportResult
                        {
                            TransID = item.JobNo,
                            HBLID = item.HblId,
                            HAWBNO = item.HwbNo,
                            LoadingDate = item.TransactionType == "CL" ? item.Etd : item.TransactionType.Contains("I") ? item.Eta : item.Etd,
                            PartnerName = lookupPartner[item.CustomerId].FirstOrDefault()?.PartnerNameEn,
                            Description = item.TransactionType == "CL" ? "Logistics" : API.Common.Globals.CustomData.Services.FirstOrDefault(x => x.Value == item.TransactionType)?.DisplayName,
                            Quantity = charge.Quantity,
                            Unit = unitLookup[charge.UnitId].Select(t=>t.Code).FirstOrDefault() ?? string.Empty,
                            UnitPrice = charge.UnitPrice ?? 0,
                            TotalValue = 0,
                            Currency = charge.CurrencyId,
                            Status = string.Empty,
                            // Total Amount phí buying kickback
                            UsdExt = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? (NumberHelper.RoundNumber(charge.VatAmountVnd ?? 0, 0) + NumberHelper.RoundNumber(charge.AmountVnd ?? 0, 0))
                                                                                           : (NumberHelper.RoundNumber(charge.VatAmountUsd ?? 0, 2) + NumberHelper.RoundNumber(charge.AmountUsd ?? 0, 2)),
                            MAWB = item.Mawb ?? string.Empty,
                            Shipper = item.ShipperDescription,
                            Consignee = item.ConsigneeDescription,
                            PartnerID = item.CustomerId,
                            Category = lookupPartner[item.CustomerId].FirstOrDefault()?.PartnerType,
                        };
                        // Lấy tất cả Buying charge
                        var chargeBuy = detailLookupSur[(Guid)item.HblId].Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE);
                        report.Costs = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? chargeBuy.Sum(x => x.AmountVnd ?? 0) + _decimalNumber : chargeBuy.Sum(x => x.AmountUsd ?? 0) + _decimalNumber; //Cộng thêm phần thập phân
                        // Lấy tất cả Selling charge
                        var chargeSell = detailLookupSur[(Guid)item.HblId].Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE);
                        report.Incomes = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? chargeSell.Sum(x => x.AmountVnd ?? 0) + _decimalNumber : chargeSell.Sum(x => x.AmountUsd ?? 0) + _decimalNumber; //Cộng thêm phần thập phân

                        results.Add(report);
                    }
                }
            }
            return results.AsQueryable();
        }

        public Crystal PreviewSaleKickBackReport(SaleReportCriteria criteria)
        {

            var data = GetSaleKickBackReport(criteria);
            Crystal result = null;
            if (data != null)
            {
                var list = data.OrderBy(x => x.PartnerName).ToList();
                var total = list.GroupBy(x => new { x.TransID, x.HBLID }).Select(p => new { incomes = p.Select(q => q.Incomes).FirstOrDefault(), costs = p.Select(q => q.Costs).FirstOrDefault(), profit = p.Select(q => q.Incomes - q.Costs).FirstOrDefault() }).ToList();
                var employeeId = userRepository.Get(x => x.Id == currentUser.UserID).FirstOrDefault()?.EmployeeId;
                string employeeContact = currentUser.UserName;
                if (employeeId == null)
                {
                    var employee = employeeRepository.Get(x => x.Id == employeeId).FirstOrDefault();
                    employeeContact = employee != null ? employee.EmployeeNameVn ?? string.Empty : string.Empty;
                }
                var company = companyRepository.Get(x => x.Id == currentUser.CompanyID).FirstOrDefault();
                var parameter = new SaleKickBackReportParameter
                {
                    CompanyName = company != null ? (company.BunameVn ?? string.Empty) : string.Empty,
                    CompanyDescription = string.Empty,
                    CompanyAddress1 = string.Empty,
                    CompanyAddress2 = string.Empty,
                    Website = string.Empty,
                    Contact = employeeContact,
                    Currency = criteria.Currency,
                    TotalIncomes = total.Sum(x => x.incomes),
                    TotalCosts = total.Sum(x => x.costs),
                    TotalProfit = total.Sum(x => Math.Abs(x.profit))
                };
                result = new Crystal
                {
                    ReportName = "SaleKickBackReport.rpt",
                    AllowPrint = true,
                    AllowExport = true,
                    IsLandscape = true
                };
                result.AddDataSource(list);
                result.FormatType = ExportFormatType.PortableDocFormat;
                result.SetParameter(parameter);
            }
            return result;
        }

        #endregion -- SALE REPORT SUMMARY --        
        public string GetShipmentTypeForPreviewPL(string transactionType)
        {
            string shipmentType = string.Empty;
            if (transactionType == TermData.InlandTrucking)
            {
                shipmentType = "Inland Trucking ";
            }
            if (transactionType == TermData.AirExport)
            {
                shipmentType = "Export (Air) ";
            }
            if (transactionType == TermData.AirImport)
            {
                shipmentType = "Import (Air) ";
            }
            if (transactionType == TermData.SeaConsolExport)
            {
                shipmentType = "Export (Sea Consol) ";
            }
            if (transactionType == TermData.SeaConsolImport)
            {
                shipmentType = "Import (Sea Consol) ";
            }
            if (transactionType == TermData.SeaFCLExport)
            {
                shipmentType = "Export (Sea FCL) ";
            }
            if (transactionType == TermData.SeaFCLImport)
            {
                shipmentType = "Import (Sea FCL) ";
            }
            if (transactionType == TermData.SeaLCLExport)
            {
                shipmentType = "Export (Sea LCL) ";
            }
            if (transactionType == TermData.SeaLCLImport)
            {
                shipmentType = "Import (Sea LCL) ";
            }
            return shipmentType;
        }
    }
}
