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

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatPartnerService : RepositoryBaseCache<CatPartner, CatPartnerModel>, ICatPartnerService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUser> sysUserRepository;

        private readonly IContextBase<CatSaleman> salemanRepository;
        private readonly ICatPlaceService placeService;
        private readonly ICatCountryService countryService;
        private readonly IOptions<WebUrl> webUrl;
        private readonly IContextBase<SysOffice> officeRepository;
        private readonly IContextBase<SysEmployee> sysEmployeeRepository;


        public CatPartnerService(IContextBase<CatPartner> repository,
            ICacheServiceBase<CatPartner> cacheService,
            IMapper mapper,
            IStringLocalizer<LanguageSub> localizer,
            ICurrentUser user,
            IContextBase<SysUser> sysUserRepo,
            ICatPlaceService place,
            ICatCountryService country,
            IContextBase<CatSaleman> salemanRepo, IOptions<WebUrl> url,
            IContextBase<SysOffice> officeRepo,
            IContextBase<SysEmployee> sysEmployeeRepo) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            placeService = place;
            salemanRepository = salemanRepo;
            sysUserRepository = sysUserRepo;
            countryService = country;
            webUrl = url;
            officeRepository = officeRepo;
            sysEmployeeRepository = sysEmployeeRepo;
            SetChildren<CsTransaction>("Id", "ColoaderId");
            SetChildren<CsTransaction>("Id", "AgentId");
            SetChildren<SysUser>("Id", "PersonIncharge");
            SetChildren<OpsTransaction>("Id", "CustomerId");
            SetChildren<OpsTransaction>("Id", "SupplierId");
            SetChildren<OpsTransaction>("Id", "AgentId");
            SetChildren<CatPartnerCharge>("Id", "PartnerId");
            SetChildren<CsManifest>("Id", "Supplier");
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
        public override HandleState Add(CatPartnerModel entity)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
            var permissionRangeWrite = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRangeWrite == PermissionRange.None) return new HandleState(403, "");
            var partner = mapper.Map<CatPartner>(entity);
            partner.DatetimeCreated = DateTime.Now;
            partner.DatetimeModified = DateTime.Now;
            partner.UserCreated = partner.UserModified = currentUser.UserID;
            partner.Active = false;
            partner.GroupId = currentUser.GroupId;
            partner.DepartmentId = currentUser.DepartmentId;
            partner.OfficeId = currentUser.OfficeID;
            partner.CompanyId = currentUser.CompanyID;
            var hs = DataContext.Add(partner);
            if (hs.Success)
            {
                if (entity.SaleMans.Count() > 0)
                {
                    var salemans = mapper.Map<List<CatSaleman>>(entity.SaleMans);
                    salemans.ForEach(x =>
                    {
                        x.Id = Guid.NewGuid();
                        x.PartnerId = partner.Id;
                        x.CreateDate = DateTime.Now;
                        x.UserCreated = currentUser.UserID;
                    });
                    partner.SalePersonId = salemans.FirstOrDefault().SaleManId.ToString();
                    DataContext.Update(partner, x => x.Id == partner.Id);
                    salemanRepository.Add(salemans);
                }
                DataContext.SubmitChanges();
                salemanRepository.SubmitChanges();
                ClearCache();
                Get();
                string employeeId = sysUserRepository.Get(x => x.Id == currentUser.UserID).Select(t => t.EmployeeId).FirstOrDefault();
                string fullNameCreatetor = "[" + sysEmployeeRepository.Get(e => e.Id == employeeId).Select(t => t.EmployeeNameVn)?.FirstOrDefault() + "]";
                string address = webUrl.Value.Url.ToString() + "/en/#/home/catalogue/partner-data/detail/" + entity.Id;
                string linkEn = "You can <a href='" + address + "'> click here </a>" + "to view detail.";
                string linkVn = "Bạn click <a href='" + address + "'> vào đây </a>" + "để xem chi tiết.";
                string subject = "eFMS - Partner Approval Request From " + fullNameCreatetor;
                string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'> Dear Accountant Team: </br>" +
                    "You have a Partner Approval request From" + fullNameCreatetor + " as info bellow: </br>" +
                    "Bạn có môt yêu cầu xác duyệt đối tượng từ" + fullNameCreatetor + " với thông tin như sau:</br>" +
                    "\t <b> Partner ID </b> / <i> Mã đối tượng:</i> " + partner.AccountNo + "</br>" +
                    "\t <b> Catagory </b> / <i> Danh mục: </i>" + partner.PartnerGroup + "</br>" +
                    "\t <b> Taxcode </b>/ <i> Mã số thuế: </i>" + partner.TaxCode + "</br>" +
                    "\t <b> Address </b> / <i> Địa chỉ: </i> " + partner.AddressEn + "</br>" +
                    "\t <b> Requestor </b> / <i> Người yêu cầu: </i> " + fullNameCreatetor + "</br>" + linkEn +"</br>" + linkVn + "</br>" +
                    "<i> Thanks and Regards </i>" + "</br>" +
                    "eFMS System </div>") ;
                SendMail.Send(subject, body, new List<string> { "samuel.an@logtechub.com", "alex.phuong@itlvn.com" }, null, null);
            }
            return hs;
        }
        public HandleState Update(CatPartnerModel model)
        {
            var listSalemans = salemanRepository.Get(x => x.PartnerId == model.Id).ToList();
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);

            int code = GetPermissionToUpdate(new ModelUpdate { UserCreator = model.UserCreated, Salemans = listSalemans, PartnerGroup = model.PartnerGroup }, permissionRange, null);
            if (code == 403) return new HandleState(403, "");
            var entity = GetModelToUpdate(model);
            if (model.SaleMans.Count > 0)
            {
                entity.SalePersonId = model.SaleMans.FirstOrDefault().SaleManId.ToString();
            }
            var hs = DataContext.Update(entity, x => x.Id == model.Id);
            if (hs.Success)
            {
                var hsoldman = salemanRepository.Delete(x => x.PartnerId == model.Id && !model.SaleMans.Any(sale => sale.Id == x.Id));
                var salemans = mapper.Map<List<CatSaleman>>(model.SaleMans);

                foreach (var item in model.SaleMans)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.PartnerId = entity.Id;
                        item.CreateDate = DateTime.Now;
                        item.UserCreated = currentUser.UserID;
                        salemanRepository.Add(item);
                    }
                    else
                    {
                        item.ModifiedDate = DateTime.Now;
                        item.UserModified = currentUser.UserID;
                        salemanRepository.Update(item, x => x.Id == item.Id);
                    }
                }
                salemanRepository.SubmitChanges();
                ClearCache();
                Get();
            }
            return hs;
        }

        private CatPartner GetModelToUpdate(CatPartnerModel model)
        {
            var entity = mapper.Map<CatPartner>(model);
            var partner = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();

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
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                var s = salemanRepository.Delete(x => x.PartnerId == id);
                salemanRepository.SubmitChanges();
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
            var salemans = salemanRepository.Get().ToList();
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
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
            var salemans = salemanRepository.Get().ToList();
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
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
        public int CheckDetailPermission(string id)
        {
            var detail = Get(x => x.Id == id).FirstOrDefault();
            var salemans = salemanRepository.Get(x => x.PartnerId == id).ToList();
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);
            int code = GetPermissionToUpdate(new ModelUpdate { GroupId = detail.GroupId, OfficeId = detail.OfficeId, CompanyId = detail.CompanyId, DepartmentId = detail.DepartmentId, UserCreator = detail.UserCreated, Salemans = salemans, PartnerGroup = detail.PartnerGroup }, permissionRange, 1);
            return code;
        }

        public int CheckDeletePermission(string id)
        {
            var detail = Get(x => x.Id == id).FirstOrDefault();
            var salemans = salemanRepository.Get(x => x.PartnerId == id).ToList();
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            int code = GetPermissionToDelete(new ModelUpdate { GroupId = detail.GroupId, OfficeId = detail.OfficeId, CompanyId = detail.CompanyId, DepartmentId = detail.DepartmentId, UserCreator = detail.UserCreated, Salemans = salemans, PartnerGroup = detail.PartnerGroup }, permissionRange);
            return code;
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
            var partners = Get(x => (x.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0);

            if (partners == null) return null;

            var query = (from partner in partners
                         join user in sysUsers on partner.UserCreated equals user.Id
                         join saleman in sysUsers on partner.SalePersonId equals saleman.Id into prods
                         from x in prods.DefaultIfEmpty()
                         select new { user, partner, x }
                          );
            if (criteria.All == null)
            {
                query = query.Where(x => ((x.partner.AccountNo ?? "").IndexOf(criteria.Id ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.ShortName ?? "").IndexOf(criteria.ShortName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.PartnerNameEn ?? "").IndexOf(criteria.PartnerNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.PartnerNameVn ?? "").IndexOf(criteria.PartnerNameVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.AddressVn ?? "").IndexOf(criteria.AddressVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.TaxCode ?? "").IndexOf(criteria.TaxCode ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.Tel ?? "").IndexOf(criteria.Tel ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.Fax ?? "").IndexOf(criteria.Fax ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.user.Username ?? "").IndexOf(criteria.UserCreated ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.AccountNo ?? "").IndexOf(criteria.AccountNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.CoLoaderCode ?? "").Contains(criteria.CoLoaderCode ?? "", StringComparison.OrdinalIgnoreCase)
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
                           || (x.partner.CoLoaderCode ?? "").Contains(criteria.All ?? "", StringComparison.OrdinalIgnoreCase)
                           )
                           && (x.partner.Active == criteria.Active || criteria.Active == null));
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
                Active = x.partner.Active
            });
            return results;
        }

        public CatPartnerModel GetDetail(string id)
        {
            var queryDetail = Get(x => x.Id == id).FirstOrDefault();
            var salemans = salemanRepository.Get(x => x.PartnerId == id).ToList();
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
            var permissionRangeWrite = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            var permissionRangeDelete = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            int checkDelete = GetPermissionToDelete(new ModelUpdate { GroupId = queryDetail.GroupId, OfficeId = queryDetail.OfficeId, CompanyId = queryDetail.CompanyId, UserCreator = queryDetail.UserCreated, Salemans = salemans, PartnerGroup = queryDetail.PartnerGroup }, permissionRangeDelete);

            queryDetail.Permission = new PermissionAllowBase
            {
                AllowUpdate = GetPermissionDetail(permissionRangeWrite, salemans, queryDetail),
                AllowDelete = checkDelete == 403 ? false : true
            };
            return queryDetail;
        }

        private bool GetPermissionDetail(PermissionRange permissionRangeWrite, List<CatSaleman> salemans, CatPartnerModel detail)
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
                var salesmans = new List<CatSaleman>();
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
                    partner.Active = active;
                    partner.InactiveOn = inactiveDate;
                    partner.CompanyId = currentUser.CompanyID;
                    partner.OfficeId = currentUser.OfficeID;
                    partner.GroupId = currentUser.GroupId;
                    partner.DepartmentId = currentUser.DepartmentId;
                    var salesman = new CatSaleman
                    {
                        Id = Guid.NewGuid(),
                        Office = item.OfficeId,
                        Company = item.CompanyId,
                        SaleManId = item.SalePersonId,
                        FreightPayment = item.PaymentTerm,
                        EffectDate = item.EffectDate != null ? Convert.ToDateTime(item.EffectDate) : (DateTime?)null,
                        Status = true,
                        PartnerId = partner.Id,
                        CreateDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        UserCreated = currentUser.UserID,
                        UserModified = currentUser.UserID,
                        Service = item.ServiceId
                    };
                    partners.Add(partner);
                    salesmans.Add(salesman);
                }
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        var hs = DataContext.Add(partners);
                        if (hs.Success)
                        {
                            hs = salemanRepository.Add(salesmans);
                            if (hs.Success)
                            {
                                trans.Commit();
                            }
                            else
                            {
                                trans.Rollback();
                            }
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
                    string taxCode = item.TaxCode.Replace(" ", "");
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
                        if (partners.Any(x => x.TaxCode == taxCode))
                        {
                            item.TaxCodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_TAXCODE_EXISTED], item.TaxCode);
                            item.IsValid = false;
                        }
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
                    item = GetSaleManInfo(item, salemans, offices, services);
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
    }
}
