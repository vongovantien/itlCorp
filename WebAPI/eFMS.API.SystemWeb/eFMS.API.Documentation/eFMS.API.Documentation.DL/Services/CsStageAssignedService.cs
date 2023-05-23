using AutoMapper;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsStageAssignedService : RepositoryBase<OpsStageAssigned, OpsStageAssignedModel>, ICsStageAssignedService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CsTransaction> csTransRepository;
        private readonly IContextBase<CsTransactionDetail> csTransDetailRepository;
        private readonly IStageService stageService;
        private readonly IContextBase<CatStage> catStageRepo;

        public CsStageAssignedService(
            ICurrentUser user,
            IContextBase<CsTransaction> csTransRepo,
            IContextBase<CsTransactionDetail> csTransDetailRepo,
            IContextBase<CatStage> catStageRepository,
            IContextBase<OpsStageAssigned> repository,
            IMapper mapper,
            IStageService catstageService
            ) : base(repository, mapper)
        {
            currentUser = user;
            csTransRepository = csTransRepo;
            csTransDetailRepository = csTransDetailRepo;
            stageService = catstageService;
            catStageRepo = catStageRepository;
        }

        public async Task<HandleState> AddNewStageAssigned(CsStageAssignedModel model)
        {
            var stageAssigned = mapper.Map<OpsStageAssigned>(model);
            stageAssigned.Id = Guid.NewGuid();
            stageAssigned.Status = TermData.Done;
            stageAssigned.DatetimeCreated = stageAssigned.DatetimeModified = stageAssigned.Deadline = DateTime.Now;
            stageAssigned.MainPersonInCharge = stageAssigned.RealPersonInCharge = currentUser.UserID;
            var hs = await DataContext.AddAsync(stageAssigned);
            return hs;
        }

        public async Task<HandleState> AddNewStageAssignedByType(CsStageAssignedCriteria criteria)
        {
            var currentJob = await csTransRepository.Get(x => x.Id == criteria.JobId).FirstOrDefaultAsync();
            var hbl = await csTransDetailRepository.Get(x => x.Id == criteria.HblId).FirstOrDefaultAsync();
            var stage = await stageService.GetStageByType(criteria.StageType);
            var orderNumber = DataContext.Where(x => x.JobId == criteria.JobId).Select(x => x.OrderNumberProcessed).Max() ?? 0;

            CsStageAssignedModel newItem = new CsStageAssignedModel();

            newItem.Id = Guid.NewGuid();
            newItem.StageId = stage.Id;
            newItem.Status = TermData.Done;
            newItem.Deadline = DateTime.Now;
            newItem.MainPersonInCharge = newItem.RealPersonInCharge = currentUser.UserID;
            newItem.Hblid = hbl?.Id;
            newItem.Hblno = hbl?.Hwbno;
            newItem.JobId = criteria.JobId;
            newItem.Type = DocumentConstants.FROM_SYSTEM;
            newItem.DatetimeCreated = newItem.DatetimeModified = DateTime.Now;
            newItem.OrderNumberProcessed = orderNumber + 1;

            var result = await AddNewStageAssigned(newItem);
            return result;
        }

        public async Task<HandleState> AddMultipleStageAssigned(Guid jobId, List<CsStageAssignedModel> listStageAssigned)
        {
            var result = new HandleState();
            var orderNumber = DataContext.Where(x => x.JobId == jobId).Select(x => x.OrderNumberProcessed).Max() ?? 0;

            foreach (var stage in listStageAssigned)
            {
                orderNumber++;
                var hbl = csTransDetailRepository.First(x => x.Id == stage.Hblid)?.Hwbno;
                var assignedItem = mapper.Map<OpsStageAssigned>(stage);
                assignedItem.Id = Guid.NewGuid();
                assignedItem.Hblno = hbl;
                if (!string.IsNullOrEmpty(stage.Code) && stage.Code.ToString() == DocumentConstants.SEND_INV_CODE && stage.Type!="User")
                {
                    assignedItem.Status = TermData.Done;
                    assignedItem.StageId = catStageRepo.Get(x=>x.Code==stage.Code).FirstOrDefault().Id;
                    assignedItem.MainPersonInCharge = stage.RealPersonInCharge = currentUser.UserID;
                    assignedItem.Deadline = DateTime.Now;
                    assignedItem.Type = DocumentConstants.FROM_SYSTEM;
                }
                assignedItem.DatetimeModified = assignedItem.DatetimeCreated = DateTime.Now;
                assignedItem.OrderNumberProcessed = orderNumber;

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

        public async Task<HandleState> SetMultipleStageAssigned(CsTransactionDetailModel currentHbl, CsTransactionModel currentJob, Guid jobId, Guid hblId, bool isHbl = false)
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

            listStageAssigned = await SetMultipleStageAssigned(listStages, jobId, hblId);

            hs = await AddMultipleStageAssigned(jobId, listStageAssigned);
            return hs;
        }

        private async Task<List<CsStageAssignedModel>> SetMultipleStageAssigned(List<CatStage> listStages, Guid jobId, Guid hblId)
        {
            List<CsStageAssignedModel> listStageAssigned = new List<CsStageAssignedModel>();

            var orderNumber = DataContext.Where(x => x.JobId == jobId).Select(x => x.OrderNumberProcessed).Max() ?? 0;
            var hbl = new CsTransactionDetail();
            var result = new HandleState();

            if (hblId != null && hblId != Guid.Empty)
            {
                hbl = await csTransDetailRepository.FirstAsync(x => x.Id == hblId);
            }
            foreach (var stage in listStages)
            {
                var stageAssigned = new CsStageAssignedModel();
                stageAssigned.Id = Guid.NewGuid();
                stageAssigned.JobId = jobId;
                stageAssigned.Deadline = DateTime.Now;
                stageAssigned.Status = TermData.Done;
                stageAssigned.Hblid = hblId;
                stageAssigned.Hblno = hbl.Hwbno;
                stageAssigned.StageId = stage.Id;
                stageAssigned.Type = DocumentConstants.FROM_SYSTEM;
                stageAssigned.DatetimeCreated = stageAssigned.DatetimeModified = DateTime.Now;
                stageAssigned.MainPersonInCharge = currentUser.UserID;
                stageAssigned.RealPersonInCharge = currentUser.UserID;
                stageAssigned.OrderNumberProcessed = orderNumber + 1;
                listStageAssigned.Add(stageAssigned);

                orderNumber++;
            }
            return listStageAssigned;
        }
    }
}
