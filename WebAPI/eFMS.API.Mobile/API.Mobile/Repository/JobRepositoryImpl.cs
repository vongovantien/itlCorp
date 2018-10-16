using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using API.Mobile.Common;
using API.Mobile.Models;
using API.Mobile.Resources;
using API.Mobile.ViewModel;
using Microsoft.Extensions.Localization;
using static API.Mobile.Common.StatusEnum;

namespace API.Mobile.Repository
{
    public class JobRepositoryImpl : IJobRepository
    {
        private List<Job> jobs = FakeData.jobs;
        private List<Stage> stages = FakeData.stages;
        private CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
        private IStringLocalizer stringLocalizer;

        public JobRepositoryImpl(IStringLocalizer<LanguageSub> localizer)
        {
            stringLocalizer = localizer;
        }

        public JobViewModel Get(JobCriteria criteria, string userId, int? offset, int limit = 15)
        {
            var data = Search(criteria, userId);
            var totalItems = data.Count;
            var numberJobFinishs = data.Count(x => x.CurrentStageStatus == StatusEnum.JobStatus.Finish);
            if (offset != null)
            {
                int skip = (int)offset;
                int take = (int)limit;
                data = data.Skip(skip).Take(take).ToList();
            }
            data.ForEach(x =>
            {
                x.CurrentStageStatusName = GetStatusName(x.CurrentStageStatus);
                x.NumberStage = stages.Count(y => y.JobId == x.Id);
                x.NumberStageFinish = stages.Count(y => y.JobId == x.Id && y.Status == StatusEnum.StageStatus.Done);
                x.PercentFinish = Math.Round(decimal.Divide(x.NumberStageFinish, x.NumberStage) * 100);
            });
            var result = new JobViewModel { Jobs = data, TotalItems = totalItems, NumberJobFinishs = numberJobFinishs, Offset = offset, Limit = limit };
            return result;
        }

