using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsBookingNoteService : RepositoryBase<CsBookingNote, CsBookingNoteModel>, ICsBookingNoteService
    {
        readonly IContextBase<CatPartner> catPartnerRepo;
        readonly IContextBase<CatPlace> catPlaceRepo;
        readonly IContextBase<SysUser> sysUserRepo;
        private readonly ICurrentUser currentUser;
        public CsBookingNoteService(IStringLocalizer<LanguageSub> localizer, IMapper mapper,
            IContextBase<CsBookingNote> repository,
            IContextBase<CatPartner> catPartner,
            IContextBase<CatPlace> catPlace,
            IContextBase<SysUser> sysUser,
            ICurrentUser user) : base(repository, mapper)
        {
            catPartnerRepo = catPartner;
            catPlaceRepo = catPlace;
            sysUserRepo = sysUser;
            currentUser = user;
        }
        #region CUD
        public HandleState UpdateCsBookingNote(CsBookingNoteEditModel model)
        {
            var bookingObj = DataContext.First(x => x.Id == model.Id);
            if (bookingObj == null)
            {
                return new HandleState("BookingNote not found !");
            }
            var booking = mapper.Map<CsBookingNote>(model);
            booking.UserModified = currentUser.UserID;
            booking.DatetimeModified = DateTime.Now;
            booking.CreatedDate = bookingObj.CreatedDate;
            booking.UserCreated = bookingObj.UserCreated;
            using (var bk = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Update(booking, x => x.Id == booking.Id);
                    bk.Commit();
                    return hs;

                }
                catch (Exception ex)
                {
                    bk.Rollback();
                    var result = new HandleState(ex.Message);
                    return result;
                }
                finally
                {
                    bk.Dispose();
                }
            }

        }

        public object AddCsBookingNote(CsBookingNoteEditModel model)
        {
            var bookingNote = mapper.Map<CsBookingNote>(model);
            bookingNote.Id = Guid.NewGuid();
            bookingNote.UserCreated = bookingNote.UserModified = currentUser.UserID;
            bookingNote.CreatedDate = bookingNote.DatetimeModified = DateTime.Now;
            using (var bk = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hsBookingNote = DataContext.Add(bookingNote);
                    var result = hsBookingNote;
                    bk.Commit();
                    return new { model = bookingNote, result };

                }
                catch (Exception ex)
                {
                    bk.Rollback();
                    var result = new HandleState(ex.Message);
                    return new { model = new object { }, result };
                }
                finally
                {
                    bk.Dispose();
                }
            }
        }

        public HandleState DeleteCsBookingNote(Guid bookingNoteId)
        {
            using (var bk = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Delete(x => x.Id == bookingNoteId);
                    if (hs.Success)
                    {
                        bk.Commit();
                    }
                    else
                    {
                        bk.Rollback();
                    }
                    return hs;
                }
                catch (Exception ex)
                {
                    bk.Rollback();
                    var result = new HandleState(ex.Message);
                    return result;
                }
                finally
                {
                    bk.Dispose();
                }
            }
        }
        #endregion

        #region LIST AND PAGING
        public List<CsBookingNoteModel> Paging(CsBookingNoteCriteria criteria, int page, int size, out int rowsCount)
        {
            var results = new List<CsBookingNoteModel>();
            var list = Query(criteria);
            if (list == null)
            {
                rowsCount = 0;
                return results;
            }
            var tempList = list;
            rowsCount = tempList.Select(s => s.Id).Count();
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                tempList = tempList.Skip((page - 1) * size).Take(size);
                results = tempList.ToList();
            }
            return results;
        }

        public IQueryable<CsBookingNoteModel> Query(CsBookingNoteCriteria criteria)
        {
            IQueryable<CsBookingNoteModel> lstBookingNotes = GetBookingNote();
            if (lstBookingNotes == null) return null;
            if (criteria.All == null)
            {
                lstBookingNotes = lstBookingNotes.Where(x => (x.BookingNo ?? "").IndexOf(criteria.BookingNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.ShipperName ?? "").IndexOf(criteria.ShipperName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.ConsigneeName ?? "").IndexOf(criteria.ConsigneeName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.POLName ?? "").IndexOf(criteria.POLName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.PODName ?? "").IndexOf(criteria.PODName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.CreatorName ?? "").IndexOf(criteria.CreatorName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && ((x.CreatedDate.HasValue && x.CreatedDate.Value.Date >= criteria.FromDate)
                           && (x.CreatedDate.Value.Date <= criteria.ToDate) || criteria.FromDate == null || criteria.ToDate == null)
              );
        
            }
            else
            {
                lstBookingNotes = lstBookingNotes.Where(x =>
                     ((x.BookingNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                     || (x.ShipperName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                     || (x.ConsigneeName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                     || (x.POLName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                     || (x.PODName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                     || (x.CreatorName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                     && ((x.CreatedDate.HasValue && x.CreatedDate.Value.Date >= criteria.FromDate) && (x.CreatedDate.Value.Date <= criteria.ToDate ) || criteria.FromDate == null || criteria.ToDate == null)
                    );
            }
            lstBookingNotes = lstBookingNotes.OrderByDescending(x => x.DatetimeModified);
            return lstBookingNotes;
        }

        private IQueryable<CsBookingNoteModel> GetBookingNote()
        {
            var lstBookingNotes = DataContext.Get();
            IQueryable<CsBookingNoteModel> query = null;
            if (lstBookingNotes == null) return null;
            var lstShipper = catPartnerRepo.Get(x => x.PartnerGroup.Contains("SHIPPER") || x.PartnerGroup.Contains("CUSTOMER"));
            var lstConsignee = catPartnerRepo.Get(x => x.PartnerGroup.Contains("CONSIGNEE"));
            var ports = catPlaceRepo.Get(x => x.PlaceTypeId == "Port");
            var creators = sysUserRepo.Get();

            query = from bookingNote in lstBookingNotes
                    join shipper in lstShipper on bookingNote.ShipperId equals shipper.Id into shipper2
                    from shipper in shipper2.DefaultIfEmpty()
                    join consignee in lstConsignee on bookingNote.ConsigneeId equals consignee.Id into consignee2
                    from consignee in consignee2.DefaultIfEmpty()
                    join pod in ports on bookingNote.Pod equals pod.Id into pod2
                    from pod in pod2.DefaultIfEmpty()
                    join pol in ports on bookingNote.Pol equals pol.Id into pol2
                    from pol in pol2.DefaultIfEmpty()
                    join creator in creators on bookingNote.UserCreated equals creator.Id into creator2
                    from creator in creator2.DefaultIfEmpty()
                    select new CsBookingNoteModel
                    {
                        Id = bookingNote.Id,
                        BookingNo = bookingNote.BookingNo,
                        ShipperName = shipper.ShortName,
                        JobId = bookingNote.JobId,
                        ConsigneeName = consignee.ShortName,
                        Etd = bookingNote.Etd,
                        Eta = bookingNote.Eta,
                        POLName = pol.NameEn,
                        PODName = pod.NameEn,
                        Gw = bookingNote.Gw,
                        Cbm = bookingNote.Cbm,
                        CreatorName = creator.Username,
                        CreatedDate = bookingNote.CreatedDate
                    };
            return query;
        }

        public CsBookingNoteModel GetDetails(Guid id)
        {
            CsBookingNoteModel detail = GetById(id);
            return detail;
        }

        private CsBookingNoteModel GetById(Guid id)
        {
            CsBookingNote data = DataContext.Get(x => x.Id == id)?.FirstOrDefault();
            if (data == null) return null;
            else
            {
                CsBookingNoteModel result = mapper.Map<CsBookingNoteModel>(data);
                if (result.Pol != null)
                {
                    CatPlace portPOLData = catPlaceRepo.Where(x => x.Id == result.Pol)?.FirstOrDefault();
                    result.POLName = portPOLData.NameEn;
                }
                if (result.Pod != null)
                {
                    CatPlace portPODData = catPlaceRepo.Where(x => x.Id == result.Pod)?.FirstOrDefault();
                    result.PODName = portPODData.NameEn;
                }

                result.CreatorName = sysUserRepo.Get(x => x.Id == result.UserCreated).FirstOrDefault()?.Username;
                result.ModifiedName = sysUserRepo.Get(x => x.Id == result.UserModified).FirstOrDefault()?.Username;

                return result;
            }
        }
        #endregion
    }
}
