using API.Mobile.Common;
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
        public int NetWeight { get; set; }
        public string Route { get; set; }
        public DateTime AssignTime { get; set; }
        public int EstimateDate { get; set; }
        public DateTime EstimateBeginDate { get; set; }
        public int NumberStage { get; set; }
        public int NumberStageFinish { get; set; }
        public decimal PercentFinish { get; set; }
        public string UserId { get; set; }
        public DateTime CurrentDeadline { get; set; }
        public JobStatus CurrentStageStatus { get; set; }
        public string CurrentStageStatusName { get; set; }
        public ServiceType ServiceType { get; set; }
        public ServiceMethod ServiceMethod { get; set; }
        public string ServiceName { get; set; }
    }

    public class JobDetailModel
    {
        public Job Job { get; set; }
        public List<Stage> Stages { get; set; }
    }

    public class JobCriteria
    {
        public string SearchText { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public JobStatusSearch SearchStatus { get; set; } = JobStatusSearch.All;
    }
    public class JobPerformance
    {
        public JobStatus JobStatus { get; set; }
        public string StatusName { get; set; }
        public int NumberJob { get; set; }
    }

    public class JobPerformanceCriteria
    {
        public bool ThisWeek { get; set; }
        public bool ThisMonth { get; set; }
        public bool ThisQuater { get; set; }
        public bool ThisYear { get; set; }
        public string UserId { get; set; }
    }
}
