using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Mobile.Common;
using API.Mobile.Models;

namespace API.Mobile.Repository
{
    public class CommentRepositoryImpl : ICommentRepository
    {
        private List<Comment> comments = FakeData.comments;
        private List<User> users = FakeData.users;

        public List<Comment> Get(string stageId)
        {
            var listComment = comments.Where(x => x.StageId == stageId).OrderByDescending(x => x.CreatedDate).ToList();
            var list = (from comment in listComment
                join user in users on comment.UserId equals user.UserId
                select new Comment {
                    Id = comment.Id,
                    StageId = comment.StageId,
                    Content = comment.Content,
                    CreatedDate = comment.CreatedDate,
                    UserId = comment.UserId,
                    CommentType = comment.CommentType,
                    UserName = user.EnglishName,
                    Role = user.Role
                }).ToList();
            return list;
        }
    }
}
