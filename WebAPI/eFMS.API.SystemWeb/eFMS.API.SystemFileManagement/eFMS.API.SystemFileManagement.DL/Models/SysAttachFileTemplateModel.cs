using eFMS.API.SystemFileManagement.Service.Models;
using System;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class SysAttachFileTemplateModel : SysAttachFileTemplate
    {
    }

    public class AttachFileTemplateTransactionTypeModel
    {
        public Guid JobId { get; set; }
        public string TransactionType { get; set; }
    }
}
