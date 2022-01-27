using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
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

        [Route("ExportCompany")]
        [HttpPost]
        public async Task<IActionResult> ExportCompany(SysCompanyCriteria sysCompanyCriteria)
        {
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(sysCompanyCriteria, aPis.HostStaging + Urls.System.CompanyUrl);

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<SysCompany>>();

            var stream = new Helper().GenerateCompanyExcel(dataObjects.Result);
            if (stream == null)
            {
                return null;
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(null,stream, "Company List");

            return fileContent;

        }

        /// <summary>
        /// Export Office
        /// </summary>
        /// <param name="sysOfficeCriteria"></param>
        /// <returns></returns>

        [Route("ExportOffice")]
        [HttpPost]
        public async Task<IActionResult> ExportOffice(SysOfficeCriteria sysOfficeCriteria)
        {
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(sysOfficeCriteria, aPis.HostStaging + Urls.System.OfficeUrl);

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<SysOfficeModel>>();

            var stream = new Helper().GenerateOfficeExcel(dataObjects.Result);
            if (stream == null)
            {
                return null;
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(null,stream, "Company List");

            return fileContent;

        }

        /// <summary>
        /// Export Department
        /// </summary>
        /// <param name="catDepartmentCriteria"></param>
        /// <returns></returns>
        [Route("ExportDepartment")]
        [HttpPost]
        public async Task<IActionResult> ExportDepartment(CatDepartmentCriteria catDepartmentCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catDepartmentCriteria, aPis.HostStaging + Urls.System.DepartmentUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatDepartmentModel>>();  //Make sure to add a reference to System.Net.Http.Formatting.dll
            var stream = helper.CreateDepartmentExcelFile(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.DepartmentName);
        }

        /// <summary>
        /// export list group to excel file
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [Route("ExportGroup")]
        [HttpPost]
        public async Task<IActionResult> ExportGroup(SysGroupCriteria criteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(criteria, aPis.HostStaging + Urls.System.GroupUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<SysGroupModel>>();  //Make sure to add a reference to System.Net.Http.Formatting.dll
            var stream = helper.CreateGroupExcelFile(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.GroupName);
        }

        /// <summary>
        /// Export User
        /// </summary>
        /// <param name="sysUserCriteria"></param>
        /// <returns></returns>

        [Route("ExportUser")]
        [HttpPost]
        public async Task<IActionResult> ExportUser(SysUserCriteria sysUserCriteria)
        {
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(sysUserCriteria, aPis.HostStaging + Urls.System.UserUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<SysUserModel>>();
            var stream = new Helper().GenerateUserExcel(dataObjects.Result);
            if (stream == null)
            {
                return null;
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(null,stream,FilesNames.UserName);
            return fileContent;

        }
    }
}