using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsMawbcontainerEdit
    {
        public List<CsMawbcontainerModel> CsMawbcontainerModels { get; set; }
        public Guid? MasterId { get; set; }
        public Guid? HousebillId { get; set; }
    }
}
