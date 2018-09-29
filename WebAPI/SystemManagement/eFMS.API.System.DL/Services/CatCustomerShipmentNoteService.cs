using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using ITL.NetCore.Connection.EF;
using SystemManagement.DL.Models;
using ITL.NetCore.Connection.BL;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public class CatCustomerShipmentNoteService : RepositoryBase<CatCustomerShipmentNote, CatCustomerShipmentNoteModel>, ICatCustomerShipmentNoteService
    {

        public CatCustomerShipmentNoteService(IContextBase<CatCustomerShipmentNote> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public override string ToString()
        {
            return base.ToString();
        }
        public IQueryable<CatCustomerShipmentNoteModel> GetFollowCustomer(string CustomerID)
        {
            return base.Get(w => w.CustomerId == CustomerID);
        }
    }
}
