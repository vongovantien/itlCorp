using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using eFMS.API.Common.Globals;
using eFMS.API.Report.DL.Common;
using eFMS.API.Report.DL.IService;
using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Service.Context;
using eFMS.API.Report.Service.Models;
using eFMS.API.Report.Service.ViewModels;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.EF;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection;

namespace eFMS.API.Report.DL.Services
{
    public class SaleReportService : ISaleReportService
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


        public SaleReportService(
            ICurrentUser currentUser, 
            decimal decimalNumber, 
            IContextBase<CsTransaction> context,
            IContextBase<SysUser> userRepository, 
            IContextBase<SysEmployee> employeeRepository, 
            IContextBase<SysCompany> companyRepository, 
            IContextBase<SysOffice> officeRepository, 
            IContextBase<CatDepartment> departmentRepository, 
            IContextBase<CatPartner> catPartnerRepository, 
            IContextBase<CatPlace> catPlaceRepository, 
            IContextBase<CsMawbcontainer> containerRepository, 
            IContextBase<CsShipmentSurcharge> surchargeRepository
            )
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

        }

        private eFMSDataContextDefault DC => (eFMSDataContextDefault)_context.DC;
        private decimal _decimalNumber = Constants.DecimalNumber;


        public Crystal PreviewCombinationSaleReport(SaleReportCriteria criteria)
        {
            throw new NotImplementedException();
        }

        public Crystal PreviewGetDepartSaleReport(SaleReportCriteria criteria)
        {
            throw new NotImplementedException();
        }

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
        private IQueryable<MonthlySaleReportResult> GetMonthlySaleReport(SaleReportCriteria criteria)
        {
            IQueryable<MonthlySaleReportResult> csShipments = null;
            csShipments = GetCSSaleReport(criteria);
            return csShipments;
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
            throw new NotImplementedException();
        }

        public Crystal PreviewSaleKickBackReport(SaleReportCriteria criteria)
        {
            throw new NotImplementedException();
        }

        public Crystal PreviewSummarySaleReport(SaleReportCriteria criteria)
        {
            throw new NotImplementedException();
        }
    }
}
