using API.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Mobile.Common
{
    public static class FakeData
    {
        public static User user = new User{ UserId = "User 1", Role = "CS", StaffId ="6789", Password = "123456", FullName ="Trịnh Xuân Thạch", EnglishName = "The Rock", Position = "CS Staff", ImageUrl = "https://m.media-amazon.com/images/M/MV5BY2EyOTI3NGUtMGRjYy00ZWUyLTk0NDgtNjJmYjQ3OWEwM2FhXkEyXkFqcGdeQXVyMzM4MjM0Nzg@._V1_.jpg" };
        public static List<Job> jobs = new List<Job> {
            new Job { UserId = user.UserId, Id = "AE1303/0022", MBL = "VPC/TAHAP18084318", PO_NO = "Q3-17-GH", CustomerName = "DONG HUNG INDUSTRIAL JOIN STOCK COMPANY", PlaceFrom = "AL Arish", PlaceTo = "Albani", EstimateDate = 0, AssignTime = DateTime.Now, NumberStage = 6, NumberStageFinish = 0, Warehouse = "Unilever VN - Củ Chi, KCN Tây Bắc Củ Chi, TP HCM", CBM = 2000, GW = 256789, ContFCL = "01X20'DC & 14X40'DC", ContLCL = "01X20'DC & 14X40'DC", CurrentDeadline = DateTime.Now.AddDays(3), CurrentStageStatus = StatusEnum.JobStatus.Processing, Service = "Air Import" },
            new Job { UserId = user.UserId, Id = "AE1303/0023", MBL = "VPC/TAHAP18084318", PO_NO = "Q3-17-GH", CustomerName = "DONG HUNG INDUSTRIAL JOIN STOCK COMPANY", PlaceFrom = "AL Arish", PlaceTo = "Albani", EstimateDate = 0, AssignTime = DateTime.Now.AddDays(-2), NumberStage = 0, NumberStageFinish = 0, Warehouse = "Unilever VN - Củ Chi, KCN Tây Bắc Củ Chi, TP HCM", CBM = 2000, GW = 256789, ContFCL = "01X20'DC & 14X40'DC", ContLCL = "01X20'DC & 14X40'DC", CurrentDeadline = DateTime.Now.AddHours(5), CurrentStageStatus = StatusEnum.JobStatus.NotStart, Service = "Air Export" },
            new Job { UserId = user.UserId, Id = "AE1303/0024", MBL = "VPC/TAHAP18084318", PO_NO = "Q3-17-GH", CustomerName = "DONG HUNG INDUSTRIAL JOIN STOCK COMPANY", PlaceFrom = "AL Arish", PlaceTo = "Albani", EstimateDate = 0, AssignTime = DateTime.Now.AddDays(-1), NumberStage = 0, NumberStageFinish = 0, Warehouse = "Unilever VN - Củ Chi, KCN Tây Bắc Củ Chi, TP HCM", CBM = 2000, GW = 256789, ContFCL = "01X20'DC & 14X40'DC", ContLCL = "01X20'DC & 14X40'DC", CurrentDeadline = DateTime.Now, CurrentStageStatus = StatusEnum.JobStatus.Overdued, Service = "Sea Import" },
            new Job { UserId = user.UserId, Id = "AE1303/0025", MBL = "VPC/TAHAP18084318", PO_NO = "Q3-17-GH", CustomerName = "DONG HUNG INDUSTRIAL JOIN STOCK COMPANY", PlaceFrom = "AL Arish", PlaceTo = "Albani", EstimateDate = 0, AssignTime = DateTime.Now.AddHours(-1), NumberStage = 0, NumberStageFinish = 0, Warehouse = "Unilever VN - Củ Chi, KCN Tây Bắc Củ Chi, TP HCM", CBM = 2000, GW = 256789, ContFCL = "01X20'DC & 14X40'DC", ContLCL = "01X20'DC & 14X40'DC", CurrentDeadline = DateTime.Now.AddHours(2), CurrentStageStatus = StatusEnum.JobStatus.WillOverDue, Service = "Sea Export" },
            new Job { UserId = user.UserId, Id = "AE1303/0026", MBL = "VPC/TAHAP18084319", PO_NO = "Q3-17-GH", CustomerName = "DONG HUNG INDUSTRIAL JOIN STOCK COMPANY", PlaceFrom = "AL Arish", PlaceTo = "Albani", EstimateDate = 0, AssignTime = DateTime.Now.AddHours(-1), NumberStage = 0, NumberStageFinish = 0, Warehouse = "Unilever VN - Củ Chi, KCN Tây Bắc Củ Chi, TP HCM", CBM = 2000, GW = 256789, ContFCL = "01X20'DC & 14X40'DC", ContLCL = "01X20'DC & 14X40'DC", CurrentDeadline = DateTime.Now.AddHours(3), CurrentStageStatus = StatusEnum.JobStatus.NotStart, Service = "Air Import" },
            new Job { UserId = user.UserId, Id = "AE1303/0027", MBL = "VPC/TAHAP18084319", PO_NO = "Q3-17-GH", CustomerName = "DONG HUNG INDUSTRIAL JOIN STOCK COMPANY", PlaceFrom = "AL Arish", PlaceTo = "Albani", EstimateDate = 0, AssignTime = DateTime.Now.AddHours(-1), NumberStage = 0, NumberStageFinish = 0, Warehouse = "Unilever VN - Củ Chi, KCN Tây Bắc Củ Chi, TP HCM", CBM = 2000, GW = 256789, ContFCL = "01X20'DC & 14X40'DC", ContLCL = "01X20'DC & 14X40'DC", CurrentDeadline = DateTime.Now.AddHours(5), CurrentStageStatus = StatusEnum.JobStatus.NotStart, Service = "Air Export" },
            new Job { UserId = user.UserId, Id = "AE1303/0028", MBL = "VPC/TAHAP18084319", PO_NO = "Q3-17-GH", CustomerName = "DONG HUNG INDUSTRIAL JOIN STOCK COMPANY", PlaceFrom = "AL Arish", PlaceTo = "Albani", EstimateDate = 0, AssignTime = DateTime.Now.AddHours(1), NumberStage = 0, NumberStageFinish = 0, Warehouse = "Unilever VN - Củ Chi, KCN Tây Bắc Củ Chi, TP HCM", CBM = 2000, GW = 256789, ContFCL = "01X20'DC & 14X40'DC", ContLCL = "01X20'DC & 14X40'DC", CurrentDeadline = DateTime.Now.AddHours(5), CurrentStageStatus = StatusEnum.JobStatus.NotStart, Service = "Sea Import" },
            new Job { UserId = user.UserId, Id = "AE1303/0029", MBL = "VPC/TAHAP18084319", PO_NO = "Q3-17-GH", CustomerName = "DONG HUNG INDUSTRIAL JOIN STOCK COMPANY", PlaceFrom = "AL Arish", PlaceTo = "Albani", EstimateDate = 0, AssignTime = DateTime.Now.AddDays(-1), NumberStage = 0, NumberStageFinish = 0, Warehouse = "Unilever VN - Củ Chi, KCN Tây Bắc Củ Chi, TP HCM", CBM = 2000, GW = 256789, ContFCL = "01X20'DC & 14X40'DC", ContLCL = "01X20'DC & 14X40'DC", CurrentDeadline = DateTime.Now.AddHours(4), CurrentStageStatus = StatusEnum.JobStatus.Processing, Service = "Air Export" },
            new Job { UserId = user.UserId, Id = "AE1303/0030", MBL = "VPC/TAHAP18084319", PO_NO = "Q3-17-GH", CustomerName = "DONG HUNG INDUSTRIAL JOIN STOCK COMPANY", PlaceFrom = "AL Arish", PlaceTo = "Albani", EstimateDate = 0, AssignTime = DateTime.Now.AddDays(-1), NumberStage = 0, NumberStageFinish = 0, Warehouse = "Unilever VN - Củ Chi, KCN Tây Bắc Củ Chi, TP HCM", CBM = 2000, GW = 256789, ContFCL = "01X20'DC & 14X40'DC", ContLCL = "01X20'DC & 14X40'DC", CurrentDeadline = DateTime.Now.AddHours(1), CurrentStageStatus = StatusEnum.JobStatus.Finish, Service = "Air Import" }
        };
        public static List<Stage> stages = new List<Stage>
        {
            new Stage { JobId = "AE1303/0022", Order = 1, Id = "Stage1-AE1303/0022", LastComment = null, Duration = 6, EndDate = DateTime.Now.AddDays(3), Name = "Stage 1 Name", Status = StatusEnum.StageStatus.Done, Role = "CS" },
            new Stage { JobId = "AE1303/0022", Order = 2, Id = "Stage2-AE1303/0022", LastComment = null, Duration = 6, EndDate = DateTime.Now.AddHours(5), Name = "Stage 2 Name", Status = StatusEnum.StageStatus.Processing, Role = "CS"},
            new Stage { JobId = "AE1303/0022", Order = 3, Id = "Stage3-AE1303/0022", LastComment = null, Duration = 6, EndDate = DateTime.Now.AddHours(5), Name = "Stage 3 Name", Status = StatusEnum.StageStatus.InSchedule, Role = "OPS" },
            new Stage { JobId = "AE1303/0022", Order = 4, Id = "Stage4-AE1303/0022", LastComment = null, Duration = 6, EndDate = DateTime.Now.AddHours(5), Name = "Stage 4 Name", Status = StatusEnum.StageStatus.InSchedule, Role = "CS" },
            new Stage { JobId = "AE1303/0022", Order = 5, Id = "Stage5-AE1303/0022", LastComment = null, Duration = 6, EndDate = DateTime.Now.AddHours(5), Name = "Stage 5 Name", Status = StatusEnum.StageStatus.InSchedule, Role = "CS"},
            new Stage { JobId = "AE1303/0022", Order = 6, Id = "Stage6-AE1303/0022", LastComment = null, Duration = 6, EndDate = DateTime.Now.AddHours(5), Name = "Stage 6 Name", Status = StatusEnum.StageStatus.InSchedule, Role = "OPS"},

            new Stage { JobId = "AE1303/0023", Order = 1, Id = "Stage1-AE1303/0023", LastComment = null, Duration = 6, EndDate = DateTime.Now.AddHours(5), Name = "Stage 1 Name", Status = StatusEnum.StageStatus.InSchedule, Role = "CS"},
            new Stage { JobId = "AE1303/0023", Order = 2, Id = "Stage2-AE1303/0023", LastComment = null, Duration = 6, EndDate = DateTime.Now.AddHours(5), Name = "Stage 2 Name", Status = StatusEnum.StageStatus.Overdued, Role = "OPS"},

            new Stage { JobId = "AE1303/0024", Order = 1, Id = "Stage1-AE1303/0024", LastComment = null, Duration = 6, EndDate = DateTime.Now, Name = "Stage 1 Name", Status = StatusEnum.StageStatus.WillOverdue, Role = "CS"},
            new Stage { JobId = "AE1303/0024", Order = 2, Id = "Stage2-AE1303/0024", LastComment = null, Duration = 6, EndDate = DateTime.Now.AddHours(2), Name = "Stage 2 Name", Status = StatusEnum.StageStatus.InSchedule, Role = "OPS"},

            new Stage { JobId = "AE1303/0025", Order = 1, Id = "Stage1-AE1303/0025", LastComment = null, Duration = 6, EndDate = DateTime.Now.AddHours(2), Name = "Stage 1 Name", Status = StatusEnum.StageStatus.WillOverdue, Role = "OPS"},
            new Stage { JobId = "AE1303/0025", Order = 2, Id = "Stage2-AE1303/0025", LastComment = null, Duration = 6, EndDate = DateTime.Now.AddHours(2), Name = "Stage 2 Name", Status = StatusEnum.StageStatus.InSchedule, Role = "OPS"},

            new Stage { JobId = "AE1303/0026", Order = 1, Id = "Stage1-AE1303/0026", LastComment = null, Duration = 6, EndDate = DateTime.Now.AddHours(5), Name = "Stage 1 Name", Status = StatusEnum.StageStatus.InSchedule, Role = "OPS"},
            new Stage { JobId = "AE1303/0027", Order = 2, Id = "Stage1-AE1303/0027", LastComment = null, Duration = 6, EndDate = DateTime.Now.AddHours(5), Name = "Stage 1 Name", Status = StatusEnum.StageStatus.InSchedule, Role = "OPS"},
            new Stage { JobId = "AE1303/0028", Order = 1, Id = "Stage1-AE1303/0028", LastComment = null, Duration = 6, EndDate = DateTime.Now.AddHours(5), Name = "Stage 1 Name", Status = StatusEnum.StageStatus.InSchedule, Role = "OPS"},
            new Stage { JobId = "AE1303/0029", Order = 1, Id = "Stage1-AE1303/0029", LastComment = null, Duration = 6, EndDate = DateTime.Now.AddHours(5), Name = "Stage 1 Name", Status = StatusEnum.StageStatus.Processing, Role = "OPS"},
            new Stage { JobId = "AE1303/0030", Order = 1, Id = "Stage1-AE1303/0030", LastComment = null, Duration = 6, EndDate = DateTime.Now.AddHours(1), Name = "Stage 1 Name", Status = StatusEnum.StageStatus.Done, Role = "OPS"}
        };
        public static List<Comment> comments = new List<Comment>
        {
            new Comment { Id = Guid.NewGuid(), StageId = "Stage1-AE1303/0022", CommentType = StatusEnum.CommentType.Progressing, Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit", CreatedDate = DateTime.Now },
            new Comment { Id = Guid.NewGuid(), StageId = "Stage1-AE1303/0022", CommentType = StatusEnum.CommentType.Pending, Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit", CreatedDate = DateTime.Now }
        };

    }
}
