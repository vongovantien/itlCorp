using eFMS.API.SystemFileManagement.Service.Models;
using System.Collections.Generic;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class CatContractModel : CatContract
    {
        public List<string> Customers { get; set; }
    }
}
