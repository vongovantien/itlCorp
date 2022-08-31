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

namespace eFMS.API.Documentation.DL.Services
{
    public class CsStageAssignedService : RepositoryBase<OpsStageAssigned, CsStageAssignedModel>, ICsStageAssignedService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<OpsTransaction> opsTransRepository;
        private readonly IContextBase<CsTransaction> csTransactionReporsitory;
        private readonly IContextBase<SysUser> userRepository;
        public CsStageAssignedService(
            ICurrentUser user,
            IContextBase<OpsTransaction> opsTransRepo,
            IContextBase<CsTransaction> csTransactionRepo,
            IContextBase<SysUser> userRepo,
            IContextBase<CatStage> catStageRepo,
            IContextBase<OpsStageAssigned> repository,
            IMapper mapper
            ) : base(repository, mapper)
        {
            currentUser = user;
            opsTransRepository = opsTransRepo;
            catStageRepository = catStageRepo;
        }
        public HandleState AddNewStageAssignedByTransactionType(CsStageAssignedCriteria criteria)
        {
            CsTransaction job = csTransactionReporsitory.First(x => x.Id == criteria.JobId);
            CatStage stage = new CatStage();
            OpsStageAssigned newStage = new OpsStageAssigned();
            int orderNumberProcess = 0;
            var dateTimeCreated = DateTime.Now;
            var transactionType = job.TransactionType.Substring(0, 1);

            switch (criteria.StageType)
            {
                case TermData.SEND_POD:
                    switch (transactionType)
                    {
                        case "A":
                            stage = catStageRepository.First(x => x.Code == TermData.SendAirPOD);
                            break;
                        case "S":
                            stage = catStageRepository.First(x => x.Code == TermData.SendSeaPOD);
                            break;
                        default:
                            break;
                    }

                    newStage.Id = Guid.NewGuid();
                    newStage.StageId = stage.Id;
                    newStage.Status = TermData.Done;
                    newStage.DatetimeCreated = newStage.DatetimeModified = dateTimeCreated;
                    newStage.Deadline = dateTimeCreated;
                    newStage.MainPersonInCharge = newStage.RealPersonInCharge = currentUser.UserID;
                    newStage.Hblid = criteria.HblId;
                    newStage.JobId = criteria.JobId;
                    orderNumberProcess = DataContext.Count(x => x.JobId == criteria.JobId);
                    newStage.OrderNumberProcessed = orderNumberProcess + 1;
                    break;

                case TermData.SEND_PA:
                    switch (transactionType)
                    {
                        case "A":
                            stage = catStageRepository.First(x => x.Code == TermData.SendAirPA);
                            break;
                        case "S":
                            stage = catStageRepository.First(x => x.Code == TermData.SendSeaPA);
                            break;
                        default:
                            break;
                    }

                    newStage.Id = Guid.NewGuid();
                    newStage.StageId = stage.Id;
                    newStage.Status = TermData.Done;
                    newStage.DatetimeCreated = newStage.DatetimeModified = dateTimeCreated;
                    newStage.Deadline = dateTimeCreated;
                    newStage.MainPersonInCharge = newStage.RealPersonInCharge = currentUser.UserID;
                    newStage.JobId = criteria.JobId;
                    newStage.Hblid = criteria.HblId;
                    orderNumberProcess = DataContext.Count(x => x.JobId == criteria.JobId);
                    newStage.OrderNumberProcessed = orderNumberProcess + 1;
                    break;
                case TermData.SEND_AL:
                    stage = catStageRepository.First(x => x.Code == TermData.SendAirAL);

                    newStage.Id = Guid.NewGuid();
                    newStage.StageId = stage.Id;
                    newStage.Status = TermData.Done;
                    newStage.DatetimeCreated = newStage.DatetimeModified = dateTimeCreated;
                    newStage.Deadline = dateTimeCreated;
                    newStage.MainPersonInCharge = newStage.RealPersonInCharge = currentUser.UserID;
                    newStage.JobId = criteria.JobId;
                    orderNumberProcess = DataContext.Count(x => x.JobId == criteria.JobId);
                    newStage.OrderNumberProcessed = orderNumberProcess + 1;
                    break;
                case TermData.SEND_AN:
                    switch (transactionType)
                    {
                        case "A":
                            stage = catStageRepository.First(x => x.Code == TermData.SendAirAN);
                            break;
                        case "S":
                            stage = catStageRepository.First(x => x.Code == TermData.SendSeaAN);
                            break;
                        default:
                            break;
                    }

                    newStage.Id = Guid.NewGuid();
                    newStage.StageId = stage.Id;
                    newStage.Status = TermData.Done;
                    newStage.DatetimeCreated = newStage.DatetimeModified = dateTimeCreated;
                    newStage.Deadline = dateTimeCreated;
                    newStage.MainPersonInCharge = newStage.RealPersonInCharge = currentUser.UserID;
                    newStage.JobId = criteria.JobId;
                    orderNumberProcess = DataContext.Count(x => x.JobId == criteria.JobId);
                    newStage.OrderNumberProcessed = orderNumberProcess + 1;
                    break;
                case TermData.SEND_DO:
                    stage = catStageRepository.First(x => x.Code == TermData.SendSeaDO);

                    newStage.Id = Guid.NewGuid();
                    newStage.StageId = stage.Id;
                    newStage.Status = TermData.Done;
                    newStage.DatetimeCreated = newStage.DatetimeModified = dateTimeCreated;
                    newStage.Deadline = dateTimeCreated;
                    newStage.MainPersonInCharge = newStage.RealPersonInCharge = currentUser.UserID;
                    newStage.JobId = criteria.JobId;
                    orderNumberProcess = DataContext.Count(x => x.JobId == criteria.JobId);
                    newStage.OrderNumberProcessed = orderNumberProcess + 1;
                    break;
                default:
                    break;
            }

            DataContext.Add(newStage, false);

            HandleState hs = DataContext.SubmitChanges();
            return hs;
        }

        public HandleState AddNewStageAssigned(CsStageAssignedModel model)
        {
                result = new HandleState(ex.Message);
            }
            return result;
        }

        public HandleState AddMutipleStageAssigned(List<CsStageAssignedModel> listStages, Guid jobId)
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
                assignedItem.DatetimeCreated = assignedItem.DatetimeModified = DateTime.Now;
                assignedItem.UserCreated = currentUser.UserID;
                assignedItem.OrderNumberProcessed = orderNumberProcess;
                DataContext.Add(assignedItem, false);

                orderNumberProcess++;
            }
            try
            {
                DataContext.SubmitChanges();
            }
            catch (Exception ex)
            {
                result = new HandleState(ex.Message);
            }
            return result;
        }
    }
}
