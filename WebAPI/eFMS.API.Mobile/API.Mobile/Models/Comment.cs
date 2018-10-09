using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static API.Mobile.Common.StatusEnum;

namespace API.Mobile.Models
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string StageId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserId { get; set; }
        public CommentType CommentType { get; set; }
        public string CommentTypeName { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
    }
}
