using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Models;
using eFMS.API.Catalogue.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatIncotermService : RepositoryBase<CatIncoterm, CatIncotermModel>, ICatIncotermService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        public CatIncotermService(IContextBase<CatIncoterm> repository, 
            IMapper mapper,
            IStringLocalizer sLocalizer,
            ICurrentUser curUser
            ) : base(repository, mapper)
        {
            sLocalizer = stringLocalizer;
            currentUser = curUser;
        }

        public HandleState Add(CatIncotermEditModel model)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                model.Id = Guid.NewGuid(); ;
                model.UserCreated = model.UserModified = currentUser.UserID;
                model.DatetimeCreated = DateTime.Now;

                // Update permission
                model.GroupId = currentUser.GroupId;
                model.DepartmentId = currentUser.DepartmentId;
                model.OfficeId = currentUser.OfficeID;
                model.CompanyId = currentUser.CompanyID;

                try
                {

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

        public bool CheckAllowDelete(Guid id)
        {
            throw new NotImplementedException();
        }

        public bool CheckAllowViewDetail(Guid id)
        {
            throw new NotImplementedException();
        }

        public HandleState Delete(Guid Id)
        {
            throw new NotImplementedException();
        }

        public CatChartOfAccountsModel GetDetail(Guid id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<CatIncoterm> Paging(CatIncotermCriteria criteria, int page, int size, out int rowsCount)
        {
            throw new NotImplementedException();
        }

        public IQueryable<CatIncoterm> Query(CatIncotermCriteria criteria)
        {
            throw new NotImplementedException();
        }

        public HandleState Update(CatIncoterm model)
        {
            throw new NotImplementedException();
        }
    }
}
