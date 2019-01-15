using AutoMapper;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
   public class HouseBillSeaFCLExportService : RepositoryBase<TestHouseBillSeaFclexport, HouseBillSeaFCLExportModel>, IHouseBillSeaFCLExportService
    {
        public HouseBillSeaFCLExportService(IContextBase<TestHouseBillSeaFclexport> repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }

}
