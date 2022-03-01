using AutoMapper;
using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Report.Infrastructure
{
    public class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<SysReportLogModel, SysReportLog>().ReverseMap();
        }
    }
}
