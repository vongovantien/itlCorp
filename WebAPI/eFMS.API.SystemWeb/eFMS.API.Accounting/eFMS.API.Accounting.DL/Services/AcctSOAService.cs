using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.ReportResults;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
        readonly IContextBase<SysUser> sysUserRepo;
        readonly IContextBase<CatChargeDefaultAccount> chargeDefaultRepo;
        readonly IContextBase<CatPlace> catPlaceRepo;
        readonly IContextBase<SysOffice> officeRepo;
        readonly IContextBase<CatCommodity> catCommodityRepo;
        readonly IContextBase<CatCommodityGroup> catCommodityGroupRepo;
        readonly IContextBase<SysCompany> sysCompanyRepo;
        private readonly ICurrencyExchangeService currencyExchangeService;
        private decimal _decimalNumber = Constants.DecimalNumber;

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
            IContextBase<CatPartner> catPartner,
            IContextBase<SysUser> sysUser,
            IContextBase<CatPlace> catplace,
            IContextBase<CatChargeDefaultAccount> chargeDefault,
            IContextBase<SysOffice> sysOffice,
            IContextBase<CatCommodity> catCommodity,
            IContextBase<CatCommodityGroup> catCommodityGroup,
            ICurrencyExchangeService currencyExchange,
            IContextBase<SysCompany> sysCompany) : base(repository, mapper)
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
            sysUserRepo = sysUser;
            chargeDefaultRepo = chargeDefault;
            catPlaceRepo = catplace;
            officeRepo = sysOffice;
            catCommodityRepo = catCommodity;
            catCommodityGroupRepo = catCommodityGroup;
            currencyExchangeService = currencyExchange;
            sysCompanyRepo = sysCompany;
        }

        #region -- Insert & Update SOA
        public HandleState AddSOA(AcctSoaModel model)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSOA);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None) return new HandleState(403, "");

            try
            {
                var userCurrent = currentUser.UserID;

                model.Status = AccountingConstants.STATUS_SOA_NEW;
                model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
                model.UserCreated = model.UserModified = userCurrent;
                model.Currency = model.Currency.Trim();
                model.GroupId = currentUser.GroupId;
                model.DepartmentId = currentUser.DepartmentId;
                model.OfficeId = currentUser.OfficeID;
                model.CompanyId = currentUser.CompanyID;

                //Check exists OBH Debit Charge
                var isExistObhDebitCharge = csShipmentSurchargeRepo.Get(x => model.Surcharges != null
                                                               && model.Surcharges.Any(c => c.surchargeId == x.Id && c.type == AccountingConstants.TYPE_CHARGE_OBH_SELL)
                                                               ).Any();
                if (isExistObhDebitCharge)
                {
                    model.PaymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                    DateTime? dueDate = null;

                    dueDate = model.DatetimeCreated.Value.AddDays(30);
                    model.PaymentDueDate = dueDate;
                }

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        //Tính phí AmountDebit, AmountCredit của SOA (tỉ giá được exchange dựa vào Final Exchange Rate, Exchange Date của charge)
                        var amountDebitCreditShipment = GetDebitCreditAmountAndTotalShipment(model);
                        model.TotalShipment = amountDebitCreditShipment.TotalShipment;
                        model.DebitAmount = amountDebitCreditShipment.DebitAmount;
                        model.CreditAmount = amountDebitCreditShipment.CreditAmount;

                        var soa = mapper.Map<AcctSoa>(model);
                        soa.Soano = model.Soano = CreateSoaNo();

                        var hs = DataContext.Add(soa, false);

                        if (hs.Success)
                        {
                            //Lấy ra những charge có type là BUY hoặc OBH-BUY mà chưa tồn tại trong 1 SOA nào cả
                            var surchargeCredit = csShipmentSurchargeRepo.Get(x => model.Surcharges != null
                                                                           && model.Surcharges.Any(c => c.surchargeId == x.Id && (c.type == AccountingConstants.TYPE_CHARGE_BUY || c.type == AccountingConstants.TYPE_CHARGE_OBH_BUY))
                                                                           ).ToList();

                            //Lấy ra những charge có type là SELL hoặc OBH-SELL mà chưa tồn tại trong 1 SOA nào cả
                            var surchargeDebit = csShipmentSurchargeRepo.Get(x => model.Surcharges != null
                                                                           && model.Surcharges.Any(c => c.surchargeId == x.Id && (c.type == AccountingConstants.TYPE_CHARGE_SELL || c.type == AccountingConstants.TYPE_CHARGE_OBH_SELL))
                                                                           ).ToList(); //&& (x.Soano == null || x.Soano == string.Empty)

                            if (surchargeCredit.Count() > 0)
                            {
                                //Update PaySOANo cho CsShipmentSurcharge có type BUY hoặc OBH-BUY(Payer)
                                //Change request: Cập nhật lại ngày ExchangeDate (23/09/2019)
                                foreach (var item in surchargeCredit)
                                {
                                    item.PaySoano = soa.Soano;
                                    item.UserModified = userCurrent;
                                    item.DatetimeModified = model.DatetimeCreated;
                                    if (string.IsNullOrEmpty(item.CreditNo) && string.IsNullOrEmpty(item.DebitNo))
                                    {
                                        //Cập nhật ExchangeDate của phí theo ngày Created Date SOA & phí chưa có tạo CDNote
                                        item.ExchangeDate = model.DatetimeCreated;
                                    }
                                    var hsUpdateSurchargeCredit = csShipmentSurchargeRepo.Update(item, x => x.Id == item.Id, false);
                                }
                            }

                            if (surchargeDebit.Count() > 0)
                            {
                                //Update SOANo cho CsShipmentSurcharge có type là SELL hoặc OBH-SELL(Receiver)
                                //Change request: Cập nhật lại ngày ExchangeDate (23/09/2019)
                                foreach (var item in surchargeDebit)
                                {
                                    item.Soano = soa.Soano;
                                    item.UserModified = userCurrent;
                                    item.DatetimeModified = model.DatetimeCreated;
                                    if (string.IsNullOrEmpty(item.CreditNo) && string.IsNullOrEmpty(item.DebitNo))
                                    {
                                        //Cập nhật ExchangeDate của phí theo ngày Created Date SOA & phí chưa có tạo CDNote
                                        item.ExchangeDate = model.DatetimeCreated;
                                    }
                                    var hsUpdateSurChargeDebit = csShipmentSurchargeRepo.Update(item, x => x.Id == item.Id, false);
                                }
                            }
                        }
                        csShipmentSurchargeRepo.SubmitChanges();
                        DataContext.SubmitChanges();
                        trans.Commit();
                        return hs;

                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return new HandleState(ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        public HandleState UpdateSOA(AcctSoaModel model)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSOA);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None) return new HandleState(403, "");

            try
            {
                var userCurrent = currentUser.UserID;
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        //Gỡ bỏ các charge có SOANo = model.Soano và PaySOANo = model.Soano
                        var surcharge = csShipmentSurchargeRepo.Get(x => x.Soano == model.Soano || x.PaySoano == model.Soano).ToList();
                        foreach (var item in surcharge)
                        {
                            //Update SOANo = NULL & PaySOANo = NULL to CsShipmentSurcharge
                            item.Soano = null;
                            item.PaySoano = null;
                            item.UserModified = currentUser.UserID;
                            item.DatetimeModified = DateTime.Now;
                            var hsUpdateSurchargeSOANoEqualNull = csShipmentSurchargeRepo.Update(item, x => x.Id == item.Id, false);
                        }

                        model.DatetimeModified = DateTime.Now;
                        model.UserModified = userCurrent;
                        model.Currency = model.Currency.Trim();

                        //Tính phí AmountDebit, AmountCredit của SOA (tỉ giá được exchange dựa vào Final Exchange Rate, Exchange Date của charge)
                        var amountDebitCreditShipment = GetDebitCreditAmountAndTotalShipment(model);
                        model.TotalShipment = amountDebitCreditShipment.TotalShipment;
                        model.DebitAmount = amountDebitCreditShipment.DebitAmount;
                        model.CreditAmount = amountDebitCreditShipment.CreditAmount;

                        var soa = mapper.Map<AcctSoa>(model);
                        var soaCurrent = DataContext.Get(x => x.Id == soa.Id).FirstOrDefault();
                        soa.GroupId = soaCurrent.GroupId;
                        soa.DepartmentId = soaCurrent.DepartmentId;
                        soa.OfficeId = soaCurrent.OfficeId;
                        soa.CompanyId = soaCurrent.CompanyId;


                        //Check exists OBH Debit Charge
                        var isExistObhDebitCharge = csShipmentSurchargeRepo.Get(x => model.Surcharges != null
                                                                       && model.Surcharges.Any(c => c.surchargeId == x.Id && c.type == AccountingConstants.TYPE_CHARGE_OBH_SELL)
                                                                       ).Any();
                        if (isExistObhDebitCharge)
                        {
                            soa.PaymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;

                            DateTime? dueDate = null;
                            dueDate = soaCurrent.DatetimeCreated.Value.AddDays(30);

                            soa.PaymentDueDate = dueDate;
                        }
                        else
                        {
                            soa.PaymentStatus = (soa.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID) ? null : soa.PaymentStatus;
                        }

                        //Update các thông tin của SOA
                        var hs = DataContext.Update(soa, x => x.Id == soa.Id, false);

                        if (hs.Success)
                        {
                            //Lấy ra những charge có type là BUY hoặc OBH-BUY mà chưa tồn tại trong 1 SOA nào cả
                            var surchargeCredit = csShipmentSurchargeRepo.Get(x => model.Surcharges != null
                                                                           && model.Surcharges.Any(c => c.surchargeId == x.Id && (c.type == AccountingConstants.TYPE_CHARGE_BUY || c.type == AccountingConstants.TYPE_CHARGE_OBH_BUY))
                                                                           ).ToList();

                            //Lấy ra những charge có type là SELL hoặc OBH-SELL mà chưa tồn tại trong 1 SOA nào cả
                            var surchargeDebit = csShipmentSurchargeRepo.Get(x => model.Surcharges != null
                                                                           && model.Surcharges.Any(c => c.surchargeId == x.Id && (c.type == AccountingConstants.TYPE_CHARGE_SELL || c.type == AccountingConstants.TYPE_CHARGE_OBH_SELL))
                                                                           ).ToList(); //&& (x.Soano == null || x.Soano == string.Empty)

                            if (surchargeCredit.Count() > 0)
                            {
                                //Update PaySOANo cho CsShipmentSurcharge có type BUY hoặc OBH-BUY(Payer)
                                //Change request: Cập nhật lại ngày ExchangeDate (23/09/2019)
                                foreach (var item in surchargeCredit)
                                {
                                    item.PaySoano = soa.Soano;
                                    item.UserModified = userCurrent;
                                    item.DatetimeModified = DateTime.Now;
                                    if (string.IsNullOrEmpty(item.CreditNo) && string.IsNullOrEmpty(item.DebitNo))
                                    {
                                        //Cập nhật ExchangeDate của phí theo ngày Created Date SOA & phí chưa có tạo CDNote
                                        item.ExchangeDate = model.DatetimeCreated;
                                    }
                                    var hsUpdateSurchargeCredit = csShipmentSurchargeRepo.Update(item, x => x.Id == item.Id, false);
                                }
                            }

                            if (surchargeDebit.Count() > 0)
                            {
                                //Update SOANo cho CsShipmentSurcharge có type là SELL hoặc OBH-SELL(Receiver)
                                //Change request: Cập nhật lại ngày ExchangeDate (23/09/2019)
                                foreach (var item in surchargeDebit)
                                {
                                    item.Soano = soa.Soano;
                                    item.UserModified = userCurrent;
                                    item.DatetimeModified = DateTime.Now;
                                    if (string.IsNullOrEmpty(item.CreditNo) && string.IsNullOrEmpty(item.DebitNo))
                                    {
                                        //Cập nhật ExchangeDate của phí theo ngày Created Date SOA & phí chưa có tạo CDNote
                                        item.ExchangeDate = model.DatetimeCreated;
                                    }
                                    var hsUpdateSurchargeDebit = csShipmentSurchargeRepo.Update(item, x => x.Id == item.Id, false);
                                }
                            }
                        }
                        csShipmentSurchargeRepo.SubmitChanges();
                        DataContext.SubmitChanges();
                        trans.Commit();
                        return hs;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return new HandleState(ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        public bool CheckUpdatePermission(string soaNo)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSOA);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None)
                return false;

            var detail = DataContext.Get(x => x.Soano == soaNo)?.FirstOrDefault();
            if (detail == null) return false;

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = detail.UserCreated,
                CompanyId = detail.CompanyId,
                DepartmentId = detail.DepartmentId,
                OfficeId = detail.OfficeId,
                GroupId = detail.GroupId
            };
            int code = PermissionExtention.GetPermissionCommonItem(baseModel, permissionRange, _user);

            if (code == 403) return false;

            return true;
        }

        public bool CheckDeletePermission(string soaNo)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSOA);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            if (permissionRange == PermissionRange.None)
                return false;

            var detail = DataContext.Get(x => x.Soano == soaNo)?.FirstOrDefault();
            if (detail == null) return false;

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = detail.UserCreated,
                CompanyId = detail.CompanyId,
                DepartmentId = detail.DepartmentId,
                OfficeId = detail.OfficeId,
                GroupId = detail.GroupId
            };
            int code = PermissionExtention.GetPermissionCommonItem(baseModel, permissionRange, _user);

            if (code == 403) return false;

            return true;
        }

        public HandleState DeleteSOA(string soaNo)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSOA);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            if (permissionRange == PermissionRange.None) return new HandleState(403, "");

            var hs = DataContext.Delete(x => x.Soano == soaNo);
            return hs;
        }

        public HandleState UpdateSOASurCharge(string soaNo)
        {
            try
            {
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        var surcharge = csShipmentSurchargeRepo.Get(x => x.Soano == soaNo || x.PaySoano == soaNo).ToList();
                        if (surcharge.Count() > 0)
                        {
                            //Update SOANo = NULL & PaySOANo = NULL to CsShipmentSurcharge
                            foreach (var item in surcharge)
                            {
                                item.Soano = null;
                                item.PaySoano = null;
                                item.UserModified = currentUser.UserID;
                                item.DatetimeModified = DateTime.Now;
                                var hsUpdateSOANoSurcharge = csShipmentSurchargeRepo.Update(item, x => x.Id == item.Id);
                            }
                        }
                        trans.Commit();
                        return new HandleState();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return new HandleState(ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        private AcctSoa GetDebitCreditAmountAndTotalShipment(AcctSoaModel model)
        {
            //Tính phí AmountDebit, AmountCredit của SOA (tỉ giá được exchange dựa vào Final Exchange Rate, Exchange Date của charge)
            Expression<Func<ChargeSOAResult, bool>> query = x => model.Surcharges.Where(c => c.surchargeId == x.ID && c.type == x.Type).Any();
            var charge = GetChargeShipmentDocAndOperation(query, null);
            //var dataResult = charge.Select(chg => new
            //{
            //    AmountDebit = (GetRateCurrencyExchange(today, chg.Currency, model.Currency) > 0
            //                ?
            //                    GetRateCurrencyExchange(today, chg.Currency, model.Currency)
            //                :
            //                    GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, model.Currency)) * (chg.Debit != null ? chg.Debit.Value : 0),
            //    AmountCredit = (GetRateCurrencyExchange(today, chg.Currency, model.Currency) > 0
            //                ?
            //                    GetRateCurrencyExchange(today, chg.Currency, model.Currency)
            //                :
            //                    GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, model.Currency)) * (chg.Credit != null ? chg.Credit.Value : 0),
            //});

            //Count number shipment (HBL)
            var totalShipment = charge.Select(s => s.HBL).Distinct().Count();

            decimal debitAmount = 0;
            decimal creditAmount = 0;
            foreach (var chg in charge)
            {
                var _exchangeDate = model.Id == 0 ? DateTime.Now : model.DatetimeCreated;
                var _exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(chg.FinalExchangeRate, _exchangeDate, chg.Currency, model.Currency);
                //Debit Amount
                debitAmount += _exchangeRate * (chg.Debit ?? 0);
                //Credit Amount
                creditAmount += _exchangeRate * (chg.Credit ?? 0);
            }
            return new AcctSoa { TotalShipment = totalShipment, DebitAmount = debitAmount, CreditAmount = creditAmount };
        }

        private string CreateSoaNo()
        {
            var prefix = (DateTime.Now.Year.ToString()).Substring(2, 2);
            string stt;
            //Lấy ra dòng cuối cùng của table acctSOA
            var rowLast = DataContext.Get().LastOrDefault();
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
        #endregion -- Insert & Update SOA             

        #region -- List Status SOA --
        public object GetListStatusSoa()
        {
            return Common.CustomData.StatusSoa;
        }
        #endregion -- List Status SOA --

        #region -- Get Rate Exchange --
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

        #endregion -- Get Rate Exchange --

        #region -- Get Data Charge Master --
        private IQueryable<ChargeSOAResult> GetChargeBuySell(Expression<Func<ChargeSOAResult, bool>> query)
        {
            //Chỉ lấy những phí từ shipment (IsFromShipment = true)
            var surcharge = csShipmentSurchargeRepo.Get(x => x.IsFromShipment == true && (x.Type == AccountingConstants.TYPE_CHARGE_BUY || x.Type == AccountingConstants.TYPE_CHARGE_SELL));
            var opst = opsTransactionRepo.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != null && x.CurrentStatus != TermData.Canceled);
            var csTrans = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled);
            var csTransDe = csTransactionDetailRepo.Get();
            var creditNote = acctCdnoteRepo.Get();
            var debitNote = acctCdnoteRepo.Get();
            var charge = catChargeRepo.Get();
            var customsDeclarations = customsDeclarationRepo.Get();
            var sysUsers = sysUserRepo.Get();
            //BUY & SELL
            var queryBuySellOperation = from sur in surcharge
                                        join ops in opst on sur.Hblid equals ops.Hblid
                                        join creditN in creditNote on sur.CreditNo equals creditN.Code into creditN2
                                        from creditN in creditN2.DefaultIfEmpty()
                                        join debitN in debitNote on sur.DebitNo equals debitN.Code into debitN2
                                        from debitN in debitN2.DefaultIfEmpty()
                                        join chg in charge on sur.ChargeId equals chg.Id into chg2
                                        from chg in chg2.DefaultIfEmpty()
                                        join cd in customsDeclarations on ops.JobNo equals cd.JobNo into cdGrps
                                        from cdGrp in cdGrps.DefaultIfEmpty()
                                        join user in sysUsers on ops.BillingOpsId equals user.Id into userGps
                                        from useGrp in userGps.DefaultIfEmpty()
                                        select new ChargeSOAResult
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
                                            Debit = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? (decimal?)sur.Total : null,
                                            Credit = sur.Type == AccountingConstants.TYPE_CHARGE_BUY ? (decimal?)sur.Total : null,
                                            SOANo = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? sur.Soano : sur.PaySoano,
                                            IsOBH = false,
                                            Currency = sur.CurrencyId,
                                            InvoiceNo = sur.InvoiceNo,
                                            Note = sur.Notes,
                                            CustomerID = sur.PaymentObjectId,
                                            ServiceDate = ops.ServiceDate,
                                            CreatedDate = ops.DatetimeCreated,
                                            InvoiceIssuedDate = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? debitN.DatetimeCreated : creditN.DatetimeCreated,
                                            TransactionType = null,
                                            UserCreated = ops.UserCreated,
                                            Quantity = sur.Quantity,
                                            UnitId = sur.UnitId,
                                            UnitPrice = sur.UnitPrice,
                                            VATRate = sur.Vatrate,
                                            CreditDebitNo = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? sur.DebitNo : sur.CreditNo,
                                            DatetimeModified = sur.DatetimeModified,
                                            CommodityGroupID = ops.CommodityGroupId,

                                            Service = "CL",
                                            CDNote = !string.IsNullOrEmpty(sur.CreditNo) ? sur.CreditNo : sur.DebitNo,
                                            TypeCharge = chg.Type,
                                            ExchangeDate = sur.ExchangeDate,
                                            FinalExchangeRate = sur.FinalExchangeRate,
                                            CustomNo = cdGrp.ClearanceNo,
                                            PIC = useGrp.Username
                                        };
            queryBuySellOperation = queryBuySellOperation.Where(x => !string.IsNullOrEmpty(x.Service)).Where(query);

            var queryBuySellDocument = from sur in surcharge
                                       join cstd in csTransDe on sur.Hblid equals cstd.Id
                                       join cst in csTrans on cstd.JobId equals cst.Id
                                       join creditN in creditNote on sur.CreditNo equals creditN.Code into creditN2
                                       from creditN in creditN2.DefaultIfEmpty()
                                       join debitN in debitNote on sur.DebitNo equals debitN.Code into debitN2
                                       from debitN in debitN2.DefaultIfEmpty()
                                       join chg in charge on sur.ChargeId equals chg.Id into chg2
                                       from chg in chg2.DefaultIfEmpty()
                                       join user in sysUsers on cst.PersonIncharge equals user.Id into userGps
                                       from useGrp in userGps.DefaultIfEmpty()
                                       select new ChargeSOAResult
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
                                           Debit = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? (decimal?)sur.Total : null,
                                           Credit = sur.Type == AccountingConstants.TYPE_CHARGE_BUY ? (decimal?)sur.Total : null,
                                           SOANo = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? sur.Soano : sur.PaySoano,
                                           IsOBH = false,
                                           Currency = sur.CurrencyId,
                                           InvoiceNo = sur.InvoiceNo,
                                           Note = sur.Notes,
                                           CustomerID = sur.PaymentObjectId,
                                           ServiceDate = (cst.TransactionType == "AI" || cst.TransactionType == "SFI" || cst.TransactionType == "SLI" || cst.TransactionType == "SCI" ? cst.Eta : cst.Etd),
                                           CreatedDate = cst.DatetimeCreated,
                                           InvoiceIssuedDate = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? debitN.DatetimeCreated : creditN.DatetimeCreated,
                                           TransactionType = cst.TransactionType,
                                           UserCreated = cst.UserCreated,
                                           Quantity = sur.Quantity,
                                           UnitId = sur.UnitId,
                                           UnitPrice = sur.UnitPrice,
                                           VATRate = sur.Vatrate,
                                           CreditDebitNo = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? sur.DebitNo : sur.CreditNo,
                                           DatetimeModified = sur.DatetimeModified,
                                           CommodityGroupID = null,
                                           Service = cst.TransactionType,
                                           CDNote = !string.IsNullOrEmpty(sur.CreditNo) ? sur.CreditNo : sur.DebitNo,
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
                                           CBM = cstd.Cbm,
                                           PackageContainer = cstd.PackageContainer,
                                           TypeCharge = chg.Type,
                                           PIC = useGrp.Username
                                       };
            queryBuySellDocument = queryBuySellDocument.Where(x => !string.IsNullOrEmpty(x.Service)).Where(query);

            var queryBuySell = queryBuySellOperation.Union(queryBuySellDocument);
            return queryBuySell;
        }

        private IQueryable<ChargeSOAResult> GetChargeOBHSell(Expression<Func<ChargeSOAResult, bool>> query, bool? isOBH)
        {
            //Lấy cả phí chứng từ & phí hiện trường [Change request 14469 (21/09/2020)]
            var surcharge = csShipmentSurchargeRepo.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH);
            var opst = opsTransactionRepo.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != null && x.CurrentStatus != TermData.Canceled);
            var csTrans = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled);
            var csTransDe = csTransactionDetailRepo.Get();
            var debitNote = acctCdnoteRepo.Get();
            var charge = catChargeRepo.Get();
            var partner = catPartnerRepo.Get();
            var customDeclearation = customsDeclarationRepo.Get();
            var sysUsers = sysUserRepo.Get();
            //OBH Receiver (SELL - Credit)
            var queryObhSellOperation = from sur in surcharge
                                        join ops in opst on sur.Hblid equals ops.Hblid
                                        join debitN in debitNote on sur.DebitNo equals debitN.Code into debitN2
                                        from debitN in debitN2.DefaultIfEmpty()
                                        join chg in charge on sur.ChargeId equals chg.Id into chg2
                                        from chg in chg2.DefaultIfEmpty()
                                        join pat in partner on sur.PaymentObjectId equals pat.Id into pat2
                                        from pat in pat2.DefaultIfEmpty()
                                        join cd in customDeclearation on ops.JobNo equals cd.JobNo into cdGrps
                                        from cdGrp in cdGrps.DefaultIfEmpty()
                                        join user in sysUsers on ops.BillingOpsId equals user.Id into userGps
                                        from useGrp in userGps.DefaultIfEmpty()
                                        select new ChargeSOAResult
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
                                            Debit = sur.Total,
                                            Credit = null,
                                            SOANo = sur.Soano,
                                            IsOBH = true,
                                            Currency = sur.CurrencyId,
                                            InvoiceNo = sur.InvoiceNo,
                                            Note = sur.Notes,
                                            CustomerID = sur.PaymentObjectId,
                                            ServiceDate = ops.ServiceDate,
                                            CreatedDate = ops.DatetimeCreated,
                                            InvoiceIssuedDate = debitN.DatetimeCreated,
                                            TransactionType = null,
                                            UserCreated = ops.UserCreated,
                                            Quantity = sur.Quantity,
                                            UnitId = sur.UnitId,
                                            UnitPrice = sur.UnitPrice,
                                            VATRate = sur.Vatrate,
                                            CreditDebitNo = sur.DebitNo,
                                            DatetimeModified = sur.DatetimeModified,
                                            CommodityGroupID = ops.CommodityGroupId,
                                            Service = "CL",
                                            CDNote = !string.IsNullOrEmpty(sur.CreditNo) ? sur.CreditNo : sur.DebitNo,
                                            TaxCodeOBH = pat.TaxCode,
                                            TypeCharge = chg.Type,
                                            ExchangeDate = sur.ExchangeDate,
                                            FinalExchangeRate = sur.FinalExchangeRate,
                                            CustomNo = cdGrp.ClearanceNo,
                                            PIC = useGrp.Username
                                        };
            queryObhSellOperation = queryObhSellOperation.Where(x => !string.IsNullOrEmpty(x.Service)).Where(query);
            if (isOBH != null)
            {
                queryObhSellOperation = queryObhSellOperation.Where(x => x.IsOBH == isOBH);
            }
            var queryObhSellDocument = from sur in surcharge
                                       join cstd in csTransDe on sur.Hblid equals cstd.Id
                                       join cst in csTrans on cstd.JobId equals cst.Id
                                       join debitN in debitNote on sur.DebitNo equals debitN.Code into debitN2
                                       from debitN in debitN2.DefaultIfEmpty()
                                       join chg in charge on sur.ChargeId equals chg.Id into chg2
                                       from chg in chg2.DefaultIfEmpty()
                                       join user in sysUsers on cst.PersonIncharge equals user.Id into userGps
                                       from useGrp in userGps.DefaultIfEmpty()
                                       select new ChargeSOAResult
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
                                           Debit = sur.Total,
                                           Credit = null,
                                           SOANo = sur.Soano,
                                           IsOBH = true,
                                           Currency = sur.CurrencyId,
                                           InvoiceNo = sur.InvoiceNo,
                                           Note = sur.Notes,
                                           CustomerID = sur.PaymentObjectId,
                                           ServiceDate = (cst.TransactionType == "AI" || cst.TransactionType == "SFI" || cst.TransactionType == "SLI" || cst.TransactionType == "SCI" ? cst.Eta : cst.Etd),
                                           CreatedDate = cst.DatetimeCreated,
                                           InvoiceIssuedDate = debitN.DatetimeCreated,
                                           TransactionType = cst.TransactionType,
                                           UserCreated = cst.UserCreated,
                                           Quantity = sur.Quantity,
                                           UnitId = sur.UnitId,
                                           UnitPrice = sur.UnitPrice,
                                           VATRate = sur.Vatrate,
                                           CreditDebitNo = sur.DebitNo,
                                           DatetimeModified = sur.DatetimeModified,
                                           CommodityGroupID = null,
                                           Service = cst.TransactionType,
                                           CDNote = !string.IsNullOrEmpty(sur.CreditNo) ? sur.CreditNo : sur.DebitNo,
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
                                           TypeCharge = chg.Type,
                                           PIC = useGrp.Username
                                       };
            queryObhSellDocument = queryObhSellDocument.Where(x => !string.IsNullOrEmpty(x.Service)).Where(query);
            if (isOBH != null)
            {
                queryObhSellDocument = queryObhSellDocument.Where(x => x.IsOBH == isOBH);
            }
            var queryObhSell = queryObhSellOperation.Union(queryObhSellDocument);
            return queryObhSell;
        }

        private IQueryable<ChargeSOAResult> GetChargeOBHBuy(Expression<Func<ChargeSOAResult, bool>> query, bool? isOBH)
        {
            //Lấy cả phí chứng từ & phí hiện trường [Change request 14469 (21/09/2020)] 
            var surcharge = csShipmentSurchargeRepo.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH);
            var opst = opsTransactionRepo.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != null && x.CurrentStatus != TermData.Canceled);
            var csTrans = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled);
            var csTransDe = csTransactionDetailRepo.Get();
            var custom = customsDeclarationRepo.Get();
            var creditNote = acctCdnoteRepo.Get();
            var charge = catChargeRepo.Get();
            var sysUsers = sysUserRepo.Get();
            //OBH Payer (BUY - Credit)
            var queryObhBuyOperation = from sur in surcharge
                                       join ops in opst on sur.Hblid equals ops.Hblid
                                       join creditN in creditNote on sur.CreditNo equals creditN.Code into creditN2
                                       from creditN in creditN2.DefaultIfEmpty()
                                       join chg in charge on sur.ChargeId equals chg.Id into chg2
                                       from chg in chg2.DefaultIfEmpty()
                                       join cd in custom on ops.JobNo equals cd.JobNo into cdGrps
                                       from cdGrp in cdGrps.DefaultIfEmpty()
                                       join user in sysUsers on ops.BillingOpsId equals user.Id into userGps
                                       from useGrp in userGps.DefaultIfEmpty()
                                       select new ChargeSOAResult
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
                                           Debit = null,
                                           Credit = sur.Total,
                                           SOANo = sur.PaySoano,
                                           IsOBH = true,
                                           Currency = sur.CurrencyId,
                                           InvoiceNo = sur.InvoiceNo,
                                           Note = sur.Notes,
                                           CustomerID = sur.PayerId,
                                           ServiceDate = ops.ServiceDate,
                                           CreatedDate = ops.DatetimeCreated,
                                           InvoiceIssuedDate = creditN.DatetimeCreated,
                                           TransactionType = null,
                                           UserCreated = ops.UserCreated,
                                           Quantity = sur.Quantity,
                                           UnitId = sur.UnitId,
                                           UnitPrice = sur.UnitPrice,
                                           VATRate = sur.Vatrate,
                                           CreditDebitNo = sur.CreditNo,
                                           DatetimeModified = sur.DatetimeModified,
                                           CommodityGroupID = ops.CommodityGroupId,
                                           Service = "CL",
                                           CDNote = !string.IsNullOrEmpty(sur.CreditNo) ? sur.CreditNo : sur.DebitNo,
                                           ExchangeDate = sur.ExchangeDate,
                                           FinalExchangeRate = sur.FinalExchangeRate,
                                           TypeCharge = chg.Type,
                                           CustomNo = cdGrp.ClearanceNo,
                                           PIC = useGrp.Username

                                       };
            queryObhBuyOperation = queryObhBuyOperation.Where(x => !string.IsNullOrEmpty(x.Service)).Where(query);
            if (isOBH != null)
            {
                queryObhBuyOperation = queryObhBuyOperation.Where(x => x.IsOBH == isOBH);
            }
            var queryObhBuyDocument = from sur in surcharge
                                      join cstd in csTransDe on sur.Hblid equals cstd.Id
                                      join cst in csTrans on cstd.JobId equals cst.Id
                                      join creditN in creditNote on sur.CreditNo equals creditN.Code into creditN2
                                      from creditN in creditN2.DefaultIfEmpty()
                                      join chg in charge on sur.ChargeId equals chg.Id into chg2
                                      from chg in chg2.DefaultIfEmpty()
                                      join user in sysUsers on cst.PersonIncharge equals user.Id into userGps
                                      from useGrp in userGps.DefaultIfEmpty()
                                      select new ChargeSOAResult
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
                                          Debit = null,
                                          Credit = sur.Total,
                                          SOANo = sur.PaySoano,
                                          IsOBH = true,
                                          Currency = sur.CurrencyId,
                                          InvoiceNo = sur.InvoiceNo,
                                          Note = sur.Notes,
                                          CustomerID = sur.PayerId,
                                          ServiceDate = (cst.TransactionType == "AI" || cst.TransactionType == "SFI" || cst.TransactionType == "SLI" || cst.TransactionType == "SCI" ? cst.Eta : cst.Etd),
                                          CreatedDate = cst.DatetimeCreated,
                                          InvoiceIssuedDate = creditN.DatetimeCreated,
                                          TransactionType = cst.TransactionType,
                                          UserCreated = cst.UserCreated,
                                          Quantity = sur.Quantity,
                                          UnitId = sur.UnitId,
                                          UnitPrice = sur.UnitPrice,
                                          VATRate = sur.Vatrate,
                                          CreditDebitNo = sur.CreditNo,
                                          DatetimeModified = sur.DatetimeModified,
                                          CommodityGroupID = null,
                                          Service = cst.TransactionType,
                                          CDNote = !string.IsNullOrEmpty(sur.CreditNo) ? sur.CreditNo : sur.DebitNo,
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
                                          CBM = cstd.Cbm,
                                          PackageContainer = cstd.PackageContainer,
                                          TypeCharge = chg.Type,
                                          PIC = useGrp.Username

                                      };
            queryObhBuyDocument = queryObhBuyDocument.Where(x => !string.IsNullOrEmpty(x.Service)).Where(query);
            if (isOBH != null)
            {
                queryObhBuyDocument = queryObhBuyDocument.Where(x => x.IsOBH == isOBH);
            }
            var queryObhBuy = queryObhBuyOperation.Union(queryObhBuyDocument);
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

        public IQueryable<ChargeSOAResult> GetChargeShipmentDocAndOperation(Expression<Func<ChargeSOAResult, bool>> query, bool? isOBH)
        {
            var charge = catChargeRepo.Get();
            var unit = catUnitRepo.Get();

            //BUY & SELL
            var queryBuySell = GetChargeBuySell(query);

            //OBH Receiver (SELL - Credit)
            var queryObhSell = GetChargeOBHSell(query, isOBH);

            //OBH Payer (BUY - Credit)
            var queryObhBuy = GetChargeOBHBuy(query, isOBH);

            //Merge data
            var dataMerge = queryBuySell.Union(queryObhBuy).Union(queryObhSell);

            var queryData = from data in dataMerge
                            join uni in unit on data.UnitId equals uni.Id into uni2
                            from uni in uni2.DefaultIfEmpty()
                            select new ChargeSOAResult
                            {
                                ID = data.ID,
                                HBLID = data.HBLID,
                                ChargeID = data.ChargeID,
                                ChargeCode = data.ChargeCode,
                                ChargeName = data.ChargeName,
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
                                CustomNo = GetTopClearanceNoByJobNo(data.JobId),
                                CDNote = data.CDNote,
                                FlightNo = data.FlightNo,
                                ShippmentDate = data.ShippmentDate,
                                AOL = data.AOL,
                                AOD = data.AOD,
                                PackageQty = data.PackageQty,
                                GrossWeight = data.GrossWeight,
                                ChargeWeight = data.ChargeWeight,
                                FinalExchangeRate = data.FinalExchangeRate,
                                ExchangeDate = data.ExchangeDate,
                                CBM = data.CBM,
                                PackageContainer = data.PackageContainer,
                                TypeCharge = data.TypeCharge,
                                PIC = data.PIC
                            };
            queryData = queryData.ToArray().OrderBy(x => x.Service).AsQueryable();
            return queryData;
        }
        #endregion -- Get Data Charge Master --

        #region -- Get List Charges Shipment By Criteria --
        private IQueryable<ChargeShipmentModel> GetChargesShipmentByCriteria(ChargeShipmentCriteria criteria)
        {
            Expression<Func<ChargeSOAResult, bool>> query = chg =>
                    string.IsNullOrEmpty(chg.SOANo)
                && chg.CustomerID == criteria.CustomerID;

            if (string.IsNullOrEmpty(criteria.DateType) || criteria.DateType == "CreatedDate")
            {
                query = query.And(chg =>
                    chg.CreatedDate.HasValue ? chg.CreatedDate.Value.Date >= criteria.FromDate.Date && chg.CreatedDate.Value.Date <= criteria.ToDate.Date : false);
            }
            else if (criteria.DateType == "ServiceDate")
            {
                query = query.And(chg =>
                    chg.ServiceDate.HasValue ? chg.ServiceDate.Value.Date >= criteria.FromDate.Date && chg.ServiceDate.Value.Date <= criteria.ToDate.Date : false);
            }
            else if (criteria.DateType == "InvoiceIssuedDate")
            {
                query = query.And(chg =>
                    chg.InvoiceIssuedDate.HasValue ? chg.InvoiceIssuedDate.Value.Date >= criteria.FromDate.Date && chg.InvoiceIssuedDate.Value.Date <= criteria.ToDate.Date : false);
            }

            if (!string.IsNullOrEmpty(criteria.Type))
            {
                if (criteria.Type == "Debit")
                {
                    query = query.And(chg => chg.Debit.HasValue);
                }
                if (criteria.Type == "Credit")
                {
                    query = query.And(chg => chg.Credit.HasValue);
                }
            }

            if (!string.IsNullOrEmpty(criteria.StrCreators) && criteria.StrCreators != "All")
            {
                var listCreator = criteria.StrCreators.Split(',').Where(x => x.ToString() != string.Empty).ToList();
                query = query.And(chg => listCreator.Contains(chg.UserCreated));
            }

            if (!string.IsNullOrEmpty(criteria.StrCharges) && criteria.StrCharges != "All")
            {
                var listCharge = criteria.StrCharges.Split(',').Where(x => x.ToString() != string.Empty).ToList();
                query = query.And(chg => listCharge.Contains(chg.ChargeCode));
            }

            if (!string.IsNullOrEmpty(criteria.StrServices) && criteria.StrServices != "All")
            {
                var listService = criteria.StrServices.Split(',').Where(x => x.ToString() != string.Empty).ToList();
                query = query.And(chg => listService.Contains(chg.Service));
            }

            if (criteria.CommodityGroupID != null)
            {
                query = query.And(chg => criteria.CommodityGroupID == chg.CommodityGroupID);
            }

            if (criteria.JobIds != null && criteria.JobIds.Count > 0)
            {
                query = query.And(chg => criteria.JobIds.Contains(chg.JobId, StringComparer.OrdinalIgnoreCase));
            }

            if (criteria.Hbls != null && criteria.Hbls.Count > 0)
            {
                query = query.And(chg => criteria.Hbls.Contains(chg.HBL, StringComparer.OrdinalIgnoreCase));
            }

            if (criteria.Mbls != null && criteria.Mbls.Count > 0)
            {
                query = query.And(chg => criteria.Mbls.Contains(chg.MBL, StringComparer.OrdinalIgnoreCase));
            }

            if (criteria.CustomNo != null && criteria.CustomNo.Count > 0)
            {
                query = query.And(chg => criteria.CustomNo.Contains(chg.CustomNo, StringComparer.OrdinalIgnoreCase));
            }

            var charge = GetChargeShipmentDocAndOperation(query, criteria.IsOBH);
            
            var data = new List<ChargeShipmentModel>();
            foreach (var item in charge)
            {
                var chg = new ChargeShipmentModel();
                chg.ID = item.ID;
                chg.ChargeCode = item.ChargeCode;
                chg.ChargeName = item.ChargeName;
                chg.JobId = item.JobId;
                chg.HBL = item.HBL;
                chg.MBL = item.MBL;
                chg.CustomNo = item.CustomNo;
                chg.Type = item.Type;
                chg.InvoiceNo = item.InvoiceNo;
                chg.ServiceDate = item.ServiceDate;
                chg.Note = item.Note;
                chg.Debit = item.Debit;
                chg.Credit = item.Credit;
                chg.Currency = item.Currency;
                chg.CurrencyToLocal = criteria.CurrencyLocal;
                chg.CurrencyToUSD = AccountingConstants.CURRENCY_USD;
                var _exchangeRateLocal = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.Currency, criteria.CurrencyLocal);
                chg.AmountDebitLocal = _exchangeRateLocal * (chg.Debit ?? 0);
                chg.AmountCreditLocal = _exchangeRateLocal * (chg.Credit ?? 0);
                var _exchangeRateUSD = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.Currency, AccountingConstants.CURRENCY_USD);
                chg.AmountDebitUSD = _exchangeRateUSD * (chg.Debit ?? 0);
                chg.AmountCreditUSD = _exchangeRateUSD * (chg.Credit ?? 0);
                chg.SOANo = item.SOANo;
                chg.DatetimeModifiedSurcharge = item.DatetimeModified;
                chg.CDNote = item.CDNote;

                data.Add(chg);
            }
            //Sort Array sẽ nhanh hơn
            var result = data.ToArray().OrderByDescending(x => x.DatetimeModifiedSurcharge).AsQueryable();
            return result;
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

        #region -- Get List More Charges & Add More Charge Shipment By Criteria --
        private IQueryable<ChargeShipmentModel> GetMoreChargesShipmentByCriteria(MoreChargeShipmentCriteria criteria)
        {
            Expression<Func<ChargeSOAResult, bool>> query = chg => chg.CustomerID == criteria.CustomerID;

            if (criteria.InSoa == true)
            {
                query = query.And(chg => !string.IsNullOrEmpty(chg.SOANo));
            }
            else
            {
                query = query.And(chg => string.IsNullOrEmpty(chg.SOANo));
            }

            if (string.IsNullOrEmpty(criteria.DateType) || criteria.DateType == "CreatedDate")
            {
                query = query.And(chg =>
                    chg.CreatedDate.HasValue ? chg.CreatedDate.Value.Date >= criteria.FromDate.Date && chg.CreatedDate.Value.Date <= criteria.ToDate.Date : 1 == 2
                );
            }
            else if (criteria.DateType == "ServiceDate")
            {
                query = query.And(chg =>
                    chg.ServiceDate.HasValue ? chg.ServiceDate.Value.Date >= criteria.FromDate.Date && chg.ServiceDate.Value.Date <= criteria.ToDate.Date : 1 == 2
                );
            }
            else if (criteria.DateType == "InvoiceIssuedDate")
            {
                query = query.And(chg =>
                    chg.InvoiceIssuedDate.HasValue ? chg.InvoiceIssuedDate.Value.Date >= criteria.FromDate.Date && chg.InvoiceIssuedDate.Value.Date <= criteria.ToDate.Date : 1 == 2
                );
            }

            if (!string.IsNullOrEmpty(criteria.Type))
            {
                if (criteria.Type == "Debit")
                {
                    query = query.And(chg => chg.Debit.HasValue);
                }
                if (criteria.Type == "Credit")
                {
                    query = query.And(chg => chg.Credit.HasValue);
                }
            }

            if (!string.IsNullOrEmpty(criteria.StrCreators) && criteria.StrCreators != "All")
            {
                var listCreator = criteria.StrCreators.Split(',').Where(x => x.ToString() != string.Empty).ToList();
                query = query.And(chg => listCreator.Contains(chg.UserCreated));
            }

            if (!string.IsNullOrEmpty(criteria.StrCharges) && criteria.StrCharges != "All")
            {
                var listCharge = criteria.StrCharges.Split(',').Where(x => x.ToString() != string.Empty).ToList();
                query = query.And(chg => listCharge.Contains(chg.ChargeCode));
            }

            if (!string.IsNullOrEmpty(criteria.JobId))
            {
                query = query.And(chg => chg.JobId == criteria.JobId);
            }

            if (!string.IsNullOrEmpty(criteria.Hbl))
            {
                query = query.And(chg => chg.HBL == criteria.Hbl);
            }

            if (!string.IsNullOrEmpty(criteria.Mbl))
            {
                query = query.And(chg => chg.MBL == criteria.Mbl);
            }

            if (!string.IsNullOrEmpty(criteria.CDNote))
            {
                query = query.And(chg => chg.CreditDebitNo == criteria.CDNote);
            }

            if (!string.IsNullOrEmpty(criteria.StrServices) && criteria.StrServices != "All")
            {
                var listService = criteria.StrServices.Split(',').Where(x => x.ToString() != string.Empty).ToList();
                query = query.And(chg => listService.Contains(chg.Service));
            }

            if (criteria.CommodityGroupID != null)
            {
                query = query.And(chg => criteria.CommodityGroupID == chg.CommodityGroupID);
            }

            var charge = GetChargeShipmentDocAndOperation(query, criteria.IsOBH);

            /*var data = charge.Select(chg => new ChargeShipmentModel
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
                CurrencyToUSD = AccountingConstants.CURRENCY_USD,
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
                            (GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, AccountingConstants.CURRENCY_USD) > 0
                            ?
                                GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, AccountingConstants.CURRENCY_USD)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, AccountingConstants.CURRENCY_USD)) * (chg.Debit != null ? chg.Debit.Value : 0),
                AmountCreditUSD =
                            (GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, AccountingConstants.CURRENCY_USD) > 0
                            ?
                                GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, AccountingConstants.CURRENCY_USD)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, AccountingConstants.CURRENCY_USD)) * (chg.Credit != null ? chg.Credit.Value : 0),
                SOANo = chg.SOANo,
                DatetimeModifiedSurcharge = chg.DatetimeModified
            });*/
            var data = new List<ChargeShipmentModel>();
            foreach (var item in charge)
            {
                var chg = new ChargeShipmentModel();
                chg.ID = item.ID;
                chg.ChargeCode = item.ChargeCode;
                chg.ChargeName = item.ChargeName;
                chg.JobId = item.JobId;
                chg.HBL = item.HBL;
                chg.MBL = item.MBL;
                chg.CustomNo = item.CustomNo;
                chg.Type = item.Type;
                chg.InvoiceNo = item.InvoiceNo;
                chg.ServiceDate = item.ServiceDate;
                chg.Note = item.Note;
                chg.Debit = item.Debit;
                chg.Credit = item.Credit;
                chg.Currency = item.Currency;
                chg.CurrencyToLocal = criteria.CurrencyLocal;
                chg.CurrencyToUSD = AccountingConstants.CURRENCY_USD;
                var _exchangeRateLocal = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.Currency, criteria.CurrencyLocal);
                chg.AmountDebitLocal = _exchangeRateLocal * (chg.Debit ?? 0);
                chg.AmountCreditLocal = _exchangeRateLocal * (chg.Credit ?? 0);
                var _exchangeRateUSD = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.Currency, AccountingConstants.CURRENCY_USD);
                chg.AmountDebitUSD = _exchangeRateUSD * (chg.Debit ?? 0);
                chg.AmountCreditUSD = _exchangeRateUSD * (chg.Credit ?? 0);
                chg.SOANo = item.SOANo;
                chg.DatetimeModifiedSurcharge = item.DatetimeModified;
                chg.CDNote = item.CDNote;

                data.Add(chg);
            }
            //Sort Array sẽ nhanh hơn
            var result = data.ToArray().OrderByDescending(x => x.DatetimeModifiedSurcharge).AsQueryable();
            return result;
        }

        public IQueryable<ChargeShipmentModel> GetListMoreCharge(MoreChargeShipmentCriteria criteria)
        {
            var moreChargeShipmentList = GetMoreChargesShipmentByCriteria(criteria);

            List<Surcharge> Surcharges = new List<Surcharge>();
            if (criteria.ChargeShipments != null)
            {
                foreach (var item in criteria.ChargeShipments.Where(x => string.IsNullOrEmpty(x.SOANo)).ToList())
                {
                    Surcharges.Add(new Surcharge { surchargeId = item.ID, type = item.Type });
                }
            }

            //Lấy ra các charge chưa tồn tại trong list criteria.Surcharges(Các Id của charge đã có trong kết quả search ở form info)
            var charge = moreChargeShipmentList.Where(x => Surcharges != null
                                                         && !Surcharges.Where(c => c.surchargeId == x.ID && c.type == x.Type).Any());
            return charge;
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
                    data.GroupShipments = null;
                    data.ChargeShipments = criteria.ChargeShipmentsCurrent;
                    data.AmountDebitLocal = criteria.ChargeShipmentsCurrent.Sum(x => x.AmountDebitLocal);
                    data.AmountCreditLocal = criteria.ChargeShipmentsCurrent.Sum(x => x.AmountCreditLocal);
                    data.AmountDebitUSD = criteria.ChargeShipmentsCurrent.Sum(x => x.AmountDebitUSD);
                    data.AmountCreditUSD = criteria.ChargeShipmentsCurrent.Sum(x => x.AmountCreditUSD);
                }
            }
            return data;
        }
        #endregion -- Get List More Charges & Add More Charge Shipment By Criteria --

        #region -- Get List & Paging SOA By Criteria --
        private IQueryable<AcctSOAResult> QueryDataListSOA(IQueryable<AcctSoa> soas)
        {
            var partner = catPartnerRepo.Get();
            var resultData = from s in soas
                             join pat in partner on s.Customer equals pat.Id into pat2
                             from pat in pat2.DefaultIfEmpty()
                             join ucreate in sysUserRepo.Get() on s.UserCreated equals ucreate.Id into ucreate2
                             from ucreate in ucreate2.DefaultIfEmpty()
                             join umodifies in sysUserRepo.Get() on s.UserModified equals umodifies.Id into umodifies2
                             from umodifies in umodifies2.DefaultIfEmpty()
                             select new AcctSOAResult
                             {
                                 Id = s.Id,
                                 Soano = s.Soano,
                                 Shipment = s.TotalShipment,
                                 PartnerName = pat.ShortName,
                                 Currency = s.Currency.Trim(),
                                 CreditAmount = s.CreditAmount,
                                 DebitAmount = s.DebitAmount,
                                 TotalAmount = s.DebitAmount - s.CreditAmount,
                                 Status = s.Status,
                                 DatetimeCreated = s.DatetimeCreated,
                                 UserCreated = ucreate.Username,
                                 DatetimeModified = s.DatetimeModified,
                                 UserModified = umodifies.Username,
                                 PaymentStatus = s.PaymentStatus,
                                 SyncStatus = s.SyncStatus,
                                 LastSyncDate = s.LastSyncDate
                             };
            //Sort Array sẽ nhanh hơn
            resultData = resultData.ToArray().OrderByDescending(x => x.DatetimeModified).AsQueryable();
            return resultData;
        }

        private IQueryable<AcctSoa> GetSoasPermission()
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSOA);
            PermissionRange _permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (_permissionRange == PermissionRange.None) return null;

            IQueryable<AcctSoa> soas = null;
            switch (_permissionRange)
            {
                case PermissionRange.None:
                    break;
                case PermissionRange.All:
                    soas = DataContext.Get();
                    break;
                case PermissionRange.Owner:
                    soas = DataContext.Get(x => x.UserCreated == _user.UserID);
                    break;
                case PermissionRange.Group:
                    soas = DataContext.Get(x => x.GroupId == _user.GroupId
                                            && x.DepartmentId == _user.DepartmentId
                                            && x.OfficeId == _user.OfficeID
                                            && x.CompanyId == _user.CompanyID);
                    break;
                case PermissionRange.Department:
                    soas = DataContext.Get(x => x.DepartmentId == _user.DepartmentId
                                            && x.OfficeId == _user.OfficeID
                                            && x.CompanyId == _user.CompanyID);
                    break;
                case PermissionRange.Office:
                    soas = DataContext.Get(x => x.OfficeId == _user.OfficeID
                                            && x.CompanyId == _user.CompanyID);
                    break;
                case PermissionRange.Company:
                    soas = DataContext.Get(x => x.CompanyId == _user.CompanyID);
                    break;
            }
            return soas;
        }

        private IQueryable<AcctSOAResult> GetDatas(AcctSOACriteria criteria, IQueryable<AcctSoa> soas)
        {
            if (soas == null) return null;

            if (!string.IsNullOrEmpty(criteria.StrCodes))
            {
                //Chỉ lấy ra những charge có SOANo (Để hạn chế việc join & get data không cần thiết)
                var listCode = criteria.StrCodes.Split(',').Where(x => x.ToString() != string.Empty).ToList();
                List<string> refNo = new List<string>();
                refNo = (from s in soas
                         join chg in csShipmentSurchargeRepo.Get() on s.Soano equals (chg.PaySoano ?? chg.Soano) into chg2
                         from chg in chg2.DefaultIfEmpty()
                         where
                                listCode.Contains(s.Soano, StringComparer.OrdinalIgnoreCase) 
                             || listCode.Contains(chg.JobNo, StringComparer.OrdinalIgnoreCase) 
                             || listCode.Contains(chg.Mblno, StringComparer.OrdinalIgnoreCase) 
                             || listCode.Contains(chg.Hblno, StringComparer.OrdinalIgnoreCase)
                         select s.Soano).ToList();
                soas = soas.Where(x => refNo.Contains(x.Soano));
            }

            if (!string.IsNullOrEmpty(criteria.CustomerID))
            {
                soas = soas.Where(x => x.Customer == criteria.CustomerID);
            }

            if (criteria.SoaFromDateCreate != null && criteria.SoaToDateCreate != null)
            {
                soas = soas.Where(x =>
                    x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date >= criteria.SoaFromDateCreate.Value.Date && x.DatetimeCreated.Value.Date <= criteria.SoaToDateCreate.Value.Date : 1 == 2
                );
            }

            if (!string.IsNullOrEmpty(criteria.SoaStatus))
            {
                soas = soas.Where(x => x.Status == criteria.SoaStatus);
            }

            if (!string.IsNullOrEmpty(criteria.SoaCurrency))
            {
                soas = soas.Where(x => x.Currency == criteria.SoaCurrency);
            }

            if (!string.IsNullOrEmpty(criteria.SoaUserCreate))
            {
                soas = soas.Where(x => x.UserCreated == criteria.SoaUserCreate);
            }

            var dataResult = QueryDataListSOA(soas);
            return dataResult;
        }

        public IQueryable<AcctSOAResult> QueryDataPermission(AcctSOACriteria criteria)
        {
            var soas = GetSoasPermission();
            return GetDatas(criteria, soas);
        }

        public IQueryable<AcctSOAResult> QueryData(AcctSOACriteria criteria)
        {
            var soas = DataContext.Get();
            return GetDatas(criteria, soas);
        }

        public IQueryable<AcctSOAResult> Paging(AcctSOACriteria criteria, int page, int size, out int rowsCount)
        {
            var data = QueryDataPermission(criteria);
            if (data == null)
            {
                rowsCount = 0;
                return null;
            }

            var _totalItem = data.Select(s => s.Id).Count();
            rowsCount = (_totalItem > 0) ? _totalItem : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }

            return data;
        }
        #endregion -- Get List & Paging SOA By Criteria --

        #region -- Details Soa --
        public bool CheckDetailPermission(string soaNo)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSOA);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);
            if (permissionRange == PermissionRange.None)
                return false;

            var detail = DataContext.Get(x => x.Soano == soaNo)?.FirstOrDefault();
            if (detail == null) return false;

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = detail.UserCreated,
                CompanyId = detail.CompanyId,
                DepartmentId = detail.DepartmentId,
                OfficeId = detail.OfficeId,
                GroupId = detail.GroupId
            };
            int code = PermissionExtention.GetPermissionCommonItem(baseModel, permissionRange, _user);

            if (code == 403) return false;

            return true;
        }

        private AcctSOADetailResult GetSoaBySoaNo(string soaNo)
        {
            var soa = DataContext.Get(x => x.Soano == soaNo);
            var partner = catPartnerRepo.Get();
            var resultData = from s in soa
                             join pat in partner on s.Customer equals pat.Id into pat2
                             from pat in pat2.DefaultIfEmpty()
                             select new AcctSOADetailResult
                             {
                                 Id = s.Id,
                                 Soano = s.Soano,
                                 Shipment = s.TotalShipment,
                                 PartnerName = pat.PartnerNameEn,
                                 Currency = s.Currency,
                                 CreditAmount = s.CreditAmount,
                                 DebitAmount = s.DebitAmount,
                                 TotalAmount = s.DebitAmount - s.CreditAmount,
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
                                 Customer = s.Customer,
                                 DateType = s.DateType,
                                 CreatorShipment = s.CreatorShipment,
                                 PaymentStatus = s.PaymentStatus,
                                 PaymentDueDate = s.PaymentDueDate,
                                 SyncStatus = s.SyncStatus,
                                 LastSyncDate = s.LastSyncDate
                             };
            var result = resultData.FirstOrDefault();
            if (result != null)
            {
                result.UserNameCreated = sysUserRepo.Get(x => x.Id == result.UserCreated).FirstOrDefault()?.Username;
                result.UserNameModified = sysUserRepo.Get(x => x.Id == result.UserModified).FirstOrDefault()?.Username;
            }
            return result;
        }

        private List<ChargeShipmentModel> GetListChargeOfSoa(IQueryable<ChargeSOAResult> charge, string soaNo, string currencyLocal)
        {
            /*var query = from chg in charge
                        select new ChargeShipmentModel
                        {
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
                            CurrencyToUSD = AccountingConstants.CURRENCY_USD,
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
                            AmountDebitUSD = (GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, AccountingConstants.CURRENCY_USD) > 0
                            ?
                                GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, AccountingConstants.CURRENCY_USD)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, AccountingConstants.CURRENCY_USD)) * (chg.Debit != null ? chg.Debit.Value : 0),
                            AmountCreditUSD = (GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, AccountingConstants.CURRENCY_USD) > 0
                            ?
                                GetRateCurrencyExchange(chg.CreatedDate, chg.Currency, AccountingConstants.CURRENCY_USD)
                            :
                                GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, AccountingConstants.CURRENCY_USD)) * (chg.Credit != null ? chg.Credit.Value : 0),
                        };
            return query;*/

            var data = new List<ChargeShipmentModel>();
            foreach (var item in charge)
            {
                var chg = new ChargeShipmentModel();
                chg.SOANo = item.SOANo;
                chg.ID = item.ID;
                chg.ChargeCode = item.ChargeCode;
                chg.ChargeName = item.ChargeName;
                chg.JobId = item.JobId;
                chg.HBL = item.HBL;
                chg.MBL = item.MBL;
                chg.CustomNo = item.CustomNo;
                chg.Type = item.Type;
                chg.Debit = item.Debit;
                chg.Credit = item.Credit;
                chg.Currency = item.Currency;
                chg.InvoiceNo = item.InvoiceNo;
                chg.ServiceDate = item.ServiceDate;
                chg.Note = item.Note;
                chg.CurrencyToLocal = currencyLocal;
                chg.CurrencyToUSD = AccountingConstants.CURRENCY_USD;
                var _exchangeRateLocal = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.Currency, currencyLocal);
                chg.AmountDebitLocal = _exchangeRateLocal * (chg.Debit ?? 0);
                chg.AmountCreditLocal = _exchangeRateLocal * (chg.Credit ?? 0);
                var _exchangeRateUSD = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.Currency, AccountingConstants.CURRENCY_USD);
                chg.AmountDebitUSD = _exchangeRateUSD * (chg.Debit ?? 0);
                chg.AmountCreditUSD = _exchangeRateUSD * (chg.Credit ?? 0);
                chg.DatetimeModifiedSurcharge = item.DatetimeModified;
                chg.CDNote = item.CDNote;
                chg.PIC = item.PIC;
                data.Add(chg);
            }
            return data;
        }

        public AcctSOADetailResult GetDetailBySoaNoAndCurrencyLocal(string soaNo, string currencyLocal)
        {
            var data = new AcctSOADetailResult();
            var soaDetail = GetSoaBySoaNo(soaNo);
            if (soaDetail == null)
            {
                return data;
            }
            //Chỉ lấy ra những charge có SOANo == soaNo (Để hạn chế việc join & get data không cần thiết)
            Expression<Func<ChargeSOAResult, bool>> query = chg => chg.SOANo == soaNo;
            var charge = GetChargeShipmentDocAndOperation(query, null);
            var chargeShipments = GetListChargeOfSoa(charge, soaNo, currencyLocal);
            var _groupShipments = new List<GroupShipmentModel>();
            _groupShipments = chargeShipments.GroupBy(g => new { g.JobId, g.HBL, g.MBL, g.PIC })
            .Select(s => new GroupShipmentModel
            {
                PIC = s.Key.PIC,
                JobId = s.Key.JobId,
                HBL = s.Key.HBL,
                MBL = s.Key.MBL,
                TotalCredit = string.Join(" | ", s.ToList().GroupBy(gr => new { gr.Currency }).Select(se => string.Format("{0:#,##0.###}", se.Sum(su => su.Credit)) + " " + se.Key.Currency).ToList()),
                TotalDebit = string.Join(" | ", s.ToList().GroupBy(gr => new { gr.Currency }).Select(se => string.Format("{0:#,##0.###}", se.Sum(su => su.Debit)) + " " + se.Key.Currency).ToList()),
                ChargeShipments = s.ToList()
            }).ToList();
            data = soaDetail;
            data.GroupShipments = _groupShipments.ToArray().OrderByDescending(o => o.JobId).ToList(); //Sắp xếp giảm dần theo số Job
            data.ChargeShipments = chargeShipments.ToArray().OrderByDescending(o => o.JobId).ToList(); //Sắp xếp giảm dần theo số Job
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
            var serviceName = string.Empty;

            if (!string.IsNullOrEmpty(serviceTypeId))
            {
                //Tách chuỗi servicetype thành mảng
                string[] arrayStrServiceTypeId = serviceTypeId.Split(';').Where(x => x.ToString() != string.Empty).ToArray();

                //Xóa các serviceTypeId trùng
                string[] arrayGrpServiceTypeId = arrayStrServiceTypeId.Distinct<string>().ToArray();

                foreach (var item in arrayGrpServiceTypeId)
                {
                    //Lấy ra DisplayName của serviceTypeId
                    serviceName += Common.CustomData.Services.Where(x => x.Value == item).FirstOrDefault() != null ?
                                Common.CustomData.Services.Where(x => x.Value == item).FirstOrDefault().DisplayName.Trim() + ";"
                                : string.Empty;
                }
                serviceName = (serviceName + ")").Replace(";)", string.Empty);
            }
            return serviceName;
        }
        #endregion --Details Soa--

        #region -- Data Export Details --
        public IQueryable<ExportSOAModel> GetDateExportDetailSOA(string soaNo)
        {
            //Lấy danh sách Currency Exchange của ngày hiện tại
            //var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();
            var soa = DataContext.Get(x => x.Soano == soaNo);

            Expression<Func<ChargeSOAResult, bool>> query = chg => chg.SOANo == soaNo;
            var charge = GetChargeShipmentDocAndOperation(query, null);
            var partner = catPartnerRepo.Get();
            var dataResult = from s in soa
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
                                 FinalExchangeRate = chg.FinalExchangeRate,
                                 ExchangeDate = chg.ExchangeDate,
                                 //CreditExchange = (GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency) > 0
                                 //?
                                 //    GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency)
                                 //:
                                 //    GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, s.Currency)) * (chg.Credit != null ? chg.Credit.Value : 0),
                                 //DebitExchange = (GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency) > 0
                                 //?
                                 //    GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency)
                                 //:
                                 //    GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, s.Currency)) * (chg.Debit != null ? chg.Debit.Value : 0),
                             };
            List<ExportSOAModel> _result = dataResult.ToList();
            _result.ForEach(fe =>
            {
                var _exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(fe.FinalExchangeRate, fe.ExchangeDate, fe.CurrencyCharge, fe.CurrencySOA);
                fe.CreditExchange = _exchangeRate * (fe.Credit ?? 0);
                fe.DebitExchange = _exchangeRate * (fe.Debit ?? 0);
            });
            return _result.AsQueryable();
        }



        public ExportSOAAirfreightModel GetSoaAirFreightBySoaNo(string soaNo, string officeId)
        {
            var soa = DataContext.Get(x => x.Soano == soaNo);
            var partner = catPartnerRepo.Get();
            var port = catPlaceRepo.Get();
            var resultData = from s in soa
                             join pat in partner on s.Customer equals pat.Id into pat2
                             from pat in pat2.DefaultIfEmpty()
                             select new ExportSOAAirfreightModel
                             {
                                 PartnerNameEn = pat.PartnerNameEn,
                                 PartnerBillingAddress = pat.AddressEn,
                                 PartnerTaxCode = pat.TaxCode,
                                 SoaNo = s.Soano,
                                 DateSOA = s.DatetimeCreated,
                                 IssuedBy = s.UserCreated,
                                 SoaFromDate = s.SoaformDate
                             };
            //information Partner
            var result = resultData.FirstOrDefault();
            if (result != null)
            {
                result.IssuedBy = sysUserRepo.Get(x => x.Id == result.IssuedBy).FirstOrDefault()?.Username;
            }

            Expression<Func<ChargeSOAResult, bool>> query = chg => chg.SOANo == soaNo;
            var charge = GetChargeShipmentDocAndOperation(query, null).Where(x => x.TransactionType == "AI" || x.TransactionType == "AE");
            var results = charge.GroupBy(x => x.HBL).AsQueryable();

            if (results.Select(x => x.Key).Count() > 0)
            {
                result.HawbAirFrieghts = new List<HawbAirFrieghtModel>();
                foreach (var item in results.Select(x => x.Key))
                {
                    var chargeData = charge.Where(x => x.HBL == item).FirstOrDefault();
                    HawbAirFrieghtModel air = new HawbAirFrieghtModel();
                    air.JobNo = chargeData.JobId;
                    air.FlightNo = chargeData.FlightNo;
                    air.ShippmentDate = chargeData.ShippmentDate;
                    air.AOL = port.Where(x => x.Id == chargeData.AOL).Select(t => t.Code).FirstOrDefault();
                    air.Mawb = chargeData.MBL.Substring(0, 3) + "-" + chargeData.MBL.Substring(chargeData.MBL.Length - 9);
                    air.AOD = port.Where(x => x.Id == chargeData.AOD).Select(t => t.Code).FirstOrDefault();
                    air.Service = "Normal"; // tạm thời hardcode;
                    air.Pcs = chargeData.PackageQty;
                    air.CW = chargeData.ChargeWeight;
                    air.GW = chargeData.GrossWeight;
                    air.Rate = chargeData.ChargeName == AccountingConstants.CHARGE_AIR_FREIGHT ? chargeData.UnitPrice : null;


                    var lstAirfrieght = charge.Where(x => x.HBL == item && x.ChargeCode == AccountingConstants.CHARGE_AIR_FREIGHT_CODE && x.Currency == AccountingConstants.CURRENCY_USD);
                    if (lstAirfrieght.Count() == 0)
                    {
                        lstAirfrieght = charge.Where(x => x.HBL == item && (x.ChargeName.ToLower() == AccountingConstants.CHARGE_AIR_FREIGHT.ToLower()) && x.Currency == AccountingConstants.CURRENCY_USD);
                    }
                    air.AirFreight = lstAirfrieght.Count() > 0 ? lstAirfrieght.Select(t => t.Debit).Sum() : null;

                    var lstFuelSurcharge = charge.Where(x => x.HBL == item && x.ChargeCode == AccountingConstants.CHARGE_FUEL_SURCHARGE_CODE && x.Currency == AccountingConstants.CURRENCY_USD);
                    if (lstFuelSurcharge.Count() == 0)
                    {
                        lstFuelSurcharge = charge.Where(x => x.HBL == item && (x.ChargeName.ToLower() == AccountingConstants.CHARGE_FUEL_SURCHARGE.ToLower()) && x.Currency == AccountingConstants.CURRENCY_USD);
                    }
                    air.FuelSurcharge = lstFuelSurcharge.Count() > 0 ? lstFuelSurcharge.Select(t => t.Debit).Sum() : null;


                    var lstWariskSurcharge = charge.Where(x => x.HBL == item && x.ChargeCode == AccountingConstants.CHARGE_WAR_RISK_SURCHARGE_CODE && x.Currency == AccountingConstants.CURRENCY_USD);
                    if (lstWariskSurcharge.Count() == 0)
                    {
                        lstWariskSurcharge = charge.Where(x => x.HBL == item && (x.ChargeName.ToLower() == AccountingConstants.CHARGE_WAR_RISK_SURCHARGE.ToLower()) && x.Currency == AccountingConstants.CURRENCY_USD);
                    }
                    air.WarriskSurcharge = lstWariskSurcharge.Count() > 0 ? lstWariskSurcharge.Select(t => t.Debit).Sum() : null;

                    var lstScreeningFee = charge.Where(x => x.HBL == item && x.ChargeCode == AccountingConstants.CHARGE_SCREENING_CODE && x.Currency == AccountingConstants.CURRENCY_USD);
                    if (lstScreeningFee.Count() == 0)
                    {
                        lstScreeningFee = charge.Where(x => x.HBL == item && (x.ChargeName.ToLower() == AccountingConstants.CHARGE_SCREENING_FEE.ToLower() || x.ChargeName.ToLower() == "x-ray charge") && x.Currency == AccountingConstants.CURRENCY_USD);
                    }
                    air.ScreeningFee = lstScreeningFee.Count() > 0 ? lstScreeningFee.Select(t => t.Debit).Sum() : null;

                    var lstAWBFee = charge.Where(x => x.HBL == item && x.ChargeCode == AccountingConstants.CHARGE_AWB_FEE_CODE && x.Currency == AccountingConstants.CURRENCY_USD);
                    if (lstAWBFee.Count() == 0)
                    {
                        lstAWBFee = charge.Where(x => x.HBL == item && (x.ChargeName.ToLower() == AccountingConstants.CHARGE_AWB_FEE.ToLower() || x.ChargeName.ToLower() == "air waybill") && x.Currency == AccountingConstants.CURRENCY_USD);
                    }
                    air.AWB = lstAWBFee.Count() > 0 ? lstAWBFee.Select(t => t.Debit).Sum() : null;


                    var lstAMSFee = charge.Where(x => x.HBL == item && x.ChargeCode == AccountingConstants.CHARGE_AMS_FEE_CODE && x.Currency == AccountingConstants.CURRENCY_USD);
                    if (lstAMSFee.Count() == 0)
                    {
                        lstAMSFee = charge.Where(x => x.HBL == item && (x.ChargeName.ToLower() == AccountingConstants.CHARGE_AMS_FEE.ToLower()) && x.Currency == AccountingConstants.CURRENCY_USD);
                    }
                    air.AMS = lstAMSFee.Count() > 0 ? lstAMSFee.Select(t => t.Debit).Sum() : null;

                    var lstDANFee = charge.Where(x => x.HBL == item && x.ChargeCode == AccountingConstants.CHARGE_SA_DAN_AIR_CODE && x.Currency == AccountingConstants.CURRENCY_USD);
                    if (lstDANFee.Count() == 0)
                    {
                        lstDANFee = charge.Where(x => x.HBL == item && (x.ChargeName.ToLower() == AccountingConstants.CHARGE_SA_DAN_AIR_FEE.ToLower()) && x.Currency == AccountingConstants.CURRENCY_USD);
                    }
                    air.DAN = lstDANFee.Count() > 0 ? lstDANFee.Select(t => t.Debit).Sum() : null;

                    var lstOTHFee = charge.Where(x => x.HBL == item && x.ChargeCode == AccountingConstants.CHARGE_SA_OTH_AIR_CODE && x.Currency == AccountingConstants.CURRENCY_USD);
                    if (lstOTHFee.Count() == 0)
                    {
                        lstOTHFee = charge.Where(x => x.HBL == item && (x.ChargeName.ToLower() == AccountingConstants.CHARGE_SA_OTH_FEE.ToLower()) && x.Currency == AccountingConstants.CURRENCY_USD);
                    }
                    if (lstOTHFee.Count() == 0)
                    {
                        lstOTHFee = charge.Where(x => x.HBL == item && (x.ChargeCode != AccountingConstants.CHARGE_AIR_FREIGHT_CODE
                                                                    && x.ChargeName.ToLower() != AccountingConstants.CHARGE_AIR_FREIGHT.ToLower()
                                                                    && x.ChargeCode != AccountingConstants.CHARGE_FUEL_SURCHARGE_CODE
                                                                    && x.ChargeName.ToLower() != AccountingConstants.CHARGE_FUEL_SURCHARGE.ToLower()
                                                                    && x.ChargeCode != AccountingConstants.CHARGE_WAR_RISK_SURCHARGE_CODE
                                                                    && x.ChargeName.ToLower() != AccountingConstants.CHARGE_WAR_RISK_SURCHARGE.ToLower()
                                                                    && x.ChargeCode != AccountingConstants.CHARGE_SCREENING_CODE
                                                                    && x.ChargeName.ToLower() != AccountingConstants.CHARGE_SCREENING_FEE.ToLower()
                                                                    && x.ChargeCode != AccountingConstants.CHARGE_AWB_FEE_CODE
                                                                    && x.ChargeName.ToLower() != AccountingConstants.CHARGE_AWB_FEE.ToLower()
                                                                    && x.ChargeCode != AccountingConstants.CHARGE_AMS_FEE_CODE
                                                                    && x.ChargeName.ToLower() != AccountingConstants.CHARGE_AMS_FEE.ToLower()
                                                                    && x.ChargeCode != AccountingConstants.CHARGE_SA_DAN_AIR_CODE
                                                                    && x.ChargeName.ToLower() != AccountingConstants.CHARGE_SA_DAN_AIR_FEE.ToLower()
                        ) && x.Currency == AccountingConstants.CURRENCY_USD);
                    }

                    air.OTH = lstOTHFee.Count() > 0 ? lstOTHFee.Select(t => t.Debit).Sum() : null;

                    var lstHandlingFee = charge.Where(x => x.HBL == item && x.ChargeName.ToLower() == AccountingConstants.CHARGE_HANDLING_FEE.ToLower() && x.Currency == AccountingConstants.CURRENCY_USD);
                    air.HandlingFee = lstHandlingFee.Count() > 0 ? lstHandlingFee.Select(t => t.Debit).Sum() : null;

                    air.NetAmount = 0;
                    if (air.AirFreight.HasValue)
                    {
                        air.NetAmount += air.AirFreight;
                    }
                    if (air.WarriskSurcharge.HasValue)
                    {
                        air.NetAmount += air.WarriskSurcharge;
                    }
                    if (air.ScreeningFee.HasValue)
                    {
                        air.NetAmount += air.ScreeningFee;
                    }
                    if (air.HandlingFee.HasValue)
                    {
                        air.NetAmount += air.HandlingFee;
                    }
                    if (air.AMS.HasValue)
                    {
                        air.NetAmount += air.AMS;
                    }
                    if (air.AWB.HasValue)
                    {
                        air.NetAmount += air.AWB;
                    }
                    if (air.DAN.HasValue)
                    {
                        air.NetAmount += air.DAN;
                    }
                    if (air.OTH.HasValue)
                    {
                        air.NetAmount += air.OTH;
                    }
                    if (air.FuelSurcharge.HasValue)
                    {
                        air.NetAmount += air.FuelSurcharge;
                    }
                    var dataCharge = charge.Where(x => x.ChargeName.ToLower() == AccountingConstants.CHARGE_AIR_FREIGHT.ToLower());
                    if (dataCharge.Any())
                    {
                        if (chargeData.FinalExchangeRate != null)
                        {
                            air.ExchangeRate = chargeData.FinalExchangeRate;
                        }
                        else
                        {
                            var dataCurrencyExchange = catCurrencyExchangeRepo.Get(x => x.CurrencyFromId == AccountingConstants.CURRENCY_USD && x.CurrencyToId == AccountingConstants.CURRENCY_LOCAL).OrderByDescending(x => x.DatetimeModified).AsQueryable();
                            var dataObjectCurrencyExchange = dataCurrencyExchange.Where(x => x.DatetimeModified.Value.Date == chargeData.DatetimeModified.Value.Date).FirstOrDefault();
                            air.ExchangeRate = dataObjectCurrencyExchange.Rate;
                        }
                    }
                    else
                    {
                        var dataCurrencyExchange = catCurrencyExchangeRepo.Get(x => x.CurrencyFromId == AccountingConstants.CURRENCY_USD && x.CurrencyToId == AccountingConstants.CURRENCY_LOCAL).OrderByDescending(x => x.DatetimeModified).AsQueryable();
                        var dataObjectCurrencyExchange = dataCurrencyExchange.Where(x => x.DatetimeModified.Value.Date == chargeData.DatetimeModified.Value.Date).FirstOrDefault();
                        air.ExchangeRate = dataObjectCurrencyExchange.Rate;
                    }

                    air.TotalAmount = air.NetAmount * air.ExchangeRate;

                    result.HawbAirFrieghts.Add(air);
                }
            }
            var officeData = officeRepo.Get(x => x.Id == new Guid(officeId)).FirstOrDefault();
            result.OfficeEn = officeData.BranchNameEn;
            result.BankAccountVND = officeData.BankAccountVnd;
            result.BankAccountUSD = officeData.BankAccountUsd;
            result.BankNameEn = officeData.BankNameEn;
            result.AddressEn = officeData.AddressEn;
            result.SwiftCode = officeData.SwiftCode;
            return result;
        }

        // Export SOA Supplier Cost 
        public ExportSOAAirfreightModel GetSoaSupplierAirFreightBySoaNo(string soaNo, string officeId)
        {
            var soa = DataContext.Get(x => x.Soano == soaNo);
            var partner = catPartnerRepo.Get();
            var port = catPlaceRepo.Get();
            var resultData = from s in soa
                             join pat in partner on s.Customer equals pat.Id into pat2
                             from pat in pat2.DefaultIfEmpty()
                             select new ExportSOAAirfreightModel
                             {
                                 PartnerNameEn = pat.PartnerNameEn,
                                 PartnerBillingAddress = pat.AddressEn,
                                 PartnerTaxCode = pat.TaxCode,
                                 SoaNo = s.Soano,
                                 DateSOA = s.SoaformDate,
                                 IssuedBy = s.UserCreated,
                             };
            // Partner information
            var result = resultData.FirstOrDefault();
            if (result != null)
            {
                result.IssuedBy = sysUserRepo.Get(x => x.Id == result.IssuedBy).FirstOrDefault()?.Username;
            }

            Expression<Func<ChargeSOAResult, bool>> query = chg => chg.SOANo == soaNo;
            var charge = GetChargeShipmentDocAndOperation(query, null).Where(x => x.TransactionType == "AI" || x.TransactionType == "AE");
            var results = charge.GroupBy(x => x.JobId).AsQueryable();
            var csTrans = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled);
            var csTransDe = csTransactionDetailRepo.Get();

            if (results.Select(x => x.Key).Count() > 0)
            {
                result.HawbAirFrieghts = new List<HawbAirFrieghtModel>();
                foreach (var item in results.Select(x => x.Key))
                {
                    var chargeData = charge.Where(x => x.JobId == item).FirstOrDefault();
                    HawbAirFrieghtModel air = new HawbAirFrieghtModel();
                    var cstrans = csTrans.Where(k => k.JobNo == chargeData.JobId).FirstOrDefault();
                    var transDetail = csTransDe.Where(x => x.JobId == cstrans.Id).FirstOrDefault();
                    air.JobNo = chargeData.JobId;
                    air.FlightNo = cstrans.FlightVesselName ?? transDetail.FlightNo;
                    air.ShippmentDate = chargeData.ShippmentDate;
                    air.AOL = port.Where(x => x.Id == cstrans.Pol).Select(t => t.Code).FirstOrDefault();
                    air.Mawb = chargeData.MBL;
                    air.AOD = port.Where(x => x.Id == cstrans.Pod).Select(t => t.Code).FirstOrDefault();
                    air.Pcs = chargeData.PackageQty;
                    air.CW = cstrans.ChargeWeight ?? transDetail.ChargeWeight;
                    air.GW = cstrans.GrossWeight ?? transDetail.GrossWeight;

                    var lstAirfrieght = charge.Where(x => x.JobId == item && (x.ChargeCode == AccountingConstants.CHARGE_BA_AIR_FREIGHT_CODE ||
                                        (x.TypeCharge.ToLower() == AccountingConstants.TYPE_SOA_CREDIT.ToLower() && x.ChargeName.ToLower() == AccountingConstants.CHARGE_AIR_FREIGHT.ToLower())));
                    // Rate
                    air.Rate = lstAirfrieght.Count() > 0 ? lstAirfrieght.Select(t => t.UnitPrice).Sum() : null;
                    // Air Freight
                    air.AirFreight = lstAirfrieght.Count() > 0 ? lstAirfrieght.Select(t => t.Credit).Sum() : null;

                    // Fuel Surcharge
                    var lstFuelSurcharge = charge.Where(x => x.JobId == item && (x.ChargeCode == AccountingConstants.CHARGE_BA_FUEL_SURCHARGE_CODE ||
                                            (x.TypeCharge.ToLower() == AccountingConstants.TYPE_SOA_CREDIT.ToLower() && x.ChargeName.ToLower() == AccountingConstants.CHARGE_FUEL_SURCHARGE.ToLower())));
                    air.FuelSurcharge = lstFuelSurcharge.Count() > 0 ? lstFuelSurcharge.Select(t => t.Credit).Sum() : null;

                    // War risk Surcharge
                    var lstWariskSurcharge = charge.Where(x => x.JobId == item && (x.ChargeCode == AccountingConstants.CHARGE_BA_WAR_RISK_SURCHARGE_CODE ||
                                            (x.TypeCharge.ToLower() == AccountingConstants.TYPE_SOA_CREDIT.ToLower() && x.ChargeName.ToLower() == AccountingConstants.CHARGE_WAR_RISK_SURCHARGE.ToLower())));
                    air.WarriskSurcharge = lstWariskSurcharge.Count() > 0 ? lstWariskSurcharge.Select(t => t.Credit).Sum() : null;

                    // Screening Fee
                    var lstScreeningFee = charge.Where(x => x.JobId == item && (x.ChargeCode == AccountingConstants.CHARGE_BA_SCREENING_CODE ||
                                        (x.TypeCharge.ToLower() == AccountingConstants.TYPE_SOA_CREDIT.ToLower() &&
                                        (x.ChargeName.ToLower() == AccountingConstants.CHARGE_SCREENING_FEE.ToLower() || x.ChargeName.ToLower() == AccountingConstants.CHARGE_X_RAY.ToLower()))));
                    air.ScreeningFee = lstScreeningFee.Count() > 0 ? lstScreeningFee.Select(t => t.Credit).Sum() : null;

                    // AWB
                    var lstAWBFee = charge.Where(x => x.JobId == item && (x.ChargeCode == AccountingConstants.CHARGE_BA_AWB_FEE_CODE ||
                                    (x.TypeCharge.ToLower() == AccountingConstants.TYPE_SOA_CREDIT.ToLower() &&
                                    (x.ChargeName.ToLower() == AccountingConstants.CHARGE_AWB_FEE.ToLower() || x.ChargeName.ToLower() == AccountingConstants.CHARGE_AWB.ToLower()))));
                    air.AWB = lstAWBFee.Count() > 0 ? lstAWBFee.Select(t => t.Credit).Sum() : null;

                    // AMS
                    var lstAMS = charge.Where(x => x.JobId == item && (x.ChargeCode == AccountingConstants.CHARGE_BA_AMS_FEE_CODE ||
                                (x.TypeCharge.ToLower() == AccountingConstants.TYPE_SOA_CREDIT.ToLower() && x.ChargeName.ToLower() == AccountingConstants.CHARGE_AMS_FEE.ToLower())));
                    air.AMS = lstAMS.Count() > 0 ? lstAMS.Select(t => t.Credit).Sum() : null;

                    // Dangerous Fee
                    var lstDangerousFee = charge.Where(x => x.JobId == item && (x.ChargeCode == AccountingConstants.CHARGE_BA_DAN_AIR_CODE ||
                                            (x.TypeCharge.ToLower() == AccountingConstants.TYPE_SOA_CREDIT.ToLower() && x.ChargeName.ToLower() == AccountingConstants.CHARGE_SA_DAN_AIR_FEE.ToLower())));
                    air.DAN = lstDangerousFee.Count() > 0 ? lstDangerousFee.Select(t => t.Credit).Sum() : null;

                    // Handling fee
                    var lstHandlingFee = charge.Where(x => x.JobId == item && (x.ChargeCode == AccountingConstants.CHARGE_BA_DHL_AIR_CODE || x.ChargeName.ToLower() == AccountingConstants.CHARGE_HANDLING_FEE.ToLower()));
                    air.HandlingFee = lstHandlingFee.Count() > 0 ? lstHandlingFee.Select(t => t.Credit).Sum() : null;

                    // Other Charges
                    var lstOtherChrg = charge.Where(x => x.JobId == item && (x.ChargeCode == AccountingConstants.CHARGE_BA_OTH_AIR_CODE ||
                                        (x.TypeCharge.ToLower() == AccountingConstants.TYPE_SOA_CREDIT.ToLower() && x.ChargeName.ToLower() == AccountingConstants.CHARGE_SA_OTH_FEE.ToLower())));
                    if (lstOtherChrg.Count() == 0)
                    {
                        lstOtherChrg = charge.Where(x => x.JobId == item && x.TypeCharge.ToLower() == AccountingConstants.TYPE_SOA_CREDIT.ToLower()).Except(lstAirfrieght);
                        lstOtherChrg = lstOtherChrg.Except(lstFuelSurcharge);
                        lstOtherChrg = lstOtherChrg.Except(lstWariskSurcharge);
                        lstOtherChrg = lstOtherChrg.Except(lstScreeningFee);
                        lstOtherChrg = lstOtherChrg.Except(lstAWBFee);
                        lstOtherChrg = lstOtherChrg.Except(lstAMS);
                        lstOtherChrg = lstOtherChrg.Except(lstDangerousFee);
                        lstOtherChrg = lstOtherChrg.Except(lstHandlingFee);
                    }
                    air.OTH = lstOtherChrg.Count() > 0 ? lstOtherChrg.Select(t => t.Credit).Sum() : null;

                    // Net Amount
                    air.NetAmount = 0;
                    if (air.AirFreight.HasValue)
                    {
                        air.NetAmount += air.AirFreight;
                    }
                    if (air.FuelSurcharge.HasValue)
                    {
                        air.NetAmount += air.FuelSurcharge;
                    }
                    if (air.WarriskSurcharge.HasValue)
                    {
                        air.NetAmount += air.WarriskSurcharge;
                    }
                    if (air.ScreeningFee.HasValue)
                    {
                        air.NetAmount += air.ScreeningFee;
                    }
                    if (air.AWB.HasValue)
                    {
                        air.NetAmount += air.AWB;
                    }
                    if (air.AMS.HasValue)
                    {
                        air.NetAmount += air.AMS;
                    }
                    if (air.DAN.HasValue)
                    {
                        air.NetAmount += air.DAN;
                    }
                    if (air.OTH.HasValue)
                    {
                        air.NetAmount += air.OTH;
                    }
                    if (air.HandlingFee.HasValue)
                    {
                        air.NetAmount += air.HandlingFee;
                    }

                    var _exchangeRateUSD = 0m;
                    if (chargeData.Currency != AccountingConstants.CURRENCY_USD)
                    {
                        _exchangeRateUSD = currencyExchangeService.CurrencyExchangeRateConvert(chargeData.FinalExchangeRate, chargeData.ExchangeDate, chargeData.Currency, AccountingConstants.CURRENCY_USD);
                        air.ExchangeRate = _exchangeRateUSD;
                        air.AirFreight = air.AirFreight * _exchangeRateUSD;
                        air.FuelSurcharge = air.FuelSurcharge * _exchangeRateUSD;
                        air.WarriskSurcharge = air.WarriskSurcharge * _exchangeRateUSD;
                        air.ScreeningFee = air.ScreeningFee * _exchangeRateUSD;
                        air.AWB = air.AWB * _exchangeRateUSD;
                        air.AMS = air.AMS * _exchangeRateUSD;
                        air.DAN = air.DAN * _exchangeRateUSD;
                        air.OTH = air.OTH * _exchangeRateUSD;
                        air.HandlingFee = air.HandlingFee * _exchangeRateUSD;
                        air.NetAmount = air.NetAmount * _exchangeRateUSD;
                    }
                    result.HawbAirFrieghts.Add(air);
                }
            }
            var officeData = officeRepo.Get(x => x.Id == new Guid(officeId)).FirstOrDefault();
            result.OfficeEn = officeData.BranchNameEn;
            result.BankAccountUSD = officeData.BankAccountUsd;
            result.BankAccountVND = officeData.BankAccountVnd;
            result.BankNameEn = officeData.BankNameEn;
            result.AddressEn = officeData.AddressEn;
            result.SwiftCode = officeData.SwiftCode;
            return result;
        }

        public SOAOPSModel GetSOAOPS(string soaNo)
        {
            SOAOPSModel opssoa = new SOAOPSModel();
            Expression<Func<ChargeSOAResult, bool>> query = chg => chg.SOANo == soaNo;
            var soa = DataContext.Get(x => x.Soano == soaNo);
            var charge = GetChargeShipmentDocAndOperation(query, null);
            if (soa?.FirstOrDefault().Type.ToLower() != AccountingConstants.TYPE_SOA_CREDIT.ToLower() &&
            soa?.FirstOrDefault().Type.ToLower() != AccountingConstants.TYPE_SOA_DEBIT.ToLower())
            {
                charge = charge.Where(x => x.TypeCharge.ToLower() == AccountingConstants.TYPE_SOA_DEBIT.ToLower() || x.TypeCharge.ToLower() == AccountingConstants.TYPE_SOA_OBH.ToLower());
            }
            List<ExportSOAOPS> lstSOAOPS = new List<ExportSOAOPS>();
            var partner = catPartnerRepo.Get();
            var port = catPlaceRepo.Get();
            var resultData = from s in soa
                             join pat in partner on s.Customer equals pat.Id into pat2
                             from pat in pat2.DefaultIfEmpty()
                             select new SOAOPSModel
                             {
                                 PartnerNameVN = pat.PartnerNameVn,
                                 BillingAddressVN = pat.AddressVn,
                                 FromDate = s.SoaformDate,
                                 ToDate = s.SoatoDate
                             };

            var results = charge.GroupBy(x => new { x.JobId, x.HBLID }).AsQueryable();

            foreach (var group in results)
            {
                ExportSOAOPS exportSOAOPS = new ExportSOAOPS();
                exportSOAOPS.Charges = new List<ChargeSOAResult>();
                var commodity = csTransactionRepo.Get(x => x.JobNo == group.Key.JobId).Select(t => t.Commodity).FirstOrDefault();
                var commodityGroup = opsTransactionRepo.Get(x => x.JobNo == group.Key.JobId).Select(t => t.CommodityGroupId).FirstOrDefault();
                string commodityName = string.Empty;
                if (commodity != null)
                {
                    string[] commodityArr = commodity.Split(',');
                    foreach (var item in commodityArr)
                    {
                        commodityName = commodityName + "," + catCommodityRepo.Get(x => x.CommodityNameEn == item.Replace("\n", "")).Select(t => t.CommodityNameEn).FirstOrDefault();
                    }
                    commodityName = commodityName.Substring(1);
                }
                if (commodityGroup != null)
                {
                    commodityName = catCommodityGroupRepo.Get(x => x.Id == commodityGroup).Select(t => t.GroupNameVn).FirstOrDefault();
                }
                exportSOAOPS.CommodityName = commodityName;

                exportSOAOPS.HwbNo = group.Select(t => t.HBL).FirstOrDefault();
                exportSOAOPS.CBM = group.Select(t => t.CBM).FirstOrDefault();
                exportSOAOPS.GW = group.Select(t => t.GrossWeight).FirstOrDefault();
                exportSOAOPS.PackageContainer = group.Select(t => t.PackageContainer).FirstOrDefault();
                exportSOAOPS.Charges.AddRange(group.Select(t => t));
                lstSOAOPS.Add(exportSOAOPS);
            }
            opssoa.exportSOAOPs = lstSOAOPS;
            opssoa.BillingAddressVN = resultData?.Select(t => t.BillingAddressVN).FirstOrDefault();
            opssoa.PartnerNameVN = resultData?.Select(t => t.PartnerNameVN).FirstOrDefault();
            opssoa.FromDate = resultData?.Select(t => t.FromDate).FirstOrDefault();

            foreach (var item in opssoa.exportSOAOPs)
            {
                foreach (var it in item.Charges)
                {
                    decimal? percent = 0;
                    if (it.VATRate > 0)
                    {
                        percent = (it.VATRate * 10) / 100;
                        it.VATAmount = percent * (it.UnitPrice * it.Quantity);
                        if (it.Currency != "VND")
                        {
                            it.VATAmount = Math.Round(it.VATAmount ?? 0, 3);

                        }
                    }
                    else
                    {
                        it.VATAmount = it.VATRate;
                    }

                    it.NetAmount = it.UnitPrice * it.Quantity;

                }

            }

            return opssoa;
        }

        public ExportSOADetailResult GetDataExportSOABySOANo(string soaNo, string currencyLocal)
        {
            var data = GetDateExportDetailSOA(soaNo);
            var result = new ExportSOADetailResult
            {
                ListCharges = data.ToList(),
                TotalDebitExchange = data.Where(x => x.DebitExchange != null).Sum(x => x.DebitExchange),
                TotalCreditExchange = data.Where(x => x.CreditExchange != null).Sum(x => x.CreditExchange)
            };
            return result;
        }

        #endregion -- Data Export Details --

        public IQueryable<ExportImportBravoFromSOAResult> GetDataExportImportBravoFromSOA(string soaNo)
        {
            //Lấy danh sách Currency Exchange của ngày hiện tại
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();
            var soa = DataContext.Get(x => x.Soano == soaNo);
            Expression<Func<ChargeSOAResult, bool>> query = chg => chg.SOANo == soaNo;
            var chargeDefaults = chargeDefaultRepo.Get(x => x.Type == "Công Nợ");
            var charge = GetChargeShipmentDocAndOperation(query, null);
            var partner = catPartnerRepo.Get();
            var dataResult = from s in soa
                             join chg in charge on s.Soano equals chg.SOANo into chg2
                             from chg in chg2.DefaultIfEmpty()
                             join pat in partner on s.Customer equals pat.Id into pat2
                             from pat in pat2.DefaultIfEmpty()
                             join cd in chargeDefaults on chg.ChargeID equals cd.ChargeId into defaults
                             from cd in defaults.DefaultIfEmpty()

                             select new ExportImportBravoFromSOAResult
                             {
                                 ServiceDate = chg.ServiceDate,
                                 SOANo = s.Soano,
                                 Service = GetServiceNameOfSoa(chg.Service).ToString(),
                                 PartnerCode = pat.TaxCode,
                                 Debit = cd.DebitAccountNo,
                                 Credit = cd.CreditAccountNo,
                                 ChargeCode = chg.ChargeCode,
                                 OriginalCurrency = chg.Currency,
                                 OriginalAmount = chg.Debit - chg.Credit,
                                 CreditExchange = (GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency) > 0
                                 ?
                                     GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency)
                                 :
                                     GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, s.Currency)) * (chg.Credit != null ? chg.Credit.Value : 0),
                                 AmountVND = chg.Credit * (chg.Debit - chg.Credit),
                                 VAT = chg.VATRate,
                                 AccountDebitNoVAT = cd.DebitAccountNo,
                                 AccountCreditNoVAT = cd.CreditAccountNo,
                                 AmountVAT = (chg.Debit - chg.Credit) * chg.VATRate,
                                 AmountVNDVAT = (chg.Credit * (chg.Debit - chg.Credit)) * chg.VATRate,
                                 Commodity = chg.Commodity,
                                 CustomerName = pat.PartnerNameVn,
                                 TaxCode = pat.TaxCode,
                                 JobId = chg.JobId,
                                 ChargeName = chg.ChargeName,
                                 TransationType = chg.TransactionType == "AI" || chg.TransactionType == "AE" ? "AIR" :
                                 chg.TransactionType == "SLE" || chg.TransactionType == "SFE" ||
                                 chg.TransactionType == "SFE" || chg.TransactionType == "SFI" ? "SEA" : "OPS",
                                 HBL = chg.HBL,
                                 Unit = chg.Unit,
                                 Payment = "TM/CK",
                                 Quantity = chg.Quantity,
                                 CustomerAddress = pat.AddressVn,
                                 MBL = chg.MBL,
                                 Email = pat.Email,
                                 TaxCodeOBH = chg.TaxCodeOBH,
                                 CustomNo = chg.CustomNo
                                 //CustomNo = chg.CustomNo,
                                 //CreditDebitNo = chg.CreditDebitNo,
                                 //CurrencySOA = s.Currency,
                                 //DebitExchange = (GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency) > 0
                                 //?
                                 //    GetRateCurrencyExchange(s.DatetimeModified, chg.Currency, s.Currency)
                                 //:
                                 //    GetRateLatestCurrencyExchange(currencyExchange, chg.Currency, s.Currency)) * (chg.Debit != null ? chg.Debit.Value : 0),
                             };


            return dataResult;
        }

        #region -- Preview --
        public Crystal PreviewAccountStatementFull(string soaNo)
        {
            //SOANo: type charge is SELL or OBH-SELL (DEBIT)
            //PaySOANo: type charge is BUY or OBH-BUY (CREDIT)
            Crystal result = null;
            var soa = DataContext.Get(x => x.Soano == soaNo).FirstOrDefault();
            if (soa == null) return null;
            var chargesOfSOA = csShipmentSurchargeRepo.Get(x => x.Soano == soaNo || x.PaySoano == soaNo);
            var partner = catPartnerRepo.Get(x => x.Id == soa.Customer).FirstOrDefault();
            var grpInvCdNoteByHbl = chargesOfSOA.GroupBy(g => new { g.Hblid, g.InvoiceNo, g.CreditNo, g.DebitNo }).Select(s => new { s.Key.Hblid, s.Key.InvoiceNo, CdNote = s.Key.CreditNo ?? s.Key.DebitNo });

            var soaCharges = new List<AccountStatementFullReport>();
            foreach (var charge in chargesOfSOA)
            {
                string _mawb = string.Empty;
                string _hwbNo = string.Empty;
                string _customNo = string.Empty;
                string _jobNo = string.Empty;
                #region -- Info MBL, HBL --
                var ops = opsTransactionRepo.Get(x => x.Hblid == charge.Hblid).FirstOrDefault();
                if (ops != null)
                {
                    _mawb = ops.Mblno;
                    _hwbNo = ops.Hwbno;
                    _customNo = GetTopClearanceNoByJobNo(ops.JobNo);
                    _jobNo = ops.JobNo;
                }
                else
                {
                    var houseBillDoc = csTransactionDetailRepo.Get(x => x.Id == charge.Hblid).FirstOrDefault();
                    if (houseBillDoc != null)
                    {
                        _hwbNo = houseBillDoc.Hwbno;
                        var masterBillDoc = csTransactionRepo.Get(x => x.Id == houseBillDoc.JobId).FirstOrDefault();
                        if (masterBillDoc != null)
                        {
                            _mawb = masterBillDoc.Mawb;
                            _customNo = GetTopClearanceNoByJobNo(masterBillDoc.JobNo);
                            _jobNo = masterBillDoc.JobNo;
                        }
                    }
                }
                #endregion -- Info MBL, HBL --

                #region -- Info CD Note --
                var cdNote = acctCdnoteRepo.Get(x => x.Code == charge.DebitNo || x.Code == charge.CreditNo).FirstOrDefault();
                #endregion -- Info CD Note --

                // Exchange Rate from currency charge to current soa
                var exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, soa.Currency?.Trim());
                var _amount = charge.Total * exchangeRate;
                var soaCharge = new AccountStatementFullReport();
                soaCharge.PartnerID = partner?.Id;
                soaCharge.PartnerName = partner?.PartnerNameEn?.ToUpper(); //Name En
                soaCharge.PersonalContact = partner?.ContactPerson?.ToUpper();
                soaCharge.Email = string.Empty; //NOT USE
                soaCharge.Address = partner?.AddressEn?.ToUpper(); //Address En 
                soaCharge.Workphone = partner?.WorkPhoneEx;
                soaCharge.Fax = string.Empty; //NOT USE
                soaCharge.Taxcode = string.Empty; //NOT USE
                soaCharge.TransID = string.Empty; //NOT USE
                soaCharge.MAWB = _mawb; //MBLNo
                soaCharge.HWBNO = _hwbNo; //HBLNo
                soaCharge.DateofInv = cdNote?.DatetimeCreated?.ToString("MMM dd, yy") ?? string.Empty; //Created Datetime CD Note
                soaCharge.Order = string.Empty; //NOT USE
                soaCharge.InvID = charge.InvoiceNo;
                soaCharge.Amount = _amount + _decimalNumber; //Cộng thêm phần thập phân
                soaCharge.Curr = soa.Currency?.Trim(); //Currency SOA
                soaCharge.Dpt = charge.Type == AccountingConstants.TYPE_CHARGE_SELL ? true : false;
                soaCharge.Vessel = string.Empty; //NOT USE
                soaCharge.Routine = string.Empty; //NOT USE
                soaCharge.LoadingDate = null; //NOT USE
                soaCharge.CustomerID = string.Empty; //NOT USE
                soaCharge.CustomerName = string.Empty; //NOT USE
                soaCharge.ArrivalDate = null; //NOT USE
                soaCharge.TpyeofService = string.Empty; //NOT USE
                soaCharge.SOANO = string.Empty; //NOT USE
                soaCharge.SOADate = null; //NOT USE
                soaCharge.FromDate = null; //NOT USE
                soaCharge.ToDate = null; //NOT USE
                soaCharge.OAmount = null; //NOT USE
                soaCharge.SAmount = null; //NOT USE
                soaCharge.CurrOP = string.Empty; //NOT USE
                soaCharge.Notes = string.Empty; //NOT USE
                soaCharge.IssuedBy = string.Empty; //NOT USE
                soaCharge.Shipper = string.Empty; //NOT USE
                soaCharge.Consignee = string.Empty; //NOT USE
                soaCharge.OtherRef = string.Empty; //NOT USE
                soaCharge.Volumne = string.Empty; //NOT USE
                soaCharge.POBH = null; //NOT USE
                soaCharge.ROBH = (charge.Type == AccountingConstants.TYPE_CHARGE_OBH) ? _amount : 0;
                soaCharge.ROBH = soaCharge.ROBH + _decimalNumber; //Cộng thêm phần thập phân
                soaCharge.CustomNo = _customNo;
                soaCharge.JobNo = _jobNo;
                soaCharge.CdCode = cdNote?.Code;
                soaCharge.Docs = string.Join("\r\n", grpInvCdNoteByHbl.Where(w => w.Hblid == charge.Hblid).Select(s => !string.IsNullOrEmpty(s.InvoiceNo) ? s.InvoiceNo : s.CdNote).Distinct()); //Ưu tiên: Invoice No >> CD Note Code

                soaCharges.Add(soaCharge);
            }

            //Sắp xếp giảm dần theo số Job
            soaCharges = soaCharges.ToArray().OrderByDescending(o => o.JobNo).ToList();

            //Info Company, Office of User Created SOA
            //var company = sysCompanyRepo.Get(x => x.Id == soa.CompanyId).FirstOrDefault();
            var office = officeRepo.Get(x => x.Id == soa.OfficeId).FirstOrDefault();

            var parameter = new AccountStatementFullReportParams();
            parameter.UptoDate = soa.SoaformDate?.ToString("dd/MM/yyyy") ?? string.Empty; //From To SOA
            parameter.dtPrintDate = soa.DatetimeCreated?.ToString("dd/MM/yyyy") ?? string.Empty; //Created Date SOA
            parameter.CompanyName = office?.BranchNameEn.ToUpper() ?? string.Empty;
            parameter.CompanyDescription = string.Empty; //NOT USE
            parameter.CompanyAddress1 = office?.AddressEn;
            parameter.CompanyAddress2 = string.Format(@"Tel: {0} Fax: {1}", office?.Tel, office?.Fax);
            parameter.Website = string.Empty; //Office ko có field Website
            parameter.IbanCode = string.Empty; //NOT USE
            parameter.AccountName = office?.BankAccountNameEn?.ToUpper() ?? string.Empty;
            parameter.AccountNameEn = office?.BankAccountNameVn?.ToUpper() ?? string.Empty;
            parameter.BankName = office?.BankNameLocal?.ToUpper() ?? string.Empty;
            parameter.BankNameEn = office?.BankNameEn?.ToUpper() ?? string.Empty;
            parameter.SwiftAccs = office?.SwiftCode ?? string.Empty;
            parameter.AccsUSD = office?.BankAccountUsd ?? string.Empty;
            parameter.AccsVND = office?.BankAccountVnd ?? string.Empty;
            parameter.BankAddress = office?.BankAddressLocal?.ToUpper() ?? string.Empty;
            parameter.BankAddressEn = office?.BankAddressEn?.ToUpper() ?? string.Empty;
            parameter.Paymentterms = string.Empty; //NOT USE
            parameter.Contact = currentUser.UserName?.ToUpper() ?? string.Empty;
            parameter.CurrDecimalNo = 3;
            parameter.RefNo = soa?.Soano; //SOA No
            parameter.Email = office?.Email ?? string.Empty;

            result = new Crystal
            {
                ReportName = "AccountStatement_Full.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(soaCharges);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }
        #endregion -- Preview --

        public List<Guid> GetSurchargeIdBySoaId(int soaId)
        {
            var soaNo = Get(x => x.Id == soaId).FirstOrDefault()?.Soano;
            var surchargeIds = csShipmentSurchargeRepo.Get(x => x.PaySoano == soaNo || x.Soano == soaNo).Select(s => s.Id).ToList();
            return surchargeIds;
        }
        
    }
}
