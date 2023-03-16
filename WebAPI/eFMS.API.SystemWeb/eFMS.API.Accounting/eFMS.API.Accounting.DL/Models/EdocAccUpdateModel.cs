using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class EdocAccUpdateModel
    {
        public string BillingNo { get; set; }
        public string BillingType { get; set; }
        public List<EdocJobModel> ListAdd { get; set; }
        public List<Guid?> ListDel { get; set; }
    }

    public class EdocJobModel
    {
        public Guid? JobId { get; set; }
        public string TransactionType { get; set; }
    }
    
}
