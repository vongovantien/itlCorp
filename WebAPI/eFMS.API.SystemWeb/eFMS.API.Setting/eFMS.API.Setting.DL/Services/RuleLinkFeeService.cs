using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Infrastructure.Extensions;
using eFMS.API.Setting.DL.Common;
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
    public class RuleLinkFeeService : RepositoryBase<CsRuleLinkFee, RuleLinkFeeModel>, IRuleLinkFeeService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUser> sysUserRepo;
        private readonly IContextBase<CatPartner> catPartnerRepo;
        private readonly IContextBase<CatCharge> catChargeRepo;

        public RuleLinkFeeService(IStringLocalizer<LanguageSub> localizer, IMapper mapper, ICurrentUser user,
                                    IContextBase<CsRuleLinkFee> repository,
                                    IContextBase<SysUser> sysUser,
                                    IContextBase<CatPartner> catPartner,
                                    IContextBase<CatCharge> catCharge
            ) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            sysUserRepo = sysUser;
            catPartnerRepo = catPartner;
            catChargeRepo = catCharge;
        }

        public HandleState AddNewRuleLinkFee(RuleLinkFeeModel model)
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
                rule.Status = true;

                if (DataContext.Any(x => x.RuleName == rule.RuleName && x.Id != rule.Id))
                {
                    return new HandleState((object)string.Format("Rule {0} was existied", model.RuleName));
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

        public HandleState DeleteRuleLinkFee(Guid? idRuleLinkFee)
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

        public List<RuleLinkFeeModel> Paging(RuleLinkFeeCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = GetRuleByCriteria(criteria);
            if (data == null)
            {
                rowsCount = 0;
                return null;
            }

            var result = new List<RuleLinkFeeModel>();

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

        public IQueryable<RuleLinkFeeModel> GetRuleByCriteria(RuleLinkFeeCriteria criteria)
        {
            List<RuleLinkFeeModel> queryable = new List<RuleLinkFeeModel>();
            var queryRuleLinkFee = ExpressionQuery(criteria);
            var ruleLinkFees = DataContext.Where(queryRuleLinkFee);
            if (ruleLinkFees != null)
            {
                ruleLinkFees = ruleLinkFees.OrderByDescending(orb => orb.DatetimeModified).AsQueryable();
            }
            var users = sysUserRepo.Get();
            var partners = catPartnerRepo.Get();
            var chargeBuyings = catChargeRepo.Where(x => x.Type == "CREDIT");
            var chargeSellings = catChargeRepo.Where(x => x.Type == "DEBIT");

            var data = from rule in ruleLinkFees
                       join user in users on rule.UserCreated equals user.Id into gr
                       from user in gr.DefaultIfEmpty()
                       join sell in partners on rule.PartnerSelling equals sell.Id into gr1
                       from sell in gr1.DefaultIfEmpty()
                       join buy in partners on rule.PartnerBuying equals buy.Id into gr2
                       from buy in gr2.DefaultIfEmpty()
                       join chargeBuy in chargeBuyings on rule.ChargeBuying equals chargeBuy.Id.ToString() into gr3
                       from chargeBuy in gr3.DefaultIfEmpty()
                       join chargeSell in chargeSellings on rule.ChargeSelling equals chargeSell.Id.ToString() into gr4
                       from chargeSell in gr4.DefaultIfEmpty()
                       select new RuleLinkFeeModel()
                       {
                           Id = rule.Id,
                           RuleName = rule.RuleName,
                           ServiceBuying = rule.ServiceBuying,
                           ChargeBuying = rule.ChargeBuying,
                           PartnerNameBuying = buy.ShortName,
                           PartnerNameSelling = sell.ShortName,
                           ServiceSelling = rule.ServiceSelling,
                           UserNameCreated = user.Username,
                           ChargeSelling = rule.ChargeSelling,
                           DatetimeModified = rule.DatetimeModified,
                           Status = rule.Status,
                           EffectiveDate = rule.EffectiveDate,
                           ExpiredDate = rule.ExpiredDate,
                           ChargeNameBuying = chargeBuy.ChargeNameVn,
                           ChargeNameSelling = chargeSell.ChargeNameVn,
                       };

            return data.ToArray().OrderByDescending(o => o.DatetimeModified).AsQueryable();
        }
        private Expression<Func<CsRuleLinkFee, bool>> ExpressionQuery(RuleLinkFeeCriteria criteria)
        {
            Expression<Func<CsRuleLinkFee, bool>> query = q => true;
            if (!string.IsNullOrEmpty(criteria.RuleName))
            {
                query = query.And(x =>
                                   x.RuleName == criteria.RuleName
                );
            }

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

            if (!string.IsNullOrEmpty(criteria.Datetype))
            {
                if (criteria.FromDate.HasValue && criteria.ToDate.HasValue)
                {
                    switch (criteria.Datetype)
                    {
                        case "CreateDate":
                            query = query.And(x => x.DatetimeCreated.Value.Date >= criteria.FromDate.Value.Date &&
                            x.DatetimeCreated.Value.Date <= criteria.ToDate.Value.Date);
                            break;
                        case "EffectiveDate":
                            query = query.And(x => x.EffectiveDate.Value.Date >= criteria.FromDate.Value.Date &&
                            x.EffectiveDate.Value.Date <= criteria.ToDate.Value.Date);
                            break;
                        case "ModifiedDate":
                            query = query.And(x => x.DatetimeModified.Value.Date >= criteria.FromDate.Value.Date
                            && x.DatetimeModified.Value.Date <= criteria.ToDate.Value.Date);
                            break;
                        case "ExpiredDate":
                            query = query.And(x => x.ExpiredDate.Value.Date >= criteria.FromDate.Value.Date &&
                            x.ExpiredDate.Value.Date <= criteria.ToDate.Value.Date);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (criteria.Status != null)
            {
                query = query.And(x => x.Status == criteria.Status);
            }

            return query;
        }

        public HandleState UpdateRuleLinkFee(RuleLinkFeeModel model)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingLinkFee);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None) return new HandleState(403, "");

            try
            {
                var rule = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
                var userCurrent = currentUser.UserID;
                var ruleLinkFee = mapper.Map<CsRuleLinkFee>(model);
                ruleLinkFee.UserModified = currentUser.UserID;
                ruleLinkFee.DatetimeModified = DateTime.Now;
                ruleLinkFee.UserCreated = rule.UserCreated;
                ruleLinkFee.DatetimeCreated = rule.DatetimeCreated;
                ruleLinkFee.Status = rule.Status;
                var hs = DataContext.Update(ruleLinkFee, x => x.Id == ruleLinkFee.Id);
                return hs;
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_LOG_CsRuleLinkFee", ex.ToString());
                return new HandleState(ex.Message);
            }
        }

        public RuleLinkFeeModel GetRuleLinkFeeById(Guid idRuleLinkFee)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingLinkFee);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);
            if (permissionRange == PermissionRange.None) return null;
            var user = sysUserRepo.Get();
            var ruleLinkFee = DataContext.Get(x => x.Id == idRuleLinkFee).FirstOrDefault();
            if (ruleLinkFee == null) return null;
            var modelMap = mapper.Map<RuleLinkFeeModel>(ruleLinkFee);
            modelMap.UserNameCreated = user.Where(x => x.Id == modelMap.UserCreated).FirstOrDefault().Username;
            modelMap.UserNameModified = user.Where(x => x.Id == modelMap.UserModified).FirstOrDefault().Username;
            return modelMap;
        }

        /// <summary>
        /// * Check tồn tại rule. Check theo các field: 
        /// - Rule Name (Không được trùng tên), 
        /// - Effective Date - Expried Date
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HandleState CheckExistsDataRule(RuleLinkFeeModel model)
        {
            try
            {
                var hs = CheckDuplicateRule(model);
                if (!hs.Success)
                {
                    return hs;
                }

                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public HandleState CheckDuplicateRule(RuleLinkFeeModel model)
        {
            try
            {
                if (model == null)
                {
                    return new HandleState("Rule is not null");
                }

                //Ngày ExpiredDate không được nhỏ hơn ngày EffectiveDate
                if (model.EffectiveDate.Value.Date > model.ExpiredDate.Value.Date)
                {
                    return new HandleState("Expired Date cannot be less than the Effective Date");
                }

                //Trường hợp Insert (Id of rule is null or empty)
                if (model.Id == Guid.Empty)
                {
                    var ruleNameExists = DataContext.Get(x => x.RuleName == model.RuleName).Any();
                    if (ruleNameExists)
                    {
                        return new HandleState("Rule name already exists");
                    }

                    //Check all rule
                    var rule = DataContext.Get(x => x.ServiceBuying == model.ServiceBuying
                                                    && x.ChargeBuying == model.ChargeBuying
                                                    && x.PartnerBuying == model.PartnerBuying
                                                    && x.ServiceSelling == model.ServiceSelling
                                                    && x.ChargeSelling == model.ChargeSelling
                                                    && x.ServiceSelling == model.ServiceSelling
                                                    );
                    if (rule.Any())
                    {
                        //Check nằm trong khoảng EffectiveDate - ExpiredDate
                        rule = rule
                            .Where(x => model.EffectiveDate.Value.Date >= x.EffectiveDate.Value.Date
                                     && model.ExpiredDate.Value.Date <= x.ExpiredDate.Value.Date);
                        if (rule.Any())
                        {
                            return new HandleState(ErrorCode.Existed, "Already exists");
                        }
                    }
                }
                else //Trường hợp Update (Id of rule is not null & not empty)
                {
                    var ruleNameExists = DataContext.Get(x => x.Id != model.Id
                                                             && x.RuleName == model.RuleName).Any();
                    if (ruleNameExists)
                    {
                        return new HandleState("rule name already exists");
                    }

                    //Check all rule
                    var rule = DataContext.Get(x => x.ServiceBuying == model.ServiceBuying
                                                    && x.ChargeBuying == model.ChargeBuying
                                                    && x.PartnerBuying == model.PartnerBuying
                                                    && x.ServiceSelling == model.ServiceSelling
                                                    && x.ChargeSelling == model.ChargeSelling
                                                    && x.ServiceSelling == model.ServiceSelling
                                                    );
                    if (rule.Any())
                    {
                        //Check nằm trong khoảng EffectiveDate - ExpiredDate
                        rule = rule
                            .Where(x => model.EffectiveDate.Value.Date >= x.EffectiveDate.Value.Date
                                     && model.ExpiredDate.Value.Date <= x.ExpiredDate.Value.Date);
                        if (rule.Any())
                        {
                            return new HandleState(ErrorCode.Existed, "Already exists");
                        }
                    }
                }

                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

    }
}
