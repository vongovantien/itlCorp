using eFMS.API.Setting.DL.Common;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.IService
{
    public interface ICategoryLogService
    {
        List<LogModel> Paging(CategoryCriteria criteria, int page, int size, out long rowsCount);
        List<CategoryCollectionModel> GetCollectionName();
    }
}
