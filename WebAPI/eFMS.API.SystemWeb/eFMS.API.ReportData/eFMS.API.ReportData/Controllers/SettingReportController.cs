using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using eFMS.API.ReportData.FormatExcel;
using eFMS.API.ReportData.Helpers;
using eFMS.API.ReportData.HttpServices;
using eFMS.API.ReportData.Models;
using eFMS.API.ReportData.Models.Criteria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace eFMS.API.ReportData.Controllers
{
    /// <summary>
    /// Setting Report Controller
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    [ApiController]
    public class SettingReportController : ControllerBase
    {
        private readonly APIs aPis;

        /// <summary>
        /// Contructor controller Setting Report
        /// </summary>
        /// <param name="appSettings"></param>
        public SettingReportController(IOptions<APIs> appSettings)
        {
            this.aPis = appSettings.Value;
        }

        /// <summary>
        /// Export List Unlock Request
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [Route("ExportUnlockRequest")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportUnlockRequest(UnlockRequestCriteria criteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.PostAPI(criteria, aPis.SettingAPI + Urls.Setting.GetDataUnlockRequestExportUrl ,accessToken);

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<UnlockRequestExport>>();
            if (dataObjects.Result == null)
            {
                return new FileHelper().ExportExcel(null,new MemoryStream(), "");
            }

            var stream = new SettingHelper().GenerateUnlockRequestExcel(dataObjects.Result, null);
            if (stream == null)
            {
                return new FileHelper().ExportExcel(null,new MemoryStream(), "");
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(null,stream, "Unlock Request");

            return fileContent;
        }

    }
}