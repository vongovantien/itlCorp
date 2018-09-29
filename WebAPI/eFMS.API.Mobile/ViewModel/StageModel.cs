using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static API.Mobile.Common.StatusEnum;

namespace API.Mobile.ViewModel
{
    public class StageModel
    {
        [Required]
        public string StageId { get; set; }
        [Required]
        public StageStatus Status { get; set; }
        public string Content { get; set; }
    }
}
