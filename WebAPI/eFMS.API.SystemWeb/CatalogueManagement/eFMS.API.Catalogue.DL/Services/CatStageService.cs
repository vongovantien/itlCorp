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

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatStageService : RepositoryBase<CatStage, CatStageModel>, ICatStageService
    {
        public CatStageService(IContextBase<CatStage> repository, IMapper mapper) : base(repository, mapper)
        {
            
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
                                    && ((stage.Code ?? "").IndexOf(criteria.Code ?? "") >= 0));

                var t = ((eFMSDataContext)DataContext.DC).CatDepartment.Where(x => (x.DeptName ?? "").IndexOf(criteria.DepartmentName ?? "") >= 0);
                result = (from i in s

                          join department in t on i.DepartmentId equals department.Id
                          select i).ToList();
                //var s = DataContext.Get();
                //var t = ((eFMSDataContext)DataContext.DC).CatDepartment;
                //result = (from stage in DataContext.Get()
                //         join department in ((eFMSDataContext)DataContext.DC).CatDepartment on stage.DepartmentId equals department.Id
                //         where ((department.DeptName ?? "").IndexOf(criteria.DepartmentName ?? "") >= 0)
                //                && (stage.Id == criteria.Id || criteria.Id == 0)
                //                    && (stage.StageNameEn ?? "").IndexOf(criteria.StageNameEn ?? "") >= 0
                //                    && (stage.StageNameVn ?? "").IndexOf(criteria.StageNameVn ?? "") >= 0
                //                    && (stage.Code ?? "").IndexOf(criteria.Code ?? "") >= 0
                //          select stage).ToList();


            }
            else
            {
                var s = DataContext.Get();
                var t = ((eFMSDataContext)DataContext.DC).CatDepartment;
                
                 result = s.Join(t, stage => stage.DepartmentId, department => department.Id, (stage, department) => new { stage, department }).Where(x => ((x.department.DeptName ?? "").IndexOf(criteria.DepartmentName ?? "") >= 0)
                    || ((x.stage.StageNameEn ?? "").IndexOf(criteria.StageNameEn ?? "") >= 0)
                    || ((x.stage.StageNameVn ?? "").IndexOf(criteria.StageNameVn ?? "") >=0)
                    || ((x.stage.Code ?? "").IndexOf(criteria.Code ?? "")>=0)
                    || (x.stage.Id == criteria.Id))
                    .Select(x => x.stage).ToList();

                //var s = DataContext.Get(stage => (stage.Id == criteria.Id || criteria.Id == 0)
                //                     || ((stage.StageNameEn ?? "").IndexOf(criteria.StageNameEn ?? "") >= 0)
                //                     || ((stage.StageNameVn ?? "").IndexOf(criteria.StageNameVn ?? "") >= 0)
                //                     || ((stage.Code ?? "").IndexOf(criteria.Code ?? "") >= 0));

                //var t = ((eFMSDataContext)DataContext.DC).CatDepartment.Where(x => (x.DeptName ?? "").IndexOf(criteria.DepartmentName ?? "") >= 0);
                //result = (from i in s

                //          join department in t on i.DepartmentId equals department.Id
                //          select i).ToList();
                         

                //var s = DataContext.Get();
                //var t = ((eFMSDataContext)DataContext.DC).CatDepartment.Where(x => (x.DeptName ?? "").IndexOf(criteria.DepartmentName ?? "") >= 0);
                //result = (from stage in DataContext.Get()
                //          join department in t on stage.DepartmentId equals department.Id
                //          where ((stage.Id == criteria.Id || criteria.Id == 0)
                //                     || ((stage.StageNameEn ?? "").IndexOf(criteria.StageNameEn ?? "") >= 0)
                //                     || ((stage.StageNameVn ?? "").IndexOf(criteria.StageNameVn ?? "") >= 0)
                //                     || ((stage.Code ?? "").IndexOf(criteria.Code ?? "") >= 0))
                //          select stage).ToList();
            }

            rowsCount = result.Count;
            
            foreach(var stage in result)
            {
                var department = ((eFMSDataContext)DataContext.DC).CatDepartment.Where(x => x.Id == stage.DepartmentId).FirstOrDefault();
                var returnStage = new { stage, department.DeptName,department.Code };
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


        public HandleState UpdateStage(CatStageModel catStage)
        {
            return DataContext.Update(catStage, x => x.Id == catStage.Id);
        }
    }
}
