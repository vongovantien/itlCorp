using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Models.ReportResults;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using eFMS.API.Common;

namespace eFMS.API.Documentation.DL.Services
{
    public class AcctAdvancePaymentService : RepositoryBase<AcctAdvancePayment, AcctAdvancePaymentModel>, IAcctAdvancePaymentService
    {
        private readonly ICurrentUser currentUser;
        private readonly IOpsTransactionService opsTransactionService;
        public AcctAdvancePaymentService(IContextBase<AcctAdvancePayment> repository, IMapper mapper, ICurrentUser user, IOpsTransactionService ops) : base(repository, mapper)
        {
            currentUser = user;
            opsTransactionService = ops;
        }

        public List<AcctAdvancePaymentResult> Paging(AcctAdvancePaymentCriteria criteria, int page, int size, out int rowsCount)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var advance = dc.AcctAdvancePayment;
            var request = dc.AcctAdvanceRequest;
            var user = dc.SysUser;

            var data = from ad in advance
                       join u in user on ad.Requester equals u.Id into u2
                       from u in u2.DefaultIfEmpty()
                       join re in request on ad.AdvanceNo equals re.AdvanceNo into re2
                       from re in re2.DefaultIfEmpty()
                       where
                        (
                            criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0 ?
                            (
                                (
                                       (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(re.AdvanceNo) : 1 == 1)
                                    || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(re.Hbl) : 1 == 1)
                                    || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(re.Mbl) : 1 == 1)
                                    || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(re.CustomNo) : 1 == 1)
                                    || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(re.JobId) : 1 == 1)
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
                                //Convert RequestDate về date nếu RequestDate có value
                                ad.RequestDate.Value.Date >= (criteria.RequestDateFrom.HasValue ? criteria.RequestDateFrom.Value.Date : criteria.RequestDateFrom)
                                && ad.RequestDate.Value.Date <= (criteria.RequestDateTo.HasValue ? criteria.RequestDateTo.Value.Date : criteria.RequestDateTo)
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

                       select new AcctAdvancePaymentResult
                       {
                           Id = ad.Id,
                           AdvanceNo = ad.AdvanceNo,
                           AdvanceNote = ad.AdvanceNote,
                           AdvanceCurrency = ad.AdvanceCurrency,
                           Requester = ad.Requester,
                           RequesterName = (u.Username),
                           RequestDate = ad.RequestDate,
                           DeadlinePayment = ad.DeadlinePayment,
                           UserCreated = ad.UserCreated,
                           DatetimeCreated = ad.DatetimeCreated,
                           UserModified = ad.UserModified,
                           DatetimeModified = ad.DatetimeModified,
                           StatusApproval = ad.StatusApproval,
                           AdvanceStatusPayment =
                            request.Where(x => x.StatusPayment == "NotSettled" && x.AdvanceNo == ad.AdvanceNo).Count() == request.Where(x => x.AdvanceNo == ad.AdvanceNo).Count()
                            ?
                                "NotSettled"
                            :
                                request.Where(x => x.StatusPayment == "Settled" && x.AdvanceNo == ad.AdvanceNo).Count() == request.Where(x => x.AdvanceNo == ad.AdvanceNo).Count() ?
                                    "Settled"
                                :
                                    "PartialSettlement",
                           PaymentMethod = ad.PaymentMethod,
                           Amount = re.Amount
                       };

            //Gom nhóm và Sắp xếp giảm dần theo Advance DatetimeModified
            data = data.GroupBy(x => new
            {
                x.Id,
                x.AdvanceNo,
                x.AdvanceNote,
                x.AdvanceCurrency,
                x.Requester,
                x.RequesterName,
                x.RequestDate,
                x.DeadlinePayment,
                x.UserCreated,
                x.DatetimeCreated,
                x.UserModified,
                x.DatetimeModified,
                x.StatusApproval,
                x.AdvanceStatusPayment,
                x.PaymentMethod
            }).Select(s => new AcctAdvancePaymentResult
            {
                Id = s.Key.Id,
                AdvanceNo = s.Key.AdvanceNo,
                AdvanceNote = s.Key.AdvanceNote,
                AdvanceCurrency = s.Key.AdvanceCurrency,
                Requester = s.Key.Requester,
                RequesterName = s.Key.RequesterName,
                RequestDate = s.Key.RequestDate,
                DeadlinePayment = s.Key.DeadlinePayment,
                UserCreated = s.Key.UserCreated,
                DatetimeCreated = s.Key.DatetimeCreated,
                UserModified = s.Key.UserModified,
                DatetimeModified = s.Key.DatetimeModified,
                StatusApproval = s.Key.StatusApproval,
                AdvanceStatusPayment = s.Key.AdvanceStatusPayment,
                PaymentMethod = s.Key.PaymentMethod,
                Amount = s.Sum(su => su.Amount)
            }
            ).OrderByDescending(orb => orb.DatetimeModified);

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

