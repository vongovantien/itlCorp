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

        public List<Object> GetStages()
        {
            List<Object> returnList = new List<Object>();
            var result = DataContext.Get();
            foreach(var stage in result)
            {
                var department = ((eFMSDataContext)DataContext.DC).CatDepartment.Where(x => x.Id == stage.DepartmentId).FirstOrDefault();
                var returnStage = new { stage, department.DeptName,department.Code };
                returnList.Add(returnStage);               
            }
            return returnList;
        }

        public HandleState UpdateStage(CatStageModel catStage)
        {
            return DataContext.Update(catStage, x => x.Id == catStage.Id);
        }
    }
}
