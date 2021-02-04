﻿using AutoMapper;
using eFMS.API.Common.Helpers;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace eFMS.API.Documentation.DL.Services
{
    public class ShipmentService : RepositoryBase<CsTransaction, CsTransactionModel>, IShipmentService
    {
        readonly IContextBase<OpsTransaction> opsRepository;
        readonly IContextBase<CsTransactionDetail> detailRepository;
        readonly IContextBase<CsShipmentSurcharge> surCharge;
        readonly IContextBase<CatPartner> catPartnerRepo;
        readonly IContextBase<OpsStageAssigned> opsStageAssignedRepo;
        private readonly ICurrentUser currentUser;
        readonly IContextBase<SysUser> sysUserRepo;
        readonly IContextBase<SysEmployee> sysEmployeeRepo;
        readonly IContextBase<CatCurrencyExchange> catCurrencyExchangeRepo;
        readonly IContextBase<CatPlace> catPlaceRepo;
        readonly IContextBase<CatCharge> catChargeRepo;
        readonly IContextBase<CatUnit> catUnitRepo;

        readonly IContextBase<CatChargeGroup> catChargeGroupRepo;
        readonly IContextBase<SysOffice> sysOfficeRepo;
        readonly IContextBase<SysUserLevel> sysUserLevelRepo;
        readonly IContextBase<CustomsDeclaration> customsDeclarationRepo;
        readonly IContextBase<CatCommodity> catCommodityRepo;
        readonly IContextBase<CatCommodityGroup> catCommodityGroupRepo;
        readonly IContextBase<CatDepartment> departmentRepository;
        private readonly ICurrencyExchangeService currencyExchangeService;

        public ShipmentService(IContextBase<CsTransaction> repository, IMapper mapper,
            IContextBase<OpsTransaction> ops,
            IContextBase<CsTransactionDetail> detail,
            IContextBase<CsShipmentSurcharge> surcharge,
            IContextBase<CatPartner> catPartner,
            IContextBase<OpsStageAssigned> opsStageAssigned,
            ICurrentUser user,
            IContextBase<SysUser> sysUser,
            IContextBase<SysEmployee> sysEmployee,
            IContextBase<CatCurrencyExchange> catCurrencyExchange,
            IContextBase<CatPlace> catPlace,
            IContextBase<CatCharge> catCharge,
            IContextBase<CatChargeGroup> catChargeGroup,
            IContextBase<SysOffice> sysOffice,
            IContextBase<SysUserLevel> sysUserLevel,
            IContextBase<CatUnit> catUnit,
            IContextBase<CustomsDeclaration> customsDeclaration,
            IContextBase<CatCommodity> catCommodity,
            IContextBase<CatCommodityGroup> catCommodityGroup,
            ICurrencyExchangeService currencyExchange,
            IContextBase<CatDepartment> departmentRepo) : base(repository, mapper)
        {
            opsRepository = ops;
            detailRepository = detail;
            surCharge = surcharge;
            catPartnerRepo = catPartner;
            opsStageAssignedRepo = opsStageAssigned;
            currentUser = user;
            sysEmployeeRepo = sysEmployee;
            sysUserRepo = sysUser;
            catCurrencyExchangeRepo = catCurrencyExchange;
            catPlaceRepo = catPlace;
            catChargeRepo = catCharge;
            catChargeGroupRepo = catChargeGroup;
            sysOfficeRepo = sysOffice;
            sysUserLevelRepo = sysUserLevel;
            currencyExchangeService = currencyExchange;
            catUnitRepo = catUnit;
            customsDeclarationRepo = customsDeclaration;
            catCommodityRepo = catCommodity;
            catCommodityGroupRepo = catCommodityGroup;
            departmentRepository = departmentRepo;
        }

        public IQueryable<Shipments> GetShipmentNotLocked()
        {
            var userCurrent = currentUser.UserID;
            //Start change request Modified 14/10/2019 by Andy.Hoa
            //Get list shipment operation theo user current
            var shipmentsOperation = from ops in opsRepository.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED && x.IsLocked == false)
                                     join osa in opsStageAssignedRepo.Get() on ops.Id equals osa.JobId
                                     where osa.MainPersonInCharge == userCurrent
                                     select new Shipments
                                     {
                                         Id = ops.Id,
                                         JobId = ops.JobNo,
                                         HBL = ops.Hwbno,
                                         MBL = ops.Mblno,
                                         CustomerId = ops.CustomerId,
                                         AgentId = ops.AgentId,
                                         CarrierId = ops.SupplierId,
                                         HBLID = ops.Hblid
                                     };
            shipmentsOperation = shipmentsOperation.GroupBy(x => new { x.Id, x.JobId, x.HBL, x.MBL, x.CustomerId, x.AgentId, x.CarrierId, x.HBLID }).Select(s => new Shipments
            {
                Id = s.Key.Id,
                JobId = s.Key.JobId,
                HBL = s.Key.HBL,
                MBL = s.Key.MBL,
                CustomerId = s.Key.CustomerId,
                AgentId = s.Key.AgentId,
                CarrierId = s.Key.CarrierId,
                HBLID = s.Key.HBLID,
                Service = "CL"
            });
            shipmentsOperation = shipmentsOperation.Distinct();

            //End change request
            var transactions = from cst in DataContext.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED && x.IsLocked == false)
                               join osa in opsStageAssignedRepo.Get() on cst.Id equals osa.JobId
                               where osa.MainPersonInCharge == userCurrent
                               select cst;
            var shipmentsDocumention = transactions.Join(detailRepository.Get(), x => x.Id, y => y.JobId, (x, y) => new { x, y }).Select(x => new Shipments
            {
                Id = x.x.Id,
                JobId = x.x.JobNo,
                HBL = x.y.Hwbno,
                MBL = x.x.Mawb,
                CustomerId = x.y.CustomerId,
                AgentId = x.x.AgentId,
                CarrierId = x.x.ColoaderId,
                HBLID = x.y.Id,
                Service = x.x.TransactionType
            });
            shipmentsDocumention = shipmentsDocumention.Distinct();

            var shipments = shipmentsOperation.Union(shipmentsDocumention);
            return shipments;
        }

        public IQueryable<Shipments> GetShipmentsCreditPayer(string partner, List<string> services)
        {
            //Chỉ lấy ra những phí Credit(BUY) & Payer (chưa bị lock)
            var surcharge = surCharge.Get(x =>
                    (x.Type == DocumentConstants.CHARGE_BUY_TYPE || (x.PayerId != null && x.CreditNo != null))
                && (x.PayerId == partner || x.PaymentObjectId == partner)
            );

            var transactions = DataContext.Get(x => x.IsLocked == false && services.Contains(x.TransactionType));
            var shipmentDocumention = transactions.Join(detailRepository.Get(), x => x.Id, y => y.JobId, (x, y) => new { x, y }).Select(x => new Shipments
            {
                Id = x.y.Id,
                JobId = x.x.JobNo,
                HBL = x.y.Hwbno,
                MBL = x.x.Mawb,
            });
            var shipmentsDocumention = surcharge.Join(shipmentDocumention, x => x.Hblid, y => y.Id, (x, y) => new { x, y })
                .Select(x => new Shipments
                {
                    Id = x.x.Id,
                    JobId = x.y.JobId,
                    HBL = x.y.HBL,
                    MBL = x.y.MBL,
                });

            IQueryable<Shipments> shipments = shipmentsDocumention
                .Where(x => x.JobId != null && x.HBL != null && x.MBL != null)
                .Select(s => new Shipments { JobId = s.JobId, HBL = s.HBL, MBL = s.MBL });
            //Nếu có chứa Service Custom Logistic
            if (services.Contains("CL"))
            {
                var shipmentOperation = opsRepository.Get(x => x.IsLocked == false && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED);
                var shipmentsOperation = from ops in shipmentOperation
                                         join customNo in customsDeclarationRepo.Get() on ops.JobNo equals customNo.JobNo into opsCustom
                                         from opsC in opsCustom.DefaultIfEmpty()
                                         join sur in surcharge on ops.Hblid equals sur.Hblid into opsSurcharge
                                         from opsSur in opsCustom.DefaultIfEmpty()
                                         select new Shipments
                                         {
                                             JobId = ops.JobNo,
                                             HBL = ops.Hwbno,
                                             MBL = ops.Mblno,
                                         };

                shipments = shipmentsDocumention.Union(shipmentsOperation).Where(x => x.JobId != null && x.HBL != null && x.MBL != null).Select(s => new Shipments { JobId = s.JobId, HBL = s.HBL, MBL = s.MBL });
            }

            var shipmentsResult = shipments.GroupBy(x => new { x.JobId, x.HBL, x.MBL }).Select(s => new Shipments
            {
                JobId = s.Key.JobId,
                HBL = s.Key.HBL,
                MBL = s.Key.MBL
            });
            return shipmentsResult;
        }

        public List<ShipmentsCopy> GetListShipmentBySearchOptions(string searchOption, List<string> keywords)
        {
            var userCurrent = currentUser.UserID;

            var dataList = new List<ShipmentsCopy>();

            if (string.IsNullOrEmpty(searchOption) || keywords == null || keywords.Count == 0 || keywords.Any(x => x == null)) return dataList;

            var surcharge = surCharge.Get();

            //Start change request Modified 14/10/2019 by Andy.Hoa
            //Get list shipment operation theo user current
            var opstransaction = opsRepository.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED);

            //OPS assign
            var opstranAssign = from ops in opstransaction
                                join osa in opsStageAssignedRepo.Get() on ops.Id equals osa.JobId //So sánh bằng
                                where osa.MainPersonInCharge == userCurrent
                                select ops;
            //OPS is BillingOps
            var opstranPic = opstransaction.Where(x => x.BillingOpsId == userCurrent);
            var opstran = opstranAssign.Union(opstranPic);
            var shipmentOperation = from ops in opstran
                                    join sur in surcharge on ops.Hblid equals sur.Hblid into sur2
                                    from sur in sur2.DefaultIfEmpty()
                                    join cus in catPartnerRepo.Get() on ops.CustomerId equals cus.Id into cus2
                                    from cus in cus2.DefaultIfEmpty()
                                    where
                                        searchOption.Equals("JobNo") ? keywords.Contains(ops.JobNo, StringComparer.OrdinalIgnoreCase) : true
                                    &&
                                        searchOption.Equals("Hwbno") ? keywords.Contains(ops.Hwbno, StringComparer.OrdinalIgnoreCase) : true
                                    &&
                                        searchOption.Equals("Mawb") ? keywords.Contains(ops.Mblno, StringComparer.OrdinalIgnoreCase) : true
                                    &&
                                        searchOption.Equals("ClearanceNo") ? keywords.Contains(sur.ClearanceNo, StringComparer.OrdinalIgnoreCase) : true
                                    select new ShipmentsCopy
                                    {
                                        JobId = ops.JobNo,
                                        Customer = cus.ShortName,
                                        MBL = ops.Mblno,
                                        HBL = ops.Hwbno,
                                        HBLID = ops.Hblid,
                                        CustomNo = sur.ClearanceNo,
                                        Service = "CL"
                                    };
            shipmentOperation = shipmentOperation.Distinct();
            //End change request

            var transactions = DataContext.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED);
            //Transaction assign
            var cstranAssign = from cstd in transactions
                               join osa in opsStageAssignedRepo.Get() on cstd.Id equals osa.JobId //So sánh bằng
                               where osa.MainPersonInCharge == userCurrent
                               select cstd;
            //Transaction is Person In Charge
            var cstranPic = transactions.Where(x => x.PersonIncharge == userCurrent);
            var cstrans = cstranAssign.Union(cstranPic);

            var cstrandel = detailRepository.Get();

            var shipmentDoc = from cstd in cstrandel
                              join cst in cstrans on cstd.JobId equals cst.Id into cst2
                              from cst in cst2.DefaultIfEmpty()
                              join sur in surcharge on cstd.Id equals sur.Hblid into sur2
                              from sur in sur2.DefaultIfEmpty()
                              join cus in catPartnerRepo.Get() on cstd.CustomerId equals cus.Id into cus2
                              from cus in cus2.DefaultIfEmpty()
                              where
                                    searchOption.Equals("JobNo") ? keywords.Contains(cst.JobNo, StringComparer.OrdinalIgnoreCase) : true
                                &&
                                    searchOption.Equals("Hwbno") ? keywords.Contains(cstd.Hwbno, StringComparer.OrdinalIgnoreCase) : true
                                &&
                                    searchOption.Equals("Mawb") ? keywords.Contains(cstd.Mawb, StringComparer.OrdinalIgnoreCase) : true
                                &&
                                    searchOption.Equals("ClearanceNo") ? keywords.Contains(sur.ClearanceNo, StringComparer.OrdinalIgnoreCase) : true
                              select new ShipmentsCopy
                              {
                                  JobId = cst.JobNo,
                                  Customer = cus.ShortName,
                                  MBL = cst.Mawb,
                                  HBL = cstd.Hwbno,
                                  HBLID = cstd.Id,
                                  CustomNo = sur.ClearanceNo,
                                  Service = cst.TransactionType
                              };
            shipmentDoc = shipmentDoc.Distinct();

            var query = shipmentOperation.Union(shipmentDoc);
            var listShipment = query.Where(x => x.JobId != null && x.HBL != null && x.MBL != null)
                            .GroupBy(x => new { x.JobId, x.Customer, x.MBL, x.HBL, x.HBLID, x.CustomNo, x.Service })
                            .Select(s => new ShipmentsCopy
                            {
                                JobId = s.Key.JobId,
                                Customer = s.Key.Customer,
                                MBL = s.Key.MBL,
                                HBL = s.Key.HBL,
                                HBLID = s.Key.HBLID,
                                CustomNo = s.Key.CustomNo,
                                Service = s.Key.Service
                            });

            dataList = listShipment.AsEnumerable().Select((x, index) => new ShipmentsCopy
            {
                No = index + 1,
                JobId = x.JobId,
                Customer = x.Customer,
                MBL = x.MBL,
                HBL = x.HBL,
                HBLID = x.HBLID,
                CustomNo = x.CustomNo,
                Service = Common.CustomData.Services.FirstOrDefault(s => s.Value == x.Service)?.DisplayName
            }).ToList();
            return dataList;
        }

        private IQueryable<CsTransaction> GetShipmentServicesByTime(IQueryable<CsTransaction> csTransactions, ShipmentCriteria criteria)
        {
            switch (criteria.TransactionType)
            {
                case TransactionTypeEnum.AirExport:
                    csTransactions = csTransactions.Where(x => (((x.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToDate ?? null)))
                                                        || (criteria.FromDate == null && criteria.ToDate == null));
                    break;
                case TransactionTypeEnum.AirImport:
                    csTransactions = csTransactions.Where(x => (((x.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null));
                    break;
                case TransactionTypeEnum.SeaFCLExport:
                    csTransactions = csTransactions.Where(x => (((x.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null));
                    break;
                case TransactionTypeEnum.SeaFCLImport:
                    csTransactions = csTransactions.Where(x => (((x.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null));
                    break;
                case TransactionTypeEnum.SeaLCLExport:
                    csTransactions = csTransactions.Where(x => (((x.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null));
                    break;
                case TransactionTypeEnum.SeaLCLImport:
                    csTransactions = csTransactions.Where(x => (((x.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null));
                    break;
            }
            return csTransactions;
        }
        public LockedLogResultModel GetShipmentToUnLock(ShipmentCriteria criteria)
        {
            LockedLogResultModel result = new LockedLogResultModel();
            IQueryable<LockedLogModel> opShipments = null;
            string transactionType = string.Empty;
            opShipments = opsRepository.Get(x => ((criteria.ShipmentPropertySearch == ShipmentPropertySearch.JOBID ? criteria.Keywords.Contains(x.JobNo, StringComparer.OrdinalIgnoreCase) : true
                                                    && criteria.ShipmentPropertySearch == ShipmentPropertySearch.MBL ? criteria.Keywords.Contains(x.Mblno, StringComparer.OrdinalIgnoreCase) : true)
                                                    || criteria.Keywords == null)
                                                    &&
                                                    (
                                                            (((x.ServiceDate ?? null) >= (criteria.FromDate ?? null)) && ((x.ServiceDate ?? null) <= (criteria.ToDate ?? null)))
                                                        || (criteria.FromDate == null && criteria.ToDate == null)
                                                    )
                                                    )
                .Select(x => new LockedLogModel
                {
                    Id = x.Id,
                    OPSShipmentNo = x.JobNo,
                    UnLockedLog = x.UnLockedLog,
                    IsLocked = x.IsLocked
                });
            if (criteria.TransactionType == TransactionTypeEnum.CustomLogistic)
            {
                if (opShipments.Any())
                {
                    if (criteria.Keywords != null)
                    {
                        result = opShipments.Count() < criteria.Keywords.Count ? result : GetLogHistory(opShipments);
                    }
                    else
                    {
                        result = GetLogHistory(opShipments);
                    }
                }
                return result;
            }
            if (criteria.TransactionType > 0)
            {

                transactionType = DataTypeEx.GetType(criteria.TransactionType);
            }
            var csTransactions = DataContext.Get(x => ((criteria.ShipmentPropertySearch == ShipmentPropertySearch.JOBID ? criteria.Keywords.Contains(x.JobNo, StringComparer.OrdinalIgnoreCase) : true
                                                 && criteria.ShipmentPropertySearch == ShipmentPropertySearch.MBL ? criteria.Keywords.Contains(x.Mawb, StringComparer.OrdinalIgnoreCase) : true)
                                                    || criteria.Keywords == null)
                                                 && (x.TransactionType == transactionType || string.IsNullOrEmpty(transactionType))
                                              );
            csTransactions = GetShipmentServicesByTime(csTransactions, criteria);
            if (criteria.ShipmentPropertySearch == ShipmentPropertySearch.HBL)
            {
                var shipmentDetails = detailRepository.Get(x => criteria.Keywords.Contains(x.Hwbno, StringComparer.OrdinalIgnoreCase));
                csTransactions = csTransactions.Join(shipmentDetails, x => x.Id, y => y.JobId, (x, y) => x);
            }
            var csShipments = csTransactions
                .Select(x => new LockedLogModel
                {
                    Id = x.Id,
                    CSShipmentNo = x.JobNo,
                    UnLockedLog = x.UnLockedLog,
                    IsLocked = x.IsLocked
                });

            IQueryable<LockedLogModel> shipments = null;

            if (opShipments.Any() || csShipments.Any())
            {
                shipments = opShipments.Union(csShipments);
                if (criteria.Keywords != null)
                {
                    result = shipments.Count() < criteria.Keywords.Count ? result : GetLogHistory(shipments);
                }
                else
                {
                    result = GetLogHistory(shipments);
                }
            }
            return result;
        }

        private LockedLogResultModel GetLogHistory(IQueryable<LockedLogModel> shipments)
        {
            var result = new LockedLogResultModel();
            if (shipments == null) return result;
            result.LockedLogs = shipments;
            result.Logs = new List<string>();
            foreach (var item in shipments)
            {
                var logs = item.UnLockedLog != null ? item.UnLockedLog.Split(';').Where(x => x.Length > 0).ToList() : new List<string>();
                if (logs.Count > 0)
                {
                    result.Logs.AddRange(logs);
                }
            }
            return result;
        }

        public HandleState UnLockShipment(List<LockedLogModel> shipments)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in shipments)
                    {
                        if (item.OPSShipmentNo != null)
                        {
                            var opsShipment = opsRepository.Get(x => x.Id == item.Id)?.FirstOrDefault();
                            if (opsShipment != null)
                            {
                                opsShipment.IsLocked = false;
                                opsShipment.DatetimeModified = DateTime.Now;
                                opsShipment.UserModified = currentUser.UserID;
                                opsShipment.LastDateUnLocked = DateTime.Now;
                                string log = opsShipment.JobNo + " has been opened at " + string.Format("{0:HH:mm:ss tt}", DateTime.Now) + " on " + DateTime.Now.ToString("dd/MM/yyyy") + " by " + currentUser.UserName + ";";
                                opsShipment.UnLockedLog = opsShipment.UnLockedLog + log;
                                var isSuccessLockOps = opsRepository.Update(opsShipment, x => x.Id == opsShipment.Id);
                            }
                        }
                        else
                        {
                            var csShipment = DataContext.Get(x => x.Id == item.Id)?.FirstOrDefault();
                            if (csShipment != null)
                            {
                                if (csShipment.IsLocked == true)
                                {
                                    csShipment.IsLocked = false;
                                    csShipment.DatetimeModified = DateTime.Now;
                                    csShipment.UserModified = currentUser.UserID;
                                    csShipment.LastDateUnLocked = DateTime.Now;
                                    string log = csShipment.JobNo + " has been opened at " + string.Format("{0:HH:mm:ss tt}", DateTime.Now) + " on " + DateTime.Now.ToString("dd/MM/yyyy") + " by " + currentUser.UserName + ";";
                                    csShipment.UnLockedLog = csShipment.UnLockedLog + log;
                                    var isSuccessLockCs = DataContext.Update(csShipment, x => x.Id == csShipment.Id);
                                }
                            }
                        }
                    }
                    trans.Commit();
                    return new HandleState();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        public IQueryable<Shipments> GetShipmentNotDelete()
        {
            //Get list shipment operation: Current Status != 'Canceled'
            //Left join với customsDeclaration
            var shipmentsOperation = from ops in opsRepository.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED)
                                         //.Join(customsDeclarationRepo.Get(), x => x.JobNo, y => y.JobNo, (x, y) => new { x, y })
                                     join cus in customsDeclarationRepo.Get() on ops.JobNo equals cus.JobNo into cus2
                                     from cus in cus2.DefaultIfEmpty()
                                     select new Shipments
                                     {
                                         Id = ops.Id,
                                         JobId = ops.JobNo,
                                         HBL = ops.Hwbno,
                                         MBL = ops.Mblno,
                                         CustomerId = ops.CustomerId,
                                         AgentId = ops.AgentId,
                                         CarrierId = ops.SupplierId,
                                         HBLID = ops.Hblid,
                                         CustomNo = cus.ClearanceNo
                                     };
            shipmentsOperation = shipmentsOperation.GroupBy(x => new { x.Id, x.JobId, x.HBL, x.MBL, x.CustomerId, x.AgentId, x.CarrierId, x.HBLID, x.CustomNo }).Select(s => new Shipments
            {
                Id = s.Key.Id,
                JobId = s.Key.JobId,
                HBL = s.Key.HBL,
                MBL = s.Key.MBL,
                CustomerId = s.Key.CustomerId,
                AgentId = s.Key.AgentId,
                CarrierId = s.Key.CarrierId,
                HBLID = s.Key.HBLID,
                Service = "CL",
                CustomNo = s.Key.CustomNo
            });
            //Get list shipment document: Current Status != 'Canceled'
            var transactions = DataContext.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED);
            var shipmentsDocumention = transactions.Join(detailRepository.Get(), x => x.Id, y => y.JobId, (x, y) => new { x, y }).Select(x => new Shipments
            {
                Id = x.x.Id,
                JobId = x.x.JobNo,
                HBL = x.y.Hwbno,
                MBL = x.x.Mawb,
                CustomerId = x.y.CustomerId,
                AgentId = x.x.AgentId,
                CarrierId = x.x.ColoaderId,
                HBLID = x.y.Id,
                Service = x.x.TransactionType
            });
            var shipments = shipmentsOperation.Union(shipmentsDocumention);
            return shipments;
        }

        public IQueryable<Shipments> GetShipmentAssignPIC()
        {
            var userCurrent = currentUser.UserID;
            var operations = opsRepository.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED && x.IsLocked == false);
            // Shipment ops assign is current user
            var shipmentsOps = from ops in operations
                               join osa in opsStageAssignedRepo.Get() on ops.Id equals osa.JobId
                               where osa.MainPersonInCharge == userCurrent
                               select ops;
            //Shipment ops PIC is current user
            var shipmentsOpsPIC = operations.Where(x => x.BillingOpsId == userCurrent);
            //Merger Shipment Ops assign & PIC
            var shipmentsOpsMerge = shipmentsOps.Union(shipmentsOpsPIC).Select(s => new Shipments
            {
                Id = s.Id,
                JobId = s.JobNo,
                HBL = s.Hwbno,
                MBL = s.Mblno,
                CustomerId = s.CustomerId,
                AgentId = s.AgentId,
                CarrierId = s.SupplierId,
                HBLID = s.Hblid,
                Service = "CL"
            }).Distinct();

            var _shipmentsOperation = shipmentsOpsMerge.GroupBy(g => g.HBL).Select(s => s.FirstOrDefault());

            var transactions = DataContext.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED && x.IsLocked == false);
            //Shipment doc assign is current user
            var shipmentsDoc = from cst in transactions
                               join osa in opsStageAssignedRepo.Get() on cst.Id equals osa.JobId
                               where osa.MainPersonInCharge == userCurrent
                               select cst;
            //Shipment doc PIC is current user
            var shipmentsDocPIC = transactions.Where(x => x.PersonIncharge == userCurrent);
            //Merge shipment Doc assign & PIC
            var shipmentsDocMerge = shipmentsDoc.Union(shipmentsDocPIC);
            shipmentsDocMerge = shipmentsDocMerge.Distinct();

            var shipmentsDocumention = shipmentsDocMerge.Join(detailRepository.Get(), x => x.Id, y => y.JobId, (x, y) => new { x, y }).Select(x => new Shipments
            {
                Id = x.x.Id,
                JobId = x.x.JobNo,
                HBL = x.y.Hwbno,
                MBL = x.x.Mawb,
                CustomerId = x.y.CustomerId,
                AgentId = x.x.AgentId,
                CarrierId = x.x.ColoaderId,
                HBLID = x.y.Id,
                Service = x.x.TransactionType
            });
            var _shipmentsDocumention = shipmentsDocumention.GroupBy(g => g.HBL).Select(s => s.FirstOrDefault());

            var result = _shipmentsOperation.Union(_shipmentsDocumention);
            return result.OrderByDescending(o => o.MBL);
        }

        #region GET QUERY SEARCH WITH REPORT CRITERIA
        /// <summary>
        /// Filter data on Transaction table
        /// </summary>
        /// <param name="criteria">GeneralReportCriteria</param>
        /// <returns></returns>
        private Expression<Func<CsTransaction, bool>> GetQueryTransationDocumentation(GeneralReportCriteria criteria)
        {
            Expression<Func<CsTransaction, bool>> queryTrans;
            // ServiceDate/DatetimeCreated Search
            if (criteria.ServiceDateFrom != null && criteria.ServiceDateTo != null)
            {
                queryTrans = q =>
                    q.TransactionType.Contains("E") ?
                    (q.Etd.HasValue ? q.Etd.Value.Date >= criteria.ServiceDateFrom.Value.Date && q.Etd.Value.Date <= criteria.ServiceDateTo.Value.Date : false)
                    :
                    (q.Eta.HasValue ? q.Eta.Value.Date >= criteria.ServiceDateFrom.Value.Date && q.Eta.Value.Date <= criteria.ServiceDateTo.Value.Date : false);
            }
            else
            {
                queryTrans = q =>
                    q.DatetimeCreated.HasValue ? q.DatetimeCreated.Value.Date >= criteria.CreatedDateFrom.Value.Date && q.DatetimeCreated.Value.Date <= criteria.CreatedDateTo.Value.Date : false;
            }
            // Search Service
            if (!string.IsNullOrEmpty(criteria.Service))
            {
                queryTrans = queryTrans.And(q => criteria.Service.Contains(q.TransactionType));
            }
            // Search JobId
            if (!string.IsNullOrEmpty(criteria.JobId))
            {
                queryTrans = queryTrans.And(q => q.JobNo == criteria.JobId);
            }
            // Search Mawb
            if (!string.IsNullOrEmpty(criteria.Mawb))
            {
                queryTrans = queryTrans.And(q => q.Mawb == criteria.Mawb);
            }

            var hasSalesman = criteria.SalesMan != null; // Check if Type = Salesman
            if (!hasSalesman)
            {
                // Search Office
                if (!string.IsNullOrEmpty(criteria.OfficeId))
                {
                    queryTrans = queryTrans.And(q => criteria.OfficeId.Contains(q.OfficeId.ToString()));
                }
                // Search Department
                if (!string.IsNullOrEmpty(criteria.DepartmentId))
                {
                    queryTrans = queryTrans.And(q => criteria.DepartmentId.Contains(q.DepartmentId.ToString()));
                }
                // Search Group
                if (!string.IsNullOrEmpty(criteria.GroupId))
                {
                    queryTrans = queryTrans.And(q => criteria.GroupId.Contains(q.GroupId.ToString()));
                }
            }
            // Search Person In Charge
            if (!string.IsNullOrEmpty(criteria.PersonInCharge))
            {
                queryTrans = queryTrans.And(q => criteria.PersonInCharge.Contains(q.PersonIncharge));
            }
            // Search Creator
            if (!string.IsNullOrEmpty(criteria.Creator))
            {
                queryTrans = queryTrans.And(q => criteria.Creator.Contains(q.UserCreated));
            }
            // Search Carrier
            if (!string.IsNullOrEmpty(criteria.CarrierId))
            {
                queryTrans = queryTrans.And(q => q.ColoaderId == criteria.CarrierId);
            }
            // Search Agent
            if (!string.IsNullOrEmpty(criteria.AgentId))
            {
                queryTrans = queryTrans.And(q => q.AgentId == criteria.AgentId);
            }
            // Search Pol
            if (criteria.Pol != null && criteria.Pol != Guid.Empty)
            {
                queryTrans = queryTrans.And(q => q.Pol == criteria.Pol);
            }
            // Search Pod
            if (criteria.Pod != null && criteria.Pod != Guid.Empty)
            {
                queryTrans = queryTrans.And(q => q.Pod == criteria.Pod);
            }
            //// Search CustomerId
            //if (!string.IsNullOrEmpty(criteria.CustomerId))
            //{
            //    queryTrans = queryTrans.And(q => q.AgentId == criteria.CustomerId);
            //}
            return queryTrans;
        }

        /// <summary>
        /// Filter data on TransactionDatail table
        /// </summary>
        /// <param name="criteria">GeneralReportCriteria</param>
        /// <returns></returns>
        private Expression<Func<CsTransactionDetail, bool>> GetQueryTransationDetailDocumentation(GeneralReportCriteria criteria)
        {
            Expression<Func<CsTransactionDetail, bool>> queryTranDetail = null;
            // Search Customer
            if (!string.IsNullOrEmpty(criteria.CustomerId))
            {
                queryTranDetail = q => q.CustomerId == criteria.CustomerId;
            }
            // Search Hawb
            if (!string.IsNullOrEmpty(criteria.Hawb))
            {
                queryTranDetail = queryTranDetail == null ?
                    (q => q.Hwbno == criteria.Hawb)
                    :
                    queryTranDetail.And(q => q.Hwbno == criteria.Hawb);
            }
            var hasSalesman = criteria.SalesMan != null; // Check if Type = Salesman
            if (hasSalesman)
            {
                // Search SalesOffice
                if (!string.IsNullOrEmpty(criteria.OfficeId))
                {
                    queryTranDetail = (queryTranDetail == null) ? (q => !string.IsNullOrEmpty(q.SalesOfficeId))
                                                                : queryTranDetail.And(q => !string.IsNullOrEmpty(q.SalesOfficeId));
                }
                // Search SalesDepartment
                if (!string.IsNullOrEmpty(criteria.DepartmentId))
                {
                    queryTranDetail = (queryTranDetail == null) ? (q => !string.IsNullOrEmpty(q.SalesDepartmentId))
                                                                : queryTranDetail.And(q => !string.IsNullOrEmpty(q.SalesDepartmentId));
                }
                // Search SalesGroup
                if (!string.IsNullOrEmpty(criteria.GroupId))
                {
                    queryTranDetail = (queryTranDetail == null) ? (q => !string.IsNullOrEmpty(q.SalesGroupId))
                                                                : queryTranDetail.And(q => !string.IsNullOrEmpty(q.SalesGroupId));
                }
                // Search SaleMan
                if (!string.IsNullOrEmpty(criteria.SalesMan))
                {
                    queryTranDetail = (queryTranDetail == null) ?
                        (q => criteria.SalesMan.Contains(q.SaleManId))
                        :
                        queryTranDetail.And(q => criteria.SalesMan.Contains(q.SaleManId));
                }
            }
            return queryTranDetail;
        }

        /// <summary>
        /// Filter data on OpsTransaction table
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private Expression<Func<OpsTransaction, bool>> GetQueryOPSTransactionOperation(GeneralReportCriteria criteria, bool? fromCost)
        {
            Expression<Func<OpsTransaction, bool>> queryOpsTrans = q => true;
            // ServiceDate/DatetimeCreated Search
            if (criteria.ServiceDateFrom != null && criteria.ServiceDateTo != null)
            {
                queryOpsTrans = q =>
                    q.ServiceDate.HasValue ? q.ServiceDate.Value.Date >= criteria.ServiceDateFrom.Value.Date && q.ServiceDate.Value.Date <= criteria.ServiceDateTo.Value.Date : false;
            }
            else
            {
                queryOpsTrans = q =>
                    q.DatetimeCreated.HasValue ? q.DatetimeCreated.Value.Date >= criteria.CreatedDateFrom.Value.Date && q.DatetimeCreated.Value.Date <= criteria.CreatedDateTo.Value.Date : false;
            }

            queryOpsTrans = queryOpsTrans.And(q => criteria.Service.Contains("CL") || string.IsNullOrEmpty(criteria.Service));
            // Search Customer
            if (!string.IsNullOrEmpty(criteria.CustomerId) && fromCost != true)
            {
                queryOpsTrans = queryOpsTrans.And(q => q.CustomerId == criteria.CustomerId);
            }
            // Search JobId
            if (!string.IsNullOrEmpty(criteria.JobId))
            {
                queryOpsTrans = queryOpsTrans.And(q => q.JobNo == criteria.JobId);
            }
            // Search Mawb
            if (!string.IsNullOrEmpty(criteria.Mawb))
            {
                queryOpsTrans = queryOpsTrans.And(q => q.Mblno == criteria.Mawb);
            }
            // Search Hawb
            if (!string.IsNullOrEmpty(criteria.Hawb))
            {
                queryOpsTrans = queryOpsTrans.And(q => q.Hwbno == criteria.Hawb);
            }

            var hasSalesman = criteria.SalesMan != null; // Check if Type=Salesman
            // Search Office
            if (!string.IsNullOrEmpty(criteria.OfficeId))
            {
                queryOpsTrans = hasSalesman ? queryOpsTrans.And(q => !string.IsNullOrEmpty(q.SalesOfficeId))
                                            : queryOpsTrans.And(q => criteria.OfficeId.Contains(q.OfficeId.ToString()));
            }
            // Search Department
            if (!string.IsNullOrEmpty(criteria.DepartmentId))
            {
                queryOpsTrans = hasSalesman ? queryOpsTrans.And(q => !string.IsNullOrEmpty(q.SalesDepartmentId))
                                            : queryOpsTrans.And(q => criteria.DepartmentId.Contains(q.DepartmentId.ToString()));
            }
            // Search Group
            if (!string.IsNullOrEmpty(criteria.GroupId))
            {
                queryOpsTrans = hasSalesman ? queryOpsTrans.And(q => !string.IsNullOrEmpty(q.SalesGroupId))
                                            : queryOpsTrans.And(q => criteria.GroupId.Contains(q.GroupId.ToString()));
            }
            // Search Person In Charge
            if (!string.IsNullOrEmpty(criteria.PersonInCharge))
            {
                queryOpsTrans = queryOpsTrans.And(q => criteria.PersonInCharge.Contains(q.BillingOpsId));
            }
            // Search SalesMan
            if (!string.IsNullOrEmpty(criteria.SalesMan))
            {
                queryOpsTrans = queryOpsTrans.And(q => criteria.SalesMan.Contains(q.SalemanId));
            }
            // Search Creator
            if (!string.IsNullOrEmpty(criteria.Creator))
            {
                queryOpsTrans = queryOpsTrans.And(q => criteria.Creator.Contains(q.UserCreated));
            }
            // Search Carrier
            if (!string.IsNullOrEmpty(criteria.CarrierId))
            {
                queryOpsTrans = queryOpsTrans.And(q => q.SupplierId == criteria.CarrierId);
            }
            // Search Agent
            if (!string.IsNullOrEmpty(criteria.AgentId))
            {
                queryOpsTrans = queryOpsTrans.And(q => q.AgentId == criteria.AgentId);
            }
            // Search Pol
            if (criteria.Pol != null && criteria.Pol != Guid.Empty)
            {
                queryOpsTrans = queryOpsTrans.And(q => q.Pol == criteria.Pol);
            }
            // Search Pod
            if (criteria.Pod != null && criteria.Pod != Guid.Empty)
            {
                queryOpsTrans = queryOpsTrans.And(q => q.Pod == criteria.Pod);
            }
            return queryOpsTrans;
        }

        /// <summary>
        /// Get Data Transation Detail with query search
        /// </summary>
        /// <param name="queryTranDetail">Query search</param>
        /// <param name="criteria">Criteria search</param>
        /// <returns>Transaction Detail Documentation after filter</returns>
        private IQueryable<CsTransactionDetail> GetTransactionDetailDocWithSalesman(Expression<Func<CsTransactionDetail, bool>> queryTranDetail, GeneralReportCriteria criteria)
        {
            var houseBills = detailRepository.Get().Where(queryTranDetail);
            var houseBillList = new List<CsTransactionDetail>();
            if (criteria.SalesMan != null)
            {
                foreach (var house in houseBills)
                {
                    if (string.IsNullOrEmpty(criteria.OfficeId) || house.SalesOfficeId.Split(';').Intersect(criteria.OfficeId.Split(';')).Any())
                    {
                        if (string.IsNullOrEmpty(criteria.DepartmentId) || house.SalesDepartmentId.Split(';').Intersect(criteria.DepartmentId.Split(';')).Any())
                        {
                            if (string.IsNullOrEmpty(criteria.GroupId) || house.SalesGroupId.Split(';').Intersect(criteria.GroupId.Split(';')).Any())
                            {
                                houseBillList.Add(house);
                            }
                        }
                    }
                }
                houseBills = houseBillList.AsQueryable();
            }
            return houseBills;
        }

        /// <summary>
        /// Get Data OpsTransation Operation with query search
        /// </summary>
        /// <param name="query">Query search</param>
        /// <param name="criteria">Criteria search</param>
        /// <returns>Transaction Operation after filter</returns>
        private IQueryable<OpsTransaction> GetOpsTransactionWithSalesman(Expression<Func<OpsTransaction, bool>> query, GeneralReportCriteria criteria)
        {
            var shipments = opsRepository.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED);
            shipments = shipments.Where(query);
            var shipmentList = new List<OpsTransaction>();
            if (criteria.SalesMan != null)
            {
                foreach (var shipment in shipments)
                {
                    if (string.IsNullOrEmpty(criteria.OfficeId) || shipment.SalesOfficeId.Split(';').Intersect(criteria.OfficeId.Split(';')).Any())
                    {
                        if (string.IsNullOrEmpty(criteria.DepartmentId) || shipment.SalesDepartmentId.Split(';').Intersect(criteria.DepartmentId.Split(';')).Any())
                        {
                            if (string.IsNullOrEmpty(criteria.GroupId) || shipment.SalesGroupId.Split(';').Intersect(criteria.GroupId.Split(';')).Any())
                            {
                                shipmentList.Add(shipment);
                            }
                        }
                    }
                }
                shipments = shipmentList.AsQueryable();
            }
            return shipments;
        }
        #endregion

        #region -- EXPORT SHIPMENT OVERVIEW
        public IQueryable<GeneralExportShipmentOverviewResult> GetDataGeneralExportShipmentOverview(GeneralReportCriteria criteria)
        {
            IQueryable<GeneralExportShipmentOverviewResult> list;
            var dataShipment = GeneralExportShipmentOverview(criteria);
            var dataOperation = GeneralExportOperationOverview(criteria);
            list = dataShipment.Union(dataOperation);
            return list;
        }


        public IQueryable<GeneralExportShipmentOverviewResult> GeneralExportShipmentOverview(GeneralReportCriteria criteria)
        {
            List<GeneralExportShipmentOverviewResult> lstShipment = new List<GeneralExportShipmentOverviewResult>();
            var dataShipment = QueryDataShipmentOverview(criteria);
            if (!dataShipment.Any()) return lstShipment.AsQueryable();
            var lstSurchage = surCharge.Get();
            var detailLookupSur = lstSurchage.ToLookup(q => q.Hblid);
            var dataOpertation = QueryDataOperation(criteria);
            var PlaceList = catPlaceRepo.Get();
            var PartnerList = catPartnerRepo.Get();
            var LookupPartner = PartnerList.ToLookup(x => x.Id);
            var LookupPlace = PlaceList.ToLookup(x => x.Id);
            var ChargeList = catChargeRepo.Get();
            var LookupCharge = ChargeList.ToLookup(x => x.Id);
            var UserList = sysUserRepo.Get();
            var LookupUser = UserList.ToLookup(x => x.Id);
            var ChargeGroupList = catChargeGroupRepo.Get();
            var ChargeGroupLookup = ChargeGroupList.ToLookup(x => x.Id);
            var OfficeList = sysOfficeRepo.Get();
            var LookupOffice = OfficeList.ToLookup(x => x.Id);
            var UserLevelList = sysUserLevelRepo.Get();
            var LookupUserLevelList = UserLevelList.ToLookup(x => x.UserId);
            var UnitList = catUnitRepo.Get();
            var LookupUnitList = UnitList.ToLookup(x => x.Id);
            foreach (var item in dataShipment)
            {
                GeneralExportShipmentOverviewResult data = new GeneralExportShipmentOverviewResult();
                if (item.ServiceName == TermData.InlandTrucking)
                {
                    data.ServiceName = "Inland Trucking ";
                }
                if (item.ServiceName == TermData.AirExport)
                {
                    data.ServiceName = "Export (Air) ";
                }
                if (item.ServiceName == TermData.AirImport)
                {
                    data.ServiceName = "Import (Air) ";
                }
                if (item.ServiceName == TermData.SeaConsolExport)
                {
                    data.ServiceName = "Export (Sea Consol) ";
                }
                if (item.ServiceName == TermData.SeaConsolImport)
                {
                    data.ServiceName = "Import (Sea Consol) ";
                }
                if (item.ServiceName == TermData.SeaFCLExport)
                {
                    data.ServiceName = "Export (Sea FCL) ";
                }
                if (item.ServiceName == TermData.SeaFCLImport)
                {
                    data.ServiceName = "Import (Sea FCL) ";
                }
                if (item.ServiceName == TermData.SeaLCLExport)
                {
                    data.ServiceName = "Export (Sea LCL) ";
                }
                if (item.ServiceName == TermData.SeaLCLImport)
                {
                    data.ServiceName = "Import (Sea LCL) ";
                }
                data.JobNo = item.JobNo;
                data.etd = item.etd;
                data.eta = item.eta;
                data.FlightNo = item.FlightNo;
                data.MblMawb = item.MblMawb;
                data.HblHawb = item.HblHawb;
                data.PolPod = item.Pol != null && item.Pol != Guid.Empty ? LookupPlace[(Guid)item.Pol].Select(t => t.Code).FirstOrDefault() : string.Empty
                    + "/" + (item.Pod != null && item.Pod != Guid.Empty ? LookupPlace[(Guid)item.Pod].Select(t => t.Code).FirstOrDefault() : string.Empty);
                data.Carrier = !string.IsNullOrEmpty(item.Carrier) ? LookupPartner[item.Carrier].FirstOrDefault()?.ShortName : string.Empty;
                data.Agent = catPartnerRepo.Get(x => x.Id == item.Agent).FirstOrDefault()?.ShortName;
                var ArrayShipperDesc = item.ShipperDescription?.Split("\n").ToArray();
                data.ShipperDescription = ArrayShipperDesc != null && ArrayShipperDesc.Length > 0 ? ArrayShipperDesc[0] : string.Empty;
                var ArrayConsgineeDesc = item.ConsigneeDescription?.Split("\n").ToArray();
                data.ConsigneeDescription = ArrayConsgineeDesc != null && ArrayConsgineeDesc.Length > 0 ? ArrayConsgineeDesc[0] : string.Empty;
                data.Consignee = !string.IsNullOrEmpty(data.ConsigneeDescription) ? data.ConsigneeDescription : catPartnerRepo.Get(x => x.Id == item.Consignee).FirstOrDefault()?.PartnerNameEn;
                data.Shipper = !string.IsNullOrEmpty(data.ShipperDescription) ? data.ShipperDescription : catPartnerRepo.Get(x => x.Id == item.Shipper).FirstOrDefault()?.PartnerNameEn;
                data.ShipmentType = item.ShipmentType;
                data.Salesman = !string.IsNullOrEmpty(item.Salesman) ? LookupUser[item.Salesman].FirstOrDefault()?.Username : string.Empty;
                data.AgentName = catPartnerRepo.Get(x => x.Id == item.Agent).FirstOrDefault()?.PartnerNameVn;
                data.GW = item.GW;
                data.CW = item.CW;
                data.CBM = item.CBM;
                data.Cont20 = item.Cont20;
                data.Cont40 = item.Cont40;
                data.Cont40HC = item.Cont40HC;
                data.Cont45 = item.Cont45;
                data.QTy = item.QTy;
                #region -- Phí Selling trước thuế --
                decimal? _totalSellAmountFreight = 0;
                decimal? _totalSellAmountTrucking = 0;
                decimal? _totalSellAmountHandling = 0;
                decimal? _totalSellAmountOther = 0;
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeSell = detailLookupSur[(Guid)item.HblId].Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE);
                    foreach (var charge in _chargeSell)
                    {
                        var chargeObj = LookupCharge[charge.ChargeId].Select(t => t).FirstOrDefault();
                        CatChargeGroup ChargeGroupModel = new CatChargeGroup();
                        ChargeGroupModel = charge.ChargeGroup != null && charge.ChargeGroup != Guid.Empty ? ChargeGroupLookup[(Guid)charge.ChargeGroup].FirstOrDefault() : null;
                        if (ChargeGroupModel == null)
                        {
                            ChargeGroupModel = chargeObj.ChargeGroup != null && chargeObj.ChargeGroup != Guid.Empty ? ChargeGroupLookup[(Guid)chargeObj.ChargeGroup].FirstOrDefault() : null;
                        }
                        // tinh total phi chargeGroup freight
                        if (ChargeGroupModel?.Name == "Freight")
                        {
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountFreight += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountFreight += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        if (ChargeGroupModel?.Name == "Trucking")
                        {
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountTrucking += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountTrucking += charge.AmountVnd;  // Phí Selling trước thuế
                            }
                        }
                        if (ChargeGroupModel?.Name == "Handling")
                        {
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountHandling += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountHandling += charge.AmountVnd;  // Phí Selling trước thuế
                            }
                        }
                        if (ChargeGroupModel?.Name != "Handling" && ChargeGroupModel?.Name != "Trucking" && ChargeGroupModel?.Name != "Freight")
                        {
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountOther += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountOther += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        //END SEL
                    }
                }
                data.TotalSellFreight = _totalSellAmountFreight;
                data.TotalSellTrucking = _totalSellAmountTrucking;
                data.TotalSellHandling = _totalSellAmountHandling;
                data.TotalSellOthers = _totalSellAmountOther;
                data.TotalSell = data.TotalSellFreight + data.TotalSellTrucking + data.TotalSellHandling + data.TotalSellOthers;
                #endregion
                #region -- Phí Buying trước thuế --
                decimal? _totalBuyAmountFreight = 0;
                decimal? _totalBuyAmountTrucking = 0;
                decimal? _totalBuyAmountHandling = 0;
                decimal? _totalBuyAmountOther = 0;
                decimal? _totalBuyAmountKB = 0;
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeBuy = detailLookupSur[(Guid)item.HblId].Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE);
                    foreach (var charge in _chargeBuy)
                    {
                        var chargeObj = LookupCharge[charge.ChargeId].Select(t => t).FirstOrDefault();
                        CatChargeGroup ChargeGroupModel = new CatChargeGroup();
                        ChargeGroupModel = charge.ChargeGroup != null && charge.ChargeGroup != Guid.Empty ? ChargeGroupLookup[(Guid)charge.ChargeGroup].FirstOrDefault() : null;
                        if (ChargeGroupModel == null)
                        {
                            ChargeGroupModel = chargeObj.ChargeGroup != null && chargeObj.ChargeGroup != Guid.Empty ? ChargeGroupLookup[(Guid)chargeObj.ChargeGroup].FirstOrDefault() : null;
                        }
                        // tinh total phi chargeGroup freight
                        if (ChargeGroupModel?.Name == "Freight")
                        {
                            if (charge.KickBack == true)
                            {
                                _totalBuyAmountFreight = 0;
                            }
                            else
                            {
                                if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                                {
                                    _totalBuyAmountFreight += charge.AmountUsd; // Phí Selling trước thuế
                                }
                                else
                                {
                                    _totalBuyAmountFreight += charge.AmountVnd;
                                }
                            }

                        }
                        if (ChargeGroupModel?.Name == "Trucking")
                        {
                            if (charge.KickBack == true)
                            {
                                _totalBuyAmountTrucking = 0;
                            }
                            else
                            {
                                if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                                {
                                    _totalBuyAmountTrucking += charge.AmountUsd;
                                }
                                else
                                {
                                    _totalBuyAmountTrucking += charge.AmountVnd; // Phí Selling trước thuế
                                }
                            }

                        }
                        if (ChargeGroupModel?.Name == "Handling")
                        {
                            if (charge.KickBack == true)
                            {
                                _totalBuyAmountHandling = 0;
                            }
                            else
                            {
                                if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                                {
                                    _totalBuyAmountHandling += charge.AmountUsd;
                                }
                                else
                                {
                                    _totalBuyAmountHandling += charge.AmountVnd; // Phí Selling trước thuế
                                }
                            }
                        }
                        if (ChargeGroupModel?.Name != "Handling" && ChargeGroupModel?.Name != "Trucking" && ChargeGroupModel?.Name != "Freight" && ChargeGroupModel?.Name != "Com")
                        {
                            if (charge.KickBack == true)
                            {
                                _totalBuyAmountOther = 0;
                            }
                            else
                            {
                                if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                                {
                                    _totalBuyAmountOther += charge.AmountUsd;
                                }
                                else
                                {
                                    _totalBuyAmountOther += charge.AmountVnd; // Phí Selling trước thuế
                                }
                            }
                        }
                        if (charge.KickBack == true || ChargeGroupModel?.Name == "Com")
                        {
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountKB += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountKB += charge.AmountVnd;
                            }
                        }

                        //END BUY
                    }
                }

                data.TotalBuyFreight = _totalBuyAmountFreight;
                data.TotalBuyTrucking = _totalBuyAmountTrucking;
                data.TotalBuyHandling = _totalBuyAmountHandling;
                data.TotalBuyOthers = _totalBuyAmountOther;
                data.TotalBuyKB = _totalBuyAmountKB;
                data.TotalBuy = data.TotalBuyFreight + data.TotalBuyTrucking + data.TotalBuyHandling + data.TotalBuyOthers + data.TotalBuyKB;
                data.Profit = data.TotalSell - data.TotalBuy;
                #endregion -- Phí Buying trước thuế --

                #region -- Phí OBH sau thuế --
                decimal? _obh = 0;
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeObh = detailLookupSur[(Guid)item.HblId].Where(x => x.Type == DocumentConstants.CHARGE_OBH_TYPE);
                    foreach (var charge in _chargeObh)
                    {
                        _obh += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, criteria.Currency);
                    }
                }

                data.AmountOBH = _obh;
                #endregion -- Phí OBH sau thuế --
                data.Destination = item.Pod != null && item.Pod != Guid.Empty ? LookupPlace[(Guid)item.Pod].Select(t => t.NameVn).FirstOrDefault() : string.Empty;
                data.RalatedHblHawb = string.Empty;// tạm thời để trống
                data.RalatedJobNo = string.Empty;// tạm thời để trống
                data.HandleOffice = item.OfficeId != null && item.OfficeId != Guid.Empty ? LookupOffice[(Guid)item.OfficeId].Select(t => t.Code).FirstOrDefault() : string.Empty;
                var OfficeSaleman = LookupUserLevelList[item.Salesman].Select(t => t.OfficeId).FirstOrDefault();
                data.SalesOffice = OfficeSaleman != Guid.Empty && OfficeSaleman != null ? LookupOffice[(Guid)OfficeSaleman].Select(t => t.Code).FirstOrDefault() : string.Empty;
                data.Creator = LookupUser[item.Creator].Select(t => t.Username).FirstOrDefault();
                data.POINV = item.POINV;
                data.BKRefNo = item.JobNo;
                data.Commodity = item.Commodity;
                data.ServiceMode = item.ServiceMode;//chua co thong tin
                data.PMTerm = item.PMTerm;
                data.ShipmentNotes = item.ShipmentNotes;
                data.Created = item.Created;
                data.CustomerId = LookupPartner[item.CustomerId].Select(t => t.AccountNo).FirstOrDefault();
                data.CustomerName = LookupPartner[item.CustomerId].Select(t => t.ShortName).FirstOrDefault();
                string Code = item.PackageQty != null ? LookupUnitList[(short)item.PackageQty].Select(t => t.Code).FirstOrDefault() : string.Empty;
                data.QTy = item.QTy + " " + Code;
                lstShipment.Add(data);
            }
            return lstShipment.AsQueryable();
        }

        public IQueryable<GeneralExportShipmentOverviewResult> GeneralExportOperationOverview(GeneralReportCriteria criteria)
        {
            List<GeneralExportShipmentOverviewResult> lstShipment = new List<GeneralExportShipmentOverviewResult>();
            var dataOpertation = QueryDataOperation(criteria);
            if (!dataOpertation.Any()) return lstShipment.AsQueryable();
            var lstSurchage = surCharge.Get();
            var detailLookupSur = lstSurchage.ToLookup(q => q.Hblid);
            var PlaceList = catPlaceRepo.Get();
            var PartnerList = catPartnerRepo.Get();
            var LookupPartner = PartnerList.ToLookup(x => x.Id);
            var LookupPlace = PlaceList.ToLookup(x => x.Id);
            var ChargeList = catChargeRepo.Get();
            var LookupCharge = ChargeList.ToLookup(x => x.Id);
            var UserList = sysUserRepo.Get();
            var LookupUser = UserList.ToLookup(x => x.Id);
            var ChargeGroupList = catChargeGroupRepo.Get();
            var ChargeGroupLookup = ChargeGroupList.ToLookup(x => x.Id);
            var OfficeList = sysOfficeRepo.Get();
            var LookupOffice = OfficeList.ToLookup(x => x.Id);
            var UserLevelList = sysUserLevelRepo.Get();
            var LookupUserLevelList = UserLevelList.ToLookup(x => x.UserId);
            foreach (var item in dataOpertation)
            {
                GeneralExportShipmentOverviewResult data = new GeneralExportShipmentOverviewResult();
                data.ServiceName = API.Common.Globals.CustomData.Services.Where(x => x.Value == "CL").FirstOrDefault()?.DisplayName;
                data.JobNo = item.JobNo;
                data.PolPod = item.Pol != null && item.Pol != Guid.Empty ? LookupPlace[(Guid)item.Pol].Select(t => t.Code).FirstOrDefault() : string.Empty
                    + "/" +  (item.Pod != null && item.Pod != Guid.Empty ? LookupPlace[(Guid)item.Pod].Select(t => t.Code).FirstOrDefault() : string.Empty);
                data.Shipper = LookupPartner[item.Shipper].Select(t => t.PartnerNameEn).FirstOrDefault();
                data.Consignee = item.Consignee;
                data.MblMawb = item.Mblno;
                data.HblHawb = item.Hwbno;
                data.CustomerId = !string.IsNullOrEmpty(item.CustomerId) ?  LookupPartner[item.CustomerId].Select(t=>t.AccountNo).FirstOrDefault() : string.Empty;
                #region -- Phí Selling trước thuế --
                decimal? _totalSellAmountFreight = 0;
                decimal? _totalSellAmountTrucking = 0;
                decimal? _totalSellAmountHandling = 0;
                decimal? _totalSellAmountOther = 0;
                if (item.Hblid != null && item.Hblid != Guid.Empty)
                {
                    var _chargeSell = detailLookupSur[(Guid)item.Hblid].Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE);

                    foreach (var charge in _chargeSell)
                    {

                        var chargeObj = LookupCharge[charge.ChargeId].Select(t=>t).FirstOrDefault();
                        CatChargeGroup ChargeGroupModel = new CatChargeGroup();
                        ChargeGroupModel = charge.ChargeGroup != null && charge.ChargeGroup != Guid.Empty ?  ChargeGroupLookup[(Guid)charge.ChargeGroup].FirstOrDefault() : null;
                        if (ChargeGroupModel == null)
                        {
                            ChargeGroupModel = chargeObj.ChargeGroup != null && chargeObj.ChargeGroup != Guid.Empty?  ChargeGroupLookup[(Guid)chargeObj.ChargeGroup].FirstOrDefault() : null;
                        }
                        //SELL

                        // tinh total phi chargeGroup freight
                        if (ChargeGroupModel?.Name == "Freight")
                        {
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountFreight += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountFreight += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        if (ChargeGroupModel?.Name == "Trucking")
                        {
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountTrucking += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountTrucking += charge.AmountVnd;  // Phí Selling trước thuế
                            }
                        }
                        if (ChargeGroupModel?.Name == "Handling")
                        {
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountHandling += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountHandling += charge.AmountVnd;  // Phí Selling trước thuế
                            }
                        }
                        if (ChargeGroupModel?.Name != "Handling" && ChargeGroupModel?.Name != "Trucking" && ChargeGroupModel?.Name != "Freight")
                        {
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountOther += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountOther += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        //END SEL
                    }
                }

                data.TotalSellFreight = _totalSellAmountFreight;
                data.TotalSellTrucking = _totalSellAmountTrucking;
                data.TotalSellHandling = _totalSellAmountHandling;
                data.TotalSellOthers = _totalSellAmountOther;
                data.TotalSell = data.TotalSellFreight + data.TotalSellTrucking + data.TotalSellHandling + data.TotalSellOthers;
                #endregion
                #region -- Phí Buying trước thuế --
                decimal? _totalBuyAmountFreight = 0;
                decimal? _totalBuyAmountTrucking = 0;
                decimal? _totalBuyAmountHandling = 0;
                decimal? _totalBuyAmountOther = 0;
                decimal? _totalBuyAmountKB = 0;
                var _chargeBuy = detailLookupSur[(Guid)item.Hblid].Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE);
                if (item.Hblid != null && item.Hblid != Guid.Empty)
                {
                    foreach (var charge in _chargeBuy)
                    {
                        var chargeObj = LookupCharge[charge.ChargeId].Select(t => t).FirstOrDefault();
                        CatChargeGroup ChargeGroupModel = new CatChargeGroup();
                        ChargeGroupModel = charge.ChargeGroup != null && charge.ChargeGroup != Guid.Empty ? ChargeGroupLookup[(Guid)charge.ChargeGroup].FirstOrDefault() : null;
                        if (ChargeGroupModel == null)
                        {
                            ChargeGroupModel = chargeObj.ChargeGroup != null && chargeObj.ChargeGroup != Guid.Empty ? ChargeGroupLookup[(Guid)chargeObj.ChargeGroup].FirstOrDefault() : null;
                        }
                        // tinh total phi chargeGroup freight
                        if (ChargeGroupModel?.Name == "Freight")
                        {
                            if (charge.KickBack == true)
                            {
                                _totalBuyAmountFreight = 0;
                            }
                            else
                            {
                                if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                                {
                                    _totalBuyAmountFreight += charge.AmountUsd; // Phí Selling trước thuế
                                }
                                else
                                {
                                    _totalBuyAmountFreight += charge.AmountVnd;
                                }
                            }

                        }
                        if (ChargeGroupModel?.Name == "Trucking")
                        {
                            if (charge.KickBack == true)
                            {
                                _totalBuyAmountTrucking = 0;
                            }
                            else
                            {
                                if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                                {
                                    _totalBuyAmountTrucking += charge.AmountUsd;
                                }
                                else
                                {
                                    _totalBuyAmountTrucking += charge.AmountVnd; // Phí Selling trước thuế
                                }
                            }

                        }
                        if (ChargeGroupModel?.Name == "Handling")
                        {
                            if (charge.KickBack == true)
                            {
                                _totalBuyAmountHandling = 0;
                            }
                            else
                            {
                                if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                                {
                                    _totalBuyAmountHandling += charge.AmountUsd;
                                }
                                else
                                {
                                    _totalBuyAmountHandling += charge.AmountVnd; // Phí Selling trước thuế
                                }
                            }
                        }
                        if (ChargeGroupModel?.Name != "Handling" && ChargeGroupModel?.Name != "Trucking" && ChargeGroupModel?.Name != "Freight" && ChargeGroupModel?.Name != "Com")
                        {
                            if (charge.KickBack == true)
                            {
                                _totalBuyAmountOther = 0;
                            }
                            else
                            {
                                if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                                {
                                    _totalBuyAmountOther += charge.AmountUsd;
                                }
                                else
                                {
                                    _totalBuyAmountOther += charge.AmountVnd; // Phí Selling trước thuế
                                }
                            }
                        }
                        if (charge.KickBack == true || ChargeGroupModel?.Name == "Com")
                        {
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountKB += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountKB += charge.AmountVnd;
                            }
                        }

                        //END BUY
                    }
                }

                data.TotalBuyFreight = _totalBuyAmountFreight;
                data.TotalBuyTrucking = _totalBuyAmountTrucking;
                data.TotalBuyHandling = _totalBuyAmountHandling;
                data.TotalBuyOthers = _totalBuyAmountOther;
                data.TotalBuyKB = _totalBuyAmountKB;
                data.TotalBuy = data.TotalBuyFreight + data.TotalBuyTrucking + data.TotalBuyHandling + data.TotalBuyOthers + data.TotalBuyKB;
                data.Profit = data.TotalSell - data.TotalBuy;
                #endregion -- Phí Buying trước thuế --

                #region -- Phí OBH sau thuế --
                decimal? _obh = 0;
                if (item.Hblid != null && item.Hblid != Guid.Empty)
                {
                    var _chargeObh = detailLookupSur[(Guid)item.Hblid].Where(x => x.Type == DocumentConstants.CHARGE_OBH_TYPE);
                    foreach (var charge in _chargeObh)
                    {
                        if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                        {
                            _obh += charge.AmountUsd + charge.VatAmountUsd;
                        }
                        else
                        {
                            _obh += charge.AmountVnd + charge.VatAmountVnd;
                        }
                    }
                }
                data.AmountOBH = _obh;
                #endregion -- Phí OBH sau thuế --
                
                data.Destination = item.Pod != null && item.Pod != Guid.Empty ? LookupPlace[(Guid)item.Pod].Select(t=>t.NameVn).FirstOrDefault() : string.Empty;
                data.CustomerName = LookupPartner[item.CustomerId].Select(t=>t.ShortName).FirstOrDefault();
                data.RalatedHblHawb = string.Empty;// tạm thời để trống
                data.RalatedJobNo = string.Empty;// tạm thời để trống
                data.HandleOffice = item.OfficeId != null && item.OfficeId != Guid.Empty ? LookupOffice[(Guid)item.OfficeId].Select(t => t.Code).FirstOrDefault() : string.Empty;
                var OfficeSaleman =  LookupUserLevelList[item.SalemanId].Select(t => t.OfficeId).FirstOrDefault();
                data.SalesOffice = OfficeSaleman != Guid.Empty && OfficeSaleman != null? LookupOffice[(Guid) OfficeSaleman].Select(t => t.Code).FirstOrDefault() : string.Empty ;
                data.BKRefNo = item.JobNo;
                data.ServiceMode = item.ServiceMode;//chua co thong tin
                data.ProductService = item.ProductService;
                data.etd = item.ServiceDate;
                data.Creator = LookupUser[item.BillingOpsId].Select(t=>t.Username).FirstOrDefault();
                data.CustomNo = GetCustomNoOldOfShipment(item.JobNo);
                data.Created = item.DatetimeCreated;
                data.Salesman = LookupUser[item.SalemanId].FirstOrDefault()?.Username;
                data.AgentName = LookupPartner[item.AgentId].Select(t => t.PartnerNameVn).FirstOrDefault();
                data.Agent = LookupPartner[item.AgentId].Select(t => t.ShortName).FirstOrDefault();
                data.Carrier = LookupPartner[item.SupplierId].Select(t => t.ShortName).FirstOrDefault();
                lstShipment.Add(data);
            }
            return lstShipment.AsQueryable();
        }

        private string GetCustomNoOldOfShipment(string jobNo)
        {
            var customNos = customsDeclarationRepo.Get(x => x.JobNo == jobNo).OrderBy(o => o.DatetimeModified).Select(s => s.ClearanceNo);
            return customNos.FirstOrDefault();
        }

        private IQueryable<GeneralExportShipmentOverviewResult> QueryDataShipmentOverview(GeneralReportCriteria criteria)
        {
            Expression<Func<CsTransaction, bool>> queryTrans = GetQueryTransationDocumentation(criteria);
            Expression<Func<CsTransactionDetail, bool>> queryTranDetail = GetQueryTransationDetailDocumentation(criteria);

            var masterBills = DataContext.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED).Where(queryTrans);//Lấy ra cả Job bị LOCK
            var dataPartner = catPartnerRepo.Get();
            if (queryTranDetail == null)
            {
                var houseBills = detailRepository.Get();
                var queryShipment = from master in masterBills
                                    join house in houseBills on master.Id equals house.JobId into housebill
                                    from house in housebill.DefaultIfEmpty()
                                    select new GeneralExportShipmentOverviewResult
                                    {
                                        ServiceName = master.TransactionType,
                                        JobNo = master.JobNo,
                                        etd = master.Etd,
                                        eta = master.Eta,
                                        FlightNo = master.FlightVesselName,
                                        MblMawb = master.Mawb,
                                        HblHawb = house.Hwbno,
                                        Pol = master.Pol,
                                        Pod = master.Pod,
                                        ShipmentType = master.ShipmentType,
                                        Salesman = house.SaleManId,
                                        Carrier = master.ColoaderId,
                                        Agent = master.AgentId,
                                        Shipper = house.ShipperId,
                                        Consignee = house.ConsigneeId,
                                        ShipperDescription = house.ShipperDescription,
                                        ConsigneeDescription = house.ConsigneeDescription,
                                        PackageType = house.PackageType,
                                        Cont20 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "20").Count : 0,
                                        Cont40 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "40´HC").Count > 0 ? Regex.Matches(house.PackageContainer, "40´HC").Count : Regex.Matches(house.PackageContainer, "40").Count : 0,
                                        Cont40HC = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "40´HC").Count : 0,
                                        Cont45 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "45").Count : 0,
                                        GW = house.GrossWeight,
                                        CW = house.ChargeWeight,
                                        CBM = house.Cbm.HasValue ? house.Cbm : master.Cbm,
                                        HblId = house.Id,
                                        CustomerId = house.CustomerId,
                                        OfficeId = master.OfficeId,
                                        Creator = master.UserCreated,
                                        POINV = master.Pono,
                                        Commodity = master.Commodity,
                                        PMTerm = master.PaymentTerm,
                                        ShipmentNotes = master.Notes,
                                        Created = master.DatetimeCreated,
                                        QTy = house.PackageQty.ToString(), //+ " " + unit.Code,
                                        //CustomerName = partner.ShortName
                                        PackageQty = house.PackageType


                                    };
                return queryShipment;
            }
            else
            {
                var houseBills = GetTransactionDetailDocWithSalesman(queryTranDetail, criteria);
                var queryShipment = from master in masterBills
                                    join house in houseBills on master.Id equals house.JobId
                                    //join unit in catUnitRepo.Get() on house.PackageType equals unit.Id into units
                                    //from unit in units.DefaultIfEmpty()
                                    //join partner in dataPartner on house.CustomerId equals partner.Id into Partner
                                    //from partner in Partner.DefaultIfEmpty()
                                    select new GeneralExportShipmentOverviewResult
                                    {
                                        ServiceName = master.TransactionType,
                                        JobNo = master.JobNo,
                                        etd = master.Etd,
                                        eta = master.Eta,
                                        FlightNo = master.FlightVesselName,
                                        MblMawb = master.Mawb,
                                        HblHawb = house.Hwbno,
                                        Pol = master.Pol,
                                        Pod = master.Pod,
                                        ShipmentType = master.ShipmentType,
                                        Salesman = house.SaleManId,
                                        Carrier = master.ColoaderId,
                                        Agent = master.AgentId,
                                        Shipper = house.ShipperId,
                                        Consignee = house.ConsigneeId,
                                        ShipperDescription = house.ShipperDescription,
                                        ConsigneeDescription = house.ConsigneeDescription,
                                        PackageType = house.PackageType,
                                        Cont20 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "20").Count : 0,
                                        Cont40 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "40´HC").Count > 0 ? Regex.Matches(house.PackageContainer, "40´HC").Count : Regex.Matches(house.PackageContainer, "40").Count : 0,
                                        Cont40HC = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "40´HC").Count : 0,
                                        Cont45 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "45").Count : 0,
                                        GW = house.GrossWeight,
                                        CW = house.ChargeWeight,
                                        CBM = house.Cbm.HasValue ? house.Cbm : master.Cbm,
                                        HblId = house.Id,
                                        CustomerId = house.CustomerId,
                                        OfficeId = master.OfficeId,
                                        Creator = master.UserCreated,
                                        POINV = master.Pono,
                                        Commodity = master.Commodity,
                                        PMTerm = master.PaymentTerm,
                                        ShipmentNotes = master.Notes,
                                        Created = master.DatetimeCreated,
                                        QTy = house.PackageQty.ToString(), //+ " " + unit.Code,        //CustomerName = partner.ShortName
                                        PackageQty = house.PackageType
                                    };

                return queryShipment;
            }
        }

        #endregion

        #region -- GENERAL REPORT --

        public List<GeneralReportResult> GetDataGeneralReport(GeneralReportCriteria criteria, int page, int size, out int rowsCount)
        {
            var dataDocumentation = GeneralReportDocumentation(criteria);
            IQueryable<GeneralReportResult> list;
            if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains("CL"))
            {
                var dataOperation = GeneralReportOperation(criteria);
                list = dataDocumentation.Union(dataOperation);
            }
            else
            {
                list = dataDocumentation;
            }

            var results = new List<GeneralReportResult>();
            if (list == null)
            {
                rowsCount = 0;
                return results;
            }
            var tempList = list.ToList();
            int no = 1;
            tempList.ForEach(fe =>
            {
                fe.No = no;
                no++;
            });
            rowsCount = tempList.Select(s => s.No).Count();
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = tempList.Skip((page - 1) * size).Take(size).ToList();
            }
            return results;
        }

        public List<GeneralReportResult> QueryDataGeneralReport(GeneralReportCriteria criteria)
        {
            var dataDocumentation = GeneralReportDocumentation(criteria);
            IQueryable<GeneralReportResult> list;
            if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains("CL"))
            {
                var dataOperation = GeneralReportOperation(criteria);
                list = dataDocumentation.Union(dataOperation);
            }
            else
            {
                list = dataDocumentation;
            }
            var tempList = list.ToList();
            int no = 1;
            tempList.ForEach(fe =>
            {
                fe.No = no;
                no++;
            });
            return tempList;
        }

        private IQueryable<OpsTransaction> QueryDataOperation(GeneralReportCriteria criteria)
        {
            Expression<Func<OpsTransaction, bool>> query = GetQueryOPSTransactionOperation(criteria, null);

            var queryShipment = GetOpsTransactionWithSalesman(query, criteria);
            return queryShipment;
        }

        private IQueryable<GeneralReportResult> GeneralReportOperation(GeneralReportCriteria criteria)
        {
            List<GeneralReportResult> dataList = new List<GeneralReportResult>();
            var dataShipment = QueryDataOperation(criteria);
            var LstSurcharge = surCharge.Get();
            var LookupSurchage = LstSurcharge.ToLookup(x => x.Hblid);
            var PartnerList = catPartnerRepo.Get();
            var LookupPartner = PartnerList.ToLookup(x => x.Id);
            var PlaceList = catPlaceRepo.Get();
            foreach (var item in dataShipment)
            {
                GeneralReportResult data = new GeneralReportResult();
                data.JobId = item.JobNo;
                data.Mawb = item.Mblno;
                data.Hawb = item.Hwbno;
                foreach (var partner in LookupPartner[item.CustomerId])
                {
                    data.CustomerName = partner?.PartnerNameEn;
                    break;
                }
                foreach (var partner in LookupPartner[item.SupplierId])
                {
                    data.CarrierName = partner?.PartnerNameEn;
                    break;
                }
                foreach (var partner in LookupPartner[item.AgentId])
                {
                    data.AgentName = partner?.PartnerNameEn;
                    break;
                }
                data.ServiceDate = item.ServiceDate;

                var _polCode = catPlaceRepo.Get(x => x.Id == item.Pol).FirstOrDefault()?.Code;
                var _podCode = catPlaceRepo.Get(x => x.Id == item.Pod).FirstOrDefault()?.Code;
                data.Route = _polCode + "/" + _podCode;

                data.Qty = item.SumPackages ?? 0;
                data.ChargeWeight = item.SumChargeWeight ?? 0;

                #region -- Phí Selling trước thuế --
                decimal? _revenue = 0;
                if (item.Hblid != null && item.Hblid != Guid.Empty)
                {
                    var _chargeSell = LookupSurchage[(Guid)item.Hblid].Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE);
                    foreach (var charge in _chargeSell)
                    {
                        if (criteria.Currency == DocumentConstants.CURRENCY_LOCAL)
                        {
                            _revenue += charge.AmountVnd;
                        }
                        else if (criteria.Currency == DocumentConstants.CURRENCY_USD)
                        {
                            _revenue += charge.AmountUsd;
                        }
                    }
                    data.Revenue = _revenue;
                }
                #endregion -- Phí Selling trước thuế --

                #region -- Phí Buying trước thuế --
                decimal? _cost = 0;
                //var _chargeBuy = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && x.Hblid == item.HblId);
                if (item.Hblid != null && item.Hblid != Guid.Empty)
                {
                    var _chargeBuy = LookupSurchage[(Guid)item.Hblid].Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE);
                    foreach (var charge in _chargeBuy)
                    {
                        if (criteria.Currency == DocumentConstants.CURRENCY_LOCAL)
                        {
                            _cost += charge.AmountVnd;
                        }
                        else if (criteria.Currency == DocumentConstants.CURRENCY_USD)
                        {
                            _cost += charge.AmountUsd;
                        }
                    }
                    data.Cost = _cost;
                }

                #endregion -- Phí Buying trước thuế --

                data.Profit = data.Revenue - data.Cost;

                #region -- Phí OBH sau thuế --
                decimal? _obh = 0;
                //var _chargeObh = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_OBH_TYPE && x.Hblid == item.HblId);
                if (item.Hblid != null && item.Hblid != Guid.Empty)
                {
                    var _chargeObh = LookupSurchage[(Guid)item.Hblid].Where(x => x.Type == DocumentConstants.CHARGE_OBH_TYPE);
                    foreach (var charge in _chargeObh)
                    {
                        // Phí OBH sau thuế
                        if (criteria.Currency == DocumentConstants.CURRENCY_LOCAL)
                        {
                            _obh += charge.VatAmountVnd + charge.AmountVnd;
                        }
                        else if (criteria.Currency == DocumentConstants.CURRENCY_USD)
                        {
                            _obh += charge.VatAmountUsd + charge.VatAmountUsd;
                        }
                    }
                    data.Obh = _obh;
                }
                #endregion -- Phí OBH sau thuế --

                var _empPic = sysUserRepo.Get(j => j.Id == item.BillingOpsId).FirstOrDefault()?.EmployeeId;
                if (!string.IsNullOrEmpty(_empPic))
                {
                    data.PersonInCharge = sysEmployeeRepo.Get(x => x.Id == _empPic).FirstOrDefault()?.EmployeeNameEn;
                }

                var _empSale = sysUserRepo.Get(j => j.Id == item.SalemanId).FirstOrDefault()?.EmployeeId;
                if (!string.IsNullOrEmpty(_empSale))
                {

                    data.Salesman = sysEmployeeRepo.Get(x => x.Id == _empSale).FirstOrDefault()?.EmployeeNameEn;
                }

                data.ServiceName = API.Common.Globals.CustomData.Services.Where(x => x.Value == "CL").FirstOrDefault()?.DisplayName;

                dataList.Add(data);
            }
            return dataList.AsQueryable();
        }

        private IQueryable<GeneralReportResult> QueryDataDocumentation(GeneralReportCriteria criteria)
        {
            Expression<Func<CsTransaction, bool>> queryTrans = GetQueryTransationDocumentation(criteria);
            Expression<Func<CsTransactionDetail, bool>> queryTranDetail = GetQueryTransationDetailDocumentation(criteria);

            var masterBills = DataContext.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED).Where(queryTrans);//Lấy hết các lô hàng bao gồm các lô bị Lock
            if (queryTranDetail == null)
            {
                var houseBills = detailRepository.Get();
                var queryShipment = from master in masterBills
                                    join house in houseBills on master.Id equals house.JobId into housebill
                                    from house in housebill.DefaultIfEmpty()
                                    select new GeneralReportResult
                                    {
                                        JobId = master.JobNo,
                                        Mawb = master.Mawb,
                                        HblId = house.Id,
                                        Hawb = house.Hwbno,
                                        CustomerId = house.CustomerId,
                                        CarrierId = master.ColoaderId,
                                        AgentId = master.AgentId,
                                        Qty = master.PackageQty ?? 0,
                                        Pol = master.Pol,
                                        Pod = master.Pod,
                                        ServiceDate = master.ServiceDate,
                                        PicId = master.PersonIncharge,
                                        SalesmanId = house.SaleManId,
                                        Service = master.TransactionType
                                    };
                return queryShipment;
            }
            else
            {
                var houseBills = GetTransactionDetailDocWithSalesman(queryTranDetail, criteria);
                var queryShipment = from master in masterBills
                                    join house in houseBills on master.Id equals house.JobId
                                    select new GeneralReportResult
                                    {
                                        JobId = master.JobNo,
                                        Mawb = master.Mawb,
                                        HblId = house.Id,
                                        Hawb = house.Hwbno,
                                        CustomerId = house.CustomerId,
                                        CarrierId = master.ColoaderId,
                                        AgentId = master.AgentId,
                                        Qty = master.PackageQty ?? 0,
                                        Pol = master.Pol,
                                        Pod = master.Pod,
                                        ServiceDate = master.ServiceDate,
                                        PicId = master.PersonIncharge,
                                        SalesmanId = house.SaleManId,
                                        Service = master.TransactionType
                                    };
                return queryShipment;
            }

        }

        private IQueryable<GeneralReportResult> GeneralReportDocumentation(GeneralReportCriteria criteria)
        {
            var dataShipment = QueryDataDocumentation(criteria);
            List<GeneralReportResult> dataList = new List<GeneralReportResult>();
            var LstSurcharge = surCharge.Get();
            var LookupSurchage = LstSurcharge.ToLookup(x => x.Hblid);
            var PartnerList = catPartnerRepo.Get();
            var LookupPartner = PartnerList.ToLookup(x => x.Id);
            var PlaceList = catPlaceRepo.Get();
            foreach (var item in dataShipment)
            {
                GeneralReportResult data = new GeneralReportResult();
                data.JobId = item.JobId;
                data.Mawb = item.Mawb;
                data.Hawb = item.Hawb;
                foreach (var partner in LookupPartner[item.CustomerId])
                {
                    data.CustomerName = partner?.PartnerNameEn;
                    break;
                }
                foreach (var partner in LookupPartner[item.CarrierId])
                {
                    data.CarrierName = partner?.PartnerNameEn;
                    break;
                }
                foreach (var partner in LookupPartner[item.AgentId])
                {
                    data.AgentName = partner?.PartnerNameEn;
                    break;
                }

                data.ServiceDate = item.ServiceDate;

                var _polCode = PlaceList.Where(x => x.Id == item.Pol).FirstOrDefault()?.Code;
                var _podCode = PlaceList.Where(x => x.Id == item.Pod).FirstOrDefault()?.Code;
                data.Route = _polCode + "/" + _podCode;

                //Qty lấy theo Housebill
                var houseBill = detailRepository.Get(x => x.Id == item.HblId).FirstOrDefault();
                data.Qty = houseBill?.PackageQty ?? 0;
                data.ChargeWeight = houseBill?.ChargeWeight ?? 0;

                #region -- Phí Selling trước thuế --
                decimal? _revenue = 0;
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeSell = LookupSurchage[(Guid)item.HblId].Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE);
                    foreach (var charge in _chargeSell)
                    {
                        if (criteria.Currency == DocumentConstants.CURRENCY_LOCAL)
                        {
                            _revenue += charge.AmountVnd;
                        }
                        else if (criteria.Currency == DocumentConstants.CURRENCY_USD)
                        {
                            _revenue += charge.AmountUsd;
                        }
                    }
                    data.Revenue = _revenue;
                }

                #endregion -- Phí Selling trước thuế --

                #region -- Phí Buying trước thuế --
                decimal? _cost = 0;
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeBuy = LookupSurchage[(Guid)item.HblId].Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE);
                    foreach (var charge in _chargeBuy)
                    {
                        if (criteria.Currency == DocumentConstants.CURRENCY_LOCAL)
                        {
                            _cost += charge.AmountVnd;
                        }
                        else if (criteria.Currency == DocumentConstants.CURRENCY_USD)
                        {
                            _cost += charge.AmountUsd;
                        }
                    }
                    data.Cost = _cost;
                }

                #endregion -- Phí Buying trước thuế --

                data.Profit = data.Revenue - data.Cost;

                #region -- Phí OBH sau thuế --
                decimal? _obh = 0;
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeObh = LookupSurchage[(Guid)item.HblId].Where(x => x.Type == DocumentConstants.CHARGE_OBH_TYPE);
                    foreach (var charge in _chargeObh)
                    {
                        // Phí OBH sau thuế
                        if (criteria.Currency == DocumentConstants.CURRENCY_LOCAL)
                        {
                            _obh += charge.VatAmountVnd + charge.AmountVnd;
                        }
                        else if (criteria.Currency == DocumentConstants.CURRENCY_USD)
                        {
                            _obh += charge.VatAmountUsd + charge.VatAmountUsd;
                        }
                    }
                    data.Obh = _obh;
                }

                #endregion -- Phí OBH sau thuế --

                var _empPic = sysUserRepo.Get(j => j.Id == item.PicId).FirstOrDefault()?.EmployeeId;
                if (!string.IsNullOrEmpty(_empPic))
                {
                    data.PersonInCharge = sysEmployeeRepo.Get(x => x.Id == _empPic).FirstOrDefault()?.EmployeeNameEn;
                }

                var _empSale = sysUserRepo.Get(j => j.Id == item.SalesmanId).FirstOrDefault()?.EmployeeId;
                if (!string.IsNullOrEmpty(_empSale))
                {

                    data.Salesman = sysEmployeeRepo.Get(x => x.Id == _empSale).FirstOrDefault()?.EmployeeNameEn;
                }

                data.ServiceName = API.Common.Globals.CustomData.Services.Where(x => x.Value == item.Service).FirstOrDefault()?.DisplayName;

                dataList.Add(data);
            }
            return dataList.AsQueryable();
        }
        #endregion -- GENERAL REPORT --     

        #region -- Export Accounting PL Sheet --
        public List<AccountingPlSheetExportResult> GetDataAccountingPLSheet(GeneralReportCriteria criteria)
        {
            var dataDocumentation = AcctPLSheetDocumentation(criteria);
            IQueryable<AccountingPlSheetExportResult> list;
            if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains("CL"))
            {
                var dataOperation = AcctPLSheetOperation(criteria);
                list = dataDocumentation.Union(dataOperation);
            }
            else
            {
                list = dataDocumentation;
            }
            return list.ToList();
        }

        private IQueryable<OpsTransaction> QueryDataOperationAcctPLSheet(GeneralReportCriteria criteria)
        {
            // Filter data without customerId
            //var criteriaNoCustomer = (GeneralReportCriteria)criteria.Clone();
            //criteriaNoCustomer.CustomerId = null;
            Expression<Func<OpsTransaction, bool>> query = GetQueryOPSTransactionOperation(criteria, null);

            var queryShipment = GetOpsTransactionWithSalesman(query, criteria);
            return queryShipment;
        }

        private IQueryable<OpsTransaction> QueryDataOperationCost(GeneralReportCriteria criteria, bool fromCost)
        {
            Expression<Func<OpsTransaction, bool>> query = GetQueryOPSTransactionOperation(criteria, true);

            var queryShipment = GetOpsTransactionWithSalesman(query, criteria);
            return queryShipment;
        }


        public List<JobProfitAnalysisExportResult> GetDataJobProfitAnalysis(GeneralReportCriteria criteria)
        {
            var dataDocumentation = JobProfitAnalysisDocumetation(criteria);
            IQueryable<JobProfitAnalysisExportResult> list;
            if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains("CL"))
            {
                var dataOperation = JobProfitAnalysisOperation(criteria);
                list = dataDocumentation.Union(dataOperation);
            }
            else
            {
                list = dataDocumentation;
            }
            return list.ToList();
        }

        private IQueryable<AccountingPlSheetExportResult> AcctPLSheetOperation(GeneralReportCriteria criteria)
        {
            List<AccountingPlSheetExportResult> dataList = new List<AccountingPlSheetExportResult>();
            var dataShipment = QueryDataOperationAcctPLSheet(criteria);
            var lstPartner = catPartnerRepo.Get();
            var lstCharge = catChargeRepo.Get();
            var lstSurchage = surCharge.Get();
            var detailLookupSur = lstSurchage.ToLookup(q => q.Hblid);
            var detailLookupPartner = lstPartner.ToLookup(q => q.Id);
            var detailLookupCharge = lstCharge.ToLookup(q => q.Id);
            foreach (var item in dataShipment)
            {
                var chargeD = detailLookupSur[(Guid)item.Hblid].Where(x => !string.IsNullOrEmpty(criteria.CustomerId) ? criteria.CustomerId == x.PaymentObjectId || criteria.CustomerId == x.PayerId : true);
                foreach (var charge in chargeD)
                {
                    AccountingPlSheetExportResult data = new AccountingPlSheetExportResult();
                    var _partnerId = !string.IsNullOrEmpty(criteria.CustomerId) ? criteria.CustomerId : charge.PaymentObjectId; //(charge.Type == DocumentConstants.CHARGE_OBH_TYPE) ? charge.PayerId : charge.PaymentObjectId;
                    data.ServiceDate = item.ServiceDate;
                    data.JobId = item.JobNo;
                    data.Hblid = charge.Hblid;
                    data.CustomNo = !string.IsNullOrEmpty(charge.ClearanceNo) ? charge.ClearanceNo : GetCustomNoOldOfShipment(item.JobNo); //Ưu tiên: ClearanceNo of charge >> ClearanceNo of Job có ngày ClearanceDate cũ nhất
                    var _exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    decimal UnitPrice = charge.UnitPrice ?? 0;
                    decimal? _amount = charge.Quantity * UnitPrice;
                    var _taxInvNoRevenue = string.Empty;
                    var _voucherRevenue = string.Empty;
                    decimal? _usdRevenue = 0;
                    decimal? _vndRevenue = 0;
                    decimal? _taxOut = 0;
                    decimal? _totalRevenue = 0;
                    if (charge.Type == DocumentConstants.CHARGE_SELL_TYPE)
                    {
                        _taxInvNoRevenue = !string.IsNullOrEmpty(charge.InvoiceNo) ? charge.InvoiceNo : charge.DebitNo;
                        _usdRevenue = (charge.CurrencyId == DocumentConstants.CURRENCY_USD) ? _amount : 0; //Amount trước thuế của phí Selling có currency là USD

                        if (charge.CurrencyId == DocumentConstants.CURRENCY_USD)
                        {
                            var _exchangeRateToVnd = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, DocumentConstants.CURRENCY_USD, DocumentConstants.CURRENCY_LOCAL);
                            _vndRevenue = _amount * _exchangeRateToVnd;
                        }
                        if (charge.CurrencyId == DocumentConstants.CURRENCY_LOCAL)
                        {
                            _vndRevenue = _amount;
                        }

                        if (charge.Vatrate > 0 && charge.Vatrate < 101)
                        {
                            _taxOut = (_amount * _exchangeRate * charge.Vatrate) / 100;
                        }
                        else
                        {
                            _taxOut = Math.Abs(charge.Vatrate ?? 0);
                        }
                        _voucherRevenue = charge.VoucherId;
                        _totalRevenue = (_amount * _exchangeRate) + _taxOut;
                    }
                    data.TaxInvNoRevenue = _taxInvNoRevenue;
                    data.VoucherIdRevenue = _voucherRevenue;
                    data.UsdRevenue = _usdRevenue;
                    data.VndRevenue = _vndRevenue;
                    data.TaxOut = _taxOut;
                    data.TotalRevenue = _totalRevenue;

                    var _taxInvNoCost = string.Empty;
                    var _voucherCost = string.Empty;
                    decimal? _usdCost = 0;
                    decimal? _vndCost = 0;
                    decimal? _taxIn = 0;
                    decimal? _totalCost = 0;
                    if (charge.Type == DocumentConstants.CHARGE_BUY_TYPE)
                    {
                        _taxInvNoCost = !string.IsNullOrEmpty(charge.InvoiceNo) ? charge.InvoiceNo : charge.CreditNo;
                        _usdCost = (charge.CurrencyId == DocumentConstants.CURRENCY_USD) ? _amount : 0; //Amount trước thuế của phí Buying có currency là USD

                        if (charge.CurrencyId == DocumentConstants.CURRENCY_USD)
                        {
                            var _exchangeRateToVnd = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, DocumentConstants.CURRENCY_USD, DocumentConstants.CURRENCY_LOCAL);
                            _vndCost = _amount * _exchangeRateToVnd;
                        }
                        if (charge.CurrencyId == DocumentConstants.CURRENCY_LOCAL)
                        {
                            _vndCost = _amount;
                        }

                        if (charge.Vatrate > 0 && charge.Vatrate < 101)
                        {
                            _taxIn = (_amount * _exchangeRate * charge.Vatrate) / 100;
                        }
                        else
                        {
                            _taxIn = Math.Abs(charge.Vatrate ?? 0);
                        }
                        _voucherCost = charge.VoucherId;
                        _totalCost = (_amount * _exchangeRate) + _taxIn;
                    }
                    data.TaxInvNoCost = _taxInvNoCost;
                    data.VoucherIdCost = _voucherCost;
                    data.UsdCost = _usdCost;
                    data.VndCost = _vndCost;
                    data.TaxIn = _taxIn;
                    data.TotalCost = _totalCost;

                    data.TotalKickBack = (charge.KickBack == true) ? _amount * _exchangeRate : 0;
                    data.ExchangeRate = _exchangeRate;
                    data.Balance = _totalRevenue - _totalCost - data.TotalKickBack;
                    data.InvNoObh = charge.Type == DocumentConstants.CHARGE_OBH_TYPE ? charge.InvoiceNo : string.Empty;
                    data.AmountObh = charge.Type == DocumentConstants.CHARGE_OBH_TYPE ? charge.Total * _exchangeRate : 0; //Amount sau thuế của phí OBH
                    data.AcVoucherNo = string.Empty;
                    data.PmVoucherNo = charge.Type == DocumentConstants.CHARGE_OBH_TYPE ? charge.VoucherId : string.Empty; //Voucher của phí OBH theo Payee
                    data.Service = API.Common.Globals.CustomData.Services.Where(x => x.Value == "CL").FirstOrDefault()?.DisplayName;
                    data.UserExport = currentUser.UserName;
                    data.CurrencyId = charge.CurrencyId;
                    data.ExchangeDate = charge.ExchangeDate;
                    data.FinalExchangeRate = charge.FinalExchangeRate;
                    data.Mbl = item.Mblno;
                    data.Hbl = item.Hwbno;
                    foreach (var partner in detailLookupPartner[_partnerId])
                    {
                        data.PartnerCode = partner?.AccountNo;
                        data.PartnerName = partner?.PartnerNameEn;
                        data.PartnerTaxCode = partner?.TaxCode;
                    }
                    foreach (var ch in detailLookupCharge[charge.ChargeId])
                    {
                        data.ChargeCode = ch?.Code;
                        data.ChargeName = ch?.ChargeNameEn;
                    }
                    dataList.Add(data);
                }
            }
            return dataList.AsQueryable();
        }

        private IQueryable<AccountingPlSheetExportResult> QueryDataDocumentationAcctPLSheet(GeneralReportCriteria criteria)
        {
            Expression<Func<CsTransaction, bool>> queryTrans = GetQueryTransationDocumentation(criteria);
            Expression<Func<CsTransactionDetail, bool>> queryTranDetail = GetQueryTransationDetailDocumentation(criteria);

            var masterBills = DataContext.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED).Where(queryTrans);
            if (queryTranDetail == null)
            {
                var houseBills = detailRepository.Get();
                var queryShipment = from master in masterBills
                                    join house in houseBills on master.Id equals house.JobId
                                    select new AccountingPlSheetExportResult
                                    {
                                        JobId = master.JobNo,
                                        Mbl = master.Mawb,
                                        Hblid = house.Id,
                                        Hbl = house.Hwbno,
                                        PaymentMethodTerm = master.PaymentTerm,
                                        ServiceDate = master.TransactionType.Contains("E") ? master.Etd : master.Eta,
                                        Service = master.TransactionType
                                    };
                return queryShipment;
            }
            else
            {
                var houseBills = GetTransactionDetailDocWithSalesman(queryTranDetail, criteria);
                var queryShipment = from master in masterBills
                                    join house in houseBills on master.Id equals house.JobId
                                    select new AccountingPlSheetExportResult
                                    {
                                        JobId = master.JobNo,
                                        Mbl = master.Mawb,
                                        Hblid = house.Id,
                                        Hbl = house.Hwbno,
                                        PaymentMethodTerm = master.PaymentTerm,
                                        ServiceDate = master.TransactionType.Contains("E") ? master.Etd : master.Eta,
                                        Service = master.TransactionType
                                    };
                return queryShipment;
            }
        }

        private IQueryable<JobProfitAnalysisExportResult> JobProfitAnalysisDocumetation(GeneralReportCriteria criteria)
        {
            // Filter data without customerId
            var criteriaNoCustomer = (GeneralReportCriteria)criteria.Clone();
            criteriaNoCustomer.CustomerId = null;
            var dataShipment = QueryDataDocumentationJobProfitAnalysis(criteriaNoCustomer);
            List<JobProfitAnalysisExportResult> dataList = new List<JobProfitAnalysisExportResult>();
            var dataCharge = catChargeRepo.Get();
            var surchargeData = surCharge.Get().ToList();
            var lookupSurcharge = surchargeData.ToLookup(q => q.Hblid);
            if (dataShipment != null)
            {
                foreach (var item in dataShipment)
                {
                    var chargeD = lookupSurcharge[item.Hblid].Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE || x.Type == DocumentConstants.CHARGE_SELL_TYPE);
                    if (!string.IsNullOrEmpty(criteria.CustomerId))
                    {
                        chargeD = chargeD.Where(x => (criteria.CustomerId == x.PaymentObjectId || criteria.CustomerId == x.PayerId) && (x.Type == DocumentConstants.CHARGE_BUY_TYPE || x.Type == DocumentConstants.CHARGE_SELL_TYPE));
                    }
                    foreach (var charge in chargeD)
                    {
                        JobProfitAnalysisExportResult data = new JobProfitAnalysisExportResult();
                        data.JobNo = item.JobNo;
                        data.Service = API.Common.Globals.CustomData.Services.Where(x => x.Value == item.Service).FirstOrDefault()?.DisplayName;
                        data.Mbl = item.Mbl;
                        data.Hbl = item.Hbl;
                        data.Eta = item.Eta;
                        data.Etd = item.Etd;
                        data.Quantity = item.Quantity;
                        data.Cont20 = item.Cont20;
                        data.Cont40 = item.Cont40;
                        data.Cont40HC = item.Cont40HC;
                        data.Cont45 = item.Cont45;
                        data.Cont = item.Cont;
                        data.CW = item.CW;
                        data.GW = item.GW;
                        data.CBM = item.CBM;
                        data.ChargeType = charge.Type;
                        var _charge = dataCharge.FirstOrDefault(x => x.Id == charge.ChargeId);
                        data.ChargeCode = _charge?.ChargeNameEn;
                        var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                        if (charge.Type == DocumentConstants.CHARGE_SELL_TYPE)
                        {
                            data.TotalRevenue = charge.Quantity * charge.UnitPrice * _rate ?? 0;
                        }
                        if (_charge.DebitCharge != null && charge.Type == DocumentConstants.CHARGE_BUY_TYPE)
                        {

                            data.TotalCost = charge.Quantity * charge.UnitPrice * _rate ?? 0;
                            var dataSelling = dataCharge.FirstOrDefault(x => x.Id == _charge.DebitCharge);
                            var dataChargeSell = chargeD.FirstOrDefault(x => x.ChargeId == dataSelling.Id && x.Type == DocumentConstants.CHARGE_SELL_TYPE);
                            if (dataChargeSell != null)
                            {
                                var _rateRevenue = currencyExchangeService.CurrencyExchangeRateConvert(dataChargeSell.FinalExchangeRate, dataChargeSell.ExchangeDate, dataChargeSell.CurrencyId, criteria.Currency);
                                data.TotalRevenue = dataChargeSell.Quantity * dataChargeSell.UnitPrice * _rateRevenue ?? 0;
                                data.isGroup = true;
                            }

                        }

                        data.TotalCost = data.TotalCost ?? 0;
                        data.TotalRevenue = data.TotalRevenue ?? 0;
                        data.JobProfit = data.TotalRevenue - data.TotalCost;
                        if (data.TotalRevenue == 0)
                        {
                            data.JobProfit = 0;
                        }

                        if (_charge.DebitCharge == null && charge.Type == DocumentConstants.CHARGE_BUY_TYPE)
                        {
                            data.TotalCost = charge.Quantity * charge.UnitPrice * _rate ?? 0;
                        }
                        dataList.Add(data);
                    }
                }
            }

            var groupedList = dataList.Where(x => x.isGroup == true).ToList();
            if (groupedList != null)
            {
                foreach (var item in groupedList)
                {
                    var ItemToRemove = dataList.FirstOrDefault(x => x.ChargeCode == item.ChargeCode && x.isGroup == null && x.ChargeType == DocumentConstants.CHARGE_SELL_TYPE && x.Hbl == item.Hbl);
                    if (ItemToRemove != null)
                    {
                        dataList.Remove(ItemToRemove);
                    }
                }
            }
            var GroupHbl = dataList.GroupBy(a => a.Hbl).Select(p => new { Hbl = p.Key, TotalCost = p.Sum(q => q.TotalCost), TotalRevenue = p.Sum(q => q.TotalRevenue), TotalJobProfit = p.Sum(q => q.JobProfit) }).ToList();
            foreach (var item in GroupHbl)
            {
                if (!string.IsNullOrEmpty(item.Hbl))
                {
                    int i = dataList.FindLastIndex(x => x.Hbl != null && x.Hbl.StartsWith(item.Hbl));
                    JobProfitAnalysisExportResult data = new JobProfitAnalysisExportResult();
                    data.TotalCost = item.TotalCost;
                    data.TotalRevenue = item.TotalRevenue;
                    data.JobProfit = item.TotalJobProfit;
                    dataList.Insert(i + 1, data);
                }
            }
            return dataList.AsQueryable();

        }

        private IQueryable<JobProfitAnalysisExportResult> JobProfitAnalysisOperation(GeneralReportCriteria criteria)
        {
            var dataShipment = QueryDataOperationAcctPLSheet(criteria);
            List<JobProfitAnalysisExportResult> dataList = new List<JobProfitAnalysisExportResult>();
            var dataCharge = catChargeRepo.Get();
            var surchargeData = surCharge.Get().ToList();
            var lookupSurcharge = surchargeData.ToLookup(q => q.Hblid);
            foreach (var item in dataShipment)
            {
                if (item.Hblid != Guid.Empty)
                {
                    var chargeD = lookupSurcharge[item.Hblid].Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE || x.Type == DocumentConstants.CHARGE_SELL_TYPE);
                    if (!string.IsNullOrEmpty(criteria.CustomerId))
                    {
                        chargeD = chargeD.Where(x => (criteria.CustomerId == x.PaymentObjectId || criteria.CustomerId == x.PayerId) && (x.Type == DocumentConstants.CHARGE_BUY_TYPE || x.Type == DocumentConstants.CHARGE_SELL_TYPE));
                    }
                    foreach (var charge in chargeD)
                    {
                        JobProfitAnalysisExportResult data = new JobProfitAnalysisExportResult();
                        data.JobNo = item.JobNo;
                        data.Service = API.Common.Globals.CustomData.Services.Where(x => x.Value == "CL").FirstOrDefault()?.DisplayName;
                        data.Mbl = item.Mblno;
                        data.Hbl = item.Hwbno;
                        data.Eta = item.ServiceDate;
                        data.Etd = item.ServiceDate;
                        data.Quantity = item.SumContainers;
                        var DetailContainer = !string.IsNullOrEmpty(item.ContainerDescription) ? item.ContainerDescription.Split(";").ToArray() : null;
                        int? Cont20 = 0;
                        int? Cont40 = 0;
                        int? Cont40HC = 0;
                        int? Cont45 = 0;
                        int? Cont = 0;
                        if (DetailContainer != null)
                        {
                            foreach (var it in DetailContainer)
                            {
                                if (Regex.Matches(it.Trim(), "20").Count > 0)
                                {
                                    Cont20 = Convert.ToInt16(it.Trim().Substring(0, 1));
                                }

                                if (Regex.Matches(it.Trim(), "40").Count > 0)
                                {
                                    Cont40 = Convert.ToInt16(it.Trim().Substring(0, 1));
                                }

                                if (Regex.Matches(it.Trim(), "40´HC").Count > 0)
                                {
                                    Cont40HC = Convert.ToInt16(it.Trim().Substring(0, 1));
                                }

                                if (Regex.Matches(it.Trim(), "45").Count > 0)
                                {
                                    Cont45 = Convert.ToInt16(it.Trim().Substring(0, 1));
                                }

                                if (Regex.Matches(it.Trim(), "Cont").Count > 0)
                                {
                                    Cont = Convert.ToInt16(it.Trim().Substring(0, 1));
                                }

                            }
                        }
                        data.Cont20 = !string.IsNullOrEmpty(item.ContainerDescription) ? Cont20 : 0;
                        data.Cont40 = !string.IsNullOrEmpty(item.ContainerDescription) ? Cont40 : 0;
                        data.Cont40HC = !string.IsNullOrEmpty(item.ContainerDescription) ? Cont40HC : 0;
                        data.Cont45 = !string.IsNullOrEmpty(item.ContainerDescription) ? Cont45 : 0;
                        data.Cont = !string.IsNullOrEmpty(item.ContainerDescription) ? Cont : 0;
                        data.CW = item.SumChargeWeight;
                        data.GW = item.SumGrossWeight;
                        data.CBM = item.SumCbm;
                        data.ChargeType = charge.Type;
                        var _charge = dataCharge.Where(x => x.Id == charge.ChargeId).FirstOrDefault();
                        data.ChargeCode = _charge?.ChargeNameEn;
                        var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                        if (charge.Type == DocumentConstants.CHARGE_SELL_TYPE)
                        {
                            data.TotalRevenue = charge.Quantity * charge.UnitPrice * _rate ?? 0;
                        }

                        if (_charge.DebitCharge != null && charge.Type == DocumentConstants.CHARGE_BUY_TYPE)
                        {

                            data.TotalCost = charge.Quantity * charge.UnitPrice * _rate ?? 0;
                            var dataSelling = dataCharge.FirstOrDefault(x => x.Id == _charge.DebitCharge);
                            var dataChargeSell = chargeD.FirstOrDefault(x => x.ChargeId == dataSelling.Id && x.Type == DocumentConstants.CHARGE_SELL_TYPE);
                            if (dataChargeSell != null)
                            {
                                var _rateRevenue = currencyExchangeService.CurrencyExchangeRateConvert(dataChargeSell.FinalExchangeRate, dataChargeSell.ExchangeDate, dataChargeSell.CurrencyId, criteria.Currency);
                                data.TotalRevenue = dataChargeSell.Quantity * dataChargeSell.UnitPrice * _rateRevenue ?? 0;
                                data.isGroup = true;
                            }

                        }

                        data.TotalCost = data.TotalCost ?? 0;
                        data.TotalRevenue = data.TotalRevenue ?? 0;
                        data.JobProfit = data.TotalRevenue - data.TotalCost;
                        if (data.TotalRevenue == 0)
                        {
                            data.JobProfit = 0;
                        }

                        if (_charge.DebitCharge == null && charge.Type == DocumentConstants.CHARGE_BUY_TYPE)
                        {
                            data.TotalCost = charge.Quantity * charge.UnitPrice * _rate ?? 0;
                        }
                        dataList.Add(data);
                    }
                }
            }
            var groupedList = dataList.Where(x => x.isGroup == true).ToList();
            if (groupedList != null)
            {
                foreach (var item in groupedList)
                {
                    var ItemToRemove = dataList.FirstOrDefault(x => x.ChargeCode == item.ChargeCode && x.isGroup == null && x.ChargeType == DocumentConstants.CHARGE_SELL_TYPE && x.Hbl == item.Hbl);
                    if (ItemToRemove != null)
                    {
                        dataList.Remove(ItemToRemove);
                    }
                }
            }
            var GroupHbl = dataList.GroupBy(a => a.Hbl).Select(p => new { Hbl = p.Key, TotalCost = p.Sum(q => q.TotalCost), TotalRevenue = p.Sum(q => q.TotalRevenue), TotalJobProfit = p.Sum(q => q.JobProfit) }).ToList();
            foreach (var item in GroupHbl)
            {
                int i = dataList.FindLastIndex(x => x.Hbl != null && x.Hbl.StartsWith(item.Hbl));
                JobProfitAnalysisExportResult data = new JobProfitAnalysisExportResult();
                data.TotalCost = item.TotalCost;
                data.TotalRevenue = item.TotalRevenue;
                data.JobProfit = item.TotalJobProfit;
                dataList.Insert(i + 1, data);
            }
            return dataList.AsQueryable();
        }


        private IQueryable<JobProfitAnalysisExportResult> QueryDataDocumentationJobProfitAnalysis(GeneralReportCriteria criteria)
        {
            Expression<Func<CsTransaction, bool>> queryTrans = GetQueryTransationDocumentation(criteria);
            Expression<Func<CsTransactionDetail, bool>> queryTranDetail = GetQueryTransationDetailDocumentation(criteria);

            var masterBills = DataContext.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED).Where(queryTrans);
            if (queryTranDetail == null)
            {
                var houseBills = detailRepository.Get();
                var queryShipment = from master in masterBills
                                    join house in houseBills on master.Id equals house.JobId
                                    select new JobProfitAnalysisExportResult
                                    {
                                        JobNo = master.JobNo,
                                        Mbl = master.Mawb,
                                        Hbl = house.Hwbno,
                                        Hblid = house.Id,
                                        Service = master.TransactionType,
                                        Eta = master.Eta,
                                        Etd = master.Etd,
                                        GW = house.GrossWeight,
                                        CW = house.ChargeWeight,
                                        CBM = house.Cbm,
                                        Quantity = house.PackageQty,
                                        Cont20 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "20").Count : 0,
                                        Cont40 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "40").Count : 0,
                                        Cont40HC = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "40´HC").Count : 0,
                                        Cont45 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "45").Count : 0,
                                        Cont = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "Cont").Count : 0
                                    };
                return queryShipment;
            }
            else
            {
                var houseBills = GetTransactionDetailDocWithSalesman(queryTranDetail, criteria);
                var queryShipment = from master in masterBills
                                    join house in houseBills on master.Id equals house.JobId
                                    select new JobProfitAnalysisExportResult
                                    {
                                        JobNo = master.JobNo,
                                        Mbl = master.Mawb,
                                        Hbl = house.Hwbno,
                                        Hblid = house.Id,
                                        Service = master.TransactionType,
                                        Eta = master.Eta,
                                        Etd = master.Etd,
                                        GW = house.GrossWeight,
                                        CW = house.ChargeWeight,
                                        CBM = house.Cbm,
                                        Quantity = house.PackageQty,
                                        Cont20 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "20").Count : 0,
                                        Cont40 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "40").Count : 0,
                                        Cont40HC = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "40´HC").Count : 0,
                                        Cont45 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "45").Count : 0,
                                        Cont = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "Cont").Count : 0
                                    };
                return queryShipment;
            }
        }
        private IQueryable<AccountingPlSheetExportResult> AcctPLSheetDocumentation(GeneralReportCriteria criteria)
        {
            // Filter data without customerId
            var criteriaNoCustomer = (GeneralReportCriteria)criteria.Clone();
            criteriaNoCustomer.CustomerId = null;
            var dataShipment = QueryDataDocumentationAcctPLSheet(criteriaNoCustomer);
            var lstPartner = catPartnerRepo.Get();
            var lstCharge = catChargeRepo.Get();
            var lstSurchage = surCharge.Get();
            var detailLookupSur = lstSurchage.ToLookup(q => q.Hblid);
            var detailLookupPartner = lstPartner.ToLookup(q => q.Id);
            var detailLookupCharge = lstCharge.ToLookup(q => q.Id);
            List<AccountingPlSheetExportResult> dataList = new List<AccountingPlSheetExportResult>();
            foreach (var item in dataShipment)
            {
                if (item.Hblid != null && item.Hblid != Guid.Empty)
                {
                    var chargeD = detailLookupSur[(Guid)item.Hblid].Where(x => !string.IsNullOrEmpty(criteria.CustomerId) ? criteria.CustomerId == x.PaymentObjectId || criteria.CustomerId == x.PayerId : true);
                    foreach (var charge in chargeD)
                    {
                        AccountingPlSheetExportResult data = new AccountingPlSheetExportResult();
                        var _partnerId = !string.IsNullOrEmpty(criteria.CustomerId) ? criteria.CustomerId : charge.PaymentObjectId; //(charge.Type == DocumentConstants.CHARGE_OBH_TYPE) ? charge.PayerId : charge.PaymentObjectId;
                        data.ServiceDate = item.ServiceDate;
                        data.JobId = item.JobId;
                        data.Hblid = charge.Hblid;
                        var _exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                        decimal UnitPrice = charge.UnitPrice ?? 0;
                        decimal? _amount = charge.Quantity * UnitPrice;
                        var _taxInvNoRevenue = string.Empty;
                        var _voucherRevenue = string.Empty;
                        decimal? _usdRevenue = 0;
                        decimal? _vndRevenue = 0;
                        decimal? _taxOut = 0;
                        decimal? _totalRevenue = 0;
                        if (charge.Type == DocumentConstants.CHARGE_SELL_TYPE)
                        {
                            _taxInvNoRevenue = !string.IsNullOrEmpty(charge.InvoiceNo) ? charge.InvoiceNo : charge.DebitNo;
                            _usdRevenue = (charge.CurrencyId == DocumentConstants.CURRENCY_USD) ? _amount : 0; //Amount trước thuế của phí Selling có currency là USD

                            if (charge.CurrencyId == DocumentConstants.CURRENCY_USD)
                            {
                                var _exchangeRateToVnd = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, DocumentConstants.CURRENCY_USD, DocumentConstants.CURRENCY_LOCAL);
                                _vndRevenue = _amount * _exchangeRateToVnd;
                            }
                            if (charge.CurrencyId == DocumentConstants.CURRENCY_LOCAL)
                            {
                                _vndRevenue = _amount;
                            }

                            if (charge.Vatrate > 0 && charge.Vatrate < 101)
                            {
                                _taxOut = (_amount * _exchangeRate * charge.Vatrate) / 100;
                            }
                            else
                            {
                                _taxOut = Math.Abs(charge.Vatrate ?? 0);
                            }
                            _voucherRevenue = charge.VoucherId;
                            _totalRevenue = (_amount * _exchangeRate) + _taxOut;
                        }
                        data.TaxInvNoRevenue = _taxInvNoRevenue;
                        data.VoucherIdRevenue = _voucherRevenue;
                        data.UsdRevenue = _usdRevenue;
                        data.VndRevenue = _vndRevenue;
                        data.TaxOut = _taxOut;
                        data.TotalRevenue = _totalRevenue;

                        var _taxInvNoCost = string.Empty;
                        var _voucherCost = string.Empty;
                        decimal? _usdCost = 0;
                        decimal? _vndCost = 0;
                        decimal? _taxIn = 0;
                        decimal? _totalCost = 0;
                        if (charge.Type == DocumentConstants.CHARGE_BUY_TYPE)
                        {
                            _taxInvNoCost = !string.IsNullOrEmpty(charge.InvoiceNo) ? charge.InvoiceNo : charge.CreditNo;
                            _usdCost = (charge.CurrencyId == DocumentConstants.CURRENCY_USD) ? _amount : 0; //Amount trước thuế của phí Buying có currency là USD

                            if (charge.CurrencyId == DocumentConstants.CURRENCY_USD)
                            {
                                var _exchangeRateToVnd = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, DocumentConstants.CURRENCY_USD, DocumentConstants.CURRENCY_LOCAL);
                                _vndCost = _amount * _exchangeRateToVnd;
                            }
                            if (charge.CurrencyId == DocumentConstants.CURRENCY_LOCAL)
                            {
                                _vndCost = _amount;
                            }

                            if (charge.Vatrate > 0 && charge.Vatrate < 101)
                            {
                                _taxIn = (_amount * _exchangeRate * charge.Vatrate) / 100;
                            }
                            else
                            {
                                _taxIn = Math.Abs(charge.Vatrate ?? 0);
                            }
                            _voucherCost = charge.VoucherId;
                            _totalCost = (_amount * _exchangeRate) + _taxIn;
                        }
                        data.TaxInvNoCost = _taxInvNoCost;
                        data.VoucherIdCost = _voucherCost;
                        data.UsdCost = _usdCost;
                        data.VndCost = _vndCost;
                        data.TaxIn = _taxIn;
                        data.TotalCost = _totalCost;

                        data.TotalKickBack = (charge.KickBack == true) ? _amount * _exchangeRate : 0;
                        data.ExchangeRate = _exchangeRate;
                        data.Balance = _totalRevenue - _totalCost - data.TotalKickBack;
                        data.InvNoObh = charge.Type == DocumentConstants.CHARGE_OBH_TYPE ? charge.InvoiceNo : string.Empty;
                        data.AmountObh = charge.Type == DocumentConstants.CHARGE_OBH_TYPE ? charge.Total * _exchangeRate : 0; //Amount sau thuế của phí OBH
                        data.AcVoucherNo = string.Empty;
                        data.PmVoucherNo = charge.Type == DocumentConstants.CHARGE_OBH_TYPE ? charge.VoucherId : string.Empty; //Voucher của phí OBH theo Payee
                        data.Service = API.Common.Globals.CustomData.Services.Where(x => x.Value == item.Service).FirstOrDefault()?.DisplayName;
                        data.UserExport = currentUser.UserName;
                        data.CurrencyId = charge.CurrencyId;
                        data.ExchangeDate = charge.ExchangeDate;
                        data.FinalExchangeRate = charge.FinalExchangeRate;
                        data.Mbl = item.Mbl;
                        data.Hbl = item.Hbl;
                        foreach (var partner in detailLookupPartner[_partnerId])
                        {
                            data.PartnerCode = partner?.AccountNo;
                            data.PartnerName = partner?.PartnerNameEn;
                            data.PartnerTaxCode = partner?.TaxCode;
                        }
                        foreach (var ch in detailLookupCharge[charge.ChargeId])
                        {
                            data.ChargeCode = ch?.Code;
                            data.ChargeName = ch?.ChargeNameEn;
                        }
                        dataList.Add(data);
                    }
                }

            }
            return dataList.AsQueryable();
        }
        #endregion -- Export Accounting PL Sheet --


        #region -- Export Summary Of Costs Incurred
        private IQueryable<SummaryOfCostsIncurredExportResult> SummaryOfCostsIncurredOperation(GeneralReportCriteria criteria)
        {
            var dataShipment = QueryDataOperationCost(criteria, true);
            if (dataShipment == null) return null;
            var port = catPlaceRepo.Get();
            List<SummaryOfCostsIncurredExportResult> dataList = new List<SummaryOfCostsIncurredExportResult>();
            Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query = chg => chg.CustomerID == criteria.CustomerId;
            var chargeData = !string.IsNullOrEmpty(criteria.CustomerId) ? GetChargeOBHSellPayee(query, null) : GetChargeOBHSellPayee(null, null);
            var detailLookupSur = chargeData.ToLookup(q => q.HBLID);
            var dataCustom = customsDeclarationRepo.Get().ToList();
            var partnerData = catPartnerRepo.Get();
            var detailLookupPartner = partnerData.ToLookup(q => q.Id);
            var DetailLookupPort = port.ToLookup(q => q.Id);
            foreach (var item in dataShipment)
            {
                if (item.Hblid != null && item.Hblid != Guid.Empty)
                {
                    foreach (var charge in detailLookupSur[item.Hblid])
                    {
                        SummaryOfCostsIncurredExportResult data = new SummaryOfCostsIncurredExportResult();
                        var _partnerId = charge.TypeCharge == "OBH" ? charge.PayerId : charge.CustomerID;
                        data.PurchaseOrderNo = item.PurchaseOrderNo;
                        data.CustomNo = GetTopClearanceNoByJobNo(item.JobNo, dataCustom);
                        data.HBL = charge.HBL;
                        data.GrossWeight = charge.GrossWeight;
                        data.CBM = charge.CBM;
                        data.PackageContainer = charge.PackageContainer;
                        foreach (var partner in detailLookupPartner[_partnerId])
                        {
                            data.SupplierCode = partner?.AccountNo;
                            data.SuplierName = partner?.PartnerNameVn;
                        }
                        if (charge.AOL != Guid.Empty && charge.AOL != null)
                        {
                            foreach (var Port in DetailLookupPort[(Guid)charge.AOL])
                            {
                                data.POLName = Port.NameEn;
                            }
                        }
                        data.ChargeName = charge.ChargeName;
                        if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                        {
                            charge.NetAmount = charge.AmountUSD;
                            charge.VATAmount = charge.VATAmountUSD;
                        }
                        else
                        {
                            charge.NetAmount = charge.AmountVND;
                            charge.VATAmount = charge.VATAmountVND;
                        }
                        data.NetAmount = charge.NetAmount;
                        data.VATAmount = charge.VATAmount;
                        data.Type = charge.Type;
                        data.TypeCharge = charge.TypeCharge;
                        dataList.Add(data);
                    }
                }

            }
            return dataList.AsQueryable();
        }
        public List<SummaryOfCostsIncurredExportResult> GetDataSummaryOfCostsIncurred(GeneralReportCriteria criteria)
        {
            var dataDocumentation = SummaryOfCostsIncurred(criteria);
            IQueryable<SummaryOfCostsIncurredExportResult> list;
            if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains("CL"))
            {
                var dataOperation = SummaryOfCostsIncurredOperation(criteria);
                list = dataDocumentation.Union(dataOperation);
            }
            else
            {
                list = dataDocumentation;
            }
            return list.ToList();
        }


        private string GetTopClearanceNoByJobNo(string JobNo, List<CustomsDeclaration> customsDeclarations)
        {
            var clearanceNo = customsDeclarations.Where(x => x.JobNo != null && x.JobNo == JobNo)
                .OrderBy(x => x.JobNo)
                .OrderByDescending(x => x.ClearanceDate)
                .FirstOrDefault()?.ClearanceNo;
            return clearanceNo;
        }
        private IQueryable<SummaryOfCostsIncurredExportResult> SummaryOfCostsIncurred(GeneralReportCriteria criteria)
        {
            var dataShipment = QueryDataSummaryOfCostsIncurred(criteria);
            if (dataShipment == null) return null;
            var port = catPlaceRepo.Get();
            List<SummaryOfCostsIncurredExportResult> dataList = new List<SummaryOfCostsIncurredExportResult>();
            Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query = chg => chg.CustomerID == criteria.CustomerId;
            var chargeData = !string.IsNullOrEmpty(criteria.CustomerId) ? GetChargeOBHSellPayee(query, null) : GetChargeOBHSellPayee(null, null);
            var detailLookupSur = chargeData.ToLookup(q => q.HBLID);
            var dataCustom = customsDeclarationRepo.Get().ToList();
            var partnerData = catPartnerRepo.Get();
            var detailLookupPartner = partnerData.ToLookup(q => q.Id);
            var DetailLookupPort = port.ToLookup(q => q.Id);
            foreach (var item in dataShipment)
            {
                if (item.HBLID != null && item.HBLID != Guid.Empty)
                {
                    foreach (var charge in detailLookupSur[item.HBLID])
                    {
                        SummaryOfCostsIncurredExportResult data = new SummaryOfCostsIncurredExportResult();
                        var _partnerId = charge.TypeCharge == "OBH" ? charge.PayerId : charge.CustomerID;
                        data.PurchaseOrderNo = item.PurchaseOrderNo;
                        data.CustomNo = GetTopClearanceNoByJobNo(item.JobId, dataCustom);
                        data.HBL = charge.HBL;
                        data.GrossWeight = charge.GrossWeight;
                        data.CBM = charge.CBM;
                        data.PackageContainer = charge.PackageContainer;
                        if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                        {
                            charge.NetAmount = charge.AmountUSD;
                            charge.VATAmount = charge.VATAmountUSD;
                        }
                        else
                        {
                            charge.NetAmount = charge.AmountVND;
                            charge.VATAmount = charge.VATAmountVND;
                        }

                        foreach (var partner in detailLookupPartner[_partnerId])
                        {
                            data.SupplierCode = partner?.AccountNo;
                            data.SuplierName = partner?.PartnerNameVn;
                            data.ChargeName = charge.ChargeName;
                        }
                        if (charge.AOL != Guid.Empty && charge.AOL != null)
                        {
                            foreach (var Port in DetailLookupPort[(Guid)charge.AOL])
                            {
                                data.POLName = Port.NameEn;
                            }
                        }
                        data.NetAmount = charge.NetAmount;
                        data.VATAmount = charge.VATAmount;
                        data.Type = charge.Type;
                        data.TypeCharge = charge.TypeCharge;
                        dataList.Add(data);
                    }
                }

            }
            return dataList.AsQueryable();
        }
        private IQueryable<SummaryOfCostsIncurredExportResult> QueryDataSummaryOfCostsIncurred(GeneralReportCriteria criteria)
        {
            // Filter data without customerId
            var criteriaNoCustomer = (GeneralReportCriteria)criteria.Clone();
            criteriaNoCustomer.CustomerId = null;
            Expression<Func<CsTransaction, bool>> queryTrans = GetQueryTransationDocumentation(criteriaNoCustomer);
            Expression<Func<CsTransactionDetail, bool>> queryTranDetail = GetQueryTransationDetailDocumentation(criteriaNoCustomer);

            var masterBills = DataContext.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED).Where(queryTrans);
            if (queryTranDetail == null)
            {
                var houseBills = detailRepository.Get();
                var queryShipment = from master in masterBills
                                    join house in houseBills on master.Id equals house.JobId
                                    select new SummaryOfCostsIncurredExportResult
                                    {
                                        JobId = master.JobNo,
                                        ServiceDate = master.ServiceDate,
                                        Service = master.TransactionType,
                                        HBLID = house.Id,
                                        PurchaseOrderNo = master.Pono,
                                        AOL = master.Pol,

                                    };

                return queryShipment.AsQueryable();
            }
            else
            {
                var houseBills = GetTransactionDetailDocWithSalesman(queryTranDetail, criteria);
                var queryShipment = from master in masterBills
                                    join house in houseBills on master.Id equals house.JobId
                                    select new SummaryOfCostsIncurredExportResult
                                    {
                                        JobId = master.JobNo,
                                        ServiceDate = master.ServiceDate,
                                        Service = master.TransactionType,
                                        HBLID = house.Id,
                                        PurchaseOrderNo = master.Pono,
                                        AOL = master.Pol
                                    };
                return queryShipment;
            }
        }

        private IQueryable<SummaryOfCostsIncurredExportResult> GetChargeOBHSellPayee(Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query, bool? isOBH)
        {
            //Chỉ lấy những phí từ shipment (IsFromShipment = true)
            var surcharge = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_OBH_TYPE || x.Type == DocumentConstants.CHARGE_BUY_TYPE);
            var opst = opsRepository.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != null && x.CurrentStatus != TermData.Canceled);
            var csTrans = DataContext.Get(x => x.CurrentStatus != TermData.Canceled);
            var csTransDe = detailRepository.Get();
            var charge = catChargeRepo.Get();
            //OBH Payer (BUY - Credit)
            var queryObhBuyOperation = from sur in surcharge
                                       join ops in opst on sur.Hblid equals ops.Hblid
                                       join chg in charge on sur.ChargeId equals chg.Id into chg2
                                       from chg in chg2.DefaultIfEmpty()
                                       select new SummaryOfCostsIncurredExportResult
                                       {
                                           ID = sur.Id,
                                           HBLID = sur.Hblid,
                                           ChargeID = sur.ChargeId,
                                           ChargeCode = chg.Code,
                                           ChargeName = chg.ChargeNameEn,
                                           JobId = ops.JobNo,
                                           HBL = ops.Hwbno,
                                           MBL = ops.Mblno,
                                           Type = sur.Type + "-SELL",
                                           Debit = null,
                                           Credit = sur.Total,
                                           IsOBH = true,
                                           Currency = sur.CurrencyId,
                                           InvoiceNo = sur.InvoiceNo,
                                           Note = sur.Notes,
                                           CustomerID = sur.Type == "OBH" ? sur.PayerId : sur.PaymentObjectId,
                                           ServiceDate = ops.ServiceDate,
                                           CreatedDate = ops.DatetimeCreated,
                                           TransactionType = null,
                                           UserCreated = ops.UserCreated,
                                           Quantity = sur.Quantity,
                                           UnitId = sur.UnitId,
                                           UnitPrice = sur.UnitPrice,
                                           VATRate = sur.Vatrate,
                                           CreditDebitNo = sur.CreditNo,
                                           CommodityGroupID = ops.CommodityGroupId,
                                           Service = "CL",
                                           ExchangeDate = sur.ExchangeDate,
                                           FinalExchangeRate = sur.FinalExchangeRate,
                                           TypeCharge = chg.Type,
                                           PayerId = sur.PayerId,
                                           VATAmountUSD = sur.VatAmountUsd,
                                           VATAmountVND = sur.VatAmountVnd,
                                           AmountUSD = sur.AmountUsd,
                                           AmountVND = sur.AmountVnd

                                       };
            if (query != null)
            {
                queryObhBuyOperation = queryObhBuyOperation.Where(x => !string.IsNullOrEmpty(x.CustomerID)).Where(query);
            }
            if (isOBH != null)
            {
                queryObhBuyOperation = queryObhBuyOperation.Where(x => x.IsOBH == isOBH);
            }
            var queryObhBuyDocument = from sur in surcharge
                                      join cstd in csTransDe on sur.Hblid equals cstd.Id
                                      join cst in csTrans on cstd.JobId equals cst.Id
                                      join chg in charge on sur.ChargeId equals chg.Id into chg2
                                      from chg in chg2.DefaultIfEmpty()
                                      select new SummaryOfCostsIncurredExportResult
                                      {
                                          ID = sur.Id,
                                          HBLID = sur.Hblid,
                                          ChargeID = sur.ChargeId,
                                          ChargeCode = chg.Code,
                                          ChargeName = chg.ChargeNameEn,
                                          JobId = cst.JobNo,
                                          HBL = cstd.Hwbno,
                                          MBL = cst.Mawb,
                                          Type = sur.Type + "-SELL",
                                          Debit = null,
                                          Credit = sur.Total,
                                          IsOBH = true,
                                          Currency = sur.CurrencyId,
                                          InvoiceNo = sur.InvoiceNo,
                                          Note = sur.Notes,
                                          CustomerID = sur.Type == "OBH" ? sur.PayerId : sur.PaymentObjectId,
                                          ServiceDate = (cst.TransactionType == "AI" || cst.TransactionType == "SFI" || cst.TransactionType == "SLI" || cst.TransactionType == "SCI" ? cst.Eta : cst.Etd),
                                          CreatedDate = cst.DatetimeCreated,
                                          TransactionType = cst.TransactionType,
                                          UserCreated = cst.UserCreated,
                                          Quantity = sur.Quantity,
                                          UnitId = sur.UnitId,
                                          UnitPrice = sur.UnitPrice,
                                          VATRate = sur.Vatrate,
                                          CreditDebitNo = sur.CreditNo,
                                          CommodityGroupID = null,
                                          Service = cst.TransactionType,
                                          CBM = cstd.Cbm,
                                          Commodity = cst.Commodity,
                                          FlightNo = cstd.FlightNo,
                                          ShippmentDate = cst.TransactionType == "AE" ? cstd.Etd : cst.TransactionType == "AI" ? cstd.Eta : null,
                                          AOL = cst.Pol,
                                          AOD = cst.Pod,
                                          PackageQty = cstd.PackageQty,
                                          GrossWeight = cstd.GrossWeight,
                                          ChargeWeight = cstd.ChargeWeight,
                                          FinalExchangeRate = sur.FinalExchangeRate,
                                          ExchangeDate = sur.ExchangeDate,
                                          PackageContainer = cstd.PackageContainer,
                                          TypeCharge = chg.Type,
                                          PayerId = sur.PayerId,
                                          VATAmountUSD = sur.VatAmountUsd,
                                          VATAmountVND = sur.VatAmountVnd,
                                          AmountUSD = sur.AmountUsd,
                                          AmountVND = sur.AmountVnd
                                      };
            if (query != null)
            {
                queryObhBuyDocument = queryObhBuyDocument.Where(x => !string.IsNullOrEmpty(x.CustomerID)).Where(query);
            }
            if (isOBH != null)
            {
                queryObhBuyDocument = queryObhBuyDocument.Where(x => x.IsOBH == isOBH);
            }
            var queryObhBuy = queryObhBuyOperation.Union(queryObhBuyDocument);
            return queryObhBuy;
        }



        #endregion

        #region Export Summary Of Revenue Incurred
        private SummaryOfRevenueModel SummaryOfRevenueIncurredOperation(GeneralReportCriteria criteria)
        {
            var dataShipment = QueryDataOperationAcctPLSheet(criteria);
            if (dataShipment == null) return null;
            var port = catPlaceRepo.Get();
            SummaryOfRevenueModel ObjectSummaryRevenue = new SummaryOfRevenueModel();
            List<SummaryOfRevenueExportResult> dataList = new List<SummaryOfRevenueExportResult>();
            Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query = chg => chg.CustomerID == criteria.CustomerId;
            var chargeData = !string.IsNullOrEmpty(criteria.CustomerId) ? GetChargeOBHSellPayerJob(query, null) : GetChargeOBHSellPayerJob(null, null);
            var results = chargeData.GroupBy(x => new { x.JobId, x.HBLID }).AsQueryable();

            var lookupReuslts = results.ToLookup(q => q.Key.JobId);
            var listPartner = catPartnerRepo.Get();
            var lookupPartner = listPartner.ToLookup(q => q.Id);
            var dataCustom = customsDeclarationRepo.Get().ToList();
            var commodityGroupData = catCommodityGroupRepo.Get().ToList();
            var lookupCommodityGroup = commodityGroupData.ToLookup(q => q.Id);
            var dataOps = opsRepository.Get().ToList();
            if (results == null)
                return null;
            foreach (var item in dataShipment)
            {
                if (item.Hblid != Guid.Empty)
                {
                    foreach (var group in lookupReuslts[item.JobNo])
                    {
                        SummaryOfRevenueExportResult SummaryRevenue = new SummaryOfRevenueExportResult();
                        SummaryRevenue.SummaryOfCostsIncurredExportResults = new List<SummaryOfCostsIncurredExportResult>();
                        var commodity = DataContext.Get(x => x.JobNo == group.Key.JobId).Select(t => t.Commodity).FirstOrDefault();
                        var commodityGroup = opsRepository.Get(x => x.JobNo == group.Key.JobId).Select(t => t.CommodityGroupId).FirstOrDefault();

                        string commodityName = string.Empty;
                        var _partnerId = group.Select(t => t.CustomerID).FirstOrDefault();
                        if (commodity != null)
                        {
                            string[] commodityArr = commodity.Split(',');
                            foreach (var it in commodityArr)
                            {
                                commodityName = commodityName + "," + catCommodityRepo.Get(x => x.CommodityNameEn == it.Replace("\n", "")).Select(t => t.CommodityNameEn).FirstOrDefault();
                            }
                            commodityName = commodityName.Substring(1);
                        }
                        if (commodityGroup != null)
                        {
                            foreach (var commodityG in lookupCommodityGroup[(short)commodityGroup])
                            {
                                commodityName = commodityG.GroupNameVn;
                            }
                        }
                        SummaryRevenue.ChargeName = commodityName;
                        SummaryRevenue.POLName = port.Where(x => x.Id == item.Pol).Select(t => t.NameEn).FirstOrDefault();
                        SummaryRevenue.CustomNo = GetTopClearanceNoByJobNo(group.Key.JobId, dataCustom);
                        SummaryRevenue.HBL = group.Select(t => t.HBL).FirstOrDefault();
                        SummaryRevenue.CBM = group.Select(t => t.CBM).FirstOrDefault();
                        SummaryRevenue.GrossWeight = group.Select(t => t.GrossWeight).FirstOrDefault();
                        SummaryRevenue.PackageContainer = group.Select(t => t.PackageContainer).FirstOrDefault();
                        foreach (var ele in group)
                        {
                            ele.SuplierName = catPartnerRepo.Get(x => x.Id == ele.CustomerID).Select(t => t.PartnerNameVn).FirstOrDefault();
                        }
                        foreach (var partner in lookupPartner[_partnerId])
                        {
                            SummaryRevenue.SupplierCode = partner?.AccountNo;
                            SummaryRevenue.SuplierName = partner?.PartnerNameVn;
                        }
                        SummaryRevenue.SummaryOfCostsIncurredExportResults.AddRange(group.Select(t => t));
                        dataList.Add(SummaryRevenue);
                    }
                }
            }
            ObjectSummaryRevenue.summaryOfRevenueExportResults = dataList;
            foreach (var item in ObjectSummaryRevenue.summaryOfRevenueExportResults)
            {
                foreach (var it in item.SummaryOfCostsIncurredExportResults)
                {
                    var _exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(it.FinalExchangeRate, it.ExchangeDate, it.Currency, criteria.Currency);
                    decimal UnitPrice = it.UnitPrice ?? 0;
                    //it.UnitPrice = NumberHelper.RoundNumber(UnitPrice, 3);
                    it.NetAmount = UnitPrice * it.Quantity * _exchangeRate;
                    if (it.Currency != DocumentConstants.CURRENCY_LOCAL)
                    {
                        it.VATAmount = NumberHelper.RoundNumber(it.VATAmount ?? 0, 2);
                        it.NetAmount = NumberHelper.RoundNumber(it.NetAmount ?? 0, 2);
                    }
                    else
                    {
                        it.VATAmount = NumberHelper.RoundNumber(it.VATAmount ?? 0, 0);
                        it.NetAmount = NumberHelper.RoundNumber(it.NetAmount ?? 0, 0);
                    }
                    if (it.VATRate > 0)
                    {
                        it.VATAmount = (it.VATRate * it.NetAmount) / 100;
                    }
                    else
                    {
                        it.VATAmount = it.VATRate != null ? Math.Abs(it.VATRate.Value) : 0;
                        it.VATAmount = it.VATAmount * _exchangeRate;
                    }
                }
            }
            return ObjectSummaryRevenue;
        }
        public SummaryOfRevenueModel GetDataSummaryOfRevenueIncurred(GeneralReportCriteria criteria)
        {
            var dataDocumentation = SummaryOfRevenueIncurred(criteria);
            SummaryOfRevenueModel obj = new SummaryOfRevenueModel();

            if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains("CL"))
            {
                var dataOperation = SummaryOfRevenueIncurredOperation(criteria);
                var lstDoc = dataDocumentation.summaryOfRevenueExportResults.AsQueryable();
                var lstOperation = dataOperation.summaryOfRevenueExportResults.AsQueryable();
                var lst = lstDoc.Union(lstOperation);
                obj.summaryOfRevenueExportResults = lst.ToList();
            }
            else
            {
                obj = dataDocumentation;
            }
            return obj;
        }
        private SummaryOfRevenueModel SummaryOfRevenueIncurred(GeneralReportCriteria criteria)
        {

            var dataShipment = QueryDataSummaryOfCostsIncurred(criteria);
            if (dataShipment == null) return null;
            var port = catPlaceRepo.Get();
            SummaryOfRevenueModel ObjectSummaryRevenue = new SummaryOfRevenueModel();
            List<SummaryOfRevenueExportResult> dataList = new List<SummaryOfRevenueExportResult>();
            Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query = chg => chg.CustomerID == criteria.CustomerId;
            var chargeData = GetChargeOBHSellPayer(query, null);
            var results = chargeData.GroupBy(x => new { x.JobId, x.HBLID }).AsQueryable();
            var lookupResults = results.ToLookup(q => q.Key.HBLID);
            var listPartner = catPartnerRepo.Get();
            var lookupPartner = listPartner.ToLookup(q => q.Id);
            var dataCustom = customsDeclarationRepo.Get().ToList();
            foreach (var item in dataShipment)
            {
                if (item.HBLID != Guid.Empty)
                {
                    foreach (var group in lookupResults[item.HBLID])
                    {
                        SummaryOfRevenueExportResult SummaryRevenue = new SummaryOfRevenueExportResult();
                        SummaryRevenue.SummaryOfCostsIncurredExportResults = new List<SummaryOfCostsIncurredExportResult>();
                        var commodity = DataContext.Get(x => x.JobNo == group.Key.JobId).Select(t => t.Commodity).FirstOrDefault();
                        var commodityGroup = opsRepository.Get(x => x.JobNo == group.Key.JobId).Select(t => t.CommodityGroupId).FirstOrDefault();
                        string commodityName = string.Empty;
                        var _partnerId = group.Select(t => t.CustomerID).FirstOrDefault();

                        if (commodity != null)
                        {
                            string[] commodityArr = commodity.Split(',');
                            foreach (var it in commodityArr)
                            {
                                if (catCommodityRepo.Any(x => x.CommodityNameEn == it.Replace("\n", "")))
                                {
                                    commodityName = commodityName + "," + catCommodityRepo.Get(x => x.CommodityNameEn == it.Replace("\n", "")).Select(t => t.CommodityNameEn).FirstOrDefault();
                                }
                                else
                                {
                                    commodityName = commodityName + "," + catCommodityRepo.Get(x => x.Code == it.Replace("\n", "")).Select(t => t.CommodityNameEn).FirstOrDefault();
                                }
                            }
                            commodityName = commodityName.Substring(1);
                        }
                        if (commodityGroup != null)
                        {
                            commodityName = catCommodityGroupRepo.Get(x => x.Id == commodityGroup).Select(t => t.GroupNameVn).FirstOrDefault();
                        }
                        SummaryRevenue.ChargeName = commodityName;
                        SummaryRevenue.POLName = port.Where(x => x.Id == item.AOL).Select(t => t.NameEn).FirstOrDefault();
                        SummaryRevenue.CustomNo = GetTopClearanceNoByJobNo(group.Key.JobId, dataCustom);
                        SummaryRevenue.HBL = group.Select(t => t.HBL).FirstOrDefault();
                        SummaryRevenue.CBM = group.Select(t => t.CBM).FirstOrDefault();
                        SummaryRevenue.GrossWeight = group.Select(t => t.GrossWeight).FirstOrDefault();
                        SummaryRevenue.PackageContainer = group.Select(t => t.PackageContainer).FirstOrDefault();
                        foreach (var ele in group)
                        {
                            ele.SuplierName = catPartnerRepo.Get(x => x.Id == ele.CustomerID).Select(t => t.PartnerNameVn).FirstOrDefault();
                        }
                        foreach (var partner in lookupPartner[_partnerId])
                        {
                            SummaryRevenue.SupplierCode = partner?.AccountNo;
                            SummaryRevenue.SuplierName = partner?.PartnerNameVn;
                        }

                        SummaryRevenue.SummaryOfCostsIncurredExportResults.AddRange(group.Select(t => t));
                        dataList.Add(SummaryRevenue);
                    }
                }
            }
            ObjectSummaryRevenue.summaryOfRevenueExportResults = dataList;
            foreach (var item in ObjectSummaryRevenue.summaryOfRevenueExportResults)
            {
                foreach (var it in item.SummaryOfCostsIncurredExportResults)
                {
                    var _exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(it.FinalExchangeRate, it.ExchangeDate, it.Currency, criteria.Currency);
                    decimal UnitPrice = it.UnitPrice ?? 0;
                    //it.UnitPrice = NumberHelper.RoundNumber(UnitPrice, 3);
                    it.NetAmount = UnitPrice * it.Quantity * _exchangeRate;
                    if (it.Currency != DocumentConstants.CURRENCY_LOCAL)
                    {
                        it.VATAmount = NumberHelper.RoundNumber(it.VATAmount ?? 0, 2);
                        it.NetAmount = NumberHelper.RoundNumber(it.NetAmount ?? 0, 2);
                    }
                    else
                    {
                        it.VATAmount = NumberHelper.RoundNumber(it.VATAmount ?? 0, 0);
                        it.NetAmount = NumberHelper.RoundNumber(it.NetAmount ?? 0, 0);
                    }
                    if (it.VATRate > 0)
                    {
                        it.VATAmount = (it.VATRate * it.NetAmount) / 100;
                    }
                    else
                    {
                        it.VATAmount = it.VATRate != null ? Math.Abs(it.VATRate.Value) : 0;
                        it.VATAmount = it.VATAmount * _exchangeRate;
                    }

                }
            }
            return ObjectSummaryRevenue;
        }

        private IQueryable<SummaryOfCostsIncurredExportResult> GetChargeOBHSellPayerJob(Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query, bool? isOBH)
        {
            //Chỉ lấy những phí từ shipment (IsFromShipment = true)
            var surcharge = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_OBH_TYPE || x.Type == DocumentConstants.CHARGE_SELL_TYPE);
            var opst = opsRepository.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != null && x.CurrentStatus != TermData.Canceled);
            var csTrans = DataContext.Get(x => x.CurrentStatus != TermData.Canceled);
            var csTransDe = detailRepository.Get();
            var charge = catChargeRepo.Get();
            var unit = catUnitRepo.Get();
            var queryObhBuyOperation = from sur in surcharge
                                       join ops in opst on sur.Hblid equals ops.Hblid
                                       join chg in charge on sur.ChargeId equals chg.Id into chg2
                                       from chg in chg2.DefaultIfEmpty()
                                       join uni in unit on sur.UnitId equals uni.Id into uni2
                                       from uni in uni2.DefaultIfEmpty()
                                       select new SummaryOfCostsIncurredExportResult
                                       {
                                           ID = sur.Id,
                                           HBLID = sur.Hblid,
                                           ChargeID = sur.ChargeId,
                                           ChargeCode = chg.Code,
                                           ChargeName = chg.ChargeNameEn,
                                           JobId = ops.JobNo,
                                           HBL = ops.Hwbno,
                                           MBL = ops.Mblno,
                                           Type = sur.Type + "-BUY",
                                           SoaNo = sur.PaySoano,
                                           Debit = null,
                                           Credit = sur.Total,
                                           IsOBH = true,
                                           Currency = sur.CurrencyId,
                                           InvoiceNo = sur.InvoiceNo,
                                           Note = sur.Notes,
                                           CustomerID = sur.PaymentObjectId,
                                           ServiceDate = ops.ServiceDate,
                                           CreatedDate = ops.DatetimeCreated,
                                           TransactionType = null,
                                           UserCreated = ops.UserCreated,
                                           Quantity = sur.Quantity,
                                           UnitId = sur.UnitId,
                                           UnitPrice = sur.UnitPrice,
                                           VATRate = sur.Vatrate,
                                           CreditDebitNo = sur.CreditNo,
                                           CommodityGroupID = ops.CommodityGroupId,
                                           Service = "CL",
                                           ExchangeDate = sur.ExchangeDate,
                                           FinalExchangeRate = sur.FinalExchangeRate,
                                           TypeCharge = chg.Type,
                                           PayerId = sur.PayerId,
                                           Unit = uni.UnitNameEn,
                                           InvoiceDate = sur.InvoiceDate,
                                           CBM = ops.SumCbm,
                                           GrossWeight = ops.SumGrossWeight,
                                           PackageContainer = ops.ContainerDescription,
                                           PackageQty = ops.SumPackages,

                                       };
            if (query != null)
            {
                queryObhBuyOperation = queryObhBuyOperation.Where(x => !string.IsNullOrEmpty(x.Service)).Where(query);
                queryObhBuyOperation = queryObhBuyOperation.Where(x => !string.IsNullOrEmpty(x.CustomerID)).Where(query);
            }
            return queryObhBuyOperation;
        }

        private IQueryable<SummaryOfCostsIncurredExportResult> GetChargeOBHSellPayer(Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query, bool? isOBH)
        {
            //Chỉ lấy những phí từ shipment (IsFromShipment = true)
            var surcharge = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_OBH_TYPE || x.Type == DocumentConstants.CHARGE_SELL_TYPE);
            var csTrans = DataContext.Get(x => x.CurrentStatus != TermData.Canceled);
            var csTransDe = detailRepository.Get();
            var charge = catChargeRepo.Get();
            var unit = catUnitRepo.Get();

            var queryObhBuyDocument = from sur in surcharge
                                      join cstd in csTransDe on sur.Hblid equals cstd.Id
                                      join cst in csTrans on cstd.JobId equals cst.Id
                                      join chg in charge on sur.ChargeId equals chg.Id into chg2
                                      from chg in chg2.DefaultIfEmpty()
                                      join uni in unit on sur.UnitId equals uni.Id into uni2
                                      from uni in uni2.DefaultIfEmpty()
                                      select new SummaryOfCostsIncurredExportResult
                                      {
                                          ID = sur.Id,
                                          HBLID = sur.Hblid,
                                          ChargeID = sur.ChargeId,
                                          ChargeCode = chg.Code,
                                          ChargeName = chg.ChargeNameEn,
                                          JobId = cst.JobNo,
                                          HBL = cstd.Hwbno,
                                          MBL = cst.Mawb,
                                          Type = sur.Type + "-BUY",
                                          SoaNo = sur.PaySoano,
                                          Debit = null,
                                          Credit = sur.Total,
                                          IsOBH = true,
                                          Currency = sur.CurrencyId,
                                          InvoiceNo = sur.InvoiceNo,
                                          Note = sur.Notes,
                                          CustomerID = sur.PaymentObjectId,
                                          ServiceDate = (cst.TransactionType == "AI" || cst.TransactionType == "SFI" || cst.TransactionType == "SLI" || cst.TransactionType == "SCI" ? cst.Eta : cst.Etd),
                                          CreatedDate = cst.DatetimeCreated,
                                          TransactionType = cst.TransactionType,
                                          UserCreated = cst.UserCreated,
                                          Quantity = sur.Quantity,
                                          UnitId = sur.UnitId,
                                          UnitPrice = sur.UnitPrice,
                                          VATRate = sur.Vatrate,
                                          CreditDebitNo = sur.CreditNo,
                                          CommodityGroupID = null,
                                          Service = cst.TransactionType,
                                          CBM = cstd.Cbm,
                                          Commodity = cst.Commodity,
                                          FlightNo = cstd.FlightNo,
                                          ShippmentDate = cst.TransactionType == "AE" ? cstd.Etd : cst.TransactionType == "AI" ? cstd.Eta : null,
                                          AOL = cst.Pol,
                                          AOD = cst.Pod,
                                          PackageQty = cstd.PackageQty,
                                          GrossWeight = cstd.GrossWeight,
                                          ChargeWeight = cstd.ChargeWeight,
                                          FinalExchangeRate = sur.FinalExchangeRate,
                                          ExchangeDate = sur.ExchangeDate,
                                          PackageContainer = cstd.PackageContainer,
                                          TypeCharge = chg.Type,
                                          PayerId = sur.PayerId,
                                          Unit = uni.UnitNameEn,
                                          InvoiceDate = sur.InvoiceDate
                                      };
            if (query != null)
            {
                queryObhBuyDocument = queryObhBuyDocument.Where(x => !string.IsNullOrEmpty(x.Service)).Where(query);
                queryObhBuyDocument = queryObhBuyDocument.Where(x => !string.IsNullOrEmpty(x.CustomerID)).Where(query);
            }
            if (isOBH != null)
            {
                queryObhBuyDocument = queryObhBuyDocument.Where(x => x.IsOBH == isOBH);
            }
            //var queryObhBuy = queryObhBuyOperation.Union(queryObhBuyDocument);
            return queryObhBuyDocument;
        }

        #endregion

    }
}
