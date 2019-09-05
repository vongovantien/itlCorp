using AutoMapper;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class AcctSettlementPaymentService : RepositoryBase<AcctSettlementPayment, AcctSettlementPaymentModel>, IAcctSettlementPaymentService
    {
        private readonly ICurrentUser currentUser;
        private readonly IOptions<WebUrl> webUrl;
        public AcctSettlementPaymentService(IContextBase<AcctSettlementPayment> repository, IMapper mapper, ICurrentUser user, IOpsTransactionService ops, IOptions<WebUrl> url) : base(repository, mapper)
        {
            currentUser = user;
            webUrl = url;
        }

        public List<AcctSettlementPaymentResult> Paging(AcctSettlementPaymentCriteria criteria, int page, int size, out int rowsCount)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var settle = dc.AcctApproveSettlement;
            
            //Phân trang
            //rowsCount = (data.Count() > 0) ? data.Count() : 0;
            //if (size > 0)
            //{
            //    if (page < 1)
            //    {
            //        page = 1;
            //    }
            //    data = data.Skip((page - 1) * size).Take(size);
            //}

            //return data.ToList();
            throw new NotImplementedException();
        }
    }
}
