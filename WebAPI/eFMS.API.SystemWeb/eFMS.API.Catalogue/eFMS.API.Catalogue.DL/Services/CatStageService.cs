using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System.Linq;
using System;
using System.Collections.Generic;
using eFMS.API.Catalogue.DL.Models.Criteria;
using Microsoft.Extensions.Localization;
using eFMS.API.Catalogue.DL.Common;
using ITL.NetCore.Connection.Caching;
using AutoMapper.QueryableExtensions;
using eFMS.IdentityServer.DL.UserManager;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatStageService : RepositoryBaseCache<CatStage, CatStageModel>, ICatStageService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IContextBase<CatDepartment> departmentRepository;
        private readonly ICurrentUser currentUser;
        public CatStageService(IContextBase<CatStage> repository, 
            ICacheServiceBase<CatStage> cacheService, 
            IMapper mapper,
            IStringLocalizer<CatalogueLanguageSub> localizer,
            IContextBase<CatDepartment> departmentRepo,
            ICurrentUser currUser) : base(repository, cacheService, mapper)
        {
            currentUser = currUser;
            stringLocalizer = localizer;
            departmentRepository = departmentRepo;
            SetChildren<OpsStageAssigned>("Id", "StageId");
        }

        public IQueryable<CatStageModel> GetAll()
        {
            var data = DataContext.Get();
            if (data == null) return null;
            var results = data.ProjectTo<CatStageModel>(mapper.ConfigurationProvider);
            return results;
        } 

        public HandleState AddStage(CatStageModel catStage)
        {
            var hs = DataContext.Add(catStage);
            if (hs.Success)
            {
                ClearCache();
                Get();
            }
            return hs;
        }
      
        public HandleState DeleteStage(int id)
        {
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                ClearCache();
                Get();
            }
            return hs;
        }

        public IQueryable<CatStageModel> Paging(CatStageCriteria criteria, int page, int size, out int rowsCount)
        {
            IQueryable<CatStageModel> returnList = Query(criteria);

            if (returnList == null) {
                rowsCount = 0;
                return returnList;
            }
            rowsCount = returnList.Count();
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                returnList = returnList.OrderByDescending(x => x.DatetimeModified).Skip((page - 1)*size).Take(size);
            }

            return returnList;
        }
       

        public IQueryable<CatStageModel> Query(CatStageCriteria criteria)
        {
            IQueryable<CatStageModel> results = null;
            var stages = DataContext.Get();
            var departments = departmentRepository.Get();
            if (criteria.All == null)
            {
                results = stages.Join(departments, x => x.DepartmentId, y => y.Id, (x, y) => new { Stage = x, Department = y })
                    .Where(x => x.Stage.Code.IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                             && x.Stage.StageNameEn.IndexOf(criteria.StageNameEn ??"", StringComparison.OrdinalIgnoreCase) > -1
                             && x.Stage.StageNameVn.IndexOf(criteria.StageNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                             && (x.Stage.Id == criteria.Id || criteria.Id == 0)
                             && (x.Stage.Active == criteria.Active || criteria.Active == null)
                             && x.Department.DeptName.IndexOf(criteria.DepartmentName ?? "", StringComparison.OrdinalIgnoreCase) > -1
                    ).Select(x => new CatStageModel
                    {   Id = x.Stage.Id,
                        Code = x.Stage.Code,
                        StageNameVn = x.Stage.StageNameVn,
                        StageNameEn = x.Stage.StageNameEn,
                        DepartmentId = x.Stage.DepartmentId,
                        DescriptionVn = x.Stage.DescriptionVn,
                        DescriptionEn = x.Stage.DescriptionEn,
                        UserCreated = x.Stage.UserCreated,
                        DatetimeCreated = x.Stage.DatetimeCreated,
                        UserModified = x.Stage.UserModified,
                        DatetimeModified = x.Stage.DatetimeModified,
                        Active = x.Stage.Active,
                        InactiveOn = x.Stage.InactiveOn,
                        DeptCode = x.Department.Code,
                        DeptName = x.Department.DeptName
                    });
            }
            else
            {
                results = stages.Join(departments, x => x.DepartmentId, y => y.Id, (x, y) => new { Stage = x, Department = y })
                    .Where(x => (x.Stage.Code.IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                             || x.Stage.StageNameEn.IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                             || x.Stage.StageNameVn.IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                             || x.Department.DeptName.IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                    ).Select(x => new CatStageModel
                    {
                        Id = x.Stage.Id,
                        Code = x.Stage.Code,
                        StageNameVn = x.Stage.StageNameVn,
                        StageNameEn = x.Stage.StageNameEn,
                        DepartmentId = x.Stage.DepartmentId,
                        DescriptionVn = x.Stage.DescriptionVn,
                        DescriptionEn = x.Stage.DescriptionEn,
                        UserCreated = x.Stage.UserCreated,
                        DatetimeCreated = x.Stage.DatetimeCreated,
                        UserModified = x.Stage.UserModified,
                        DatetimeModified = x.Stage.DatetimeModified,
                        Active = x.Stage.Active,
                        InactiveOn = x.Stage.InactiveOn,
                        DeptCode = x.Department.Code,
                        DeptName = x.Department.DeptName
                    });
            }
            return results;
        }

        public HandleState UpdateStage(CatStageModel catStage)
        {
            var hs = DataContext.Update(catStage, x => x.Id == catStage.Id);
            if (hs.Success)
            {
                ClearCache();
                Get();
            }
            return hs;
        }

        public List<CatStageImportModel> CheckValidImport(List<CatStageImportModel> list)
        {
            var stages = Get();
            list.ForEach(item =>
            {
               
                if (string.IsNullOrEmpty(item.StageNameEn))
                {
                    item.StageNameEn = stringLocalizer[CatalogueLanguageSub.MSG_STAGE_NAME_EN_EMPTY];
                    item.IsValid = false;
                }
                if (item.DepartmentId==null)
                {
                    item.DepartmentId = -1;
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.Status))
                {
                    item.Status = stringLocalizer[CatalogueLanguageSub.MSG_STAGE_STATUS_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.Code))
                {
                    item.Code = stringLocalizer[CatalogueLanguageSub.MSG_STAGE_CODE_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    var stage = stages.FirstOrDefault(x => x.Code.ToLower() == item.Code.ToLower());
                    if (stage != null)
                    {
                        item.Code = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_STAGE_EXISTED],item.Code);
                        item.IsValid = false;
                    }
                    if (list.Count(x => x.Code.ToLower() == item.Code.ToLower()) > 1)
                    {
                        item.Code = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_STAGE_CODE_DUPLICATE], item.Code);
                        item.IsValid = false;
                    }
                }

            });
            return list;
        }

        public HandleState Import(List<CatStageImportModel> data)
        {
            try
            {
                var list = new List<CatStage>();
                foreach(var item in data)
                {
                    bool active = !string.IsNullOrEmpty(item.Status) && (item.Status.ToLower() == "active");
                    DateTime? inactiveDate = active == false ? (DateTime?)DateTime.Now : null;
                    var stage = new CatStage
                    {
                        Code = item.Code,
                        StageNameEn = item.StageNameEn,
                        StageNameVn = item.StageNameVn,
                        DepartmentId = item.DepartmentId,
                        DescriptionEn = item.DescriptionEn,
                        DescriptionVn = item.DescriptionVn,
                        DatetimeCreated = DateTime.Now,
                        UserCreated = currentUser.UserID,
                        Active = active,
                        InactiveOn = inactiveDate
                    };
                    list.Add(stage);
                }
                var hs = DataContext.Add(list);
                DataContext.SubmitChanges();
                if (hs.Success)
                {
                    ClearCache();
                    Get();
                }
                return hs;
            }
            catch(Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

    }
}
