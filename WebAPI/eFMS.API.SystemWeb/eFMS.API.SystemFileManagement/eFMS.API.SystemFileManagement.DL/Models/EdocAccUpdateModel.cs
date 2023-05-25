using System;
using System.Collections.Generic;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class EdocAccUpdateModel
    {
        public string BillingNo { get; set; }
        public string BillingType { get; set; }
        public List<Guid> ListAdd { get; set; }
        public List<Guid> ListDel { get; set; }
        public bool FromRep { get; set; }
    }

}
