using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models.ReportResults.Sales;
using Microsoft.AspNetCore.Mvc;
using SystemManagementAPI.Infrastructure.Middlewares;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SaleReportController : ControllerBase
    {
        public SaleReportController()
        {
        }

        [HttpGet]
        public IActionResult MonthlySalereport()
        {
            var list = new List<MonthlySaleReportResult>() {
                new MonthlySaleReportResult
                {
                    Department = "Department A",
                    ContactName = "An Mai Loan",
                    SalesManager ="SalesManager A",
                    PartnerName = "PartnerName A",
                    Description = "Logistics",
                    Area ="Area A",
                    POL = "POL A",
                    POD ="POD A",
                    Lines ="Lines A",
                    Agent ="Agent A",
                    NominationParty ="NominationParty A",
                    assigned =false,
                    TransID ="TransID A",
                    HWBNO ="HWBNO A",
                    Qty20 =2,
                    Qty40 =2,
                    Cont40HC =2,
                    KGS = 3,
                    CBM=3,
                    SellingRate =3,
                    SharedProfit =3,
                    BuyingRate = 3,
                    OtherCharges =3,
                    SalesTarget =3,
                    Bonus = 3,
                    TpyeofService ="TpyeofService A",
                    Shipper = "Shipper A",
                    Consignee = "Consignee A"
                },
                new MonthlySaleReportResult
                {
                    Department = "Department B",
                    ContactName = "An Mai Loan",
                    SalesManager ="SalesManager B",
                    PartnerName = "PartnerName B",
                    Description = "Logistics",
                    Area ="Area B",
                    POL = "POL B",
                    POD ="POD B",
                    Lines ="Lines B",
                    Agent ="Agent B",
                    NominationParty ="NominationParty B",
                    assigned =false,
                    TransID ="TransID B",
                    HWBNO ="HWBNO B",
                    Qty20 =2,
                    Qty40 =2,
                    Cont40HC =2,
                    KGS = 3,
                    CBM=3,
                    SellingRate =3,
                    SharedProfit =3,
                    BuyingRate = 3,
                    OtherCharges =3,
                    SalesTarget =3,
                    Bonus = 3,
                    TpyeofService ="TpyeofService B",
                    Shipper = "Shipper B",
                    Consignee = "Consignee B"
                },
                new MonthlySaleReportResult
                {
                    Department = "Department C",
                    ContactName = "An Mai Loan",
                    SalesManager ="SalesManager C",
                    PartnerName = "PartnerName C",
                    Description = "Logistics",
                    Area ="Area C",
                    POL = "POL C",
                    POD ="POD C",
                    Lines ="Lines C",
                    Agent ="Agent C",
                    NominationParty ="NominationParty C",
                    assigned =false,
                    TransID ="TransID C",
                    HWBNO ="HWBNO C",
                    Qty20 =2,
                    Qty40 =2,
                    Cont40HC =2,
                    KGS = 3,
                    CBM=3,
                    SellingRate =3,
                    SharedProfit =3,
                    BuyingRate = 3,
                    OtherCharges =3,
                    SalesTarget =3,
                    Bonus = 3,
                    TpyeofService ="TpyeofService C",
                    Shipper = "Shipper C",
                    Consignee = "Consignee C"
                }

            };
            Crystal result = new Crystal();
            var parameter = new MonthlySaleReportParameter
            {
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                Contact = "admin",
                CompanyName ="Company Name",
                CompanyAddress1 = "CompanyAddress1",
                CurrDecimalNo =2,
                ReportBy ="Tui day ne",
                Director = "Director",
                ChiefAccountant = "ChiefAccountant",
                CompanyDescription = "CompanyDescription",
                CompanyAddress2 = "CompanyAddress2",
                Website = "Website",
                SalesManager = "SaleManager"
            };
            result = new Crystal
            {
                ReportName = "Monthly Sale Report.rpt",
                AllowPrint = true,
                AllowExport = true,
                IsLandscape = true
            };
            result.AddDataSource(list);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return Ok(result);
        }
    }
}
