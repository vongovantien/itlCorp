using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using SystemManagementAPI.Infrastructure.Middlewares;

namespace eFMS.API.Documentation.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsShipmentSurchargeController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsShipmentSurchargeService csShipmentSurchargeService;
        private readonly ICurrentUser currentUser;
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="user"></param>
        /// <param name="hostingEnvironment"></param>
        public CsShipmentSurchargeController(IStringLocalizer<LanguageSub> localizer, ICsShipmentSurchargeService service, ICurrentUser user, IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            csShipmentSurchargeService = service;
            currentUser = user;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// get list of surcharge by house bill and type
        /// </summary>
        /// <param name="hbId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetByHB")]
        public IActionResult GetByHouseBill(Guid hbId, string type)
        {
            return Ok(csShipmentSurchargeService.GetByHB(hbId, type));
        }

        /// <summary>
        /// get list of surcharge by house bill anf partner id
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="partnerID"></param>
        /// <param name="IsHouseBillID"></param>
        /// <param name="cdNoteCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GroupByListHB")]
        public List<GroupChargeModel> GetByListHouseBill(Guid Id, string partnerID, bool IsHouseBillID, string cdNoteCode)
        {
            return csShipmentSurchargeService.GroupChargeByHB(Id, partnerID, IsHouseBillID, cdNoteCode);
        }

        /// <summary>
        /// get surcharge by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetBy")]
        public IActionResult GetBy(Guid id)
        {
            var result = csShipmentSurchargeService.Get(x => x.Id == id);
            return Ok(result);
        }

        /// <summary>
        /// get partners have surcharge
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="IsHouseBillID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPartners")]
        [Authorize]
        public List<CatPartner> GetPartners(Guid Id, bool IsHouseBillID)
        {
            return csShipmentSurchargeService.GetAllParner(Id, IsHouseBillID);
        }


        /// <summary>
        /// get profit of a house bill
        /// </summary>
        /// <param name="hblid"></param>
        /// <returns></returns>
        [HttpGet("GetHouseBillProfit")]
        public IActionResult GetHouseBillProfit(Guid hblid)
        {
            var result = csShipmentSurchargeService.GetHouseBillTotalProfit(hblid);
            return Ok(result);
        }


        /// <summary>
        /// get profit of a shipment
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet("GetshipmentProfit")]
        [Authorize]
        public IActionResult GetshipmentProfit(Guid jobId)
        {
            var result = csShipmentSurchargeService.GetShipmentTotalProfit(jobId);
            return Ok(result);
        }

        /// <summary>
        /// add new surcharge
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult AddNew(CsShipmentSurchargeModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.UserCreated = currentUser.UserID;
            model.Id = Guid.NewGuid();
            model.ExchangeDate = DateTime.Now;
            model.DatetimeCreated = DateTime.Now;

            #region --Tính giá trị các field: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
            var amountOriginal = csShipmentSurchargeService.CalculatorAmountAccountingByCurrency(model.CurrencyId, model.Vatrate, model.UnitPrice, model.Quantity, model.FinalExchangeRate, model.ExchangeDate, model.CurrencyId);
            model.NetAmount = amountOriginal.NetAmount; //Thành tiền trước thuế (Original)
            model.Total = amountOriginal.NetAmount + amountOriginal.VatAmount; //Thành tiền sau thuế (Original)
            model.FinalExchangeRate = model.FinalExchangeRate == null ? amountOriginal.ExchangeRate : model.FinalExchangeRate; //Tỉ giá so với Local

            var amountLocal = csShipmentSurchargeService.CalculatorAmountAccountingByCurrency(model.CurrencyId, model.Vatrate, model.UnitPrice, model.Quantity, model.FinalExchangeRate, model.ExchangeDate, DocumentConstants.CURRENCY_LOCAL);
            model.AmountVnd = amountLocal.NetAmount; //Thành tiền trước thuế (Local)
            model.VatAmountVnd = amountLocal.VatAmount; //Tiền thuế (Local)

            var amountUsd = csShipmentSurchargeService.CalculatorAmountAccountingByCurrency(model.CurrencyId, model.Vatrate, model.UnitPrice, model.Quantity, model.FinalExchangeRate, model.ExchangeDate, DocumentConstants.CURRENCY_USD);
            model.AmountUsd = amountUsd.NetAmount; //Thành tiền trước thuế (USD)
            model.VatAmountUsd = amountUsd.VatAmount; //Tiền thuế (USD)
            #endregion --Tính giá trị các field: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --

            var hs = csShipmentSurchargeService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// add list surcharge
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [HttpPost("AddAndUpdate")]
        [Authorize]
        public IActionResult Add([FromBody]List<CsShipmentSurchargeModel> list)
        {
            if (!ModelState.IsValid) return BadRequest();

            string type = list.Select(t => t.Type).FirstOrDefault();
            if(type == "BUY" || type == "OBH")
            {
                var query = list.Where(x => !isSurchargeSpecialCase(x) && !string.IsNullOrEmpty(x.InvoiceNo)).GroupBy(x => new { x.InvoiceNo, x.ChargeId })
                             .Where(g => g.Count() > 1)
                             .Select(y => y.Key);

                if (query.Any())
                {
                    return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[DocumentationLanguageSub.MSG_SURCHARGE_ARE_DUPLICATE_INVOICE].Value });
                }
            }
            
            var hs = csShipmentSurchargeService.AddAndUpdate(list);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// update an existed surcharge
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult Update(CsShipmentSurchargeModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.UserModified = currentUser.UserID;
            model.DatetimeModified = DateTime.Now;

            #region --Tính giá trị các field: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
            var amountOriginal = csShipmentSurchargeService.CalculatorAmountAccountingByCurrency(model.CurrencyId, model.Vatrate, model.UnitPrice, model.Quantity, model.FinalExchangeRate, model.ExchangeDate, model.CurrencyId);
            model.NetAmount = amountOriginal.NetAmount; //Thành tiền trước thuế (Original)
            model.Total = amountOriginal.NetAmount + amountOriginal.VatAmount; //Thành tiền sau thuế (Original)
            model.FinalExchangeRate = model.FinalExchangeRate == null ? amountOriginal.ExchangeRate : model.FinalExchangeRate; //Tỉ giá so với Local

            var amountLocal = csShipmentSurchargeService.CalculatorAmountAccountingByCurrency(model.CurrencyId, model.Vatrate, model.UnitPrice, model.Quantity, model.FinalExchangeRate, model.ExchangeDate, DocumentConstants.CURRENCY_LOCAL);
            model.AmountVnd = amountLocal.NetAmount; //Thành tiền trước thuế (Local)
            model.VatAmountVnd = amountLocal.VatAmount; //Tiền thuế (Local)

            var amountUsd = csShipmentSurchargeService.CalculatorAmountAccountingByCurrency(model.CurrencyId, model.Vatrate, model.UnitPrice, model.Quantity, model.FinalExchangeRate, model.ExchangeDate, DocumentConstants.CURRENCY_USD);
            model.AmountUsd = amountUsd.NetAmount; //Thành tiền trước thuế (USD)
            model.VatAmountUsd = amountUsd.VatAmount; //Tiền thuế (USD)
            #endregion --Tính giá trị các field: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --

            var hs = csShipmentSurchargeService.Update(model, x => x.Id == model.Id);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);

        }

        /// <summary>
        /// delete an existed surcharge
        /// </summary>
        /// <param name="chargId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public IActionResult Delete(Guid chargId)
        {
            var hs = csShipmentSurchargeService.DeleteCharge(chargId);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Get list charge shipment by conditions
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("ListChargeShipment")]
        public ChargeShipmentResult ListChargeShipment(ChargeShipmentCriteria criteria)
        {
            var data = csShipmentSurchargeService.GetListChargeShipment(criteria);
            return data;
        }

        /// <summary>
        /// get total profit of a house bill
        /// </summary>
        /// <param name="hblid"></param>
        /// <returns></returns>
        [HttpGet("GetHouseBillTotalProfit")]
        public IActionResult GetHouseBillTotalProfit(Guid hblid)
        {
            var result = csShipmentSurchargeService.GetHouseBillTotalProfit(hblid);
            return Ok(result);
        }

        /// <summary>
        /// get total profit of a shipment
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet("GetShipmentTotalProfit")]
        [Authorize]
        public IActionResult GetShipmentTotalProfit(Guid jobId)
        {
            var result = csShipmentSurchargeService.GetShipmentTotalProfit(jobId);
            return Ok(result);
        }

        /// <summary>
        /// get list of surcharge not exists in cd note by house bill and partner id
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GroupByListHBNotExistsCDNote")]
        public List<GroupChargeModel> GroupByListHBNotExists(GroupByListHBCriteria criteria)
        {
            var dataSearch = csShipmentSurchargeService.GroupChargeByHB(criteria.Id, criteria.partnerID, criteria.IsHouseBillID, string.Empty);
            var dataTmp = dataSearch;
            //Ds idcharge Param            
            List<Guid> chargeIdParam = new List<Guid>();
            foreach (var charges in criteria.listData)
            {
                foreach (var charge in charges.listCharges)
                {
                    chargeIdParam.Add(charge.Id);
                }
            }
            
            for (int i = 0; i < dataTmp.Count; i++)
            {
                //Lấy ra ds charge có chứa idCharge
                var charge = dataTmp[i].listCharges.Where(x => chargeIdParam.Contains(x.Id)).ToList();
                for (int j = 0; j < charge.Count(); j++)
                {
                    var hs = dataSearch[i].listCharges.Remove(charge[j]);
                }
                //if (dataTmp[i].listCharges.Count() == 0)
                //{
                //    var hs = dataSearch.Remove(dataTmp[i]);
                //}
            }

            return dataSearch;
        }

        /// <summary>
        /// get recently charges
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GetRecentlyCharges")]
        [Authorize]
        public IActionResult GetRecentlyCharges(RecentlyChargeCriteria criteria)
        {
            var results = csShipmentSurchargeService.GetRecentlyCharges(criteria);
            return Ok(results);
        }

        private bool isSurchargeSpecialCase(CsShipmentSurcharge charge)
        {
            return !string.IsNullOrEmpty(charge.Soano)
            || !string.IsNullOrEmpty(charge.CreditNo)
            || !string.IsNullOrEmpty(charge.DebitNo)
            || !string.IsNullOrEmpty(charge.SettlementCode)
            || !string.IsNullOrEmpty(charge.VoucherId)
            ;
        }
        #region import 

        /// <summary>
        /// download file excel from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("DownloadExcel")]
        public async Task<ActionResult> DownloadExcel()
        {
            string fileName = Templates.SurCharge.ExcelImportFileName;
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
        /// <summary>
        /// read data from file excel
        /// </summary>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("uploadFile")]
        public IActionResult UploadFile(IFormFile uploadedFile)
        {
            var file = new FileHelper().UploadExcel(uploadedFile);
            if (file != null)
            {
                DateTime temp;
                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;
                if (rowCount < 2) return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.NOT_FOUND_DATA_EXCEL].Value });
                List<CsShipmentSurchargeImportModel> list = new List<CsShipmentSurchargeImportModel>();
                for (int row = 2; row <= rowCount; row++)
                {
                    string ExchangeDate = worksheet.Cells[row, 12].Value?.ToString().Trim();
                    DateTime? dateToPase = null;
                    if (DateTime.TryParse(ExchangeDate, out temp))
                    {
                        CultureInfo culture = new CultureInfo("es-ES");
                        dateToPase = DateTime.Parse(temp.ToString("dd/MM/yyyy"), culture);
                    }
                    else
                    {
                        CultureInfo culture = new CultureInfo("es-ES");
                        if (ExchangeDate != null)
                        {
                            dateToPase = DateTime.Parse(ExchangeDate, culture);
                        }
                    }

                    string InvoiceDate = worksheet.Cells[row, 15].Value?.ToString().Trim();
                    DateTime? dateToPaseInvoice = null;
                    if (DateTime.TryParse(InvoiceDate, out temp))
                    {
                        CultureInfo culture = new CultureInfo("es-ES");
                        dateToPaseInvoice = DateTime.Parse(temp.ToString("dd/MM/yyyy"), culture);
                    }
                    else
                    {
                        CultureInfo culture = new CultureInfo("es-ES");
                        if (ExchangeDate != null)
                        {
                            dateToPaseInvoice = DateTime.Parse(InvoiceDate, culture);
                        }
                    }
                    double? UnitPrice = worksheet.Cells[row, 8].Value != null ? (double?)worksheet.Cells[row, 8].Value : (double?)null;
                    double? Vatrate = worksheet.Cells[row, 10].Value != null ? (double?)worksheet.Cells[row, 10].Value : (double?)null;
                    double? TotalAmount = worksheet.Cells[row, 11].Value != null ? (double?)worksheet.Cells[row, 11].Value : (double?)null;
                    double? FinalExchangeRate = worksheet.Cells[row, 13].Value != null ? (double?)worksheet.Cells[row, 13].Value : (double?)null;
                    var surcharge = new CsShipmentSurchargeImportModel
                    {
                        IsValid = true,
                        Hblno = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                        Mblno = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                        ClearanceNo = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                        PartnerCode = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                        ChargeCode = worksheet.Cells[row, 5].Value?.ToString().Trim(),
                        Qty = worksheet.Cells[row, 6].Value != null ? (double?) worksheet.Cells[row, 6].Value : (double?)null,
                        Unit = worksheet.Cells[row, 7].Value?.ToString().Trim(),
                        UnitPrice = (decimal?)UnitPrice,
                        CurrencyId = worksheet.Cells[row, 9].Value?.ToString().Trim(),
                        Vatrate = (decimal?)Vatrate,
                        TotalAmount = (decimal?)TotalAmount,
                        ExchangeDate = !string.IsNullOrEmpty(ExchangeDate) ? dateToPase : (DateTime?)null,
                        FinalExchangeRate = (decimal?)FinalExchangeRate, 
                        InvoiceNo = worksheet.Cells[row, 14].Value?.ToString().Trim(),
                        InvoiceDate = !string.IsNullOrEmpty(InvoiceDate) ? dateToPase : (DateTime?)null,
                        SeriesNo = worksheet.Cells[row, 16].Value?.ToString().Trim(),
                        Type = worksheet.Cells[row, 17].Value?.ToString().Trim(),
                        Notes = worksheet.Cells[row, 18].Value?.ToString().Trim(),
                    };
                    list.Add(surcharge);
                }
                var data = csShipmentSurchargeService.CheckValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// import list partner
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("import")]
        [Authorize]
        public IActionResult Import([FromBody] List<CsShipmentSurchargeImportModel> data)
        {
            var hs = csShipmentSurchargeService.Import(data);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Import successfully !!!" };
            if (!hs.Success)
            {
                return BadRequest(new ResultHandle { Status = false, Message = hs.Message.ToString() });
            }
            return Ok(result);
        }
        #endregion

        /// <summary>
        /// Only Use for Dev
        /// </summary>
        /// <returns></returns>
        [HttpPut("UpdateFieldNetAmount_AmountUSD_VatAmountUSD")]
        public IActionResult UpdatUpdateFieldNetAmount_AmountUSD_VatAmountUSDeField()
        {
            var hs = csShipmentSurchargeService.UpdateFieldNetAmount_AmountUSD_VatAmountUSD();
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Update Success" };
            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString() };
                return BadRequest(_result);
            }
            return Ok(result);
        }
    }
}


