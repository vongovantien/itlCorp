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

        public HandleState CreateInvoice(InvoiceCreateInfo model)
        {
            try
            {
                AccAccountingManagementModel invoiceModel = new AccAccountingManagementModel();
                invoiceModel.Id = Guid.NewGuid();
                var partner = partnerRepo.Get(x => x.AccountNo == model.PartnerCode).FirstOrDefault();
                invoiceModel.PartnerId = partner?.Id;//Find Partner ID by model.PartnerCode;
                invoiceModel.InvoiceNoReal = invoiceModel.InvoiceNoTempt = model.InvoiceNo;
                invoiceModel.Date = model.InvoiceDate;
                invoiceModel.Serie = model.SerieNo;
                invoiceModel.Status = "Updated Invoice";
                invoiceModel.PaymentStatus = "Unpaid";

                invoiceModel.UserCreated = invoiceModel.UserModified = null; //Chờ Update theo API Key của User Bravo
                invoiceModel.DatetimeCreated = invoiceModel.DatetimeModified = DateTime.Now;
                invoiceModel.GroupId = null;
                invoiceModel.DepartmentId = null;
                invoiceModel.OfficeId = null;
                invoiceModel.CompanyId = null;
                
                AccAccountingManagement invoice = mapper.Map<AccAccountingManagement>(invoiceModel);

                var debitCharges = model.Charges.Where(x => x.ChargeType != "OBH");
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        HandleState hs = DataContext.Add(invoice, false);
                        if (hs.Success)
                        {
                            foreach(var charge in debitCharges)
                            {
                                CsShipmentSurcharge surcharge = surchargeRepo.Get(x => x.Id == charge.ChargeId).FirstOrDefault();
                                surcharge.AcctManagementId = invoice.Id;
                                surcharge.FinalExchangeRate = charge.ExchangeRate;
                                surcharge.ReferenceNo = charge.ReferenceNo;
                                surcharge.DatetimeModified = DateTime.Now;
                                //surcharge.UserModified = null; //Chờ Update theo API Key của User Bravo
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

        public HandleState ReplaceInvoice(InvoiceUpdateInfo model)
        {
            return new HandleState();
        }
    }
}
