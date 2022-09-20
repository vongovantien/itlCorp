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
using Microsoft.EntityFrameworkCore;
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
        private readonly IContextBase<CsTransactionDetail> csTransDetailRepository;
        private readonly IStageService stageService;

        public CsStageAssignedService(
            ICurrentUser user,
            IContextBase<CsTransaction> csTransRepo,
            IContextBase<CsTransactionDetail> csTransDetailRepo,
            IContextBase<OpsStageAssigned> repository,
            IMapper mapper,
            IStageService catstageService
            ) : base(repository, mapper)
        {
            currentUser = user;
            csTransRepository = csTransRepo;
            csTransDetailRepository = csTransDetailRepo;
            stageService = catstageService;
        }

        public async Task<HandleState> AddNewStageAssigned(CsStageAssignedModel model)
        {
            var stageAssigned = mapper.Map<OpsStageAssigned>(model);
            var hs = await DataContext.AddAsync(stageAssigned);
            return hs;
        }

        public async Task<HandleState> AddNewStageAssignedByType(CsStageAssignedCriteria criteria)
        {
            var currentJob = await csTransRepository.Get(x => x.Id == criteria.JobId).FirstOrDefaultAsync();
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

        public async Task<HandleState> SetMutipleStageAssigned(CsTransactionDetailModel currentHbl, CsTransactionModel currentJob, Guid jobId, Guid hblId, bool isHbl = false)
        {
            var listStageAssigned = new List<CsStageAssignedModel>();
            var listStages = new List<CatStage>();
            var stage = new CatStage();
            var hs = new HandleState();

            if (isHbl)
            {
                var updatedHbl = await csTransDetailRepository.Get(x => x.Id == hblId).FirstOrDefaultAsync();
                if (currentHbl.ArrivalDate != updatedHbl.ArrivalDate)
                {
                    stage = await stageService.GetStageByType(DocumentConstants.UPDATE_ATA);
                    listStages.Add(stage);
                }

                if (currentHbl.IncotermId != updatedHbl.IncotermId)
                {
                    stage = await stageService.GetStageByType(DocumentConstants.UPDATE_INCOTERM);
                    listStages.Add(stage);
                }
            }
            else
            {
                var updatedJob = await csTransRepository.Get(x => x.Id == jobId).FirstOrDefaultAsync();
                if (currentJob.Ata != updatedJob.Ata)
                {
                    stage = await stageService.GetStageByType(DocumentConstants.UPDATE_ATA);
                    listStages.Add(stage);
                }

                if (currentJob.Atd != updatedJob.Atd)
                {
                    stage = await stageService.GetStageByType(DocumentConstants.UPDATE_ATD);
                    listStages.Add(stage);
                }

                if (currentJob.IncotermId != updatedJob.IncotermId)
                {
                    stage = await stageService.GetStageByType(DocumentConstants.UPDATE_INCOTERM);
                    listStages.Add(stage);
                }
            }

            listStageAssigned = await SetMutipleStageAssigned(listStages, jobId, hblId);

            hs = await AddMutipleStageAssigned(listStageAssigned);
            return hs;
        }

        private async Task<List<CsStageAssignedModel>> SetMutipleStageAssigned(List<CatStage> listStages, Guid jobId, Guid hblId)
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
