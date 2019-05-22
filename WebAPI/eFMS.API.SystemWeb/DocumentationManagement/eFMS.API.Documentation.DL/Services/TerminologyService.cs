using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;

namespace eFMS.API.Documentation.DL.Services
{
    public class TerminologyService : ITerminologyService
    {

        public object GetAllShipmentCommonData()
        {
            var freightTerms = TermData.FreightTerms;
            var shipmentTypes = TermData.ShipmentTypes;
            var billOfLadings = TermData.BillofLadingTypes;
            var serviceTypes = TermData.ServiceTypes;
            var typeOfMoves = TermData.TypeOfMoves;
            var termData = new { freightTerms, shipmentTypes, billOfLadings, serviceTypes, typeOfMoves };
            return termData;
        }
        public object GetOPSShipmentCommonData()
        {
            var productServices = TermData.ProductServices;
            var serviceModes = TermData.ServiceModes;
            var shipmentModes = TermData.ShipmentModes;
            var termData = new { productServices, serviceModes, shipmentModes };
            return termData;
        }
    }
}
