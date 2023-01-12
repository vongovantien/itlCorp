﻿using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
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
        private readonly IContextBase<SysSettingFlow> settingFlowRepository;
        private readonly IContextBase<CatContract> catContractRepository;
        private readonly IContextBase<CatDepartment> catDepartmentRepository;
        private readonly IContextBase<SysUserLevel> userlevelRepository;
        private readonly IContextBase<SysNotifications> notificationRepository;
        private readonly IContextBase<SysUserNotification> sysUserNotifyRepository;
        private readonly IContextBase<AccAccountReceivable> accAccountReceivableRepository;
        private readonly IContextBase<CatUnit> unitRepository;
        private readonly ICurrentUser currentUser;
        private readonly ICsTransactionDetailService transactionDetailService;
        private readonly ICurrencyExchangeService currencyExchangeService;
        private readonly IContextBase<CustomsDeclaration> customsDeclarationRepository;
        private readonly IContextBase<CatChargeGroup> catChargeGroupRepository;
        private readonly IContextBase<CatCurrency> currencyRepository;
        private readonly IContextBase<CsRuleLinkFee> csRuleLinkFeeRepository;

        private readonly IContextBase<AcctApproveSettlement> acctApproveSettlementRepository;

        public CsShipmentSurchargeService(IContextBase<CsShipmentSurcharge> repository, IMapper mapper, IStringLocalizer<LanguageSub> localizer,
            IContextBase<CsTransactionDetail> tranDetailRepo,
            IContextBase<CatPartner> partnerRepo,
            IContextBase<OpsTransaction> opsTransRepo,
            IContextBase<CatCurrencyExchange> currentExchangeRateRepo,
            IContextBase<CatCharge> catChargeRepo,
            IContextBase<CsTransaction> csTransactionRepo,
            IContextBase<SysSettingFlow> sysSettingFlowRepo,
            IContextBase<CatContract> catContractRepo,
            IContextBase<CatDepartment> catDepartmentRepo,
            IContextBase<SysUserLevel> userLevelRepo,
            IContextBase<SysNotifications> notificationRepo,
            IContextBase<SysUserNotification> sysUserNotifyRepo,
            IContextBase<AccAccountReceivable> accAccountRepo,
            IContextBase<CatUnit> unitRepo,
            ICurrentUser currUser,
            ICsTransactionDetailService transDetailService,
            ICurrencyExchangeService currencyExchange,
            IContextBase<CustomsDeclaration> customsDeclarationRepo,
            IContextBase<CatCurrency> currencyRepo,
            IContextBase<AcctApproveSettlement> acctApproveSettlementRepo,
            IContextBase<CatChargeGroup> catChargeGroupRepo,
            IContextBase<CsRuleLinkFee> csRuleLinkFeeRepo
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
            settingFlowRepository = sysSettingFlowRepo;
            catContractRepository = catContractRepo;
            catDepartmentRepository = catDepartmentRepo;
            userlevelRepository = userLevelRepo;
            notificationRepository = notificationRepo;
            sysUserNotifyRepository = sysUserNotifyRepo;
            accAccountReceivableRepository = accAccountRepo;
            unitRepository = unitRepo;
            customsDeclarationRepository = customsDeclarationRepo;
            catChargeGroupRepository = catChargeGroupRepo;
            currencyRepository = currencyRepo;
            acctApproveSettlementRepository = acctApproveSettlementRepo;
            csRuleLinkFeeRepository = csRuleLinkFeeRepo;
        }

        public HandleState DeleteCharge(Guid chargeId)
        {
            var hs = new HandleState();
            try
            {
                var charge = DataContext.Where(x => x.Id == chargeId).FirstOrDefault();
                if (charge == null)
                    hs = new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_SURCHARGE_NOT_FOUND].Value);
                if (charge != null
                    && (!string.IsNullOrEmpty(charge.Soano)
                    || !string.IsNullOrEmpty(charge.PaySoano)
                    || !string.IsNullOrEmpty(charge.CreditNo)
                    || !string.IsNullOrEmpty(charge.DebitNo)
                    || !string.IsNullOrEmpty(charge.SettlementCode)
                    || !string.IsNullOrEmpty(charge.VoucherId)
                    || !string.IsNullOrEmpty(charge.VoucherIdre)
                    || charge.AcctManagementId != null
                    || charge.PayerAcctManagementId != null))
                {
                    hs = new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_SURCHARGE_NOT_ALLOW_DELETED].Value);
                }
                else
                {
                    var mesageDelete = CheckDeleteChargeShipmentNoProfit(charge); //Check is valid delete charge Buy/Sell in shipment with no profit
                    if (!string.IsNullOrEmpty(mesageDelete))
                    {
                        return new HandleState((object)mesageDelete);
                    }

                    DataContext.Delete(x => x.Id == chargeId);
                }
            }
            catch (Exception ex)
            {
                hs = new HandleState(ex.Message);
            }
            return hs;
        }

        /// <summary>
        /// Check is valid delete charge Buy/Sell in shipment with no profit
        /// </summary>
        /// <param name="charge"></param>
        /// <returns></returns>
        private string CheckDeleteChargeShipmentNoProfit(CsShipmentSurcharge charge)
        {
            if (charge.Type == DocumentConstants.CHARGE_OBH_TYPE)
            {
                return string.Empty;
            }
            var hblId = charge.Hblid;
            var opsTrans = opsTransRepository.Get(x => x.Hblid == hblId).FirstOrDefault();
            if (opsTrans != null)
            {
                if (opsTrans.NoProfit != true)
                {
                    return string.Empty;
                }
                var surcharges = DataContext.Get(x => x.Hblid == opsTrans.Hblid && x.Type != DocumentConstants.CHARGE_OBH_TYPE && x.Id != charge.Id);
                var totalBuying = surcharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE).Sum(x => x.AmountVnd ?? 0);
                var totalSelling = surcharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE).Sum(x => x.AmountVnd ?? 0);
                if (totalSelling - totalBuying > 0)
                {
                    return "You can not delete this charge because Profit of Shipment NoProfit is bigger than 0";
                }
            }
            else
            {
                var hblDetail = tranDetailRepository.Get(x => x.Id == hblId).FirstOrDefault();
                if (hblDetail != null)
                {
                    var csTransaction = csTransactionRepository.Get(x => x.Id == hblDetail.JobId).FirstOrDefault();
                    if (csTransaction.NoProfit != true)
                    {
                        return string.Empty;
                    }
                    var surcharges = DataContext.Get(x => x.JobNo == csTransaction.JobNo && x.Type != DocumentConstants.CHARGE_OBH_TYPE && x.Id != charge.Id);
                    var totalBuying = surcharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE).Sum(x => x.AmountVnd ?? 0);
                    var totalSelling = surcharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE).Sum(x => x.AmountVnd ?? 0);
                    if (totalSelling - totalBuying > 0)
                    {
                        return "You can not delete this charge because Profit of Shipment NoProfit is bigger than 0";
                    }
                }
            }
            return string.Empty;
        }

        public HandleState CancelLinkCharge(Guid chargeId)
        {
            var hs = new HandleState();
            try
            {
                var charge = DataContext.Where(x => x.Id == chargeId).FirstOrDefault();
                if (charge == null)
                    hs = new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_SURCHARGE_NOT_FOUND].Value);
                if (charge != null
                    && (!string.IsNullOrEmpty(charge.Soano)
                    || !string.IsNullOrEmpty(charge.PaySoano)
                    || !string.IsNullOrEmpty(charge.CreditNo)
                    || !string.IsNullOrEmpty(charge.DebitNo)
                    || !string.IsNullOrEmpty(charge.SettlementCode)
                    || !string.IsNullOrEmpty(charge.VoucherId)
                    || !string.IsNullOrEmpty(charge.VoucherIdre)
                    || charge.AcctManagementId != null
                    || charge.PayerAcctManagementId != null))
                {
                    hs = new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_SURCHARGE_NOT_ALLOW_DELETED].Value);
                }
                else
                {
                    if (charge.LinkChargeId != null)
                    {
                        var chargeUpdate = DataContext.Where(x => x.Id == Guid.Parse(charge.LinkChargeId)).FirstOrDefault();
                        chargeUpdate.LinkChargeId = null;
                        DataContext.Update(chargeUpdate, x => x.Id == chargeUpdate.Id, false);
                        DataContext.SubmitChanges();
                    }
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
                        // Charges IsFromShipment = true
                        var chargesFromShipment = listCharges.Where(x =>
                            x.IsFromShipment == true &&
                            (x.Type == DocumentConstants.CHARGE_OBH_TYPE ?
                            (string.IsNullOrEmpty(x.CreditNo) && !string.IsNullOrEmpty(x.DebitNo) ?
                                string.IsNullOrEmpty(x.CreditNo) && x.PayerId == partnerId
                                :
                                (
                                    string.IsNullOrEmpty(x.DebitNo) && !string.IsNullOrEmpty(x.CreditNo) ?
                                        string.IsNullOrEmpty(x.DebitNo) && x.PaymentObjectId == partnerId
                                     : string.IsNullOrEmpty(x.DebitNo) && string.IsNullOrEmpty(x.CreditNo)
                                )
                            )
                            : string.IsNullOrEmpty(x.CreditNo) && string.IsNullOrEmpty(x.DebitNo))
                        ).ToList();

                        // Charges IsFromShipment = false 
                        var chargesNotFromShipment = listCharges.Where(x =>
                            x.IsFromShipment == false &&
                            (x.Type == DocumentConstants.CHARGE_OBH_TYPE ?
                            (string.IsNullOrEmpty(x.CreditNo) && !string.IsNullOrEmpty(x.DebitNo) ?
                                string.IsNullOrEmpty(x.CreditNo) && x.PayerId == partnerId
                                :
                                (
                                    string.IsNullOrEmpty(x.DebitNo) && !string.IsNullOrEmpty(x.CreditNo) ?
                                        string.IsNullOrEmpty(x.DebitNo) && x.PaymentObjectId == partnerId
                                     : string.IsNullOrEmpty(x.DebitNo) && string.IsNullOrEmpty(x.CreditNo)
                                )
                            )
                            : string.IsNullOrEmpty(x.CreditNo) && string.IsNullOrEmpty(x.DebitNo))
                        ).ToList();
                        var chargesDebitSettle = chargesNotFromShipment.Where(x => x.Type == DocumentConstants.CHARGE_OBH_TYPE && x.PaymentObjectId == partnerId && !string.IsNullOrEmpty(x.SettlementCode) && string.IsNullOrEmpty(x.DebitNo));
                        listCharges = chargesNotFromShipment.Except(chargesDebitSettle).ToList();

                        var settleInCharge = chargesDebitSettle.Select(x => x.SettlementCode).ToList();
                        var validSettleDebit = acctApproveSettlementRepository.Get(x => settleInCharge.Any(z => z == x.SettlementNo) && x.IsDeny == false && !string.IsNullOrEmpty(x.ManagerApr) && x.ManagerAprDate != null).Select(x => x.SettlementNo).ToList();
                        listCharges.AddRange(chargesDebitSettle.Where(x => validSettleDebit.Any(z => z == x.SettlementCode)));
                        listCharges.AddRange(chargesFromShipment);
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
                    var returnObj = new GroupChargeModel { Hwbno = houseBill.Hwbno, Hbltype = houseBill.Hbltype, Id = houseBill.Id, SalemanId = houseBill.SaleManId, listCharges = _listCharges, FlexId = houseBill.FlexId, ReferenceNoHBL = houseBill.ReferenceNo };
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
                    // Charges IsFromShipment = true
                    var chargesFromShipment = listCharges.Where(x =>
                         x.IsFromShipment == true &&
                            (x.Type == DocumentConstants.CHARGE_OBH_TYPE ?
                            (string.IsNullOrEmpty(x.CreditNo) && !string.IsNullOrEmpty(x.DebitNo) ?
                                string.IsNullOrEmpty(x.CreditNo) && x.PayerId == partnerId
                                :
                                (
                                    string.IsNullOrEmpty(x.DebitNo) && !string.IsNullOrEmpty(x.CreditNo) ?
                                        string.IsNullOrEmpty(x.DebitNo) && x.PaymentObjectId == partnerId
                                     : string.IsNullOrEmpty(x.DebitNo) && string.IsNullOrEmpty(x.CreditNo)
                                )
                            )
                            : string.IsNullOrEmpty(x.CreditNo) && string.IsNullOrEmpty(x.DebitNo))
                    ).ToList();

                    // Charges IsFromShipment = false
                    var chargesNotFromShipment = listCharges.Where(x =>
                            x.IsFromShipment == false &&
                            (x.Type == DocumentConstants.CHARGE_OBH_TYPE ?
                            (string.IsNullOrEmpty(x.CreditNo) && !string.IsNullOrEmpty(x.DebitNo) ?
                                string.IsNullOrEmpty(x.CreditNo) && x.PayerId == partnerId
                                :
                                (
                                    string.IsNullOrEmpty(x.DebitNo) && !string.IsNullOrEmpty(x.CreditNo) ?
                                        string.IsNullOrEmpty(x.DebitNo) && x.PaymentObjectId == partnerId
                                     : string.IsNullOrEmpty(x.DebitNo) && string.IsNullOrEmpty(x.CreditNo)
                                )
                            )
                            : string.IsNullOrEmpty(x.CreditNo) && string.IsNullOrEmpty(x.DebitNo))
                        ).ToList();
                    var chargesDebitSettle = chargesNotFromShipment.Where(x => x.Type == DocumentConstants.CHARGE_OBH_TYPE && x.PaymentObjectId == partnerId && !string.IsNullOrEmpty(x.SettlementCode) && string.IsNullOrEmpty(x.DebitNo));
                    listCharges = chargesNotFromShipment.Except(chargesDebitSettle).ToList();

                    var settleInCharge = chargesDebitSettle.Select(x => x.SettlementCode).ToList();
                    var validSettleDebit = acctApproveSettlementRepository.Get(x => settleInCharge.Any(z => z == x.SettlementNo) && x.IsDeny == false && !string.IsNullOrEmpty(x.ManagerApr) && x.ManagerAprDate != null).Select(x => x.SettlementNo).ToList();
                    listCharges.AddRange(chargesDebitSettle.Where(x => validSettleDebit.Any(z => z == x.SettlementCode)));
                    listCharges.AddRange(chargesFromShipment);
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
                var returnObj = new GroupChargeModel { Hwbno = houseBill.Hwbno, Id = houseBill.Id, SalemanId = houseBill.SalemanId, listCharges = _listCharges, FlexId = null };
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
                charge.IsRefundFee = item.IsRefundFee;
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
            //List<spc_GetSurchargeByHouseBill> surcharges = GetChargeByHouseBill(hblid, string.Empty, null);
            var typeSurcharges = new List<string>()
            {
                DocumentConstants.CHARGE_BUY_TYPE,
                DocumentConstants.CHARGE_SELL_TYPE,
                DocumentConstants.CHARGE_OBH_TYPE
            };
            var surcharges = DataContext.Get(x => x.Hblid == hblid && typeSurcharges.Any(z => z == x.Type)).Select(x => new { x.Type, x.AmountVnd, x.AmountUsd });
            if (surcharges.Count() == 0) return result;
            foreach (var item in surcharges)
            {
                if (item.Type == DocumentConstants.CHARGE_BUY_TYPE)
                {
                    result.HouseBillTotalCharge.TotalBuyingLocal += item.AmountVnd ?? 0;
                    result.HouseBillTotalCharge.TotalBuyingUSD += item.AmountUsd ?? 0;
                }
                else if (item.Type == DocumentConstants.CHARGE_SELL_TYPE)
                {
                    result.HouseBillTotalCharge.TotalSellingLocal += item.AmountVnd ?? 0;
                    result.HouseBillTotalCharge.TotalSellingUSD += item.AmountUsd ?? 0;
                }
                else
                {
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

        public HandleState AddAndUpdate(List<CsShipmentSurchargeModel> list, out List<Guid> Ids)
        {
            var result = new HandleState();
            Ids = new List<Guid>(); // ds các charge phí update công nợ.
            var surcharges = mapper.Map<List<CsShipmentSurcharge>>(list);

            var surchargesAdd = new List<CsShipmentSurcharge>();
            var surchargesUpdate = new List<CsShipmentSurcharge>();
            decimal kickBackExcRate = currentUser.KbExchangeRate ?? 20000;

            foreach (var item in surcharges)
            {
                if (item.Id == Guid.Empty)
                {
                    //** FinalExchangeRate = null do cần tính lại dựa vào ExchangeDate mới
                    item.FinalExchangeRate = null;
                    var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(item, kickBackExcRate);
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

                    item.TransactionType = GetTransactionType(item.JobNo);
                    if (item.Hblid != Guid.Empty)
                    {
                        if (item.TransactionType != "CL")
                        {
                            CsTransactionDetail hbl = tranDetailRepository.Get(x => x.Id == item.Hblid).FirstOrDefault();
                            item.OfficeId = hbl?.OfficeId ?? Guid.Empty;
                            item.CompanyId = hbl?.CompanyId ?? Guid.Empty;
                            // lưu cứng HBL Tránh bug.
                            item.Hblno = hbl?.Hwbno;
                            if (hbl != null)
                            {
                                var masterBill = csTransactionRepository.Get(x => x.Id == hbl.JobId).FirstOrDefault();
                                item.JobNo = masterBill?.JobNo;
                                //Ưu tiên lấy MBL của MasterBill >> HouseBill
                                item.Mblno = !string.IsNullOrEmpty(masterBill?.Mawb) ? masterBill?.Mawb : hbl.Mawb;
                            }
                        }
                        else
                        {
                            OpsTransaction hbl = opsTransRepository.Get(x => x.Hblid == item.Hblid).FirstOrDefault();
                            item.OfficeId = hbl?.OfficeId ?? Guid.Empty;
                            item.CompanyId = hbl?.CompanyId ?? Guid.Empty;
                            // set cứng thông tin từ lô hàng.
                            item.JobNo = hbl.JobNo;
                            item.Mblno = hbl.Mblno;
                            item.Hblno = hbl.Hwbno;
                            //Cập nhật Clearance No cũ nhất cho phí (nếu có), nếu phí đã có Clearance No & Settlement thì không cập nhật [15563 - 29/03/2021]
                            item.ClearanceNo = !string.IsNullOrEmpty(item.ClearanceNo) && !string.IsNullOrEmpty(item.SettlementCode) ? item.ClearanceNo : GetCustomNoOldOfShipment(item.JobNo);
                        }
                    }

                    surchargesAdd.Add(item);
                }
                else
                {
                    string _jobNo = string.Empty;
                    string _mblNo = string.Empty;
                    string _hblNo = string.Empty;
                    if (item.TransactionType != "CL")
                    {
                        var houseBill = tranDetailRepository.Get(x => x.Id == item.Hblid).FirstOrDefault();
                        _hblNo = houseBill?.Hwbno;
                        if (houseBill != null)
                        {
                            var masterBill = csTransactionRepository.Get(x => x.Id == houseBill.JobId).FirstOrDefault();
                            _jobNo = masterBill?.JobNo;
                            //Ưu tiên lấy MBL của MasterBill >> HouseBill
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
                        if (string.IsNullOrEmpty(surcharge.SettlementCode))
                        {
                            if (surcharge.Type == DocumentConstants.CHARGE_OBH_TYPE)
                            {
                                if (surcharge.PayerAcctManagementId == null
                                    || string.IsNullOrEmpty(surcharge.PaySoano)
                                    || string.IsNullOrEmpty(surcharge.CreditNo))
                                {
                                    surcharge.PayerId = item.PayerId;
                                }
                            }
                        }

                        //Chỉ cập nhật và tính lại giá trị Amount cho các charge chưa issue Settlement, Voucher, Invoice, SOA, CDNote
                        if (string.IsNullOrEmpty(surcharge.SettlementCode)
                            && (surcharge.AcctManagementId == Guid.Empty || surcharge.AcctManagementId == null)
                            && (surcharge.PayerAcctManagementId == Guid.Empty || surcharge.PayerAcctManagementId == null)
                            && (string.IsNullOrEmpty(surcharge.Soano) && string.IsNullOrEmpty(surcharge.PaySoano))
                            && (string.IsNullOrEmpty(surcharge.DebitNo) && string.IsNullOrEmpty(surcharge.CreditNo)))
                        {
                            surcharge.PaymentObjectId = item.PaymentObjectId;
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
                            surcharge.VatPartnerId = item.VatPartnerId;

                            //** FinalExchangeRate = null do cần tính lại dựa vào ExchangeDate mới
                            surcharge.FinalExchangeRate = null;
                            surcharge.ExchangeDate = item.ExchangeDate;

                            var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(surcharge, kickBackExcRate);
                            surcharge.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                            surcharge.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                            surcharge.FinalExchangeRate = amountSurcharge.FinalExchangeRate; //Tỉ giá so với Local
                            surcharge.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                            surcharge.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                            surcharge.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                            surcharge.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)

                            //Chỉ phí BUY & OBH mới được update InvoiceNo, InvoiceDate, SeriesNo
                            if (item.Type != DocumentConstants.CHARGE_SELL_TYPE)
                            {
                                surcharge.InvoiceNo = item.InvoiceNo;
                                surcharge.InvoiceDate = item.InvoiceDate;
                                surcharge.SeriesNo = item.SeriesNo;
                            }

                        }

                        // set cứng thông tin từ lô hàng.
                        surcharge.JobNo = _jobNo;
                        surcharge.Mblno = _mblNo;
                        surcharge.Hblno = _hblNo;
                        surcharge.DatetimeModified = DateTime.Now;
                        surcharge.UserModified = currentUser.UserID;
                        if (surcharge.TransactionType == "CL")
                        {
                            //Cập nhật Clearance No cũ nhất cho phí (nếu có), nếu phí đã có Clearance No & Settlement thì không cập nhật [15563 - 29/03/2021]
                            surcharge.ClearanceNo = !string.IsNullOrEmpty(surcharge.ClearanceNo) && !string.IsNullOrEmpty(surcharge.SettlementCode) ? surcharge.ClearanceNo : GetCustomNoOldOfShipment(surcharge.JobNo);
                        }

                        surcharge.IsRefundFee = item.IsRefundFee;
                        surchargesUpdate.Add(surcharge);
                    }
                }

                Ids.Add(item.Id);
            }

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (var surchargeAdd in surchargesAdd)
                    {
                        var hsAdd = DataContext.Add(surchargeAdd, false);
                    }

                    foreach (var surchargeUpdate in surchargesUpdate)
                    {
                        var hsUpdate = DataContext.Update(surchargeUpdate, x => x.Id == surchargeUpdate.Id, false);
                    }

                    result = DataContext.SubmitChanges();
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    new LogHelper("CsShipmentSurChargeLog", ex.ToString());
                    result = new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
                return result;
            }
        }

        /// <summary>
        /// Check add or update fee (Buy/Sell) is valid in no profit shipment
        /// </summary>
        /// <param name="list"></param>
        /// <param name="isValid"></param>
        /// <returns></returns>
        public List<CsShipmentSurchargeModel> CheckAddAndUpdateSellingsShipmentNoProfit(List<CsShipmentSurchargeModel> list, out bool isValid)
        {
            isValid = true;
            if (list.Where(x => x.Type == DocumentConstants.CHARGE_OBH_TYPE).FirstOrDefault() != null) // Chỉ áp dụng check nhập và sửa phí Buy và Sell
            {
                return new List<CsShipmentSurchargeModel>();
            }

            var hblId = list.Where(x => x.Hblid != Guid.Empty).FirstOrDefault().Hblid;
            var opsTrans = opsTransRepository.Get(x => x.Hblid == hblId).FirstOrDefault();
            CsTransaction csTransaction = null;
            if (opsTrans != null)
            {
                if (opsTrans.NoProfit != true)
                {
                    return new List<CsShipmentSurchargeModel>();
                }
            }
            else
            {
                var hblDetail = tranDetailRepository.Get(x => x.Id == hblId).FirstOrDefault();
                if (hblDetail != null)
                {
                    csTransaction = csTransactionRepository.Get(x => x.Id == hblDetail.JobId).FirstOrDefault();
                    if (csTransaction.NoProfit != true)
                    {
                        return new List<CsShipmentSurchargeModel>();
                    }
                }
            }

            var newSurcharges = list.Where(x => x.Id == Guid.Empty);
            var updateSurcharges = list.Where(x => x.Id != Guid.Empty);
            decimal kickBackExcRate = currentUser.KbExchangeRate ?? 20000;
            foreach (var item in newSurcharges)
            {
                //** FinalExchangeRate = null do cần tính lại dựa vào ExchangeDate mới
                item.FinalExchangeRate = null;
                var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(item, kickBackExcRate);
                item.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                item.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
            }
            foreach (var item in updateSurcharges)
            {
                var surcharge = DataContext.Get(x => x.Id == item.Id).FirstOrDefault();
                //Chỉ cập nhật và tính lại giá trị Amount cho các charge chưa issue Settlement, Voucher, Invoice, SOA, CDNote
                if (string.IsNullOrEmpty(item.SettlementCode)
                    && (surcharge.AcctManagementId == Guid.Empty || surcharge.AcctManagementId == null)
                    && (surcharge.PayerAcctManagementId == Guid.Empty || surcharge.PayerAcctManagementId == null)
                    && (string.IsNullOrEmpty(surcharge.Soano) && string.IsNullOrEmpty(surcharge.PaySoano))
                    && (string.IsNullOrEmpty(surcharge.DebitNo) && string.IsNullOrEmpty(surcharge.CreditNo)))
                {
                    //** FinalExchangeRate = null do cần tính lại dựa vào ExchangeDate mới
                    var surchargeMapper = mapper.Map<CsShipmentSurcharge>(item);
                    surchargeMapper.FinalExchangeRate = null;
                    surchargeMapper.ExchangeDate = item.ExchangeDate;

                    var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(surchargeMapper, kickBackExcRate);
                    item.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                    item.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                }
            }

            var chargesResult = new List<CsShipmentSurchargeModel>();
            if (newSurcharges.Count() > 0)
            {
                chargesResult.AddRange(newSurcharges);
            }
            if (updateSurcharges.Count() > 0)
            {
                chargesResult.AddRange(updateSurcharges);
            }


            if (opsTrans != null)
            {
                if (chargesResult.FirstOrDefault().Type == DocumentConstants.CHARGE_BUY_TYPE)
                {
                    var chargeSellings = DataContext.Get(x => x.Hblid == opsTrans.Hblid && x.Type == DocumentConstants.CHARGE_SELL_TYPE);
                    if (chargeSellings.Sum(x => x.AmountVnd ?? 0) - chargesResult.Sum(x => x.AmountVnd ?? 0) > 0)
                    {
                        isValid = false;
                        return chargesResult;
                    }
                }
                else
                {
                    var chargeBuyings = DataContext.Get(x => x.Hblid == opsTrans.Hblid && x.Type == DocumentConstants.CHARGE_BUY_TYPE);
                    if (chargesResult.Sum(x => x.AmountVnd ?? 0) - chargeBuyings.Sum(x => x.AmountVnd ?? 0) > 0)
                    {
                        isValid = false;
                        return chargesResult;
                    }
                }
            }
            else
            {
                var surcharges = DataContext.Get(x => x.JobNo == csTransaction.JobNo && x.Type != DocumentConstants.CHARGE_OBH_TYPE);
                if (chargesResult.FirstOrDefault().Type != DocumentConstants.CHARGE_BUY_TYPE)
                {
                    var chargeBuyings = surcharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE);
                    var remainSellings = surcharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE && x.Hblid != hblId);
                    var totalSellings = mapper.Map<List<CsShipmentSurchargeModel>>(remainSellings);
                    totalSellings.AddRange(chargesResult);

                    if (totalSellings.Sum(x => x.AmountVnd ?? 0) - chargeBuyings.Sum(x => x.AmountVnd ?? 0) > 0)
                    {
                        isValid = false;
                        return chargesResult;
                    }
                }
                else
                {
                    var chargeSellings = surcharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE);
                    var remainBuyings = surcharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && x.Hblid != hblId);
                    var totalBuyings = mapper.Map<List<CsShipmentSurchargeModel>>(remainBuyings);
                    totalBuyings.AddRange(chargesResult);

                    if (chargeSellings.Sum(x => x.AmountVnd ?? 0) - totalBuyings.Sum(x => x.AmountVnd ?? 0) > 0)
                    {
                        isValid = false;
                        return chargesResult;
                    }
                }

            }

            return chargesResult;
        }

        public HandleState UpdateFieldNetAmount_AmountUSD_VatAmountUSD(List<Guid> Ids)
        {
            var result = new HandleState();
            var surcharges = DataContext.Get(x => Ids.Contains(x.Id));
            decimal kickBackExcRate = 20000;
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in surcharges)
                    {
                        item.Vatrate = 8;
                        item.Notes = "IT hổ trợ update vat";
                        var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(item, kickBackExcRate);
                        item.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                        item.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                        item.FinalExchangeRate = item.FinalExchangeRate == null ? amountSurcharge.FinalExchangeRate : item.FinalExchangeRate; //Tỉ giá so với Local
                        item.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                        item.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                        item.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                        item.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)
                        item.DatetimeModified = DateTime.Now;
                        item.UserModified = "d1bb21ea-249a-455c-a981-dcb554c3b848";
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
                    //IQueryable<CsTransactionDetail> houseBills = transactionDetailService.GetHouseBill(csShipment.TransactionType);
                    hblids = tranDetailRepository.Get(x => x.JobId == csShipment.Id && x.ParentId == null).Select(x =>
                                    new HousbillProfit { HBLID = x.Id, HBLNo = x.Hwbno });
                    //hblids = houseBills.Where(x => x.JobId == csShipment.Id && x.ParentId == null).Select(x =>
                    //                new HousbillProfit { HBLID = x.Id, HBLNo = x.Hwbno });
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
        public IQueryable<CsShipmentSurchargeDetailsModel> GetRecentlyChargesJobOps(RecentlyChargeCriteria criteria)
        {
            Expression<Func<OpsTransaction, bool>> queryShipmentNearest = x => (x.OfficeId == currentUser.OfficeID
                                                   && (x.CustomerId == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                                                   && (x.SupplierId == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId)));
            if (queryShipmentNearest == null) return null;
            List<Guid> houseIds = new List<Guid>();
            queryShipmentNearest = queryShipmentNearest.And(x => x.Id != criteria.JobId); // kHác với lô hiện tại

            OpsTransaction shipment = opsTransRepository.Get(queryShipmentNearest)?.OrderByDescending(x => x.DatetimeCreated).FirstOrDefault();

            if (shipment == null) return null;

            if (criteria.ChargeType == DocumentConstants.CHARGE_BUY_TYPE)
            {
                if (criteria.ColoaderId == null) return null;
                houseIds = opsTransRepository.Get(x => x.Id == shipment.Id && x.SupplierId == criteria.ColoaderId).Select(x => x.Hblid).ToList();
            }
            else
            {
                if (criteria.CustomerId == null) return null;
                houseIds = opsTransRepository.Get(x => x.Id == shipment.Id && x.CustomerId == criteria.CustomerId).Select(x => x.Hblid).ToList();
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
                   ChargeGroup = surcharge.ChargeGroup
               });
            return result;
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
                    ChargeGroup = surcharge.ChargeGroup

                });
            return result;
        }

        public HandleState NotificationCreditTerm(List<CsShipmentSurchargeModel> list)
        {
            try
            {
                string jobno = list.Select(t => t.JobNo).FirstOrDefault();
                string transactionType = GetTransactionType(jobno);
                CsTransaction csTransaction = new CsTransaction();
                OpsTransaction opsTransaction = new OpsTransaction();
                var hs = new HandleState(false);
                bool isCheckedCreditRate = false;

                if (transactionType == "CL")
                {
                    opsTransaction = opsTransRepository.Get(x => x.JobNo == jobno).FirstOrDefault();
                    isCheckedCreditRate = settingFlowRepository.Any(x => x.OfficeId == opsTransaction.OfficeId && x.CreditLimit == true);
                }
                else
                {
                    csTransaction = csTransactionRepository.Get(x => x.JobNo == jobno).FirstOrDefault();
                    isCheckedCreditRate = settingFlowRepository.Any(x => x.OfficeId == csTransaction.OfficeId && x.CreditLimit == true);
                }
                /// Check credit term office
                if (isCheckedCreditRate)
                {
                    List<string> listPartner = list.Select(t => t.PaymentObjectId).ToList();
                    List<string> listPartnerByAc = partnerRepository.Get(x => listPartner.Contains(x.Id)).Select(t => t.ParentId).ToList();

                    var dataAgreements = transactionType == "CL" ? catContractRepository.Get(x => x.OfficeId.Contains(opsTransaction.OfficeId.ToString()) && x.SaleService.Contains(transactionType) && listPartnerByAc.Contains(x.PartnerId))
                        : catContractRepository.Get(x => x.OfficeId.Contains(csTransaction.OfficeId.ToString()) && x.SaleService.Contains(transactionType) && listPartnerByAc.Contains(x.PartnerId));
                    if (dataAgreements != null && dataAgreements.Any())
                    {
                        foreach (var item in dataAgreements)
                        {
                            if (item.CreditRate >= 120)
                            {
                                item.Active = false;
                                item.DatetimeModified = DateTime.Now;
                                item.UserModified = currentUser.UserID;
                                hs = catContractRepository.Update(item, x => x.Id == item.Id, false);
                            }
                        }
                        catContractRepository.SubmitChanges();
                        if (hs.Success)
                        {
                            //string title = "Over Credit Term";
                            var dataPartner = partnerRepository.Get(x => listPartner.Contains(x.Id)).ToList();
                            List<string> descriptions = new List<string>();
                            foreach (var item in dataPartner)
                            {
                                descriptions.Add(string.Format(@"<b style='color:#3966b6'>" + item.ShortName + "</b> is over credit limit with " + dataAgreements.Where(x => x.CreditRate >= 120 && x.PartnerId == item.Id).Select(t => t.CreditRate).FirstOrDefault() + " Please check it soon "));
                            }
                            // Add Notification
                            HandleState resultAddNotification = AddNotifications(descriptions, dataAgreements.ToList());
                            if (resultAddNotification.Success)
                            {
                                return resultAddNotification;
                            }
                        }
                        else
                        {
                            return new HandleState(false, "Credit rate not greater than 120!");
                        }
                    }
                    else
                    {
                        return new HandleState(false, "Agrements Data Not Found!");
                    }
                }
                else
                {
                    return new HandleState(false, "Credit limit not checked or office not found!");
                }
                // end check credit term

                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(false, ex.Message);
            }
        }

        public HandleState NotificationExpiredAgreement(List<CsShipmentSurchargeModel> list)
        {
            try
            {
                string jobno = list.Select(t => t.JobNo).FirstOrDefault();
                string transactionType = GetTransactionType(jobno);
                CsTransaction csTransaction = new CsTransaction();
                OpsTransaction opsTransaction = new OpsTransaction();
                var hs = new HandleState(false);
                bool isCheckedExpiredAgreement = false;

                if (transactionType == "CL")
                {
                    opsTransaction = opsTransRepository.Get(x => x.JobNo == jobno).FirstOrDefault();
                    isCheckedExpiredAgreement = settingFlowRepository.Any(x => x.OfficeId == opsTransaction.OfficeId && x.ExpiredAgreement == true);
                }
                else
                {
                    csTransaction = csTransactionRepository.Get(x => x.JobNo == jobno).FirstOrDefault();
                    isCheckedExpiredAgreement = settingFlowRepository.Any(x => x.OfficeId == csTransaction.OfficeId && x.ExpiredAgreement == true);
                }
                /// Check credit term office
                if (isCheckedExpiredAgreement)
                {
                    List<string> listPartner = list.Select(t => t.PaymentObjectId).ToList();
                    List<string> listPartnerByAc = partnerRepository.Get(x => listPartner.Contains(x.Id)).Select(t => t.ParentId).ToList();

                    var dataAgreements = transactionType == "CL" ? catContractRepository.Get(x => x.OfficeId.Contains(opsTransaction.OfficeId.ToString()) && x.SaleService.Contains(transactionType) && listPartnerByAc.Contains(x.PartnerId))
                        : catContractRepository.Get(x => x.OfficeId.Contains(csTransaction.OfficeId.ToString()) && x.SaleService.Contains(transactionType) && listPartnerByAc.Contains(x.PartnerId));
                    if (dataAgreements != null && dataAgreements.Any())
                    {
                        foreach (var item in dataAgreements)
                        {
                            if ((item.ContractType == "Official" && item.ExpiredDate > DateTime.Now) || (item.ContractType == "Trial" && item.TrialExpiredDate > DateTime.Now))
                            {
                                item.Active = false;
                                item.DatetimeModified = DateTime.Now;
                                item.UserModified = currentUser.UserID;
                                hs = catContractRepository.Update(item, x => x.Id == item.Id, false);
                            }
                        }
                        catContractRepository.SubmitChanges();
                        if (hs.Success)
                        {
                            var dataPartner = partnerRepository.Get(x => listPartner.Contains(x.Id)).ToList();
                            List<string> descriptions = new List<string>();
                            foreach (var item in dataPartner)
                            {
                                descriptions.Add(string.Format(@"<b style='color:#3966b6'>" + item.ShortName + "</b> is over Expired Date with" + dataAgreements.Where(x => ((x.ContractType == "Official" && x.ExpiredDate > DateTime.Now) || (x.ContractType == "Trial" && x.TrialExpiredDate > DateTime.Now)) && x.PartnerId == item.Id).Select(t => t.ExpiredDate).FirstOrDefault() + " Please check it soon "));
                            }
                            // Add Notification
                            HandleState resultAddNotification = AddNotifications(descriptions, dataAgreements.ToList());
                            if (resultAddNotification.Success)
                            {
                                return resultAddNotification;
                            }
                        }
                        else
                        {
                            return new HandleState(false, "Agrements Data Not Found!");
                        }
                    }
                    else
                    {
                        return new HandleState(false, "Agrements Data Not Found!");
                    }
                }
                else
                {
                    return new HandleState(false, "Expired Agreement not checked or office not found!");
                }
                // end check credit term

                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(false, ex.Message);
            }
        }

        public HandleState NotificationPaymenTerm(List<CsShipmentSurchargeModel> list)
        {
            try
            {
                string jobno = list.Select(t => t.JobNo).FirstOrDefault();
                string transactionType = GetTransactionType(jobno);
                CsTransaction csTransaction = new CsTransaction();
                OpsTransaction opsTransaction = new OpsTransaction();
                var hs = new HandleState(false);
                bool isCheckedPaymenterm = false;

                if (transactionType == "CL")
                {
                    opsTransaction = opsTransRepository.Get(x => x.JobNo == jobno).FirstOrDefault();
                    isCheckedPaymenterm = settingFlowRepository.Any(x => x.OfficeId == opsTransaction.OfficeId && x.OverPaymentTerm == true);
                }
                else
                {
                    csTransaction = csTransactionRepository.Get(x => x.JobNo == jobno).FirstOrDefault();
                    isCheckedPaymenterm = settingFlowRepository.Any(x => x.OfficeId == csTransaction.OfficeId && x.OverPaymentTerm == true);
                }
                /// Check credit term office
                if (isCheckedPaymenterm)
                {
                    List<string> listPartner = list.Select(t => t.PaymentObjectId).ToList();
                    List<string> listPartnerByAc = partnerRepository.Get(x => listPartner.Contains(x.Id)).Select(t => t.ParentId).ToList();

                    var dataAgreements = transactionType == "CL" ? catContractRepository.Get(x => x.OfficeId.Contains(opsTransaction.OfficeId.ToString()) && x.SaleService.Contains(transactionType) && listPartnerByAc.Contains(x.PartnerId))
                        : catContractRepository.Get(x => x.OfficeId.Contains(csTransaction.OfficeId.ToString()) && x.SaleService.Contains(transactionType) && listPartnerByAc.Contains(x.PartnerId));
                    if (dataAgreements != null && dataAgreements.Any())
                    {
                        foreach (var item in dataAgreements)
                        {
                            bool dataAccountReceivable = accAccountReceivableRepository.Any(x => item.OfficeId.Contains(x.Office.ToString()) && item.SaleService.Contains(x.Service) && item.PartnerId == x.PartnerId && x.Over30Day > 0);
                            if (dataAccountReceivable)
                            {
                                item.Active = false;
                                item.DatetimeModified = DateTime.Now;
                                item.UserModified = currentUser.UserID;
                                hs = catContractRepository.Update(item, x => x.Id == item.Id, false);

                            }
                        }
                        catContractRepository.SubmitChanges();
                        if (hs.Success)
                        {
                            List<string> descriptions = new List<string>();
                            var dataPartner = partnerRepository.Get(x => listPartner.Contains(x.Id)).ToList();
                            foreach (var item in dataPartner)
                            {
                                string type = string.Empty;
                                if (item.PartnerType == "Customer")
                                {
                                    type = "Customer";
                                }
                                else if (item.PartnerType == "Agent")
                                {
                                    type = "Agent";
                                }
                                else
                                {
                                    type = "Partner Data";
                                }
                                descriptions.Add(type + " " + string.Format(@"<b style='color:#3966b6'>" + item.ShortName + "</b> has debit overdue" + dataAgreements.Where(x => x.PartnerId == item.Id).Select(t => t.PaymentTerm).FirstOrDefault() + " Please check it soon "));
                            }
                            // Add Notification
                            HandleState resultAddNotification = AddNotifications(descriptions, dataAgreements.ToList());
                            if (resultAddNotification.Success)
                            {
                                return resultAddNotification;
                            }
                        }
                        else
                        {
                            return new HandleState(false, "Agrements Data Not Found!");
                        }
                    }
                    else
                    {
                        return new HandleState(false, "Agrements Data Not Found!");
                    }
                }
                else
                {
                    return new HandleState(false, "Expired Agreement not checked or office not found!");
                }
                // end check credit term

                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(false, ex.Message);
            }
        }

        public object CheckAccountReceivable(List<CsShipmentSurchargeModel> list)
        {
            bool validCreditTerm = true;
            bool validPaymentTerm = true;
            bool validExpiredDate = true;
            try
            {
                string jobno = list.Select(t => t.JobNo).FirstOrDefault();
                string transactionType = GetTransactionType(jobno);
                CsTransaction csTransaction = new CsTransaction();
                OpsTransaction opsTransaction = new OpsTransaction();
                List<PartnerAccountReceivable> partnerAccountReceivables = new List<PartnerAccountReceivable>();
                var hs = new HandleState(false);
                bool isCheckedPaymenterm = false;
                bool isCheckedCreditterm = false;
                bool isCheckedExpiredAgreement = false;

                List<string> listPartner = list.Select(t => t.PaymentObjectId).ToList();
                List<string> listPartnerByAc = partnerRepository.Get(x => listPartner.Contains(x.Id)).Select(t => t.ParentId).ToList();

                var dataAgreements = transactionType == "CL" ? catContractRepository.Get(x => x.OfficeId.Contains(opsTransaction.OfficeId.ToString()) && x.SaleService.Contains(transactionType) && listPartnerByAc.Contains(x.PartnerId))
                : catContractRepository.Get(x => x.OfficeId.Contains(csTransaction.OfficeId.ToString()) && x.SaleService.Contains(transactionType) && listPartnerByAc.Contains(x.PartnerId));

                if (transactionType == "CL")
                {
                    opsTransaction = opsTransRepository.Get(x => x.JobNo == jobno).FirstOrDefault();
                    isCheckedCreditterm = settingFlowRepository.Any(x => x.OfficeId == csTransaction.OfficeId && x.CreditLimit == true);
                    isCheckedPaymenterm = settingFlowRepository.Any(x => x.OfficeId == csTransaction.OfficeId && x.OverPaymentTerm == true);
                    isCheckedExpiredAgreement = settingFlowRepository.Any(x => x.OfficeId == csTransaction.OfficeId && x.ExpiredAgreement == true);
                }
                else
                {
                    csTransaction = csTransactionRepository.Get(x => x.JobNo == jobno).FirstOrDefault();
                    isCheckedCreditterm = settingFlowRepository.Any(x => x.OfficeId == csTransaction.OfficeId && x.CreditLimit == true);
                    isCheckedPaymenterm = settingFlowRepository.Any(x => x.OfficeId == csTransaction.OfficeId && x.OverPaymentTerm == true);
                    isCheckedExpiredAgreement = settingFlowRepository.Any(x => x.OfficeId == csTransaction.OfficeId && x.ExpiredAgreement == true);
                }
                if (dataAgreements != null && dataAgreements.Any())
                {
                    string transactionTypes = API.Common.Globals.CustomData.Services.FirstOrDefault(x => x.Value == transactionType)?.DisplayName;
                    var dataPartner = partnerRepository.Get(x => listPartner.Contains(x.Id)).ToList();
                    foreach (var item in dataPartner)
                    {

                        foreach (var agreement in dataAgreements)
                        {
                            PartnerAccountReceivable partnerAccountReceivable = new PartnerAccountReceivable();
                            partnerAccountReceivable.ShortName = item.ShortName;
                            if (item.ParentId == agreement.PartnerId && ((agreement.CreditRate >= 120) || (accAccountReceivableRepository.Any(x => x.PartnerId == agreement.PartnerId && (agreement.OfficeId.Contains(x.Office.ToString()) && agreement.SaleService.Contains(x.Service) && x.Over30Day > 0)))))
                            {
                                partnerAccountReceivable.PaymentTerm = agreement.PaymentTerm;
                                partnerAccountReceivable.CreditRate = agreement.CreditRate;
                                partnerAccountReceivables.Add(partnerAccountReceivable);

                            }
                        }
                    }
                    if (isCheckedCreditterm)
                    {

                        if (dataAgreements.Any(x => x.CreditRate >= 120)) validCreditTerm = false;
                        var data = partnerAccountReceivables.Find(x => x.CreditRate != null && x.CreditRate < 120);
                        partnerAccountReceivables.Remove(data);
                    }
                    if (isCheckedExpiredAgreement)
                    {
                        if (dataAgreements.Any(x => (x.ContractType == "Official" && x.ExpiredDate > DateTime.Now) || (x.ContractType == "Trial" && x.TrialExpiredDate > DateTime.Now))) validExpiredDate = false;
                    }
                    if (isCheckedPaymenterm)
                    {
                        foreach (var item in dataAgreements)
                        {
                            bool dataAccountReceivable = accAccountReceivableRepository.Any(x => item.OfficeId.Contains(x.Office.ToString()) && item.SaleService.Contains(x.Service) && item.PartnerId == x.PartnerId && x.Over30Day > 0);
                            if (dataAccountReceivable)
                            {
                                validPaymentTerm = false;
                                break;
                            }
                        }

                    }

                    return new { validCreditTerm, validPaymentTerm, validExpiredDate, transactionTypes, partnerAccountReceivables };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }


        private HandleState AddNotifications(List<string> descriptions, List<CatContract> dataAgreements)
        {
            HandleState hsSysNotification = new HandleState(false);
            List<SysNotifications> notifications = new List<SysNotifications>();
            foreach (var description in descriptions)
            {
                SysNotifications sysNotification = new SysNotifications
                {
                    Id = Guid.NewGuid(),
                    Title = description,
                    Description = description,
                    Type = "User",
                    UserCreated = currentUser.UserID,
                    DatetimeCreated = DateTime.Now,
                    DatetimeModified = DateTime.Now,
                    UserModified = currentUser.UserID,
                    Action = "Detail",
                    ActionLink = string.Empty,
                    IsClosed = false,
                    IsRead = false
                };
                notifications.Add(sysNotification);
            }
            hsSysNotification = notificationRepository.Add(notifications, false);
            if (hsSysNotification.Success)
            {
                List<string> users = GetUserSaleAndDepartmentAR(dataAgreements);
                List<SysUserNotification> userNotifications = new List<SysUserNotification>();
                foreach (var item in users)
                {
                    foreach (var noti in notifications)
                    {
                        SysUserNotification userNotify = new SysUserNotification
                        {
                            Id = Guid.NewGuid(),
                            DatetimeCreated = DateTime.Now,
                            DatetimeModified = DateTime.Now,
                            Status = "New",
                            NotitficationId = noti.Id,
                            UserId = item,
                            UserCreated = currentUser.UserID,
                            UserModified = currentUser.UserID,
                        };
                        userNotifications.Add(userNotify);
                    }

                }
                HandleState hsSysUserNotification = sysUserNotifyRepository.Add(userNotifications, false);
                notificationRepository.SubmitChanges();
                sysUserNotifyRepository.SubmitChanges();
                if (hsSysUserNotification.Success) return hsSysUserNotification;
            }
            return new HandleState(false);
        }

        private List<string> GetUserSaleAndDepartmentAR(List<CatContract> contracts)
        {
            List<string> users = new List<string>();
            List<string> usersDepartmentAR = new List<string>();
            int DepartmentId = catDepartmentRepository.Get(x => x.DeptType == "AR").Select(t => t.Id).FirstOrDefault();
            usersDepartmentAR = userlevelRepository.Get(x => x.DepartmentId == DepartmentId).Select(t => t.UserId).ToList();
            users.AddRange(contracts.Select(t => t.SaleManId).ToList());
            if (usersDepartmentAR != null)
            {
                users.AddRange(usersDepartmentAR);
            }
            return users;
        }

        public List<CsShipmentSurchargeImportModel> CheckValidImport(List<CsShipmentSurchargeImportModel> list)
        {
            var listChargeOps = DataContext.Get(x => x.TransactionType == "CL");
            var listPartner = partnerRepository.Get(x => x.Active == true);
            var chargeData = catChargeRepository.Get(x => x.Active == true).ToLookup(x => x.Code);
            var opsTransaction = opsTransRepository.Get(x => x.CurrentStatus != "Canceled" && x.IsLocked == false);
            var customsDeclaration = customsDeclarationRepository.Get().ToLookup(x => x.ClearanceNo); ;
            string TypeCompare = string.Empty;
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.Hblno))
                {
                    item.HBLNoError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_HBLNO_EMPTY]);
                    item.IsValid = false;
                }
                else
                {
                    if (!opsTransaction.Any(x => (string.IsNullOrEmpty(item.Mblno) || x.Mblno == item.Mblno) && x.Hwbno == item.Hblno))
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
                    if (!opsTransaction.Any(x => x.Mblno == item.Mblno.Trim() && (string.IsNullOrEmpty(item.Hblno) || x.Hwbno == item.Hblno)))
                    {
                        item.MBLNoError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_MBLNO_NOT_EXIST], item.Mblno);
                        item.IsValid = false;
                    }
                    else if (!string.IsNullOrEmpty(item.Hblno) && !string.IsNullOrEmpty(item.Mblno))
                    {
                        if (!opsTransaction.Any(x => x.Mblno == item.Mblno.Trim() && x.Hwbno == item.Hblno))
                        {
                            item.HBLNoError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_HBLNO_NOT_EXIST], item.Hblno);
                            item.MBLNoError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_MBLNO_NOT_EXIST], item.Mblno);
                            item.IsValid = false;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(item.ClearanceNo))
                {
                    var customNo = customsDeclaration[item.ClearanceNo.Trim()].Any(x => x.JobNo != null);
                    if (!customNo)
                    {     
                        item.ClearanceNoError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_CUSTOM_NO_NOT_EXIST_JOB]);
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
                    if (item.Type == "Selling")
                    {
                        var validCustomer = listPartner.Any(x => x.AccountNo.Trim() == item.PartnerCode.Replace("'", "").Trim() && (x.PartnerGroup.Contains("AGENT") || x.PartnerGroup.Contains("CUSTOMER")));
                        if (!validCustomer)
                        {
                            item.PartnerCodeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_PARTER_CODE_NOT_VALID_TYPE], item.PartnerCode);
                            item.IsValid = false;
                        }
                    }

                }
                if (!string.IsNullOrEmpty(item.ObhPartner))
                {
                    if (!listPartner.Any(x => x.AccountNo.Trim() == item.ObhPartner.Replace("'", "").Trim()))
                    {
                        item.ObhPartnerError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_PARTER_CODE_NOT_EXIST], item.ObhPartner);
                        item.IsValid = false;
                    }
                    else if (item.PartnerCode == item.ObhPartner)
                    {
                        item.ObhPartnerError = stringLocalizer[DocumentationLanguageSub.MSG_PARTNER_CODE_DUPLICATE];
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
                    else
                    {
                        var lookupcharges = chargeData[item.ChargeCode.Trim()];
                        // check valid obh partner
                        if (lookupcharges.Any(x => x.Type == "CREDIT") && !string.IsNullOrEmpty(item.ObhPartner))
                        {
                            item.ObhPartnerError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_OBH_PARTNER_CODE_WRONG], item.ChargeCode);
                            item.IsValid = false;
                        }
                        else if (lookupcharges.Where(x => x.Type == "OBH").Any() && string.IsNullOrEmpty(item.ObhPartner))
                        {
                            item.ObhPartnerError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_OBH_PARTNER_CODE_EMPTY], item.ChargeCode);
                            item.IsValid = false;
                        }
                        if (!lookupcharges.Any(x => x.ServiceTypeId.Contains("CL")))
                        {
                            item.ChargeCodeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_CHARGE_CODE_WRONG_SERVICE], item.ChargeCode);
                            item.IsValid = false;
                        }
                        if (lookupcharges.FirstOrDefault().Offices != null && !(lookupcharges.FirstOrDefault().Offices.ToLower().Contains(currentUser.OfficeID.ToString().ToLower())))
                        {
                            item.ChargeCodeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_CHARGE_CODE_WRONG_OFFICE], item.ChargeCode, currentUser.OfficeCode);
                            item.IsValid = false;
                        }
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
                if (!item.UnitPrice.HasValue || item.UnitPrice == 0)
                {
                    item.UnitPriceError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_UNIT_PRICE_EMPTY]);
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CurrencyId))
                {
                    item.CurrencyError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_CURRENCY_EMPTY]);
                    item.IsValid = false;
                }
                else
                {
                    if (!currencyRepository.Any(x => x.Id == item.CurrencyId.Trim()))
                    {
                        item.UnitError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_CURRENCY_NOT_EXIST], item.CurrencyId);
                        item.IsValid = false;
                    }
                }
                if (!item.Vatrate.HasValue)
                {
                    item.VatError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_VAT_EMPTY]);
                    item.IsValid = false;
                }
                //if (!item.TotalAmount.HasValue)
                //{
                //    item.TotalAmountError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_TOTAL_AMOUNT_EMPTY]);
                //    item.IsValid = false;
                //}
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
                //if (!string.IsNullOrEmpty(item.InvoiceNo))
                //{
                //    if (string.IsNullOrEmpty(item.SeriesNo))
                //    {
                //        item.SerieNoError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_SERIENO_EMPTY]);
                //        item.IsValid = false;
                //    }
                //}
                //if (!string.IsNullOrEmpty(item.InvoiceNo) && string.IsNullOrEmpty(item.SeriesNo) && item.Type.ToLower() == "obh")
                //{
                //    item.SerieNoError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_SERIES_NO_REQUIRED], item.ChargeCode);
                //    item.IsValid = false;
                //}
                //if (!string.IsNullOrEmpty(item.SeriesNo) && string.IsNullOrEmpty(item.InvoiceNo) && item.Type.ToLower() == "obh")
                //{
                //    item.InvoiceNoError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_INVOICE_NO_REQUIRED], item.ChargeCode);
                //    item.IsValid = false;
                //}
                else
                {
                    if (item.Type.ToLower() != "buying" && item.Type.ToLower() != "selling" && item.Type.ToLower() != "obh")
                    {
                        item.TypeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_TYPE_NOT_VALID], item.Type);
                        item.IsValid = false;
                    }
                    else
                    {
                        TypeCompare = string.Empty;
                        if (item.Type.ToLower() == "buying")
                        {
                            TypeCompare = "CREDIT";
                        }
                        else if (item.Type.ToLower() == "selling")
                        {
                            TypeCompare = "DEBIT";
                        }
                        else if (item.Type.ToLower() == "obh")
                        {
                            TypeCompare = "OBH";
                        }
                        if (string.IsNullOrEmpty(item.ChargeCodeError))
                        {
                            var trueType = chargeData[item.ChargeCode.Trim()].Any(x => x.Type == TypeCompare);
                            if (!trueType && string.IsNullOrEmpty(item.ChargeCodeError))
                            {
                                item.ChargeCodeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_CHARGE_CODE_INVALID_TYPE], item.ChargeCode);
                                item.IsValid = false;
                            }
                        }
                        if (TypeCompare == "OBH" && string.IsNullOrEmpty(item.ObhPartner))
                        {
                            item.ObhPartnerError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_OBH_PARTNER_CODE_EMPTY], item.ChargeCode);
                            item.IsValid = false;
                        }
                    }
                }

                if (item.IsValid)
                {
                    OpsTransaction currentOpsJob = opsTransaction.Where(x => x.Hwbno == item.Hblno.Trim() && x.Mblno == item.Mblno.Trim() && x.OfficeId == currentUser.OfficeID).FirstOrDefault();
                    if (currentOpsJob != null)
                    {
                        string PartnerId = listPartner.Where(x => x.AccountNo.Trim() == item.PartnerCode.Trim()).Select(t => t.Id).FirstOrDefault();
                        string obhPartnerId = string.IsNullOrEmpty(item.ObhPartner) ? string.Empty : listPartner.Where(x => x.AccountNo.Trim() == item.ObhPartner).Select(t => t.Id).FirstOrDefault();
                        Guid ChargeId = catChargeRepository.Get(x => x.Code == item.ChargeCode).Select(t => t.Id).FirstOrDefault();
                        item.ChargeId = ChargeId;
                        short UnitId = unitRepository.Get(x => x.UnitNameEn == item.Unit.Trim()).Select(t => t.Id).FirstOrDefault();
                        item.UnitId = UnitId;
                        item.PaymentObjectId = PartnerId;
                        item.Quantity = (decimal)item.Qty;

                        item.Hblid = currentOpsJob.Hblid;
                        item.JobNo = currentOpsJob.JobNo;
                        item.TransactionType = "CL";
                        string jobNo = currentOpsJob.JobNo;
                        if (item.Type.ToLower() == "obh")
                        {
                            item.PaymentObjectId = obhPartnerId;
                            item.PayerId = PartnerId;
                        }

                        if (item.Type.ToLower() == "buying")
                        {
                            TypeCompare = "BUY";
                        }
                        else if (item.Type.ToLower() == "selling")
                        {
                            TypeCompare = "SELL";
                        }
                        else if (item.Type.ToLower() == "obh")
                        {
                            TypeCompare = "OBH";
                        }

                        if (string.IsNullOrEmpty(item.ObhPartner))
                        {
                            if (listChargeOps.Any(x => x.Mblno.Trim() == item.Mblno.Trim() && x.Hblno.Trim() == item.Hblno.Trim() && x.PaymentObjectId == PartnerId && x.ChargeId == ChargeId && x.Type == TypeCompare))
                            {
                                item.ChargeCodeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_CHARGE_CODE_DUPLICATE], item.ChargeCode, jobNo);
                                item.IsValid = false;
                            }
                        }
                        else
                        {
                            if (listChargeOps.Any(x => x.Mblno.Trim() == item.Mblno.Trim() && x.Hblno.Trim() == item.Hblno.Trim() && x.PaymentObjectId == obhPartnerId && x.PayerId == PartnerId && x.ChargeId == ChargeId && x.Type == TypeCompare))
                            {
                                item.ChargeCodeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_CHARGE_CODE_DUPLICATE], item.ChargeCode, jobNo);
                                item.IsValid = false;
                            }
                        }
                        if (!string.IsNullOrEmpty(item.SeriesNo))
                        {
                            if (listChargeOps.Any(x => x.Mblno.Trim() == item.Mblno.Trim() && x.Hblno.Trim() == item.Hblno.Trim() && x.PaymentObjectId == PartnerId && x.ChargeId == ChargeId && x.SeriesNo == item.SeriesNo && x.Type == TypeCompare))
                            {
                                item.ChargeCodeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_CHARGE_CODE_DUPLICATE], item.ChargeCode, jobNo);
                                item.IsValid = false;
                            }
                        }
                        if (!string.IsNullOrEmpty(item.InvoiceNo))
                        {
                            if (listChargeOps.Any(x => x.Mblno.Trim() == item.Mblno.Trim() && x.Hblno.Trim() == item.Hblno.Trim() && x.PaymentObjectId == PartnerId && x.ChargeId == ChargeId && x.InvoiceNo == item.InvoiceNo && x.Type == TypeCompare))
                            {
                                item.ChargeCodeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_CHARGE_CODE_DUPLICATE], item.ChargeCode, jobNo);
                                item.IsValid = false;
                            }
                        }
                        if (!string.IsNullOrEmpty(item.SeriesNo) && !string.IsNullOrEmpty(item.InvoiceNo))
                        {
                            if (listChargeOps.Any(x => x.Mblno.Trim() == item.Mblno.Trim() && x.Hblno.Trim() == item.Hblno.Trim() && x.PaymentObjectId == PartnerId && x.ChargeId == ChargeId && x.InvoiceNo == item.InvoiceNo && x.SeriesNo == item.SeriesNo && x.Type == TypeCompare))
                            {
                                item.ChargeCodeError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_CHARGE_CODE_DUPLICATE], item.ChargeCode, jobNo);
                                item.IsValid = false;
                            }
                        }

                        if (item.CurrencyId == DocumentConstants.CURRENCY_USD) // Error if total > 100,000usd
                        {
                            var _roundDecimal = 2;
                            var _netAmount = NumberHelper.RoundNumber((item.UnitPrice * item.Quantity) ?? 0, _roundDecimal);
                            var _vatAmount = NumberHelper.RoundNumber(item.Vatrate < 0 ? Math.Abs(item.Vatrate ?? 0) : ((_netAmount * item.Vatrate) ?? 0) / 100, _roundDecimal);
                            if ((_netAmount + _vatAmount) > 100000)
                            {
                                item.CurrencyError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_TOTAL_OVER_LIMIT], item.CurrencyId);
                                item.IsValid = false;
                            }
                        }
                    }
                }
            });
            var validList = list.Where(x => x.IsValid && x.Type.ToLower() != "obh").ToList();
            var jobNoProfits = InvalidShipmentNoProfitImport(validList);
            if (jobNoProfits.Count > 0)
            {
                var notSellingCharges = list.Where(x => !x.IsValid || x.Type.ToLower() != "selling").ToList();
                var sellingCharges = list.Where(x => x.IsValid && x.Type.ToLower() == "selling").ToList();
                sellingCharges.ForEach(item =>
                {
                    //if (item.IsValid && item.Type.ToLower() == "sell") Check import fee is valid with no profit and noti to selling charges
                    {
                        if (jobNoProfits.Any(x => x.Hwbno == item.Hblno && x.Mblno == item.Mblno))
                        {
                            item.HBLNoError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_SHIPMENT_INVALID_NO_PROFIT], "hbl " + item.Hblno);
                            item.MBLNoError = string.Format(stringLocalizer[DocumentationLanguageSub.MSG_SHIPMENT_INVALID_NO_PROFIT], "mbl " + item.Mblno);
                            item.IsValid = false;
                        }
                    }
                });
                list = notSellingCharges;
                list.AddRange(sellingCharges);
            }

            if (list.Count > 1)
            {
                for (int i = 0; i < list.Count() - 1; i++)
                {
                    int j = i + 1;
                    while (j < list.Count())
                    {
                        if (list[i].InvoiceNo == list[j].InvoiceNo && list[i].InvoiceNo != null)
                        {
                            list[i].IsValid = false;
                            list[j].IsValid = false;
                            if ((list[i].Type.ToLower() == "obh" && list[j].Type.ToLower() == "buying") || (list[j].Type.ToLower() == "obh" && list[i].Type.ToLower() == "buying"))
                            {
                                list[i].IsValid = true;
                                list[j].IsValid = true;
                            }
                        }
                        j++;
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Check if import fee is valid with no profit shipment
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<OpsTransaction> InvalidShipmentNoProfitImport(List<CsShipmentSurchargeImportModel> list)
        {
            var shipmentGrp = list.Where(x => x.Type.ToLower() != "obh").GroupBy(x => new { x.Hblno, x.Mblno }).Select(x => new { x.Key, charges = x.Select(z => z).ToList() });
            var opsTransaction = opsTransRepository.Get(x => x.CurrentStatus != "Canceled" && x.IsLocked == false && x.OfficeId == currentUser.OfficeID);
            var chargeGroup = catChargeGroupRepository.Get();
            var catCharge = catChargeRepository.Get();
            var jobNoProfit = new List<OpsTransaction>();
            foreach (var shipment in shipmentGrp)
            {
                var opsDetail = opsTransaction.Where(x => x.Hwbno == shipment.Key.Hblno && x.Mblno == shipment.Key.Mblno).FirstOrDefault();
                if (opsDetail != null)
                {
                    if (opsDetail.NoProfit == true)
                    {
                        shipment.charges.ForEach(item =>
                        {
                            var charge = mapper.Map<CsShipmentSurcharge>(item);
                            switch (charge.Type.ToLower())
                            {
                                case "buying":
                                    charge.Type = "BUY";
                                    break;
                                case "obh":
                                    charge.Type = "OBH";
                                    break;
                                case "selling":
                                    charge.Type = "SELL";
                                    break;
                            }
                            var chargeGroupId = catCharge.Where(x => x.Id == charge.ChargeId).Select(x => x.ChargeGroup).FirstOrDefault();
                            charge.KickBack = chargeGroup.Where(x => x.Id == chargeGroupId && x.Name == "Com").FirstOrDefault() != null ? true : false;

                            decimal kickBackExcRate = currentUser.KbExchangeRate ?? 20000;

                            #region --Tính giá trị các field: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
                            var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(charge, kickBackExcRate);
                            charge.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                            #endregion --Tính giá trị các field: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
                        });
                        var surcharges = DataContext.Get(x => x.Hblid == opsDetail.Hblid && x.Type != DocumentConstants.CHARGE_OBH_TYPE);
                        var chargesBuyings = new List<CsShipmentSurcharge>();
                        var chargesSellings = new List<CsShipmentSurcharge>();
                        chargesBuyings.AddRange(surcharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE));
                        chargesBuyings.AddRange(shipment.charges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE));

                        chargesSellings.AddRange(surcharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE));
                        chargesSellings.AddRange(shipment.charges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE));
                        if (chargesSellings.Sum(x => x.AmountVnd ?? 0) - chargesBuyings.Sum(x => x.AmountVnd ?? 0) > 0)
                        {
                            jobNoProfit.Add(opsDetail);
                        }
                    }
                }
            }
            return jobNoProfit;
        }

        public HandleState Import(List<CsShipmentSurchargeImportModel> list, out List<Guid> Ids)
        {
            Ids = new List<Guid>(); // ds charge dùng để tính công nợ
            var chargeGroup = catChargeGroupRepository.Get();
            var listImport = new List<CsShipmentSurchargeImportModel>();
            foreach (var item in list)
            {
                OpsTransaction hbl = opsTransRepository.Get(x => x.Hblid == item.Hblid).FirstOrDefault();
                if (hbl.OfficeId == currentUser.OfficeID)
                {
                    switch (item.Type.ToLower())
                    {
                        case "buying":
                            item.Type = "BUY";
                            break;
                        case "obh":
                            item.Type = "OBH";
                            break;
                        case "selling":
                            item.Type = "SELL";
                            break;
                    }
                    item.UserCreated = item.UserModified = currentUser.UserID;
                    item.Id = Guid.NewGuid();
                    item.DatetimeCreated = item.DatetimeModified = DateTime.Now;
                    item.OfficeId = hbl?.OfficeId ?? Guid.Empty;
                    item.CompanyId = hbl?.CompanyId ?? Guid.Empty;
                    var chargeGroupId = catChargeRepository.Get(x => x.Id == item.ChargeId).Select(x => x.ChargeGroup).FirstOrDefault();
                    item.KickBack = chargeGroup.Where(x => x.Id == chargeGroupId && x.Name == "Com").Any();

                    decimal kickBackExcRate = currentUser.KbExchangeRate ?? 20000;

                    #region --Tính giá trị các field: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
                    var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(item, kickBackExcRate);
                    item.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                    item.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                    item.FinalExchangeRate = amountSurcharge.FinalExchangeRate; //Tỉ giá so với Local
                    item.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                    item.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                    item.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                    item.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)
                    #endregion --Tính giá trị các field: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
                    listImport.Add(item);
                    //if (item.Type.ToLower() == "buying")
                    //{
                    //    item.Type = "BUY";
                    //}
                    //if (item.Type.ToLower() == "selling")
                    //{
                    //    item.Type = "SELL";
                    //}

                    Ids.Add(item.Id);
                }
            }
            var datas = mapper.Map<List<CsShipmentSurcharge>>(listImport);
            if (datas.Count == 0)
            {
                return new HandleState(true, "");
            }
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

        public HandleState ImportPQL(List<string> list)
        {
            var surcharges = new List<CsShipmentSurcharge>();

            foreach (var cd in list)
            {
                surcharges.Add(new CsShipmentSurcharge
                {
                    Id = Guid.NewGuid(),
                    Hblid = Guid.Parse("C45EFF8A-9197-4D0C-A723-F416F21E90BC"),
                    Type = "SELL",
                    ChargeId = Guid.Parse("19E68F55-1724-4B8B-AB73-E318D3A5D0B0"),
                    Quantity = 1,
                    UnitPrice = 350000,
                    UnitId = 183,
                    CurrencyId = "VND",
                    Vatrate = 8,
                    Total = 378000,
                    PaymentObjectId = "bc7fe421-e049-4b64-abd7-ab4865db4040",
                    ExchangeDate = DateTime.Now,
                    ClearanceNo = cd,
                    IsFromShipment = true,
                    UserCreated = "d1bb21ea-249a-455c-a981-dcb554c3b848",
                    UserModified = "d1bb21ea-249a-455c-a981-dcb554c3b848",
                    DatetimeCreated = DateTime.Now,
                    DatetimeModified = DateTime.Now,
                    FinalExchangeRate = 1,
                    JobNo = "RLOG2207/01809",
                    Mblno = "PQL-052022",
                    Hblno = "PQL-052022",
                    AmountVnd = 350000,
                    VatAmountVnd = 28000,
                    AmountUsd = (decimal)15.02,
                    VatAmountUsd = (decimal)1.2,
                    TransactionType = "CL",
                    OfficeId = Guid.Parse("FC576371-1779-4632-A510-7CA828227F48"),
                    CompanyId = Guid.Parse("27D26ACB-E247-47B7-961E-AFA7B3D7E111"),
                    NetAmount = 350000,
                });
            }
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Add(surcharges);
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
                IQueryable<CsTransaction> docTransaction = csTransactionRepository.Get().ToLookup(x => x.JobNo)[jobNo].AsQueryable();
                if (docTransaction != null && docTransaction.Count() > 0)
                {
                    transactionType = docTransaction?.FirstOrDefault()?.TransactionType;
                }
                else
                {
                    IQueryable<OpsTransaction> opsTransaction = opsTransRepository.Get().ToLookup(x => x.JobNo)[jobNo].AsQueryable();
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

        /// <summary>
        /// Get custom no old of shipment
        /// </summary>
        /// <param name="jobNo"></param>
        /// <returns></returns>
        private string GetCustomNoOldOfShipment(string jobNo)
        {
            var LookupCustomDeclaration = customsDeclarationRepository.Get().ToLookup(x => x.JobNo);
            var customNos = LookupCustomDeclaration[jobNo].OrderBy(o => o.DatetimeModified).FirstOrDefault()?.ClearanceNo;
            return customNos;
        }

        public HandleState UpdateChargeLinkFee(List<CsShipmentSurchargeModel> list)
        {
            var result = new HandleState();
            var surChargeUp = new CsShipmentSurcharge();
            var surchargesUpdate = new List<CsShipmentSurcharge>();
            var surchargesAddBuy = new List<CsShipmentSurcharge>();

            var shipment = opsTransRepository.Get(x => x.JobNo == list[0].JobNo).FirstOrDefault();
            if (shipment != null)
                shipment.IsLinkFee = list.Where(x => x.LinkFee == true).FirstOrDefault() != null ? true : false;

            foreach (var item in list)
            {
                var rules = csRuleLinkFeeRepository.Get().ToList();
                var rule = rules.Where(x => x.Status == true
                && x.ServiceSelling == item.TransactionType
                && x.ChargeSelling.ToLower() == item.ChargeId.ToString().ToLower()
                && x.PartnerSelling.ToLower() == item.PaymentObjectId.ToLower()).FirstOrDefault();

                if (rule == null)
                    return new HandleState(false, "There is no link fee rule ");

                var charge = DataContext.Get(x => x.Id == item.Id).FirstOrDefault();
                CsShipmentSurcharge chargeBuy = mapper.Map<CsShipmentSurcharge>(charge);
                CsShipmentSurcharge chargeUpdate = mapper.Map<CsShipmentSurcharge>(item);

                //Update charge selling 
                chargeUpdate.LinkFee = item.LinkFee;
                chargeUpdate.ModifiedDateLinkFee = DateTime.Now;
                chargeUpdate.UserIdLinkFee = new Guid(currentUser.UserID);
                chargeUpdate.UserNameLinkFee = currentUser.UserName;

                surchargesUpdate.Add(chargeUpdate);

                //Add charge type buy
                chargeBuy.Id = Guid.NewGuid();
                chargeBuy.Type = DocumentConstants.CHARGE_BUY_TYPE;
                chargeBuy.TransactionType = rule.ServiceBuying;
                chargeBuy.ChargeId = Guid.Parse(rule.ChargeBuying);
                chargeBuy.JobNo = shipment.ServiceNo;
                chargeBuy.LinkFee = true;

                //Nếu HBL từ link nội bộ null
                if (shipment.ServiceHblId == null)
                {
                    var jobTrans = csTransactionRepository.Get(x => x.JobNo == shipment.ServiceNo).FirstOrDefault();
                    if (jobTrans != null)
                    {
                        var hbl = tranDetailRepository.Get(x => x.JobId == jobTrans.Id).FirstOrDefault();
                        chargeBuy.Hblid = hbl != null ? hbl.Id : new Guid();
                        chargeBuy.Hblno = hbl != null ? hbl.Hwbno : null;
                    }
                }
                else
                    chargeBuy.Hblid = shipment.ServiceHblId ?? new Guid();

                surchargesAddBuy.Add(chargeBuy);
            }

            if (!result.Success)
                return result;

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    if (surchargesUpdate.Count() > 0)
                    {
                        foreach (var surchargeUpdate in surchargesUpdate)
                        {
                            var hsUpdate = DataContext.Update(surchargeUpdate, x => x.Id == surchargeUpdate.Id, false);
                        }
                    }

                    if (surchargesAddBuy.Count() > 0)
                        DataContext.Add(surchargesAddBuy);

                    opsTransRepository.Update(shipment, x => x.Id == shipment.Id, false);
                    opsTransRepository.SubmitChanges();
                    result = DataContext.SubmitChanges();
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    new LogHelper("CsShipmentSurChargeLog", ex.ToString());
                    result = new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
                return result;
            }
        }

        public HandleState RevertChargeLinkFee(List<CsShipmentSurchargeModel> list)
        {
            var result = new HandleState();

            var surchargesUpdate = new List<CsShipmentSurcharge>();

            var shipment = opsTransRepository.Get(x => x.JobNo == list[0].JobNo).FirstOrDefault();
            if (shipment != null)
                shipment.IsLinkFee = list.Where(x => x.LinkFee == true).FirstOrDefault() != null ? true : false;

            foreach (var i in list)
            {
                i.ModifiedDateLinkFee = DateTime.Now;
                surchargesUpdate.Add(i);
            }

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    if (surchargesUpdate.Count() > 0)
                    {
                        foreach (var surchargeUpdate in surchargesUpdate)
                        {
                            var hsUpdate = DataContext.Update(surchargeUpdate, x => x.Id == surchargeUpdate.Id, false);
                        }
                    }

                    opsTransRepository.Update(shipment, x => x.Id == shipment.Id, false);
                    opsTransRepository.SubmitChanges();
                    result = DataContext.SubmitChanges();
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    new LogHelper("RevertChargeLinkFeeLog", ex.ToString());
                    result = new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
                return result;
            }
        }

        public List<AmountSurchargeResult> GetAmountSurchargeResult(List<Guid> Ids)
        {
            List<AmountSurchargeResult> results = new List<AmountSurchargeResult>();

            var surcharges = DataContext.Get(x => Ids.Contains(x.Id));
            foreach (var item in surcharges)
            {
                var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(item, 20000);
                results.Add(amountSurcharge);
            }

            return results;
        }
        public bool CheckExistRefundFee(Guid jobId, string transactionType)
        {
            bool result = false;
            var surcharges = Enumerable.Empty<CsShipmentSurcharge>().AsQueryable();
            if (transactionType == TermData.OpsTransaction)
            {
                OpsTransaction ops = opsTransRepository.Get(x => x.Id == jobId).FirstOrDefault();
                surcharges = DataContext.Get(x => x.Hblid == ops.Hblid);
            }
            else
            {
                List<Guid> hblIds = tranDetailRepository.Get(x => x.JobId == jobId).Select(x => x.Id).ToList();
                surcharges = DataContext.Get(x => hblIds.Contains(x.Hblid));
            }

            result = surcharges.Any(x => x.IsRefundFee == true);

            return result;
        }

    }
}

