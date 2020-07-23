using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Models;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatIncotermService : RepositoryBase<CatIncoterm, CatIncotermModel>, ICatIncotermService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CatChargeIncoterm> catChargeIncotermRepository;

        public CatIncotermService(IContextBase<CatIncoterm> repository,
            IMapper mapper,
            IStringLocalizer<CatalogueLanguageSub> localizer,
            IContextBase<CatChargeIncoterm> catChargeIncotermRepo,
            ICurrentUser curUser
            ) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            currentUser = curUser;
            catChargeIncotermRepository = catChargeIncotermRepo;
        }

        public HandleState AddNew(CatIncotermEditModel model)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {

                model.Incoterm.Id = Guid.NewGuid();
                model.Incoterm.UserCreated = model.Incoterm.UserModified = currentUser.UserID;
                model.Incoterm.DatetimeCreated = DateTime.Now;
                model.Incoterm.GroupId = currentUser.GroupId;
                model.Incoterm.DepartmentId = currentUser.DepartmentId;
                model.Incoterm.OfficeId = currentUser.OfficeID;
                model.Incoterm.CompanyId = currentUser.CompanyID;
                try
                {
                    HandleState hs = DataContext.Add(model.Incoterm, false);
                    if(hs.Success)
                    {
                        List<CatChargeIncoterm> listCharge = new List<CatChargeIncoterm>();

                        if (model.buyings.Count() > 0 )
                        {
                            listCharge.AddRange(model.buyings);
                        }
                        if (model.sellings.Count() > 0)
                        {
                           listCharge.AddRange(model.sellings);
                        }

                        if(listCharge.Count() > 0)
                        {
                            foreach (CatChargeIncoterm item in listCharge)
                            {
                                item.Id = Guid.NewGuid();
                                item.IncotermId = model.Incoterm.Id;
                                item.UserCreated = item.UserModified = currentUser.UserID;
                                item.DatetimeCreated = item.DatetimeModified = DateTime.Now;
                                catChargeIncotermRepository.Add(item, false);
                            }
                        }
                        DataContext.SubmitChanges();
                        catChargeIncotermRepository.SubmitChanges();

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

        public HandleState Update(CatIncotermEditModel model)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    CatIncoterm catIncoterm = DataContext.Get(x => x.Id == model.Incoterm.Id)?.FirstOrDefault();
                    HandleState hs = null;
                    if(catIncoterm == null)
                    {
                        hs = new HandleState("Not found incoterm");
                    }
                    model.Incoterm.GroupId = catIncoterm.GroupId;
                    model.Incoterm.DepartmentId = catIncoterm.DepartmentId;
                    model.Incoterm.OfficeId = catIncoterm.OfficeId;
                    model.Incoterm.CompanyId = catIncoterm.CompanyId;

                    model.Incoterm.UserModified = currentUser.UserID;
                    model.Incoterm.DatetimeModified = DateTime.Now;

                    hs = DataContext.Update(model.Incoterm, x => x.Id == model.Incoterm.Id, false);
                    if (hs.Success == false)
                    {
                        return hs;
                    }

                    List<CatChargeIncoterm> listCharge = new List<CatChargeIncoterm>();

                    if (model.buyings.Count() > 0)
                    {
                        listCharge.AddRange(model.buyings);
                    }
                    if (model.sellings.Count() > 0)
                    {
                        listCharge.AddRange(model.sellings);
                    }

                    if (listCharge.Count() > 0)
                    {
                        List<Guid> lstCatChargeIncotermNeedRemove = catChargeIncotermRepository.Where(x => x.IncotermId == model.Incoterm.Id).Select(t => t.Id).ToList();

                        foreach (Guid id in lstCatChargeIncotermNeedRemove)
                        {
                            catChargeIncotermRepository.Delete(x => x.Id == id, false);
                        }

                        foreach (CatChargeIncoterm item in listCharge)
                        {
                            item.Id = Guid.NewGuid();
                            item.IncotermId = model.Incoterm.Id;
                            item.UserCreated = item.UserModified = currentUser.UserID;
                            item.DatetimeCreated = item.DatetimeModified = DateTime.Now;

                            catChargeIncotermRepository.Add(item, false);
                        }
                    }
                    else
                    {
                        catChargeIncotermRepository.Delete(x => x.IncotermId == model.Incoterm.Id, false);
                    }
                    DataContext.SubmitChanges();
                    catChargeIncotermRepository.SubmitChanges();
                    trans.Commit();
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
        public bool CheckAllowDelete(Guid id)
        {
            throw new NotImplementedException();
        }

        public bool CheckAllowViewDetail(Guid id)
        {
            throw new NotImplementedException();
        }

        public HandleState Delete(Guid Id)
        {
            throw new NotImplementedException();
        }

        public CatChartOfAccountsModel GetDetail(Guid id)
        {

            throw new NotImplementedException();
        }

        public IQueryable<CatIncoterm> Paging(CatIncotermCriteria criteria, int page, int size, out int rowsCount)
        {
            throw new NotImplementedException();
        }

        public IQueryable<CatIncoterm> Query(CatIncotermCriteria criteria)
        {
            throw new NotImplementedException();
        }

        
    }
}
