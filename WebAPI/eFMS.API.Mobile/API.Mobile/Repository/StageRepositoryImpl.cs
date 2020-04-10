using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using API.Mobile.Common;
using API.Mobile.Models;
using API.Mobile.Resources;
using API.Mobile.ViewModel;
using Microsoft.Extensions.Localization;

namespace API.Mobile.Repository
{
    public class StageRepositoryImpl : IStageRepository
    {
        private List<Job> jobs = FakeData.jobs;
        private List<Stage> stages = FakeData.stages;
        private List<Comment> comments = FakeData.comments;
        private User user = FakeData.user;
        private CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
        private readonly IStringLocalizer stringLocalizer;

        public StageRepositoryImpl(IStringLocalizer<LanguageSub> localizer)
        {
            stringLocalizer = localizer;
        }
        public List<Stage> Get([Required]string jobId, int? offset = null, int? limit = null)
        {
            //var job = jobs.First(x => x.Id == jobId);
            var stageList = stages;
            stageList = stageList.Where(x => x.JobId == jobId).OrderBy(x => x.Order).ToList();
            if(offset != null && limit != null)
            {
                var skip = (int)offset;
                var take = (int)limit;
                stageList = stageList.Skip(skip).Take(take).ToList();
            }
            var results = new List<Stage>();
            foreach(var item in stageList)
            {
                var stage = GetStage(item);
                results.Add(stage);
            }
            //results.ForEach(x => {
            //    x = GetStage(x);
            //});
            return results;
        }
        private Stage GetStage(Stage stage)
        {
            //if (stage.Status != StatusEnum.StageStatus.Done && stage.Status != StatusEnum.StageStatus.Pending)
            if (stage.Status == StatusEnum.StageStatus.Processing)
            {
                var time = (stage.EndDate - DateTime.Now).TotalMinutes;
                if (time <= 120 && time >0)
                {
                    stage.Status = StatusEnum.StageStatus.WillOverdue;
                }
                if(time <= 0)
                {
                    stage.Status = StatusEnum.StageStatus.Overdued;
                }
            }
            stage.LastComment = comments.Where(y => y.StageId == stage.Id).FirstOrDefault()?.Content;
            stage.StatusName = GetStatusName(stage.Status);
            return stage;
        }

        private string GetStatusName(StatusEnum.StageStatus status)
        {
            string statusName = string.Empty;
            switch (status)
            {
                case StatusEnum.StageStatus.Overdued:
                    statusName = stringLocalizer[LanguageSub.STAGE_STATUS_OVERDUED].Value;
                    break;
                case StatusEnum.StageStatus.WillOverdue:
                    statusName = stringLocalizer[LanguageSub.STAGE_STATUS_WILLOVERDUED].Value;
                    break;
                case StatusEnum.StageStatus.Processing:
                    statusName = stringLocalizer[LanguageSub.STAGE_STATUS_PROCESSING].Value;
                    break;
                case StatusEnum.StageStatus.InSchedule:
                    statusName = stringLocalizer[LanguageSub.STAGE_STATUS_INSCHEDULE].Value;
                    break;
                case StatusEnum.StageStatus.Pending:
                    statusName = stringLocalizer[LanguageSub.STAGE_STATUS_PENDING].Value;
                    break;
                case StatusEnum.StageStatus.Done:
                    statusName = stringLocalizer[LanguageSub.STAGE_STATUS_DONE].Value;
                    break;
            }
            return statusName;
        }

        public Stage GetBy(string Id)
        {
            return stages.FirstOrDefault(x => x.Id == Id);
        }

        public HandleState UpdateStatus(StageComment model)
        {
            HandleState result = null;
            var stage = stages.Find(x => x.Id == model.StageId);
            if (stage == null)
            {
                result = new HandleState(404, "Not found");
            }
            else
            {
                stages.First(x => x.Id == model.StageId).Status = model.Status;
                Comment newComment = null;

                if (model.Status == StatusEnum.StageStatus.Pending)
                {
                    newComment = new Comment {
                        Id = Guid.NewGuid(),
                        CommentType = StatusEnum.CommentType.Pending,
                        Content = model.Content,
                        StageId = model.StageId,
                        CreatedDate = DateTime.Now,
                        UserId = user.UserId };
                    comments.Add(newComment);
                    jobs.First(x => x.Id == stage.JobId).CurrentStageStatus = StatusEnum.JobStatus.Pending;
                }
                if(model.Status == StatusEnum.StageStatus.Done)
                {
                    var list = stages.Where(x => x.JobId == stage.JobId);
                    var countFinish = list.Count(x => x.Status == StatusEnum.StageStatus.Done);
                    if(list.Count() == countFinish)
                    {
                        jobs.First(x => x.Id == stage.JobId).CurrentStageStatus = StatusEnum.JobStatus.Finish;
                    }

                    newComment = new Comment
                    {
                        Id = Guid.NewGuid(),
                        CommentType = StatusEnum.CommentType.Done,
                        Content = model.Content,
                        StageId = model.StageId,
                        CreatedDate = DateTime.Now,
                        UserId = user.UserId
                    };
                    comments.Add(newComment);
                }
                jobs.First(x => x.Id == stage.JobId).CurrentDeadline = stage.EndDate;
                result = new  HandleState();
            }
            return result;
        }
    }
}
