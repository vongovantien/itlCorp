using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Catalogue.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatBankController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatBankService catBankService;
        private readonly ICurrentUser currentUser;
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        ///
        /// </summary>
        /// <param name="localizer">inject interface IStringLocalizer</param>
        /// <param name="service">inject interface ICatBankService</param>
        /// <param name="currUser">inject interface ICurrentUser</param>
        /// <param name="hostingEnvironment">inject interface IHostingEnvironment</param>
        public CatBankController(IStringLocalizer<LanguageSub> localizer, ICatBankService service,
            ICurrentUser currUser, IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            catBankService = service;
            currentUser = currUser;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// get the list of all banks
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getAll")]
        public IActionResult Get()
        {
            var data = catBankService.GetAll()?.OrderBy(x => x.BankNameVn);
            return Ok(data);
        }

        /// <summary>
        /// get commodity by id
        /// </summary>
        /// <param name="id">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDetailById/{id}")]
        public IActionResult Get(Guid id)
        {
            var data = catBankService.GetDetail(id);
            return Ok(data);
        }

        /// <summary>
        /// get and paging the list of banks by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("paging")]
        public IActionResult Get(CatBankCriteria criteria, int page, int size)
        {
            var data = catBankService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// get the list of banks
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <returns></returns>
        [HttpPost]
        [Route("getAllByQuery")]
        public IActionResult Get(CatBankCriteria criteria)
        {
            var data = catBankService.Query(criteria);
            return Ok(data);
        }

        /// <summary>
        /// add new item
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("add")]
        //[Authorize]
        public IActionResult Post(CatBankModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (model.PartnerId == null)
            {
                var checkExistMessage = CheckExist(string.Empty, model);
                if (checkExistMessage.Length > 0)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
                }
            }
            var hs = catBankService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// update an existed item
        /// </summary>
        /// <param name="model">model to update</param>
        /// <returns></returns>
        [HttpPut]
        [Route("update")]
        //[Authorize]
        public IActionResult Put(CatBankModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            if (model.PartnerId == null)
            {
                var checkExistMessage = CheckExist(model.Id.ToString(), model);
                if (checkExistMessage.Length > 0)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
                }
            }


            var hs = catBankService.Update(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id">id of data that want to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        //[Authorize]
        public IActionResult Delete(Guid id)
        {
            var hs = catBankService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// download exel from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("downloadExcel")]
        public async Task<ActionResult> DownloadExcel()
        {
            try
            {
                string fileName = Templates.CatBank.ExcelImportFileName + Templates.ExcelImportEx;
                string templateName = _hostingEnvironment.ContentRootPath;
                var result = await new FileHelper().ExportExcel(templateName, fileName);
                if (result != null)
                {
                    return result;
                }
                else
                {
                    return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
                }
            }
            catch (Exception)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
            }
        }

        /// <summary>
        /// read charge data from file excel
        /// </summary>
        /// <param name="uploadedFile">file to read data</param>
        /// <returns></returns>
        [HttpPost]
        [Route("uploadFile")]
        public IActionResult UploadFile(IFormFile uploadedFile)
        {
            var file = new FileHelper().UploadExcel(uploadedFile);
            if (file != null)
            {
                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int rowCount = worksheet.Dimension.Rows;
                if (rowCount < 2) return BadRequest();

                List<CatBankImportModel> list = new List<CatBankImportModel>();
                for (int row = 2; row <= rowCount; row++)
                {
                    bool active = true;
                    string status = worksheet.Cells[row, 4].Value?.ToString().Trim();
                    if (status.ToLower() != "active")
                        active = false;

                    var bank = new CatBankImportModel
                    {
                        IsValid = true,
                        Code = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                        BankNameVn = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                        BankNameEn = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                        Status = status,
                        Active = active
                    };
                    list.Add(bank);
                }
                var data = catBankService.CheckValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// import list data into database
        /// </summary>
        /// <param name="data">list of data</param>
        /// <returns></returns>
        [HttpPost]
        [Route("import")]
        [Authorize]
        public IActionResult Import([FromBody] List<CatBankImportModel> data)
        {
            var hs = catBankService.Import(data);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Import successfully!!!" };
            if (hs.Success)
                return Ok(result);
            else
                return BadRequest(new ResultHandle { Status = false, Message = hs.Exception.Message });
        }


        [HttpGet]
        [Route("GetBankByPartnerId/{id}")]
        [Authorize]
        public async Task<IActionResult> GetBankByPartnerId(Guid id)
        {
            var data = await catBankService.GetBankByPartnerId(id);
            return Ok(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("BankInfoSyncUpdateStatus")]
        [Authorize]
        public async Task<IActionResult> UpdateBankInfoSyncStatus(BankStatusUpdateModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = await catBankService.UpdateBankInfoSyncStatus(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model.BankInfo };
            return Ok(result);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("SyncVoucherToAccountantSystem")]
        [Authorize]
        //public async Task<IActionResult> SyncBankAccountToAccountantSystem(List<RequestGuidListModel> request)
        //{
        //    var _startDateProgress = DateTime.Now;
        //    if (!ModelState.IsValid) return BadRequest();

        //    try
        //    {
        //        // 1. Login
        //        HttpResponseMessage responseFromApi = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api/Login", loginInfo, null);
        //        BravoLoginResponseModel loginResponse = responseFromApi.Content.ReadAsAsync<BravoLoginResponseModel>().Result;

        //        if (loginResponse.Success == "1")
        //        {
        //            // 2. Get Data To Sync.
        //            List<Guid> Ids = request.Select(x => x.Id).ToList();

        //            List<Guid> IdsAdd = request.Where(action => action.Action == ACTION.ADD).Select(x => x.Id).ToList();
        //            List<Guid> IdsUpdate = request.Where(action => action.Action == ACTION.UPDATE).Select(x => x.Id).ToList();

        //            List<BravoVoucherModel> listAdd = (IdsAdd.Count > 0) ? accountingService.GetListVoucherToSyncBravo(IdsAdd) : new List<BravoVoucherModel>();
        //            List<BravoVoucherModel> listUpdate = (IdsUpdate.Count > 0) ? accountingService.GetListVoucherToSyncBravo(IdsUpdate) : new List<BravoVoucherModel>();

        //            HttpResponseMessage resAdd = new HttpResponseMessage();
        //            HttpResponseMessage resUpdate = new HttpResponseMessage();
        //            BravoResponseModel responseAddModel = new BravoResponseModel();
        //            BravoResponseModel responseUpdateModel = new BravoResponseModel();

        //            // 3. Call Bravo to SYNC.
        //            if (listAdd.Count > 0)
        //            {
        //                resAdd = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Catalogue/api?func=EFMSBankInfoSyncAdd ", listAdd, loginResponse.TokenKey);
        //                responseAddModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();
        //            }

        //            if (listUpdate.Count > 0)
        //            {
        //                resUpdate = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Catalogue/api?func=EFMSBankInfoSyncUpdate ", listUpdate, loginResponse.TokenKey);
        //                responseUpdateModel = await resUpdate.Content.ReadAsAsync<BravoResponseModel>();

        //                #region -- Ghi Log --
        //                var modelLog = new SysActionFuncLogModel
        //                {
        //                    FuncLocal = "SyncVoucherToAccountantSystem",
        //                    FuncPartner = "EFMSVoucherDataSyncUpdate",
        //                    ObjectRequest = JsonConvert.SerializeObject(listUpdate),
        //                    ObjectResponse = JsonConvert.SerializeObject(responseUpdateModel),
        //                    Major = "Nghiệp Vụ Chi Phí",
        //                    StartDateProgress = _startDateProgress,
        //                    EndDateProgress = DateTime.Now
        //                };
        //                var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
        //                #endregion
        //            }

        //            // 4. Update STATUS
        //            if (responseAddModel.Success == "1" || responseUpdateModel.Success == "1")
        //            {
        //                HandleState hs = accountingService.SyncListVoucherToBravo(Ids, out Ids);
        //                string message = HandleError.GetMessage(hs, Crud.Update);
        //                ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = Ids };
        //                if (!hs.Success)
        //                {
        //                    return BadRequest(result);
        //                }
        //                return Ok(result);
        //            }
        //            return BadRequest(new ResultHandle { Message = responseAddModel.Msg + "\n" + responseUpdateModel.Msg });
        //        }
        //        new LogHelper("eFMS_SYNC_LOG", loginResponse.ToString());
        //        return BadRequest(new ResultHandle { Message = "Sync fail" });
        //    }
        //    catch (Exception ex)
        //    {
        //        new LogHelper("eFMS_SYNC_LOG", ex.ToString());
        //        return BadRequest(new ResultHandle { Message = "Sync fail" });
        //    }
        //}

        private string CheckExist(string id, CatBankModel model)
        {
            string message = string.Empty;
            if (id == string.Empty)
            {
                if (catBankService.Any(x => x.Code.ToString().ToLower() == model.Code.ToString().ToLower()))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            else
            {
                if (catBankService.Any(x => x.BankNameVn.ToLower() == model.BankNameVn.ToLower() && x.Id.ToString().ToLower() != id.ToLower()))
                {
                    message = stringLocalizer[LanguageSub.MSG_NAME_EXISTED].Value;
                }
            }
            return message;
        }

    }
}