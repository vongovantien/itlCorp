using eFMS.API.Log.DL.Common;
using eFMS.API.Log.DL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Log.DL.IService
{
    public interface ICategoryLogService
    {
        List<LogModel> Paging(CategoryTable tableType, string query, int page, int size, out long rowsCount);
    }
}
