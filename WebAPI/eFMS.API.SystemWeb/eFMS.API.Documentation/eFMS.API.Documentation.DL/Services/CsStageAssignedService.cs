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
        private readonly IContextBase<CsTransaction> csTransRepository;
        private readonly IStageService stageService;

        public CsStageAssignedService(
            ICurrentUser user,
            IContextBase<OpsTransaction> opsTransRepo,
            IContextBase<CsTransaction> csTransRepo,
            IContextBase<OpsStageAssigned> repository,
            IMapper mapper,
            IStageService catstageService
            ) : base(repository, mapper)
        {
            currentUser = user;
            csTransRepository = csTransRepo;
            stageService = catstageService;
        }

        public async Task<HandleState> AddMutipleStageAssigned(List<CsStageAssignedModel> listStageAssigned)
        {
            var result = new HandleState();

            foreach (var stage in listStageAssigned)
            {
                var assignedItem = mapper.Map<OpsStageAssigned>(stage);
                assignedItem.Id = Guid.NewGuid();
                assignedItem.JobId = stage.JobId;
                assignedItem.Deadline = DateTime.Now;
                assignedItem.Status = TermData.Done;
                assignedItem.Hblid = stage.Hblid;
                assignedItem.StageId = stage.StageId;
                assignedItem.DatetimeCreated = assignedItem.DatetimeModified = DateTime.Now;
                assignedItem.UserCreated = currentUser.UserID;
                assignedItem.MainPersonInCharge = stage.MainPersonInCharge;
                assignedItem.RealPersonInCharge = stage.RealPersonInCharge;
                assignedItem.OrderNumberProcessed = stage.OrderNumberProcessed;
                await DataContext.AddAsync(assignedItem, false);
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
            await DataContext.AddAsync(stageAssigned, false);

            HandleState hs = DataContext.SubmitChanges();
            return hs;
        }

        public async Task<HandleState> AddNewStageAssignedByType(CsStageAssignedCriteria criteria)
        {
            var currentJob = await csTransRepository.FirstAsync(x => x.Id == criteria.JobId);
            var stage = await stageService.GetStageByType(criteria.StageType);

            CsStageAssignedModel newItem = new CsStageAssignedModel();

            newItem.Id = Guid.NewGuid();
            newItem.StageId = stage.Id;
            newItem.Status = TermData.Done;
            newItem.DatetimeCreated = newItem.DatetimeModified;
            newItem.Deadline = DateTime.Now;
            newItem.MainPersonInCharge = newItem.RealPersonInCharge = currentUser.UserID;
            newItem.Hblid = criteria.HblId;
            newItem.JobId = criteria.JobId;
            var orderNumberProcess = await DataContext.CountAsync(x => x.JobId == criteria.JobId);
            newItem.OrderNumberProcessed = orderNumberProcess + 1;

            var result = await AddNewStageAssigned(newItem);
            return result;
        }

        public async Task<List<CsStageAssignedModel>> SetMutipleStageAssigned(List<CatStage> listStages, Guid jobId, Guid hblId)
        {
            List<CsStageAssignedModel> listStageAssigned = new List<CsStageAssignedModel>();
            int orderNumberProcess = await DataContext.CountAsync(x => x.JobId == jobId);
            var result = new HandleState();

            foreach (var stage in listStages)
            {
                var stageAssigned = new CsStageAssignedModel();
                stageAssigned.Id = Guid.NewGuid();
                stageAssigned.JobId = jobId;
                stageAssigned.Deadline = DateTime.Now;
                stageAssigned.Status = TermData.Done;
                stageAssigned.Hblid = hblId;
                stageAssigned.StageId = stage.Id;
                stageAssigned.DatetimeCreated = stageAssigned.DatetimeModified = DateTime.Now;
                stageAssigned.UserCreated = currentUser.UserID;
                stageAssigned.MainPersonInCharge = currentUser.UserID;
                stageAssigned.RealPersonInCharge = currentUser.UserID;
                stageAssigned.OrderNumberProcessed = orderNumberProcess + 1;
                listStageAssigned.Add(stageAssigned);

                orderNumberProcess++;
            }
            return listStageAssigned;
        }
    }
}
