using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Accounting.DL.Services
{
    public class AccountingManagementService : RepositoryBase<AccAccountingManagement, AccAccountingManagementModel>, IAccountingManagementService
    {
        private IContextBase<CsShipmentSurcharge> surchargeRepository;
        private ICurrentUser currentUser;
        public AccountingManagementService(IContextBase<AccAccountingManagement> repository, IMapper mapper,
            IContextBase<CsShipmentSurcharge> surchargeRepo,
            ICurrentUser currUser) : base(repository, mapper)
        {
            surchargeRepository = surchargeRepo;
            currentUser = currUser;
        }

        public int CheckDeletePermission(Guid id)
        {
            var detail = DataContext.Get(x => x.Id == id).FirstOrDefault();
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.accManagement);//Set default
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            int code = PermissionExtention.GetPermissionCommonItem(new BaseUpdateModel { UserCreated = detail.UserCreated, CompanyId = detail.CompanyId, OfficeId = detail.OfficeId, DepartmentId = detail.DepartmentId, GroupId = detail.GroupId }, permissionRange, _user);
            return code;
        }

        public HandleState Delete(Guid id)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var data = DataContext.Get(x => x.Id == id).FirstOrDefault();
                    var hs = DataContext.Delete(x => x.Id == id);
                    if (hs.Success)
                    {
                        var charges = surchargeRepository.Get(x => x.AcctManagementId == id);
                        foreach(var item in charges)
                        {
                            item.AcctManagementId = null;
                            if(data.Type == AccountingConstants.ACCOUNTING_VOUCHER_TYPE)
                            {
                                item.VoucherId = null;
                                item.VoucherIddate = null;
                            }
                            if(data.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE)
                            {
                                item.InvoiceNo = null;
                                item.InvoiceDate = null;
                            }
                            item.DatetimeModified = DateTime.Now;
                            item.UserModified = currentUser.UserID;
                            surchargeRepository.Update(item, x => x.Id == item.Id, false);
                        }
                        surchargeRepository.SubmitChanges();
                        DataContext.SubmitChanges();
                        trans.Commit();
                    }
                    return hs;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }
    }
}
