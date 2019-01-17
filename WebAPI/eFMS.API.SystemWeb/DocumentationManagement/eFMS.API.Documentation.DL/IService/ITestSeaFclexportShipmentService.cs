using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ITestSeaFclexportShipmentService: IRepositoryBase<TestSeaFclexportShipment, TestSeaFclexportShipmentModel>
    {
        IQueryable<TestSeaFclexportShipmentModel> Query(TestSeaFclexportShipmentCriteria criteria);
        List<TestSeaFclexportShipmentModel> Paging(TestSeaFclexportShipmentCriteria criteria, int page, int size, out int rowsCount);
    }
}
