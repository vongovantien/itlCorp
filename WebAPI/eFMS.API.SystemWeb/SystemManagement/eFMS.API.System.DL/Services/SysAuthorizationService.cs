using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace eFMS.API.System.DL.Services
{
    public class SysAuthorizationService : RepositoryBase<SysAuthorization, SysAuthorizationModel>, ISysAuthorizationService
    {
        private readonly ICurrentUser currentUser;
        public SysAuthorizationService(IContextBase<SysAuthorization> repository, IMapper mapper, ICurrentUser user) : base(repository, mapper)
        {
            currentUser = user;
        }

        public IQueryable<SysAuthorizationModel> QueryData(SysAuthorizationCriteria criteria)
        {
            Expression<Func<SysAuthorization, bool>> query = null;

            if (!string.IsNullOrEmpty(criteria.All))
            {                
                query = x =>
                           x.Services.IndexOf(criteria.Service ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        || x.UserId.IndexOf(criteria.UserID ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        || x.AssignTo.IndexOf(criteria.AssignTo ?? "", StringComparison.OrdinalIgnoreCase) >= 0;
                if (criteria.StartDate != null)
                {
                    query = query.Or(x => x.StartDate.Date == criteria.StartDate.Value.Date);
                }
                if(criteria.EndDate != null)
                {
                    query = query.Or(x => x.EndDate.Value.Date == criteria.EndDate.Value.Date);
                }
                if (criteria.Active != null)
                {
                    query = query.Or(x => x.Active == criteria.Active);
                }
            }
            else
            {
                query = x =>
                       x.Services.IndexOf(criteria.Service ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && x.UserId.IndexOf(criteria.UserID ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && x.AssignTo.IndexOf(criteria.AssignTo ?? "", StringComparison.OrdinalIgnoreCase) >= 0;
                if (criteria.StartDate != null)
                {
                    query = query.And(x => x.StartDate.Date == criteria.StartDate.Value.Date);
                }
                if (criteria.EndDate != null)
                {
                    query = query.And(x => x.EndDate.Value.Date == criteria.EndDate.Value.Date);
                }
                if(criteria.Active != null)
                {
                    query = query.And(x => x.Active == criteria.Active);
                }
            }

            var queryData = from d in DataContext.Get().Where(query)
                            select new SysAuthorizationModel
                            {
                                Id = d.Id,
                                Name = d.Name,
                                ServicesName = GetServiceNameOfAuthorization(d.Services),
                                Description = d.Description,
                                UserId = d.UserId,
                                AssignTo = d.AssignTo,
                                StartDate = d.StartDate,
                                EndDate = d.EndDate,
                                UserCreated = d.UserCreated,
                                DatetimeCreated = d.DatetimeCreated,
                                UserModified = d.UserModified,
                                DatetimeModified = d.DatetimeModified,
                                Active = d.Active,
                                InactiveOn = d.InactiveOn
                            };

            var result = queryData.ToArray().OrderByDescending(x => x.DatetimeModified).AsQueryable();
            return result;
        }

        private string GetServiceNameOfAuthorization(string services)
        {
            var serviceName = string.Empty;
            if (!string.IsNullOrEmpty(services))
            {
                //Tách chuỗi services thành mảng
                string[] arrayStrServices = services.Split(';').Where(x => x.ToString() != string.Empty).ToArray();
                //Xóa các services trùng
                string[] arrayGrpServices = arrayStrServices.Distinct<string>().ToArray();
                serviceName = string.Join("; ", arrayGrpServices.Select(s => CustomData.Services.Where(x => x.Value == s).FirstOrDefault()?.DisplayName));
            }
            return serviceName;
        }

        public IQueryable<SysAuthorizationModel> Paging(SysAuthorizationCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = QueryData(criteria);
            rowsCount = (data.Count() > 0) ? data.Count() : 0;
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

        public SysAuthorizationModel GetAuthorizationById(int id)
        {
            var data = new SysAuthorizationModel();
            var authoriza = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if (authoriza != null)
            {
                data = mapper.Map<SysAuthorizationModel>(authoriza);
                data.ServicesName = GetServiceNameOfAuthorization(data.Services);
            }
            return data;
        }

        public HandleState Insert(SysAuthorizationModel model)
        {
            try
            {
                var userCurrent = currentUser.UserID;
                var today = DateTime.Now;
                var modelAdd = mapper.Map<SysAuthorization>(model);                
                modelAdd.UserCreated = modelAdd.UserModified = userCurrent;
                modelAdd.DatetimeCreated = modelAdd.DatetimeModified = today;
                if (modelAdd.Active == false)
                {
                    modelAdd.InactiveOn = today;
                }
                var hs = DataContext.Add(modelAdd);                
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }       
    }
}
