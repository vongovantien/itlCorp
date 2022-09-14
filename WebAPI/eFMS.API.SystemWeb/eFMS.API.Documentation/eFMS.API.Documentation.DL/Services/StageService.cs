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
        public StageService(IContextBase<CatStage> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public CatStage GetStageByType(string stageType)
        {
            CatStage stage = new CatStage();
            switch (stageType)
            {
                case DocumentConstants.UPDATE_ATA:
                    stage = DataContext.First(x => x.Code == DocumentConstants.UPDATE_ATA_CODE);
                    break;
                case DocumentConstants.UPDATE_ATD:
                    stage = DataContext.First(x => x.Code == DocumentConstants.UPDATE_ATD_CODE);
                    break;
                case DocumentConstants.UPDATE_INCOTERM:
                    stage = DataContext.First(x => x.Code == DocumentConstants.UPDATE_ICT_CODE);
                    break;
                case DocumentConstants.SEND_POD:
                    stage = DataContext.First(x => x.Code == DocumentConstants.SEND_POD_CODE);
                    break;
                case DocumentConstants.SEND_PA:
                    stage = DataContext.First(x => x.Code == DocumentConstants.SEND_PA_CODE);
                    break;
                case DocumentConstants.SEND_AN:
                    stage = DataContext.First(x => x.Code == DocumentConstants.SEND_AN_CODE);
                    break;
                case DocumentConstants.SEND_AL:
                    stage = DataContext.First(x => x.Code == DocumentConstants.SEND_AL_CODE);
                    break;
                default:
                    break;
            }

            return stage;
        }
    }
}
