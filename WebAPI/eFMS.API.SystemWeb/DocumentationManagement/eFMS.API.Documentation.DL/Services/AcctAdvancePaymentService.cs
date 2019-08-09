using AutoMapper;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class AcctAdvancePaymentService : RepositoryBase<AcctAdvancePayment, AcctAdvancePaymentModel>, IAcctAdvancePaymentService
    {
        private readonly ICurrentUser currentUser;
        public AcctAdvancePaymentService(IContextBase<AcctAdvancePayment> repository, IMapper mapper, ICurrentUser user) : base(repository, mapper)
        {
            currentUser = user;
        }

        public List<AcctAdvanceRequestResult> Page()
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var advance = dc.AcctAdvancePayment;
            var request = dc.AcctAdvanceRequest;
            var data = from re in request
                       join ad in request on re.AdvanceNo equals ad.AdvanceNo
                       select new AcctAdvanceRequestResult
                       {
                           Id = re.Id,
                           AdvanceNo = re.AdvanceNo,
                           CustomNo = re.CustomNo,
                           JobId = re.JobId,
                           Hbl = re.Hbl                           
                       };
            return data.ToList();
        }

        /// <summary>
        /// Get shipments (JobId, HBL, MBL) from shipment documentation and shipment operation
        /// </summary>
        /// <returns></returns>
        public List<Shipments> GetShipments()
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var shipmentsOperation = dc.OpsTransaction
                                    .Where(x => x.Hblid != Guid.Empty && x.CurrentStatus != "Canceled")
                                    .Select(x => new Shipments
                                    {
                                        JobId = x.JobNo,
                                        HBL = x.Hwbno,
                                        MBL = x.Mblno
                                    });

            var shipmentsDocumention = (from t in dc.CsTransaction
                                        join td in dc.CsTransactionDetail on t.Id equals td.JobId
                                        select new Shipments
                                        {
                                            JobId = t.JobNo,
                                            HBL = td.Hwbno,
                                            MBL = td.Mawb,
                                        });

            var shipments = shipmentsOperation.Union(shipmentsDocumention).ToList();
            return shipments;
        }

        private string CreateAdvanceNo(eFMSDataContext dc)
        {           
            string year = (DateTime.Now.Year.ToString()).Substring(2, 2);
            string month = DateTime.Now.ToString("DDMMYYYY").Substring(2,2);
            string prefix = "AD" + year + month + "/";
            string stt;

            //Lấy ra dòng cuối cùng của table acctAdvancePayment
            var rowlast = dc.AcctAdvancePayment.LastOrDefault();

            if(rowlast == null)
            {
                stt = "0001";
            }
            else
            {
                var advanceCurrent = rowlast.AdvanceNo;
                var prefixCurrent = advanceCurrent.Substring(2, 4);
                //Reset về 1 khi qua tháng tiếp theo
                if (prefixCurrent != (year + month))
                {
                    stt = "0001";
                }
                else
                {
                    stt = (Convert.ToInt32(advanceCurrent.Substring(7, 4)) + 1).ToString();
                    stt = stt.PadLeft(4, '0');
                }
            }

            return prefix + stt;
        }
        
        public HandleState AddAdvancePayment(AcctAdvancePaymentModel model)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var advance = mapper.Map<AcctAdvancePayment>(model);
                advance.AdvanceNo = model.AdvanceNo = CreateAdvanceNo(dc);
                advance.StatusApproval = model.StatusApproval = "New";

                advance.DatetimeCreated = advance.DatetimeModified = DateTime.Now;
                advance.UserCreated = advance.UserModified = currentUser.UserID;

                var hs = DataContext.Add(advance);
                if (hs.Success)
                {
                    var request = mapper.Map<List<AcctAdvanceRequest>>(model.AdvanceRequests);
                    request.ForEach(req =>
                    {
                        req.Id = Guid.NewGuid();
                        req.AdvanceNo = advance.AdvanceNo;
                        req.DatetimeCreated = req.DatetimeModified = DateTime.Now;
                        req.UserCreated = req.UserModified = currentUser.UserID;
                        req.StatusPayment = "NotSettled";
                    });
                    dc.AcctAdvanceRequest.AddRange(request);
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

        /// <summary>
        /// Kiểm tra lô hàng (JobId, HBL, MBL) đã được add trong advance payment nào hay chưa?
        /// </summary>
        /// <param name="JobId"></param>
        /// <param name="HBL"></param>
        /// <param name="MBL"></param>
        /// <returns>true: đã tồn tại; false: chưa tồn tại</returns>
        public bool CheckShipmentsExistInAdvancePayment(ShipmentAdvancePaymentCriteria criteria)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var result = false;
                //Check trường hợp Add new advance payment
                if (string.IsNullOrEmpty(criteria.AdvanceNo))
                {
                    result  = dc.AcctAdvanceRequest.Any(x =>
                       x.JobId == criteria.JobId
                    && x.Hbl == criteria.HBL
                    && x.Mbl == criteria.MBL);
                }
                else //Check trường hợp Update advance payment
                {
                    result  = dc.AcctAdvanceRequest.Any(x =>
                       x.JobId == criteria.JobId
                    && x.Hbl == criteria.HBL
                    && x.Mbl == criteria.MBL
                    && x.AdvanceNo != criteria.AdvanceNo);
                }
                return result;
            } catch(Exception ex)
            {
                return false;
            }
        }


    }
}
