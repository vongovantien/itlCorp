using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Services
{
    public class CatDepartmentService : RepositoryBase<CatDepartment,CatDepartmentModel>,ICatDepartmentService
    {
        public CatDepartmentService(IContextBase<CatDepartment> repository, IMapper mapper) : base(repository, mapper)
        {

        }
    }
}
