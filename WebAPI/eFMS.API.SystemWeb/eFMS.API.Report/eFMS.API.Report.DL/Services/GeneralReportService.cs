using eFMS.API.Report.DL.Common;
using eFMS.API.Report.DL.IService;
using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Remotion.Linq.Clauses.ResultOperators;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace eFMS.API.Report.DL.Services
{
    public class GeneralReportService : IGeneralReportService
    {
        private readonly IContextBase<OpsTransaction> opsRepository;
        private readonly IContextBase<CsTransactionDetail> detailRepository;
        private readonly IContextBase<CsTransaction> tranRepository;
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
        private readonly IContextBase<SysImageDetail> imageDetailRepository;
        private readonly IContextBase<SysAttachFileTemplate> sysattachRepository;
        private readonly IContextBase<CatDepartment> deptRepository;
        private readonly IContextBase<SysGroup> groupRepository;

        private eFMSDataContextDefault DC => (eFMSDataContextDefault)opsRepository.DC;

        public GeneralReportService(
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
            IContextBase<CatIncoterm> catIncoterm,
            IContextBase<SysImageDetail> imageDetailRepo,
            IContextBase<CsTransaction> tranRepo,
            IContextBase<SysAttachFileTemplate> sysattachRepo,
            IContextBase<CatDepartment> deptRepo,
            IContextBase<SysGroup> groupRepo,
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
                catIncotermRepository = catIncoterm;
                imageDetailRepository = imageDetailRepo;
                tranRepository = tranRepo;
                sysattachRepository = sysattachRepo;
            deptRepository = deptRepo;
            groupRepository= groupRepo;
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
            //var dataShipment = QueryDataDocumentation(criteria);
            var dataShipment = GetDataGeneralReport(criteria);
            List<GeneralReportResult> dataList = new List<GeneralReportResult>();
            var hblIds = dataShipment.Select(x => x.HblId).Distinct().ToList();
            var LstSurcharge = surCharge.Get(x => hblIds.Contains(x.Hblid)).ToList();

            var customerIds = dataShipment.Where(x => x.CustomerId != null).Select(x => x.CustomerId).ToList();
            var carrierIds = dataShipment.Where(x => x.ColoaderId != null).Select(x => x.ColoaderId).ToList();
            var agentIds = dataShipment.Where(x => x.AgentId != null).Select(x => x.AgentId).ToList();

            var partnerIds = customerIds.Union(carrierIds).Union(agentIds).Distinct().ToList();
            var partnerList = catPartnerRepo.Get(x => partnerIds.Contains(x.Id));
            Dictionary<string, CatPartner> partnerMap = partnerList.ToDictionary(x => x.Id);

            var polIds = dataShipment.Where(x => x.Pol != null).Select(x => x.Pol).ToList();
            var podIds = dataShipment.Where(x => x.Pol != null).Select(x => x.Pod).ToList();
            var portIds = polIds.Union(podIds);
            
            Dictionary<Guid, CatPlace> placeMap = catPlaceRepo.Get(x => portIds.Contains(x.Id)).ToDictionary(x => x.Id);

            var lookupUser = sysUserRepo.Get().ToLookup(q => q.Id);
            var lookupEmployee = sysEmployeeRepo.Get().ToLookup(q => q.Id);


            int _no = 1;
            foreach (var item in dataShipment)
            {
                GeneralReportResult data = new GeneralReportResult();
                data.JobId = item.JobNo;
                data.Mawb = item.Mawb;
                data.Hawb = item.HwbNo;
       
                if (item.CustomerId != null && partnerMap.TryGetValue(item.CustomerId, out var customer))
                {
                    data.CustomerName = customer.PartnerNameEn;
                }

                if (item.ColoaderId != null && partnerMap.TryGetValue(item.ColoaderId, out var carrier))
                {
                    data.CarrierName = carrier.PartnerNameEn;
                }
                if (item.AgentId != null && partnerMap.TryGetValue(item.AgentId, out var agent))
                {
                    data.AgentName = agent.PartnerNameEn;
                }
                data.ServiceDate = item.ServiceDate;
                data.VesselFlight = item.FlightNo;

                var _polCode = string.Empty;
                var _podCode = string.Empty;

                if (item.Pol != null && placeMap.TryGetValue((Guid)item.Pol, out var pol))
                {
                    _polCode = pol.Code;
                }
                if (item.Pod != null && placeMap.TryGetValue((Guid)item.Pod, out var pod))
                {
                    _podCode = pod.Code;
                }
                data.Route = _polCode + (!string.IsNullOrEmpty(_polCode) || !string.IsNullOrEmpty(_podCode) ? "/" : "") + _podCode;

                //Qty lấy theo Housebill
                data.Qty = item.PackageQty ?? 0;
                data.ChargeWeight = item.ChargeWeight ?? 0;
                var _charge = LstSurcharge.Where(x => x.Hblid == item.HblId);

                #region -- Phí Selling trước thuế --
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeSell = _charge.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE);
                    data.Revenue = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? _chargeSell.Sum(x => x.AmountVnd ?? 0) : _chargeSell.Sum(x => x.AmountUsd ?? 0);
                }

                #endregion -- Phí Selling trước thuế --

                #region -- Phí Buying trước thuế --
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeBuy = _charge.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE);
                    data.Cost = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? _chargeBuy.Sum(x => x.AmountVnd ?? 0) : _chargeBuy.Sum(x => x.AmountUsd ?? 0);
                }

                #endregion -- Phí Buying trước thuế --

                data.Profit = data.Revenue - data.Cost;

                #region -- Phí OBH sau thuế --
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeObh = _charge.Where(x => x.Type == ReportConstants.CHARGE_OBH_TYPE);
                    data.Obh = criteria.Currency == ReportConstants.CURRENCY_LOCAL ? _chargeObh.Sum(x => x.AmountVnd + x.VatAmountVnd ?? 0) : _chargeObh.Sum(x => x.AmountUsd + x.VatAmountUsd ?? 0);
                }

                #endregion -- Phí OBH sau thuế --

                var _empPic = lookupUser[item.PersonInCharge].FirstOrDefault()?.EmployeeId;
                if (!string.IsNullOrEmpty(_empPic))
                {
                    data.PersonInCharge = lookupEmployee[_empPic].FirstOrDefault()?.EmployeeNameEn;
                }

                var _empSale = lookupUser[item.SalemanId].FirstOrDefault()?.EmployeeId;
                if (!string.IsNullOrEmpty(_empSale))
                {
                    data.Salesman = lookupEmployee[_empSale].FirstOrDefault()?.EmployeeNameEn;
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
            //var lstSurchage = surCharge.Get();
            //var detailLookupSur = lstSurchage.ToLookup(q => q.Hblid);
            var hblIds = dataShipment.Select(x => x.HblId).Distinct().ToList();
            var LstSurcharge = surCharge.Get(x => hblIds.Contains(x.Hblid)).ToList();

            var PlaceList = catPlaceRepo.Get();
            var PartnerList = catPartnerRepo.Get();
            var LookupPartner = PartnerList.ToLookup(x => x.Id);
            var LookupPlace = PlaceList.ToLookup(x => x.Id);
            var ChargeList = catChargeRepo.Get();
            var LookupCharge = ChargeList.ToLookup(x => x.Id);
            var UserList = sysUserRepo.Get();
            var LookupUser = UserList.ToLookup(x => x.Id);
            var ChargeGroupList = catChargeGroupRepo.Get();
            var ChargeGroupLookup = ChargeGroupList.ToLookup(x => x.Id);
            var OfficeList = sysOfficeRepo.Get();
            var LookupOffice = OfficeList.ToLookup(x => x.Id);
            var UserLevelList = sysUserLevelRepo.Get();
            var LookupUserLevelList = UserLevelList.ToLookup(x => x.UserId);
            var UnitList = catUnitRepo.Get();
            var LookupUnitList = UnitList.ToLookup(x => x.Id);
            foreach (var item in dataShipment)
            {
                GeneralExportShipmentOverviewResult data = new GeneralExportShipmentOverviewResult();
                if (item.TransactionType == TermData.InlandTrucking)
                {
                    data.ServiceName = "Inland Trucking ";
                }
                if (item.TransactionType == TermData.AirExport)
                {
                    data.ServiceName = "Export (Air) ";
                }
                if (item.TransactionType == TermData.AirImport)
                {
                    data.ServiceName = "Import (Air) ";
                }
                if (item.TransactionType == TermData.SeaConsolExport)
                {
                    data.ServiceName = "Export (Sea Consol) ";
                }
                if (item.TransactionType == TermData.SeaConsolImport)
                {
                    data.ServiceName = "Import (Sea Consol) ";
                }
                if (item.TransactionType == TermData.SeaFCLExport)
                {
                    data.ServiceName = "Export (Sea FCL) ";
                }
                if (item.TransactionType == TermData.SeaFCLImport)
                {
                    data.ServiceName = "Import (Sea FCL) ";
                }
                if (item.TransactionType == TermData.SeaLCLExport)
                {
                    data.ServiceName = "Export (Sea LCL) ";
                }
                if (item.TransactionType == TermData.SeaLCLImport)
                {
                    data.ServiceName = "Import (Sea LCL) ";
                }
                if (item.TransactionType == "CL")
                {
                    data.ServiceName = API.Common.Globals.CustomData.Services.Where(x => x.Value == "CL").FirstOrDefault()?.DisplayName;
                }
                data.JobNo = item.JobNo;
                data.etd = item.Etd;
                data.eta = item.Eta;
                data.ServiceDate = item.ServiceDate;
                data.FlightNo = item.FlightNo;
                data.MblMawb = item.Mawb;
                data.HblHawb = item.HwbNo;
                string pol = (item.Pol != null && item.Pol != Guid.Empty) ? LookupPlace[(Guid)item.Pol].Select(t => t.Code).FirstOrDefault() : string.Empty;
                data.PolPod = (item.Pod != null && item.Pod != Guid.Empty) ? pol + "/" + LookupPlace[(Guid)item.Pod].Select(t => t.Code).FirstOrDefault() : pol;
                data.Carrier = !string.IsNullOrEmpty(item.ColoaderId) ? LookupPartner[item.ColoaderId].FirstOrDefault()?.ShortName : string.Empty;
                data.Agent = LookupPartner[item.AgentId].FirstOrDefault()?.ShortName;
                var ArrayShipperDesc = item.ShipperDescription?.Split("\n").ToArray();
                data.ShipperDescription = ArrayShipperDesc != null && ArrayShipperDesc.Length > 0 ? ArrayShipperDesc[0] : string.Empty;
                var ArrayConsgineeDesc = item.ConsigneeDescription?.Split("\n").ToArray();
                data.ConsigneeDescription = ArrayConsgineeDesc != null && ArrayConsgineeDesc.Length > 0 ? ArrayConsgineeDesc[0] : string.Empty;
                data.Consignee = !string.IsNullOrEmpty(data.ConsigneeDescription) ? data.ConsigneeDescription : LookupPartner[item.ConsigneeId].FirstOrDefault()?.PartnerNameEn;
                data.Shipper = !string.IsNullOrEmpty(data.ShipperDescription) ? data.ShipperDescription : LookupPartner[item.Shipper].FirstOrDefault()?.PartnerNameEn;
                data.ShipmentType = item.ShipmentType;
                data.Salesman = !string.IsNullOrEmpty(item.SalemanId) ? LookupUser[item.SalemanId].FirstOrDefault()?.Username : string.Empty;
                data.AgentName = LookupPartner[item.AgentId].FirstOrDefault()?.PartnerNameVn;
                data.GW = item.GrossWeight;
                data.CW = item.ChargeWeight;
                data.CBM = item.Cbm;
                data.SaleInfo = getSaleManInfo(item.SalemanId);
                data.PICInfo = getPICInfo(item.PersonInCharge);

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
                var chargeList = LstSurcharge.Where(x => x.Hblid == item.HblId);
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeSell = chargeList.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE);
                    foreach (var charge in _chargeSell)
                    {
                        var chargeObj = LookupCharge[charge.ChargeId].Select(t => t).FirstOrDefault();
                        CatChargeGroup ChargeGroupModel = new CatChargeGroup();
                        ChargeGroupModel = charge.ChargeGroup != null && charge.ChargeGroup != Guid.Empty ? ChargeGroupLookup[(Guid)charge.ChargeGroup].FirstOrDefault() : null;
                        if (ChargeGroupModel == null)
                        {
                            ChargeGroupModel = chargeObj.ChargeGroup != null && chargeObj.ChargeGroup != Guid.Empty ? ChargeGroupLookup[(Guid)chargeObj.ChargeGroup].FirstOrDefault() : null;
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
                    var _chargeBuy = chargeList.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE);
                    foreach (var charge in _chargeBuy)
                    {
                        var chargeObj = LookupCharge[charge.ChargeId].Select(t => t).FirstOrDefault();
                        CatChargeGroup ChargeGroupModel = new CatChargeGroup();
                        ChargeGroupModel = charge.ChargeGroup != null && charge.ChargeGroup != Guid.Empty ? ChargeGroupLookup[(Guid)charge.ChargeGroup].FirstOrDefault() : null;
                        if (ChargeGroupModel == null)
                        {
                            ChargeGroupModel = chargeObj.ChargeGroup != null && chargeObj.ChargeGroup != Guid.Empty ? ChargeGroupLookup[(Guid)chargeObj.ChargeGroup].FirstOrDefault() : null;
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
                    var _chargeObh = chargeList.Where(x => x.Type == ReportConstants.CHARGE_OBH_TYPE);
                    foreach (var charge in _chargeObh)
                    {
                        _obh += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, criteria.Currency);
                    }
                }

                data.AmountOBH = _obh;
                #endregion -- Phí OBH sau thuế --
                data.Destination = item.Pod != null && item.Pod != Guid.Empty ? LookupPlace[(Guid)item.Pod].Select(t => t.NameVn).FirstOrDefault() : string.Empty;
                data.RalatedHblHawb = string.Empty;// tạm thời để trống
                data.RalatedJobNo = string.Empty;// tạm thời để trống
                data.HandleOffice = item.OfficeId != null && item.OfficeId != Guid.Empty ? LookupOffice[(Guid)item.OfficeId].Select(t => t.Code).FirstOrDefault() : string.Empty;
                var OfficeSaleman = LookupUserLevelList[item.SalemanId].Select(t => t.OfficeId).FirstOrDefault();
                data.SalesOffice = OfficeSaleman != Guid.Empty && OfficeSaleman != null ? LookupOffice[(Guid)OfficeSaleman].Select(t => t.Code).FirstOrDefault() : string.Empty;
                data.Creator = item.TransactionType == "CL" ? LookupUser[item.PersonInCharge].Select(t => t.Username).FirstOrDefault() : LookupUser[item.UserCreated].Select(t => t.Username).FirstOrDefault();
                data.POINV = item.Pono;
                data.Commodity = item.Commodity;
                data.ProductService = item.ProductService;
                data.ServiceMode = string.Empty;//chua co thong tin
                data.PMTerm = item.PaymentTerm;
                data.ShipmentNotes = item.Notes;
                data.Created = item.DatetimeCreated;
                data.CustomerId = LookupPartner[item.CustomerId].Select(t => t.AccountNo).FirstOrDefault();
                data.CustomerName = LookupPartner[item.CustomerId].Select(t => t.ShortName).FirstOrDefault();
                string Code = item.PackageType != null ? LookupUnitList[(short)item.PackageType].Select(t => t.Code).FirstOrDefault() : string.Empty;
                data.QTy = item.PackageQty.ToString() + " " + Code;
                data.CustomNo = item.TransactionType == "CL" ? GetCustomNoOldOfShipment(item.JobNo) : string.Empty;
                data.BKRefNo = item.BookingNo;
                data.ReferenceNo = item.ReferenceNo;
                lstShipment.Add(data);
            }
            return lstShipment.AsQueryable();
        }

        private SaleManInfo getSaleManInfo(string saleManId)
        {
            var saleMan = sysUserLevelRepo.Get(x => x.UserId == saleManId).FirstOrDefault();
            var group = groupRepository.Get(x => x.Id == saleMan.GroupId).FirstOrDefault();
            var dept=deptRepository.Get(x=>x.Id == saleMan.GroupId).FirstOrDefault();
            return new SaleManInfo
            {
                DeptSaleMan = dept?.Code,
                GroupSaleMan = group?.Code,
            };
        }

        private PICInfo getPICInfo(string picId)
        {
            var pic = sysUserLevelRepo.Get(x => x.UserId == picId).FirstOrDefault();
            var group = groupRepository.Get(x => x.Id == pic.GroupId).FirstOrDefault();
            var dept = deptRepository.Get(x => x.Id == pic.GroupId).FirstOrDefault();
            return new PICInfo
            {
                DeptPIC = dept?.Code,
                GroupPIC = group?.Code,
            };
        }

        private string GetCustomNoOldOfShipment(string jobNo)
        {
            var customNos = customsDeclarationRepo.Get(x => x.JobNo == jobNo).OrderBy(o => o.DatetimeModified).Select(s => s.ClearanceNo);
            return customNos.FirstOrDefault();
        }
        private string GetServiceNameReport(string transactionType)
        {
            string _serviceName = string.Empty;
            switch (transactionType)
            {
                case "IT":
                    _serviceName = "Inland Trucking ";
                    break;
                case "AE":
                    _serviceName = "Export (Air) ";
                    break;
                case "AI":
                    _serviceName = "Import (Air) ";
                    break;
                case "SCE":
                    _serviceName = "Export (Sea Consol) ";
                    break;
                case "SCI":
                    _serviceName = "Import (Sea Consol) ";
                    break;
                case "SFE":
                    _serviceName = "Export (Sea FCL) ";
                    break;
                case "SFI":
                    _serviceName = "Import (Sea FCL) ";
                    break;
                case "SLE":
                    _serviceName = "Export (Sea LCL) ";
                    break;
                case "SLI":
                    _serviceName = "Import (Sea LCL) ";
                    break;
                case "CL":
                    _serviceName = API.Common.Globals.CustomData.Services.Where(x => x.Value == "CL").FirstOrDefault()?.DisplayName;
                    break;
                default:
                    break;
            }

            return _serviceName;
        }
        public IQueryable<GeneralExportShipmentOverviewFCLResult> GetDataGeneralExportShipmentOverviewFCL(GeneralReportCriteria criteria)
        {
            var dataShipment = GeneralExportShipmentOverviewFCL(criteria);
            return dataShipment;
        }

        private IQueryable<GeneralExportShipmentOverviewFCLResult> GeneralExportShipmentOverviewFCL(GeneralReportCriteria criteria)
        {
            List<GeneralExportShipmentOverviewFCLResult> lstShipment = new List<GeneralExportShipmentOverviewFCLResult>();
            var dataShipment = GetDataGeneralReport(criteria);
            if (!dataShipment.Any()) return lstShipment.AsQueryable();
            var PlaceList = catPlaceRepo.Get();
            var PartnerList = catPartnerRepo.Get();
            var ChargeList = catChargeRepo.Get();
            var UserList = sysUserRepo.Get();
            var ChargeGroupList = catChargeGroupRepo.Get();
            var OfficeList = sysOfficeRepo.Get();
            var UserLevelList = sysUserLevelRepo.Get();
            var UnitList = catUnitRepo.Get();
            foreach (var item in dataShipment)
            {
                GeneralExportShipmentOverviewFCLResult data = new GeneralExportShipmentOverviewFCLResult();

                data.ServiceName = GetServiceNameReport(item.TransactionType);
                data.JobNo = item.JobNo;
                data.ServiceDate = item.ServiceDate;
                data.etd = item.Etd;
                data.eta = item.Eta;
                data.FlightNo = item.FlightNo;
                data.MblMawb = item.Mawb;
                data.HblHawb = item.HwbNo;
                string pol = (item.Pol != null && item.Pol != Guid.Empty) ? PlaceList.Where(x => x.Id == item.Pol).Select(t => t.Code).FirstOrDefault() : string.Empty;
                data.PolPod = (item.Pod != null && item.Pod != Guid.Empty) ? pol + "/" + PlaceList.Where(x => x.Id == item.Pod).Select(t => t.Code).FirstOrDefault() : pol;
                data.Carrier = !string.IsNullOrEmpty(item.ColoaderId) ? PartnerList.Where(x => x.Id == item.ColoaderId)?.FirstOrDefault()?.ShortName : string.Empty;
                data.Agent = PartnerList.Where(x => x.Id == item.AgentId)?.FirstOrDefault()?.ShortName;
                var ArrayShipperDesc = item.ShipperDescription?.Split("\n").ToArray();
                data.ShipperDescription = ArrayShipperDesc != null && ArrayShipperDesc.Length > 0 ? ArrayShipperDesc[0] : string.Empty;
                var ArrayConsgineeDesc = item.ConsigneeDescription?.Split("\n").ToArray();
                data.ConsigneeDescription = ArrayConsgineeDesc != null && ArrayConsgineeDesc.Length > 0 ? ArrayConsgineeDesc[0] : string.Empty;
                data.Consignee = !string.IsNullOrEmpty(data.ConsigneeDescription) ? data.ConsigneeDescription : PartnerList.Where(x => x.Id == item.ConsigneeId)?.FirstOrDefault()?.PartnerNameEn;
                data.Shipper = !string.IsNullOrEmpty(data.ShipperDescription) ? data.ShipperDescription : PartnerList.Where(x => x.Id == item.Shipper)?.FirstOrDefault()?.PartnerNameEn;
                data.ShipmentType = item.ServiceType;
                data.Salesman = !string.IsNullOrEmpty(item.SalemanId) ? UserList.Where(x => x.Id == item.SalemanId).FirstOrDefault()?.Username : string.Empty;

                var ArrNotifyPartyDesc = item.NotifyPartyDescription?.Split("\n").ToArray();
                data.AgentName = ArrNotifyPartyDesc != null && ArrNotifyPartyDesc.Length > 0 ? ArrNotifyPartyDesc[0] : string.Empty;

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
                decimal? _totalSellAmountTermial = 0;
                decimal? _totalSellAmountBillFee = 0;
                decimal? _totalSellAmountSealFee = 0;
                decimal? _totalSellAmountTelex = 0;
                decimal? _totalSellAmountAutomated = 0;
                decimal? _totalSellAmountVGM = 0;
                decimal? _totalSellAmountBooking = 0;
                decimal? _totalSellAmountOther = 0;

                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeSell = surCharge.Get(x => x.Hblid == item.HblId && x.Type == ReportConstants.CHARGE_SELL_TYPE);
                    foreach (var charge in _chargeSell)
                    {
                        var chargeObj = ChargeList.Where(x => x.Id == charge.ChargeId).Select(t => t).FirstOrDefault();
                        CatChargeGroup ChargeGroupModel = new CatChargeGroup();
                        ChargeGroupModel = charge.ChargeGroup != null && charge.ChargeGroup != Guid.Empty ? ChargeGroupList.Where(x => x.Id == charge.ChargeGroup).FirstOrDefault() : null;
                        bool isOtherSell = true;
                        if (ChargeGroupModel == null)
                        {
                            ChargeGroupModel = chargeObj.ChargeGroup != null && chargeObj.ChargeGroup != Guid.Empty ? ChargeGroupList.Where(x => x.Id == charge.ChargeGroup).FirstOrDefault() : null;
                        }
                        // tinh total phi chargeGroup freight
                        if (ChargeGroupModel?.Name == "Freight")
                        {
                            isOtherSell = false;
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountFreight += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountFreight += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains Terminal
                        if (chargeObj.ChargeNameEn.ToLower().Equals("terminal handling") || chargeObj.ChargeNameEn.ToLower().Equals("THC"))
                        {
                            isOtherSell = false;
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountTermial += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountTermial += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains bill fee
                        if (chargeObj.ChargeNameEn.ToLower().Equals("bill fee"))
                        {
                            isOtherSell = false;
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountBillFee += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountBillFee += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains seal fee
                        if (chargeObj.ChargeNameEn.ToLower().Equals("seal fee"))
                        {
                            isOtherSell = false;
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountSealFee += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountSealFee += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains telex release
                        if (chargeObj.ChargeNameEn.ToLower().Equals("telex release"))
                        {
                            isOtherSell = false;
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountTelex += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountTelex += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains automated manifest
                        if (chargeObj.ChargeNameEn.ToLower().Equals("automated manifest") || chargeObj.ChargeNameEn.ToLower().Equals("AMS"))
                        {
                            isOtherSell = false;
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountAutomated += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountAutomated += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains vgm admin
                        if (chargeObj.ChargeNameEn.ToLower().Equals("vgm admin"))
                        {
                            isOtherSell = false;
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountVGM += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountVGM += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains booking fee
                        if (chargeObj.ChargeNameEn.ToLower().Equals("booking fee"))
                        {
                            isOtherSell = false;
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountBooking += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountBooking += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        if (isOtherSell == true)
                        {
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
                data.TotalSellTerminal = _totalSellAmountTermial;
                data.TotalSellContainerSealFee = _totalSellAmountSealFee;
                data.TotalSellTelexRelease = _totalSellAmountTelex;
                data.TotalSellAutomated = _totalSellAmountAutomated;
                data.TotalSellVGM = _totalSellAmountVGM;
                data.TotalSellBookingFee = _totalSellAmountBooking;
                data.TotalSellOthers = _totalSellAmountOther;
                data.TotalSell = data.TotalSellFreight + data.TotalSellTerminal + data.TotalSellContainerSealFee + data.TotalSellTelexRelease + data.TotalSellAutomated + data.TotalSellVGM + data.TotalSellBookingFee + data.TotalSellOthers;
                #endregion
                #region -- Phí Buying trước thuế --
                decimal? _totalBuyAmountFreight = 0;
                decimal? _totalBuyAmountTermial = 0;
                decimal? _totalBuyAmountBillFee = 0;
                decimal? _totalBuyAmountSealFee = 0;
                decimal? _totalBuyAmountTelex = 0;
                decimal? _totalBuyAmountAutomated = 0;
                decimal? _totalBuyAmountVGM = 0;
                decimal? _totalBuyAmountBooking = 0;
                decimal? _totalBuyAmountOther = 0;
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeBuy = surCharge.Where(x => x.Hblid == item.HblId && x.Type == ReportConstants.CHARGE_BUY_TYPE);
                    foreach (var charge in _chargeBuy)
                    {
                        var chargeObj = ChargeList.Where(x => x.Id == charge.ChargeId).Select(t => t).FirstOrDefault();
                        CatChargeGroup ChargeGroupModel = new CatChargeGroup();
                        ChargeGroupModel = charge.ChargeGroup != null && charge.ChargeGroup != Guid.Empty ? ChargeGroupList.Where(x => x.Id == charge.ChargeGroup).FirstOrDefault() : null;
                        bool isOther = true;
                        if (ChargeGroupModel == null)
                        {
                            ChargeGroupModel = chargeObj.ChargeGroup != null && chargeObj.ChargeGroup != Guid.Empty ? ChargeGroupList.Where(x => x.Id == charge.ChargeGroup).FirstOrDefault() : null;
                        }
                        // tinh total phi chargeGroup freight
                        if (ChargeGroupModel?.Name == "Freight")
                        {
                            isOther = false;
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountFreight += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountFreight += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains Terminal
                        if (chargeObj.ChargeNameEn.ToLower().Equals("terminal handling") || chargeObj.ChargeNameEn.ToLower().Equals("THC"))
                        {
                            isOther = false;
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountTermial += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountTermial += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains bill fee
                        if (chargeObj.ChargeNameEn.ToLower().Equals("bill fee"))
                        {
                            isOther = false;
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountBillFee += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountBillFee += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains seal fee
                        if (chargeObj.ChargeNameEn.ToLower().Equals("seal fee"))
                        {
                            isOther = false;
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountSealFee += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountSealFee += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains telex release
                        if (chargeObj.ChargeNameEn.ToLower().Equals("telex release"))
                        {
                            isOther = false;
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountTelex += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountTelex += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains automated manifest
                        if (chargeObj.ChargeNameEn.ToLower().Equals("automated manifest") || chargeObj.ChargeNameEn.ToLower().Equals("AMS"))
                        {
                            isOther = false;
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountAutomated += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountAutomated += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains vgm admin
                        if (chargeObj.ChargeNameEn.ToLower().Equals("vgm admin"))
                        {
                            isOther = false;
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountVGM += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountVGM += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains booking fee
                        if (chargeObj.ChargeNameEn.ToLower().Equals("booking fee"))
                        {
                            isOther = false;
                            if (criteria.Currency != ReportConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountBooking += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountBooking += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        if (isOther == true)
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
                data.TotalBuyTerminal = _totalBuyAmountTermial;
                data.TotalBuyContainerSealFee = _totalBuyAmountSealFee;
                data.TotalBuyTelexRelease = _totalBuyAmountTelex;
                data.TotalBuyAutomated = _totalBuyAmountAutomated;
                data.TotalBuyVGM = _totalBuyAmountVGM;
                data.TotalBuyBookingFee = _totalBuyAmountBooking;
                data.TotalBuyOthers = _totalBuyAmountOther;
                data.TotalBuy = data.TotalBuyFreight + data.TotalBuyTerminal + data.TotalBuyContainerSealFee + data.TotalBuyTelexRelease + data.TotalBuyAutomated + data.TotalBuyVGM + data.TotalBuyBookingFee + data.TotalBuyOthers;
                data.Profit = data.TotalSell - data.TotalBuy;
                #endregion -- Phí Buying trước thuế --

                #region -- Phí OBH sau thuế --
                decimal? _obh = 0;
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeObh = surCharge.Get(x => x.Hblid == item.HblId && x.Type == ReportConstants.CHARGE_OBH_TYPE);
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
                data.BKRefNo = item.ReferenceNo;
                data.ReferenceNo = item.ReferenceNo;
                data.FinalDestination = item.FinalDestination;
                lstShipment.Add(data);
            }


            return lstShipment.AsQueryable();
        }

        public IQueryable<GeneralExportShipmentOverviewFCLResult> GetDataGeneralExportShipmentOverviewLCL(GeneralReportCriteria criteria)
        {
            List<GeneralExportShipmentOverviewFCLResult> lstShipment = new List<GeneralExportShipmentOverviewFCLResult>();
            var dataShipment = GetDataGeneralReport(criteria);
            if (!dataShipment.Any()) return null;
            var shipments = dataShipment.OrderBy(x => x.TransactionType).ThenBy(x => x.JobNo);
            var placesData = catPlaceRepo.Get().ToLookup(x => x.Id);
            var partnersData = catPartnerRepo.Get().ToLookup(x => x.Id);
            var usersData = sysUserRepo.Get().ToLookup(u => u.Id);
            var chargesData = surCharge.Get().ToLookup(u => u.Hblid);
            // Get id charges with filter
            var freightChargeId = catChargeGroupRepo.Get(x => x.Name.ToUpper().Contains("FREIGHT"))?.Select(x => x.Id).FirstOrDefault();
            var terminalHandling = catChargeRepo.Get(x => x.ChargeNameEn.ToUpper().Contains("TERMINAL HANDLING") || x.ChargeNameEn.ToUpper().Contains("THC"))?.Select(x => x.Id).ToList();
            var billFee = catChargeRepo.Get(x => x.ChargeNameEn.ToUpper().Contains("BILL FEE"))?.Select(x => x.Id).ToList();
            var telexRelease = catChargeRepo.Get(x => x.ChargeNameEn.ToUpper().Contains("TELEX RELEASE"))?.Select(x => x.Id).ToList();
            var cfsCharge = catChargeRepo.Get(x => x.ChargeNameEn.ToUpper().Contains("CFS"))?.Select(x => x.Id).ToList();
            var securityCharge = catChargeRepo.Get(x => x.ChargeNameEn.ToUpper().Contains("SECURITY") || x.ChargeNameEn.ToUpper().Contains("EBS"))?.Select(x => x.Id).ToList();
            var autoManCharge = catChargeRepo.Get(x => x.ChargeNameEn.ToUpper().Contains("AUTOMATED MANIFEST") || x.ChargeNameEn.ToUpper().Contains("AMS"))?.Select(x => x.Id).ToList();
            var vgmCharge = catChargeRepo.Get(x => x.ChargeNameEn.ToUpper().Contains("VGM"))?.Select(x => x.Id).ToList();
            var lclBookingFee = catChargeRepo.Get(x => x.ChargeNameEn.ToUpper().Contains("LCL BOOKING FEE"))?.Select(x => x.Id).ToList();
            var customFeeName = new string[] { "PICK-UP CHARGE", "CUSTOMS CLEARANCE", "CUSTOMS INSPECTION" };
            var customFee = catChargeRepo.Get(x => Array.Exists(customFeeName, element => x.ChargeNameEn.ToUpper().Contains(element)))?.Select(x => x.Id).ToList();

            foreach (var shipment in shipments)
            {
                var data = new GeneralExportShipmentOverviewFCLResult();
                data.ReferenceNo = shipment.ReferenceNo;
                data.ServiceName = API.Common.Globals.CustomData.Services.Where(x => x.Value == shipment.TransactionType).Select(x => x.DisplayName).FirstOrDefault();
                data.JobNo = shipment.JobNo;
                data.ServiceDate = shipment.ServiceDate;
                data.etd = shipment.Etd;
                data.eta = shipment.Eta;
                data.FlightNo = shipment.FlightNo;
                data.MblMawb = shipment.Mawb;
                data.HblHawb = shipment.HwbNo;
                data.Incoterm = shipment.IncotermId == null ? string.Empty : catIncotermRepository.Get(x => x.Id == shipment.IncotermId).Select(x => x.Code).FirstOrDefault();
                if (shipment.Pol != null && shipment.Pod != null)
                {
                    data.PolPod = placesData[(Guid)shipment.Pol]?.Select(x => x.Code).FirstOrDefault() + "/" + placesData[(Guid)shipment.Pod]?.Select(x => x.Code).FirstOrDefault();
                }
                else
                {
                    if (!(shipment.Pol == null && shipment.Pod == null))
                    {
                        data.PolPod = shipment.Pol == null ? placesData[(Guid)shipment.Pod]?.Select(x => x.Code).FirstOrDefault() :
                                                                            placesData[(Guid)shipment.Pol]?.Select(x => x.Code).FirstOrDefault();
                    }
                }
                data.FinalDestination = shipment.FinalDestination;
                data.Carrier = partnersData[shipment.ColoaderId]?.Select(x => x.ShortName).FirstOrDefault();
                data.AgentName = partnersData[shipment.AgentId]?.Select(x => x.PartnerNameEn).FirstOrDefault();
                data.Shipper = shipment.ShipperDescription?.Split('\n').FirstOrDefault();
                data.Consignee = shipment.ConsigneeDescription?.Split('\n').FirstOrDefault();
                data.ShipmentType = shipment.ShipmentType;
                data.Salesman = usersData[shipment.SalemanId]?.Select(x => x.Username).FirstOrDefault();
                data.NotifyParty = shipment.NotifyPartyDescription?.Split('\n').FirstOrDefault();
                data.PackageQty = shipment.PackageQty;
                data.Cont20 = shipment.Cont20 ?? 0;
                data.Cont40 = shipment.Cont40 ?? 0;
                data.Cont40HC = shipment.Cont40HC ?? 0;
                data.Cont45 = shipment.Cont45 ?? 0;
                data.GW = shipment.GrossWeight;
                data.CW = shipment.ChargeWeight;
                data.CBM = shipment.Cbm;
                var surChargesShipment = chargesData[(Guid)shipment.HblId];
                data.TotalBuy = 0;
                data.TotalSell = 0;
                data.Profit = 0;
                if (criteria.Currency == ReportConstants.CURRENCY_LOCAL)
                {
                    #region -- currency = VND
                    // Freight
                    var _freightCharges = surChargesShipment.Where(x => x.ChargeGroup == freightChargeId);
                    if (_freightCharges?.Count() > 0)
                    {
                        data.TotalBuyFreight = _freightCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSellFreight = _freightCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalBuy += data.TotalBuyFreight;
                        data.TotalSell += data.TotalSellFreight;
                    }
                    // Terminal Handling (THC)
                    var _terminalHandlingCharges = surChargesShipment.Where(x => terminalHandling.Contains(x.ChargeId));
                    if (_terminalHandlingCharges?.Count() > 0)
                    {
                        data.TotalBuyTerminal = _terminalHandlingCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSellTerminal = _terminalHandlingCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalBuy += data.TotalBuyTerminal;
                        data.TotalSell += data.TotalSellTerminal;
                    }
                    // Origin Doc Fee (B/L)
                    var _billFeeCharges = surChargesShipment.Where(x => billFee.Contains(x.ChargeId));
                    if (_billFeeCharges?.Count() > 0)
                    {
                        data.TotalBuyBillFee = _billFeeCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSellBillFee = _billFeeCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalBuy += data.TotalBuyBillFee;
                        data.TotalSell += data.TotalSellBillFee;
                    }
                    // Telex release Fee
                    var _telexReleaseCharges = surChargesShipment.Where(x => telexRelease.Contains(x.ChargeId));
                    if (_telexReleaseCharges?.Count() > 0)
                    {
                        data.TotalBuyTelexRelease = _telexReleaseCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSellTelexRelease = _telexReleaseCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalBuy += data.TotalBuyTelexRelease;
                        data.TotalSell += data.TotalSellTelexRelease;
                    }
                    // Origin Container Freight Station Fee (CFS)
                    var _cfsCharges = surChargesShipment.Where(x => cfsCharge.Contains(x.ChargeId));
                    if (_cfsCharges?.Count() > 0)
                    {
                        data.TotalBuyCFSFee = _cfsCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSellCFSFee = _cfsCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalBuy += data.TotalBuyCFSFee;
                        data.TotalSell += data.TotalSellCFSFee;
                    }
                    // Security (EBS)
                    var _securityCharges = surChargesShipment.Where(x => securityCharge.Contains(x.ChargeId));
                    if (_securityCharges?.Count() > 0)
                    {
                        data.TotalBuyEBSFee = _securityCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSellEBSFee = _securityCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalBuy += data.TotalBuyEBSFee;
                        data.TotalSell += data.TotalSellEBSFee;
                    }
                    // Automated Manifest System (AMS)
                    var _autoManCharges = surChargesShipment.Where(x => autoManCharge.Contains(x.ChargeId));
                    if (_autoManCharges?.Count() > 0)
                    {
                        data.TotalBuyAutomated = _autoManCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSellAutomated = _autoManCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalBuy += data.TotalBuyAutomated;
                        data.TotalSell += data.TotalSellAutomated;
                    }
                    // VGM admin Fee
                    var _vgmCharges = surChargesShipment.Where(x => vgmCharge.Contains(x.ChargeId));
                    if (_vgmCharges?.Count() > 0)
                    {
                        data.TotalBuyVGM = _vgmCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSellVGM = _vgmCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalBuy += data.TotalBuyVGM;
                        data.TotalSell += data.TotalSellVGM;
                    }
                    // Booking fee - LCL
                    var _lclBookingFeeCharges = surChargesShipment.Where(x => lclBookingFee.Contains(x.ChargeId));
                    if (_lclBookingFeeCharges?.Count() > 0)
                    {
                        data.TotalSellBookingFee = _lclBookingFeeCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSell += data.TotalSellBookingFee;
                    }
                    // Pick up charge + Customs fee
                    var _customFeeCharges = surChargesShipment.Where(x => customFee.Contains(x.ChargeId));
                    if (_customFeeCharges?.Count() > 0)
                    {
                        data.TotalBuyCustomFee = _customFeeCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSellCustomFee = _customFeeCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalBuy += data.TotalBuyCustomFee;
                        data.TotalSell += data.TotalSellCustomFee;
                    }
                    // OTHERS Fee
                    var _lclOtherCharges = surChargesShipment.Except(_freightCharges)
                                                                            .Except(_terminalHandlingCharges)
                                                                            .Except(_billFeeCharges)
                                                                            .Except(_telexReleaseCharges)
                                                                            .Except(_cfsCharges)
                                                                            .Except(_securityCharges)
                                                                            .Except(_autoManCharges)
                                                                            .Except(_vgmCharges)
                                                                            .Except(_customFeeCharges);
                    // Total Others Fee Of Buying
                    data.TotalBuyOthers = _lclOtherCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                    // Total Others Fee Of Selling
                    _lclOtherCharges = _lclOtherCharges.Except(_lclBookingFeeCharges);
                    data.TotalSellOthers = _lclOtherCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                    // TOTAL
                    data.TotalBuy += data.TotalBuyOthers;
                    data.TotalSell += data.TotalSellOthers;
                    data.Profit = data.TotalSell - data.TotalBuy;
                    #endregion
                }
                else
                {
                    #region -- currency = USD
                    // Freight
                    var _freightCharges = surChargesShipment.Where(x => x.ChargeGroup == freightChargeId);
                    if (_freightCharges?.Count() > 0)
                    {
                        data.TotalBuyFreight = _freightCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSellFreight = _freightCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalBuy += data.TotalBuyFreight;
                        data.TotalSell += data.TotalSellFreight;
                    }
                    // Terminal Handling (THC)
                    var _terminalHandlingCharges = surChargesShipment.Where(x => terminalHandling.Contains(x.ChargeId));
                    if (_terminalHandlingCharges?.Count() > 0)
                    {
                        data.TotalBuyTerminal = _terminalHandlingCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSellTerminal = _terminalHandlingCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalBuy += data.TotalBuyTerminal;
                        data.TotalSell += data.TotalSellTerminal;
                    }
                    // Origin Doc Fee (B/L)
                    var _billFeeCharges = surChargesShipment.Where(x => billFee.Contains(x.ChargeId));
                    if (_billFeeCharges?.Count() > 0)
                    {
                        data.TotalBuyBillFee = _billFeeCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSellBillFee = _billFeeCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalBuy += data.TotalBuyBillFee;
                        data.TotalSell += data.TotalSellBillFee;
                    }
                    // Telex release Fee
                    var _telexReleaseCharges = surChargesShipment.Where(x => telexRelease.Contains(x.ChargeId));
                    if (_telexReleaseCharges?.Count() > 0)
                    {
                        data.TotalBuyTelexRelease = _telexReleaseCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSellTelexRelease = _telexReleaseCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalBuy += data.TotalBuyTelexRelease;
                        data.TotalSell += data.TotalSellTelexRelease;
                    }
                    // Origin Container Freight Station Fee (CFS)
                    var _cfsCharges = surChargesShipment.Where(x => cfsCharge.Contains(x.ChargeId));
                    if (_cfsCharges?.Count() > 0)
                    {
                        data.TotalBuyCFSFee = _cfsCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSellCFSFee = _cfsCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalBuy += data.TotalBuyCFSFee;
                        data.TotalSell += data.TotalSellCFSFee;
                    }
                    // Security (EBS)
                    var _securityCharges = surChargesShipment.Where(x => securityCharge.Contains(x.ChargeId));
                    if (_securityCharges?.Count() > 0)
                    {
                        data.TotalBuyEBSFee = _securityCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSellEBSFee = _securityCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalBuy += data.TotalBuyEBSFee;
                        data.TotalSell += data.TotalSellEBSFee;
                    }
                    // Automated Manifest System (AMS)
                    var _autoManCharges = surChargesShipment.Where(x => autoManCharge.Contains(x.ChargeId));
                    if (_autoManCharges?.Count() > 0)
                    {
                        data.TotalBuyAutomated = _autoManCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSellAutomated = _autoManCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalBuy += data.TotalBuyAutomated;
                        data.TotalSell += data.TotalSellAutomated;
                    }
                    // VGM admin Fee
                    var _vgmCharges = surChargesShipment.Where(x => vgmCharge.Contains(x.ChargeId));
                    if (_vgmCharges?.Count() > 0)
                    {
                        data.TotalBuyVGM = _vgmCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSellVGM = _vgmCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalBuy += data.TotalBuyVGM;
                        data.TotalSell += data.TotalSellVGM;
                    }
                    // Booking fee - LCL
                    var _lclBookingFeeCharges = surChargesShipment.Where(x => lclBookingFee.Contains(x.ChargeId));
                    if (_lclBookingFeeCharges?.Count() > 0)
                    {
                        data.TotalSellBookingFee = _lclBookingFeeCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSell += data.TotalSellBookingFee;
                    }
                    // Pick up charge + Customs fee
                    var _customFeeCharges = surChargesShipment.Where(x => customFee.Contains(x.ChargeId));
                    if (_customFeeCharges?.Count() > 0)
                    {
                        data.TotalBuyCustomFee = _customFeeCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSellCustomFee = _customFeeCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalBuy += data.TotalBuyCustomFee;
                        data.TotalSell += data.TotalSellCustomFee;
                    }
                    // OTHERS Fee
                    var _lclOtherCharges = surChargesShipment.Except(_freightCharges)
                                                                            .Except(_terminalHandlingCharges)
                                                                            .Except(_billFeeCharges)
                                                                            .Except(_telexReleaseCharges)
                                                                            .Except(_cfsCharges)
                                                                            .Except(_securityCharges)
                                                                            .Except(_autoManCharges)
                                                                            .Except(_vgmCharges)
                                                                            .Except(_customFeeCharges);
                    // Total Others Fee Of Buying
                    data.TotalBuyOthers = _lclOtherCharges.Where(x => x.Type == ReportConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                    // Total Others Fee Of Selling
                    _lclOtherCharges = _lclOtherCharges.Except(_lclBookingFeeCharges);
                    data.TotalSellOthers = _lclOtherCharges.Where(x => x.Type == ReportConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                    // TOTAL
                    data.TotalBuy += data.TotalBuyOthers;
                    data.TotalSell += data.TotalSellOthers;
                    data.Profit = data.TotalSell - data.TotalBuy;
                    #endregion
                }
                #region -- Phí OBH sau thuế --
                decimal? _obh = 0;
                var surChargesObh = surChargesShipment.Where(x => x.Type == ReportConstants.CHARGE_OBH_TYPE);
                foreach (var charge in surChargesObh)
                {
                    _obh += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, criteria.Currency);
                }

                data.AmountOBH = _obh;
                #endregion -- Phí OBH sau thuế --
                lstShipment.Add(data);
            }
            return lstShipment.AsQueryable();
        }
    }
}
