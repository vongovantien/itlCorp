using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Models.ReportResults;
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
        readonly IContextBase<CatCountry> countryRepo;
        public CsBookingNoteService(IStringLocalizer<LanguageSub> localizer, IMapper mapper,
            IContextBase<CsBookingNote> repository,
            IContextBase<CatPartner> catPartner,
            IContextBase<CatPlace> catPlace,
            IContextBase<SysUser> sysUser,
            ICurrentUser user,
            IContextBase<CatCountry> catCountry) : base(repository, mapper)
        {
            catPartnerRepo = catPartner;
            catPlaceRepo = catPlace;
            sysUserRepo = sysUser;
            currentUser = user;
            countryRepo = catCountry;
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

        #region PREVIEW
        public Crystal PreviewHBSeaBookingNote(Guid id)
        {
            Crystal result = null;
            var data = Get(x => x.Id == id).FirstOrDefault();
            if (data == null)
            {
                return null;
            }
            var bookingNotes = new List<HLSeaBooingNoteReport>();
            var bookingNote = new HLSeaBooingNoteReport();
            bookingNote.BookingID = data.BookingNo?.ToUpper();
            bookingNote.TransID = data.Id.ToString();
            bookingNote.LotNo = string.Empty; //NOT USE
            bookingNote.DateMaking = null; //NOT USE
            bookingNote.Revision = data.Revision?.ToUpper();
            bookingNote.Attn = data.To?.ToUpper();
            bookingNote.PartnerID = string.Empty; //NOT USE
            bookingNote.PartnerName = data.ShipperDescription?.ToUpper(); //Shipper Description
            bookingNote.Email = string.Empty; //NOT USE
            bookingNote.Address = string.Empty; //NOT USE
            bookingNote.Homephone = data.TelFrom; // Tel From
            bookingNote.Workphone = data.TelTo; // Tel To
            bookingNote.Fax = string.Empty; //NOT USE
            bookingNote.Cell = string.Empty; //NOT USE
            bookingNote.Taxcode = string.Empty; //NOT USE
            var consignee = catPartnerRepo.Get(x => x.Id == data.ConsigneeId).FirstOrDefault();
            bookingNote.ConsigneeCode = consignee?.AccountNo; //NOT USE            
            bookingNote.ConsigneeName = data.ConsigneeDescription?.ToUpper(); //Consignee Description
            bookingNote.ConsigneeAddress = consignee?.AddressEn?.ToUpper(); //NOT USE
            bookingNote.ReceiptAt = data.DateOfStuffing?.ToString("dd/MM/yyyy"); //Date Of Stuffing
            bookingNote.Deliveryat = data.PlaceOfStuffing?.ToUpper(); //Place Of Stuffing
            bookingNote.ServiceMode = data.ServiceRequired?.ToUpper(); //Service Requeired
            bookingNote.SC = data.Contact?.ToUpper(); //Contact
            
            var dataPOL = catPlaceRepo.Get(x => x.Id == data.Pol).FirstOrDefault();
            if (dataPOL != null)
            {
                var polCountry = countryRepo.Get(x => x.Id == dataPOL.CountryId).FirstOrDefault()?.NameEn;
                bookingNote.PortofLading = dataPOL?.NameEn + (!string.IsNullOrEmpty(polCountry) ? ", " + polCountry : string.Empty);
                bookingNote.PortofLading = bookingNote.PortofLading?.ToUpper();
            }
            
            var dataPOD = catPlaceRepo.Get(x => x.Id == data.Pod).FirstOrDefault();
            if (dataPOD != null)
            {
                var podCountry = countryRepo.Get(x => x.Id == dataPOD.CountryId).FirstOrDefault()?.NameEn;
                bookingNote.PortofUnlading = dataPOD?.NameEn + (!string.IsNullOrEmpty(podCountry) ? ", " + podCountry : string.Empty);
                bookingNote.PortofUnlading = bookingNote.PortofUnlading?.ToUpper();
            }

            bookingNote.ModeSea = string.Empty; //NOT USE
            bookingNote.EstimatedVessel = (data.Vessel + " / " + data.Voy)?.ToUpper(); //Vessel/ Voy
            bookingNote.LoadingDate = data.Etd;
            bookingNote.DestinationDate = data.Eta;
            bookingNote.Quantity = string.Empty; //NOT USE
            bookingNote.ContainerSize = data.NoOfContainer; //No of Container(s) or Package(s)
            bookingNote.Commidity = data.Commodity?.ToUpper();
            bookingNote.GrosWeight = data.Gw;
            bookingNote.CBMSea = data.Cbm;
            bookingNote.SpecialRequest = data.SpecialRequest;
            bookingNote.CloseTime20 = data.OtherTerms; //Other terms & conditions
            bookingNote.CloseTime40 = data.PlaceOfDelivery; //Place Of Delivery
            bookingNote.CloseTimeLCL = data.ClosingTime?.ToString("HH tt, dd MMM yyyy");
            bookingNote.PickupAt = data.PickupAt; //Pick-up at
            bookingNote.DropoffAt = data.DropoffAt; //Drop-off at
            bookingNote.ContainerNo = string.Empty; //NOT USE
            bookingNote.HBLData = data.NoOfBl; //No of B/L
            bookingNote.BlCorrection = data.FreightRate; //Freight Rate
            bookingNote.SCIACI = data.PaymentTerm; //Payment Term
            bookingNote.Remark = ReportUltity.ReplaceHtmlBaseForPreviewReport(data.Note);

            bookingNotes.Add(bookingNote);

            var parameter = new HLSeaBooingNoteReportParameter();
            parameter.ContactList = data?.From?.ToUpper() ?? string.Empty;
            parameter.CompanyName = DocumentConstants.COMPANY_NAME;
            parameter.CompanyDescription = string.Empty;
            parameter.CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1;
            parameter.CompanyAddress2 = DocumentConstants.COMPANY_CONTACT;
            parameter.Website = DocumentConstants.COMPANY_WEBSITE;
            parameter.Contact = currentUser.UserName;
            parameter.DecimalNo = 3;
            parameter.HBL = data?.HblNo ?? string.Empty;

            result = new Crystal
            {
                ReportName = "HLSeaBooingNote.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            string folderDownloadReport = CrystalEx.GetFolderDownloadReports();
            var _pathReportGenerate = folderDownloadReport + "\\HLSeaBooingNote" + DateTime.Now.ToString("ddMMyyHHssmm") + ".pdf";
            result.PathReportGenerate = _pathReportGenerate;

            result.AddDataSource(bookingNotes);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }
        #endregion PREVIEW
    }
}
