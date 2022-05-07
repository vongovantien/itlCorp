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
            var users = sysUserRepo.Get();
            var partners = catPartnerRepo.Get();
            var chargeBuyings = catChargeRepo.Where(x => x.Type == "CREDIT");
            var chargeSellings = catChargeRepo.Where(x => x.Type == "DEBIT");

            var data = from rule in ruleLinkFees
                       join user in users on rule.UserCreated equals user.Id into gr
                       from user in gr.DefaultIfEmpty()
                       join modified in users on rule.UserModified equals modified.Id
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
                           Status =rule.Status==true?( CheckExpired(rule.EffectiveDate, rule.ExpiredDate) ? false : true):rule.Status,
                           EffectiveDate = rule.EffectiveDate,
                           ExpiredDate = rule.ExpiredDate,
                           ChargeNameBuying = chargeBuy.ChargeNameEn,
                           ChargeNameSelling = chargeSell.ChargeNameEn,
                           PartnerBuying = rule.PartnerBuying,
                           PartnerSelling = rule.PartnerSelling,
                           UserCreated = rule.UserCreated,
                           DatetimeCreated = rule.DatetimeCreated,
                           UserModified = rule.UserModified,
                           UserNameModified = modified.Username,
                       };
            data.ToList().ForEach(x =>
               DataContext.Update(mapper.Map<CsRuleLinkFee>(x), y => y.Id == x.Id));

            DataContext.SubmitChanges();

            return data.ToList().OrderByDescending(o => o.DatetimeModified).AsQueryable();
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

            if (!string.IsNullOrEmpty(criteria.PartnerSelling))
            {
                query = query.And(x =>
                                   x.PartnerSelling == criteria.PartnerSelling
                );
            }

            if (!string.IsNullOrEmpty(criteria.PartnerBuying))
            {
                query = query.And(x => x.PartnerBuying == criteria.PartnerBuying);
            }

            if (criteria.DateType != null)
            {
                if (criteria.FromDate.HasValue && criteria.ToDate.HasValue)
                {
                    switch (criteria.DateType)
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
            try
            {
                var rule = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
                var userCurrent = currentUser.UserID;
                var ruleLinkFee = mapper.Map<CsRuleLinkFee>(model);
                ruleLinkFee.UserModified = currentUser.UserID;
                ruleLinkFee.DatetimeModified = DateTime.Now;
                ruleLinkFee.UserCreated = rule.UserCreated;
                ruleLinkFee.DatetimeCreated = rule.DatetimeCreated;
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
            var user = sysUserRepo.Get();
            var ruleLinkFee = DataContext.Get(x => x.Id == idRuleLinkFee).FirstOrDefault();
            if (ruleLinkFee == null) return null;
            var modelMap = mapper.Map<RuleLinkFeeModel>(ruleLinkFee);
            if (CheckExpired(ruleLinkFee.EffectiveDate, ruleLinkFee.ExpiredDate) && ruleLinkFee.Status == true)
            {
                modelMap.Status = false;
                DataContext.Update(ruleLinkFee, x => x.Id == modelMap.Id);
                DataContext.SubmitChanges();
            }
            modelMap.UserNameCreated = user.Where(x => x.Id == modelMap.UserCreated).FirstOrDefault().Username;
            modelMap.UserNameModified = user.Where(x => x.Id == modelMap.UserModified).FirstOrDefault().Username;
            return modelMap;
        }

        private bool CheckExpired(DateTime? effectiveDate, DateTime? expriredDate)
        {
            if (expriredDate.HasValue)
            {
                //if (DateTime.Now > expriredDate.Value && effectiveDate.Value!=expriredDate.Value) return true;
                if (DateTime.Now.Date > expriredDate.Value.Date) return true;
                return false;
            }
            return false;
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

                if (model.ExpiredDate != null)
                {
                    if (model.EffectiveDate.Value.Date > model.ExpiredDate.Value.Date)
                    {
                        return new HandleState("Expiration date must be greater than or equal to the Effective date");
                    }
                }

                //Trường hợp Insert (Id of rule is null or empty)
                if (model.Id == null)
                {
                    var ruleNameExists = DataContext.Get(x => x.RuleName == model.RuleName).Any();
                    if (ruleNameExists)
                    {
                        return new HandleState("Rule Name already exists");
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
                        if (rule.Any())
                        {
                            return new HandleState(ErrorCode.Existed, "Rule "+model.RuleName+ " redupplicated from Rule " +rule.FirstOrDefault().RuleName+" Already exists");
                        }
                    }
                }
                else //Trường hợp Update (Id of rule is not null & not empty)
                {
                    var ruleNameExists = DataContext.Get(x => x.Id != model.Id
                                         && x.RuleName == model.RuleName).Any();
                    if (ruleNameExists)
                    {
                        return new HandleState("Rule name already exists");
                    }

                    //Check all rule
                    var rule = DataContext.Get(x => x.Id != model.Id
                                                    && x.ServiceBuying == model.ServiceBuying
                                                    && x.ChargeBuying == model.ChargeBuying
                                                    && x.PartnerBuying == model.PartnerBuying
                                                    && x.ServiceSelling == model.ServiceSelling
                                                    && x.ChargeSelling == model.ChargeSelling
                                                    && x.ServiceSelling == model.ServiceSelling
                                                    );
                    if (rule.Any())
                    {
                        if (rule.Any())
                        {
                            return new HandleState(ErrorCode.Existed, "Rule " + rule.FirstOrDefault().RuleName + " Already exists");
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

        public List<RuleLinkFeeImportModel> CheckRuleLinkFeeValidImport(List<RuleLinkFeeImportModel> list)
        {
            var results = new List<RuleLinkFeeImportModel>();

            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item.RuleName))
                {
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.ServiceBuying))
                {
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.ServiceSelling))
                {
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.ChargeBuying))
                {
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.ChargeSelling))
                {
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.PartnerBuying))
                {
                    item.IsValid = false;
                }
                if (!CheckCharge(item.ChargeBuying, item.ChargeSelling, ConvertService(item.ServiceBuying), ConvertService(item.ServiceSelling)) || ConvertService(item.ServiceBuying) is null ||ConvertService(item.ServiceSelling) is null)
                {
                    item.IsValid = false;
                }
                results.Add(item);
            }
            if (list.Count > 1) { 
                for(int i = 0; i < list.Count()-1; i++)
                {
                    int j = i+1;
                    while (j < list.Count())
                    {
                        if (list[i].RuleName == list[j].RuleName||(list[i].ServiceBuying == list[j].ServiceBuying&& list[i].ServiceSelling == list[j].ServiceSelling && list[i].ChargeBuying == list[j].ChargeBuying && list[i].ChargeSelling == list[j].ChargeSelling && list[i].PartnerBuying == list[j].PartnerBuying && list[i].PartnerSelling == list[j].PartnerSelling ))
                        {
                            list[i].IsValid = false;
                        }
                        j++;
                    }
                }
            }
            return results;
        }

        private string ConvertService(string serviceName)
        {
            switch (serviceName)
            {
                case "Air Import":
                    return "AI";
                case "Custom Logistic":
                    return "CL";
                case "Air Export":
                    return "AE";
                case "Sea FCL Export":
                    return "SFE";
                case "Sea FCL Import":
                    return "SFI";
                case "Sea LCL Export":
                    return "SLE";
                case "Sea LCL Import":
                    return "SLI";
                case "Inland Trucking":
                    return "IT";
                case "Sea Consol Export":
                    return "SCE";
                case "Sea Consol Import":
                    return "SCI";
                default:
                    return null;
            }
        }

        private bool CheckCharge(string chargeBuying, string chargeSelling, string serviceBuying, string serviceSelling)
        {
            var charge = catChargeRepo.Get();
            var buying = charge.Where(x => x.Code==chargeBuying && x.Type == "CREDIT" && x.ServiceTypeId.Contains(serviceBuying)).FirstOrDefault();
            var selling = charge.Where(x => x.Code==chargeSelling && x.Type == "DEBIT" && x.ServiceTypeId.Contains(serviceSelling)).FirstOrDefault();
            if (buying!=null&&selling!=null)
            {
                return true;
            }
            return false;
        }

        public HandleState Import(List<RuleLinkFeeImportModel> data)
        {
            try
            {
                var charge = catChargeRepo.Get();
                var partner = catPartnerRepo.Get();
                foreach (var item in data)
                {
                    if (!CheckCharge(item.ChargeBuying,item.ChargeSelling, ConvertService(item.ServiceBuying), ConvertService(item.ServiceSelling)))
                    {
                        return new HandleState(false, "Charge Buying or Selling not match Type");
                    }
                    bool active = item.Status.ToLower() == "active" ? true : false;
                    var ruleLinkFee = new CsRuleLinkFee
                    {
                        Id = Guid.NewGuid(),
                        RuleName = item.RuleName,
                        ChargeBuying = charge.Where(x => x.Code==item.ChargeBuying && x.Type == "CREDIT").FirstOrDefault().Id.ToString(),
                        ChargeSelling = charge.Where(x => x.Code==item.ChargeSelling && x.Type == "DEBIT").FirstOrDefault().Id.ToString(),
                        PartnerBuying = partner.Where(x => x.AccountNo==item.PartnerBuying).FirstOrDefault().Id,
                        PartnerSelling = item.PartnerSelling != null ? partner.Where(x => x.AccountNo==item.PartnerSelling).FirstOrDefault().Id : null,
                        ServiceBuying = ConvertService(item.ServiceBuying),
                        ServiceSelling = ConvertService(item.ServiceSelling),
                        EffectiveDate = DateTime.Now,
                        DatetimeModified = DateTime.Now,
                        DatetimeCreated = DateTime.Now,
                        UserCreated = currentUser.UserID,
                        UserModified = currentUser.UserID,
                        Status = active,
                    };
                    var model = mapper.Map<RuleLinkFeeModel>(ruleLinkFee);
                    model.Id = null;
                    var hs = CheckExistsDataRule(model);
                    if (!hs.Success)
                    {
                        return hs;
                    }
                    DataContext.Add(ruleLinkFee, false);
                }
                DataContext.SubmitChanges();
                Get();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
    }
}
