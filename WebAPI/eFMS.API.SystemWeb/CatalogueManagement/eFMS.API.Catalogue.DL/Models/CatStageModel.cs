using eFMS.API.Catalogue.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatStageModel : CatStage
    {
        public string DeptName { get; set; }
        public string DeptCode { get; set; }
    }
}
