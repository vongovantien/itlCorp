using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Service.Contexts;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Accounting.Service.ViewModels;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace eFMS.API.Accounting.DL.Services
{
    public class AcctSOAService : RepositoryBase<AcctSoa, AcctSoaModel>, IAcctSOAService
    {
        private readonly ICurrentUser currentUser;
        readonly IContextBase<CsShipmentSurcharge> csShipmentSurchargeRepo;
        readonly IContextBase<CatCurrencyExchange> catCurrencyExchangeRepo;
        readonly IContextBase<OpsTransaction> opsTransactionRepo;
        readonly IContextBase<CsTransaction> csTransactionRepo;
        readonly IContextBase<CsTransactionDetail> csTransactionDetailRepo;
        readonly IContextBase<CatCharge> catChargeRepo;
        readonly IContextBase<CatUnit> catUnitRepo;
        readonly IContextBase<CustomsDeclaration> customsDeclarationRepo;
        readonly IContextBase<AcctCdnote> acctCdnoteRepo;
        readonly IContextBase<CatPartner> catPartnerRepo;
        public AcctSOAService(IContextBase<AcctSoa> repository,
            IMapper mapper,
            ICurrentUser user,
            IContextBase<CsShipmentSurcharge> csShipmentSurcharge,
            IContextBase<CatCurrencyExchange> catCurrencyExchange,
            IContextBase<OpsTransaction> opsTransaction,
            IContextBase<CsTransaction> csTransaction,
            IContextBase<CsTransactionDetail> csTransactionDetail,
            IContextBase<CatCharge> catCharge,
            IContextBase<CatUnit> catUnit,
            IContextBase<CustomsDeclaration> customsDeclaration,
            IContextBase<AcctCdnote> acctCdnote,
            IContextBase<CatPartner> catPartner) : base(repository, mapper)
        {
            currentUser = user;
            csShipmentSurchargeRepo = csShipmentSurcharge;
            catCurrencyExchangeRepo = catCurrencyExchange;
            opsTransactionRepo = opsTransaction;
            csTransactionRepo = csTransaction;
            csTransactionDetailRepo = csTransactionDetail;
            catChargeRepo = catCharge;
            catUnitRepo = catUnit;
            customsDeclarationRepo = customsDeclaration;
            acctCdnoteRepo = acctCdnote;
            catPartnerRepo = catPartner;
        }

        public HandleState AddSOA(AcctSoaModel model)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var soa = mapper.Map<AcctSoa>(model);
                soa.Soano = model.Soano = CreateSoaNo();
                //var hs = dc.AcctSoa.Add(soa);
                var hs = DataContext.Add(soa);

                if (hs.Success)
                {
                    //Lấy ra những charge có type là BUY hoặc OBH-BUY mà chưa tồn tại trong 1 SOA nào cả
                    var surchargeCredit = csShipmentSurchargeRepo.Get(x => model.Surcharges != null
                                                                   && model.Surcharges.Any(c => c.surchargeId == x.Id && (c.type == Constants.TYPE_CHARGE_BUY || c.type == Constants.TYPE_CHARGE_OBH_BUY))
                                                                   && (x.Soano == null || x.Soano == "")).ToList();

                    //Lấy ra những charge có type là SELL hoặc OBH-SELL mà chưa tồn tại trong 1 SOA nào cả
                    var surchargeDebit = csShipmentSurchargeRepo.Get(x => model.Surcharges != null
                                                                   && model.Surcharges.Any(c => c.surchargeId == x.Id && (c.type == Constants.TYPE_CHARGE_SELL || c.type == Constants.TYPE_CHARGE_OBH_SELL))
                                                                   && (x.Soano == null || x.Soano == "")).ToList();

                    if (surchargeCredit.Count() > 0)
                    {
                        //Update PaySOANo cho CsShipmentSurcharge có type BUY hoặc OBH-BUY(Payer)
                        //Change request: Cập nhật lại ngày ExchangeDate (23/09/2019)
                        surchargeCredit.ForEach(a =>
                            {
                                a.PaySoano = soa.Soano;
                                a.UserModified = currentUser.UserID;
                                a.DatetimeModified = a.ExchangeDate = DateTime.Now;
                            }
                        );
                        dc.CsShipmentSurcharge.UpdateRange(surchargeCredit);
                    }

                    if (surchargeDebit.Count() > 0)
                    {
                        //Update SOANo cho CsShipmentSurcharge có type là SELL hoặc OBH-SELL(Receiver)
                        //Change request: Cập nhật lại ngày ExchangeDate (23/09/2019)
                        surchargeDebit.ForEach(a =>
                            {
                                a.Soano = soa.Soano;
                                a.UserModified = currentUser.UserID;
                                a.DatetimeModified = a.ExchangeDate = DateTime.Now;
                            }
                        );
                        dc.CsShipmentSurcharge.UpdateRange(surchargeDebit);
                    }
                }
                dc.SaveChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        private string CreateSoaNo()
        {
            var prefix = (DateTime.Now.Year.ToString()).Substring(2, 2);
            string stt;
            //Lấy ra dòng cuối cùng của table acctSOA
            var rowLast = DataContext.Get().LastOrDefault(); //dc.AcctSoa.LastOrDefault();
            if (rowLast == null)
            {
                stt = "00001";
            }
            else
            {
                var soaCurrent = rowLast.Soano;
                var prefixCurrent = soaCurrent.Substring(0, 2);
                //Reset về 1 khi qua năm mới
                if (prefixCurrent != prefix)
                {
                    stt = "00001";
                }
                else
                {
                    stt = (Convert.ToInt32(soaCurrent.Substring(2, 5)) + 1).ToString();
                    stt = stt.PadLeft(5, '0');
                }
            }
            return prefix + stt;
        }

        public IQueryable<AcctSOAResult> Paging(AcctSOACriteria criteria, int page, int size, out int rowsCount)
        {
            var data = GetListSOA(criteria); //GetListAcctSOA(criteria).AsQueryable();
            rowsCount = (data.Count() > 0) ? data.Count() : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }

            //var dataMap = mapper.Map<List<spc_GetListAcctSOAByMaster>, List<AcctSOAResult>>(data.ToList());
            //return dataMap;
            return data;
        }

        private List<spc_GetListAcctSOAByMaster> GetListAcctSOA(AcctSOACriteria criteria)
        {
            DbParameter[] parameters =
            {
                SqlParam.GetParameter("strCodes", criteria.StrCodes),
                SqlParam.GetParameter("customerID", criteria.CustomerID),
                SqlParam.GetParameter("soaFromDateCreate", criteria.SoaFromDateCreate),
                SqlParam.GetParameter("soaToDateCreate", criteria.SoaToDateCreate),
                SqlParam.GetParameter("soaStatus", criteria.SoaStatus),
                SqlParam.GetParameter("soaCurrency", criteria.SoaCurrency),
                SqlParam.GetParameter("soaUserCreate", criteria.SoaUserCreate),
                SqlParam.GetParameter("currencyLocal", criteria.CurrencyLocal)
            };
            return ((eFMSDataContext)DataContext.DC).ExecuteProcedure<spc_GetListAcctSOAByMaster>(parameters);
        }

        public HandleState UpdateSOASurCharge(string soaNo)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var surcharge = csShipmentSurchargeRepo.Get(x => x.Soano == soaNo || x.PaySoano == soaNo).ToList();
                if (surcharge.Count() > 0)
                {
                    //Update SOANo = NULL & PaySOANo = NULL to CsShipmentSurcharge
                    surcharge.ForEach(a =>
                        {
                            a.Soano = null;
                            a.PaySoano = null;
                            a.UserModified = currentUser.UserID;
                            a.DatetimeModified = DateTime.Now;
                        }
                    );
                    dc.CsShipmentSurcharge.UpdateRange(surcharge);
                    dc.SaveChanges();
                }

                return new HandleState();
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        public AcctSOADetailResult GetBySoaNoAndCurrencyLocal(string soaNo, string currencyLocal)
        {
            var criteria = new AcctSOACriteria
            {
                StrCodes = soaNo,
                CurrencyLocal = currencyLocal
            };
            var dataSOA = GetListAcctSOA(criteria).AsQueryable();
            var dataMapSOA = mapper.Map<spc_GetListAcctSOAByMaster, Models.AcctSOADetailResult>(dataSOA.FirstOrDefault());

            var chargeShipmentList = GetSpcChargeShipmentBySOANo(soaNo, currencyLocal).ToList();
            var dataMapChargeShipment = mapper.Map<List<spc_GetListChargeShipmentMasterBySOANo>, List<ChargeShipmentModel>>(chargeShipmentList);

            dataMapSOA.ChargeShipments = dataMapChargeShipment;
            dataMapSOA.AmountDebitLocal = Math.Round(chargeShipmentList.Sum(x => x.AmountDebitLocal), 3);
            dataMapSOA.AmountCreditLocal = Math.Round(chargeShipmentList.Sum(x => x.AmountCreditLocal), 3);
            dataMapSOA.AmountDebitUSD = Math.Round(chargeShipmentList.Sum(x => x.AmountDebitUSD), 3);
            dataMapSOA.AmountCreditUSD = Math.Round(chargeShipmentList.Sum(x => x.AmountCreditUSD), 3);

            //Thông tin các Service Name của SOA
            dataMapSOA.ServicesNameSoa = GetInfoServiceOfSoa(soaNo).ToString();

            return dataMapSOA;
        }

        private List<spc_GetListChargeShipmentMasterBySOANo> GetSpcChargeShipmentBySOANo(string soaNo, string currencyLocal)
        {
            DbParameter[] parameters =
            {
                SqlParam.GetParameter("currencyLocal", currencyLocal),
                SqlParam.GetParameter("soaNo", soaNo)
            };
            return ((eFMSDataContext)DataContext.DC).ExecuteProcedure<spc_GetListChargeShipmentMasterBySOANo>(parameters);
        }

        public object GetListServices()
        {
            return CustomData.Services;
        }

        public object GetListStatusSoa()
        {
            return CustomData.StatusSoa;
        }

        /// <summary>
        /// Lấy thông tin các ServiceName của SOA
        /// </summary>
        /// <param name="soaNo">SOANo</param>
        /// <returns></returns>
        public string GetInfoServiceOfSoa(string soaNo)
        {
            var serviceTypeId = DataContext.Get(x => x.Soano == soaNo).FirstOrDefault()?.ServiceTypeId;

            var serviceName = "";

            if (!string.IsNullOrEmpty(serviceTypeId))
            {
                //Tách chuỗi servicetype thành mảng
                string[] arrayStrServiceTypeId = serviceTypeId.Split(';').Where(x => x.ToString() != "").ToArray();

                //Xóa các serviceTypeId trùng
                string[] arrayGrpServiceTypeId = arrayStrServiceTypeId.Distinct<string>().ToArray();

                var serviceId = "";
                foreach (var item in arrayGrpServiceTypeId)
                {
                    //Lấy ra DisplayName của serviceTypeId
                    serviceName += CustomData.Services.Where(x => x.Value == item).FirstOrDefault() != null ?
                                CustomData.Services.Where(x => x.Value == item).FirstOrDefault().DisplayName.Trim() + ";"
                                : "";
                    serviceId += item + ";";
                }
                serviceName = (serviceName + ")").Replace(";)", "");
            }
            return serviceName;
        }

        public HandleState UpdateSOA(AcctSoaModel model)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;

                //Gỡ bỏ các charge có SOANo = model.Soano và PaySOANo = model.Soano
                UpdateSOASurCharge(model.Soano);

                var soa = mapper.Map<AcctSoa>(model);
                //Update các thông tin của SOA
                //var hs = dc.AcctSoa.Update(soa);
                var hs = DataContext.Update(soa, x => x.Id == soa.Id);

                if (hs.Success)
                {
                    //Lấy ra những charge có type là BUY hoặc OBH-BUY mà chưa tồn tại trong 1 SOA nào cả
                    var surchargeCredit = csShipmentSurchargeRepo.Get(x => model.Surcharges != null
                                                                   && model.Surcharges.Any(c => c.surchargeId == x.Id && (c.type == Constants.TYPE_CHARGE_BUY || c.type == Constants.TYPE_CHARGE_OBH_BUY))
                                                                   && (x.Soano == null || x.Soano == "")).ToList();

                    //Lấy ra những charge có type là SELL hoặc OBH-SELL mà chưa tồn tại trong 1 SOA nào cả
                    var surchargeDebit = csShipmentSurchargeRepo.Get(x => model.Surcharges != null
                                                                   && model.Surcharges.Any(c => c.surchargeId == x.Id && (c.type == Constants.TYPE_CHARGE_SELL || c.type == Constants.TYPE_CHARGE_OBH_SELL))
                                                                   && (x.Soano == null || x.Soano == "")).ToList();

                    if (surchargeCredit.Count() > 0)
                    {
                        //Update PaySOANo cho CsShipmentSurcharge có type BUY hoặc OBH-BUY(Payer)
                        //Change request: Cập nhật lại ngày ExchangeDate (23/09/2019)
                        surchargeCredit.ForEach(a =>
                        {
                            a.PaySoano = soa.Soano;
                            a.UserModified = currentUser.UserID;
                            a.DatetimeModified = a.ExchangeDate = DateTime.Now;
                        }
                        );
                        dc.CsShipmentSurcharge.UpdateRange(surchargeCredit);
                    }

                    if (surchargeDebit.Count() > 0)
                    {
                        //Update SOANo cho CsShipmentSurcharge có type là SELL hoặc OBH-SELL(Receiver)
                        //Change request: Cập nhật lại ngày ExchangeDate (23/09/2019)
                        surchargeDebit.ForEach(a =>
                        {
                            a.Soano = soa.Soano;
                            a.UserModified = currentUser.UserID;
                            a.DatetimeModified = a.ExchangeDate = DateTime.Now;
                        }
                        );
                        dc.CsShipmentSurcharge.UpdateRange(surchargeDebit);
                    }

                }
                dc.SaveChanges();

                return new HandleState();
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        public List<ChargeShipmentModel> GetListMoreChargeByCondition(MoreChargeShipmentCriteria criteria)
        {
            var moreChargeShipmentList = GetSpcMoreChargeShipmentByCondition(criteria);

            List<Surcharge> Surcharges = new List<Surcharge>();
            if (criteria.ChargeShipments != null)
            {
                foreach (var item in criteria.ChargeShipments.Where(x => x.SOANo == null || x.SOANo == "").ToList())
                {
                    Surcharges.Add(new Surcharge { surchargeId = item.ID, type = item.Type });
                }
            }

            //Lấy ra các charge chưa tồn tại trong list criteria.Surcharges(Các Id của charge đã có trong kết quả search ở form info)
            List<spc_GetListMoreChargeMasterByCondition> charges = new List<spc_GetListMoreChargeMasterByCondition>();
            charges = moreChargeShipmentList.Where(x => Surcharges != null
                                                         && !Surcharges.Where(c => c.surchargeId == x.ID && c.type == x.Type).Any()).ToList();
            var dataMapMoreChargeShipment = mapper.Map<List<spc_GetListMoreChargeMasterByCondition>, List<ChargeShipmentModel>>(charges);

            return dataMapMoreChargeShipment;
        }

        private List<spc_GetListMoreChargeMasterByCondition> GetSpcMoreChargeShipmentByCondition(MoreChargeShipmentCriteria criteria)
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
                SqlParam.GetParameter("inSOA", criteria.InSoa),
                SqlParam.GetParameter("jobId", criteria.JobId),
                SqlParam.GetParameter("hbl", criteria.Hbl),
                SqlParam.GetParameter("mbl", criteria.Mbl),
                SqlParam.GetParameter("cdNote", criteria.CDNote),
                SqlParam.GetParameter("commodityGroupID", criteria.CommodityGroupID),
                SqlParam.GetParameter("strServices", criteria.StrServices)
            };
            return ((eFMSDataContext)DataContext.DC).ExecuteProcedure<spc_GetListMoreChargeMasterByCondition>(parameters);
        }

        public AcctSOADetailResult AddMoreCharge(AddMoreChargeCriteria criteria)
        {
            var data = new AcctSOADetailResult();
            if (criteria != null)
            {
                if (criteria.ChargeShipmentsCurrent != null)
                {
                    if (criteria.ChargeShipmentsAddMore != null)
                    {
                        foreach (var item in criteria.ChargeShipmentsAddMore)
                        {
                            criteria.ChargeShipmentsCurrent.Add(item);
                        }
                    }
                    data.Shipment = criteria.ChargeShipmentsCurrent.Where(x => x.HBL != null).GroupBy(x => x.HBL).Count();
                    data.TotalCharge = criteria.ChargeShipmentsCurrent.Count();
                    data.ChargeShipments = criteria.ChargeShipmentsCurrent;
                    data.AmountDebitLocal = criteria.ChargeShipmentsCurrent.Sum(x => x.AmountDebitLocal);
                    data.AmountCreditLocal = criteria.ChargeShipmentsCurrent.Sum(x => x.AmountCreditLocal);
                    data.AmountDebitUSD = criteria.ChargeShipmentsCurrent.Sum(x => x.AmountDebitUSD);
                    data.AmountCreditUSD = criteria.ChargeShipmentsCurrent.Sum(x => x.AmountCreditUSD);
                }
            }
            return data;
        }

        public ExportSOADetailResult GetDataExportSOABySOANo(string soaNo, string currencyLocal)
        {
            var data = GetDateExportDetailSOA(soaNo);//GetSpcDataExportSOABySOANo(soaNo, currencyLocal);
            //var dataMap = mapper.Map<List<spc_GetDataExportSOABySOANo>, List<ExportSOAModel>>(data);
            var result = new ExportSOADetailResult
            {
                ListCharges = data.ToList(),
                TotalDebitExchange = data.Where(x => x.DebitExchange != null).Sum(x => x.DebitExchange),
                TotalCreditExchange = data.Where(x => x.CreditExchange != null).Sum(x => x.CreditExchange)
            };
            return result;
        }

        private List<spc_GetDataExportSOABySOANo> GetSpcDataExportSOABySOANo(string soaNo, string currencyLocal)
        {
            DbParameter[] parameters =
            {
                SqlParam.GetParameter("soaNo", soaNo),
                SqlParam.GetParameter("currencyLocal", currencyLocal)
            };
            return ((eFMSDataContext)DataContext.DC).ExecuteProcedure<spc_GetDataExportSOABySOANo>(parameters);
        }

        #region -- --
        private decimal GetRateLatestCurrencyExchange(List<CatCurrencyExchange> currencyExchange, string currencyFrom, string currencyTo)
        {
            if (currencyExchange.Count == 0) return 0;

            currencyFrom = !string.IsNullOrEmpty(currencyFrom) ? currencyFrom.Trim() : currencyFrom;
            currencyTo = !string.IsNullOrEmpty(currencyTo) ? currencyTo.Trim() : currencyTo;

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

        private decimal GetRateCurrencyExchange(DateTime? datetime, string currencyFrom, string currencyTo)
        {
            if (datetime == null) return 0;
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == datetime.Value.Date);

            if (currencyExchange.Count() == 0) return 0;

            currencyFrom = !string.IsNullOrEmpty(currencyFrom) ? currencyFrom.Trim() : currencyFrom;
            currencyTo = !string.IsNullOrEmpty(currencyTo) ? currencyTo.Trim() : currencyTo;

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

        #region -- Get Data Charge Master --
        private IQueryable<ChargeSOAResult> GetChargeBuySell()
        {
            //Chỉ lấy những phí từ shipment (IsFromShipment = true)
            var surcharge = csShipmentSurchargeRepo.Get(x => x.IsFromShipment == true && (x.Type == "BUY" || x.Type == "SELL"));
            var opst = opsTransactionRepo.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != null && x.CurrentStatus != Constants.CURRENT_STATUS_CANCELED);
            var csTrans = csTransactionRepo.Get();
            var csTransDe = csTransactionDetailRepo.Get();
            var creditNote = acctCdnoteRepo.Get();
            var debitNote = acctCdnoteRepo.Get();

            //BUY & SELL
            var queryBuySell = from sur in surcharge
                               join ops in opst on sur.Hblid equals ops.Hblid into ops2
                               from ops in ops2.DefaultIfEmpty()
                               join cstd in csTransDe on sur.Hblid equals cstd.Id into cstd2
                               from cstd in cstd2.DefaultIfEmpty()
                               join cst in csTrans on cstd.JobId equals cst.Id into cst2
                               from cst in cst2.DefaultIfEmpty()
                               join creditN in creditNote on sur.CreditNo equals creditN.Code into creditN2
                               from creditN in creditN2.DefaultIfEmpty()
                               join debitN in debitNote on sur.DebitNo equals debitN.Code into debitN2
                               from debitN in debitN2.DefaultIfEmpty()
                               select new ChargeSOAResult
                               {
                                   ID = sur.Id,
                                   HBLID = sur.Hblid,
                                   ChargeID = sur.ChargeId,
                                   JobId = (cst.JobNo != null ? cst.JobNo : ops.JobNo),
                                   HBL = (cstd.Hwbno != null ? cstd.Hwbno : ops.Hwbno),
                                   MBL = (cstd.Mawb != null ? cstd.Mawb : ops.Mblno),
                                   Type = sur.Type,
                                   Debit = sur.Type == "SELL" ? (decimal?)sur.Total : null,
                                   Credit = sur.Type == "BUY" ? (decimal?)sur.Total : null,
                                   SOANo = sur.Type == "SELL" ? sur.Soano : sur.PaySoano,
                                   IsOBH = false,
                                   Currency = sur.CurrencyId,
                                   InvoiceNo = sur.InvoiceNo,
                                   Note = sur.Notes,
                                   CustomerID = sur.PaymentObjectId,
                                   ServiceDate = ops.Hblid == sur.Hblid ? ops.ServiceDate :
                                   (cst.TransactionType == "AI" || cst.TransactionType == "SFI" || cst.TransactionType == "SLI" || cst.TransactionType == "SCI" ? cst.Eta : cst.Etd),
                                   CreatedDate = ops.Hblid == sur.Hblid ? ops.CreatedDate : cst.CreatedDate,
                                   InvoiceIssuedDate = sur.Type == "SELL" ? debitN.DatetimeCreated : creditN.DatetimeCreated,
                                   TransactionType = cst.TransactionType,
                                   UserCreated = ops.Hblid == sur.Hblid ? ops.UserCreated : cst.UserCreated,
                                   Quantity = sur.Quantity,
                                   UnitId = sur.UnitId,
                                   UnitPrice = sur.UnitPrice,
                                   VATRate = sur.Vatrate,
                                   CreditDebitNo = sur.Type == "SELL" ? sur.DebitNo : sur.CreditNo,
                                   DatetimeModified = sur.DatetimeModified,
                                   CommodityGroupID = ops.CommodityGroupId,
                                   Service = ops.Hblid == sur.Hblid ? "CL" : cst.TransactionType
                               };
            queryBuySell = queryBuySell.Where(x => !string.IsNullOrEmpty(x.Service));
            return queryBuySell;
        }

        private IQueryable<ChargeSOAResult> GetChargeOBHSell()
        {
            //Chỉ lấy những phí từ shipment (IsFromShipment = true)
            var surcharge = csShipmentSurchargeRepo.Get(x => x.IsFromShipment == true && x.Type == "OBH");
            var opst = opsTransactionRepo.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != null && x.CurrentStatus != Constants.CURRENT_STATUS_CANCELED);
            var csTrans = csTransactionRepo.Get();
            var csTransDe = csTransactionDetailRepo.Get();
            var debitNote = acctCdnoteRepo.Get();
            //OBH Receiver (SELL - Credit)
            var queryObhSell = from sur in surcharge
                               join ops in opst on sur.Hblid equals ops.Hblid into ops2
                               from ops in ops2.DefaultIfEmpty()
                               join cstd in csTransDe on sur.Hblid equals cstd.Id into cstd2
                               from cstd in cstd2.DefaultIfEmpty()
                               join cst in csTrans on cstd.JobId equals cst.Id into cst2
                               from cst in cst2.DefaultIfEmpty()
                               join debitN in debitNote on sur.DebitNo equals debitN.Code into debitN2
                               from debitN in debitN2.DefaultIfEmpty()
                               select new ChargeSOAResult
                               {
                                   ID = sur.Id,
                                   HBLID = sur.Hblid,
                                   ChargeID = sur.ChargeId,
                                   JobId = (cst.JobNo != null ? cst.JobNo : ops.JobNo),
                                   HBL = (cstd.Hwbno != null ? cstd.Hwbno : ops.Hwbno),
                                   MBL = (cstd.Mawb != null ? cstd.Mawb : ops.Mblno),
                                   Type = sur.Type + "-SELL",
                                   Debit = sur.Total,
                                   Credit = null,
                                   SOANo = sur.Soano,
                                   IsOBH = true,
                                   Currency = sur.CurrencyId,
                                   InvoiceNo = sur.InvoiceNo,
                                   Note = sur.Notes,
                                   CustomerID = sur.PaymentObjectId,
                                   ServiceDate = ops.Hblid == sur.Hblid ? ops.ServiceDate :
                                   (cst.TransactionType == "AI" || cst.TransactionType == "SFI" || cst.TransactionType == "SLI" || cst.TransactionType == "SCI" ? cst.Eta : cst.Etd),
                                   CreatedDate = ops.Hblid == sur.Hblid ? ops.CreatedDate : cst.CreatedDate,
                                   InvoiceIssuedDate = debitN.DatetimeCreated,
                                   TransactionType = cst.TransactionType,
                                   UserCreated = ops.Hblid == sur.Hblid ? ops.UserCreated : cst.UserCreated,
                                   Quantity = sur.Quantity,
                                   UnitId = sur.UnitId,
                                   UnitPrice = sur.UnitPrice,
                                   VATRate = sur.Vatrate,
                                   CreditDebitNo = sur.DebitNo,
                                   DatetimeModified = sur.DatetimeModified,
                                   CommodityGroupID = ops.CommodityGroupId,
                                   Service = ops.Hblid == sur.Hblid ? "CL" : cst.TransactionType
                               };
            queryObhSell = queryObhSell.Where(x => !string.IsNullOrEmpty(x.Service));
            return queryObhSell;
        }

        private IQueryable<ChargeSOAResult> GetChargeOBHBuy()
        {
            //Chỉ lấy những phí từ shipment (IsFromShipment = true)
            var surcharge = csShipmentSurchargeRepo.Get(x => x.IsFromShipment == true && x.Type == "OBH");
            var opst = opsTransactionRepo.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != null && x.CurrentStatus != Constants.CURRENT_STATUS_CANCELED);
            var csTrans = csTransactionRepo.Get();
            var csTransDe = csTransactionDetailRepo.Get();
            var custom = customsDeclarationRepo.Get();
            var creditNote = acctCdnoteRepo.Get();
            //OBH Payer (BUY - Credit)
            var queryObhBuy = from sur in surcharge
                              join ops in opst on sur.Hblid equals ops.Hblid into ops2
                              from ops in ops2.DefaultIfEmpty()
                              join cstd in csTransDe on sur.Hblid equals cstd.Id into cstd2
                              from cstd in cstd2.DefaultIfEmpty()
                              join cst in csTrans on cstd.JobId equals cst.Id into cst2
                              from cst in cst2.DefaultIfEmpty()
                              join creditN in creditNote on sur.CreditNo equals creditN.Code into creditN2
                              from creditN in creditN2.DefaultIfEmpty()
                              select new ChargeSOAResult
                              {
                                  ID = sur.Id,
                                  HBLID = sur.Hblid,
                                  ChargeID = sur.ChargeId,
                                  JobId = (cst.JobNo != null ? cst.JobNo : ops.JobNo),
                                  HBL = (cstd.Hwbno != null ? cstd.Hwbno : ops.Hwbno),
                                  MBL = (cstd.Mawb != null ? cstd.Mawb : ops.Mblno),
                                  Type = sur.Type + "-BUY",
                                  Debit = null,
                                  Credit = sur.Total,
                                  SOANo = sur.PaySoano,
                                  IsOBH = true,
                                  Currency = sur.CurrencyId,
                                  InvoiceNo = sur.InvoiceNo,
                                  Note = sur.Notes,
                                  CustomerID = sur.PayerId,
                                  ServiceDate = ops.Hblid == sur.Hblid ? ops.ServiceDate :
                                  (cst.TransactionType == "AI" || cst.TransactionType == "SFI" || cst.TransactionType == "SLI" || cst.TransactionType == "SCI" ? cst.Eta : cst.Etd),
                                  CreatedDate = ops.Hblid == sur.Hblid ? ops.CreatedDate : cst.CreatedDate,
                                  InvoiceIssuedDate = creditN.DatetimeCreated,
                                  TransactionType = cst.TransactionType,
                                  UserCreated = ops.Hblid == sur.Hblid ? ops.UserCreated : cst.UserCreated,
                                  Quantity = sur.Quantity,
                                  UnitId = sur.UnitId,
                                  UnitPrice = sur.UnitPrice,
                                  VATRate = sur.Vatrate,
                                  CreditDebitNo = sur.CreditNo,
                                  DatetimeModified = sur.DatetimeModified,
                                  CommodityGroupID = ops.CommodityGroupId,
                                  Service = ops.Hblid == sur.Hblid ? "CL" : cst.TransactionType
                              };
            queryObhBuy = queryObhBuy.Where(x => !string.IsNullOrEmpty(x.Service));
            return queryObhBuy;
        }

        private string GetTopClearanceNoByJobNo(string JobNo)
        {
            var custom = customsDeclarationRepo.Get();
            var clearanceNo = custom.Where(x => x.JobNo != null && x.JobNo == JobNo)
                .OrderBy(x => x.JobNo)
                .OrderByDescending(x => x.ClearanceDate)
                .FirstOrDefault()?.ClearanceNo;
            return clearanceNo;
        }

        public IQueryable<ChargeSOAResult> GetChargeShipmentDocAndOperation()
        {
            var charge = catChargeRepo.Get();
            var unit = catUnitRepo.Get();

            //BUY & SELL
            var queryBuySell = GetChargeBuySell();

            //OBH Receiver (SELL - Credit)
            var queryObhSell = GetChargeOBHSell();

            //OBH Payer (BUY - Credit)
            var queryObhBuy = GetChargeOBHBuy();

            var dataMerge = queryBuySell.Union(queryObhBuy).Union(queryObhSell);

            var queryData = from data in dataMerge
                            join chg in charge on data.ChargeID equals chg.Id into chg2
                            from chg in chg2.DefaultIfEmpty()
                            join uni in unit on data.UnitId equals uni.Id into uni2
                            from uni in uni2.DefaultIfEmpty()
                            select new ChargeSOAResult
                            {
                                ID = data.ID,
                                HBLID = data.HBLID,
                                ChargeID = data.ChargeID,
                                ChargeCode = chg.Code,
                                ChargeName = chg.ChargeNameEn,
                                JobId = data.JobId,
                                HBL = data.HBL,
                                MBL = data.MBL,
                                Type = data.Type,
                                Debit = data.Debit,
                                Credit = data.Credit,
                                SOANo = data.SOANo,
                                IsOBH = data.IsOBH,
                                Currency = data.Currency.Trim(),
                                InvoiceNo = data.InvoiceNo,
                                Note = data.Note,
                                CustomerID = data.CustomerID,
                                ServiceDate = data.ServiceDate,
                                CreatedDate = data.CreatedDate,
                                InvoiceIssuedDate = data.InvoiceIssuedDate,
                                TransactionType = data.TransactionType,
                                UserCreated = data.UserCreated,
                                Quantity = data.Quantity,
                                UnitId = data.UnitId,
                                Unit = uni.UnitNameEn,
                                UnitPrice = data.UnitPrice,
                                VATRate = data.VATRate,
                                CreditDebitNo = data.CreditDebitNo,
                                DatetimeModified = data.DatetimeModified,
                                CommodityGroupID = data.CommodityGroupID,
                                Service = data.Service,
                                CustomNo = GetTopClearanceNoByJobNo(data.JobId)
                            };

            return queryData.OrderBy(x => x.Service);
        }
        #endregion -- Get Data Charge Master --

        #region -- Get List Charges Shipment By Criteria --
        private IQueryable<ChargeShipmentModel> GetChargesShipmentByCriteria(ChargeShipmentCriteria criteria)
        {
            //Lấy danh sách Currency Exchange của ngày hiện tại
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();

            var charge = GetChargeShipmentDocAndOperation().Where(chg =>
                    string.IsNullOrEmpty(chg.SOANo)
                && chg.CustomerID == criteria.CustomerID
                && chg.IsOBH == (criteria.IsOBH == true ? chg.IsOBH : criteria.IsOBH)
            );

            if(string.IsNullOrEmpty(criteria.DateType) || criteria.DateType == "CreatedDate")
            {
                charge = charge.Where(chg =>
                    chg.CreatedDate.HasValue ? chg.CreatedDate.Value.Date  >= criteria.FromDate.Date && chg.CreatedDate.Value.Date <= criteria.ToDate.Date : 1 == 2
                );
            }
            else if(criteria.DateType == "ServiceDate")
            {
                charge = charge.Where(chg =>
                    chg.ServiceDate.HasValue ? chg.ServiceDate.Value.Date >= criteria.FromDate.Date && chg.ServiceDate.Value.Date <= criteria.ToDate.Date : 1 == 2
                );
            }
            else if (criteria.DateType == "InvoiceIssuedDate")
            {
                charge = charge.Where(chg =>
                    chg.InvoiceIssuedDate.HasValue ? chg.InvoiceIssuedDate.Value.Date >= criteria.FromDate.Date && chg.InvoiceIssuedDate.Value.Date <= criteria.ToDate.Date : 1 == 2
                );
            }

            if (!string.IsNullOrEmpty(criteria.Type))
            {
                charge = charge.Where(chg =>
                       (criteria.Type == "Debit" || chg.Type == "OBH-SELL") ? chg.Debit.HasValue : 
                       ((criteria.Type == "Credit" || chg.Type == "OBH-BUY") ? chg.Credit.HasValue : (chg.Debit.HasValue || chg.Credit.HasValue))
                );
            }

            if (!string.IsNullOrEmpty(criteria.StrCreators) || criteria.StrCreators == "All")
            {
                var listCreator = criteria.StrCreators.Split(',').Where(x => x.ToString() != "").ToList();
                charge = charge.Where(chg => listCreator.Contains(chg.UserCreated));
            }

            if (!string.IsNullOrEmpty(criteria.StrCharges) || criteria.StrCharges == "All")
            {
                var listCharge = criteria.StrCharges.Split(',').Where(x => x.ToString() != "").ToList();
                charge = charge.Where(chg => listCharge.Contains(chg.ChargeCode));
            }

            if(!string.IsNullOrEmpty(criteria.StrServices) || criteria.StrServices == "All")
            {
                var listService = criteria.StrServices.Split(',').Where(x => x.ToString() != "").ToList();
                charge = charge.Where(chg => listService.Contains(chg.Service));
            }

            if(criteria.CommodityGroupID != null)
            {
                charge = charge.Where(chg => criteria.CommodityGroupID == chg.CommodityGroupID);
            }

            var query = charge.Select(chg => new ChargeShipmentModel
                        {
                            ID = chg.ID,
                            ChargeCode = chg.ChargeCode,
                            ChargeName = chg.ChargeName,
                            JobId = chg.JobId,
                            HBL = chg.HBL,
                            MBL = chg.MBL,
                            CustomNo = chg.CustomNo,
                            Type = chg.Type,
                            InvoiceNo = chg.InvoiceNo,
                            ServiceDate = chg.ServiceDate,
                            Note = chg.Note,
                            Debit = chg.Debit,
                            Credit = chg.Credit,
                            Currency = chg.Currency,
                            CurrencyToLocal = criteria.CurrencyLocal,
                            CurrencyToUSD = Constants.CURRENCY_USD,
                            AmountDebitLocal =
                            (GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, criteria.CurrencyLocal) > 0
                            ?
                                GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, criteria.CurrencyLocal)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, criteria.CurrencyLocal)) * (chg.Debit != null ? chg.Debit.Value : 0),
                            AmountCreditLocal =
                            (GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, criteria.CurrencyLocal) > 0
                            ?
                                GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, criteria.CurrencyLocal)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, criteria.CurrencyLocal)) * (chg.Credit != null ? chg.Credit.Value : 0),
                            AmountDebitUSD =
                            (GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, Constants.CURRENCY_USD) > 0
                            ?
                                GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, Constants.CURRENCY_USD)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, Constants.CURRENCY_USD)) * (chg.Debit != null ? chg.Debit.Value : 0),
                            AmountCreditUSD =
                            (GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, Constants.CURRENCY_USD) > 0
                            ?
                                GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, Constants.CURRENCY_USD)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, Constants.CURRENCY_USD)) * (chg.Credit != null ? chg.Credit.Value : 0),
                            SOANo = chg.SOANo,
                            DatetimeModifiedSurcharge = chg.DatetimeModified
                        });
            query = query.OrderByDescending(x => x.DatetimeModifiedSurcharge);
            return query;
        }

        public ChargeShipmentResult GetListChargeShipment(ChargeShipmentCriteria criteria)
        {
            var chargeShipmentList = GetChargesShipmentByCriteria(criteria);
            var result = new ChargeShipmentResult
            {
                ChargeShipments = chargeShipmentList.ToList(),
                TotalShipment = chargeShipmentList.Where(x => x.HBL != null).GroupBy(x => x.HBL).Count(),
                TotalCharge = chargeShipmentList.Count(),
                AmountDebitLocal = chargeShipmentList.Sum(x => x.AmountDebitLocal),
                AmountCreditLocal = chargeShipmentList.Sum(x => x.AmountCreditLocal),
                AmountDebitUSD = chargeShipmentList.Sum(x => x.AmountDebitUSD),
                AmountCreditUSD = chargeShipmentList.Sum(x => x.AmountCreditUSD),
            };
            return result;
        }
        #endregion -- Get List Charges Shipment By Criteria --

        #region -- Get List More Charges Shipment By Criteria --
        private IQueryable<ChargeShipmentModel> GetMoreChargesShipmentByCriteria(MoreChargeShipmentCriteria criteria)
        {
            //Lấy danh sách Currency Exchange của ngày hiện tại
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();

            var charge = GetChargeShipmentDocAndOperation().Where(chg =>
                   chg.CustomerID == criteria.CustomerID
                && chg.IsOBH == (criteria.IsOBH == true ? chg.IsOBH : criteria.IsOBH)
                && (criteria.InSoa == true ? !string.IsNullOrEmpty(chg.SOANo) : string.IsNullOrEmpty(chg.SOANo))
            );

            if (string.IsNullOrEmpty(criteria.DateType) || criteria.DateType == "CreatedDate")
            {
                charge = charge.Where(chg =>
                    chg.CreatedDate.HasValue ? chg.CreatedDate.Value.Date >= criteria.FromDate.Date && chg.CreatedDate.Value.Date <= criteria.ToDate.Date : 1 == 2
                );
            }
            else if (criteria.DateType == "ServiceDate")
            {
                charge = charge.Where(chg =>
                    chg.ServiceDate.HasValue ? chg.ServiceDate.Value.Date >= criteria.FromDate.Date && chg.ServiceDate.Value.Date <= criteria.ToDate.Date : 1 == 2
                );
            }
            else if (criteria.DateType == "InvoiceIssuedDate")
            {
                charge = charge.Where(chg =>
                    chg.InvoiceIssuedDate.HasValue ? chg.InvoiceIssuedDate.Value.Date >= criteria.FromDate.Date && chg.InvoiceIssuedDate.Value.Date <= criteria.ToDate.Date : 1 == 2
                );
            }

            if (!string.IsNullOrEmpty(criteria.Type))
            {
                charge = charge.Where(chg =>
                       (criteria.Type == "Debit" || chg.Type == "OBH-SELL") ? chg.Debit.HasValue :
                       ((criteria.Type == "Credit" || chg.Type == "OBH-BUY") ? chg.Credit.HasValue : (chg.Debit.HasValue || chg.Credit.HasValue))
                );
            }

            if (!string.IsNullOrEmpty(criteria.StrCreators) || criteria.StrCreators == "All")
            {
                var listCreator = criteria.StrCreators.Split(',').Where(x => x.ToString() != "").ToList();
                charge = charge.Where(chg => listCreator.Contains(chg.UserCreated));
            }

            if (!string.IsNullOrEmpty(criteria.StrCharges) || criteria.StrCharges == "All")
            {
                var listCharge = criteria.StrCharges.Split(',').Where(x => x.ToString() != "").ToList();
                charge = charge.Where(chg => listCharge.Contains(chg.ChargeCode));
            }

            if (!string.IsNullOrEmpty(criteria.JobId))
            {
                charge = charge.Where(chg => chg.JobId == criteria.JobId);
            }

            if (!string.IsNullOrEmpty(criteria.Hbl))
            {
                charge = charge.Where(chg => chg.HBL == criteria.Hbl);
            }

            if (!string.IsNullOrEmpty(criteria.Mbl))
            {
                charge = charge.Where(chg => chg.MBL == criteria.Mbl);
            }

            if (!string.IsNullOrEmpty(criteria.CDNote))
            {
                charge = charge.Where(chg => chg.CreditDebitNo == criteria.CDNote);
            }

            if (!string.IsNullOrEmpty(criteria.StrServices) ||criteria.StrServices == "All")
            {
                var listService = criteria.StrServices.Split(',').Where(x => x.ToString() != "").ToList();
                charge = charge.Where(chg => listService.Contains(chg.Service));
            }

            if (criteria.CommodityGroupID != null)
            {
                charge = charge.Where(chg => criteria.CommodityGroupID == chg.CommodityGroupID);
            }

            var query = charge.Select(chg => new ChargeShipmentModel
            {
                ID = chg.ID,
                ChargeCode = chg.ChargeCode,
                ChargeName = chg.ChargeName,
                JobId = chg.JobId,
                HBL = chg.HBL,
                MBL = chg.MBL,
                CustomNo = chg.CustomNo,
                Type = chg.Type,
                InvoiceNo = chg.InvoiceNo,
                ServiceDate = chg.ServiceDate,
                Note = chg.Note,
                Debit = chg.Debit,
                Credit = chg.Credit,
                Currency = chg.Currency,
                CurrencyToLocal = criteria.CurrencyLocal,
                CurrencyToUSD = Constants.CURRENCY_USD,
                AmountDebitLocal =
                            (GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, criteria.CurrencyLocal) > 0
                            ?
                                GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, criteria.CurrencyLocal)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, criteria.CurrencyLocal)) * (chg.Debit != null ? chg.Debit.Value : 0),
                AmountCreditLocal =
                            (GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, criteria.CurrencyLocal) > 0
                            ?
                                GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, criteria.CurrencyLocal)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, criteria.CurrencyLocal)) * (chg.Credit != null ? chg.Credit.Value : 0),
                AmountDebitUSD =
                            (GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, Constants.CURRENCY_USD) > 0
                            ?
                                GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, Constants.CURRENCY_USD)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, Constants.CURRENCY_USD)) * (chg.Debit != null ? chg.Debit.Value : 0),
                AmountCreditUSD =
                            (GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, Constants.CURRENCY_USD) > 0
                            ?
                                GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, Constants.CURRENCY_USD)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, Constants.CURRENCY_USD)) * (chg.Credit != null ? chg.Credit.Value : 0),
                SOANo = chg.SOANo,
                DatetimeModifiedSurcharge = chg.DatetimeModified
            });
            query = query.OrderByDescending(x => x.DatetimeModifiedSurcharge);
            return query;
        }

        public IQueryable<ChargeShipmentModel> GetListMoreCharge(MoreChargeShipmentCriteria criteria)
        {
            var moreChargeShipmentList = GetMoreChargesShipmentByCriteria(criteria);

            List<Surcharge> Surcharges = new List<Surcharge>();
            if (criteria.ChargeShipments != null)
            {
                foreach (var item in criteria.ChargeShipments.Where(x => x.SOANo == null || x.SOANo == "").ToList())
                {
                    Surcharges.Add(new Surcharge { surchargeId = item.ID, type = item.Type });
                }
            }

            //Lấy ra các charge chưa tồn tại trong list criteria.Surcharges(Các Id của charge đã có trong kết quả search ở form info)
            var charge = moreChargeShipmentList.Where(x => Surcharges != null
                                                         && !Surcharges.Where(c => c.surchargeId == x.ID && c.type == x.Type).Any());
            return charge;
        }
        #endregion -- Get List More Charges Shipment By Criteria --

        #region -- Get List SOA By Criteria --
        private IQueryable<AcctSOAResult> QueryDataListSOA(IQueryable<ChargeSOAResult> charge, IQueryable<AcctSoa> soa, string currencyLocal)
        {
            //Lấy danh sách Currency Exchange của ngày hiện tại
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();
            var partner = catPartnerRepo.Get();
            var query = from s in soa
                        join chg in charge on s.Soano equals chg.SOANo into chg2
                        from chg in chg2.DefaultIfEmpty()
                        select new AcctSOAResult
                        {
                            Id = s.Id,
                            Soano = s.Soano,
                            HBL = chg.HBL,
                            Customer = s.Customer,
                            Currency = s.Currency,
                            CreditAmount = (GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency) > 0
                            ?
                                GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, s.Currency)) * (chg.Credit != null ? chg.Credit.Value : 0),
                            DebitAmount = (GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency) > 0
                            ?
                                GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, s.Currency)) * (chg.Debit != null ? chg.Debit.Value : 0),
                            Status = s.Status,
                            DatetimeCreated = s.DatetimeCreated,
                            UserCreated = s.UserCreated,
                            DatetimeModified = s.DatetimeModified,
                            UserModified = s.UserModified,
                        };
            var grpData = query.GroupBy(x => new {
                x.Id,
                x.Soano,
                x.Customer,
                x.Currency,
                x.Status,
                x.DatetimeCreated,
                x.DatetimeModified,
                x.UserCreated,
                x.UserModified,
            }).Select(x => new AcctSOAResult
            {
                Id = x.Key.Id,
                Soano = x.Key.Soano,
                Shipment = x.Select(s => s.HBL).Distinct().Count(),
                Currency = x.Key.Currency,
                CreditAmount = x.Sum(s => s.CreditAmount),
                DebitAmount = x.Sum(s => s.DebitAmount),
                Status = x.Key.Status,
                DatetimeCreated = x.Key.DatetimeCreated,
                UserCreated = x.Key.UserCreated,
                DatetimeModified = x.Key.DatetimeModified,
                UserModified = x.Key.UserModified,
            });

            var resultData = from grp in grpData
                             join pat in partner on grp.Customer equals pat.Id into pat2
                             from pat in pat2.DefaultIfEmpty()
                             select new AcctSOAResult
                             {
                                 Id = grp.Id,
                                 Soano = grp.Soano,
                                 Shipment = grp.Shipment,
                                 PartnerName = pat.ShortName,
                                 Currency = grp.Currency.Trim(),
                                 CreditAmount = grp.CreditAmount,
                                 DebitAmount = grp.DebitAmount,
                                 Status = grp.Status,
                                 DatetimeCreated = grp.DatetimeCreated,
                                 UserCreated = grp.UserCreated,
                                 DatetimeModified = grp.DatetimeModified,
                                 UserModified = grp.UserModified,
                             };
            return resultData;
        }

        public IQueryable<AcctSOAResult> GetListSOA(AcctSOACriteria criteria)
        {
            var charge = GetChargeShipmentDocAndOperation();
            var soa = DataContext.Get();

            if (!string.IsNullOrEmpty(criteria.StrCodes))
            {
                var listCode = criteria.StrCodes.Split(',').Where(x => x.ToString() != "").ToList();
                List<string> refNo = new List<string>();
                refNo = (from s in soa
                        join chg in charge on s.Soano equals chg.SOANo into chg2
                        from chg in chg2.DefaultIfEmpty()
                        where
                            listCode.Contains(s.Soano) || listCode.Contains(chg.JobId) || listCode.Contains(chg.MBL) || listCode.Contains(chg.HBL)
                        select s.Soano).ToList();
                soa = soa.Where(x => refNo.Contains(x.Soano));
            }

            if (!string.IsNullOrEmpty(criteria.CustomerID))
            {
                soa = soa.Where(x => x.Customer == criteria.CustomerID);
            }

            if (criteria.SoaFromDateCreate != null && criteria.SoaToDateCreate != null)
            {
                soa = soa.Where(x =>
                    x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date >= criteria.SoaFromDateCreate.Value.Date && x.DatetimeCreated.Value.Date <= criteria.SoaToDateCreate.Value.Date : 1 == 2
                );
            }

            if(!string.IsNullOrEmpty(criteria.SoaStatus))
            {
                soa = soa.Where(x => x.Status == criteria.SoaStatus);
            }

            if (!string.IsNullOrEmpty(criteria.SoaCurrency))
            {
                soa = soa.Where(x => x.Currency == criteria.SoaCurrency);
            }

            if(!string.IsNullOrEmpty(criteria.SoaUserCreate))
            {
                soa = soa.Where(x => x.UserCreated == criteria.SoaUserCreate);
            }

            var dataResult = QueryDataListSOA(charge, soa, criteria.CurrencyLocal);
            return dataResult;
        }
        #endregion -- Get List SOA By Criteria --

        #region -- Details Soa --
        private AcctSOADetailResult GetSoaBySoaNo(string soaNo, string currencyLocal)
        {
            //Lấy danh sách Currency Exchange của ngày hiện tại
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();
            var charge = GetChargeShipmentDocAndOperation();
            var soa = DataContext.Get(x => x.Soano == soaNo);
            var partner = catPartnerRepo.Get();
            var query = from s in soa
                        join chg in charge on s.Soano equals chg.SOANo into chg2
                        from chg in chg2.DefaultIfEmpty()
                        select new AcctSOADetailResult
                        {
                            Id = s.Id,
                            Soano = s.Soano,
                            HBL = chg.HBL,
                            Customer = s.Customer,
                            Currency = s.Currency,
                            CreditAmount = (GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency) > 0
                            ?
                                GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, s.Currency)) * (chg.Credit != null ? chg.Credit.Value : 0),
                            DebitAmount = (GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency) > 0
                            ?
                                GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, s.Currency)) * (chg.Debit != null ? chg.Debit.Value : 0),
                            Status = s.Status,
                            DatetimeCreated = s.DatetimeCreated,
                            UserCreated = s.UserCreated,
                            DatetimeModified = s.DatetimeModified,
                            UserModified = s.UserModified,
                            SoaformDate = s.SoaformDate,
                            SoatoDate = s.SoatoDate,
                            Note = s.Note,
                            Type = s.Type,
                            Obh = s.Obh,
                            ServiceTypeId = s.ServiceTypeId,
                            DateType = s.DateType,
                            CreatorShipment = s.CreatorShipment
                        };
            var grpData = query.GroupBy(x => new {
                x.Id,
                x.Soano,
                x.Customer,
                x.Currency,
                x.Status,
                x.DatetimeCreated,
                x.DatetimeModified,
                x.UserCreated,
                x.UserModified,
                x.SoaformDate,
                x.SoatoDate,
                x.Note,
                x.Type,
                x.Obh,
                x.ServiceTypeId,
                x.DateType,
                x.CreatorShipment
            }).Select(x => new AcctSOADetailResult
            {
                Id = x.Key.Id,
                Soano = x.Key.Soano,
                Shipment = x.Select(s => s.HBL).Distinct().Count(),
                Currency = x.Key.Currency,
                CreditAmount = x.Sum(s => s.CreditAmount),
                DebitAmount = x.Sum(s => s.DebitAmount),
                Status = x.Key.Status,
                DatetimeCreated = x.Key.DatetimeCreated,
                UserCreated = x.Key.UserCreated,
                DatetimeModified = x.Key.DatetimeModified,
                UserModified = x.Key.UserModified,
                SoaformDate = x.Key.SoaformDate,
                SoatoDate = x.Key.SoatoDate,
                Note = x.Key.Note,
                Type = x.Key.Type,
                Obh = x.Key.Obh,
                ServiceTypeId = x.Key.ServiceTypeId,
                Customer = x.Key.Customer,
                DateType = x.Key.DateType,
                CreatorShipment = x.Key.CreatorShipment
            });

            var resultData = from grp in grpData
                             join pat in partner on grp.Customer equals pat.Id into pat2
                             from pat in pat2.DefaultIfEmpty()
                             select new AcctSOADetailResult
                             {
                                 Id = grp.Id,
                                 Soano = grp.Soano,
                                 Shipment = grp.Shipment,
                                 PartnerName = pat.PartnerNameEn,
                                 Currency = grp.Currency,
                                 CreditAmount = grp.CreditAmount,
                                 DebitAmount = grp.DebitAmount,
                                 Status = grp.Status,
                                 DatetimeCreated = grp.DatetimeCreated,
                                 UserCreated = grp.UserCreated,
                                 DatetimeModified = grp.DatetimeModified,
                                 UserModified = grp.UserModified,
                                 SoaformDate = grp.SoaformDate,
                                 SoatoDate = grp.SoatoDate,
                                 Note = grp.Note,
                                 Type = grp.Type,
                                 Obh = grp.Obh,
                                 ServiceTypeId = grp.ServiceTypeId,
                                 Customer = grp.Customer,
                                 DateType = grp.DateType,
                                 CreatorShipment = grp.CreatorShipment
                             };
            return resultData.FirstOrDefault();
        } 

        private IQueryable<ChargeShipmentModel> GetListChargeOfSoa(string soaNo, string currencyLocal)
        {
            //Lấy danh sách Currency Exchange của ngày hiện tại
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();
            var charge = GetChargeShipmentDocAndOperation();
            var query = from chg in charge
                        where chg.SOANo == soaNo
                        select new ChargeShipmentModel {
                            SOANo = chg.SOANo,
                            ID = chg.ID,
                            ChargeCode = chg.ChargeCode,
                            ChargeName = chg.ChargeName,
                            JobId = chg.JobId,
                            HBL = chg.HBL,
                            MBL = chg.MBL,
                            CustomNo = chg.CustomNo,
                            Type = chg.Type,
                            Debit = chg.Debit,
                            Credit = chg.Credit,
                            Currency = chg.Currency,
                            InvoiceNo = chg.InvoiceNo,
                            ServiceDate = chg.ServiceDate,
                            Note = chg.Note,
                            CurrencyToLocal = currencyLocal,
                            CurrencyToUSD = Constants.CURRENCY_USD,
                            AmountDebitLocal = (GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, currencyLocal) > 0
                            ?
                                GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, currencyLocal)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, currencyLocal)) * (chg.Debit != null ? chg.Debit.Value : 0),
                            AmountCreditLocal = (GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, currencyLocal) > 0
                            ?
                                GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, currencyLocal)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, currencyLocal)) * (chg.Credit != null ? chg.Credit.Value : 0),
                            AmountDebitUSD = (GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, Constants.CURRENCY_USD) > 0
                            ?
                                GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, Constants.CURRENCY_USD)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, Constants.CURRENCY_USD)) * (chg.Debit != null ? chg.Debit.Value : 0),
                            AmountCreditUSD = (GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, Constants.CURRENCY_USD) > 0
                            ?
                                GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, Constants.CURRENCY_USD)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, Constants.CURRENCY_USD)) * (chg.Credit != null ? chg.Credit.Value : 0),
                        };
            return query;
        }

        public AcctSOADetailResult GetDetailBySoaNoAndCurrencyLocal(string soaNo, string currencyLocal)
        {
            var chargeShipments = GetListChargeOfSoa(soaNo, currencyLocal).ToList();
            var data = new AcctSOADetailResult();
            data = GetSoaBySoaNo(soaNo, currencyLocal);
            data.ChargeShipments = chargeShipments;
            data.AmountDebitLocal = Math.Round(chargeShipments.Sum(x => x.AmountDebitLocal), 3);
            data.AmountCreditLocal = Math.Round(chargeShipments.Sum(x => x.AmountCreditLocal), 3);
            data.AmountDebitUSD = Math.Round(chargeShipments.Sum(x => x.AmountDebitUSD), 3);
            data.AmountCreditUSD = Math.Round(chargeShipments.Sum(x => x.AmountCreditUSD), 3);

            //Thông tin các Service Name của SOA
            data.ServicesNameSoa = GetServiceNameOfSoa(data.ServiceTypeId).ToString();

            return data;
        }

        private string GetServiceNameOfSoa(string serviceTypeId)
        {
            var serviceName = "";

            if (!string.IsNullOrEmpty(serviceTypeId))
            {
                //Tách chuỗi servicetype thành mảng
                string[] arrayStrServiceTypeId = serviceTypeId.Split(';').Where(x => x.ToString() != "").ToArray();

                //Xóa các serviceTypeId trùng
                string[] arrayGrpServiceTypeId = arrayStrServiceTypeId.Distinct<string>().ToArray();

                var serviceId = "";
                foreach (var item in arrayGrpServiceTypeId)
                {
                    //Lấy ra DisplayName của serviceTypeId
                    serviceName += CustomData.Services.Where(x => x.Value == item).FirstOrDefault() != null ?
                                CustomData.Services.Where(x => x.Value == item).FirstOrDefault().DisplayName.Trim() + ";"
                                : "";
                    serviceId += item + ";";
                }
                serviceName = (serviceName + ")").Replace(";)", "");
            }
            return serviceName;
        }
        #endregion --Details Soa--

        public IQueryable<ExportSOAModel> GetDateExportDetailSOA(string soaNo)
        {
            //Lấy danh sách Currency Exchange của ngày hiện tại
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();
            var soa = DataContext.Get(x => x.Soano == soaNo);
            var charge = GetChargeShipmentDocAndOperation();
            var partner = catPartnerRepo.Get();
            var query = from s in soa
                        join chg in charge on s.Soano equals chg.SOANo into chg2
                        from chg in chg2.DefaultIfEmpty()
                        join pat in partner on s.Customer equals pat.Id into pat2
                        from pat in pat2.DefaultIfEmpty()
                        select new ExportSOAModel
                        {
                            SOANo = s.Soano,
                            CustomerName = pat.PartnerNameEn,
                            TaxCode = pat.TaxCode,
                            CustomerAddress = pat.AddressEn,
                            ServiceDate = chg.ServiceDate,
                            JobId = chg.JobId,
                            HBL = chg.HBL,
                            MBL = chg.MBL,
                            CustomNo = chg.CustomNo,
                            ChargeCode = chg.ChargeCode,
                            ChargeName = chg.ChargeName,
                            CreditDebitNo = chg.CreditDebitNo,
                            Debit = chg.Debit,
                            Credit = chg.Credit,
                            CurrencySOA = s.Currency,
                            CurrencyCharge = chg.Currency,
                            CreditExchange = (GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency) > 0
                            ?
                                GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, s.Currency)) * (chg.Credit != null ? chg.Credit.Value : 0),
                            DebitExchange = (GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency) > 0
                            ?
                                GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, s.Currency)) * (chg.Debit != null ? chg.Debit.Value : 0),                           
                        };
            return query;
        }
        #endregion -- --
    }
}
