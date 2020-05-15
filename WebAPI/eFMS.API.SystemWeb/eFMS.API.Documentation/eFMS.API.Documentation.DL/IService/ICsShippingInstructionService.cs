using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsShippingInstructionService : IRepositoryBase<CsShippingInstruction, CsShippingInstructionModel>
    {
        CsShippingInstructionModel GetById(Guid jobId);
        HandleState AddOrUpdate(CsShippingInstructionModel model);
        Crystal PreviewFCLShippingInstruction(CsShippingInstructionReportModel model);
        Crystal PreviewOCL(CsShippingInstructionReportModel model);
        Crystal PreviewFCLShippingInstructionByJobId(Guid jobId);
        Crystal PreviewSISummary(CsShippingInstructionReportModel model);
    }
}
