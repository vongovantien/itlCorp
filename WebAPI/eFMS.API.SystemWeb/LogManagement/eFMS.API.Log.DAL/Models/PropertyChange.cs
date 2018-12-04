using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Log.Service.Models
{
    public class PropertyChange
    {
        public string PropertyName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
