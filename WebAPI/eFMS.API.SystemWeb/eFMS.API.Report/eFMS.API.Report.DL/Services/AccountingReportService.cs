using AutoMapper;
using eFMS.API.Report.DL.Common;
using eFMS.API.Report.DL.IService;
using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Service.Context;
using eFMS.API.Report.Service.Models;
using eFMS.API.Report.Service.ViewModels;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace eFMS.API.Report.DL.Services
{
    public class AccountingReportService : IAccountingReportService
    {
        private readonly IMapper mapper;
        private readonly IContextBase<OpsTransaction> opsRepository;
        private readonly IContextBase<CsTransactionDetail> detailRepository;
        private readonly IContextBase<CsShipmentSurcharge> surCharge;
        private readonly IContextBase<CatPartner> catPartnerRepo;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUser> sysUserRepo;
        private readonly IContextBase<SysEmployee> sysEmployeeRepo;
        private readonly IContextBase<CatCurrencyExchange> catCurrencyExchangeRepo;
        private readonly IContextBase<CatPlace> catPlaceRepo;
        private readonly IContextBase<CatUnit> catUnitRepo;
        private readonly IContextBase<CatCharge> catChargeRepo;
        private readonly IContextBase<CatChargeGroup> catChargeGroupRepo;
        private readonly IContextBase<SysOffice> sysOfficeRepo;
        private readonly IContextBase<SysUserLevel> sysUserLevelRepo;
        private readonly IContextBase<CustomsDeclaration> customsDeclarationRepo;
        private readonly ICurrencyExchangeService currencyExchangeService;
        private readonly IContextBase<CatIncoterm> catIncotermRepository;
        readonly IContextBase<CatCommodity> catCommodityRepo;
        readonly IContextBase<CatCommodityGroup> catCommodityGroupRepo;
        private eFMSDataContextDefault DC => (eFMSDataContextDefault)opsRepository.DC;

        public AccountingReportService(IMapper _mapper,
            IContextBase<OpsTransaction> opsRepository, 
            IContextBase<CsTransactionDetail> detailRepository, 
            IContextBase<CsShipmentSurcharge> surCharge, 
            IContextBase<CatPartner> catPartnerRepo, 
            ICurrentUser currentUser, 
            IContextBase<SysUser> sysUserRepo, 
            IContextBase<SysEmployee> sysEmployeeRepo, 
            IContextBase<CatCurrencyExchange> catCurrencyExchangeRepo, 
            IContextBase<CatPlace> catPlaceRepo, 
            IContextBase<CatUnit> catUnitRepo, 
            IContextBase<CatCharge> catChargeRepo, 
            IContextBase<CatChargeGroup> catChargeGroupRepo,
            IContextBase<CatCommodity> catCommodity,
            IContextBase<CatCommodityGroup> catCommodityGroup,
            IContextBase<SysOffice> sysOfficeRepo, 
            IContextBase<SysUserLevel> sysUserLevelRepo, 
            IContextBase<CustomsDeclaration> customsDeclarationRepo, 
            ICurrencyExchangeService currencyExchangeService, 
            IContextBase<CatIncoterm> catIncotermRepository)
        {
            this.mapper = _mapper;
            this.opsRepository = opsRepository;
            this.detailRepository = detailRepository;
            this.surCharge = surCharge;
            this.catPartnerRepo = catPartnerRepo;
            this.currentUser = currentUser;
            this.sysUserRepo = sysUserRepo;
            this.sysEmployeeRepo = sysEmployeeRepo;
            this.catCurrencyExchangeRepo = catCurrencyExchangeRepo;
            this.catPlaceRepo = catPlaceRepo;
            this.catUnitRepo = catUnitRepo;
            this.catChargeRepo = catChargeRepo;
            this.catChargeGroupRepo = catChargeGroupRepo;
            this.sysOfficeRepo = sysOfficeRepo;
            this.sysUserLevelRepo = sysUserLevelRepo;
            this.customsDeclarationRepo = customsDeclarationRepo;
            this.currencyExchangeService = currencyExchangeService;
            this.catIncotermRepository = catIncotermRepository;
            this.catCommodityRepo = catCommodity;
            this.catCommodityGroupRepo = catCommodityGroup;
        }

        private List<sp_GetDataExportAccountant> GetDataExportAccountant(GeneralReportCriteria criteria)
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
            var list = ((eFMSDataContext)DC).ExecuteProcedure<sp_GetDataExportAccountant>(parameters);
            return list;
        }

        private IQueryable<AccountingPlSheetExportResult> AcctPLSheetDocumentation(GeneralReportCriteria criteria, List<sp_GetDataExportAccountant> dataExportAccountants)
        {
            // Filter data without customerId
            var criteriaNoCustomer = (GeneralReportCriteria)criteria.Clone();
            criteriaNoCustomer.CustomerId = null;
            if (!dataExportAccountants.Any()) return null;
            var lstPartner = catPartnerRepo.Get();
            var lstCharge = catChargeRepo.Get();
            var detailLookupPartner = lstPartner.ToLookup(q => q.Id);
            var detailLookupCharge = lstCharge.ToLookup(q => q.Id);
            var dataCustom = customsDeclarationRepo.Get();
            var LookupUser = sysUserRepo.Get().ToLookup(x => x.Id);
            List<AccountingPlSheetExportResult> dataList = new List<AccountingPlSheetExportResult>();
            foreach (var charge in dataExportAccountants)
            {
                AccountingPlSheetExportResult data = new AccountingPlSheetExportResult();
                data.ServiceDate = charge.ServiceDate;
                data.JobId = charge.JobNo;
                data.CustomNo = charge.ClearanceNo;
                var _taxInvNoRevenue = string.Empty;
                var _voucherRevenue = string.Empty;
                decimal? _usdRevenue = 0;
                decimal? _vndRevenue = 0;
                decimal? _taxOut = 0;
                decimal? _totalRevenue = 0;
                if (charge.Type == ReportConstants.CHARGE_SELL_TYPE)
                {
                    _taxInvNoRevenue = !string.IsNullOrEmpty(charge.InvoiceNo) ? charge.InvoiceNo : charge.DebitNo;
                    _usdRevenue = charge.AmountUsd;
                    _vndRevenue = charge.AmountVnd;
                    if (criteria.Currency == ReportConstants.CURRENCY_USD)
                    {
                        _taxOut = charge.VatAmountUsd;
                        _totalRevenue = charge.AmountUsd + _taxOut;

                    }
                    if (criteria.Currency == ReportConstants.CURRENCY_LOCAL)
                    {
                        _taxOut = charge.VatAmountVnd;
                        _totalRevenue = charge.AmountVnd + _taxOut;
                    }
                    data.CdNote = charge.DebitNo;
                    _voucherRevenue = charge.VoucherId;
                }
                data.TaxInvNoRevenue = _taxInvNoRevenue;
                data.VoucherIdRevenue = _voucherRevenue;
                data.UsdRevenue = _usdRevenue;
                data.VndRevenue = _vndRevenue;
                data.TaxOut = _taxOut;
                data.TotalRevenue = _totalRevenue;

                var _taxInvNoCost = string.Empty;
                var _voucherCost = string.Empty;
                decimal? _usdCost = 0;
                decimal? _vndCost = 0;
                decimal? _taxIn = 0;
                decimal? _totalCost = 0;
                if (charge.Type == ReportConstants.CHARGE_BUY_TYPE)
                {
                    _taxInvNoCost = !string.IsNullOrEmpty(charge.InvoiceNo) ? charge.InvoiceNo : charge.CreditNo;
                    _vndCost = charge.AmountVnd;
                    _usdCost = charge.AmountUsd;
                    if (criteria.Currency == ReportConstants.CURRENCY_USD)
                    {
                        _taxIn = charge.VatAmountUsd;
                        _totalCost = charge.AmountUsd + _taxIn;


                    }
                    if (criteria.Currency == ReportConstants.CURRENCY_LOCAL)
                    {
                        _taxIn = charge.VatAmountVnd;
                        _totalCost = charge.AmountVnd + _taxIn;
                    }
                    data.CdNote = charge.CreditNo;
                    _voucherCost = charge.VoucherId;
                }
                data.TaxInvNoCost = _taxInvNoCost;
                data.VoucherIdCost = _voucherCost;
                data.UsdCost = _usdCost;
                data.VndCost = _vndCost;
                data.TaxIn = _taxIn;
                data.TotalCost = _totalCost;

                if (criteria.Currency == ReportConstants.CURRENCY_LOCAL)
                {
                    if (charge.KickBack == true)
                    {
                        data.TotalKickBack = charge.AmountVnd;
                    }
                }
                if (criteria.Currency == ReportConstants.CURRENCY_USD)
                {
                    if (charge.KickBack == true)
                    {
                        data.TotalKickBack = charge.AmountUsd;
                    }
                }
                data.ExchangeRate = charge.FinalExchangeRate;
                // [CR: cột Balance chỉ tính cho total selling - total buying]
                data.Balance = _totalRevenue - _totalCost;
                data.InvNoObh = charge.Type == ReportConstants.CHARGE_OBH_TYPE ? charge.InvoiceNo : string.Empty;

                if (charge.Type == ReportConstants.CHARGE_OBH_TYPE)
                {
                    var _mapCharge = mapper.Map<CsShipmentSurcharge>(charge);
                    data.OBHNetAmount = currencyExchangeService.ConvertNetAmountChargeToNetAmountObj(_mapCharge, criteria.Currency); // Amount trước thuế OBH
                    data.AmountObh = currencyExchangeService.ConvertAmountChargeToAmountObj(_mapCharge, criteria.Currency); //Amount sau thuế của phí OBH
                    data.CdNote = charge.DebitNo;
                }
                data.AcVoucherNo = string.Empty;
                data.PmVoucherNo = charge.Type == ReportConstants.CHARGE_OBH_TYPE ? charge.VoucherId : string.Empty; //Voucher của phí OBH theo Payee
                data.Service = API.Common.Globals.CustomData.Services.Where(x => x.Value == charge.Service).FirstOrDefault()?.DisplayName;
                data.UserExport = currentUser.UserName;
                data.CurrencyId = charge.CurrencyId;
                data.ExchangeDate = charge.ExchangeDate;
                data.Mbl = charge.MAWB;
                data.Hbl = charge.HWBNo;
                data.PartnerCode = detailLookupPartner[charge.PartnerId].FirstOrDefault()?.AccountNo;
                data.PartnerName = detailLookupPartner[charge.PartnerId].FirstOrDefault()?.PartnerNameEn;
                data.PartnerTaxCode = detailLookupPartner[charge.PartnerId].FirstOrDefault()?.TaxCode;
                data.ChargeCode = detailLookupCharge[charge.ChargeId].FirstOrDefault()?.Code;
                data.ChargeName = detailLookupCharge[charge.ChargeId].FirstOrDefault()?.ChargeNameEn;
                data.Creator = LookupUser[charge.UserCreated].Select(t => t.Username).FirstOrDefault();
                data.SyncedFrom = charge.SyncedFrom;
                data.VatPartnerName = detailLookupPartner[charge.VatPartnerID].FirstOrDefault()?.ShortName;
                //data.BillNoSynced = getBillNoSynced(charge);
                data.BillNoSynced = charge.BillNoSynced;
                data.PaySyncedFrom = charge.PaySyncedFrom;
                data.PayBillNoSynced = charge.PayBillNoSynced;
                data.Quantity = charge.Quantity;
                data.UnitPrice = charge.UnitPrice;
                dataList.Add(data);
            }
            return dataList.AsQueryable();
        }

        public IQueryable<AccountingPlSheetExportResult> GetDataAccountingPLSheet(GeneralReportCriteria criteria)
        {
            var list = GetDataExportAccountant(criteria);
            var dataDocumentation = AcctPLSheetDocumentation(criteria, list);
            return dataDocumentation;
        }

        private Expression<Func<CsTransaction, bool>> GetQueryTransationDocumentation(GeneralReportCriteria criteria)
        {
            Expression<Func<CsTransaction, bool>> queryTrans;
            // ServiceDate/DatetimeCreated Search
            if (criteria.ServiceDateFrom != null && criteria.ServiceDateTo != null)
            {
                // [CR: 15857]
                //queryTrans = q =>
                //    q.TransactionType.Contains("E") ?
                //    (q.Etd.HasValue ? q.Etd.Value.Date >= criteria.ServiceDateFrom.Value.Date && q.Etd.Value.Date <= criteria.ServiceDateTo.Value.Date : false)
                //    :
                //    (q.Eta.HasValue ? q.Eta.Value.Date >= criteria.ServiceDateFrom.Value.Date && q.Eta.Value.Date <= criteria.ServiceDateTo.Value.Date : false);
                queryTrans = q =>
                    q.ServiceDate.HasValue ? (criteria.ServiceDateFrom.Value.Date <= q.ServiceDate.Value.Date && q.ServiceDate.Value.Date <= criteria.ServiceDateTo.Value.Date) : false;
            }
            else
            {
                queryTrans = q =>
                    q.DatetimeCreated.HasValue ? q.DatetimeCreated.Value.Date >= criteria.CreatedDateFrom.Value.Date && q.DatetimeCreated.Value.Date <= criteria.CreatedDateTo.Value.Date : false;
            }
            // Search Service
            if (!string.IsNullOrEmpty(criteria.Service))
            {
                queryTrans = queryTrans.And(q => criteria.Service.Contains(q.TransactionType));
            }
            // Search JobId
            if (!string.IsNullOrEmpty(criteria.JobId))
            {
                queryTrans = queryTrans.And(q => criteria.JobId.Contains(q.JobNo));
            }
            // Search Mawb
            if (!string.IsNullOrEmpty(criteria.Mawb))
            {
                queryTrans = queryTrans.And(q => criteria.Mawb.Contains(q.Mawb));
            }

            var hasSalesman = criteria.SalesMan != null; // Check if Type = Salesman
            if (!hasSalesman)
            {
                // Search Office
                if (!string.IsNullOrEmpty(criteria.OfficeId))
                {
                    queryTrans = queryTrans.And(q => criteria.OfficeId.Contains(q.OfficeId.ToString()));
                }
                // Search Department
                if (!string.IsNullOrEmpty(criteria.DepartmentId))
                {
                    queryTrans = queryTrans.And(q => criteria.DepartmentId.Contains(q.DepartmentId.ToString()));
                }
                // Search Group
                if (!string.IsNullOrEmpty(criteria.GroupId))
                {
                    queryTrans = queryTrans.And(q => criteria.GroupId.Contains(q.GroupId.ToString()));
                }
            }
            // Search Person In Charge
            if (!string.IsNullOrEmpty(criteria.PersonInCharge))
            {
                queryTrans = queryTrans.And(q => criteria.PersonInCharge.Contains(q.PersonIncharge));
            }
            // Search Creator
            if (!string.IsNullOrEmpty(criteria.Creator))
            {
                queryTrans = queryTrans.And(q => criteria.Creator.Contains(q.UserCreated));
            }
            // Search Carrier
            if (!string.IsNullOrEmpty(criteria.CarrierId))
            {
                queryTrans = queryTrans.And(q => q.ColoaderId == criteria.CarrierId);
            }
            // Search Agent
            if (!string.IsNullOrEmpty(criteria.AgentId))
            {
                queryTrans = queryTrans.And(q => q.AgentId == criteria.AgentId);
            }
            // Search Pol
            if (criteria.Pol != null && criteria.Pol != Guid.Empty)
            {
                queryTrans = queryTrans.And(q => q.Pol == criteria.Pol);
            }
            // Search Pod
            if (criteria.Pod != null && criteria.Pod != Guid.Empty)
            {
                queryTrans = queryTrans.And(q => q.Pod == criteria.Pod);
            }
            //// Search CustomerId
            //if (!string.IsNullOrEmpty(criteria.CustomerId))
            //{
            //    queryTrans = queryTrans.And(q => q.AgentId == criteria.CustomerId);
            //}
            return queryTrans;
        }

        private Expression<Func<CsTransactionDetail, bool>> GetQueryTransationDetailDocumentation(GeneralReportCriteria criteria)
        {
            Expression<Func<CsTransactionDetail, bool>> queryTranDetail = null;
            // Search Customer
            if (!string.IsNullOrEmpty(criteria.CustomerId))
            {
                queryTranDetail = q => criteria.CustomerId.Contains(q.CustomerId);
            }
            // Search Hawb
            if (!string.IsNullOrEmpty(criteria.Hawb))
            {
                queryTranDetail = queryTranDetail == null ?
                    (q => criteria.Hawb.Contains(q.Hwbno))
                    :
                    queryTranDetail.And(q => criteria.Hawb.Contains(q.Hwbno));
            }
            var hasSalesman = criteria.SalesMan != null; // Check if Type = Salesman
            if (hasSalesman)
            {
                // Search SalesOffice
                if (!string.IsNullOrEmpty(criteria.OfficeId))
                {
                    queryTranDetail = (queryTranDetail == null) ? (q => !string.IsNullOrEmpty(q.SalesOfficeId))
                                                                : queryTranDetail.And(q => !string.IsNullOrEmpty(q.SalesOfficeId));
                }
                // Search SalesDepartment
                if (!string.IsNullOrEmpty(criteria.DepartmentId))
                {
                    queryTranDetail = (queryTranDetail == null) ? (q => !string.IsNullOrEmpty(q.SalesDepartmentId))
                                                                : queryTranDetail.And(q => !string.IsNullOrEmpty(q.SalesDepartmentId));
                }
                // Search SalesGroup
                if (!string.IsNullOrEmpty(criteria.GroupId))
                {
                    queryTranDetail = (queryTranDetail == null) ? (q => !string.IsNullOrEmpty(q.SalesGroupId))
                                                                : queryTranDetail.And(q => !string.IsNullOrEmpty(q.SalesGroupId));
                }
                // Search SaleMan
                if (!string.IsNullOrEmpty(criteria.SalesMan))
                {
                    queryTranDetail = (queryTranDetail == null) ?
                        (q => criteria.SalesMan.Contains(q.SaleManId))
                        :
                        queryTranDetail.And(q => criteria.SalesMan.Contains(q.SaleManId));
                }
            }
            return queryTranDetail;
        }

        private IQueryable<SummaryOfCostsIncurredExportResult> QueryDataSummaryOfCostsIncurred(GeneralReportCriteria criteria)
        {
            // Filter data without customerId
            var criteriaNoCustomer = (GeneralReportCriteria)criteria.Clone();
            criteriaNoCustomer.CustomerId = null;
            Expression<Func<CsTransaction, bool>> queryTrans = GetQueryTransationDocumentation(criteriaNoCustomer);
            Expression<Func<CsTransactionDetail, bool>> queryTranDetail = GetQueryTransationDetailDocumentation(criteriaNoCustomer);

            var masterBills = DC.CsTransaction.Where(x => x.CurrentStatus != ReportConstants.CURRENT_STATUS_CANCELED).Where(queryTrans);
            if (queryTranDetail == null)
            {
                var houseBills = detailRepository.Get();
                var queryShipment = from master in masterBills
                                    join house in houseBills on master.Id equals house.JobId
                                    select new SummaryOfCostsIncurredExportResult
                                    {
                                        JobId = master.JobNo,
                                        ServiceDate = master.ServiceDate,
                                        Service = master.TransactionType,
                                        HBLID = house.Id,
                                        PurchaseOrderNo = master.Pono,
                                        AOL = master.Pol,

                                    };

                return queryShipment.AsQueryable();
            }
            else
            {
                var houseBills = GetTransactionDetailDocWithSalesman(queryTranDetail, criteria);
                var queryShipment = from master in masterBills
                                    join house in houseBills on master.Id equals house.JobId
                                    select new SummaryOfCostsIncurredExportResult
                                    {
                                        JobId = master.JobNo,
                                        ServiceDate = master.ServiceDate,
                                        Service = master.TransactionType,
                                        HBLID = house.Id,
                                        PurchaseOrderNo = master.Pono,
                                        AOL = master.Pol
                                    };
                return queryShipment;
            }
        }

        private IQueryable<CsTransactionDetail> GetTransactionDetailDocWithSalesman(Expression<Func<CsTransactionDetail, bool>> queryTranDetail, GeneralReportCriteria criteria)
        {
            var houseBills = detailRepository.Get().Where(queryTranDetail);
            var houseBillList = new List<CsTransactionDetail>();
            if (criteria.SalesMan != null)
            {
                foreach (var house in houseBills)
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
                houseBills = houseBillList.AsQueryable();
            }
            return houseBills;
        }

        private SummaryOfRevenueModel SummaryOfCostsByPartner(GeneralReportCriteria criteria, IQueryable<SummaryOfCostsIncurredExportResult> chargeData)
        {
            var dataShipment = QueryDataSummaryOfCostsIncurred(criteria);
            if (dataShipment == null) return null;
            SummaryOfRevenueModel ObjectSummaryRevenue = new SummaryOfRevenueModel();
            List<SummaryOfRevenueExportResult> dataList = new List<SummaryOfRevenueExportResult>();
            var results = chargeData.AsEnumerable().GroupBy(x => new { x.JobId, x.HBLID });
            if (results == null) return null;
            var lookupReuslts = results.ToLookup(q => q.Key.HBLID);
            var listPartner = catPartnerRepo.Get();
            var lookupPartner = listPartner.ToLookup(q => q.Id);
            var dataCustom = customsDeclarationRepo.Get().ToList();
            var port = catPlaceRepo.Get();
            foreach (var item in dataShipment)
            {
                if (item.HBLID != Guid.Empty)
                {
                    foreach (var group in lookupReuslts[item.HBLID])
                    {
                        SummaryOfRevenueExportResult SummaryRevenue = new SummaryOfRevenueExportResult();
                        SummaryRevenue.SummaryOfCostsIncurredExportResults = new List<SummaryOfCostsIncurredExportResult>();
                        var commodity = DC.CsTransaction.Where(x => x.JobNo == group.Key.JobId).Select(t => t.Commodity).FirstOrDefault();
                        var commodityGroup = opsRepository.Get(x => x.JobNo == group.Key.JobId).Select(t => t.CommodityGroupId).FirstOrDefault();
                        string commodityName = string.Empty;
                        var _partnerId = group.Select(t => t.CustomerID).FirstOrDefault();

                        if (commodity != null)
                        {
                            string[] commodityArr = commodity.Split(',');
                            foreach (var it in commodityArr)
                            {
                                if (catCommodityRepo.Any(x => x.CommodityNameEn == it.Replace("\n", "")))
                                {
                                    commodityName = commodityName + "," + catCommodityRepo.Get(x => x.CommodityNameEn == it.Replace("\n", "")).Select(t => t.CommodityNameEn).FirstOrDefault();
                                }
                                else
                                {
                                    commodityName = commodityName + "," + catCommodityRepo.Get(x => x.Code == it.Replace("\n", "")).Select(t => t.CommodityNameEn).FirstOrDefault();
                                }
                            }
                            commodityName = commodityName.Substring(1);
                        }
                        if (commodityGroup != null)
                        {
                            commodityName = catCommodityGroupRepo.Get(x => x.Id == commodityGroup).Select(t => t.GroupNameVn).FirstOrDefault();
                        }
                        SummaryRevenue.ChargeName = commodityName;
                        SummaryRevenue.POLName = port.Where(x => x.Id == item.AOL).Select(t => t.NameEn).FirstOrDefault();
                        SummaryRevenue.CustomNo = GetTopClearanceNoByJobNo(group.Key.JobId, dataCustom);
                        SummaryRevenue.HBL = group.Select(t => t.HBL).FirstOrDefault();
                        SummaryRevenue.CBM = group.Select(t => t.CBM).FirstOrDefault();
                        SummaryRevenue.GrossWeight = group.Select(t => t.GrossWeight).FirstOrDefault();
                        SummaryRevenue.PackageContainer = group.Select(t => t.PackageContainer).FirstOrDefault();
                        foreach (var ele in group)
                        {
                            ele.SuplierName = lookupPartner[ele.CustomerID].Select(t => t.PartnerNameVn).FirstOrDefault();
                        }
                        foreach (var partner in lookupPartner[_partnerId])
                        {
                            SummaryRevenue.SupplierCode = partner?.AccountNo;
                            SummaryRevenue.SuplierName = partner?.PartnerNameVn;
                        }

                        SummaryRevenue.SummaryOfCostsIncurredExportResults.AddRange(group.Select(t => t));
                        dataList.Add(SummaryRevenue);
                    }
                }
            }
            ObjectSummaryRevenue.summaryOfRevenueExportResults = dataList;
            foreach (var item in ObjectSummaryRevenue.summaryOfRevenueExportResults)
            {
                foreach (var it in item.SummaryOfCostsIncurredExportResults)
                {
                    if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                    {
                        it.VATAmount = it.VATAmountUSD;
                        it.NetAmount = it.AmountUSD;
                    }
                    else
                    {
                        it.VATAmount = it.VATAmountVND;
                        it.NetAmount = it.AmountVND;
                    }
                    if (it.VATRate > 0)
                    {
                        it.VATAmount = (it.VATRate * it.NetAmount) / 100;
                    }
                    else
                    {
                        it.VATAmount = it.VATRate != null ? Math.Abs(it.VATRate.Value) : 0;
                        if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                        {
                            it.VATAmount = it.VATAmountUSD;
                        }
                        else
                        {
                            it.VATAmount = it.VATAmountVND;
                        }

                    }

                }
            }
            return ObjectSummaryRevenue;
        }

        private IQueryable<SummaryOfCostsIncurredExportResult> GetChargeOBHSellPayee(Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query, bool? isOBH)
        {
            var surcharge = surCharge.Get(x => x.Type == ReportConstants.CHARGE_OBH_TYPE || x.Type == ReportConstants.CHARGE_BUY_TYPE);
            var opst = opsRepository.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != null && x.CurrentStatus != TermData.Canceled);
            var csTrans = DC.CsTransaction.Where(x => x.CurrentStatus != TermData.Canceled);
            var csTransDe = detailRepository.Get();
            var charge = catChargeRepo.Get();
            //OBH Payer (BUY - Credit)
            var queryObhBuyOperation = from sur in surcharge
                                       join ops in opst on sur.Hblid equals ops.Hblid
                                       join chg in charge on sur.ChargeId equals chg.Id into chg2
                                       from chg in chg2.DefaultIfEmpty()
                                       select new SummaryOfCostsIncurredExportResult
                                       {
                                           ID = sur.Id,
                                           HBLID = sur.Hblid,
                                           ChargeID = sur.ChargeId,
                                           ChargeCode = chg.Code,
                                           ChargeName = chg.ChargeNameEn,
                                           JobId = ops.JobNo,
                                           HBL = ops.Hwbno,
                                           MBL = ops.Mblno,
                                           Type = sur.Type + "-SELL",
                                           Debit = null,
                                           Credit = sur.Total,
                                           IsOBH = true,
                                           Currency = sur.CurrencyId,
                                           InvoiceNo = sur.InvoiceNo,
                                           Note = sur.Notes,
                                           CustomerID = sur.Type == "OBH" ? sur.PayerId : sur.PaymentObjectId,
                                           ServiceDate = ops.ServiceDate,
                                           CreatedDate = ops.DatetimeCreated,
                                           TransactionType = null,
                                           UserCreated = ops.UserCreated,
                                           Quantity = sur.Quantity,
                                           UnitId = sur.UnitId,
                                           UnitPrice = sur.UnitPrice,
                                           VATRate = sur.Vatrate,
                                           CreditDebitNo = sur.CreditNo,
                                           CommodityGroupID = ops.CommodityGroupId,
                                           Service = "CL",
                                           ExchangeDate = sur.ExchangeDate,
                                           FinalExchangeRate = sur.FinalExchangeRate,
                                           TypeCharge = chg.Type,
                                           PayerId = sur.PayerId,
                                           VATAmountUSD = sur.VatAmountUsd,
                                           VATAmountVND = sur.VatAmountVnd,
                                           AmountUSD = sur.AmountUsd,
                                           AmountVND = sur.AmountVnd

                                       };
            if (query != null)
            {
                queryObhBuyOperation = queryObhBuyOperation.Where(x => !string.IsNullOrEmpty(x.CustomerID)).Where(query);
            }
            if (isOBH != null)
            {
                queryObhBuyOperation = queryObhBuyOperation.Where(x => x.IsOBH == isOBH);
            }
            var queryObhBuyDocument = from sur in surcharge
                                      join cstd in csTransDe on sur.Hblid equals cstd.Id
                                      join cst in csTrans on cstd.JobId equals cst.Id
                                      join chg in charge on sur.ChargeId equals chg.Id into chg2
                                      from chg in chg2.DefaultIfEmpty()
                                      select new SummaryOfCostsIncurredExportResult
                                      {
                                          ID = sur.Id,
                                          HBLID = sur.Hblid,
                                          ChargeID = sur.ChargeId,
                                          ChargeCode = chg.Code,
                                          ChargeName = chg.ChargeNameEn,
                                          JobId = cst.JobNo,
                                          HBL = cstd.Hwbno,
                                          MBL = cst.Mawb,
                                          Type = sur.Type + "-SELL",
                                          Debit = null,
                                          Credit = sur.Total,
                                          IsOBH = true,
                                          Currency = sur.CurrencyId,
                                          InvoiceNo = sur.InvoiceNo,
                                          Note = sur.Notes,
                                          CustomerID = sur.Type == "OBH" ? sur.PayerId : sur.PaymentObjectId,
                                          ServiceDate = cst.ServiceDate,
                                          CreatedDate = cst.DatetimeCreated,
                                          TransactionType = cst.TransactionType,
                                          UserCreated = cst.UserCreated,
                                          Quantity = sur.Quantity,
                                          UnitId = sur.UnitId,
                                          UnitPrice = sur.UnitPrice,
                                          VATRate = sur.Vatrate,
                                          CreditDebitNo = sur.CreditNo,
                                          CommodityGroupID = null,
                                          Service = cst.TransactionType,
                                          CBM = cstd.Cbm,
                                          Commodity = cst.Commodity,
                                          FlightNo = cstd.FlightNo,
                                          ShippmentDate = cst.TransactionType == "AE" ? cstd.Etd : cst.TransactionType == "AI" ? cstd.Eta : null,
                                          AOL = cst.Pol,
                                          AOD = cst.Pod,
                                          PackageQty = cstd.PackageQty,
                                          GrossWeight = cstd.GrossWeight,
                                          ChargeWeight = cstd.ChargeWeight,
                                          FinalExchangeRate = sur.FinalExchangeRate,
                                          ExchangeDate = sur.ExchangeDate,
                                          PackageContainer = cstd.PackageContainer,
                                          TypeCharge = chg.Type,
                                          PayerId = sur.PayerId,
                                          VATAmountUSD = sur.VatAmountUsd,
                                          VATAmountVND = sur.VatAmountVnd,
                                          AmountUSD = sur.AmountUsd,
                                          AmountVND = sur.AmountVnd
                                      };
            if (query != null)
            {
                queryObhBuyDocument = queryObhBuyDocument.Where(x => !string.IsNullOrEmpty(x.CustomerID)).Where(query);
            }
            if (isOBH != null)
            {
                queryObhBuyDocument = queryObhBuyDocument.Where(x => x.IsOBH == isOBH);
            }
            var queryObhBuy = queryObhBuyOperation.Union(queryObhBuyDocument);
            return queryObhBuy;
        }

        private IQueryable<SummaryOfCostsIncurredExportResult> GetChargeOBHPayee(Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query, bool? isOBH)
        {
            var surcharge = surCharge.Get(x => x.Type == ReportConstants.CHARGE_OBH_TYPE || x.Type == ReportConstants.CHARGE_BUY_TYPE);
            var opst = opsRepository.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != null && x.CurrentStatus != TermData.Canceled);
            var csTrans = DC.CsTransaction.Where(x => x.CurrentStatus != TermData.Canceled);
            var csTransDe = detailRepository.Get();
            var charge = catChargeRepo.Get();
            //OBH Payer (BUY - Credit)
            var queryObhBuyOperation = from sur in surcharge
                                       join ops in opst on sur.Hblid equals ops.Hblid
                                       join chg in charge on sur.ChargeId equals chg.Id into chg2
                                       from chg in chg2.DefaultIfEmpty()
                                       select new SummaryOfCostsIncurredExportResult
                                       {
                                           ID = sur.Id,
                                           HBLID = sur.Hblid,
                                           ChargeID = sur.ChargeId,
                                           ChargeCode = chg.Code,
                                           ChargeName = chg.ChargeNameEn,
                                           JobId = ops.JobNo,
                                           HBL = ops.Hwbno,
                                           MBL = ops.Mblno,
                                           Type = sur.Type,
                                           Debit = null,
                                           Credit = sur.Total,
                                           IsOBH = true,
                                           Currency = sur.CurrencyId,
                                           InvoiceNo = sur.InvoiceNo,
                                           InvoiceDate = sur.InvoiceDate,
                                           Note = sur.Notes,
                                           CustomerID = sur.Type == "OBH" ? sur.PayerId : sur.PaymentObjectId,
                                           ServiceDate = ops.ServiceDate,
                                           CreatedDate = ops.DatetimeCreated,
                                           TransactionType = null,
                                           UserCreated = ops.UserCreated,
                                           Quantity = sur.Quantity,
                                           UnitId = sur.UnitId,
                                           UnitPrice = sur.UnitPrice,
                                           VATRate = sur.Vatrate,
                                           CreditDebitNo = sur.CreditNo,
                                           CommodityGroupID = ops.CommodityGroupId,
                                           Service = "CL",
                                           ExchangeDate = sur.ExchangeDate,
                                           FinalExchangeRate = sur.FinalExchangeRate,
                                           TypeCharge = chg.Type,
                                           PayerId = sur.PayerId,
                                           VATAmountUSD = sur.VatAmountUsd,
                                           VATAmountVND = sur.VatAmountVnd,
                                           AmountUSD = sur.AmountUsd,
                                           AmountVND = sur.AmountVnd

                                       };
            if (query != null)
            {
                queryObhBuyOperation = queryObhBuyOperation.Where(x => !string.IsNullOrEmpty(x.CustomerID)).Where(query);
            }
            if (isOBH != null)
            {
                queryObhBuyOperation = queryObhBuyOperation.Where(x => x.IsOBH == isOBH);
            }
            var queryObhBuyDocument = from sur in surcharge
                                      join cstd in csTransDe on sur.Hblid equals cstd.Id
                                      join cst in csTrans on cstd.JobId equals cst.Id
                                      join chg in charge on sur.ChargeId equals chg.Id into chg2
                                      from chg in chg2.DefaultIfEmpty()
                                      select new SummaryOfCostsIncurredExportResult
                                      {
                                          ID = sur.Id,
                                          HBLID = sur.Hblid,
                                          ChargeID = sur.ChargeId,
                                          ChargeCode = chg.Code,
                                          ChargeName = chg.ChargeNameEn,
                                          JobId = cst.JobNo,
                                          HBL = cstd.Hwbno,
                                          MBL = cst.Mawb,
                                          Type = sur.Type,
                                          Debit = null,
                                          Credit = sur.Total,
                                          IsOBH = true,
                                          Currency = sur.CurrencyId,
                                          InvoiceNo = sur.InvoiceNo,
                                          Note = sur.Notes,
                                          CustomerID = sur.Type == "OBH" ? sur.PayerId : sur.PaymentObjectId,
                                          ServiceDate = cst.ServiceDate,
                                          CreatedDate = cst.DatetimeCreated,
                                          TransactionType = cst.TransactionType,
                                          UserCreated = cst.UserCreated,
                                          Quantity = sur.Quantity,
                                          UnitId = sur.UnitId,
                                          UnitPrice = sur.UnitPrice,
                                          VATRate = sur.Vatrate,
                                          CreditDebitNo = sur.CreditNo,
                                          CommodityGroupID = null,
                                          Service = cst.TransactionType,
                                          CBM = cstd.Cbm,
                                          Commodity = cst.Commodity,
                                          FlightNo = cstd.FlightNo,
                                          ShippmentDate = cst.TransactionType == "AE" ? cstd.Etd : cst.TransactionType == "AI" ? cstd.Eta : null,
                                          AOL = cst.Pol,
                                          AOD = cst.Pod,
                                          PackageQty = cstd.PackageQty,
                                          GrossWeight = cstd.GrossWeight,
                                          ChargeWeight = cstd.ChargeWeight,
                                          FinalExchangeRate = sur.FinalExchangeRate,
                                          ExchangeDate = sur.ExchangeDate,
                                          PackageContainer = cstd.PackageContainer,
                                          TypeCharge = chg.Type,
                                          PayerId = sur.PayerId,
                                          VATAmountUSD = sur.VatAmountUsd,
                                          VATAmountVND = sur.VatAmountVnd,
                                          AmountUSD = sur.AmountUsd,
                                          AmountVND = sur.AmountVnd,
                                          InvoiceDate = sur.InvoiceDate,
                                      };
            if (query != null)
            {
                queryObhBuyDocument = queryObhBuyDocument.Where(x => !string.IsNullOrEmpty(x.CustomerID)).Where(query);
            }
            if (isOBH != null)
            {
                queryObhBuyDocument = queryObhBuyDocument.Where(x => x.IsOBH == isOBH);
            }
            var queryObhBuy = queryObhBuyOperation.Union(queryObhBuyDocument);
            return queryObhBuy;
        }


        private IQueryable<SummaryOfCostsIncurredExportResult> SummaryOfCostsIncurred(GeneralReportCriteria criteria)
        {
            var dataShipment = QueryDataSummaryOfCostsIncurred(criteria);
            if (dataShipment == null) return null;
            var port = catPlaceRepo.Get();
            List<SummaryOfCostsIncurredExportResult> dataList = new List<SummaryOfCostsIncurredExportResult>();
            Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query = chg => criteria.CustomerId.Contains(chg.CustomerID);
            var chargeData = !string.IsNullOrEmpty(criteria.CustomerId) ? GetChargeOBHSellPayee(query, null) : GetChargeOBHSellPayee(null, null);
            var detailLookupSur = chargeData.ToLookup(q => q.HBLID);
            var dataCustom = customsDeclarationRepo.Get().ToList();
            var partnerData = catPartnerRepo.Get();
            var detailLookupPartner = partnerData.ToLookup(q => q.Id);
            var DetailLookupPort = port.ToLookup(q => q.Id);
            foreach (var item in dataShipment)
            {
                if (item.HBLID != null && item.HBLID != Guid.Empty)
                {
                    foreach (var charge in detailLookupSur[item.HBLID])
                    {
                        SummaryOfCostsIncurredExportResult data = new SummaryOfCostsIncurredExportResult();
                        var _partnerId = charge.TypeCharge == "OBH" ? charge.PayerId : charge.CustomerID;
                        data.PurchaseOrderNo = item.PurchaseOrderNo;
                        data.CustomNo = GetTopClearanceNoByJobNo(item.JobId, dataCustom);
                        data.HBL = charge.HBL;
                        data.GrossWeight = charge.GrossWeight;
                        data.CBM = charge.CBM;
                        data.PackageContainer = charge.PackageContainer;
                        if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                        {
                            charge.NetAmount = charge.AmountUSD;
                            charge.VATAmount = charge.VATAmountUSD;
                        }
                        else
                        {
                            charge.NetAmount = charge.AmountVND;
                            charge.VATAmount = charge.VATAmountVND;
                        }

                        foreach (var partner in detailLookupPartner[_partnerId])
                        {
                            data.SupplierCode = partner?.AccountNo;
                            data.SuplierName = partner?.PartnerNameVn;
                            data.ChargeName = charge.ChargeName;
                        }
                        if (charge.AOL != Guid.Empty && charge.AOL != null)
                        {
                            foreach (var Port in DetailLookupPort[(Guid)charge.AOL])
                            {
                                data.POLName = Port.NameEn;
                            }
                        }
                        data.NetAmount = charge.NetAmount;
                        data.VATAmount = charge.VATAmount;
                        data.Type = charge.Type;
                        data.TypeCharge = charge.TypeCharge;
                        dataList.Add(data);
                    }
                }

            }
            return dataList.AsQueryable();
        }

        private IQueryable<SummaryOfCostsIncurredExportResult> SummaryOfCostsIncurredOperation(GeneralReportCriteria criteria)
        {
            var dataShipment = QueryDataOperationCost(criteria, null);
            if (dataShipment == null) return null;
            var port = catPlaceRepo.Get();
            List<SummaryOfCostsIncurredExportResult> dataList = new List<SummaryOfCostsIncurredExportResult>();
            Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query = chg => criteria.CustomerId.Contains(chg.CustomerID);
            var chargeData = !string.IsNullOrEmpty(criteria.CustomerId) ? GetChargeOBHSellPayee(query, null) : GetChargeOBHSellPayee(null, null);
            var detailLookupSur = chargeData.ToLookup(q => q.HBLID);
            var dataCustom = customsDeclarationRepo.Get().ToList();
            var partnerData = catPartnerRepo.Get();
            var detailLookupPartner = partnerData.ToLookup(q => q.Id);
            var DetailLookupPort = port.ToLookup(q => q.Id);
            foreach (var item in dataShipment)
            {
                if (item.Hblid != null && item.Hblid != Guid.Empty)
                {
                    foreach (var charge in detailLookupSur[item.Hblid])
                    {
                        SummaryOfCostsIncurredExportResult data = new SummaryOfCostsIncurredExportResult();
                        var _partnerId = charge.TypeCharge == "OBH" ? charge.PayerId : charge.CustomerID;
                        data.PurchaseOrderNo = item.PurchaseOrderNo;
                        data.CustomNo = GetTopClearanceNoByJobNo(item.JobNo, dataCustom);
                        data.HBL = charge.HBL;
                        data.GrossWeight = charge.GrossWeight;
                        data.CBM = charge.CBM;
                        data.PackageContainer = charge.PackageContainer;
                        foreach (var partner in detailLookupPartner[_partnerId])
                        {
                            data.SupplierCode = partner?.AccountNo;
                            data.SuplierName = partner?.PartnerNameVn;
                        }
                        if (charge.AOL != Guid.Empty && charge.AOL != null)
                        {
                            foreach (var Port in DetailLookupPort[(Guid)charge.AOL])
                            {
                                data.POLName = Port.NameEn;
                            }
                        }
                        data.ChargeName = charge.ChargeName;
                        if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                        {
                            charge.NetAmount = charge.AmountUSD;
                            charge.VATAmount = charge.VATAmountUSD;
                        }
                        else
                        {
                            charge.NetAmount = charge.AmountVND;
                            charge.VATAmount = charge.VATAmountVND;
                        }
                        data.NetAmount = charge.NetAmount;
                        data.VATAmount = charge.VATAmount;
                        data.Type = charge.Type;
                        data.TypeCharge = charge.TypeCharge;
                        dataList.Add(data);
                    }
                }

            }
            return dataList.AsQueryable();
        }

        private string GetTopClearanceNoByJobNo(string JobNo, List<CustomsDeclaration> customsDeclarations)
        {
            var clearanceNo = customsDeclarations.Where(x => x.JobNo != null && x.JobNo == JobNo)
                .OrderBy(x => x.JobNo)
                .OrderByDescending(x => x.ClearanceDate)
                .FirstOrDefault()?.ClearanceNo;
            return clearanceNo;
        }

        private Expression<Func<OpsTransaction, bool>> GetQueryOPSTransactionOperation(GeneralReportCriteria criteria, bool? fromCost)
        {
            Expression<Func<OpsTransaction, bool>> queryOpsTrans = q => true;
            // ServiceDate/DatetimeCreated Search
            if (criteria.ServiceDateFrom != null && criteria.ServiceDateTo != null)
            {
                queryOpsTrans = q =>
                    q.ServiceDate.HasValue ? q.ServiceDate.Value.Date >= criteria.ServiceDateFrom.Value.Date && q.ServiceDate.Value.Date <= criteria.ServiceDateTo.Value.Date : false;
            }
            else
            {
                queryOpsTrans = q =>
                    q.DatetimeCreated.HasValue ? q.DatetimeCreated.Value.Date >= criteria.CreatedDateFrom.Value.Date && q.DatetimeCreated.Value.Date <= criteria.CreatedDateTo.Value.Date : false;
            }

            queryOpsTrans = queryOpsTrans.And(q => criteria.Service.Contains("CL") || string.IsNullOrEmpty(criteria.Service));
            // Search Customer
            if (!string.IsNullOrEmpty(criteria.CustomerId) && fromCost == true)
            {
                queryOpsTrans = queryOpsTrans.And(q => criteria.CustomerId.Contains(q.CustomerId));
            }
            // Search JobId
            if (!string.IsNullOrEmpty(criteria.JobId))
            {
                queryOpsTrans = queryOpsTrans.And(q => criteria.JobId.Contains(q.JobNo));
            }
            // Search Mawb
            if (!string.IsNullOrEmpty(criteria.Mawb))
            {
                queryOpsTrans = queryOpsTrans.And(q => criteria.Mawb.Contains(q.Mblno));
            }
            // Search Hawb
            if (!string.IsNullOrEmpty(criteria.Hawb))
            {
                queryOpsTrans = queryOpsTrans.And(q => criteria.Hawb.Contains(q.Hwbno));
            }

            var hasSalesman = criteria.SalesMan != null; // Check if Type=Salesman
            // Search Office
            if (!string.IsNullOrEmpty(criteria.OfficeId))
            {
                queryOpsTrans = hasSalesman ? queryOpsTrans.And(q => !string.IsNullOrEmpty(q.SalesOfficeId))
                                            : queryOpsTrans.And(q => criteria.OfficeId.Contains(q.OfficeId.ToString()));
            }
            // Search Department
            if (!string.IsNullOrEmpty(criteria.DepartmentId))
            {
                queryOpsTrans = hasSalesman ? queryOpsTrans.And(q => !string.IsNullOrEmpty(q.SalesDepartmentId))
                                            : queryOpsTrans.And(q => criteria.DepartmentId.Contains(q.DepartmentId.ToString()));
            }
            // Search Group
            if (!string.IsNullOrEmpty(criteria.GroupId))
            {
                queryOpsTrans = hasSalesman ? queryOpsTrans.And(q => !string.IsNullOrEmpty(q.SalesGroupId))
                                            : queryOpsTrans.And(q => criteria.GroupId.Contains(q.GroupId.ToString()));
            }
            // Search Person In Charge
            if (!string.IsNullOrEmpty(criteria.PersonInCharge))
            {
                queryOpsTrans = queryOpsTrans.And(q => criteria.PersonInCharge.Contains(q.BillingOpsId));
            }
            // Search SalesMan
            if (!string.IsNullOrEmpty(criteria.SalesMan))
            {
                queryOpsTrans = queryOpsTrans.And(q => criteria.SalesMan.Contains(q.SalemanId));
            }
            // Search Creator
            if (!string.IsNullOrEmpty(criteria.Creator))
            {
                queryOpsTrans = queryOpsTrans.And(q => criteria.Creator.Contains(q.UserCreated));
            }
            // Search Carrier
            if (!string.IsNullOrEmpty(criteria.CarrierId))
            {
                queryOpsTrans = queryOpsTrans.And(q => q.SupplierId == criteria.CarrierId);
            }
            // Search Agent
            if (!string.IsNullOrEmpty(criteria.AgentId))
            {
                queryOpsTrans = queryOpsTrans.And(q => q.AgentId == criteria.AgentId);
            }
            // Search Pol
            if (criteria.Pol != null && criteria.Pol != Guid.Empty)
            {
                queryOpsTrans = queryOpsTrans.And(q => q.Pol == criteria.Pol);
            }
            // Search Pod
            if (criteria.Pod != null && criteria.Pod != Guid.Empty)
            {
                queryOpsTrans = queryOpsTrans.And(q => q.Pod == criteria.Pod);
            }
            return queryOpsTrans;
        }

        private IQueryable<OpsTransaction> GetOpsTransactionWithSalesman(Expression<Func<OpsTransaction, bool>> query, GeneralReportCriteria criteria)
        {
            var shipments = opsRepository.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != ReportConstants.CURRENT_STATUS_CANCELED);
            shipments = shipments.Where(query);
            var shipmentList = new List<OpsTransaction>();
            if (criteria.SalesMan != null)
            {
                foreach (var shipment in shipments)
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
                shipments = shipmentList.AsQueryable();
            }
            return shipments;
        }

        private IQueryable<OpsTransaction> QueryDataOperationCost(GeneralReportCriteria criteria, bool? fromCost)
        {
            Expression<Func<OpsTransaction, bool>> query = GetQueryOPSTransactionOperation(criteria, null);

            var queryShipment = GetOpsTransactionWithSalesman(query, criteria);
            return queryShipment;
        }

        public IQueryable<SummaryOfCostsIncurredExportResult> GetDataSummaryOfCostsIncurred(GeneralReportCriteria criteria)
        {
            var dataDocumentation = SummaryOfCostsIncurred(criteria);
            IQueryable<SummaryOfCostsIncurredExportResult> list;
            if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains(TermData.CustomLogistic))
            {
                var dataOperation = SummaryOfCostsIncurredOperation(criteria);
                list = dataDocumentation.Union(dataOperation);
            }
            else
            {
                list = dataDocumentation;
            }
            return list;
        }

        private IQueryable<SummaryOfCostsIncurredExportResult> GetChargeOBHSellPayer(Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query, bool? isOBH)
        {
            //Chỉ lấy những phí từ shipment (IsFromShipment = true)
            var surcharge = surCharge.Get(x => x.Type == ReportConstants.CHARGE_OBH_TYPE || x.Type == ReportConstants.CHARGE_SELL_TYPE);
            var csTrans = DC.CsTransaction.Where(x => x.CurrentStatus != TermData.Canceled);
            var csTransDe = detailRepository.Get();
            var charge = catChargeRepo.Get();
            var unit = catUnitRepo.Get();

            var queryObhBuyDocument = from sur in surcharge
                                      join cstd in csTransDe on sur.Hblid equals cstd.Id
                                      join cst in csTrans on cstd.JobId equals cst.Id
                                      join chg in charge on sur.ChargeId equals chg.Id into chg2
                                      from chg in chg2.DefaultIfEmpty()
                                      join uni in unit on sur.UnitId equals uni.Id into uni2
                                      from uni in uni2.DefaultIfEmpty()
                                      select new SummaryOfCostsIncurredExportResult
                                      {
                                          ID = sur.Id,
                                          HBLID = sur.Hblid,
                                          ChargeID = sur.ChargeId,
                                          ChargeCode = chg.Code,
                                          ChargeName = chg.ChargeNameEn,
                                          JobId = cst.JobNo,
                                          HBL = cstd.Hwbno,
                                          MBL = cst.Mawb,
                                          Type = sur.Type + "-BUY",
                                          SoaNo = sur.PaySoano,
                                          Debit = null,
                                          Credit = sur.Total,
                                          IsOBH = true,
                                          Currency = sur.CurrencyId,
                                          InvoiceNo = sur.InvoiceNo,
                                          Note = sur.Notes,
                                          CustomerID = sur.PaymentObjectId,
                                          ServiceDate = cst.ServiceDate,
                                          CreatedDate = cst.DatetimeCreated,
                                          TransactionType = cst.TransactionType,
                                          UserCreated = cst.UserCreated,
                                          Quantity = sur.Quantity,
                                          UnitId = sur.UnitId,
                                          UnitPrice = sur.UnitPrice,
                                          VATRate = sur.Vatrate,
                                          CreditDebitNo = sur.CreditNo,
                                          CommodityGroupID = null,
                                          Service = cst.TransactionType,
                                          CBM = cstd.Cbm,
                                          Commodity = cst.Commodity,
                                          FlightNo = cstd.FlightNo,
                                          ShippmentDate = cst.TransactionType == "AE" ? cstd.Etd : cst.TransactionType == "AI" ? cstd.Eta : null,
                                          AOL = cst.Pol,
                                          AOD = cst.Pod,
                                          PackageQty = cstd.PackageQty,
                                          GrossWeight = cstd.GrossWeight,
                                          ChargeWeight = cstd.ChargeWeight,
                                          FinalExchangeRate = sur.FinalExchangeRate,
                                          ExchangeDate = sur.ExchangeDate,
                                          PackageContainer = cstd.PackageContainer,
                                          TypeCharge = chg.Type,
                                          PayerId = sur.PayerId,
                                          Unit = uni.UnitNameEn,
                                          InvoiceDate = sur.InvoiceDate,
                                          VATAmountUSD = sur.VatAmountUsd,
                                          VATAmountVND = sur.VatAmountVnd,
                                          AmountUSD = sur.AmountUsd,
                                          AmountVND = sur.AmountVnd
                                      };
            if (query != null)
            {
                queryObhBuyDocument = queryObhBuyDocument.Where(x => !string.IsNullOrEmpty(x.Service)).Where(query);
                queryObhBuyDocument = queryObhBuyDocument.Where(x => !string.IsNullOrEmpty(x.CustomerID)).Where(query);
            }
            if (isOBH != null)
            {
                queryObhBuyDocument = queryObhBuyDocument.Where(x => x.IsOBH == isOBH);
            }
            //var queryObhBuy = queryObhBuyOperation.Union(queryObhBuyDocument);
            return queryObhBuyDocument;
        }

        private SummaryOfRevenueModel SummaryOfRevenueIncurred(GeneralReportCriteria criteria)
        {

            var dataShipment = QueryDataSummaryOfCostsIncurred(criteria);
            if (dataShipment == null) return null;
            var port = catPlaceRepo.Get();
            SummaryOfRevenueModel ObjectSummaryRevenue = new SummaryOfRevenueModel();
            List<SummaryOfRevenueExportResult> dataList = new List<SummaryOfRevenueExportResult>();
            Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query = chg => criteria.CustomerId.Contains(chg.CustomerID);
            var chargeData = GetChargeOBHSellPayer(query, null);
            var results = chargeData.GroupBy(x => new { x.JobId, x.HBLID }).AsQueryable();
            var lookupResults = results.ToLookup(q => q.Key.HBLID);
            var listPartner = catPartnerRepo.Get();
            var lookupPartner = listPartner.ToLookup(q => q.Id);
            var dataCustom = customsDeclarationRepo.Get().ToList();
            foreach (var item in dataShipment)
            {
                if (item.HBLID != Guid.Empty)
                {
                    foreach (var group in lookupResults[item.HBLID])
                    {
                        SummaryOfRevenueExportResult SummaryRevenue = new SummaryOfRevenueExportResult();
                        SummaryRevenue.SummaryOfCostsIncurredExportResults = new List<SummaryOfCostsIncurredExportResult>();
                        var commodity = DC.CsTransaction.Where(x => x.JobNo == group.Key.JobId).Select(t => t.Commodity).FirstOrDefault();
                        var commodityGroup = opsRepository.Get(x => x.JobNo == group.Key.JobId).Select(t => t.CommodityGroupId).FirstOrDefault();
                        string commodityName = string.Empty;
                        var _partnerId = group.Select(t => t.CustomerID).FirstOrDefault();

                        if (commodity != null)
                        {
                            string[] commodityArr = commodity.Split(',');
                            foreach (var it in commodityArr)
                            {
                                if (catCommodityRepo.Any(x => x.CommodityNameEn == it.Replace("\n", "")))
                                {
                                    commodityName = commodityName + "," + catCommodityRepo.Get(x => x.CommodityNameEn == it.Replace("\n", "")).Select(t => t.CommodityNameEn).FirstOrDefault();
                                }
                                else
                                {
                                    commodityName = commodityName + "," + catCommodityRepo.Get(x => x.Code == it.Replace("\n", "")).Select(t => t.CommodityNameEn).FirstOrDefault();
                                }
                            }
                            commodityName = commodityName.Substring(1);
                        }
                        if (commodityGroup != null)
                        {
                            commodityName = catCommodityGroupRepo.Get(x => x.Id == commodityGroup).Select(t => t.GroupNameVn).FirstOrDefault();
                        }
                        SummaryRevenue.ChargeName = commodityName;
                        SummaryRevenue.POLName = port.Where(x => x.Id == item.AOL).Select(t => t.NameEn).FirstOrDefault();
                        SummaryRevenue.CustomNo = GetTopClearanceNoByJobNo(group.Key.JobId, dataCustom);
                        SummaryRevenue.HBL = group.Select(t => t.HBL).FirstOrDefault();
                        SummaryRevenue.CBM = group.Select(t => t.CBM).FirstOrDefault();
                        SummaryRevenue.GrossWeight = group.Select(t => t.GrossWeight).FirstOrDefault();
                        SummaryRevenue.PackageContainer = group.Select(t => t.PackageContainer).FirstOrDefault();
                        foreach (var ele in group)
                        {
                            ele.SuplierName = catPartnerRepo.Get(x => x.Id == ele.CustomerID).Select(t => t.PartnerNameVn).FirstOrDefault();
                        }
                        foreach (var partner in lookupPartner[_partnerId])
                        {
                            SummaryRevenue.SupplierCode = partner?.AccountNo;
                            SummaryRevenue.SuplierName = partner?.PartnerNameVn;
                        }

                        SummaryRevenue.SummaryOfCostsIncurredExportResults.AddRange(group.Select(t => t));
                        dataList.Add(SummaryRevenue);
                    }
                }
            }
            ObjectSummaryRevenue.summaryOfRevenueExportResults = dataList;
            foreach (var item in ObjectSummaryRevenue.summaryOfRevenueExportResults)
            {
                foreach (var it in item.SummaryOfCostsIncurredExportResults)
                {
                    if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                    {
                        it.VATAmount = it.VATAmountUSD;
                        it.NetAmount = it.AmountUSD;
                    }
                    else
                    {
                        it.VATAmount = it.VATAmountVND;
                        it.NetAmount = it.AmountVND;
                    }
                    //if (it.VATRate > 0)
                    //{
                    //    it.VATAmount = (it.VATRate * it.NetAmount) / 100;
                    //}
                    //else
                    //{
                    //    it.VATAmount = it.VATRate != null ? Math.Abs(it.VATRate.Value) : 0;
                    //    if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                    //    {
                    //        it.VATAmount = it.VATAmountUSD;
                    //    }
                    //    else
                    //    {
                    //        it.VATAmount = it.VATAmountVND;
                    //    }

                    //}

                }
            }
            return ObjectSummaryRevenue;
        }

        private IQueryable<SummaryOfCostsIncurredExportResult> GetChargeOBHSellPayerJob(Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query, bool? isOBH)
        {
            //Chỉ lấy những phí từ shipment (IsFromShipment = true)
            var surcharge = surCharge.Get(x => x.Type == ReportConstants.CHARGE_OBH_TYPE || x.Type == ReportConstants.CHARGE_SELL_TYPE);
            var opst = opsRepository.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != null && x.CurrentStatus != TermData.Canceled);
            var csTrans = DC.CsTransaction.Where(x => x.CurrentStatus != TermData.Canceled);
            var csTransDe = detailRepository.Get();
            var charge = catChargeRepo.Get();
            var unit = catUnitRepo.Get();
            var queryObhBuyOperation = from sur in surcharge
                                       join ops in opst on sur.Hblid equals ops.Hblid
                                       join chg in charge on sur.ChargeId equals chg.Id into chg2
                                       from chg in chg2.DefaultIfEmpty()
                                       join uni in unit on sur.UnitId equals uni.Id into uni2
                                       from uni in uni2.DefaultIfEmpty()
                                       select new SummaryOfCostsIncurredExportResult
                                       {
                                           ID = sur.Id,
                                           HBLID = sur.Hblid,
                                           ChargeID = sur.ChargeId,
                                           ChargeCode = chg.Code,
                                           ChargeName = chg.ChargeNameEn,
                                           JobId = ops.JobNo,
                                           HBL = ops.Hwbno,
                                           MBL = ops.Mblno,
                                           Type = sur.Type + "-BUY",
                                           SoaNo = sur.PaySoano,
                                           Debit = null,
                                           Credit = sur.Total,
                                           IsOBH = true,
                                           Currency = sur.CurrencyId,
                                           InvoiceNo = sur.InvoiceNo,
                                           Note = sur.Notes,
                                           CustomerID = sur.PaymentObjectId,
                                           ServiceDate = ops.ServiceDate,
                                           CreatedDate = ops.DatetimeCreated,
                                           TransactionType = null,
                                           UserCreated = ops.UserCreated,
                                           Quantity = sur.Quantity,
                                           UnitId = sur.UnitId,
                                           UnitPrice = sur.UnitPrice,
                                           VATRate = sur.Vatrate,
                                           CreditDebitNo = sur.CreditNo,
                                           CommodityGroupID = ops.CommodityGroupId,
                                           Service = "CL",
                                           ExchangeDate = sur.ExchangeDate,
                                           FinalExchangeRate = sur.FinalExchangeRate,
                                           TypeCharge = chg.Type,
                                           PayerId = sur.PayerId,
                                           Unit = uni.UnitNameEn,
                                           InvoiceDate = sur.InvoiceDate,
                                           CBM = ops.SumCbm,
                                           GrossWeight = ops.SumGrossWeight,
                                           PackageContainer = ops.ContainerDescription,
                                           PackageQty = ops.SumPackages,
                                           VATAmountUSD = sur.VatAmountUsd,
                                           VATAmountVND = sur.VatAmountVnd,
                                           AmountUSD = sur.AmountUsd,
                                           AmountVND = sur.AmountVnd
                                       };
            if (query != null)
            {
                queryObhBuyOperation = queryObhBuyOperation.Where(x => !string.IsNullOrEmpty(x.Service)).Where(query);
                queryObhBuyOperation = queryObhBuyOperation.Where(x => !string.IsNullOrEmpty(x.CustomerID)).Where(query);
            }
            return queryObhBuyOperation;
        }
        private IQueryable<OpsTransaction> QueryDataOperationAcctPLSheet(GeneralReportCriteria criteria)
        {
            // Filter data without customerId
            //var criteriaNoCustomer = (GeneralReportCriteria)criteria.Clone();
            //criteriaNoCustomer.CustomerId = null;
            Expression<Func<OpsTransaction, bool>> query = GetQueryOPSTransactionOperation(criteria, null);

            var queryShipment = GetOpsTransactionWithSalesman(query, criteria);
            return queryShipment;
        }
        private SummaryOfRevenueModel SummaryOfRevenueIncurredOperation(GeneralReportCriteria criteria)
        {
            var dataShipment = QueryDataOperationAcctPLSheet(criteria);
            if (dataShipment == null) return null;
            var port = catPlaceRepo.Get();
            SummaryOfRevenueModel ObjectSummaryRevenue = new SummaryOfRevenueModel();
            List<SummaryOfRevenueExportResult> dataList = new List<SummaryOfRevenueExportResult>();
            Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query = chg => criteria.CustomerId.Contains(chg.CustomerID);
            var chargeData = !string.IsNullOrEmpty(criteria.CustomerId) ? GetChargeOBHSellPayerJob(query, null) : GetChargeOBHSellPayerJob(null, null);
            var results = chargeData.GroupBy(x => new { x.JobId, x.HBLID }).AsQueryable();

            var lookupReuslts = results.ToLookup(q => q.Key.JobId);
            var listPartner = catPartnerRepo.Get();
            var lookupPartner = listPartner.ToLookup(q => q.Id);
            var dataCustom = customsDeclarationRepo.Get().ToList();
            var commodityGroupData = catCommodityGroupRepo.Get().ToList();
            var lookupCommodityGroup = commodityGroupData.ToLookup(q => q.Id);
            if (results == null)
                return null;
            foreach (var item in dataShipment)
            {
                if (item.Hblid != Guid.Empty)
                {
                    foreach (var group in lookupReuslts[item.JobNo])
                    {
                        SummaryOfRevenueExportResult SummaryRevenue = new SummaryOfRevenueExportResult();
                        SummaryRevenue.SummaryOfCostsIncurredExportResults = new List<SummaryOfCostsIncurredExportResult>();
                        var commodity = DC.CsTransaction.Where(x => x.JobNo == group.Key.JobId).Select(t => t.Commodity).FirstOrDefault();
                        var commodityGroup = opsRepository.Get(x => x.JobNo == group.Key.JobId).Select(t => t.CommodityGroupId).FirstOrDefault();

                        string commodityName = string.Empty;
                        var _partnerId = group.Select(t => t.CustomerID).FirstOrDefault();
                        if (commodity != null)
                        {
                            string[] commodityArr = commodity.Split(',');
                            foreach (var it in commodityArr)
                            {
                                commodityName = commodityName + "," + catCommodityRepo.Get(x => x.CommodityNameEn == it.Replace("\n", "")).Select(t => t.CommodityNameEn).FirstOrDefault();
                            }
                            commodityName = commodityName.Substring(1);
                        }
                        if (commodityGroup != null)
                        {
                            foreach (var commodityG in lookupCommodityGroup[(short)commodityGroup])
                            {
                                commodityName = commodityG.GroupNameVn;
                            }
                        }
                        SummaryRevenue.ChargeName = commodityName;
                        SummaryRevenue.POLName = port.Where(x => x.Id == item.Pol).Select(t => t.NameEn).FirstOrDefault();
                        SummaryRevenue.CustomNo = GetTopClearanceNoByJobNo(group.Key.JobId, dataCustom);
                        SummaryRevenue.HBL = group.Select(t => t.HBL).FirstOrDefault();
                        SummaryRevenue.CBM = group.Select(t => t.CBM).FirstOrDefault();
                        SummaryRevenue.GrossWeight = group.Select(t => t.GrossWeight).FirstOrDefault();
                        SummaryRevenue.PackageContainer = group.Select(t => t.PackageContainer).FirstOrDefault();
                        foreach (var ele in group)
                        {
                            ele.SuplierName = catPartnerRepo.Get(x => x.Id == ele.CustomerID).Select(t => t.PartnerNameVn).FirstOrDefault();
                        }
                        foreach (var partner in lookupPartner[_partnerId])
                        {
                            SummaryRevenue.SupplierCode = partner?.AccountNo;
                            SummaryRevenue.SuplierName = partner?.PartnerNameVn;
                        }
                        SummaryRevenue.SummaryOfCostsIncurredExportResults.AddRange(group.Select(t => t));
                        dataList.Add(SummaryRevenue);
                    }
                }
            }
            ObjectSummaryRevenue.summaryOfRevenueExportResults = dataList;
            foreach (var item in ObjectSummaryRevenue.summaryOfRevenueExportResults)
            {
                foreach (var it in item.SummaryOfCostsIncurredExportResults)
                {
                    if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                    {
                        it.VATAmount = it.VATAmountUSD;
                        it.NetAmount = it.AmountUSD;
                    }
                    else
                    {
                        it.VATAmount = it.VATAmountVND;
                        it.NetAmount = it.AmountVND;
                    }
                    //if (it.VATRate > 0)
                    //{
                    //    it.VATAmount = (it.VATRate * it.NetAmount) / 100;
                    //}
                    //else
                    //{
                    //    it.VATAmount = it.VATRate != null ? Math.Abs(it.VATRate.Value) : 0;
                    //    if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                    //    {
                    //        it.VATAmount = it.VATAmountUSD;
                    //    }
                    //    else
                    //    {
                    //        it.VATAmount = it.VATAmountVND;
                    //    }

                    //}

                }
            }
            return ObjectSummaryRevenue;
        }

        public SummaryOfRevenueModel GetDataSummaryOfRevenueIncurred(GeneralReportCriteria criteria)
        {
            var dataDocumentation = SummaryOfRevenueIncurred(criteria);
            SummaryOfRevenueModel obj = new SummaryOfRevenueModel();

            if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains(TermData.CustomLogistic))
            {
                var dataOperation = SummaryOfRevenueIncurredOperation(criteria);
                var lstDoc = dataDocumentation.summaryOfRevenueExportResults.AsQueryable();
                var lstOperation = dataOperation.summaryOfRevenueExportResults.AsQueryable();
                var lst = lstDoc.Union(lstOperation);
                obj.summaryOfRevenueExportResults = lst.ToList();
            }
            else
            {
                obj = dataDocumentation;
            }
            return obj;
        }

        private SummaryOfRevenueModel SummaryOfCostsByPartnerOperation(GeneralReportCriteria criteria, IQueryable<SummaryOfCostsIncurredExportResult> chargeData)
        {
            var dataShipment = QueryDataOperationAcctPLSheet(criteria);
            if (dataShipment == null) return null;
            var port = catPlaceRepo.Get();
            SummaryOfRevenueModel ObjectSummaryRevenue = new SummaryOfRevenueModel();
            List<SummaryOfRevenueExportResult> dataList = new List<SummaryOfRevenueExportResult>();
            Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query = chg => criteria.CustomerId.Contains(chg.CustomerID);
            var results = chargeData.AsEnumerable().GroupBy(x => new { x.JobId, x.HBLID }).AsQueryable();

            var lookupReuslts = results.ToLookup(q => q.Key.JobId);
            var listPartner = catPartnerRepo.Get();
            var lookupPartner = listPartner.ToLookup(q => q.Id);
            var dataCustom = customsDeclarationRepo.Get().ToList();
            var commodityGroupData = catCommodityGroupRepo.Get().ToList();
            var lookupCommodityGroup = commodityGroupData.ToLookup(q => q.Id);
            if (results == null)
                return null;
            foreach (var item in dataShipment)
            {
                if (item.Hblid != Guid.Empty)
                {
                    foreach (var group in lookupReuslts[item.JobNo])
                    {
                        SummaryOfRevenueExportResult SummaryRevenue = new SummaryOfRevenueExportResult();
                        SummaryRevenue.SummaryOfCostsIncurredExportResults = new List<SummaryOfCostsIncurredExportResult>();
                        var commodity = DC.CsTransaction.Where(x => x.JobNo == group.Key.JobId).Select(t => t.Commodity).FirstOrDefault();
                        var commodityGroup = opsRepository.Get(x => x.JobNo == group.Key.JobId).Select(t => t.CommodityGroupId).FirstOrDefault();

                        string commodityName = string.Empty;
                        var _partnerId = group.Select(t => t.CustomerID).FirstOrDefault();
                        if (commodity != null)
                        {
                            string[] commodityArr = commodity.Split(',');
                            foreach (var it in commodityArr)
                            {
                                commodityName = commodityName + "," + catCommodityRepo.Get(x => x.CommodityNameEn == it.Replace("\n", "")).Select(t => t.CommodityNameEn).FirstOrDefault();
                            }
                            commodityName = commodityName.Substring(1);
                        }
                        if (commodityGroup != null)
                        {
                            foreach (var commodityG in lookupCommodityGroup[(short)commodityGroup])
                            {
                                commodityName = commodityG.GroupNameVn;
                            }
                        }
                        SummaryRevenue.ChargeName = commodityName;
                        SummaryRevenue.POLName = port.Where(x => x.Id == item.Pol).Select(t => t.NameEn).FirstOrDefault();
                        SummaryRevenue.CustomNo = GetTopClearanceNoByJobNo(group.Key.JobId, dataCustom);
                        SummaryRevenue.HBL = group.Select(t => t.HBL).FirstOrDefault();
                        SummaryRevenue.CBM = group.Select(t => t.CBM).FirstOrDefault();
                        SummaryRevenue.GrossWeight = group.Select(t => t.GrossWeight).FirstOrDefault();
                        SummaryRevenue.PackageContainer = group.Select(t => t.PackageContainer).FirstOrDefault();
                        foreach (var ele in group)
                        {
                            ele.SuplierName = lookupPartner[ele.CustomerID].Select(t => t.PartnerNameVn).FirstOrDefault();
                        }
                        foreach (var partner in lookupPartner[_partnerId])
                        {
                            SummaryRevenue.SupplierCode = partner?.AccountNo;
                            SummaryRevenue.SuplierName = partner?.PartnerNameVn;
                        }
                        SummaryRevenue.SummaryOfCostsIncurredExportResults.AddRange(group.Select(t => t));
                        dataList.Add(SummaryRevenue);
                    }
                }
            }
            ObjectSummaryRevenue.summaryOfRevenueExportResults = dataList;
            foreach (var item in ObjectSummaryRevenue.summaryOfRevenueExportResults)
            {
                foreach (var it in item.SummaryOfCostsIncurredExportResults)
                {
                    if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                    {
                        it.VATAmount = it.VATAmountUSD;
                        it.NetAmount = it.AmountUSD;
                    }
                    else
                    {
                        it.VATAmount = it.VATAmountVND;
                        it.NetAmount = it.AmountVND;
                    }
                    if (it.VATRate > 0)
                    {
                        it.VATAmount = (it.VATRate * it.NetAmount) / 100;
                    }
                    else
                    {
                        it.VATAmount = it.VATRate != null ? Math.Abs(it.VATRate.Value) : 0;
                        if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                        {
                            it.VATAmount = it.VATAmountUSD;
                        }
                        else
                        {
                            it.VATAmount = it.VATAmountVND;
                        }

                    }

                }
            }
            return ObjectSummaryRevenue;
        }

        public SummaryOfRevenueModel GetDataCostsByPartner(GeneralReportCriteria criteria)
        {
            Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query = chg => criteria.CustomerId.Contains(chg.CustomerID);
            var chargeData = !string.IsNullOrEmpty(criteria.CustomerId) ? GetChargeOBHPayee(query, null) : GetChargeOBHPayee(null, null);
            var dataDocumentation = SummaryOfCostsByPartner(criteria, chargeData);
            SummaryOfRevenueModel obj = new SummaryOfRevenueModel();

            if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains("CL"))
            {
                var dataOperation = SummaryOfCostsByPartnerOperation(criteria, chargeData);
                var lstDoc = dataDocumentation.summaryOfRevenueExportResults.AsQueryable();
                var lstOperation = dataOperation.summaryOfRevenueExportResults.AsQueryable();
                var lst = lstDoc.Union(lstOperation);
                obj.summaryOfRevenueExportResults = lst.ToList();
            }
            else
            {
                obj = dataDocumentation;
            }
            return obj;
        }

        private IQueryable<JobProfitAnalysisExportResult> QueryDataDocumentationJobProfitAnalysis(GeneralReportCriteria criteria)
        {
            Expression<Func<CsTransaction, bool>> queryTrans = GetQueryTransationDocumentation(criteria);
            Expression<Func<CsTransactionDetail, bool>> queryTranDetail = GetQueryTransationDetailDocumentation(criteria);

            var masterBills = DC.CsTransaction.Where(x => x.CurrentStatus != ReportConstants.CURRENT_STATUS_CANCELED).Where(queryTrans);
            if (queryTranDetail == null)
            {
                var houseBills = detailRepository.Get();
                var queryShipment = from master in masterBills
                                    join house in houseBills on master.Id equals house.JobId
                                    select new JobProfitAnalysisExportResult
                                    {
                                        JobNo = master.JobNo,
                                        Mbl = master.Mawb,
                                        Hbl = house.Hwbno,
                                        Hblid = house.Id,
                                        Service = master.TransactionType,
                                        Eta = master.Eta,
                                        Etd = master.Etd,
                                        GW = house.GrossWeight,
                                        CW = house.ChargeWeight,
                                        CBM = house.Cbm,
                                        Quantity = house.PackageQty,
                                        Cont20 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "20").Count : 0,
                                        Cont40 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "40").Count : 0,
                                        Cont40HC = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "40´HC").Count : 0,
                                        Cont45 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "45").Count : 0,
                                        Cont = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "Cont").Count : 0
                                    };
                return queryShipment;
            }
            else
            {
                var houseBills = GetTransactionDetailDocWithSalesman(queryTranDetail, criteria);
                var queryShipment = from master in masterBills
                                    join house in houseBills on master.Id equals house.JobId
                                    select new JobProfitAnalysisExportResult
                                    {
                                        JobNo = master.JobNo,
                                        Mbl = master.Mawb,
                                        Hbl = house.Hwbno,
                                        Hblid = house.Id,
                                        Service = master.TransactionType,
                                        Eta = master.Eta,
                                        Etd = master.Etd,
                                        GW = house.GrossWeight,
                                        CW = house.ChargeWeight,
                                        CBM = house.Cbm,
                                        Quantity = house.PackageQty,
                                        Cont20 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "20").Count : 0,
                                        Cont40 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "40").Count : 0,
                                        Cont40HC = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "40´HC").Count : 0,
                                        Cont45 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "45").Count : 0,
                                        Cont = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "Cont").Count : 0
                                    };
                return queryShipment;
            }
        }

        private IQueryable<JobProfitAnalysisExportResult> JobProfitAnalysisDocumetation(GeneralReportCriteria criteria)
        {
            // Filter data without customerId
            var criteriaNoCustomer = (GeneralReportCriteria)criteria.Clone();
            criteriaNoCustomer.CustomerId = null;
            var dataShipment = QueryDataDocumentationJobProfitAnalysis(criteriaNoCustomer);
            List<JobProfitAnalysisExportResult> dataList = new List<JobProfitAnalysisExportResult>();
            var dataCharge = catChargeRepo.Get();
            var surchargeData = surCharge.Get().ToList();
            var lookupSurcharge = surchargeData.ToLookup(q => q.Hblid);
            if (dataShipment != null)
            {
                foreach (var item in dataShipment)
                {
                    var chargeD = lookupSurcharge[item.Hblid].Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE || x.Type == ReportConstants.CHARGE_SELL_TYPE);
                    if (!string.IsNullOrEmpty(criteria.CustomerId))
                    {
                        chargeD = chargeD.Where(x => (criteria.CustomerId == x.PaymentObjectId || criteria.CustomerId == x.PayerId) && (x.Type == ReportConstants.CHARGE_BUY_TYPE || x.Type == ReportConstants.CHARGE_SELL_TYPE));
                    }
                    foreach (var charge in chargeD)
                    {
                        JobProfitAnalysisExportResult data = new JobProfitAnalysisExportResult();
                        data.JobNo = item.JobNo;
                        data.Service = API.Common.Globals.CustomData.Services.Where(x => x.Value == item.Service).FirstOrDefault()?.DisplayName;
                        data.Mbl = item.Mbl;
                        data.Hbl = item.Hbl;
                        data.Eta = item.Eta;
                        data.Etd = item.Etd;
                        data.Quantity = item.Quantity;
                        data.Cont20 = item.Cont20;
                        data.Cont40 = item.Cont40;
                        data.Cont40HC = item.Cont40HC;
                        data.Cont45 = item.Cont45;
                        data.Cont = item.Cont;
                        data.CW = item.CW;
                        data.GW = item.GW;
                        data.CBM = item.CBM;
                        data.ChargeType = charge.Type;
                        var _charge = dataCharge.FirstOrDefault(x => x.Id == charge.ChargeId);
                        data.ChargeCode = _charge?.ChargeNameEn;
                        var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                        if (charge.Type == ReportConstants.CHARGE_SELL_TYPE)
                        {
                            data.TotalRevenue = charge.Quantity * charge.UnitPrice * _rate ?? 0;
                        }
                        if (_charge.DebitCharge != null && charge.Type == ReportConstants.CHARGE_BUY_TYPE)
                        {

                            data.TotalCost = charge.Quantity * charge.UnitPrice * _rate ?? 0;
                            var dataSelling = dataCharge.FirstOrDefault(x => x.Id == _charge.DebitCharge);
                            var dataChargeSell = chargeD.FirstOrDefault(x => x.ChargeId == dataSelling.Id && x.Type == ReportConstants.CHARGE_SELL_TYPE);
                            if (dataChargeSell != null)
                            {
                                var _rateRevenue = currencyExchangeService.CurrencyExchangeRateConvert(dataChargeSell.FinalExchangeRate, dataChargeSell.ExchangeDate, dataChargeSell.CurrencyId, criteria.Currency);
                                data.TotalRevenue = dataChargeSell.Quantity * dataChargeSell.UnitPrice * _rateRevenue ?? 0;
                                data.isGroup = true;
                            }

                        }

                        data.TotalCost = data.TotalCost ?? 0;
                        data.TotalRevenue = data.TotalRevenue ?? 0;
                        data.JobProfit = data.TotalRevenue - data.TotalCost;
                        if (data.TotalRevenue == 0)
                        {
                            data.JobProfit = 0;
                        }

                        if (_charge.DebitCharge == null && charge.Type == ReportConstants.CHARGE_BUY_TYPE)
                        {
                            data.TotalCost = charge.Quantity * charge.UnitPrice * _rate ?? 0;
                        }
                        dataList.Add(data);
                    }
                }
            }

            var groupedList = dataList.Where(x => x.isGroup == true).ToList();
            if (groupedList != null)
            {
                foreach (var item in groupedList)
                {
                    var ItemToRemove = dataList.FirstOrDefault(x => x.ChargeCode == item.ChargeCode && x.isGroup == null && x.ChargeType == ReportConstants.CHARGE_SELL_TYPE && x.Hbl == item.Hbl);
                    if (ItemToRemove != null)
                    {
                        dataList.Remove(ItemToRemove);
                    }
                }
            }
            var GroupHbl = dataList.GroupBy(a => a.Hbl).Select(p => new { Hbl = p.Key, TotalCost = p.Sum(q => q.TotalCost), TotalRevenue = p.Sum(q => q.TotalRevenue), TotalJobProfit = p.Sum(q => q.JobProfit) }).ToList();
            foreach (var item in GroupHbl)
            {
                if (!string.IsNullOrEmpty(item.Hbl))
                {
                    int i = dataList.FindLastIndex(x => x.Hbl != null && x.Hbl.StartsWith(item.Hbl));
                    JobProfitAnalysisExportResult data = new JobProfitAnalysisExportResult();
                    data.TotalCost = item.TotalCost;
                    data.TotalRevenue = item.TotalRevenue;
                    data.JobProfit = item.TotalJobProfit;
                    dataList.Insert(i + 1, data);
                }
            }
            return dataList.AsQueryable();

        }

        private IQueryable<JobProfitAnalysisExportResult> JobProfitAnalysisOperation(GeneralReportCriteria criteria)
        {
            var dataShipment = QueryDataOperationAcctPLSheet(criteria);
            List<JobProfitAnalysisExportResult> dataList = new List<JobProfitAnalysisExportResult>();
            var dataCharge = catChargeRepo.Get();
            var surchargeData = surCharge.Get().ToList();
            var lookupSurcharge = surchargeData.ToLookup(q => q.Hblid);
            foreach (var item in dataShipment)
            {
                if (item.Hblid != Guid.Empty)
                {
                    var chargeD = lookupSurcharge[item.Hblid].Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE || x.Type == ReportConstants.CHARGE_SELL_TYPE);
                    if (!string.IsNullOrEmpty(criteria.CustomerId))
                    {
                        chargeD = chargeD.Where(x => (criteria.CustomerId == x.PaymentObjectId || criteria.CustomerId == x.PayerId) && (x.Type == ReportConstants.CHARGE_BUY_TYPE || x.Type == ReportConstants.CHARGE_SELL_TYPE));
                    }
                    foreach (var charge in chargeD)
                    {
                        JobProfitAnalysisExportResult data = new JobProfitAnalysisExportResult();
                        data.JobNo = item.JobNo;
                        data.Service = API.Common.Globals.CustomData.Services.Where(x => x.Value == "CL").FirstOrDefault()?.DisplayName;
                        data.Mbl = item.Mblno;
                        data.Hbl = item.Hwbno;
                        data.Eta = item.ServiceDate;
                        data.Etd = item.ServiceDate;
                        data.Quantity = item.SumContainers;
                        var DetailContainer = !string.IsNullOrEmpty(item.ContainerDescription) ? item.ContainerDescription.Split(";").ToArray() : null;
                        int? Cont20 = 0;
                        int? Cont40 = 0;
                        int? Cont40HC = 0;
                        int? Cont45 = 0;
                        int? Cont = 0;
                        if (DetailContainer != null)
                        {
                            foreach (var it in DetailContainer)
                            {
                                if (Regex.Matches(it.Trim(), "20").Count > 0)
                                {
                                    Cont20 = Convert.ToInt16(it.Trim().Substring(0, 1));
                                }

                                if (Regex.Matches(it.Trim(), "40").Count > 0)
                                {
                                    Cont40 = Convert.ToInt16(it.Trim().Substring(0, 1));
                                }

                                if (Regex.Matches(it.Trim(), "40´HC").Count > 0)
                                {
                                    Cont40HC = Convert.ToInt16(it.Trim().Substring(0, 1));
                                }

                                if (Regex.Matches(it.Trim(), "45").Count > 0)
                                {
                                    Cont45 = Convert.ToInt16(it.Trim().Substring(0, 1));
                                }

                                if (Regex.Matches(it.Trim(), "Cont").Count > 0)
                                {
                                    Cont = Convert.ToInt16(it.Trim().Substring(0, 1));
                                }

                            }
                        }
                        data.Cont20 = !string.IsNullOrEmpty(item.ContainerDescription) ? Cont20 : 0;
                        data.Cont40 = !string.IsNullOrEmpty(item.ContainerDescription) ? Cont40 : 0;
                        data.Cont40HC = !string.IsNullOrEmpty(item.ContainerDescription) ? Cont40HC : 0;
                        data.Cont45 = !string.IsNullOrEmpty(item.ContainerDescription) ? Cont45 : 0;
                        data.Cont = !string.IsNullOrEmpty(item.ContainerDescription) ? Cont : 0;
                        data.CW = item.SumChargeWeight;
                        data.GW = item.SumGrossWeight;
                        data.CBM = item.SumCbm;
                        data.ChargeType = charge.Type;
                        var _charge = dataCharge.Where(x => x.Id == charge.ChargeId).FirstOrDefault();
                        data.ChargeCode = _charge?.ChargeNameEn;
                        var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                        if (charge.Type == ReportConstants.CHARGE_SELL_TYPE)
                        {
                            data.TotalRevenue = charge.Quantity * charge.UnitPrice * _rate ?? 0;
                        }

                        if (_charge.DebitCharge != null && charge.Type == ReportConstants.CHARGE_BUY_TYPE)
                        {

                            data.TotalCost = charge.Quantity * charge.UnitPrice * _rate ?? 0;
                            var dataSelling = dataCharge.FirstOrDefault(x => x.Id == _charge.DebitCharge);
                            var dataChargeSell = chargeD.FirstOrDefault(x => x.ChargeId == dataSelling.Id && x.Type == ReportConstants.CHARGE_SELL_TYPE);
                            if (dataChargeSell != null)
                            {
                                var _rateRevenue = currencyExchangeService.CurrencyExchangeRateConvert(dataChargeSell.FinalExchangeRate, dataChargeSell.ExchangeDate, dataChargeSell.CurrencyId, criteria.Currency);
                                data.TotalRevenue = dataChargeSell.Quantity * dataChargeSell.UnitPrice * _rateRevenue ?? 0;
                                data.isGroup = true;
                            }

                        }

                        data.TotalCost = data.TotalCost ?? 0;
                        data.TotalRevenue = data.TotalRevenue ?? 0;
                        data.JobProfit = data.TotalRevenue - data.TotalCost;
                        if (data.TotalRevenue == 0)
                        {
                            data.JobProfit = 0;
                        }

                        if (_charge.DebitCharge == null && charge.Type == ReportConstants.CHARGE_BUY_TYPE)
                        {
                            data.TotalCost = charge.Quantity * charge.UnitPrice * _rate ?? 0;
                        }
                        dataList.Add(data);
                    }
                }
            }
            var groupedList = dataList.Where(x => x.isGroup == true).ToList();
            if (groupedList != null)
            {
                foreach (var item in groupedList)
                {
                    var ItemToRemove = dataList.FirstOrDefault(x => x.ChargeCode == item.ChargeCode && x.isGroup == null && x.ChargeType == ReportConstants.CHARGE_SELL_TYPE && x.Hbl == item.Hbl);
                    if (ItemToRemove != null)
                    {
                        dataList.Remove(ItemToRemove);
                    }
                }
            }
            var GroupHbl = dataList.GroupBy(a => a.Hbl).Select(p => new { Hbl = p.Key, TotalCost = p.Sum(q => q.TotalCost), TotalRevenue = p.Sum(q => q.TotalRevenue), TotalJobProfit = p.Sum(q => q.JobProfit) }).ToList();
            foreach (var item in GroupHbl)
            {
                int i = dataList.FindLastIndex(x => x.Hbl != null && x.Hbl.StartsWith(item.Hbl));
                JobProfitAnalysisExportResult data = new JobProfitAnalysisExportResult();
                data.TotalCost = item.TotalCost;
                data.TotalRevenue = item.TotalRevenue;
                data.JobProfit = item.TotalJobProfit;
                dataList.Insert(i + 1, data);
            }
            return dataList.AsQueryable();
        }

        public IQueryable<JobProfitAnalysisExportResult> GetDataJobProfitAnalysis(GeneralReportCriteria criteria)
        {
            var dataDocumentation = JobProfitAnalysisDocumetation(criteria);
            IQueryable<JobProfitAnalysisExportResult> list;
            if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains(TermData.CustomLogistic))
            {
                var dataOperation = JobProfitAnalysisOperation(criteria);
                list = dataDocumentation.Union(dataOperation);
            }
            else
            {
                list = dataDocumentation;
            }
            return list;
        }
    }
}