        private string GetStatusName(JobStatus status)
        {
            string statusName = string.Empty;
            switch (status)
            {
                case JobStatus.InSchedule:
                    statusName = stringLocalizer[LanguageSub.JOB_STATUS_NOTSTART].Value;
                    break;
                case JobStatus.Overdued:
                    statusName = stringLocalizer[LanguageSub.JOB_STATUS_OVERDUED].Value;
                    break;
                case JobStatus.Pending:
                    statusName = stringLocalizer[LanguageSub.JOB_STATUS_PENDING].Value;
                    break;
                case JobStatus.Processing:
                    statusName = stringLocalizer[LanguageSub.JOB_STATUS_PROCESSING].Value;
                    break;
                //case JobStatus.WillOverDue:
                //    statusName = stringLocalizer[LanguageSub.JOB_STATUS_WILLOVERDUED].Value;
                //    break;
                case JobStatus.Finish:
                    statusName = stringLocalizer[LanguageSub.JOB_STATUS_FINISH].Value;
                    break;
                case JobStatus.Canceled:
                    statusName = stringLocalizer[LanguageSub.JOB_STATUS_CANCELED].Value;
                    break;
            }
            return statusName;
        }
        public List<JobPerformance> Get(JobPerformanceCriteria criteria)
        {
            var listJob = jobs;
            if (criteria.ThisWeek)
            {
                DateTime fromDate = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
                DateTime toDate = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
                listJob = listJob.Where(x => (x.AssignTime >= fromDate && x.AssignTime <= toDate)).ToList();
            }
            if (criteria.ThisMonth)
            {
                listJob = listJob.Where(x => (x.AssignTime.Month == DateTime.Now.Month)).ToList();
            }
            if (criteria.ThisQuater)
            {

            }
            if (criteria.ThisYear)
            {
                listJob = listJob.Where(x => (x.AssignTime.Year == DateTime.Now.Year)).ToList();
            }
            if (!string.IsNullOrEmpty(criteria.UserId))
            {
                listJob = listJob.Where(x => x.UserId == criteria.UserId).ToList();
            }
            var results = new List<JobPerformance>();
            var list = listJob.GroupBy(x => x.CurrentStageStatus).Select(x => new { JobStatus = x.Key, NumberJob = x.Count() });
            if(!list.Any(x => x.JobStatus == JobStatus.Canceled))
            {
                results.Add(new JobPerformance { JobStatus = JobStatus.Canceled, NumberJob = 0, StatusName = GetStatusName(JobStatus.Canceled) });
            }
            if (!list.Any(x => x.JobStatus == JobStatus.Finish))
            {
                results.Add(new JobPerformance { JobStatus = JobStatus.Finish, NumberJob = 0, StatusName = GetStatusName(JobStatus.Finish) });
            }
            if (!list.Any(x => x.JobStatus == JobStatus.InSchedule))
            {
                results.Add(new JobPerformance { JobStatus = JobStatus.InSchedule, NumberJob = 0, StatusName = GetStatusName(JobStatus.InSchedule) });
            }
            if (!list.Any(x => x.JobStatus == JobStatus.Overdued))
            {
                results.Add(new JobPerformance { JobStatus = JobStatus.Overdued, NumberJob = 0, StatusName = GetStatusName(JobStatus.Overdued) });
            }
            if (!list.Any(x => x.JobStatus == JobStatus.Pending))
            {
                results.Add(new JobPerformance { JobStatus = JobStatus.Pending, NumberJob = 0, StatusName = GetStatusName(JobStatus.Pending) });
            }
            if (!list.Any(x => x.JobStatus == JobStatus.Processing))
            {
                results.Add(new JobPerformance { JobStatus = JobStatus.Processing, NumberJob = 0, StatusName = GetStatusName(JobStatus.Processing) });
            }
            //if (!list.Any(x => x.JobStatus == JobStatus.WillOverDue))
            //{
            //    results.Add(new JobPerformance { JobStatus = JobStatus.WillOverDue, NumberJob = 0, StatusName = GetStatusName(JobStatus.WillOverDue) });
            //}
            foreach (var item in list)
            {
                var performance = new JobPerformance();
                performance.JobStatus = item.JobStatus;
                performance.NumberJob = item.NumberJob;
                performance.StatusName = GetStatusName(item.JobStatus);
                results.Add(performance);
            }
            return results;
        }
        public Job Get(string id)
        {
            var job = jobs.Find(x => x.Id == id);
            var numberStage = stages.Count(y => y.JobId == job.Id);
            var numberStageFinish = stages.Count(y => y.JobId == job.Id && y.Status == StatusEnum.StageStatus.Done);
            job.PercentFinish = Math.Round(decimal.Divide(numberStageFinish, numberStage) * 100);
            return job;
        }

        public List<Job> GetBy(JobCriteria criteria)
        {
            return Search(criteria, null);
        }

        private List<Job> Search(JobCriteria criteria, string userId)
        {
            var listJob = jobs;
            listJob = listJob.Where(x => ((x.Id ?? "").Contains(criteria.SearchText ?? "") 
                                       || (x.CustomerName ?? "").Contains(criteria.SearchText ?? "")
                                       || (x.PO_NO ?? "").Contains(criteria.SearchText ?? "")
                                       || (x.MBL ?? "").Contains(criteria.SearchText ?? ""))
                                       && (x.UserId == userId || string.IsNullOrEmpty(userId))
                                       && (x.AssignTime >= criteria.FromDate || criteria.FromDate == null)
                                       && (x.AssignTime <= criteria.ToDate || criteria.ToDate == null)
            ).ToList();
            var results = listJob;
            switch (criteria.SearchStatus)
            {
                case StatusEnum.JobStatusSearch.Finish:
                    results = results.Where(x => x.CurrentStageStatus == StatusEnum.JobStatus.Finish).OrderByDescending(x => x.ServiceDate).ToList();
                    break;
                case StatusEnum.JobStatusSearch.InProgess:
                    results = results.Where(x => x.CurrentStageStatus != StatusEnum.JobStatus.Finish).OrderBy(x => x.CurrentStageStatus).ToList();
                    break;
                default:
                    results = results.OrderByDescending(x => x.ServiceDate).ToList();
                    break;
            }

            return results;
        }
    }
}
