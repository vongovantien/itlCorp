using eFMS.API.Common.Models;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace eFMS.API.Setting.DL.IService
{
    public interface IFileManagementService: IRepositoryBase<SysImage, SysImageModel>
    {
        IQueryable<SysImageViewModel> Get(FileManagementCriteria criteria, int page, int size, out int rowsCount);
        List<SysImageViewModel> GetDetail(string folderName, string objectId);
        Task<IQueryable<EDocFile>> GetEdocManagement(EDocManagementCriterial criterial);
    }
}
