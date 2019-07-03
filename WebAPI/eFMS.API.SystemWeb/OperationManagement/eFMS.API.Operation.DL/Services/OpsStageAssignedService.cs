using AutoMapper;
using eFMS.API.Operation.DL.IService;
using eFMS.API.Operation.DL.Models;
using eFMS.API.Operation.Service.Models;
using eFMS.API.Provider.Services.IService;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using eFMS.API.Provider.Models;
using ITL.NetCore.Common;
using eFMS.API.Catalogue.Service.Contexts;
using eFMS.API.Operation.DL.Common;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.API.Common.NoSql;

namespace eFMS.API.Operation.DL.Services
{
    public class OpsStageAssignedService : RepositoryBase<OpsStageAssigned, OpsStageAssignedModel>, IOpsStageAssignedService
    {
        private readonly ICatStageApiService catStageApi;
        private readonly ICurrentUser currentUser;
        private readonly ICatDepartmentApiService catDepartmentApi;
        public OpsStageAssignedService(IContextBase<OpsStageAssigned> repository, IMapper mapper,
            ICatStageApiService stageApi,
            ICurrentUser user,
            ICatDepartmentApiService departmentApi) : base(repository, mapper)
        {
            catStageApi = stageApi;
            currentUser = user;
            catDepartmentApi = departmentApi;
        }

        public HandleState AddMultipleStage(List<OpsStageAssignedEditModel> models, Guid jobId)
        {
            //ChangeTrackerHelper.currentUser = currentUser.UserID;
            var result = new HandleState();
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var stagesByJob = DataContext.Get(x => x.JobId == jobId);
            var listToDelete = stagesByJob.Where(x => !models.Any(model => model.Id == x.Id));
            foreach(var item in models)
            {
                if(item.Id == Guid.Empty)
                {
                    var assignedItem = mapper.Map<OpsStageAssigned>(item);
                    item.Id = Guid.NewGuid();
                    item.JobId = jobId;
                    item.Deadline = item.Deadline ?? null;
                    item.CreatedDate = item.ModifiedDate = DateTime.Now;
                    item.UserCreated = "admin";//currentUser.UserID;
                    dc.Add(item);
                }
            }
            if(listToDelete.Count() > 0)
            {
                var list = mapper.Map<List<OpsStageAssigned>>(listToDelete);
                dc.RemoveRange(list);
            }
            try
            {
                dc.SaveChanges();
            }
            catch (Exception ex)
            {
                result = new HandleState(ex.Message);
            }
            return result;
        }

        public OpsStageAssignedModel GetBy(Guid id)
        {
            var data = DataContext.First(x => x.Id == id);
            if (data == null) return null;
            var stages = catStageApi.GetAll().Result;
            var result = mapper.Map<OpsStageAssignedModel>(data);
            if(stages != null)
            {
                result.StageCode = stages.FirstOrDefault(x => x.Id == result.StageId).Code;
                result.StageNameEN = stages.FirstOrDefault(x => x.Id == result.StageId).StageNameEn;
            }
            return result;
        }

        public List<OpsStageAssignedModel> GetByJob(Guid jobId)
        {
            List<OpsStageAssignedModel> results = null;
            var data = DataContext.Get(x => x.JobId == jobId);
            if (data == null) return null;
            results = MapListToModel(data);
            return results;
        }

        public List<CatStageApiModel> GetNotAssigned(Guid jobId)
        {
            var data = DataContext.Get(x => x.JobId == jobId);
            var stages = catStageApi.GetAll().Result;
            if (stages == null) return null;
            stages = stages.Where(x => !data.Any(assigned => assigned.StageId == x.Id)).ToList();
            return stages;
        }

        private List<OpsStageAssignedModel> MapListToModel(IQueryable<OpsStageAssigned> data)
        {
            var stages = catStageApi.GetAll().Result;
            var departments = catDepartmentApi.GetAll().Result;
            var results = new List<OpsStageAssignedModel>();
            foreach (var item in data)
            {
                var stage = stages?.FirstOrDefault(x => x.Id == item.StageId);
                var assignedItem = mapper.Map<OpsStageAssignedModel>(item);
                assignedItem.StageCode = stage?.Code;
                assignedItem.StageNameEN = stage?.StageNameEn;
                assignedItem.DepartmentName = departments?.FirstOrDefault(x => x.Id == stage.DepartmentId)?.DeptName;
                results.Add(assignedItem);
            }
            return results;
        }
    }
}
