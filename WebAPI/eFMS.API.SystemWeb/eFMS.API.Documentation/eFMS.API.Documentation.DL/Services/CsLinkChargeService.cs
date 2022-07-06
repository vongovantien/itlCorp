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
            var csLinkCharge = new CsLinkCharge();
            var lstShipment = new List<OpsTransaction>();

            var hblId = "";

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
            myDict.Add("Custom Logistic", "CL");

            var rules = _csRuleLinkFeeRepository.Get().ToList();
            var catPartners = _catPartnerRepository.Get();

            foreach (var item in list)
            {
                var shipment = _opsTransRepository.Get(x => x.JobNo == item.JobNo).FirstOrDefault();
                if (shipment != null)
                    shipment.IsLinkFee = true;
                if (shipment.ShipmentMode == "External")
                    return new HandleState("There job shipment mode External");

                //Map transtype
                var proSerVe = myDict[shipment.ProductService + " " + shipment.ServiceMode];

                var rule = rules.Where(x =>
                x.ServiceBuying == proSerVe
                && x.ServiceSelling == item.TransactionType
                && x.ChargeSelling.ToLower() == item.ChargeId.ToString().ToLower()
                && x.Status == true).OrderByDescending(x => x.DatetimeCreated).FirstOrDefault();

                if (rule == null)
                    return new HandleState("There is no link fee rule ");
                if (rule.PartnerSelling != "All" && rule.PartnerSelling != null && rule.PartnerSelling != item.PaymentObjectId)
                    return new HandleState("There is no link fee rule Partner Selling");

                var charge = _csSurchargeRepository.Get(x => x.Id == item.Id).FirstOrDefault();

                CsShipmentSurcharge chargeBuy = mapperSurcharge(item);
                CsShipmentSurcharge chargeUpdate = mapperSurcharge(charge);

                //Nếu HBL từ link nội bộ null
                if (shipment.ServiceHblId == null)
                {
                    var jobTrans = _csTransactionRepository.Get(x => x.JobNo == shipment.ServiceNo).FirstOrDefault();
                    if (jobTrans != null)
                    {
                        var hbl = _tranDetailRepository.Get(x => x.JobId == jobTrans.Id).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                        if (hbl == null)
                            return new HandleState("There is no hbl job service");
                        if (jobTrans.TransactionType != rule.ServiceBuying)
                            return new HandleState("There is no service type");

                        chargeBuy.Hblid = hbl != null ? hbl.Id : new Guid();
                        chargeBuy.Hblno = hbl != null ? hbl.Hwbno : null;
                    }
                }
                else
                    chargeBuy.Hblid = shipment.ServiceHblId ?? new Guid();

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
                chargeBuy.Notes = null;
                chargeBuy.IsFromShipment = true;
                chargeBuy.SyncedFrom = null;
                chargeBuy.PaySyncedFrom = null;
                chargeBuy.PaySoano = null;
                chargeBuy.PaymentRefNo = null;
                chargeBuy.CombineBillingNo = null;
                chargeBuy.ObhcombineBillingNo = null;

                surchargesAddBuy.Add(chargeBuy);

                //Add charge csLinkCharge
                csLinkCharge = new CsLinkCharge();
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
                csLinkCharge.LinkChargeType = "LINK_FEE";

                csLinkCharges.Add(csLinkCharge);
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
            var ls = Get(x=>listID.Contains(x.ChargeLinkId)||listID.Contains(x.ChargeOrgId)).FirstOrDefault();
         
            if (ls != null)
            {
                shipment = _opsTransRepository.Get(x=>x.JobNo == ls.JobNoOrg).FirstOrDefault();
                if (shipment != null)
                {
                    shipment.IsLinkFee = Get(x => x.JobNoOrg == shipment.JobNo).Count() > 1 ? true : false;
                    shipment.UserCreatedLinkJob = null;
                    shipment.DateCreatedLinkJob = null;
                    shipment.DatetimeModified = DateTime.Now;
                }
            }

            foreach (var i in list)
            {
                if (i.Type == "BUY")
                {
                    if (!string.IsNullOrEmpty(i.Soano)
                        || !string.IsNullOrEmpty(i.PaySoano)
                        || !string.IsNullOrEmpty(i.SettlementCode)
                        || !string.IsNullOrEmpty(i.VoucherId)
                        || !string.IsNullOrEmpty(i.CreditNo)
                        || !string.IsNullOrEmpty(i.DebitNo))
                    {
                        return new HandleState("Please recheck ! Some Fee's you've choosed have issue CD note,SOA,Voucher,Settlement");
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
                else if (i.Type == "SELL")
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
                            return new HandleState("Please recheck ! Some Fee's you've choosed have issue CD note,SOA,Voucher,Settlement");
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
                        foreach (var item in surchargeDelIds)
                        {
                            _csSurchargeRepository.Delete(x => x.Id == new Guid(item));
                        }
                    }

                    if (hisLinkFees.Count > 0)
                    {
                        foreach (var item in hisLinkFees)
                        {
                            DataContext.Delete(x => x.Id == item.Id);
                        }
                    }

                    _opsTransRepository.Update(shipment, x => x.Id == shipment.Id, false);
                    _opsTransRepository.SubmitChanges();
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
