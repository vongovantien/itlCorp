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
                soa.Soano = CreateSoaNo(dc);
                var hs = dc.AcctSoa.Add(soa);

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
    }
}
