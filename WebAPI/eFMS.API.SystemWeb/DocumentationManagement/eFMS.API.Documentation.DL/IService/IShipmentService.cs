using eFMS.API.Documentation.DL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eFMS.API.Documentation.DL.Models;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IShipmentService
    {
        IQueryable<Shipments> GetShipmentNotLocked();

        IQueryable<Shipments> GetShipmentsCreditPayer(string partner, List<string> services);
    }
}
