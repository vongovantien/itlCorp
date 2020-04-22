﻿using AutoMapper;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Service.Models;

namespace eFMS.API.Setting.Infrastructure
{
    public class MappingProfile : Profile
    {
        public  MappingProfile()
        {
            // Add as many of these lines as you need to map your objects
            //CreateMap<SysUserGroup, SysUserGroupModel>();
            //CreateMap<SysUserGroupModel, SysUserGroup>();
            //CreateMap<SysUserGroupEditModel, SysUserGroupModel>();
            CreateMap<SetTariff, SetTariffModel>();
            CreateMap<SetTariffDetail, SetTariffDetailModel>();
            CreateMap<SetTariff, TariffViewModel>();
            CreateMap<SetTariffModel, SetTariff>();
            CreateMap<SetTariffDetailModel, SetTariffDetail>();
            CreateMap<SetTariffModel, TariffViewModel>();

        }
    }
}
