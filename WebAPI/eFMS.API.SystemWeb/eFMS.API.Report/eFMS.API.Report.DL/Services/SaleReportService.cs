using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using eFMS.API.Common.Globals;
using eFMS.API.Report.DL.Common;
using eFMS.API.Report.DL.IService;
using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Service.Models;
using eFMS.API.Report.Service.ViewModels;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.EF;
using ITL.NetCore.Connection;
using eFMS.API.Common.Helpers;
using ITL.NetCore.Connection.BL;
using AutoMapper;

namespace eFMS.API.Report.DL.Services
{
    public class SaleReportService : RepositoryBase<SysUser, SysUserModel>, ISaleReportService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUser> userRepository;
        private readonly IContextBase<SysEmployee> employeeRepository;
        private readonly IContextBase<SysCompany> companyRepository;
        private readonly IContextBase<SysOffice> officeRepository;
        private readonly IContextBase<CatDepartment> departmentRepository;
        private readonly IContextBase<CatPartner> catPartnerRepository;
        private readonly IContextBase<CatPlace> catPlaceRepository; 
        private readonly IContextBase<CsMawbcontainer> containerRepository;
        private readonly IContextBase<CsShipmentSurcharge> surchargeRepository;
        private IContextBase<CsTransaction> _context;
        private IContextBase<CatUnit> uniRepository;
        private IContextBase<CatChargeGroup> catChargeGroupRepo;
        private readonly IUserBaseService userBaseService;

        public SaleReportService(
            IMapper mapper,
            ICurrentUser currentUser, 
            IContextBase<CsTransaction> context,
            IContextBase<SysUser> userRepository, 
            IContextBase<SysEmployee> employeeRepository, 
            IContextBase<SysCompany> companyRepository, 
            IContextBase<SysOffice> officeRepository, 
            IContextBase<CatDepartment> departmentRepository, 
            IContextBase<CatPartner> catPartnerRepository, 
            IContextBase<CatPlace> catPlaceRepository, 
            IContextBase<CsMawbcontainer> containerRepository, 
            IContextBase<CsShipmentSurcharge> surchargeRepository,
            IUserBaseService _userBaseService,
            IContextBase<CatUnit> unit,
            IContextBase<CatChargeGroup> chargeGroup
            ): base(userRepository, mapper)
        {
            this.currentUser = currentUser;
            this.userRepository = userRepository;
            this.employeeRepository = employeeRepository;
            this.companyRepository = companyRepository;
            this.officeRepository = officeRepository;
            this.departmentRepository = departmentRepository;
            this.catPartnerRepository = catPartnerRepository;
            this.catPlaceRepository = catPlaceRepository;
            this.containerRepository = containerRepository;
            this.surchargeRepository = surchargeRepository;
            _context = context;
            userBaseService = _userBaseService;
            catChargeGroupRepo = chargeGroup;
            uniRepository = unit;
        }

        private eFMSDataContextDefault DC => (eFMSDataContextDefault)_context.DC;
        private decimal _decimalNumber = Constants.DecimalNumber;


