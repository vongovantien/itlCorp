using eFMS.API.Documentation.Service.Models;
using System;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsStageAssignedCriteria
    {
        public string StageType { get; set; }
        public Guid JobId { get; set; }
        public Guid HblId { get; set; }
    }
}
