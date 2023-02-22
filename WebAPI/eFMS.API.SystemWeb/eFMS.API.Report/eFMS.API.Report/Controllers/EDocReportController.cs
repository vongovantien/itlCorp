using eFMS.API.Report.DL.Common;
using eFMS.API.Report.DL.IService;
using eFMS.API.Report.DL.Models;
using eFMS.API.Report.DL.Services;
using eFMS.API.Report.Helpers;
using eFMS.API.Report.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace eFMS.API.Report.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class EDocReportController : Controller
    {
        private IEDocReportService edocReportService;
        public EDocReportController(IEDocReportService edocReport)
        {
            edocReportService = edocReport;
        }

        [HttpPost("QueryDataEDocReport")]
        public IActionResult QueryDataEDocReport(GeneralReportCriteria criteria)
        {
            var data = edocReportService.QueryDataEDocsReport(criteria);
            return Ok(data);
        }

        [Route("ExportEDocTemplateReport")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportEDocTemplateReport(GeneralReportCriteria criteria)
        {
            #region -- Ghi Log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = ReportConstants.Standard_Report,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = ReportConstants.Export_Excel
            };
            #endregion -- Ghi Log Report --

            var data = edocReportService.QueryDataEDocsReport(criteria);
            if (data == null)
            {
                return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }
            var stream = new ReportHelper().BindingDataEDocReport(data, criteria);
            if (stream == null)
            {
                return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }
            FileContentResult fileContent = new Helpers.FileHelper().ExportExcel(null, stream, "EdocReportTemplate");
            HeaderResponse(fileContent.FileDownloadName);
            return fileContent;
        }
        private void HeaderResponse(string fileName)
        {
            Response.Headers.Add("efms-file-name", fileName);
            Response.Headers.Add("Access-Control-Expose-Headers", "efms-file-name");
        }
    }
}
