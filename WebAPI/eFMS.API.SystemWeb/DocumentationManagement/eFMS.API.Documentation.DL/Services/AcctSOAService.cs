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
                    var surcharge = dc.CsShipmentSurcharge.Where(x => model.SurchargeIds != null
                                                                   && model.SurchargeIds.Contains(x.Id)
                                                                   && (x.Soano == null || x.Soano == "")).ToList();

                    if (surcharge.Count() > 0)
                    {
                        //Update SOANo to CsShipmentSurcharge
                        surcharge.ForEach(a =>
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
                SqlParam.GetParameter("soaUserCreate", criteria.SoaUserCreate)
            };
            return ((eFMSDataContext)DataContext.DC).ExecuteProcedure<spc_GetListAcctSOAByMaster>(parameters);
        }

        public HandleState UpdateSOASurCharge(string soaNo)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var surcharge = dc.CsShipmentSurcharge.Where(x => x.Soano == soaNo).ToList();
                if (surcharge.Count() > 0)
                {
                    //Update SOANo = NULL to CsShipmentSurcharge
                    surcharge.ForEach(a =>
                        {
                            a.Soano = null;
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
                StrCodes = soaNo
            };
            var dataSOA = GetListAcctSOA(criteria).AsQueryable();
            var dataMapSOA = mapper.Map<spc_GetListAcctSOAByMaster, AcctSOADetailResult>(dataSOA.FirstOrDefault());

            var chargeShipmentList = GetSpcChargeShipmentBySOANo(soaNo, currencyLocal).ToList();
            var dataMapChargeShipment = mapper.Map<List<spc_GetListChargeShipmentMasterBySOANo>, List<ChargeShipmentModel>>(chargeShipmentList);

            dataMapSOA.ChargeShipments = dataMapChargeShipment;
            dataMapSOA.AmountDebitLocal = chargeShipmentList.Sum(x => x.AmountDebitLocal);
            dataMapSOA.AmountCreditLocal = chargeShipmentList.Sum(x => x.AmountCreditLocal);
            dataMapSOA.AmountDebitUSD = chargeShipmentList.Sum(x => x.AmountDebitUSD);
            dataMapSOA.AmountCreditUSD = chargeShipmentList.Sum(x => x.AmountCreditUSD);

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

                //Gỡ bỏ các charge có SOANo = model.Soano
                UpdateSOASurCharge(model.Soano);

                var soa = mapper.Map<AcctSoa>(model);
                //Update các thông tin của SOA
                //var hs = dc.AcctSoa.Update(soa);
                var hs = DataContext.Update(soa, x => x.Id == soa.Id);

                if (hs.Success)
                {
                    //Duyệt qua các charge được tick chọn. Chỉ lấy ra các charge chưa có gán SOA 
                    var surcharge = dc.CsShipmentSurcharge.Where(x => model.SurchargeIds != null
                                                                   && model.SurchargeIds.Contains(x.Id)
                                                                   && (x.Soano == null || x.Soano == "")).ToList();

                    if (surcharge.Count() > 0)
                    {
                        //Update SOANo to CsShipmentSurcharge
                        surcharge.ForEach(a =>
                            {
                                a.Soano = model.Soano;
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

            List<Guid> SurchargeIds = new List<Guid>();
            if (criteria.ChargeShipments != null)
            {
                foreach (var item in criteria.ChargeShipments)
                {
                    SurchargeIds.Add(item.ID);
                }
            }

            //Lấy ra các charge chưa tồn tại trong list criteria.SurchargeIds(Các Id của charge đã có trong kết quả search ở form info)
            var charges = moreChargeShipmentList.Where(x=> SurchargeIds != null 
                                                        && !SurchargeIds.Contains(x.ID)).ToList();

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
                if(criteria.ChargeShipmentsCurrent != null && criteria.ChargeShipmentsAddMore != null)
                {
                    foreach(var item in criteria.ChargeShipmentsAddMore)
                    {
                        criteria.ChargeShipmentsCurrent.Add(item);
                    }
                }
            }

            data.ChargeShipments = criteria.ChargeShipmentsCurrent;
            data.AmountDebitLocal = criteria.ChargeShipmentsCurrent.Sum(x => x.AmountDebitLocal);
            data.AmountCreditLocal = criteria.ChargeShipmentsCurrent.Sum(x => x.AmountCreditLocal);
            data.AmountDebitUSD = criteria.ChargeShipmentsCurrent.Sum(x => x.AmountDebitUSD);
            data.AmountCreditUSD = criteria.ChargeShipmentsCurrent.Sum(x => x.AmountCreditUSD);
            
            return data;
        }

    }
}
