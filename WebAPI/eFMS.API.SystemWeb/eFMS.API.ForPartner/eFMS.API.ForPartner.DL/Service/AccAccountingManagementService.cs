using eFMS.API.ForPartner.Service.Models;
using eFMS.API.ForPartner.DL.Models;
using ITL.NetCore.Connection.BL;
using System;
using ITL.NetCore.Connection.EF;
using eFMS.IdentityServer.DL.UserManager;
using AutoMapper;
using System.Linq;
using ITL.NetCore.Common;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Hosting;
using eFMS.API.Common.Helpers;
using Microsoft.Extensions.Options;
using eFMS.API.Common;
using Newtonsoft.Json;
using eFMS.API.ForPartner.DL.Common;
using eFMS.API.ForPartner.DL.IService;

namespace eFMS.API.ForPartner.DL.Service
{
    public class AccAccountingManagementService : RepositoryBase<AccAccountingManagement, AccAccountingManagementModel>, IAccountingManagementService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysPartnerApi> sysPartnerApiRepository;
        private readonly IHostingEnvironment environment;
        private readonly IOptions<AuthenticationSetting> configSetting;
        private readonly IContextBase<AcctAdvancePayment> acctAdvanceRepository;
        private readonly IContextBase<CsShipmentSurcharge> surchargeRepo;
        private readonly IStringLocalizer stringLocalizer;
        private readonly IContextBase<CatPartner> partnerRepo;

        public AccAccountingManagementService(  
            IContextBase<AccAccountingManagement> repository,
            IContextBase<SysPartnerApi> sysPartnerApiRep,
            IContextBase<AcctAdvancePayment> acctAdvanceRepo,
            IOptions<AuthenticationSetting> config,
            IHostingEnvironment env,
            IMapper mapper,
            ICurrentUser cUser,
            IContextBase<CsShipmentSurcharge> csShipmentSurcharge,
            IStringLocalizer<ForPartnerLanguageSub> localizer,
            IContextBase<CatPartner> catPartner
            ) : base(repository, mapper)
        {
            currentUser = cUser;
            sysPartnerApiRepository = sysPartnerApiRep;
            acctAdvanceRepository = acctAdvanceRepo;
            environment = env;
            configSetting = config;
            surchargeRepo = csShipmentSurcharge;
            stringLocalizer = localizer;
            partnerRepo = catPartner;
        }

        public AccAccountingManagementModel GetById(Guid id)
        {
            AccAccountingManagement accounting = DataContext.Get(x => x.Id == id).FirstOrDefault();
            AccAccountingManagementModel result = mapper.Map<AccAccountingManagementModel>(accounting);
            return result;
        }

        public bool ValidateApiKey(string apiKey)
        {
            bool isValid = false;
            SysPartnerApi partnerAPiInfo = sysPartnerApiRepository.Get(x => x.ApiKey == apiKey).FirstOrDefault();
            if(partnerAPiInfo != null && partnerAPiInfo.Active == true && partnerAPiInfo.Environment.ToLower() == environment.EnvironmentName.ToLower())
            {
                isValid = true;
            }
            return isValid;
        }

        public bool ValidateHashString(object body, string apiKey, string hash)
        {
            bool valid = false;
            if(body != null)
            {
                string bodyString = JsonConvert.SerializeObject(body) + apiKey + configSetting.Value.PartnerShareKey;

                string eFmsHash = Md5Helper.CreateMD5(bodyString);

                if(eFmsHash == hash)
                {
                    valid = true;
                }
                else
                {
                    valid = false;
                }

            }

            return valid;
        }

        public string GenerateHashStringTest(object body, string apiKey)
        {
            object data = body;
            string bodyString = JsonConvert.SerializeObject(data) + apiKey + configSetting.Value.PartnerShareKey;
            return Md5Helper.CreateMD5(bodyString);
        }

        public HandleState UpdateVoucherAdvance(VoucherAdvance model)
        {
            HandleState result = new HandleState();

            return result;
        }

