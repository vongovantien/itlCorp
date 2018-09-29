
using ITL.NetCore.Connection.BL;
using System.Collections.Generic;
using System.Linq;
using SystemManagement.DL.Models;
using SystemManagement.DL.Models.Views;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public interface ICatBranchService: IRepositoryBase<CatBranch, CatBranchModel>
    {
        List<vw_catBranch> GetByView();
        //vw_catBranch save(vw_catBranch dr);
    }
}