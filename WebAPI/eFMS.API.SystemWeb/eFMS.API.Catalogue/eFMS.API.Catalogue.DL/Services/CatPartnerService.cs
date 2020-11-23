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
        private readonly IOptions<ApiUrl> ApiUrl;


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
            ApiUrl = apiurl;
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
                                foreach (var item in entity.Contracts)
                                {
                                    if (item.IsRequestApproval == true)
                                    {
                                        entity.ContractService = item.SaleService;
                                        entity.ContractType = item.ContractType;
                                        entity.SalesmanId = item.SaleManId;
                                        entity.UserCreated = partner.UserCreated;
                                        SendMailRequestApproval(entity);
                                    }
                                }
                            }

                        }
                        SendMailCreatedSuccess(partner);
                    }
                    ClearCache();
                    Get();
                    var result = hsTransPartner;
                    trans.Commit();
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

            ApiUrl.Value.Url = ApiUrl.Value.Url.Replace("Catalogue", "");
            body = body.Replace("[logoEFMS]", ApiUrl.Value.Url.ToString() + "/ReportPreview/Images/logo-eFMS.png");

            List<string> lstCc = ListMailCC();
            List<string> lstTo = new List<string>();

            lstTo.Add(creatorObj?.Email);

            //return SendMail.Send(subject, body, lstTo, null, null, lstCc);

            bool result = SendMail.Send(subject, body, lstTo, null, null, lstCc);
            var logSendMail = new SysSentEmailHistory
            {
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

        private void SendMailRequestApproval(CatPartnerModel partner)
        {
            string employeeId = sysUserRepository.Get(x => x.Id == partner.UserCreated).Select(t => t.EmployeeId).FirstOrDefault();
            var objInfoCreator = sysEmployeeRepository.Get(e => e.Id == employeeId)?.FirstOrDefault();
            string EnNameCreatetor = objInfoCreator?.EmployeeNameEn;
            List<string> lstTo = new List<string>();

            // info send to and cc
            var listEmailAR = catDepartmentRepository.Get(x => x.DeptType == "AR" && x.BranchId == currentUser.OfficeID)?.Select(t => t.Email).FirstOrDefault();

            if (listEmailAR != null && listEmailAR.Any())
            {
                lstTo = listEmailAR.Split(";").ToList();
            }
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
            string linkVn = string.Empty;
            string linkEn = string.Empty;
            string subject = string.Empty;
            string body = string.Empty;
            string address = webUrl.Value.Url + "/en/#/" + url + partner.Id;


            linkEn = "You can <a href='" + address + "'> click here </a>" + "to view detail.";
            linkVn = "Bạn click <a href='" + address + "'> vào đây </a>" + "để xem chi tiết.";
            subject = "eFMS - Customer Approval Request From " + EnNameCreatetor;

            body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt; color:#004080;'> Dear Accountant/AR Team, " + " </br> </br>" +

              "<i> You have a Customer Approval request from " + EnNameCreatetor + " as info below </i> </br>" +
              "<i> Bạn có một yêu cầu xác duyệt khách hàng từ " + EnNameCreatetor + " với thông tin như sau: </i> </br> </br>" +

              "\t  Customer ID  / <i> Mã Agent:</i> " + "<b>" + partner.AccountNo + "</b>" + "</br>" +
              "\t  Customer Name  / <i> Tên khách hàng:</i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>" +
              "\t  Taxcode / <i> Mã số thuế: </i>" + "<b>" + partner.TaxCode + "</b>" + "</br>" +

              "\t  Service  / <i> Dịch vụ: </i>" + "<b>" + partner.ContractService + "</b>" + "</br>" +
              "\t  Contract type  / <i> Loại hợp đồng: </i> " + "<b>" + partner.ContractType + "</b>" + "</br>" +
              "\t  Contract No  / <i> Số hợp đồng: </i> " + "<b>" + partner.ContractNo + "</b>" + "</br>" +
              "\t  Requestor  / <i> Người yêu cầu: </i> " + "<b>" + EnNameCreatetor + "</b>" + "</br> </br>"

              + linkEn + "</br>" + linkVn + "</br> </br>" +
              "<i> Thanks and Regards </i>" + "</br> </br>" +
              "<b> eFMS System, </b>" +
              "</br>"
              + "<p><img src = '[logoEFMS]' /></p> " + " </div>");
            ApiUrl.Value.Url = ApiUrl.Value.Url.Replace("Catalogue", "");
            body = body.Replace("[logoEFMS]", ApiUrl.Value.Url.ToString() + "/ReportPreview/Images/logo-eFMS.png");


            List<string> lstCc = ListMailCC();

            lstCc.Add(objInfoSalesman?.Email);
            //SendMail.Send(subject, body, lstTo, null, null, lstCc);

            bool result = SendMail.Send(subject, body, lstTo, null, null, lstCc);

            var logSendMail = new SysSentEmailHistory
            {
                Receivers = string.Join("; ", lstTo),
                Ccs = string.Join("; ", lstCc),
                Subject = subject,
                Sent = result,
                SentDateTime = DateTime.Now,
                Body = body
            };
            var hsLogSendMail = sendEmailHistoryRepository.Add(logSendMail);
            var hsSm = sendEmailHistoryRepository.SubmitChanges();

        }


        private void SendMailCreatedSuccess(CatPartner partner)
        {
            string employeeId = sysUserRepository.Get(x => x.Id == currentUser.UserID).Select(t => t.EmployeeId).FirstOrDefault();
            string fullNameCreatetor = sysEmployeeRepository.Get(e => e.Id == employeeId).Select(t => t.EmployeeNameVn)?.FirstOrDefault();
            string url = string.Empty;
            List<string> lstToAR = new List<string>();
            List<string> lstToAccountant = new List<string>();
            List<string> lstCc = ListMailCC();

            // info send to and cc
            var listEmailAR = catDepartmentRepository.Get(x => x.DeptType == "AR" && x.BranchId == currentUser.OfficeID)?.Select(t => t.Email).FirstOrDefault();
            var listEmailAccountant = catDepartmentRepository.Get(x => x.DeptType == "ACCOUNTANT" /*&& x.BranchId == currentUser.OfficeID*/)?.Select(t => t.Email).FirstOrDefault();

            if (listEmailAR != null && listEmailAR.Any())
            {
                lstToAR = listEmailAR.Split(";").ToList();
            }

            if (listEmailAccountant != null && listEmailAccountant.Any())
            {
                lstToAccountant = listEmailAccountant.Split(";").ToList();
            }

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
            string linkEn = "You can <a href='" + address + "'> click here </a>" + "to view detail.";
            string linkVn = "Bạn click <a href='" + address + "'> vào đây </a>" + "để xem chi tiết.";
            string subject = "eFMS - Partner Approval Request From " + fullNameCreatetor;
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt; color:#004080;'> Dear Accountant Team: </br> </br>" +
                "<i> You have a Partner Approval request From " + fullNameCreatetor + " as info bellow: </i> </br>" +
                "<i> Bạn có môt yêu cầu xác duyệt đối tượng từ " + fullNameCreatetor + " với thông tin như sau: </i> </br> </br>" +
                "\t  Partner ID  / <i> Mã đối tượng:</i> " + "<b>" + partner.AccountNo + "</b>" + "</br>" +
                "\t  Partner Name  / <i> Tên đối tượng:</i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>" +
                "\t  Category  / <i> Danh mục: </i>" + "<b>" + partner.PartnerGroup + "</b>" + "</br>" +
                "\t  Taxcode / <i> Mã số thuế: </i>" + "<b>" + partner.TaxCode + "</b>" + "</br>" +
                "\t  Address  / <i> Địa chỉ: </i> " + "<b>" + partner.AddressEn + "</b>" + "</br>" +
                "\t  Requestor / <i> Người yêu cầu: </i> " + "<b>" + fullNameCreatetor + "</b>" + "</br> </br>" + linkEn + "</br>" + linkVn + "</br> </br>" +
                "<i> Thanks and Regards </i>" + "</br> </br>" +
                "<b> eFMS System, </b>" + "</br>" +
                "<p><img src = '[logoEFMS]' /></p> " + " </div>");
            ApiUrl.Value.Url = ApiUrl.Value.Url.Replace("Catalogue", "");
            body = body.Replace("[logoEFMS]", ApiUrl.Value.Url.ToString() + "/ReportPreview/Images/logo-eFMS.png");

            bool resultSenmail = false;
            if ( (partner.PartnerType != "Customer" && partner.PartnerType != "Agent") || string.IsNullOrEmpty(partner.PartnerType))
            {
                if (lstToAccountant.Any() || lstCc.Any())
                {
                    if (lstToAccountant.Count() == 0)
                    {
                        lstToAccountant = lstCc;
                    }
                    resultSenmail  = SendMail.Send(subject, body, lstToAccountant, null, null, lstCc);
                }
            }

            if (partner.PartnerType == "Customer" || partner.PartnerType == "Agent")
            {
                lstToAccountant.AddRange(lstToAR);
                if (lstToAccountant.Any())
                {
                    resultSenmail = SendMail.Send(subject, body, lstToAccountant, null, null, lstCc);
                }
            }

            var logSendMail = new SysSentEmailHistory
            {
                Receivers = string.Join("; ", lstToAccountant),
                Ccs = string.Join("; ", lstCc),
                Subject = subject,
                Sent = resultSenmail,
                SentDateTime = DateTime.Now,
                Body = body
            };

            var hsLogSendMail = sendEmailHistoryRepository.Add(logSendMail);
            var hsSm = sendEmailHistoryRepository.SubmitChanges();
        }

        private List<string> ListMailCC()
        {
            List<string> lstCc = new List<string>
            {
                "alex.phuong@itlvn.com",
                "luis.quang@itlvn.com",
                "andy.hoa@itlvn.com",
                "cara.oanh@itlvn.com",
                "lynne.loc@itlvn.com",
                "samuel.an@logtechub.com",
                "kenny.thuong@itlvn.com"
            };
            return lstCc;
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
            //foreach (var file in model.Files)
            //{
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
            //}
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

            if (model.Contracts.Count > 0)
            {
                entity.SalePersonId = model.Contracts.FirstOrDefault().SaleManId.ToString();
            }
            var hs = DataContext.Update(entity, x => x.Id == model.Id);
            if (hs.Success)
            {
                //var hsoldman = contractRepository.Delete(x => x.PartnerId == model.Id && !model.contracts.Any(sale => sale.Id == x.Id));
                //var salemans = mapper.Map<List<CatContract>>(model.contracts);

                //foreach (var item in model.contracts)
                //{
                //    if (item.Id == Guid.Empty)
                //    {
                //        item.Id = Guid.NewGuid();
                //        item.PartnerId = entity.Id;
                //        item.DatetimeCreated = DateTime.Now;
                //        item.UserCreated = currentUser.UserID;
                //        contractRepository.Add(item);
                //    }
                //    else
                //    {
                //        item.DatetimeCreated = DateTime.Now;
                //        item.UserModified = currentUser.UserID;
                //        contractRepository.Update(item, x => x.Id == item.Id);
                //    }
                //}
                //contractRepository.SubmitChanges();
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
            //ChangeTrackerHelper.currentUser = currentUser.UserID;
            if (!string.IsNullOrEmpty(id))
            {
                if (transactionDetailRepository.Any(x => x.CustomerId == id))
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

        public IQueryable<CatPartnerViewModel> Paging(CatPartnerCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = QueryPaging(criteria);
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
            }
            if (data == null)
            {
                rowsCount = 0;
                return null;
            }

            rowsCount = data.Select(x => x.Id).Count();
            if (rowsCount == 0)
            {
                return null;
            }
            IQueryable<CatPartnerViewModel> results = null;
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

        /// <summary>
        /// Check detail partner to view
        /// </summary>
        /// <param name="id">partner id</param>
        /// <returns></returns>
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
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);
            int code = GetPermissionToUpdate(new ModelUpdate { GroupId = detail.GroupId, OfficeId = detail.OfficeId, CompanyId = detail.CompanyId, DepartmentId = detail.DepartmentId, UserCreator = detail.UserCreated, Salemans = salemans, PartnerGroup = detail.PartnerGroup }, permissionRange, 1);
            if (code == 403) return new HandleState(403, "");
            return new HandleState();
        }

        /// <summary>
        /// Check detail partner to delete
        /// </summary>
        /// <param name="id">partner id</param>
        /// <returns></returns>
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
            ClearCache();
            var partners = Get(x => (x.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
            if (partners == null) return null;

            var query = (from partner in partners
                         join user in sysUsers on partner.UserCreated equals user.Id
                         join saleman in sysUsers on partner.SalePersonId equals saleman.Id into prods
                         from x in prods.DefaultIfEmpty()
                         select new { user, partner, x }
                          );
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
                           ));
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
                           || (x.partner.AccountNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           //|| (x.partner.CoLoaderCode ?? "").Contains(criteria.All ?? "", StringComparison.OrdinalIgnoreCase)
                           )
                           && (x.partner.Active == criteria.Active || criteria.Active == null)
                           && (x.partner.PartnerType == criteria.PartnerType || criteria.PartnerType == null));

            }
            if (query == null) return null;
            var results = query.Select(x => new CatPartnerViewModel
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
                PartnerType = x.partner.PartnerType

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

            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
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
                    partner.UserCreated = currentUser.UserID;
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
                    partner.UserCreated = currentUser.UserID;
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
                    if(taxCode.Any(x => Char.IsWhiteSpace(x)))
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
                        if (group == null)
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

                if(!string.IsNullOrEmpty(item.PartnerMode) && item.PartnerMode == "Internal")
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
                    else if (list.Count(x => x.TaxCode == taxCode) > 1 && list.Count(x => x.InternalReferenceNo == internalReferenceNo) > 1)
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

        public IQueryable<CatPartnerModel> GetBy(CatPartnerGroupEnum partnerGroup)
        {
            ClearCache();
            string group = PlaceTypeEx.GetPartnerGroup(partnerGroup);
            IQueryable<CatPartnerModel> data = Get().Where(x => (x.PartnerGroup ?? "").IndexOf(group ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
            return data;
        }

        public IQueryable<CatPartnerViewModel> GetMultiplePartnerGroup(PartnerMultiCriteria criteria)
        {
            IQueryable<CatPartnerModel> data = Get();
            List<string> grpCodes = new List<string>();
            if (criteria.PartnerGroups != null)
            {
                foreach (var grp in criteria.PartnerGroups)
                {
                    string group = PlaceTypeEx.GetPartnerGroup(grp);
                    grpCodes.Add(group);
                }
                Expression<Func<CatPartnerModel, bool>> query = null;
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
                data = data.Where(query);
            }
            else
            {
                data = data.Where(x => x.Active == criteria.Active || criteria.Active == null);
            }
            if (data == null) return null;
            var results = data.Select(x => new CatPartnerViewModel
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
                AccountNo = x.AccountNo
            });
            return results;
        }

        public IQueryable<CatPartnerViewModel> Query(CatPartnerCriteria criteria)
        {
            string partnerGroup = criteria != null ? PlaceTypeEx.GetPartnerGroup(criteria.PartnerGroup) : null;
            var data = Get().Where(x => (x.PartnerGroup ?? "").Contains(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase)
                                && (x.Active == criteria.Active || criteria.Active == null)
                                && (x.CoLoaderCode ?? "").Contains(criteria.CoLoaderCode ?? "", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(criteria.Id))
            {
                data = data.Where(x => x.ParentId != criteria.Id);
            }
            if (data == null) return null;
            var results = data.Select(x => new CatPartnerViewModel
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
                AccountNo = x.AccountNo
            });
            return results;
        }

        /// <summary>
        /// Get partner list by parentId
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public List<CatPartnerViewModel> GetSubListPartnerByID(string id, string partnerType)
        {
            if (id == null) return null;
            var currPartnerId = Get(x => x.Id == id).FirstOrDefault()?.AccountNo;
            var data = Get(x => x.ParentId == id && x.AccountNo != currPartnerId && x.PartnerType == partnerType);
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
    }
}
