using eFMS.API.Documentation.DL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ITerminologyService
    {
        List<FreightTerm> GetFreightTerms();
        List<ShipmentType> GetShipmentTypes();
        List<BillofLoadingType> GetBillofLoadingTypes();
        List<ServiceType> GetServiceTypes();
        List<TypeOfMove> GetTypeOfMoves();
    }
}
