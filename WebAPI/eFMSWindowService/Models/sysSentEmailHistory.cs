//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace eFMSWindowService.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class sysSentEmailHistory
    {
        public int ID { get; set; }
        public string SentUser { get; set; }
        public string Receivers { get; set; }
        public string CCs { get; set; }
        public string BCCs { get; set; }
        public string Subject { get; set; }
        public string Type { get; set; }
        public Nullable<bool> Sent { get; set; }
        public Nullable<System.DateTime> SentDateTime { get; set; }
        public string Description { get; set; }
        public string Body { get; set; }
    }
}
