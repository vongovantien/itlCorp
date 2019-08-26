using eFMS.API.Operation.DL.Models;
using eFMS.API.Operation.DL.Models.Criteria;
using eFMS.API.Operation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Operation.DL.IService
{
    public interface ICustomsDeclarationService : IRepositoryBase<CustomsDeclaration, CustomsDeclarationModel>
    {
        IQueryable<CustomsDeclarationModel> GetAll();
        object GetClearanceTypeData();
        HandleState ImportClearancesFromEcus();
        List<CustomsDeclarationModel> Paging(CustomsDeclarationCriteria criteria, int page, int size, out int rowsCount);
        IQueryable<CustomsDeclarationModel> Query(CustomsDeclarationCriteria criteria);
        List<CustomsDeclarationModel> GetBy(string jobNo);
        HandleState UpdateJobToClearances(List<CustomsDeclarationModel> clearances);
        CustomsDeclaration GetById(int id);
        HandleState DeleteMultiple(List<CustomsDeclarationModel> customs);
        List<CustomClearanceImportModel> CheckValidImport(List<CustomClearanceImportModel> list);
        HandleState Import(List<CustomsDeclarationModel> data);
    }
}
