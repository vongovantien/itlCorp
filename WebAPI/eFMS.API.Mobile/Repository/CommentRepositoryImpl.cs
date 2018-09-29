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

        public List<Comment> Get(string stageId)
        {
            return comments.Where(x => x.StageId == stageId).ToList();
        }
    }
}
