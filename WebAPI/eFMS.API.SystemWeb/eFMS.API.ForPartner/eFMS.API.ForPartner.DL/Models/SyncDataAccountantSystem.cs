using System;

namespace eFMS.API.ForPartner.DL.Models
{
    public class SyncDataAccountantSystem
    {
        public Guid ID { get; set; }
        public TypeSystem Type { get; set; }
    }

    public enum TypeSystem
    {
        Voucher,
        SOA,
        CDNote 
    }
}
