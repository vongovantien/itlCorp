using AutoMapper;
using eFMS.API.Common.Helpers;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsLinkChargeService : RepositoryBase<CsLinkCharge, CsLinkChargeModel>, ICsLinkChargeService
    {
        private ICurrentUser currentUser;
        private IContextBase<CsRuleLinkFee> _csRuleLinkFeeRepository;
        private IContextBase<OpsTransaction> _opsTransRepository;
        private IContextBase<CsTransaction> _csTransactionRepository;
        private IContextBase<CsTransactionDetail> _tranDetailRepository;
        private IContextBase<CsShipmentSurcharge> _csSurchargeRepository;
        private IContextBase<CatPartner> _catPartnerRepository;
        private IContextBase<SysUser> _sysUserRepository;
        private IMapper _mapper;

        public CsLinkChargeService(ICurrentUser currUser,
            IContextBase<CsRuleLinkFee> ruleLinkRepo,
            IContextBase<OpsTransaction> opsTransRepo,
            IContextBase<CsTransaction> csTransRepo,
            IContextBase<CsTransactionDetail> csTransDetailRepo,
            IContextBase<CsShipmentSurcharge> csShipSurchargeRepo,
            IContextBase<CatPartner> catPartnerRepo,
            IContextBase<SysUser> sysUserRepo,
            IContextBase<CsLinkCharge> repository, IMapper mapper) : base(repository, mapper)
        {
            currentUser = currUser;
            _csRuleLinkFeeRepository = ruleLinkRepo;
            _opsTransRepository = opsTransRepo;
            _csTransactionRepository = csTransRepo;
            _tranDetailRepository = csTransDetailRepo;
            _csSurchargeRepository = csShipSurchargeRepo;
            _catPartnerRepository = catPartnerRepo;
            _sysUserRepository = sysUserRepo;
        }

        public HandleState UpdateChargeLinkFee(List<CsShipmentSurchargeModel> list)
        {
            var result = new HandleState();
            var surChargeUp = new CsShipmentSurcharge();
            var surchargesUpdate = new List<CsShipmentSurcharge>();
            var surchargesAddBuy = new List<CsShipmentSurcharge>();
            var csLinkCharges = new List<CsLinkCharge>();

            var lstShipment = new List<OpsTransaction>();

            var myDict = new Dictionary<string, string>();

            // Adding key/value pairs in myDict
            myDict.Add("Air Import", "AI");
            myDict.Add("Air Export", "AE");
            myDict.Add("Sea FCL Export", "SFE");
            myDict.Add("Sea FCL Import", "SFI");
            myDict.Add("Sea LCL Export", "SLE");
            myDict.Add("Sea LCL Import", "SLI");
            myDict.Add("Sea Export", "SCE");
            myDict.Add("Sea Import", "SCI");
            myDict.Add("Sea Consol Export", "SCE");
            myDict.Add("Sea Consol Import", "SCI");
            myDict.Add("Custom Logistic", "CL");

            var rules = _csRuleLinkFeeRepository.Get(x => x.Status == true);
            var catPartners = _catPartnerRepository.Get();
            var linkCharges = DataContext.Get(x => x.LinkChargeType == DocumentConstants.LINK_CHARGE_TYPE_LINK_FEE);

            var shipmentGrp = list.GroupBy(x => x.Hblid).Select(x => new { x.Key, surcharges = x.Select(z => z) });

            foreach (var data in shipmentGrp)
            {
                var shipment = _opsTransRepository.Get(x => x.JobNo == list.FirstOrDefault().JobNo).FirstOrDefault();
                if (shipment == null)
                {
                    return new HandleState("This shipment has been deleted.");
                }
                else
                {
                    shipment.IsLinkFee = true;
                }
                if (shipment.ShipmentMode == "External")
                    return new HandleState("The shipment mode is External, you can not link fee");

                var jobTrans = _csTransactionRepository.Get(x => x.JobNo == shipment.ServiceNo).FirstOrDefault();
                //Nếu HBL từ link nội bộ null
                var hbl = new CsTransactionDetail();
                {
                    if (shipment.ServiceHblId == null)
                    {
                        var surchargesOrg = _csSurchargeRepository.Get(x => x.Hblid == shipment.Hblid);
                        var surchargesLink = _csSurchargeRepository.Get(x => x.JobNo == shipment.ServiceNo);
                        var hasLinkCharges = from org in surchargesOrg
                                             join linkCharge in linkCharges on org.Id.ToString() equals linkCharge.ChargeOrgId
                                             join link in surchargesLink on linkCharge.ChargeLinkId equals link.Id.ToString()
                                             select linkCharge;
                        if (hasLinkCharges != null && hasLinkCharges.Any())
                        {

                            var _hbllinkId = new Guid(hasLinkCharges.FirstOrDefault().HbllinkId);
                            hbl = _tranDetailRepository.Get(x => x.Id == _hbllinkId).FirstOrDefault();
                        }
                        else
                        {
                            hbl = _tranDetailRepository.Get(x => x.JobId == jobTrans.Id).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                        }
                        
                    }
                    else
                    {
                        hbl = _tranDetailRepository.Get(x => x.Id == shipment.ServiceHblId).FirstOrDefault();
                        if (hbl == null)
                        {
                            hbl = _tranDetailRepository.Get(x => x.JobId == jobTrans.Id).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                        }
                    }
                }

                if (hbl == null)
                    return new HandleState("There is no house bill of shipment to link");
                shipment.ServiceHblId = hbl.Id;

                //Map transtype
                var service = myDict[shipment.ProductService + " " + shipment.ServiceMode];

                foreach (var item in data.surcharges)
                {
                    var rule = rules.Where(x =>
                    x.ServiceBuying == service
                    && x.ServiceSelling == item.TransactionType
                    && x.ChargeSelling.ToLower() == item.ChargeId.ToString().ToLower()).OrderByDescending(x => x.DatetimeCreated).FirstOrDefault();

                    if (rule == null)
                        return new HandleState("There is no link fee rule");
                    if (rule.PartnerSelling != "All" && rule.PartnerSelling != null && rule.PartnerSelling != item.PaymentObjectId)
                        return new HandleState("There is no link fee rule Partner Selling");

                    var charge = _csSurchargeRepository.Get(x => x.Id == item.Id).FirstOrDefault();

                    CsShipmentSurcharge chargeBuy = mapperSurcharge(item);
                    CsShipmentSurcharge chargeUpdate = mapperSurcharge(charge);

                    //Nếu HBL từ link nội bộ null
                    //if (shipment.ServiceHblId == null)
                    {
                        if (jobTrans != null)
                        {
                            if (jobTrans.TransactionType != rule.ServiceBuying)
                                return new HandleState("There is no service type");
                        }
                    }

                    chargeBuy.Hblid = hbl.Id;
                    chargeBuy.Hblno = hbl.Hwbno;

                    //Update shipment
                    shipment.DateCreatedLinkJob = DateTime.Now;
                    shipment.UserCreatedLinkJob = currentUser.UserID;

                    if (!lstShipment.Any(x => x.Id == shipment.Id))
                        lstShipment.Add(shipment);

                    //Update charge selling
                    chargeUpdate.LinkFee = true;
                    chargeUpdate.ModifiedDateLinkFee = DateTime.Now;
                    chargeUpdate.UserIdLinkFee = new Guid(currentUser.UserID);
                    chargeUpdate.UserNameLinkFee = currentUser.UserName;

                    surchargesUpdate.Add(chargeUpdate);

                    //Add charge type buy
                    chargeBuy.Id = Guid.NewGuid();
                    chargeBuy.Type = DocumentConstants.CHARGE_BUY_TYPE;
                    chargeBuy.TransactionType = rule.ServiceBuying;
                    chargeBuy.ChargeId = Guid.Parse(rule.ChargeBuying);
                    chargeBuy.JobNo = shipment.ServiceNo;
                    chargeBuy.LinkFee = true;
                    chargeBuy.PaymentObjectId = rule.PartnerBuying;
                    chargeBuy.Soano = null;
                    chargeBuy.PaySoano = null;
                    chargeBuy.CreditNo = null;
                    chargeBuy.DebitNo = null;
                    chargeBuy.SettlementCode = null;
                    chargeBuy.VoucherId = null;
                    chargeBuy.VoucherIddate = null;
                    chargeBuy.VoucherIdre = null;
                    chargeBuy.VoucherIdredate = null;
                    chargeBuy.AcctManagementId = null;
                    chargeBuy.InvoiceNo = null;
                    chargeBuy.InvoiceDate = null;
                    chargeBuy.LinkChargeId = null;

                    //[13/03/2022][Link thêm thông tin note khi link fee từ job OPS sang job Service]
                    //chargeBuy.Notes = null;

                    chargeBuy.IsFromShipment = true;
                    chargeBuy.SyncedFrom = null;
                    chargeBuy.PaySyncedFrom = null;
                    chargeBuy.PaySoano = null;
                    chargeBuy.PaymentRefNo = null;
                    chargeBuy.CombineBillingNo = null;
                    chargeBuy.ObhcombineBillingNo = null;
                    chargeBuy.ClearanceNo = null;
                    chargeBuy.IsRefundFee = false;

                    surchargesAddBuy.Add(chargeBuy);

                    //Add charge csLinkCharge
                    var csLinkCharge = new CsLinkCharge();
                    csLinkCharge.Id = Guid.NewGuid();
                    csLinkCharge.JobNoOrg = chargeUpdate.JobNo;
                    csLinkCharge.ChargeOrgId = chargeUpdate.Id.ToString();
                    csLinkCharge.HblorgId = chargeUpdate.Hblid.ToString();
                    csLinkCharge.HblnoOrg = chargeUpdate.Hblno;
                    csLinkCharge.MblnoOrg = chargeUpdate.Mblno;
                    csLinkCharge.PartnerOrgId = item.PaymentObjectId;

                    csLinkCharge.JobNoLink = chargeBuy.JobNo;
                    csLinkCharge.ChargeLinkId = chargeBuy.Id.ToString();
                    csLinkCharge.HbllinkId = chargeBuy.Hblid.ToString();
                    csLinkCharge.HblnoLink = chargeBuy.Hblno;
                    csLinkCharge.MblnoLink = chargeBuy.Mblno;

                    csLinkCharge.DatetimeCreated = DateTime.Now;
                    csLinkCharge.UserCreated = currentUser.UserID;
                    csLinkCharge.PartnerLinkId = chargeBuy.PaymentObjectId;
                    csLinkCharge.LinkChargeType = DocumentConstants.LINK_CHARGE_TYPE_LINK_FEE;

                    csLinkCharges.Add(csLinkCharge);
                }
            }

            if (!result.Success)
                return result;

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    if (surchargesUpdate.Count() > 0)
                    {
                        foreach (var surchargeUpdate in surchargesUpdate)
                        {
                            var hsUpdate = _csSurchargeRepository.Update(surchargeUpdate, x => x.Id == surchargeUpdate.Id, false);
                        }
                    }

                    if (surchargesAddBuy.Count() > 0)
                        _csSurchargeRepository.Add(surchargesAddBuy);
                    if (csLinkCharges.Count() > 0)
                        DataContext.Add(csLinkCharges);

                    if (lstShipment.Count() > 0)
                    {
                        foreach (var shipment in lstShipment)
                        {
                            _opsTransRepository.Update(shipment, x => x.Id == shipment.Id, false);
                        }
                    }
                    _opsTransRepository.SubmitChanges();
                    result = DataContext.SubmitChanges();
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    new LogHelper("UpdateChargeLinkFee", ex.ToString());
                    result = new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
                return result;
            }
        }

        public HandleState RevertChargeLinkFee(List<CsShipmentSurchargeModel> list)
        {
            var result = new HandleState();

            var surchargesUpdate = new List<CsShipmentSurcharge>();
            var surchargeDelIds = new List<string>();
            var hisLinkFees = new List<CsLinkCharge>();
            var shipment = new OpsTransaction();

            var listID = list.Select(x => x.Id.ToString());
            var ls = DataContext.Get(x => listID.Contains(x.ChargeLinkId) || listID.Contains(x.ChargeOrgId)).FirstOrDefault();

            if (ls != null)
            {
                shipment = _opsTransRepository.Get(x => x.JobNo == ls.JobNoOrg).FirstOrDefault();
                if (shipment != null)
                {
                    if (DataContext.Get(x => x.JobNoOrg == shipment.JobNo).Count() > 1)
                    {
                        shipment.IsLinkFee = true;
                        shipment.DatetimeModified = DateTime.Now;
                    }
                    else
                    {
                        shipment.IsLinkFee = false;
                        shipment.UserCreatedLinkJob = null;
                        shipment.DateCreatedLinkJob = null;
                        shipment.DatetimeModified = DateTime.Now;
                    }
                }
            }
            else
            {
                return result;
            }

            foreach (var i in list)
            {
                if (i.Type == DocumentConstants.CHARGE_BUY_TYPE)
                {
                    if (!string.IsNullOrEmpty(i.Soano)
                        || !string.IsNullOrEmpty(i.PaySoano)
                        || !string.IsNullOrEmpty(i.SettlementCode)
                        || !string.IsNullOrEmpty(i.VoucherId)
                        || !string.IsNullOrEmpty(i.CreditNo)
                        || !string.IsNullOrEmpty(i.DebitNo))
                    {
                        return new HandleState("Some Fee's you've choosed have issue CD note,SOA,Voucher,Settlement");
                    }

                    var his = DataContext.Get(x => x.ChargeLinkId == i.Id.ToString()).FirstOrDefault();
                    if (his != null)
                    {
                        surchargeDelIds.Add(his.ChargeLinkId);
                        hisLinkFees.Add(his);

                        var sell = _csSurchargeRepository.Where(x => x.Id == Guid.Parse(his.ChargeOrgId)).FirstOrDefault();
                        sell.LinkFee = false;
                        i.ModifiedDateLinkFee = DateTime.Now;
                        surchargesUpdate.Add(sell);
                    }
                }
                else if (i.Type == DocumentConstants.CHARGE_SELL_TYPE)
                {
                    var his = DataContext.Get(x => x.JobNoOrg == i.JobNo && x.ChargeOrgId == i.Id.ToString()).FirstOrDefault();

                    if (his != null)
                    {
                        var buy = _csSurchargeRepository.Where(x => x.Id == Guid.Parse(his.ChargeLinkId)).FirstOrDefault();
                        if (buy != null && (
                          !string.IsNullOrEmpty(buy.Soano)
                          || !string.IsNullOrEmpty(buy.PaySoano)
                          || !string.IsNullOrEmpty(buy.SettlementCode)
                          || !string.IsNullOrEmpty(buy.VoucherId)
                          || !string.IsNullOrEmpty(buy.CreditNo)
                          || !string.IsNullOrEmpty(buy.DebitNo)))
                        {
                            return new HandleState("Some Fee's you've choosed have issue CD note,SOA,Voucher,Settlement");
                        }
                        surchargeDelIds.Add(his.ChargeLinkId);
                        hisLinkFees.Add(his);
                    }
                    i.LinkFee = false;
                    i.ModifiedDateLinkFee = DateTime.Now;
                    surchargesUpdate.Add(i);
                }
            }

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    if (surchargesUpdate.Count() > 0)
                    {
                        foreach (var surchargeUpdate in surchargesUpdate)
                        {
                            var hsUpdate = _csSurchargeRepository.Update(surchargeUpdate, x => x.Id == surchargeUpdate.Id, false);
                        }
                    }

                    if (surchargeDelIds.Count > 0)
                    {
                        //foreach (var item in surchargeDelIds)
                        {
                            var deleteIds = surchargeDelIds.Select(x => Guid.Parse(x));
                            _csSurchargeRepository.DeleteAsync(x => deleteIds.Contains(x.Id), false);
                        }
                    }

                    if (hisLinkFees.Count > 0)
                    {
                        foreach (var item in hisLinkFees)
                        {
                            DataContext.Delete(x => x.Id == item.Id, false);
                        }
                    }

                    _opsTransRepository.Update(shipment, x => x.Id == shipment.Id);
                    _csSurchargeRepository.SubmitChanges();
                    result = DataContext.SubmitChanges();
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    new LogHelper("RevertChargeLinkFeeLog", ex.ToString());
                    result = new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
                return result;
            }
        }

        public CsLinkChargeModel DetailByChargeOrgId(Guid chargeId)
        {
            var csLinkCharge = Get(x => x.ChargeOrgId == chargeId.ToString()
            || x.ChargeLinkId == chargeId.ToString()).FirstOrDefault();
            if (csLinkCharge == null) return null;
            var user = _sysUserRepository.Get(x => x.Id == csLinkCharge.UserCreated).FirstOrDefault();
            var userUp = _sysUserRepository.Get(x => x.Id == csLinkCharge.UserModified).FirstOrDefault();
            var partOrg = _catPartnerRepository.Get(x => x.Id == csLinkCharge.PartnerOrgId).FirstOrDefault();
            var partLink = _catPartnerRepository.Get(x => x.Id == csLinkCharge.PartnerLinkId).FirstOrDefault();
            csLinkCharge.UserCreatedName = user != null ? user.Username : "";
            csLinkCharge.UserModifiedName = userUp != null ? userUp.Username : "";
            csLinkCharge.PartnerNameOrg = partOrg != null ? partOrg.PartnerNameEn : "";
            csLinkCharge.PartnerNameLink = partLink != null ? partLink.PartnerNameEn : "";
            return csLinkCharge;
        }

        /// <summary>
        /// Link fee with shipments
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public HandleState LinkFeeJob(List<OpsTransactionModel> list)
        {
            var result = new HandleState();
            var lstCharge = new List<CsShipmentSurchargeModel>();

            foreach (var ops in list)
            {
                var lstChargeSell = _csSurchargeRepository.Get(x => x.Hblid == ops.Hblid && x.Type == "SELL");
                if (lstChargeSell != null)
                {
                    foreach (var i in lstChargeSell)
                    {
                        if (i.LinkFee == null || i.LinkFee == false)
                        {
                            var propInfo = i.GetType().GetProperties();
                            CsShipmentSurchargeModel chargeModel = new CsShipmentSurchargeModel();
                            foreach (var item in propInfo)
                            {
                                chargeModel.GetType().GetProperty(item.Name).SetValue(chargeModel, item.GetValue(i, null), null);
                            }

                            lstCharge.Add(chargeModel);
                        }
                    }
                }
            }

            if (lstCharge.Count > 0)
                result = UpdateChargeLinkFee(lstCharge);
            else
                result = new HandleState("No charge Link Fee");
            return result;
        }

        CsShipmentSurcharge mapperSurcharge(CsShipmentSurcharge modelMap)
        {
            var model = new CsShipmentSurcharge();
            var propInfo = modelMap.GetType().GetProperties();
            foreach (var item in propInfo)
            {
                var p = model.GetType().GetProperty(item.Name);
                if (p != null)
                    p.SetValue(model, item.GetValue(modelMap, null), null);
            }

            return model;
        }
    }
}
