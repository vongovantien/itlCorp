using AutoMapper;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace eFMS.API.Documentation.DL.Services
{
    public class AcctSOAService : RepositoryBase<AcctSoa, AcctSoaModel>, IAcctSOAService
    {
        private readonly ICurrentUser currentUser;
        public AcctSOAService(IContextBase<AcctSoa> repository, IMapper mapper, ICurrentUser user) : base(repository, mapper)
        {
            currentUser = user;
        }

        public HandleState AddSOA(AcctSoaModel model)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var soa = mapper.Map<AcctSoa>(model);
                soa.Soano = model.Soano = CreateSoaNo(dc);
                //var hs = dc.AcctSoa.Add(soa);
                var hs = DataContext.Add(soa);

                if (hs.Success)
                {
                    //Lấy ra những charge có type là BUY hoặc OBH-BUY mà chưa tồn tại trong 1 SOA nào cả
                    var surchargeCredit = dc.CsShipmentSurcharge.Where(x => model.Surcharges != null
                                                                   && model.Surcharges.Any(c => c.surchargeId == x.Id && (c.type == "BUY" || c.type == "OBH-BUY") )
                                                                   && (x.Soano == null || x.Soano == "")).ToList();

                    //Lấy ra những charge có type là SELL hoặc OBH-SELL mà chưa tồn tại trong 1 SOA nào cả
                    var surchargeDebit = dc.CsShipmentSurcharge.Where(x => model.Surcharges != null
                                                                   && model.Surcharges.Any(c => c.surchargeId == x.Id && (c.type == "SELL" || c.type == "OBH-SELL"))
                                                                   && (x.Soano == null || x.Soano == "")).ToList();

                    if (surchargeCredit.Count() > 0)
                    {
                        //Update PaySOANo cho CsShipmentSurcharge có type BUY hoặc OBH-BUY(Payer)
                        surchargeCredit.ForEach(a =>
                            {
                                a.PaySoano = soa.Soano;
                                a.UserModified = currentUser.UserID;
                                a.DatetimeModified = DateTime.Now;
                            }
                        );
                    }

                    if (surchargeDebit.Count() > 0)
                    {
                        //Update SOANo cho CsShipmentSurcharge có type là SELL hoặc OBH-SELL(Receiver)
                        surchargeDebit.ForEach(a =>
                            {
                                a.Soano = soa.Soano;
                                a.UserModified = currentUser.UserID;
                                a.DatetimeModified = DateTime.Now;
                            }
                        );
                    }
                }
                dc.SaveChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        private string CreateSoaNo(eFMSDataContext dc)
        {
            var prefix = (DateTime.Now.Year.ToString()).Substring(2, 2);
            string stt;
            //Lấy ra dòng cuối cùng của table acctSOA
            var rowLast = dc.AcctSoa.LastOrDefault();
            if (rowLast == null)
            {
                stt = "00001";
            }
            else
            {
                var soaCurrent = rowLast.Soano;
                var prefixCurrent = soaCurrent.Substring(0, 2);
                //Reset về 1 khi qua năm mới
                if (prefixCurrent != prefix)
                {
                    stt = "00001";
                }
                else
                {
                    stt = (Convert.ToInt32(soaCurrent.Substring(2, 5)) + 1).ToString();
                    stt = stt.PadLeft(5, '0');
                }
            }
            return prefix + stt;
        }

        public List<AcctSOAResult> Paging(AcctSOACriteria criteria, int page, int size, out int rowsCount)
        {
            var data = GetListAcctSOA(criteria).AsQueryable();
            rowsCount = (data.Count() > 0) ? data.Count() : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }

            var dataMap = mapper.Map<List<spc_GetListAcctSOAByMaster>, List<AcctSOAResult>>(data.ToList());
            return dataMap;
        }

        private List<spc_GetListAcctSOAByMaster> GetListAcctSOA(AcctSOACriteria criteria)
        {
            DbParameter[] parameters =
            {
                SqlParam.GetParameter("strCodes", criteria.StrCodes),
                SqlParam.GetParameter("customerID", criteria.CustomerID),
                SqlParam.GetParameter("soaFromDateCreate", criteria.SoaFromDateCreate),
                SqlParam.GetParameter("soaToDateCreate", criteria.SoaToDateCreate),
                SqlParam.GetParameter("soaStatus", criteria.SoaStatus),
                SqlParam.GetParameter("soaCurrency", criteria.SoaCurrency),
                SqlParam.GetParameter("soaUserCreate", criteria.SoaUserCreate),
                SqlParam.GetParameter("currencyLocal", criteria.CurrencyLocal)
            };
            return ((eFMSDataContext)DataContext.DC).ExecuteProcedure<spc_GetListAcctSOAByMaster>(parameters);
        }

        public HandleState UpdateSOASurCharge(string soaNo)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var surcharge = dc.CsShipmentSurcharge.Where(x => x.Soano == soaNo || x.PaySoano == soaNo).ToList();
                if (surcharge.Count() > 0)
                {
                    //Update SOANo = NULL & PaySOANo = NULL to CsShipmentSurcharge
                    surcharge.ForEach(a =>
                        {
                            a.Soano = null;
                            a.PaySoano = null;
                            a.UserModified = currentUser.UserID;
                            a.DatetimeModified = DateTime.Now;
                        }
                    );
                    dc.SaveChanges();
                }

                return new HandleState();
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        public AcctSOADetailResult GetBySoaNoAndCurrencyLocal(string soaNo, string currencyLocal)
        {
            var criteria = new AcctSOACriteria
            {
                StrCodes = soaNo,
                CurrencyLocal = currencyLocal
            };
            var dataSOA = GetListAcctSOA(criteria).AsQueryable();
            var dataMapSOA = mapper.Map<spc_GetListAcctSOAByMaster, AcctSOADetailResult>(dataSOA.FirstOrDefault());

            var chargeShipmentList = GetSpcChargeShipmentBySOANo(soaNo, currencyLocal).ToList();
            var dataMapChargeShipment = mapper.Map<List<spc_GetListChargeShipmentMasterBySOANo>, List<ChargeShipmentModel>>(chargeShipmentList);

            dataMapSOA.ChargeShipments = dataMapChargeShipment;
            dataMapSOA.AmountDebitLocal = Math.Round(chargeShipmentList.Sum(x => x.AmountDebitLocal),3);
            dataMapSOA.AmountCreditLocal = Math.Round(chargeShipmentList.Sum(x => x.AmountCreditLocal),3);
            dataMapSOA.AmountDebitUSD = Math.Round(chargeShipmentList.Sum(x => x.AmountDebitUSD),3);
            dataMapSOA.AmountCreditUSD = Math.Round(chargeShipmentList.Sum(x => x.AmountCreditUSD),3);

            //Thông tin các Service Name của SOA
            dataMapSOA.ServicesNameSoa = GetInfoServiceOfSoa(soaNo).ToString();

            return dataMapSOA;
        }

        private List<spc_GetListChargeShipmentMasterBySOANo> GetSpcChargeShipmentBySOANo(string soaNo, string currencyLocal)
        {
            DbParameter[] parameters =
            {
                SqlParam.GetParameter("currencyLocal", currencyLocal),
                SqlParam.GetParameter("soaNo", soaNo)
            };
            return ((eFMSDataContext)DataContext.DC).ExecuteProcedure<spc_GetListChargeShipmentMasterBySOANo>(parameters);
        }

        public object GetListServices()
        {
            return CustomData.Services;
        }

        public object GetListStatusSoa()
        {
            return CustomData.StatusSoa;
        }

        /// <summary>
        /// Lấy thông tin các ServiceName của SOA
        /// </summary>
        /// <param name="soaNo">SOANo</param>
        /// <returns></returns>
        public string GetInfoServiceOfSoa(string soaNo)
        {
            var serviceTypeId = ((eFMSDataContext)DataContext.DC).AcctSoa.Where(x => x.Soano == soaNo).FirstOrDefault()?.ServiceTypeId;

            var serviceName = "";

            if (serviceTypeId != null)
            {
                //Tách chuỗi servicetype thành mảng
                string[] arrayStrServiceTypeId = serviceTypeId.Split(';').Where(x => x.ToString() != "").ToArray();

                //Xóa các serviceTypeId trùng
                string[] arrayGrpServiceTypeId = arrayStrServiceTypeId.Distinct<string>().ToArray();

                var serviceId = "";
                foreach (var item in arrayGrpServiceTypeId)
                {
                    //Lấy ra DisplayName của serviceTypeId
                    serviceName += CustomData.Services.Where(x => x.Value == item).FirstOrDefault() != null ?
                                CustomData.Services.Where(x => x.Value == item).FirstOrDefault().DisplayName.Trim() + ";"
                                : "";
                    serviceId += item + ";";
                }
                serviceName = (serviceName + ")").Replace(";)", "");                
            }
            return serviceName;
        }

        public HandleState UpdateSOA(AcctSoaModel model)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;

                //Gỡ bỏ các charge có SOANo = model.Soano và PaySOANo = model.Soano
                UpdateSOASurCharge(model.Soano);

                var soa = mapper.Map<AcctSoa>(model);
                //Update các thông tin của SOA
                //var hs = dc.AcctSoa.Update(soa);
                var hs = DataContext.Update(soa, x => x.Id == soa.Id);

                if (hs.Success)
                {
                    //Lấy ra những charge có type là BUY hoặc OBH-BUY mà chưa tồn tại trong 1 SOA nào cả
                    var surchargeCredit = dc.CsShipmentSurcharge.Where(x => model.Surcharges != null
                                                                   && model.Surcharges.Any(c => c.surchargeId == x.Id && (c.type == "BUY" || c.type == "OBH-BUY"))
                                                                   && (x.Soano == null || x.Soano == "")).ToList();

                    //Lấy ra những charge có type là SELL hoặc OBH-SELL mà chưa tồn tại trong 1 SOA nào cả
                    var surchargeDebit = dc.CsShipmentSurcharge.Where(x => model.Surcharges != null
                                                                   && model.Surcharges.Any(c => c.surchargeId == x.Id && (c.type == "SELL" || c.type == "OBH-SELL"))
                                                                   && (x.Soano == null || x.Soano == "")).ToList();

                    if (surchargeCredit.Count() > 0)
                    {
                        //Update PaySOANo cho CsShipmentSurcharge có type BUY hoặc OBH-BUY(Payer)
                        surchargeCredit.ForEach(a =>
                        {
                            a.PaySoano = soa.Soano;
                            a.UserModified = currentUser.UserID;
                            a.DatetimeModified = DateTime.Now;
                        }
                        );
                    }

                    if (surchargeDebit.Count() > 0)
                    {
                        //Update SOANo cho CsShipmentSurcharge có type là SELL hoặc OBH-SELL(Receiver)
                        surchargeDebit.ForEach(a =>
                        {
                            a.Soano = soa.Soano;
                            a.UserModified = currentUser.UserID;
                            a.DatetimeModified = DateTime.Now;
                        }
                        );
                    }

                }
                dc.SaveChanges();

                return new HandleState();
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        public List<ChargeShipmentModel> GetListMoreChargeByCondition(MoreChargeShipmentCriteria criteria)
        {
            var moreChargeShipmentList = GetSpcMoreChargeShipmentByCondition(criteria);
            
            List<Surcharge> Surcharges = new List<Surcharge>();
            if (criteria.ChargeShipments != null)
            {
                foreach (var item in criteria.ChargeShipments.Where(x => x.SOANo == null || x.SOANo == "").ToList())
                {
                    Surcharges.Add(new Surcharge { surchargeId = item.ID, type = item.Type });
                }
            }

            //Lấy ra các charge chưa tồn tại trong list criteria.Surcharges(Các Id của charge đã có trong kết quả search ở form info)
            List<spc_GetListMoreChargeMasterByCondition> charges = new List<spc_GetListMoreChargeMasterByCondition>();
            charges = moreChargeShipmentList.Where(x => Surcharges != null
                                                         && !Surcharges.Where(c => c.surchargeId == x.ID && c.type == x.Type).Any() ).ToList();            
            var dataMapMoreChargeShipment = mapper.Map<List<spc_GetListMoreChargeMasterByCondition>, List<ChargeShipmentModel>>(charges);

            return dataMapMoreChargeShipment;
        }

        private List<spc_GetListMoreChargeMasterByCondition> GetSpcMoreChargeShipmentByCondition(MoreChargeShipmentCriteria criteria)
        {
            DbParameter[] parameters =
            {
                SqlParam.GetParameter("CurrencyLocal", criteria.CurrencyLocal),
                SqlParam.GetParameter("CustomerID", criteria.CustomerID),
                SqlParam.GetParameter("DateType", criteria.DateType),
                SqlParam.GetParameter("FromDate", criteria.FromDate),
                SqlParam.GetParameter("ToDate", criteria.ToDate),
                SqlParam.GetParameter("Type", criteria.Type),
                SqlParam.GetParameter("IsOBH", criteria.IsOBH),
                SqlParam.GetParameter("StrCreators", criteria.StrCreators),
                SqlParam.GetParameter("StrCharges", criteria.StrCharges),
                SqlParam.GetParameter("inSOA", criteria.InSoa),
                SqlParam.GetParameter("jobId", criteria.JobId),
                SqlParam.GetParameter("hbl", criteria.Hbl),
                SqlParam.GetParameter("mbl", criteria.Mbl),
                SqlParam.GetParameter("cdNote", criteria.CDNote)
            };
            return ((eFMSDataContext)DataContext.DC).ExecuteProcedure<spc_GetListMoreChargeMasterByCondition>(parameters);
        }

        public AcctSOADetailResult AddMoreCharge(AddMoreChargeCriteria criteria)
        {
            var data = new AcctSOADetailResult();
            if(criteria!= null)
            {
                if(criteria.ChargeShipmentsCurrent != null)
                {
                    if (criteria.ChargeShipmentsAddMore != null)
                    {
                        foreach (var item in criteria.ChargeShipmentsAddMore)
                        {
                            criteria.ChargeShipmentsCurrent.Add(item);
                        }
                    }
                    data.Shipment = criteria.ChargeShipmentsCurrent.Where(x => x.HBL != null).GroupBy(x => x.HBL).Count();
                    data.TotalCharge = criteria.ChargeShipmentsCurrent.Count();
                    data.ChargeShipments = criteria.ChargeShipmentsCurrent;
                    data.AmountDebitLocal = criteria.ChargeShipmentsCurrent.Sum(x => x.AmountDebitLocal);
                    data.AmountCreditLocal = criteria.ChargeShipmentsCurrent.Sum(x => x.AmountCreditLocal);
                    data.AmountDebitUSD = criteria.ChargeShipmentsCurrent.Sum(x => x.AmountDebitUSD);
                    data.AmountCreditUSD = criteria.ChargeShipmentsCurrent.Sum(x => x.AmountCreditUSD);
                }
            }            
            return data;
        }

        public ExportSOADetailResult GetDataExportSOABySOANo(string soaNo, string currencyLocal)
        {
            var data = GetSpcDataExportSOABySOANo(soaNo, currencyLocal);
            var dataMap = mapper.Map<List<spc_GetDataExportSOABySOANo>, List<ExportSOAModel>>(data);
            var result = new ExportSOADetailResult
            {
                ListCharges = dataMap,
                TotalDebitExchange = dataMap.Where(x => x.DebitExchange != null).Sum(x => x.DebitExchange),
                TotalCreditExchange = dataMap.Where(x => x.CreditExchange != null).Sum(x => x.CreditExchange)
            };
            return result;
        }

        private List<spc_GetDataExportSOABySOANo> GetSpcDataExportSOABySOANo(string soaNo, string currencyLocal)
        {
            DbParameter[] parameters =
            {
                SqlParam.GetParameter("soaNo", soaNo),
                SqlParam.GetParameter("currencyLocal", currencyLocal)
            };
            return ((eFMSDataContext)DataContext.DC).ExecuteProcedure<spc_GetDataExportSOABySOANo>(parameters);
        }
    }
}
