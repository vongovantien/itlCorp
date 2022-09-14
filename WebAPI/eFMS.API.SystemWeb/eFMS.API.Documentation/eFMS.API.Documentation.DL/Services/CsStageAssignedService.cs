using AutoMapper;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsStageAssignedService : RepositoryBase<OpsStageAssigned, CsStageAssignedCriteria>, ICsStageAssignedService 
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<OpsTransaction> opsTransRepository;
        private readonly IContextBase<CsTransaction> csTransRepository;
        private readonly IStageService _stageService;
        public CsStageAssignedService(
            ICurrentUser user,
            IContextBase<OpsTransaction> opsTransRepo,
            IContextBase<CsTransaction> csTransRepo,
            IContextBase<CatStage> catStageRepo,
            IStageService stageService,
            IContextBase<OpsStageAssigned> repository,
            IMapper mapper
            ) : base(repository, mapper)
        {
            currentUser = user;
            opsTransRepository = opsTransRepo;
            csTransRepository = csTransRepo;
            _stageService = stageService;
        }

        public async Task<HandleState> AddMutipleStageAssigned(List<CsStageAssignedModel> listStages, Guid jobId)
        {
            int orderNumberProcess = DataContext.Count(x => x.JobId == jobId);
            var result = new HandleState();

            foreach (var stage in listStages)
            {
                var assignedItem = mapper.Map<OpsStageAssigned>(stage);
                assignedItem.Id = Guid.NewGuid();
                assignedItem.JobId = jobId;
                assignedItem.Deadline = stage.Deadline ?? null;
                assignedItem.Status = TermData.Done;
                assignedItem.Hblid = stage.Hblid;
                assignedItem.JobId = stage.JobId;
                assignedItem.DatetimeCreated = assignedItem.DatetimeModified = DateTime.Now;
                assignedItem.UserCreated = currentUser.UserID;
                assignedItem.OrderNumberProcessed = orderNumberProcess + 1;
                DataContext.Add(assignedItem, false);

                orderNumberProcess++;
            }

            try
            {
                result = DataContext.SubmitChanges();
            }
            catch (Exception ex)
            {
                result = new HandleState(ex.Message);
            }
            return result;
        }


        public async Task<HandleState> AddNewStageAssigned(CsStageAssignedModel model)
        {
            var stageAssigned = mapper.Map<OpsStageAssigned>(model);
            DataContext.Add(stageAssigned, false);

            HandleState hs = DataContext.SubmitChanges();
            return hs;
        }

        public async Task<HandleState> AddNewStageAssignedByType(CsStageAssignedCriteria criteria)
        {
            var currentJob = csTransRepository.First(x => x.Id == criteria.JobId);
            var stage = _stageService.GetStageByType(criteria.StageType);

            CsStageAssignedModel newItem = new CsStageAssignedModel();

            newItem.Id = Guid.NewGuid();
            newItem.StageId = stage.Id;
            newItem.Status = TermData.Done;
            newItem.DatetimeCreated = newItem.DatetimeModified;
            newItem.Deadline = DateTime.Now;
            newItem.MainPersonInCharge = newItem.RealPersonInCharge = currentUser.UserID;
            newItem.Hblid = criteria.HblId;
            newItem.JobId = criteria.JobId;
            var orderNumberProcess = DataContext.Count(x => x.JobId == criteria.JobId);
            newItem.OrderNumberProcessed = orderNumberProcess + 1;

            var result = await AddNewStageAssigned(newItem);
            return result;
        }
    }
}
