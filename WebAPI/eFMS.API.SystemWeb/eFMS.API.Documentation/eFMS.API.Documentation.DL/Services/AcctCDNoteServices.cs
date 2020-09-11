using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Models.ReportResults;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
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

        //IOpsTransactionService opsTransactionService;
        private readonly ICurrencyExchangeService currencyExchangeService;

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
            //IOpsTransactionService opsTransService
            ICurrencyExchangeService currencyExchange
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
            //opsTransactionService = opsTransService;
            currencyExchangeService = currencyExchange;
            customsDeclarationRepository = customsDeclarationRepo;
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
                    currentCdNote = currentCdNotes.Where(x => x.Code.StartsWith("HAN")).FirstOrDefault();
                }
                else if (office.Code == "ITLDAD")
                {
                    currentCdNote = currentCdNotes.Where(x => x.Code.StartsWith("DAD")).FirstOrDefault();
                }
                else
                {
                    currentCdNote = currentCdNotes.Where(x => !x.Code.StartsWith("DAD")
                                                           && !x.Code.StartsWith("HAN")).FirstOrDefault();
                }
            }
            else
            {
                currentCdNote = currentCdNotes.Where(x => !x.Code.StartsWith("DAD")
                                                       && !x.Code.StartsWith("HAN")).FirstOrDefault();
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
                    prefixCode = "HAN-";
                }
                else if (officeCode == "ITLDAD")
                {
                    prefixCode = "DAD-";
                }
            }
            return prefixCode;
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
                var hs = DataContext.Add(model, false);

                if (hs.Success)
                {
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
                            charge.ExchangeDate = model.DatetimeCreated;//Cập nhật Exchange Date equal Created Date CD Note
                            charge.DatetimeModified = DateTime.Now;
                            charge.UserModified = currentUser.UserID;
                        }
                        var hsSurcharge = surchargeRepository.Update(charge, x => x.Id == charge.Id, false);
                    }
                }
                UpdateJobModifyTime(model.JobId);
                var sc = DataContext.SubmitChanges();
                if (sc.Success)
                {
                    var hsSc = surchargeRepository.SubmitChanges();
                    var hsOt = opstransRepository.SubmitChanges();
                    var hsCt = cstransRepository.SubmitChanges();
                }
                return sc;
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
                entity.GroupId = model.GroupId;
                entity.DepartmentId = model.DepartmentId;
                entity.OfficeId = model.OfficeId;
                entity.CompanyId = model.CompanyId;
                var stt = DataContext.Update(entity, x => x.Id == cdNote.Id, false);
                if (stt.Success)
                {
                    var chargeOfCdNote = surchargeRepository.Get(x => x.CreditNo == cdNote.Code || x.DebitNo == cdNote.Code);
                    //Cập nhật các credit debit note code của của các charge thành null
                    foreach (var item in chargeOfCdNote)
                    {
                        item.DatetimeModified = DateTime.Now;
                        item.UserModified = currentUser.UserID;
                        item.CreditNo = item.DebitNo = null;
                        var hsSur = surchargeRepository.Update(item, x => x.Id == item.Id, false);
                    }

                    //Cập nhật lại cd note code cho các charge cũ
                    foreach (var item in model.listShipmentSurcharge.Where(x => !string.IsNullOrEmpty(x.CreditNo) || !string.IsNullOrEmpty(x.DebitNo)))
                    {
                        item.Cdclosed = true;
                        item.DatetimeModified = DateTime.Now;
                        item.UserModified = currentUser.UserID; // need update in the future 
                        item.ExchangeDate = model.DatetimeCreated;//Cập nhật Exchange Date equal Created Date CD Note
                        var hsSur = surchargeRepository.Update(item, x => x.Id == item.Id, false);
                    }

                    //Thêm mới cd note code cho các charge mới add
                    foreach (var c in model.listShipmentSurcharge.Where(x => string.IsNullOrEmpty(x.CreditNo) && string.IsNullOrEmpty(x.DebitNo)))
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
                            charge.ExchangeDate = model.DatetimeCreated;//Cập nhật Exchange Date equal Created Date CD Note
                            charge.DatetimeModified = DateTime.Now;
                            charge.UserModified = currentUser.UserID;
                        }
                        var hsSurcharge = surchargeRepository.Update(charge, x => x.Id == charge.Id, false);
                    }

                    UpdateJobModifyTime(model.Id);
                }
                var hsSc = DataContext.SubmitChanges();
                var hsSurSc = surchargeRepository.SubmitChanges();
                var hsOtSc = opstransRepository.SubmitChanges();
                var hsCtSc = cstransRepository.SubmitChanges();

                return hsSc;
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
                    //List<CsTransactionDetail> housebills = trandetailRepositoty.Get(x => x.JobId == id).ToList();
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
                    //PermissionRange rangeSearch = PermissionEx.GetPermissionRange(currentUser.UserMenuPermission.List);
                    //var shipments = opsTransactionService.QueryByPermission(rangeSearch);
                    //var hblid = shipments.Where(x => x.Id == id).FirstOrDefault()?.Hblid;

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
                        // -to continue
                        var chargesOfCDNote = listCharges.Where(x => x.CreditNo == cdNote.Code || x.DebitNo == cdNote.Code);
                        //decimal totalDebit = 0;
                        //decimal totalCredit = 0;
                        foreach (var charge in chargesOfCDNote)
                        {
                            /*if (charge.Type == DocumentConstants.CHARGE_BUY_TYPE || (charge.Type == DocumentConstants.CHARGE_OBH_TYPE && cdNote.PartnerId == charge.PayerId))
                            {
                                // calculate total credit
                                totalCredit += (decimal)(charge.Total * charge.ExchangeRate);
                            }
                            if (charge.Type == DocumentConstants.CHARGE_SELL_TYPE || (charge.Type == DocumentConstants.CHARGE_OBH_TYPE && cdNote.PartnerId == charge.PaymentObjectId))
                            {
                                // calculate total debit 
                                totalDebit += (decimal)(charge.Total * charge.ExchangeRate);
                            }*/
                        }
                        // cdNote.Total = Math.Abs(totalDebit - totalCredit);
                        cdNote.soaNo = String.Join(", ", chargesOfCDNote.Select(x => !string.IsNullOrEmpty(x.Soano) ? x.Soano : x.PaySoano).Distinct());
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

        public AcctCDNoteDetailsModel GetCDNoteDetails(Guid JobId, string cdNo)
        {
            var places = placeRepository.Get();
            AcctCDNoteDetailsModel soaDetails = new AcctCDNoteDetailsModel();
            var cdNote = DataContext.Where(x => x.Code == cdNo).FirstOrDefault();
            if (cdNote == null) return soaDetails;
            var partner = partnerRepositoty.Get(x => x.Id == cdNote.PartnerId).FirstOrDefault();

            CatPlace pol = new CatPlace();
            CatPlace pod = new CatPlace();

            var transaction = cstransRepository.Get(x => x.Id == JobId).FirstOrDefault();
            var opsTransaction = opstransRepository.Get(x => x.Id == JobId).FirstOrDefault();
            //string warehouseId = null;
            if (transaction != null)
            {
                pol = places.FirstOrDefault(x => x.Id == transaction.Pol);
                pod = places.FirstOrDefault(x => x.Id == transaction.Pod);
                //warehouseId = transaction.WareHouseId ?? null;
            }
            else
            {
                if (opsTransaction != null)
                {
                    pol = places.FirstOrDefault(x => x.Id == opsTransaction.Pol);
                    pod = places.FirstOrDefault(x => x.Id == opsTransaction.Pod);
                }
                //warehouseId = opsTransaction.WarehouseId?.ToString();
            }
            if ((transaction == null && opsTransaction == null) || cdNote == null || partner == null)
            {
                return null;
            }
            //to continue
            var charges = surchargeRepository.Get(x => x.CreditNo == cdNo || x.DebitNo == cdNo).ToList();

            List<CsTransactionDetail> HBList = new List<CsTransactionDetail>();
            List<CsShipmentSurchargeDetailsModel> listSurcharges = new List<CsShipmentSurchargeDetailsModel>();

            soaDetails.CreatedDate = ((DateTime)cdNote.DatetimeCreated).ToString("dd'/'MM'/'yyyy");
            foreach (var item in charges)
            {
                var charge = mapper.Map<CsShipmentSurchargeDetailsModel>(item);
                var hb = trandetailRepositoty.Get(x => x.Id == item.Hblid).FirstOrDefault();
                var catCharge = catchargeRepository.Get(x => x.Id == charge.ChargeId).FirstOrDefault();

                //Quy đổi theo Final Exchange Rate. Nếu Final Exchange Rate is null thì
                //Check ExchangeDate # null: nếu bằng null thì gán ngày hiện tại.
                //var exchargeDateSurcharge = item.ExchangeDate == null ? DateTime.Now : item.ExchangeDate;
                //var exchangeRate = catCurrencyExchangeRepository.Get(x => (x.DatetimeCreated.Value.Date == exchargeDateSurcharge.Value.Date && x.CurrencyFromId == item.CurrencyId && x.CurrencyToId == DocumentConstants.CURRENCY_LOCAL)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                decimal _exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.CurrencyId, DocumentConstants.CURRENCY_LOCAL);
                charge.Currency = currencyRepository.Get(x => x.Id == charge.CurrencyId).FirstOrDefault()?.CurrencyName;
                charge.ExchangeRate = _exchangeRate;
                charge.Hwbno = hb != null ? hb.Hwbno : opsTransaction?.Hwbno;
                var unit = unitRepository.Get(x => x.Id == charge.UnitId).FirstOrDefault();
                charge.Unit = unit?.UnitNameEn;
                charge.UnitCode = unit?.Code;
                charge.ChargeCode = catCharge?.Code;
                charge.NameEn = catCharge?.ChargeNameEn;
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
                soaDetails.MbLadingNo = transaction?.Mawb;
                hbOfLadingNo = string.Join(", ", HBList.Select(x => x.Hwbno).Distinct());
                soaDetails.HbLadingNo = hbOfLadingNo;
            }
            else
            {
                soaDetails.CBM = opsTransaction.SumCbm;
                soaDetails.GW = opsTransaction.SumGrossWeight;
                soaDetails.ServiceDate = opsTransaction.ServiceDate;
                soaDetails.HbLadingNo = opsTransaction?.Hwbno;
                soaDetails.MbLadingNo = opsTransaction?.Mblno;
                soaDetails.SumContainers = opsTransaction?.SumContainers;
                soaDetails.SumPackages = opsTransaction?.SumPackages;
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
                    volum += conts.Sum(s => s.Cbm);
                    hbGw += conts.Sum(s => s.Gw);
                    hbCw += conts.Sum(s => s.ChargeAbleWeight);
                }
                else
                {
                    volum += item.Cbm;
                    hbGw += item.GrossWeight;
                    hbCw += item.ChargeWeight;
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

            hbShippers = String.Join(", ", partnerRepositoty.Get(x => HBList.Select(s => s.ShipperId).Contains(x.Id)).Select(s => s.PartnerNameEn).Distinct().ToList());
            hbConsignees = String.Join(", ", partnerRepositoty.Get(x => HBList.Select(s => s.ConsigneeId).Contains(x.Id)).Select(s => s.PartnerNameEn).Distinct().ToList());

            var countries = countryRepository.Get().ToList();
            soaDetails.PartnerNameEn = partner?.PartnerNameEn;
            soaDetails.PartnerShippingAddress = partner?.AddressEn; //Billing Address Name En
            soaDetails.PartnerTel = partner?.Tel;
            soaDetails.PartnerTaxcode = partner?.TaxCode;
            soaDetails.PartnerId = partner?.Id;
            soaDetails.PartnerFax = partner?.Fax;
            soaDetails.PartnerPersonalContact = partner.ContactPerson;
            soaDetails.JobId = transaction != null ? transaction.Id : opsTransaction.Id;
            soaDetails.JobNo = transaction != null ? transaction.JobNo : opsTransaction?.JobNo;
            soaDetails.Pol = pol?.NameEn;
            if (soaDetails.Pol != null)
            {
                soaDetails.PolCountry = pol == null ? null : countries.FirstOrDefault(x => x.Id == pol.CountryId)?.NameEn;
            }
            soaDetails.Pod = pod?.NameEn;
            if (soaDetails.Pod != null)
            {
                soaDetails.PodCountry = pod == null ? null : countries.FirstOrDefault(x => x.Id == pod.CountryId)?.NameEn;
            }
            soaDetails.Vessel = transaction != null ? transaction.FlightVesselName : opsTransaction.FlightVessel;
            soaDetails.VesselDate = transaction != null ? transaction.FlightDate : null;
            soaDetails.HbConstainers = hbConstainers; //Container Quantity
            soaDetails.HbPackages = hbPackages; // Package Quantity
            soaDetails.Etd = transaction != null ? transaction.Etd : opsTransaction.ServiceDate;
            soaDetails.Eta = transaction != null ? transaction.Eta : opsTransaction.FinishDate;
            soaDetails.IsLocked = false;
            soaDetails.Volum = volum;
            soaDetails.ListSurcharges = listSurcharges;
            soaDetails.CDNote = cdNote;
            soaDetails.ProductService = opsTransaction?.ProductService;
            soaDetails.ServiceMode = opsTransaction?.ServiceMode;
            soaDetails.SoaNo = String.Join(", ", charges.Select(x => !string.IsNullOrEmpty(x.Soano) ? x.Soano : x.PaySoano).Distinct()); ;
            soaDetails.HbSealNo = sealsContsNo;//SealNo/ContNo
            soaDetails.HbGrossweight = hbGw;
            soaDetails.HbShippers = hbShippers; //Shipper
            soaDetails.HbConsignees = hbConsignees; //Consignee
            soaDetails.HbChargeWeight = hbCw;
            soaDetails.FlexId = cdNote.FlexId;
            return soaDetails;
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

        public Crystal Preview(AcctCDNoteDetailsModel model)
        {
            if (model == null)
            {
                return null;
            }
            Crystal result = null;
            //Lấy thông tin Office của User Login
            var officeOfUser = GetInfoOfficeOfUser(currentUser.OfficeID);
            var _accountName = officeOfUser?.BankAccountNameVn ?? string.Empty;
            var _accountNameEN = officeOfUser?.BankAccountNameEn ?? string.Empty;
            var _bankName = officeOfUser?.BankNameLocal ?? string.Empty;
            var _bankNameEN = officeOfUser?.BankNameEn ?? string.Empty;
            var _bankAddress = officeOfUser?.BankAddressLocal ?? string.Empty;
            var _bankAddressEN = officeOfUser?.BankAddressEn ?? string.Empty;
            var _swiftAccs = officeOfUser?.SwiftCode ?? string.Empty;
            var _accsUsd = officeOfUser?.BankAccountUsd ?? string.Empty;
            var _accsVnd = officeOfUser?.BankAccountVnd ?? string.Empty;

            IQueryable<CustomsDeclaration> _customClearances = customsDeclarationRepository.Get(x => x.JobNo == model.JobNo);
            CustomsDeclaration _clearance = null;
            if (_customClearances.Count() > 0 || _customClearances != null)
            {
                var orderClearance = _customClearances.OrderBy(x => x.ClearanceDate);
                _clearance = orderClearance.FirstOrDefault();

            }

            var parameter = new AcctSOAReportParams
            {
                DBTitle = "N/A",
                DebitNo = model.CDNote.Code,
                TotalDebit = model.TotalDebit == null ? string.Empty : model.TotalDebit.ToString(),
                TotalCredit = model.TotalCredit == null ? string.Empty : model.TotalCredit.ToString(),
                DueToTitle = "N/A",
                DueTo = "N/A",
                DueToCredit = "N/A",
                SayWordAll = "N/A",
                CompanyName = DocumentConstants.COMPANY_NAME,
                CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1,
                CompanyAddress2 = "Tel‎: (‎84‎-‎8‎) ‎3948 6888  Fax‎: +‎84 8 38488 570‎",
                CompanyDescription = "N/A",
                Website = DocumentConstants.COMPANY_WEBSITE,//"efms.itlvn.com",
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
                OtherRef = "N/A"
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
                        Commodity = "N/A",
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
                        Quantity = item.Quantity,
                        QUnit = "N/A",
                        UnitPrice = item.UnitPrice,
                        VAT = null,
                        Debit = model.TotalDebit,
                        Credit = model.TotalCredit,
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
                        NW = null,
                        SeaCBM = model.CBM,
                        SOTK = _clearance?.ClearanceNo,
                        NgayDK = null,
                        Cuakhau = port,
                        DeliveryPlace = model.WarehouseName?.ToUpper(),
                        TransDate = null,
                        Unit = item.CurrencyId,
                        UnitPieaces = "N/A",
                        CustomDate = _clearance?.ClearanceDate
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
                                                  || (x.DebitNo == cdNote.Code && !string.IsNullOrEmpty(x.Soano)));
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
        public Crystal PreviewSIF(PreviewCdNoteCriteria criteria)
        {
            Crystal result = null;
            var _currentUser = currentUser.UserName;

            var listCharge = new List<SeaDebitAgentsNewReport>();
            var data = GetCDNoteDetails(criteria.JobId, criteria.CreditDebitNo);
            if (data != null)
            {
                //Loop qua ds charge từ detail cdnote
                foreach (var item in data.ListSurcharges)
                {
                    var exchargeDateSurcharge = item.ExchangeDate == null ? DateTime.Now : item.ExchangeDate;
                    //Exchange Rate theo Currency truyền vào
                    decimal _exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.CurrencyId, criteria.Currency);

                    var charge = new SeaDebitAgentsNewReport();
                    //Thông tin Partner
                    var partner = partnerRepositoty.Get(x => x.Id == data.PartnerId).FirstOrDefault();
                    charge.PartnerID = data.PartnerId?.ToUpper();
                    charge.PartnerName = data.PartnerNameEn?.ToUpper();
                    charge.Address = (DocumentConstants.CURRENCY_LOCAL == criteria.Currency && data.CDNote.Type == "DEBIT") ? partner.AddressVn : partner.AddressEn;//Lấy địa chỉ Billing
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
                    charge.Subject = "LOCAL CHARGES";
                    charge.Description = item.NameEn;//Charge name
                    charge.Quantity = item.Quantity;
                    charge.Unit = item.UnitCode; //Unit Code
                    charge.QUnit = criteria.Currency;
                    charge.UnitPrice = (item.UnitPrice != null ? item.UnitPrice : 0) * _exchangeRate; //Unit Price đã được Exchange Rate theo Currency
                    charge.VAT = item.Vatrate != null ? item.Vatrate : 0;
                    var _credit = (item.Type == DocumentConstants.CHARGE_BUY_TYPE || (item.Type == DocumentConstants.CHARGE_OBH_TYPE && data.PartnerId == item.PayerId)) ? item.Total * _exchangeRate : 0;
                    charge.Credit = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? Math.Round(_credit) : Math.Round(_credit, 3);
                    var _debit = (item.Type == DocumentConstants.CHARGE_SELL_TYPE || (item.Type == DocumentConstants.CHARGE_OBH_TYPE && data.PartnerId == item.PaymentObjectId)) ? item.Total * _exchangeRate : 0;
                    charge.Debit = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? Math.Round(_debit) : Math.Round(_debit, 3);
                    listCharge.Add(charge);
                }
            }
            var parameter = new SeaDebitAgentsNewReportParams();
            parameter.CompanyName = DocumentConstants.COMPANY_NAME;
            parameter.CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1;
            parameter.CompanyAddress2 = DocumentConstants.COMPANY_CONTACT;
            parameter.Website = DocumentConstants.COMPANY_WEBSITE;
            parameter.Contact = _currentUser;//Get user name login
            parameter.CompanyDescription = string.Empty;

            parameter.DebitNo = criteria.CreditDebitNo;
            parameter.IssuedDate = data != null && data.CDNote != null && data.CDNote.DatetimeCreated != null ? data.CDNote.DatetimeCreated.Value.ToString("dd MMM, yyyy") : string.Empty;//Lấy ngày tạo CDNote
            parameter.DBTitle = data.CDNote.Type == "CREDIT" ? "CREDIT NOTE" : data.CDNote.Type == "DEBIT" ? "DEBIT NOTE" : "INVOICE";
            parameter.ReviseNotice = "Revised: " + DateTime.Now.ToString("dd/MM/yyyy");

            var _totalDebit = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? Math.Round(listCharge.Sum(x => x.Debit).Value) : Math.Round(listCharge.Sum(x => x.Debit).Value, 3);
            var _totalCredit = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? Math.Round(listCharge.Sum(x => x.Credit).Value) : Math.Round(listCharge.Sum(x => x.Credit).Value, 3);
            parameter.TotalDebit = string.Empty;
            if (_totalDebit != 0)
            {
                parameter.TotalDebit = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? String.Format("{0:n0}", _totalDebit) : String.Format("{0:n3}", _totalDebit);
            }

            parameter.TotalCredit = string.Empty;
            if (_totalCredit != 0)
            {
                parameter.TotalCredit = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? String.Format("{0:n0}", _totalCredit) : String.Format("{0:n3}", _totalCredit);
            }

            var _blAmount = Math.Abs(_totalDebit - _totalCredit);
            parameter.BalanceAmount = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? String.Format("{0:n0}", _blAmount) : String.Format("{0:n3}", _blAmount);

            //Chuyển tiền Amount thành chữ
            decimal _balanceAmount = Math.Round(_blAmount, 3);
            var _inword = string.Empty;
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
            parameter.InvoiceInfo = string.Empty;//Tạm thời để trống
            parameter.OtherRef = string.Empty;//Tạm thời để trống

            //Lấy thông tin Office của User Login
            var officeOfUser = GetInfoOfficeOfUser(currentUser.OfficeID);
            var _accountName = officeOfUser?.BankAccountNameVn ?? string.Empty;
            var _accountNameEN = officeOfUser?.BankAccountNameEn ?? string.Empty;
            var _bankName = officeOfUser?.BankNameLocal ?? string.Empty;
            var _bankNameEN = officeOfUser?.BankNameEn ?? string.Empty;
            var _bankAddress = officeOfUser?.BankAddressLocal ?? string.Empty;
            var _bankAddressEN = officeOfUser?.BankAddressEn ?? string.Empty;
            var _swiftAccs = officeOfUser?.SwiftCode ?? string.Empty;
            var _accsUsd = officeOfUser?.BankAccountUsd ?? string.Empty;
            var _accsVnd = officeOfUser?.BankAccountVnd ?? string.Empty;

            //Thông tin Bank
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
                ReportName = "SeaDebitAgentsNewVND.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listCharge);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }

        public Crystal PreviewAir(PreviewCdNoteCriteria criteria)
        {
            Crystal result = null;
            var _currentUser = currentUser.UserName;
            var listCharge = new List<AirShipperDebitNewReport>();
            var data = GetCDNoteDetails(criteria.JobId, criteria.CreditDebitNo);

            string _hbllist = string.Empty;
            if (data != null)
            {
                _hbllist = string.Join("\r\n", data.ListSurcharges.Select(s => s.Hwbno).Distinct());
                int i = 1;
                foreach (var item in data.ListSurcharges)
                {
                    var exchargeDateSurcharge = item.ExchangeDate == null ? DateTime.Now : item.ExchangeDate;
                    //Exchange Rate theo Currency truyền vào
                    //var exchangeRate = catCurrencyExchangeRepository.Get(x => (x.DatetimeCreated.Value.Date == exchargeDateSurcharge.Value.Date && x.CurrencyFromId == item.CurrencyId && x.CurrencyToId == criteria.Currency)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                    decimal _exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.CurrencyId, criteria.Currency);
                    /*if ((exchangeRate != null && exchangeRate.Rate != 0))
                    {
                        _exchangeRate = exchangeRate.Rate;
                    }
                    else
                    {
                        //Exchange Rate ngược
                        var exchangeRateReverse = catCurrencyExchangeRepository.Get(x => (x.DatetimeCreated.Value.Date == exchargeDateSurcharge.Value.Date && x.CurrencyFromId == criteria.Currency && x.CurrencyToId == item.CurrencyId)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                        _exchangeRate = (exchangeRateReverse != null && exchangeRateReverse.Rate != 0) ? 1 / exchangeRateReverse.Rate : 1;
                    }*/

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

                    //Thông tin list charge
                    charge.Subject = "FREIGHT CHARGES";
                    charge.Description = item.NameEn;//Charge name
                    charge.Quantity = item.Quantity;
                    charge.Unit = item.Unit;
                    charge.QUnit = criteria.Currency;
                    charge.UnitPrice = (item.UnitPrice != null ? item.UnitPrice : 0) * _exchangeRate; //Unit Price đã được Exchange Rate theo Currency
                    charge.VAT = item.Vatrate != null ? item.Vatrate : 0;
                    var _credit = (item.Type == DocumentConstants.CHARGE_BUY_TYPE || (item.Type == DocumentConstants.CHARGE_OBH_TYPE && data.PartnerId == item.PayerId)) ? item.Total * _exchangeRate : 0;
                    charge.Credit = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? Math.Round(_credit) : Math.Round(_credit, 3);
                    var _debit = (item.Type == DocumentConstants.CHARGE_SELL_TYPE || (item.Type == DocumentConstants.CHARGE_OBH_TYPE && data.PartnerId == item.PaymentObjectId)) ? item.Total * _exchangeRate : 0;
                    charge.Debit = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? Math.Round(_debit) : Math.Round(_debit, 3);
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
            parameter.CompanyName = DocumentConstants.COMPANY_NAME;
            parameter.CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1;
            parameter.CompanyAddress2 = DocumentConstants.COMPANY_CONTACT;
            parameter.Website = DocumentConstants.COMPANY_WEBSITE;
            parameter.Contact = _currentUser;//Get user name login
            parameter.CompanyDescription = string.Empty;

            parameter.DebitNo = criteria.CreditDebitNo;
            parameter.IssuedDate = data != null && data.CDNote != null && data.CDNote.DatetimeCreated != null ? data.CDNote.DatetimeCreated.Value.ToString("dd/MM/yyyy") : string.Empty;//Lấy ngày tạo CDNote
            parameter.DBTitle = data.CDNote.Type == "CREDIT" ? "CREDIT NOTE" : data.CDNote.Type == "DEBIT" ? "DEBIT NOTE" : "INVOICE";
            parameter.ReviseNotice = "Revised: " + DateTime.Now.ToString("dd/MM/yyyy");

            var _totalDebit = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? Math.Round(listCharge.Sum(x => x.Debit).Value) : Math.Round(listCharge.Sum(x => x.Debit).Value, 3);
            var _totalCredit = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? Math.Round(listCharge.Sum(x => x.Credit).Value) : Math.Round(listCharge.Sum(x => x.Credit).Value, 3);
            parameter.TotalDebit = string.Empty;
            if (_totalDebit != 0)
            {
                parameter.TotalDebit = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? String.Format("{0:n0}", _totalDebit) : String.Format("{0:n3}", _totalDebit);
            }

            parameter.TotalCredit = string.Empty;
            if (_totalCredit != 0)
            {
                parameter.TotalCredit = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? String.Format("{0:n0}", _totalCredit) : String.Format("{0:n3}", _totalCredit);
            }

            decimal _balanceAmount = Math.Abs(_totalDebit - _totalCredit);
            parameter.BalanceAmount = (criteria.Currency == DocumentConstants.CURRENCY_LOCAL) ? String.Format("{0:n0}", _balanceAmount) : String.Format("{0:n3}", _balanceAmount);

            //Chuyển tiền Amount thành chữ
            _balanceAmount = Math.Round(_balanceAmount, 3);
            var _inword = string.Empty;
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
            parameter.InvoiceInfo = string.Empty;//Tạm thời để trống
            parameter.OtherRef = string.Empty;//Tạm thời để trống

            //Lấy thông tin Office của User Login
            var officeOfUser = GetInfoOfficeOfUser(currentUser.OfficeID);
            var _accountName = officeOfUser?.BankAccountNameVn ?? string.Empty;
            var _accountNameEN = officeOfUser?.BankAccountNameEn ?? string.Empty;
            var _bankName = officeOfUser?.BankNameLocal ?? string.Empty;
            var _bankNameEN = officeOfUser?.BankNameEn ?? string.Empty;
            var _bankAddress = officeOfUser?.BankAddressLocal ?? string.Empty;
            var _bankAddressEN = officeOfUser?.BankAddressEn ?? string.Empty;
            var _swiftAccs = officeOfUser?.SwiftCode ?? string.Empty;
            var _accsUsd = officeOfUser?.BankAccountUsd ?? string.Empty;
            var _accsVnd = officeOfUser?.BankAccountVnd ?? string.Empty;

            //Thông tin Bank
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
            parameter.HBLList = _hbllist?.ToUpper();
            parameter.DecimalNo = 3;
            //Exchange Rate USD to VND
            var _exchangeRateUSDToVND = catCurrencyExchangeRepository.Get(x => (x.DatetimeCreated.Value.Date == DateTime.Now.Date && x.CurrencyFromId == DocumentConstants.CURRENCY_USD && x.CurrencyToId == DocumentConstants.CURRENCY_LOCAL)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
            parameter.RateUSDToVND = _exchangeRateUSDToVND != null ? _exchangeRateUSDToVND.Rate : 0;

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

        private SysOffice GetInfoOfficeOfUser(Guid officeId)
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
            var results = DataContext.Get(query);
            if (results == null) return results;
            if (!string.IsNullOrEmpty(criteria.ReferenceNos))
            {
                IEnumerable<string> refNos = criteria.ReferenceNos.Split('\n').Select(x => x.Trim()).Where(x => x != null);
                results = results.Where(x => refNos.Contains(x.Code));
            }
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
            var data = Query(criteria);
            if (data == null) { rowsCount = 0; return results; }
            var cdNotes = Query(criteria)?.OrderByDescending(x => x.DatetimeModified).Select(x => new CDNoteModel
            {
                ReferenceNo = x.Code,
                PartnerId = x.PartnerId,
                Id = x.Id,
                IssuedDate = x.DatetimeCreated,
                Creator = x.UserCreated,
                JobId = x.JobId,
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
                    x.Type
                }).GroupBy(x => new { x.ReferenceNo, x.Currency }).Select(x => new CDNoteModel
                {
                    Currency = x.Key.Currency,
                    ReferenceNo = x.Key.ReferenceNo,
                    HBLNo = string.Join(";", x.Select(i => i.HBLNo)),
                    Total = x.Sum(y => y.Total),
                    Status = x.Any(y => !string.IsNullOrEmpty(y.VoucherId) || (!string.IsNullOrEmpty(y.InvoiceNo) && y.Type == "SELL")) ? "Issued" : "New",
                    IssuedStatus = x.Any(y => !string.IsNullOrEmpty(y.InvoiceNo)) ? "Issued Invoice" : x.Any(y => !string.IsNullOrEmpty(y.VoucherId)) ? "Issued Voucher" : "New"
                });
            cdNotesGroupByCurrency = GetByStatus(criteria.Status, cdNotesGroupByCurrency);

            rowsCount = cdNotesGroupByCurrency.Count();
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                cdNotesGroupByCurrency = cdNotesGroupByCurrency.Skip((page - 1) * size).Take(size);
                results = GetCDNotes(cdNotes, cdNotesGroupByCurrency);
            }
            return results;
        }

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

        private List<CDNoteModel> GetCDNotes(List<CDNoteModel> cdNotes, IQueryable<CDNoteModel> cdNotesGroupByCurrency)
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
                            Status = charge.Status
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
        }
    }
}
