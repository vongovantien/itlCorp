using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.Service.Models
{
    public class PropertyCommon
    {
        public string PrimaryKeyValue { get; set; }
        public EntityState ActionType { get; set; }
        public PropertyChange PropertyChange { get; set; }
        public DateTime DatetimeModified { get; set; }
        public string UserModified { get; set; }
    }
}
