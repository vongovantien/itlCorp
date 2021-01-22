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
        readonly IContextBase<SysNotifications> sysNotificationRepository;
        readonly IContextBase<SysUserNotification> sysUserNotificationRepository;
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
            IContextBase<SysCompany> sysCompany,
            IContextBase<SysNotifications> sysNotifyRepo,
            IContextBase<SysUserNotification> sysUsernotifyRepo) : base(repository, mapper)
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
            sysNotificationRepository = sysNotifyRepo;
            sysUserNotificationRepository = sysUsernotifyRepo;
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
                                                               && model.Surcharges.Any(c => c.surchargeId == x.Id)
                                                               && x.Type == "OBH"
                                                               && x.PaymentObjectId == model.Customer).Any();
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
                                                                           && model.Surcharges.Any(c => c.surchargeId == x.Id)
                                                                           && (x.Type == "BUY" || (x.Type == "OBH" && x.PayerId == model.Customer))
                                                                           ).ToList();

                            //Lấy ra những charge có type là SELL hoặc OBH-SELL mà chưa tồn tại trong 1 SOA nào cả
                            var surchargeDebit = csShipmentSurchargeRepo.Get(x => model.Surcharges != null
                                                                           && model.Surcharges.Any(c => c.surchargeId == x.Id)
                                                                           && (x.Type == "SELL" || (x.Type == "OBH" && x.PaymentObjectId == model.Customer))
                                                                           ).ToList();

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
                        var surcharge = csShipmentSurchargeRepo.Get(x => (model.Type == "Debit" ? x.Soano : x.PaySoano) == model.Soano).ToList();
                        foreach (var item in surcharge)
                        {
                            //Update SOANo = NULL & PaySOANo = NULL to CsShipmentSurcharge
                            if (model.Type == "Debit")
                            {
                                item.Soano = null;
                            }
                            else
                            {
                                item.PaySoano = null;
                            }                                                       
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
                        soa.SyncStatus = soaCurrent.SyncStatus;
                        soa.LastSyncDate = soaCurrent.LastSyncDate;
                        soa.ReasonReject = soaCurrent.ReasonReject;

                        //Check exists OBH Debit Charge
                        var isExistObhDebitCharge = csShipmentSurchargeRepo.Get(x => model.Surcharges != null
                                                                       && model.Surcharges.Any(c => c.surchargeId == x.Id)
                                                                       && x.Type == "OBH" 
                                                                       && x.PaymentObjectId == model.Customer).Any();
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
                                                                           && model.Surcharges.Any(c => c.surchargeId == x.Id)
                                                                           && (x.Type == "BUY" || (x.Type == "OBH" && x.PayerId == model.Customer))
                                                                           ).ToList();

                            //Lấy ra những charge có type là SELL hoặc OBH-SELL mà chưa tồn tại trong 1 SOA nào cả
                            var surchargeDebit = csShipmentSurchargeRepo.Get(x => model.Surcharges != null
                                                                           && model.Surcharges.Any(c => c.surchargeId == x.Id)
                                                                           && (x.Type == "SELL" || (x.Type == "OBH" && x.PaymentObjectId == model.Customer))
                                                                           ).ToList();

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
            var surcharges = csShipmentSurchargeRepo.Get(x => model.Surcharges.Any(s => s.surchargeId == x.Id));
            //Count number shipment (JobNo, HBL)
            var totalShipment = surcharges.Where(x => x.Hblno != null).GroupBy(x => x.JobNo + "_" + x.Hblno).Count();

            decimal debitAmount = 0;
            decimal creditAmount = 0;
            foreach (var surcharge in surcharges)
            {
                var _exchangeDate = model.Id == 0 ? DateTime.Now : model.DatetimeCreated;
                var _exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, _exchangeDate, surcharge.CurrencyId, model.Currency);
                var _debit = surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL || (surcharge.PaymentObjectId == model.Customer && surcharge.Type == "OBH") ? surcharge.Total : 0;
                var _credit = surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY || (surcharge.PayerId == model.Customer && surcharge.Type == "OBH") ? surcharge.Total : 0;
                //Debit Amount
                debitAmount += _exchangeRate * _debit;
                //Credit Amount
                creditAmount += _exchangeRate * _credit;
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
        
        private string GetTopClearanceNoByJobNo(string JobNo)
        {
            var custom = customsDeclarationRepo.Get();
            var clearanceNo = custom.Where(x => x.JobNo != null && x.JobNo == JobNo)
                .OrderBy(x => x.JobNo)
                .OrderByDescending(x => x.ClearanceDate)
                .FirstOrDefault()?.ClearanceNo;
            return clearanceNo;
        }
        
        #region -- Get List Charges Shipment By Criteria --
        
        private IQueryable<ChargeShipmentModel> GetChargeForIssueSoaByCriteria(ChargeShipmentCriteria criteria)
        {
            IQueryable<ChargeShipmentModel> charges = null;
            IQueryable<CsShipmentSurcharge> surcharges = null;
            IQueryable<CsShipmentSurcharge> obhSurcharges = null;
            IQueryable<OpsTransaction> operations = null;
            IQueryable<CsTransaction> transactions = null;

            string typeCharge = AccountingConstants.TYPE_CHARGE_SELL; //Default is SELL

            //Type Charge
            if (!string.IsNullOrEmpty(criteria.Type))
            {
                if (criteria.Type == "Debit")
                {
                    typeCharge = AccountingConstants.TYPE_CHARGE_SELL;
                }
                if (criteria.Type == "Credit")
                {
                    typeCharge = AccountingConstants.TYPE_CHARGE_BUY;
                }
            }

            #region -- Search by Customer --
            if (!string.IsNullOrEmpty(criteria.CustomerID))
            {
                //Get charge by: Customer, loại phí, phí chưa sync, phí chưa issue SOA
                surcharges = csShipmentSurchargeRepo.Get(x => x.Type == typeCharge
                                                             && x.PaymentObjectId == criteria.CustomerID
                                                             && string.IsNullOrEmpty(x.SyncedFrom)
                                                             && (x.Type == AccountingConstants.TYPE_CHARGE_SELL ? string.IsNullOrEmpty(x.Soano) : string.IsNullOrEmpty(x.PaySoano)));
                if (criteria.IsOBH) //**
                {
                    //SELL ~ PaymentObjectID, SOANo
                    obhSurcharges = csShipmentSurchargeRepo.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH
                                                                  && (typeCharge == AccountingConstants.TYPE_CHARGE_SELL ? x.PaymentObjectId : x.PayerId) == criteria.CustomerID
                                                                  && string.IsNullOrEmpty(x.PaySyncedFrom)
                                                                  && (typeCharge == AccountingConstants.TYPE_CHARGE_SELL ? string.IsNullOrEmpty(x.Soano) : string.IsNullOrEmpty(x.PaySoano)));
                }
            }
            #endregion -- Search by Customer --

            #region -- Search by Services --
            if (!string.IsNullOrEmpty(criteria.StrServices))
            {
                surcharges = surcharges.Where(x => criteria.StrServices.Contains(x.TransactionType));
                if (criteria.IsOBH) //**
                {
                    obhSurcharges = obhSurcharges.Where(x => criteria.StrServices.Contains(x.TransactionType));
                }
            }
            #endregion -- Search by Services --

            #region -- Search by Created Date or Service Date --
            //Created Date of Job
            if (criteria.DateType == "CreatedDate")
            {
                if (criteria.StrServices.Contains("CL"))
                {
                    operations = opsTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled && (x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date >= criteria.FromDate.Date && x.DatetimeCreated.Value.Date <= criteria.ToDate.Date : false));
                    if (criteria.StrServices.Contains("I") || criteria.StrServices.Contains("A"))
                    {
                        transactions = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled && (x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date >= criteria.FromDate.Date && x.DatetimeCreated.Value.Date <= criteria.ToDate.Date : false));
                    }
                }
                else
                {
                    transactions = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled && (x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date >= criteria.FromDate.Date && x.DatetimeCreated.Value.Date <= criteria.ToDate.Date : false));
                }
            }

            //Service Date of Job
            if (criteria.DateType == "ServiceDate")
            {
                if (criteria.StrServices.Contains("CL"))
                {
                    operations = opsTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled && (x.ServiceDate.HasValue ? x.ServiceDate.Value.Date >= criteria.FromDate.Date && x.ServiceDate.Value.Date <= criteria.ToDate.Date : false));

                    if (criteria.StrServices.Contains("I") || criteria.StrServices.Contains("A"))
                    {
                        transactions = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled
                                                          && (
                                                                (x.TransactionType.Contains("I") ? x.Eta.HasValue : x.Etd.HasValue)
                                                                ?
                                                                (x.TransactionType.Contains("I") ? x.Eta.Value.Date : x.Etd.Value.Date) >= criteria.FromDate.Date && (x.TransactionType.Contains("I") ? x.Eta.Value.Date : x.Etd.Value.Date) <= criteria.ToDate.Date
                                                                : false
                                                             )); //Import - ETA, Export - ETD
                    }
                }
                else
                {
                    transactions = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled
                                                          && (
                                                                (x.TransactionType.Contains("I") ? x.Eta.HasValue : x.Etd.HasValue)
                                                                ?
                                                                (x.TransactionType.Contains("I") ? x.Eta.Value.Date : x.Etd.Value.Date) >= criteria.FromDate.Date && (x.TransactionType.Contains("I") ? x.Eta.Value.Date : x.Etd.Value.Date) <= criteria.ToDate.Date
                                                                : false
                                                             )); //Import - ETA, Export - ETD
                }
            }

            var dateModeJobNos = new List<string>();
            if (operations != null)
            {
                dateModeJobNos = operations.Select(s => s.JobNo).ToList();
            }
            if (transactions != null)
            {
                dateModeJobNos.AddRange(transactions.Select(s => s.JobNo).ToList());
            }

            if (dateModeJobNos.Count > 0 && surcharges != null)
            {
                surcharges = surcharges.Where(x => dateModeJobNos.Where(w => w == x.JobNo).Any());
                if (criteria.IsOBH) //**
                {
                    obhSurcharges = obhSurcharges.Where(x => dateModeJobNos.Where(w => w == x.JobNo).Any());
                }
            }
            else
            {
                surcharges = null;
                obhSurcharges = null;
            }

            #endregion -- Search by Created Date or Service Date --

            #region -- Search by Creator of Job --
            if (!string.IsNullOrEmpty(criteria.StrCreators))
            {
                var creators = criteria.StrCreators.Split(',').Where(x => x.ToString() != string.Empty).ToList();
                if (criteria.StrServices.Contains("CL"))
                {
                    operations = operations.Where(x => creators.Where(w => w == x.UserCreated).Any());
                    if (criteria.StrServices.Contains("I") || criteria.StrServices.Contains("A"))
                    {
                        transactions = transactions.Where(x => creators.Where(w => w == x.UserCreated).Any());
                    }
                }
                else
                {
                    transactions = transactions.Where(x => creators.Where(w => w == x.UserCreated).Any());
                }

                var creatorJobNos = new List<string>();
                if (operations != null)
                {
                    creatorJobNos = operations.Select(s => s.JobNo).ToList();
                }
                if (transactions != null)
                {
                    creatorJobNos.AddRange(transactions.Select(s => s.JobNo).ToList());
                }
                if (creatorJobNos.Count > 0)
                {
                    surcharges = surcharges.Where(x => creatorJobNos.Where(w => w == x.JobNo).Any());
                    if (criteria.IsOBH) //**
                    {
                        obhSurcharges = obhSurcharges.Where(x => creatorJobNos.Where(w => w == x.JobNo).Any());
                    }
                }
                else
                {
                    surcharges = null;
                    obhSurcharges = null;
                }
            }
            #endregion -- Search by Creator of Job --

            #region -- Search by ChargeId --
            if (!string.IsNullOrEmpty(criteria.StrCharges))
            {
                var chargeIds = criteria.StrCharges.Split(',').Where(x => x.ToString() != string.Empty).ToList();
                if (chargeIds.Count > 0 && surcharges != null)
                {
                    surcharges = surcharges.Where(x => chargeIds.Where(w => w == x.ChargeId.ToString()).Any());
                    if (criteria.IsOBH) //**
                    {
                        obhSurcharges = obhSurcharges.Where(x => chargeIds.Where(w => w == x.ChargeId.ToString()).Any());
                    }
                }
            }
            #endregion -- Search by ChargeId --

            #region -- Search by JobNo --
            if (criteria.JobIds != null)
            {
                var jobNos = criteria.JobIds.Where(x => !string.IsNullOrEmpty(x)).ToList();
                if (jobNos.Count > 0 && surcharges != null)
                {
                    surcharges = surcharges.Where(x => criteria.JobIds.Where(w => w == x.JobNo).Any());
                    if (criteria.IsOBH) //**
                    {
                        obhSurcharges = obhSurcharges.Where(x => criteria.JobIds.Where(w => w == x.JobNo).Any());
                    }
                }
            }
            #endregion -- Search by JobNo --

            #region -- Search by HBL --
            if (criteria.Hbls != null)
            {
                var hbls = criteria.Hbls.Where(x => !string.IsNullOrEmpty(x)).ToList();
                if (hbls.Count > 0 && surcharges != null)
                {
                    surcharges = surcharges.Where(x => criteria.Hbls.Where(w => w == x.Hblno).Any());
                    if (criteria.IsOBH) //**
                    {
                        obhSurcharges = obhSurcharges.Where(x => criteria.Hbls.Where(w => w == x.Hblno).Any());
                    }
                }
            }
            #endregion -- Search by HBL --

            #region -- Search by MBL --
            if (criteria.Mbls != null)
            {
                var mbls = criteria.Mbls.Where(x => !string.IsNullOrEmpty(x)).ToList();
                if (mbls.Count > 0 && surcharges != null)
                {
                    surcharges = surcharges.Where(x => criteria.Mbls.Where(w => w == x.Mblno).Any());
                    if (criteria.IsOBH) //**
                    {
                        obhSurcharges = obhSurcharges.Where(x => criteria.Mbls.Where(w => w == x.Mblno).Any());
                    }
                }
            }
            #endregion -- Search by MBL --

            #region -- Search by CustomNo --
            if (criteria.CustomNo != null)
            {
                //Custom only for OPS
                if (criteria.StrServices.Contains("CL"))
                {
                    var customNos = criteria.CustomNo.Where(x => !string.IsNullOrEmpty(x)).ToList();
                    if (customNos.Count > 0 && surcharges != null)
                    {
                        //Get JobNo from CustomDeclaration by list param Custom No
                        var clearanceJobNo = customsDeclarationRepo.Get(x => customNos.Where(w => w == x.ClearanceNo).Any()).Select(s => s.JobNo).ToList();
                        if (clearanceJobNo.Count > 0)
                        {
                            surcharges = surcharges.Where(x => clearanceJobNo.Where(w => w == x.JobNo).Any());
                            if (criteria.IsOBH) //**
                            {
                                obhSurcharges = obhSurcharges.Where(x => clearanceJobNo.Where(w => w == x.JobNo).Any());
                            }
                        }
                        else
                        {
                            surcharges = null;
                            obhSurcharges = null;
                        }
                    }
                }
            }
            #endregion -- Search by CustomNo --

            #region -- Get more OBH charge --
            //Lấy thêm phí OBH
            if (criteria.IsOBH)
            {
                if (obhSurcharges != null && surcharges != null)
                {
                    surcharges = surcharges.Union(obhSurcharges);
                }
            }
            #endregion -- Get more OBH charge --

            var data = new List<ChargeShipmentModel>();
            if (surcharges == null) return data.AsQueryable();
            foreach (var surcharge in surcharges)
            {
                var chg = new ChargeShipmentModel();
                chg.ID = surcharge.Id;
                var charge = catChargeRepo.Get(x => x.Id == surcharge.ChargeId).FirstOrDefault();
                chg.ChargeCode = charge?.Code;
                chg.ChargeName = charge?.ChargeNameEn;
                chg.JobId = surcharge.JobNo;
                chg.HBL = surcharge.Hblno;
                chg.MBL = surcharge.Mblno;
                chg.Type = surcharge.Type;
                chg.Debit = surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL || (surcharge.PaymentObjectId == criteria.CustomerID && surcharge.Type == "OBH") ? (decimal?)surcharge.Total : null;
                chg.Credit = surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY || (surcharge.PayerId == criteria.CustomerID && surcharge.Type == "OBH") ? (decimal?)surcharge.Total : null;
                chg.Currency = surcharge.CurrencyId;
                chg.InvoiceNo = surcharge.InvoiceNo;
                chg.Note = surcharge.Notes;
                chg.CurrencyToLocal = AccountingConstants.CURRENCY_LOCAL;
                chg.CurrencyToUSD = AccountingConstants.CURRENCY_USD;
                var _exchangeRateLocal = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, AccountingConstants.CURRENCY_LOCAL);
                chg.AmountDebitLocal = _exchangeRateLocal * (chg.Debit ?? 0);
                chg.AmountCreditLocal = _exchangeRateLocal * (chg.Credit ?? 0);
                var _exchangeRateUSD = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, AccountingConstants.CURRENCY_USD);
                chg.AmountDebitUSD = _exchangeRateUSD * (chg.Debit ?? 0);
                chg.AmountCreditUSD = _exchangeRateUSD * (chg.Credit ?? 0);
                chg.DatetimeModifiedSurcharge = surcharge.DatetimeModified;

                string _pic = string.Empty;
                DateTime? _serviceDate = null;
                string _customNo = string.Empty;
                if (surcharge.TransactionType == "CL")
                {
                    var ops = opsTransactionRepo.Get(x => x.JobNo == surcharge.JobNo && x.CurrentStatus != TermData.Canceled).FirstOrDefault();
                    if (ops != null)
                    {
                        _serviceDate = ops.ServiceDate;
                        var user = sysUserRepo.Get(x => x.Id == ops.BillingOpsId).FirstOrDefault();
                        _pic = user?.Username;
                        _customNo = customsDeclarationRepo.Get(x => x.JobNo == ops.JobNo).OrderByDescending(x => x.ClearanceDate).FirstOrDefault()?.ClearanceNo;
                    }
                }
                else
                {
                    var tran = csTransactionRepo.Get(x => x.JobNo == surcharge.JobNo && x.CurrentStatus != TermData.Canceled).FirstOrDefault();
                    if (tran != null)
                    {
                        _serviceDate = tran.TransactionType.Contains("I") ? tran.Eta : tran.Etd;
                        var user = sysUserRepo.Get(x => x.Id == tran.PersonIncharge).FirstOrDefault();
                        _pic = user?.Username;
                    }
                }
                chg.CustomNo = _customNo;
                chg.ServiceDate = _serviceDate;
                chg.PIC = _pic;

                string _cdNote = string.Empty;
                if (criteria.CustomerID != null)
                {
                    if (criteria.CustomerID == surcharge.PayerId && surcharge.Type == "OBH")
                    {
                        _cdNote = surcharge.CreditNo;
                    }
                    else
                    {
                        if (surcharge.Type == "BUY")
                        {
                            _cdNote = surcharge.CreditNo;
                        }
                        if (surcharge.Type == "SELL" || surcharge.Type == "OBH")
                        {
                            _cdNote = surcharge.DebitNo;
                        }
                    }
                }
                chg.CDNote = _cdNote;
                data.Add(chg);
            }
            //Sort Array sẽ nhanh hơn
            charges = data.ToArray().OrderByDescending(x => x.DatetimeModifiedSurcharge).AsQueryable();
            return charges;
        }

        public ChargeShipmentResult GetListChargeShipment(ChargeShipmentCriteria criteria)
        {
            var chargeShipmentList = GetChargeForIssueSoaByCriteria(criteria);
            var result = new ChargeShipmentResult
            {
                ChargeShipments = chargeShipmentList.ToList(),
                TotalShipment = chargeShipmentList.Where(x => x.HBL != null).GroupBy(x => x.JobId + "_" + x.HBL).Count(),
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
        private IQueryable<ChargeShipmentModel> GetMoreChargesForIssueSoaByCriteria(MoreChargeShipmentCriteria criteria)
        {
            IQueryable<ChargeShipmentModel> charges = null;
            IQueryable<CsShipmentSurcharge> surcharges = null;
            IQueryable<CsShipmentSurcharge> obhSurcharges = null;
            IQueryable<OpsTransaction> operations = null;
            IQueryable<CsTransaction> transactions = null;

            string typeCharge = AccountingConstants.TYPE_CHARGE_SELL; //Default is SELL

            //Type Charge
            if (!string.IsNullOrEmpty(criteria.Type))
            {
                if (criteria.Type == "Debit")
                {
                    typeCharge = AccountingConstants.TYPE_CHARGE_SELL;
                }
                if (criteria.Type == "Credit")
                {
                    typeCharge = AccountingConstants.TYPE_CHARGE_BUY;
                }
            }

            #region -- Search by Customer --
            if (!string.IsNullOrEmpty(criteria.CustomerID))
            {
                //Get charge by: Customer, loại phí
                surcharges = csShipmentSurchargeRepo.Get(x => x.Type == typeCharge
                                                             && x.PaymentObjectId == criteria.CustomerID);
                if (criteria.IsOBH) //**
                {
                    //SELL ~ PaymentObjectID, SOANo
                    obhSurcharges = csShipmentSurchargeRepo.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH
                                                                  && (typeCharge == AccountingConstants.TYPE_CHARGE_SELL ? x.PaymentObjectId : x.PayerId) == criteria.CustomerID);
                }
            }
            #endregion -- Search by Customer --

            #region -- Search by Created Date or Service Date --
            //Created Date of Job
            if (criteria.DateType == "CreatedDate")
            {
                if (criteria.StrServices.Contains("CL"))
                {
                    operations = opsTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled && (x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date >= criteria.FromDate.Date && x.DatetimeCreated.Value.Date <= criteria.ToDate.Date : false));
                    if (criteria.StrServices.Contains("I") || criteria.StrServices.Contains("A"))
                    {
                        transactions = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled && (x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date >= criteria.FromDate.Date && x.DatetimeCreated.Value.Date <= criteria.ToDate.Date : false));
                    }
                }
                else
                {
                    transactions = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled && (x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date >= criteria.FromDate.Date && x.DatetimeCreated.Value.Date <= criteria.ToDate.Date : false));
                }
            }

            //Service Date of Job
            if (criteria.DateType == "ServiceDate")
            {
                if (criteria.StrServices.Contains("CL"))
                {
                    operations = opsTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled && (x.ServiceDate.HasValue ? x.ServiceDate.Value.Date >= criteria.FromDate.Date && x.ServiceDate.Value.Date <= criteria.ToDate.Date : false));

                    if (criteria.StrServices.Contains("I") || criteria.StrServices.Contains("A"))
                    {
                        transactions = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled
                                                          && (
                                                                (x.TransactionType.Contains("I") ? x.Eta.HasValue : x.Etd.HasValue)
                                                                ?
                                                                (x.TransactionType.Contains("I") ? x.Eta.Value.Date : x.Etd.Value.Date) >= criteria.FromDate.Date && (x.TransactionType.Contains("I") ? x.Eta.Value.Date : x.Etd.Value.Date) <= criteria.ToDate.Date
                                                                : false
                                                             )); //Import - ETA, Export - ETD
                    }
                }
                else
                {
                    transactions = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled
                                                          && (
                                                                (x.TransactionType.Contains("I") ? x.Eta.HasValue : x.Etd.HasValue)
                                                                ?
                                                                (x.TransactionType.Contains("I") ? x.Eta.Value.Date : x.Etd.Value.Date) >= criteria.FromDate.Date && (x.TransactionType.Contains("I") ? x.Eta.Value.Date : x.Etd.Value.Date) <= criteria.ToDate.Date
                                                                : false
                                                             )); //Import - ETA, Export - ETD
                }
            }

            var dateModeJobNos = new List<string>();
            if (operations != null)
            {
                dateModeJobNos = operations.Select(s => s.JobNo).ToList();
            }
            if (transactions != null)
            {
                dateModeJobNos.AddRange(transactions.Select(s => s.JobNo).ToList());
            }

            if (dateModeJobNos.Count > 0 && surcharges != null)
            {
                surcharges = surcharges.Where(x => dateModeJobNos.Where(w => w == x.JobNo).Any());
                if (criteria.IsOBH) //**
                {
                    obhSurcharges = obhSurcharges.Where(x => dateModeJobNos.Where(w => w == x.JobNo).Any());
                }
            }
            else
            {
                surcharges = null;
                obhSurcharges = null;
            }

            #endregion -- Search by Created Date or Service Date --

            #region -- Search by Creator of Job --
            if (!string.IsNullOrEmpty(criteria.StrCreators))
            {
                var creators = criteria.StrCreators.Split(',').Where(x => x.ToString() != string.Empty).ToList();
                if (criteria.StrServices.Contains("CL"))
                {
                    operations = operations.Where(x => creators.Where(w => w == x.UserCreated).Any());
                    if (criteria.StrServices.Contains("I") || criteria.StrServices.Contains("A"))
                    {
                        transactions = transactions.Where(x => creators.Where(w => w == x.UserCreated).Any());
                    }
                }
                else
                {
                    transactions = transactions.Where(x => creators.Where(w => w == x.UserCreated).Any());
                }

                var creatorJobNos = new List<string>();
                if (operations != null)
                {
                    creatorJobNos = operations.Select(s => s.JobNo).ToList();
                }
                if (transactions != null)
                {
                    creatorJobNos.AddRange(transactions.Select(s => s.JobNo).ToList());
                }
                if (creatorJobNos.Count > 0)
                {
                    surcharges = surcharges.Where(x => creatorJobNos.Where(w => w == x.JobNo).Any());
                    if (criteria.IsOBH) //**
                    {
                        obhSurcharges = obhSurcharges.Where(x => creatorJobNos.Where(w => w == x.JobNo).Any());
                    }
                }
                else
                {
                    surcharges = null;
                    obhSurcharges = null;
                }
            }
            #endregion -- Search by Creator of Job --

            #region -- Shipment (JobNo, MBL, HBL)
            if (!string.IsNullOrEmpty(criteria.JobId))
            {
                if(surcharges != null)
                {
                    surcharges = surcharges.Where(x => x.JobNo == criteria.JobId);
                    if (criteria.IsOBH) //**
                    {
                        obhSurcharges = obhSurcharges.Where(x => x.JobNo == criteria.JobId);
                    }
                }
            }
            if (!string.IsNullOrEmpty(criteria.Mbl))
            {
                if (surcharges != null)
                {
                    surcharges = surcharges.Where(x => x.Mblno == criteria.Mbl);
                    if (criteria.IsOBH) //**
                    {
                        obhSurcharges = obhSurcharges.Where(x => x.Mblno == criteria.Mbl);
                    }
                }
            }
            if (!string.IsNullOrEmpty(criteria.Hbl))
            {
                if (surcharges != null)
                {
                    surcharges = surcharges.Where(x => x.Hblno == criteria.Hbl);
                    if (criteria.IsOBH) //**
                    {
                        obhSurcharges = obhSurcharges.Where(x => x.Hblno == criteria.Hbl);
                    }
                }
            }
            #endregion -- Shipment (JobNo, MBL, HBL)

            #region -- CD Note --
            if (!string.IsNullOrEmpty(criteria.CDNote))
            {
                if (surcharges != null)
                {
                    surcharges = surcharges.Where(x => ((criteria.CustomerID == x.PayerId && x.Type == "OBH") || x.Type == "BUY" ? x.CreditNo : x.DebitNo) == criteria.CDNote);
                    if (criteria.IsOBH) //**
                    {
                        obhSurcharges = obhSurcharges.Where(x => ((criteria.CustomerID == x.PayerId && x.Type == "OBH") || x.Type == "BUY" ? x.CreditNo : x.DebitNo) == criteria.CDNote);
                    }
                }
            }
            #endregion -- CD Note --

            #region -- Search by ChargeId --
            if (!string.IsNullOrEmpty(criteria.StrCharges))
            {
                var chargeIds = criteria.StrCharges.Split(',').Where(x => x.ToString() != string.Empty).ToList();
                if (chargeIds.Count > 0 && surcharges != null)
                {
                    surcharges = surcharges.Where(x => chargeIds.Where(w => w == x.ChargeId.ToString()).Any());
                    if (criteria.IsOBH) //**
                    {
                        obhSurcharges = obhSurcharges.Where(x => chargeIds.Where(w => w == x.ChargeId.ToString()).Any());
                    }
                }
            }
            #endregion -- Search by ChargeId --

            #region -- In SOA --
            if (criteria.InSoa)
            {
                if (surcharges != null)
                {
                    surcharges = surcharges.Where(x => !string.IsNullOrEmpty(criteria.Type == "Debit" ? x.Soano : x.PaySoano));
                    if (criteria.IsOBH) //**
                    {
                        obhSurcharges = obhSurcharges.Where(x => !string.IsNullOrEmpty(criteria.Type == "Debit" ? x.Soano : x.PaySoano));
                    }
                }
            }
            else
            {
                if (surcharges != null)
                {
                    surcharges = surcharges.Where(x => string.IsNullOrEmpty(criteria.Type == "Debit" ? x.Soano : x.PaySoano));
                    if (criteria.IsOBH) //**
                    {
                        obhSurcharges = obhSurcharges.Where(x => string.IsNullOrEmpty(criteria.Type == "Debit" ? x.Soano : x.PaySoano));
                    }
                }
            }
            #endregion -- In SOA --

            #region -- Get more OBH charge --
            //Lấy thêm phí OBH
            if (criteria.IsOBH)
            {
                if (obhSurcharges != null && surcharges != null)
                {
                    surcharges = surcharges.Union(obhSurcharges);
                }
            }
            #endregion -- Get more OBH charge --

            var data = new List<ChargeShipmentModel>();
            if (surcharges == null) return data.AsQueryable();
            foreach (var surcharge in surcharges)
            {
                var chg = new ChargeShipmentModel();
                chg.ID = surcharge.Id;
                var charge = catChargeRepo.Get(x => x.Id == surcharge.ChargeId).FirstOrDefault();
                chg.ChargeCode = charge?.Code;
                chg.ChargeName = charge?.ChargeNameEn;
                chg.JobId = surcharge.JobNo;
                chg.HBL = surcharge.Hblno;
                chg.MBL = surcharge.Mblno;
                chg.Type = surcharge.Type;
                chg.Debit = surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL || (surcharge.PaymentObjectId == criteria.CustomerID && surcharge.Type == "OBH") ? (decimal?)surcharge.Total : null;
                chg.Credit = surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY || (surcharge.PayerId == criteria.CustomerID && surcharge.Type == "OBH") ? (decimal?)surcharge.Total : null;
                chg.Currency = surcharge.CurrencyId;
                chg.InvoiceNo = surcharge.InvoiceNo;
                chg.Note = surcharge.Notes;
                chg.CurrencyToLocal = AccountingConstants.CURRENCY_LOCAL;
                chg.CurrencyToUSD = AccountingConstants.CURRENCY_USD;
                var _exchangeRateLocal = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, AccountingConstants.CURRENCY_LOCAL);
                chg.AmountDebitLocal = _exchangeRateLocal * (chg.Debit ?? 0);
                chg.AmountCreditLocal = _exchangeRateLocal * (chg.Credit ?? 0);
                var _exchangeRateUSD = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, AccountingConstants.CURRENCY_USD);
                chg.AmountDebitUSD = _exchangeRateUSD * (chg.Debit ?? 0);
                chg.AmountCreditUSD = _exchangeRateUSD * (chg.Credit ?? 0);
                chg.DatetimeModifiedSurcharge = surcharge.DatetimeModified;
                chg.SOANo = criteria.Type == "Debit" ? surcharge.Soano : surcharge.PaySoano;

                string _pic = string.Empty;
                DateTime? _serviceDate = null;
                string _customNo = string.Empty;
                if (surcharge.TransactionType == "CL")
                {
                    var ops = opsTransactionRepo.Get(x => x.JobNo == surcharge.JobNo && x.CurrentStatus != TermData.Canceled).FirstOrDefault();
                    if (ops != null)
                    {
                        _serviceDate = ops.ServiceDate;
                        var user = sysUserRepo.Get(x => x.Id == ops.BillingOpsId).FirstOrDefault();
                        _pic = user?.Username;
                        _customNo = customsDeclarationRepo.Get(x => x.JobNo == ops.JobNo).OrderByDescending(x => x.ClearanceDate).FirstOrDefault()?.ClearanceNo;
                    }
                }
                else
                {
                    var tran = csTransactionRepo.Get(x => x.JobNo == surcharge.JobNo && x.CurrentStatus != TermData.Canceled).FirstOrDefault();
                    if (tran != null)
                    {
                        _serviceDate = tran.TransactionType.Contains("I") ? tran.Eta : tran.Etd;
                        var user = sysUserRepo.Get(x => x.Id == tran.PersonIncharge).FirstOrDefault();
                        _pic = user?.Username;
                    }
                }
                chg.CustomNo = _customNo;
                chg.ServiceDate = _serviceDate;
                chg.PIC = _pic;

                string _cdNote = string.Empty;
                if (criteria.CustomerID != null)
                {
                    if (criteria.CustomerID == surcharge.PayerId && surcharge.Type == "OBH")
                    {
                        _cdNote = surcharge.CreditNo;
                    }
                    else
                    {
                        if (surcharge.Type == "BUY")
                        {
                            _cdNote = surcharge.CreditNo;
                        }
                        if (surcharge.Type == "SELL" || surcharge.Type == "OBH")
                        {
                            _cdNote = surcharge.DebitNo;
                        }
                    }
                }
                chg.CDNote = _cdNote;
                data.Add(chg);
            }
            //Sort Array sẽ nhanh hơn
            charges = data.ToArray().OrderByDescending(x => x.DatetimeModifiedSurcharge).AsQueryable();
            return charges;
        }

        public IQueryable<ChargeShipmentModel> GetListMoreCharge(MoreChargeShipmentCriteria criteria)
        {
            var moreChargeShipmentList = GetMoreChargesForIssueSoaByCriteria(criteria);

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
                    data.Shipment = criteria.ChargeShipmentsCurrent.Where(x => x.HBL != null).GroupBy(x => x.JobId + "_" + x.HBL).Count();
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
                                 LastSyncDate = s.LastSyncDate,
                                 ReasonReject = s.ReasonReject
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
                                 LastSyncDate = s.LastSyncDate,
                                 ReasonReject = s.ReasonReject,
                                 CreditPayment = pat.CreditPayment
                             };
            var result = resultData.FirstOrDefault();
            if (result != null)
            {
                result.UserNameCreated = sysUserRepo.Get(x => x.Id == result.UserCreated).FirstOrDefault()?.Username;
                result.UserNameModified = sysUserRepo.Get(x => x.Id == result.UserModified).FirstOrDefault()?.Username;
            }
            return result;
        }

        private List<ChargeShipmentModel> GetChargesForDetailSoa(string soaNo)
        {
            var data = new List<ChargeShipmentModel>();
            var soa = DataContext.Get(x => x.Soano == soaNo).FirstOrDefault();
            if (soa != null)
            {
                var surcharges = csShipmentSurchargeRepo.Get(x => soa.Type == "Debit" ? x.Soano == soaNo : x.PaySoano == soaNo);
                foreach (var surcharge in surcharges)
                {
                    var chg = new ChargeShipmentModel();
                    chg.SOANo = soa.Type == "Debit" ? surcharge.Soano : surcharge.PaySoano;
                    chg.ID = surcharge.Id;
                    var charge = catChargeRepo.Get(x => x.Id == surcharge.ChargeId).FirstOrDefault();
                    chg.ChargeCode = charge?.Code;
                    chg.ChargeName = charge?.ChargeNameEn;
                    chg.JobId = surcharge.JobNo;
                    chg.HBL = surcharge.Hblno;
                    chg.MBL = surcharge.Mblno;
                    chg.Type = surcharge.Type;
                    chg.Debit = surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL || (surcharge.PaymentObjectId == soa.Customer && surcharge.Type == "OBH") ? (decimal?)surcharge.Total : null;
                    chg.Credit = surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY || (surcharge.PayerId == soa.Customer && surcharge.Type == "OBH") ? (decimal?)surcharge.Total : null;
                    chg.Currency = surcharge.CurrencyId;
                    chg.InvoiceNo = surcharge.InvoiceNo;
                    chg.Note = surcharge.Notes;
                    chg.CurrencyToLocal = AccountingConstants.CURRENCY_LOCAL;
                    chg.CurrencyToUSD = AccountingConstants.CURRENCY_USD;
                    var _exchangeRateLocal = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, AccountingConstants.CURRENCY_LOCAL);
                    chg.AmountDebitLocal = _exchangeRateLocal * (chg.Debit ?? 0);
                    chg.AmountCreditLocal = _exchangeRateLocal * (chg.Credit ?? 0);
                    var _exchangeRateUSD = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, AccountingConstants.CURRENCY_USD);
                    chg.AmountDebitUSD = _exchangeRateUSD * (chg.Debit ?? 0);
                    chg.AmountCreditUSD = _exchangeRateUSD * (chg.Credit ?? 0);
                    chg.DatetimeModifiedSurcharge = surcharge.DatetimeModified;

                    string _pic = string.Empty;
                    DateTime? _serviceDate = null;
                    string _customNo = string.Empty;
                    if (surcharge.TransactionType == "CL")
                    {
                        var ops = opsTransactionRepo.Get(x => x.JobNo == surcharge.JobNo && x.CurrentStatus != TermData.Canceled).FirstOrDefault();
                        if (ops != null)
                        {
                            _serviceDate = ops.ServiceDate;
                            var user = sysUserRepo.Get(x => x.Id == ops.BillingOpsId).FirstOrDefault();
                            _pic = user?.Username;
                            _customNo = customsDeclarationRepo.Get(x => x.JobNo == ops.JobNo).OrderByDescending(x => x.ClearanceDate).FirstOrDefault()?.ClearanceNo;
                        }
                    }
                    else
                    {
                        var tran = csTransactionRepo.Get(x => x.JobNo == surcharge.JobNo && x.CurrentStatus != TermData.Canceled).FirstOrDefault();
                        if (tran != null)
                        {
                            _serviceDate = tran.TransactionType.Contains("I") ? tran.Eta : tran.Etd;
                            var user = sysUserRepo.Get(x => x.Id == tran.PersonIncharge).FirstOrDefault();
                            _pic = user?.Username;
                        }
                    }
                    chg.CustomNo = _customNo;
                    chg.ServiceDate = _serviceDate;
                    chg.PIC = _pic;

                    bool _isSynced = false;
                    string _syncedFromBy = null;
                    string _cdNote = string.Empty;
                    if (soa != null)
                    {
                        if (soa.Customer == surcharge.PayerId && surcharge.Type == "OBH")
                        {
                            _cdNote = surcharge.CreditNo;

                            _isSynced = !string.IsNullOrEmpty(surcharge.PaySyncedFrom) && (surcharge.PaySyncedFrom.Equals("SOA") || surcharge.PaySyncedFrom.Equals("CDNOTE") || surcharge.PaySyncedFrom.Equals("VOUCHER") || surcharge.PaySyncedFrom.Equals("SETTLEMENT"));
                            if (surcharge.PaySyncedFrom == "SOA")
                            {
                                _syncedFromBy = chg.SOANo;
                            }
                            if (surcharge.PaySyncedFrom == "CDNOTE")
                            {
                                _syncedFromBy = _cdNote;
                            }
                            if (surcharge.PaySyncedFrom == "VOUCHER")
                            {
                                _syncedFromBy = surcharge.VoucherId;
                            }
                            if (surcharge.PaySyncedFrom == "SETTLEMENT")
                            {
                                _syncedFromBy = surcharge.SettlementCode;
                            }
                        }
                        else
                        {
                            if (surcharge.Type == "BUY")
                            {
                                _cdNote = surcharge.CreditNo;
                            }
                            if (surcharge.Type == "SELL" || surcharge.Type == "OBH")
                            {
                                _cdNote = surcharge.DebitNo;
                            }

                            _isSynced = !string.IsNullOrEmpty(surcharge.SyncedFrom) && (surcharge.SyncedFrom.Equals("SOA") || surcharge.SyncedFrom.Equals("CDNOTE") || surcharge.SyncedFrom.Equals("VOUCHER") || surcharge.SyncedFrom.Equals("SETTLEMENT"));
                            if (surcharge.SyncedFrom == "SOA")
                            {
                                _syncedFromBy = chg.SOANo;
                            }
                            if (surcharge.SyncedFrom == "CDNOTE")
                            {
                                _syncedFromBy = _cdNote;
                            }
                            if (surcharge.SyncedFrom == "VOUCHER")
                            {
                                _syncedFromBy = surcharge.VoucherId;
                            }
                            if (surcharge.SyncedFrom == "SETTLEMENT")
                            {
                                _syncedFromBy = surcharge.SettlementCode;
                            }
                        }
                    }
                    chg.CDNote = _cdNote;
                    chg.IsSynced = _isSynced;
                    chg.SyncedFromBy = _syncedFromBy;

                    data.Add(chg);
                }
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
            var chargeShipments = GetChargesForDetailSoa(soaNo);
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
            data.TotalCharge = chargeShipments.Count;
            data.GroupShipments = _groupShipments.ToArray().OrderByDescending(o => o.JobId).ToList(); //Sắp xếp giảm dần theo số Job
            data.ChargeShipments = chargeShipments.ToArray().OrderByDescending(o => o.JobId).ToList(); //Sắp xếp giảm dần theo số Job
            data.AmountDebitLocal = Math.Round(chargeShipments.Sum(x => x.AmountDebitLocal), 3);
            data.AmountCreditLocal = Math.Round(chargeShipments.Sum(x => x.AmountCreditLocal), 3);
            data.AmountDebitUSD = Math.Round(chargeShipments.Sum(x => x.AmountDebitUSD), 3);
            data.AmountCreditUSD = Math.Round(chargeShipments.Sum(x => x.AmountCreditUSD), 3);
            //Thông tin các Service Name của SOA
            data.ServicesNameSoa = DataTypeEx.GetServiceNameOfSoa(data.ServiceTypeId).ToString();
            data.IsExistChgCurrDiffLocalCurr = soaDetail.Currency != AccountingConstants.CURRENCY_LOCAL || chargeShipments.Any(x => x.Currency != AccountingConstants.CURRENCY_LOCAL);
            return data;
        }

        #endregion --Details Soa--

        #region -- Data Export Details --
        public IQueryable<ExportSOAModel> GetDateExportDetailSOA(string soaNo)
        {
            //Lấy danh sách Currency Exchange của ngày hiện tại
            //var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();
            var soa = DataContext.Get(x => x.Soano == soaNo);

            // Expression<Func<ChargeSOAResult, bool>> query = chg => chg.SOANo == soaNo;
            var charge = GetChargeExportForSOA(soa.FirstOrDefault());
            var customerId = soa?.FirstOrDefault().Customer;
            var partner = catPartnerRepo.Get(x => x.Id == customerId || string.IsNullOrEmpty(customerId));
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
                                 ExchangeDate = chg.ExchangeDate
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

            // Expression<Func<ChargeSOAResult, bool>> query = chg => chg.SOANo == soaNo;
            var charge = GetChargeExportForSOA(soa.FirstOrDefault()).Where(x => x.TransactionType == "AI" || x.TransactionType == "AE");
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
                    air.Mawb = chargeData.MBL?.Trim();
                    air.AOD = port.Where(x => x.Id == chargeData.AOD).Select(t => t.Code).FirstOrDefault();
                    air.Service = "Normal"; // tạm thời hardcode;
                    air.Pcs = chargeData.PackageQty;
                    air.CW = chargeData.ChargeWeight;
                    air.GW = chargeData.GrossWeight;
                    var chargeAF = charge.Where(x => x.HBL == item && x.ChargeName.ToUpper() == AccountingConstants.CHARGE_AIR_FREIGHT.ToUpper());
                    air.Rate = chargeAF.FirstOrDefault()?.UnitPrice;

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
                                                                    && x.ChargeCode != AccountingConstants.CHARGE_SA_HDL_AIR_CODE
                                                                    && x.ChargeName.ToLower() != AccountingConstants.CHARGE_HANDLING_FEE.ToLower()
                        ) && x.Currency == AccountingConstants.CURRENCY_USD);
                    }

                    air.OTH = lstOTHFee.Count() > 0 ? lstOTHFee.Select(t => t.Debit).Sum() : null;

                    var lstHandlingFee = charge.Where(x => x.HBL == item && x.ChargeName.ToLower().Contains(AccountingConstants.CHARGE_HANDLING_FEE) && x.Currency == AccountingConstants.CURRENCY_USD);
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

                    // get exchange rate(vnd)
                    var _exchangeRateVND = currencyExchangeService.CurrencyExchangeRateConvert(chargeData.FinalExchangeRate, chargeData.ExchangeDate, chargeData.Currency, AccountingConstants.CURRENCY_LOCAL);
                    air.ExchangeRate = _exchangeRateVND;
                    //var dataCharge = charge.Where(x => x.ChargeName.ToLower() == AccountingConstants.CHARGE_AIR_FREIGHT.ToLower());
                    //if (dataCharge.Any())
                    //{
                    //    if (chargeData.FinalExchangeRate != null)
                    //    {
                    //        air.ExchangeRate = chargeData.FinalExchangeRate;
                    //    }
                    //    else
                    //    {
                    //        var dataCurrencyExchange = catCurrencyExchangeRepo.Get(x => x.CurrencyFromId == AccountingConstants.CURRENCY_USD && x.CurrencyToId == AccountingConstants.CURRENCY_LOCAL).OrderByDescending(x => x.DatetimeModified).AsQueryable();
                    //        var dataObjectCurrencyExchange = dataCurrencyExchange.Where(x => x.DatetimeModified.Value.Date == chargeData.DatetimeModified.Value.Date).FirstOrDefault();
                    //        air.ExchangeRate = dataObjectCurrencyExchange.Rate;
                    //    }
                    //}
                    //else
                    //{
                    //    var dataCurrencyExchange = catCurrencyExchangeRepo.Get(x => x.CurrencyFromId == AccountingConstants.CURRENCY_USD && x.CurrencyToId == AccountingConstants.CURRENCY_LOCAL).OrderByDescending(x => x.DatetimeModified).AsQueryable();
                    //    var dataObjectCurrencyExchange = dataCurrencyExchange.Where(x => x.DatetimeModified.Value.Date == chargeData.DatetimeModified.Value.Date).FirstOrDefault();
                    //    air.ExchangeRate = dataObjectCurrencyExchange.Rate;
                    //}

                    air.TotalAmount = Math.Round((air.NetAmount * air.ExchangeRate) ?? 0);

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

            // Expression<Func<ChargeSOAResult, bool>> query = chg => chg.SOANo == soaNo;
            var charge = GetChargeExportForSOA(soa.FirstOrDefault()).Where(x => x.TransactionType == "AI" || x.TransactionType == "AE");
            var results = charge.GroupBy(x => x.JobId).AsQueryable();

            if (results.Select(x => x.Key).Count() > 0)
            {
                var csTrans = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled);
                var csTransDe = csTransactionDetailRepo.Get();
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
                    var lstHandlingFee = charge.Where(x => x.JobId == item && (x.ChargeCode == AccountingConstants.CHARGE_BA_DHL_AIR_CODE || x.ChargeName.ToLower().Contains(AccountingConstants.CHARGE_HANDLING_FEE)));
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

        /// <summary>
        /// Get data export for SOA with soa no
        /// </summary>
        /// <param name="soa">AcctSoa</param>
        /// <returns></returns>
        public IQueryable<ChargeSOAResult> GetChargeExportForSOA(AcctSoa soa)
        {
            //Chỉ lấy những phí từ shipment (IsFromShipment = true)
            var surCharges = csShipmentSurchargeRepo.Get(x => soa.Type == "Debit" ? x.Soano == soa.Soano : x.PaySoano == soa.Soano);                    
            // BUY & SELL
            var result = new List<ChargeSOAResult>();
            foreach (var sur in surCharges)
            {
                var charge = catChargeRepo.Get().Where(x => x.Id == sur.ChargeId).FirstOrDefault();
                var unit = catUnitRepo.Get().Where(x => x.Id == sur.UnitId).FirstOrDefault();
                DateTime? _serviceDate, _createdDate, _shippmentDate;
                string _service, _userCreated, _commodity, _flightNo, _packageContainer, _customNo;
                Guid? _aol, _aod;
                int? _packageQty;
                decimal? _grossWeight, _chargeWeight, _cbm;
                if (sur.TransactionType == "CL")
                {
                    var opst = opsTransactionRepo.Get().Where(x => x.Hblid == sur.Hblid).FirstOrDefault();
                    _serviceDate = opst?.ServiceDate;
                    _createdDate = opst?.DatetimeCreated;
                    _service = "CL";
                    _userCreated = opst?.UserCreated;
                    _commodity = string.Empty;
                    _flightNo = string.Empty;
                    _shippmentDate = null;
                    _aol = null;
                    _aod = null;
                    _packageContainer = string.Empty;
                    _packageQty = opst?.SumPackages;
                    _grossWeight = opst?.SumGrossWeight;
                    _chargeWeight = opst ?.SumChargeWeight;
                    _cbm = opst?.SumCbm;
                    _customNo = customsDeclarationRepo.Get().Where(x => x.JobNo == opst.JobNo).OrderByDescending(x => x.ClearanceDate).FirstOrDefault()?.ClearanceNo;
                } else {
                    var csTransDe = csTransactionDetailRepo.Get(x => x.Id == sur.Hblid).FirstOrDefault();
                    var csTrans = csTransDe == null ? new CsTransaction() : csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled && x.Id == csTransDe.JobId).FirstOrDefault();
                    _serviceDate = (csTrans?.TransactionType == "AI" || csTrans?.TransactionType == "SFI" || csTrans?.TransactionType == "SLI" || csTrans?.TransactionType == "SCI") ?
                        csTrans?.Eta : csTrans?.Etd;
                    _createdDate = csTrans?.DatetimeCreated;
                    _service = csTrans.TransactionType;
                    _userCreated = csTrans?.UserCreated;
                    _commodity = csTrans?.Commodity;
                    _flightNo = csTransDe?.FlightNo;
                    _shippmentDate = csTrans?.TransactionType == "AE" ? csTransDe?.Etd : csTrans?.TransactionType == "AI" ? csTransDe?.Eta : null;
                    _aol = csTrans?.Pol;
                    _aod = csTrans?.Pod;
                    _packageQty = csTransDe?.PackageQty;
                    _grossWeight = csTransDe?.GrossWeight;
                    _chargeWeight = csTransDe?.ChargeWeight;
                    _cbm = csTransDe?.Cbm;
                    _packageContainer = csTransDe?.PackageContainer;
                    _customNo = string.Empty;
                }
                var chg = new ChargeSOAResult()
                {
                    ID = sur.Id,
                    HBLID = sur.Hblid,
                    ChargeID = sur.ChargeId,
                    ChargeCode = charge?.Code,
                    ChargeName = charge?.ChargeNameEn,
                    JobId = sur.JobNo,
                    HBL = sur.Hblno,
                    MBL = sur.Mblno,
                    Type = sur.Type,
                    CustomNo = _customNo,
                    Debit = sur.Type == AccountingConstants.TYPE_CHARGE_SELL || (sur.PaymentObjectId == soa.Customer && sur.Type == "OBH") ? (decimal?)sur.Total : null,
                    Credit = sur.Type == AccountingConstants.TYPE_CHARGE_BUY || (sur.PayerId == soa.Customer && sur.Type == "OBH") ? (decimal?)sur.Total : null,
                    SOANo = soa.Type == "Debit" ? sur.Soano : sur.PaySoano,
                    IsOBH = false,
                    Currency = sur.CurrencyId,
                    InvoiceNo = sur.InvoiceNo,
                    Note = sur.Notes,
                    CustomerID = sur.PaymentObjectId,
                    ServiceDate = _serviceDate,
                    CreatedDate = _createdDate,
                    InvoiceIssuedDate = null,
                    TransactionType = _service,
                    UserCreated = _userCreated,
                    Commodity = _commodity,
                    FlightNo = _flightNo,
                    ShippmentDate = _shippmentDate,
                    AOL = _aol,
                    AOD = _aod,
                    Quantity = sur.Quantity,
                    UnitId = sur.UnitId,
                    Unit = unit?.UnitNameEn,
                    UnitPrice = sur.UnitPrice,
                    VATRate = sur.Vatrate,
                    PackageQty = _packageQty,
                    GrossWeight = _grossWeight,
                    ChargeWeight = _chargeWeight,
                    CBM = _cbm,
                    PackageContainer = _packageContainer,
                    CreditDebitNo = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? sur.DebitNo : sur.CreditNo,
                    DatetimeModified = sur.DatetimeModified,
                    CommodityGroupID = null,
                    Service = _service,
                    CDNote = !string.IsNullOrEmpty(sur.CreditNo) ? sur.CreditNo : sur.DebitNo,
                    TypeCharge = charge?.Type,
                    ExchangeDate = sur.ExchangeDate,
                    FinalExchangeRate = sur.FinalExchangeRate,
                    PIC = null,
                    IsSynced = !string.IsNullOrEmpty(sur.SyncedFrom) && (sur.SyncedFrom.Equals("SOA") || sur.SyncedFrom.Equals("CDNOTE") || sur.SyncedFrom.Equals("VOUCHER") || sur.SyncedFrom.Equals("SETTLEMENT"))
                };
                result.Add(chg);
            }
            return result.OrderBy(x => x.Service).AsQueryable();
        }

        public SOAOPSModel GetSOAOPS(string soaNo)
        {
            SOAOPSModel opssoa = new SOAOPSModel();
            var soa = DataContext.Get(x => x.Soano == soaNo).FirstOrDefault();
            if (soa == null)
            {
                return opssoa;
            }
            var charge = GetChargeExportForSOA(soa);
            if (soa.Type?.ToLower() != AccountingConstants.TYPE_SOA_CREDIT.ToLower() && soa.Type?.ToLower() != AccountingConstants.TYPE_SOA_DEBIT.ToLower())
            {
                charge = charge.Where(x => x.TypeCharge.ToLower() == AccountingConstants.TYPE_SOA_DEBIT.ToLower() || x.TypeCharge.ToLower() == AccountingConstants.TYPE_SOA_OBH.ToLower());
            }
            List<ExportSOAOPS> lstSOAOPS = new List<ExportSOAOPS>();
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
            var customerId = soa.Customer;
            var partner = catPartnerRepo.Get(x => x.Id == customerId).FirstOrDefault();
            opssoa.BillingAddressVN = partner?.AddressVn;
            opssoa.PartnerNameVN = partner?.PartnerNameVn;
            opssoa.FromDate = soa.SoaformDate;
            opssoa.SoaNo = soa.Soano;

            foreach (var item in opssoa.exportSOAOPs)
            {
                foreach (var it in item.Charges)
                {
                    decimal? percent = 0;
                    if (it.VATRate > 0)
                    {
                        percent = it.VATRate / 100;
                        it.VATAmount = percent * (it.UnitPrice * it.Quantity);
                        if (it.Currency != "VND")
                        {
                            it.VATAmount = Math.Round(it.VATAmount ?? 0, 2);

                        }
                        else
                        {
                            it.VATAmount = Math.Round(it.VATAmount ?? 0);
                        }
                    }
                    else
                    {
                        it.VATAmount = (it.Currency == "VND" ? Math.Round(it.VATRate ?? 0) : Math.Round(it.VATRate ?? 0, 2));
                    }

                    it.NetAmount = (it.Currency == "VND" ? Math.Round((it.UnitPrice * it.Quantity) ?? 0) : Math.Round((it.UnitPrice * it.Quantity) ?? 0, 2));
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
            // Expression<Func<ChargeSOAResult, bool>> query = chg => chg.SOANo == soaNo;
            var chargeDefaults = chargeDefaultRepo.Get(x => x.Type == "Công Nợ");
            var charge = GetChargeExportForSOA(soa.FirstOrDefault());
            var customerId = soa.FirstOrDefault()?.Customer;
            var partner = catPartnerRepo.Get(x => x.Id == customerId);
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
                                 Service = DataTypeEx.GetServiceNameOfSoa(chg.Service).ToString(),
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
                soaCharge.Docs = string.Join("\r\n", grpInvCdNoteByHbl.ToList().Where(w => w.Hblid == charge.Hblid).Select(s => !string.IsNullOrEmpty(s.InvoiceNo) ? s.InvoiceNo : s.CdNote).Distinct()); //Ưu tiên: Invoice No >> CD Note Code

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

        public HandleState RejectSoaCredit(RejectSoaModel model)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var soa = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
                    if (soa == null) return new HandleState((object)"Not found SOA");

                    soa.SyncStatus = "Rejected";
                    soa.UserModified = currentUser.UserID;
                    soa.DatetimeModified = DateTime.Now;
                    soa.ReasonReject = model.Reason;

                    HandleState hs = DataContext.Update(soa, x => x.Id == soa.Id, false);
                    if (hs.Success)
                    {
                        HandleState smSoa = DataContext.SubmitChanges();
                        if (smSoa.Success)
                        {
                            string title = string.Format(@"Accountant Rejected Data SOA {0}", soa.Soano);
                            SysNotifications sysNotify = new SysNotifications
                            {
                                Id = Guid.NewGuid(),
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                Type = "User",
                                Title = title,
                                IsClosed = false,
                                IsRead = false,
                                Description = model.Reason,
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                                Action = "Detail",
                                ActionLink = string.Format(@"home/accounting/statement-of-account/detail?no={0}&currency=VND", soa.Soano),
                                UserIds = soa.UserCreated
                            };
                            sysNotificationRepository.Add(sysNotify);

                            SysUserNotification sysUserNotify = new SysUserNotification
                            {
                                Id = Guid.NewGuid(),
                                UserId = soa.UserCreated,
                                Status = "New",
                                NotitficationId = sysNotify.Id,
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                            };
                            sysUserNotificationRepository.Add(sysUserNotify);

                            //Update PaySyncedFrom or SyncedFrom equal NULL by SOA No
                            var surcharges = csShipmentSurchargeRepo.Get(x => x.Soano == soa.Soano || x.PaySoano == soa.Soano);
                            foreach (var surcharge in surcharges)
                            {
                                if (surcharge.Type == "OBH")
                                {
                                    surcharge.PaySyncedFrom = (soa.Soano == surcharge.PaySoano) ? null : surcharge.PaySyncedFrom;
                                    surcharge.SyncedFrom = (soa.Soano == surcharge.Soano) ? null : surcharge.SyncedFrom;
                                }
                                else
                                {
                                    surcharge.SyncedFrom = null;
                                }
                                surcharge.UserModified = currentUser.UserID;
                                surcharge.DatetimeModified = DateTime.Now;
                                var hsUpdateSurcharge = csShipmentSurchargeRepo.Update(surcharge, x => x.Id == surcharge.Id, false);
                            }
                            var smSurcharge = csShipmentSurchargeRepo.SubmitChanges();
                        }
                        trans.Commit();
                    }
                    return hs;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState((object)ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }
    }
}
