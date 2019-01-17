using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class TestSeaFclexportShipmentService : RepositoryBase<TestSeaFclexportShipment, TestSeaFclexportShipmentModel>, ITestSeaFclexportShipmentService
    {
        public TestSeaFclexportShipmentService(IContextBase<TestSeaFclexportShipment> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public List<TestSeaFclexportShipmentModel> Paging(TestSeaFclexportShipmentCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = Query(criteria);
            rowsCount = data.Count();
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }
            var results = new List<TestSeaFclexportShipmentModel>();
            foreach(var item in data)
            {
                var shipment = mapper.Map<TestSeaFclexportShipmentModel>(item);
            }
            return results;
        }

        public IQueryable<TestSeaFclexportShipmentModel> Query(TestSeaFclexportShipmentCriteria criteria)
        {
            var coloaders = (from partner in ((eFMSDataContext)DataContext.DC).CatPartner where partner.PartnerGroup.Contains("CUSTOMER") select partner);
            var agents = (from partner in ((eFMSDataContext)DataContext.DC).CatPartner where partner.PartnerGroup.Contains("AGENT") select partner);
            var query = (from   shipment in ((eFMSDataContext)DataContext.DC).TestSeaFclexportShipment
                    join    houseBill in ((eFMSDataContext)DataContext.DC).TestHouseBillSeaFclexport on shipment.Mblno equals houseBill.Mblno
                    join    user in ((eFMSDataContext)DataContext.DC).SysUser on shipment.UserCreated equals user.Id
                    join    coloader in coloaders on shipment.ColoaderId equals coloader.Id into coloaderLeft
                            from x in coloaderLeft.DefaultIfEmpty()
                    join    agent in agents on shipment.AgentId equals agent.Id into agentLeft
                            from y in agentLeft
                    join    container in ((eFMSDataContext)DataContext.DC).TestContainerList on shipment.JobId equals container.JobId into containerLeft
                            from z in containerLeft
                    select  new { shipment, houseBill, coloader = x, agent = y, containers = z });
            var results = new List<TestSeaFclexportShipmentModel>();
            foreach (var item in query)
            {
                var shiment = mapper.Map<TestSeaFclexportShipmentModel>(item.shipment);
                
            }
            return results.AsQueryable();
        }
    }
}
