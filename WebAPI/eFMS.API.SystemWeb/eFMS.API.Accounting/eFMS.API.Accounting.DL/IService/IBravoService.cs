using eFMS.API.Accounting.DL.Models.Accounting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IBravoService 
    {
        BravoResponseModel SyncAdvanceAdd(List<BravoAdvanceModel> model);

    }
}
