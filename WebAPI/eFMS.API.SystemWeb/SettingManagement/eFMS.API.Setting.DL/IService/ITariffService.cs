using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.IService
{
    public interface ITariffService :  IRepositoryBase<Tariff, TariffModel>
    {
        List<TariffViewModel> Query(TariffCriteria employee);


    }
}
