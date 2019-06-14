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
    public interface ICustomsDeclarationService : IRepositoryBase<CustomsDeclaration, CustomsDeclarationModel>
    {
        HandleState ImportClearancesFromEcus();
        List<CustomsDeclarationModel> Paging(CustomsDeclarationCriteria criteria, int page, int size, out int rowsCount);
    }
}
