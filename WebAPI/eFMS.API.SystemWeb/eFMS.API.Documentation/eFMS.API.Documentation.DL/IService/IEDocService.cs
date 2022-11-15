using ITL.NetCore.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IEDocService
    {
        Task<HandleState> DeleteEdocByHBLId(Guid HBLId);
        Task<HandleState> DeleteEdocByJobId(Guid JobId);
    }
}
