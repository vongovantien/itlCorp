using eFMS.API.Setting.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models
{
    public class LogModel
    {
        public Guid Id { get; set; }
        public string ObjectId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Action { get; set; }
        public string UserUpdated { get; set; }
        public DateTime DatetimeUpdated { get; set; }
        public PropertyChange PropertyChange { get; set; }
    }
}
