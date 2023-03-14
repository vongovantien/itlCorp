using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Setting.DL.Models
{
    public class FileManageResponse
    {
        public IQueryable<EDocFile> Data { get; set; }
        public int TotalItem { get; set; }
    }
}
