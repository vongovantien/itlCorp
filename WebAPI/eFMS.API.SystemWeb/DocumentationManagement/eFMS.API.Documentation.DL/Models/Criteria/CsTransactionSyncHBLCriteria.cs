
using System;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class CsTransactionSyncHBLCriteria
    {
        public string FlightVesselName;
        public DateTime? Etd;
        public DateTime? Eta;
        public DateTime? FlightDate;
        public Guid? Pol;
        public Guid? Pod;
        public string AgentId;
        public string IssuedBy;
        public Guid? WarehouseId;

    }
}
