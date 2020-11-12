using eFMS.API.Accounting.Service.Models;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models
{
    public class CatContractModel : CatContract
    {
        public List<string> Customers { get; set; }
    }
}
