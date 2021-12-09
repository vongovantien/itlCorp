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
    public interface IRuleLinkFeeService : IRepositoryBase<CsRuleLinkFee, CsRuleLinkFeeModel>
    {
        HandleState AddNewRuleLinkFee(CsRuleLinkFeeModel model);
        HandleState UpdateRuleLinkFee(CsRuleLinkFeeModel model);
        List<CsRuleLinkFeeModel> Paging(CsRuleLinkFeeCriteria criteria, int page, int size, out int rowsCount);
        CsRuleLinkFeeModel GetRuleLinkFeeById(Guid idRuleLinkFee);
        HandleState DeleteRuleLinkFee(Guid? id);
    }
}
