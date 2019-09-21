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
using Microsoft.Extensions.Options;

namespace eFMS.API.Documentation.DL.Services
{
    public class AcctAdvancePaymentService : RepositoryBase<AcctAdvancePayment, AcctAdvancePaymentModel>, IAcctAdvancePaymentService
    {
        private readonly ICurrentUser currentUser;
        private readonly IOpsTransactionService opsTransactionService;
        private readonly IOptions<WebUrl> webUrl;
        readonly IContextBase<AcctAdvanceRequest> acctAdvanceRequestRepo;
        readonly IContextBase<SysUser> sysUserRepo;
        readonly IContextBase<OpsTransaction> opsTransactionRepo;
        readonly IContextBase<CsTransaction> csTransactionRepo;
        readonly IContextBase<CsTransactionDetail> csTransactionDetailRepo;
        readonly IContextBase<SysEmployee> sysEmployeeRepo;
        readonly IContextBase<AcctApproveAdvance> AcctApproveAdvanceRepo;
        readonly IContextBase<SysUserGroup> sysUserGroupRepo;
        readonly IContextBase<CatDepartment> catDepartmentRepo;
        readonly IContextBase<SysGroup> sysGroupRepo;
        readonly IContextBase<SysBranch> sysBranchRepo;

        public AcctAdvancePaymentService(IContextBase<AcctAdvancePayment> repository, 
            IMapper mapper, 
            ICurrentUser user, 
            IOpsTransactionService ops, 
            IOptions<WebUrl> url,
            IContextBase<AcctAdvanceRequest> acctAdvanceRequest,
            IContextBase<SysUser> sysUser,
            IContextBase<OpsTransaction> opsTransaction,
            IContextBase<CsTransaction> csTransaction,
            IContextBase<CsTransactionDetail> csTransactionDetail,
            IContextBase<SysEmployee> sysEmployee,
            IContextBase<AcctApproveAdvance> acctApproveAdvance,
            IContextBase<SysUserGroup> sysUserGroup,
            IContextBase<CatDepartment> catDepartment,
            IContextBase<SysGroup> sysGroup,
            IContextBase<SysBranch> sysBranch) : base(repository, mapper)
        {
            currentUser = user;
            opsTransactionService = ops;
            webUrl = url;
            acctAdvanceRequestRepo = acctAdvanceRequest;
            sysUserRepo = sysUser;
            opsTransactionRepo = opsTransaction;
            csTransactionRepo = csTransaction;
            csTransactionDetailRepo = csTransactionDetail;
            sysEmployeeRepo = sysEmployee;
            AcctApproveAdvanceRepo = acctApproveAdvance;
            sysUserGroupRepo = sysUserGroup;
            catDepartmentRepo = catDepartment;
            sysGroupRepo = sysGroup;
            sysBranchRepo = sysBranch;
        }

        public List<AcctAdvancePaymentResult> Paging(AcctAdvancePaymentCriteria criteria, int page, int size, out int rowsCount)
        {
            var advance = DataContext.Get();
            var request = acctAdvanceRequestRepo.Get();
            var approveAdvance = AcctApproveAdvanceRepo.Get(x => x.IsDeputy == false);
            var user = sysUserRepo.Get();

            List<string> refNo = new List<string>();
            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                refNo = (from ad in advance
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
                         select ad.AdvanceNo).ToList();
            }

