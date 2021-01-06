using AutoMapper;
using AutoMapper.QueryableExtensions;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Linq;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatPartnerEmailService : RepositoryBase<CatPartnerEmail, CatPartnerEmailModel>, ICatPartnerEmailService
    {

        private ICurrentUser currentUser;
        private readonly IContextBase<CatPartner> catPartnerRepository;
        private readonly IContextBase<SysUser> sysUserRepository;

        public CatPartnerEmailService(IContextBase<CatPartnerEmail> repository,
            IMapper mapper,
            IContextBase<CatPartner> catPartnerRepo,
            IContextBase<SysUser> userRepo,
            ICurrentUser currtUser) : base(repository, mapper)
        {
            currentUser = currtUser;
            catPartnerRepository = catPartnerRepo;
            sysUserRepository = userRepo;
        }
        #region List
        public IQueryable<CatPartnerEmailModel> GetBy(string partnerId)
        {
            var data = DataContext.Get(x => x.PartnerId == partnerId);
            if (data == null) return null;
            var results = data.ProjectTo<CatPartnerEmailModel>(mapper.ConfigurationProvider).ToList();
            results.ForEach(x => x.UserModfiedName = sysUserRepository.Get(y => y.Id == x.UserModified).Select(t => t.Username).FirstOrDefault());
            return results?.AsQueryable();
        }

        #endregion

        #region CRUD
        public CatPartnerEmailModel GetDetail(Guid id)
        {
            CatPartnerEmailModel queryDetail = Get(x => x.Id == id).FirstOrDefault();
            if (queryDetail == null) return null;
            queryDetail.UserCreatedName = sysUserRepository.Get(x => x.Id == queryDetail.UserCreated).Select(t => t.Username).FirstOrDefault();
            queryDetail.UserModfiedName = sysUserRepository.Get(x => x.Id == queryDetail.UserModified).Select(t => t.Username).FirstOrDefault();
            return queryDetail;
        }
        public HandleState AddEmail(CatPartnerEmailModel model)
        {
            model.UserModified = model.UserCreated = currentUser.UserID;
            model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Add(model);
                    if (hs.Success)
                    {
                        trans.Commit();
                    }
                    return hs;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    var result = new HandleState(ex.Message);
                    return result;
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        public HandleState Delete(Guid id)
        {
            var hs = DataContext.Delete(x => x.Id == id);
            return hs;
        }

        public HandleState Update(CatPartnerEmail model)
        {
            model.UserModified = currentUser.UserID;
            model.DatetimeModified = DateTime.Now;
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Update(model,x=>x.Id == model.Id);
                    if (hs.Success)
                    {
                        trans.Commit();
                    }
                    return hs;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    var result = new HandleState(ex.Message);
                    return result;
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }
        #endregion



    }

}
