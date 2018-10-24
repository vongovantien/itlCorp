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
            if (criteria.condition == Condition.AND)
            {
                result = DataContext.Get().Where(x => (x.Id == criteria.Id || criteria.Id == 0)
                && (x.StageNameEn ?? "").IndexOf(criteria.StageNameEn ?? "") >=0 
                && (x.StageNameVn ?? "").IndexOf(criteria.StageNameVn ?? "") >=0
                && (x.Code ?? "").IndexOf(criteria.Code ?? "") >=0
                && (x.DepartmentId == criteria.DepartmentId || criteria.Id==0)).ToList();               
            }
            else
            {
                result = DataContext.Get().Where(x => (x.Id == criteria.Id || criteria.Id == 0)
                || (x.StageNameEn ?? "").IndexOf(criteria.StageNameEn ?? "") >= 0
                || (x.StageNameVn ?? "").IndexOf(criteria.StageNameVn ?? "") >= 0
                || (x.Code ?? "").IndexOf(criteria.Code ?? "") >= 0
                || (x.DepartmentId == criteria.DepartmentId || criteria.Id == 0)).ToList();
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