        public List<AcctAdvanceRequestModel> GetGroupRequestsByAdvanceNo(string advanceNo)
        {
            //Sum(Amount) theo lô hàng (JobId, HBL)
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var list = dc.AcctAdvanceRequest.Where(x => x.AdvanceNo == advanceNo)
                .GroupBy(g => new { g.JobId, g.Hbl, g.CustomNo })
                .Select(se => new AcctAdvanceRequest
                {
                    JobId = se.First().JobId,
                    Hbl = se.First().Hbl,
                    CustomNo = se.First().CustomNo,
                    Amount = se.Sum(s => s.Amount),
                    RequestCurrency = se.First().RequestCurrency,
                    StatusPayment = se.First().StatusPayment
                }).ToList();
            var datamap = mapper.Map<List<AcctAdvanceRequestModel>>(list);
            return datamap;
        }

        public List<AcctAdvanceRequestModel> GetGroupRequestsByAdvanceId(Guid advanceId)
        {
            //Sum(Amount) theo lô hàng (JobId, HBL)
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var list = dc.AcctAdvanceRequest.Where(x => x.Id == advanceId)
                .GroupBy(g => new { g.JobId, g.Hbl, g.CustomNo })
                .Select(se => new AcctAdvanceRequest
                {
                    JobId = se.First().JobId,
                    Hbl = se.First().Hbl,
                    CustomNo = se.First().CustomNo,
                    Amount = se.Sum(s => s.Amount),
                    RequestCurrency = se.First().RequestCurrency,
                    StatusPayment = se.First().StatusPayment
                }).ToList();
            var datamap = mapper.Map<List<AcctAdvanceRequestModel>>(list);
            return datamap;
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
            var rowlast = dc.AcctAdvancePayment.OrderByDescending(x => x.AdvanceNo).FirstOrDefault();

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
                advance.Id = model.Id = Guid.NewGuid();
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

        public HandleState DeleteAdvancePayment(string advanceNo)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var requests = dc.AcctAdvanceRequest.Where(x => x.AdvanceNo == advanceNo).ToList();
                if (requests == null) return new HandleState("Not Found Advance Request");
                //Xóa các Advance Request có AdvanceNo = AdvanceNo truyền vào
                dc.AcctAdvanceRequest.RemoveRange(requests);
                var advance = dc.AcctAdvancePayment.Where(x => x.AdvanceNo == advanceNo).FirstOrDefault();
                if (advance == null) return new HandleState("Not Found Advance Payment");
                dc.AcctAdvancePayment.Remove(advance);
                dc.SaveChanges();
                return new HandleState();
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

        public AcctAdvancePaymentModel GetAdvancePaymentByAdvanceId(Guid advanceId)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var advanceModel = new AcctAdvancePaymentModel();

            //Lấy ra Advance Payment dựa vào Advance Id
            var advance = dc.AcctAdvancePayment.Where(x => x.Id == advanceId).FirstOrDefault();
            //Không tìm thấy Advance Payment thì trả về null
            if (advance == null) return null;

            //Lấy ra danh sách Advance Request dựa vào Advance No và sắp xếp giảm dần theo DatetimeModified Advance Request
            var request = dc.AcctAdvanceRequest.Where(x => x.AdvanceNo == advance.AdvanceNo).OrderByDescending(x => x.DatetimeModified).ToList();
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

                var advanceCurrent = dc.AcctAdvancePayment.Where(x => x.Id == advance.Id).FirstOrDefault();
                advance.DatetimeCreated = advanceCurrent.DatetimeCreated;
                advance.UserCreated = advanceCurrent.UserCreated;

                advance.DatetimeModified = DateTime.Now;
                advance.UserModified = currentUser.UserID;

                var hs = DataContext.Update(advance, x => x.Id == advance.Id);

                if (hs.Success)
                {
                    var request = mapper.Map<List<AcctAdvanceRequest>>(model.AdvanceRequests);
                    //Lấy ra các Request cũ cần update
                    var requestUpdate = request.Where(x => x.UserCreated != null || x.UserCreated != "").ToList();

                    //Lấy ra các Request có cùng AdvanceNo và không tồn tại trong requestUpdate
                    var requestNeedRemove = dc.AcctAdvanceRequest.Where(x => x.AdvanceNo == advance.AdvanceNo && !requestUpdate.Contains(x)).ToList();
                    //Xóa các requestNeedRemove
                    dc.AcctAdvanceRequest.RemoveRange(requestNeedRemove);

                    //Lấy ra những request mới (có UserCreated = null)
                    var requestNew = request.Where(x => x.UserCreated == null || x.UserCreated == "").ToList();
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

                    //Cập nhật những request cũ cần update
                    requestUpdate.ForEach(req =>
                    {
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

        #region PREVIEW ADVANCE PAYMENT
        public Crystal Preview(Guid advanceId)
        {
            Crystal result = null;
            string strJobId = "";
            string strHbl = "";
            string strCustomNo = "";
            int contQty = 0;
            decimal nw = 0;
            int psc = 0;
            decimal cbm = 0;

            var advance = GetAdvancePaymentByAdvanceId(advanceId);

            if (advance == null) return null;

            if (advance.AdvanceRequests.Count > 0)
            {
                foreach (var jobId in advance.AdvanceRequests.GroupBy(x=>x.JobId).Select(x => x.FirstOrDefault().JobId))
                {
                    OpsTransactionCriteria criteria = new OpsTransactionCriteria
                    {
                        JobNo = jobId
                    };
                    //Lấy ra NW, CBM, PSC, Container Qty
                    var ops = opsTransactionService.Query(criteria).FirstOrDefault();
                    if(ops != null)
                    {
                        contQty += ops.SumContainers.HasValue ? ops.SumContainers.Value : 0;
                        nw += ops.SumNetWeight.HasValue ? ops.SumNetWeight.Value : 0;
                        psc += ops.SumPackages.HasValue ? ops.SumPackages.Value : 0;
                        cbm += ops.SumCbm.HasValue ? ops.SumCbm.Value : 0;
                    }

                    //Lấy ra chuỗi JobId
                    strJobId += !string.IsNullOrEmpty(jobId) ? jobId + "," : "";
                }

                //Lấy ra chuỗi HBL
                foreach (var hbl in advance.AdvanceRequests.GroupBy(x=>x.Hbl).Select(x => x.FirstOrDefault().Hbl))
                {
                    strHbl += !string.IsNullOrEmpty(hbl) ? hbl + "," : "";
                }

                //Lấy ra chuỗi CustomNo
                foreach (var customNo in advance.AdvanceRequests.GroupBy(x=>x.CustomNo).Select(x => x.FirstOrDefault().CustomNo))
                {
                    strCustomNo += !string.IsNullOrEmpty(customNo) ? customNo + "," : "";
                }

                strJobId += ")";
                strJobId = strJobId.Replace(",)", "");
                strHbl += ")";
                strHbl = strHbl.Replace(",)", "");
                strCustomNo += ")";
                strCustomNo = strCustomNo.Replace(",)", "").Replace(")","");
            }

            //Lấy ra tên requester
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var employeeId = dc.SysUser.Where(x => x.Id == advance.Requester).Select(x => x.EmployeeId).FirstOrDefault();
            var requesterName = dc.SysEmployee.Where(x=>x.Id == employeeId).Select(x=>x.EmployeeNameVn).FirstOrDefault();

            var acctAdvance = new AdvancePaymentRequestReport
            {
                AdvID = advance.AdvanceNo,
                RefNo = "N/A",
                AdvDate = advance.RequestDate.Value.Date,
                AdvTo = "N/A",
                AdvContactID = "N/A",
                AdvContact = requesterName,//cần lấy ra username
                AdvAddress = "",
                AdvValue = advance.AdvanceRequests.Sum(x=>x.Amount),
                AdvCurrency = advance.AdvanceCurrency,
                AdvCondition = advance.AdvanceNote,
                AdvRef = strJobId,
                AdvHBL = strHbl,
                AdvPaymentDate = null,
                AdvPaymentNote = "N/A",
                AdvDpManagerID = "N/A",
                AdvDpManagerStickDeny = null,
                AdvDpManagerStickApp = null,
                AdvDpManagerName = "",
                AdvDpSignDate = null,
                AdvAcsDpManagerID = "N/A",
                AdvAcsDpManagerStickDeny = null,
                AdvAcsDpManagerStickApp = null,
                AdvAcsDpManagerName = "N/A",
                AdvAcsSignDate = null,
                AdvBODID = "N/A",
                AdvBODStickDeny = null,
                AdvBODStickApp = null,
                AdvBODName = "N/A",
                AdvBODSignDate = null,
                AdvCashier = "N/A",
                AdvCashierName = "N/A",
                CashedDate = null,
                Saved = null,
                SettleNo = "N/A",
                PaidDate = null,
                AmountSettle = 0,
                SettleCurrency = "N/A",
                ClearStatus = null,
                Status = "N/A",
                AcsApproval = null,
                Description = "N/A",
                JobNo = "N/A",
                MAWB = "N/A",
                Amount = 0,
                Currency = "N/A",
                ExchangeRate = 0,
                TotalAmount = 0,
                PaymentDate = advance.DeadlinePayment.Value.Date,
                InvoiceNo = "N/A",
                CustomID = strCustomNo,
                HBLNO = "N/A",
                Norm = null,
                Validfee = null,
                Others = null,
                CSApp = null,
                CSDecline = null,
                CSUser = "N/A",
                CSAppDate = null,
                Customer = "",
                Shipper = "",
                Consignee = "",
                ContQty = contQty.ToString(),
                Noofpieces = psc,
                UnitPieaces = "N/A",
                GW = 0,
                NW = nw,
                CBM = cbm,
                ServiceType = "N/A",
                AdvCSName = "",
                AdvCSSignDate = null,
                AdvCSStickApp = null,
                AdvCSStickDeny = null,
                TotalNorm = advance.AdvanceRequests.Where(x => x.AdvanceType == "Norm").Sum(x => x.Amount),
                TotalInvoice = advance.AdvanceRequests.Where(x => x.AdvanceType == "Invoice").Sum(x => x.Amount),
                TotalOrther = advance.AdvanceRequests.Where(x => x.AdvanceType == "Other").Sum(x => x.Amount)
            };

            acctAdvance.TotalNorm = acctAdvance.TotalNorm != 0 ? acctAdvance.TotalNorm : null;
            acctAdvance.TotalInvoice = acctAdvance.TotalInvoice != 0 ? acctAdvance.TotalInvoice : null;
            acctAdvance.TotalOrther = acctAdvance.TotalOrther != 0 ? acctAdvance.TotalOrther : null;

            var listAdvance = new List<AdvancePaymentRequestReport>
            {
                acctAdvance
            };

            //Chuyển tiền Amount thành chữ
            decimal _amount = acctAdvance.AdvValue.HasValue ? acctAdvance.AdvValue.Value : 0;
            //decimal _amount2 = 3992.123M;
            
            var _currency = advance.AdvanceCurrency == "VND" ?
                       (_amount % 1 > 0 ? "đồng lẻ" : "đồng chẵn") 
                    : 
                    advance.AdvanceCurrency;

            var _inword = InWordCurrency.ConvertNumberCurrencyToString(_amount, _currency);

            var parameter = new AdvancePaymentRequestReportParams
            {
                CompanyName = "INDO TRANS LOGISTICS CORPORATION‎",
                CompanyAddress1 = "52‎-‎54‎-‎56 ‎Truong Son St‎.‎, ‎Tan Binh Dist‎.‎, ‎HCM City‎, ‎Vietnam‎",
                CompanyAddress2 = "",
                Website = "www‎.‎itlvn‎.‎com‎",
                Contact = "Tel‎: (‎84‎-‎8‎) ‎3948 6888  Fax‎: +‎84 8 38488 570‎",
                Inword = _inword
            };

            result = new Crystal
            {
                ReportName = "AdvancePaymentRequest.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listAdvance);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }

        public Crystal Preview(AcctAdvancePaymentModel advance)
        {
            Crystal result = null;
            string strJobId = "";
            string strHbl = "";
            string strCustomNo = "";
            int contQty = 0;
            decimal nw = 0;
            int psc = 0;
            decimal cbm = 0;

            if (advance == null) return null;

            if (advance.AdvanceRequests.Count > 0)
            {
                foreach (var jobId in advance.AdvanceRequests.GroupBy(x=>x.JobId).Select(x => x.FirstOrDefault().JobId))
                {
                    OpsTransactionCriteria criteria = new OpsTransactionCriteria
                    {
                        JobNo = jobId
                    };
                    //Lấy ra NW, CBM, PSC, Container Qty
                    var ops = opsTransactionService.Query(criteria).FirstOrDefault();
                    if (ops != null)
                    {
                        contQty += ops.SumContainers.HasValue ? ops.SumContainers.Value : 0;
                        nw += ops.SumNetWeight.HasValue ? ops.SumNetWeight.Value : 0;
                        psc += ops.SumPackages.HasValue ? ops.SumPackages.Value : 0;
                        cbm += ops.SumCbm.HasValue ? ops.SumCbm.Value : 0;
                    }

                    //Lấy ra chuỗi JobId
                    strJobId += !string.IsNullOrEmpty(jobId) ? jobId + "," : "";
                }

                //Lấy ra chuỗi HBL
                foreach (var hbl in advance.AdvanceRequests.GroupBy(x=>x.Hbl).Select(x => x.FirstOrDefault().Hbl))
                {
                    strHbl += !string.IsNullOrEmpty(hbl) ? hbl + "," : "";
                }

                //Lấy ra chuỗi CustomNo
                foreach (var customNo in advance.AdvanceRequests.GroupBy(x=>x.CustomNo).Select(x => x.FirstOrDefault().CustomNo))
                {
                    strCustomNo += !string.IsNullOrEmpty(customNo) ? customNo + "," : "";
                }

                strJobId += ")";
                strJobId = strJobId.Replace(",)", "");
                strHbl += ")";
                strHbl = strHbl.Replace(",)", "");
                strCustomNo += ")";
                strCustomNo = strCustomNo.Replace(",)", "").Replace(")","");
            }

            //Lấy ra tên requester
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var employeeId = dc.SysUser.Where(x => x.Id == advance.Requester).Select(x => x.EmployeeId).FirstOrDefault();
            var requesterName = dc.SysEmployee.Where(x => x.Id == employeeId).Select(x => x.EmployeeNameVn).FirstOrDefault();

            var acctAdvance = new AdvancePaymentRequestReport
            {
                AdvID = advance.AdvanceNo,
                RefNo = "N/A",
                AdvDate = advance.RequestDate.Value.Date,
                AdvTo = "N/A",
                AdvContactID = "N/A",
                AdvContact = requesterName,
                AdvAddress = "",
                AdvValue = advance.AdvanceRequests.Sum(x => x.Amount),
                AdvCurrency = advance.AdvanceCurrency,
                AdvCondition = advance.AdvanceNote,
                AdvRef = strJobId,
                AdvHBL = strHbl,
                AdvPaymentDate = null,
                AdvPaymentNote = "N/A",
                AdvDpManagerID = "N/A",
                AdvDpManagerStickDeny = null,
                AdvDpManagerStickApp = null,
                AdvDpManagerName = "",
                AdvDpSignDate = null,
                AdvAcsDpManagerID = "N/A",
                AdvAcsDpManagerStickDeny = null,
                AdvAcsDpManagerStickApp = null,
                AdvAcsDpManagerName = "N/A",
                AdvAcsSignDate = null,
                AdvBODID = "N/A",
                AdvBODStickDeny = null,
                AdvBODStickApp = null,
                AdvBODName = "N/A",
                AdvBODSignDate = null,
                AdvCashier = "N/A",
                AdvCashierName = "N/A",
                CashedDate = null,
                Saved = null,
                SettleNo = "N/A",
                PaidDate = null,
                AmountSettle = 0,
                SettleCurrency = "N/A",
                ClearStatus = null,
                Status = "N/A",
                AcsApproval = null,
                Description = "N/A",
                JobNo = "N/A",
                MAWB = "N/A",
                Amount = 0,
                Currency = "N/A",
                ExchangeRate = 0,
                TotalAmount = 0,
                PaymentDate = advance.DeadlinePayment.Value.Date,
                InvoiceNo = "N/A",
                CustomID = strCustomNo,
                HBLNO = "N/A",
                Norm = null,
                Validfee = null,
                Others = null,
                CSApp = null,
                CSDecline = null,
                CSUser = "N/A",
                CSAppDate = null,
                Customer = "",
                Shipper = "",
                Consignee = "",
                ContQty = contQty.ToString(),
                Noofpieces = psc,
                UnitPieaces = "N/A",
                GW = 0,
                NW = nw,
                CBM = cbm,
                ServiceType = "N/A",
                AdvCSName = "",
                AdvCSSignDate = null,
                AdvCSStickApp = null,
                AdvCSStickDeny = null,
                TotalNorm = advance.AdvanceRequests.Where(x=>x.AdvanceType == "Norm").Sum(x=>x.Amount),
                TotalInvoice = advance.AdvanceRequests.Where(x => x.AdvanceType == "Invoice").Sum(x => x.Amount),
                TotalOrther = advance.AdvanceRequests.Where(x => x.AdvanceType == "Other").Sum(x => x.Amount)
            };

            acctAdvance.TotalNorm = acctAdvance.TotalNorm != 0 ? acctAdvance.TotalNorm : null;
            acctAdvance.TotalInvoice = acctAdvance.TotalInvoice != 0 ? acctAdvance.TotalInvoice : null;
            acctAdvance.TotalOrther = acctAdvance.TotalOrther != 0 ? acctAdvance.TotalOrther : null;

            var listAdvance = new List<AdvancePaymentRequestReport>
            {
                acctAdvance
            };

            //Chuyển tiền Amount thành chữ
            decimal _amount = acctAdvance.AdvValue.HasValue ? acctAdvance.AdvValue.Value : 0;

            var _currency = advance.AdvanceCurrency == "VND" ?
                       (_amount % 1 > 0 ? "đồng lẻ" : "đồng chẵn")
                    :
                    advance.AdvanceCurrency;
            var _inword = InWordCurrency.ConvertNumberCurrencyToString(_amount, _currency);

            var parameter = new AdvancePaymentRequestReportParams
            {
                CompanyName = "INDO TRANS LOGISTICS CORPORATION‎",
                CompanyAddress1 = "52‎-‎54‎-‎56 ‎Truong Son St‎.‎, ‎Tan Binh Dist‎.‎, ‎HCM City‎, ‎Vietnam‎",
                CompanyAddress2 = "‎",
                Website = "www‎.‎itlvn‎.‎com‎",
                Contact = "Tel‎: (‎84‎-‎8‎) ‎3948 6888  Fax‎: +‎84 8 38488 570‎",
                Inword = _inword
            };

            result = new Crystal
            {
                ReportName = "AdvancePaymentRequest.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listAdvance);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }
        #endregion PREVIEW ADVANCE PAYMENT

        #region APPROVAL ADVANCE PAYMENT
        //Inset Or Update AcctApproveAdvance by AdvanceNo
        private HandleState InsertOrUpdateApprovalAdvance(AcctApproveAdvance acctApprove)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;

                //Lấy ra các user Leader, Manager Dept của user requester, user Accountant, BUHead(nếu có)
                acctApprove.Leader = null;
                acctApprove.Manager = null;
                acctApprove.Accountant = null;
                acctApprove.Buhead = null;

                var checkExistsApproveByAdvanceNo = dc.AcctApproveAdvance.Where(x => x.AdvanceNo == acctApprove.AdvanceNo).FirstOrDefault();
                if (checkExistsApproveByAdvanceNo == null) //Insert AcctApproveAdvance
                {
                    acctApprove.Id = Guid.NewGuid();
                    acctApprove.UserCreated = acctApprove.UserModified = "admin";
                    acctApprove.DateCreated = acctApprove.DateModified = DateTime.Now;
                    dc.AcctApproveAdvance.Add(acctApprove);
                }
                else //Update AcctApproveAdvance by AdvanceNo
                {
                    acctApprove.UserCreated = checkExistsApproveByAdvanceNo.UserCreated;
                    acctApprove.DateCreated = checkExistsApproveByAdvanceNo.DateCreated;
                    dc.AcctApproveAdvance.Update(acctApprove);
                }
                dc.SaveChanges();

                //Send mail đề nghị approve đến Leader(Nếu có) nếu không có thì send tới Manager Dept
                //Lấy ra Leader của User & Manager Dept của User Requester
                var sendMailResult = SendMailSuggestApproval(acctApprove.AdvanceNo, "admin");

                return !sendMailResult ? new HandleState("Send Mail Suggest Approval Fail") : new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message.ToString());
            }
        }

        //Lấy ra ds các user được ủy quyền theo nhóm leader, manager department, accountant manager, BUHead
        

        //Check group trước đó đã được approve hay chưa? Nếu group trước đó đã approve thì group hiện tại mới được Approve
        //Nếu group hiện tại đã được approve thì không cho approve nữa
        private HandleState CheckApproved(string advanceNo, string grpOfUser)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;

            var result = new HandleState();
            //Lấy ra Advance Approval dựa vào advanceNo
            var acctApprove = dc.AcctApproveAdvance.Where(x => x.AdvanceNo == advanceNo).FirstOrDefault();
            if (acctApprove == null) return new HandleState("Not Found Advance Approval by AdvanceNo is" + advanceNo);

            //Lấy ra Advance Payment dựa vào advanceNo
            var advance = dc.AcctAdvancePayment.Where(x => x.AdvanceNo == advanceNo).FirstOrDefault();
            if (advance == null) return new HandleState("Not Found Advance Payment by AdvanceNo is" + advanceNo);

            //Trường hợp không có Leader
            if (string.IsNullOrEmpty(acctApprove.Leader))
            {
                //Manager Department Approve
                if (grpOfUser == "CSManager")
                {
                    //Requester đã approve
                    if (!string.IsNullOrEmpty(acctApprove.Requester) && acctApprove.RequesterAprDate != null)
                    {
                        result = new HandleState();
                    }
                    else
                    {
                        result = new HandleState("Not found Requester or Requester not approve");
                    }

                    //Check group CSManager đã approve chưa
                    if (!advance.StatusApproval.Equals("DepartmentManagerApproved") && acctApprove.ManagerAprDate == null)
                    {
                        result = new HandleState();
                    }
                    else
                    {
                        result = new HandleState("Manager Department Approved");
                    }
                }

                if (grpOfUser == "SaleManager")
                {
                    //Requester đã approve
                    if (!string.IsNullOrEmpty(acctApprove.Requester) && acctApprove.RequesterAprDate != null)
                    {
                        result = new HandleState();
                    }
                    else //Requester chưa approve
                    {
                        result = new HandleState("Not found Requester or Requester not approve");
                    }

                    //Check group SaleManager đã approve chưa
                    if (!advance.StatusApproval.Equals("DepartmentManagerApproved") && acctApprove.ManagerAprDate == null)
                    {
                        result = new HandleState();
                    }
                    else
                    {
                        result = new HandleState("Manager Department Approved");
                    }
                }
                

                //Accountant Approve
                if (grpOfUser == "AccountantManager")
                {
                    //Check group DepartmentManager đã được Approve chưa
                    if (!string.IsNullOrEmpty(acctApprove.Manager) 
                        && advance.StatusApproval.Equals("DepartmentManagerApproved") 
                        && acctApprove.ManagerAprDate != null)
                    {
                        result = new HandleState();
                    }
                    else
                    {
                        result = new HandleState("Not found Manager or Manager not approve");
                    }

                    //Check group Accountant đã approve chưa
                    if(!advance.StatusApproval.Equals("Done"))
                    {
                        result = new HandleState();
                    }
                    else
                    {
                        result = new HandleState("Accountant Approved");
                    }
                }
            }
            else //Trường hợp có leader
            {
                //grp Leader là group mẫu (chưa có tồn tại trong db)
                //Leader Approve
                if(grpOfUser == "Leader")
                {
                    if (!string.IsNullOrEmpty(acctApprove.Requester) 
                        && acctApprove.RequesterAprDate != null)
                    {
                        result = new HandleState();
                    }
                    else
                    {
                        result = new HandleState("Not found Requester or Requester not approve");
                    }

                    //Check group Leader đã được approve chưa
                    if (!advance.StatusApproval.Equals("LeaderApproved") 
                        && acctApprove.LeaderAprDate != null)
                    {
                        result = new HandleState();
                    }
                    else
                    {
                        result = new HandleState("Leader Approved");
                    }
                }
                
                //Manager Department Approve
                if (grpOfUser == "CSManager")
                {
                    if (!string.IsNullOrEmpty(acctApprove.Leader) 
                        && advance.StatusApproval.Equals("LeaderApproved")
                        && acctApprove.LeaderAprDate != null)
                    {
                        result = new HandleState();
                    }
                    else
                    {
                        result = new HandleState("Not found Leader or Leader not approve");
                    }

                    //Check group Manager Department đã approve chưa
                    if (!advance.StatusApproval.Equals("DepartmentManagerApproved") 
                        && acctApprove.ManagerAprDate != null)
                    {
                        result = new HandleState();
                    }
                    else
                    {
                        result = new HandleState("Manager Department Approved");
                    }
                }

                if (grpOfUser == "SaleManager")
                {
                    if (!string.IsNullOrEmpty(acctApprove.Leader)
                        && advance.StatusApproval.Equals("LeaderApproved")
                        && acctApprove.LeaderAprDate != null)
                    {
                        result = new HandleState();
                    }
                    else
                    {
                        result = new HandleState("Not found Leader or Leader not approve");
                    }

                    //Check group Manager Department đã approve chưa
                    if (!advance.StatusApproval.Equals("DepartmentManagerApproved")
                        && acctApprove.ManagerAprDate != null)
                    {
                        result = new HandleState();
                    }
                    else
                    {
                        result = new HandleState("Manager Department Approved");
                    }
                }


                //Accountant Approve
                if (grpOfUser == "AccountantManager")
                {
                    //Check group DepartmentManager đã được Approve chưa
                    if (!string.IsNullOrEmpty(acctApprove.Manager) 
                        && advance.StatusApproval.Equals("DepartmentManagerApproved")
                        && acctApprove.ManagerAprDate != null)
                    {
                        result = new HandleState();
                    }
                    else
                    {
                        result = new HandleState("Not found Manager or Manager not approve");
                    }

                    //Check group Accountant đã approve chưa
                    if (!advance.StatusApproval.Equals("Done") 
                        && acctApprove.AccountantAprDate != null)
                    {
                        result = new HandleState();
                    }
                    else
                    {
                        result = new HandleState("Accountant Approved");
                    }
                }
            }

            return result;
        }

        //Update Approval cho từng group
        public HandleState UpdateApproval(string advanceNo, string userApprove)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var approve = dc.AcctApproveAdvance.Where(x => x.AdvanceNo == advanceNo).FirstOrDefault();
            var advance = dc.AcctAdvancePayment.Where(x => x.AdvanceNo == advanceNo).FirstOrDefault();

            if (approve == null) return new HandleState("Not Found Advance Approval by AdvanceNo is " + advanceNo);

            //Lấy ra group của userApprove dựa vào userApprove
            var grpOfUser = "CSManager";

            //Kiểm tra group trước đó đã được approve chưa và group của userApprove đã được approve chưa
            var checkApr = CheckApproved(advanceNo, grpOfUser);
            if (checkApr.Success == false) return new HandleState(checkApr.Message.ToString());

            if (grpOfUser.Equals("Leader"))
            {
                advance.StatusApproval = "LeaderApproved";
                approve.LeaderAprDate = DateTime.Now;//Cập nhật ngày Approve của Leader
            }
            else if (grpOfUser.Equals("CSManager") || grpOfUser.Equals("SaleManager"))
            {
                advance.StatusApproval = "DepartmentManagerApproved";
                approve.ManagerAprDate = DateTime.Now;//Cập nhật ngày Approve của Manager
            }
            else if (grpOfUser.Equals("AccountantManager"))
            {
                advance.StatusApproval = "Done";
                approve.AccountantAprDate = approve.BuheadAprDate = DateTime.Now;//Cập nhật ngày Approve của Accountant & BUHead
            }

            dc.AcctAdvancePayment.Update(advance);
            dc.AcctApproveAdvance.Update(approve);
            dc.SaveChanges();

            //Send mail đề nghị approve
            var sendMailResult = SendMailSuggestApproval(advanceNo, userApprove);

            return sendMailResult ? new HandleState() : new HandleState("Send Mail Suggest Approval Fail");
        }

        //Send Mail đề nghị Approve
        private bool SendMailSuggestApproval(string advanceNo, string userApprove)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;  
            //Lấy ra AdvancePayment dựa vào AdvanceNo
            var advance = dc.AcctAdvancePayment.Where(x => x.AdvanceNo == advanceNo).FirstOrDefault();

            //Lấy ra tên & email của user Requester
            var requesterId = dc.SysUser.Where(x => x.Id == advance.Requester).Select(x => x.EmployeeId).FirstOrDefault();
            var requesterName = dc.SysEmployee.Where(x => x.Id == requesterId).Select(x => x.EmployeeNameVn).FirstOrDefault();
            var emailRequester = dc.SysEmployee.Where(x => x.Id == requesterId).Select(x => x.Email).FirstOrDefault();

            //Lấy ra email của user Approve (Leader, Manager Dept, Accountant, BUHead)
            var approverId = dc.SysUser.Where(x => x.Id == userApprove).Select(x => x.EmployeeId).FirstOrDefault();
            var emailApprover = "andy.hoa@itlvn.com"; //dc.SysEmployee.Where(x => x.Id == approverId).Select(x => x.Email).FirstOrDefault();
            if (emailApprover == null) return false;

            //Lấy ra email của các User được ủy quyền của group của User Approve


            //Mail Info
            string subject = "eFMS - Advance Payment Approval Request from [RequesterName]";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = "<div style='font-family: Time New Roman; font-size: 12pt'><p><i><b>Dear Mr/Mrs [UserName],</b></i></p><p>You have new Advance Payment Approval Request from [RequesterName] as below info:</p><p><i>Anh/ Chị có một yêu cầu duyệt tạm ứng từ [RequesterName] với thông tin như sau:</i></p><ul><li>Advance No / <i>Mã tạm ứng</i> : [AdvanceNo]</li><li>Advance Amount/ <i>Số tiền tạm ứng</i> : [TotalAmount]<li>Shipments/ <i>Lô hàng</i> : [JobIds]</li><li>Requester/ <i>Người đề nghị</i> : [RequesterName]</li><li>Request date/ <i>Thời gian đề nghị</i> : [RequestDate]</li></ul><p>You click here to check more detail and approve.</p><p><i>Anh/ Chị chọn vào đây để biết thêm thông tin chi tiết và phê duyệt.</i></p><p>Thanks and Regards,<p><p><b>eFMS System,</b></p></div>";
            body = body.Replace("[UserName]", userApprove);
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[AdvanceNo]", advanceNo);
            body = body.Replace("[TotalAmount]", "100");
            body = body.Replace("[JobIds]", "LOG1908/0001,LOG1908/0002");
            body = body.Replace("[RequestDate]", advance.RequestDate.Value.ToString("dd/MM/yyyy"));
            List<string> toEmails = new List<string> {
                emailApprover
            };
            List<string> attachments = null;

            //CC cho User Requester để biết được quá trình Approve đã đến bước nào
            //Và các User thuộc group của User Approve được ủy quyền
            List<string> emailCCs = new List<string> {
                emailRequester
            };

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        //Send Mail Approved
        private bool SendMailApproved(string advanceNo, string userApprove, DateTime approvedDate)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            //Lấy ra AdvancePayment dựa vào AdvanceNo
            var advance = dc.AcctAdvancePayment.Where(x => x.AdvanceNo == advanceNo).FirstOrDefault();

            //Lấy ra tên của user Requester
            var requesterId = dc.SysUser.Where(x => x.Id == advance.Requester).Select(x => x.EmployeeId).FirstOrDefault();
            var requesterName = dc.SysEmployee.Where(x => x.Id == requesterId).Select(x => x.EmployeeNameVn).FirstOrDefault();
            var emailRequester = dc.SysEmployee.Where(x => x.Id == requesterId).Select(x => x.Email).FirstOrDefault();            

            //Mail Info
            string subject = "eFMS - Advance Payment from [RequesterName] is approved";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = "<div style='font-family: Time New Roman; font-size: 12pt'><p><i><b>Dear Mr/Mrs [RequesterName],</b></i></p><p>You have an Advance Payment is approved at [ApprovedDate] as below info:</p><p><i>Anh/ Chị có một yêu cầu tạm ứng đã được phê duyệt vào lúc [ApprovedDate] với thông tin như sau:</i></p><ul><li>Advance No / <i>Mã tạm ứng</i> : [AdvanceNo]</li><li>Advance Amount/ <i>Số tiền tạm ứng</i> : [TotalAmount]<li>Shipments/ <i>Lô hàng</i> : [JobIds]</li><li>Requester/ <i>Người đề nghị</i> : [RequesterName]</li><li>Request date/ <i>Thời gian đề nghị</i> : [RequestDate]</li></ul><p>You can click here to check more detail.</p><p><i>Anh/ Chị có thể chọn vào đây để biết thêm thông tin chi tiết.</i></p><p>Thanks and Regards,<p><p><b>eFMS System,</b></p></div>";
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[ApprovedDate]", approvedDate.ToString("HH:MM - DD/MM/YYYY"));
            body = body.Replace("[AdvanceNo]", advanceNo);
            body = body.Replace("[TotalAmount]", "100");
            body = body.Replace("[JobIds]", "LOG1908/0001,LOG1908/0002");
            body = body.Replace("[RequestDate]", advance.RequestDate.Value.ToString("dd/MM/yyyy"));
            List<string> toEmails = new List<string> {
                emailRequester
            };
            List<string> attachments = null;
            List<string> emailCCs = new List<string> {
                
            };

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        //Send Mail Deny Approve (Gửi đến Requester và các Leader, Manager, Accountant, BUHead đã approve trước đó)
        private bool SendMailDeniedApproval(string advanceNo, string userApprove, DateTime DeniedDate)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            //Lấy ra AdvancePayment dựa vào AdvanceNo
            var advance = dc.AcctAdvancePayment.Where(x => x.AdvanceNo == advanceNo).FirstOrDefault();

            //Lấy ra tên của user Requester
            var requesterId = dc.SysUser.Where(x => x.Id == advance.Requester).Select(x => x.EmployeeId).FirstOrDefault();
            var requesterName = dc.SysEmployee.Where(x => x.Id == requesterId).Select(x => x.EmployeeNameVn).FirstOrDefault();
            var emailRequester = dc.SysEmployee.Where(x => x.Id == requesterId).Select(x => x.Email).FirstOrDefault();

            //Mail Info
            string subject = "eFMS - Advance Payment from [RequesterName] is denied";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = "<div style='font-family: Time New Roman; font-size: 12pt'><p><i><b>Dear Mr/Mrs [RequesterName],</b></i></p><p>You have an Advance Payment is denied at [DeniedDate] by as below info:</p><p><i>Anh/ Chị có một yêu cầu tạm ứng đã bị từ chối vào lúc [DeniedDate] by với thông tin như sau:</i></p><ul><li>Advance No / <i>Mã tạm ứng</i> : [AdvanceNo]</li><li>Advance Amount/ <i>Số tiền tạm ứng</i> : [TotalAmount]<li>Shipments/ <i>Lô hàng</i> : [JobIds]</li><li>Requester/ <i>Người đề nghị</i> : [RequesterName]</li><li>Request date/ <i>Thời gian đề nghị</i> : [RequestDate]</li><li>Comment/ <i>Lý do từ chối</i> : [Comment]</li></ul><p>You click here to recheck detail.</p><p><i>Anh/ Chị chọn vào đây để kiểm tra lại thông tin chi tiết.</i></p><p>Thanks and Regards,<p><p><b>eFMS System,</b></p></div>";
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[DeniedDate]", DeniedDate.ToString("HH:MM - DD/MM/YYYY"));
            body = body.Replace("[AdvanceNo]", advanceNo);
            body = body.Replace("[TotalAmount]", "100");
            body = body.Replace("[JobIds]", "LOG1908/0001,LOG1908/0002");
            body = body.Replace("[RequestDate]", advance.RequestDate.Value.ToString("dd/MM/yyyy"));
            body = body.Replace("[Comment]", "Không được phép tam ứng");
            List<string> toEmails = new List<string> {
                emailRequester
            };
            List<string> attachments = null;
            List<string> emailCCs = new List<string>
            {
                //Add các email của các user  đã approve trước đó
            };

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        #endregion APPROVAL ADVANCE PAYMENT

    }
}
