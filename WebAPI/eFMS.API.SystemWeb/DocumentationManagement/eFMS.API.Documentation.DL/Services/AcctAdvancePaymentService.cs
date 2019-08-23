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
                foreach (var jobId in advance.AdvanceRequests.Select(x => x.JobId))
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
                    strJobId += jobId + ",";
                }

                //Lấy ra chuỗi HBL
                foreach (var hbl in advance.AdvanceRequests.Select(x => x.Hbl))
                {
                    strHbl += hbl + ",";
                }

                //Lấy ra chuỗi CustomNo
                foreach (var customNo in advance.AdvanceRequests.Select(x => x.CustomNo))
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
                foreach (var jobId in advance.AdvanceRequests.Select(x => x.JobId))
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
                    strJobId += jobId + ",";
                }

                //Lấy ra chuỗi HBL
                foreach (var hbl in advance.AdvanceRequests.Select(x => x.Hbl))
                {
                    strHbl += hbl + ",";
                }

                //Lấy ra chuỗi CustomNo
                foreach (var customNo in advance.AdvanceRequests.Select(x => x.CustomNo))
                {
                    strCustomNo += !string.IsNullOrEmpty(customNo) ? customNo + "," : "";
                }

                strJobId += ")";
                strJobId = strJobId.Replace(",)", "");
                strHbl += ")";
                strHbl = strHbl.Replace(",)", "");
                strCustomNo += ")";
                strCustomNo = strCustomNo.Replace(",)", "");
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

    }
}
