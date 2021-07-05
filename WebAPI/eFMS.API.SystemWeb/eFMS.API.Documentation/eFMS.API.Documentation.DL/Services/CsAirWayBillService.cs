﻿using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Exports;
using eFMS.API.Documentation.DL.Models.ReportResults;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsAirWayBillService : RepositoryBase<CsAirWayBill, CsAirWayBillModel>, ICsAirWayBillService
    {
        private readonly ICsDimensionDetailService dimensionDetailService;
        private readonly ICsShipmentOtherChargeService shipmentOtherChargeService;
        private readonly ICurrentUser currentUser;
        readonly IContextBase<CatPlace> catPlaceRepo;
        readonly IContextBase<CsTransaction> csTransactionRepo;
        readonly IContextBase<CatPartner> catPartnerRepo;
        readonly IContextBase<CatCountry> countryRepo;
        readonly IContextBase<CatUnit> unitRepository;
        private readonly IOptions<WebUrl> webUrl;
        public CsAirWayBillService(IContextBase<CsAirWayBill> repository, 
            IMapper mapper,
            ICsDimensionDetailService dimensionService,
            ICsShipmentOtherChargeService otherChargeService,
            ICurrentUser currUser,
            IContextBase<CatPlace> catPlace,
            IContextBase<CsTransaction> csTransaction,
            IContextBase<CatPartner> catPartner,
            IContextBase<CatUnit> unitRepo,
            IContextBase<CatCountry> catCountry,
            IOptions<WebUrl> url) : base(repository, mapper)
        {
            dimensionDetailService = dimensionService;
            shipmentOtherChargeService = otherChargeService;
            currentUser = currUser;
            catPlaceRepo = catPlace;
            csTransactionRepo = csTransaction;
            catPartnerRepo = catPartner;
            countryRepo = catCountry;
            unitRepository = unitRepo;
            webUrl = url;
        }

        public CsAirWayBillModel GetBy(Guid jobId)
        {
            var result = Get(x => x.JobId == jobId).FirstOrDefault();
            if (result == null) return null;
            result.DimensionDetails = dimensionDetailService.Get(x => x.AirWayBillId == result.Id).ToList();
            result.OtherCharges = shipmentOtherChargeService.Get(x => x.JobId == jobId).ToList();
            return result;
        }

        public override HandleState Add(CsAirWayBillModel entity)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var model = mapper.Map<CsAirWayBill>(entity);
                    model.Id = Guid.NewGuid();
                    model.DatetimeModified = DateTime.Now;
                    model.UserModified = currentUser.UserID;
                    var hs = DataContext.Add(model);
                    if (hs.Success)
                    {
                        if(entity.DimensionDetails != null)
                        {
                            entity.DimensionDetails.ForEach(x => {
                                x.UserCreated = currentUser.UserID;
                                x.DatetimeCreated = DateTime.Now;
                                x.Id = Guid.NewGuid();
                                x.AirWayBillId = model.Id;
                            });
                            var hsDimensions = dimensionDetailService.Add(entity.DimensionDetails);
                        }
                        if(entity.OtherCharges != null)
                        {
                            entity.OtherCharges.ForEach(x => {
                                x.UserModified = currentUser.UserID;
                                x.DatetimeModified = DateTime.Now;
                                x.Id = Guid.NewGuid();
                                x.JobId = model.JobId;
                            });
                            var hsOtherCharges = shipmentOtherChargeService.Add(entity.OtherCharges);
                        }
                    }
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

        public HandleState Update(CsAirWayBillModel model)
        {
            var bill = mapper.Map<CsAirWayBill>(model);
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Update(bill, x => x.Id == model.Id);
                    if (hs.Success)
                    {
                        if(model.DimensionDetails != null)
                        {
                            var hsdimensions = dimensionDetailService.UpdateAirWayBill(model.DimensionDetails, model.Id);
                        }
                        if(model.OtherCharges != null)
                        {
                            var hsOtherCharges = shipmentOtherChargeService.UpdateOtherChargeMasterBill(model.OtherCharges, model.JobId);
                        }
                    }
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

        public AirwayBillExportResult AirwayBillExport(Guid jobId)
        {
            var masterbill = Get(x => x.JobId == jobId).FirstOrDefault();
            if (masterbill == null) return null;
            masterbill.OtherCharges = shipmentOtherChargeService.Get(x => x.JobId == jobId).ToList();
            
            var result = new AirwayBillExportResult();
            result.MawbNo1 = masterbill.Mblno1;
            result.MawbNo2 = masterbill.Mblno2;
            result.MawbNo3 = masterbill.Mblno3;
            var pol = catPlaceRepo.Get(x => x.Id == masterbill.Pol).FirstOrDefault();
            var pod = catPlaceRepo.Get(x => x.Id == masterbill.Pod).FirstOrDefault();
            result.AolCode = pol?.Code;
            result.AodCode = pod?.Code;
            result.Shipper = masterbill.ShipperDescription;
            result.Route = masterbill.Route;

            //Airline lấy từ Shipment
            var shipment = csTransactionRepo.Get(x => x.Id == masterbill.JobId).FirstOrDefault();
            var airlineId = shipment?.ColoaderId;
            result.AirlineNameEn = catPartnerRepo.Get(x => x.Id == airlineId).FirstOrDefault()?.ShortName; // Name ABBR
            result.Consignee = masterbill.ConsigneeDescription;

            var _airFrieghtDa = string.Empty;
            if (!string.IsNullOrEmpty(masterbill.FreightPayment))
            {
                if (masterbill.FreightPayment == "Sea - Air Difference" || masterbill.FreightPayment == "Prepaid")
                {
                    _airFrieghtDa = "PP IN " + pol?.Code;
                }
                else
                {
                    _airFrieghtDa = "CLL IN " + pod?.Code;
                }
            }
            result.AirFrieghtDa = _airFrieghtDa;

            result.DepartureAirport = pol?.NameEn;
            result.FirstTo = masterbill.FirstCarrierTo;
            result.FirstCarrier = masterbill.FirstCarrierBy;
            result.SecondTo = masterbill.TransitPlaceTo2;
            result.SecondBy = masterbill.TransitPlaceBy2;
            result.Currency = masterbill.CurrencyId;
            result.Dclrca = masterbill.Dclrca;
            result.Dclrcus = masterbill.Dclrcus;
            result.DestinationAirport = pod?.NameEn;
            result.FlightNo = masterbill.FlightNo;
            result.FlightDate = masterbill.FlightDate;
            result.IssuranceAmount = masterbill.IssuranceAmount;
            result.HandingInfo = masterbill.HandingInformation;
            result.Pieces = masterbill.PackageQty;
            if (!string.IsNullOrEmpty(shipment?.PackageType))
            {
                result.PackageUnit = unitRepository.Get(x => x.Id == Convert.ToInt32(shipment.PackageType)).FirstOrDefault()?.Code;
            }
            result.Gw = masterbill.GrossWeight;
            result.Cw = masterbill.ChargeWeight;
            result.RateCharge = masterbill.RateCharge;
            result.Total = masterbill.Total;
            result.DesOfGood = masterbill.DesOfGoods;
            result.VolumeField = masterbill.VolumeField;

            result.PrepaidWt = masterbill.Wtpp;
            result.CollectWt = masterbill.Wtcll;
            result.PrepaidVal = masterbill.Valpp;
            result.CollectVal = masterbill.Valcll;
            result.PrepaidTax = masterbill.Taxpp;
            result.CollectTax = masterbill.Taxcll;
            result.PrepaidDueToCarrier = masterbill.DueCarrierPp;
            result.CollectDueToCarrier = masterbill.DueCarrierCll;

            // result.OtherCharges = masterbill.OtherCharges;
            List<string> otherCharges = masterbill.OtherCharge?.Split('\n').Where(x => x.ToString() != string.Empty).ToList() ?? new List<string>();
            var _otherCharges = new List<CsShipmentOtherChargeModel>();
            foreach(var otherCharge in otherCharges)
            {
                var _otherCharge = new CsShipmentOtherChargeModel();
                _otherCharge.ChargeName = otherCharge;
                _otherCharges.Add(_otherCharge);
            }
            result.OtherCharges = _otherCharges;

            result.PrepaidTotal = masterbill.TotalPp;
            result.CollectTotal = masterbill.TotalCll;
            result.IssueOn = masterbill.IssuedPlace;
            result.IssueDate = masterbill.IssuedDate;
            return result;                 
        }

        private List<MasterAirwayBillReport> AsignValueAirwayBillPreview(CsAirWayBillModel data)
        {
            MasterAirwayBillReport awb = new MasterAirwayBillReport();
            List<MasterAirwayBillReport> airWayBills = new List<MasterAirwayBillReport>();
            if (data != null)
            {
                CsTransaction csTransaction = csTransactionRepo.Get(x => x.Id == data.JobId).FirstOrDefault();

                var dataPOD = catPlaceRepo.Get(x => x.Id == data.Pod).FirstOrDefault();
                var dataPOL = catPlaceRepo.Get(x => x.Id == data.Pol).FirstOrDefault();
                var airlineId = csTransaction.ColoaderId;
                string nameEn = catPartnerRepo.Get(x => x.Id == airlineId).FirstOrDefault()?.PartnerNameEn;
                //awb.AirlineAbbrName = catPartnerRepo.Get(x => x.Id == airlineId).FirstOrDefault()?.ShortName ; // Name ABBR
                awb.AirlineAbbrName = csTransaction.AirlineInfo;
                awb.CarrierNameEn = !string.IsNullOrEmpty(nameEn) ? " - " + nameEn : string.Empty;
                awb.MAWB = data.Mblno1 + "-" + data.Mblno3;
                awb.MBLNO1 = data.Mblno1;
                awb.MBLNO2 = data.Mblno2;
                awb.MBLNO3 = data.Mblno3;
                awb.ATTN = ReportUltity.ReplaceNullAddressDescription(data.ShipperDescription)?.ToUpper(); //ShipperName & Address
                awb.Consignee = ReportUltity.ReplaceNullAddressDescription(data.ConsigneeDescription)?.ToUpper(); //Consignee & Address
                awb.AccountingInfo = "FREIGHT " + data.FreightPayment?.ToUpper(); //'FREIGHT ' + Air Freight
                awb.AgentIATACode = "37-3 0118/0000"; //Set Default Value
                if (dataPOL != null)
                {
                    awb.LastDestination = dataPOL?.NameEn;
                    awb.DepartureAirport = dataPOL?.NameEn?.ToUpper(); 
                }
                awb.FirstDestination = data.FirstCarrierTo;
                awb.FirstCarrier = data.FirstCarrierBy;
                awb.SecondDestination = data.TransitPlaceTo1;
                awb.SecondCarrier = data.TransitPlaceBy1;
                awb.ThirdDestination = data.TransitPlaceTo2;
                awb.ThirdCarrier = data.TransitPlaceBy2;
                awb.Currency = data.CurrencyId?.ToUpper(); //Currency
                awb.CHGSCode = data.Chgs;
                awb.WTPP = data.Wtpp;
                awb.WTCLL = data.Wtcll;
                awb.ORPP = data.OtherPayment == "PP" ? data.OtherPayment : string.Empty;
                awb.ORCLL = data.OtherPayment == "CLL" ? data.OtherPayment : string.Empty;
                awb.DlvCarriage = data.Dclrca?.ToUpper(); //DCLR-CA
                awb.DlvCustoms = data.Dclrcus?.ToUpper(); //DCLR-CUS
                if (dataPOD != null)
                {
                    awb.LastDestination = dataPOD?.NameEn;
                    awb.LastDestination = awb.LastDestination?.ToUpper();
                }
                awb.FlightNo = data.FlightNo;
                awb.FlightDate = data.FlightDate;
                awb.ConnectingFlight = string.Empty; //Để rỗng
                awb.ConnectingFlightDate = null; //Gán null
                awb.insurAmount = data.IssuranceAmount?.ToUpper(); //Issurance Amount
                awb.HandlingInfo = data.HandingInformation?.ToUpper(); //Handing Information
                awb.Notify = data.Notify?.ToUpper(); //Notify
                awb.SCI = string.Empty; //NOT USE
                awb.ReferrenceNo = string.Empty; //NOT USE
                awb.OSI = string.Empty; //NOT USE
                awb.ISSUED = data.IssuedBy; //NOT USE
                awb.ConsigneeID = data.ConsigneeId; //NOT USE
                awb.ICASNC = string.Empty; //NOT USE
                awb.NoPieces = data.PackageQty != null ? data.PackageQty.ToString() : string.Empty; //Số kiện (Pieces)
                awb.GrossWeight = data.GrossWeight ?? 0; //GrossWeight
                awb.GrwDecimal = 2; //NOT USE
                awb.Wlbs = data.KgIb?.ToUpper(); //KgIb
                awb.RateClass = data.Rclass;
                awb.ItemNo = data.ComItemNo?.ToUpper(); //ComItemNo - Commodity Item no
                awb.WChargeable = data.ChargeWeight ?? 0; //CW
                awb.ChWDecimal = 2; //NOT USE

                decimal RateCharge =  Convert.ToDecimal(data.RateCharge);

                awb.Rchge =  RateCharge.ToString("0.00"); //RateCharge
                double num;
                decimal total = 0M;
                bool TotalIsNumber = false;
                if (double.TryParse(data.Total, out num))
                {
                    // It's a number!
                    total = Convert.ToDecimal(data.Total);
                    TotalIsNumber = true;
                }

                awb.Ttal = TotalIsNumber ? total.ToString("0.00") : data.Total;
                awb.Description = data.DesOfGoods?.ToUpper(); //Natural and Quality Goods

                decimal Wtpp = 0M;
                bool WghtPPNumber = false;
                if (double.TryParse(data.Wtpp, out num))
                {
                    // It's a number!
                    Wtpp = Convert.ToDecimal(data.Wtpp);
                    WghtPPNumber = true;
                }               

                decimal Wtcc = 0M;
                bool WghtCCNumber = false;
                if (double.TryParse(data.Wtcll, out num))
                {
                    // It's a number!
                    Wtcc = Convert.ToDecimal(data.Wtcll);
                    WghtCCNumber = true;
                }

                decimal DueAgentPp = 0M;
                bool DueAgentPpNumber = false;
                if (double.TryParse(data.DueAgentPp, out num))
                {
                    // It's a number!
                    DueAgentPp = Convert.ToDecimal(data.DueAgentPp);
                    DueAgentPpNumber = true;
                }

                decimal DueAgentCc = 0M;
                bool DueAgentCcNumber = false;
                if (double.TryParse(data.DueAgentCll, out num))
                {
                    // It's a number!
                    DueAgentCc = Convert.ToDecimal(data.DueAgentCll);
                    DueAgentCcNumber = true;
                }

                decimal DueCarrierPp = 0M;
                bool DueCarrierPpNumber = false;
                if (double.TryParse(data.DueCarrierPp, out num))
                {
                    // It's a number!
                    DueCarrierPp = Convert.ToDecimal(data.DueCarrierPp);
                    DueCarrierPpNumber = true;
                }

                decimal DueCarrierCc = 0M;
                bool DueCarrierCcNumber = false;
                if (double.TryParse(data.DueAgentCll, out num))
                {
                    // It's a number!
                    DueCarrierCc = Convert.ToDecimal(data.DueCarrierCll);
                    DueCarrierCcNumber = true;
                }

                decimal TtalPP = 0M;
                bool TtalPPNumber = false;
                if (double.TryParse(data.TotalPp, out num))
                {
                    // It's a number!
                    TtalPP = Convert.ToDecimal(data.TotalPp);
                    TtalPPNumber = true;
                }

                decimal TtalCC = 0M;
                bool TtalCCNumber = false;
                if (double.TryParse(data.TotalCll, out num))
                {
                    // It's a number!
                    TtalCC = Convert.ToDecimal(data.TotalCll);
                    TtalCCNumber = true;
                }

                decimal ValChPP = 0M;
                bool ValChPPNumber = false;
                if (double.TryParse(data.Valpp, out num))
                {
                    // It's a number!
                    ValChPP = Convert.ToDecimal(data.Valpp);
                    ValChPPNumber = true;
                }

                decimal ValChCC = 0M;
                bool ValChCCNumber = false;
                if (double.TryParse(data.Valcll, out num))
                {
                    // It's a number!
                    ValChCC = Convert.ToDecimal(data.Valcll);
                    ValChCCNumber = true;
                }

                decimal TxPP = 0M;
                bool TxPPNumber = false;
                if (double.TryParse(data.Taxpp, out num))
                {
                    // It's a number!
                    TxPP = Convert.ToDecimal(data.Taxpp);
                    TxPPNumber = true;
                }

                decimal TxCC = 0M;
                bool TxCCNumber = false;
                if (double.TryParse(data.Taxcll, out num))
                {
                    // It's a number!
                    TxCC = Convert.ToDecimal(data.Taxcll);
                    TxCCNumber = true;
                }

                decimal TTCarrPP = 0M;
                bool TTCarrPPNumber = false;
                if (double.TryParse(data.DueCarrierPp, out num))
                {
                    // It's a number!
                    TTCarrPP = Convert.ToDecimal(data.DueCarrierPp);
                    TTCarrPPNumber = true;
                }

                decimal TTCarrCC = 0M;
                bool TTCarrCCNumber = false;
                if (double.TryParse(data.DueCarrierCll, out num))
                {
                    // It's a number!
                    TTCarrCC = Convert.ToDecimal(data.DueCarrierCll);
                    TTCarrCCNumber = true;
                }

                awb.WghtPP = WghtPPNumber ? (Wtpp != 0 ? Wtpp.ToString("0.00") : string.Empty) : data.Wtpp?.ToUpper();//WT (prepaid)
                awb.WghtCC = WghtCCNumber ? (Wtcc != 0 ? Wtcc.ToString("0.00") : string.Empty) : data.Wtcll?.ToUpper();//WT (Collect)
                //awb.WghtCC = data.Wtcll?.ToUpper(); 

                awb.ValChPP = ValChPPNumber ? (ValChPP != 0 ? ValChPP.ToString("0.00") : string.Empty) : data.Valpp?.ToUpper(); //Val (Prepaid)
                awb.ValChCC = ValChCCNumber ? (ValChCC != 0 ? ValChCC.ToString("0.00") : string.Empty) : data.Valcll?.ToUpper(); //Val (Collect)
                awb.TxPP = TxPPNumber ? (TxPP != 0 ? TxPP.ToString("0.00") : string.Empty) : data.Taxpp?.ToUpper(); //Tax (Prepaid)
                awb.TxCC = TxCCNumber ? (TxCC != 0 ? TxCC.ToString("0.00") : string.Empty) : data.Taxcll?.ToUpper(); //Tax (Collect)
                awb.OrchW = data.OtherCharge?.ToUpper(); //Other Charge
                awb.OChrVal = string.Empty; //NOT USE
                awb.TTChgAgntPP = DueAgentPpNumber ? (DueAgentPp != 0 ? DueAgentPp.ToString("0.00") : string.Empty) :  data.DueAgentPp?.ToUpper(); //Due to agent (prepaid)
                awb.TTChgAgntCC = DueAgentCcNumber ? (DueAgentCc != 0 ? DueAgentCc.ToString("0.00") : string.Empty) :   data.DueAgentCll?.ToUpper(); //Due to agent (Collect)
                awb.TTCarrPP = TTCarrPPNumber ? (TTCarrPP != 0 ? TTCarrPP.ToString("0.00") : string.Empty) : data.DueCarrierPp?.ToUpper(); //Due to carrier (prepaid)
                awb.TTCarrCC = TTCarrCCNumber ? (TTCarrCC != 0 ? TTCarrCC.ToString("0.00") : string.Empty) : data.DueCarrierCll?.ToUpper(); //Due to carrier (Collect)
                awb.TtalPP = TtalPPNumber? (TtalPP != 0 ? TtalPP.ToString("0.00") : string.Empty) : data.TotalPp?.ToUpper(); //Total (prepaid)
                awb.TtalCC = TtalCCNumber? (TtalCC != 0 ? TtalCC.ToString("0.00") : string.Empty) :  data.TotalCll?.ToUpper(); //Total (Collect)
                awb.CurConvRate = string.Empty; //NOT USE
                awb.CCChgDes = string.Empty; //NOT USE
                awb.SpecialNote = data.ShippingMark?.ToUpper(); //Shipping Mark
                awb.ShipperCertf = string.Empty; //NOT USE
                awb.ExecutedOn = data.IssuedPlace?.ToUpper(); //Issued On
                awb.ExecutedAt = data.IssuedDate != null ? data.IssuedDate.Value.ToString("dd MMM, yyyy")?.ToUpper() : string.Empty; //Issue At
                awb.Signature = string.Empty; //NOT USE
                //var dimAir = dimensionDetailService.Get(x => x.AirWayBillId == data.Id);
                //string _dimensions = string.Join("\r\n", dimAir.Select(s => (int)s.Length + "*" + (int)s.Width + "*" + (int)s.Height + "*" + (int)s.Package));
                awb.Dimensions = data.VolumeField; 
                awb.ShipPicture = null; //NOT USE
                awb.PicMarks = string.Empty; //Gán rỗng
                awb.GoodsDelivery = string.Empty; //
                airWayBills.Add(awb);
            }
            return airWayBills;
        }

        public Crystal PreviewAirwayBill(Guid jobId,string reportType)
        {
            Crystal result = null;
            var data = GetBy(jobId);
         
            List<MasterAirwayBillReport> airWayBills = AsignValueAirwayBillPreview(data); ;
            if (airWayBills.Count() > 0)
            {
                List<string> lstFooter = new List<string>
                {
                    "ORIGINAL 3 (FOR SHIPPER)",
                    "ORIGINAL 2 (FOR CONSIGNEE)",
                    "ORIGINAL 1 (FOR ISSUING CARRIER)",
                    "Copy 4 - (Delivery Receipt)",
                    "Copy 5 - (Extra Copy)",
                    "Copy 6 - (Extra Copy)",
                    "Copy 7 - (Extra Copy)",
                    "Copy 8 - (For Agent)"
                };
                if (reportType == "Full")
                {
                    for(int i = 0; i < 7; i++)
                    {
                        List<MasterAirwayBillReport> airWayBillsFull = AsignValueAirwayBillPreview(data);
                        airWayBills.AddRange(airWayBillsFull);
                        airWayBills[i].FooterName = lstFooter[i];
                    }
                    airWayBills[7].FooterName = lstFooter[7];
                }
            }
            string _reportName = reportType == "SingleNoFrame" ? "MAWBITLNoFrame.rpt" : "MAWBITLFrame.rpt";
            result = new Crystal
            {
                ReportName = _reportName,
                AllowPrint = true,
                AllowExport = true
            };

            // Get path link to report
            CrystalEx._apiUrl = webUrl.Value.Url;
            string folderDownloadReport = CrystalEx.GetLinkDownloadReports();
            var reportName = "MAWBITL" + DateTime.Now.ToString("ddMMyyHHssmm") + ".pdf";
            var _pathReportGenerate = folderDownloadReport + "/" + reportName;
            result.PathReportGenerate = _pathReportGenerate;

            result.AddDataSource(airWayBills);
            result.FormatType = ExportFormatType.PortableDocFormat;
            var parameter = new MasterAirwayBillReportParams()
            {
                MAWBN = string.Empty
            };
            result.SetParameter(parameter);
            return result;
        }
    }
}
