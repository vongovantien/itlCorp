using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.Caching;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using eFMS.API.Common;
using eFMS.API.Infrastructure.Extensions;
using eFMS.API.Common.Models;
using System.Text;
using System.Text.RegularExpressions;
using eFMS.API.Common.Helpers;
using AutoMapper.QueryableExtensions;
using eFMS.API.Catalogue.Service.Contexts;
using ITL.NetCore.Connection;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatPartnerService : RepositoryBaseCache<CatPartner, CatPartnerModel>, ICatPartnerService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUser> sysUserRepository;

        private readonly IContextBase<CatContract> contractRepository;
        private readonly ICatPlaceService placeService;
        private readonly ICatCountryService countryService;
        private readonly IOptions<WebUrl> webUrl;
        private readonly IContextBase<SysOffice> officeRepository;
        private readonly IContextBase<SysEmployee> sysEmployeeRepository;
        private readonly IContextBase<CatCountry> catCountryRepository;
        private readonly IContextBase<SysImage> sysImageRepository;
        private readonly IContextBase<CsTransactionDetail> transactionDetailRepository;
        private readonly IContextBase<CatDepartment> catDepartmentRepository;
        readonly IContextBase<SysUserLevel> userlevelRepository;
        private readonly IContextBase<SysSentEmailHistory> sendEmailHistoryRepository;
        private readonly IContextBase<CatPartnerEmail> catpartnerEmailRepository;
        private readonly IContextBase<CustomsDeclaration> customsDeclarationRepository;
        private readonly IOptions<ApiUrl> ApiUrl;
        private readonly ICacheServiceBase<CatPartner> cache;
        private readonly IContextBase<SysEmailTemplate> sysEmailTemplateRepository;
        private readonly IContextBase<SysEmailSetting> sysEmailSettingRepository;
        string salemanBOD;
        public CatPartnerService(IContextBase<CatPartner> repository,
            ICacheServiceBase<CatPartner> cacheService,
            IMapper mapper,
            IStringLocalizer<LanguageSub> localizer,
            ICurrentUser user,
            IContextBase<SysUser> sysUserRepo,
            ICatPlaceService place,
            ICatCountryService country,
            IContextBase<CatContract> contractRepo, IOptions<WebUrl> url,
            IContextBase<SysOffice> officeRepo,
            IContextBase<CatCountry> catCountryRepo,
            IContextBase<SysEmployee> sysEmployeeRepo,
            IContextBase<SysImage> sysImageRepo,
            IContextBase<CsTransactionDetail> transactionDetailRepo,
            IContextBase<CatDepartment> catDepartmentRepo,
            IContextBase<SysSentEmailHistory> sendEmailHistoryRepo,
            IContextBase<SysUserLevel> userlevelRepo,
            IContextBase<CatPartnerEmail> emailRepo,
            IContextBase<CustomsDeclaration> customsDeclarationRepo,
            IContextBase<SysEmailTemplate> sysEmailTemplateRepo,
            IContextBase<SysEmailSetting> sysEmailSettingRepo,
            IOptions<ApiUrl> apiurl) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            placeService = place;
            contractRepository = contractRepo;
            sysUserRepository = sysUserRepo;
            countryService = country;
            webUrl = url;
            officeRepository = officeRepo;
            sysEmployeeRepository = sysEmployeeRepo;
            catCountryRepository = catCountryRepo;
            sysImageRepository = sysImageRepo;
            transactionDetailRepository = transactionDetailRepo;
            catDepartmentRepository = catDepartmentRepo;
            userlevelRepository = userlevelRepo;
            sendEmailHistoryRepository = sendEmailHistoryRepo;
            catpartnerEmailRepository = emailRepo;
            customsDeclarationRepository = customsDeclarationRepo;
            ApiUrl = apiurl;
            cache = cacheService;
            sysEmailTemplateRepository = sysEmailTemplateRepo;
            sysEmailSettingRepository = sysEmailSettingRepo;

            SetChildren<CsTransaction>("Id", "ColoaderId");
            SetChildren<CsTransaction>("Id", "AgentId");
            SetChildren<SysUser>("Id", "PersonIncharge");
            SetChildren<OpsTransaction>("Id", "CustomerId");
            SetChildren<OpsTransaction>("Id", "SupplierId");
            SetChildren<OpsTransaction>("Id", "AgentId");
            SetChildren<CatPartnerCharge>("Id", "PartnerId");
            SetChildren<CatContract>("Id", "PartnerId");
            SetChildren<CsManifest>("Id", "Supplier");
            SetChildren<CsShipmentSurcharge>("Id", "PayerID");
            SetChildren<CsShipmentSurcharge>("Id", "PaymentObjectID");

            salemanBOD = sysUserRepository.First(x => x.Username == "ITL.BOD").Id;
        }

        public IQueryable<CatPartnerModel> GetPartners()
        {
            return Get();
        }
        public List<DepartmentPartner> GetDepartments()
        {
            return DataEnums.Departments;
        }

        #region CRUD
        public object Add(CatPartnerModel entity)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
            var permissionRangeWrite = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRangeWrite == PermissionRange.None) return new HandleState(403, "");
            CatPartnerModel partner = GetModelToAdd(entity);
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hsTransPartner = DataContext.Add(partner);
                    if (hsTransPartner.Success)
                    {
                        if (entity.Contracts.Count > 0)
                        {
                            var contracts = mapper.Map<List<CatContract>>(entity.Contracts);
                            contracts.ForEach(x =>
                            {
                                x.PartnerId = partner.Id;
                                x.DatetimeCreated = DateTime.Now;
                                x.UserCreated = x.UserModified = currentUser.UserID;
                            });
                            partner.SalePersonId = contracts.FirstOrDefault().SaleManId.ToString();

                            var hsContract = contractRepository.Add(contracts);

                            if (hsContract.Success)
                            {
                                if(partner.IsRequestApproval == true)
                                {
                                    foreach (var item in entity.Contracts)
                                    {
                                        entity.ContractType = item.ContractType;
                                        entity.SalesmanId = item.SaleManId;
                                        entity.UserCreated = partner.UserCreated;
                                        entity.ContractService = GetContractServicesName(item.SaleService);
                                        entity.ContractNo = item.ContractNo;
                                        entity.OfficeIdContract = item.OfficeId;
                                        SendMailRequestApproval(entity);
                                    }
                                }
                              
                            }

                        }
                        if (entity.PartnerEmails.Count > 0)
                        {
                            var emails = mapper.Map<List<CatPartnerEmail>>(entity.PartnerEmails);
                            emails.ForEach(x =>
                            {
                                x.Id = Guid.NewGuid();
                                x.PartnerId = partner.Id;
                                x.DatetimeCreated = DateTime.Now;
                                x.UserCreated = x.UserModified = currentUser.UserID;
                            });
                            var hsEmail = catpartnerEmailRepository.Add(emails);
                        }
                        trans.Commit();
                        if (partner.PartnerType != "Customer" && partner.PartnerType != "Agent")
                        {
                            SendMailCreatedSuccess(partner);
                        }
                    }
                    ClearCache();
                    Get();
                    var result = hsTransPartner;
                    return new { model = partner, result };
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    var result = new HandleState(ex.Message);
                    return new { model = new object { }, result };
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        private CatPartnerModel GetModelToAdd(CatPartnerModel entity)
        {
            var partner = entity;
            if (!string.IsNullOrEmpty(entity.ParentId))
            {
                partner.ParentId = entity.ParentId;
            }
            else if (string.IsNullOrEmpty(partner.InternalReferenceNo) && string.IsNullOrEmpty(entity.ParentId))
            {
                partner.ParentId = partner.Id;
            }
            else
            {
                var refPartner = DataContext.Get(x => x.TaxCode == partner.TaxCode && string.IsNullOrEmpty(x.InternalReferenceNo)).FirstOrDefault();
                if (refPartner == null)
                {
                    partner.ParentId = entity.Id;
                }
                else
                {
                    partner.ParentId = refPartner.Id;
                }
            }
            partner.DatetimeCreated = DateTime.Now;
            partner.DatetimeModified = DateTime.Now;
            partner.UserCreated = partner.UserModified = currentUser.UserID;
            partner.Active = false;
            partner.GroupId = currentUser.GroupId;
            partner.DepartmentId = currentUser.DepartmentId;
            partner.OfficeId = currentUser.OfficeID;
            partner.CompanyId = currentUser.CompanyID;
            return partner;
        }

        public bool SendMailRejectComment(string partnerId, string comment)
        {
            ClearCache();
            var partner = Get(x => x.Id == partnerId).FirstOrDefault();
            string subject = string.Empty;
            string linkVn = string.Empty;
            string linkEn = string.Empty;
            string body = string.Empty;

            string employeeId = sysUserRepository.Get(x => x.Id == partner.UserCreated).Select(t => t.EmployeeId).FirstOrDefault();
            var creatorObj = sysEmployeeRepository.Get(e => e.Id == employeeId)?.FirstOrDefault();
            string UrlClone = string.Copy(ApiUrl.Value.Url);
            List<string> ListCC = new List<string>();

            string address = webUrl.Value.Url + "/en/#/" + "home/catalogue/partner-data/detail/" + partner.Id;
            subject = "Reject Partner - " + partner.PartnerNameVn;
            linkEn = "View more detail, please you <a href='" + address + "'> click here </a>" + "to view detail.";
            linkVn = "Bạn click <a href='" + address + "'> vào đây </a>" + "để xem chi tiết.";

            body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt; color:#004080;'> Dear " + creatorObj.EmployeeNameVn + "," + " </br> </br>" +
                   "<i> Your Partner " + partner.PartnerNameVn + " is rejected by AR/Accountant as info below </i> </br>" +
                   "<i> Khách hàng or thỏa thuận " + partner.PartnerNameVn + " đã bị từ chối với lý do sau: </i> </br></br>" +
                   "\t  Customer Name  / <i> Tên khách hàng: </i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>" +
                   "\t  Taxcode  / <i> Mã số thuế: </i> " + "<b>" + partner.TaxCode + "</b>" + "</br>" +
                   "\t  Reason  / <i> Lý do: </i> " + "<b>" + comment + "</b>" + "</br></br>"
                  + linkEn + "</br>" + linkVn + "</br> </br>" +
                  "<i> Thanks and Regards </i>" + "</br> </br>" +
                  "<b> eFMS System, </b>" +
                  "</br>"
                  + "<p><img src = '[logoEFMS]' /></p> " + " </div>");

            string UrlImage = UrlClone.Replace("Catalogue", "");
            body = body.Replace("[logoEFMS]", UrlImage.ToString() + "/ReportPreview/Images/logo-eFMS.png");
            ListCC.Add(creatorObj?.Email);
            List<string> lstCc = ListMailBCC();
            List<string> lstTo = new List<string>();

            lstTo.Add(creatorObj?.Email);

            bool result = SendMail.Send(subject, body, lstTo, null, null, lstCc);
            var logSendMail = new SysSentEmailHistory
            {
                SentUser = SendMail._emailFrom,
                Receivers = string.Join("; ", lstTo),
                Ccs = string.Join("; ", lstCc),
                Subject = subject,
                Sent = result,
                SentDateTime = DateTime.Now,
                Body = body
            };
            var hsLogSendMail = sendEmailHistoryRepository.Add(logSendMail);
            var hsSm = sendEmailHistoryRepository.SubmitChanges();
            return result;
        }

        private ListEmailViewModel GetListAccountantAR(string OfficeId, string typeOfActive)
        {
            List<string> lstAccountant = new List<string>();
            List<string> lstCCAccountant = new List<string>();
            List<string> lstAR = new List<string>();
            List<string> lstCCAR = new List<string>();
            ListEmailViewModel EmailModel = new ListEmailViewModel();
            var arrayOffice = OfficeId.Split(";").ToArray();
            int lengthOffice = arrayOffice.Length;

            var DataHeadOffice = officeRepository.Get(x => x.OfficeType == "Head" && arrayOffice.Contains(x.Id.ToString().ToLower())).FirstOrDefault();
            var DataBranchOffice = officeRepository.Get(x => x.OfficeType == "Branch" && arrayOffice.Contains(x.Id.ToString().ToLower())).Select(t => t.Id).ToList();

            if (lengthOffice == 1)
            {
                // Get email of accountant, AR
                if (OfficeId == null)
                {
                    lstAccountant = null;
                    lstAR = null;
                }
                else
                {
                    var departmentAccountant = catDepartmentRepository.Get(x => x.DeptType == "ACCOUNTANT" && x.BranchId == new Guid(OfficeId.Replace(";", ""))).FirstOrDefault();
                    var emailSetting = departmentAccountant == null ? null : sysEmailSettingRepository.Get(x => x.EmailType == typeOfActive && x.DeptId == departmentAccountant.Id).FirstOrDefault()?.EmailInfo;
                    if (!string.IsNullOrEmpty(emailSetting))
                    {
                        lstAccountant = emailSetting.Split(";").ToList();
                    }
                    else
                    {
                        var listEmailAccountant = departmentAccountant?.Email;
                        lstAccountant = listEmailAccountant?.Split(";").ToList();
                    }

                    var departmentAR = catDepartmentRepository.Get(x => x.DeptType == "AR" && x.BranchId == new Guid(OfficeId.Replace(";", ""))).FirstOrDefault();
                    emailSetting = departmentAR == null ? null : sysEmailSettingRepository.Get(x => x.EmailType == typeOfActive && x.DeptId == departmentAR.Id).FirstOrDefault()?.EmailInfo;
                    if (!string.IsNullOrEmpty(emailSetting))
                    {
                        lstAR = emailSetting.Split(";").ToList();
                    }
                    else
                    {
                        var listEmailAR = departmentAR?.Email;
                        lstAR = listEmailAR?.Split(";").ToList();
                    }
                }

                var DataHeadOfficeAR = officeRepository.Get(x => x.OfficeType == "Head").FirstOrDefault();
                if (DataHeadOfficeAR == null)
                {
                    lstCCAR = null;
                }
                else
                {
                    var departmentHeadAR = catDepartmentRepository.Get(x => x.DeptType == "AR" && x.BranchId == DataHeadOfficeAR.Id).FirstOrDefault();
                    var emailSetting = departmentHeadAR == null ? null : sysEmailSettingRepository.Get(x => x.EmailType == typeOfActive && x.DeptId == departmentHeadAR.Id).FirstOrDefault()?.EmailInfo;
                    if (!string.IsNullOrEmpty(emailSetting))
                    {
                        lstCCAR = emailSetting.Split(";").ToList();
                    }
                    else
                    {
                        var listEmailCCAR = departmentHeadAR?.Email;
                        lstCCAR = listEmailCCAR?.Split(";").ToList();
                    }
                }
            }
            else if (lengthOffice > 1)
            {
                // list mail to Accountant, AR
                if (DataHeadOffice == null)
                {
                    lstAccountant = null;
                    lstAR = null;
                }
                else
                {
                    var departmentAccountant = catDepartmentRepository.Get(x => x.DeptType == "ACCOUNTANT" && x.BranchId == DataHeadOffice.Id).FirstOrDefault();
                    var emailSetting = departmentAccountant == null ? null : sysEmailSettingRepository.Get(x => x.EmailType == typeOfActive && x.DeptId == departmentAccountant.Id).FirstOrDefault()?.EmailInfo;
                    if (!string.IsNullOrEmpty(emailSetting))
                    {
                        lstAccountant = emailSetting.Split(";").ToList();
                    }
                    else
                    {
                        var listEmailAccountant = departmentAccountant?.Email;
                        lstAccountant = listEmailAccountant?.Split(";").ToList();
                    }

                    var departmentAR = DataHeadOffice == null ? null : catDepartmentRepository.Get(x => x.DeptType == "AR" && x.BranchId == DataHeadOffice.Id).FirstOrDefault();
                    emailSetting = departmentAR == null ? null : sysEmailSettingRepository.Get(x => x.EmailType == typeOfActive && x.DeptId == departmentAR.Id).FirstOrDefault()?.EmailInfo;
                    if (!string.IsNullOrEmpty(emailSetting))
                    {
                        lstAR = emailSetting.Split(";").ToList();
                    }
                    else
                    {
                        var listEmailAR = departmentAR?.Email;
                        lstAR = listEmailAR?.Split(";").ToList();
                    }
                }
                // list mail cc Accountant, AR
                if (DataBranchOffice == null)
                {
                    lstCCAccountant = null;
                    lstCCAR = null;
                }
                else
                {
                    var departmentAccountant = catDepartmentRepository.Get(x => x.DeptType == "ACCOUNTANT" && DataBranchOffice.Contains((Guid)x.BranchId)).FirstOrDefault();
                    var emailSetting = departmentAccountant == null ? null : sysEmailSettingRepository.Get(x => x.EmailType == typeOfActive && x.DeptId == departmentAccountant.Id).Select(x => x.EmailInfo).FirstOrDefault();
                    if (!string.IsNullOrEmpty(emailSetting))
                    {
                        lstCCAccountant = emailSetting.Split(";").ToList();
                    }
                    else
                    {
                        var listEmailCCAcountant = departmentAccountant?.Email;
                        lstCCAccountant = listEmailCCAcountant?.Split(";").ToList();
                    }

                    var departmentAR = catDepartmentRepository.Get(x => x.DeptType == "AR" && DataBranchOffice.Contains((Guid)x.BranchId)).FirstOrDefault();
                    emailSetting = departmentAR == null ? null : sysEmailSettingRepository.Get(x => x.EmailType == typeOfActive && x.DeptId == departmentAR.Id).Select(x => x.EmailInfo).FirstOrDefault();
                    if (!string.IsNullOrEmpty(emailSetting))
                    {
                        lstCCAR = emailSetting.Split(";").ToList();
                    }
                    else
                    {
                        var listEmailCCAR = departmentAR?.Email;
                        lstCCAR = listEmailCCAR?.Split(";").ToList();
                    }
                }
            }
            EmailModel.ListAccountant = lstAccountant?.Where(t => !string.IsNullOrEmpty(t)).ToList();
            EmailModel.ListCCAccountant = lstCCAccountant?.Where(t => !string.IsNullOrEmpty(t)).ToList();

            EmailModel.ListAR = lstAR?.Where(t => !string.IsNullOrEmpty(t)).ToList();
            EmailModel.ListCCAR = lstCCAR?.Where(t => !string.IsNullOrEmpty(t)).ToList();

            return EmailModel;
        }

        private void SendMailRequestApproval(CatPartnerModel partner)
        {
            string employeeId = sysUserRepository.Get(x => x.Id == partner.UserCreated).Select(t => t.EmployeeId).FirstOrDefault();
            var objInfoCreator = sysEmployeeRepository.Get(e => e.Id == employeeId)?.FirstOrDefault();
            string EnNameCreatetor = objInfoCreator?.EmployeeNameEn;

            string employeeIdUserModified = sysUserRepository.Get(x => x.Id == partner.UserModified).Select(t => t.EmployeeId).FirstOrDefault();
            var objInfoModified = sysEmployeeRepository.Get(e => e.Id == employeeIdUserModified)?.FirstOrDefault();

            string employeeIdPartner = sysUserRepository.Get(x => x.Id == partner.UserCreated).Select(t => t.EmployeeId).FirstOrDefault();
            var objInfoCreatorPartner = sysEmployeeRepository.Get(e => e.Id == employeeIdPartner)?.FirstOrDefault();

            List<string> lstTo = new List<string>();
            List<string> lstToAcc = new List<string>();
            string UrlClone = string.Copy(ApiUrl.Value.Url);

            // info send to and cc
            ListEmailViewModel listEmailViewModel = GetListAccountantAR(partner.OfficeIdContract, DataEnums.EMAIL_TYPE_ACTIVE_PARTNER);

            string url = string.Empty;
            string employeeIdSalemans = sysUserRepository.Get(x => x.Id == partner.SalesmanId).Select(t => t.EmployeeId).FirstOrDefault();
            var objInfoSalesman = sysEmployeeRepository.Get(e => e.Id == employeeIdSalemans)?.FirstOrDefault();
            switch (partner.PartnerType)
            {
                case "Customer":
                    url = "home/commercial/customer/";
                    break;
                case "Agent":
                    url = "home/commercial/agent/";
                    break;
            }
            string address = webUrl.Value.Url + "/en/#/" + url + partner.Id;

            string title = string.Empty;
            title = partner.PartnerType == "Customer" ? "Customer" : "Agent";

            #region change string to using template
            //linkEn = "You can <a href='" + address + "'> click here </a>" + "to view detail.";
            //linkVn = "Bạn click <a href='" + address + "'> vào đây </a>" + "để xem chi tiết.";
            //subject = "eFMS - Customer Approval Request From " + EnNameCreatetor;

            //body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt; color:#004080;'> Dear Accountant/AR Team, " + " </br> </br>" +

            //  "<i> You have a " + title +" Approval request from " + EnNameCreatetor + " as info below </i> </br>" +
            //  "<i> Bạn có một yêu cầu xác duyệt khách hàng từ " + EnNameCreatetor + " với thông tin như sau: </i> </br> </br>" +

            //  "\t  Customer ID  / <i> Mã Agent:</i> " + "<b>" + partner.AccountNo + "</b>" + "</br>" +
            //  "\t  Customer Name  / <i> Tên khách hàng:</i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>" +
            //  "\t  Taxcode / <i> Mã số thuế: </i>" + "<b>" + partner.TaxCode + "</b>" + "</br>" +

            //  "\t  Service  / <i> Dịch vụ: </i>" + "<b>" + partner.ContractService + "</b>" + "</br>" +
            //  "\t  Contract type  / <i> Loại hợp đồng: </i> " + "<b>" + partner.ContractType + "</b>" + "</br>" +
            //  "\t  Contract No  / <i> Số hợp đồng: </i> " + "<b>" + partner.ContractNo + "</b>" + "</br>" +
            //  "\t  Requestor  / <i> Người yêu cầu: </i> " + "<b>" + EnNameCreatetor + "</b>" + "</br> </br>"

            //  + linkEn + "</br>" + linkVn + "</br> </br>" +
            //  "<i> Thanks and Regards </i>" + "</br> </br>" +
            //  "<b> eFMS System, </b>" +
            //  "</br>"
            //  + "<p><img src = '[logoEFMS]' /></p> " + " </div>");
            //string urlImage = UrlClone.Replace("Catalogue", "");
            //body = body.Replace("[logoEFMS]", urlImage + "/ReportPreview/Images/logo-eFMS.png");
            #endregion

            // Filling email with template
            var emailTemplate = sysEmailTemplateRepository.Get(x => x.Code == "CONTRACT-APPROVEDREQUEST").FirstOrDefault();
            // Subject
            var subject = new StringBuilder(emailTemplate.Subject);
            subject.Replace("{{enNameCreatetor}}", EnNameCreatetor);

            // Body
            var body = new StringBuilder(emailTemplate.Body);
            string urlToSend = UrlClone.Replace("Catalogue", "");
            body.Replace("{{dear}}", partner.ContractType == "Cash" ? "Accountant Team" : "AR Team");
            body.Replace("{{title}}", title);
            body.Replace("{{enNameCreatetor}}", EnNameCreatetor);
            body.Replace("{{accountNo}}", partner.AccountNo);
            body.Replace("{{partnerNameVn}}", partner.PartnerNameVn);
            body.Replace("{{taxCode}}", partner.TaxCode);
            body.Replace("{{contractService}}", partner.ContractService);
            body.Replace("{{contractType}}", partner.ContractType);
            body.Replace("{{contractNo}}", partner.ContractNo);
            body.Replace("{{address}}", address);
            body.Replace("{{logoEFMS}}", urlToSend + "/ReportPreview/Images/logo-eFMS.png");

            List<string> lstBCc = ListMailBCC();
            List<string> lstCc = new List<string>();
            if (partner.ContractType == "Cash")
            {
                lstTo = listEmailViewModel.ListAccountant;
                if(listEmailViewModel.ListCCAccountant != null)
                {
                    lstCc.AddRange(listEmailViewModel.ListCCAccountant);
                }
            }
            else
            {
                lstTo = listEmailViewModel.ListAR;
                if(listEmailViewModel.ListCCAR != null)
                {
                    lstCc.AddRange(listEmailViewModel.ListCCAR);

                }
            }

            lstCc.Add(objInfoSalesman?.Email);
            lstCc.Add(objInfoCreatorPartner?.Email);
            lstCc.Add(objInfoModified?.Email);
            lstCc.Add(objInfoCreator?.Email);

            bool result = SendMail.Send(subject.ToString(), body.ToString(), lstTo, null, lstCc, lstBCc);

            var logSendMail = new SysSentEmailHistory
            {
                SentUser = SendMail._emailFrom,
                Receivers = string.Join("; ", lstTo),
                Ccs = string.Join("; ", lstCc),
                Subject = subject.ToString(),
                Sent = result,
                SentDateTime = DateTime.Now,
                Body = body.ToString()
            };
            var hsLogSendMail = sendEmailHistoryRepository.Add(logSendMail);
            var hsSm = sendEmailHistoryRepository.SubmitChanges();
        }


        public bool SendMailCreatedSuccess(CatPartner partner)
        {
            string employeeId = sysUserRepository.Get(x => x.Id == currentUser.UserID).Select(t => t.EmployeeId).FirstOrDefault();
            var infoCreatetor = sysEmployeeRepository.Get(e => e.Id == employeeId).FirstOrDefault();
            string url = string.Empty;
            List<string> lstToAR = new List<string>();
            List<string> lstToAccountant = new List<string>();
            List<string> lstCc = ListMailBCC();
            List<string> lstCcCreator = new List<string>();
            string UrlClone = string.Copy(ApiUrl.Value.Url);
            // info send to and cc
            ListEmailViewModel listEmailViewModel = GetListAccountantAR(currentUser.OfficeID.ToString(), DataEnums.EMAIL_TYPE_ACTIVE_PARTNER);

            switch (partner.PartnerType)
            {
                case "Customer":
                    url = "home/commercial/customer/";
                    break;
                case "Agent":
                    url = "home/commercial/agent/";
                    break;
                default:
                    url = "home/catalogue/partner-data/detail/";
                    break;
            }

            string address = webUrl.Value.Url + "/en/#/" + url + partner.Id;
            #region change string to using template
            //string linkEn = "You can <a href='" + address + "'> click here </a>" + "to view detail.";
            //string linkVn = "Bạn click <a href='" + address + "'> vào đây </a>" + "để xem chi tiết.";
            //string subject = "eFMS - Partner Approval Request From " + infoCreatetor?.EmployeeNameVn;
            //string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt; color:#004080;'> Dear Accountant Team: </br> </br>" +
            //    "<i> You have a Partner Approval request From " + infoCreatetor?.EmployeeNameVn + " as info bellow: </i> </br>" +
            //    "<i> Bạn có môt yêu cầu xác duyệt đối tượng từ " + infoCreatetor?.EmployeeNameVn + " với thông tin như sau: </i> </br> </br>" +
            //    "\t  Partner ID  / <i> Mã đối tượng:</i> " + "<b>" + partner.AccountNo + "</b>" + "</br>" +
            //    "\t  Partner Name  / <i> Tên đối tượng:</i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>" +
            //    "\t  Category  / <i> Danh mục: </i>" + "<b>" + partner.PartnerGroup + "</b>" + "</br>" +
            //    "\t  Taxcode / <i> Mã số thuế: </i>" + "<b>" + partner.TaxCode + "</b>" + "</br>" +
            //    "\t  Address  / <i> Địa chỉ: </i> " + "<b>" + partner.AddressEn + "</b>" + "</br>" +
            //    "\t  Requestor / <i> Người yêu cầu: </i> " + "<b>" + infoCreatetor?.EmployeeNameVn + "</b>" + "</br> </br>" + linkEn + "</br>" + linkVn + "</br> </br>" +
            //    "<i> Thanks and Regards </i>" + "</br> </br>" +
            //    "<b> eFMS System, </b>" + "</br>" +
            //    "<p><img src = '[logoEFMS]' /></p> " + " </div>");
            //var urlTo = UrlClone.Replace("Catalogue", "");
            //body = body.Replace("[logoEFMS]", urlTo.ToString() + "/ReportPreview/Images/logo-eFMS.png");
            #endregion

            // Filling email with template
            var emailTemplate = sysEmailTemplateRepository.Get(x => x.Code == "PARTNER-APPROVEDREQUEST").FirstOrDefault();
            // Subject
            var subject = new StringBuilder(emailTemplate.Subject);
            subject.Replace("{{employeeNameVn}}", infoCreatetor?.EmployeeNameVn);

            var body = new StringBuilder(emailTemplate.Body);
            var urlToSend = UrlClone.Replace("Catalogue", "");
            body.Replace("{{employeeNameVn}}", infoCreatetor?.EmployeeNameVn);
            body.Replace("{{accountNo}}", partner.AccountNo);
            body.Replace("{{partnerNameVn}}", partner.PartnerNameVn);
            body.Replace("{{partnerGroup}}", partner.PartnerGroup);
            body.Replace("{{taxCode}}", partner.TaxCode);
            body.Replace("{{addressEn}}", partner.AddressEn);
            body.Replace("{{address}}", address);
            body.Replace("{{logoEFMS}}", urlToSend + "/ReportPreview/Images/logo-eFMS.png");

            lstCcCreator.Add(infoCreatetor?.Email);
            bool resultSenmail = false;
            lstToAccountant = listEmailViewModel.ListAccountant;
            lstToAR = listEmailViewModel.ListAR;
            if ((partner.PartnerType != "Customer" && partner.PartnerType != "Agent") || string.IsNullOrEmpty(partner.PartnerType))
            {
                if (lstToAccountant.Any() || lstCc.Any())
                {
                    if (lstToAccountant.Count() == 0)
                    {
                        lstToAccountant = lstCc;
                    }
                    resultSenmail = SendMail.Send(subject.ToString(), body.ToString(), lstToAccountant, null, lstCcCreator, lstCc);
                }
            }

            if (partner.PartnerType == "Customer" || partner.PartnerType == "Agent")
            {
                lstToAccountant.AddRange(lstToAR);
                lstToAccountant = lstToAccountant.Where(x => !string.IsNullOrWhiteSpace(x) && !string.IsNullOrEmpty(x)).Distinct().ToList();
                if (lstToAccountant.Any())
                {
                    resultSenmail = SendMail.Send(subject.ToString(), body.ToString(), lstToAccountant, null, lstCcCreator, lstCc);
                }
            }

            var logSendMail = new SysSentEmailHistory
            {
                SentUser = SendMail._emailFrom,
                Receivers = string.Join("; ", lstToAccountant),
                Ccs = string.Join("; ", lstCcCreator),
                Bccs = string.Join("; ", lstCc),
                Subject = subject.ToString(),
                Sent = resultSenmail,
                SentDateTime = DateTime.Now,
                Body = body.ToString()
            };

            var hsLogSendMail = sendEmailHistoryRepository.Add(logSendMail);
            var hsSm = sendEmailHistoryRepository.SubmitChanges();
            return resultSenmail;
        }

        private List<string> ListMailBCC()
        {
            var emailBcc = ((eFMSDataContext)DataContext.DC).ExecuteFuncScalar("[dbo].[fn_GetEmailBcc]");
            List<string> emailBCCs = new List<string>();
            if (emailBcc != null)
            {
                emailBCCs = emailBcc.ToString().Split(";").ToList();
            }
            return emailBCCs;
        }

        private async void UploadFileContract(ContractFileUploadModel model)
        {
            string fileName = "";
            string path = this.webUrl.Value.Url;
            var list = new List<SysImage>();
            /* Kiểm tra các thư mục có tồn tại */
            var hs = new HandleState();
            ImageHelper.CreateDirectoryFile(model.FolderName, model.PartnerId);
            List<SysImage> resultUrls = new List<SysImage>();
            fileName = model.Files.FileName;
            string objectId = model.PartnerId;
            await ImageHelper.SaveFile(fileName, model.FolderName, objectId, model.Files);
            string urlImage = path + "/" + model.FolderName + "files/" + objectId + "/" + fileName;
            var sysImage = new SysImage
            {
                Id = Guid.NewGuid(),
                Url = urlImage,
                Name = fileName,
                Folder = model.FolderName ?? "Partner",
                ObjectId = model.PartnerId.ToString(),
                ChildId = model.ChildId.ToString(),
                UserCreated = currentUser.UserName, //admin.
                UserModified = currentUser.UserName,
                DateTimeCreated = DateTime.Now,
                DatetimeModified = DateTime.Now
            };
            resultUrls.Add(sysImage);
            if (!sysImageRepository.Any(x => x.ObjectId == objectId && x.Url == urlImage && x.ChildId == model.ChildId))
            {
                list.Add(sysImage);
            }
            if (list.Count > 0)
            {
                list.ForEach(x => x.IsTemp = model.IsTemp);
                hs = await sysImageRepository.AddAsync(list);
            }

        }
        public HandleState Update(CatPartnerModel model)
        {
            var listSalemans = contractRepository.Get(x => x.PartnerId == model.Id).ToList();
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            var entity = GetModelToUpdate(model);

            int code = GetPermissionToUpdate(new ModelUpdate { GroupId = entity.GroupId, DepartmentId = entity.DepartmentId, OfficeId = entity.OfficeId, CompanyId = entity.CompanyId, UserCreator = model.UserCreated, Salemans = listSalemans, PartnerGroup = model.PartnerGroup }, permissionRange, null);
            if (code == 403) return new HandleState(403, "");

            if (model.Contracts?.Count > 0)
            {
                entity.SalePersonId = model.Contracts.FirstOrDefault().SaleManId.ToString();
            }
            var hs = DataContext.Update(entity, x => x.Id == model.Id);
            if (hs.Success)
            {
                ClearCache();
                Get();
            }
            return hs;
        }

        private CatPartner GetModelToUpdate(CatPartnerModel model)
        {
            var entity = mapper.Map<CatPartner>(model);
            var partner = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();

            if (!string.IsNullOrEmpty(entity.InternalReferenceNo) && string.IsNullOrEmpty(entity.ParentId))
            {
                var refPartner = DataContext.Get(x => x.TaxCode == entity.TaxCode && string.IsNullOrEmpty(x.InternalReferenceNo)).FirstOrDefault();
                if (refPartner == null)
                {
                    entity.ParentId = entity.Id;
                }
                else
                {
                    entity.ParentId = refPartner.Id;
                }
            }

            entity.DatetimeModified = DateTime.Now;
            entity.UserModified = currentUser.UserID;
            entity.GroupId = partner.GroupId;
            entity.DepartmentId = partner.DepartmentId;
            entity.OfficeId = partner.OfficeId;
            entity.CompanyId = partner.CompanyId;
            if (entity.Active == false)
            {
                entity.InactiveOn = DateTime.Now;
            }
            return entity;
        }

        public HandleState Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var partner = DataContext.Get(x => x.Id == id).FirstOrDefault();
                var existClearance = customsDeclarationRepository.Any(x => (x.AccountNo ?? "").Contains(partner.AccountNo) || x.PartnerTaxCode.Contains(partner.TaxCode));
                if (transactionDetailRepository.Any(x => x.CustomerId == id) || existClearance)
                {
                    return new HandleState("This partner is already in use so you can not delete it");
                }
            }
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                var s = contractRepository.Delete(x => x.PartnerId == id);
                contractRepository.SubmitChanges();
                var partnerUpdate = DataContext.Get(x => x.ParentId == id);
                foreach (var partner in partnerUpdate)
                {
                    partner.ParentId = partner.Id;
                    DataContext.Update(partner, x => x.Id == partner.Id);
                }
                ClearCache();
                Get();
            }
            return hs;
        }
        #endregion

        public IQueryable<CatPartnerViewModel> QueryExport(CatPartnerCriteria criteria)
        {
            var data = QueryPaging(criteria);
            if (data == null)
            {
                return null;
            }
            var salemans = contractRepository.Get().ToList();
            ICurrentUser _user = null;
            switch (criteria.PartnerType)
            {
                case "Customer":
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialCustomer);
                    break;
                case "Agent":
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialAgent);
                    break;
                default:
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
                    break;
            }
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            switch (rangeSearch)
            {
                case PermissionRange.None:
                    data = null;
                    break;
                case PermissionRange.All:
                    break;
                case PermissionRange.Owner:
                    if (criteria.PartnerGroup.ToString() == DataEnums.CustomerPartner || criteria.PartnerGroup == 0)
                    {
                        data = data.Where(x => salemans.Any(y => y.SaleManId == currentUser.UserID && y.PartnerId.Equals(x.Id))
                        || x.UserCreated == currentUser.UserID);
                    }
                    else
                    {
                        data = data.Where(x => x.UserCreated == currentUser.UserID);
                    }
                    break;
                case PermissionRange.Group:
                    if (criteria.PartnerGroup.ToString() == DataEnums.CustomerPartner || criteria.PartnerGroup == 0)
                    {
                        data = data.Where(x => (x.GroupId == currentUser.GroupId && (x.DepartmentId == currentUser.DepartmentId) && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                       || x.UserCreated == currentUser.UserID
                       || salemans.Any(y => y.SaleManId == currentUser.UserID && y.PartnerId.Equals(x.Id))
                       );
                    }
                    else
                    {
                        data = data.Where(x => (x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                        || x.UserCreated == currentUser.UserID
                        );
                    }
                    break;
                case PermissionRange.Department:
                    if (criteria.PartnerGroup.ToString() == DataEnums.CustomerPartner || criteria.PartnerGroup == 0)
                    {
                        data = data.Where(x => (x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                       || x.UserCreated == currentUser.UserID
                       || salemans.Any(y => y.SaleManId == currentUser.UserID && y.PartnerId.Equals(x.Id))
                       );
                    }
                    else
                    {
                        data = data.Where(x => (x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.DepartmentId == currentUser.DepartmentId && x.CompanyId == currentUser.CompanyID)
                        || x.UserCreated == currentUser.UserID
                        );
                    }
                    break;
                case PermissionRange.Office:
                    if (criteria.PartnerGroup.ToString() == DataEnums.CustomerPartner || criteria.PartnerGroup == 0)
                    {
                        data = data.Where(x => (x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                       || x.UserCreated == currentUser.UserID
                       || salemans.Any(y => y.SaleManId == currentUser.UserID && y.PartnerId.Equals(x.Id))
                       );
                    }
                    else
                    {
                        data = data.Where(x => (x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                        || x.UserCreated == currentUser.UserID
                        );
                    }
                    break;
                case PermissionRange.Company:
                    if (criteria.PartnerGroup.ToString() == DataEnums.CustomerPartner || criteria.PartnerGroup == 0)
                    {
                        data = data.Where(x => (x.CompanyId == currentUser.CompanyID)
                       || x.UserCreated == currentUser.UserID
                       || salemans.Any(y => y.SaleManId == currentUser.UserID && y.PartnerId.Equals(x.Id))
                       );
                    }
                    else
                    {
                        data = data.Where(x => (x.CompanyId == currentUser.CompanyID)
                        || x.UserCreated == currentUser.UserID
                        );
                    }
                    break;
            }

            if (data == null)
            {
                return null;
            }
            return data.AsQueryable();
        }

        public IQueryable<QueryExportAgreementInfo> QueryExportAgreement(CatPartnerCriteria criteria)
        {

            Expression<Func<CatContract, bool>> q = query => true;

            if (criteria.Active != null)
            {
                q = q.And(x => x.Active == criteria.Active);
            }
            if (!string.IsNullOrEmpty(criteria.Saleman))
            {
                var s = sysUserRepository.Get().Where(x => criteria.Saleman.Contains(x.Id)).Select(t => t.Id).ToList();
                q = q.And(x => s.Contains(x.SaleManId));
            }
            var contracts = contractRepository.Get(q);
            if (criteria.DatetimeCreatedFrom.HasValue && criteria.DatetimeCreatedTo.HasValue)
            {
                contracts = contracts.Where(x => x.DatetimeCreated.Value.Date <= criteria.DatetimeCreatedTo.Value.Date && x.DatetimeCreated.Value.Date >= criteria.DatetimeCreatedFrom.Value.Date);
            }
            var sysUSer = sysUserRepository.Get();
            IQueryable<QueryExportAgreementInfo> result = Enumerable.Empty<QueryExportAgreementInfo>().AsQueryable();
            if (!string.IsNullOrEmpty(criteria.PartnerType))
            {
                result = from c in contracts
                             join p in DataContext.Get() on c.PartnerId equals p.Id
                             join user1 in sysUSer on c.SaleManId equals user1.Id into grpUs1
                             from g1 in grpUs1.DefaultIfEmpty()
                             join user2 in sysUSer on c.UserCreated equals user2.Id into grpUs2
                             from g2 in grpUs2.DefaultIfEmpty()
                             where p.PartnerType == criteria.PartnerType
                             select new QueryExportAgreementInfo
                             {
                                 Active = c.Active,
                                 AgreementNo = c.ContractNo,
                                 AgreementType = c.ContractType,
                                 ARComfirm = c.Arconfirmed,
                                 CreditLimit = c.ContractType == DataEnums.CONTRACT_TRIAL ? c.TrialCreditLimited : c.CreditLimit,
                                 Currency = c.CurrencyId,
                                 EffectiveDate = c.EffectiveDate,
                                 ExpiredDate = c.ExpiredDate,
                                 PartnerCode = p.TaxCode,
                                 PartnerNameEn = p.PartnerNameEn,
                                 PartnerNameVn = p.PartnerNameVn,
                                 PaymentTerm = c.PaymentTerm,
                                 SaleManName = g1.Username,
                                 UserCreatedName = g2.Username,
                                 Service = GetContractServicesName(c.SaleService),
                                 Office = GetContractOfficeName(c.OfficeId),
                                 PartnerType = p.PartnerType
                             };

            }
            if (string.IsNullOrEmpty(criteria.PartnerType))
            {
                result = from c in contracts
                             join p in DataContext.Get() on c.PartnerId equals p.Id
                             join user1 in sysUSer on c.SaleManId equals user1.Id into grpUs1
                             from g1 in grpUs1.DefaultIfEmpty()
                             join user2 in sysUSer on c.UserCreated equals user2.Id into grpUs2
                             from g2 in grpUs2.DefaultIfEmpty()
                             select new QueryExportAgreementInfo
                             {
                                 Active = c.Active,
                                 AgreementNo = c.ContractNo,
                                 AgreementType = c.ContractType,
                                 ARComfirm = c.Arconfirmed,
                                 CreditLimit = c.ContractType == DataEnums.CONTRACT_TRIAL ? c.TrialCreditLimited : c.CreditLimit,
                                 Currency = c.CurrencyId,
                                 EffectiveDate = c.EffectiveDate,
                                 ExpiredDate = c.ExpiredDate,
                                 PartnerCode = p.TaxCode,
                                 PartnerNameEn = p.PartnerNameEn,
                                 PartnerNameVn = p.PartnerNameVn,
                                 PaymentTerm = c.PaymentTerm,
                                 SaleManName = g1.Username,
                                 UserCreatedName = g2.Username,
                                 Service = GetContractServicesName(c.SaleService),
                                 Office = GetContractOfficeName(c.OfficeId),
                                 PartnerType = p.PartnerType
                             };

            }
            return result;
        }

        public  IQueryable<QueryExportAgreementInfo> MappingQueryAgreementInfo(IQueryable<CatPartner> queryPartner, bool? AgreeActive, string partnerType)
        {
            var contract = contractRepository.Get();
            var sysUSer = sysUserRepository.Get();
            var office = officeRepository.Get();
            var query = from c in contract
                        join p in queryPartner on c.PartnerId equals p.Id
                        join user1 in sysUSer on c.SaleManId equals user1.Id into grpUs1
                        from g1 in grpUs1.DefaultIfEmpty()
                        join user2 in sysUSer on c.UserCreated equals user2.Id into grpUs2
                        from g2 in grpUs2.DefaultIfEmpty()
                        where ((p.PartnerType == partnerType) && c.SaleManId == p.SalePersonId)
                        select new QueryExportAgreementInfo
                        {
                            Active = c.Active,
                            AgreementNo = c.ContractNo,
                            AgreementType = c.ContractType,
                            ARComfirm = c.Arconfirmed,
                            CreditLimit = c.ContractType == DataEnums.CONTRACT_TRIAL ? c.TrialCreditLimited : c.CreditLimit,
                            Currency = c.CurrencyId,
                            EffectiveDate = c.EffectiveDate,
                            ExpiredDate = c.ExpiredDate,
                            PartnerCode = p.TaxCode,
                            PartnerNameEn = p.PartnerNameEn,
                            PartnerNameVn = p.PartnerNameVn,
                            PaymentTerm = c.PaymentTerm,
                            SaleManName = g1.Username,
                            UserCreatedName = g2.Username,
                            Service = GetContractServicesName(c.SaleService),
                            Office = GetContractOfficeName(c.OfficeId),
                        };
            if (AgreeActive != null)
            {
                return query.Where(x => x.Active == AgreeActive);
            }
            return query;
        }

        public IQueryable<CatPartnerViewModel> Paging(CatPartnerCriteria criteria, int page, int size, out int rowsCount)
        {

            /* 
             * var data = QueryPaging(criteria);
            if (data == null)
            {
                rowsCount = 0;
                return null;
            }
            var salemans = contractRepository.Get().ToList();
            ICurrentUser _user = null;
            switch (criteria.PartnerType)
            {
                case "Customer":
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialCustomer);
                    break;
                case "Agent":
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialAgent);
                    break;
                default:
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
                    break;
            }
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            switch (rangeSearch)
            {
                case PermissionRange.None:
                    data = null;
                    break;
                case PermissionRange.All:
                    break;
                case PermissionRange.Owner:
                    if (criteria.PartnerGroup.ToString() == DataEnums.CustomerPartner || criteria.PartnerGroup == 0)
                    {
                        data = data.Where(x => salemans.Any(y => y.SaleManId == currentUser.UserID && y.PartnerId.Equals(x.Id))
                        || x.UserCreated == currentUser.UserID);
                    }
                    else
                    {
                        data = data.Where(x => x.UserCreated == currentUser.UserID);
                    }
                    break;
                case PermissionRange.Group:
                    if (criteria.PartnerGroup.ToString() == DataEnums.CustomerPartner || criteria.PartnerGroup == 0)
                    {
                        var dataUserLevel = userlevelRepository.Get(x => x.GroupId == currentUser.GroupId).Select(t => t.UserId).ToList();
                        data = data.Where(x => (x.GroupId == currentUser.GroupId && (x.DepartmentId == currentUser.DepartmentId) && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                       || x.UserCreated == currentUser.UserID
                       || salemans.Any(y => y.SaleManId == currentUser.UserID && y.PartnerId.Equals(x.Id))
                       || salemans.Any(y => dataUserLevel.Contains(y.SaleManId) && y.PartnerId.Equals(x.Id))
                       );
                    }
                    else
                    {
                        data = data.Where(x => (x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                        || x.UserCreated == currentUser.UserID
                        );
                    }
                    break;
                case PermissionRange.Department:
                    if (criteria.PartnerGroup.ToString() == DataEnums.CustomerPartner || criteria.PartnerGroup == 0)
                    {
                        var dataUserLevelDepartment = userlevelRepository.Get(x => x.DepartmentId == currentUser.DepartmentId).Select(t => t.UserId).ToList();

                        data = data.Where(x => (x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                       || x.UserCreated == currentUser.UserID
                       || salemans.Any(y => y.SaleManId == currentUser.UserID && y.PartnerId.Equals(x.Id))
                       || salemans.Any(y => dataUserLevelDepartment.Contains(y.SaleManId) && y.PartnerId.Equals(x.Id))
                       );
                    }
                    else
                    {
                        data = data.Where(x => (x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.DepartmentId == currentUser.DepartmentId && x.CompanyId == currentUser.CompanyID)
                        || x.UserCreated == currentUser.UserID
                        );
                    }
                    break;
                case PermissionRange.Office:
                    if (criteria.PartnerGroup.ToString() == DataEnums.CustomerPartner || criteria.PartnerGroup == 0)
                    {
                        data = data.ToList().Where(x => (x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                         || x.UserCreated.IndexOf(currentUser.UserID, StringComparison.OrdinalIgnoreCase) > -1
                         || salemans.Any(y => y.SaleManId == currentUser.UserID && y.PartnerId.Equals(x.Id))
                       )?.AsQueryable();
                    }
                    else
                    {
                        data = data.Where(x => (x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                        || x.UserCreated == currentUser.UserID
                        );
                    }
                    break;
                case PermissionRange.Company:
                    if (criteria.PartnerGroup.ToString() == DataEnums.CustomerPartner || criteria.PartnerGroup == 0)
                    {
                        data = data.Where(x => (x.CompanyId == currentUser.CompanyID)
                        || x.UserCreated == currentUser.UserID
                        || salemans.Any(y => y.SaleManId == currentUser.UserID && y.PartnerId.Equals(x.Id))
                       );
                    }
                    else
                    {
                        data = data.Where(x => (x.CompanyId == currentUser.CompanyID)
                        || x.UserCreated == currentUser.UserID
                        );
                    }
                    break;
            } */
            var data = QueryExport(criteria);
            if (data == null)
            {
                rowsCount = 0;
                return null;
            }

            var dataCachedMap = data.ProjectTo<CatPartner>(mapper.ConfigurationProvider);

            // cache for export 1 minutes
            bool stateCaching = cache.Set(dataCachedMap.ToList(), TimeSpan.FromMinutes(1));

            rowsCount = data.Select(x => x.Id).Count();
            if (rowsCount == 0)
            {
                return null;
            }
            IQueryable<CatPartnerViewModel> datas = null;
            IQueryable<CatPartnerViewModel> results = null;
            List<CatPartnerViewModel> partners = new List<CatPartnerViewModel>();
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = data.OrderByDescending(x => x.DatetimeModified).Skip((page - 1) * size).Take(size).AsQueryable();
            }
            return results;
        }

        public HandleState CheckDetailPermission(string id)
        {
            ClearCache();
            var detail = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if (detail == null)
            {
                return new HandleState("has been deleted, Please check again!");
            }
            var salemans = contractRepository.Get(x => x.PartnerId == id).ToList();
            ICurrentUser _user = null;
            switch (detail.PartnerType)
            {
                case "Customer":
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialCustomer);
                    break;
                case "Agent":
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialAgent);
                    break;
                default:
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
                    break;
            }
            var permissionRange = new PermissionRange();
            if (_user.UserMenuPermission == null)
            {
                permissionRange = PermissionRange.None;
            }
            else
            {
                permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);
            }
            int code = GetPermissionToUpdate(new ModelUpdate { GroupId = detail.GroupId, OfficeId = detail.OfficeId, CompanyId = detail.CompanyId, DepartmentId = detail.DepartmentId, UserCreator = detail.UserCreated, Salemans = salemans, PartnerGroup = detail.PartnerGroup }, permissionRange, 1);
            if (code == 403) return new HandleState(403, "");
            return new HandleState();
        }
        
        public HandleState CheckDeletePermission(string id)
        {
            var detail = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if (detail == null)
            {
                return new HandleState("has been deleted, Please check again!");
            }
            else
            {
                if (detail.Active == true)
                {
                    return new HandleState("can't delete, Please reload!");
                }
            }
            var salemans = contractRepository.Get(x => x.PartnerId == id).ToList();
            ICurrentUser _user = null;
            switch (detail.PartnerType)
            {
                case "Customer":
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialCustomer);
                    break;
                case "Agent":
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialAgent);
                    break;
                default:
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
                    break;
            }
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            int code = GetPermissionToDelete(new ModelUpdate { GroupId = detail.GroupId, OfficeId = detail.OfficeId, CompanyId = detail.CompanyId, DepartmentId = detail.DepartmentId, UserCreator = detail.UserCreated, Salemans = salemans, PartnerGroup = detail.PartnerGroup }, permissionRange);
            if (code == 403) return new HandleState(403, "");
            return new HandleState();
        }

        private int GetPermissionToUpdate(ModelUpdate model, PermissionRange permissionRange, int? flagDetail)
        {
            int code = PermissionEx.GetPermissionToUpdate(model, permissionRange, currentUser, flagDetail);
            return code;
        }

        private int GetPermissionToDelete(ModelUpdate model, PermissionRange permissionRange)
        {
            int code = PermissionEx.GetPermissionToDelete(model, permissionRange, currentUser);
            return code;
        }

        private IQueryable<CatPartnerViewModel> QueryPaging(CatPartnerCriteria criteria)
        {
            string partnerGroup = criteria != null ? PlaceTypeEx.GetPartnerGroup(criteria.PartnerGroup) : null;
            var sysUsers = sysUserRepository.Get();
            var agreementData = contractRepository.Get();
            string salemans = string.IsNullOrEmpty(criteria.Saleman) ? criteria.All : criteria.Saleman;
            var SalemanId = sysUsers.Where(x => salemans.Contains(x.Username)).Select(t => t.Id).ToList();
            var offices = officeRepository.Get();

            string ContractType = string.IsNullOrEmpty(criteria.ContractType) ? criteria.All : criteria.ContractType.Trim();
            ClearCache();
            var partners = Get(x => (x.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
            if (partners == null) return null;

            var query = (from partner in partners
                         join user in sysUsers on partner.UserCreated equals user.Id
                         join saleman in sysUsers on partner.SalePersonId equals saleman.Id into prods
                         from x in prods.DefaultIfEmpty()
                         join agreement in agreementData on partner.Id equals agreement.PartnerId into agreements
                         from agreement in agreements.DefaultIfEmpty()
                         join office in offices on partner.OfficeId equals office.Id into officeGr
                         from office in officeGr.DefaultIfEmpty()
                         select new { user, partner, x, agreement, office.ShortName }
                        );
            // Allow search partner when don't have contract
            //if (!string.IsNullOrEmpty(criteria.PartnerType))
            //{
            //    query = query.Where(x => x.agreement != null && x.agreement.Id != null);
            //}
            if (string.IsNullOrEmpty(criteria.All))
            {
                query = query.Where(x => ((x.partner.AccountNo ?? "").IndexOf(criteria.AccountNo ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           && (x.partner.ShortName ?? "").IndexOf(criteria.ShortName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.PartnerNameEn ?? "").IndexOf(criteria.PartnerNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.PartnerNameVn ?? "").IndexOf(criteria.PartnerNameVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.AddressVn ?? "").IndexOf(criteria.AddressVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.TaxCode ?? "").IndexOf(criteria.TaxCode ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.Tel ?? "").IndexOf(criteria.Tel ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.Fax ?? "").IndexOf(criteria.Fax ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.user.Username ?? "").IndexOf(criteria.UserCreated ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           //&& (x.partner.AccountNo ?? "").IndexOf(criteria.AccountNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.CoLoaderCode ?? "").Contains(criteria.CoLoaderCode ?? "", StringComparison.OrdinalIgnoreCase)
                           && (x.partner.PartnerType ?? "").Contains(criteria.PartnerType ?? "", StringComparison.OrdinalIgnoreCase)
                           && (x.partner.Active == criteria.Active || criteria.Active == null)
                           && ((criteria.DatetimeCreatedFrom <= x.partner.DatetimeCreated && x.partner.DatetimeCreated <= criteria.DatetimeCreatedTo) || criteria.DatetimeCreatedFrom == null)
                           ));
                if (SalemanId.Count() > 0 && !string.IsNullOrEmpty(criteria.PartnerType))
                {
                    query = query.Where(x => SalemanId.Any(sm => sm == x.agreement.SaleManId));
                }
                //else if (!string.IsNullOrEmpty(criteria.Saleman))
                //{
                //    query = null;
                //}
                if (!string.IsNullOrEmpty(ContractType))
                {
                    query = query.Where(x => x.agreement.ContractType.ToLower().Contains(ContractType.ToLower()));
                }
                else if (!string.IsNullOrEmpty(criteria.ContractType))
                {
                    query = null;
                }
            }
            else
            {
                query = query.Where(x =>
                           (
                           (x.partner.AccountNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           || (x.partner.ShortName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           || (x.partner.PartnerNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           || (x.partner.PartnerNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           || (x.partner.AddressVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           || (x.partner.TaxCode ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           || (x.partner.Tel ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           || (x.partner.Fax ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           || (x.user.Username ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           //|| (x.partner.CoLoaderCode ?? "").Contains(criteria.All ?? "", StringComparison.OrdinalIgnoreCase)
                           || (x.agreement != null && SalemanId.Any(sm => sm == x.agreement.SaleManId))
                           || (x.agreement != null && x.agreement.ContractType.ToLower().Contains(ContractType.ToLower()))
                           )
                           && (x.partner.Active == criteria.Active || criteria.Active == null)
                           && (x.partner.PartnerType == criteria.PartnerType || criteria.PartnerType == null));
                //if (!string.IsNullOrEmpty(SalemanId))
                //{
                //    query = query.Where(x => x.agreements.Any(y => y.SaleManId == SalemanId));
                //}

            }
            if (query == null) return null;
            var dataGrp = query.GroupBy(x => x.partner.Id).Select(x => x.FirstOrDefault());
            var results = dataGrp.Select(x => new CatPartnerViewModel
            {
                Id = x.partner.Id,
                PartnerGroup = x.partner.PartnerGroup,
                PartnerNameVn = x.partner.PartnerNameVn,
                PartnerNameEn = x.partner.PartnerNameEn,
                ShortName = x.partner.ShortName,
                TaxCode = x.partner.TaxCode,
                SalePersonId = x.partner.SalePersonId,
                Tel = x.partner.Tel,
                AddressEn = x.partner.AddressEn,
                AddressVn = x.partner.AddressVn,
                AddressShippingEn = x.partner.AddressShippingEn,
                AddressShippingVn = x.partner.AddressShippingVn,
                Fax = x.partner.Fax,
                CoLoaderCode = x.partner.CoLoaderCode,
                AccountNo = x.partner.AccountNo,
                UserCreatedName = x.user != null ? x.user.Username : string.Empty,
                SalePersonName = x.x != null ? x.x.Username : string.Empty,
                DatetimeModified = x.partner.DatetimeModified,
                UserCreated = x.partner.UserCreated,
                CompanyId = x.partner.CompanyId,
                DepartmentId = x.partner.DepartmentId,
                GroupId = x.partner.GroupId,
                OfficeId = x.partner.OfficeId,
                Active = x.partner.Active,
                PartnerType = x.partner.PartnerType,
                PartnerMode = x.partner.PartnerMode,
                PartnerLocation = x.partner.PartnerLocation,
                Email = x.partner.Email,
                BillingEmail = x.partner.BillingEmail,
                ContactPerson = x.partner.ContactPerson,
                BankAccountNo = x.partner.BankAccountNo,
                BankAccountName = x.partner.BankAccountName,
                BankName = x.partner.BankName,
                Note = x.partner.Note,
                OfficeName=x.ShortName
            });
            return results;
        }

        public CatPartnerModel GetDetail(string id)
        {
            ClearCache();
            CatPartnerModel queryDetail = Get(x => x.Id == id).FirstOrDefault();
            if (queryDetail == null)
            {
                return null;
            }
            List<CatContract> salemans = contractRepository.Get(x => x.PartnerId == id).ToList();

            ICurrentUser _user = null;
            switch (queryDetail.PartnerType)
            {
                case "Customer":
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialCustomer);
                    break;
                case "Agent":
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialAgent);
                    break;
                default:
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
                    break;
            }

            PermissionRange permissionRangeWrite = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            PermissionRange permissionRangeDelete = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            int checkDelete = GetPermissionToDelete(new ModelUpdate { GroupId = queryDetail.GroupId, OfficeId = queryDetail.OfficeId, CompanyId = queryDetail.CompanyId, UserCreator = queryDetail.UserCreated, Salemans = salemans, PartnerGroup = queryDetail.PartnerGroup }, permissionRangeDelete);

            queryDetail.Permission = new PermissionAllowBase
            {
                AllowUpdate = GetPermissionDetail(permissionRangeWrite, salemans, queryDetail),
                AllowDelete = checkDelete == 403 ? false : true
            };

            if (queryDetail.CountryId != null)
            {
                CatCountry country = catCountryRepository.Get(x => x.Id == queryDetail.CountryId)?.FirstOrDefault();
                queryDetail.CountryName = country.NameEn;
            }
            if (queryDetail.CountryShippingId != null)
            {
                CatCountry country = catCountryRepository.Get(x => x.Id == queryDetail.CountryShippingId)?.FirstOrDefault();
                queryDetail.CountryShippingName = country.NameEn;
            }

            if (queryDetail.ProvinceId != null)
            {
                CatPlaceModel province = placeService.Get(x => x.Id == queryDetail.ProvinceId && x.PlaceTypeId == GetTypeFromData.GetPlaceType(CatPlaceTypeEnum.Province))?.FirstOrDefault();
                queryDetail.ProvinceName = province.NameEn;
            }
            if (queryDetail.ProvinceShippingId != null)
            {
                CatPlaceModel province = placeService.Get(x => x.Id == queryDetail.ProvinceShippingId && x.PlaceTypeId == GetTypeFromData.GetPlaceType(CatPlaceTypeEnum.Province))?.FirstOrDefault();
                queryDetail.ProvinceShippingName = province.NameEn;
            }
            // Get usercreate name
            if (queryDetail.UserCreated != null)
            {
                queryDetail.UserCreatedName = sysUserRepository.Get(x => x.Id == queryDetail.UserCreated)?.FirstOrDefault()?.Username;
            }
            // Get usermodified name
            if (queryDetail.UserCreated != null)
            {
                queryDetail.UserModifiedName = sysUserRepository.Get(x => x.Id == queryDetail.UserModified)?.FirstOrDefault()?.Username;
            }
            return queryDetail;
        }

        private bool GetPermissionDetail(PermissionRange permissionRangeWrite, List<CatContract> salemans, CatPartnerModel detail)
        {
            bool result = false;
            switch (permissionRangeWrite)
            {
                case PermissionRange.None:
                    result = false;
                    break;
                case PermissionRange.All:
                    result = true;
                    break;
                case PermissionRange.Owner:
                    if (salemans.Any(y => y.SaleManId == currentUser.UserID && y.PartnerId.Equals(detail.Id)) || detail.UserCreated == currentUser.UserID)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                case PermissionRange.Group:
                    if ((detail.GroupId == currentUser.GroupId && detail.DepartmentId == currentUser.DepartmentId && detail.OfficeId == currentUser.OfficeID && detail.CompanyId == currentUser.CompanyID || detail.UserCreated == currentUser.UserID)
                     )
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                case PermissionRange.Department:
                    if ((detail.DepartmentId == currentUser.DepartmentId && detail.OfficeId == currentUser.OfficeID && detail.CompanyId == currentUser.CompanyID) || salemans.Any(y => y.SaleManId == currentUser.UserID && y.PartnerId.Equals(detail.Id)) || detail.UserCreated == currentUser.UserID)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                case PermissionRange.Office:
                    if ((detail.OfficeId == currentUser.OfficeID && detail.CompanyId == currentUser.CompanyID) || salemans.Any(y => y.SaleManId == currentUser.UserID && y.PartnerId.Equals(detail.Id)) || detail.UserCreated == currentUser.UserID)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                case PermissionRange.Company:
                    if (detail.CompanyId == currentUser.CompanyID || salemans.Any(y => y.SaleManId == currentUser.UserID && y.PartnerId.Equals(detail.Id)) || detail.UserCreated == currentUser.UserID)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
            }
            return result;
        }

        #region import
        public HandleState Import(List<CatPartnerImportModel> data)
        {
            try
            {
                var partners = new List<CatPartner>();
                var salesmans = new List<CatContract>();
                foreach (var item in data)
                {
                    bool active = string.IsNullOrEmpty(item.Status) || (item.Status.ToLower() == "active");
                    DateTime? inactiveDate = active == false ? (DateTime?)DateTime.Now : null;
                    var partner = mapper.Map<CatPartner>(item);
                    partner.UserCreated = partner.UserModified = currentUser.UserID;
                    partner.DatetimeModified = DateTime.Now;
                    partner.DatetimeCreated = DateTime.Now;
                    partner.Id = Guid.NewGuid().ToString();
                    partner.AccountNo = partner.TaxCode;
                    if (!string.IsNullOrEmpty(item.AcReference))
                    {
                        partner.ParentId = DataContext.Get(x => x.AccountNo == item.AcReference).Select(x => x.Id)?.FirstOrDefault();
                    }
                    partner.Active = active;
                    partner.InactiveOn = inactiveDate;
                    partner.CompanyId = currentUser.CompanyID;
                    partner.OfficeId = currentUser.OfficeID;
                    partner.GroupId = currentUser.GroupId;
                    partner.DepartmentId = currentUser.DepartmentId;
                    partner.PartnerType = "Supplier";
                    partners.Add(partner);
                }
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        var hs = DataContext.Add(partners);
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
                        ClearCache();
                        Get();
                        trans.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public HandleState ImportCustomerAgent(List<CatPartnerImportModel> data, string type)
        {
            try
            {
                var partners = new List<CatPartner>();
                var salesmans = new List<CatContract>();
                foreach (var item in data)
                {
                    DateTime? inactiveDate = DateTime.Now;
                    var partner = mapper.Map<CatPartner>(item);
                    partner.UserCreated = partner.UserModified = currentUser.UserID;
                    partner.DatetimeModified = DateTime.Now;
                    partner.DatetimeCreated = DateTime.Now;

                    partner.Id = Guid.NewGuid().ToString();
                    partner.AccountNo = partner.TaxCode;
                    if (!string.IsNullOrEmpty(partner.AccountNo) && !string.IsNullOrEmpty(item.InternalReferenceNo))
                    {
                        partner.AccountNo = partner.AccountNo + "." + item.InternalReferenceNo;
                    }
                    partner.Active = true;
                    partner.InactiveOn = inactiveDate;
                    partner.CompanyId = currentUser.CompanyID;
                    partner.OfficeId = currentUser.OfficeID;
                    partner.GroupId = currentUser.GroupId;
                    partner.DepartmentId = currentUser.DepartmentId;
                    partner.PartnerGroup = type == "Customer" ? "CUSTOMER" : "CUSTOMER;AGENT";
                    partner.PartnerType = type == "Customer" ? "Customer" : "Agent";
                    if (!string.IsNullOrEmpty(item.AcReference))
                    {
                        partner.ParentId = DataContext.Get(x => x.AccountNo == item.AcReference).Select(x => x.Id)?.FirstOrDefault();
                    }
                    partner.SalePersonId = sysUserRepository.Get(x => x.Username == item.SaleManName).Select(x => x.Id).FirstOrDefault();
                    partners.Add(partner);
                }
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        var hs = DataContext.Add(partners);
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
                        ClearCache();
                        Get();
                        trans.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public List<CatPartnerImportModel> CheckValidImport(List<CatPartnerImportModel> list)
        {
            ClearCache();
            var partners = Get().ToList();
            var users = sysUserRepository.Get().ToList();
            var countries = countryService.Get().ToList();
            var provinces = placeService.Get(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Province)).ToList();
            var branchs = placeService.Get(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Branch)).ToList();
            var salemans = sysUserRepository.Get().ToList();
            var offices = officeRepository.Get().ToList();
            var regexItem = new Regex("^[a-zA-Z0-9-]+$");
            var paymentTerms = new List<string> { "All", "Prepaid", "Collect" };
            var services = API.Common.Globals.CustomData.Services;

            var allGroup = DataEnums.PARTNER_GROUP;
            var partnerGroups = allGroup.Split(";");
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.TaxCode))
                {
                    item.TaxCodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_TAXCODE_EMPTY]);
                    item.IsValid = false;
                }
                else
                {
                    string taxCode = item.TaxCode;
                    var asciiBytesCount = Encoding.ASCII.GetByteCount(taxCode);
                    var unicodBytesCount = Encoding.UTF8.GetByteCount(taxCode);
                    if (asciiBytesCount != unicodBytesCount || !regexItem.IsMatch(taxCode))
                    {
                        item.TaxCodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_TAXCODE_INVALID], item.TaxCode);
                        item.IsValid = false;
                    }
                    else if (list.Count(x => x.TaxCode == taxCode) > 1)
                    {
                        item.TaxCodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_TAXCODE_DUPLICATED]);
                        item.IsValid = false;
                    }
                    else
                    {
                        if (partners.Any(x => x.TaxCode?.Replace(" ", "") == taxCode))
                        {
                            item.TaxCodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_TAXCODE_EXISTED], item.TaxCode);
                            item.IsValid = false;
                        }
                    }

                    if (taxCode.Length < 8 || taxCode.Length > 14)
                    {
                        item.TaxCodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_TAXCODE_LENGTH], item.TaxCode);
                        item.IsValid = false;
                    }
                    if (taxCode.Any(x => Char.IsWhiteSpace(x)))
                    {
                        item.TaxCodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_TAXCODE_SPACE], item.TaxCode);
                        item.IsValid = false;
                    }
                }
                if (string.IsNullOrEmpty(item.PartnerGroup))
                {
                    item.PartnerGroupError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_GROUP_EMPTY]);
                    item.IsValid = false;
                }
                else
                {
                    item.PartnerGroup = item.PartnerGroup.ToUpper();
                    if (item.PartnerGroup == DataEnums.AllPartner)
                    {
                        item.PartnerGroup = allGroup;
                    }
                    else
                    {
                        var groups = item.PartnerGroup.Split(";").Select(x => x.Trim());
                        var group = partnerGroups.Intersect(groups);
                        if (group.Count() == 0)
                        {
                            item.PartnerGroupError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_GROUP_NOT_FOUND], item.PartnerGroup);
                            item.IsValid = false;
                        }
                        else
                        {
                            item.PartnerGroup = String.Join(";", groups);
                        }
                    }
                    //item = GetSaleManInfo(item, salemans, offices, services);
                }
                if (string.IsNullOrEmpty(item.PartnerLocation))
                {
                    item.PartnerLocationError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_PARTNER_LOCATION_EMPTY]);
                    item.IsValid = false;
                }

                if (!string.IsNullOrEmpty(item.PartnerMode) && item.PartnerMode == "Internal")
                {
                    if (string.IsNullOrEmpty(item.InternalCode))
                    {
                        item.PartnerInternalCodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_INTERNAL_CODE_EMPTY]);
                        item.IsValid = false;
                    }
                }

                if (string.IsNullOrEmpty(item.PartnerNameEn))
                {
                    item.PartnerNameEnError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_NAME_EN_EMPTY]);
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.PartnerNameVn))
                {
                    item.PartnerNameVnError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_NAME_VN_EMPTY]);
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.ShortName))
                {
                    item.ShortNameError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_SHORT_NAME_EMPTY]);
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.AddressEn))
                {
                    item.AddressEnError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_ADDRESS_BILLING_EN_NOT_FOUND];
                    item.IsValid = false;

                }

                if (string.IsNullOrEmpty(item.AddressVn))
                {
                    item.AddressVnError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_ADDRESS_BILLING_VN_NOT_FOUND];
                    item.IsValid = false;
                }

                if (string.IsNullOrEmpty(item.AddressShippingVn))
                {
                    item.AddressShippingVnError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_ADDRESS_SHIPPING_VN_NOT_FOUND];
                    item.IsValid = false;
                }

                if (string.IsNullOrEmpty(item.AddressShippingEn))
                {
                    item.AddressShippingEnError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_ADDRESS_SHIPPING_EN_NOT_FOUND];
                    item.IsValid = false;
                }

                if (!string.IsNullOrEmpty(item.AcReference))
                {
                    if (!partners.Any(x => x.AccountNo?.ToLower() == item.AcReference?.ToLower()))
                    {
                        item.AcReferenceError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_AC_REFERENCE_NOT_FOUND], item.AcReference);
                        item.IsValid = false;
                    }
                }

                if (string.IsNullOrEmpty(item.CountryBilling))
                {
                    if (!string.IsNullOrEmpty(item.CityBilling))
                    {
                        item.CityBillingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_PROVINCE_REQUIRED_COUNTRY], item.CityBilling);
                        item.IsValid = false;
                    }
                    item.CountryBillingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_COUNTRY_BILLING_EMPTY]);
                    item.IsValid = false;
                }
                else
                {
                    string countryBilling = item.CountryBilling?.ToLower();
                    var country = countries.FirstOrDefault(i => i.NameEn.ToLower() == countryBilling);
                    if (country != null)
                    {
                        item.CountryId = country.Id;
                        if (!string.IsNullOrEmpty(item.CityBilling))
                        {
                            string cityBilling = item.CityBilling.ToLower();
                            var province = provinces.FirstOrDefault(i => i.NameEn.ToLower() == cityBilling && i.CountryId == country.Id);
                            if (province == null)
                            {
                                item.CityBillingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_PROVINCE_BILLING_NOT_FOUND], item.CityBilling);
                                item.IsValid = false;
                            }
                            else
                            {
                                item.ProvinceId = province.Id;
                            }
                        }
                    }
                    else
                    {
                        item.CountryBillingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_COUNTRY_BILLING_NOT_FOUND], item.CountryBilling);
                        item.IsValid = false;
                    }
                }
                if (string.IsNullOrEmpty(item.CountryShipping))
                {
                    if (!string.IsNullOrEmpty(item.CityShipping))
                    {
                        item.CityShippingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_PROVINCE_REQUIRED_COUNTRY], item.CityShipping);
                        item.IsValid = false;
                    }
                    item.CountryShippingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_COUNTRY_SHIPPING_EMPTY]);
                    item.IsValid = false;
                }
                else
                {
                    string countShipping = item.CountryShipping?.ToLower();
                    var country = countries.FirstOrDefault(i => i.NameEn.ToLower() == countShipping);
                    if (country != null)
                    {
                        item.CountryShippingId = country.Id;
                        if (!string.IsNullOrEmpty(item.CityShipping))
                        {
                            string cityShipping = item.CityShipping.ToLower();
                            var province = provinces.FirstOrDefault(i => i.NameEn.ToLower() == cityShipping && i.CountryId == country.Id);
                            if (province == null)
                            {
                                item.CityShippingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_PROVINCE_SHIPPING_NOT_FOUND], item.CityShipping);
                                item.IsValid = false;
                            }
                            else
                            {
                                item.ProvinceShippingId = province.Id;
                            }
                        }
                    }
                    else
                    {
                        item.CountryShippingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_COUNTRY_SHIPPING_NOT_FOUND], item.CountryShipping);
                        item.IsValid = false;
                    }
                }
            });
            return list;
        }

        public List<CatPartnerImportModel> CheckValidCustomerAgentImport(List<CatPartnerImportModel> list)
        {
            var partners = Get().ToList();
            var users = sysUserRepository.Get().ToList();
            var countries = countryService.Get().ToList();
            var provinces = placeService.Get(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Province)).ToList();
            var branchs = placeService.Get(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Branch)).ToList();
            var salemans = sysUserRepository.Get().ToList();
            var offices = officeRepository.Get().ToList();
            var regexItem = new Regex("^[a-zA-Z0-9-]+$");
            var paymentTerms = new List<string> { "All", "Prepaid", "Collect" };

            var allGroup = DataEnums.PARTNER_GROUP;
            var partnerGroups = allGroup.Split(";");
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.TaxCode))
                {
                    item.TaxCodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_TAXCODE_EMPTY]);
                    item.IsValid = false;
                }
                else
                {
                    string taxCode = item.TaxCode;
                    string internalReferenceNo = !string.IsNullOrEmpty(item.InternalReferenceNo) ? item.InternalReferenceNo.Replace(" ", "") : string.Empty;

                    var asciiBytesCount = Encoding.ASCII.GetByteCount(taxCode);
                    var unicodBytesCount = Encoding.UTF8.GetByteCount(taxCode);
                    if (asciiBytesCount != unicodBytesCount || !regexItem.IsMatch(taxCode))
                    {
                        item.TaxCodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_TAXCODE_INVALID], item.TaxCode);
                        item.IsValid = false;
                    }
                    else if (list.Count(x => x.TaxCode == taxCode) > 1)
                    {
                        item.TaxCodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_TAXCODE_DUPLICATED]);
                        item.IsValid = false;
                    }
                    else
                    {
                        if (partners.Any(x => x.TaxCode?.Replace(" ", "") == taxCode))
                        {
                            item.TaxCodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_TAXCODE_EXISTED], item.TaxCode);
                            item.IsValid = false;
                        }
                    }
                    if (taxCode.Length < 8 || taxCode.Length > 14)
                    {
                        item.TaxCodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_TAXCODE_LENGTH]);
                        item.IsValid = false;
                    }
                    if (taxCode.Any(x => Char.IsWhiteSpace(x)))
                    {
                        item.TaxCodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_TAXCODE_SPACE], item.TaxCode);
                        item.IsValid = false;
                    }
                }
                if (!string.IsNullOrEmpty(item.InternalReferenceNo))
                {
                    string internalReferenceNo = item.InternalReferenceNo.Replace(" ", "");
                    var asciiBytesCount = Encoding.ASCII.GetByteCount(internalReferenceNo);
                    var unicodBytesCount = Encoding.UTF8.GetByteCount(internalReferenceNo);
                    if (asciiBytesCount != unicodBytesCount || !regexItem.IsMatch(internalReferenceNo))
                    {
                        item.TaxCodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_INTERNAL_REFERENCENO_INVALID], item.InternalReferenceNo);
                        item.IsValid = false;
                    }
                    if (internalReferenceNo.Length < 3 || internalReferenceNo.Length > 10)
                    {
                        item.InternalReferenceNoError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_INTERNAL_REFERENCENO_LENGTH]);
                        item.IsValid = false;
                    }
                }

                if (!string.IsNullOrEmpty(item.AcReference))
                {
                    if (!partners.Any(x => x.AccountNo?.ToLower() == item.AcReference?.ToLower()))
                    {
                        item.AcReferenceError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_AC_REFERENCE_NOT_FOUND], item.AcReference);
                        item.IsValid = false;
                    }
                }
                if (string.IsNullOrEmpty(item.PartnerNameEn))
                {
                    item.PartnerNameEnError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_NAME_EN_EMPTY]);
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.PartnerNameVn))
                {
                    item.PartnerNameVnError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_NAME_VN_EMPTY]);
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.ShortName))
                {
                    item.ShortNameError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_SHORT_NAME_EMPTY]);
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.AddressEn))
                {
                    item.AddressEnError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_ADDRESS_BILLING_EN_NOT_FOUND];
                    item.IsValid = false;

                }
                if (string.IsNullOrEmpty(item.AddressVn))
                {
                    item.AddressVnError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_ADDRESS_BILLING_VN_NOT_FOUND];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.AddressShippingVn))
                {
                    item.AddressShippingVnError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_ADDRESS_SHIPPING_VN_NOT_FOUND];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.AddressShippingEn))
                {
                    item.AddressShippingEnError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_ADDRESS_SHIPPING_EN_NOT_FOUND];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.PartnerLocation))
                {
                    item.PartnerLocationError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_PARTNER_LOCATION_EMPTY]);
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CountryBilling))
                {
                    if (!string.IsNullOrEmpty(item.CityBilling))
                    {
                        item.CityBillingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_PROVINCE_REQUIRED_COUNTRY], item.CityBilling);
                        item.IsValid = false;
                    }
                    item.CountryBillingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_COUNTRY_BILLING_EMPTY]);
                    item.IsValid = false;
                }
                else
                {
                    string countryBilling = item.CountryBilling?.ToLower();
                    var country = countries.FirstOrDefault(i => i.NameEn.ToLower() == countryBilling);
                    if (country != null)
                    {
                        item.CountryId = country.Id;
                        if (!string.IsNullOrEmpty(item.CityBilling))
                        {
                            string cityBilling = item.CityBilling.ToLower();
                            var province = provinces.FirstOrDefault(i => i.NameEn.ToLower() == cityBilling && i.CountryId == country.Id);
                            if (province == null)
                            {
                                item.CityBillingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_PROVINCE_BILLING_NOT_FOUND], item.CityBilling);
                                item.IsValid = false;
                            }
                            else
                            {
                                item.ProvinceId = province.Id;
                            }
                        }
                    }
                    else
                    {
                        item.CountryBillingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_COUNTRY_BILLING_NOT_FOUND], item.CountryBilling);
                        item.IsValid = false;
                    }
                }
                if (string.IsNullOrEmpty(item.CountryShipping))
                {
                    if (!string.IsNullOrEmpty(item.CityShipping))
                    {
                        item.CityShippingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_PROVINCE_REQUIRED_COUNTRY], item.CityShipping);
                        item.IsValid = false;
                    }
                    item.CountryShippingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_COUNTRY_SHIPPING_EMPTY]);
                    item.IsValid = false;
                }
                else
                {
                    string countShipping = item.CountryShipping?.ToLower();
                    var country = countries.FirstOrDefault(i => i.NameEn.ToLower() == countShipping);
                    if (country != null)
                    {
                        item.CountryShippingId = country.Id;
                        if (!string.IsNullOrEmpty(item.CityShipping))
                        {
                            string cityShipping = item.CityShipping.ToLower();
                            var province = provinces.FirstOrDefault(i => i.NameEn.ToLower() == cityShipping && i.CountryId == country.Id);
                            if (province == null)
                            {
                                item.CityShippingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_PROVINCE_SHIPPING_NOT_FOUND], item.CityShipping);
                                item.IsValid = false;
                            }
                            else
                            {
                                item.ProvinceShippingId = province.Id;
                            }
                        }
                    }
                    else
                    {
                        item.CountryShippingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_COUNTRY_SHIPPING_NOT_FOUND], item.CountryShipping);
                        item.IsValid = false;
                    }
                }
            });
            return list;
        }

        private CatPartnerImportModel GetSaleManInfo(CatPartnerImportModel item, List<SysUser> salemans, List<SysOffice> offices, List<API.Common.Globals.CommonData> services)
        {
            if (item.PartnerGroup.Contains(DataEnums.CustomerPartner))
            {
                if (string.IsNullOrEmpty(item.SaleManName))
                {
                    item.SaleManNameError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_SALEMAN_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    var salePersonId = salemans.FirstOrDefault(i => i.Username == item.SaleManName && i.Active == true)?.Id;
                    if (string.IsNullOrEmpty(salePersonId))
                    {
                        item.SaleManNameError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_SALEMAN_NOT_FOUND], item.SaleManName);
                        item.IsValid = false;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(item.OfficeSalemanDefault))
                        {
                            item.OfficeSalemanDefaultError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_SALEMAN_DEFAULT_OFFICE_NOT_ALLOW_EMPTY];
                            item.IsValid = false;
                        }
                        else if (string.IsNullOrEmpty(item.ServiceSalemanDefault))
                        {
                            item.ServiceSalemanDefaultError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_SALEMAN_DEFAULT_SERVICE_NOT_ALLOW_EMPTY];
                            item.IsValid = false;
                        }
                        else
                        {
                            var office = offices.FirstOrDefault(x => x.Code.ToLower() == item.OfficeSalemanDefault.ToLower());
                            var service = services.FirstOrDefault(x => x.DisplayName == item.ServiceSalemanDefault)?.Value;
                            if (office == null)
                            {
                                item.OfficeSalemanDefaultError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_SALEMAN_DEFAULT_OFFICE_NOT_FOUND], item.OfficeSalemanDefault);
                                item.IsValid = false;
                            }
                            else if (service == null)
                            {
                                item.ServiceSalemanDefaultError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_SALEMAN_DEFAULT_SERVICE_NOT_FOUND], item.ServiceSalemanDefaultError);
                                item.IsValid = false;
                            }
                            else
                            {
                                item.OfficeId = office.Id;
                                item.CompanyId = office.Buid;
                                item.SalePersonId = salePersonId;
                                item.ServiceId = service;
                            }
                        }
                    }
                }
            }
            return item;
        }
        #endregion

        private string GetContractOfficeName(string officeIds)
        {
            string officeName = string.Empty;

            var officeIdArr = officeIds.Split(";").ToArray();
            if (officeIdArr.Any())
            {
                foreach (var officeId in officeIdArr)
                {
                    var _office = officeRepository.Get(x => x.Id.ToString() == officeId)?.FirstOrDefault();
                    if(_office != null)
                    {
                        officeName += _office.ShortName +"; ";
                    }
                }
            }

            if (!string.IsNullOrEmpty(officeName))
            {
                officeName = officeName.Remove(officeName.Length - 2);
            }

            return officeName;
        }
        private string GetContractServicesName(string ContractService)
        {
            string ContractServicesName = string.Empty;
            var ContractServiceArr = ContractService.Split(";").ToArray();
            if (ContractServiceArr.Any())
            {
                foreach (var item in ContractServiceArr)
                {
                    switch (item)
                    {
                        case "AE":
                            ContractServicesName += "Air Export; ";
                            break;
                        case "AI":
                            ContractServicesName += "Air Import; ";
                            break;
                        case "SCE":
                            ContractServicesName += "Sea Consol Export; ";
                            break;
                        case "SCI":
                            ContractServicesName += "Sea Consol Import; ";
                            break;
                        case "SFE":
                            ContractServicesName += "Sea FCL Export; ";
                            break;
                        case "SLE":
                            ContractServicesName += "Sea LCL Export; ";
                            break;
                        case "SLI":
                            ContractServicesName += "Sea LCL Import; ";
                            break;
                        case "CL":
                            ContractServicesName += "Custom Logistic; ";
                            break;
                        case "IT":
                            ContractServicesName += "Trucking; ";
                            break;
                        case "SFI":
                            ContractServicesName += "Sea FCL Import; ";
                            break;
                        default:
                            ContractServicesName = "Air Export; Air Import; Sea Consol Export; Sea Consol Import; Sea FCL Export; Sea LCL Export; Sea LCL Import; Custom Logistic; Trucking  ";
                            break;
                    }
                }

            }
            if (!string.IsNullOrEmpty(ContractServicesName))
            {
                ContractServicesName = ContractServicesName.Remove(ContractServicesName.Length - 2);
            }
            return ContractServicesName;
        }

        public IQueryable<CatPartnerModel> GetBy(CatPartnerGroupEnum partnerGroup)
        {
            ClearCache();
            string group = PlaceTypeEx.GetPartnerGroup(partnerGroup);
            IQueryable<CatPartnerModel> data = Get().Where(x => (x.PartnerGroup ?? "").IndexOf(group ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
            return data;
        }

        public IQueryable<CatPartnerViewModel2> GetMultiplePartnerGroup(PartnerMultiCriteria criteria)
        {
            IQueryable<CatPartner> data;
            List<string> grpCodes = new List<string>();
            if (criteria.PartnerGroups != null)
            {
                foreach (var grp in criteria.PartnerGroups)
                {
                    string group = PlaceTypeEx.GetPartnerGroup(grp);
                    grpCodes.Add(group);
                }
                Expression<Func<CatPartner, bool>> query = null;
                foreach (var group in grpCodes.Distinct())
                {
                    if (query == null)
                    {
                        query = x => (x.PartnerGroup ?? "").IndexOf(group ?? "", StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                    else
                    {
                        query = query.Or(x => (x.PartnerGroup ?? "").IndexOf(group ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
                    }
                }
                query = criteria.Active != null ? query.And(x => x.Active == criteria.Active) : query;


                data = DataContext.Get(query);
                Expression<Func<CatContract, bool>> queryContract = x => x.Active == true && (x.IsExpired == null || x.IsExpired == false);
                if(!string.IsNullOrEmpty(criteria.Service))
                {
                    queryContract = queryContract.And(x => IsMatchService(x.SaleService, criteria.Service));
                }

                if (!string.IsNullOrEmpty(criteria.Office))
                {
                    queryContract = queryContract.And(x => IsMatchOffice(x.OfficeId, criteria.Office));
                }

                if(criteria.SalemanId != null)
                {
                    queryContract = queryContract.And(x => x.SaleManId == criteria.SalemanId);
                }

                IQueryable<CatContract> contracts = contractRepository.Get(queryContract);
                data = from p in data
                       join c in contracts on p.Id equals c.PartnerId into pGrps
                       from pgrp in pGrps.DefaultIfEmpty()
                       select p;

            }
            else
            {
                data = DataContext.Where(x => x.Active == criteria.Active || criteria.Active == null);
            }
            if (data == null) return null;
            var results = data.Select(x => new CatPartnerViewModel2
            {
                Id = x.Id,
                PartnerGroup = x.PartnerGroup,
                PartnerNameVn = x.PartnerNameVn,
                PartnerNameEn = x.PartnerNameEn,
                ShortName = x.ShortName,
                TaxCode = x.TaxCode,
                SalePersonId = x.SalePersonId,
                Tel = x.Tel,
                AddressEn = x.AddressEn,
                Fax = x.Fax,
                CoLoaderCode = x.CoLoaderCode,
                RoundUpMethod = x.RoundUpMethod,
                ApplyDim = x.ApplyDim,
                AccountNo = x.AccountNo,
                PartnerType = x.PartnerType,
                TaxCodeAbbrName = x.AccountNo + " - " + x.ShortName
            }).ToList();
            return results.AsQueryable();
        }

        private bool IsMatchService(string saleService, string serviceTerm)
        {
            bool isMatch = true;

            if (!string.IsNullOrEmpty(saleService) && !string.IsNullOrEmpty(serviceTerm))
            {
                var serviceList = saleService.ToLower().Split(";").ToList();
                if (serviceList.Count > 0)
                {
                    isMatch = serviceList.Any(z => z == serviceTerm.ToLower());
                }
            }

            return isMatch;
        }

        private bool IsMatchOffice(string saleOffice, string officeTerm)
        {
            bool isMatch = true;

            if (!string.IsNullOrEmpty(saleOffice) && !string.IsNullOrEmpty(officeTerm))
            {
                var officeList = saleOffice.ToLower().Split(";").ToList();
                if (officeList.Count > 0)
                {
                    isMatch = officeList.Any(z => z == officeTerm.ToLower());
                }
            }

            return isMatch;
        }

        public IQueryable<CatPartnerViewModel> Query(CatPartnerCriteria criteria)
        {
            //ClearCache();
            //string partnerGroup = criteria != null ? PlaceTypeEx.GetPartnerGroup(criteria.PartnerGroup) : null;
            //IQueryable<CatPartnerModel> data = Get().Where(x => (x.PartnerGroup ?? "").Contains(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase)
            //                    && (x.Active == criteria.Active || criteria.Active == null)
            //                    && (x.CoLoaderCode ?? "").Contains(criteria.CoLoaderCode ?? "", StringComparison.OrdinalIgnoreCase));
            //if (!string.IsNullOrEmpty(criteria.ExceptId))
            //{
            //    data = data.Where(x => x.ParentId != criteria.ExceptId);
            //}
            //if (!string.IsNullOrEmpty(criteria.PartnerMode))
            //{
            //    data = data.Where(x => x.PartnerMode == criteria.PartnerMode);
            //}
            //if (data == null) return null;
            string partnerGroup = criteria != null ? PlaceTypeEx.GetPartnerGroup(criteria.PartnerGroup) : null;
            Expression<Func<CatPartner, bool>> query = q => true;
            if(criteria.Active != null)
            {
                query = query.And(x => x.Active == criteria.Active);
            }
            if (!string.IsNullOrEmpty(partnerGroup))
            {
                query = query.And(x => (x.PartnerGroup ?? "").Contains(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(criteria.CoLoaderCode))
            {
                query = query.And(x => (x.CoLoaderCode ?? "").Contains(criteria.CoLoaderCode ?? "", StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(criteria.ExceptId))
            {
                query = query.And(x => x.ParentId != criteria.ExceptId);
            }
            if (!string.IsNullOrEmpty(criteria.PartnerMode))
            {
                query = query.And(x => x.PartnerMode == criteria.PartnerMode);
            }

            if (!string.IsNullOrEmpty(criteria.NotEqualInternalCode))
            {
                query = query.And(x => !string.IsNullOrEmpty(x.InternalCode) && x.InternalCode != criteria.NotEqualInternalCode);
            }
            if (!string.IsNullOrEmpty(criteria.InternalCode))
            {
                query = query.And(x => x.InternalCode == criteria.InternalCode);
            }

            var dataQuery = DataContext.Get(query);

            IQueryable<CatPartnerViewModel> results = dataQuery.Select(x => new CatPartnerViewModel
            {
                Id = x.Id,
                PartnerGroup = x.PartnerGroup,
                PartnerNameVn = x.PartnerNameVn,
                PartnerNameEn = x.PartnerNameEn,
                ShortName = x.ShortName,
                TaxCode = x.TaxCode,
                SalePersonId = x.SalePersonId,
                Tel = x.Tel,
                AddressEn = x.AddressEn,
                Fax = x.Fax,
                CoLoaderCode = x.CoLoaderCode,
                RoundUpMethod = x.RoundUpMethod,
                ApplyDim = x.ApplyDim,
                AccountNo = x.AccountNo,
                BankAccountNo = x.BankAccountNo,
                BankAccountName = x.BankAccountName,
                TaxCodeAbbrName = x.TaxCode + " - " + x.ShortName,
                BankName = x.BankName,
                BankCode = x.BankCode,
                PartnerType = x.PartnerType,
                InternalCode = x.InternalCode
            });

            return results;
        }

        public List<CatPartnerViewModel> GetSubListPartnerByID(string id)
        {
            if (id == null) return null;
            var data = DataContext.Get(x => x.ParentId == id && x.Id != id);
            if (data == null) return null;
            var results = new List<CatPartnerViewModel>();
            foreach (var partner in data)
            {
                var item = new CatPartnerViewModel()
                {
                    Id = partner.Id,
                    PartnerGroup = partner.PartnerGroup,
                    PartnerNameVn = partner.PartnerNameVn,
                    PartnerNameEn = partner.PartnerNameEn,
                    ShortName = partner.ShortName,
                    TaxCode = partner.TaxCode,
                    SalePersonId = partner.SalePersonId,
                    Tel = partner.Tel,
                    AddressEn = partner.AddressEn,
                    Fax = partner.Fax,
                    CoLoaderCode = partner.CoLoaderCode,
                    RoundUpMethod = partner.RoundUpMethod,
                    ApplyDim = partner.ApplyDim,
                    AccountNo = partner.AccountNo,
                    Active = partner.Active,
                    PartnerType = partner.PartnerType,
                    CountryShippingName = catCountryRepository.Where(k => k.Id == partner.CountryShippingId)?.FirstOrDefault().NameEn
                };
                results.Add(item);
            }
            return results;
        }

        public HandleState UpdatePartnerData(CatPartnerModel model)
        {
            var listSalemans = contractRepository.Get(x => x.PartnerId == model.Id).ToList();
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            var entity = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();

            if (entity == null) return new HandleState(403, "");
            int code = GetPermissionToUpdate(new ModelUpdate { GroupId = entity.GroupId, DepartmentId = entity.DepartmentId, OfficeId = entity.OfficeId, CompanyId = entity.CompanyId, UserCreator = model.UserCreated, Salemans = listSalemans, PartnerGroup = entity.PartnerGroup }, permissionRange, null);
            if (code == 403) return new HandleState(403, "");

            var userUpdated = userlevelRepository.Get(x => x.UserId == model.UserCreated && x.CompanyId == model.CompanyId && x.OfficeId == model.OfficeId && x.GroupId == model.GroupId).FirstOrDefault();
            if (userUpdated == null)
            {
                return new HandleState(false, "Update false, Please try again.");
            }
            entity.UserCreated = userUpdated.UserId; // change creator
            entity.CompanyId = userUpdated.CompanyId;
            entity.OfficeId = userUpdated.OfficeId;
            entity.DepartmentId = userUpdated.DepartmentId;
            entity.GroupId = userUpdated.GroupId;
            var hs = DataContext.Update(entity, x => x.Id == model.Id);
            if (hs.Success)
            {
                ClearCache();
                Get();
            }
            return hs;
        }

        public List<SysUserViewModel> GetListSaleman(string partnerId, string transactionType)
        {
            List<SysUserViewModel> salemans = new List<SysUserViewModel>();
            var contracts = contractRepository.Get(x => x.PartnerId == partnerId 
            && x.OfficeId.Contains(currentUser.OfficeID.ToString())
            && x.SaleService.Contains(transactionType) 
            && x.Active == true);
            if(contracts.Count() > 0)
            {
                var salemansIds = contracts.Select(x => x.SaleManId).ToList();
                var users = sysUserRepository.Get(x => salemansIds.Contains(x.Id));
                var employees = sysEmployeeRepository.Get();


                var userQ = from u in users
                            join em in employees on u.EmployeeId equals em.Id into emGrps
                            from emGrp in emGrps.DefaultIfEmpty()
                            select new SysUserViewModel
                            {
                                Id = u.Id,
                                Active = u.Active,
                                EmployeeNameEn = emGrp.EmployeeNameEn,
                                EmployeeNameVn = emGrp.EmployeeNameVn,
                                StaffCode = emGrp.StaffCode,
                                Status = u.WorkingStatus,
                                Title = emGrp.Title,
                                Username = u.Username,
                                UserType = u.UserType
                            };
                if(userQ.Count() > 0)
                {
                    salemans = userQ.ToList();
                }
            }

            return salemans;
        }

        public IQueryable<CatPartnerForKeyinCharge> GetPartnerForKeyinCharge(PartnerMultiCriteria criteria)
        {
            
            IQueryable<CatPartner> dataAgents = Enumerable.Empty<CatPartner>().AsQueryable();
            IQueryable<CatPartner> dataCustomers = Enumerable.Empty<CatPartner>().AsQueryable();

            Expression<Func<CatPartner, bool>> queryAgent = x => x.Active == true && x.PartnerType == DataEnums.PARTNER_TYPE_AGENT;
            Expression<Func<CatPartner, bool>> queryCustomer = x => x.Active == true && x.PartnerType == DataEnums.PARTNER_TYPE_CUSTOMER;


            dataAgents = DataContext.Get(queryAgent);
            dataCustomers = DataContext.Get(queryCustomer);
            Expression<Func<CatContract, bool>> queryContract = x => x.Active == true
                                                            && (x.IsExpired == null || x.IsExpired == false)
                                                            && IsMatchService(x.SaleService, criteria.Service)
                                                            && IsMatchOffice(x.OfficeId, criteria.Office);
           
            IQueryable<CatContract> contractAgents = contractRepository.Get(queryContract);

            var d = from p in dataAgents
                    join c in contractAgents on p.Id equals c.PartnerId
                    select new CatPartnerForKeyinCharge
                    {
                        Id = p.Id,
                        PartnerGroup = p.PartnerGroup,
                        PartnerNameVn = p.PartnerNameVn,
                        PartnerNameEn = p.PartnerNameEn,
                        ShortName = p.ShortName,
                        TaxCode = p.TaxCode,
                        AccountNo = p.AccountNo,
                        PartnerType = p.PartnerType,
                    };

            if (criteria.SalemanId != null)
            {
                queryContract = queryContract.And(x => x.SaleManId == criteria.SalemanId);
            }
            IQueryable<CatContract> contractCustomers = contractRepository.Get(queryContract);

            var d2 = from p in dataCustomers
                    join c in contractCustomers on p.Id equals c.PartnerId
                    select new CatPartnerForKeyinCharge
                    {
                        Id = p.Id,
                        PartnerGroup = p.PartnerGroup,
                        PartnerNameVn = p.PartnerNameVn,
                        PartnerNameEn = p.PartnerNameEn,
                        ShortName = p.ShortName,
                        TaxCode = p.TaxCode,
                        AccountNo = p.AccountNo,
                        PartnerType = p.PartnerType,
                    };

            var results = d.Union(d2);
            return results;
        }
    }
}
