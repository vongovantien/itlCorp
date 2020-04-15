using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models.ReportResults.Sales;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class SaleReportService : ISaleReportService
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

        public SaleReportService(IContextBase<OpsTransaction> opsRepo,
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
            IContextBase<SysUserLevel> userLevelRepo)
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
        }
        private IQueryable<MonthlySaleReportResult> GetMonthlySaleReport(SaleReportCriteria criteria)
        {
            IQueryable<MonthlySaleReportResult> opsShipments = null;
            IQueryable<MonthlySaleReportResult> csShipments = null;
            if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains("CL"))
            {
                opsShipments = GetOpsSaleReport(criteria);
            }
            csShipments = GetCSSaleReport(criteria);
            if (opsShipments == null) return csShipments;
            else if (csShipments == null) return opsShipments;
            else return opsShipments.Union(csShipments);
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
                    KGS = item.SumNetWeight == null ? 0 : (decimal)item.SumNetWeight,
                    CBM = item.SumCbm == null ? 0 : (decimal)item.SumCbm,
                    SharedProfit = 0,
                    OtherCharges = 0,
                    SalesTarget = 0,
                    Bonus = 0,
                    TpyeofService = "CL",
                    Shipper = item.Shipper,
                    Consignee = item.Consignee,
                    LoadingDate = item.ServiceDate
                };
                string employeeId = userRepository.Get(x => x.Id == item.SalemanId).FirstOrDefault()?.EmployeeId;
                if (employeeId != null)
                {
                    report.ContactName = employeeRepository.Get(x => x.Id == employeeId).FirstOrDefault()?.EmployeeNameVn;
                }
                //Tổng amount trước thuế selling của HBL
                report.SellingRate = GetSellingRate(item.Hblid, criteria.Currency);
                //Tổng amount trước thuế Buying của HBL (ko lấy phí có tick KB)
                report.BuyingRate = GetBuyingRate(item.Hblid, criteria.Currency);
                //Tổng amount Trước thuế của phí tick chon Kick Back
                report.SharedProfit = GetShareProfit(item.Hblid, criteria.Currency);
                var contInfo = GetContainer(containerData, item.Id);
                report.Cont40HC = (decimal)contInfo?.Cont40HC;
                report.Qty20 = (decimal)contInfo?.Qty20;
                report.Qty40 = (decimal)contInfo?.Qty40;
                results.Add(report);
            }
            return results.AsQueryable();
        }
        private IQueryable<OpsTransaction> QueryOpsSaleReport(SaleReportCriteria criteria)
        {
            Expression<Func<OpsTransaction, bool>> queryOpsTrans = x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED
                                                                 && (x.AgentId == criteria.AgentId || criteria.AgentId == null)
                                                                 && (x.Mblno.IndexOf(criteria.Mawb, StringComparison.OrdinalIgnoreCase) > -1 || string.IsNullOrEmpty(criteria.Mawb))
                                                                 && (x.JobNo.IndexOf(criteria.JobId, StringComparison.OrdinalIgnoreCase) > -1 || string.IsNullOrEmpty(criteria.JobId))
                                                                 && (x.Hwbno.IndexOf(criteria.Hawb, StringComparison.OrdinalIgnoreCase) > -1 || string.IsNullOrEmpty(criteria.Hawb))
                                                                 && (x.Pod == criteria.Pod || criteria.Pod == null)
                                                                 && (x.Pol == criteria.Pol || criteria.Pol == null)
                                                                 && (x.SupplierId == criteria.CarrierId || criteria.CarrierId == null)
                                                                 && (criteria.OfficeId.Contains(x.OfficeId.ToString()) || string.IsNullOrEmpty(criteria.OfficeId))
                                                                 && (criteria.DepartmentId.Contains(x.DepartmentId.ToString()) || string.IsNullOrEmpty(criteria.DepartmentId))
                                                                 && (criteria.GroupId.Contains(x.GroupId.ToString()) || string.IsNullOrEmpty(criteria.GroupId))
                                                                 && (criteria.PersonInCharge.Contains(x.BillingOpsId) || string.IsNullOrEmpty(criteria.PersonInCharge))
                                                                 && (criteria.Creator.Contains(x.UserCreated) || string.IsNullOrEmpty(criteria.Creator)
                                                                 && (criteria.SalesMan.Contains(x.SalemanId) || string.IsNullOrEmpty(criteria.SalesMan)))
                                                                 ;
            if (criteria.ServiceDateFrom != null && criteria.ServiceDateTo != null)
            {
                queryOpsTrans = queryOpsTrans.And(x => x.ServiceDate.Value.Date >= criteria.ServiceDateFrom.Value.Date && x.ServiceDate.Value.Date <= criteria.ServiceDateTo.Value.Date);
            }
            if (criteria.CreatedDateFrom != null && criteria.CreatedDateTo != null)
            {
                queryOpsTrans = queryOpsTrans.And(x => x.DatetimeCreated.Value.Date >= criteria.CreatedDateFrom.Value.Date && x.DatetimeCreated.Value.Date <= criteria.CreatedDateTo.Value.Date);
            }
            var data = opsRepository.Get(queryOpsTrans);
            return data;
        }
        private IQueryable<MonthlySaleReportResult> GetCSSaleReport(SaleReportCriteria criteria)
        {
            IQueryable<CsTransaction> shipments = QueryCsTransaction(criteria);
            if (shipments == null) return null;
            IQueryable<CsTransactionDetail> housebills = QueryHouseBills(criteria);

            var data = (from shipment in shipments
                        join housebill in housebills on shipment.Id equals housebill.JobId
                        select new
                        {
                            shipment.DepartmentId,
                            shipment.TransactionType,
                            shipment.JobNo,
                            shipment.ShipmentType,
                            shipment.Pol,
                            shipment.Pod,
                            shipment.ColoaderId,
                            shipment.AgentId,
                            housebill.CustomerId,
                            housebill.NotifyPartyDescription,
                            HBLID = housebill.Id,
                            housebill.Hwbno,
                            housebill.NetWeight,
                            housebill.Cbm,
                            housebill.ShipperDescription,
                            housebill.ConsigneeDescription,
                            housebill.SaleManId,
                            shipment.Eta,
                            shipment.Etd
                        });
            if (data == null) return null;
            var results = new List<MonthlySaleReportResult>();
            var containerData = uniRepository.Get(x => x.UnitType == "Container");
            foreach (var item in data)
            {
                var report = new MonthlySaleReportResult
                {
                    Department = departmentRepository.Get(x => x.Id == item.DepartmentId).FirstOrDefault()?.DeptNameEn,
                    //ContactName = "An Mai Loan",
                    SalesManager = string.Empty,
                    PartnerName = catPartnerRepository.Get(x => x.Id == item.CustomerId).FirstOrDefault()?.PartnerNameEn,
                    Description = API.Common.Globals.CustomData.Services.FirstOrDefault(x => x.Value == item.TransactionType)?.DisplayName,
                    Area = string.Empty,
                    POL = item.Pol != null ? catPlaceRepository.Get(x => x.Id == item.Pol).FirstOrDefault()?.NameEn : string.Empty,
                    POD = item.Pod != null ? catPlaceRepository.Get(x => x.Id == item.Pod).FirstOrDefault()?.NameEn : string.Empty,
                    Lines = item.ColoaderId != null ? catPartnerRepository.Get(x => x.Id == item.ColoaderId).FirstOrDefault()?.PartnerNameEn : string.Empty,
                    Agent = item.AgentId != null ? catPartnerRepository.Get(x => x.Id == item.AgentId).FirstOrDefault()?.PartnerNameEn : string.Empty,
                    NominationParty = item.NotifyPartyDescription ?? string.Empty,
                    assigned = item.ShipmentType == "Nominated",
                    TransID = item.JobNo,
                    HWBNO = item.Hwbno,
                    KGS = item.NetWeight == null ? 0 : (decimal)item.NetWeight,
                    CBM = item.Cbm == null ? 0 : (decimal)item.Cbm,
                    SharedProfit = 0,
                    OtherCharges = 0,
                    SalesTarget = 0,
                    Bonus = 0,
                    TpyeofService = item.TransactionType.Contains("I") ? "IMP" : "EXP",
                    Shipper = item.ShipperDescription,
                    Consignee = item.ConsigneeDescription,
                    LoadingDate = item.TransactionType.Contains("I") ? item.Eta : item.Etd
                };
                string employeeId = userRepository.Get(x => x.Id == item.SaleManId).FirstOrDefault()?.EmployeeId;
                if (employeeId != null)
                {
                    report.ContactName = employeeRepository.Get(x => x.Id == employeeId).FirstOrDefault()?.EmployeeNameVn;
                }
                //Tổng amount trước thuế selling của HBL
                report.SellingRate = GetSellingRate(item.HBLID, criteria.Currency);
                //Tổng amount trước thuế Buying của HBL (ko lấy phí có tick KB)
                report.BuyingRate = GetBuyingRate(item.HBLID, criteria.Currency);
                //Tổng Amount Trước thuế của phí tick chon Kick Back
                report.SharedProfit = GetShareProfit(item.HBLID, criteria.Currency);
                var contInfo = GetContainer(containerData, item.HBLID);
                report.Cont40HC = (decimal)contInfo?.Cont40HC;
                report.Qty20 = (decimal)contInfo?.Qty20;
                report.Qty40 = (decimal)contInfo?.Qty40;
                results.Add(report);
            }
            return results.AsQueryable();
        }

        private IQueryable<CsTransactionDetail> QueryHouseBills(SaleReportCriteria criteria)
        {
            Expression<Func<CsTransactionDetail, bool>> queryTranDetail = null;
            if (!string.IsNullOrEmpty(criteria.CustomerId))
            {
                queryTranDetail = x => x.CustomerId == criteria.CustomerId;
            }
            if (!string.IsNullOrEmpty(criteria.Hawb))
            {
                queryTranDetail = queryTranDetail == null ? (x => x.Hwbno.IndexOf(criteria.Hawb, StringComparison.OrdinalIgnoreCase) > -1)
                                                        : queryTranDetail.And(x => x.Hwbno.IndexOf(criteria.Hawb, StringComparison.OrdinalIgnoreCase) > -1);
            }
            if (!string.IsNullOrEmpty(criteria.SalesMan))
            {
                queryTranDetail = x => criteria.SalesMan.Contains(x.SaleManId);
            }
            var housebills = queryTranDetail == null ? detailRepository.Get() : detailRepository.Get(queryTranDetail);
            return housebills;
        }

        private IQueryable<CsTransaction> QueryCsTransaction(SaleReportCriteria criteria)
        {
            Expression<Func<CsTransaction, bool>> queryTrans = x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED
                                                                 && (x.AgentId == criteria.AgentId || criteria.AgentId == null)
                                                                 && (x.Mawb.IndexOf(criteria.Mawb, StringComparison.OrdinalIgnoreCase) > -1 || string.IsNullOrEmpty(criteria.Mawb))
                                                                 && (x.JobNo.IndexOf(criteria.JobId, StringComparison.OrdinalIgnoreCase) > -1 || string.IsNullOrEmpty(criteria.JobId))
                                                                 && (criteria.Service.Contains(x.TransactionType) || string.IsNullOrEmpty(x.TransactionType))
                                                                 && (x.Pod == criteria.Pod || criteria.Pod == null)
                                                                 && (x.Pol == criteria.Pol || criteria.Pol == null)
                                                                 && (x.ColoaderId == criteria.CarrierId || criteria.CarrierId == null)
                                                                 && (criteria.OfficeId.Contains(x.OfficeId.ToString()) || string.IsNullOrEmpty(criteria.OfficeId))
                                                                 && (criteria.DepartmentId.Contains(x.DepartmentId.ToString()) || string.IsNullOrEmpty(criteria.DepartmentId))
                                                                 && (criteria.GroupId.Contains(x.GroupId.ToString()) || string.IsNullOrEmpty(criteria.GroupId))
                                                                 && (criteria.PersonInCharge.Contains(x.PersonIncharge) || string.IsNullOrEmpty(criteria.PersonInCharge))
                                                                 && (criteria.Creator.Contains(x.UserCreated) || string.IsNullOrEmpty(criteria.Creator))
                                                                 ;
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
                report.Cont40HC = (decimal)conts.Where(x => x.Code == "Cont40HC").Sum(x => x.x.Quantity);
                report.Qty20 = (decimal)conts.Where(x => x.Code == "Cont20DC").Sum(x => x.x.Quantity);
                report.Qty40 = (decimal)conts.Where(x => x.Code == "Cont40DC").Sum(x => x.x.Quantity);
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
                    var rate = charge.FinalExchangeRate;
                    if (rate == null)
                    {
                        rate = GetCurrencyExchangeRate(charge, toCurrency);
                    }
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
                    var rate = charge.FinalExchangeRate;
                    if (rate == null)
                    {
                        rate = GetCurrencyExchangeRate(charge, currencyTo);
                    }
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
                    var rate = charge.FinalExchangeRate;
                    if (rate == null)
                    {
                        rate = GetCurrencyExchangeRate(charge, toCurrency);
                    }
                    revenue += charge.Quantity * charge.UnitPrice * rate ?? 0;
                }
            }
            return revenue;
        }

        private decimal GetCurrencyExchangeRate(CsShipmentSurcharge charge, string toCurrency)
        {
            if (charge.CurrencyId == toCurrency) return 1;
            var currencyExchange = exchangeRepository.Get(x => x.DatetimeModified.Value.Date == charge.ExchangeDate.Value.Date
                                                            && x.CurrencyFromId == charge.CurrencyId && x.CurrencyToId == toCurrency)?.OrderByDescending(x => x.DatetimeModified)?.FirstOrDefault();
            if (currencyExchange != null) return currencyExchange.Rate;
            else
            {
                currencyExchange = exchangeRepository.Get(x => x.DatetimeModified.Value.Date == charge.ExchangeDate.Value.Date
                                                            && x.CurrencyFromId == charge.CurrencyId && x.CurrencyToId == toCurrency)?.OrderByDescending(x => x.DatetimeModified)?.FirstOrDefault();
                if (currencyExchange != null)
                {
                    return (1 / currencyExchange.Rate);
                }
                return 0;
            }
        }

        public Crystal PreviewGetMonthlySaleReport(SaleReportCriteria criteria)
        {
            var data = GetMonthlySaleReport(criteria);
            Crystal result = new Crystal();
            if (data == null) result = null;
            else
            {
                var list = data.ToList();
                var employeeId = userRepository.Get(x => x.Id == currentUser.UserID).FirstOrDefault()?.EmployeeId;
                string employeeContact = string.Empty;
                if (employeeId == null)
                {
                    var employee = employeeRepository.Get(x => x.Id == employeeId).FirstOrDefault();
                    employeeContact = employee != null ? employee.EmployeeNameVn ?? string.Empty : string.Empty;
                }
                var company = companyRepository.Get(x => x.Id == currentUser.CompanyID).FirstOrDefault();
                var parameter = new MonthlySaleReportParameter
                {
                    FromDate = DateTime.Now,
                    ToDate = DateTime.Now,
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

        private List<string> GetCompanyManager(Guid? companyId)
        {
            var companyManager = userLevelRepository.Get(x => x.GroupId == 11
                                                    && x.CompanyId == companyId
                                                    && x.Position == "Manager-Leader")
                                                    .Select(s => s.UserId).ToList();
            return companyManager;
        }

        #region -- SALE REPORT BY QUATER --
        private decimal GetRateCurrencyExchange(List<CatCurrencyExchange> currencyExchange, string currencyFrom, string currencyTo)
        {
            if (currencyExchange.Count == 0 || string.IsNullOrEmpty(currencyFrom)) return 0;

            currencyFrom = currencyFrom.Trim();
            currencyTo = currencyTo.Trim();

            if (currencyFrom != currencyTo)
            {
                var get1 = currencyExchange.Where(x => x.CurrencyFromId.Trim() == currencyFrom && x.CurrencyToId.Trim() == currencyTo).OrderByDescending(x => x.Rate).FirstOrDefault();
                if (get1 != null)
                {
                    return get1.Rate;
                }
                else
                {
                    var get2 = currencyExchange.Where(x => x.CurrencyFromId.Trim() == currencyTo && x.CurrencyToId.Trim() == currencyFrom).OrderByDescending(x => x.Rate).FirstOrDefault();
                    if (get2 != null)
                    {
                        return 1 / get2.Rate;
                    }
                    else
                    {
                        var get3 = currencyExchange.Where(x => x.CurrencyFromId.Trim() == currencyFrom || x.CurrencyFromId.Trim() == currencyTo).OrderByDescending(x => x.Rate).ToList();
                        if (get3.Count > 1)
                        {
                            if (get3[0].CurrencyFromId.Trim() == currencyFrom && get3[1].CurrencyFromId.Trim() == currencyTo)
                            {
                                return get3[0].Rate / get3[1].Rate;
                            }
                            else
                            {
                                return get3[1].Rate / get3[0].Rate;
                            }
                        }
                        else
                        {
                            //Nến không tồn tại Currency trong Exchange thì return về 0
                            return 0;
                        }
                    }
                }
            }
            return 1;
        }

        private decimal CurrencyExchangeRateConvert(decimal? finalExchangeRate, DateTime? exchangeDate, string currencyFrom, string currencyTo)
        {
            var exchargeDateSurcharge = exchangeDate == null ? DateTime.Now.Date : exchangeDate.Value.Date;
            List<CatCurrencyExchange> currencyExchange = exchangeRepository.Get(x => x.DatetimeCreated.Value.Date == exchargeDateSurcharge).ToList();
            decimal _exchangeRateCurrencyTo = GetRateCurrencyExchange(currencyExchange, currencyTo, DocumentConstants.CURRENCY_LOCAL); //Lấy currency Local làm gốc để quy đỗi
            decimal _exchangeRateCurrencyFrom = GetRateCurrencyExchange(currencyExchange, currencyFrom, DocumentConstants.CURRENCY_LOCAL); //Lấy currency Local làm gốc để quy đỗi

            decimal _exchangeRate = 0;
            if (finalExchangeRate != null)
            {
                if (currencyFrom == currencyTo)
                {
                    _exchangeRate = 1;
                }
                else if (currencyFrom == DocumentConstants.CURRENCY_LOCAL && currencyTo != DocumentConstants.CURRENCY_LOCAL)
                {
                    _exchangeRate = (finalExchangeRate.Value != 0) ? (1 / finalExchangeRate.Value) : 0;
                }
                else if (currencyFrom != DocumentConstants.CURRENCY_LOCAL && currencyTo == DocumentConstants.CURRENCY_LOCAL)
                {
                    _exchangeRate = finalExchangeRate.Value;
                }
                else
                {
                    _exchangeRate = (_exchangeRateCurrencyTo != 0) ? (finalExchangeRate.Value / _exchangeRateCurrencyTo) : 0;
                }
            }
            else
            {
                if (currencyFrom == currencyTo)
                {
                    _exchangeRate = 1;
                }
                else if (currencyFrom == DocumentConstants.CURRENCY_LOCAL && currencyTo != DocumentConstants.CURRENCY_LOCAL)
                {
                    _exchangeRate = (_exchangeRateCurrencyTo != 0) ? (1 / _exchangeRateCurrencyTo) : 0;
                }
                else if (currencyFrom != DocumentConstants.CURRENCY_LOCAL && currencyTo == DocumentConstants.CURRENCY_LOCAL)
                {
                    _exchangeRate = GetRateCurrencyExchange(currencyExchange, currencyFrom, DocumentConstants.CURRENCY_LOCAL);
                }
                else
                {
                    _exchangeRate = (_exchangeRateCurrencyTo != 0) ? (_exchangeRateCurrencyFrom / _exchangeRateCurrencyTo) : 0;
                }
            }
            return _exchangeRate;
        }

        private IQueryable<QuaterSaleReportResult> GetQuaterOpsSaleReport(SaleReportCriteria criteria)
        {
            List<QuaterSaleReportResult> results = null;
            IQueryable<OpsTransaction> data = QueryOpsSaleReport(criteria);
            if (data == null) return null;
            results = new List<QuaterSaleReportResult>();
            foreach (var item in data)
            {
                string employeeId = userRepository.Get(x => x.Id == item.SalemanId).FirstOrDefault()?.EmployeeId;
                string _contactName = string.Empty;
                if (employeeId != null)
                {
                    _contactName = employeeRepository.Get(x => x.Id == employeeId).FirstOrDefault()?.EmployeeNameVn;
                }

                string _deptManagerSale = GetDeptManager(item.CompanyId, item.OfficeId, item.DepartmentId).FirstOrDefault();
                string _employeeIdDeptManagerSale = userRepository.Get(x => x.Id == _deptManagerSale).FirstOrDefault()?.EmployeeId;
                string _saleManager = string.Empty;
                if(_employeeIdDeptManagerSale != null)
                {
                    _saleManager = employeeRepository.Get(x => x.Id == _employeeIdDeptManagerSale).FirstOrDefault()?.EmployeeNameVn;
                }

                #region -- Tổng amount trước thuế selling của HBL --               
                decimal _sellingRate = 0;
                var _chargeSell = surchargeRepository.Get(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE && x.Hblid == item.Hblid);
                foreach (var charge in _chargeSell)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var _rate = CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    _sellingRate += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                }
                #endregion -- Tổng amount trước thuế selling của HBL --

                #region -- Tổng amount trước thuế Buying của HBL (ko lấy phí có tick KB) --
                decimal _buyingRate = 0;
                var _chargeBuy = surchargeRepository.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE 
                                                            && x.Hblid == item.Hblid 
                                                            && (x.KickBack == false || x.KickBack == null));
                foreach (var charge in _chargeBuy)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var _rate = CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    _buyingRate += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                }
                #endregion -- Tổng amount trước thuế Buying của HBL (ko lấy phí có tick KB) --

                #region -- Tổng amount trước thuế Buying của HBL (lấy phí có tick Kick Back) --
                decimal _sharedProfit = 0;
                var _chargeBuyKB = surchargeRepository.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE
                                                            && x.Hblid == item.Hblid
                                                            && x.KickBack == true);
                foreach (var charge in _chargeBuy)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var _rate = CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    _sharedProfit += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                }
                #endregion -- Tổng amount trước thuế Buying của HBL (lấy phí có tick Kick Back) --

                var report = new QuaterSaleReportResult
                {
                    Department = departmentRepository.Get(x => x.Id == item.DepartmentId).FirstOrDefault()?.DeptNameEn, // Department của Sale lô hàng đó
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
                    LoadingDate = item.ServiceDate,
                    HWBNO = string.Empty,
                    Volumne = string.Empty,
                    Qty20 = 0,
                    Qty40 = 0,
                    Cont40HC = 0,
                    KGS = 0,
                    CBM = 0,
                    SellingRate = _sellingRate, // Total Amount Selling
                    SharedProfit = _sharedProfit, // Total Amount Buying có Check Kick Back
                    BuyingRate = _buyingRate, // Total Amount Buying ko có Check Kick Back
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

        private IQueryable<QuaterSaleReportResult> GetQuaterCSSaleReport(SaleReportCriteria criteria)
        {
            IQueryable<CsTransaction> shipments = QueryCsTransaction(criteria);
            if (shipments == null) return null;
            IQueryable<CsTransactionDetail> housebills = QueryHouseBills(criteria);

            var data = (from shipment in shipments
                        join housebill in housebills on shipment.Id equals housebill.JobId
                        select new
                        {
                            shipment.DepartmentId,
                            shipment.TransactionType,
                            shipment.JobNo,
                            shipment.ShipmentType,
                            shipment.Pol,
                            shipment.Pod,
                            shipment.ColoaderId,
                            shipment.AgentId,
                            housebill.CustomerId,
                            housebill.NotifyPartyDescription,
                            HBLID = housebill.Id,
                            housebill.Hwbno,
                            housebill.NetWeight,
                            housebill.Cbm,
                            housebill.ShipperDescription,
                            housebill.ConsigneeDescription,
                            housebill.SaleManId,
                            shipment.Eta,
                            shipment.Etd,
                            housebill.OfficeId,
                            housebill.CompanyId,
                            DepartmentIdHbl = housebill.DepartmentId
                        });
            if (data == null) return null;
            var results = new List<QuaterSaleReportResult>();
            foreach (var item in data)
            {
                string employeeId = userRepository.Get(x => x.Id == item.SaleManId).FirstOrDefault()?.EmployeeId;
                string _contactName = string.Empty;
                if (employeeId != null)
                {
                    _contactName = employeeRepository.Get(x => x.Id == employeeId).FirstOrDefault()?.EmployeeNameVn;
                }

                string _deptManagerSale = GetDeptManager(item.CompanyId, item.OfficeId, item.DepartmentIdHbl).FirstOrDefault();
                string _employeeIdDeptManagerSale = userRepository.Get(x => x.Id == _deptManagerSale).FirstOrDefault()?.EmployeeId;
                string _saleManager = string.Empty;
                if (_employeeIdDeptManagerSale != null)
                {
                    _saleManager = employeeRepository.Get(x => x.Id == _employeeIdDeptManagerSale).FirstOrDefault()?.EmployeeNameVn;
                }

                #region -- Tổng amount trước thuế selling của HBL --               
                decimal _sellingRate = 0;
                var _chargeSell = surchargeRepository.Get(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE && x.Hblid == item.HBLID);
                foreach (var charge in _chargeSell)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var _rate = CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    _sellingRate += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                }
                #endregion -- Tổng amount trước thuế selling của HBL --

                #region -- Tổng amount trước thuế Buying của HBL (ko lấy phí có tick KB) --
                decimal _buyingRate = 0;
                var _chargeBuy = surchargeRepository.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE
                                                            && x.Hblid == item.HBLID
                                                            && (x.KickBack == false || x.KickBack == null));
                foreach (var charge in _chargeBuy)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var _rate = CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    _buyingRate += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                }
                #endregion -- Tổng amount trước thuế Buying của HBL (ko lấy phí có tick KB) --

                #region -- Tổng amount trước thuế Buying của HBL (lấy phí có tick Kick Back) --
                decimal _sharedProfit = 0;
                var _chargeBuyKB = surchargeRepository.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE
                                                            && x.Hblid == item.HBLID
                                                            && x.KickBack == true);
                foreach (var charge in _chargeBuy)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var _rate = CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    _sharedProfit += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                }
                #endregion -- Tổng amount trước thuế Buying của HBL (lấy phí có tick Kick Back) --

                var report = new QuaterSaleReportResult
                {
                    Department = departmentRepository.Get(x => x.Id == item.DepartmentId).FirstOrDefault()?.DeptNameEn, // Department của Sale lô hàng đó
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
                    LoadingDate = item.TransactionType.Contains("I") ? item.Eta : item.Etd,
                    HWBNO = string.Empty,
                    Volumne = string.Empty,
                    Qty20 = 0,
                    Qty40 = 0,
                    Cont40HC = 0,
                    KGS = 0,
                    CBM = 0,
                    SellingRate = _sellingRate, // Total Amount Selling
                    SharedProfit = _sharedProfit, // Total Amount Buying có Check Kick Back
                    BuyingRate = _buyingRate, // Total Amount Buying ko có Check Kick Back
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

        private IQueryable<QuaterSaleReportResult> GetQuaterSaleReport(SaleReportCriteria criteria)
        {
            IQueryable<QuaterSaleReportResult> opsShipments = null;
            IQueryable<QuaterSaleReportResult> csShipments = null;
            if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains("CL"))
            {
                opsShipments = GetQuaterOpsSaleReport(criteria);
            }
            csShipments = GetQuaterCSSaleReport(criteria);
            if (opsShipments == null) return csShipments;
            else if (csShipments == null) return opsShipments;
            else return opsShipments.Union(csShipments);
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

                string _userIdComManager = GetCompanyManager(currentUser.CompanyID).FirstOrDefault();
                var _employeeIdComManager = userRepository.Get(x => x.Id == _userIdComManager).FirstOrDefault()?.EmployeeId;
                string _comManagerName = employeeRepository.Get(x => x.Id == _employeeIdAcountant).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;

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
                    Director = _comManagerName, // Company Manager
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

        #endregion -- SALE REPORT BY QUATER --
    }
}
