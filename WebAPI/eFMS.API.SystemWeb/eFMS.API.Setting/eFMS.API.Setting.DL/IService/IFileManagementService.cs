using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Service.Models;
using ITL.NetCore.Connection.BL;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Setting.DL.IService
{
    public interface IFileManagementService: IRepositoryBase<SysImage, SysImageModel>
    {
        IQueryable<SysImageViewModel> Get(FileManagementCriteria criteria, int page, int size, out int rowsCount);
        List<SysImageViewModel> GetDetail(string folderName, string objectId);
    }
}
