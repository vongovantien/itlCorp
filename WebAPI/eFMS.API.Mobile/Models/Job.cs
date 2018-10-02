using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static API.Mobile.Common.StatusEnum;

namespace API.Mobile.Models
{
    public class Job
    {
        public string Id { get; set; }
        public string MBL { get; set; }
        public string CustomerName { get; set; }
        public string PO_NO { get; set; }
        public DateTime ServiceDate { get; set; }
        public string PlaceFrom { get; set; }
        public string PlaceTo { get; set; }
        public string Warehouse { get; set; }
        public string ContFCL { get; set; }
        public string ContLCL { get; set; }
        public decimal GW { get; set; }
        public int CBM { get; set; }
        public int Weight { get; set; }
        public string Route { get; set; }
        public DateTime AssignTime { get; set; }
        public int EstimateDate { get; set; }
        public int NumberStage { get; set; }
        public int NumberStageFinish { get; set; }
        public string UserId { get; set; }
        public DateTime CurrentDeadline { get; set; }
        public JobStatus CurrentStageStatus { get; set; }
    }

    public class JobCriteria
    {
        //public string Id { get; set; }
        //public string MBL { get; set; }
        //public string CustomerName { get; set; }
        //public string PO_NO { get; set; }
        //public string UserId { get; set; }
        public string SearchText { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public JobStatusSearch SearchStatus { get; set; } = JobStatusSearch.All;
    }
}
