using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsManifestService : IRepositoryBase<CsManifest, CsManifestModel>
    {
        HandleState AddOrUpdateManifest(CsManifestEditModel model);
        CsManifestModel GetById(Guid jobId);
        Crystal PreviewSeaExportManifest(ManifestReportModel model);
        Crystal PreviewSeaImportManifest(ManifestReportModel model);
        Crystal PreviewAirExportManifest(ManifestReportModel model);
        Crystal PreviewAirExportManifestByJobId(Guid jobId);
        Crystal PreviewSeaExportManifestByJobId(Guid jobId);
        bool CheckExistManifestExport(Guid jobId);
    }
}
