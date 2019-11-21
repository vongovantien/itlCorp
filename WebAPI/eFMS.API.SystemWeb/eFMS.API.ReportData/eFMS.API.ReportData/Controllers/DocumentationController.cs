using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using eFMS.API.ReportData.FormatExcel;
using eFMS.API.ReportData.Helpers;
using eFMS.API.ReportData.HttpServices;
using eFMS.API.ReportData.Models;
using eFMS.API.ReportData.Models.Documentation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.ReportData.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    [ApiController]
    public class DocumentationController : Controller
    {
        private readonly APIs aPis;

        /// <summary>
        /// Contructor controller Accounting Report
        /// </summary>
        /// <param name="appSettings"></param>
        public DocumentationController(IOptions<APIs> appSettings)
        {
            this.aPis = appSettings.Value;
        }

        [Route("ExportEManifest")]
        [HttpGet]
        public async Task<IActionResult> ExportEManifest(Guid hblid)
        {
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.HostStaging + Urls.Documentation.HouseBillDetailUrl + hblid);

            var dataObject = responseFromApi.Content.ReadAsAsync<CsTransactionDetailModel>();

            var stream = new DocumentationHelper().CreateEManifestExcelFile(dataObject.Result);
            if (stream == null)
            {
                return null;
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "E-Manifest.xlsx");

            return fileContent;
        }
    }
}
