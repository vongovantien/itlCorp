using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Services
{
    public class DocSendMailService : RepositoryBase<CsTransaction, CsTransactionModel>, IDocSendMailService
    {
        private readonly ICurrentUser currentUser;
        readonly IContextBase<CatPartner> catPartnerRepo;
        readonly IContextBase<CsTransactionDetail> detailRepository;
        readonly IContextBase<SysEmployee> sysEmployeeRepo;
        readonly IContextBase<SysUser> sysUserRepo;
        readonly IContextBase<CatPlace> catPlaceRepo;
        readonly IContextBase<CsShippingInstruction> csShippingInstructionRepository;
        private readonly IContextBase<CatUnit> unitRepository;
        readonly IContextBase<CsMawbcontainer> csMawbcontainerRepo;

        public DocSendMailService(IContextBase<CsTransaction> repository,
            IMapper mapper,
            ICurrentUser user,
            IContextBase<CatPartner> catPartner,
            IContextBase<CsTransactionDetail> detail,
            IContextBase<SysEmployee> sysEmployee,
            IContextBase<SysUser> sysUser,
            IContextBase<CatPlace> catPlace,
            IContextBase<CsShippingInstruction> csShippingInstruction,
            IContextBase<CatUnit> unitRepo,
            IContextBase<CsMawbcontainer> csMawbcontainer) : base(repository, mapper)
        {
            currentUser = user;
            catPartnerRepo = catPartner;
            detailRepository = detail;
            sysEmployeeRepo = sysEmployee;
            sysUserRepo = sysUser;
            catPlaceRepo = catPlace;
            csShippingInstructionRepository = csShippingInstruction;
            unitRepository = unitRepo;
            csMawbcontainerRepo = csMawbcontainer;
        }

        public bool SendMailDocument(EmailContentModel emailContent)
        {
            try
            {
                List<string> toEmails = new List<string>();
                if (!string.IsNullOrEmpty(emailContent.To))
                {
                    toEmails = emailContent.To.Split(';').Where(x => x.Trim().ToString() != string.Empty).ToList();
                }
                List<string> ccEmails = new List<string>();
                if (!string.IsNullOrEmpty(emailContent.Cc))
                {
                    ccEmails = emailContent.Cc.Split(';').Where(x => x.Trim().ToString() != string.Empty).ToList();
                }
                var sendMailResult = SendMail.Send(emailContent.Subject, emailContent.Body, toEmails, emailContent.AttachFiles, ccEmails);
                return sendMailResult;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public EmailContentModel GetInfoMailHBLAirImport(Guid hblId)
        {
            var _housebill = detailRepository.Get(x => x.Id == hblId).FirstOrDefault();
            if (_housebill == null) return null;
            var _shipment = DataContext.Get(x => x.Id == _housebill.JobId).FirstOrDefault();
            var _airlineMail = (_shipment != null) ? catPartnerRepo.Get(x => x.Id == _shipment.ColoaderId).FirstOrDefault()?.Email : string.Empty;
            var _currentUser = sysUserRepo.Get(x => x.Id == currentUser.UserID).FirstOrDefault();
            var _empCurrentUser = sysEmployeeRepo.Get(x => x.Id == _currentUser.EmployeeId).FirstOrDefault();
            var _consignee = catPartnerRepo.Get(x => x.Id == _housebill.ConsigneeId).FirstOrDefault();
            var _warehouse = catPlaceRepo.Get(x => x.PlaceTypeId == "Warehouse" && x.Id == _housebill.WarehouseId).FirstOrDefault();
            var _warehouseName = string.Empty;
            if (_warehouse != null)
            {
                if (_warehouse.Code == "TCS")
                {
                    _warehouseName = "TAN SON NHAT AIRPORT, WH: TCS";
                }
                if (_warehouse.Code == "SCSC")
                {
                    _warehouseName = "TAN SON NHAT AIRPORT, WH: SCSC";
                }
            }

            string _subject = string.Format(@"INDO TRANS LOGISTICS: ARRIVAL NOTICE // {0} // {1} // {2} (From: {3})",
                _housebill.Mawb,
                _housebill.Hwbno,
                _consignee?.PartnerNameEn,
                currentUser.UserName);
            string _body = string.Format(@"<div><b>Dear Valued Customer,</b></div><div>We would like to send <b>Arrival Notice and docs in attached file</b> for your air import shipment with details as below:</div><div><div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;MAWB#: {0}</div><div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;HAWB#: {1}</div><div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;Quantity: {2} CTNS / G.W: {3}</div><div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;Flight # / ETA: {4} / {5}</div><div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;Routing: {6}</div><div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;Warehouse: <b>{7}</b></div></div><p>Please check docs and confirm by return with thanks.</p><p>This is system auto email please do not reply it directly. Please confirm the attached files or inform us about any amendment by mail to: {8}</p>",
                _housebill.Mawb,
                _housebill.Hwbno,
                _housebill.PackageQty,
                _housebill.GrossWeight,
                _housebill.FlightNo,
                (_housebill.FlightDate != null) ? _housebill.FlightDate.Value.ToString("dd MMM, yyyy") : string.Empty,
                _housebill.Route,
                _warehouseName,
                _empCurrentUser?.Email);

            var emailContent = new EmailContentModel();
            emailContent.From = "Info FMS";
            emailContent.To = _airlineMail; //Email của Airlines
            emailContent.Cc = _empCurrentUser?.Email; //Email của Current User
            emailContent.Subject = _subject;
            emailContent.Body = _body;
            emailContent.AttachFiles = new List<string>();
            return emailContent;
        }

        public EmailContentModel GetInfoMailHBLAirExport(Guid hblId)
        {
            var _housebill = detailRepository.Get(x => x.Id == hblId).FirstOrDefault();
            if (_housebill == null) return null;
            var _currentUser = sysUserRepo.Get(x => x.Id == currentUser.UserID).FirstOrDefault();
            var _empCurrentUser = sysEmployeeRepo.Get(x => x.Id == _currentUser.EmployeeId).FirstOrDefault();
            var _pol = catPlaceRepo.Get(x => x.Id == _housebill.Pol).FirstOrDefault(); // Departure Airport
            var _pod = catPlaceRepo.Get(x => x.Id == _housebill.Pod).FirstOrDefault(); // Destination Airpor
            var _shipper = catPartnerRepo.Get(x => x.Id == _housebill.ShipperId).FirstOrDefault();
            var _consignee = catPartnerRepo.Get(x => x.Id == _housebill.ConsigneeId).FirstOrDefault();

            string _subject = string.Format(@"Pre-alert {0}/{1} {2}", _pol?.Code, _pod?.Code, _housebill.Hwbno);
            string _body = string.Format(@"<div><b>Dear Sir/Madam,</b></div><div>Enclosed pls find attd docs and confirm receipt for below Pre-Alert.</div><br/><div>MAWB : {0} (FREIGHT {1})</div><div>Flight : {2}/{3}. {4} - {5}</div><br/><div>HAWB : {6} (FREIGHT {1})</div><div>Shipper : {7}</div><div>Consignee : {8}</div><div>Notify : {9}</div><div>{10}</div><p>Attached herewith 4 pages ( HAWB, MAWB, CN & MANIFEST ) for your ref. Please check docs and confirm by return. The original Hawb#, Mawb#, and Manifest were enclosed with cargo.</p>",
                _housebill.Mawb,
                _housebill.FreightPayment,
                _housebill.FlightNo,
                (_housebill.FlightDate != null) ? _housebill.FlightDate.Value.ToString("dd MMM, yyyy") : string.Empty,
                _pol?.Code,
                _pod?.Code,
                _housebill.Hwbno,
                _shipper?.PartnerNameEn,
                _consignee?.PartnerNameEn,
                _housebill.Notify,
                _housebill.HandingInformation);

            var emailContent = new EmailContentModel();
            emailContent.From = "Info FMS";
            emailContent.To = string.Empty;
            emailContent.Cc = _empCurrentUser?.Email; //Email của Current User
            emailContent.Subject = _subject;
            emailContent.Body = _body;
            emailContent.AttachFiles = new List<string>();
            return emailContent;
        }

        #region Mail info Sea Import - Export
        // Mail Info: Sea Import
        public EmailContentModel GetInfoMailHBLSeaImport(Guid hblId, string serviceId)
        {
            var _housebill = detailRepository.Get(x => x.Id == hblId).FirstOrDefault();
            if (_housebill == null) return null;
            var _shippinglineMail = catPartnerRepo.Get(x => x.Id == _housebill.CustomerId).FirstOrDefault()?.Email;
            var _currentUser = sysUserRepo.Get(x => x.Id == currentUser.UserID).FirstOrDefault();
            var _empCurrentUser = sysEmployeeRepo.Get(x => x.Id == _currentUser.EmployeeId).FirstOrDefault();
            var _consignee = catPartnerRepo.Get(x => x.Id == _housebill.ConsigneeId).FirstOrDefault();
            var _warehouse = catPlaceRepo.Get(x => x.PlaceTypeId == "Warehouse" && x.Id == _housebill.WarehouseId).FirstOrDefault();
            var _shipper = catPartnerRepo.Get(x => x.Id == _housebill.ShipperId).FirstOrDefault();

            var pol = catPlaceRepo.Get(x => x.Id == _housebill.Pol).FirstOrDefault()?.NameEn;
            var pod = catPlaceRepo.Get(x => x.Id == _housebill.Pod).FirstOrDefault()?.NameEn;
            string _subject = string.Empty;
            switch (serviceId)
            {
                case "SFI":
                    {
                        _subject = string.Format(@"ARRIVAL NOTICE- HB/L#: {0}// {1} - {2}// {3}// {4}",
                        _housebill.Hwbno,
                         pol,
                         pod,
                        _housebill.PackageContainer,
                        _housebill.Eta == null ? string.Empty : _housebill.Eta.Value.ToString("dd MMM").ToUpper());
                    }
                    break;
                case "SLI":
                    {
                        var packageType = _housebill.PackageType == null ? string.Empty : unitRepository.Get(x => x.Id == _housebill.PackageType).FirstOrDefault()?.UnitNameEn;
                        _subject = string.Format(@"ARRIVAL NOTICE- HB/L#: {0}// {1} - {2}// {3} {4}// {5}",
                        _housebill.Hwbno,
                        pol,
                        pod,
                        _housebill.PackageQty,
                        packageType,
                        _housebill.Eta == null ? string.Empty : _housebill.Eta.Value.ToString("dd MMM").ToUpper());
                    }
                    break;
            }
            string _body = string.Format(@"<div><b>Dear Client,</b></div></br><div>Please find Arrival notice in the attachment and confirm receipt.<br></br>" +
                                        "<div><div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;Vessel : {0}</div><div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;POL: {1}</div>" +
                                        "<div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;POD: {2}</div><div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;MB/L#: {3}</div>" +
                                        "<div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;HB/L: {4}</div><div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;Shipper: {5}</div></div>" +
                                        "<div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;Cnee: {6}</div><div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;Notify: {7}</div></div>" +
                                        "<div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;ETD: {8}</div><div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;ETA: {9}</div></div>",
                _housebill.LocalVessel,
                pol,
                pod,
                _housebill.Mawb,
                _housebill.Hwbno,
                _shipper?.PartnerNameEn,
                _consignee?.PartnerNameEn,
                _housebill.NotifyPartyDescription,
                _housebill.Etd == null ? string.Empty : _housebill.Etd.Value.ToString("dd MMM").ToUpper(),
                _housebill.Eta == null ? string.Empty : _housebill.Eta.Value.ToString("dd MMM").ToUpper());

            var emailContent = new EmailContentModel();
            emailContent.From = "Info FMS";
            emailContent.To = _shippinglineMail;
            emailContent.Cc = _empCurrentUser?.Email; //Email của Current User
            emailContent.Subject = _subject;
            emailContent.Body = _body;
            emailContent.AttachFiles = new List<string>();
            return emailContent;
        }

        public EmailContentModel GetInfoMailSISeaExport(Guid jobId)
        {
            var _shipment = DataContext.Get(x => x.Id == jobId).FirstOrDefault();
            if (_shipment == null) return null;
            var _shippinglineMail = (_shipment != null) ? catPartnerRepo.Get(x => x.Id == _shipment.ColoaderId).FirstOrDefault()?.Email : string.Empty;
            var _si = csShippingInstructionRepository.Get(x => x.JobId == jobId).FirstOrDefault();
            var _currentUser = sysUserRepo.Get(x => x.Id == currentUser.UserID).FirstOrDefault();
            var _empCurrentUser = sysEmployeeRepo.Get(x => x.Id == _currentUser.EmployeeId).FirstOrDefault();

            string _subject = string.Format(@"INDO TRANS LOGISTICS - SHIPPING INSTRUCTION / {0} (From: {1})", _si?.BookingNo, currentUser.UserName);
            string _body = string.Format(@"<p>Booking number: {0}</p><p>Please kindly find Shipping Instruction(s) as in attached file. If there is any requirement, pls contact us by mailto: sea@itlvn.com</p><div>Best Regard,</div><div>{1}</div><div>Ho Chi Minh City, Vietnam</div><div>TEL : (848) 3948 6888</div>",
                _si?.BookingNo,
                _empCurrentUser?.EmployeeNameEn);

            var emailContent = new EmailContentModel();
            emailContent.From = "Info FMS";
            emailContent.To = _shippinglineMail;
            emailContent.Cc = _empCurrentUser?.Email; //Email của Current User
            emailContent.Subject = _subject;
            emailContent.Body = _body;
            emailContent.AttachFiles = new List<string>();
            return emailContent;
        }
        #endregion

        #region Mail infor Pre Alert Sea Export
        public EmailContentModel GetInfoMailHBLPreAlerSeaExport(Guid hblId, string serviceId)
        {
            var _housebill = detailRepository.Get(x => x.Id == hblId).FirstOrDefault();           
            if (_housebill == null) return null;
            var _shipment = DataContext.Get(x => x.Id == _housebill.JobId).FirstOrDefault();
            var _pol = catPlaceRepo.Get(x => x.Id == _housebill.Pol).FirstOrDefault(); // Departure Airport
            var _pod = catPlaceRepo.Get(x => x.Id == _housebill.Pod).FirstOrDefault(); // Destination Airpor
            var _saleman = sysUserRepo.Get(x => x.Id == _housebill.SaleManId).FirstOrDefault();
            var _salemanDetail = sysEmployeeRepo.Get(x => x.Id == _saleman.EmployeeId).FirstOrDefault();
            var _agentDetail = catPartnerRepo.Get(x => x.Id == _shipment.AgentId).FirstOrDefault();
            var _shipper = catPartnerRepo.Get(x => x.Id == _housebill.ShipperId).FirstOrDefault();
            var _consignee = catPartnerRepo.Get(x => x.Id == _housebill.ConsigneeId).FirstOrDefault();
            var packageType = GetUnitNameById(_housebill.PackageType);
            var csMawbcontainers = csMawbcontainerRepo.Get(x => x.Hblid == _housebill.Id);

            string _subject = string.Empty;
            string _body = string.Empty;
            if (serviceId == "SFE")
            {
                // Subject
                var etdDate = _housebill.Etd?? _shipment.Etd;
                string etd = etdDate == null ? string.Empty : etdDate.Value.ToString("dd MMM").ToUpper();
                _subject = string.Format(@"PRE ALERT –{0} – {1} // HBL: {2} // MBL: {3}// {4}// {5} // {6} // ETD: {7}",
                _pol?.NameEn, _pod?.NameEn, _housebill.Hwbno, _housebill.Mawb, _housebill.PackageContainer, _shipper?.PartnerNameEn, _consignee?.PartnerNameEn, etd);

                // Body
                string containerDetail = string.Empty;
                foreach (var con in csMawbcontainers)
                {
                    string conType = GetUnitNameById(con.ContainerTypeId);
                    string packType = GetUnitNameById(con.PackageTypeId);
                    containerDetail += string.Format("<div>{0}x{1}, CTNR/SEAL NO.: {2}/{3}/{4}, {5} {6}, G.W: {7}, CBM: {8}</div>",
                        con.Quantity, conType, con.ContainerNo, conType, con.SealNo, con.PackageQuantity, packType, con.Gw, con.Cbm);
                }

                string packageTotal = string.Join(", ", csMawbcontainers.GroupBy(x => x.PackageTypeId).Select(x => x.Sum(c => c.PackageQuantity) + " " + GetUnitNameById(x.Key)));
                string packTypeTotal = _housebill.PackageType == null ? string.Empty : unitRepository.Get(x => x.Id == _housebill.PackageType).FirstOrDefault()?.UnitNameEn;
                etd = etdDate == null ? string.Empty : etdDate.Value.ToString("dd MMM, yyyy").ToUpper();
                _body = string.Format(@"<div><b>Dear Sir/Madam,</b></div></br><div>Please find pre-alert docs in the attachment and confirm receipt.</div><br/>" +
                                        "<div>POL : {0}</div><div>POD : {1}</div><br/>" +
                                        "<div>VSL : {2}</div><div>ETD : {3}</div><br/>" +
                                        "<div>MBL : {4}({5}, {6})</div>" + containerDetail +
                                        "<div>TOTAL : {7}, {8}, G.W: {9}, CBM: {10}</div><br/>" +
                                        "<div>HBL : {11} ({12}, {13})</div>" +
                                        "<div>Shipper: {14}</div>" +
                                        "<div>Cnee: {15}</div>" +
                                        "<div>Notify: {16}</div>",
                                        _pol?.NameEn, _pod?.NameEn, _housebill.OceanVoyNo, etd, _housebill.Mawb == null ? _shipment.Mawb : _housebill.Mawb, _shipment?.Mbltype, _housebill.FreightPayment,
                                        _housebill.PackageContainer, packageTotal, _housebill.GrossWeight, _housebill.Cbm,
                                        _housebill.Hwbno, _housebill.Hbltype, _housebill.FreightPayment, _shipper.PartnerNameEn, _consignee?.PartnerNameEn, _housebill.NotifyPartyDescription);
            }
            else
            {
                // Subject
                var etdDate = _housebill.Etd?? _shipment.Etd;
                string etd = etdDate == null ? string.Empty : etdDate.Value.ToString("dd MMM").ToUpper();
                _subject = string.Format(@"PRE ALERT –{0} – {1} // HBL: {2} // MBL: {3}// {4} {5}// {6} // {7} // ETD: {8}",
                _pol?.NameEn, _pod?.NameEn, _housebill.Hwbno, _housebill.Mawb, _housebill.PackageQty, packageType, _shipper?.PartnerNameEn, _consignee?.PartnerNameEn, etd);

                // Body
                string packageDetail = string.Empty;
                foreach (var con in csMawbcontainers)
                {
                    string conType = GetUnitNameById(con.ContainerTypeId);
                    string packType = GetUnitNameById(con.PackageTypeId);
                    packageDetail += string.Format("<div>{0} {1}, G.W: {2}, CBM: {3}</div>", con.PackageQuantity, packType, con.Gw, con.Cbm);
                }

                string packageTotal = string.Join(", ", csMawbcontainers.GroupBy(x => x.PackageTypeId).Select(x => x.Sum(c => c.PackageQuantity) + " " + GetUnitNameById(x.Key)));
                etd = etdDate == null ? string.Empty : etdDate.Value.ToString("dd MMM, yyyy").ToUpper();
                _body = string.Format(@"<div><b>Dear Sir/Madam,</b></div></br><div>Please find pre-alert docs in the attachment and confirm receipt.</div><br/>" +
                                        "<div>POL : {0}</div><div>POD : {1}</div><br/>" +
                                        "<div>VSL : {2}</div><div>ETD : {3}</div><br/>" +
                                        "<div>MBL : {4}({5}, {6})</div>" + packageDetail +
                                        "<div>TOTAL : {7}, G.W: {8}, CBM: {9}</div><br/>" +
                                        "<div>HBL : {10} ({11}, {12})</div>" +
                                        "<div>Shipper: {13}</div>" +
                                        "<div>Cnee: {14}</div>" +
                                        "<div>Notify: {15}</div>",
                                        _pol?.NameEn, _pod?.NameEn, _housebill.OceanVoyNo, etd, _housebill.Mawb == null ? _shipment.Mawb : _housebill.Mawb, _shipment?.Mbltype, _housebill.FreightPayment,
                                        packageTotal, _housebill.GrossWeight, _housebill.Cbm,
                                        _housebill.Hwbno, _housebill.Hbltype, _housebill.FreightPayment, _shipper.PartnerNameEn, _consignee?.PartnerNameEn, _housebill.NotifyPartyDescription);
            }


            var emailContent = new EmailContentModel();
            emailContent.From = "Info FMS";
            emailContent.To = _agentDetail?.Email;
            emailContent.Cc = _salemanDetail?.Email; //Email của Current User
            emailContent.Subject = _subject;
            emailContent.Body = _body;
            emailContent.AttachFiles = new List<string>();
            return emailContent;
        }
        private string GetUnitNameById(short? id)
        {
            var result = string.Empty;
            var data = unitRepository.Get(g => g.Id == id).FirstOrDefault();
            result = (data != null) ? data.UnitNameEn : string.Empty;
            return result;
        }
        #endregion
    }
}
