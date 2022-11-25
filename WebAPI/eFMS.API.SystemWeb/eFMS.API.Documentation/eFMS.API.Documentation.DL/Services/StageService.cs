using AutoMapper;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Services
{
    public class StageService : RepositoryBase<CatStage, CatStageModel>, IStageService
    {
        public StageService(IContextBase<CatStage> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public async Task<CatStage> GetStageByType(string stageType)
        {
            CatStage stage = new CatStage();
            switch (stageType)
            {
                case DocumentConstants.UPDATE_ATA:
                    stage = await DataContext.Get(x => x.Code == DocumentConstants.UPDATE_ATA_CODE).FirstOrDefaultAsync();
                    break;
                case DocumentConstants.UPDATE_ATD:
                    stage = await DataContext.Get(x => x.Code == DocumentConstants.UPDATE_ATD_CODE).FirstOrDefaultAsync();
                    break;
                case DocumentConstants.UPDATE_INCOTERM:
                    stage = await DataContext.Get(x => x.Code == DocumentConstants.UPDATE_ICT_CODE).FirstOrDefaultAsync();
                    break;
                case DocumentConstants.UPDATE_POD:
                    stage = await DataContext.Get(x => x.Code == DocumentConstants.UPDATE_POD_CODE).FirstOrDefaultAsync();
                    break;
                case DocumentConstants.SEND_POD:
                    stage = await DataContext.Get(x => x.Code == DocumentConstants.SEND_POD_CODE).FirstOrDefaultAsync();
                    break;
                case DocumentConstants.SEND_PA:
                    stage = await DataContext.Get(x => x.Code == DocumentConstants.SEND_PA_CODE).FirstOrDefaultAsync();
                    break;
                case DocumentConstants.SEND_AN:
                    stage = await DataContext.Get(x => x.Code == DocumentConstants.SEND_AN_CODE).FirstOrDefaultAsync();
                    break;
                case DocumentConstants.SEND_AL:
                    stage = await DataContext.Get(x => x.Code == DocumentConstants.SEND_AL_CODE).FirstOrDefaultAsync();
                    break;
                case DocumentConstants.SEND_DO:
                    stage = await DataContext.Get(x => x.Code == DocumentConstants.SEND_DO_CODE).FirstOrDefaultAsync();
                    break;
                case DocumentConstants.SEND_HB:
                    stage = await DataContext.Get(x => x.Code == DocumentConstants.SEND_HB_CODE).FirstOrDefaultAsync();
                    break;
                default:
                    break;
            }

            return stage;
        }
    }
}
