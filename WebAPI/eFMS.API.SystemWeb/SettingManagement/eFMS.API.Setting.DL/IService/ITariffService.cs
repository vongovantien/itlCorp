using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Setting.DL.IService
{
    public interface ITariffService :  IRepositoryBase<SetTariff, SetTariffModel>
    {
        HandleState CheckExistsDataTariff(TariffModel model);
        HandleState AddTariff(TariffModel model);
        HandleState UpdateTariff(TariffModel model);
        HandleState DeleteTariff(Guid tariffId);
        List<TariffViewModel> Query(TariffCriteria tariff);
        IQueryable<TariffViewModel> Paging(TariffCriteria criteria, int pageNumber, int pageSize, out int rowsCount);
        SetTariffModel GetTariffById(Guid tariffId);
        SetTariffDetailModel GetTariffDetailById(Guid tariffDetailId);
        IQueryable<SetTariffDetailModel> GetListTariffDetailByTariffId(Guid tariffId);
    }
}
