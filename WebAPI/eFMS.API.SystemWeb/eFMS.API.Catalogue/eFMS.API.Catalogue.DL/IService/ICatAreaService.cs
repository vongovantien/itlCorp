using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Connection.BL;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatAreaService : IRepositoryBaseCache<CatArea, CatAreaModel>
    {
        List<CatAreaViewModel> GetByLanguage();
    }
}
