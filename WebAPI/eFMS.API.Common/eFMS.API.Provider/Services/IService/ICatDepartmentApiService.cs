using eFMS.API.Provider.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eFMS.API.Provider.Services.IService
{
    public interface ICatDepartmentApiService
    {
        Task<List<CatDepartmentApiModel>> GetAll();
    }
}
