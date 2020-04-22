using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Linq;

namespace eFMS.API.System.DL.IService
{
    public interface ICatDepartmentService : IRepositoryBase<CatDepartment, CatDepartmentModel>
    {
        IQueryable<CatDepartmentModel> QueryData(CatDepartmentCriteria criteria);

        IQueryable<CatDepartmentModel> Paging(CatDepartmentCriteria criteria, int page, int size, out int rowsCount);

        CatDepartmentModel GetDepartmentById(int id);

        HandleState Insert(CatDepartmentModel model);

        HandleState Update(CatDepartmentModel model);

        HandleState Delete(int id);

        IQueryable<CatDepartmentModel> GetDepartmentsByOfficeId(Guid id);

        bool CheckExistsDeptAccountantInOffice(CatDepartmentModel model);

        object GetDepartmentTypes();

    }

}
