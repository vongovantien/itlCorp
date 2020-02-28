using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatSalemanService : RepositoryBase<CatSaleman, CatSaleManModel>, ICatSaleManService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUser> sysUserRepository;
        private readonly IContextBase<CatPartner> catPartnerRepository;

        public CatSalemanService(IContextBase<CatSaleman> repository, IMapper mapper, IStringLocalizer<CatalogueLanguageSub> localizer, ICurrentUser user, IContextBase<SysUser> sysUserRepo, IContextBase<CatPartner> partnerRepo) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            sysUserRepository = sysUserRepo;
            catPartnerRepository = partnerRepo;
        }

        public IQueryable<CatSaleman> GetSaleMan()
        {
            return DataContext.Get();
        }

        public List<CatSaleManModel> GetBy(string partnerId)
        {
            //List<CatSaleManModel> results = null;
            //var data = DataContext.Get(x => x.JobNo == jobNo);
            var data = GetSaleMan().Where(x => x.PartnerId.Trim() == partnerId);
            var sysUser = sysUserRepository.Get();
            var query = from sale in data
                        join user in sysUser on sale.SaleManId equals user.Id
                        select new { sale, user };


            List<CatSaleManModel> results = new List<CatSaleManModel>();
            if (data.Count() == 0) return null;

            foreach (var item in query)
            {

                var saleman = mapper.Map<CatSaleManModel>(item.sale);
                saleman.Username = item.user.Username;
                results.Add(saleman);
            }
            return results;
        }

        #region CRUD
        public override HandleState Add(CatSaleManModel entity)
        {
            entity.Id = Guid.NewGuid();
            var saleMan = mapper.Map<CatSaleman>(entity);
            saleMan.CreateDate = DateTime.Now;
            saleMan.UserCreated = currentUser.UserID;

            var hs = DataContext.Add(saleMan);
            return hs;
        }

        public HandleState Update(CatSaleManModel model)
        {
            var entity = mapper.Map<CatSaleman>(model);
            entity.UserModified = currentUser.UserID;
            var hs = DataContext.Update(entity, x => x.Id == model.Id);
            return hs;
        }
        public HandleState Delete(Guid id)
        {
            var hs = DataContext.Delete(x => x.Id == id);
            return hs;
        }
        #endregion

        public List<CatSaleManViewModel> Query(CatSalemanCriteria criteria)
        {
            var salesMan = GetSaleMan().Where(x => x.PartnerId == criteria.PartnerId);
            var salesManData = GetSaleMan().Where(x => x.PartnerId == criteria.PartnerId);

            var sysUser = sysUserRepository.Get();
            //var partner = catPartnerRepository.Get(x => x.Id == criteria.PartnerId).FirstOrDefault();
            //ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
            //PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            //switch (rangeSearch)
            //{
            //    case PermissionRange.None:
            //        salesMan = null;
            //        break;
            //    case PermissionRange.All:
            //        break;
            //    case PermissionRange.Owner:
            //        salesMan = salesMan.Where(x => x.SaleManId == currentUser.UserID || x.UserCreated == currentUser.UserID);
            //        break;
            //    case PermissionRange.Group:
            //        salesMan = salesMan.Where(x => (x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
            //        || x.UserCreated == currentUser.UserID || x.SaleManId ==  currentUser.UserID);
            //        break;
            //    case PermissionRange.Department:
            //        salesMan = salesMan.Where(x => (x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.DepartmentId == currentUser.DepartmentId && x.CompanyId == currentUser.CompanyID)
            //        || x.UserCreated == currentUser.UserID);
            //        break;
            //    case PermissionRange.Office:
            //        salesMan = salesMan.Where(x => (x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
            //           || x.UserCreated == currentUser.UserID || x.SaleManId == currentUser.UserID
            //           );
            //        break;
            //    case PermissionRange.Company:
            //        salesMan = salesMan.Where(x => (x.CompanyId == currentUser.CompanyID)
            //         || x.UserCreated == currentUser.UserID || x.SaleManId == currentUser.UserID
            //         );
            //        break;

            //}
            //if (salesMan == null)
            //{
            //    return null;
            //}
            var query = from saleman in salesMan
                        join users in sysUser on saleman.SaleManId equals users.Id
                        select new { saleman, users };
            if (criteria.All == null)
            {
                query = query.Where(x =>
                           //(x.saleman.Company ?? "").IndexOf(criteria.Company ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           (x.saleman.Company == criteria.Company || criteria.Company == Guid.Empty)
                           //&& (x.saleman.Office ?? "").IndexOf(criteria.Office ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.saleman.Office == criteria.Office || criteria.Office == Guid.Empty)
                           && (x.saleman.Status == criteria.Status || criteria.Status == null)
                           );
            }
            else
            {
                query = query.Where(x =>
                            //(x.saleman.Company ?? "").IndexOf(criteria.Company ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                            (x.saleman.Company == criteria.Company || criteria.Company == Guid.Empty)
                            //|| (x.saleman.Office ?? "").IndexOf(criteria.Office ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                            || (x.saleman.Office == criteria.Office || criteria.Office == Guid.Empty)
                            || (x.saleman.Status == criteria.Status || criteria.Status == null)
                            || (x.saleman.PartnerId == criteria.PartnerId)
                            );
            }
            if (query.Count() == 0) return null;
            List<CatSaleManViewModel> results = new List<CatSaleManViewModel>();
            foreach (var item in query)
            {

                var saleman = mapper.Map<CatSaleManViewModel>(item.saleman);
                saleman.Username = item.users.Username;
                results.Add(saleman);
            }
            return results;
        }

        public List<CatSaleManViewModel> Paging(CatSalemanCriteria criteria, int page, int size, out int rowsCount)
        {
            List<CatSaleManViewModel> results = null;
            var list = Query(criteria);
            if (list == null)
            {
                rowsCount = 0;
                return results;
            }
            rowsCount = list.ToList().Count;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = list.OrderByDescending(x => x.ModifiedDate).Skip((page - 1) * size).Take(size).ToList();
            }
            return results;
        }

    }
}
