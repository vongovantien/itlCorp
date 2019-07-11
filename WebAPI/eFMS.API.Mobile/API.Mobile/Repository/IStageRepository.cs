using API.Mobile.Common;
using API.Mobile.Models;
using API.Mobile.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static API.Mobile.Common.StatusEnum;

namespace API.Mobile.Repository
{
    public interface IStageRepository
    {
        List<Stage> Get(string jobId, int? offset = null , int? limit = null);
        HandleState UpdateStatus(StageComment model);
        Stage GetBy(string Id);
    }
}
