using AutoMapper;
using eFMS.IdentityServer.DL.Models;
using eFMS.IdentityServer.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.IdentityServer.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SysUserLogModel, SysUserLog>();
        }
    }
}
