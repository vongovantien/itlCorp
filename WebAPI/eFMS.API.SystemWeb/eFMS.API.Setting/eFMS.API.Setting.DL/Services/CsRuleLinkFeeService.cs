using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Infrastructure.Extensions;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace eFMS.API.Setting.DL.Services
{
    public class CsRuleLinkFeeService : RepositoryBase<CsRuleLinkFee, CsRuleLinkFeeModel>, ICsRuleLinkFeeService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;

        public CsRuleLinkFeeService(IStringLocalizer<LanguageSub> localizer, IMapper mapper, ICurrentUser user, IContextBase<CsRuleLinkFee> repository) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
        }

        public HandleState AddNewRuleLinkFee(CsRuleLinkFeeModel model)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingLinkFee);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None) return new HandleState(403, "");

            try
            {
                var rule = mapper.Map<CsRuleLinkFee>(model);
                rule.Id = Guid.NewGuid();
                rule.UserCreated = currentUser.UserID;
                rule.DatetimeCreated = DateTime.Now;
                rule.DatetimeModified = DateTime.Now;
                rule.UserModified = currentUser.UserID;
                rule.Active = true;
                rule.InactiveOn = null;

                if (DataContext.Any(x => x.NameRule == rule.NameRule && x.Id != rule.Id))
                {
                    return new HandleState((object)string.Format("Rule {0} was existied", model.NameRule));
                }

                var hs = DataContext.Add(rule);
                return hs;
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_LOG_SettingRuleLinkFee", ex.ToString());
                return new HandleState(ex.Message);
            }

        }

        public HandleState DeleteRuleLinkFee(Guid idRuleLinkFee)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingLinkFee);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            if (permissionRange == PermissionRange.None) return new HandleState(403, "");
            try
            {
                var hs = DataContext.Delete(x => x.Id == idRuleLinkFee);
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState((object)ex.Message);
            }

        }

        public List<CsRuleLinkFeeModel> Paging(CsRuleLinkFeeCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = GetRuleByCriteria(criteria);
            if (data == null)
            {
                rowsCount = 0;
                return null;
            }

            var result = new List<CsRuleLinkFeeModel>();

            //Phân trang
            var _totalItem = data.Select(s => s.Id).Count();
            rowsCount = (_totalItem > 0) ? _totalItem : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);

                result = data.ToList();
            }

            return result;
        }

        public IQueryable<CsRuleLinkFeeModel> GetRuleByCriteria(CsRuleLinkFeeCriteria criteria)
        {
            List<CsRuleLinkFeeModel> queryable = new List<CsRuleLinkFeeModel>();
            var queryRuleLinkFee = ExpressionQuery(criteria);
            var ruleLinkFees = DataContext.Where(queryRuleLinkFee);
            if (ruleLinkFees != null)
            {
                ruleLinkFees = ruleLinkFees.OrderByDescending(orb => orb.DatetimeModified).AsQueryable();
            }
            foreach (var rule in ruleLinkFees)
            {
                queryable.Add(
                    _mapper.Map<CsRuleLinkFeeModel>(rule)
                    );
            }
            return queryable.AsQueryable();
        }
        private Expression<Func<CsRuleLinkFee, bool>> ExpressionQuery(CsRuleLinkFeeCriteria criteria)
        {
            Expression<Func<CsRuleLinkFee, bool>> query = q => true;
            if (!string.IsNullOrEmpty(criteria.ServiceSelling))
            {
                query = query.And(x =>
                                   x.ServiceSelling == criteria.ServiceSelling
                );
            }

            if (!string.IsNullOrEmpty(criteria.ServiceBuying))
            {
                query = query.And(x => x.ServiceBuying == criteria.ServiceBuying);
            }

            if (criteria.EffectiveDate.HasValue)
            {
                query = query.And(x => x.EffectiveDate == criteria.EffectiveDate);
            }

            if (criteria.ExpirationDate.HasValue)
            {
                query = query.And(x => x.ExpirationDate == criteria.ExpirationDate);
            }

            if (criteria.Status != null)
            {
                query = query.And(x => x.Active == criteria.Status);
            }

            return query;
        }

        public HandleState UpdateRuleLinkFee(CsRuleLinkFeeModel model)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None) return new HandleState(403, "");

            try
            {
                var userCurrent = currentUser.UserID;
                var ruleLinkFee = mapper.Map<CsRuleLinkFee>(model);

                var hs = DataContext.Update(ruleLinkFee, x => x.Id == ruleLinkFee.Id);
                return hs;
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_LOG_CsRuleLinkFee", ex.ToString());
                return new HandleState(ex.Message);
            }
        }
    }
}