        public Crystal PreviewCombinationSaleReport(SaleReportCriteria criteria)
        {
            Crystal crystal = null;
            IQueryable<CombinationSaleReportResult> data = GetDataCSShipmentCombinationReport(criteria);

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
                string _userIdAccountant = userBaseService.GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault();
                var _employeeIdAcountant = userRepository.Get(x => x.Id == _userIdAccountant).FirstOrDefault()?.EmployeeId;
                string _accountantName = employeeRepository.Get(x => x.Id == _employeeIdAcountant).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;

                // get current user's company manager 
                string _userIdComManager = userBaseService.GetCompanyManager(currentUser.CompanyID).FirstOrDefault();
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

        private IQueryable<CombinationSaleReportResult> GetDataCSShipmentCombinationReport(SaleReportCriteria criteria)
        {
            var data = GetDataSaleReport(criteria);
            if (data == null) return null;
            var listEmployee = employeeRepository.Get();
            var lookupEmployee = listEmployee.ToLookup(q => q.Id);
            var listUser = userRepository.Get();
            var lookupUser = listUser.ToLookup(q => q.Id);
            var listCharges = surchargeRepository.Get();
            var lookupSellingCharges = listCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE).ToLookup(q => q.Hblid);
            var lookupBuying = listCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE && (x.KickBack == false || x.KickBack == null)).ToLookup(q => q.Hblid);
            var lookupShareProfit = listCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE && x.KickBack == true).ToLookup(q => q.Hblid);
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
                    POD = item.Pod != null ? lookupPlace[(Guid)item.Pod]?.FirstOrDefault()?.Code : string.Empty,
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
                    Lines = item.ColoaderId != null ? partnerLookup[item.ColoaderId].FirstOrDefault()?.PartnerNameEn : string.Empty,
                    Agent = item.AgentId != null ? partnerLookup[item.AgentId].FirstOrDefault()?.PartnerNameEn : string.Empty,
                    NominationParty = item.NominationParty ?? string.Empty,
                    HAWBNO = item.HwbNo,
                    TpyeofService = item.TransactionType == "CL" ? API.Common.Globals.CustomData.Services.FirstOrDefault(c => c.Value == ReportConstants.LG_SHIPMENT)?.Value : item.TypeOfService != null ? (item.TypeOfService.Contains("LCL") ? "LCL" : string.Empty) : string.Empty,//item.ShipmentType.Contains("I") ? "IMP" : "EXP",
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
                report.SellingRate = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? (decimal)lookupSellingCharges[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupSellingCharges[item.HblId].Sum(x => x.AmountUsd ?? 0) + _decimalNumber;//GetSellingRate(item.HblId, criteria.Currency) + _decimalNumber; //Cộng thêm phần thập phân
                report.BuyingRate = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? (decimal)lookupBuying[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupBuying[item.HblId].Sum(x => x.AmountUsd ?? 0) + _decimalNumber; //Cộng thêm phần thập phân
                report.SharedProfit = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? (decimal)lookupShareProfit[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupShareProfit[item.HblId].Sum(x => x.AmountUsd) + _decimalNumber; //Cộng thêm phần thập phân

                report.Cont40HC = item.Cont40HC != null ? item.Cont40HC + _decimalNumber : 0;
                report.Qty20 = item.Qty20 != null ? item.Qty20 + _decimalNumber : 0;
                report.Qty40 = item.Qty40 != null ? item.Qty40 + _decimalNumber : 0;
                results.Add(report);
            }
            return results.AsQueryable();
        }
        public Crystal PreviewGetDepartSaleReport(SaleReportCriteria criteria)
        {
            var data = GetDataQuaterCSSaleReport(criteria);
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

                string _userIdAccountant = userBaseService.GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault();
                var _employeeIdAcountant = userRepository.Get(x => x.Id == _userIdAccountant).FirstOrDefault()?.EmployeeId;
                string _accountantName = employeeRepository.Get(x => x.Id == _employeeIdAcountant).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;

                string _managerName = string.Empty;
                string _userIdOfficeManager = userBaseService.GetOfficeManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault();
                var _employeeIdOfficeManager = userRepository.Get(x => x.Id == _userIdOfficeManager).FirstOrDefault()?.EmployeeId;
                _managerName = employeeRepository.Get(x => x.Id == _employeeIdOfficeManager).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;
                if (string.IsNullOrEmpty(_managerName))
                {
                    string _userIdComManager = userBaseService.GetCompanyManager(currentUser.CompanyID).FirstOrDefault();
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

        private IQueryable<QuaterSaleReportResult> GetDataQuaterCSSaleReport(SaleReportCriteria criteria)
        {
            var data = GetDataSaleReport(criteria);
            if (data == null) return null;
            var listEmployee = employeeRepository.Get();
            var lookupEmployee = listEmployee.ToLookup(q => q.Id);
            var listUser = userRepository.Get();
            var lookupUser = listUser.ToLookup(q => q.Id);
            var listCharges = surchargeRepository.Get();
            var lookupSellingCharges = listCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE).ToLookup(q => q.Hblid);
            var lookupBuying = listCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE && (x.KickBack == false || x.KickBack == null)).ToLookup(q => q.Hblid);
            var lookupBuyKB = listCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE && x.KickBack == true).ToLookup(q => q.Hblid);
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
                _sellingRate = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? (decimal)lookupSellingCharges[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupSellingCharges[item.HblId].Sum(x => x.AmountUsd ?? 0);
                #endregion -- Tổng amount trước thuế selling của HBL --

                #region -- Tổng amount trước thuế Buying của HBL (ko lấy phí có tick KB) --
                decimal _buyingRate = 0;
                _buyingRate = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? (decimal)lookupBuying[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupBuying[item.HblId].Sum(x => x.AmountUsd ?? 0);

                #endregion -- Tổng amount trước thuế Buying của HBL (ko lấy phí có tick KB) --

                #region -- Tổng amount trước thuế Buying của HBL (lấy phí có tick Kick Back) --
                decimal _sharedProfit = 0;
                _sharedProfit = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? (decimal)lookupBuyKB[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupBuyKB[item.HblId].Sum(x => x.AmountUsd ?? 0);
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
                    LoadingDate = item.TransactionType == "CL" ? item.Etd : (item.TransactionType.Contains("I") ? item.Eta : item.Etd),
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

        public Crystal PreviewGetMonthlySaleReport(SaleReportCriteria criteria)
        {
            var data = GetCSSaleReport(criteria);
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
                    CurrDecimalNo = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? 0 : 2,
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
            var lookupSellingCharges = listCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE).ToLookup(q => q.Hblid);
            var lookupBuying = listCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE && (x.KickBack == false || x.KickBack == null)).ToLookup(q => q.Hblid);
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
                    Description = item.TransactionType == "CL" ? "Logistics" : API.Common.Globals.CustomData.Services.FirstOrDefault(x => x.Value == item.TransactionType)?.DisplayName,
                    Area = string.Empty,
                    POL = item.Pol != null ? placeLookup[(Guid)item.Pol].FirstOrDefault()?.Code : string.Empty,
                    POD = item.Pod != null ? placeLookup[(Guid)item.Pod].FirstOrDefault()?.Code : string.Empty,
                    Lines = item.TransactionType == "CL" ? partnerLookup[item.ColoaderId].FirstOrDefault()?.PartnerNameEn : item.ColoaderId != null ? partnerLookup[item.ColoaderId].FirstOrDefault()?.PartnerNameEn : string.Empty,
                    Agent = item.AgentId != null ? partnerLookup[item.AgentId].FirstOrDefault()?.PartnerNameEn : string.Empty,
                    NominationParty = item.NominationParty ?? string.Empty,
                    assigned = item.TransactionType == "CL" ? false : item.ShipmentType == "Nominated",
                    TransID = item.JobNo,
                    HWBNO = item.HwbNo,
                    KGS = item.TransactionType == "CL" ? (item.GrossWeight ?? 0) + _decimalNumber : (item.TransactionType.Contains("A") ? (item.ChargeWeight ?? 0) : (item.GrossWeight ?? 0)) + _decimalNumber, //CR: CW đối với hàng Air, GW các hàng còn lại [25-09-2020]
                    CBM = (item.CBM ?? 0) + _decimalNumber, //Cộng thêm phần thập phân
                    SharedProfit = 0,
                    OtherCharges = 0,
                    SalesTarget = 0,
                    Bonus = 0,
                    TypeOfService = item.TransactionType == "CL" ? "CL" : (item.TypeOfService != null) ? (item.TypeOfService.Contains("LCL") ? "LCL" : string.Empty) : string.Empty,//item.ShipmentType.Contains("I") ? "IMP" : "EXP",
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
                report.SellingRate = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? (decimal)lookupSellingCharges[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupSellingCharges[item.HblId].Sum(x => x.AmountUsd ?? 0) + _decimalNumber;//GetSellingRate(item.HblId, criteria.Currency) + _decimalNumber; //Cộng thêm phần thập phân
                //Tổng amount trước thuế Buying của HBL (ko lấy phí có tick KB)
                report.BuyingRate = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? (decimal)lookupBuying[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupBuying[item.HblId].Sum(x => x.AmountUsd ?? 0) + _decimalNumber; //Cộng thêm phần thập phân
                //Tổng Amount Trước thuế của phí tick chon Kick Back
                report.SharedProfit = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? (decimal)lookupShareProfit[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupShareProfit[item.HblId].Sum(x => x.AmountUsd ?? 0) + _decimalNumber; //Cộng thêm phần thập phân

                report.Cont40HC = item.Cont40HC ?? 0 + _decimalNumber;
                report.Qty20 = item.Qty20 ?? 0 + _decimalNumber;
                report.Qty40 = item.Qty40 ?? 0 + _decimalNumber;
                results.Add(report);
            }
            return results.AsQueryable();
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
            // var list = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetDataSaleReport>(parameters);
            var list = DC.ExecuteProcedure<sp_GetDataSaleReport>(parameters);

            return list;
        }

        public Crystal PreviewGetQuaterSaleReport(SaleReportCriteria criteria)
        {
            var data = GetDataQuaterCSSaleReport(criteria);
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

                string _userIdAccountant = userBaseService.GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault();
                var _employeeIdAcountant = userRepository.Get(x => x.Id == _userIdAccountant).FirstOrDefault()?.EmployeeId;
                string _accountantName = employeeRepository.Get(x => x.Id == _employeeIdAcountant).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;

                string _managerName = string.Empty;
                string _userIdOfficeManager = userBaseService.GetOfficeManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault();
                var _employeeIdOfficeManager = userRepository.Get(x => x.Id == _userIdOfficeManager).FirstOrDefault()?.EmployeeId;
                _managerName = employeeRepository.Get(x => x.Id == _employeeIdOfficeManager).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;
                if (string.IsNullOrEmpty(_managerName))
                {
                    string _userIdComManager = userBaseService.GetCompanyManager(currentUser.CompanyID).FirstOrDefault();
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

        public Crystal PreviewSummarySaleReport(SaleReportCriteria criteria)
        {
            IQueryable<SummarySaleReportResult> data = GetDataCsShipmentReport(criteria);

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
                string _userIdAccountant = userBaseService.GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault();
                var _employeeIdAcountant = userRepository.Get(x => x.Id == _userIdAccountant).FirstOrDefault()?.EmployeeId;
                string _accountantName = employeeRepository.Get(x => x.Id == _employeeIdAcountant).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;

                // get current user's company manager 
                string _userIdComManager = userBaseService.GetCompanyManager(currentUser.CompanyID).FirstOrDefault();
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
        private IQueryable<SummarySaleReportResult> GetDataCsShipmentReport(SaleReportCriteria criteria)
        {
            var data = GetDataSaleReport(criteria);
            if (data == null) return null;
            var listEmployee = employeeRepository.Get();
            var lookupEmployee = listEmployee.ToLookup(q => q.Id);
            var listUser = userRepository.Get();
            var lookupUser = listUser.ToLookup(q => q.Id);
            var listCharges = surchargeRepository.Get();
            var lookupSellingCharges = listCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE).ToLookup(q => q.Hblid);
            var lookupBuying = listCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE && (x.KickBack == false || x.KickBack == null)).ToLookup(q => q.Hblid);
            var lookupShareProfit = listCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE && x.KickBack == true).ToLookup(q => q.Hblid);
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
                report.SellingRate = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? (decimal)lookupSellingCharges[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupSellingCharges[item.HblId].Sum(x => x.AmountUsd ?? 0) + _decimalNumber;//GetSellingRate(item.HblId, criteria.Currency) + _decimalNumber; //Cộng thêm phần thập phân
                //Buying without kickBack.
                report.BuyingRate = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? (decimal)lookupBuying[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupBuying[item.HblId].Sum(x => x.AmountUsd ?? 0) + _decimalNumber; //Cộng thêm phần thập phân
                //KickBack
                report.SharedProfit = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? (decimal)lookupShareProfit[item.HblId].Sum(x => x.AmountVnd ?? 0) : (decimal)lookupShareProfit[item.HblId].Sum(x => x.AmountUsd ?? 0) + _decimalNumber; //Cộng thêm phần thập phân

                report.Cont40HC = item.Cont40HC != null ? (decimal)item.Cont40HC + _decimalNumber : 0;
                report.Qty20 = item.Qty20 != null ? (decimal)item.Qty20 + _decimalNumber : 0;
                report.Qty40 = item.Qty40 != null ? (decimal)item.Qty40 + _decimalNumber : 0;
                results.Add(report);
            }
            return results.AsQueryable();
        }
        public Crystal PreviewSaleKickBackReport(SaleReportCriteria criteria)
        {
            var data = GetCSKickBackReport(criteria);
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
                    var surcharge = detailLookupSur[item.HblId].Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE && (x.KickBack == true || x.ChargeGroup == comChargeGroup?.Id));
                    foreach (var charge in surcharge)
                    {
                        var partner = lookupPartner[charge.PaymentObjectId].FirstOrDefault();
                        var report = new SaleKickBackReportResult
                        {
                            TransID = item.JobNo,
                            HBLID = item.HblId,
                            HAWBNO = item.HwbNo,
                            LoadingDate = item.TransactionType == "CL" ? item.Etd : item.TransactionType.Contains("I") ? item.Eta : item.Etd,
                            PartnerName = partner?.PartnerNameEn,
                            Description = item.TransactionType == "CL" ? "Logistics" : API.Common.Globals.CustomData.Services.FirstOrDefault(x => x.Value == item.TransactionType)?.DisplayName,
                            Quantity = charge.Quantity,
                            Unit = unitLookup[charge.UnitId].Select(t => t.Code).FirstOrDefault() ?? string.Empty,
                            UnitPrice = charge.UnitPrice ?? 0,
                            TotalValue = 0,
                            Currency = charge.CurrencyId,
                            Status = string.Empty,
                            // Total Amount phí buying kickback
                            UsdExt = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? (NumberHelper.RoundNumber(charge.VatAmountVnd ?? 0, 0) + NumberHelper.RoundNumber(charge.AmountVnd ?? 0, 0))
                                                                                           : (NumberHelper.RoundNumber(charge.VatAmountUsd ?? 0, 2) + NumberHelper.RoundNumber(charge.AmountUsd ?? 0, 2)),
                            MAWB = item.Mawb ?? string.Empty,
                            Shipper = item.ShipperDescription == null ? string.Empty : item.ShipperDescription.Split('\n').FirstOrDefault(),
                            Consignee = item.ConsigneeDescription == null ? string.Empty : item.ConsigneeDescription.Split('\n').FirstOrDefault(),
                            PartnerID = item.CustomerId,
                            Category = partner?.PartnerType,
                        };
                        // Lấy tất cả Buying charge
                        var chargeBuy = detailLookupSur[(Guid)item.HblId].Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE);
                        report.Costs = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? chargeBuy.Sum(x => x.AmountVnd ?? 0) + _decimalNumber : chargeBuy.Sum(x => x.AmountUsd ?? 0) + _decimalNumber; //Cộng thêm phần thập phân
                        // Lấy tất cả Selling charge
                        var chargeSell = detailLookupSur[(Guid)item.HblId].Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE);
                        report.Incomes = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? chargeSell.Sum(x => x.AmountVnd ?? 0) + _decimalNumber : chargeSell.Sum(x => x.AmountUsd ?? 0) + _decimalNumber; //Cộng thêm phần thập phân

                        results.Add(report);
                    }
                }
            }
            return results.AsQueryable();
        }
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
