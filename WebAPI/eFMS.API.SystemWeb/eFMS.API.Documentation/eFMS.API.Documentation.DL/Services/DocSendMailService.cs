using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Helpers;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
        private readonly IContextBase<SysSentEmailHistory> sentEmailHistoryRepo;
        private readonly IContextBase<SysEmailTemplate> sysEmailTemplateRepo;
        private readonly IContextBase<CatDepartment> catDepartmentRepo;
        private readonly IContextBase<CatIncoterm> catIncotermRepo;
        private readonly IContextBase<CsShipmentSurcharge> suchargeRepo;
        private readonly IContextBase<AcctCdnote> acctCdnoteRepo;
        private readonly IContextBase<SysGroup> sysGroupRepo;
        private readonly IOptions<ApiServiceUrl> apiServiceUrl;

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
            IContextBase<CsMawbcontainer> csMawbcontainer,
            IContextBase<SysSentEmailHistory> sentEmailHistory,
            IContextBase<SysEmailTemplate> sysEmailTemplate,
            IContextBase<CatDepartment> catDepartment,
            IContextBase<CatIncoterm> catIncoterm,
            IContextBase<CsShipmentSurcharge> sucharge,
            IContextBase<AcctCdnote> acctCdnote,
            IContextBase<SysGroup> sysGroup,
            IOptions<ApiServiceUrl> serviceUrl) : base(repository, mapper)
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
            sentEmailHistoryRepo = sentEmailHistory;
            apiServiceUrl = serviceUrl;
            sysEmailTemplateRepo = sysEmailTemplate;
            catDepartmentRepo = catDepartment;
            catIncotermRepo = catIncoterm;
            suchargeRepo = sucharge;
            acctCdnoteRepo = acctCdnote;
            sysGroupRepo = sysGroup;
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

                if (SendMail.IsValidEmail(emailContent.From))
                {
                    SendMail._emailFrom = emailContent.From;
                }
                var sendMailResult = SendMail.Send(emailContent.Subject, emailContent.Body, toEmails, emailContent.AttachFiles, ccEmails);
                
                #region --- Ghi Log Send Mail ---
                var logSendMail = new SysSentEmailHistory
                {
                    SentUser = SendMail._emailFrom,
                    Receivers = string.Join("; ", toEmails),
                    Ccs = string.Join("; ", ccEmails),
                    Subject = emailContent.Subject,
                    Sent = sendMailResult,
                    SentDateTime = DateTime.Now,
                    Body = emailContent.Body
                };
                var hsLogSendMail = sentEmailHistoryRepo.Add(logSendMail);
                var hsSm = sentEmailHistoryRepo.SubmitChanges();
                #endregion --- Ghi Log Send Mail ---

                return sendMailResult;
            }
            catch (Exception ex)
            {
                new LogHelper("LOG_SEND_MAIl", ex.ToString());
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

            #region delete Old
            //string _subject = string.Format(@"INDO TRANS LOGISTICS: ARRIVAL NOTICE // {0} // {1} // {2} (From: {3})",
            //    _housebill.Mawb,
            //    _housebill.Hwbno,
            //    _consignee?.PartnerNameEn,
            //    currentUser.UserName);
            //string _body = string.Format(@"<div><b>Dear Valued Customer,</b></div><div>We would like to send <b>Arrival Notice and docs in attached file</b> for your air import shipment with details as below:</div><div><div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;MAWB#: {0}</div><div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;HAWB#: {1}</div><div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;Quantity: {2} CTNS / G.W: {3}</div><div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;Flight # / ETA: {4} / {5}</div><div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;Routing: {6}</div><div>&nbsp;&nbsp;&nbsp;&nbsp;-&nbsp;&nbsp;&nbsp;Warehouse: <b>{7}</b></div></div><p>Please check docs and confirm by return with thanks.</p><p>This is system auto email please do not reply it directly. Please confirm the attached files or inform us about any amendment by mail to: {8}</p>",
            //    _housebill.Mawb,
            //    _housebill.Hwbno,
            //    _housebill.PackageQty,
            //    _housebill.GrossWeight,
            //    _housebill.FlightNo,
            //    (_housebill.FlightDate != null) ? _housebill.FlightDate.Value.ToString("dd MMM, yyyy") : string.Empty,
            //    _housebill.Route,
            //    _warehouseName,
            //    _empCurrentUser?.Email);
            #endregion

            // Email PIC
            var _picId = !string.IsNullOrEmpty(_shipment.PersonIncharge) ? sysUserRepo.Get(x => x.Id.ToString() == _shipment.PersonIncharge).FirstOrDefault()?.EmployeeId : string.Empty;
            var picEmail = sysEmployeeRepo.Get(x => x.Id == _picId).FirstOrDefault()?.Email; //Email from

            var templateEmail = sysEmailTemplateRepo.Get(x => x.Code == "AI-ARRIVAL-NOTICE").FirstOrDefault();
            string _subject = templateEmail.Subject;
            _subject = _subject.Replace("{{MAWB}}", _housebill.Mawb);
            _subject = _subject.Replace("{{HAWB}}", _housebill.Hwbno);
            _subject = _subject.Replace("{{Consignee}}", _consignee?.PartnerNameEn);
            _subject = _subject.Replace("{{UserName}}", currentUser.UserName);

            var debitNo = suchargeRepo.Get(x => x.Hblid == hblId && !string.IsNullOrEmpty(x.DebitNo)).Select(x=>x.DebitNo).FirstOrDefault();
            decimal? exchangeRate = null;
            if(!string.IsNullOrEmpty(debitNo))
            {
                exchangeRate = acctCdnoteRepo.First(x => x.Code == debitNo && x.Type.ToLower() != "credit")?.ExchangeRate;
            }
            string _body = templateEmail.Body;
            _body = _body.Replace("{{MAWB}}", _housebill.Mawb);
            _body = _body.Replace("{{HAWB}}", _housebill.Hwbno);
            _body = _body.Replace("{{QTy}}", _housebill.PackageQty?.ToString());
            _body = _body.Replace("{{GW}}", string.Format("{0:n2}", _housebill.GrossWeight));
            _body = _body.Replace("{{FlightNo}}", _housebill.FlightNo);
            _body = _body.Replace("{{ATA}}", (_housebill.FlightDate != null) ? _housebill.FlightDate.Value.ToString("dd MMM, yyyy") : string.Empty);
            _body = _body.Replace("{{Routing}}", _shipment.Route);
            _body = _body.Replace("{{WareHouse}}", _warehouseName);
            _body = _body.Replace("{{ExcRate}}", exchangeRate == null ? string.Empty : string.Format("{0:n2}", exchangeRate));
            _body = _body.Replace("{{pic}}", picEmail);

            var emailContent = new EmailContentModel();
            // Email to: agent/customer
            var partnerInfo = catPartnerRepo.Get(x => x.Id == _shipment.AgentId).FirstOrDefault()?.Email; //Email to
            if (string.IsNullOrEmpty(partnerInfo))
            {
                partnerInfo = catPartnerRepo.Get(x => x.Id == _housebill.CustomerId).FirstOrDefault()?.Email;
            }

            // Group email của PIC
            var groupUser = sysGroupRepo.Get(x => x.Id == _shipment.GroupId).FirstOrDefault();
            var mailFrom = "Info FMS";
            if (!string.IsNullOrEmpty(picEmail))
            {
                mailFrom = picEmail;
            }
            else
            {
                mailFrom = @"air@itlvn.com";
            }
            emailContent.From = mailFrom; //email PIC của lô hàng
            emailContent.To = string.IsNullOrEmpty(partnerInfo) ? string.Empty : partnerInfo; //Email của Customer/Agent
            emailContent.Cc = "fin-inv.fm@itlvn.com;" + groupUser?.Email; // fin-inv.fm@itlvn.com và Group email của PIC trên Lô hàng
            emailContent.Subject = _subject;
            emailContent.Body = _body;
            emailContent.AttachFiles = new List<string>();
            return emailContent;
        }

        public EmailContentModel GetInfoMailHBLAirExport(Guid? hblId)
        {
            var _housebill = detailRepository.Get(x => x.Id == hblId).FirstOrDefault();
            if (_housebill == null) return null;
            var shipmentInfo = DataContext.Get(x => x.Id == _housebill.JobId).FirstOrDefault();
            //var _currentUser = sysUserRepo.Get(x => x.Id == currentUser.UserID).FirstOrDefault();
            //var _empCurrentUser = sysEmployeeRepo.Get(x => x.Id == _currentUser.EmployeeId).FirstOrDefault();
            var _pol = catPlaceRepo.Get(x => x.Id == _housebill.Pol).FirstOrDefault()?.Code; // Departure Airport
            _pol = string.IsNullOrEmpty(_pol) ? string.Empty : _pol;
            var _pod = catPlaceRepo.Get(x => x.Id == _housebill.Pod).FirstOrDefault()?.Code; // Destination Airpor
            var polPod = (!string.IsNullOrEmpty(_pol) && !string.IsNullOrEmpty(_pod)) ? string.Format("{0}-{1}", _pol, _pod) : (_pol + _pod);

            var _shipper = catPartnerRepo.Get(x => x.Id == _housebill.ShipperId).FirstOrDefault();
            var _consignee = catPartnerRepo.Get(x => x.Id == _housebill.ConsigneeId).FirstOrDefault();
            var incoterm = catIncotermRepo.Get(x => x.Id == _housebill.IncotermId).FirstOrDefault()?.Code;

            var flightNo = string.IsNullOrEmpty(_housebill.FlightNo) ? string.Empty : _housebill.FlightNo;
            flightNo = string.IsNullOrEmpty(flightNo) ? string.Empty : ("/ " + flightNo);
            var remark = string.IsNullOrEmpty(_housebill.Remark) ? _housebill.ShippingMark : _housebill.Remark;

            // Email PIC
            var _picId = !string.IsNullOrEmpty(shipmentInfo.PersonIncharge) ? sysUserRepo.Get(x => x.Id.ToString() == shipmentInfo.PersonIncharge).FirstOrDefault()?.EmployeeId : string.Empty;
            var picEmail = sysEmployeeRepo.Get(x => x.Id == _picId).FirstOrDefault()?.Email; //Email from

            //string _subject = string.Format(@"Pre-alert {0}/{1} {2}", _pol?.Code, _pod?.Code, _housebill.Hwbno);
            #region Old-Template
            //string _subject = string.Format(@"PRE-ALERT{0} {1} / {2} / {3} {4} {5}", string.IsNullOrEmpty(incoterm) ? string.Empty : ("/ " + incoterm), polPod, mawb, _housebill.Hwbno, flightNo,
            //    string.IsNullOrEmpty(remark) ? string.Empty : ("/ " + remark.Replace("\n"," ")));
            //string _body = string.Format(@"<div><b>Dear Sir/Madam,</b></div><div>Please find attd docs and confirm receipt for below Pre-Alert.</div><br/>
            //                                <div>{0}</div>
            //                                <div>MAWB : {1}</div>
            //                                <div>Flight : {2}/{3}</div>
            //                                <div>HAWB : {4} (FREIGHT {5})</div>
            //                                <div>Shipper : {6}</div>
            //                                <div>Consignee : {7}</div>
            //                                <div>Quantity : {8} // {9} // {10}</div>
            //                                <div>Term : {11}</div>
            //                                <div>Com : </div>
            //                                <div>Remark : {12}</div>
            //                                <div>Attached here with HAWB, Cargo Manifest, MAWB, INV & PKL for your ref. Please check docs and confirm by return. The original {4}, {1}, and Manifest were enclosed with cargo.</div>
            //                                <p>Pls inform us when the cnee pickup the shipment.</p>",
            //    (!string.IsNullOrEmpty(_pol) && !string.IsNullOrEmpty(_pod)) ? string.Format("{0} - {1}", _pol, _pod) : (_pol + _pod),
            //    mawb,
            //    _housebill.FlightNo,
            //    (_housebill.Etd != null) ? _housebill.Etd.Value.ToString("dd MMM, yyyy") : string.Empty,
            //    _housebill.Hwbno,
            //    _housebill.FreightPayment,
            //    _shipper?.PartnerNameEn,
            //    _consignee?.PartnerNameEn,
            //    _housebill.PackageQty,
            //    string.Format("{0:n2}", _housebill.GrossWeight),
            //    string.Format("{0:n2}", _housebill.ChargeWeight),
            //    incoterm,
            //    remark);
            #endregion        

            var template = sysEmailTemplateRepo.Get(x => x.Code == "AE-PRE-ALERT").FirstOrDefault();
            var _subject = template.Subject;
            _subject = _subject.Replace("{{Incoterm}}", string.IsNullOrEmpty(incoterm) ? string.Empty : ("/ " + incoterm));
            _subject = _subject.Replace("{{PolPod}}", string.IsNullOrEmpty(polPod) ? string.Empty : ("/ " + polPod));
            _subject = _subject.Replace("{{Mawb}}", string.IsNullOrEmpty(_housebill.Mawb) ? string.Empty : ("/ " + _housebill.Mawb));
            _subject = _subject.Replace("{{Hwbno}}", string.IsNullOrEmpty(_housebill.Hwbno) ? string.Empty : ("/ " + _housebill.Hwbno));
            _subject = _subject.Replace("{{Flight}}", string.IsNullOrEmpty(flightNo) ? string.Empty : ("/ " + flightNo));
            _subject = _subject.Replace("{{PO}}", string.Empty);

            var _body = template.Body;
            _body = _body.Replace("{{PolPod}}", polPod);
            var _content = template.Content;
            _content = _content.Replace("{{NumOrder}}", string.Empty);
            _content = _content.Replace("{{Mawb}}", _housebill.Mawb);
            _content = _content.Replace("{{Flight}}", _housebill.FlightNo);
            _content = _content.Replace("{{ETD}}", (_housebill.Etd != null) ? _housebill.Etd.Value.ToString("dd MMM, yyyy") : string.Empty);
            _content = _content.Replace("{{Hwbno}}", _housebill.Hwbno);
            _content = _content.Replace("{{FreightPayment}}", _housebill.FreightPayment);
            _content = _content.Replace("{{Shipper}}", _shipper?.PartnerNameEn);
            _content = _content.Replace("{{Consignee}}", _consignee?.PartnerNameEn);
            _content = _content.Replace("{{Qty}}", _housebill.PackageQty?.ToString() + _housebill.KgIb);
            _content = _content.Replace("{{GW}}", string.Format("{0:n2}", _housebill.GrossWeight) + "(KGS)");
            _content = _content.Replace("{{CW}}", string.Format("{0:n2}", _housebill.ChargeWeight) + "(KGS)");
            _content = _content.Replace("{{Incoterm}}", incoterm);
            _content = _content.Replace("{{NQGoods}}", _housebill.DesOfGoods);

            _body = _body.Replace("{{Content}}", _content);
            _body = _body.Replace("{{PO}}", string.Empty);
            _body = _body.Replace("{{Hwbno}}", _housebill.Hwbno);
            _body = _body.Replace("{{Mawb}}", _housebill.Mawb);
            _body = _body.Replace("{{EmailPic}}", picEmail);

            // Get email from of person in charge
            var groupUser = sysGroupRepo.Get(x => x.Id == shipmentInfo.GroupId).FirstOrDefault();
            var partnerInfo = catPartnerRepo.Get(x => x.Id == shipmentInfo.AgentId).FirstOrDefault()?.Email; //Email to
            if (string.IsNullOrEmpty(partnerInfo))
            {
                partnerInfo = catPartnerRepo.Get(x => x.Id == _housebill.CustomerId).FirstOrDefault()?.Email;
            }

            var emailContent = new EmailContentModel();
            var mailFrom = string.IsNullOrEmpty(picEmail) ? "Info FMS" : picEmail;
            emailContent.From = mailFrom;
            emailContent.To = string.IsNullOrEmpty(partnerInfo) ? string.Empty : partnerInfo;
            emailContent.Cc = groupUser?.Email; // @"air@itlvn.com";
            emailContent.Subject = _subject;
            emailContent.Body = _body;
            emailContent.AttachFiles = new List<string>();
            return emailContent;
        }

        public EmailContentModel GetInfoMailAEPreAlert(Guid? jobId)
        {
            var shipmentInfo = DataContext.First(x => x.Id == jobId);
            if (shipmentInfo == null) return null;

            var _housebills = detailRepository.Get(x => x.JobId == jobId);
            var _pol = catPlaceRepo.Get(x => x.Id == shipmentInfo.Pol).FirstOrDefault()?.Code; // Departure Airport
            _pol = string.IsNullOrEmpty(_pol) ? string.Empty : _pol;
            var _pod = catPlaceRepo.Get(x => x.Id == shipmentInfo.Pod).FirstOrDefault()?.Code; // Destination Airpor
            var polPod = (!string.IsNullOrEmpty(_pol) && !string.IsNullOrEmpty(_pod)) ? string.Format("{0}-{1}", _pol, _pod) : (_pol + _pod);
            var incoterm = catIncotermRepo.Get(x => x.Id == shipmentInfo.IncotermId).FirstOrDefault()?.Code;

            var hwbNos = string.Join(" - ", _housebills.Select(x => x.Hwbno).Distinct());
            var mawb = !string.IsNullOrEmpty(shipmentInfo.Mawb) ? shipmentInfo.Mawb : string.Join(";", _housebills.Select(x => x.Mawb).Distinct());

            // Email PIC
            var _picId = !string.IsNullOrEmpty(shipmentInfo.PersonIncharge) ? sysUserRepo.Get(x => x.Id.ToString() == shipmentInfo.PersonIncharge).FirstOrDefault()?.EmployeeId : string.Empty;
            var picEmail = sysEmployeeRepo.Get(x => x.Id == _picId).FirstOrDefault()?.Email; //Email from

            var template = sysEmailTemplateRepo.Get(x => x.Code == "AE-PRE-ALERT").FirstOrDefault();
            var _subject = template.Subject;
            _subject = _subject.Replace("{{Incoterm}}", string.IsNullOrEmpty(incoterm) ? string.Empty : ("/ " + incoterm));
            _subject = _subject.Replace("{{PolPod}}", string.IsNullOrEmpty(polPod) ? string.Empty : ("/ " + polPod));
            _subject = _subject.Replace("{{Mawb}}", string.IsNullOrEmpty(shipmentInfo.Mawb) ? string.Empty : ("/ " + shipmentInfo.Mawb));
            _subject = _subject.Replace("{{Hwbno}}", string.IsNullOrEmpty(hwbNos) ? string.Empty : ("/ " + hwbNos));
            _subject = _subject.Replace("{{Flight}}", string.IsNullOrEmpty(shipmentInfo.FlightVesselName) ? string.Empty : ("/ " + shipmentInfo.FlightVesselName));
            _subject = _subject.Replace("{{PO}}", string.Empty);

            var _body = template.Body;
            _body = _body.Replace("{{PolPod}}", polPod);

            var numOrder = 1;
            var contenEmail = string.Empty;
            foreach (var _hbl in _housebills)
            {
                var _content = template.Content;
                var _shipper = catPartnerRepo.Get(x => x.Id == _hbl.ShipperId).FirstOrDefault();
                var _consignee = catPartnerRepo.Get(x => x.Id == _hbl.ConsigneeId).FirstOrDefault();
                var _incoterm = catIncotermRepo.Get(x => x.Id == _hbl.IncotermId).FirstOrDefault()?.Code;
                _content = _content.Replace("{{NumOrder}}", numOrder + ". ");
                _content = _content.Replace("{{Mawb}}", _hbl.Mawb);
                _content = _content.Replace("{{Flight}}", _hbl.FlightNo);
                _content = _content.Replace("{{ETD}}", (_hbl.Etd != null) ? _hbl.Etd.Value.ToString("dd MMM, yyyy") : string.Empty);
                _content = _content.Replace("{{Hwbno}}", _hbl.Hwbno);
                _content = _content.Replace("{{FreightPayment}}", _hbl.FreightPayment);
                _content = _content.Replace("{{Shipper}}", _shipper?.PartnerNameEn);
                _content = _content.Replace("{{Consignee}}", _consignee?.PartnerNameEn);
                _content = _content.Replace("{{Qty}}", _hbl.PackageQty?.ToString() +_hbl.KgIb);
                _content = _content.Replace("{{GW}}", string.Format("{0:n2}", _hbl.GrossWeight) + "(KGS)");
                _content = _content.Replace("{{CW}}", string.Format("{0:n2}", _hbl.ChargeWeight) + "(KGS)");
                _content = _content.Replace("{{Incoterm}}", _incoterm);
                
                var _desOfGoodArrs = string.IsNullOrEmpty(_hbl.DesOfGoods) ? null : _hbl.DesOfGoods.Split("\n").Where(x => !string.IsNullOrEmpty(x));
                var _desOfGood = string.Empty;
                if (_desOfGoodArrs != null && _desOfGoodArrs.Count() > 0)
                {
                    _desOfGood += _desOfGoodArrs.First();
                    _desOfGoodArrs = _desOfGoodArrs.Skip(1);
                    foreach (var item in _desOfGoodArrs)
                    {
                        _desOfGood += string.Format("<div style=\"margin-left: 10px;\">{0}</div>", item);
                    }
                }
                _content = _content.Replace("{{NQGoods}}", _desOfGood);
                contenEmail += _content;
                numOrder += 1;
            }

            if (_housebills == null || _housebills.Count() == 0)
            {
                var _content = template.Content;
                _content = _content.Replace("{{NumOrder}}", numOrder + ". ");
                _content = _content.Replace("{{Mawb}}", string.Empty);
                _content = _content.Replace("{{Flight}}", string.Empty);
                _content = _content.Replace("{{ETD}}", string.Empty);
                _content = _content.Replace("{{Hwbno}}", string.Empty);
                _content = _content.Replace("{{FreightPayment}}", string.Empty);
                _content = _content.Replace("{{Shipper}}", string.Empty);
                _content = _content.Replace("{{Consignee}}", string.Empty);
                _content = _content.Replace("{{Qty}}", string.Empty);
                _content = _content.Replace("{{GW}}", string.Empty + "(KGS)");
                _content = _content.Replace("{{CW}}", string.Empty + "(KGS)");
                _content = _content.Replace("{{Incoterm}}", string.Empty);
                _content = _content.Replace("{{NQGoods}}", string.Empty);
                contenEmail += _content;
            }

            _body = _body.Replace("{{Content}}", contenEmail);
            _body = _body.Replace("{{PO}}", string.Empty);
            _body = _body.Replace("{{Hwbno}}", hwbNos);
            _body = _body.Replace("{{Mawb}}", mawb);
            _body = _body.Replace("{{EmailPic}}", picEmail);

            // Get email from of person in charge
            var groupUser = sysGroupRepo.Get(x => x.Id == shipmentInfo.GroupId).FirstOrDefault();

            // Get email from of person in charge
            var partnerInfo = catPartnerRepo.Get(x => x.Id == shipmentInfo.AgentId).FirstOrDefault()?.Email; //Email to
            if (string.IsNullOrEmpty(partnerInfo))
            {
                var customers = _housebills.Select(x => x.CustomerId).Distinct().ToList();
                partnerInfo = string.Join(";", catPartnerRepo.Get(x => customers.Any(z => z == x.Id)).Select(x => x.Email));
            }

            var emailContent = new EmailContentModel();
            var mailFrom = string.IsNullOrEmpty(picEmail) ? "Info FMS" : picEmail;
            emailContent.From = mailFrom;
            emailContent.To = string.IsNullOrEmpty(partnerInfo) ? string.Empty : partnerInfo;
            emailContent.Cc = groupUser?.Email; // @"air@itlvn.com";
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
            var pod = _housebill.FinalDestinationPlace; // catPlaceRepo.Get(x => x.Id == _housebill.Pod).FirstOrDefault()?.NameEn; => Final destination
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
            // Email PIC
            var _shipment = DataContext.First(x => x.Id == _housebill.JobId);
            var _picId = !string.IsNullOrEmpty(_shipment.PersonIncharge) ? sysUserRepo.Get(x => x.Id.ToString() == _shipment.PersonIncharge).FirstOrDefault()?.EmployeeId : string.Empty;
            var picEmail = sysEmployeeRepo.Get(x => x.Id == _picId).FirstOrDefault()?.Email; //Email from

            // Email to: agent/customer + consignee
            var mailTo = string.Empty;
            var partnerEmail = catPartnerRepo.Get(x => x.Id == _shipment.AgentId).FirstOrDefault()?.Email; //Email to
            if (string.IsNullOrEmpty(partnerEmail))
            {
                partnerEmail = catPartnerRepo.Get(x => x.Id == _housebill.CustomerId).FirstOrDefault()?.Email;
            }
            mailTo += partnerEmail;
            var emailConsignee = catPartnerRepo.Get(x => x.Id == _housebill.ConsigneeId).FirstOrDefault()?.Email;
            mailTo += ";" + emailConsignee;

            // Get email from of person in charge
            var groupUser = sysGroupRepo.Get(x => x.Id == _shipment.GroupId).FirstOrDefault();
            var mailFrom = "Info FMS";
            if (!string.IsNullOrEmpty(picEmail))
            {
                mailFrom = picEmail;
            }
            else
            {
                mailFrom = @"sea@itlvn.com";
            }

            emailContent.From = mailFrom; //email PIC của lô hàng
            emailContent.To = string.IsNullOrEmpty(mailTo) ? string.Empty : mailTo; //Email của Customer/Agent
            emailContent.Cc = groupUser?.Email; // Group Mail của pic trên Lô hàng
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
        public EmailContentModel GetInfoMailHBLPreAlerSeaExport(Guid? hblId, string serviceId)
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

            var emailTemplate = sysEmailTemplateRepo.Get(x => x.Code == "SE-PRE-ALERT").FirstOrDefault();
            string _subject = emailTemplate.Subject;
            string _body = emailTemplate.Body;
            var _content = emailTemplate.Content;
            var subTemplate = sysEmailTemplateRepo.Get(x => x.Code == "SE-PRE-ALERT-SUB").FirstOrDefault();
            if (serviceId == "SFE")
            {
                // Subject
                var etdDate = _housebill.Etd ?? _shipment.Etd;
                string etd = etdDate == null ? string.Empty : etdDate.Value.ToString("dd MMM").ToUpper();
                #region Remove Old
                //_subject = string.Format(@"PRE ALERT –{0} – {1} // HBL: {2} // MBL: {3}// {4}// {5} // {6} // ETD: {7}",
                //_pol?.NameEn, _pod?.NameEn, _housebill.Hwbno, _housebill.Mawb, _housebill.PackageContainer, _shipper?.PartnerNameEn, _consignee?.PartnerNameEn, etd);
                #endregion

                // Body
                string containerDetail = string.Empty;
                foreach (var con in csMawbcontainers)
                {
                    string conType = GetUnitNameById(con.ContainerTypeId);
                    string packType = GetUnitNameById(con.PackageTypeId);
                    #region Remove Old
                    //containerDetail += string.Format("<div>{0}x{1}, CTNR/SEAL NO.: {2}/{3}/{4}, {5} {6}, G.W: {7}, CBM: {8}</div>",
                    //    con.Quantity, conType, con.ContainerNo, conType, con.SealNo, con.PackageQuantity, packType, String.Format("{0:#.###}", con.Gw), String.Format("{0:#.###}", con.Cbm));
                    #endregion
                    containerDetail = subTemplate.Body;
                    containerDetail = containerDetail.Replace("{{Qty}}", string.Format("{0}x{1}", con.Quantity, conType));
                    containerDetail = containerDetail.Replace("{{SealNo}}", string.Format(", CTNR/SEAL NO.: {0}/{1}/{2}, {3} {4}", con.ContainerNo, conType, con.SealNo, con.PackageQuantity, packType));
                    containerDetail = containerDetail.Replace("{{GW}}", String.Format("{0:#.###}", con.Gw));
                    containerDetail = containerDetail.Replace("{{CBM}}", String.Format("{0:#.###}", con.Cbm));
                }

                string packageTotal = string.Join(", ", csMawbcontainers.GroupBy(x => x.PackageTypeId).Select(x => x.Sum(c => c.PackageQuantity) + " " + GetUnitNameById(x.Key)));
                //string packTypeTotal = _housebill.PackageType == null ? string.Empty : unitRepository.Get(x => x.Id == _housebill.PackageType).FirstOrDefault()?.UnitNameEn;
                etd = etdDate == null ? string.Empty : etdDate.Value.ToString("dd MMM, yyyy").ToUpper();
                #region Remove Old
                //_body = string.Format(@"<div><b>Dear Sir/Madam,</b></div></br><div>Please find pre-alert docs in the attachment and confirm receipt.</div><br/>" +
                //                        "<div>POL : {0}</div><div>POD : {1}</div><br/>" +
                //                        "<div>VSL : {2}</div><div>ETD : {3}</div><br/>" +
                //                        "<div>MBL : {4}({5}, {6})</div>" + containerDetail +
                //                        "<div>TOTAL : {7}, {8}, G.W: {9}, CBM: {10}</div><br/>" +
                //                        "<div>HBL : {11} ({12}, {13})</div>" +
                //                        "<div>Shipper: {14}</div>" +
                //                        "<div>Cnee: {15}</div>" +
                //                        "<div>Notify: {16}</div>",
                //                        _pol?.NameEn, _pod?.NameEn, _housebill.OceanVoyNo, etd, _housebill.Mawb == null ? _shipment.Mawb : _housebill.Mawb, _shipment?.Mbltype, _housebill.FreightPayment,
                //                        _housebill.PackageContainer, packageTotal, String.Format("{0:#.####}", _housebill.GrossWeight), String.Format("{0:#.####}", _housebill.Cbm),
                //                        _housebill.Hwbno, _housebill.Hbltype, _housebill.FreightPayment, _shipper.PartnerNameEn, _consignee?.PartnerNameEn, _housebill.NotifyPartyDescription);
                #endregion
                _subject = _subject.Replace("{{Pol}}", _pol?.NameEn);
                _subject = _subject.Replace("{{Pod}}", _pod?.NameEn);
                _subject = _subject.Replace("{{HBL}}", _housebill.Hwbno);
                _subject = _subject.Replace("{{MBL}}", _housebill.Mawb);
                _subject = _subject.Replace("{{Package}}", _housebill.PackageContainer);
                _subject = _subject.Replace("{{Shipper}}", _shipper?.PartnerNameEn);
                _subject = _subject.Replace("{{Cnee}}", _consignee?.PartnerNameEn);
                _subject = _subject.Replace("{{ETD}}", etd);

                _body = _body.Replace("{{Pol}}", _pol?.NameEn);
                _body = _body.Replace("{{Pod}}", _pod?.NameEn);
                _body = _body.Replace("{{VSL}}", _housebill.OceanVoyNo);
                _body = _body.Replace("{{ETD}}", etd);

                _content = _content.Replace("{{NumOrder}}", string.Empty);
                _content = _content.Replace("{{MBL}}", _housebill.Mawb == null ? _shipment.Mawb : _housebill.Mawb);
                _content = _content.Replace("{{Mbltype}}", _shipment?.Mbltype);
                _content = _content.Replace("{{FreightPayment}}", _housebill.FreightPayment);
                _content = _content.Replace("{{ContainerDetail}}", containerDetail);
                _content = _content.Replace("{{Total}}", _housebill.PackageContainer + ", " + packageTotal);
                _content = _content.Replace("{{GW}}", String.Format("{0:#.####}", _housebill.GrossWeight));
                _content = _content.Replace("{{CBM}}", String.Format("{0:#.####}", _housebill.Cbm));
                _content = _content.Replace("{{HBL}}", string.Format("{0} ({1}, {2})", _housebill.Hwbno, _housebill.Hbltype, _housebill.FreightPayment));
                _content = _content.Replace("{{Shipper}}", _shipper.PartnerNameEn);
                _content = _content.Replace("{{Cnee}}", _consignee?.PartnerNameEn);
                _content = _content.Replace("{{Notify}}", _housebill.NotifyPartyDescription);

                _body = _body.Replace("{{Content}}", _content);
            }
            else
            {
                // Subject
                var etdDate = _housebill.Etd ?? _shipment.Etd;
                string etd = etdDate == null ? string.Empty : etdDate.Value.ToString("dd MMM").ToUpper();
                //_subject = string.Format(@"PRE ALERT –{0} – {1} // HBL: {2} // MBL: {3}// {4} {5}// {6} // {7} // ETD: {8}",
                //_pol?.NameEn, _pod?.NameEn, _housebill.Hwbno, _housebill.Mawb, _housebill.PackageQty, packageType, _shipper?.PartnerNameEn, _consignee?.PartnerNameEn, etd);
                _subject = _subject.Replace("{{Pol}}", _pol?.NameEn);
                _subject = _subject.Replace("{{Pod}}", _pod?.NameEn);
                _subject = _subject.Replace("{{HBL}}", _housebill.Hwbno);
                _subject = _subject.Replace("{{MBL}}", _housebill.Mawb);
                _subject = _subject.Replace("{{Package}}", _housebill.PackageQty + " " + packageType);
                _subject = _subject.Replace("{{Shipper}}", _shipper?.PartnerNameEn);
                _subject = _subject.Replace("{{Cnee}}", _consignee?.PartnerNameEn);
                _subject = _subject.Replace("{{ETD}}", etd);

                // Body
                string packageDetail = string.Empty;
                foreach (var con in csMawbcontainers)
                {
                    string conType = GetUnitNameById(con.PackageTypeId);
                    #region Remove Old
                    //string packType = GetUnitNameById(con.PackageTypeId);
                    //packageDetail += string.Format("<div>{0} {1}, G.W: {2}, CBM: {3}</div>", con.PackageQuantity, packType, String.Format("{0:#.###}", con.Gw), String.Format("{0:#.###}", con.Cbm));
                    #endregion
                    packageDetail = subTemplate.Body;
                    packageDetail = packageDetail.Replace("{{Qty}}", string.Format("{0} {1}", con.Quantity, conType));
                    packageDetail = packageDetail.Replace("{{SealNo}}", string.Empty);
                    packageDetail = packageDetail.Replace("{{GW}}", String.Format("{0:#.###}", con.Gw));
                    packageDetail = packageDetail.Replace("{{CBM}}", String.Format("{0:#.###}", con.Cbm));
                }

                string packageTotal = string.Join(", ", csMawbcontainers.GroupBy(x => x.PackageTypeId).Select(x => x.Sum(c => c.PackageQuantity) + " " + GetUnitNameById(x.Key)));
                etd = etdDate == null ? string.Empty : etdDate.Value.ToString("dd MMM, yyyy").ToUpper();
                #region Remove Old
                //_body = string.Format(@"<div><b>Dear Sir/Madam,</b></div></br><div>Please find pre-alert docs in the attachment and confirm receipt.</div><br/>" +
                //                        "<div>POL : {0}</div><div>POD : {1}</div><br/>" +
                //                        "<div>VSL : {2}</div><div>ETD : {3}</div><br/>" +
                //                        "<div>MBL : {4}({5}, {6})</div>" + packageDetail +
                //                        "<div>TOTAL : {7}, G.W: {8}, CBM: {9}</div><br/>" +
                //                        "<div>HBL : {10} ({11}, {12})</div>" +
                //                        "<div>Shipper: {13}</div>" +
                //                        "<div>Cnee: {14}</div>" +
                //                        "<div>Notify: {15}</div>",
                //                        _pol?.NameEn, _pod?.NameEn, _housebill.OceanVoyNo, etd, _housebill.Mawb == null ? _shipment.Mawb : _housebill.Mawb, _shipment?.Mbltype, _housebill.FreightPayment,
                //                        packageTotal, String.Format("{0:#.####}", _housebill.GrossWeight), String.Format("{0:#.####}", _housebill.Cbm),
                //                        _housebill.Hwbno, _housebill.Hbltype, _housebill.FreightPayment, _shipper.PartnerNameEn, _consignee?.PartnerNameEn, _housebill.NotifyPartyDescription);
                #endregion
                

                _body = _body.Replace("{{Pol}}", _pol?.NameEn);
                _body = _body.Replace("{{Pod}}", _pod?.NameEn);
                _body = _body.Replace("{{VSL}}", _housebill.OceanVoyNo);
                _body = _body.Replace("{{ETD}}", etd);

                _content = _content.Replace("{{NumOrder}}", string.Empty);
                _content = _content.Replace("{{MBL}}", _housebill.Mawb == null ? _shipment.Mawb : _housebill.Mawb);
                _content = _content.Replace("{{Mbltype}}", _shipment?.Mbltype);
                _content = _content.Replace("{{FreightPayment}}", _housebill.FreightPayment);
                _content = _content.Replace("{{ContainerDetail}}", packageDetail);
                _content = _content.Replace("{{Total}}", packageTotal);
                _content = _content.Replace("{{GW}}", String.Format("{0:#.####}", _housebill.GrossWeight));
                _content = _content.Replace("{{CBM}}", String.Format("{0:#.####}", _housebill.Cbm));
                _content = _content.Replace("{{HBL}}", string.Format("{0} ({1}, {2})", _housebill.Hwbno, _housebill.Hbltype, _housebill.FreightPayment));
                _content = _content.Replace("{{Shipper}}", _shipper.PartnerNameEn);
                _content = _content.Replace("{{Cnee}}", _consignee?.PartnerNameEn);
                _content = _content.Replace("{{Notify}}", _housebill.NotifyPartyDescription);

                _body = _body.Replace("{{Content}}", _content);
            }
            // Get email from of person in charge
            var groupUser = sysGroupRepo.Get(x => x.Id == _shipment.GroupId).FirstOrDefault();

            // Email PIC
            var _picId = !string.IsNullOrEmpty(_shipment.PersonIncharge) ? sysUserRepo.Get(x => x.Id.ToString() == _shipment.PersonIncharge).FirstOrDefault()?.EmployeeId : string.Empty;
            var picEmail = sysEmployeeRepo.Get(x => x.Id == _picId).FirstOrDefault()?.Email; //Email from
            var mailFrom = "Info FMS";
            if (!string.IsNullOrEmpty(picEmail))
            {
                mailFrom = picEmail;
            }
            else
            {
                mailFrom = @"sea@itlvn.com";
            }

            // Email to: agent/customer
            var partnerInfo = catPartnerRepo.Get(x => x.Id == _shipment.AgentId).FirstOrDefault()?.Email; //Email to
            if (string.IsNullOrEmpty(partnerInfo))
            {
                partnerInfo = catPartnerRepo.Get(x => x.Id == _housebill.CustomerId).FirstOrDefault()?.Email;
            }

            var emailContent = new EmailContentModel();
            emailContent.From = mailFrom;
            emailContent.To = string.IsNullOrEmpty(partnerInfo) ? string.Empty : partnerInfo; //Email của Customer/Agent
            emailContent.Cc = groupUser?.Email;
            emailContent.Subject = _subject;
            emailContent.Body = _body;
            emailContent.AttachFiles = new List<string>();
            return emailContent;
        }

        public EmailContentModel GetInfoMailPreAlerSeaExport(Guid? jobId, string serviceId)
        {
            var _shipment = DataContext.Get(x => x.Id == jobId).FirstOrDefault();
            if (_shipment == null) return null;
            var _housebills = detailRepository.Get(x => x.JobId == _shipment.Id);

            var _pol = catPlaceRepo.Get(x => x.Id == _shipment.Pol).FirstOrDefault(); // Departure Airport
            var _pod = catPlaceRepo.Get(x => x.Id == _shipment.Pod).FirstOrDefault(); // Destination Airpor
            var _agentDetail = catPartnerRepo.Get(x => x.Id == _shipment.AgentId).FirstOrDefault();
            var emailTemplate = sysEmailTemplateRepo.Get(x => x.Code == "SE-PRE-ALERT").FirstOrDefault();
            string _subject = emailTemplate.Subject;
            string _body = emailTemplate.Body;
            var subTemplate = sysEmailTemplateRepo.Get(x => x.Code == "SE-PRE-ALERT-SUB").FirstOrDefault();
            if (serviceId == "SFE")
            {
                // Subject
                var etdDate = _shipment.Etd;
                string etd = etdDate == null ? string.Empty : etdDate.Value.ToString("dd MMM").ToUpper();
                var hwbNos = string.Join(" - ", _housebills.Select(x => x.Hwbno).Distinct());
                _subject = _subject.Replace("{{Pol}}", _pol?.NameEn);
                _subject = _subject.Replace("{{Pod}}", _pod?.NameEn);
                _subject = _subject.Replace("{{HBL}}", hwbNos);
                _subject = _subject.Replace("{{MBL}}", _shipment.Mawb);
                _subject = _subject.Replace("{{Package}}", _shipment.PackageContainer);
                _subject = _subject.Replace("{{Shipper}}", string.Empty);
                _subject = _subject.Replace("{{Cnee}}", string.Empty);
                _subject = _subject.Replace("{{ETD}}", etd);
                // Body
                etd = etdDate == null ? string.Empty : etdDate.Value.ToString("dd MMM, yyyy").ToUpper();
                _body = _body.Replace("{{Pol}}", _pol?.NameEn);
                _body = _body.Replace("{{Pod}}", _pod?.NameEn);
                _body = _body.Replace("{{VSL}}", _shipment.FlightVesselName);
                _body = _body.Replace("{{ETD}}", etd);

                var contentEmail = string.Empty;
                var numOrder = 1;
                foreach (var _housebill in _housebills)
                {
                    var csMawbcontainers = csMawbcontainerRepo.Get(x => x.Hblid == _housebill.Id);
                    string containerDetail = string.Empty;
                    foreach (var con in csMawbcontainers)
                    {
                        string conType = GetUnitNameById(con.ContainerTypeId);
                        string packType = GetUnitNameById(con.PackageTypeId);
                        containerDetail = subTemplate.Body;
                        containerDetail = containerDetail.Replace("{{Qty}}", string.Format("{0}x{1}", con.Quantity, conType));
                        containerDetail = containerDetail.Replace("{{SealNo}}", string.Format(", CTNR/SEAL NO.: {0}/{1}/{2}, {3} {4}", con.ContainerNo, conType, con.SealNo, con.PackageQuantity, packType));
                        containerDetail = containerDetail.Replace("{{GW}}", String.Format("{0:#.###}", con.Gw));
                        containerDetail = containerDetail.Replace("{{CBM}}", String.Format("{0:#.###}", con.Cbm));
                    }
                    string packageTotal = string.Join(", ", csMawbcontainers.GroupBy(x => x.PackageTypeId).Select(x => x.Sum(c => c.PackageQuantity) + " " + GetUnitNameById(x.Key)));

                    var _shipper = catPartnerRepo.Get(x => x.Id == _housebill.ShipperId).FirstOrDefault();
                    var _consignee = catPartnerRepo.Get(x => x.Id == _housebill.ConsigneeId).FirstOrDefault();
                    var _content = emailTemplate.Content;
                    _content = _content.Replace("{{NumOrder}}", numOrder + ". ");
                    _content = _content.Replace("{{MBL}}", _housebill.Mawb == null ? _shipment.Mawb : _housebill.Mawb);
                    _content = _content.Replace("{{Mbltype}}", _shipment?.Mbltype);
                    _content = _content.Replace("{{FreightPayment}}", _housebill.FreightPayment);
                    _content = _content.Replace("{{ContainerDetail}}", containerDetail);
                    _content = _content.Replace("{{Total}}", _housebill.PackageContainer + ", " + packageTotal);
                    _content = _content.Replace("{{GW}}", String.Format("{0:#.####}", _housebill.GrossWeight));
                    _content = _content.Replace("{{CBM}}", String.Format("{0:#.####}", _housebill.Cbm));
                    _content = _content.Replace("{{HBL}}", string.Format("{0} ({1}, {2})", _housebill.Hwbno, _housebill.Hbltype, _housebill.FreightPayment));
                    _content = _content.Replace("{{Shipper}}", _shipper.PartnerNameEn);
                    _content = _content.Replace("{{Cnee}}", _consignee?.PartnerNameEn);
                    _content = _content.Replace("{{Notify}}", _housebill.NotifyPartyDescription);
                    _content += "<div></div>";
                    contentEmail += _content;
                    numOrder += 1;
                }

                if (_housebills == null || _housebills.Count() == 0)
                {
                    var _content = emailTemplate.Content;
                    _content = _content.Replace("{{NumOrder}}", numOrder + ". ");
                    _content = _content.Replace("{{MBL}}", string.Empty);
                    _content = _content.Replace("{{Mbltype}}", string.Empty);
                    _content = _content.Replace("{{FreightPayment}}", string.Empty);
                    _content = _content.Replace("{{ContainerDetail}}", string.Empty);
                    _content = _content.Replace("{{Total}}", string.Empty);
                    _content = _content.Replace("{{GW}}", string.Empty);
                    _content = _content.Replace("{{CBM}}", string.Empty);
                    _content = _content.Replace("{{HBL}}", string.Empty);
                    _content = _content.Replace("{{Shipper}}", string.Empty);
                    _content = _content.Replace("{{Cnee}}", string.Empty);
                    _content = _content.Replace("{{Notify}}", string.Empty);
                    contentEmail += _content;
                }
                _body = _body.Replace("{{Content}}", contentEmail);
            }
            else
            {
                var etdDate = _shipment.Etd;
                string etd = etdDate == null ? string.Empty : etdDate.Value.ToString("dd MMM").ToUpper();
                var hwbNos = string.Join(" - ", _housebills.Select(x => x.Hwbno).Distinct());

                // Subject
                _subject = _subject.Replace("{{Pol}}", _pol?.NameEn);
                _subject = _subject.Replace("{{Pod}}", _pod?.NameEn);
                _subject = _subject.Replace("{{HBL}}", hwbNos);
                _subject = _subject.Replace("{{MBL}}", _shipment.Mawb);
                _subject = _subject.Replace("{{Package}}", _shipment.PackageQty + " " + _shipment.PackageType);
                _subject = _subject.Replace("{{Shipper}}", string.Empty);
                _subject = _subject.Replace("{{Cnee}}", string.Empty);
                _subject = _subject.Replace("{{ETD}}", etd);
                // Body
                etd = etdDate == null ? string.Empty : etdDate.Value.ToString("dd MMM, yyyy").ToUpper();
                _body = _body.Replace("{{Pol}}", _pol?.NameEn);
                _body = _body.Replace("{{Pod}}", _pod?.NameEn);
                _body = _body.Replace("{{VSL}}", _shipment.FlightVesselName);
                _body = _body.Replace("{{ETD}}", etd);

                var contentEmail = string.Empty;
                var numOrder = 1;
                foreach (var _housebill in _housebills)
                {
                    string packageDetail = string.Empty;
                    var csMawbcontainers = csMawbcontainerRepo.Get(x => x.Hblid == _housebill.Id);
                    foreach (var con in csMawbcontainers)
                    {
                        string conType = GetUnitNameById(con.PackageTypeId);
                        packageDetail = subTemplate.Body;
                        packageDetail = packageDetail.Replace("{{Qty}}", string.Format("{0} {1}", con.Quantity, conType));
                        packageDetail = packageDetail.Replace("{{SealNo}}", string.Empty);
                        packageDetail = packageDetail.Replace("{{GW}}", String.Format("{0:#.###}", con.Gw));
                        packageDetail = packageDetail.Replace("{{CBM}}", String.Format("{0:#.###}", con.Cbm));
                    }
                    string packageTotal = string.Join(", ", csMawbcontainers.GroupBy(x => x.PackageTypeId).Select(x => x.Sum(c => c.PackageQuantity) + " " + GetUnitNameById(x.Key)));
                    var _shipper = catPartnerRepo.Get(x => x.Id == _housebill.ShipperId).FirstOrDefault();
                    var _consignee = catPartnerRepo.Get(x => x.Id == _housebill.ConsigneeId).FirstOrDefault();

                    var _content = emailTemplate.Content;
                    _content = _content.Replace("{{NumOrder}}", numOrder + ". ");
                    _content = _content.Replace("{{MBL}}", _housebill.Mawb == null ? _shipment.Mawb : _housebill.Mawb);
                    _content = _content.Replace("{{Mbltype}}", _shipment?.Mbltype);
                    _content = _content.Replace("{{FreightPayment}}", _housebill.FreightPayment);
                    _content = _content.Replace("{{ContainerDetail}}", packageDetail);
                    _content = _content.Replace("{{Total}}", packageTotal);
                    _content = _content.Replace("{{GW}}", String.Format("{0:#.####}", _housebill.GrossWeight));
                    _content = _content.Replace("{{CBM}}", String.Format("{0:#.####}", _housebill.Cbm));

                    _content = _content.Replace("{{HBL}}", string.Format("{0} ({1}, {2})", _housebill.Hwbno, _housebill.Hbltype, _housebill.FreightPayment));
                    _content = _content.Replace("{{Shipper}}", _shipper.PartnerNameEn);
                    _content = _content.Replace("{{Cnee}}", _consignee?.PartnerNameEn);
                    _content = _content.Replace("{{Notify}}", _housebill.NotifyPartyDescription);
                    _content += "<div></div>";
                    contentEmail += _content;
                    numOrder += 1;
                }
                if (_housebills == null || _housebills.Count() == 0)
                {
                    var _content = emailTemplate.Content;
                    _content = _content.Replace("{{NumOrder}}", numOrder + ". ");
                    _content = _content.Replace("{{MBL}}", string.Empty);
                    _content = _content.Replace("{{Mbltype}}", string.Empty);
                    _content = _content.Replace("{{FreightPayment}}", string.Empty);
                    _content = _content.Replace("{{ContainerDetail}}", string.Empty);
                    _content = _content.Replace("{{Total}}", string.Empty);
                    _content = _content.Replace("{{GW}}", string.Empty);
                    _content = _content.Replace("{{CBM}}", string.Empty);
                    _content = _content.Replace("{{HBL}}", string.Empty);
                    _content = _content.Replace("{{Shipper}}", string.Empty);
                    _content = _content.Replace("{{Cnee}}", string.Empty);
                    _content = _content.Replace("{{Notify}}", string.Empty);
                    contentEmail += _content;
                }

                _body = _body.Replace("{{Content}}", contentEmail);
            }
            //var salemanIds = _housebills.Select(x => x.SaleManId).Distinct().ToList();
            //var _salemans = sysUserRepo.Get(x => salemanIds.Any(z => z == x.Id)).Select(x => x.EmployeeId).ToList();
            //var _salemanEmails = string.Join(";", sysEmployeeRepo.Get(x => _salemans.Any(z => z == x.Id)).Select(x => x.Email));
            // Get email from of person in charge
            var groupUser = sysGroupRepo.Get(x => x.Id == _shipment.GroupId).FirstOrDefault();

            // Email PIC
            var _picId = !string.IsNullOrEmpty(_shipment.PersonIncharge) ? sysUserRepo.Get(x => x.Id.ToString() == _shipment.PersonIncharge).FirstOrDefault()?.EmployeeId : string.Empty;
            var picEmail = sysEmployeeRepo.Get(x => x.Id == _picId).FirstOrDefault()?.Email; //Email from
            var mailFrom = "Info FMS";
            if (!string.IsNullOrEmpty(picEmail))
            {
                mailFrom = picEmail;
            }
            else
            {
                mailFrom = @"sea@itlvn.com";
            }

            // Email to: agent/customer
            var partnerInfo = catPartnerRepo.Get(x => x.Id == _shipment.AgentId).FirstOrDefault()?.Email; //Email to
            if (string.IsNullOrEmpty(partnerInfo))
            {
                var customers = _housebills.Select(x => x.CustomerId).Distinct().ToList();
                partnerInfo = string.Join(";", catPartnerRepo.Get(x => customers.Any(z => z == x.Id)).Select(x => x.Email));
            }

            var emailContent = new EmailContentModel();
            emailContent.From = mailFrom;
            emailContent.To = string.IsNullOrEmpty(partnerInfo) ? string.Empty : partnerInfo; //Email của Customer/Agent
            emailContent.Cc = groupUser?.Email;
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool SendMailContractCashWithOutstandingDebit()
        {
            try
            {
                var dtData = ((eFMSDataContext)DataContext.DC).GetViewData<vw_GetDataCustomerContractCashWithOutstandingDebit>();
                var emailBcc = ((eFMSDataContext)DataContext.DC).ExecuteFuncScalar("[dbo].[fn_GetEmailBcc]");
                new LogHelper("SendMailContractCashWithOutstandingDebit", "Data : " + JsonConvert.SerializeObject(dtData, Formatting.Indented));
                var dtGrp = dtData.Where(x => !string.IsNullOrEmpty(x.SaleManId)).GroupBy(x => new
                {
                    x.SaleManId
                });
                var emailTemplate = sysEmailTemplateRepo.Get(x => x.Code == "CASH-WITH-OUTSTANDING-DEBIT").FirstOrDefault();
                string subject = emailTemplate.Subject;
                List<string> emailBCCs = new List<string>();
                if (emailBcc != null)
                {
                    emailBCCs = emailBcc.ToString().Split(";").ToList();
                }
                foreach (var saleman in dtGrp)
                {
                    var attachFile = GetAttachExportShipmentOutstandingDebit(saleman.Key.SaleManId).Result;
                    string body = emailTemplate.Body;
                    body = body.Replace("{{SalemanName}}", saleman.FirstOrDefault().SalemanName);

                    int number = 0;

                    string content = string.Empty;
                    var contractGrp = saleman.GroupBy(x => new { x.AccountNo, x.ContractId });
                    foreach (var item in contractGrp)
                    {
                        string formatCurrency = item.FirstOrDefault().CreditCurrency == "VND" ? "N0" : "N02";
                        var office = string.Join(";", item.GroupBy(x => x.OfficeName).Select(x => x.Key));
                        string row = emailTemplate.Content;
                        row = row.Replace("{{STT}}", (number + 1).ToString());
                        row = row.Replace("{{Customer}}", item.FirstOrDefault().AccountNo + "-" + item.FirstOrDefault().CustomerName);
                        row = row.Replace("{{Branch}}", office);
                        row = row.Replace("{{Amount}}", item.FirstOrDefault().DebitAmount?.ToString(formatCurrency));
                        row = row.Replace("{{Currency}}", item.FirstOrDefault().CreditCurrency);
                        content += row;
                        number++;
                    }

                    body = body.Replace("{{Content}}", content);

                    string footer = emailTemplate.Footer;
                    // Mail to
                    var mailTo = new List<string> { saleman.FirstOrDefault().SalemanEmail };
                    var mailCC = saleman.FirstOrDefault().EmailCC.Split(";").ToList();
                    // Bcc
                    if (saleman.Count() > 0 && attachFile.Status)
                    {
                        string email = body + footer;

                        List<string> pathFile = new List<string>() { attachFile.Data.ToString() };
                        var s = SendMail.Send(subject, email, mailTo, pathFile, mailCC, emailBCCs);

                        #region --- Ghi Log Send Mail ---
                        var logSendMail = new SysSentEmailHistory
                        {
                            SentUser = SendMail._emailFrom,
                            Receivers = string.Join("; ", mailTo),
                            Subject = subject,
                            Sent = s,
                            SentDateTime = DateTime.Now,
                            Body = body,
                            Ccs = string.Join("; ", mailCC),
                            Bccs = string.Join("; ", emailBCCs)
                        };
                        var hsLogSendMail = sentEmailHistoryRepo.Add(logSendMail);
                        var hsSm = sentEmailHistoryRepo.SubmitChanges();
                        #endregion --- Ghi Log Send Mail ---
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                var logMessage = string.Format(" *  \n [END]: {0} * ,\n Exception message: {1}", DateTime.Now.ToString(), ex.ToString());
                logMessage += "\n END LOG SENDMAILCONTRACTCASHWITHOUTSTANDINGDEBIT-------------------------------------------------------------------------\n";
                new LogHelper("[EFMS_SENDMAILCONTRACTCASHWITHOUTSTANDINGDEBIT]", logMessage);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="salemanId"></param>
        /// <returns></returns>
        public List<sp_GetShipmentDataWithOutstandingDebit> GetDataOustandingDebit(string salemanId)
        {
            var parameter = new[]{
                new SqlParameter(){ ParameterName = "@salemanId", Value = salemanId }
            };
            var data = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetShipmentDataWithOutstandingDebit>(parameter);
            return data;
        }

        private async Task<ResultHandle> GetAttachExportShipmentOutstandingDebit(string salemanId)
        {
            Uri urlExport = new Uri(apiServiceUrl.Value.ApiUrlExport);

            HttpResponseMessage resquest = await HttpClientService.GetApi(urlExport + "/api/v1/en-US/Documentation/ExportShipmentOutstandingDebit?salemanId=" + salemanId, null);
            var response = await resquest.Content.ReadAsAsync<ResultHandle>();
            return response;
        }
    }
}
