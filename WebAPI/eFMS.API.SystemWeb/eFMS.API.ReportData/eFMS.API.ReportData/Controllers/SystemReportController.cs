using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.ReportData.Helpers;
using eFMS.API.ReportData.HttpServices;
using eFMS.API.ReportData.Models;
using eFMS.API.ReportData.Models.Criteria;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace eFMS.API.ReportData.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    [ApiController]
    public class SystemReportController : ControllerBase
    {
        private readonly APIs aPis;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="appSettings"></param>
        /// <param name="fileHelper"></param>
        public SystemReportController(IOptions<APIs> appSettings)
        {
            this.aPis = appSettings.Value;

        }

        [Route("System/ExportCompany")]
        [HttpPost]
        public async Task<IActionResult> ExportCompany(SysCompanyCriteria sysCompanyCriteria)
        {
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(sysCompanyCriteria, aPis.HostStaging + Urls.System.CompanyUrl);

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<SysCompany>>();

            var stream = new Helper().generateCompanyExcel(dataObjects.Result);
            if (stream == null)
            {
                return null;
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Company List.xlsx");

            return fileContent;

        }

        /// <summary>
        /// Export Office
        /// </summary>
        /// <param name="sysOfficeCriteria"></param>
        /// <returns></returns>

        [Route("System/ExportOffice")]
        [HttpPost]
        public async Task<IActionResult> ExportOffice(SysOfficeCriteria sysOfficeCriteria)
        {
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(sysOfficeCriteria, aPis.HostStaging + Urls.System.OfficeUrl);

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<SysOfficeModel>>();

            var stream = new Helper().generateOfficeExcel(dataObjects.Result);
            if (stream == null)
            {
                return null;
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Company List.xlsx");

            return fileContent;

        }
    }
}