        #region --- CRUD INVOICE ---
        public HandleState CreateInvoice(InvoiceCreateInfo model, string apiKey)
        {
            ICurrentUser _currentUser = setCurrentUserPartner(currentUser, apiKey);
            currentUser.UserID = _currentUser.UserID;
            currentUser.GroupId = _currentUser.GroupId;
            currentUser.DepartmentId = _currentUser.DepartmentId;
            currentUser.OfficeID = _currentUser.OfficeID;
            currentUser.CompanyID = _currentUser.CompanyID;

            try
            {
                var debitCharges = model.Charges.Where(x => x.ChargeType?.ToUpper() == ForPartnerConstants.TYPE_DEBIT).ToList();
                var obhCharges = model.Charges.Where(x => x.ChargeType?.ToUpper() == ForPartnerConstants.TYPE_CHARGE_OBH).ToList();

                AccAccountingManagement invoice = new AccAccountingManagement();
                invoice.Id = Guid.NewGuid();
                var partner = partnerRepo.Get(x => x.AccountNo == model.PartnerCode).FirstOrDefault();
                invoice.PartnerId = partner?.Id; //Find Partner ID by model.PartnerCode;
                invoice.InvoiceNoReal = invoice.InvoiceNoTempt = model.InvoiceNo;
                invoice.Date = model.InvoiceDate;
                invoice.Serie = model.SerieNo;
                invoice.Status = ForPartnerConstants.ACCOUNTING_INVOICE_STATUS_UPDATED; //Set default "Updated Invoice"
                invoice.PaymentStatus = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID; //Set default "Unpaid"
                invoice.Type = ForPartnerConstants.ACCOUNTING_INVOICE_TYPE; //Type is Invoice
                invoice.VoucherId = GenerateVoucherId(invoice.Type, string.Empty); //Auto Gen VoucherId
                invoice.ReferenceNo = debitCharges[0].ReferenceNo; //Cập nhật Reference No cho Invoice
                invoice.UserCreated = invoice.UserModified = currentUser.UserID;
                invoice.DatetimeCreated = invoice.DatetimeModified = DateTime.Now;
                invoice.GroupId = currentUser.GroupId;
                invoice.DepartmentId = currentUser.DepartmentId;
                invoice.OfficeId = currentUser.OfficeID;
                invoice.CompanyId = currentUser.CompanyID;
                
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        HandleState hs = DataContext.Add(invoice, false);
                        if (hs.Success)
                        {
                            //Cập nhật những phí của Invoice
                            foreach(var charge in debitCharges)
                            {
                                CsShipmentSurcharge surcharge = surchargeRepo.Get(x => x.Id == charge.ChargeId).FirstOrDefault();
                                surcharge.AcctManagementId = invoice.Id;
                                surcharge.InvoiceNo = invoice.InvoiceNoReal;
                                surcharge.InvoiceDate = invoice.Date;
                                surcharge.VoucherId = invoice.VoucherId;
                                surcharge.VoucherIddate = invoice.Date;
                                surcharge.FinalExchangeRate = charge.ExchangeRate;
                                surcharge.ReferenceNo = charge.ReferenceNo;
                                surcharge.DatetimeModified = DateTime.Now;
                                surcharge.UserModified = currentUser.UserID;
                                var updateSurcharge = surchargeRepo.Update(surcharge, x => x.Id == surcharge.Id, false);
                            }

                            //Cập nhật số RefNo cho phí OBH
                            foreach(var charge in obhCharges)
                            {
                                CsShipmentSurcharge surcharge = surchargeRepo.Get(x => x.Id == charge.ChargeId).FirstOrDefault();
                                surcharge.FinalExchangeRate = charge.ExchangeRate;
                                surcharge.ReferenceNo = charge.ReferenceNo;
                                surcharge.DatetimeModified = DateTime.Now;
                                surcharge.UserModified = currentUser.UserID;
                                var updateSurcharge = surchargeRepo.Update(surcharge, x => x.Id == surcharge.Id, false);
                            }

                            var smSurcharge = surchargeRepo.SubmitChanges();
                            var sm = DataContext.SubmitChanges();
                            trans.Commit();
                        }
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
                return new HandleState(ex.Message);
            }
        }

