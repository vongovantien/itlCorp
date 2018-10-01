using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Mobile.Common;
using API.Mobile.Models;
using API.Mobile.ViewModel;

namespace API.Mobile.Repository
{
    public class JobRepositoryImpl : IJobRepository
    {
        private List<Job> jobs = FakeData.jobs;
        private List<Stage> stages = FakeData.stages;

        public JobViewModel Get(JobCriteria criteria, int? offset, int limit = 15)
        {
            var data = Search(criteria);
            var totalItems = data.Count;
            var numberJobFinishs = data.Count(x => x.CurrentStageStatus == StatusEnum.JobStatus.Finish);
            if (offset != null)
            {
                int skip = (int)offset;
                int take = (int)limit;
                data = data.Skip(skip).Take(take).ToList();
            }

            data.ForEach(x => {
                x.NumberStageFinish = stages.Count(y => y.JobId == x.Id && y.Status == StatusEnum.StageStatus.Done);
                x.NumberStage = stages.Count(y => y.JobId == x.Id);
            });
            var result = new JobViewModel { Jobs = data, TotalItems = totalItems, NumberJobFinishs = numberJobFinishs, Offset = offset, Limit = limit };
            return result;
        }

        public List<Job> GetBy(JobCriteria criteria)
        {
            return Search(criteria);
        }

        private List<Job> Search(JobCriteria criteria)
        {
            jobs = jobs.Where(x => ((x.Id ?? "").Contains(criteria.SearchText ?? ""))
                                       && ((x.CustomerName ?? "").Contains(criteria.SearchText ?? ""))
                                       && ((x.PO_NO ?? "").Contains(criteria.SearchText ?? ""))
                                       && (x.AssignTime >= criteria.FromDate || criteria.FromDate == null)
                                       && (x.AssignTime <= criteria.ToDate || criteria.ToDate == null)
                                       && ((x.UserId ?? "").Contains(criteria.SearchText ?? ""))
                                       && ((x.MBL ?? "").Contains(criteria.SearchText ?? ""))
            ).ToList();
            var results = jobs;
            switch (criteria.SearchStatus)
            {
                case StatusEnum.JobStatusSearch.Finish:
                    results = results.Where(x => x.CurrentStageStatus == StatusEnum.JobStatus.Finish).OrderBy(x => x.ServiceDate).ToList();
                    break;
                case StatusEnum.JobStatusSearch.InProgess:
                    results = results.Where(x => x.CurrentStageStatus != StatusEnum.JobStatus.Finish).OrderBy(x => x.CurrentStageStatus).ToList();
                    break;
                default:
                    results = results.OrderBy(x => x.ServiceDate).ToList();
                    break;
            }

            return results;
        }
    }
}
