using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class TerminologyService : ITerminologyService
    {
        public List<FreightTerm> GetFreightTerms()
        {
            return TermData.FreightTerms;
        }

        public List<ShipmentType> GetShipmentTypes()
        {
            return TermData.ShipmentTypes;
        }

        public List<BillofLoadingType> GetBillofLoadingTypes()
        {
            return TermData.BillofLoadingTypes;
        }
        public List<ServiceType> GetServiceTypes()
        {
            return TermData.ServiceTypes;
        }

        public List<TypeOfMove> GetTypeOfMoves()
        {
            return TermData.TypeOfMoves;
        }
    }
}
