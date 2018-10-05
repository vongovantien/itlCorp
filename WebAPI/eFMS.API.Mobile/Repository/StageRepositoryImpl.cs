using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Mobile.Common;
using API.Mobile.Models;
using API.Mobile.ViewModel;

namespace API.Mobile.Repository
{
    public class StageRepositoryImpl : IStageRepository
    {
        private List<Job> jobs = FakeData.jobs;
        private List<Stage> stages = FakeData.stages;
        private List<Comment> comments = FakeData.comments;
        private User user = FakeData.user;

        public List<Stage> Get(string jobId, int? offset = null, int? limit = null)
        {
            var job = jobs.First(x => x.Id == jobId);
            var results = stages.Where(x => x.JobId == jobId).OrderBy(x => x.Order).ToList();
            if(offset != null && limit != null)
            {
                var skip = (int)offset;
                var take = (int)limit;
                results = results.Skip(skip).Take(take).ToList();
            }
            results.ForEach(x => {
                x.LastComment = comments.Where(y => y.StageId == x.Id).FirstOrDefault()?.Content;
                x.Status = GetStatus(x.EndDate, x.Status);
            });
            return results;
        }

        private StatusEnum.StageStatus GetStatus(DateTime deadLine, StatusEnum.StageStatus status)
        {
            if (status == StatusEnum.StageStatus.Done || status == StatusEnum.StageStatus.Pending) return status;
            var time = (deadLine - DateTime.Now).Hours;
            if (time <= 2)
            {
                status = StatusEnum.StageStatus.WillOverdue;
            }
            if (time == 0)
            {
                status = StatusEnum.StageStatus.Overdued;
            }
            return status;
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
                    newComment = new Comment { Id = Guid.NewGuid(), CommentType = StatusEnum.CommentType.Pending, Content = model.Content, StageId = model.StageId, CreatedDate = DateTime.Now, UserId = user.UserId };
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
                }
                jobs.First(x => x.Id == stage.JobId).CurrentDeadline = stage.EndDate;
                result = new  HandleState();
            }
            return result;
        }
    }
}
