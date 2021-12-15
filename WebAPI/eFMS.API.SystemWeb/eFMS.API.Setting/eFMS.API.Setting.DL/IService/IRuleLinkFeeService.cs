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
    public interface IRuleLinkFeeService : IRepositoryBase<CsRuleLinkFee, RuleLinkFeeModel>
    {
        HandleState AddNewRuleLinkFee(RuleLinkFeeModel model);
        HandleState UpdateRuleLinkFee(RuleLinkFeeModel model);
        List<RuleLinkFeeModel> Paging(RuleLinkFeeCriteria criteria, int page, int size, out int rowsCount);
        RuleLinkFeeModel GetRuleLinkFeeById(Guid idRuleLinkFee);
        HandleState DeleteRuleLinkFee(Guid? id);
        HandleState CheckExistsDataRule(RuleLinkFeeModel model);
    }
}
