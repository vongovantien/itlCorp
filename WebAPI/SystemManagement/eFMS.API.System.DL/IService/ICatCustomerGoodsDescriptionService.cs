﻿using SystemManagement.DL.Models;
using ITL.NetCore.Connection.BL;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public interface ICatCustomerGoodsDescriptionService: IRepositoryBase<CatCustomerGoodsDescription, CatCustomerGoodsDescriptionModel>
    {
    }
}