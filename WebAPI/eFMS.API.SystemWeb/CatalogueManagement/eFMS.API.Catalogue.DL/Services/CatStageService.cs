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
using System.Text;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Common.Globals;
using eFMS.API.Catalogue.Service.Helpers;
using Microsoft.Extensions.Localization;
using eFMS.API.Catalogue.DL.Common;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatStageService : RepositoryBase<CatStage, CatStageModel>, ICatStageService
    {
        private readonly IStringLocalizer stringLocalizer;
        public CatStageService(IContextBase<CatStage> repository, IMapper mapper, IStringLocalizer<LanguageSub> localizer) : base(repository, mapper)
        {
            stringLocalizer = localizer;
        }        
        

        public HandleState AddStage(CatStageModel catStage)
        {
            return DataContext.Add(catStage);
        }
      
        public HandleState DeleteStage(int id)
        {
            return DataContext.Delete(x => x.Id == id);
        }

        public List<Object> GetStages(CatStageCriteria criteria, int page, int size, out int rowsCount)
        {
            List<Object> returnList = new List<Object>();
            var result = new List<CatStage>();
            var departmentList = new List<CatDepartment>();
            if (criteria.condition == SearchCondition.AND)
            {
                var s = DataContext.Get(stage => (stage.Id == criteria.Id || criteria.Id == 0)
                                    && ((stage.StageNameEn ?? "").IndexOf(criteria.StageNameEn ?? "") >= 0)
                                    && ((stage.StageNameVn ?? "").IndexOf(criteria.StageNameVn ?? "") >= 0)
                                    && ((stage.Code ?? "").IndexOf(criteria.Code ?? "") >= 0)
                                    && (stage.Inactive == criteria.Inactive || criteria.Inactive == null))
                                    .OrderByDescending(x => x.DatetimeModified);

                var t = ((eFMSDataContext)DataContext.DC).CatDepartment.Where(x => (x.DeptName ?? "").IndexOf(criteria.DepartmentName ?? "") >= 0);
                result = (from i in s

                          join department in t on i.DepartmentId equals department.Id
                          select i).ToList();
            
            }
            else
            {
                var s = DataContext.Get().OrderByDescending(x => x.DatetimeModified);
                var t = ((eFMSDataContext)DataContext.DC).CatDepartment;
                
                 result = s.Join(t, stage => stage.DepartmentId, department => department.Id, (stage, department) => new { stage, department }).Where(x => 
                    ((x.department.DeptName ?? "").IndexOf(criteria.DepartmentName ?? "") >= 0
                    || (x.stage.StageNameEn ?? "").IndexOf(criteria.StageNameEn ?? "") >= 0
                    || (x.stage.StageNameVn ?? "").IndexOf(criteria.StageNameVn ?? "") >= 0
                    || (x.stage.Code ?? "").IndexOf(criteria.Code ?? "")>=0
                    || (x.stage.Id == criteria.Id)))
                    .Select(x => x.stage).ToList();              
            }

            rowsCount = result.Count;
            
            foreach(var stage in result)
            {
                var department = ((eFMSDataContext)DataContext.DC).CatDepartment.Where(x => x.Id == stage.DepartmentId).FirstOrDefault();
                var returnStage = new { stage, department?.DeptName,department?.Code };
                returnList.Add(returnStage);               
            }

            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                returnList = returnList.Skip((page - 1)*size).Take(size).ToList();
            }

            return returnList;
        }
       

        public List<object> Query(CatStageCriteria criteria)
        {

            List<Object> returnList = new List<Object>();
            var result = new List<CatStage>();
            var departmentList = new List<CatDepartment>();
            if (criteria.condition == SearchCondition.AND)
            {
                var s = DataContext.Get(stage => (stage.Id == criteria.Id || criteria.Id == 0)
                                    && ((stage.StageNameEn ?? "").IndexOf(criteria.StageNameEn ?? "") >= 0)
                                    && ((stage.StageNameVn ?? "").IndexOf(criteria.StageNameVn ?? "") >= 0)
                                    && ((stage.Code ?? "").IndexOf(criteria.Code ?? "") >= 0));

                var t = ((eFMSDataContext)DataContext.DC).CatDepartment.Where(x => (x.DeptName ?? "").IndexOf(criteria.DepartmentName ?? "") >= 0);
                result = (from i in s

                          join department in t on i.DepartmentId equals department.Id
                          select i).ToList();

            }
            else
            {
                var s = DataContext.Get();
                var t = ((eFMSDataContext)DataContext.DC).CatDepartment;

                result = s.Join(t, stage => stage.DepartmentId, department => department.Id, (stage, department) => new { stage, department }).Where(x => ((x.department.DeptName ?? "").IndexOf(criteria.DepartmentName ?? "") >= 0)
                   || ((x.stage.StageNameEn ?? "").IndexOf(criteria.StageNameEn ?? "") >= 0)
                   || ((x.stage.StageNameVn ?? "").IndexOf(criteria.StageNameVn ?? "") >= 0)
                   || ((x.stage.Code ?? "").IndexOf(criteria.Code ?? "") >= 0)
                   || (x.stage.Id == criteria.Id))
                   .Select(x => x.stage).ToList();
            }         

            foreach (var stage in result)
            {
                var department = ((eFMSDataContext)DataContext.DC).CatDepartment.Where(x => x.Id == stage.DepartmentId).FirstOrDefault();
                var returnStage = new { stage, department?.DeptName, department?.Code };
                returnList.Add(returnStage);
            }

            return returnList;
        }

        public HandleState UpdateStage(CatStageModel catStage)
        {
            return DataContext.Update(catStage, x => x.Id == catStage.Id);
        }

        public List<CatStageImportModel> CheckValidImport(List<CatStageImportModel> list)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var stages = dc.CatStage.ToList();
            list.ForEach(item =>
            {
               
                if (string.IsNullOrEmpty(item.StageNameEn))
                {
                    item.StageNameEn = stringLocalizer[LanguageSub.MSG_STAGE_NAME_EN_EMPTY];
                    item.IsValid = false;
                }
                if (item.DepartmentId==null)
                {
                    item.DepartmentId = -1;
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.Status))
                {
                    item.Status = stringLocalizer[LanguageSub.MSG_STAGE_STATUS_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.Code))
                {
                    item.Code = stringLocalizer[LanguageSub.MSG_STAGE_CODE_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    var stage = stages.FirstOrDefault(x => x.Code.ToLower() == item.Code.ToLower());
                    if (stage != null)
                    {
                        item.Code = string.Format(stringLocalizer[LanguageSub.MSG_STAGE_EXISTED],item.Code);
                        item.IsValid = false;
                    }
                    if (list.Count(x => x.Code.ToLower() == item.Code.ToLower()) > 1)
                    {
                        item.Code = string.Format(stringLocalizer[LanguageSub.MSG_STAGE_CODE_DUPLICATE], item.Code);
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
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                foreach(var item in data)
                {
                    var stage = new CatStage
                    {
                        Code = item.Code,
                        StageNameEn = item.StageNameEn,
                        StageNameVn = item.StageNameVn,
                        DepartmentId = item.DepartmentId,
                        DescriptionEn = item.DescriptionEn,
                        DescriptionVn = item.DescriptionVn,
                        DatetimeCreated = DateTime.Now,
                        UserCreated = ChangeTrackerHelper.currentUser,
                        Inactive = item.Status.ToString().ToLower()=="active"?false:true
                    };
                    dc.CatStage.Add(stage);
                }
                dc.SaveChanges();
                return new HandleState();
            }
            catch(Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

    }
}
