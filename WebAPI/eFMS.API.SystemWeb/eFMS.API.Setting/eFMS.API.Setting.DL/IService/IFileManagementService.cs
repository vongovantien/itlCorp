using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.IService
{
    public interface IFileManagementService: IRepositoryBase<SysImage, SysImageModel>
    {
        List<SysImageViewModel> Search(SysImageCriteria criteria, int page, int size, out int rowsCount);
        List<SysImageViewModel> Get(string folderName, string keyWord, int page, int size, out int rowsCount);
        List<SysImageViewModel> GetDetail(string folderName, string objectId);
    }
}
