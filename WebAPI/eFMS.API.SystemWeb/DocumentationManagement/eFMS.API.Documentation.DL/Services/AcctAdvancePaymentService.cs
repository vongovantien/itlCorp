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

        public List<AcctAdvanceRequestResult> Paging(AcctAdvancePaymentCriteria criteria, int page, int size, out int rowsCount)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var advance = dc.AcctAdvancePayment;
            var request = dc.AcctAdvanceRequest;
            var user = dc.SysUser;
            
            var data = from re in request
                       join ad in advance on re.AdvanceNo equals ad.AdvanceNo
                       join u in user on ad.Requester equals u.Id into u2
                       from u3 in u2.DefaultIfEmpty()
                       where
                        (
                            criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0 ?
                            (
                                (
                                       criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(re.AdvanceNo) : 1 == 1
                                    || criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(re.Hbl) : 1 == 1
                                    || criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(re.Mbl) : 1 == 1
                                    || criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(re.CustomNo) : 1 == 1
                                    || criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(re.JobId) : 1 == 1
                                )
                            )
                            :
                            (
                                1 == 1
                            )
                         )

                         &&

                         (
                            !string.IsNullOrEmpty(criteria.Requester) ?
                                ad.Requester == criteria.Requester
                            :
                                1 == 1
                         )
                         &&
                         (
                            criteria.RequestDateFrom.HasValue && criteria.RequestDateTo.HasValue ?
                                ad.RequestDate >= criteria.RequestDateFrom
                                && ad.RequestDate <= criteria.RequestDateTo
                            :
                                1 == 1
                         )
                         &&
                         (
                            !string.IsNullOrEmpty(criteria.StatusApproval) && !criteria.StatusApproval.Equals("All") ?
                                ad.StatusApproval == criteria.StatusApproval
                            :
                                1 == 1
                         )
                         &&
                         (
                            criteria.AdvanceModifiedDateFrom.HasValue && criteria.AdvanceModifiedDateTo.HasValue ?
                                //Convert DatetimeModified về date nếu DatetimeModified có value
                                ad.DatetimeModified.Value.Date >= (criteria.AdvanceModifiedDateFrom.HasValue ? criteria.AdvanceModifiedDateFrom.Value.Date : criteria.AdvanceModifiedDateFrom)
                                && ad.DatetimeModified.Value.Date <= (criteria.AdvanceModifiedDateTo.HasValue ? criteria.AdvanceModifiedDateTo.Value.Date : criteria.AdvanceModifiedDateTo)
                            :
                                1 == 1
                         )
                         &&
                         (
                           !string.IsNullOrEmpty(criteria.StatusPayment) && !criteria.StatusPayment.Equals("All") ?
                                re.StatusPayment == criteria.StatusPayment
                           :
                                1 == 1
                         )
                         &&
                         (
                           !string.IsNullOrEmpty(criteria.PaymentMethod) && !criteria.PaymentMethod.Equals("All") ?
                                ad.PaymentMethod == criteria.PaymentMethod
                           :
                                1 == 1
                          )

                       select new AcctAdvanceRequestResult
                       {
                           Id = re.Id,
                           AdvanceNo = re.AdvanceNo,
                           CustomNo = re.CustomNo,
                           JobId = re.JobId,
                           Hbl = re.Hbl,
                           Description = re.Description,
                           Amount = re.Amount,
                           RequestCurrency = re.RequestCurrency,
                           Requester = ad.Requester,
                           RequesterName = u3.Username,
                           RequestDate = ad.RequestDate,
                           DeadlinePayment = ad.DeadlinePayment,
                           AdvanceDatetimeModified = ad.DatetimeModified,
                           StatusApproval = ad.StatusApproval,
                           StatusPayment = re.StatusPayment,
                           PaymentMethod = ad.PaymentMethod
                       };

            //Sắp xếp giảm dần theo Advance DatetimeModified
            data = data.OrderByDescending(x => x.AdvanceDatetimeModified);

            //Phân trang
            rowsCount = (data.Count() > 0) ? data.Count() : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }

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
            string month = DateTime.Now.ToString("DDMMYYYY").Substring(2, 2);
            string prefix = "AD" + year + month + "/";
            string stt;

            //Lấy ra dòng cuối cùng của table acctAdvancePayment
            var rowlast = dc.AcctAdvancePayment.LastOrDefault();

            if (rowlast == null)
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
                    result = dc.AcctAdvanceRequest.Any(x =>
                      x.JobId == criteria.JobId
                   && x.Hbl == criteria.HBL
                   && x.Mbl == criteria.MBL);
                }
                else //Check trường hợp Update advance payment
                {
                    result = dc.AcctAdvanceRequest.Any(x =>
                      x.JobId == criteria.JobId
                   && x.Hbl == criteria.HBL
                   && x.Mbl == criteria.MBL
                   && x.AdvanceNo != criteria.AdvanceNo);
                }
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public HandleState DeleteAdvanceRequest(Guid idAdvanceRequest)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                //Lấy ra 1 Advance Request dựa vào Id của Advance Request
                var request = dc.AcctAdvanceRequest.Where(x => x.Id == idAdvanceRequest).FirstOrDefault();
                if (request != null)
                {
                    //Đếm số lượng Advance Request của 1 Advance Payment
                    var countRequestOfAdvance = dc.AcctAdvanceRequest.Where(x => x.AdvanceNo == request.AdvanceNo).Count();
                    //Lấy ra Advance Payment dựa vào AdvanceNo
                    var advance = dc.AcctAdvancePayment.Where(x => x.AdvanceNo == request.AdvanceNo).FirstOrDefault();

                    if (advance == null) return new HandleState("Not Found Advance Payment");

                    if (countRequestOfAdvance == 1)
                    {
                        //Xóa Advance Payment nếu số lượng Advance Request của Advance Payment bằng 1
                        dc.AcctAdvancePayment.Remove(advance);
                    }
                    else
                    {
                        //Cập nhật lại UserModified và DatetimeModified của Advance Payment
                        advance.UserModified = currentUser.UserID;
                        advance.DatetimeModified = DateTime.Now;
                        dc.AcctAdvancePayment.Update(advance);
                    }
                    //Xóa 1 Advance Request
                    dc.AcctAdvanceRequest.Remove(request);

                    dc.SaveChanges();
                    return new HandleState();
                }
                else
                {
                    return new HandleState("Not Found Advance Request");
                }
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        public AcctAdvancePaymentModel GetAdvancePaymentByAdvanceNo(string advanceNo)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var advanceModel = new AcctAdvancePaymentModel();
            
            //Lấy ra Advance Payment dựa vào Advance No
            var advance = dc.AcctAdvancePayment.Where(x => x.AdvanceNo == advanceNo).FirstOrDefault();
            //Không tìm thấy Advance Payment thì trả về null
            if (advance == null) return null;
            
            //Lấy ra danh sách Advance Request dựa vào Advance No và sắp xếp giảm dần theo DatetimeModified Advance Request
            var request = dc.AcctAdvanceRequest.Where(x => x.AdvanceNo == advanceNo).OrderByDescending(x => x.DatetimeModified).ToList();
            //Không tìm thấy Advance Request thì trả về null
            if (request == null) return null;
            
            //Mapper AcctAdvancePayment thành AcctAdvancePaymentModel
            advanceModel = mapper.Map<AcctAdvancePaymentModel>(advance);
            //Mapper List<AcctAdvanceRequest> thành List<AcctAdvanceRequestModel>
            advanceModel.AdvanceRequests = mapper.Map<List<AcctAdvanceRequestModel>>(request);
            
            return advanceModel;
        }

        public HandleState UpdateAdvancePayment(AcctAdvancePaymentModel model)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var advance = mapper.Map<AcctAdvancePayment>(model);
                
                advance.DatetimeModified = DateTime.Now;
                advance.UserModified = currentUser.UserID;

                var hs = DataContext.Update(advance, x => x.AdvanceNo == advance.AdvanceNo);

                if (hs.Success)
                {                    
                    var request = mapper.Map<List<AcctAdvanceRequest>>(model.AdvanceRequests);
                    //Lấy ra những request mới (có UserCreated = null)
                    var requestNew = request.Where(x=>x.UserCreated == null || x.UserCreated == "").ToList();
                    if (requestNew != null && requestNew.Count > 0)
                    {
                        requestNew.ForEach(req =>
                        {
                            req.Id = Guid.NewGuid();
                            req.AdvanceNo = advance.AdvanceNo;
                            req.DatetimeCreated = req.DatetimeModified = DateTime.Now;
                            req.UserCreated = req.UserModified = currentUser.UserID;
                            req.StatusPayment = "NotSettled";
                        });
                        dc.AcctAdvanceRequest.AddRange(requestNew);
                    }

                    //Lấy ra những request cũ cần update
                    var requestUpdate = request.Where(x => x.UserCreated != null || x.UserCreated != "").ToList();
                    requestUpdate.ForEach(req=> {
                        req.DatetimeModified = DateTime.Now;
                        req.UserModified = currentUser.UserID;
                    });
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
    }
}
