using eFMS.API.Report.DL.Common;
using eFMS.API.Report.DL.IService;
using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace eFMS.API.Report.DL.Services
{
    public class GeneralReportService : IGeneralReportService
    {
        readonly IContextBase<OpsTransaction> opsRepository;
        readonly IContextBase<CsTransactionDetail> detailRepository;
        readonly IContextBase<CsShipmentSurcharge> surCharge;
        readonly IContextBase<CatPartner> catPartnerRepo;
        private readonly ICurrentUser currentUser;
        readonly IContextBase<SysUser> sysUserRepo;
        readonly IContextBase<SysEmployee> sysEmployeeRepo;
        readonly IContextBase<CatCurrencyExchange> catCurrencyExchangeRepo;
        readonly IContextBase<CatPlace> catPlaceRepo;
        readonly IContextBase<CatUnit> catUnitRepo;
        readonly IContextBase<CatCharge> catChargeRepo;
        readonly IContextBase<CatChargeGroup> catChargeGroupRepo;
        readonly IContextBase<SysOffice> sysOfficeRepo;
        readonly IContextBase<SysUserLevel> sysUserLevelRepo;
        readonly IContextBase<CustomsDeclaration> customsDeclarationRepo;
        private readonly ICurrencyExchangeService currencyExchangeService;


        private eFMSDataContextDefault DC => (eFMSDataContextDefault)opsRepository.DC;

        public GeneralReportService(ICurrentUser currentUser,
            IContextBase<OpsTransaction> ops,
            IContextBase<CsTransactionDetail> detail,
            IContextBase<CsShipmentSurcharge> surcharge,
            IContextBase<CatPartner> catPartner,
            ICurrentUser user,
            ICurrencyExchangeService currencyExchange,
            IContextBase<SysUser> sysUser,
            IContextBase<SysEmployee> sysEmployee,
            IContextBase<CatCurrencyExchange> catCurrencyExchange,
            IContextBase<CatPlace> catPlace,
            IContextBase<CatChargeGroup> catChargeGroup,
            IContextBase<SysOffice> sysOffice,
            IContextBase<SysUserLevel> sysUserLevel,
            IContextBase<CatUnit> catUnit,
            IContextBase<CatDepartment> departmentRepo,
            IContextBase<CsMawbcontainer> csMawbcontainer,
            IContextBase<CatCharge> catCharge,
            IContextBase<CatChargeGroup> ChargeGroup,
            IContextBase<SysOffice> Office,
            IContextBase<CustomsDeclaration> customsDeclaration,
            IContextBase<SysUserLevel> UserLevel)
        {
            opsRepository = ops;
            detailRepository = detail;
            surCharge = surcharge;
            catPartnerRepo = catPartner;
            currentUser = user;
            sysEmployeeRepo = sysEmployee;
            sysUserRepo = sysUser;
            catCurrencyExchangeRepo = catCurrencyExchange;
            catPlaceRepo = catPlace;
            catChargeRepo = catCharge;
            catChargeGroupRepo = ChargeGroup;
            sysOfficeRepo = Office;
            sysUserLevelRepo = UserLevel;
            customsDeclarationRepo = customsDeclaration;
            currencyExchangeService = currencyExchange;
            catUnitRepo = catUnit;
        }

        public List<GeneralReportResult> GetDataGeneralReport(GeneralReportCriteria criteria, int page, int size, out int rowsCount)
        {
            var dataDocumentation = GeneralReportDocumentation(criteria);
            IQueryable<GeneralReportResult> list;
            list = dataDocumentation;
            var results = new List<GeneralReportResult>();
            if (list == null)
            {
                rowsCount = 0;
                return results;
            }
            
            rowsCount = list.Select(s => s.No).Count();
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = list.Skip((page - 1) * size).Take(size).ToList();
            }
            return results;
        }

        private IQueryable<GeneralReportResult> GeneralReportDocumentation(GeneralReportCriteria criteria)
        {
            var dataShipment = GetDataGeneralReport(criteria);
            List<GeneralReportResult> dataList = new List<GeneralReportResult>();
            var LstSurcharge = surCharge.Get();
            //  LookupSurchage = LstSurcharge.ToLookup(x => x.Hblid);
            var PartnerList = catPartnerRepo.Get();
           //  var LookupPartner = PartnerList.ToLookup(x => x.Id);
            // var PlaceLookup = catPlaceRepo.Get().ToLookup(q => q.Id);
            var PlaceLookup = catPlaceRepo.Get();
            // var lookupUser = sysUserRepo.Get().ToLookup(q => q.Id);
            var lookupUser = sysUserRepo.Get();
            //var lookupEmployee = sysEmployeeRepo.Get().ToLookup(q => q.Id);
            var lookupEmployee = sysEmployeeRepo.Get();
            int _no = 1;

            foreach (var item in dataShipment)
            {
                GeneralReportResult data = new GeneralReportResult();
                data.JobId = item.JobNo;
                data.Mawb = item.Mawb;
                data.Hawb = item.HwbNo;
                data.CustomerName = PartnerList.Where(x => x.Id == item.CustomerId).FirstOrDefault()?.PartnerNameEn;
                data.CarrierName = PartnerList.Where(x => x.Id == item.ColoaderId).FirstOrDefault()?.PartnerNameEn;
                data.AgentName = PartnerList.Where(x => x.Id == item.AgentId).FirstOrDefault()?.PartnerNameEn;
                data.ServiceDate = item.ServiceDate;
                data.VesselFlight = item.FlightNo;

                var _polCode = item.Pol != null ? PlaceLookup.Where(x => x.Id == item.Pol).FirstOrDefault()?.Code : string.Empty;
                var _podCode = item.Pod != null ? PlaceLookup.Where(x => x.Id == item.Pod).FirstOrDefault()?.Code : string.Empty;
                data.Route = _polCode + (!string.IsNullOrEmpty(_polCode) || !string.IsNullOrEmpty(_podCode) ? "/" : "") + _podCode;

                //Qty lấy theo Housebill
                data.Qty = item.PackageQty ?? 0;
                data.ChargeWeight = item.ChargeWeight ?? 0;

                #region -- Phí Selling trước thuế --
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeSell = LstSurcharge.Where(x => x.Hblid == item.HblId && x.Type == ReportConstants.CHARGE_SELL_TYPE);
                    data.Revenue = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? _chargeSell.Sum(x => x.AmountVnd ?? 0) : _chargeSell.Sum(x => x.AmountUsd ?? 0);
                }

                #endregion -- Phí Selling trước thuế --

                #region -- Phí Buying trước thuế --
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeBuy = LstSurcharge.Where(x => x.Hblid == item.HblId && x.Type == ReportConstants.CHARGE_BUY_TYPE);
                    data.Cost = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? _chargeBuy.Sum(x => x.AmountVnd ?? 0) : _chargeBuy.Sum(x => x.AmountUsd ?? 0);
                }

                #endregion -- Phí Buying trước thuế --

                data.Profit = data.Revenue - data.Cost;

                #region -- Phí OBH sau thuế --
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeObh = LstSurcharge.Where(x => x.Hblid == item.HblId && x.Type == ReportConstants.CHARGE_OBH_TYPE);
                    data.Obh = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? _chargeObh.Sum(x => x.AmountVnd + x.VatAmountVnd ?? 0) : _chargeObh.Sum(x => x.AmountUsd + x.VatAmountUsd ?? 0);
                }

                #endregion -- Phí OBH sau thuế --

                var _empPic = lookupUser.Where(x => x.Id == item.PersonInCharge).FirstOrDefault()?.EmployeeId;
                if (!string.IsNullOrEmpty(_empPic))
                {
                    data.PersonInCharge = lookupEmployee.Where(x => x.Id == _empPic).FirstOrDefault()?.EmployeeNameEn;
                }

                var _empSale = lookupUser.Where(x => x.Id == item.SalemanId).FirstOrDefault()?.EmployeeId;
                if (!string.IsNullOrEmpty(_empSale))
                {
                    data.Salesman = lookupEmployee.Where(x => x.Id == _empSale).FirstOrDefault()?.EmployeeNameEn;
                }

                data.ServiceName = API.Common.Globals.CustomData.Services.Where(x => x.Value == item.TransactionType).FirstOrDefault()?.DisplayName;

                data.No = _no;
                dataList.Add(data);

                _no++;
            }
            return dataList.AsQueryable();
        }

        private List<sp_GetDataGeneralReport> GetDataGeneralReport(GeneralReportCriteria criteria)
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
            //var list = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetDataGeneralReport>(parameters);
            var list = DC.ExecuteProcedure<sp_GetDataGeneralReport>(parameters);
            return list;
        }

        public IQueryable<GeneralReportResult> QueryDataGeneralReport(GeneralReportCriteria criteria)
        {
            var dataDocumentation = GeneralReportDocumentation(criteria);
            return dataDocumentation;
        }

        public IQueryable<GeneralExportShipmentOverviewResult> GetDataGeneralExportShipmentOverview(GeneralReportCriteria criteria)
        {
            var dataShipment = GeneralExportShipmentOverview(criteria);
            return dataShipment;
        }

        public IQueryable<GeneralExportShipmentOverviewResult> GeneralExportShipmentOverview(GeneralReportCriteria criteria)
        {
            List<GeneralExportShipmentOverviewResult> lstShipment = new List<GeneralExportShipmentOverviewResult>();
            var dataShipment = GetDataGeneralReport(criteria);
            if (!dataShipment.Any()) return lstShipment.AsQueryable();
            var lstSurchage = surCharge.Get();
            // var detailLookupSur = lstSurchage.ToLookup(q => q.Hblid);
            var PlaceList = catPlaceRepo.Get();
            var PartnerList = catPartnerRepo.Get();
            // var LookupPartner = PartnerList.ToLookup(x => x.Id);
            // var LookupPlace = PlaceList.ToLookup(x => x.Id);
            var ChargeList = catChargeRepo.Get();
            // var LookupCharge = ChargeList.ToLookup(x => x.Id);
            var UserList = sysUserRepo.Get();
            // var LookupUser = UserList.ToLookup(x => x.Id);
            var ChargeGroupList = catChargeGroupRepo.Get();
            // var ChargeGroupLookup = ChargeGroupList.ToLookup(x => x.Id);
            var OfficeList = sysOfficeRepo.Get();
            // var LookupOffice = OfficeList.ToLookup(x => x.Id);
            var UserLevelList = sysUserLevelRepo.Get();
            // var LookupUserLevelList = UserLevelList.ToLookup(x => x.UserId);
            var UnitList = catUnitRepo.Get();
            // var LookupUnitList = UnitList.ToLookup(x => x.Id);
            foreach (var item in dataShipment)
            {
                GeneralExportShipmentOverviewResult data = new GeneralExportShipmentOverviewResult();
                switch (item.TransactionType)
                {
                    case "IT":
                        data.ServiceName = "Inland Trucking ";
                        break;
                    case "AE":
                        data.ServiceName = "Export (Air) ";
                        break;
                    case "AI":
                        data.ServiceName = "Import (Air) ";
                        break;
                    case "SCE":
                        data.ServiceName = "Export (Sea Consol) ";
                        break;
                    case "SCI":
                        data.ServiceName = "Import (Sea Consol) ";
                        break;
                    case "SFE":
                        data.ServiceName = "Export (Sea FCL) ";
                        break;
                    case "SFI":
                        data.ServiceName = "Import (Sea FCL) ";
                        break;
                    case "SLE":
                        data.ServiceName = "Export (Sea LCL) ";
                        break;
                    case "SLI":
                        data.ServiceName = "Import (Sea LCL) ";
                        break;
                    case "CL":
                        data.ServiceName = API.Common.Globals.CustomData.Services.Where(x => x.Value == "CL").FirstOrDefault()?.DisplayName;
                        break;
                    default:
                        break;
                }
             
                data.JobNo = item.JobNo;
                data.etd = item.Etd;
                data.eta = item.Eta;
                data.ServiceDate = item.ServiceDate;
                data.FlightNo = item.FlightNo;
                data.MblMawb = item.Mawb;
                data.HblHawb = item.HwbNo;
                string pol = (item.Pol != null && item.Pol != Guid.Empty) ? PlaceList.Where(x => x.Id == item.Pol).Select(t => t.Code).FirstOrDefault() : string.Empty;
                data.PolPod = (item.Pod != null && item.Pod != Guid.Empty) ? pol + "/" + PlaceList.Where(x => x.Id == item.Pod).Select(t => t.Code).FirstOrDefault() : pol;
                data.Carrier = !string.IsNullOrEmpty(item.ColoaderId) ? PartnerList.Where(x => x.Id == item.ColoaderId).FirstOrDefault()?.ShortName : string.Empty;
                data.Agent = PartnerList.Where(x => x.Id == item.AgentId).FirstOrDefault()?.ShortName;
                var ArrayShipperDesc = item.ShipperDescription?.Split("\n").ToArray();
                data.ShipperDescription = ArrayShipperDesc != null && ArrayShipperDesc.Length > 0 ? ArrayShipperDesc[0] : string.Empty;
                var ArrayConsgineeDesc = item.ConsigneeDescription?.Split("\n").ToArray();
                data.ConsigneeDescription = ArrayConsgineeDesc != null && ArrayConsgineeDesc.Length > 0 ? ArrayConsgineeDesc[0] : string.Empty;
                data.Consignee = !string.IsNullOrEmpty(data.ConsigneeDescription) ? data.ConsigneeDescription : PartnerList.Where(x => x.Id == item.ConsigneeId).FirstOrDefault()?.PartnerNameEn;
                data.Shipper = !string.IsNullOrEmpty(data.ShipperDescription) ? data.ShipperDescription : PartnerList.Where(x => x.Id == item.Shipper).FirstOrDefault()?.PartnerNameEn;
                data.ShipmentType = item.ShipmentType;
                data.Salesman = !string.IsNullOrEmpty(item.SalemanId) ? UserList.Where(x => x.Id == item.SalemanId).FirstOrDefault()?.Username : string.Empty;
                data.AgentName = PartnerList.Where(x => x.Id == item.AgentId).FirstOrDefault()?.PartnerNameVn;
                data.GW = item.GrossWeight;
                data.CW = item.ChargeWeight;
                data.CBM = item.Cbm;

                data.Cont20 = item.Cont20 ?? 0;
                data.Cont40 = item.Cont40 ?? 0;
                data.Cont40HC = item.Cont40HC ?? 0;
                data.Cont45 = item.Cont45 ?? 0;

                data.QTy = item.PackageQty.ToString();
                #region -- Phí Selling trước thuế --
                decimal? _totalSellAmountFreight = 0;
                decimal? _totalSellAmountTrucking = 0;
                decimal? _totalSellAmountHandling = 0;
                decimal? _totalSellAmountOther = 0;
                decimal? _totalSellCustom = 0;
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeSell = lstSurchage.Where(x => x.Hblid == item.HblId && x.Type == ReportConstants.CHARGE_SELL_TYPE);
                    foreach (var charge in _chargeSell)
                    {
                        var chargeObj = ChargeList.Where(x => x.Id == charge.ChargeId).Select(t => t).FirstOrDefault();
                        CatChargeGroup ChargeGroupModel = new CatChargeGroup();
                        ChargeGroupModel = charge.ChargeGroup != null && charge.ChargeGroup != Guid.Empty ? ChargeGroupList.Where(x => x.Id == charge.ChargeGroup).FirstOrDefault() : null;
                        if (ChargeGroupModel == null)
                        {
                            ChargeGroupModel = chargeObj.ChargeGroup != null && chargeObj.ChargeGroup != Guid.Empty ? ChargeGroupList.Where(x => x.Id == chargeObj.ChargeGroup).FirstOrDefault() : null;
                        }
                        // tinh total phi chargeGroup freight
                        if (ChargeGroupModel?.Name == "Freight")
                        {
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountFreight += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountFreight += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        else if (ChargeGroupModel?.Name == "Trucking")
                        {
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountTrucking += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountTrucking += charge.AmountVnd;  // Phí Selling trước thuế
                            }
                        }
                        else if (ChargeGroupModel?.Name == "Handling")
                        {
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountHandling += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountHandling += charge.AmountVnd;  // Phí Selling trước thuế
                            }
                        }
                        // bổ sung total custom sell
                        else if (chargeObj.Type == "DEBIT" && ChargeGroupModel?.Name == "Logistics")
                        {
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalSellCustom += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellCustom += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        else
                        {
                            // Amount other = sum các phí debit có charge group khác Freight,Trucking,handling,Logistics
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountOther += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountOther += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        //END SEL
                    }
                }
                data.TotalSellFreight = _totalSellAmountFreight;
                data.TotalSellTrucking = _totalSellAmountTrucking;
                data.TotalSellHandling = _totalSellAmountHandling;
                data.TotalSellOthers = _totalSellAmountOther;
                data.TotalCustomSell = _totalSellCustom;
                #region [CR: 19/01/22] Alex bo rule nay
                //if (data.TotalSellOthers > 0 && data.TotalCustomSell > 0)
                //{
                //    data.TotalSellOthers = data.TotalSellOthers - data.TotalCustomSell;
                //}
                #endregion
                data.TotalSell = data.TotalSellFreight + data.TotalSellTrucking + data.TotalSellHandling + data.TotalSellOthers + data.TotalCustomSell;
                #endregion
                #region -- Phí Buying trước thuế --
                decimal? _totalBuyAmountFreight = 0;
                decimal? _totalBuyAmountTrucking = 0;
                decimal? _totalBuyAmountHandling = 0;
                decimal? _totalBuyAmountOther = 0;
                decimal? _totalBuyAmountKB = 0;
                decimal? _totalBuyCustom = 0;
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeBuy = lstSurchage.Where(x => x.Hblid == item.HblId && x.Type == ReportConstants.CHARGE_BUY_TYPE);
                    foreach (var charge in _chargeBuy)
                    {
                        var chargeObj = ChargeList.Where(x => x.Id == charge.ChargeId).Select(t => t).FirstOrDefault();
                        CatChargeGroup ChargeGroupModel = new CatChargeGroup();
                        ChargeGroupModel = charge.ChargeGroup != null && charge.ChargeGroup != Guid.Empty ? ChargeGroupList.Where(x => x.Id == charge.ChargeGroup).FirstOrDefault() : null;
                        if (ChargeGroupModel == null)
                        {
                            ChargeGroupModel = chargeObj.ChargeGroup != null && chargeObj.ChargeGroup != Guid.Empty ? ChargeGroupList.Where(x => x.Id == chargeObj.ChargeGroup).FirstOrDefault() : null;
                        }
                        // tinh total phi chargeGroup freight
                        if (ChargeGroupModel?.Name == "Freight")
                        {
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountFreight += charge.AmountUsd; // Phí Selling trước thuế
                            }
                            else
                            {
                                _totalBuyAmountFreight += charge.AmountVnd;
                            }
                        }
                        else if (ChargeGroupModel?.Name == "Trucking")
                        {
                            if (charge.KickBack == true)
                            {
                                _totalBuyAmountTrucking = 0;
                            }
                            else
                            {
                                if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                                {
                                    _totalBuyAmountTrucking += charge.AmountUsd;
                                }
                                else
                                {
                                    _totalBuyAmountTrucking += charge.AmountVnd; // Phí Selling trước thuế
                                }
                            }
                        }
                        else if (ChargeGroupModel?.Name == "Handling")
                        {
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountHandling += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountHandling += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        else if (charge.KickBack == true || ChargeGroupModel?.Name == "Com")
                        {
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountKB += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountKB += charge.AmountVnd;
                            }
                        }
                        else if (chargeObj.Type == "CREDIT" && ChargeGroupModel?.Name == "Logistics") // bổ sung total custom buy
                        {
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyCustom += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyCustom += charge.AmountVnd; // Phí buying trước thuế
                            }
                        }
                        // Amount other = sum các phí có charge group khác Freight,Trucking,handling,Logistics,com
                        //if (ChargeGroupModel?.Name != "Handling" && ChargeGroupModel?.Name != "Trucking" && ChargeGroupModel?.Name != "Freight" && ChargeGroupModel?.Name != "Com" && ChargeGroupModel?.Name != "Logistics" && charge.KickBack != true)
                        else
                        {
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountOther += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountOther += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        //END BUY
                    }
                }

                data.TotalBuyFreight = _totalBuyAmountFreight;
                data.TotalBuyTrucking = _totalBuyAmountTrucking;
                data.TotalBuyHandling = _totalBuyAmountHandling;
                data.TotalBuyOthers = _totalBuyAmountOther;
                data.TotalBuyKB = _totalBuyAmountKB;
                data.TotalCustomBuy = _totalBuyCustom;
                data.TotalBuy = data.TotalBuyFreight + data.TotalBuyTrucking + data.TotalBuyHandling + data.TotalBuyOthers + data.TotalBuyKB + data.TotalCustomBuy;
                data.Profit = data.TotalSell - data.TotalBuy;
                #endregion -- Phí Buying trước thuế --

                #region -- Phí OBH sau thuế --
                decimal? _obh = 0;
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeObh = lstSurchage.Where(x => x.Hblid == item.HblId && x.Type == ReportConstants.CHARGE_OBH_TYPE);
                    foreach (var charge in _chargeObh)
                    {
                        _obh += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, criteria.Currency);
                    }
                }

                data.AmountOBH = _obh;
                #endregion -- Phí OBH sau thuế --
                data.Destination = item.Pod != null && item.Pod != Guid.Empty ? PlaceList.Where(x => x.Id == item.Pod).Select(t => t.NameVn).FirstOrDefault() : string.Empty;
                data.RalatedHblHawb = string.Empty;// tạm thời để trống
                data.RalatedJobNo = string.Empty;// tạm thời để trống
                data.HandleOffice = item.OfficeId != null && item.OfficeId != Guid.Empty ? OfficeList.Where(x => x.Id == item.OfficeId).Select(t => t.Code).FirstOrDefault() : string.Empty;
                var OfficeSaleman = UserLevelList.Where(x => x.UserId == item.SalemanId).Select(t => t.OfficeId).FirstOrDefault();
                data.SalesOffice = OfficeSaleman != Guid.Empty && OfficeSaleman != null ? OfficeList.Where(x => x.Id == OfficeSaleman).Select(t => t.Code).FirstOrDefault() : string.Empty;
                data.Creator = item.TransactionType == "CL" ? UserList.Where(x => x.Id == item.PersonInCharge).Select(t => t.Username).FirstOrDefault() : UserList.Where(x => x.Id == item.UserCreated).Select(t => t.Username).FirstOrDefault();
                data.POINV = item.Pono;
                data.Commodity = item.Commodity;
                data.ProductService = item.ProductService;
                data.ServiceMode = string.Empty;//chua co thong tin
                data.PMTerm = item.PaymentTerm;
                data.ShipmentNotes = item.Notes;
                data.Created = item.DatetimeCreated;
                var customer = PartnerList.Where(x => x.Id == item.CustomerId)?.FirstOrDefault();
                data.CustomerId = customer.AccountNo;
                data.CustomerName = customer.ShortName;
                string Code = item.PackageType != null ? UnitList.Where(x => x.Id == item.PackageType).Select(t => t.Code).FirstOrDefault() : string.Empty;
                data.QTy = item.PackageQty.ToString() + " " + Code;
                data.CustomNo = item.TransactionType == "CL" ? GetCustomNoOldOfShipment(item.JobNo) : string.Empty;
                data.BKRefNo = item.BookingNo;
                data.ReferenceNo = item.ReferenceNo;
                lstShipment.Add(data);
            }
            return lstShipment.AsQueryable();
        }

        private string GetCustomNoOldOfShipment(string jobNo)
        {
            var customNos = customsDeclarationRepo.Get(x => x.JobNo == jobNo).OrderBy(o => o.DatetimeModified).Select(s => s.ClearanceNo);
            return customNos.FirstOrDefault();
        }
    }
}
