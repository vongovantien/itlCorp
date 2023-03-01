using AutoMapper;
using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Service.Models;
using eFMS.API.Report.Service.ViewModels;

namespace eFMS.API.Report.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SysReportLogModel, SysReportLog>().ReverseMap();
            CreateMap<sp_GetDataExportAccountant, CsShipmentSurcharge>().ReverseMap();
        }
    }
}
