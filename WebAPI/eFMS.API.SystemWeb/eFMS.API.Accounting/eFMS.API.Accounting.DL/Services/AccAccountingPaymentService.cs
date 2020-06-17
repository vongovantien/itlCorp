using AutoMapper;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Accounting.DL.Services
{
    public class AccAccountingPaymentService : RepositoryBase<AccAccountingPayment, AccAccountingPaymentModel>, IAccAccountingPaymentService
    {
        private readonly IContextBase<SysUser> userRepository;
        public AccAccountingPaymentService(IContextBase<AccAccountingPayment> repository, IMapper mapper, IContextBase<SysUser> userRepo) : base(repository, mapper)
        {
            userRepository = userRepo;
        }

        public IQueryable<AccAccountingPaymentModel> GetBy(string refNo)
        {
            var data = Get(x => x.RefNo == refNo);
            var users = userRepository.Get();
            var results = data.Join(users, x => x.UserModified, y => y.Id, (x, y) => new AccAccountingPaymentModel
            {
                Id = x.Id,
                RefNo = x.RefNo,
                PaymentNo = x.PaymentNo,
                PaymentAmount = x.PaymentAmount,
                Balance = x.Balance,
                CurrencyId = x.CurrencyId,
                PaidDate = x.PaidDate,
                PaymentType = x.PaymentType,
                UserCreated = x.UserCreated,
                DatetimeCreated = x.DatetimeCreated,
                UserModified = x.UserModified,
                DatetimeModified = x.DatetimeModified,
                UserModifiedName = y.Username
            });
            return results;
        }
    }
}
