using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class OpsTransactionClearanceModel
    {
        public OpsTransactionModel OpsTransaction { get; set; }
        public CustomsDeclarationModel CustomsDeclaration { get; set; }
    }
}
