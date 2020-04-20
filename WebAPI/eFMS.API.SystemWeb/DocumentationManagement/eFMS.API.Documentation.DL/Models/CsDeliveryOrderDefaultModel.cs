﻿using eFMS.API.Documentation.DL.Common;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsDeliveryOrderDefaultModel
    {
        public TransactionTypeEnum Type { get; set; }
        public string TransactionType { get; set; }
        public string UserDefault { get; set; }
        public string Doheader1 { get; set; }
        public string Doheader2 { get; set; }
        public string Dofooter { get; set; }
    }
}
