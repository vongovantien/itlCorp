using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models.ReportResults.Sales;
using eFMS.API.Documentation.Service.Models;
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
        public SaleReportService(IContextBase<OpsTransaction> opsRepo,
            IContextBase<CsTransaction> csRepo,
            IContextBase<CsTransactionDetail> detailRepo,
            IContextBase<CatDepartment> departmentRepo,
            IContextBase<CsMawbcontainer> containerRepo,
            IContextBase<CatPartner> catPartnerRepo,
            IContextBase<CatPlace> placeRepo,
            IContextBase<CatUnit> uniRepo,
            IContextBase<CsShipmentSurcharge> surchargeRepo,
            IContextBase<CatCurrencyExchange> exchangeRepo)
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
        }
        public IQueryable<MonthlySaleReportResult> GetMonthlySaleReport(SaleReportCriteria criteria)
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
                    ContactName = "An Mai Loan",
                    SalesManager = "SalesManager A",
                    PartnerName = catPartnerRepository.Get(x => x.Id == item.CustomerId).FirstOrDefault()?.PartnerNameEn,
                    Description = "Logistics",
                    Area = "Area " + item.JobNo,
                    POL = item.Pol != null ? catPlaceRepository.Get(x => x.Id == item.Pol).FirstOrDefault()?.NameEn : string.Empty,
                    POD = item.Pod != null ? catPlaceRepository.Get(x => x.Id == item.Pod).FirstOrDefault()?.NameEn : string.Empty,
                    Lines = "Lines " + item.JobNo,
                    Agent = item.AgentId != null ? catPartnerRepository.Get(x => x.Id == item.AgentId).FirstOrDefault()?.PartnerNameEn : string.Empty,
                    NominationParty = "NominationParty " + item.JobNo,
                    assigned = false,
                    TransID = item.JobNo,
                    HWBNO = item.Hwbno,
                    //Qty20 = 0,
                    //Qty40 = 2,
                    //Cont40HC = 2,
                    KGS = item.SumNetWeight ?? (decimal)item.SumNetWeight,
                    CBM = item.SumCbm ?? (decimal)item.SumCbm,
                    //SellingRate = 3,
                    SharedProfit = 3,
                    //BuyingRate = 3,
                    OtherCharges = 3,
                    SalesTarget = 0,
                    Bonus = 0,
                    TpyeofService = "TpyeofService " + item.JobNo,
                    Shipper = item.Shipper,
                    Consignee = item.Consignee
                };
                report.SellingRate = GetSellingRate(item.Hblid);
                report.BuyingRate = GetBuyingRate(item.Hblid);
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
                                                                 && (x.Pod == criteria.Pod || criteria.Pod == null)
                                                                 && (x.Pol == criteria.Pol || criteria.Pol == null)
                                                                 && (x.SupplierId == criteria.CarrierId || criteria.CarrierId == null)
                                                                 && (criteria.OfficeId.Contains(x.OfficeId.ToString()) || string.IsNullOrEmpty(criteria.OfficeId))
                                                                 && (criteria.DepartmentId.Contains(x.DepartmentId.ToString()) || string.IsNullOrEmpty(criteria.DepartmentId))
                                                                 && (criteria.GroupId.Contains(x.GroupId.ToString()) || string.IsNullOrEmpty(criteria.GroupId))
                                                                 && (criteria.PersonInCharge.Contains(x.BillingOpsId) || string.IsNullOrEmpty(criteria.PersonInCharge))
                                                                 && (criteria.Creator.Contains(x.UserCreated) || string.IsNullOrEmpty(criteria.Creator))
                                                                 ;
            if (criteria.ServiceDateFrom != null && criteria.ServiceDateTo != null)
            {
                queryOpsTrans = queryOpsTrans.And(x => x.ServiceDate.Value.Date >= criteria.ServiceDateFrom.Value.Date && x.ServiceDate.Value.Date <= criteria.ServiceDateTo.Value.Date);
            }
            if (criteria.CreatedDateFrom != null && criteria.CreatedDateTo != null)
            {
                queryOpsTrans = queryOpsTrans.And(x => x.DatetimeCreated.Value.Date >= criteria.CreatedDateFrom.Value && x.DatetimeCreated.Value.Date <= criteria.CreatedDateTo);
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
                         select new {
                             shipment.DepartmentId,
                             shipment.TransactionType,
                             shipment.JobNo,
                             shipment.ShipmentType,
                             housebill.CustomerId,
                             housebill.NotifyPartyDescription,
                             housebill.Id,
                             housebill.Hwbno,
                             housebill.NetWeight,
                             housebill.Cbm,
                             housebill.ShipperDescription,
                             housebill.ConsigneeDescription
                         });
            if (data == null) return null;
            var results = new List<MonthlySaleReportResult>();
            var containerData = uniRepository.Get(x => x.UnitType == "Container");
            foreach (var item in data)
            {
                var report = new MonthlySaleReportResult
                {
                    Department = departmentRepository.Get(x => x.Id == item.DepartmentId).FirstOrDefault()?.DeptNameEn,
                    ContactName = "An Mai Loan",
                    SalesManager = "SalesManager A",
                    PartnerName = catPartnerRepository.Get(x => x.Id == item.CustomerId).FirstOrDefault()?.PartnerNameEn,
                    Description = item.TransactionType,
                    Area = "Area " + item.JobNo,
                    POL = "Chưa biết lấy gì",
                    POD = "Chưa biết lấy gì",
                    Lines = "Lines " + item.JobNo,
                    Agent = "Chưa biết lấy gì",
                    NominationParty = item.NotifyPartyDescription,
                    assigned = item.ShipmentType == "Nominated",
                    TransID = item.JobNo,
                    HWBNO = item.Hwbno,
                    KGS = item.NetWeight??(decimal)item.NetWeight,
                    CBM = item.Cbm ?? (decimal)item.Cbm,
                    SharedProfit = 3,
                    OtherCharges = 3,
                    SalesTarget = 0,
                    Bonus = 0,
                    TpyeofService = "TpyeofService " + item.JobNo,
                    Shipper = item.ShipperDescription,
                    Consignee = item.ConsigneeDescription
                };
                report.SellingRate = GetSellingRate(item.Id);
                report.BuyingRate = GetBuyingRate(item.Id);
                var contInfo = GetContainer(containerData, item.Id);
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
            if (mblid!= null)
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

        private decimal GetBuyingRate(Guid hblid)
        {
            decimal cost = 0;
            var buyingCharges = surchargeRepository.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && x.Hblid == hblid);
            if (buyingCharges != null)
            {
                foreach (var charge in buyingCharges)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var rate = charge.FinalExchangeRate;
                    if (rate == null)
                    {
                        var currencyExchange = exchangeRepository.Get(x => x.DatetimeModified.Value.Date == charge.ExchangeDate.Value.Date).ToList();
                        //_rate = GetRateCurrencyExchange(currencyExchange, charge.CurrencyId, criteria.Currency);
                    }
                    cost += charge.Quantity * charge.UnitPrice * rate ?? 0; // Phí Selling trước thuế
                }
            }
            return cost;
        }

        private decimal GetSellingRate(Guid hblid)
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
                        var currencyExchange = exchangeRepository.Get(x => x.DatetimeModified.Value.Date == charge.ExchangeDate.Value.Date).ToList();
                        //_rate = GetRateCurrencyExchange(currencyExchange, charge.CurrencyId, criteria.Currency);
                    }
                    revenue += charge.Quantity * charge.UnitPrice * rate ?? 0; // Phí Selling trước thuế
                }
            }
            return revenue;
        }
    }
}
