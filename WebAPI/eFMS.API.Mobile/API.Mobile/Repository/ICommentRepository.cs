using API.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Mobile.Repository
{
    public interface ICommentRepository
    {
        List<Comment> Get(string stageId);
    }
}
