using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatPartnerService : RepositoryBase<CatPartner, CatPartnerModel>, ICatPartnerService
    {
        public CatPartnerService(IContextBase<CatPartner> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public List<CatPartnerImportModel> CheckValidImport(List<CatPartnerImportModel> list)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var partners = dc.CatPartner.ToList();
            list.ForEach(item =>
            {

            });
            return list;
        }

        public List<DepartmentPartner> GetDepartments()
        {
            return DataEnums.Departments;
        }

        public HandleState Import(List<CatPartnerImportModel> data)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                foreach (var item in data)
                {
                    var partner = mapper.Map<CatPartner>(item);
                    partner.Inactive = item.Status.ToString().ToLower() == "active" ? false : true;
                    dc.CatPartner.Add(partner);
                }
                dc.SaveChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public IQueryable<CatPartnerViewModel> Paging(CatPartnerCriteria criteria, int page, int size, out int rowsCount)
        {
            //var query = GetQueryExpression(criteria);
            //return Paging(query, page, size, out rowsCount);
            List<CatPartnerViewModel> results = null;
            var list = Query(criteria);
            rowsCount = list.ToList().Count;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = list.Skip((page - 1) * size).Take(size).ToList();
            }
            return results.AsQueryable();
        }

        public List<CustomerPartnerViewModel> PagingCustomer(CatPartnerCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = Query(criteria).GroupBy(x => x.SalePersonId);
            rowsCount = data.ToList().Count;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }
            List<CustomerPartnerViewModel> results = new List<CustomerPartnerViewModel>();
            foreach (var item in data)
            {
                var partner = new CustomerPartnerViewModel();
                partner.SalePersonId = item.Key;
                partner.SalePersonName = ((eFMSDataContext)DataContext.DC).SysUser.First(x => x.Id == item.Key).Username;
                partner.CatPartnerModels = item.ToList();
                partner.SumNumberPartner = item.Count();
                results.Add(partner);
            }
            return results;
        }

        public IQueryable<CatPartnerViewModel> Query(CatPartnerCriteria criteria)
        {
            var query = (from partner in ((eFMSDataContext)DataContext.DC).CatPartner
                         join user in ((eFMSDataContext)DataContext.DC).SysUser on partner.UserCreated equals user.Id into userPartners
                         from y in userPartners.DefaultIfEmpty()
                         join saleman in ((eFMSDataContext)DataContext.DC).SysUser on partner.SalePersonId equals saleman.Id into prods
                         from x in prods.DefaultIfEmpty()
                         select new { user = y, partner, saleman = x }
                          );
            IQueryable<CatPartnerViewModel> results = null;
            string partnerGroup = PlaceTypeEx.GetPartnerGroup(criteria.PartnerGroup);
            if (criteria.All == null)
            {
                results = query.Where(x => ((x.partner.Id ?? "").IndexOf(criteria.Id ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.ShortName ?? "").IndexOf(criteria.ShortName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.AddressVn ?? "").IndexOf(criteria.AddressVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.TaxCode ?? "").IndexOf(criteria.TaxCode ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.Tel ?? "").IndexOf(criteria.Tel ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.Fax ?? "").IndexOf(criteria.Fax ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.user.Username ?? "").IndexOf(criteria.UserCreated ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           )).Select(x => new CatPartnerViewModel {
                               Id = x.partner.Id,
                               PartnerGroup = x.partner.PartnerGroup,
                               PartnerNameVn = x.partner.PartnerNameVn,
                               PartnerNameEn = x.partner.PartnerNameEn,
                               AddressVn = x.partner.AddressVn,
                               AddressEn = x.partner.AddressEn,
                               ShortName = x.partner.ShortName,
                               CountryId = x.partner.CountryId,
                               AccountNo = x.partner.AccountNo,
                               Tel = x.partner.Tel,
                               Fax = x.partner.Fax,
                               TaxCode = x.partner.TaxCode,
                               Email = x.partner.Email,
                               Website = x.partner.Website,
                               BankAccountNo = x.partner.BankAccountNo,
                               BankAccountName = x.partner.BankAccountName,
                               BankAccountAddress = x.partner.BankAccountAddress,
                               Note = x.partner.Note,
                               SalePersonId = x.partner.SalePersonId,
                               Public = x.partner.Public,
                               CreditAmount = x.partner.CreditAmount,
                               DebitAmount = x.partner.DebitAmount,
                               RefuseEmail = x.partner.RefuseEmail,
                               ReceiveAttachedWaybill = x.partner.ReceiveAttachedWaybill,
                               RoundedSoamethod = x.partner.RoundedSoamethod,
                               TaxExemption = x.partner.TaxExemption,
                               ReceiveEtaemail = x.partner.ReceiveEtaemail,
                               ShowInDashboard = x.partner.ShowInDashboard,
                               ProvinceId = x.partner.ProvinceId,
                               ParentId = x.partner.ParentId,
                               PercentCredit = x.partner.PercentCredit,
                               AlertPercentCreditEmail = x.partner.AlertPercentCreditEmail,
                               PaymentBeneficiary = x.partner.PaymentBeneficiary,
                               UsingParrentRateCard = x.partner.UsingParrentRateCard,
                               SugarId = x.partner.SugarId,
                               BookingOverdueDay = x.partner.BookingOverdueDay,
                               FixRevenueByProject = x.partner.FixRevenueByProject,
                               UserCreated = x.partner.UserCreated,
                               DatetimeCreated = x.partner.DatetimeCreated,
                               UserModified = x.partner.UserModified,
                               DatetimeModified = x.partner.DatetimeModified,
                               Inactive = x.partner.Inactive,
                               InactiveOn = x.partner.InactiveOn,
                               UserCreatedName = x.user.Username,
                               SalePersonName = x.saleman.Username
                            }).OrderBy(x => x.AccountNo);
            }
            else
            {
                results = query.Where(x => ((x.partner.Id ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.partner.ShortName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.partner.AddressVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.partner.TaxCode ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.partner.Tel ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.partner.Fax ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.user.Username ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           ) && ((x.partner.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0))
                           .Select(x => new CatPartnerViewModel {
                               Id = x.partner.Id,
                               PartnerGroup = x.partner.PartnerGroup,
                               PartnerNameVn = x.partner.PartnerNameVn,
                               PartnerNameEn = x.partner.PartnerNameEn,
                               AddressVn = x.partner.AddressVn,
                               AddressEn = x.partner.AddressEn,
                               ShortName = x.partner.ShortName,
                               CountryId = x.partner.CountryId,
                               AccountNo = x.partner.AccountNo,
                               Tel = x.partner.Tel,
                               Fax = x.partner.Fax,
                               TaxCode = x.partner.TaxCode,
                               Email = x.partner.Email,
                               Website = x.partner.Website,
                               BankAccountNo = x.partner.BankAccountNo,
                               BankAccountName = x.partner.BankAccountName,
                               BankAccountAddress = x.partner.BankAccountAddress,
                               Note = x.partner.Note,
                               SalePersonId = x.partner.SalePersonId,
                               Public = x.partner.Public,
                               CreditAmount = x.partner.CreditAmount,
                               DebitAmount = x.partner.DebitAmount,
                               RefuseEmail = x.partner.RefuseEmail,
                               ReceiveAttachedWaybill = x.partner.ReceiveAttachedWaybill,
                               RoundedSoamethod = x.partner.RoundedSoamethod,
                               TaxExemption = x.partner.TaxExemption,
                               ReceiveEtaemail = x.partner.ReceiveEtaemail,
                               ShowInDashboard = x.partner.ShowInDashboard,
                               ProvinceId = x.partner.ProvinceId,
                               ParentId = x.partner.ParentId,
                               PercentCredit = x.partner.PercentCredit,
                               AlertPercentCreditEmail = x.partner.AlertPercentCreditEmail,
                               PaymentBeneficiary = x.partner.PaymentBeneficiary,
                               UsingParrentRateCard = x.partner.UsingParrentRateCard,
                               SugarId = x.partner.SugarId,
                               BookingOverdueDay = x.partner.BookingOverdueDay,
                               FixRevenueByProject = x.partner.FixRevenueByProject,
                               UserCreated = x.partner.UserCreated,
                               DatetimeCreated = x.partner.DatetimeCreated,
                               UserModified = x.partner.UserModified,
                               DatetimeModified = x.partner.DatetimeModified,
                               Inactive = x.partner.Inactive,
                               InactiveOn = x.partner.InactiveOn,
                               UserCreatedName = x.user.Username,
                               SalePersonName = x.saleman.Username
                           }).OrderBy(x => x.AccountNo);
            }
            return results;
            //var data = Get();
            //IQueryable<CatPartnerModel> results = null;
            //string partnerGroup = PlaceTypeEx.GetPartnerGroup(criteria.PartnerGroup);
            //if (criteria.All == null)
            //{
            //    results = data.Where(x => ((x.Id ?? "").IndexOf(criteria.Id ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               && (x.ShortName ?? "").IndexOf(criteria.ShortName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               && (x.AddressVn ?? "").IndexOf(criteria.AddressVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               && (x.TaxCode ?? "").IndexOf(criteria.TaxCode ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               && (x.Tel ?? "").IndexOf(criteria.Tel ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               && (x.Fax ?? "").IndexOf(criteria.Fax ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               && (x.UserCreated ?? "").IndexOf(criteria.UserCreated ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               && (x.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               ));
            //}
            //else
            //{
            //    results = data.Where(x => ((x.Id ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               || (x.ShortName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               || (x.AddressVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               || (x.TaxCode ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               || (x.Tel ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               || (x.Fax ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               || (x.UserCreated ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               ) && ((x.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0));
            //}
            //return results;
        }

        private Expression<Func<CatPartnerModel, bool>> GetQueryExpression(CatPartnerCriteria criteria)
        {
            Expression<Func<CatPartnerModel, bool>> query = null;
            string partnerGroup = PlaceTypeEx.GetPartnerGroup(criteria.PartnerGroup);
            if (criteria.All == null)
            {
                query = x => ((x.Id ?? "").IndexOf(criteria.Id ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.ShortName ?? "").IndexOf(criteria.ShortName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.AddressVn ?? "").IndexOf(criteria.AddressVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.TaxCode ?? "").IndexOf(criteria.TaxCode ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.Tel ?? "").IndexOf(criteria.Tel ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.Fax ?? "").IndexOf(criteria.Fax ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.UserCreated ?? "").IndexOf(criteria.UserCreated ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           );
            }
            else
            {
                query = x => ((x.Id ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.ShortName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.AddressVn ?? "").IndexOf(criteria.AddressVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.TaxCode ?? "").IndexOf(criteria.TaxCode ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.Tel ?? "").IndexOf(criteria.Tel ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.Fax ?? "").IndexOf(criteria.Fax ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.UserCreated ?? "").IndexOf(criteria.UserCreated ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           ) && ((x.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
            }
            return query;
        }
    }
}