            var data = from ad in advance
                       join u in user on ad.Requester equals u.Id into u2
                       from u in u2.DefaultIfEmpty()
                       join re in request on ad.AdvanceNo equals re.AdvanceNo into re2
                       from re in re2.DefaultIfEmpty()
                       join apr in approveAdvance on ad.AdvanceNo equals apr.AdvanceNo into apr2
                       from apr in apr2.DefaultIfEmpty()
                       where
                         (
                            criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0 ? refNo.Contains(ad.AdvanceNo) : 1 == 1
                         )
                         &&
                         (
                            !string.IsNullOrEmpty(criteria.Requester) ?
                            (
                                    ad.Requester == criteria.Requester
                                ||  (apr.Manager == criteria.Requester && apr.ManagerAprDate != null)
                                ||  (apr.Accountant == criteria.Requester && apr.AccountantAprDate != null)
                                ||  (apr.ManagerApr == criteria.Requester && apr.ManagerAprDate != null)
                                ||  (apr.AccountantApr == criteria.Requester && apr.AccountantAprDate != null)
                            )
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
                AdvanceStatusPayment = GetAdvanceStatusPayment(s.Key.AdvanceNo),
                PaymentMethod = s.Key.PaymentMethod,
                PaymentMethodName = Common.CustomData.PaymentMethod.Where(x => x.Value == s.Key.PaymentMethod).Select(x => x.DisplayName).FirstOrDefault(),
                Amount = s.Sum(su => su.Amount),
                StatusApprovalName = Common.CustomData.StatusApproveAdvance.Where(x => x.Value == s.Key.StatusApproval).Select(x => x.DisplayName).FirstOrDefault()
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

        public string GetAdvanceStatusPayment(string advanceNo)
        {
            var requestTmp = acctAdvanceRequestRepo.Get();
            var result = requestTmp.Where(x => x.StatusPayment == "NotSettled" && x.AdvanceNo == advanceNo).Count() == requestTmp.Where(x => x.AdvanceNo == advanceNo).Count()
                            ?
                                "NotSettled"
                            :
                                requestTmp.Where(x => x.StatusPayment == "Settled" && x.AdvanceNo == advanceNo).Count() == requestTmp.Where(x => x.AdvanceNo == advanceNo).Count() ?
                                    "Settled"
                                :
                                    "PartialSettlement";

            return result;
        }

        public List<AcctAdvanceRequestModel> GetGroupRequestsByAdvanceNo(string advanceNo)
        {
            //Sum(Amount) theo lô hàng (JobId, HBL)
            var list = acctAdvanceRequestRepo.Get(x => x.AdvanceNo == advanceNo)
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
            var list = acctAdvanceRequestRepo.Get(x => x.Id == advanceId)
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
            //eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var shipmentsOperation = opsTransactionRepo
                                    .Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != "Canceled")
                                    .Select(x => new Shipments
                                    {
                                        JobId = x.JobNo,
                                        HBL = x.Hwbno,
                                        MBL = x.Mblno
                                    });

            var shipmentsDocumention = (from t in csTransactionRepo.Get()
                                        join td in csTransactionDetailRepo.Get() on t.Id equals td.JobId
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
                var userCurrent = currentUser.UserID;
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var advance = mapper.Map<AcctAdvancePayment>(model);
                advance.Id = model.Id = Guid.NewGuid();
                advance.AdvanceNo = model.AdvanceNo = CreateAdvanceNo(dc);
                advance.StatusApproval = model.StatusApproval = string.IsNullOrEmpty(model.StatusApproval) ? Constants.STATUS_APPROVAL_NEW : model.StatusApproval;

                advance.DatetimeCreated = advance.DatetimeModified = DateTime.Now;
                advance.UserCreated = advance.UserModified = userCurrent;

                var hs = DataContext.Add(advance);
                if (hs.Success)
                {
                    var request = mapper.Map<List<AcctAdvanceRequest>>(model.AdvanceRequests);
                    request.ForEach(req =>
                    {
                        req.Id = Guid.NewGuid();
                        req.AdvanceNo = advance.AdvanceNo;
                        req.DatetimeCreated = req.DatetimeModified = DateTime.Now;
                        req.UserCreated = req.UserModified = userCurrent;
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
                var result = false;
                //Check trường hợp Add new advance payment
                if (string.IsNullOrEmpty(criteria.AdvanceNo))
                {
                    result = acctAdvanceRequestRepo.Get().Any(x =>
                      x.JobId == criteria.JobId
                   && x.Hbl == criteria.HBL
                   && x.Mbl == criteria.MBL);
                }
                else //Check trường hợp Update advance payment
                {
                    result = acctAdvanceRequestRepo.Get().Any(x =>
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
                var requests = acctAdvanceRequestRepo.Get(x => x.AdvanceNo == advanceNo).ToList();
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
            //eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var advanceModel = new AcctAdvancePaymentModel();

            //Lấy ra Advance Payment dựa vào Advance No
            var advance = DataContext.Get(x => x.AdvanceNo == advanceNo).FirstOrDefault();
            //Không tìm thấy Advance Payment thì trả về null
            if (advance == null) return null;

            //Lấy ra danh sách Advance Request dựa vào Advance No và sắp xếp giảm dần theo DatetimeModified Advance Request
            var request = acctAdvanceRequestRepo.Get(x => x.AdvanceNo == advanceNo).OrderByDescending(x => x.DatetimeModified).ToList();
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
            //eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var advanceModel = new AcctAdvancePaymentModel();

            //Lấy ra Advance Payment dựa vào Advance Id
            var advance = DataContext.Get(x => x.Id == advanceId).FirstOrDefault();
            //Không tìm thấy Advance Payment thì trả về null
            if (advance == null) return null;

            //Lấy ra danh sách Advance Request dựa vào Advance No và sắp xếp giảm dần theo DatetimeModified Advance Request
            var request = acctAdvanceRequestRepo.Get(x => x.AdvanceNo == advance.AdvanceNo).OrderByDescending(x => x.DatetimeModified).ToList();
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
                var userCurrent = currentUser.UserID;

                var advance = mapper.Map<AcctAdvancePayment>(model);

                var advanceCurrent = DataContext.Get(x => x.Id == advance.Id).FirstOrDefault();
                advance.DatetimeCreated = advanceCurrent.DatetimeCreated;
                advance.UserCreated = advanceCurrent.UserCreated;

                advance.DatetimeModified = DateTime.Now;
                advance.UserModified = userCurrent;

                var hs = DataContext.Update(advance, x => x.Id == advance.Id);

                if (hs.Success)
                {
                    var request = mapper.Map<List<AcctAdvanceRequest>>(model.AdvanceRequests);
                    //Lấy ra các Request cũ cần update
                    var requestUpdate = request.Where(x => x.UserCreated != null && x.UserCreated != "").ToList();

                    //Lấy ra các Request có cùng AdvanceNo và không tồn tại trong requestUpdate
                    var requestNeedRemove = acctAdvanceRequestRepo.Get(x => x.AdvanceNo == advance.AdvanceNo && !requestUpdate.Contains(x)).ToList();
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
                            req.UserCreated = req.UserModified = userCurrent;
                            req.StatusPayment = "NotSettled";
                        });
                        dc.AcctAdvanceRequest.AddRange(requestNew);
                    }

                    if (requestUpdate != null && requestUpdate.Count > 0)
                    {
                        //Cập nhật những request cũ cần update
                        requestUpdate.ForEach(req =>
                        {
                            req.DatetimeModified = DateTime.Now;
                            req.UserModified = userCurrent;
                        });
                        dc.AcctAdvanceRequest.UpdateRange(requestUpdate);
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
                foreach (var jobId in advance.AdvanceRequests.GroupBy(x => x.JobId).Select(x => x.FirstOrDefault().JobId))
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
                foreach (var hbl in advance.AdvanceRequests.GroupBy(x => x.Hbl).Select(x => x.FirstOrDefault().Hbl))
                {
                    strHbl += !string.IsNullOrEmpty(hbl) ? hbl + "," : "";
                }

                //Lấy ra chuỗi CustomNo
                foreach (var customNo in advance.AdvanceRequests.GroupBy(x => x.CustomNo).Select(x => x.FirstOrDefault().CustomNo))
                {
                    strCustomNo += !string.IsNullOrEmpty(customNo) ? customNo + "," : "";
                }

                strJobId += ")";
                strJobId = strJobId.Replace(",)", "").Replace(")", "");
                strHbl += ")";
                strHbl = strHbl.Replace(",)", "").Replace(")", "");
                strCustomNo += ")";
                strCustomNo = strCustomNo.Replace(",)", "").Replace(")", "");
            }

            //Lấy ra tên requester
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var employeeId = sysUserRepo.Get(x => x.Id == advance.Requester).Select(x => x.EmployeeId).FirstOrDefault();
            var requesterName = sysEmployeeRepo.Get(x => x.Id == employeeId).Select(x => x.EmployeeNameVn).FirstOrDefault();

            string managerName = "";
            string accountantName = "";
            var approveAdvance = AcctApproveAdvanceRepo.Get(x => x.AdvanceNo == advance.AdvanceNo && x.IsDeputy == false).FirstOrDefault();
            if (approveAdvance != null)
            {
                managerName = string.IsNullOrEmpty(approveAdvance.Manager) ? null : GetEmployeeByUserId(approveAdvance.Manager).EmployeeNameVn;
                accountantName = string.IsNullOrEmpty(approveAdvance.Accountant) ? null : GetEmployeeByUserId(approveAdvance.Accountant).EmployeeNameVn;
            }

            var acctAdvance = new AdvancePaymentRequestReport
            {
                AdvID = advance.AdvanceNo,
                RefNo = "N/A",
                AdvDate = advance.RequestDate.Value.Date,
                AdvTo = "N/A",
                AdvContactID = "N/A",
                AdvContact = requesterName,//cần lấy ra username
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
                AdvDpManagerName = managerName,
                AdvDpSignDate = null,
                AdvAcsDpManagerID = "N/A",
                AdvAcsDpManagerStickDeny = null,
                AdvAcsDpManagerStickApp = null,
                AdvAcsDpManagerName = accountantName,
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
                foreach (var jobId in advance.AdvanceRequests.GroupBy(x => x.JobId).Select(x => x.FirstOrDefault().JobId))
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
                foreach (var hbl in advance.AdvanceRequests.GroupBy(x => x.Hbl).Select(x => x.FirstOrDefault().Hbl))
                {
                    strHbl += !string.IsNullOrEmpty(hbl) ? hbl + "," : "";
                }

                //Lấy ra chuỗi CustomNo
                foreach (var customNo in advance.AdvanceRequests.GroupBy(x => x.CustomNo).Select(x => x.FirstOrDefault().CustomNo))
                {
                    strCustomNo += !string.IsNullOrEmpty(customNo) ? customNo + "," : "";
                }

                strJobId += ")";
                strJobId = strJobId.Replace(",)", "").Replace(")", "");
                strHbl += ")";
                strHbl = strHbl.Replace(",)", "").Replace(")", "");
                strCustomNo += ")";
                strCustomNo = strCustomNo.Replace(",)", "").Replace(")", "");
            }

            //Lấy ra tên requester
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var employeeId = sysUserRepo.Get(x => x.Id == advance.Requester).Select(x => x.EmployeeId).FirstOrDefault();
            var requesterName = sysEmployeeRepo.Get(x => x.Id == employeeId).Select(x => x.EmployeeNameVn).FirstOrDefault();

            string managerName = "";
            string accountantName = "";
            var approveAdvance = AcctApproveAdvanceRepo.Get(x => x.AdvanceNo == advance.AdvanceNo && x.IsDeputy == false).FirstOrDefault();
            if (approveAdvance != null)
            {
                managerName = string.IsNullOrEmpty(approveAdvance.Manager) ? null : GetEmployeeByUserId(approveAdvance.Manager).EmployeeNameVn;
                accountantName = string.IsNullOrEmpty(approveAdvance.Accountant) ? null : GetEmployeeByUserId(approveAdvance.Accountant).EmployeeNameVn;
            }

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
                AdvDpManagerName = managerName,
                AdvDpSignDate = null,
                AdvAcsDpManagerID = "N/A",
                AdvAcsDpManagerStickDeny = null,
                AdvAcsDpManagerStickApp = null,
                AdvAcsDpManagerName = accountantName,
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
        //Lấy ra groupId của User
        private int GetGroupIdOfUser(string userId)
        {
            //Lấy ra groupId của user
            var grpIdOfUser = sysUserGroupRepo.Get(x => x.UserId == userId).FirstOrDefault().GroupId;
            return grpIdOfUser;
        }

        //Lấy Info của Group 
        private SysGroup GetInfoGroupOfUser(string userId)
        {
            var grpIdOfUser = GetGroupIdOfUser(userId);
            var infoGrpOfUser = sysGroupRepo.Get(x => x.Id == grpIdOfUser).FirstOrDefault();
            return infoGrpOfUser;
        }

        //Lấy Info Dept của User
        private CatDepartment GetInfoDeptOfUser(string userId, string idBranch = "27d26acb-e247-47b7-961e-afa7b3d7e11e")
        {
            var deptIdOfUser = GetInfoGroupOfUser(userId).DepartmentId;
            var deptOfUser = catDepartmentRepo.Get(x => x.BranchId == Guid.Parse(idBranch) && x.Id == deptIdOfUser).FirstOrDefault();
            return deptOfUser;
        }

        //Lấy ra Leader của User
        //Leader đây chính là ManagerID của Group
        private string GetLeaderIdOfUser(string userId)
        {
            var leaderIdOfUser = GetInfoGroupOfUser(userId).ManagerId;
            return leaderIdOfUser;
        }

        //Lấy ra ManagerId của User
        private string GetManagerIdOfUser(string userId, string idBranch = "27d26acb-e247-47b7-961e-afa7b3d7e11e")
        {
            //Lấy ra deptId của User
            var deptIdOfUser = GetInfoGroupOfUser(userId).DepartmentId;
            //Lấy ra mangerId của User
            var managerIdOfUser = catDepartmentRepo.Get(x => x.BranchId == Guid.Parse(idBranch) && x.Id == deptIdOfUser).FirstOrDefault().ManagerId;
            return managerIdOfUser;
        }

        //Lấy ra AccountantManagerId của Dept Accountant
        //Đang gán cứng BrandId của Branch ITL HCM (27d26acb-e247-47b7-961e-afa7b3d7e11e)
        private string GetAccountantId(string idBranch = "27d26acb-e247-47b7-961e-afa7b3d7e11e")
        {
            var accountantManagerId = catDepartmentRepo.Get(x => x.BranchId == Guid.Parse(idBranch) && x.Code == "Accountant").FirstOrDefault().ManagerId;
            return accountantManagerId;
        }

        //Lấy ra BUHeadId của BUHead
        //Đang gán cứng BrandId của Branch ITL HCM (27d26acb-e247-47b7-961e-afa7b3d7e11e)
        private string GetBUHeadId(string idBranch = "27d26acb-e247-47b7-961e-afa7b3d7e11e")
        {
            var buHeadId = sysBranchRepo.Get(x => x.Id == Guid.Parse(idBranch)).FirstOrDefault().ManagerId;
            return buHeadId;
        }

        //Lấy ra employeeId của User
        private string GetEmployeeIdOfUser(string UserId)
        {
            return sysUserRepo.Get(x => x.Id == UserId).FirstOrDefault().EmployeeId;
        }

        //Lấy info Employee của User dựa vào employeeId
        private SysEmployee GetEmployeeByEmployeeId(string employeeId)
        {
            return sysEmployeeRepo.Get(x => x.Id == employeeId).FirstOrDefault();
        }

        //Lấy info Employee của User dựa vào userId
        private SysEmployee GetEmployeeByUserId(string userId)
        {
            var employeeId = GetEmployeeIdOfUser(userId);
            return sysEmployeeRepo.Get(x => x.Id == employeeId).FirstOrDefault();
        }


        //Insert Or Update AcctApproveAdvance by AdvanceNo
        public HandleState InsertOrUpdateApprovalAdvance(AcctApproveAdvanceModel approve)
        {
            try
            {
                var userCurrent = currentUser.UserID;

                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;

                var acctApprove = mapper.Map<AcctApproveAdvance>(approve);

                if (!string.IsNullOrEmpty(approve.AdvanceNo))
                {
                    var advance = DataContext.Get(x => x.AdvanceNo == approve.AdvanceNo).FirstOrDefault();
                    //&& advance.StatusApproval != "RequestApproval"
                    if (advance.StatusApproval != Constants.STATUS_APPROVAL_NEW && advance.StatusApproval != Constants.STATUS_APPROVAL_DENIED && advance.StatusApproval != Constants.STATUS_APPROVAL_DONE)
                    {
                        return new HandleState("Awaiting Approval");
                    }
                }

                //Lấy ra các user Leader, Manager Dept của user requester, user Accountant, BUHead(nếu có) của user requester
                acctApprove.Leader = GetLeaderIdOfUser(userCurrent);
                acctApprove.Manager = GetManagerIdOfUser(userCurrent);
                acctApprove.Accountant = GetAccountantId();
                acctApprove.Buhead = GetBUHeadId();


                var checkExistsApproveByAdvanceNo = AcctApproveAdvanceRepo.Get(x => x.AdvanceNo == acctApprove.AdvanceNo && x.IsDeputy == false).FirstOrDefault();
                if (checkExistsApproveByAdvanceNo == null) //Insert AcctApproveAdvance
                {
                    acctApprove.Id = Guid.NewGuid();
                    acctApprove.RequesterAprDate = DateTime.Now;
                    acctApprove.UserCreated = acctApprove.UserModified = userCurrent;
                    acctApprove.DateCreated = acctApprove.DateModified = DateTime.Now;
                    acctApprove.IsDeputy = false;
                    dc.AcctApproveAdvance.Add(acctApprove);
                }
                else //Update AcctApproveAdvance by AdvanceNo
                {
                    checkExistsApproveByAdvanceNo.RequesterAprDate = DateTime.Now;
                    checkExistsApproveByAdvanceNo.UserModified = userCurrent;
                    checkExistsApproveByAdvanceNo.DateModified = DateTime.Now;
                    dc.AcctApproveAdvance.Update(checkExistsApproveByAdvanceNo);
                }
                dc.SaveChanges();

                var emailLeaderOrManager = "";
                var userLeaderOrManager = "";
                //Send mail đề nghị approve đến Leader(Nếu có) nếu không có thì send tới Manager Dept
                //Lấy ra Leader của User & Manager Dept của User Requester
                if (string.IsNullOrEmpty(acctApprove.Leader))
                {
                    userLeaderOrManager = acctApprove.Manager;
                    //Lấy ra employeeId của managerIdOfUser
                    var employeeIdOfUserManager = GetEmployeeIdOfUser(userLeaderOrManager);
                    //Lấy ra email của Manager
                    emailLeaderOrManager = GetEmployeeByEmployeeId(employeeIdOfUserManager).Email;
                }
                else
                {
                    userLeaderOrManager = acctApprove.Leader;
                    //Lấy ra employeeId của managerIdOfUser
                    var employeeIdOfUserLeader = GetEmployeeIdOfUser(userLeaderOrManager);
                    //Lấy ra email của Leader (hiện tại chưa có nên gán rỗng)
                    emailLeaderOrManager = GetEmployeeByEmployeeId(employeeIdOfUserLeader).Email;
                }

                if (string.IsNullOrEmpty(emailLeaderOrManager)) return new HandleState("Not Found Leader or Manager");

                var sendMailResult = SendMailSuggestApproval(acctApprove.AdvanceNo, userLeaderOrManager, emailLeaderOrManager);

                return !sendMailResult ? new HandleState("Send Mail Suggest Approval Fail") : new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message.ToString());
            }
        }

        //Lấy ra ds các user được ủy quyền theo nhóm leader, manager department, accountant manager, BUHead dựa vào dept
        //Đang gán cứng BrandId của Branch ITL HCM (27d26acb-e247-47b7-961e-afa7b3d7e11e)
        private List<string> GetListUserDeputyByDept(string dept, string idBranch = "27d26acb-e247-47b7-961e-afa7b3d7e11e")
        {
            Dictionary<string, string> listUsers = new Dictionary<string, string> {
                 { "william.hiep","OPS" },//User ủy quyền cho dept OPS
                 { "linda.linh","Accountant" },//User ủy quyền cho dept Accountant
                 { "christina.my","Accountant" }//User ủy quyền cho dept Accountant
            };
            var list = listUsers.ToList();
            var deputy = listUsers.Where(x => x.Value == dept).Select(x => x.Key).ToList();
            return deputy;
        }

        //Check group trước đó đã được approve hay chưa? Nếu group trước đó đã approve thì group hiện tại mới được Approve
        //Nếu group hiện tại đã được approve thì không cho approve nữa
        private HandleState CheckApprovedOfDeptPrevAndDeptCurrent(string advanceNo, string userId, string deptOfUser)
        {
            HandleState result = new HandleState("Not Found");

            //Lấy ra Advance Approval dựa vào advanceNo
            var acctApprove = AcctApproveAdvanceRepo.Get(x => x.AdvanceNo == advanceNo && x.IsDeputy == false).FirstOrDefault();
            if (acctApprove == null)
            {
                result = new HandleState("Not Found Advance Approval by AdvanceNo is " + advanceNo);
                return result;
            }

            //Lấy ra Advance Payment dựa vào advanceNo
            var advance = DataContext.Get(x => x.AdvanceNo == advanceNo).FirstOrDefault();
            if (advance == null)
            {
                result = new HandleState("Not Found Advance Payment by AdvanceNo is" + advanceNo);
                return result;
            }


            //Trường hợp không có Leader
            if (string.IsNullOrEmpty(acctApprove.Leader))
            {
                //Manager Department Approve
                //Kiểm tra user có phải là dept manager hoặc có phải là user được ủy quyền duyệt hay không
                if (userId == GetManagerIdOfUser(advance.Requester) || GetListUserDeputyByDept(deptOfUser).Contains(userId))
                {
                    //Kiểm tra User Approve có thuộc cùng dept với User Requester hay
                    //Nếu không cùng thì không cho phép Approve (đối với Dept Manager)
                    if (GetInfoGroupOfUser(userId).DepartmentId != GetInfoGroupOfUser(advance.Requester).DepartmentId)
                    {
                        result = new HandleState("Not in the same department");
                    }
                    else
                    {
                        result = new HandleState();
                    }

                    //Requester đã approve thì Manager mới được phép Approve
                    if (!string.IsNullOrEmpty(acctApprove.Requester) && acctApprove.RequesterAprDate != null)
                    {
                        result = new HandleState();
                        //Check group CSManager đã approve chưa
                        //Nếu đã approve thì không được approve nữa
                        if (advance.StatusApproval.Equals(Constants.STATUS_APPROVAL_DEPARTMENTAPPROVED)
                            && acctApprove.ManagerAprDate != null
                            && !string.IsNullOrEmpty(acctApprove.ManagerApr))
                        {
                            result = new HandleState("Manager Department Approved");
                        }
                    }
                    else
                    {
                        result = new HandleState("Not found Requester or Requester not approve");
                    }
                }

                //Accountant Approve
                //Kiểm tra user có phải là Accountant Manager hoặc có phải là user được ủy quyền duyệt hay không
                if (userId == GetAccountantId() || GetListUserDeputyByDept(deptOfUser).Contains(userId))
                {
                    //Check group DepartmentManager đã được Approve chưa
                    if (!string.IsNullOrEmpty(acctApprove.Manager)
                        && advance.StatusApproval.Equals(Constants.STATUS_APPROVAL_DEPARTMENTAPPROVED)
                        && acctApprove.ManagerAprDate != null
                        && !string.IsNullOrEmpty(acctApprove.ManagerApr))
                    {
                        result = new HandleState();
                        //Check group Accountant đã approve chưa
                        //Nếu đã approve thì không được approve nữa
                        if (advance.StatusApproval.Equals(Constants.STATUS_APPROVAL_DONE)
                            && acctApprove.AccountantAprDate != null
                            && !string.IsNullOrEmpty(acctApprove.AccountantApr))
                        {
                            result = new HandleState("Accountant Approved");
                        }
                    }
                    else
                    {
                        result = new HandleState("Not found Manager or Manager not approve");
                    }
                }
            }
            else //Trường hợp có leader
            {
                //Leader Approve
                if (userId == GetLeaderIdOfUser(advance.Requester) || GetListUserDeputyByDept(deptOfUser).Contains(userId))
                {
                    //Kiểm tra User Approve có thuộc cùng dept với User Requester hay
                    //Nếu không cùng thì không cho phép Approve (đối với Dept Manager)
                    if (GetInfoGroupOfUser(userId).DepartmentId != GetInfoGroupOfUser(advance.Requester).DepartmentId)
                    {
                        result = new HandleState("Not in the same department");
                    }
                    else
                    {
                        result = new HandleState();
                    }

                    //Check Requester đã approve chưa
                    if (!string.IsNullOrEmpty(acctApprove.Requester)
                        && acctApprove.RequesterAprDate != null)
                    {
                        result = new HandleState();
                        //Check group Leader đã được approve chưa
                        //Nếu đã approve thì không được approve nữa
                        if (advance.StatusApproval.Equals(Constants.STATUS_APPROVAL_LEADERAPPROVED)
                            && acctApprove.LeaderAprDate != null
                            && !string.IsNullOrEmpty(acctApprove.Leader))
                        {
                            result = new HandleState("Leader Approved");
                        }
                    }
                    else
                    {
                        result = new HandleState("Not found Requester or Requester not approve");
                    }
                }

                //Manager Department Approve
                if (userId == GetManagerIdOfUser(advance.Requester) || GetListUserDeputyByDept(deptOfUser).Contains(userId))
                {
                    //Kiểm tra User Approve có thuộc cùng dept với User Requester hay
                    //Nếu không cùng thì không cho phép Approve (đối với Dept Manager)
                    if (GetInfoGroupOfUser(userId).DepartmentId != GetInfoGroupOfUser(advance.Requester).DepartmentId)
                    {
                        result = new HandleState("Not in the same department");
                    }
                    else
                    {
                        result = new HandleState();
                    }

                    //Check group Leader đã được approve chưa
                    if (!string.IsNullOrEmpty(acctApprove.Leader)
                        && advance.StatusApproval.Equals(Constants.STATUS_APPROVAL_LEADERAPPROVED)
                        && acctApprove.LeaderAprDate != null)
                    {
                        result = new HandleState();
                        //Check group Manager Department đã approve chưa
                        //Nếu đã approve thì không được approve nữa
                        if (advance.StatusApproval.Equals(Constants.STATUS_APPROVAL_DEPARTMENTAPPROVED)
                            && acctApprove.ManagerAprDate != null
                            && !string.IsNullOrEmpty(acctApprove.ManagerApr))
                        {
                            result = new HandleState("Manager Department Approved");
                        }
                    }
                    else
                    {
                        result = new HandleState("Not found Leader or Leader not approve");
                    }

                }

                //Accountant Approve
                //Kiểm tra user có phải là Accountant Manager hoặc có phải là user được ủy quyền duyệt hay không
                if (userId == GetAccountantId() || GetListUserDeputyByDept(deptOfUser).Contains(userId))
                {
                    //Check group DepartmentManager đã được Approve chưa
                    if (!string.IsNullOrEmpty(acctApprove.Manager)
                        && advance.StatusApproval.Equals(Constants.STATUS_APPROVAL_DEPARTMENTAPPROVED)
                        && acctApprove.ManagerAprDate != null
                        && !string.IsNullOrEmpty(acctApprove.ManagerApr))
                    {
                        result = new HandleState();
                        //Check group Accountant đã approve chưa
                        //Nếu đã approve thì không được approve nữa
                        if (advance.StatusApproval.Equals(Constants.STATUS_APPROVAL_DONE)
                            && acctApprove.AccountantAprDate != null
                            && !string.IsNullOrEmpty(acctApprove.AccountantApr))
                        {
                            result = new HandleState("Accountant Approved");
                        }
                    }
                    else
                    {
                        result = new HandleState("Not found Manager or Manager not approve");
                    }
                }
            }

            return result;
        }

        //Update Approval cho từng group
        public HandleState UpdateApproval(Guid advanceId)
        {
            var userCurrent = currentUser.UserID;

            var userAprNext = "";
            var emailUserAprNext = "";

            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var advance = DataContext.Get(x => x.Id == advanceId).FirstOrDefault();

            if (advance == null) return new HandleState("Not Found Advance Payment");

            var approve = AcctApproveAdvanceRepo.Get(x => x.AdvanceNo == advance.AdvanceNo && x.IsDeputy == false).FirstOrDefault();

            if (approve == null) return new HandleState("Not Found Advance Approval by AdvanceNo is " + advance.AdvanceNo);

            //Lấy ra dept code của userApprove dựa vào userApprove
            var deptCodeOfUser = GetInfoDeptOfUser(userCurrent).Code;


            //Kiểm tra group trước đó đã được approve chưa và group của userApprove đã được approve chưa
            var checkApr = CheckApprovedOfDeptPrevAndDeptCurrent(advance.AdvanceNo, userCurrent, deptCodeOfUser);
            if (checkApr.Success == false) return new HandleState(checkApr.Exception.Message);
            if (approve != null && advance != null)
            {
                if (userCurrent == GetLeaderIdOfUser(advance.Requester) || GetListUserDeputyByDept(deptCodeOfUser).Contains(userCurrent))
                {
                    advance.StatusApproval = Constants.STATUS_APPROVAL_LEADERAPPROVED;
                    approve.LeaderAprDate = DateTime.Now;//Cập nhật ngày Approve của Leader

                    //Lấy email của Department Manager
                    userAprNext = GetManagerIdOfUser(userCurrent);
                    var userAprNextId = GetEmployeeIdOfUser(userAprNext);
                    emailUserAprNext = GetEmployeeByEmployeeId(userAprNextId).Email;
                }
                else if (userCurrent == GetManagerIdOfUser(advance.Requester) || GetListUserDeputyByDept(deptCodeOfUser).Contains(userCurrent))
                {
                    advance.StatusApproval = Constants.STATUS_APPROVAL_DEPARTMENTAPPROVED;
                    approve.ManagerAprDate = DateTime.Now;//Cập nhật ngày Approve của Manager
                    approve.ManagerApr = userCurrent;

                    //Lấy email của Accountant Manager
                    userAprNext = GetAccountantId();
                    var userAprNextId = GetEmployeeIdOfUser(userAprNext);
                    emailUserAprNext = GetEmployeeByEmployeeId(userAprNextId).Email;
                }
                else if (userCurrent == GetAccountantId() || GetListUserDeputyByDept(deptCodeOfUser).Contains(userCurrent))
                {
                    advance.StatusApproval = Constants.STATUS_APPROVAL_DONE;
                    approve.AccountantAprDate = approve.BuheadAprDate = DateTime.Now;//Cập nhật ngày Approve của Accountant & BUHead
                    approve.AccountantApr = userCurrent;
                    approve.BuheadApr = approve.Buhead;

                    //Send mail approval success when Accountant approved, mail send to requester
                    SendMailApproved(advance.AdvanceNo, DateTime.Now);
                }

                advance.UserModified = approve.UserModified = userCurrent;
                advance.DatetimeModified = approve.DateModified = DateTime.Now;

                dc.AcctAdvancePayment.Update(advance);
                dc.AcctApproveAdvance.Update(approve);
                dc.SaveChanges();
            }

            if (userCurrent == GetAccountantId())
            {
                return new HandleState();
            }
            else
            {
                if (string.IsNullOrEmpty(emailUserAprNext)) return new HandleState("Not found email of user " + userAprNext);

                //Send mail đề nghị approve
                var sendMailResult = SendMailSuggestApproval(advance.AdvanceNo, userAprNext, emailUserAprNext);

                return sendMailResult ? new HandleState() : new HandleState("Send Mail Suggest Approval Fail");
            }
        }

        public HandleState DeniedApprove(Guid advanceId, string comment)
        {
            var userCurrent = currentUser.UserID;

            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var advance = DataContext.Get(x => x.Id == advanceId).FirstOrDefault();

            if (advance == null) return new HandleState("Not Found Advance Payment");

            var approve = AcctApproveAdvanceRepo.Get(x => x.AdvanceNo == advance.AdvanceNo && x.IsDeputy == false).FirstOrDefault();
            if (approve == null) return new HandleState("Not Found Approve Advance by advanceNo " + advance.AdvanceNo);

            //Lấy ra dept code của userApprove dựa vào userApprove
            var deptCodeOfUser = GetInfoDeptOfUser(userCurrent).Code;

            //Kiểm tra group trước đó đã được approve chưa và group của userApprove đã được approve chưa
            var checkApr = CheckApprovedOfDeptPrevAndDeptCurrent(advance.AdvanceNo, userCurrent, deptCodeOfUser);
            if (checkApr.Success == false) return new HandleState(checkApr.Exception.Message);
            if (approve != null && advance != null)
            {
                if (userCurrent == GetLeaderIdOfUser(advance.Requester) || GetListUserDeputyByDept(deptCodeOfUser).Contains(userCurrent))
                {
                    approve.LeaderAprDate = DateTime.Now;//Cập nhật ngày Denie của Leader
                }
                else if (userCurrent == GetManagerIdOfUser(advance.Requester) || GetListUserDeputyByDept(deptCodeOfUser).Contains(userCurrent))
                {
                    approve.ManagerAprDate = DateTime.Now;//Cập nhật ngày Denie của Manager
                    approve.ManagerApr = userCurrent; //Cập nhật user manager denie                   
                }
                else if (userCurrent == GetAccountantId() || GetListUserDeputyByDept(deptCodeOfUser).Contains(userCurrent))
                {
                    approve.AccountantAprDate = DateTime.Now;//Cập nhật ngày Denie của Accountant
                    approve.AccountantApr = userCurrent; //Cập nhật user accountant denie
                }

                approve.UserModified = userCurrent;
                approve.DateModified = DateTime.Now;
                approve.Comment = comment;
                approve.IsDeputy = true;
                dc.AcctApproveAdvance.Update(approve);

                //Cập nhật lại advance status của Advance Payment
                advance.StatusApproval = Constants.STATUS_APPROVAL_DENIED;
                advance.UserModified = userCurrent;
                advance.DatetimeModified = DateTime.Now;
                dc.AcctAdvancePayment.Update(advance);

                dc.SaveChanges();
            }

            //Send mail denied approval
            var sendMailResult = SendMailDeniedApproval(advance.AdvanceNo, comment, DateTime.Now);
            return sendMailResult ? new HandleState() : new HandleState("Send Mail Denie Approval Fail");
        }

        //Send Mail đề nghị Approve
        private bool SendMailSuggestApproval(string advanceNo, string userReciver, string emailUserReciver)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            //Lấy ra AdvancePayment dựa vào AdvanceNo
            var advance = DataContext.Get(x => x.AdvanceNo == advanceNo).FirstOrDefault();
            
            //Lấy ra tên & email của user Requester
            var requesterId = GetEmployeeIdOfUser(advance.Requester);
            var requesterName = GetEmployeeByEmployeeId(requesterId).EmployeeNameVn;
            var emailRequester = GetEmployeeByEmployeeId(requesterId).Email;

            //Lấy ra thông tin của Advance Request dựa vào AdvanceNo
            var requests = dc.AcctAdvanceRequest.Where(x => x.AdvanceNo == advanceNo).GroupBy(x => x.JobId).Select(x => x.FirstOrDefault().JobId);
            string jobIds = "";
            foreach (var request in requests)
            {
                jobIds += !string.IsNullOrEmpty(request) ? request + "; " : "";
            }
            jobIds += ")";
            jobIds = jobIds.Replace("; )", "").Replace(")", "");

            var totalAmount = dc.AcctAdvanceRequest.Where(x => x.AdvanceNo == advanceNo).Sum(x => x.Amount);
            if (totalAmount != null)
            {
                totalAmount = Math.Round(totalAmount.Value, 2);
            }

            //Mail Info
            string subject = "eFMS - Advance Payment Approval Request from [RequesterName]";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p><i><b>Dear Mr/Mrs [UserName],</b></i></p><p>You have new Advance Payment Approval Request from <b>[RequesterName]</b> as below info:</p><p><i>Anh/ Chị có một yêu cầu duyệt tạm ứng từ <b>[RequesterName]</b> với thông tin như sau:</i></p><ul><li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNo]</b></li><li>Advance Amount/ <i>Số tiền tạm ứng</i> : <b>[TotalAmount] [CurrencyAdvance]</b><li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li><li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li><li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li></ul><p>You click here to check more detail and approve: <span><a href='[Url]/[lang]/[UrlFunc]/[AdvanceId]/approve' target='_blank'>Detail Advance Request</a></span></p><p><i>Anh/ Chị chọn vào đây để biết thêm thông tin chi tiết và phê duyệt: <span><a href='[Url]/[lang]/[UrlFunc]/[AdvanceId]/approve' target='_blank'>Chi tiết phiếu tạm ứng</a></span></i></p><p>Thanks and Regards,<p><p><b>eFMS System,</b></p><p><img src='{0}'/></p></div>", logoeFMSBase64());
            body = body.Replace("[UserName]", userReciver);
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[AdvanceNo]", advanceNo);
            body = body.Replace("[TotalAmount]", String.Format("{0:n}", totalAmount));
            body = body.Replace("[CurrencyAdvance]", advance.AdvanceCurrency);
            body = body.Replace("[JobIds]", jobIds);
            body = body.Replace("[RequestDate]", advance.RequestDate.Value.ToString("dd/MM/yyyy"));
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", "#/home/accounting/advance-payment");
            body = body.Replace("[AdvanceId]", advance.Id.ToString());
            List<string> toEmails = new List<string> {
                emailUserReciver
            };
            List<string> attachments = null;

            //CC cho User Requester để biết được quá trình Approve đã đến bước nào
            //Và các User thuộc group của User Approve được ủy quyền
            List<string> emailCCs = new List<string> {
                emailRequester
            };

            //Lấy ra email của các User được ủy quyền của group của User Approve
            //var deptCodeOfUserReciver = GetInfoDeptOfUser(advance.Requester).Code;
            var deptCodeOfUserReciver = GetInfoDeptOfUser(userReciver).Code;
            var usersDeputy = GetListUserDeputyByDept(deptCodeOfUserReciver);
            if (usersDeputy.Count > 0)
            {
                foreach (var userId in usersDeputy)
                {
                    //Lấy ra employeeId của user
                    var employeeIdOfUser = GetEmployeeIdOfUser(userId);
                    //Lấy ra email của user
                    var emailUser = GetEmployeeByEmployeeId(employeeIdOfUser).Email;
                    emailCCs.Add(emailUser);
                }
            }

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        //Send Mail Approved
        private bool SendMailApproved(string advanceNo, DateTime approvedDate)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            //Lấy ra AdvancePayment dựa vào AdvanceNo
            var advance = dc.AcctAdvancePayment.Where(x => x.AdvanceNo == advanceNo).FirstOrDefault();

            //Lấy ra tên & email của user Requester
            var requesterId = GetEmployeeIdOfUser(advance.Requester);
            var requesterName = GetEmployeeByEmployeeId(requesterId).EmployeeNameVn;
            var emailRequester = GetEmployeeByEmployeeId(requesterId).Email;


            //Lấy ra thông tin của Advance Request dựa vào AdvanceNo
            var requests = dc.AcctAdvanceRequest.Where(x => x.AdvanceNo == advanceNo).GroupBy(x => x.JobId).Select(x => x.FirstOrDefault().JobId);
            string jobIds = "";
            foreach (var request in requests)
            {
                jobIds += !string.IsNullOrEmpty(request) ? request + ", " : "";
            }
            jobIds += ")";
            jobIds = jobIds.Replace(", )", "").Replace(")", "");

            var totalAmount = dc.AcctAdvanceRequest.Where(x => x.AdvanceNo == advanceNo).Sum(x => x.Amount);
            if (totalAmount != null)
            {
                totalAmount = Math.Round(totalAmount.Value, 2);
            }

            //Mail Info
            string subject = "eFMS - Advance Payment from [RequesterName] is approved";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p><i><b>Dear Mr/Mrs [RequesterName],</b></i></p><p>You have an Advance Payment is approved at <b>[ApprovedDate]</b> as below info:</p><p><i>Anh/ Chị có một yêu cầu tạm ứng đã được phê duyệt vào lúc <b>[ApprovedDate]</b> với thông tin như sau:</i></p><ul><li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNo]</b></li><li>Advance Amount/ <i>Số tiền tạm ứng</i> : <b>[TotalAmount] [CurrencyAdvance]</b><li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li><li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li><li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li></ul><p>You can click here to check more detail: <span><a href='[Url]/[lang]/[UrlFunc]/[AdvanceId]' target='_blank'>Detail Advance Request</a></span></p><p><i>Anh/ Chị có thể chọn vào đây để biết thêm thông tin chi tiết: <span><a href='[Url]/[lang]/[UrlFunc]/[AdvanceId]' target='_blank'>Chi tiết tạm ứng</a></span></i></p><p>Thanks and Regards,<p><p><b>eFMS System,</b></p><p><img src='{0}'/></p></div>", logoeFMSBase64());
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[ApprovedDate]", approvedDate.ToString("HH:mm - dd/MM/yyyy"));
            body = body.Replace("[AdvanceNo]", advanceNo);
            body = body.Replace("[TotalAmount]", String.Format("{0:n}", totalAmount));
            body = body.Replace("[CurrencyAdvance]", advance.AdvanceCurrency);
            body = body.Replace("[JobIds]", jobIds);
            body = body.Replace("[RequestDate]", advance.RequestDate.Value.ToString("dd/MM/yyyy"));
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", "#/home/accounting/advance-payment");
            body = body.Replace("[AdvanceId]", advance.Id.ToString());
            List<string> toEmails = new List<string> {
                emailRequester
            };
            List<string> attachments = null;
            List<string> emailCCs = new List<string>
            {
                //Không cần email cc
            };

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        //Send Mail Deny Approve (Gửi đến Requester và các Leader, Manager, Accountant, BUHead đã approve trước đó)
        private bool SendMailDeniedApproval(string advanceNo, string comment, DateTime DeniedDate)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            //Lấy ra AdvancePayment dựa vào AdvanceNo
            var advance = dc.AcctAdvancePayment.Where(x => x.AdvanceNo == advanceNo).FirstOrDefault();

            //Lấy ra tên & email của user Requester
            var requesterId = GetEmployeeIdOfUser(advance.Requester);
            var requesterName = GetEmployeeByEmployeeId(requesterId).EmployeeNameVn;
            var emailRequester = GetEmployeeByEmployeeId(requesterId).Email;

            //Lấy ra thông tin của Advance Request dựa vào AdvanceNo
            var requests = dc.AcctAdvanceRequest.Where(x => x.AdvanceNo == advanceNo).GroupBy(x => x.JobId).Select(x => x.FirstOrDefault().JobId);
            string jobIds = "";
            foreach (var request in requests)
            {
                jobIds += !string.IsNullOrEmpty(request) ? request + ", " : "";
            }
            jobIds += ")";
            jobIds = jobIds.Replace(", )", "").Replace(")", "");

            var totalAmount = dc.AcctAdvanceRequest.Where(x => x.AdvanceNo == advanceNo).Sum(x => x.Amount);
            if (totalAmount != null)
            {
                totalAmount = Math.Round(totalAmount.Value, 2);
            }

            //Mail Info
            string subject = "eFMS - Advance Payment from [RequesterName] is denied";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p><i><b>Dear Mr/Mrs [RequesterName],</b></i></p><p>You have an Advance Payment is denied at <b>[DeniedDate]</b> by as below info:</p><p><i>Anh/ Chị có một yêu cầu tạm ứng đã bị từ chối vào lúc <b>[DeniedDate]</b> by với thông tin như sau:</i></p><ul><li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNo]</b></li><li>Advance Amount/ <i>Số tiền tạm ứng</i> : <b>[TotalAmount] [CurrencyAdvance]</b><li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li><li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li><li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li><li>Comment/ <i>Lý do từ chối</i> : <b>[Comment]</b></li></ul><p>You click here to recheck detail: <span><a href='[Url]/[lang]/[UrlFunc]/[AdvanceId]' target='_blank'>Detail Advance Request</a></span></p><p><i>Anh/ Chị chọn vào đây để kiểm tra lại thông tin chi tiết: <span><a href='[Url]/[lang]/[UrlFunc]/[AdvanceId]' target='_blank'>Chi tiết tạm ứng</a></span></i></p><p>Thanks and Regards,<p><p><b>eFMS System,</b></p><p><img src='{0}'/></p></div>", logoeFMSBase64());
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[DeniedDate]", DeniedDate.ToString("HH:mm - dd/MM/yyyy"));
            body = body.Replace("[AdvanceNo]", advanceNo);
            body = body.Replace("[TotalAmount]", String.Format("{0:n}", totalAmount));
            body = body.Replace("[CurrencyAdvance]", advance.AdvanceCurrency);
            body = body.Replace("[JobIds]", jobIds);
            body = body.Replace("[RequestDate]", advance.RequestDate.Value.ToString("dd/MM/yyyy"));
            body = body.Replace("[Comment]", comment);
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", "#/home/accounting/advance-payment");
            body = body.Replace("[AdvanceId]", advance.Id.ToString());
            List<string> toEmails = new List<string> {
                emailRequester
            };
            List<string> attachments = null;
            List<string> emailCCs = new List<string>
            {
                //Add các email của các user đã approve trước đó (không cần thiết)
            };

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        //Logo eFMS
        private string logoeFMSBase64()
        {
            return "data:image/png;base64,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";
        }

        //Kiểm tra User đăng nhập vào có thuộc các user Approve Advance không, nếu không thuộc bất kỳ 1 user nào thì gán cờ IsApproved bằng true
        //Kiểm tra xem dept đã approve chưa, nếu dept của user đó đã approve thì gán cờ IsApproved bằng true
        private bool CheckUserInApproveAdvanceAndDeptApproved(string userCurrent, AcctApproveAdvanceModel approveAdvance)
        {
            var isApproved = false;
            if (userCurrent == approveAdvance.Requester) //Requester
            {
                isApproved = true;
                if (approveAdvance.RequesterAprDate == null)
                {
                    isApproved = false;
                }
            }
            else if (userCurrent == approveAdvance.Leader) //Leader
            {
                isApproved = true;
                if (approveAdvance.LeaderAprDate == null)
                {
                    isApproved = false;
                }
            }
            else if (userCurrent == approveAdvance.Manager || userCurrent == approveAdvance.ManagerApr) //Dept Manager
            {
                isApproved = true;
                if (string.IsNullOrEmpty(approveAdvance.ManagerApr) && approveAdvance.ManagerAprDate == null)
                {
                    isApproved = false;
                }
            }
            else if (userCurrent == approveAdvance.Accountant || userCurrent == approveAdvance.AccountantApr) //Accountant Manager
            {
                isApproved = true;
                if (string.IsNullOrEmpty(approveAdvance.AccountantApr) && approveAdvance.AccountantAprDate == null)
                {
                    isApproved = false;
                }
            }
            else if (userCurrent == approveAdvance.Buhead || userCurrent == approveAdvance.BuheadApr) //BUHead
            {
                isApproved = true;
                if (string.IsNullOrEmpty(approveAdvance.BuheadApr) && approveAdvance.BuheadAprDate == null)
                {
                    isApproved = false;
                }
            }
            else
            {
                //Đây là trường hợp các User không thuộc Approve Advance
                isApproved = true;
            }
            return isApproved;
        }

        public AcctApproveAdvanceModel GetInfoApproveAdvanceByAdvanceNo(string advanceNo)
        {
            var userCurrent = currentUser.UserID;

            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var approveAdvance = dc.AcctApproveAdvance.Where(x => x.AdvanceNo == advanceNo && x.IsDeputy == false).FirstOrDefault();
            var aprAdvanceMap = new AcctApproveAdvanceModel();

            if (approveAdvance != null)
            {
                aprAdvanceMap = mapper.Map<AcctApproveAdvanceModel>(approveAdvance);

                //Kiểm tra User đăng nhập vào có thuộc các user Approve Advance không, nếu không thuộc bất kỳ 1 user nào thì gán cờ IsApproved bằng true
                //Kiểm tra xem dept đã approve chưa, nếu dept của user đó đã approve thì gán cờ IsApproved bằng true           
                aprAdvanceMap.IsApproved = CheckUserInApproveAdvanceAndDeptApproved(userCurrent, aprAdvanceMap);
                aprAdvanceMap.RequesterName = string.IsNullOrEmpty(aprAdvanceMap.Requester) ? null : GetEmployeeByUserId(aprAdvanceMap.Requester).EmployeeNameVn;
                aprAdvanceMap.LeaderName = string.IsNullOrEmpty(aprAdvanceMap.Leader) ? null : GetEmployeeByUserId(aprAdvanceMap.Leader).EmployeeNameVn;
                aprAdvanceMap.ManagerName = string.IsNullOrEmpty(aprAdvanceMap.Manager) ? null : GetEmployeeByUserId(aprAdvanceMap.Manager).EmployeeNameVn;
                aprAdvanceMap.AccountantName = string.IsNullOrEmpty(aprAdvanceMap.Accountant) ? null : GetEmployeeByUserId(aprAdvanceMap.Accountant).EmployeeNameVn;
                aprAdvanceMap.BUHeadName = string.IsNullOrEmpty(aprAdvanceMap.Buhead) ? null : GetEmployeeByUserId(aprAdvanceMap.Buhead).EmployeeNameVn;
            }
            else
            {
                //Mặc định nếu chưa send request thì gán IsApproved bằng true (nhằm để disable button Approve & Deny)
                aprAdvanceMap.IsApproved = true;
            }
            return aprAdvanceMap;
        }

        #endregion APPROVAL ADVANCE PAYMENT

    }
}
