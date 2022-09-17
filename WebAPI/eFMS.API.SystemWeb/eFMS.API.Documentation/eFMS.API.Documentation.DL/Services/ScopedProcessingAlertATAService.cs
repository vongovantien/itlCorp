using AutoMapper;
using eFMS.API.Common.Helpers;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Services
{
    public class ScopedProcessingAlertATAService: RepositoryBase<CsTransaction, CsTransactionModel>, IScopedProcessingAlertATAService
    {
        private readonly ITerminologyService service;

        public ScopedProcessingAlertATAService(IContextBase<CsTransaction> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        //public ScopedProcessingAlertATAService()
        //{
        //    //service = _service;
        //}
        public async Task AlertATA()
        {
            Console.WriteLine("RUNNNING");
            //var d = service.GetAllShipmentCommonData();
            await Task.Delay(3000);
        }
    }
}
