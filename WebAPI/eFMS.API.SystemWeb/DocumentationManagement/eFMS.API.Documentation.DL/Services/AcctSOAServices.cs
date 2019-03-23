using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class AcctSOAServices : RepositoryBase<AcctSoa,AcctSOAModel>,IAcctSOAServices
    {
        public AcctSOAServices(IContextBase<AcctSoa> repository,IMapper mapper) : base(repository, mapper)
        {

        }

        private string RandomCode()
        {
            var allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
            var head = new char[3];
            var body = new char[4];
            var rd = new Random();
            for (var i = 0; i < 3; i++)
            {
                head[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            for (var i = 0; i < 4; i++)
            {
                body[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new string(head+"-"+body).ToUpper();
        }

        public HandleState AddNewSOA(AcctSOAModel model)
        {
            try
            {

                var soa = mapper.Map<AcctSoa>(model);
                soa.Id = Guid.NewGuid();
                soa.Code = RandomCode();
                DataContext.Add(soa);

                foreach (var c in model.listCharges)
                {
                    var charge = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Id == c.Id).FirstOrDefault();
                    if (charge != null)
                    {
                        charge.Soano = soa.Code;
                        charge.Soaclosed = true;
                    }
                }
                ((eFMSDataContext)DataContext.DC).SaveChanges();

                return new HandleState();
            }
            catch(Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }

        }

        public HandleState UpdateSOA(AcctSOAModel model)
        {
            throw new NotImplementedException();
        }
    }
}