        public HandleState ReplaceInvoice(InvoiceUpdateInfo model, string apiKey)
        {
            return new HandleState();
        }

        public HandleState DeleteInvoice(InvoiceInfo model, string apiKey)
        {
            return new HandleState();
        }
        #endregion --- CRUD INVOICE ---

        #region --- PRIVATE METHOD ---
        private SysPartnerApi GetInfoPartnerByApiKey(string apiKey)
        {
            var partnerApi = sysPartnerApiRepository.Get(x => x.ApiKey == apiKey).FirstOrDefault();
            return partnerApi;
        }

        private ICurrentUser setCurrentUserPartner(ICurrentUser currentUser, string apiKey)
        {
            var partnerApi = GetInfoPartnerByApiKey(apiKey);
            currentUser.UserID = (partnerApi != null) ? partnerApi.UserId.ToString() : Guid.Empty.ToString();
            currentUser.GroupId = 0;
            currentUser.DepartmentId = 0;
            currentUser.OfficeID = Guid.Empty;
            currentUser.CompanyID = Guid.Empty;
            return currentUser;
        }

        /// <summary>
        /// Generate Voucher ID
        /// </summary>
        /// <param name="acctMngtType">Invoice or Voucher</param>
        /// <param name="voucherType">Voucher Type of Voucher</param>
        /// <returns></returns>
        private string GenerateVoucherId(string acctMngtType, string voucherType)
        {
            if (string.IsNullOrEmpty(acctMngtType)) return string.Empty;
            int monthCurrent = DateTime.Now.Month;
            string year = DateTime.Now.Year.ToString();
            string month = monthCurrent.ToString().PadLeft(2, '0');//Nếu tháng < 10 thì gắn thêm số 0 phía trước, VD: 09
            string no = "001";

            IQueryable<string> voucherNewests = null;
            string _prefixVoucher = string.Empty;
            if (acctMngtType == ForPartnerConstants.ACCOUNTING_INVOICE_TYPE)
            {
                _prefixVoucher = "FDT";
            }
            else if (acctMngtType == ForPartnerConstants.ACCOUNTING_VOUCHER_TYPE)
            {
                if (string.IsNullOrEmpty(voucherType)) return string.Empty;
                _prefixVoucher = GetPrefixVoucherByVoucherType(voucherType);
            }
            voucherNewests = Get(x => x.Type == acctMngtType && x.VoucherId.Contains(_prefixVoucher) && x.VoucherId.Substring(0, 4) == year && x.VoucherId.Substring(11, 2) == month)
                .OrderByDescending(o => o.VoucherId).Select(s => s.VoucherId);

            string voucherNewest = voucherNewests.FirstOrDefault();
            if (!string.IsNullOrEmpty(voucherNewest))
            {
                var _noNewest = voucherNewest.Substring(7, 3);
                if (_noNewest != "999")
                {
                    no = (Convert.ToInt32(_noNewest) + 1).ToString();
                    no = no.PadLeft(3, '0');
                }
            }
            string voucher = year + _prefixVoucher + no + "/" + month;
            return voucher;
        }

        private string GetPrefixVoucherByVoucherType(string voucherType)
        {
            string _prefixVoucher = string.Empty;
            if (string.IsNullOrEmpty(voucherType)) return _prefixVoucher;
            switch (voucherType.Trim().ToUpper())
            {
                case "CASH RECEIPT":
                    _prefixVoucher = "FPT";
                    break;
                case "CASH PAYMENT":
                    _prefixVoucher = "FPC";
                    break;
                case "DEBIT SLIP":
                    _prefixVoucher = "FBN";
                    break;
                case "CREDIT SLIP":
                    _prefixVoucher = "FBC";
                    break;
                case "PURCHASING NOTE":
                    _prefixVoucher = "FNM";
                    break;
                case "OTHER ENTRY":
                    _prefixVoucher = "FPK";
                    break;
                default:
                    _prefixVoucher = string.Empty;
                    break;
            }
            return _prefixVoucher;
        }

        #endregion --- PRIVATE METHOD ---
    }
}
