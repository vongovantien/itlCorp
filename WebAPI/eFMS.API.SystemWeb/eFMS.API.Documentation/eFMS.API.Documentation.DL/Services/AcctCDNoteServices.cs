using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Models.Exports;
using eFMS.API.Documentation.DL.Models.ReportResults;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace eFMS.API.Documentation.DL.Services
{
    public class AcctCDNoteServices : RepositoryBase<AcctCdnote, AcctCdnoteModel>, IAcctCDNoteServices
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        IContextBase<CsShipmentSurcharge> surchargeRepository;
        IContextBase<OpsTransaction> opstransRepository;
        IContextBase<CsTransaction> cstransRepository;
        IContextBase<CatPartner> partnerRepositoty;
        IContextBase<CsTransactionDetail> trandetailRepositoty;
        IContextBase<CatCharge> catchargeRepository;
        IContextBase<CatCurrency> currencyRepository;
        IContextBase<CatUnit> unitRepository;
        IContextBase<CatPlace> placeRepository;
        IContextBase<CatCurrencyExchange> catCurrencyExchangeRepository;
        IContextBase<CatCountry> countryRepository;
        IContextBase<CsMawbcontainer> csMawbcontainerRepository;
        IContextBase<SysUser> sysUserRepo;
        IContextBase<SysEmployee> sysEmployeeRepo;
        IContextBase<SysOffice> sysOfficeRepo;
        ICsShipmentSurchargeService surchargeService;
        ICsTransactionDetailService transactionDetailService;
        IContextBase<CustomsDeclaration> customsDeclarationRepository;
        IContextBase<SysCompany> sysCompanyRepository;
        IContextBase<CatContract> catContractRepo;
        IContextBase<SysNotifications> sysNotificationRepository;
        IContextBase<SysUserNotification> sysUserNotificationRepository;
        IContextBase<CatCommodityGroup> catCommodityGroupRepository;
        IContextBase<AccAccountingManagement> accountingManagementRepository;
        private readonly ICurrencyExchangeService currencyExchangeService;
        private decimal _decimalNumber = Constants.DecimalNumber;

        public AcctCDNoteServices(IStringLocalizer<LanguageSub> localizer,
            IContextBase<AcctCdnote> repository, IMapper mapper, ICurrentUser user,
            IContextBase<CsShipmentSurcharge> surchargeRepo,
            IContextBase<OpsTransaction> opstransRepo,
            IContextBase<CsTransaction> cstransRepo,
            IContextBase<CatPartner> partnerRepo,
            IContextBase<CsTransactionDetail> trandetailRepo,
            IContextBase<CatCharge> catchargeRepo,
            IContextBase<CatCurrency> currencyRepo,
            IContextBase<CatUnit> unitRepo,
            IContextBase<CatPlace> placeRepo,
            IContextBase<CatCurrencyExchange> catCurrencyExchangeRepo,
            IContextBase<CatCountry> countryRepo,
            IContextBase<CsMawbcontainer> csMawbcontainerRepo,
            IContextBase<SysUser> sysUser,
            IContextBase<SysEmployee> sysEmployee,
            IContextBase<SysOffice> sysOffice,
            ICsShipmentSurchargeService surcharge,
            ICsTransactionDetailService transDetailService,
            IContextBase<CustomsDeclaration> customsDeclarationRepo,
            ICurrencyExchangeService currencyExchange,
            IContextBase<SysCompany> sysCompanyRepo,
            IContextBase<CatContract> catContract,
            IContextBase<SysNotifications> sysNotifyRepo,
            IContextBase<SysUserNotification> sysUsernotifyRepo,
            IContextBase<CatCommodityGroup> catCommodityGroupRepo,
            IContextBase<AccAccountingManagement> accountingManagementRepo
            ) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            surchargeRepository = surchargeRepo;
            opstransRepository = opstransRepo;
            cstransRepository = cstransRepo;
            partnerRepositoty = partnerRepo;
            trandetailRepositoty = trandetailRepo;
            catchargeRepository = catchargeRepo;
            currencyRepository = currencyRepo;
            unitRepository = unitRepo;
            placeRepository = placeRepo;
            catCurrencyExchangeRepository = catCurrencyExchangeRepo;
            countryRepository = countryRepo;
            csMawbcontainerRepository = csMawbcontainerRepo;
            sysUserRepo = sysUser;
            sysEmployeeRepo = sysEmployee;
            sysOfficeRepo = sysOffice;
            surchargeService = surcharge;
            transactionDetailService = transDetailService;
            currencyExchangeService = currencyExchange;
            customsDeclarationRepository = customsDeclarationRepo;
            sysCompanyRepository = sysCompanyRepo;
            catContractRepo = catContract;
            sysNotificationRepository = sysNotifyRepo;
            sysUserNotificationRepository = sysUsernotifyRepo;
            catCommodityGroupRepository = catCommodityGroupRepo;
            accountingManagementRepository = accountingManagementRepo;
        }

        private string CreateCode(string typeCDNote, TransactionTypeEnum typeEnum)
        {
            string code = string.Empty;
            SysOffice office = null;
            var currentUserOffice = currentUser?.OfficeID ?? null;
            if (currentUserOffice != null)
            {
                office = sysOfficeRepo.Get(x => x.Id == currentUserOffice).FirstOrDefault();
                code = SetPrefixJobIdByOfficeCode(office?.Code);
            }

            switch (typeEnum)
            {
                case TransactionTypeEnum.CustomLogistic:
                    code += DocumentConstants.LG_SHIPMENT;
                    break;
                case TransactionTypeEnum.InlandTrucking:
                    code += DocumentConstants.IT_SHIPMENT;
                    break;
                case TransactionTypeEnum.AirExport:
                    code += DocumentConstants.AE_SHIPMENT;
                    break;
                case TransactionTypeEnum.AirImport:
                    code += DocumentConstants.AI_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaConsolExport:
                    code += DocumentConstants.SEC_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaConsolImport:
                    code += DocumentConstants.SIC_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaFCLExport:
                    code += DocumentConstants.SEF_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaFCLImport:
                    code += DocumentConstants.SIF_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaLCLExport:
                    code += DocumentConstants.SEL_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaLCLImport:
                    code += DocumentConstants.SIL_SHIPMENT;
                    break;
                default:
                    break;
            }
            switch (typeCDNote)
            {
                case "CREDIT":
                    code = code + "CN";
                    break;
                case "DEBIT":
                    code = code + "DN";
                    break;
                case "INVOICE":
                    code = code + "IV";
                    break;
            }
            int count = 0;
            var cdCode = GetCdNoteToGenerateCode(office, code)?.Code;
            if (cdCode != null)
            {
                cdCode = cdCode.Substring(code.Length + 4, 5);
                Int32.TryParse(cdCode, out count);
            }
            code = GenerateID.GenerateCDNoteNo(code, count);
            return code;
        }

        private AcctCdnote GetCdNoteToGenerateCode(SysOffice office, string code)
        {
            AcctCdnote currentCdNote = null;
            var currentCdNotes = DataContext.Get(x => x.Code.StartsWith(code)
                                                    && x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                    && x.DatetimeCreated.Value.Year == DateTime.Now.Year)
                                                    .OrderByDescending(x => x.DatetimeCreated);
            if (office != null)
            {
                if (office.Code == "ITLHAN")
                {
                    currentCdNote = currentCdNotes.Where(x => x.Code.StartsWith("H") && !x.Code.StartsWith("HAN-")).FirstOrDefault(); //CR: HAN -> H [15202]
                }
                else if (office.Code == "ITLDAD")
                {
                    currentCdNote = currentCdNotes.Where(x => x.Code.StartsWith("D") && !x.Code.StartsWith("DAD-")).FirstOrDefault(); //CR: DAD -> D [15202]
                }
                else
                {
                    currentCdNote = currentCdNotes.Where(x => !x.Code.StartsWith("D") && !x.Code.StartsWith("DAD-")
                                                           && !x.Code.StartsWith("H") && !x.Code.StartsWith("HAN-")).FirstOrDefault();
                }
            }
            else
            {
                currentCdNote = currentCdNotes.Where(x => !x.Code.StartsWith("D") && !x.Code.StartsWith("DAD-")
                                                       && !x.Code.StartsWith("H") && !x.Code.StartsWith("HAN-")).FirstOrDefault();
            }
            return currentCdNote;
        }

        private string SetPrefixJobIdByOfficeCode(string officeCode)
        {
            string prefixCode = string.Empty;
            if (!string.IsNullOrEmpty(officeCode))
            {
                if (officeCode == "ITLHAN")
                {
                    prefixCode = "H"; //HAN- >> H
                }
                else if (officeCode == "ITLDAD")
                {
                    prefixCode = "D"; //DAD- >> D
                }
            }
            return prefixCode;
        }

        private string GetTransactionType(TransactionTypeEnum typeEnum)
        {
            string _transactionType = string.Empty;
            switch (typeEnum)
            {
                case TransactionTypeEnum.CustomLogistic:
                    _transactionType = TermData.CustomLogistic;
                    break;
                case TransactionTypeEnum.InlandTrucking:
                    _transactionType = TermData.InlandTrucking;
                    break;
                case TransactionTypeEnum.AirExport:
                    _transactionType = TermData.AirExport;
                    break;
                case TransactionTypeEnum.AirImport:
                    _transactionType = TermData.AirImport;
                    break;
                case TransactionTypeEnum.SeaConsolExport:
                    _transactionType = TermData.SeaConsolExport;
                    break;
                case TransactionTypeEnum.SeaConsolImport:
                    _transactionType = TermData.SeaConsolImport;
                    break;
                case TransactionTypeEnum.SeaFCLExport:
                    _transactionType = TermData.SeaFCLExport;
                    break;
                case TransactionTypeEnum.SeaFCLImport:
                    _transactionType = TermData.SeaFCLImport;
                    break;
                case TransactionTypeEnum.SeaLCLExport:
                    _transactionType = TermData.SeaLCLExport;
                    break;
                case TransactionTypeEnum.SeaLCLImport:
                    _transactionType = TermData.SeaLCLImport;
                    break;
                default:
                    break;
            }
            return _transactionType;
        }

        public HandleState AddNewCDNote(AcctCdnoteModel model)
        {
            try
            {
                model.Id = Guid.NewGuid();
                model.Code = CreateCode(model.Type, model.TransactionTypeEnum);
                model.UserCreated = currentUser.UserID;
                model.DatetimeCreated = DateTime.Now;
                model.Status = TermData.CD_NOTE_NEW;
                model.GroupId = currentUser.GroupId;
                model.OfficeId = currentUser.OfficeID;
                model.DepartmentId = currentUser.DepartmentId;
                model.CompanyId = currentUser.CompanyID;

                #region --- Set Currency For CD Note ---
                CatPartner _partnerAcRef = new CatPartner();
                var _partner = partnerRepositoty.Get(x => x.Id == model.PartnerId).FirstOrDefault();
                if (!string.IsNullOrEmpty(_partner?.ParentId) && _partner?.ParentId != _partner?.Id)
                {
                    _partnerAcRef = partnerRepositoty.Get(x => x.Id == _partner.ParentId).FirstOrDefault();
                }
                else
                {
                    _partnerAcRef = _partner;
                }
                var _transactionType = GetTransactionType(model.TransactionTypeEnum);
                var _contractAcRef = catContractRepo.Get(x => x.Active == true && x.PartnerId == (_partnerAcRef != null ? _partnerAcRef.Id : string.Empty) && x.OfficeId.Contains(currentUser.OfficeID.ToString()) && x.SaleService.Contains(_transactionType)).FirstOrDefault();
                if (!string.IsNullOrEmpty(_contractAcRef?.CurrencyId))
                {
                    model.CurrencyId = _contractAcRef.CurrencyId;
                }
                else
                {
                    model.CurrencyId = (_partnerAcRef?.PartnerLocation == DocumentConstants.PARTNER_LOCATION_OVERSEA) ? DocumentConstants.CURRENCY_USD : DocumentConstants.CURRENCY_LOCAL;
                }
                #endregion  --- Set Currency For CD Note ---

                //Quy đổi tỉ giá currency CD Note về currency Local
                var _exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(null, model.DatetimeCreated, model.CurrencyId, DocumentConstants.CURRENCY_LOCAL);
                model.ExchangeRate = _exchangeRate;
                //Quy đổi tỉ giá USD to Local dựa vào ngày tạo CDNote
                var _excRateUsdToLocal = currencyExchangeService.CurrencyExchangeRateConvert(null, model.DatetimeCreated, DocumentConstants.CURRENCY_USD, DocumentConstants.CURRENCY_LOCAL);
                model.ExcRateUsdToLocal = _excRateUsdToLocal;

                decimal _totalCdNote = 0;
                foreach (var c in model.listShipmentSurcharge)
                {
                    var charge = surchargeRepository.Get(x => x.Id == c.Id).FirstOrDefault();
                    if (charge != null)
                    {
                        if (charge.Type == DocumentConstants.CHARGE_BUY_TYPE)
                        {
                            charge.CreditNo = model.Code;
                        }
                        else if (charge.Type == DocumentConstants.CHARGE_SELL_TYPE)
                        {
                            charge.DebitNo = model.Code;
                        }
                        else
                        {
                            if (model.PartnerId == charge.PaymentObjectId)
                            {
                                charge.DebitNo = model.Code;
                            }
                            if (model.PartnerId == charge.PayerId)
                            {
                                charge.CreditNo = model.Code;
                            }
                        }

                        if (string.IsNullOrEmpty(charge.Soano) && string.IsNullOrEmpty(charge.PaySoano))
                        {
                            //Cập nhật ExchangeDate của phí theo ngày Created Date CD Note & phí chưa có tạo SOA
                            charge.ExchangeDate = model.DatetimeCreated.HasValue ? model.DatetimeCreated.Value.Date : model.DatetimeCreated;
                            //FinalExchangeRate = null do cần tính lại dựa vào ExchangeDate mới
                            charge.FinalExchangeRate = null;

                            var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(charge);
                            charge.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                            charge.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                            charge.FinalExchangeRate = amountSurcharge.FinalExchangeRate; //Tỉ giá so với Local
                            charge.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                            charge.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                            charge.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                            charge.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)
                        }

                        charge.DatetimeModified = DateTime.Now;
                        charge.UserModified = currentUser.UserID;

                        if (model.CurrencyId == DocumentConstants.CURRENCY_LOCAL)
                        {
                            _totalCdNote += (charge.AmountVnd + charge.VatAmountVnd) ?? 0;
                        }
                        if (model.CurrencyId == DocumentConstants.CURRENCY_USD)
                        {
                            _totalCdNote += (charge.AmountUsd + charge.VatAmountUsd) ?? 0;
                        }
                    }
                    var hsSurcharge = surchargeRepository.Update(charge, x => x.Id == charge.Id, false);
                }
                model.Total = _totalCdNote;

                var hs = new HandleState();
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        hs = DataContext.Add(model, false);
                        var sc = DataContext.SubmitChanges();

                        UpdateJobModifyTime(model.JobId);

                        if (hs.Success)
                        {
                            var hsSc = surchargeRepository.SubmitChanges();
                            var hsOt = opstransRepository.SubmitChanges();
                            var hsCt = cstransRepository.SubmitChanges();
                        }
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        hs = new HandleState(ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
                return hs;
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        public HandleState UpdateCDNote(AcctCdnoteModel model)
        {
            try
            {
                var cdNote = DataContext.First(x => (x.Id == model.Id && x.Code == model.Code));
                if (cdNote == null)
                {
                    return new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_CDNOTE_NOT_NOT_FOUND].Value);
                }
                var entity = mapper.Map<AcctCdnote>(model);
                entity.GroupId = cdNote.GroupId;
                entity.DepartmentId = cdNote.DepartmentId;
                entity.OfficeId = cdNote.OfficeId;
                entity.CompanyId = cdNote.CompanyId;
                entity.LastSyncDate = cdNote.LastSyncDate;
                entity.SyncStatus = cdNote.SyncStatus;
                entity.ReasonReject = cdNote.ReasonReject;
                entity.ExcRateUsdToLocal = cdNote.ExcRateUsdToLocal;

                #region --- Set Currency For CD Note ---
                CatPartner _partnerAcRef = new CatPartner();
                var _partner = partnerRepositoty.Get(x => x.Id == model.PartnerId).FirstOrDefault();
                if (!string.IsNullOrEmpty(_partner?.ParentId) && _partner?.ParentId != _partner?.Id)
                {
                    _partnerAcRef = partnerRepositoty.Get(x => x.Id == _partner.ParentId).FirstOrDefault();
                }
                else
                {
                    _partnerAcRef = _partner;
                }
                var _transactionType = GetTransactionType(model.TransactionTypeEnum);
                var _contractAcRef = catContractRepo.Get(x => x.Active == true && x.PartnerId == (_partnerAcRef != null ? _partnerAcRef.Id : string.Empty) && x.OfficeId.Contains(currentUser.OfficeID.ToString()) && x.SaleService.Contains(_transactionType)).FirstOrDefault();
                if (!string.IsNullOrEmpty(_contractAcRef?.CurrencyId))
                {
                    entity.CurrencyId = _contractAcRef.CurrencyId;
                }
                else
                {
                    entity.CurrencyId = (_partnerAcRef?.PartnerLocation == DocumentConstants.PARTNER_LOCATION_OVERSEA) ? DocumentConstants.CURRENCY_USD : DocumentConstants.CURRENCY_LOCAL;
                }
                #endregion  --- Set Currency For CD Note ---

                if (cdNote.CurrencyId != entity.CurrencyId)
                {
                    //Quy đổi tỉ giá currency CD Note về currency Local
                    var _exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(null, model.DatetimeCreated, model.CurrencyId, DocumentConstants.CURRENCY_LOCAL);
                    entity.ExchangeRate = _exchangeRate;
                }
                else
                {
                    entity.ExchangeRate = cdNote.ExchangeRate;
                }

                //***Note: Khi update CD Note thì không cần cập nhật tỷ giá ExcRateUsdToLocal của CDNote

                decimal _totalCdNote = 0;

                var chargeOfCdNote = surchargeRepository.Get(x => x.CreditNo == cdNote.Code || x.DebitNo == cdNote.Code);
                //Cập nhật các credit debit note code của của các charge thành null
                foreach (var item in chargeOfCdNote)
                {
                    item.DatetimeModified = DateTime.Now;
                    item.UserModified = currentUser.UserID;
                    if (item.CreditNo == cdNote.Code)
                    {
                        item.CreditNo = null;
                    }
                    if (item.DebitNo == cdNote.Code)
                    {
                        item.DebitNo = null;
                    }
                    var hsSur = surchargeRepository.Update(item, x => x.Id == item.Id, false);
                }

                foreach (var c in model.listShipmentSurcharge)
                {
                    var charge = surchargeRepository.Get(x => x.Id == c.Id).FirstOrDefault();
                    if (charge != null)
                    {
                        if (charge.Type == DocumentConstants.CHARGE_BUY_TYPE)
                        {
                            charge.CreditNo = model.Code;
                        }
                        else if (charge.Type == DocumentConstants.CHARGE_SELL_TYPE)
                        {
                            charge.DebitNo = model.Code;
                        }
                        else
                        {
                            if (model.PartnerId == charge.PaymentObjectId)
                            {
                                charge.DebitNo = model.Code;
                            }
                            if (model.PartnerId == charge.PayerId)
                            {
                                charge.CreditNo = model.Code;
                            }
                        }

                        if (string.IsNullOrEmpty(charge.Soano) && string.IsNullOrEmpty(charge.PaySoano))
                        {
                            //Cập nhật ExchangeDate của phí theo ngày Created Date CD Note & phí chưa có tạo SOA
                            charge.ExchangeDate = model.DatetimeCreated.HasValue ? model.DatetimeCreated.Value.Date : model.DatetimeCreated;

                            if (charge.CurrencyId == DocumentConstants.CURRENCY_USD)
                            {
                                charge.FinalExchangeRate = cdNote.ExcRateUsdToLocal;
                            }
                            else if (charge.CurrencyId == DocumentConstants.CURRENCY_LOCAL)
                            {
                                charge.FinalExchangeRate = 1;
                            }
                            else
                            {
                                charge.FinalExchangeRate = null;
                            }

                            var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(charge);
                            charge.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                            charge.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                            charge.FinalExchangeRate = amountSurcharge.FinalExchangeRate; //Tỉ giá so với Local
                            charge.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                            charge.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                            charge.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                            charge.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)
                        }

                        charge.DatetimeModified = DateTime.Now;
                        charge.UserModified = currentUser.UserID;
                        charge.Cdclosed = true;

                        if (model.CurrencyId == DocumentConstants.CURRENCY_LOCAL)
                        {
                            _totalCdNote += (charge.AmountVnd + charge.VatAmountVnd) ?? 0;
                        }
                        if (model.CurrencyId == DocumentConstants.CURRENCY_USD)
                        {
                            _totalCdNote += (charge.AmountUsd + charge.VatAmountUsd) ?? 0;
                        }
                    }
                    var hsSurcharge = surchargeRepository.Update(charge, x => x.Id == charge.Id, false);
                }
                entity.Total = _totalCdNote;

                var hs = new HandleState();
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        hs = DataContext.Update(entity, x => x.Id == cdNote.Id, false);
                        var sc = DataContext.SubmitChanges();

                        UpdateJobModifyTime(model.Id);

                        var hsSurSc = surchargeRepository.SubmitChanges();
                        var hsOtSc = opstransRepository.SubmitChanges();
                        var hsCtSc = cstransRepository.SubmitChanges();

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        hs = new HandleState(ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
                return hs;
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        public List<object> GroupCDNoteByPartner(Guid id, bool IsShipmentOperation)
        {
            try
            {
                List<object> returnList = new List<object>();
                List<CatPartner> listPartners = new List<CatPartner>();
                List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();

                if (IsShipmentOperation == false)
                {
                    var csShipment = cstransRepository.Get(x => x.Id == id)?.FirstOrDefault();
                    var houseBillPermission = transactionDetailService.GetHouseBill(csShipment.TransactionType);
                    List<CsTransactionDetail> housebills = houseBillPermission.Where(x => x.JobId == id && x.ParentId == null).ToList();

                    List<CsShipmentSurchargeDetailsModel> _listCharges = new List<CsShipmentSurchargeDetailsModel>();
                    foreach (var housebill in housebills)
                    {
                        if (housebill != null)
                        {
                            _listCharges = Query(housebill.Id);

                            foreach (var c in _listCharges)
                            {
                                if (c.PaymentObjectId != null)
                                {
                                    var partner = partnerRepositoty.Get(x => x.Id == c.PaymentObjectId).FirstOrDefault();
                                    if (partner != null) listPartners.Add(partner);
                                }
                                if (c.PayerId != null)
                                {
                                    var partner = partnerRepositoty.Get(x => x.Id == c.PayerId).FirstOrDefault();
                                    if (partner != null) listPartners.Add(partner);
                                }
                            }
                        }
                        listCharges.AddRange(_listCharges);
                    }
                }
                else
                {
                    var hblid = opstransRepository.Get(x => x.Id == id).FirstOrDefault()?.Hblid;
                    listCharges = Query(hblid.Value);

                    foreach (var c in listCharges)
                    {
                        if (c.PaymentObjectId != null)
                        {
                            var partner = partnerRepositoty.Get(x => x.Id == c.PaymentObjectId).FirstOrDefault();
                            if (partner != null) listPartners.Add(partner);
                        }
                        if (c.PayerId != null)
                        {
                            var partner = partnerRepositoty.Get(x => x.Id == c.PayerId).FirstOrDefault();
                            if (partner != null) listPartners.Add(partner);
                        }
                    }
                }

                listPartners = listPartners.Distinct().ToList();
                foreach (var item in listPartners)
                {
                    var cdNotes = DataContext.Where(x => x.PartnerId == item.Id && x.JobId == id).ToList();
                    var cdNotesModel = mapper.Map<List<AcctCdnoteModel>>(cdNotes);
                    List<object> listCDNote = new List<object>();
                    foreach (var cdNote in cdNotesModel)
                    {
                        var chargesOfCDNote = listCharges.Where(x => x.CreditNo == cdNote.Code || x.DebitNo == cdNote.Code);
                        cdNote.soaNo = string.Join(", ", chargesOfCDNote.Where(x => !string.IsNullOrEmpty(x.Soano) || !string.IsNullOrEmpty(x.PaySoano)).Select(x => !string.IsNullOrEmpty(x.Soano) ? x.Soano : x.PaySoano).Distinct());
                        cdNote.total_charge = chargesOfCDNote.Count();
                        cdNote.UserCreated = sysUserRepo.Get(x => x.Id == cdNote.UserCreated).FirstOrDefault()?.Username;
                        var _cdCurrency = chargesOfCDNote.Select(s => new
                        {
                            Currency = s.CurrencyId,
                            Debit = (s.Type == DocumentConstants.CHARGE_SELL_TYPE || (s.Type == DocumentConstants.CHARGE_OBH_TYPE && cdNote.PartnerId == s.PaymentObjectId)) ? s.Total : 0,
                            Credit = (s.Type == DocumentConstants.CHARGE_BUY_TYPE || (s.Type == DocumentConstants.CHARGE_OBH_TYPE && cdNote.PartnerId == s.PayerId)) ? s.Total : 0
                        });
                        var _balanceByCurrency = _cdCurrency.GroupBy(g => new { g.Currency }).Select(s => new { currency = s.Key.Currency, balance = s.Sum(su => su.Debit) - s.Sum(su => su.Credit), balancePositive = Math.Abs(s.Sum(su => su.Debit) - s.Sum(su => su.Credit)) });
                        cdNote.balanceCdNote = _balanceByCurrency;
                        cdNote.SyncStatus = cdNote.SyncStatus;
                        cdNote.LastSyncDate = cdNote.LastSyncDate;
                        listCDNote.Add(cdNote);
                    }

                    var obj = new { item.PartnerNameEn, item.PartnerNameVn, item.Id, listCDNote };
                    if (listCDNote.Count > 0)
                    {
                        returnList.Add(obj);
                    }

                }
                return returnList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<CsShipmentSurchargeDetailsModel> Query(Guid hbId)
        {
            var surcharges = surchargeService.GetByHB(hbId);
            return surcharges;
        }

        public AcctCDNoteDetailsModel GetCDNoteDetails(Guid JobId, string cdNo, List<AcctCdnoteModel> acctCdNoteList = null)
        {
            var places = placeRepository.Get();
            AcctCDNoteDetailsModel cdNoteDetails = new AcctCDNoteDetailsModel();
            var cdNote = new AcctCdnote();

            var cdNoList = new List<string>();
            if (string.IsNullOrEmpty(cdNo))
            {               
                foreach (var item in acctCdNoteList)
                {
                    cdNoList.Add(item.Code);
                }
                cdNote = DataContext.Where(x => x.Code == cdNoList.FirstOrDefault()).FirstOrDefault();
            }
            else
            {
                cdNote = DataContext.Where(x => x.Code == cdNo).FirstOrDefault();
            }
            if (cdNote == null) return cdNoteDetails;
            var partner = partnerRepositoty.Get(x => x.Id == cdNote.PartnerId).FirstOrDefault();

            CatPlace pol = new CatPlace();
            CatPlace pod = new CatPlace();

            var transaction = cstransRepository.Get(x => x.Id == JobId).FirstOrDefault();
            var opsTransaction = opstransRepository.Get(x => x.Id == JobId).FirstOrDefault();
            if (transaction != null)
            {
                pol = places.FirstOrDefault(x => x.Id == transaction.Pol);
                pod = places.FirstOrDefault(x => x.Id == transaction.Pod);
            }
            else
            {
                if (opsTransaction != null)
                {
                    pol = places.FirstOrDefault(x => x.Id == opsTransaction.Pol);
                    pod = places.FirstOrDefault(x => x.Id == opsTransaction.Pod);
                }
            }
            if ((transaction == null && opsTransaction == null) || cdNote == null || partner == null)
            {
                return null;
            }

            var charges = new List<CsShipmentSurcharge>();
            if (string.IsNullOrEmpty(cdNo))
            {
                foreach (var code in cdNoList)
                {
                    var surcharge = surchargeRepository.Get(x => x.CreditNo == code || x.DebitNo == code);
                    if(surcharge != null)
                    {
                        charges.AddRange(surcharge);
                    }
                }
            }
            else
            {
                charges = surchargeRepository.Get(x => x.CreditNo == cdNo || x.DebitNo == cdNo).ToList();
            }

            List<CsTransactionDetail> HBList = new List<CsTransactionDetail>();
            List<CsShipmentSurchargeDetailsModel> listSurcharges = new List<CsShipmentSurchargeDetailsModel>();

            cdNoteDetails.CreatedDate = string.IsNullOrEmpty(cdNo) ? ((DateTime)acctCdNoteList.OrderBy(x => x.DatetimeCreated).FirstOrDefault().DatetimeCreated).ToString("dd'/'MM'/'yyyy")
                                                                    : ((DateTime)cdNote.DatetimeCreated).ToString("dd'/'MM'/'yyyy");
            foreach (var item in charges)
            {
                var charge = mapper.Map<CsShipmentSurchargeDetailsModel>(item);
                var hb = trandetailRepositoty.Get(x => x.Id == item.Hblid).FirstOrDefault();
                var catCharge = catchargeRepository.Get(x => x.Id == charge.ChargeId).FirstOrDefault();

                //Quy đổi theo Final Exchange Rate. Nếu Final Exchange Rate is null thì
                //Check ExchangeDate # null: nếu bằng null thì gán ngày hiện tại.
                var exchargeDateSurcharge = item.ExchangeDate == null ? DateTime.Now : item.ExchangeDate;
                decimal _exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, exchargeDateSurcharge, item.CurrencyId, DocumentConstants.CURRENCY_LOCAL);
                charge.Currency = currencyRepository.Get(x => x.Id == charge.CurrencyId).FirstOrDefault()?.CurrencyName;
                charge.ExchangeRate = _exchangeRate;
                charge.Hwbno = hb != null ? hb.Hwbno : opsTransaction?.Hwbno;
                var unit = unitRepository.Get(x => x.Id == charge.UnitId).FirstOrDefault();
                charge.Unit = unit?.UnitNameEn;
                charge.UnitCode = unit?.Code;
                charge.ChargeCode = catCharge?.Code;
                charge.NameEn = catCharge?.ChargeNameEn;

                string _syncedFromBy = null;
                if (charge.Type == DocumentConstants.CHARGE_OBH_TYPE && charge.CreditNo == cdNote.Code)
                {
                    charge.IsSynced = !string.IsNullOrEmpty(charge.PaySyncedFrom) && (charge.PaySyncedFrom.Equals("CDNOTE") || charge.PaySyncedFrom.Equals("SOA") || charge.PaySyncedFrom.Equals("VOUCHER"));

                    if (charge.PaySyncedFrom == "SOA")
                    {
                        _syncedFromBy = charge.PaySoano;
                    }
                    if (charge.PaySyncedFrom == "CDNOTE")
                    {
                        _syncedFromBy = charge.CreditNo;
                    }
                    if (charge.PaySyncedFrom == "VOUCHER")
                    {
                        _syncedFromBy = charge.VoucherId;
                    }
                }
                else
                {
                    charge.IsSynced = !string.IsNullOrEmpty(charge.SyncedFrom) && (charge.SyncedFrom.Equals("CDNOTE") || charge.SyncedFrom.Equals("SOA") || charge.SyncedFrom.Equals("VOUCHER"));

                    if (charge.SyncedFrom == "SOA")
                    {
                        _syncedFromBy = (charge.Type == DocumentConstants.CHARGE_BUY_TYPE) ? charge.PaySoano : charge.Soano;
                    }
                    if (charge.SyncedFrom == "CDNOTE")
                    {
                        _syncedFromBy = (charge.Type == DocumentConstants.CHARGE_BUY_TYPE) ? charge.CreditNo : charge.DebitNo;
                    }
                    if (charge.SyncedFrom == "VOUCHER")
                    {
                        _syncedFromBy = charge.VoucherId;
                    }
                }
                charge.SyncedFromBy = _syncedFromBy;

                listSurcharges.Add(charge);
                if (hb != null)
                {
                    HBList.Add(hb);
                }

                HBList = HBList.Distinct().ToList().OrderByDescending(x => x?.Hwbno).ToList();
            }
            var hbOfLadingNo = string.Empty;
            var mbOfLadingNo = string.Empty;

            if (transaction != null)
            {
                cdNoteDetails.MbLadingNo = transaction?.Mawb;
                hbOfLadingNo = string.Join(", ", HBList.Where(x => !string.IsNullOrEmpty(x.Hwbno)).Select(x => x.Hwbno).Distinct());
                cdNoteDetails.HbLadingNo = hbOfLadingNo;
            }
            else
            {
                cdNoteDetails.CBM = opsTransaction.SumCbm;
                cdNoteDetails.GW = opsTransaction.SumGrossWeight;
                cdNoteDetails.ServiceDate = opsTransaction.ServiceDate;
                cdNoteDetails.HbLadingNo = opsTransaction?.Hwbno;
                cdNoteDetails.MbLadingNo = opsTransaction?.Mblno;
                cdNoteDetails.SumContainers = opsTransaction?.SumContainers;
                cdNoteDetails.SumPackages = opsTransaction?.SumPackages;
                cdNoteDetails.NW = opsTransaction?.SumNetWeight;
                cdNoteDetails.PackageUnit = string.Concat((opsTransaction.SumPackages + _decimalNumber)?.ToString("0.000"), " "
                , opsTransaction?.PackageTypeId == null ? string.Empty : unitRepository.Get(x => x.Id == opsTransaction.PackageTypeId).FirstOrDefault().UnitNameEn);
                cdNoteDetails.CommodityGroupId = opsTransaction?.CommodityGroupId;
            }
            var hbConstainers = string.Empty;
            var hbPackages = string.Empty;
            var sealsContsNo = string.Empty;
            decimal? volum = 0;
            decimal? hbGw = 0;
            decimal? hbCw = 0; //House Bill Charge Weight
            var hbShippers = string.Empty;
            var hbConsignees = string.Empty;
            foreach (var item in HBList)
            {
                var conts = csMawbcontainerRepository.Get(x => x.Hblid == item.Id).ToList();
                foreach (var cont in conts)
                {
                    var contUnit = unitRepository.Get(x => x.Id == cont.ContainerTypeId).FirstOrDefault();
                    if (contUnit != null)
                    {
                        hbConstainers += (cont.Quantity + "x" + contUnit.UnitNameEn + ", ");
                    }

                    //Đối với service là Sea FCL (Import & Export)
                    if (transaction != null && transaction.TransactionType.Contains("SF"))
                    {
                        var packageUnit = unitRepository.Get(x => x.Id == cont.PackageTypeId).FirstOrDefault();
                        if (packageUnit != null)
                        {
                            hbPackages += (cont.Quantity + "x" + contUnit.UnitNameEn + " (" + cont.PackageQuantity + " " + packageUnit.UnitNameEn + "); ");
                        }
                    }
                    sealsContsNo += (!string.IsNullOrEmpty(cont.ContainerNo) || !string.IsNullOrEmpty(cont.SealNo)) ? (cont.ContainerNo + "/" + cont.SealNo + ", ") : "";
                }

                // Đối với các Service là OPS hoặc các service khác Sea FCL, Air thì lấy package qty & package type theo Housebill
                if ((transaction != null && !transaction.TransactionType.Contains("SF") && !transaction.TransactionType.Contains("A")) || opsTransaction != null)
                {
                    hbPackages += item.PackageQty + " " + unitRepository.Get(x => x.Id == item.PackageType).FirstOrDefault()?.UnitNameEn + "; ";
                }

                if (conts.Count() > 0)
                {
                    volum += conts.Sum(s => (s.Cbm ?? 0));
                    hbGw += conts.Sum(s => (s.Gw ?? 0));
                    hbCw += conts.Sum(s => (s.ChargeAbleWeight ?? 0));
                }
                else
                {
                    volum += (item.Cbm ?? 0);
                    hbGw += (item.GrossWeight ?? 0);
                    hbCw += (item.ChargeWeight ?? 0);
                }
            }

            //Đối với hàng Air (Import & Export) thì sẽ sum PackageQty của các Housebill
            if (transaction != null && (transaction.TransactionType.Equals("AI") || transaction.TransactionType.Equals("AE")))
            {
                hbPackages = HBList.Select(s => s.PackageQty).Sum()?.ToString();
            }

            hbConstainers += ".";
            hbConstainers = hbConstainers != "." ? hbConstainers.Replace(", .", "") : string.Empty;
            hbPackages += ".";
            hbPackages = hbPackages != "." ? hbPackages.Replace("; .", "") : string.Empty;
            sealsContsNo += ".";
            sealsContsNo = sealsContsNo != "." ? sealsContsNo.Replace(", .", "") : string.Empty;

            hbShippers = string.Join(", ", partnerRepositoty.Get(x => HBList.Select(s => s.ShipperId).Contains(x.Id)).Select(s => s.PartnerNameEn).Distinct().ToList());
            hbConsignees = string.Join(", ", partnerRepositoty.Get(x => HBList.Select(s => s.ConsigneeId).Contains(x.Id)).Select(s => s.PartnerNameEn).Distinct().ToList());

            var countries = countryRepository.Get().ToList();
            cdNoteDetails.PartnerNameEn = partner?.PartnerNameEn;
            cdNoteDetails.PartnerShippingAddress = partner?.AddressEn; //Billing Address Name En
            cdNoteDetails.PartnerTel = partner?.Tel;
            cdNoteDetails.PartnerTaxcode = partner?.TaxCode;
            cdNoteDetails.PartnerId = partner?.Id;
            cdNoteDetails.PartnerFax = partner?.Fax;
            cdNoteDetails.PartnerPersonalContact = partner.ContactPerson;
            cdNoteDetails.CreditPayment = partner.CreditPayment;
            cdNoteDetails.JobId = transaction != null ? transaction.Id : opsTransaction.Id;
            cdNoteDetails.JobNo = transaction != null ? transaction.JobNo : opsTransaction?.JobNo;
            cdNoteDetails.Pol = pol?.NameEn;
            if (cdNoteDetails.Pol != null)
            {
                cdNoteDetails.PolCountry = pol == null ? null : countries.FirstOrDefault(x => x.Id == pol.CountryId)?.NameEn;
            }
            cdNoteDetails.Pod = pod?.NameEn;
            if (cdNoteDetails.Pod != null)
            {
                cdNoteDetails.PodCountry = pod == null ? null : countries.FirstOrDefault(x => x.Id == pod.CountryId)?.NameEn;
            }
            cdNoteDetails.Vessel = transaction != null ? transaction.FlightVesselName : opsTransaction.FlightVessel;
            cdNoteDetails.VesselDate = transaction != null ? transaction.FlightDate : null;
            cdNoteDetails.HbConstainers = transaction != null ? hbConstainers : opsTransaction.ContainerDescription; //Container Quantity
            cdNoteDetails.HbPackages = transaction != null ? hbPackages : opsTransaction.SumPackages.ToString(); // Package Quantity
            cdNoteDetails.Etd = transaction != null ? transaction.Etd : opsTransaction.ServiceDate;
            cdNoteDetails.Eta = transaction != null ? transaction.Eta : opsTransaction.FinishDate;
            cdNoteDetails.IsLocked = false;
            cdNoteDetails.Volum = volum;
            cdNoteDetails.ListSurcharges = listSurcharges;
            cdNoteDetails.CDNote = cdNote;
            if (opsTransaction != null)
            {
                cdNoteDetails.CDNote.InvoiceNo = opsTransaction.InvoiceNo;
            }
            if (string.IsNullOrEmpty(cdNo))
            {
                cdNoteDetails.CDNote.Code = string.Join(';', cdNoList);
            }
            cdNoteDetails.ProductService = opsTransaction?.ProductService;
            cdNoteDetails.ServiceMode = opsTransaction?.ServiceMode;
            cdNoteDetails.SoaNo = string.Join(", ", charges.Where(x => !string.IsNullOrEmpty(x.Soano) || !string.IsNullOrEmpty(x.PaySoano)).Select(x => !string.IsNullOrEmpty(x.Soano) ? x.Soano : x.PaySoano).Distinct()); ;
            cdNoteDetails.HbSealNo = sealsContsNo;//SealNo/ContNo
            cdNoteDetails.HbGrossweight = hbGw;
            cdNoteDetails.HbShippers = hbShippers; //Shipper
            cdNoteDetails.HbConsignees = hbConsignees; //Consignee
            cdNoteDetails.HbChargeWeight = hbCw;
            cdNoteDetails.FlexId = cdNote.FlexId;
            cdNoteDetails.Status = cdNote.Status;
            cdNoteDetails.SyncStatus = cdNote.SyncStatus;
            cdNoteDetails.LastSyncDate = cdNote.LastSyncDate;
            cdNoteDetails.Currency = cdNote.CurrencyId;
            cdNoteDetails.ExchangeRate = cdNote.ExchangeRate;
            cdNoteDetails.Note = cdNote.Note;
            cdNoteDetails.ReasonReject = cdNote.ReasonReject;
            cdNoteDetails.IsExistChgCurrDiffLocalCurr = cdNote.CurrencyId != DocumentConstants.CURRENCY_LOCAL || listSurcharges.Any(x => x.CurrencyId != DocumentConstants.CURRENCY_LOCAL);
            cdNoteDetails.DatetimeCreated = cdNote.DatetimeCreated;
            cdNoteDetails.ExcRateUsdToLocal = cdNote.ExcRateUsdToLocal;
            return cdNoteDetails;
        }

        public HandleState DeleteCDNote(Guid idSoA)
        {
            var hs = new HandleState();
            try
            {
                var cdNote = DataContext.Where(x => x.Id == idSoA).FirstOrDefault();
                if (cdNote == null)
                {
                    hs = new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_CDNOTE_NOT_ALLOW_DELETED_NOT_FOUND]);
                }
                else
                {
                    if (cdNote.SyncStatus == "Synced") return new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_CDNOTE_NOT_ALLOW_DELETED_HAD_SYNCED]);
                    var charges = surchargeRepository.Get(x => x.CreditNo == cdNote.Code || x.DebitNo == cdNote.Code);
                    var isOtherSOA = charges.Where(x => !string.IsNullOrEmpty(x.Soano) || !string.IsNullOrEmpty(x.PaySoano)).Any();
                    if (isOtherSOA == true)
                    {
                        hs = new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_CDNOTE_NOT_ALLOW_DELETED_HAD_SOA]);
                    }
                    else
                    {
                        var _hs = DataContext.Delete(x => x.Id == idSoA, false);
                        if (hs.Success)
                        {
                            foreach (var item in charges)
                            {
                                if (item.Type == DocumentConstants.CHARGE_BUY_TYPE)
                                {
                                    item.CreditNo = null;
                                }
                                else if (item.Type == DocumentConstants.CHARGE_SELL_TYPE)
                                {
                                    item.DebitNo = null;
                                }
                                else
                                {
                                    if (item.DebitNo == cdNote.Code)
                                    {
                                        item.DebitNo = null;
                                    }
                                    if (item.CreditNo == cdNote.Code)
                                    {
                                        item.CreditNo = null;
                                    }
                                }
                                item.UserModified = cdNote.UserModified;
                                item.DatetimeModified = DateTime.Now;
                                surchargeRepository.Update(item, x => x.Id == item.Id, false);
                            }
                            DataContext.SubmitChanges();
                            var jobOpsTrans = opstransRepository.Get(x => x.Id == cdNote.JobId).FirstOrDefault();
                            if (jobOpsTrans != null)
                            {
                                jobOpsTrans.UserModified = currentUser.UserID;
                                jobOpsTrans.DatetimeModified = DateTime.Now;
                                opstransRepository.Update(jobOpsTrans, x => x.Id == jobOpsTrans.Id, false);
                            }
                            var jobCSTrans = cstransRepository.Get(x => x.Id == cdNote.JobId).FirstOrDefault();
                            if (jobCSTrans != null)
                            {
                                jobCSTrans.UserModified = currentUser.UserID;
                                jobCSTrans.DatetimeModified = DateTime.Now;
                                cstransRepository.Update(jobCSTrans, x => x.Id == jobCSTrans.Id, false);
                            }
                        }
                        DataContext.SubmitChanges();
                        surchargeRepository.SubmitChanges();
                        opstransRepository.SubmitChanges();
                        cstransRepository.SubmitChanges();
                    }
                }

            }
            catch (Exception ex)
            {
                hs = new HandleState((object)ex.Message);
            }
            return hs;
        }

        /// <summary>
        /// Get Data To Preview CDNote Combine
        /// </summary>
        /// <param name="acctCdNoteList">AcctCdnoteModel List</param>
        /// <param name="isOrigin"></param>
        /// <returns></returns>
        public AcctCDNoteDetailsModel GetDataPreviewCDNotes(List<AcctCdnoteModel> acctCdNoteList)
        {
            AcctCDNoteDetailsModel model = new AcctCDNoteDetailsModel();
            var firstAcctCDNote = acctCdNoteList.FirstOrDefault();
            var cdNoteDetail = DataContext.Get(x => x.Id == firstAcctCDNote.Id);
            model.CDNote = mapper.Map<AcctCdnote>(firstAcctCDNote);
            model.CDNote.Code = string.Join(";", acctCdNoteList.Select(x => x.Code));
            var opsTransaction = opstransRepository.Get(x => x.Id == firstAcctCDNote.JobId).FirstOrDefault();
            if (opsTransaction == null)
            {
                return null;
            }
            var partner = partnerRepositoty.Get(x => x.Id == firstAcctCDNote.PartnerId).FirstOrDefault();
            model.JobNo = opsTransaction.JobNo;
            model.CBM = opsTransaction.SumCbm;
            model.GW = opsTransaction.SumGrossWeight;
            model.NW = opsTransaction.SumNetWeight;
            model.ServiceDate = opsTransaction.ServiceDate;
            model.HbLadingNo = opsTransaction?.Hwbno;
            model.MbLadingNo = opsTransaction?.Mblno;
            model.SumContainers = opsTransaction?.SumContainers;
            model.SumPackages = opsTransaction?.SumPackages;
            model.ServiceMode = opsTransaction?.ServiceMode;
            model.CommodityGroupId = opsTransaction?.CommodityGroupId;
            model.HbConstainers = opsTransaction.ContainerDescription;
            model.PartnerId = partner?.Id;
            model.PartnerNameEn = partner?.PartnerNameEn;
            model.PartnerPersonalContact = partner?.ContactPerson;
            model.PartnerShippingAddress = partner?.AddressEn; //Billing Address Name En
            model.PartnerTel = partner?.Tel;
            model.PartnerTaxcode = partner?.TaxCode;
            model.PartnerFax = partner?.Fax;
            model.CreatedDate = ((DateTime)acctCdNoteList.OrderBy(x => x.DatetimeCreated).FirstOrDefault().DatetimeCreated).ToString("dd'/'MM'/'yyyy");

            var places = placeRepository.Get();
            var pol = places.FirstOrDefault(x => x.Id == opsTransaction.Pol);
            var pod = places.FirstOrDefault(x => x.Id == opsTransaction.Pod);
            model.Pol = pol?.NameEn;
            if (model.Pol != null)
            {
                model.PolCountry = pol == null ? null : countryRepository.Get().FirstOrDefault(x => x.Id == pol.CountryId)?.NameEn;
            }
            model.PolName = pol?.NameEn;
            model.Pod = pod?.NameEn;
            if (model.Pod != null)
            {
                model.PodCountry = pod == null ? null : countryRepository.Get().FirstOrDefault(x => x.Id == pod.CountryId)?.NameEn;
            }
            model.PodName = pod?.NameEn;

            List<CsShipmentSurchargeDetailsModel> listSurcharges = new List<CsShipmentSurchargeDetailsModel>();
            foreach (var cdNote in acctCdNoteList)
            {
                var charges = surchargeRepository.Get(x => x.CreditNo == cdNote.Code || x.DebitNo == cdNote.Code).ToList();
                foreach (var item in charges)
                {
                    var charge = mapper.Map<CsShipmentSurchargeDetailsModel>(item);
                    var catCharge = catchargeRepository.Get(x => x.Id == charge.ChargeId).FirstOrDefault();
                    charge.Currency = currencyRepository.Get(x => x.Id == charge.CurrencyId).FirstOrDefault()?.CurrencyName;
                    charge.ChargeCode = catCharge?.Code;
                    charge.NameEn = catCharge?.ChargeNameEn;
                    listSurcharges.Add(charge);
                }
            }
            if (listSurcharges.Count() > 0)
            {
                listSurcharges = listSurcharges.OrderBy(x => (firstAcctCDNote.Type == "DEBIT" ? x.DebitNo : x.CreditNo)).ThenBy(x => x.NameEn).ToList();
            }
            model.ListSurcharges = listSurcharges;
            return model;
        }

        public Crystal Preview(AcctCDNoteDetailsModel model, bool isOrigin)
        {
            if (model == null)
            {
                return null;
            }
            Crystal result = null;
            // Thông tin Company của Creator
            var companyOfUser = sysCompanyRepository.Get(x => x.Id == model.CDNote.CompanyId).FirstOrDefault();
            //Lấy thông tin Office của Creator
            var officeOfUser = GetInfoOfficeOfUser(model.CDNote.OfficeId);
            var _accountName = officeOfUser?.BankAccountNameVn ?? string.Empty;
            var _accountNameEN = officeOfUser?.BankAccountNameEn ?? string.Empty;
            var _bankName = officeOfUser?.BankNameLocal ?? string.Empty;
            var _bankNameEN = officeOfUser?.BankNameEn ?? string.Empty;
            var _bankAddress = officeOfUser?.BankAddressLocal ?? string.Empty;
            var _bankAddressEN = officeOfUser?.BankAddressEn ?? string.Empty;
            var _swiftAccs = officeOfUser?.SwiftCode ?? string.Empty;
            var _accsUsd = officeOfUser?.BankAccountUsd ?? string.Empty;
            var _accsVnd = officeOfUser?.BankAccountVnd ?? string.Empty;
            var commodity = model.CommodityGroupId == null ? "N/A" : catCommodityGroupRepository.Get(x => x.Id == model.CommodityGroupId).Select(x => x.GroupNameEn).FirstOrDefault();

            IQueryable<CustomsDeclaration> _customClearances = customsDeclarationRepository.Get(x => x.JobNo == model.JobNo);
            CustomsDeclaration _clearance = null;
            if (_customClearances.Count() > 0 || _customClearances != null)
            {
                var orderClearance = _customClearances.OrderBy(x => x.ClearanceDate);
                _clearance = orderClearance.FirstOrDefault();

            }

            string cdNoteType = !string.IsNullOrEmpty(model.CDNote.Type) && model.CDNote.Type != "INVOICE" ? model.CDNote.Type + " NOTE" : (model.CDNote.Type ?? string.Empty);

            var parameter = new AcctSOAReportParams
            {
                DBTitle = cdNoteType,
                DebitNo = model.CDNote.Code,
                TotalDebit = model.TotalDebit == null ? string.Empty : model.TotalDebit.ToString(),
                TotalCredit = model.TotalCredit == null ? string.Empty : model.TotalCredit.ToString(),
                DueToTitle = "N/A",
                DueTo = "N/A",
                DueToCredit = "N/A",
                SayWordAll = "N/A",
                CompanyName = companyOfUser?.BunameEn?.ToUpper(),
                CompanyAddress1 = officeOfUser?.AddressEn,
                CompanyAddress2 = "Tel: " + officeOfUser?.Tel + "\nFax: " + officeOfUser?.Fax,
                CompanyDescription = "N/A",
                Website = companyOfUser?.Website,
                IbanCode = "N/A",
                AccountName = _accountName,
                AccountNameEN = _accountNameEN,
                BankName = _bankName,
                BankNameEN = _bankNameEN,
                SwiftAccs = _swiftAccs,
                AccsUSD = _accsUsd,
                AccsVND = _accsVnd,
                BankAddress = _bankAddress,
                BankAddressEN = _bankAddressEN,
                Paymentterms = "N/A",
                DecimalNo = 2,
                CurrDecimal = 2,
                IssueInv = "N/A",
                InvoiceInfo = "N/A",
                Contact = currentUser.UserName,
                IssuedDate = model.CreatedDate,
                OtherRef = "N/A",
                IsOrigin = isOrigin
            };
            string trans = string.Empty;
            string port = string.Empty;
            if (model.ServiceMode == "Export")
            {
                trans = "X";
                port = model.Pol;
            }
            else
            {
                port = model.Pod;
            }
            var listSOA = new List<AcctSOAReport>();
            if (model.ListSurcharges.Count > 0)
            {
                foreach (var item in model.ListSurcharges)
                {
                    string subject = string.Empty;
                    if (item.Type == "OBH")
                    {
                        subject = "ON BEHALF";
                    }

                    decimal? _vatAmount = 0, _vatAmountUsd = 0, exchangeRateToUsd = 0, exchangeRateToVnd = 0;
                    // Get exchange rate
                    if (!isOrigin)
                    {
                        exchangeRateToUsd = currencyExchangeService.CurrencyExchangeRateConvert(null, model.CDNote.DatetimeCreated, item.CurrencyId, DocumentConstants.CURRENCY_USD);
                        exchangeRateToVnd = currencyExchangeService.CurrencyExchangeRateConvert(null, model.CDNote.DatetimeCreated, item.CurrencyId, DocumentConstants.CURRENCY_LOCAL);
                    }

                    // Get Vat amount
                    if (isOrigin)
                    {
                        if (item.CurrencyId != DocumentConstants.CURRENCY_LOCAL && item.CurrencyId != DocumentConstants.CURRENCY_USD && isOrigin)
                        {
                            decimal _exchangeRateToUsd = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.CurrencyId, DocumentConstants.CURRENCY_USD);
                            //Quy đổi về USD đối với các currency khác
                            _vatAmount = item.Vatrate != null && item.Vatrate < 0 ? Math.Abs(item.Vatrate.Value) : (((item.UnitPrice * item.Quantity) * item.Vatrate) / 100) * _exchangeRateToUsd;
                        }
                        else
                        {
                            _vatAmount = item.Vatrate != null && item.Vatrate < 0 ? Math.Abs(item.Vatrate.Value) : ((item.UnitPrice * item.Quantity) * item.Vatrate) / 100;
                        }
                    }
                    else
                    {
                        decimal? _vatRate = item.Vatrate != null && item.Vatrate < 0 ? Math.Abs(item.Vatrate.Value) : ((item.UnitPrice * item.Quantity) * item.Vatrate) / 100;
                        _vatAmount = _vatRate * exchangeRateToVnd; // To VND
                        _vatAmountUsd = _vatRate * exchangeRateToUsd; // To USD
                    }

                    var acctCDNo = new AcctSOAReport
                    {
                        SortIndex = null,
                        Subject = subject,
                        PartnerID = model.PartnerId,
                        PartnerName = model.PartnerNameEn?.ToUpper(),
                        PersonalContact = model.PartnerPersonalContact?.ToUpper(),
                        Address = model.PartnerShippingAddress?.ToUpper(),
                        Taxcode = model.PartnerTaxcode?.ToUpper(),
                        Workphone = model.PartnerTel?.ToUpper(),
                        Fax = model.PartnerFax?.ToUpper(),
                        TransID = trans,
                        LoadingDate = null,
                        Commodity = commodity,
                        PortofLading = model.PolName?.ToUpper(),
                        PortofUnlading = model.PodName?.ToUpper(),
                        MAWB = model.MbLadingNo,
                        Invoice = model.CDNote.InvoiceNo,
                        EstimatedVessel = "N/A",
                        ArrivalDate = null,
                        Noofpieces = null,
                        Delivery = null,
                        HWBNO = model.HbLadingNo,
                        Description = item.NameEn,
                        Quantity = item.Quantity + _decimalNumber,
                        QUnit = "N/A",
                        UnitPrice = (item.UnitPrice ?? 0) + _decimalNumber, //Cộng thêm phần thập phân
                        VAT = (_vatAmount ?? 0) + _decimalNumber, //Cộng thêm phần thập phân
                        Debit = model.TotalDebit + _decimalNumber, //Cộng thêm phần thập phân
                        Credit = model.TotalCredit + _decimalNumber, //Cộng thêm phần thập phân
                        Notes = item.Notes,
                        InputData = "N/A",
                        PONo = string.Empty,
                        TransNotes = "N/A",
                        Shipper = model.PartnerNameEn?.ToUpper(),
                        Consignee = model.PartnerNameEn?.ToUpper(),
                        ContQty = model.HbConstainers,
                        ContSealNo = "N/A",
                        Deposit = null,
                        DepositCurr = "N/A",
                        DecimalSymbol = "N/A",
                        DigitSymbol = "N/A",
                        DecimalNo = null,
                        CurrDecimalNo = null,
                        VATInvoiceNo = item.InvoiceNo,
                        GW = model.GW,
                        NW = model.NW,
                        SeaCBM = model.CBM,
                        SOTK = _clearance?.ClearanceNo,
                        NgayDK = null,
                        Cuakhau = port,
                        DeliveryPlace = model.WarehouseName?.ToUpper(),
                        TransDate = null,
                        Unit = item.CurrencyId,
                        UnitPieaces = "N/A",
                        CustomDate = _clearance?.ClearanceDate,
                        JobNo = model.JobNo,
                        ExchangeRateToUsd = exchangeRateToUsd,
                        ExchangeRateToVnd = exchangeRateToVnd,
                        ExchangeVATToUsd = (_vatAmountUsd ?? 0) + _decimalNumber
                    };
                    listSOA.Add(acctCDNo);
                }
            }
            else
            {
                return null;
            }

            result = new Crystal
            {
                ReportName = "LogisticsDebitNewDNTT.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listSOA);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;

        }

        /// <summary>
        /// Preview CD Note with Local or USD Currency
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public Crystal PreviewOPSCDNoteWithCurrency(PreviewCdNoteCriteria criteria)
        {
            Crystal result = null;
            var _currentUser = currentUser.UserName;
            var listCharge = new List<LogisticCDNoteReport>();
            var data = GetCDNoteDetails(criteria.JobId, criteria.CreditDebitNo);

            if (data != null)
            {
                int i = 1;
                foreach (var item in data.ListSurcharges)
                {
                    var exchargeDateSurcharge = item.ExchangeDate == null ? DateTime.Now : item.ExchangeDate;
                    //Exchange Rate theo Currency truyền vào
                    decimal _exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.CurrencyId, criteria.Currency);
                    var charge = new LogisticCDNoteReport();

                    //Thông tin Partner
                    charge.PartnerID = data.PartnerId?.ToUpper();
                    charge.PartnerName = data.PartnerNameEn?.ToUpper();
                    charge.Address = data.PartnerShippingAddress?.ToUpper();
                    charge.Workphone = data.PartnerTel?.ToUpper();
                    charge.Fax = data.PartnerFax?.ToUpper();
                    charge.Taxcode = data.PartnerTaxcode?.ToUpper();
                    charge.PersonalContact = data.PartnerPersonalContact?.ToUpper();

                    //Thông tin Shipment
                    charge.TransID = data.JobNo?.ToUpper();
                    charge.PortofLading = data.Pol;
                    charge.PortofUnlading = data.Pod;

                    charge.LoadingDate = data.Etd;//ETD
                    charge.ETA = data?.Eta; //ETA

                    charge.ATTN = string.Empty; //NOT USE
                    charge.LocalVessel = data.Vessel?.ToUpper();//Flight No
                    charge.MAWB = data.MbLadingNo?.ToUpper(); //MBLNO
                    charge.Consignee = data.HbConsignees?.ToUpper();//Consignee -- lấy từ Housebill
                    charge.GrossWeight = data.HbGrossweight ?? 0;//Total GW of HBL
                    charge.CBM = data.Volum ?? 0; //Total CBM of HBL
                    charge.HWBNO = data.HbLadingNo?.ToUpper(); //HBLNOs

                    //Thông tin list charges
                    charge.Subject = (item.Type == DocumentConstants.CHARGE_OBH_TYPE) ? "I. Chi hộ/Pay on behalf of customer" : "II. Phí dịch vụ & vận chuyển/Logistics service";
                    charge.Description = item.NameEn;//Charge name
                    charge.Quantity = item.Quantity + _decimalNumber; //Cộng thêm phần thập phân
                    charge.Unit = item.UnitCode;
                    charge.QUnit = criteria.Currency;

                    var _unitPrice = (item.UnitPrice ?? 0) * _exchangeRate; //Unit Price đã được Exchange Rate theo Currency và không làm tròn
                    charge.UnitPrice = _unitPrice + _decimalNumber; //cộng thêm phần thập phân
                    //Giá trị thực tế VAT (% VAT hoặc số tiền tuyệt đối)
                    charge.VAT = Math.Abs(item.Vatrate ?? 0) + _decimalNumber; //Cộng thêm phần thập phân

                    //Amount trước thuế
                    decimal _netAmount = _unitPrice * item.Quantity;
                    //Tiền thuế (nếu có)
                    decimal _taxMoney = 0;

                    _netAmount = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_netAmount, 0) : NumberHelper.RoundNumber(_netAmount, 2); //Làm tròn NetAmount
                    _taxMoney = (item.Vatrate != null) ? (item.Vatrate < 101 & item.Vatrate >= 0) ? ((item.Vatrate ?? 0) * _netAmount / 100) : Math.Abs(item.Vatrate * _exchangeRate ?? 0) : 0;
                    _taxMoney = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_taxMoney, 0) : NumberHelper.RoundNumber(_taxMoney, 2); //Làm tròn tiền thuế

                    var _totalAmount = _netAmount + _taxMoney; //Total Amount = Amount trước thuế + Tiền thuế

                    var _credit = (item.Type == DocumentConstants.CHARGE_BUY_TYPE || (item.Type == DocumentConstants.CHARGE_OBH_TYPE && data.PartnerId == item.PayerId)) ? _totalAmount : 0;
                    var _debit = (item.Type == DocumentConstants.CHARGE_SELL_TYPE || (item.Type == DocumentConstants.CHARGE_OBH_TYPE && data.PartnerId == item.PaymentObjectId)) ? _totalAmount : 0;

                    charge.Credit = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_credit, 0) : NumberHelper.RoundNumber(_credit, 2); //Làm tròn Amount Credit
                    charge.Debit = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_debit, 0) : NumberHelper.RoundNumber(_debit, 2); //Làm tròn Amount Debit
                    charge.Credit = charge.Credit + _decimalNumber; //Cộng thêm phần thập phân
                    charge.Debit = charge.Debit + _decimalNumber; //Cộng thêm phần thập phân

                    charge.ExtVND = 0; //NOT USE
                    charge.Notes = string.IsNullOrEmpty(item.Notes) ? "" : "(" + item.Notes + ")";

                    charge.InputData = string.Empty; //Chưa biết
                    charge.Deposit = 0; //NOT USE
                    charge.DepositCurr = string.Empty; //NOT USE
                    charge.Commodity = data.CommodityGroupId == null ? "N/A" : catCommodityGroupRepository.Get(x => x.Id == data.CommodityGroupId).Select(x => x.GroupNameEn).FirstOrDefault();
                    charge.DecimalSymbol = ",";
                    charge.DigitSymbol = ",";
                    charge.DecimalNo = 0; //NOT USE
                    charge.CurrDecimalNo = 0; //NOT USE
                    listCharge.Add(charge);
                }
            }
            var parameter = new LogisticCDNoteReportParams();
            parameter.DebitNo = criteria.CreditDebitNo;
            parameter.IssuedDate = data != null && data.CDNote != null && data.CDNote.DatetimeCreated != null ? data.CDNote.DatetimeCreated.Value.ToString("dd MMM, yyyy") : string.Empty;//Lấy ngày tạo CDNote
            parameter.DBTitle = data.CDNote.Type == "CREDIT" ? "CREDIT NOTE" : data.CDNote.Type == "DEBIT" ? "DEBIT NOTE" : "INVOICE";
            parameter.ReviseNotice = DateTime.Now.ToString("dd/MM/yyyy"); //Bỏ chữ Revised

            var _inword = string.Empty;
            // Preview with USD/VND
            var _totalDebit = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(listCharge.Sum(x => x.Debit).Value, 0) : NumberHelper.RoundNumber(listCharge.Sum(x => x.Debit).Value, 2);
            var _totalCredit = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(listCharge.Sum(x => x.Credit).Value, 0) : NumberHelper.RoundNumber(listCharge.Sum(x => x.Credit).Value, 2);
            parameter.TotalDebit = string.Empty;
            if (_totalDebit != 0)
            {
                parameter.TotalDebit = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? String.Format("{0:n0}", _totalDebit) : String.Format("{0:n2}", _totalDebit);
            }

            parameter.TotalCredit = string.Empty;
            if (_totalCredit != 0)
            {
                parameter.TotalCredit = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? String.Format("{0:n0}", _totalCredit) : String.Format("{0:n2}", _totalCredit);
            }

            decimal _balanceAmount = Math.Abs(_totalDebit - _totalCredit);
            parameter.BalanceAmount = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? String.Format("{0:n0}", _balanceAmount) : String.Format("{0:n2}", _balanceAmount);

            //Chuyển tiền Amount thành chữ
            _balanceAmount = NumberHelper.RoundNumber(_balanceAmount, 2);

            var _currency = criteria.Currency == DocumentConstants.CURRENCY_LOCAL && _balanceAmount >= 1 ?
                       (_balanceAmount % 1 > 0 ? "đồng lẻ" : "đồng chẵn")
                    :
                    "U.S. dollar(s)";

            _inword = criteria.Currency == DocumentConstants.CURRENCY_LOCAL && _balanceAmount >= 1 ?
                    InWordCurrency.ConvertNumberCurrencyToString(_balanceAmount, _currency)
                :
                    InWordCurrency.ConvertNumberCurrencyToStringUSD(_balanceAmount, _currency);

            parameter.InwordVND = !string.IsNullOrEmpty(_inword) ? _inword.ToUpper() : string.Empty;
            parameter.IssueInv = string.Empty; //Tạm thời để trống
            parameter.InvoiceInfo = data.CDNote.InvoiceNo == null ? string.Empty : data.CDNote.InvoiceNo;
            parameter.OtherRef = string.Empty;//Tạm thời để trống
            parameter.PackageUnit = data.PackageUnit == null ? string.Empty : data.PackageUnit;

            // Lấy thông tin Office của Creator
            var officeOfUser = GetInfoOfficeOfUser(data.CDNote.OfficeId);
            // Thông tin công ty
            var companyOfUser = sysCompanyRepository.Get(x => x.Id == data.CDNote.CompanyId).FirstOrDefault();
            parameter.CompanyName = companyOfUser?.BunameEn?.ToUpper();
            parameter.CompanyAddress1 = officeOfUser?.AddressEn ?? string.Empty;
            parameter.CompanyAddress2 = "Tel: " + officeOfUser?.Tel + "\nFax: " + officeOfUser?.Fax;
            parameter.Website = companyOfUser?.Website ?? string.Empty;
            parameter.Contact = _currentUser;//Get user name login
            parameter.CompanyDescription = string.Empty;

            // Thông tin Bank
            var _accountName = officeOfUser?.BankAccountNameVn ?? string.Empty;
            var _accountNameEN = officeOfUser?.BankAccountNameEn ?? string.Empty;
            var _bankName = officeOfUser?.BankNameLocal ?? string.Empty;
            var _bankNameEN = officeOfUser?.BankNameEn ?? string.Empty;
            var _bankAddress = officeOfUser?.BankAddressLocal ?? string.Empty;
            var _bankAddressEN = officeOfUser?.BankAddressEn ?? string.Empty;
            var _swiftAccs = officeOfUser?.SwiftCode ?? string.Empty;
            var _accsUsd = officeOfUser?.BankAccountUsd ?? string.Empty;
            var _accsVnd = officeOfUser?.BankAccountVnd ?? string.Empty;
            parameter.AccountName = _accountName;
            parameter.AccountNameEN = _accountNameEN;
            parameter.BankName = _bankName;
            parameter.BankNameEN = _bankNameEN;
            parameter.BankAddress = _bankAddress;
            parameter.BankAddressEN = _bankAddressEN;
            parameter.SwiftAccs = _swiftAccs;
            parameter.AccsUSD = _accsUsd;
            parameter.AccsVND = _accsVnd;
            parameter.Currency = criteria.Currency;

            result = new Crystal
            {
                ReportName = "LogisticCDNotePreviewNew.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listCharge);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }

        private void UpdateJobModifyTime(Guid jobId)
        {
            var jobOpsTrans = opstransRepository.First(x => x.Id == jobId);
            if (jobOpsTrans != null)
            {
                jobOpsTrans.UserModified = currentUser.UserID;
                jobOpsTrans.DatetimeModified = DateTime.Now;
                opstransRepository.Update(jobOpsTrans, x => x.Id == jobId, false);
            }
            var jobCSTrans = cstransRepository.First(x => x.Id == jobId);
            if (jobCSTrans != null)
            {
                jobCSTrans.UserModified = currentUser.UserID;
                jobCSTrans.DatetimeModified = DateTime.Now;
                cstransRepository.Update(jobCSTrans, x => x.Id == jobId, false);
            }
        }

        public bool CheckAllowDelete(Guid cdNoteId)
        {
            var cdNote = DataContext.Get(x => x.Id == cdNoteId).FirstOrDefault();
            var query = surchargeRepository.Get(x => (x.CreditNo == cdNote.Code && !string.IsNullOrEmpty(x.PaySoano))
                                                  || (x.DebitNo == cdNote.Code && (!string.IsNullOrEmpty(x.Soano) || x.AcctManagementId != null)));

            if (query.Any())
            {
                return false;
            }
            return true;
        }

        #region -- PREVIEW CD NOTE --
        /// <summary>
        /// Preview CD Note - Sea FCL Import
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public Crystal PreviewSIF(AcctCDNoteDetailsModel data, string currency)
        {
            Crystal result = null;
            var _currentUser = currentUser.UserName;

            var listCharge = new List<SeaDebitAgentsNewReport>();
            // var data = GetCDNoteDetails(criteria.JobId, criteria.CreditDebitNo);
            bool isOriginCurr = currency == DocumentConstants.CURRENCY_ORIGIN;
            if (data != null)
            {
                //Loop qua ds charge từ detail cdnote
                foreach (var item in data.ListSurcharges)
                {
                    var exchargeDateSurcharge = item.ExchangeDate == null ? DateTime.Now : item.ExchangeDate;
                    //Exchange Rate theo Currency truyền vào
                    decimal _exchangeRate = isOriginCurr ? 1 : currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.CurrencyId, currency);

                    var charge = new SeaDebitAgentsNewReport();
                    //Thông tin Partner
                    var partner = partnerRepositoty.Get(x => x.Id == data.PartnerId).FirstOrDefault();
                    charge.PartnerID = data.PartnerId?.ToUpper();
                    charge.PartnerName = data.PartnerNameEn?.ToUpper();
                    if (isOriginCurr)
                    {
                        charge.Address = (DocumentConstants.CURRENCY_LOCAL == item.CurrencyId && data.CDNote.Type == "DEBIT") ? partner.AddressVn : partner.AddressEn;//Lấy địa chỉ Billing
                    }
                    else
                    {
                        charge.Address = (DocumentConstants.CURRENCY_LOCAL == currency && data.CDNote.Type == "DEBIT") ? partner.AddressVn : partner.AddressEn;//Lấy địa chỉ Billing
                    }
                    charge.Workphone = data.PartnerTel?.ToUpper();
                    charge.Fax = data.PartnerFax?.ToUpper();
                    charge.Taxcode = data.PartnerTaxcode?.ToUpper();
                    charge.PersonalContact = data.PartnerPersonalContact?.ToUpper();

                    //Thông tin Shipment
                    charge.TransID = data.JobNo?.ToUpper();
                    charge.DepartureAirport = data.Pol + (!string.IsNullOrEmpty(data.PolCountry) ? ", " + data.PolCountry : string.Empty); //POL
                    charge.DepartureAirport = charge.DepartureAirport?.ToUpper();
                    charge.PlaceDelivery = data.Pod + (!string.IsNullOrEmpty(data.PodCountry) ? ", " + data.PodCountry : string.Empty); //POD
                    charge.PlaceDelivery = charge.PlaceDelivery?.ToUpper();
                    if (data.Etd != null)
                    {
                        charge.LoadingDate = data.Etd.Value;//ETD
                    }
                    if (data.Eta != null)
                    {
                        charge.ETA = data.Eta.Value; //ETA
                    }
                    charge.ATTN = data.HbShippers?.ToUpper();//Shipper -- lấy từ Housebill
                    charge.LocalVessel = data.Vessel?.ToUpper();//Vessel
                    charge.MAWB = data.MbLadingNo?.ToUpper(); //MBLNO
                    charge.Consignee = data.HbConsignees?.ToUpper();//Consignee -- lấy từ Housebill
                    charge.ContainerSize = data.HbPackages?.ToUpper(); //Quantity Cont
                    charge.GrossWeight = data.HbGrossweight ?? 0;//Total GW of HBL
                    charge.CBM = data.Volum ?? 0; //Total CBM of HBL
                    charge.SealNo = data.HbSealNo?.ToUpper(); //Cont/Seal No
                    charge.HWBNO = data.HbLadingNo?.ToUpper(); //HBLNOs

                    //Thông tin list charge
                    charge.Subject = (item.Type == DocumentConstants.CHARGE_OBH_TYPE) ? "ON BEHALF" : "CHARGES";
                    charge.Description = item.NameEn;//Charge name
                    charge.Notes = string.IsNullOrEmpty(item.Notes) ? "" : "(" + item.Notes + ")";
                    charge.Quantity = item.Quantity + _decimalNumber; //Cộng thêm phần thập phân
                    charge.Unit = item.UnitCode; //Unit Code
                    charge.QUnit = isOriginCurr ? item.CurrencyId : currency;

                    var _unitPrice = (item.UnitPrice ?? 0) * _exchangeRate; //Unit Price đã được Exchange Rate theo Currency và không làm tròn
                    charge.UnitPrice = _unitPrice + _decimalNumber; //cộng thêm phần thập phân
                    //Giá trị thực tế VAT (% VAT hoặc số tiền tuyệt đối)
                    charge.VAT = Math.Abs(item.Vatrate ?? 0) + _decimalNumber; //Cộng thêm phần thập phân

                    //Amount trước thuế
                    decimal _netAmount = _unitPrice * item.Quantity;
                    //Tiền thuế (nếu có)
                    decimal _taxMoney = 0;
                    if (isOriginCurr)
                    {
                        _netAmount = (item.CurrencyId == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_netAmount, 0) : NumberHelper.RoundNumber(_netAmount, 2); //Làm tròn NetAmount
                        _taxMoney = (item.Vatrate != null) ? (item.Vatrate < 101 & item.Vatrate >= 0) ? ((item.Vatrate ?? 0) * _netAmount / 100) : Math.Abs(item.Vatrate * _exchangeRate ?? 0) : 0;
                        _taxMoney = (item.CurrencyId == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_taxMoney, 0) : NumberHelper.RoundNumber(_taxMoney, 2); //Làm tròn tiền thuế
                    }
                    else
                    {
                        _netAmount = (currency == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_netAmount, 0) : NumberHelper.RoundNumber(_netAmount, 2); //Làm tròn NetAmount
                        _taxMoney = (item.Vatrate != null) ? (item.Vatrate < 101 & item.Vatrate >= 0) ? ((item.Vatrate ?? 0) * _netAmount / 100) : Math.Abs(item.Vatrate * _exchangeRate ?? 0) : 0;
                        _taxMoney = (currency == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_taxMoney, 0) : NumberHelper.RoundNumber(_taxMoney, 2); //Làm tròn tiền thuế
                    }

                    var _totalAmount = _netAmount + _taxMoney; //Total Amount = Amount trước thuế + Tiền thuế

                    var _credit = (item.Type == DocumentConstants.CHARGE_BUY_TYPE || (item.Type == DocumentConstants.CHARGE_OBH_TYPE && data.PartnerId == item.PayerId)) ? _totalAmount : 0;
                    var _debit = (item.Type == DocumentConstants.CHARGE_SELL_TYPE || (item.Type == DocumentConstants.CHARGE_OBH_TYPE && data.PartnerId == item.PaymentObjectId)) ? _totalAmount : 0;
                    if (isOriginCurr)
                    {
                        charge.Credit = (item.CurrencyId == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_credit, 0) : NumberHelper.RoundNumber(_credit, 2); //Làm tròn Amount Credit
                        charge.Debit = (item.CurrencyId == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_debit, 0) : NumberHelper.RoundNumber(_debit, 2); //Làm tròn Amount Debit
                    }
                    else
                    {
                        charge.Credit = (currency == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_credit, 0) : NumberHelper.RoundNumber(_credit, 2); //Làm tròn Amount Credit
                        charge.Debit = (currency == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_debit, 0) : NumberHelper.RoundNumber(_debit, 2); //Làm tròn Amount Debit
                    }
                    charge.Credit = charge.Credit + _decimalNumber; //Cộng thêm phần thập phân
                    charge.Debit = charge.Debit + _decimalNumber; //Cộng thêm phần thập phân

                    listCharge.Add(charge);
                }
            }
            var parameter = new SeaDebitAgentsNewReportParams();
            parameter.DebitNo = data.CDNote.Code;
            parameter.IssuedDate = data != null && data.CDNote != null && data.CDNote.DatetimeCreated != null ? data.CDNote.DatetimeCreated.Value.ToString("dd MMM, yyyy") : string.Empty;//Lấy ngày tạo CDNote
            parameter.DBTitle = data.CDNote.Type == "CREDIT" ? "CREDIT NOTE" : data.CDNote.Type == "DEBIT" ? "DEBIT NOTE" : "INVOICE";
            parameter.ReviseNotice = DateTime.Now.ToString("dd/MM/yyyy"); //Bỏ chữ Revised

            var _inword = string.Empty;
            // Preview with Original
            if (isOriginCurr)
            {
                var currencyLst = listCharge.GroupBy(x => x.QUnit).Select(x => x.Key).ToList();
                var arrCurr = new List<Tuple<string, decimal?, decimal?>>();
                foreach (var item in currencyLst)
                {
                    arrCurr.Add(Tuple.Create(item, listCharge.Where(x => x.QUnit == item).Sum(x => x.Debit), listCharge.Where(x => x.QUnit == item).Sum(x => x.Credit)));
                }

                string _totalDebit = string.Empty, _totalCredit = string.Empty, _balanceAmount = string.Empty;
                for (int i = 0; i < arrCurr.Count(); i++)
                {
                    decimal _debit = 0, _credit = 0, _balance = 0;
                    _debit = arrCurr[i].Item1 == "DONG" ? NumberHelper.RoundNumber(arrCurr[i].Item2.Value, 0) : NumberHelper.RoundNumber(arrCurr[i].Item2.Value, 2);
                    _credit = arrCurr[i].Item1 == "DONG" ? NumberHelper.RoundNumber(arrCurr[i].Item3.Value, 0) : NumberHelper.RoundNumber(arrCurr[i].Item3.Value, 2);
                    string formatAmount = arrCurr[i].Item1 == "DONG" ? "{0:n0}" : "{0:n2}";
                    _balance = Math.Abs(_debit - _credit);
                    // Total debit
                    if (_debit > 0)
                    {
                        _totalDebit += string.IsNullOrEmpty(_totalDebit) ? string.Empty : "\n";
                        _totalDebit += string.Format(formatAmount, _debit) + " " + arrCurr[i].Item1;
                    }
                    // Total credit
                    if (_credit > 0)
                    {
                        _totalCredit += string.IsNullOrEmpty(_totalCredit) ? string.Empty : "\n";
                        _totalCredit += string.Format(formatAmount, _credit) + " " + arrCurr[i].Item1;
                    }
                    // Balance Amount
                    _balanceAmount += string.IsNullOrEmpty(_balanceAmount) ? string.Empty : "\n";
                    _balanceAmount += string.Format(formatAmount, _balance) + " " + arrCurr[i].Item1;

                    //Chuyển tiền Amount thành chữ
                    _balance = NumberHelper.RoundNumber(_balance, 2);
                    _inword += string.IsNullOrEmpty(_inword) ? string.Empty : "\n";
                    _inword += InWordCurrency.ConvertNumberCurrencyToStringUSD(_balance, arrCurr[i].Item1);
                }
                parameter.TotalDebit = _totalDebit;
                parameter.TotalCredit = _totalCredit;
                parameter.BalanceAmount = _balanceAmount;
            }
            else // Preview with USD/VND
            {
                var _totalDebit = (currency == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(listCharge.Sum(x => x.Debit).Value, 0) : NumberHelper.RoundNumber(listCharge.Sum(x => x.Debit).Value, 2);
                var _totalCredit = (currency == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(listCharge.Sum(x => x.Credit).Value, 0) : NumberHelper.RoundNumber(listCharge.Sum(x => x.Credit).Value, 2);
                parameter.TotalDebit = string.Empty;
                if (_totalDebit != 0)
                {
                    parameter.TotalDebit = (currency == DocumentConstants.CURRENCY_LOCAL) ? string.Format("{0:n0}", _totalDebit) : string.Format("{0:n2}", _totalDebit);
                }

                parameter.TotalCredit = string.Empty;
                if (_totalCredit != 0)
                {
                    parameter.TotalCredit = (currency == DocumentConstants.CURRENCY_LOCAL) ? string.Format("{0:n0}", _totalCredit) : string.Format("{0:n2}", _totalCredit);
                }

                var _blAmount = Math.Abs(_totalDebit - _totalCredit);
                parameter.BalanceAmount = (currency == DocumentConstants.CURRENCY_LOCAL) ? string.Format("{0:n0}", _blAmount) : string.Format("{0:n2}", _blAmount);

                //Chuyển tiền Amount thành chữ
                decimal _balanceAmount = NumberHelper.RoundNumber(_blAmount, 2);
                var _currency = currency == DocumentConstants.CURRENCY_LOCAL && _balanceAmount >= 1 ?
                           (_balanceAmount % 1 > 0 ? "đồng lẻ" : "đồng chẵn")
                        :
                        "U.S. dollar(s)";

                _inword = currency == DocumentConstants.CURRENCY_LOCAL && _balanceAmount >= 1 ?
                        InWordCurrency.ConvertNumberCurrencyToString(_balanceAmount, _currency)
                    :
                        InWordCurrency.ConvertNumberCurrencyToStringUSD(_balanceAmount, _currency);
            }
            parameter.InwordVND = !string.IsNullOrEmpty(_inword) ? _inword.ToUpper() : string.Empty;
            parameter.IssueInv = string.Empty; //Tạm thời để trống
            parameter.InvoiceInfo = string.Empty;//Tạm thời để trống
            parameter.OtherRef = string.Empty;//Tạm thời để trống

            // Lấy thông tin Office của Creator
            var officeOfUser = GetInfoOfficeOfUser(data.CDNote.OfficeId);
            // Thông tin Company của Creator
            var companyOfUser = sysCompanyRepository.Get(x => x.Id == data.CDNote.CompanyId).FirstOrDefault();
            // Thông tin công ty
            parameter.CompanyName = companyOfUser?.BunameEn?.ToUpper();
            parameter.CompanyAddress1 = officeOfUser?.AddressEn ?? string.Empty;
            parameter.CompanyAddress2 = "Tel: " + officeOfUser?.Tel + "\nFax: " + officeOfUser?.Fax;
            parameter.Website = companyOfUser?.Website;
            parameter.Contact = _currentUser;//Get user name login
            parameter.CompanyDescription = string.Empty;

            // Thông tin Bank
            var _accountName = officeOfUser?.BankAccountNameVn ?? string.Empty;
            var _accountNameEN = officeOfUser?.BankAccountNameEn ?? string.Empty;
            var _bankName = officeOfUser?.BankNameLocal ?? string.Empty;
            var _bankNameEN = officeOfUser?.BankNameEn ?? string.Empty;
            var _bankAddress = officeOfUser?.BankAddressLocal ?? string.Empty;
            var _bankAddressEN = officeOfUser?.BankAddressEn ?? string.Empty;
            var _swiftAccs = officeOfUser?.SwiftCode ?? string.Empty;
            var _accsUsd = officeOfUser?.BankAccountUsd ?? string.Empty;
            var _accsVnd = officeOfUser?.BankAccountVnd ?? string.Empty;
            parameter.AccountName = _accountName;
            parameter.AccountNameEN = _accountNameEN;
            parameter.BankName = _bankName;
            parameter.BankNameEN = _bankNameEN;
            parameter.BankAddress = _bankAddress;
            parameter.BankAddressEN = _bankAddressEN;
            parameter.SwiftAccs = _swiftAccs;
            parameter.AccsUSD = _accsUsd;
            parameter.AccsVND = _accsVnd;

            parameter.Currency = currency;

            result = new Crystal
            {
                ReportName = "SeaDebitAgentsNewVND.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listCharge);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }

        public Crystal PreviewAir(AcctCDNoteDetailsModel data, string currency)
        {
            Crystal result = null;
            var _currentUser = currentUser.UserName;
            var listCharge = new List<AirShipperDebitNewReport>();
            //var data = GetCDNoteDetails(criteria.JobId, criteria.CreditDebitNo);

            string _hbllist = string.Empty;
            bool isOriginCurr = currency == DocumentConstants.CURRENCY_ORIGIN;
            if (data != null)
            {
                _hbllist = string.Join("\r\n", data.ListSurcharges.Where(x => !string.IsNullOrEmpty(x.Hwbno)).Select(s => s.Hwbno).Distinct());
                int i = 1;
                foreach (var item in data.ListSurcharges)
                {
                    var exchargeDateSurcharge = item.ExchangeDate == null ? DateTime.Now : item.ExchangeDate;
                    //Exchange Rate theo Currency truyền vào
                    decimal _exchangeRate = isOriginCurr ? 1 : currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.CurrencyId, currency);
                    var charge = new AirShipperDebitNewReport();
                    charge.IndexSort = i++;

                    //Thông tin Partner
                    charge.PartnerID = data.PartnerId?.ToUpper();
                    charge.PartnerName = data.PartnerNameEn?.ToUpper();
                    charge.Address = data.PartnerShippingAddress?.ToUpper();
                    charge.Workphone = data.PartnerTel?.ToUpper();
                    charge.Fax = data.PartnerFax?.ToUpper();
                    charge.TaxCode = data.PartnerTaxcode?.ToUpper();
                    charge.PersonalContact = data.PartnerPersonalContact?.ToUpper();
                    charge.Cell = string.Empty; //NOT USE

                    //Thông tin Shipment
                    charge.TransID = data.JobNo?.ToUpper();
                    charge.DepartureAirport = data.Pol + (!string.IsNullOrEmpty(data.PolCountry) ? ", " + data.PolCountry : string.Empty); //POL
                    charge.DepartureAirport = charge.DepartureAirport?.ToUpper();
                    charge.LastDestination = data.Pod + (!string.IsNullOrEmpty(data.PodCountry) ? ", " + data.PodCountry : string.Empty); //POD
                    charge.LastDestination = charge.LastDestination?.ToUpper();

                    charge.LoadingDate = data.Etd;//ETD
                    charge.ETA = data?.Eta; //ETA

                    charge.ATTN = string.Empty; //NOT USE
                    charge.FlightNo = data.Vessel?.ToUpper();//Flight No
                    charge.FlightDate = data.VesselDate; //Flight Date
                    charge.MAWB = data.MbLadingNo?.ToUpper(); //MBLNO
                    charge.Consignee = data.HbConsignees?.ToUpper();//Consignee -- lấy từ Housebill
                    charge.GrossWeight = data.HbGrossweight;//Total GW of HBL
                    charge.HWBNO = data.HbLadingNo?.ToUpper(); //HBLNOs
                    charge.WChargeable = data.HbChargeWeight; //Total Charge Weight of HBL

                    //Thông tin list charges
                    charge.Subject = (item.Type == DocumentConstants.CHARGE_OBH_TYPE) ? "OB BEHALF" : "FREIGHT CHARGES";
                    charge.Description = item.NameEn;//Charge name
                    charge.Quantity = item.Quantity + _decimalNumber; //Cộng thêm phần thập phân
                    charge.Unit = item.Unit;
                    charge.QUnit = isOriginCurr ? item.CurrencyId : currency;

                    var _unitPrice = (item.UnitPrice ?? 0) * _exchangeRate; //Unit Price đã được Exchange Rate theo Currency và không làm tròn
                    charge.UnitPrice = _unitPrice + _decimalNumber; //cộng thêm phần thập phân
                    //Giá trị thực tế VAT (% VAT hoặc số tiền tuyệt đối)
                    charge.VAT = Math.Abs(item.Vatrate ?? 0) + _decimalNumber; //Cộng thêm phần thập phân

                    //Amount trước thuế
                    decimal _netAmount = _unitPrice * item.Quantity;
                    //Tiền thuế (nếu có)
                    decimal _taxMoney = 0;
                    if (isOriginCurr)
                    {
                        _netAmount = (item.CurrencyId == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_netAmount, 0) : NumberHelper.RoundNumber(_netAmount, 2); //Làm tròn NetAmount
                        _taxMoney = (item.Vatrate != null) ? (item.Vatrate < 101 & item.Vatrate >= 0) ? ((item.Vatrate ?? 0) * _netAmount / 100) : Math.Abs(item.Vatrate * _exchangeRate ?? 0) : 0;
                        _taxMoney = (item.CurrencyId == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_taxMoney, 0) : NumberHelper.RoundNumber(_taxMoney, 2); //Làm tròn tiền thuế
                    }
                    else
                    {
                        _netAmount = (currency == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_netAmount, 0) : NumberHelper.RoundNumber(_netAmount, 2); //Làm tròn NetAmount
                        _taxMoney = (item.Vatrate != null) ? (item.Vatrate < 101 & item.Vatrate >= 0) ? ((item.Vatrate ?? 0) * _netAmount / 100) : Math.Abs(item.Vatrate * _exchangeRate ?? 0) : 0;
                        _taxMoney = (currency == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_taxMoney, 0) : NumberHelper.RoundNumber(_taxMoney, 2); //Làm tròn tiền thuế
                    }

                    var _totalAmount = _netAmount + _taxMoney; //Total Amount = Amount trước thuế + Tiền thuế

                    var _credit = (item.Type == DocumentConstants.CHARGE_BUY_TYPE || (item.Type == DocumentConstants.CHARGE_OBH_TYPE && data.PartnerId == item.PayerId)) ? _totalAmount : 0;
                    var _debit = (item.Type == DocumentConstants.CHARGE_SELL_TYPE || (item.Type == DocumentConstants.CHARGE_OBH_TYPE && data.PartnerId == item.PaymentObjectId)) ? _totalAmount : 0;
                    if (isOriginCurr)
                    {
                        charge.Credit = (item.CurrencyId == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_credit, 0) : NumberHelper.RoundNumber(_credit, 2); //Làm tròn Amount Credit
                        charge.Debit = (item.CurrencyId == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_debit, 0) : NumberHelper.RoundNumber(_debit, 2); //Làm tròn Amount Debit
                    }
                    else
                    {
                        charge.Credit = (currency == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_credit, 0) : NumberHelper.RoundNumber(_credit, 2); //Làm tròn Amount Credit
                        charge.Debit = (currency == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(_debit, 0) : NumberHelper.RoundNumber(_debit, 2); //Làm tròn Amount Debit
                    }
                    charge.Credit = charge.Credit + _decimalNumber; //Cộng thêm phần thập phân
                    charge.Debit = charge.Debit + _decimalNumber; //Cộng thêm phần thập phân

                    charge.ExtVND = 0; //NOT USE
                    charge.Notes = item.Notes;

                    charge.InputData = string.Empty; //Chưa biết
                    charge.CTNSQty = 0; //NOT USE
                    charge.CTNSUnit = string.Empty; //NOT USE
                    charge.Deposit = 0; //NOT USE
                    charge.DepositCurr = string.Empty; //NOT USE
                    charge.Commodity = string.Empty; //NOT USE
                    charge.DecimalSymbol = ",";
                    charge.DigitSymbol = ",";
                    charge.DecimalNo = 0; //NOT USE
                    charge.CurrDecimalNo = 0; //NOT USE
                    charge.FlexID = data.FlexId;
                    listCharge.Add(charge);
                }
            }
            var parameter = new AirShipperDebitNewReportParams();
            parameter.DebitNo = data.CDNote.Code;
            parameter.IssuedDate = data != null && data.CDNote != null && data.CDNote.DatetimeCreated != null ? data.CDNote.DatetimeCreated.Value.ToString("dd/MM/yyyy") : string.Empty;//Lấy ngày tạo CDNote
            parameter.DBTitle = data.CDNote.Type == "CREDIT" ? "CREDIT NOTE" : data.CDNote.Type == "DEBIT" ? "DEBIT NOTE" : "INVOICE";
            parameter.ReviseNotice = DateTime.Now.ToString("dd/MM/yyyy"); //Bỏ chữ Revised

            var _inword = string.Empty;
            // Preview with Original
            if (isOriginCurr)
            {
                var currencyLst = listCharge.GroupBy(x => x.QUnit).Select(x => x.Key).ToList();
                var arrCurr = new List<Tuple<string, decimal?, decimal?>>();
                foreach (var item in currencyLst)
                {
                    arrCurr.Add(Tuple.Create(item, listCharge.Where(x => x.QUnit == item).Sum(x => x.Debit), listCharge.Where(x => x.QUnit == item).Sum(x => x.Credit)));
                }

                string _totalDebit = string.Empty, _totalCredit = string.Empty, _balanceAmount = string.Empty;
                for (int i = 0; i < arrCurr.Count(); i++)
                {
                    decimal _debit = 0, _credit = 0, _balance = 0;
                    string formatAmount = arrCurr[i].Item1 == "DONG" ? "{0:n0}" : "{0:n2}";
                    _debit = arrCurr[i].Item1 == "DONG" ? NumberHelper.RoundNumber(arrCurr[i].Item2.Value, 0) : NumberHelper.RoundNumber(arrCurr[i].Item2.Value, 2);
                    _credit = arrCurr[i].Item1 == "DONG" ? NumberHelper.RoundNumber(arrCurr[i].Item3.Value, 0) : NumberHelper.RoundNumber(arrCurr[i].Item3.Value, 2);
                    _balance = Math.Abs(_debit - _credit);

                    // Total debit
                    if (_debit > 0)
                    {
                        _totalDebit += string.IsNullOrEmpty(_totalDebit) ? string.Empty : "\n";
                        _totalDebit += String.Format(formatAmount, _debit) + " " + arrCurr[i].Item1;
                    }
                    // Total credit
                    if (_credit > 0)
                    {
                        _totalCredit += string.IsNullOrEmpty(_totalCredit) ? string.Empty : "\n";
                        _totalCredit += String.Format(formatAmount, _credit) + " " + arrCurr[i].Item1;
                    }
                    // Balance Amount
                    _balanceAmount += string.IsNullOrEmpty(_balanceAmount) ? string.Empty : "\n";
                    _balanceAmount += String.Format(formatAmount, _balance) + " " + arrCurr[i].Item1;
                    //Chuyển tiền Amount thành chữ
                    _balance = NumberHelper.RoundNumber(_balance, 2);
                    _inword += string.IsNullOrEmpty(_inword) ? string.Empty : "\n";
                    _inword += InWordCurrency.ConvertNumberCurrencyToStringUSD(_balance, arrCurr[i].Item1);
                }
                parameter.TotalDebit = _totalDebit;
                parameter.TotalCredit = _totalCredit;
                parameter.BalanceAmount = _balanceAmount;
            }
            else // Preview with USD/VND
            {
                var _totalDebit = (currency == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(listCharge.Sum(x => x.Debit).Value, 0) : NumberHelper.RoundNumber(listCharge.Sum(x => x.Debit).Value, 2);
                var _totalCredit = (currency == DocumentConstants.CURRENCY_LOCAL) ? NumberHelper.RoundNumber(listCharge.Sum(x => x.Credit).Value, 0) : NumberHelper.RoundNumber(listCharge.Sum(x => x.Credit).Value, 2);
                parameter.TotalDebit = string.Empty;
                if (_totalDebit != 0)
                {
                    parameter.TotalDebit = (currency == DocumentConstants.CURRENCY_LOCAL) ? String.Format("{0:n0}", _totalDebit) : String.Format("{0:n2}", _totalDebit);
                }

                parameter.TotalCredit = string.Empty;
                if (_totalCredit != 0)
                {
                    parameter.TotalCredit = (currency == DocumentConstants.CURRENCY_LOCAL) ? String.Format("{0:n0}", _totalCredit) : String.Format("{0:n2}", _totalCredit);
                }

                decimal _balanceAmount = Math.Abs(_totalDebit - _totalCredit);
                parameter.BalanceAmount = (currency == DocumentConstants.CURRENCY_LOCAL) ? String.Format("{0:n0}", _balanceAmount) : String.Format("{0:n2}", _balanceAmount);

                //Chuyển tiền Amount thành chữ
                _balanceAmount = NumberHelper.RoundNumber(_balanceAmount, 2);

                var _currency = currency == DocumentConstants.CURRENCY_LOCAL && _balanceAmount >= 1 ?
                           (_balanceAmount % 1 > 0 ? "đồng lẻ" : "đồng chẵn")
                        :
                        "U.S. dollar(s)";

                _inword = currency == DocumentConstants.CURRENCY_LOCAL && _balanceAmount >= 1 ?
                        InWordCurrency.ConvertNumberCurrencyToString(_balanceAmount, _currency)
                    :
                        InWordCurrency.ConvertNumberCurrencyToStringUSD(_balanceAmount, _currency);
            }
            parameter.InwordVND = !string.IsNullOrEmpty(_inword) ? _inword.ToUpper() : string.Empty;
            parameter.IssueInv = string.Empty; //Tạm thời để trống
            parameter.InvoiceInfo = string.Empty;//Tạm thời để trống
            parameter.OtherRef = string.Empty;//Tạm thời để trống

            // Lấy thông tin Office của Creator
            var officeOfUser = GetInfoOfficeOfUser(data.CDNote.OfficeId);
            // Thông tin công ty
            var companyOfUser = sysCompanyRepository.Get(x => x.Id == data.CDNote.CompanyId).FirstOrDefault();
            parameter.CompanyName = companyOfUser?.BunameEn?.ToUpper();
            parameter.CompanyAddress1 = officeOfUser?.AddressEn ?? string.Empty;
            parameter.CompanyAddress2 = "Tel: " + officeOfUser?.Tel + "\nFax: " + officeOfUser?.Fax;
            parameter.Website = companyOfUser?.Website ?? string.Empty;
            parameter.Contact = _currentUser;//Get user name login
            parameter.CompanyDescription = string.Empty;

            // Thông tin Bank
            var _accountName = officeOfUser?.BankAccountNameVn ?? string.Empty;
            var _accountNameEN = officeOfUser?.BankAccountNameEn ?? string.Empty;
            var _bankName = officeOfUser?.BankNameLocal ?? string.Empty;
            var _bankNameEN = officeOfUser?.BankNameEn ?? string.Empty;
            var _bankAddress = officeOfUser?.BankAddressLocal ?? string.Empty;
            var _bankAddressEN = officeOfUser?.BankAddressEn ?? string.Empty;
            var _swiftAccs = officeOfUser?.SwiftCode ?? string.Empty;
            var _accsUsd = officeOfUser?.BankAccountUsd ?? string.Empty;
            var _accsVnd = officeOfUser?.BankAccountVnd ?? string.Empty;
            parameter.AccountName = _accountName;
            parameter.AccountNameEN = _accountNameEN;
            parameter.BankName = _bankName;
            parameter.BankNameEN = _bankNameEN;
            parameter.BankAddress = _bankAddress;
            parameter.BankAddressEN = _bankAddressEN;
            parameter.SwiftAccs = _swiftAccs;
            parameter.AccsUSD = _accsUsd;
            parameter.AccsVND = _accsVnd;

            parameter.Currency = isOriginCurr ? DocumentConstants.CURRENCY_LOCAL : currency;
            parameter.HBLList = _hbllist?.ToUpper();
            parameter.DecimalNo = 2;

            if (data.ExcRateUsdToLocal != null)
            {
                parameter.RateUSDToVND = data.ExcRateUsdToLocal.Value;
            }
            else
            {
                //Exchange Rate USD to VND (Lấy tỷ giá theo ngày tạo CDNote)
                var _exchangeRateUSDToVND = catCurrencyExchangeRepository.Get(x => (x.DatetimeCreated.Value.Date == data.DatetimeCreated.Value.Date && x.CurrencyFromId == DocumentConstants.CURRENCY_USD && x.CurrencyToId == DocumentConstants.CURRENCY_LOCAL)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                parameter.RateUSDToVND = _exchangeRateUSDToVND?.Rate ?? 0;
            }

            result = new Crystal
            {
                ReportName = "AirShipperDebitNewVND.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listCharge);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }

        /// <summary>
        /// Export Excel Template of OPS CD Note
        /// </summary>
        /// <param name="cdNoteDetail"></param>
        /// <param name="officeId"></param>
        /// <returns></returns>
        public AcctCDNoteExportResult GetDataExportOpsCDNote(AcctCDNoteDetailsModel cdNoteDetail, Guid officeId)
        {
            var result = new AcctCDNoteExportResult();
            if (cdNoteDetail != null)
            {
                IQueryable<CustomsDeclaration> _customClearances = customsDeclarationRepository.Get(x => x.JobNo == cdNoteDetail.JobNo);
                CustomsDeclaration _clearance = null;
                if (_customClearances.Count() > 0 || _customClearances != null)
                {
                    _clearance = _customClearances.OrderBy(x => x.ClearanceDate).FirstOrDefault();
                }

                result.CDNo = cdNoteDetail.CDNote.Code;
                result.PartnerNameEn = cdNoteDetail.PartnerNameEn;
                result.BillingAddress = cdNoteDetail.PartnerShippingAddress;
                result.Taxcode = cdNoteDetail.PartnerTaxcode;
                result.ClearanceNo = _clearance?.ClearanceNo;
                result.GW = cdNoteDetail.GW;
                result.CBM = cdNoteDetail.CBM;
                result.HBL = cdNoteDetail.MbLadingNo;
                result.Cont = cdNoteDetail.HbConstainers;
                result.WareHouseName = cdNoteDetail.WarehouseName;

                result.ListCharges = new List<ExportCDNoteModel>();
                foreach (var item in cdNoteDetail.ListSurcharges)
                {
                    var cdNote = new ExportCDNoteModel();
                    decimal _exchangeRateToVND = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.CurrencyId, DocumentConstants.CURRENCY_LOCAL);
                    _exchangeRateToVND = item.CurrencyId == DocumentConstants.CURRENCY_LOCAL ? 1 : _exchangeRateToVND;
                    decimal? fee = NumberHelper.RoundNumber(((item.UnitPrice * item.Quantity) * _exchangeRateToVND).Value, 0);
                    decimal? vat = item.Vatrate != null ? (item.Vatrate < 0 ? Math.Abs(item.Vatrate.Value) : ((fee * item.Vatrate) / 100)) : 0;
                    cdNote.Type = item.Type;
                    cdNote.Description = item.NameEn;
                    cdNote.VATInvoiceNo = item.InvoiceNo;
                    cdNote.Amount = fee;
                    cdNote.Notes = item.Notes;
                    cdNote.VATAmount = NumberHelper.RoundNumber(vat.Value, 0);
                    cdNote.TotalAmount = fee + vat;
                    cdNote.Notes = item.Notes;
                    result.ListCharges.Add(cdNote);
                }

                //Lấy thông tin Office của User Login
                var officeOfUser = GetInfoOfficeOfUser(officeId);
                result.BankNameEn = officeOfUser?.BankNameEn ?? string.Empty;
                result.OfficeEn = officeOfUser?.BranchNameEn ?? string.Empty;
                result.BankAddressEn = officeOfUser?.BankAddressEn ?? string.Empty;
                result.BankAccountNameEn = officeOfUser?.BankAccountNameEn ?? string.Empty;
                result.SwiftCode = officeOfUser?.SwiftCode ?? string.Empty;
                result.BankAccountVND = officeOfUser?.BankAccountVnd ?? string.Empty;

                string cdNoteType = !string.IsNullOrEmpty(cdNoteDetail.CDNote.Type) && cdNoteDetail.CDNote.Type != "INVOICE" ? cdNoteDetail.CDNote.Type + " NOTE" : (cdNoteDetail.CDNote.Type ?? string.Empty);
                result.CdNoteType = cdNoteType;
            }
            return result;
        }

        private SysOffice GetInfoOfficeOfUser(Guid? officeId)
        {
            SysOffice result = sysOfficeRepo.Get(x => x.Id == officeId).FirstOrDefault();
            return result;
        }
        #endregion -- PREVIEW CD NOTE --        

        private IQueryable<AcctCdnote> Query(CDNoteCriteria criteria)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.accManagement);
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (rangeSearch == PermissionRange.None) return null;
            Expression<Func<AcctCdnote, bool>> perQuery = GetQueryPermission(rangeSearch, _user);
            Expression<Func<AcctCdnote, bool>> query = x => (x.PartnerId == criteria.PartnerId || string.IsNullOrEmpty(criteria.PartnerId))
                                            && (x.UserCreated == criteria.CreatorId || string.IsNullOrEmpty(criteria.CreatorId))
                                            && (x.Type == criteria.Type || string.IsNullOrEmpty(criteria.Type));
            if (criteria.IssuedDate != null)
            {
                query = query.And(x => x.DatetimeCreated.Value.Date == criteria.IssuedDate.Value.Date);
            }

            if (perQuery != null)
            {
                query = query.And(perQuery);
            }

            if (!string.IsNullOrEmpty(criteria.ReferenceNos))
            {
                IEnumerable<string> refNos = criteria.ReferenceNos.Split('\n').Select(x => x.Trim()).Where(x => x != null);
                query = query.And(x => refNos.Any(a => a == x.Code));
            }

            if (string.IsNullOrEmpty(criteria.ReferenceNos)
                && string.IsNullOrEmpty(criteria.PartnerId)
                && criteria.IssuedDate == null
                && string.IsNullOrEmpty(criteria.CreatorId)
                && string.IsNullOrEmpty(criteria.Type)
                && string.IsNullOrEmpty(criteria.Status))
            {
                var maxDate = DataContext.Get().Max(x => x.DatetimeCreated) ?? DateTime.Now;
                var minDate = maxDate.AddMonths(-1); //Bắt đầu từ ngày MaxDate trở về trước 1 tháng
                query = query.And(x => x.DatetimeCreated.Value.Date >= minDate.Date && x.DatetimeCreated.Value.Date <= maxDate.Date);
            }

            var results = DataContext.Get(query);
            return results;
        }

        private Expression<Func<AcctCdnote, bool>> GetQueryPermission(PermissionRange rangeSearch, ICurrentUser user)
        {
            Expression<Func<AcctCdnote, bool>> perQuery = null;
            switch (rangeSearch)
            {
                case PermissionRange.All:
                    break;
                case PermissionRange.Owner:
                    perQuery = x => x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Group:
                    perQuery = x => (x.GroupId == user.GroupId && x.DepartmentId == user.DepartmentId && x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Department:
                    perQuery = x => (x.DepartmentId == user.DepartmentId && x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Office:
                    perQuery = x => (x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == currentUser.UserID;
                    break;
                case PermissionRange.Company:
                    perQuery = x => x.CompanyId == user.CompanyID
                                                || x.UserCreated == currentUser.UserID;
                    break;
            }
            return perQuery;
        }

        public List<CDNoteModel> Paging(CDNoteCriteria criteria, int page, int size, out int rowsCount)
        {
            List<CDNoteModel> results = null;
            var cdNotes = Query(criteria).Select(s => new AcctCdnote
            {
                Id = s.Id,
                Code = s.Code,
                JobId = s.JobId,
                PartnerId = s.PartnerId,
                UserCreated = s.UserCreated,
                DatetimeCreated = s.DatetimeCreated,
                DatetimeModified = s.DatetimeModified,
                LastSyncDate = s.LastSyncDate,
                SyncStatus = s.SyncStatus
            }).ToArray().OrderByDescending(o => o.DatetimeModified).AsQueryable();
            
            if (cdNotes == null)
            {
                rowsCount = 0;
                return results;
            }
            
            var charges = surchargeRepository.Get().Select(s => new CsShipmentSurcharge
            {
                CurrencyId = s.CurrencyId,
                Total = s.Total,
                JobNo = s.JobNo,
                InvoiceNo = s.InvoiceNo,
                VoucherId = s.VoucherId,
                AcctManagementId = s.AcctManagementId,
                Hblno = s.Hblno,
                CreditNo = s.CreditNo,
                DebitNo = s.DebitNo
            });
            
            var query = from cdnote in cdNotes
                        join charge in charges on cdnote.Code equals (charge.DebitNo ?? charge.CreditNo)
                        select new { cdnote, charge };

            var grpQuery = query.GroupBy(g => new {
                ReferenceNo = g.charge.DebitNo ?? g.charge.CreditNo,
                Currency = g.charge.CurrencyId
            }).Select(se => new {
                ReferenceNo = se.Key.ReferenceNo,
                Currency = se.Key.Currency,
                CdNote = se.Select(s => s.cdnote),
                Charge = se.Select(s => s.charge)
            });

            var selectData = grpQuery.Select(se => new CDNoteModel
            {
                Id = se.CdNote.FirstOrDefault().Id,
                JobId = se.CdNote.FirstOrDefault().JobId,
                PartnerId = se.CdNote.FirstOrDefault().PartnerId,
                PartnerName = string.Empty,
                ReferenceNo = se.ReferenceNo,
                JobNo = se.Charge.FirstOrDefault().JobNo,
                HBLNo = string.Join("; ", se.Charge.Select(s => s.Hblno).Distinct()),
                Total = se.Charge.Sum(x => x.Total),
                Currency = se.Currency,
                IssuedDate = se.CdNote.FirstOrDefault().DatetimeCreated,
                Creator = se.CdNote.FirstOrDefault().UserCreated,
                Status = se.Charge.FirstOrDefault().AcctManagementId != null ? "Issued" : "New",
                InvoiceNo = se.Charge.FirstOrDefault().InvoiceNo,
                VoucherId = se.Charge.FirstOrDefault().VoucherId,
                IssuedStatus = se.Charge.Any(y => !string.IsNullOrEmpty(y.InvoiceNo) && y.AcctManagementId != null) ? "Issued Invoice" : se.Charge.Any(y => !string.IsNullOrEmpty(y.VoucherId) && y.AcctManagementId != null) ? "Issued Voucher" : "New",
                SyncStatus = se.CdNote.FirstOrDefault().SyncStatus,
                LastSyncDate = se.CdNote.FirstOrDefault().LastSyncDate,
                DatetimeModified = se.CdNote.FirstOrDefault().DatetimeModified
            });

            var _resultDatas = GetByStatus(criteria.Status, selectData).ToArray();

            rowsCount = _resultDatas.ToArray().Count();

            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                var take = _resultDatas.Skip((page - 1) * size).Take(size).AsQueryable();

                //Join to get info PartnerName, Username create CDNote
                var joinData = from cd in take
                               join partner in partnerRepositoty.Get() on cd.PartnerId equals partner.Id into partnerGrp
                               from partner in partnerGrp.DefaultIfEmpty()
                               join creator in sysUserRepo.Get() on cd.Creator equals creator.Id into creatorGrp
                               from creator in creatorGrp.DefaultIfEmpty()
                               select new CDNoteModel
                               {
                                   Id = cd.Id,
                                   JobId = cd.JobId,
                                   PartnerId = cd.PartnerId,
                                   PartnerName = partner.PartnerNameEn,
                                   ReferenceNo = cd.ReferenceNo,
                                   JobNo = cd.JobNo,
                                   HBLNo = cd.HBLNo,
                                   Total = cd.Total,
                                   Currency = cd.Currency,
                                   IssuedDate = cd.IssuedDate,
                                   Creator = creator.Username,
                                   Status = cd.Status,
                                   InvoiceNo = cd.InvoiceNo,
                                   VoucherId = cd.VoucherId,
                                   IssuedStatus = cd.IssuedStatus,
                                   SyncStatus = cd.SyncStatus,
                                   LastSyncDate = cd.LastSyncDate,
                                   DatetimeModified = cd.DatetimeModified
                               };

                results = joinData.ToList();
            }
            return results;
        }

        /*public List<CDNoteModel> Paging1(CDNoteCriteria criteria, int page, int size, out int rowsCount)
        {
            List<CDNoteModel> results = null;
            List<CDNoteModel> resultDatas = new List<CDNoteModel>();
            var cdNotes = Query(criteria).ToArray().OrderByDescending(o => o.DatetimeModified).ToArray();
            if (cdNotes == null)
            {
                rowsCount = 0;
                return results;
            }

            var charges = surchargeRepository.Get();
            var surchargeDebits = charges.Where(w => !string.IsNullOrEmpty(w.DebitNo) && cdNotes.Any(a => a.Code == w.DebitNo)).ToLookup(x => x.DebitNo);
            var surchargeCredits = charges.Where(w => !string.IsNullOrEmpty(w.CreditNo) && cdNotes.Any(a => a.Code == w.CreditNo)).ToLookup(x => x.CreditNo);
            var partnersLookup = partnerRepositoty.Get().ToLookup(x => x.Id);
            var usersLookup = sysUserRepo.Get().ToLookup(x => x.Id);

            IQueryable<CsShipmentSurcharge> surcharges = null;
            foreach(var cdNote in cdNotes)
            {
                if (cdNote.Type == "CREDIT")
                {
                    surcharges = surchargeCredits[cdNote.Code].AsQueryable();               
                }
                else
                {
                    surcharges = surchargeDebits[cdNote.Code].AsQueryable();
                }

                if (surcharges != null)
                {                    
                    var _partnerName = partnersLookup[cdNote.PartnerId].FirstOrDefault()?.PartnerNameEn;
                    var _creatorCdNote = usersLookup[cdNote.UserCreated].FirstOrDefault()?.Username;
                    
                    var chargeGrps = surcharges.GroupBy(x => new { ReferenceNo = (cdNote.Type == "CREDIT") ? x.CreditNo : x.DebitNo, Currency = x.CurrencyId }).Select(se => new CDNoteModel
                    {
                        Id = cdNote.Id,
                        JobId = cdNote.JobId,
                        PartnerId = cdNote.PartnerId,
                        PartnerName = _partnerName,
                        ReferenceNo = se.Key.ReferenceNo,
                        JobNo = se.FirstOrDefault().JobNo,
                        HBLNo = string.Join("; ", se.Select(s => s.Hblno).Distinct()),
                        Total = se.Sum(x => x.Total),
                        Currency = se.Key.Currency,
                        IssuedDate = cdNote.DatetimeCreated,
                        Creator = _creatorCdNote,
                        Status = se.FirstOrDefault().AcctManagementId != null ? "Issued" : "New",
                        InvoiceNo = se.FirstOrDefault().InvoiceNo,
                        VoucherId = se.FirstOrDefault().VoucherId,
                        IssuedStatus = se.Any(y => !string.IsNullOrEmpty(y.InvoiceNo) && y.AcctManagementId != null) ? "Issued Invoice" : se.Any(y => !string.IsNullOrEmpty(y.VoucherId) && y.AcctManagementId != null) ? "Issued Voucher" : "New",
                        SyncStatus = cdNote.SyncStatus,
                        LastSyncDate = cdNote.LastSyncDate,
                        DatetimeModified = cdNote.DatetimeModified
                    });
                    resultDatas.AddRange(chargeGrps);
                }
            }

            var _resultDatas = GetByStatus(criteria.Status, resultDatas.AsQueryable()).ToArray();

            rowsCount = _resultDatas.ToArray().Count();

            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = _resultDatas.Skip((page - 1) * size).Take(size).ToList();
            }
            return results;
        }*/

        /*public List<CDNoteModel> Paging2(CDNoteCriteria criteria, int page, int size, out int rowsCount)
        {
            List<CDNoteModel> results = null;
            var data = Query(criteria);
            if (data == null) { rowsCount = 0; return results; }
            var cdNotes = Query(criteria)?.ToArray().OrderByDescending(x => x.DatetimeModified).Select(x => new CDNoteModel
            {
                ReferenceNo = x.Code,
                PartnerId = x.PartnerId,
                Id = x.Id,
                IssuedDate = x.DatetimeCreated,
                Creator = x.UserCreated,
                JobId = x.JobId,
                SyncStatus = x.SyncStatus,
                LastSyncDate = x.LastSyncDate
            })?.ToList();

            rowsCount = cdNotes.Count;
            if (rowsCount == 0)
            {
                return results;
            }
            var cdNotesGroupByCurrency = surchargeRepository.Get(x => cdNotes.Any(cd => cd.ReferenceNo == x.DebitNo || cd.ReferenceNo == x.CreditNo))
                .Select(x => new
                {
                    ReferenceNo = string.IsNullOrEmpty(x.DebitNo) ? x.CreditNo : x.DebitNo,
                    HBLNo = x.Hblno,
                    Currency = x.CurrencyId,
                    x.Total,
                    x.VoucherId,
                    x.InvoiceNo,
                    x.Type,
                    x.AcctManagementId
                }).GroupBy(x => new { x.ReferenceNo, x.Currency }).Select(x => new CDNoteModel
                {
                    Currency = x.Key.Currency,
                    ReferenceNo = x.Key.ReferenceNo,
                    HBLNo = string.Join("; ", x.Select(i => i.HBLNo).Distinct()),
                    Total = x.Sum(y => y.Total),
                    Status = x.FirstOrDefault().AcctManagementId != null ? "Issued" : "New",//x.Any(y => !string.IsNullOrEmpty(y.VoucherId) || (!string.IsNullOrEmpty(y.InvoiceNo) && y.Type == "SELL")) ? "Issued" : "New",
                    IssuedStatus = x.Any(y => !string.IsNullOrEmpty(y.InvoiceNo) && y.AcctManagementId != null) ? "Issued Invoice" : x.Any(y => !string.IsNullOrEmpty(y.VoucherId) && y.AcctManagementId != null) ? "Issued Voucher" : "New",
                    VoucherId = x.FirstOrDefault().VoucherId
                });
            cdNotesGroupByCurrency = GetByStatus(criteria.Status, cdNotesGroupByCurrency);

            rowsCount = cdNotesGroupByCurrency.Count();
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                // cdNotesGroupByCurrency = cdNotesGroupByCurrency.Skip((page - 1) * size).Take(size);
                results = GetCDNotes(cdNotes, cdNotesGroupByCurrency);
                results = results.Skip((page - 1) * size).Take(size).ToList();
            }
            return results;
        }*/

        private IQueryable<CDNoteModel> GetByStatus(string status, IQueryable<CDNoteModel> cdNotesGroupByCurrency)
        {
            switch (status)
            {
                case "Issued Invoice":
                    cdNotesGroupByCurrency = cdNotesGroupByCurrency.Where(x => x.IssuedStatus == "Issued Invoice");
                    break;
                case "Issued Voucher":
                    cdNotesGroupByCurrency = cdNotesGroupByCurrency.Where(x => x.IssuedStatus == "Issued Voucher");
                    break;
                case "New":
                    cdNotesGroupByCurrency = cdNotesGroupByCurrency.Where(x => x.Status == "New");
                    break;
            }
            return cdNotesGroupByCurrency;
        }

        /*private List<CDNoteModel> GetCDNotes(List<CDNoteModel> cdNotes, IQueryable<CDNoteModel> cdNotesGroupByCurrency)
        {
            var data = (from cd in cdNotes
                        join charge in cdNotesGroupByCurrency on cd.ReferenceNo equals charge.ReferenceNo
                        join partner in partnerRepositoty.Get() on cd.PartnerId equals partner.Id
                        join creator in sysUserRepo.Get() on cd.Creator equals creator.Id
                        select new CDNoteModel
                        {
                            Id = cd.Id,
                            JobId = cd.JobId,
                            PartnerId = partner.Id,
                            PartnerName = partner.PartnerNameEn,
                            ReferenceNo = cd.ReferenceNo,
                            Total = charge.Total,
                            Currency = charge.Currency,
                            IssuedDate = cd.IssuedDate,
                            Creator = creator.Username,
                            HBLNo = charge.HBLNo,
                            Status = charge.Status,
                            SyncStatus = cd.SyncStatus,
                            LastSyncDate = cd.LastSyncDate,
                            VoucherId = charge.VoucherId
                        })?.AsQueryable();
            var results = new List<CDNoteModel>();
            foreach (var item in data)
            {
                var ops = opstransRepository.Get(op => op.Id == item.JobId).FirstOrDefault();
                if (ops != null)
                {
                    item.JobNo = ops.JobNo;
                }
                else
                {
                    var cs = cstransRepository.Get(trans => trans.Id == item.JobId).FirstOrDefault();
                    if (cs != null)
                    {
                        item.JobNo = cs.JobNo;
                    }
                }
                results.Add(item);
            }
            return results;
        }*/

        public HandleState RejectCreditNote(RejectCreditNoteModel model)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var cdNote = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
                    if (cdNote == null) return new HandleState((object)"Not found Credit Note");

                    cdNote.SyncStatus = "";
                    cdNote.UserModified = currentUser.UserID;
                    cdNote.DatetimeModified = DateTime.Now;
                    cdNote.ReasonReject = model.Reason;
                    cdNote.Note += " Rejected from Accountant";

                    HandleState hs = DataContext.Update(cdNote, x => x.Id == cdNote.Id, false);
                    if (hs.Success)
                    {
                        HandleState smCdNote = DataContext.SubmitChanges();
                        if (smCdNote.Success)
                        {
                            string title = string.Format(@"Accountant Rejected Data CDNote {0}", cdNote.Code);
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
                                ActionLink = GetLinkCdNote(cdNote.Code, cdNote.JobId),
                                UserIds = cdNote.UserCreated
                            };
                            var hsNotifi = sysNotificationRepository.Add(sysNotify);

                            SysUserNotification sysUserNotify = new SysUserNotification
                            {
                                Id = Guid.NewGuid(),
                                UserId = cdNote.UserCreated,
                                Status = "New",
                                NotitficationId = sysNotify.Id,
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                            };
                            var hsUserNotifi = sysUserNotificationRepository.Add(sysUserNotify);

                            //Update PaySyncedFrom or SyncedFrom equal NULL CDNote Code
                            var surcharges = surchargeRepository.Get(x => x.CreditNo == cdNote.Code || x.DebitNo == cdNote.Code);
                            foreach (var surcharge in surcharges)
                            {
                                if (surcharge.Type == "OBH")
                                {
                                    surcharge.PaySyncedFrom = (cdNote.Code == surcharge.CreditNo) ? null : surcharge.PaySyncedFrom;
                                    surcharge.SyncedFrom = (cdNote.Code == surcharge.DebitNo) ? null : surcharge.SyncedFrom;
                                }
                                else
                                {
                                    surcharge.SyncedFrom = null;
                                }
                                surcharge.UserModified = currentUser.UserID;
                                surcharge.DatetimeModified = DateTime.Now;
                                var hsUpdateSurcharge = surchargeRepository.Update(surcharge, x => x.Id == surcharge.Id, false);
                            }
                            var smSurcharge = surchargeRepository.SubmitChanges();
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

        private string GetLinkCdNote(string cdNoteNo, Guid jobId)
        {
            string _link = string.Empty;
            if (cdNoteNo.Contains("CL"))
            {
                _link = string.Format(@"home/operation/job-management/job-edit/{0}?tab=CDNOTE", jobId.ToString());
            }
            else
            {
                string prefixService = "home/documentation/";
                if (cdNoteNo.Contains("IT"))
                {
                    prefixService += "inland-trucking";
                }
                else if (cdNoteNo.Contains("AE"))
                {
                    prefixService += "air-export";
                }
                else if (cdNoteNo.Contains("AI"))
                {
                    prefixService += "air-import";
                }
                else if (cdNoteNo.Contains("SEC"))
                {
                    prefixService += "sea-consol-export";
                }
                else if (cdNoteNo.Contains("SIC"))
                {
                    prefixService += "sea-consol-import";
                }
                else if (cdNoteNo.Contains("SEF"))
                {
                    prefixService += "sea-fcl-export";
                }
                else if (cdNoteNo.Contains("SIF"))
                {
                    prefixService += "sea-fcl-import";
                }
                else if (cdNoteNo.Contains("SEL"))
                {
                    prefixService += "sea-lcl-export";
                }
                else if (cdNoteNo.Contains("SIL"))
                {
                    prefixService += "sea-lcl-import";
                }
                _link = string.Format(@"{0}/{1}?tab=CDNOTE", prefixService, jobId.ToString());
            }
            return _link;
        }
    }
}
