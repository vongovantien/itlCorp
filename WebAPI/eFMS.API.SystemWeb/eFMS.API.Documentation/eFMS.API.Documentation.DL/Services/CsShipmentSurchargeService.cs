using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsShipmentSurchargeService : RepositoryBase<CsShipmentSurcharge, CsShipmentSurchargeModel>, ICsShipmentSurchargeService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IContextBase<CsTransactionDetail> tranDetailRepository;
        private readonly IContextBase<CatPartner> partnerRepository;
        private readonly IContextBase<OpsTransaction> opsTransRepository;
        private readonly IContextBase<CatCurrencyExchange> currentExchangeRateRepository;
        private readonly IContextBase<CsTransaction> csTransactionRepository;
        private readonly IContextBase<CatCharge> catChargeRepository;
        private readonly IContextBase<CatUnit> unitRepository;
        private readonly ICurrentUser currentUser;
        private readonly ICsTransactionDetailService transactionDetailService;
        private readonly ICurrencyExchangeService currencyExchangeService;

        public CsShipmentSurchargeService(IContextBase<CsShipmentSurcharge> repository, IMapper mapper, IStringLocalizer<LanguageSub> localizer,
            IContextBase<CsTransactionDetail> tranDetailRepo,
            IContextBase<CatPartner> partnerRepo,
            IContextBase<OpsTransaction> opsTransRepo,
            IContextBase<CatCurrencyExchange> currentExchangeRateRepo,
            IContextBase<CatCharge> catChargeRepo,
            IContextBase<CsTransaction> csTransactionRepo,
            IContextBase<CatUnit> unitRepo,
            ICurrentUser currUser,
            ICsTransactionDetailService transDetailService,
            ICurrencyExchangeService currencyExchange
            ) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            tranDetailRepository = tranDetailRepo;
            partnerRepository = partnerRepo;
            opsTransRepository = opsTransRepo;
            currentExchangeRateRepository = currentExchangeRateRepo;
            csTransactionRepository = csTransactionRepo;
            currentUser = currUser;
            catChargeRepository = catChargeRepo;
            transactionDetailService = transDetailService;
            currencyExchangeService = currencyExchange;
            unitRepository = unitRepo;
        }

        public HandleState DeleteCharge(Guid chargeId)
        {
            var hs = new HandleState();
            try
            {
                var charge = DataContext.Where(x => x.Id == chargeId).FirstOrDefault();
                if (charge == null)
                    hs = new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_SURCHARGE_NOT_FOUND].Value);
                if (charge != null && (charge.CreditNo != null || charge.Soano != null || charge.DebitNo != null || charge.PaySoano != null))
                {
                    hs = new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_SURCHARGE_NOT_ALLOW_DELETED].Value);
                }
                else
                {
                    DataContext.Delete(x => x.Id == chargeId);
                }
            }
            catch (Exception ex)
            {
                hs = new HandleState(ex.Message);
            }
            return hs;
        }

        public List<CatPartner> GetAllParner(Guid id, bool isHouseBillID = false)
        {
            try
            {
                List<CatPartner> listPartners = new List<CatPartner>();
                if (isHouseBillID == false)
                {
                    //var houseBills = tranDetailRepository.Get(x => x.JobId == id);
                    var csShipment = csTransactionRepository.Get(x => x.Id == id)?.FirstOrDefault();
                    var houseBillPermission = transactionDetailService.GetHouseBill(csShipment.TransactionType);
                    var houseBills = houseBillPermission.Where(x => x.JobId == id && x.ParentId == null);
                    foreach (var housebill in houseBills)
                    {
                        List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
                        listCharges = Query(housebill.Id, null);

                        foreach (var c in listCharges)
                        {
                            if (c.PaymentObjectId != null)
                            {
                                var partner = partnerRepository.Get(x => x.Id == c.PaymentObjectId).FirstOrDefault();
                                if (partner != null) listPartners.Add(partner);
                            }
                            if (c.PayerId != null)
                            {
                                var partner = partnerRepository.Get(x => x.Id == c.PayerId).FirstOrDefault();
                                if (partner != null) listPartners.Add(partner);
                            }
                        }
                    }
                }
                else
                {
                    var hblid = opsTransRepository.Get(x => x.Id == id).FirstOrDefault()?.Hblid;
                    //PermissionRange rangeSearch = PermissionEx.GetPermissionRange(currentUser.UserMenuPermission.List);
                    //var shipments = opsTransactionService.QueryByPermission(rangeSearch);
                    //var hblid = shipments.Where(x => x.Id == id).FirstOrDefault()?.Hblid;
                    List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();

                    listCharges = Query(hblid.Value, null);

                    foreach (var c in listCharges)
                    {
                        if (c.PaymentObjectId != null)
                        {
                            var partner = partnerRepository.Get(x => x.Id == c.PaymentObjectId).FirstOrDefault();
                            if (partner != null) listPartners.Add(partner);
                        }
                        if (c.PayerId != null)
                        {
                            var partner = partnerRepository.Get(x => x.Id == c.PayerId).FirstOrDefault();
                            if (partner != null) listPartners.Add(partner);
                        }
                    }
                }

                listPartners = listPartners.Distinct().ToList();
                return listPartners;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IQueryable<CsShipmentSurchargeDetailsModel> GetByHB(Guid hbID, string type)
        {
            var data = Query(hbID, type);
            if (data == null) return null;
            return data.OrderBy(x => x.DatetimeCreated).AsQueryable();
        }

        public List<GroupChargeModel> GroupChargeByHB(Guid id, string partnerId, bool isHouseBillID, string cdNoteCode)
        {
            List<GroupChargeModel> returnList = new List<GroupChargeModel>();
            List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
            if (isHouseBillID == false)
            {
                var houseBills = tranDetailRepository.Get(x => x.JobId == id);
                foreach (var houseBill in houseBills)
                {
                    listCharges = Query(houseBill.Id, null);
                    listCharges = listCharges.Where(x => ((x.PayerId == partnerId && x.Type == DocumentConstants.CHARGE_OBH_TYPE) || x.PaymentObjectId == partnerId)).ToList();
                    if (!string.IsNullOrEmpty(cdNoteCode))
                    {
                        listCharges = listCharges.Where(x => (x.CreditNo == cdNoteCode || x.DebitNo == cdNoteCode)).ToList();
                    }
                    else
                    {
                        listCharges = listCharges.Where(x =>
                            x.Type == DocumentConstants.CHARGE_OBH_TYPE ?
                            (string.IsNullOrEmpty(x.CreditNo) && !string.IsNullOrEmpty(x.DebitNo) ?
                                string.IsNullOrEmpty(x.CreditNo) && x.PayerId == partnerId
                                :
                                (
                                    string.IsNullOrEmpty(x.DebitNo) && !string.IsNullOrEmpty(x.CreditNo) ?
                                        string.IsNullOrEmpty(x.DebitNo) && x.PaymentObjectId == partnerId
                                     : string.IsNullOrEmpty(x.DebitNo) && string.IsNullOrEmpty(x.CreditNo)
                                )
                            )
                            : string.IsNullOrEmpty(x.CreditNo) && string.IsNullOrEmpty(x.DebitNo)
                        ).ToList();
                    }
                    listCharges.ForEach(fe =>
                    {
                        fe.Hwbno = houseBill.Hwbno;
                        if (fe.Type == DocumentConstants.CHARGE_OBH_TYPE && fe.PayerId == partnerId)
                        {
                            fe.IsSynced = !string.IsNullOrEmpty(fe.PaySyncedFrom) && (fe.PaySyncedFrom.Equals("CDNOTE") || fe.PaySyncedFrom.Equals("SOA") || fe.PaySyncedFrom.Equals("VOUCHER"));
                        }
                        else
                        {
                            fe.IsSynced = !string.IsNullOrEmpty(fe.SyncedFrom) && (fe.SyncedFrom.Equals("CDNOTE") || fe.SyncedFrom.Equals("SOA") || fe.SyncedFrom.Equals("VOUCHER"));
                        }
                    });

                    //Chỉ lấy những charge chưa sync
                    var _listCharges = listCharges.Where(x => x.IsSynced == false).ToList();
                    var returnObj = new GroupChargeModel { Hwbno = houseBill.Hwbno, Hbltype = houseBill.Hbltype, Id = houseBill.Id, listCharges = _listCharges, FlexId = houseBill.FlexId };
                    returnList.Add(returnObj);
                }
            }
            else
            {
                var houseBill = opsTransRepository.Get(x => x.Id == id).FirstOrDefault();
                listCharges = Query(houseBill.Hblid, null);
                listCharges = listCharges.Where(x => ((x.PayerId == partnerId && x.Type == DocumentConstants.CHARGE_OBH_TYPE) || x.PaymentObjectId == partnerId)).ToList();
                if (!string.IsNullOrEmpty(cdNoteCode))
                {
                    listCharges = listCharges.Where(x => (x.CreditNo == cdNoteCode || x.DebitNo == cdNoteCode)).ToList();
                }
                else
                {
                    listCharges = listCharges.Where(x =>
                            x.Type == DocumentConstants.CHARGE_OBH_TYPE ?
                            (string.IsNullOrEmpty(x.CreditNo) && !string.IsNullOrEmpty(x.DebitNo) ?
                                string.IsNullOrEmpty(x.CreditNo) && x.PayerId == partnerId
                                :
                                (
                                    string.IsNullOrEmpty(x.DebitNo) && !string.IsNullOrEmpty(x.CreditNo) ?
                                        string.IsNullOrEmpty(x.DebitNo) && x.PaymentObjectId == partnerId
                                     : string.IsNullOrEmpty(x.DebitNo) && string.IsNullOrEmpty(x.CreditNo)
                                )
                            )
                            : string.IsNullOrEmpty(x.CreditNo) && string.IsNullOrEmpty(x.DebitNo)
                    ).ToList();
                }
                listCharges.ForEach(fe =>
                {
                    fe.Hwbno = houseBill.Hwbno;
                    if (fe.Type == DocumentConstants.CHARGE_OBH_TYPE && fe.PayerId == partnerId)
                    {
                        fe.IsSynced = !string.IsNullOrEmpty(fe.PaySyncedFrom) && (fe.PaySyncedFrom.Equals("CDNOTE") || fe.PaySyncedFrom.Equals("SOA") || fe.PaySyncedFrom.Equals("VOUCHER"));
                    }
                    else
                    {
                        fe.IsSynced = !string.IsNullOrEmpty(fe.SyncedFrom) && (fe.SyncedFrom.Equals("CDNOTE") || fe.SyncedFrom.Equals("SOA") || fe.SyncedFrom.Equals("VOUCHER"));
                    }
                });

                //Chỉ lấy những charge chưa sync
                var _listCharges = listCharges.Where(x => x.IsSynced == false).ToList();
                var returnObj = new GroupChargeModel { Hwbno = houseBill.Hwbno, Id = houseBill.Id, listCharges = _listCharges, FlexId = null };
                returnList.Add(returnObj);
            }

            return returnList;

        }

        public ChargeShipmentResult GetListChargeShipment(ChargeShipmentCriteria criteria)
        {
            var chargeShipmentList = GetSpcChargeShipment(criteria).ToList();
            var dataMap = mapper.Map<List<spc_GetListChargeShipmentMaster>, List<ChargeShipmentModel>>(chargeShipmentList);
            var result = new ChargeShipmentResult
            {
                ChargeShipments = dataMap,
                TotalShipment = chargeShipmentList.Where(x => x.HBL != null).GroupBy(x => x.HBL).Count(),
                TotalCharge = chargeShipmentList.Count(),
                AmountDebitLocal = chargeShipmentList.Sum(x => x.AmountDebitLocal),
                AmountCreditLocal = chargeShipmentList.Sum(x => x.AmountCreditLocal),
                AmountDebitUSD = chargeShipmentList.Sum(x => x.AmountDebitUSD),
                AmountCreditUSD = chargeShipmentList.Sum(x => x.AmountCreditUSD),
            };
            return result;
        }
        List<CsShipmentSurchargeDetailsModel> Query(Guid hbId, string type)
        {
            if (type == null) type = string.Empty;
            List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
            var query = GetChargeByHouseBill(hbId, type, string.Empty);
            if (query.Count == 0) return listCharges;
            foreach (var item in query)
            {
                var charge = mapper.Map<CsShipmentSurchargeDetailsModel>(item);
                charge.Currency = item.CurrencyCode;
                charge.Unit = item.UnitNameEn;
                charge.NameEn = item.ChargeNameEn;
                charge.UnitCode = item.UnitCode;
                charge.ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.CurrencyId, DocumentConstants.CURRENCY_LOCAL);//item.RateToLocal;
                if (charge.Type == DocumentConstants.CHARGE_BUY_TYPE)
                {
                    charge.DebitCharge = catChargeRepository.Get(c => c.Id == charge.ChargeId).FirstOrDefault()?.DebitCharge;
                }
                listCharges.Add(charge);
            }
            return listCharges;
        }
        private List<spc_GetSurchargeByHouseBill> GetChargeByHouseBill(Guid id, string type, string currencyLocal)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@hblid", Value = id.ToString() },
                new SqlParameter(){ ParameterName = "@type", Value = type },
                new SqlParameter(){ ParameterName = "@currencyLocal", Value = currencyLocal }
            };
            var list = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<spc_GetSurchargeByHouseBill>(parameters);
            return list;
        }
        private List<spc_GetListChargeShipmentMaster> GetSpcChargeShipment(ChargeShipmentCriteria criteria)
        {
            DbParameter[] parameters =
            {
                SqlParam.GetParameter("currencyLocal", criteria.CurrencyLocal),
                SqlParam.GetParameter("customerID", criteria.CustomerID),
                SqlParam.GetParameter("dateType", criteria.DateType),
                SqlParam.GetParameter("fromDate", criteria.FromDate),
                SqlParam.GetParameter("toDate", criteria.ToDate),
                SqlParam.GetParameter("type", criteria.Type),
                SqlParam.GetParameter("isOBH", criteria.IsOBH),
                SqlParam.GetParameter("strCreators", criteria.StrCreators),
                SqlParam.GetParameter("strCharges", criteria.StrCharges),
                SqlParam.GetParameter("commodityGroupID", criteria.CommodityGroupID),
                SqlParam.GetParameter("strServices", criteria.StrServices)
            };
            return ((eFMSDataContext)DataContext.DC).ExecuteProcedure<spc_GetListChargeShipmentMaster>(parameters);
        }

        public List<CsShipmentSurchargeDetailsModel> GetByHB(Guid hblid)
        {
            List<CsShipmentSurchargeDetailsModel> results = new List<CsShipmentSurchargeDetailsModel>();
            var data = GetChargeByHouseBill(hblid, string.Empty, null);
            if (data.Count == 0) return results;
            foreach (var item in data)
            {
                var charge = mapper.Map<CsShipmentSurchargeDetailsModel>(item);
                charge.Unit = item.UnitNameEn;
                charge.Currency = item.CurrencyCode;
                charge.ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.CurrencyId, DocumentConstants.CURRENCY_LOCAL); //item.RateToLocal;
                results.Add(charge);
            }
            return results;
        }

        public HousbillProfit GetHouseBillTotalProfit(Guid hblid)
        {
            HousbillProfit result = new HousbillProfit
            {
                HBLID = hblid,
                HouseBillTotalCharge = new HouseBillTotalCharge()
            };
            List<spc_GetSurchargeByHouseBill> surcharges = GetChargeByHouseBill(hblid, string.Empty, null);
            if (!surcharges.Any()) return result;
            foreach (var item in surcharges)
            {
                //decimal _rateToLocal = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.CurrencyId, DocumentConstants.CURRENCY_LOCAL);
                //decimal _rateToUSD = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.CurrencyId, DocumentConstants.CURRENCY_USD);

                //decimal totalLocal = item.Quantity * (item.UnitPrice ?? 0) * _rateToLocal; // without vat - 15305
                //decimal totalUSD = item.Quantity * (item.UnitPrice ?? 0) * _rateToUSD; // without vat - 15305

                if (item.Type == DocumentConstants.CHARGE_BUY_TYPE)
                {
                    //result.HouseBillTotalCharge.TotalBuyingLocal = result.HouseBillTotalCharge.TotalBuyingLocal + totalLocal;
                    //result.HouseBillTotalCharge.TotalBuyingUSD = result.HouseBillTotalCharge.TotalBuyingUSD + totalUSD;

                    result.HouseBillTotalCharge.TotalBuyingLocal += item.AmountVnd ?? 0;
                    result.HouseBillTotalCharge.TotalBuyingUSD += item.AmountUsd ?? 0;
                }
                else if (item.Type == DocumentConstants.CHARGE_SELL_TYPE)
                {
                    //result.HouseBillTotalCharge.TotalSellingLocal = result.HouseBillTotalCharge.TotalSellingLocal + totalLocal;
                    //result.HouseBillTotalCharge.TotalSellingUSD = result.HouseBillTotalCharge.TotalSellingUSD + totalUSD;

                    result.HouseBillTotalCharge.TotalSellingLocal += item.AmountVnd ?? 0;
                    result.HouseBillTotalCharge.TotalSellingUSD += item.AmountUsd ?? 0;
                }
                else
                {
                    //result.HouseBillTotalCharge.TotalOBHLocal = result.HouseBillTotalCharge.TotalOBHLocal + totalLocal;
                    //result.HouseBillTotalCharge.TotalOBHUSD = result.HouseBillTotalCharge.TotalOBHUSD + totalUSD;

                    result.HouseBillTotalCharge.TotalOBHLocal += item.AmountVnd ?? 0;
                    result.HouseBillTotalCharge.TotalOBHUSD += item.AmountUsd ?? 0;
                }
            }

            result.ProfitLocal = result.HouseBillTotalCharge.TotalSellingLocal - result.HouseBillTotalCharge.TotalBuyingLocal;
            result.ProfitUSD = result.HouseBillTotalCharge.TotalSellingUSD - result.HouseBillTotalCharge.TotalBuyingUSD;

            return result;
        }

        public HandleState DeleteMultiple(List<Guid> listId)
        {
            var hs = new HandleState();
            try
            {
                foreach (var item in listId)
                {
                    var charge = DataContext.Where(x => x.Id == item).FirstOrDefault();
                    if (charge == null)
                    {
                        hs = new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_SURCHARGE_NOT_FOUND].Value);
                        return hs;
                    }
                    if (charge != null && (charge.CreditNo != null || charge.Soano != null || charge.DebitNo != null || charge.PaySoano != null))
                    {
                        hs = new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_SURCHARGE_NOT_ALLOW_DELETED].Value);
                        return hs;
                    }
                    hs = DataContext.Delete(x => x.Id == item, false);
                    if (hs.Success == false)
                    {
                        return hs;
                    }
                }
                DataContext.SubmitChanges();
            }
            catch (Exception ex)
            {
                hs = new HandleState(ex.Message);
            }
            return hs;
        }

        public HandleState AddAndUpdate(List<CsShipmentSurchargeModel> list)
        {
            var result = new HandleState();
            var surcharges = mapper.Map<List<CsShipmentSurcharge>>(list);

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in surcharges)
                    {
                        item.Notes = string.IsNullOrEmpty(item.Notes?.Trim()) ? null : item.Notes;
                        item.InvoiceNo = string.IsNullOrEmpty(item.InvoiceNo?.Trim()) ? null : item.InvoiceNo;
                        item.SeriesNo = string.IsNullOrEmpty(item.SeriesNo?.Trim()) ? null : item.SeriesNo;
                        item.CreditNo = string.IsNullOrEmpty(item.CreditNo?.Trim()) ? null : item.CreditNo;
                        item.DebitNo = string.IsNullOrEmpty(item.DebitNo?.Trim()) ? null : item.DebitNo;
                        item.Soano = string.IsNullOrEmpty(item.Soano?.Trim()) ? null : item.Soano;
                        item.PaySoano = string.IsNullOrEmpty(item.PaySoano?.Trim()) ? null : item.PaySoano;

                        if (item.Id == Guid.Empty)
                        {
                            //** FinalExchangeRate = null do cần tính lại dựa vào ExchangeDate mới
                            item.FinalExchangeRate = null;
                            var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(item);
                            item.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                            item.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                            item.FinalExchangeRate = amountSurcharge.FinalExchangeRate; //Tỉ giá so với Local
                            item.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                            item.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                            item.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                            item.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)

                            item.DatetimeCreated = item.DatetimeModified = DateTime.Now;
                            item.UserCreated = currentUser.UserID;
                            item.Id = Guid.NewGuid();
                            //item.ExchangeDate = DateTime.Now; ??? - Rule lạ

                            item.TransactionType = GetTransactionType(item.JobNo);
                            if (item.Hblid != Guid.Empty)
                            {
                                if (item.TransactionType != "CL")
                                {
                                    CsTransactionDetail hbl = tranDetailRepository.Get(x => x.Id == item.Hblid)?.FirstOrDefault();
                                    item.OfficeId = hbl?.OfficeId ?? Guid.Empty;
                                    item.CompanyId = hbl?.CompanyId ?? Guid.Empty;
                                }
                                if (item.TransactionType == "CL")
                                {
                                    OpsTransaction hbl = opsTransRepository.Get(x => x.Hblid == item.Hblid).FirstOrDefault();
                                    item.OfficeId = hbl?.OfficeId ?? Guid.Empty;
                                    item.CompanyId = hbl?.CompanyId ?? Guid.Empty;
                                }
                            }
                            var t = DataContext.Add(item, true);
                        }
                        else
                        {
                            string _jobNo = string.Empty;
                            string _mblNo = string.Empty;
                            string _hblNo = string.Empty;
                            if (item.TransactionType != "CL")
                            {
                                var houseBill = tranDetailRepository.Get(x => x.Id == item.Hblid)?.FirstOrDefault();
                                _hblNo = houseBill?.Hwbno;
                                if (houseBill != null)
                                {
                                    var masterBill = csTransactionRepository.Get(x => x.Id == houseBill.JobId).FirstOrDefault();
                                    _jobNo = masterBill?.JobNo;
                                    _mblNo = !string.IsNullOrEmpty(masterBill?.Mawb) ? masterBill?.Mawb : houseBill.Mawb;
                                }
                            }
                            else
                            {
                                var masterBill = opsTransRepository.Get(x => x.Hblid == item.Hblid).FirstOrDefault();
                                _jobNo = masterBill?.JobNo;
                                _mblNo = masterBill?.Mblno;
                                _hblNo = masterBill?.Hwbno;
                            }
                            var surcharge = DataContext.Get(x => x.Id == item.Id).FirstOrDefault();
                            if (surcharge != null)
                            {
                                surcharge.PaymentObjectId = item.PaymentObjectId;
                                surcharge.PayerId = item.PayerId;
                                surcharge.ChargeId = item.ChargeId;
                                surcharge.ChargeGroup = item.ChargeGroup;
                                surcharge.Quantity = item.Quantity;
                                surcharge.UnitId = item.UnitId;
                                surcharge.UnitPrice = item.UnitPrice;
                                surcharge.Vatrate = item.Vatrate;
                                surcharge.CurrencyId = item.CurrencyId;
                                surcharge.Notes = item.Notes;
                                surcharge.KickBack = item.KickBack;
                                surcharge.QuantityType = item.QuantityType;

                                //Chỉ tính lại giá trị cho các charge chưa issue Settlement, Voucher, Invoice, SOA, CDNote
                                if (string.IsNullOrEmpty(surcharge.SettlementCode)
                                    && (surcharge.AcctManagementId == Guid.Empty || surcharge.AcctManagementId == null)
                                    && (string.IsNullOrEmpty(surcharge.Soano) && string.IsNullOrEmpty(surcharge.PaySoano))
                                    && (string.IsNullOrEmpty(surcharge.DebitNo) && string.IsNullOrEmpty(surcharge.CreditNo)))
                                {
                                    //** FinalExchangeRate = null do cần tính lại dựa vào ExchangeDate mới
                                    surcharge.FinalExchangeRate = null;
                                    surcharge.ExchangeDate = item.ExchangeDate;
                                }

                                var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(surcharge);
                                surcharge.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                                surcharge.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                                surcharge.FinalExchangeRate = amountSurcharge.FinalExchangeRate; //Tỉ giá so với Local
                                surcharge.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                                surcharge.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                                surcharge.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                                surcharge.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)

                                if (item.Type == DocumentConstants.CHARGE_BUY_TYPE)
                                {
                                    surcharge.InvoiceNo = item.InvoiceNo;
                                    surcharge.InvoiceDate = item.InvoiceDate;
                                    surcharge.SeriesNo = item.SeriesNo;
                                }

                                surcharge.JobNo = _jobNo;
                                surcharge.Mblno = _mblNo;
                                surcharge.Hblno = _hblNo;
                                surcharge.DatetimeModified = DateTime.Now;
                                surcharge.UserModified = currentUser.UserID;
                                var d = DataContext.Update(surcharge, x => x.Id == surcharge.Id, true);
                            }
                        }
                    }
                    DataContext.SubmitChanges();
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    result = new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
                return result;
            }
        }

        public HandleState UpdateFieldNetAmount_AmountUSD_VatAmountUSD()
        {
            var result = new HandleState();
            var surcharges = DataContext.Get(x => (x.AmountVnd == null && x.VatAmountVnd == null) && x.NetAmount == null).Take(500);
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in surcharges)
                    {
                        var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(item);
                        item.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                        item.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                        item.FinalExchangeRate = item.FinalExchangeRate == null ? amountSurcharge.FinalExchangeRate : item.FinalExchangeRate; //Tỉ giá so với Local
                        item.AmountVnd = item.AmountVnd == null ? amountSurcharge.AmountVnd : item.AmountVnd; //Thành tiền trước thuế (Local)
                        item.VatAmountVnd = item.VatAmountVnd == null ? amountSurcharge.VatAmountVnd : item.VatAmountVnd; //Tiền thuế (Local)
                        item.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                        item.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)

                        var d = DataContext.Update(item, x => x.Id == item.Id, false);
                    }
                    DataContext.SubmitChanges();
                    trans.Commit();
                    return new HandleState();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    result = new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
                return result;
            }
        }

        public List<HousbillProfit> GetShipmentTotalProfit(Guid jobId)
        {
            List<HousbillProfit> results = new List<HousbillProfit>();
            IQueryable<OpsTransaction> opsShipments = null;
            CsTransaction csShipment = null;
            IQueryable<HousbillProfit> hblids = null;

            opsShipments = opsTransRepository.Get(x => x.Id == jobId);

            if (opsShipments.Count() == 0)
            {
                csShipment = csTransactionRepository.Get(x => x.Id == jobId)?.FirstOrDefault();
                if (csShipment != null)
                {
                    IQueryable<CsTransactionDetail> houseBills = transactionDetailService.GetHouseBill(csShipment.TransactionType);
                    //hblids = tranDetailRepository.Get(x => x.JobId == csShipment.Id).Select(x => 
                    //                new HousbillProfit { HBLID = x.Id, HBLNo = x.Hwbno });
                    hblids = houseBills.Where(x => x.JobId == csShipment.Id && x.ParentId == null).Select(x =>
                                    new HousbillProfit { HBLID = x.Id, HBLNo = x.Hwbno });
                }
            }
            else
            {
                hblids = opsShipments.Select(x => new HousbillProfit { HBLID = x.Hblid, HBLNo = x.Hwbno });
            }
            if (hblids.Count() == 0) return results;
            foreach (HousbillProfit item in hblids)
            {
                HousbillProfit profit = GetHouseBillTotalProfit(item.HBLID);
                profit.HBLNo = item.HBLNo;
                results.Add(profit);
            }
            return results;
        }

        public IQueryable<CsShipmentSurchargeDetailsModel> GetRecentlyCharges(RecentlyChargeCriteria criteria)
        {
            // get charge info of newest shipment by charge type of an PIC and not existed in current shipment and by criteria: POL, POD, Customer, Shipping Line, Consignee
            string transactionType = DataTypeEx.GetType(criteria.TransactionType);

            Expression<Func<CsTransaction, bool>> queryShipmentNearest = x => (x.OfficeId == currentUser.OfficeID
                                                        && (x.AgentId == criteria.AgentId || string.IsNullOrEmpty(criteria.AgentId))
                                                        && (x.ColoaderId == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                                                        && x.TransactionType == transactionType);


            if (queryShipmentNearest == null) return null;
            List<Guid> houseIds = new List<Guid>();

            if (criteria.ChargeType == DocumentConstants.CHARGE_BUY_TYPE)
            {
                if (criteria.ColoaderId == null) return null;
                queryShipmentNearest = queryShipmentNearest.And(x => x.Id != criteria.JobId); // kHác với lô hiện tại

                CsTransaction shipment = csTransactionRepository.Get(queryShipmentNearest)?.OrderByDescending(x => x.DatetimeCreated).FirstOrDefault();
                if (shipment == null) return null;

                houseIds = tranDetailRepository.Get(x => x.JobId == shipment.Id && x.Id != criteria.HblId).Select(x => x.Id).ToList();
            }
            else
            {
                if (criteria.CustomerId == null) return null;
                queryShipmentNearest = queryShipmentNearest.And(x => x.Id != criteria.JobId);
                IQueryable<CsTransaction> shipmentQuery = csTransactionRepository.Get(queryShipmentNearest)?.OrderByDescending(x => x.DatetimeCreated).Take(1);
                if (shipmentQuery == null) return null;

                // Chỉ lấy house
                foreach (var shipment in shipmentQuery)
                {
                    var houseId = tranDetailRepository.Get(x => x.JobId == shipment.Id && x.CustomerId == criteria.CustomerId && x.Id != criteria.HblId).Select(x => x.Id).ToList();
                    houseIds.AddRange(houseId);
                }
            }

            if (houseIds.Count == 0) return null;
            IQueryable<CsShipmentSurcharge> csShipmentSurcharge = DataContext.Get(x => houseIds.Contains(x.Hblid) && x.Type == criteria.ChargeType && x.IsFromShipment == true);
            if (csShipmentSurcharge == null) return null;

            IQueryable<CsShipmentSurchargeDetailsModel> result = (
                from surcharge in csShipmentSurcharge
                join charge in catChargeRepository.Get() on surcharge.ChargeId equals charge.Id
                join p in partnerRepository.Get() on surcharge.PaymentObjectId equals p.Id into gp
                from p1 in gp.DefaultIfEmpty()
                join payer in partnerRepository.Get() on surcharge.PayerId equals payer.Id into gp2
                from p2 in gp2.DefaultIfEmpty()
                select new CsShipmentSurchargeDetailsModel
                {
                    Type = surcharge.Type,
                    ChargeId = surcharge.ChargeId,
                    Quantity = surcharge.Quantity,
                    QuantityType = surcharge.QuantityType,
                    UnitId = surcharge.UnitId,
                    UnitPrice = surcharge.UnitPrice,
                    CurrencyId = surcharge.CurrencyId,
                    IncludedVat = surcharge.IncludedVat,
                    Vatrate = surcharge.Vatrate,
                    Total = surcharge.Total,
                    PayerId = surcharge.PayerId,
                    ObjectBePaid = surcharge.ObjectBePaid,
                    PaymentObjectId = surcharge.PaymentObjectId,
                    ExchangeDate = surcharge.ExchangeDate,
                    Notes = surcharge.Notes,
                    IsFromShipment = true,
                    TypeOfFee = surcharge.TypeOfFee,
                    KickBack = surcharge.KickBack,

                    PartnerShortName = p1.ShortName,
                    PartnerName = p1.PartnerNameEn,
                    ReceiverShortName = p1.ShortName,
                    ReceiverName = p1.PartnerNameEn,
                    PayerShortName = p2.ShortName,
                    PayerName = p2.PartnerNameEn,

                    ChargeNameEn = charge.ChargeNameEn,
                    ChargeCode = charge.Code,
                });
            return result;
        }

        public List<CsShipmentSurchargeImportModel> CheckValidImport(List<CsShipmentSurchargeImportModel> list)
        {
            var listChargeOps = DataContext.Get(x => x.TransactionType == "CL");
            var listPartner = partnerRepository.Get(x => x.Active == true);
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.Hblno))
                {
                    item.HBLNoError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_HBLNO_EMPTY]);
                    item.IsValid = false;
                }
                else
                {
                    if (!opsTransRepository.Any(x => x.Hwbno == item.Hblno.Trim()))
                    {
                        item.HBLNoError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_HBLNO_NOT_EXIST], item.Hblno);
                        item.IsValid = false;
                    }

                }
                if (string.IsNullOrEmpty(item.Mblno))
                {
                    item.MBLNoError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_MBLNO_EMPTY]);
                    item.IsValid = false;
                }
                else
                {
                    if (!opsTransRepository.Any(x => x.Mblno == item.Mblno.Trim()))
                    {
                        item.MBLNoError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_MBLNO_NOT_EXIST], item.Mblno);
                        item.IsValid = false;
                    }
                }
                if (string.IsNullOrEmpty(item.PartnerCode))
                {
                    item.PartnerCodeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_PARTNER_CODE_EMPTY]);
                    item.IsValid = false;
                }
                else
                {
                    if (!listPartner.Any(x => x.AccountNo.Trim() == item.PartnerCode.Replace("'", "").Trim()))
                    {
                        item.PartnerCodeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_PARTER_CODE_NOT_EXIST], item.PartnerCode);
                        item.IsValid = false;
                    }
                }
                if (string.IsNullOrEmpty(item.ChargeCode))
                {
                    item.ChargeCodeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_CHARGE_CODE_EMPTY]);
                    item.IsValid = false;
                }
                else
                {
                    if (!catChargeRepository.Any(x => x.Code == item.ChargeCode.Trim()))
                    {
                        item.ChargeCodeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_CHARGE_CODE_NOT_EXIST], item.ChargeCode);
                        item.IsValid = false;
                    }
                }

                if (!item.Qty.HasValue)
                {
                    item.QtyError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_QTY_EMPTY]);
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.Unit))
                {
                    item.UnitError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_UNIT_EMPTY]);
                    item.IsValid = false;
                }
                else
                {
                    if (!unitRepository.Any(x => x.UnitNameEn.Trim() == item.Unit.Trim()))
                    {
                        item.UnitError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_UNIT_NOT_EXIST], item.Unit);
                        item.IsValid = false;
                    }
                }
                if (!item.UnitPrice.HasValue)
                {
                    item.UnitPriceError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_UNIT_PRICE_EMPTY]);
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CurrencyId))
                {
                    item.CurrencyError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_CURRENCY_EMPTY]);
                    item.IsValid = false;
                }
                if (!item.Vatrate.HasValue)
                {
                    item.VatError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_VAT_EMPTY]);
                    item.IsValid = false;
                }
                if (!item.TotalAmount.HasValue)
                {
                    item.TotalAmountError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_TOTAL_AMOUNT_EMPTY]);
                    item.IsValid = false;
                }
                if (!item.ExchangeDate.HasValue)
                {
                    item.ExchangeDateError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_EXCHANGE_DATE_EMPTY]);
                    item.IsValid = false;
                }

                if (!item.FinalExchangeRate.HasValue)
                {
                    item.FinalExchangeRateError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_FINAL_EXCHANGE_EMPTY]);
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.Type))
                {
                    item.TypeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_TYPE_EMPTY]);
                    item.IsValid = false;
                }
                else
                {
                    if (item.Type.ToLower() != "buying" && item.Type.ToLower() != "sell" && item.Type.ToLower() != "obh")
                    {
                        item.TypeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_TYPE_NOT_VALID], item.Type);
                        item.IsValid = false;
                    }
                }
                if (!string.IsNullOrEmpty(item.Hblno))
                {

                    if (!opsTransRepository.Any(x => x.Mblno == item.Mblno && x.Hwbno == item.Hblno))
                    {
                        item.HBLNoError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_HBLNO_NOT_EXIST], item.Hblno);
                        item.IsValid = false;

                    }
                }
                if (!string.IsNullOrEmpty(item.Mblno))
                {
                    if (!opsTransRepository.Any(x => x.Mblno.Trim() == item.Mblno.Trim()))
                    {
                        item.MBLNoError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_MBLNO_NOT_EXIST], item.Mblno);
                        item.IsValid = false;
                    }
                }
                if (item.IsValid)
                {
                    string PartnerId = listPartner.Where(x => x.AccountNo.Trim() == item.PartnerCode.Trim()).Select(t => t.Id).FirstOrDefault();
                    Guid ChargeId = catChargeRepository.Get(x => x.Code == item.ChargeCode).Select(t => t.Id).FirstOrDefault();
                    item.ChargeId = ChargeId;
                    short UnitId = unitRepository.Get(x => x.UnitNameEn == item.Unit.Trim()).Select(t => t.Id).FirstOrDefault();
                    item.UnitId = UnitId;
                    item.PaymentObjectId = PartnerId;
                    Guid HblId = opsTransRepository.Get(x => x.Hwbno == item.Hblno.Trim()).Select(t => t.Hblid).FirstOrDefault();
                    item.Hblid = HblId;

                    #region --Tính giá trị các field: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
                    var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(item);
                    item.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                    item.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                    item.FinalExchangeRate = amountSurcharge.FinalExchangeRate; //Tỉ giá so với Local
                    item.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                    item.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                    item.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                    item.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)
                    #endregion --Tính giá trị các field: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --

                    item.Quantity = (decimal)item.Qty;
                    item.TransactionType = "CL";
                    string jobNo = opsTransRepository.Get(x => x.Hwbno == item.Hblno.Trim() && x.Mblno == item.Mblno.Trim()).Select(t => t.JobNo).FirstOrDefault();
                    if (item.Type.ToLower() == "obh")
                    {
                        item.PayerId = PartnerId;
                    }
                    if (item.Type.ToLower() == "buying")
                    {
                        item.Type = "BUY";
                    }
                    if (listChargeOps.Any(x => x.Mblno.Trim() == item.Mblno.Trim() && x.Hblno.Trim() == item.Hblno.Trim() && x.PaymentObjectId == PartnerId && x.ChargeId == ChargeId && x.Type == item.Type))
                    {
                        item.ChargeCodeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_CHARGE_CODE_DUPLICATE], item.ChargeCode, jobNo);
                        item.IsValid = false;
                    }
                    if (!string.IsNullOrEmpty(item.SeriesNo))
                    {
                        if (listChargeOps.Any(x => x.Mblno.Trim() == item.Mblno.Trim() && x.Hblno.Trim() == item.Hblno.Trim() && x.PaymentObjectId == PartnerId && x.ChargeId == ChargeId && x.SeriesNo == item.SeriesNo && x.Type == item.Type))
                        {
                            item.ChargeCodeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_CHARGE_CODE_DUPLICATE], item.ChargeCode, jobNo);
                            item.IsValid = false;
                        }
                    }
                    if (!string.IsNullOrEmpty(item.InvoiceNo))
                    {
                        if (listChargeOps.Any(x => x.Mblno.Trim() == item.Mblno.Trim() && x.Hblno.Trim() == item.Hblno.Trim() && x.PaymentObjectId == PartnerId && x.ChargeId == ChargeId && x.InvoiceNo == item.InvoiceNo && x.Type == item.Type))
                        {
                            item.ChargeCodeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_CHARGE_CODE_DUPLICATE], item.ChargeCode, jobNo);
                            item.IsValid = false;
                        }
                    }
                    if (!string.IsNullOrEmpty(item.SeriesNo) && !string.IsNullOrEmpty(item.InvoiceNo))
                    {
                        if (listChargeOps.Any(x => x.Mblno.Trim() == item.Mblno.Trim() && x.Hblno.Trim() == item.Hblno.Trim() && x.PaymentObjectId == PartnerId && x.ChargeId == ChargeId && x.InvoiceNo == item.InvoiceNo && x.SeriesNo == item.SeriesNo && x.Type == item.Type))
                        {
                            item.ChargeCodeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_CHARGE_CODE_DUPLICATE], item.ChargeCode, jobNo);
                            item.IsValid = false;
                        }
                    }
                }

            });
            return list;
        }

        public HandleState Import(List<CsShipmentSurchargeImportModel> list)
        {
            foreach (var item in list)
            {
                switch (item.Type.ToLower())
                {
                    case "buying":
                        item.Type = "BUY";
                        break;
                    case "obh":
                        item.Type = item.Type.ToUpper();
                        break;
                    case "sell":
                        item.Type = item.Type.ToUpper();
                        break;
                }
                item.UserCreated = currentUser.UserID;
                item.Id = Guid.NewGuid();
                item.ExchangeDate = DateTime.Now;
                item.DatetimeCreated = DateTime.Now;
                OpsTransaction hbl = opsTransRepository.Get(x => x.Hblid == item.Hblid).FirstOrDefault();
                item.OfficeId = hbl?.OfficeId ?? Guid.Empty;
                item.CompanyId = hbl?.CompanyId ?? Guid.Empty;
            }
            var datas = mapper.Map<List<CsShipmentSurcharge>>(list);
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Add(datas);
                    if (hs.Success)
                    {
                        trans.Commit();
                    }
                    else
                    {
                        trans.Rollback();
                    }
                    return new HandleState();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    Get();
                    trans.Dispose();
                }
            }
        }

        private string GetTransactionType(string jobNo)
        {
            string transactionType = null;
            if (!string.IsNullOrEmpty(jobNo))
            {
                IQueryable<CsTransaction> docTransaction = csTransactionRepository.Get(x => x.JobNo == jobNo);
                if (docTransaction != null && docTransaction.Count() > 0)
                {
                    transactionType = docTransaction?.FirstOrDefault()?.TransactionType;
                }
                else
                {
                    IQueryable<OpsTransaction> opsTransaction = opsTransRepository.Get(x => x.JobNo == jobNo);
                    if (opsTransaction != null && opsTransaction.Count() > 0)
                    {
                        transactionType = "CL";
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                return transactionType;
            }
            return string.Empty;

        }
    }
}

