using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace eFMS.API.Report.Controllers
{
    public class SaleReportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}