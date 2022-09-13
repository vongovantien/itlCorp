using AutoMapper;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class StageService : RepositoryBase<CatStage, CatStageModel>, IStageService
    {
        private readonly IContextBase<CatStage> catStageRepository;
        public StageService(IContextBase<CatStage> repository, IMapper mapper, IContextBase<CatStage> catStageRepo) : base(repository, mapper)
        {
            catStageRepository = catStageRepo;
        }

        public CatStage GetStageByType(string stageType, string transactionType)
        {
            CatStage stage = new CatStage();
            transactionType = transactionType.Substring(0, 1);
            switch (transactionType)
            {
                case "A":
                    switch (stageType)
                    {
                        case DocumentConstants.UPDATE_ATA:
                            stage = catStageRepository.First(x => x.Code == DocumentConstants.UPDATE_AIR_ATA);
                            break;
                        case DocumentConstants.UPDATE_ATD:
                            stage = catStageRepository.First(x => x.Code == DocumentConstants.UPDATE_AIR_ATD);
                            break;
                        case DocumentConstants.UPDATE_INCOTERM:
                            stage = catStageRepository.First(x => x.Code == DocumentConstants.UPDATE_AIR_ICT);
                            break;
                        case DocumentConstants.SEND_POD:
                            stage = catStageRepository.First(x => x.Code == DocumentConstants.SEND_AIR_POD);
                            break;
                        case DocumentConstants.SEND_PA:
                            stage = catStageRepository.First(x => x.Code == DocumentConstants.SEND_AIR_PA);
                            break;
                        case DocumentConstants.SEND_AN:
                            stage = catStageRepository.First(x => x.Code == DocumentConstants.SEND_AIR_AN);
                            break;
                        case DocumentConstants.SEND_AL:
                            stage = catStageRepository.First(x => x.Code == DocumentConstants.SEND_AIR_AL);
                            break;
                        default:
                            break;
                    }
                    break;
                case "S":
                    switch (stageType)
                    {
                        case DocumentConstants.UPDATE_ATA:
                            stage = catStageRepository.First(x => x.Code == DocumentConstants.UPDATE_AIR_ATA);
                            break;
                        case DocumentConstants.UPDATE_ATD:
                            stage = catStageRepository.First(x => x.Code == DocumentConstants.UPDATE_AIR_ATD);
                            break;
                        case DocumentConstants.UPDATE_INCOTERM:
                            stage = catStageRepository.First(x => x.Code == DocumentConstants.UPDATE_AIR_ICT);
                            break;
                        case DocumentConstants.SEND_POD:
                            stage = catStageRepository.First(x => x.Code == DocumentConstants.SEND_AIR_POD);
                            break;
                        case DocumentConstants.SEND_PA:
                            stage = catStageRepository.First(x => x.Code == DocumentConstants.SEND_AIR_PA);
                            break;
                        case DocumentConstants.SEND_AN:
                            stage = catStageRepository.First(x => x.Code == DocumentConstants.SEND_AIR_AN);
                            break;
                        case DocumentConstants.SEND_DO:
                            stage = catStageRepository.First(x => x.Code == DocumentConstants.SEND_SEA_DO);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            return stage;
        }
    }
}
