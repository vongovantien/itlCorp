﻿using AutoMapper;
using eFMS.API.Common.Helpers;
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
using System.Data.SqlClient;
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
        readonly IContextBase<CatIncoterm> catIncotermRepository;
        readonly IContextBase<AcctAdvanceRequest> acctAdvanceRequestRepository;
        private readonly IContextBase<CsMawbcontainer> csMawbcontainerRepo;
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
            IContextBase<CatDepartment> departmentRepo,
            IContextBase<CatIncoterm> catIncotermRepo,
            IContextBase<CsMawbcontainer> csMawbcontainer,
            IContextBase<AcctAdvanceRequest> acctAdvanceRequestRepo) : base(repository, mapper)
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
            catIncotermRepository = catIncotermRepo;
            csMawbcontainerRepo = csMawbcontainer;
            acctAdvanceRequestRepository = acctAdvanceRequestRepo;
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
                && (!string.IsNullOrEmpty(partner) ? (x.PayerId == partner || x.PaymentObjectId == partner) : false)
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
                .Where(x => x.JobId != null && x.HBL != null)
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

                shipments = shipmentsDocumention.Union(shipmentsOperation).Where(x => x.JobId != null && x.HBL != null).Select(s => new Shipments { JobId = s.JobId, HBL = s.HBL, MBL = s.MBL });
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
            string userCurrent = currentUser.UserID;

            List<ShipmentsCopy> dataList = new List<ShipmentsCopy>();

            if (string.IsNullOrEmpty(searchOption) || keywords == null || keywords.Count == 0 || keywords.Any(x => x == null)) return dataList;

            IQueryable<CsShipmentSurcharge> surcharge = surCharge.Get();

            //Start change request Modified 14/10/2019 by Andy.Hoa
            //Get list shipment operation theo user current
            IQueryable<OpsTransaction> opstransaction = opsRepository.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED && x.OfficeId == currentUser.OfficeID);  // Lấy theo office current user

            //OPS assign
            IQueryable<OpsTransaction> opstranAssign = from ops in opstransaction
                                                       join osa in opsStageAssignedRepo.Get() on ops.Id equals osa.JobId //So sánh bằng
                                                       where osa.MainPersonInCharge == userCurrent
                                                       select ops;
            //OPS is BillingOps
            IQueryable<OpsTransaction> opstranPic = opstransaction.Where(x => x.BillingOpsId == userCurrent);
            IQueryable<OpsTransaction> opstran = opstranAssign.Union(opstranPic);

            IQueryable<ShipmentsCopy> shipmentOperation = Enumerable.Empty<ShipmentsCopy>().AsQueryable();

            if (searchOption == "ClearanceNo")
            {
                shipmentOperation = from ops in opstran
                                    join cd in customsDeclarationRepo.Get() on ops.JobNo equals cd.JobNo into cdGrps
                                    from cdgrp in cdGrps.DefaultIfEmpty()
                                    join sur in surcharge on ops.Hblid equals sur.Hblid into sur2
                                    from sur in sur2.DefaultIfEmpty()
                                    join cus in catPartnerRepo.Get() on ops.CustomerId equals cus.Id into cus2
                                    from cus in cus2.DefaultIfEmpty()
                                    where keywords.Contains(cdgrp.ClearanceNo, StringComparer.OrdinalIgnoreCase)
                                    select new ShipmentsCopy
                                    {
                                        JobId = ops.JobNo,
                                        Customer = cus.ShortName,
                                        MBL = ops.Mblno,
                                        HBL = ops.Hwbno,
                                        HBLID = ops.Hblid,
                                        CustomNo = cdgrp.ClearanceNo,
                                        Service = "CL"
                                    };
            }
            else
            {
                shipmentOperation = from ops in opstran
                                    join cd in customsDeclarationRepo.Get() on ops.JobNo equals cd.JobNo into cdGrps
                                    from cdgrp in cdGrps.DefaultIfEmpty()
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
                                    select new ShipmentsCopy
                                    {
                                        JobId = ops.JobNo,
                                        Customer = cus.ShortName,
                                        MBL = ops.Mblno,
                                        HBL = ops.Hwbno,
                                        HBLID = ops.Hblid,
                                        CustomNo = cdgrp.ClearanceNo,
                                        Service = "CL"
                                    };
            }
            shipmentOperation = shipmentOperation.Distinct();

            IQueryable<ShipmentsCopy> shipmentDoc = Enumerable.Empty<ShipmentsCopy>().AsQueryable();

            if (searchOption != "ClearanceNo")
            {
                IQueryable<CsTransaction> transactions = DataContext.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED && x.OfficeId == currentUser.OfficeID);  // Lấy theo office current user
                //Transaction assign
                IQueryable<CsTransaction> cstranAssign = from cstd in transactions
                                                         join osa in opsStageAssignedRepo.Get() on cstd.Id equals osa.JobId //So sánh bằng
                                                         where osa.MainPersonInCharge == userCurrent
                                                         select cstd;
                //Transaction is Person In Charge
                IQueryable<CsTransaction> cstranPic = transactions.Where(x => x.PersonIncharge == userCurrent);
                IQueryable<CsTransaction> cstrans = cstranAssign.Union(cstranPic);

                IQueryable<CsTransactionDetail> cstrandel = detailRepository.Get();

                shipmentDoc = from cstd in cstrandel
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
                              //&&
                              //    searchOption.Equals("ClearanceNo") ? keywords.Contains(sur.ClearanceNo, StringComparer.OrdinalIgnoreCase) : true
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
            }
            var queryUnion = Enumerable.Empty<ShipmentsCopy>().AsQueryable();
            if (searchOption != "ClearanceNo")
            {
                queryUnion = shipmentOperation.Union(shipmentDoc);
            }
            else
            {
                queryUnion = shipmentOperation;
            }
            IQueryable<ShipmentsCopy> listShipment = queryUnion.Where(x => x.JobId != null && x.HBL != null)
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
            IQueryable<LockedLogModel> opShipments = Enumerable.Empty<LockedLogModel>().AsQueryable();
            string transactionType = string.Empty;

            if ((criteria.TransactionType == 0 || criteria.TransactionType == TransactionTypeEnum.CustomLogistic) && criteria.ShipmentPropertySearch != ShipmentPropertySearch.CD)
            {
                opShipments = opsRepository.Get(x => ((criteria.ShipmentPropertySearch == ShipmentPropertySearch.JOBID ? criteria.Keywords.Contains(x.JobNo, StringComparer.OrdinalIgnoreCase) : true
                                                   && criteria.ShipmentPropertySearch == ShipmentPropertySearch.MBL ? criteria.Keywords.Contains(x.Mblno, StringComparer.OrdinalIgnoreCase) : true)
                                                   || criteria.Keywords == null)
                                                   &&
                                                   (
                                                        criteria.OfficeIds.Contains(x.OfficeId.ToString())
                                                        &&
                                                       (((x.ServiceDate ?? null) >= (criteria.FromDate ?? null)) && ((x.ServiceDate ?? null) <= (criteria.ToDate ?? null)))
                                                       || (criteria.FromDate == null && criteria.ToDate == null)
                                                   ))
                                                   .Select(x => new LockedLogModel
                                                   {
                                                       Id = x.Id,
                                                       OPSShipmentNo = x.JobNo,
                                                       UnLockedLog = x.UnLockedLog,
                                                       IsLocked = x.IsLocked
                                                   });
            }
            else if (criteria.TransactionType == 0 || criteria.TransactionType == TransactionTypeEnum.CustomLogistic)
            {
                var customs = customsDeclarationRepo.Get(x => criteria.Keywords.Contains(x.ClearanceNo, StringComparer.OrdinalIgnoreCase) && !string.IsNullOrEmpty(x.JobNo));
                if (customs != null && customs.Count() > 0)
                {
                    opShipments = from cd in customs
                                  join ops in opsRepository.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED) on cd.JobNo equals ops.JobNo
                                  select new LockedLogModel
                                  {
                                      Id = ops.Id,
                                      IsLocked = ops.IsLocked,
                                      OPSShipmentNo = ops.JobNo,
                                      UnLockedLog = ops.UnLockedLog
                                  };
                }
            }

            if (criteria.TransactionType == TransactionTypeEnum.CustomLogistic || criteria.ShipmentPropertySearch == ShipmentPropertySearch.CD)
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
            IQueryable<CsTransaction> csTransactions = Enumerable.Empty<CsTransaction>().AsQueryable();

            csTransactions = DataContext.Get(x =>
                                        ((criteria.ShipmentPropertySearch == ShipmentPropertySearch.JOBID ? criteria.Keywords.Contains(x.JobNo, StringComparer.OrdinalIgnoreCase) : true
                                         && criteria.ShipmentPropertySearch == ShipmentPropertySearch.MBL ? criteria.Keywords.Contains(x.Mawb, StringComparer.OrdinalIgnoreCase) : true)
                                            || criteria.Keywords == null)
                                         && (criteria.OfficeIds != null ? criteria.OfficeIds.Contains(x.OfficeId.ToString()) : true)
                                         && (x.TransactionType == transactionType || string.IsNullOrEmpty(transactionType))
                                      );
            if (csTransactions.Count() > 0)
            {
                csTransactions = GetShipmentServicesByTime(csTransactions, criteria);
                if (criteria.ShipmentPropertySearch == ShipmentPropertySearch.HBL)
                {
                    var shipmentDetails = detailRepository.Get(x => criteria.Keywords.Contains(x.Hwbno, StringComparer.OrdinalIgnoreCase));
                    csTransactions = csTransactions.Join(shipmentDetails, x => x.Id, y => y.JobId, (x, y) => x);
                }
            }

            IQueryable<LockedLogModel> csShipments = csTransactions.Select(x => new LockedLogModel
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
                    string unlockJob = string.Empty;
                    foreach (var item in shipments)
                    {
                        if (item.OPSShipmentNo != null)
                        {
                            var opsShipment = opsRepository.Get(x => x.Id == item.Id)?.FirstOrDefault();
                            if (opsShipment != null)
                            {
                                if (opsShipment.IsLocked == true)
                                {
                                    opsShipment.IsLocked = false;
                                    opsShipment.DatetimeModified = DateTime.Now;
                                    opsShipment.UserModified = currentUser.UserID;
                                    opsShipment.LastDateUnLocked = DateTime.Now;
                                    string log = opsShipment.JobNo + " has been opened at " + string.Format("{0:HH:mm:ss tt}", DateTime.Now) + " on " + DateTime.Now.ToString("dd/MM/yyyy") + " by " + currentUser.UserName + ";";
                                    opsShipment.UnLockedLog = opsShipment.UnLockedLog + log;
                                    unlockJob += opsShipment.JobNo;
                                    var isSuccessLockOps = opsRepository.Update(opsShipment, x => x.Id == opsShipment.Id);
                                }
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
                                    unlockJob += csShipment.JobNo;
                                    var isSuccessLockCs = DataContext.Update(csShipment, x => x.Id == csShipment.Id);
                                }
                            }
                        }
                    }
                    trans.Commit();
                    if (!string.IsNullOrEmpty(unlockJob))
                    {
                        return new HandleState(true, (object)"Unlock Successful");
                    }
                    return new HandleState(true, (object)"These shipment's open.");
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

        public HandleState LockShipmentList(List<string> JobIds)
        {
            HandleState res = new HandleState();

            if (JobIds.Count == 0)
            {
                return res;
            }

            List<string> jobOps = JobIds.Where(x => x.Contains("LOG")).ToList();
            List<string> jobCs = JobIds.Where(x => !jobOps.Contains(x)).ToList();

            if (jobOps.Count == 0 && jobCs.Count == 0)
            {
                return res;
            }

            List<OpsTransaction> opsShipments = new List<OpsTransaction>();
            if (jobOps.Count > 0)
            {
                foreach (var item in jobOps)
                {
                    var shipment = opsRepository.Get(x => x.JobNo == item && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED && x.IsLocked == false)?.FirstOrDefault();
                    if (shipment != null)
                    {
                        opsShipments.Add(shipment);
                    }

                }

                if (opsShipments.Count() > 0)
                {
                    foreach (var job in opsShipments)
                    {
                        job.UserModified = currentUser.UserID;
                        job.DatetimeModified = DateTime.Now;
                        job.IsLocked = true;
                        job.LockedDate = DateTime.Now;
                        job.LockedUser = currentUser.UserName;

                        opsRepository.Update(job, x => x.Id == job.Id, false);
                    }
                    res = opsRepository.SubmitChanges();
                }
            }
            List<CsTransaction> csShipments = new List<CsTransaction>();

            if (jobCs.Count > 0)
            {
                foreach (var item in jobCs)
                {
                    var shipment = DataContext.Get(x => x.JobNo == item && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED && x.IsLocked == false)?.FirstOrDefault();
                    if (shipment != null)
                    {
                        csShipments.Add(shipment);
                    }
                }
                if (csShipments.Count() > 0)
                {
                    foreach (var job in csShipments)
                    {
                        job.UserModified = currentUser.UserID;
                        job.DatetimeModified = DateTime.Now;
                        job.IsLocked = true;
                        job.LockedDate = DateTime.Now;
                        job.LockedUser = currentUser.UserName;

                        DataContext.Update(job, x => x.Id == job.Id, false);
                    }
                    res = DataContext.SubmitChanges();
                }
            }

            return res;

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
                                         JobId = !string.IsNullOrEmpty(ops.JobNo) ? ops.JobNo.Trim() : ops.JobNo,
                                         HBL = !string.IsNullOrEmpty(ops.Hwbno) ? ops.Hwbno.Trim() : ops.Hwbno,
                                         MBL = !string.IsNullOrEmpty(ops.Mblno) ? ops.Mblno.Trim() : ops.Mblno,
                                         CustomerId = ops.CustomerId,
                                         AgentId = ops.AgentId,
                                         CarrierId = ops.SupplierId,
                                         HBLID = ops.Hblid,
                                         CustomNo = !string.IsNullOrEmpty(cus.ClearanceNo) ? cus.ClearanceNo.Trim() : cus.ClearanceNo
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
            var operations = opsRepository.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED && x.IsLocked == false && x.OfficeId == currentUser.OfficeID); // Lấy theo office current user
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

            var _shipmentsOperation = shipmentsOpsMerge.GroupBy(g => g.HBLID).Select(s => s.FirstOrDefault());

            var transactions = DataContext.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED && x.IsLocked == false && x.OfficeId == currentUser.OfficeID);  // Lấy theo office current user
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
            var _shipmentsDocumention = shipmentsDocumention.GroupBy(g => g.HBLID).Select(s => s.FirstOrDefault());

            var result = _shipmentsOperation.Union(_shipmentsDocumention);
            return result.OrderByDescending(o => o.MBL);
        }

        /// <summary>
        /// get list of shipment assign or PIC is current user for adv carrier
        /// </summary>
        /// <returns></returns>
        public IQueryable<Shipments> GetShipmentAssignPICCarrier(string typeAdvFor)
        {
            var userCurrent = currentUser.UserID;
            var advanceRequestJob = acctAdvanceRequestRepository.Get().Select(x => new { x.JobId, x.Mbl, x.Hblid }).ToList();
            var operations = opsRepository.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED && x.IsLocked == false && x.OfficeId == currentUser.OfficeID);  // Lấy theo office current user
            var customs = customsDeclarationRepo.Get(x => !string.IsNullOrEmpty(x.JobNo));
            // Shipment ops assign is current user
            var shipmentsOps = from ops in operations
                               join osa in opsStageAssignedRepo.Get() on ops.Id equals osa.JobId
                               where osa.MainPersonInCharge == userCurrent
                               select ops;
            //Shipment ops PIC is current user
            var shipmentsOpsPIC = operations.Where(x => x.BillingOpsId == userCurrent);
            //Merger Shipment Ops assign & PIC
            var shipmentsOpsMerge = from data in shipmentsOps.Union(shipmentsOpsPIC)
                                    join cus in customs on data.JobNo equals cus.JobNo into grpCus
                                    from cus in grpCus.DefaultIfEmpty()
                                    select new Shipments
                                    {
                                        Id = data.Id,
                                        JobId = data.JobNo,
                                        HBL = data.Hwbno,
                                        MBL = data.Mblno,
                                        HBLID = data.Hblid,
                                        Service = "CL",
                                        CustomNo = cus == null ? string.Empty : cus.ClearanceNo
                                    };

            IQueryable<Shipments> _shipmentsOperation = null;
            if (typeAdvFor == "HBL")
            {
                _shipmentsOperation = shipmentsOpsMerge.GroupBy(g => new { g.HBLID, g.CustomNo }).Select(s => s.FirstOrDefault());
            }
            else
            {
                _shipmentsOperation = shipmentsOpsMerge.GroupBy(g => new { g.MBL, g.HBLID }).Where(x => x.Count() > 1).Select(s => s.FirstOrDefault());
            }

            var transactions = DataContext.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED && x.IsLocked == false && x.OfficeId == currentUser.OfficeID);  // Lấy theo office current user
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
                HBLID = x.y.Id,
                Service = x.x.TransactionType,
                CustomNo = string.Empty
            });
            IQueryable<Shipments> _shipmentsDocumention = null;
            if (typeAdvFor == "HBL")
            {
                _shipmentsDocumention = shipmentsDocumention.GroupBy(g => g.HBLID).Select(s => s.FirstOrDefault());
            }
            else
            {
                _shipmentsDocumention = shipmentsDocumention.GroupBy(g => new { g.MBL, g.HBLID }).Where(x => x.Count() > 1).Select(s => s.FirstOrDefault());
            }

            var result = _shipmentsOperation.Union(_shipmentsDocumention);
            return result.OrderByDescending(o => o.JobId);
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
                // [CR: 15857]
                //queryTrans = q =>
                //    q.TransactionType.Contains("E") ?
                //    (q.Etd.HasValue ? q.Etd.Value.Date >= criteria.ServiceDateFrom.Value.Date && q.Etd.Value.Date <= criteria.ServiceDateTo.Value.Date : false)
                //    :
                //    (q.Eta.HasValue ? q.Eta.Value.Date >= criteria.ServiceDateFrom.Value.Date && q.Eta.Value.Date <= criteria.ServiceDateTo.Value.Date : false);
                queryTrans = q =>
                    q.ServiceDate.HasValue ? (criteria.ServiceDateFrom.Value.Date <= q.ServiceDate.Value.Date && q.ServiceDate.Value.Date <= criteria.ServiceDateTo.Value.Date) : false;
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
                queryTrans = queryTrans.And(q => criteria.JobId.Contains(q.JobNo));
            }
            // Search Mawb
            if (!string.IsNullOrEmpty(criteria.Mawb))
            {
                queryTrans = queryTrans.And(q => criteria.Mawb.Contains(q.Mawb));
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
                queryTranDetail = q => criteria.CustomerId.Contains(q.CustomerId);
            }
            // Search Hawb
            if (!string.IsNullOrEmpty(criteria.Hawb))
            {
                queryTranDetail = queryTranDetail == null ?
                    (q => criteria.Hawb.Contains(q.Hwbno))
                    :
                    queryTranDetail.And(q => criteria.Hawb.Contains(q.Hwbno));
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
            if (!string.IsNullOrEmpty(criteria.CustomerId) && fromCost == true)
            {
                queryOpsTrans = queryOpsTrans.And(q => criteria.CustomerId.Contains(q.CustomerId));
            }
            // Search JobId
            if (!string.IsNullOrEmpty(criteria.JobId))
            {
                queryOpsTrans = queryOpsTrans.And(q => criteria.JobId.Contains(q.JobNo));
            }
            // Search Mawb
            if (!string.IsNullOrEmpty(criteria.Mawb))
            {
                queryOpsTrans = queryOpsTrans.And(q => criteria.Mawb.Contains(q.Mblno));
            }
            // Search Hawb
            if (!string.IsNullOrEmpty(criteria.Hawb))
            {
                queryOpsTrans = queryOpsTrans.And(q => criteria.Hawb.Contains(q.Hwbno));
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
            //var dataOperation = GeneralExportOperationOverview(criteria);
            list = dataShipment;
            return list;
        }


        public IQueryable<GeneralExportShipmentOverviewResult> GeneralExportShipmentOverview(GeneralReportCriteria criteria)
        {
            List<GeneralExportShipmentOverviewResult> lstShipment = new List<GeneralExportShipmentOverviewResult>();
            var dataShipment = GetDataGeneralReport(criteria);
            if (!dataShipment.Any()) return lstShipment.AsQueryable();
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
            var UnitList = catUnitRepo.Get();
            var LookupUnitList = UnitList.ToLookup(x => x.Id);
            foreach (var item in dataShipment)
            {
                GeneralExportShipmentOverviewResult data = new GeneralExportShipmentOverviewResult();
                if (item.TransactionType == TermData.InlandTrucking)
                {
                    data.ServiceName = "Inland Trucking ";
                }
                if (item.TransactionType == TermData.AirExport)
                {
                    data.ServiceName = "Export (Air) ";
                }
                if (item.TransactionType == TermData.AirImport)
                {
                    data.ServiceName = "Import (Air) ";
                }
                if (item.TransactionType == TermData.SeaConsolExport)
                {
                    data.ServiceName = "Export (Sea Consol) ";
                }
                if (item.TransactionType == TermData.SeaConsolImport)
                {
                    data.ServiceName = "Import (Sea Consol) ";
                }
                if (item.TransactionType == TermData.SeaFCLExport)
                {
                    data.ServiceName = "Export (Sea FCL) ";
                }
                if (item.TransactionType == TermData.SeaFCLImport)
                {
                    data.ServiceName = "Import (Sea FCL) ";
                }
                if (item.TransactionType == TermData.SeaLCLExport)
                {
                    data.ServiceName = "Export (Sea LCL) ";
                }
                if (item.TransactionType == TermData.SeaLCLImport)
                {
                    data.ServiceName = "Import (Sea LCL) ";
                }
                if (item.TransactionType == "CL")
                {
                    data.ServiceName = API.Common.Globals.CustomData.Services.Where(x => x.Value == "CL").FirstOrDefault()?.DisplayName;
                }
                data.JobNo = item.JobNo;
                data.etd = item.Etd;
                data.eta = item.Eta;
                data.ServiceDate = item.ServiceDate;
                data.FlightNo = item.FlightNo;
                data.MblMawb = item.Mawb;
                data.HblHawb = item.HwbNo;
                string pol = (item.Pol != null && item.Pol != Guid.Empty) ? LookupPlace[(Guid)item.Pol].Select(t => t.Code).FirstOrDefault() : string.Empty;
                data.PolPod = (item.Pod != null && item.Pod != Guid.Empty) ? pol + "/" + LookupPlace[(Guid)item.Pod].Select(t => t.Code).FirstOrDefault() : pol;
                data.Carrier = !string.IsNullOrEmpty(item.ColoaderId) ? LookupPartner[item.ColoaderId].FirstOrDefault()?.ShortName : string.Empty;
                data.Agent = LookupPartner[item.AgentId].FirstOrDefault()?.ShortName;
                var ArrayShipperDesc = item.ShipperDescription?.Split("\n").ToArray();
                data.ShipperDescription = ArrayShipperDesc != null && ArrayShipperDesc.Length > 0 ? ArrayShipperDesc[0] : string.Empty;
                var ArrayConsgineeDesc = item.ConsigneeDescription?.Split("\n").ToArray();
                data.ConsigneeDescription = ArrayConsgineeDesc != null && ArrayConsgineeDesc.Length > 0 ? ArrayConsgineeDesc[0] : string.Empty;
                data.Consignee = !string.IsNullOrEmpty(data.ConsigneeDescription) ? data.ConsigneeDescription : LookupPartner[item.ConsigneeId].FirstOrDefault()?.PartnerNameEn;
                data.Shipper = !string.IsNullOrEmpty(data.ShipperDescription) ? data.ShipperDescription : LookupPartner[item.Shipper].FirstOrDefault()?.PartnerNameEn;
                data.ShipmentType = item.ShipmentType;
                data.Salesman = !string.IsNullOrEmpty(item.SalemanId) ? LookupUser[item.SalemanId].FirstOrDefault()?.Username : string.Empty;
                data.AgentName = LookupPartner[item.AgentId].FirstOrDefault()?.PartnerNameVn;
                data.GW = item.GrossWeight;
                data.CW = item.ChargeWeight;
                data.CBM = item.Cbm;

                data.Cont20 = item.Cont20 ?? 0;
                data.Cont40 = item.Cont40 ?? 0;
                data.Cont40HC = item.Cont40HC ?? 0;
                data.Cont45 = item.Cont45 ?? 0;

                data.QTy = item.PackageQty.ToString();
                #region -- Phí Selling trước thuế --
                decimal? _totalSellAmountFreight = 0;
                decimal? _totalSellAmountTrucking = 0;
                decimal? _totalSellAmountHandling = 0;
                decimal? _totalSellAmountOther = 0;
                decimal? _totalSellCustom = 0;
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
                        else if (ChargeGroupModel?.Name == "Trucking")
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
                        else if (ChargeGroupModel?.Name == "Handling")
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
                        // bổ sung total custom sell
                        else if (chargeObj.Type == "DEBIT" && ChargeGroupModel?.Name == "Logistics")
                        {
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalSellCustom += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellCustom += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        else
                        {
                            // Amount other = sum các phí debit có charge group khác Freight,Trucking,handling,Logistics
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
                data.TotalCustomSell = _totalSellCustom;
                #region [CR: 19/01/22] Alex bo rule nay
                //if (data.TotalSellOthers > 0 && data.TotalCustomSell > 0)
                //{
                //    data.TotalSellOthers = data.TotalSellOthers - data.TotalCustomSell;
                //}
                #endregion
                data.TotalSell = data.TotalSellFreight + data.TotalSellTrucking + data.TotalSellHandling + data.TotalSellOthers + data.TotalCustomSell;
                #endregion
                #region -- Phí Buying trước thuế --
                decimal? _totalBuyAmountFreight = 0;
                decimal? _totalBuyAmountTrucking = 0;
                decimal? _totalBuyAmountHandling = 0;
                decimal? _totalBuyAmountOther = 0;
                decimal? _totalBuyAmountKB = 0;
                decimal? _totalBuyCustom = 0;
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
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountFreight += charge.AmountUsd; // Phí Selling trước thuế
                            }
                            else
                            {
                                _totalBuyAmountFreight += charge.AmountVnd;
                            }
                        }
                        else if (ChargeGroupModel?.Name == "Trucking")
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
                        else if (ChargeGroupModel?.Name == "Handling")
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
                        else if (charge.KickBack == true || ChargeGroupModel?.Name == "Com")
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
                        else if (chargeObj.Type == "CREDIT" && ChargeGroupModel?.Name == "Logistics") // bổ sung total custom buy
                        {
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyCustom += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyCustom += charge.AmountVnd; // Phí buying trước thuế
                            }
                        }
                        // Amount other = sum các phí có charge group khác Freight,Trucking,handling,Logistics,com
                        //if (ChargeGroupModel?.Name != "Handling" && ChargeGroupModel?.Name != "Trucking" && ChargeGroupModel?.Name != "Freight" && ChargeGroupModel?.Name != "Com" && ChargeGroupModel?.Name != "Logistics" && charge.KickBack != true)
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
                        //END BUY
                    }
                }

                data.TotalBuyFreight = _totalBuyAmountFreight;
                data.TotalBuyTrucking = _totalBuyAmountTrucking;
                data.TotalBuyHandling = _totalBuyAmountHandling;
                data.TotalBuyOthers = _totalBuyAmountOther;
                data.TotalBuyKB = _totalBuyAmountKB;
                data.TotalCustomBuy = _totalBuyCustom;
                data.TotalBuy = data.TotalBuyFreight + data.TotalBuyTrucking + data.TotalBuyHandling + data.TotalBuyOthers + data.TotalBuyKB + data.TotalCustomBuy;
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
                var OfficeSaleman = LookupUserLevelList[item.SalemanId].Select(t => t.OfficeId).FirstOrDefault();
                data.SalesOffice = OfficeSaleman != Guid.Empty && OfficeSaleman != null ? LookupOffice[(Guid)OfficeSaleman].Select(t => t.Code).FirstOrDefault() : string.Empty;
                data.Creator = item.TransactionType == "CL" ? LookupUser[item.PersonInCharge].Select(t => t.Username).FirstOrDefault() : LookupUser[item.UserCreated].Select(t => t.Username).FirstOrDefault();
                data.POINV = item.Pono;
                data.Commodity = item.Commodity;
                data.ProductService = item.ProductService;
                data.ServiceMode = string.Empty;//chua co thong tin
                data.PMTerm = item.PaymentTerm;
                data.ShipmentNotes = item.Notes;
                data.Created = item.DatetimeCreated;
                data.CustomerId = LookupPartner[item.CustomerId].Select(t => t.AccountNo).FirstOrDefault();
                data.CustomerName = LookupPartner[item.CustomerId].Select(t => t.ShortName).FirstOrDefault();
                string Code = item.PackageType != null ? LookupUnitList[(short)item.PackageType].Select(t => t.Code).FirstOrDefault() : string.Empty;
                data.QTy = item.PackageQty.ToString() + " " + Code;
                data.CustomNo = item.TransactionType == "CL" ? GetCustomNoOldOfShipment(item.JobNo) : string.Empty;
                data.BKRefNo = item.BookingNo;
                data.ReferenceNo = item.ReferenceNo;
                lstShipment.Add(data);
            }
            return lstShipment.AsQueryable();
        }

        public IQueryable<GeneralExportShipmentOverviewFCLResult> GetDataGeneralExportShipmentOverviewFCL(GeneralReportCriteria criteria)
        {
            IQueryable<GeneralExportShipmentOverviewFCLResult> list;
            var dataShipment = GeneralExportShipmentOverviewFCL(criteria);
            list = dataShipment;
            return list;
        }

        public IQueryable<GeneralExportShipmentOverviewFCLResult> GeneralExportShipmentOverviewFCL(GeneralReportCriteria criteria)
        {
            List<GeneralExportShipmentOverviewFCLResult> lstShipment = new List<GeneralExportShipmentOverviewFCLResult>();
            var dataShipment = GetDataGeneralReport(criteria);
            if (!dataShipment.Any()) return lstShipment.AsQueryable();
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
            var UnitList = catUnitRepo.Get();
            var LookupUnitList = UnitList.ToLookup(x => x.Id);
            foreach (var item in dataShipment)
            {
                GeneralExportShipmentOverviewFCLResult data = new GeneralExportShipmentOverviewFCLResult();
                if (item.TransactionType == TermData.InlandTrucking)
                {
                    data.ServiceName = "Inland Trucking ";
                }
                if (item.TransactionType == TermData.AirExport)
                {
                    data.ServiceName = "Export (Air) ";
                }
                if (item.TransactionType == TermData.AirImport)
                {
                    data.ServiceName = "Import (Air) ";
                }
                if (item.TransactionType == TermData.SeaConsolExport)
                {
                    data.ServiceName = "Export (Sea Consol) ";
                }
                if (item.TransactionType == TermData.SeaConsolImport)
                {
                    data.ServiceName = "Import (Sea Consol) ";
                }
                if (item.TransactionType == TermData.SeaFCLExport)
                {
                    data.ServiceName = "Export (Sea FCL) ";
                }
                if (item.TransactionType == TermData.SeaFCLImport)
                {
                    data.ServiceName = "Import (Sea FCL) ";
                }
                if (item.TransactionType == TermData.SeaLCLExport)
                {
                    data.ServiceName = "Export (Sea LCL) ";
                }
                if (item.TransactionType == TermData.SeaLCLImport)
                {
                    data.ServiceName = "Import (Sea LCL) ";
                }
                if (item.TransactionType == "CL")
                {
                    data.ServiceName = API.Common.Globals.CustomData.Services.Where(x => x.Value == "CL").FirstOrDefault()?.DisplayName;
                }
                data.JobNo = item.JobNo;
                data.ServiceDate = item.ServiceDate;
                data.etd = item.Etd;
                data.eta = item.Eta;
                data.FlightNo = item.FlightNo;
                data.MblMawb = item.Mawb;
                data.HblHawb = item.HwbNo;
                string pol = (item.Pol != null && item.Pol != Guid.Empty) ? LookupPlace[(Guid)item.Pol].Select(t => t.Code).FirstOrDefault() : string.Empty;
                data.PolPod = (item.Pod != null && item.Pod != Guid.Empty) ? pol + "/" + LookupPlace[(Guid)item.Pod].Select(t => t.Code).FirstOrDefault() : pol;
                data.Carrier = !string.IsNullOrEmpty(item.ColoaderId) ? LookupPartner[item.ColoaderId].FirstOrDefault()?.ShortName : string.Empty;
                data.Agent = LookupPartner[item.AgentId].FirstOrDefault()?.ShortName;
                var ArrayShipperDesc = item.ShipperDescription?.Split("\n").ToArray();
                data.ShipperDescription = ArrayShipperDesc != null && ArrayShipperDesc.Length > 0 ? ArrayShipperDesc[0] : string.Empty;
                var ArrayConsgineeDesc = item.ConsigneeDescription?.Split("\n").ToArray();
                data.ConsigneeDescription = ArrayConsgineeDesc != null && ArrayConsgineeDesc.Length > 0 ? ArrayConsgineeDesc[0] : string.Empty;
                data.Consignee = !string.IsNullOrEmpty(data.ConsigneeDescription) ? data.ConsigneeDescription : LookupPartner[item.ConsigneeId].FirstOrDefault()?.PartnerNameEn;
                data.Shipper = !string.IsNullOrEmpty(data.ShipperDescription) ? data.ShipperDescription : LookupPartner[item.Shipper].FirstOrDefault()?.PartnerNameEn;
                data.ShipmentType = item.ServiceType;
                data.Salesman = !string.IsNullOrEmpty(item.SalemanId) ? LookupUser[item.SalemanId].FirstOrDefault()?.Username : string.Empty;

                var ArrNotifyPartyDesc = item.NotifyPartyDescription?.Split("\n").ToArray();
                data.AgentName = ArrNotifyPartyDesc != null && ArrNotifyPartyDesc.Length > 0 ? ArrNotifyPartyDesc[0] : string.Empty;

                data.GW = item.GrossWeight;
                data.CW = item.ChargeWeight;
                data.CBM = item.Cbm;

                data.Cont20 = item.Cont20 ?? 0;
                data.Cont40 = item.Cont40 ?? 0;
                data.Cont40HC = item.Cont40HC ?? 0;
                data.Cont45 = item.Cont45 ?? 0;

                data.QTy = item.PackageQty.ToString();
                #region -- Phí Selling trước thuế --
                decimal? _totalSellAmountFreight = 0;
                decimal? _totalSellAmountTermial = 0;
                decimal? _totalSellAmountBillFee = 0;
                decimal? _totalSellAmountSealFee = 0;
                decimal? _totalSellAmountTelex = 0;
                decimal? _totalSellAmountAutomated = 0;
                decimal? _totalSellAmountVGM = 0;
                decimal? _totalSellAmountBooking = 0;
                decimal? _totalSellAmountOther = 0;

                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeSell = detailLookupSur[(Guid)item.HblId].Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE);
                    foreach (var charge in _chargeSell)
                    {
                        var chargeObj = LookupCharge[charge.ChargeId].Select(t => t).FirstOrDefault();
                        CatChargeGroup ChargeGroupModel = new CatChargeGroup();
                        ChargeGroupModel = charge.ChargeGroup != null && charge.ChargeGroup != Guid.Empty ? ChargeGroupLookup[(Guid)charge.ChargeGroup].FirstOrDefault() : null;
                        bool isOtherSell = true;
                        if (ChargeGroupModel == null)
                        {
                            ChargeGroupModel = chargeObj.ChargeGroup != null && chargeObj.ChargeGroup != Guid.Empty ? ChargeGroupLookup[(Guid)chargeObj.ChargeGroup].FirstOrDefault() : null;
                        }
                        // tinh total phi chargeGroup freight
                        if (ChargeGroupModel?.Name == "Freight")
                        {
                            isOtherSell = false;
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountFreight += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountFreight += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains Terminal
                        if (chargeObj.ChargeNameEn.ToLower().Equals("terminal handling") || chargeObj.ChargeNameEn.ToLower().Equals("THC"))
                        {
                            isOtherSell = false;
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountTermial += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountTermial += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains bill fee
                        if (chargeObj.ChargeNameEn.ToLower().Equals("bill fee"))
                        {
                            isOtherSell = false;
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountBillFee += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountBillFee += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains seal fee
                        if (chargeObj.ChargeNameEn.ToLower().Equals("seal fee"))
                        {
                            isOtherSell = false;
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountSealFee += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountSealFee += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains telex release
                        if (chargeObj.ChargeNameEn.ToLower().Equals("telex release"))
                        {
                            isOtherSell = false;
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountTelex += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountTelex += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains automated manifest
                        if (chargeObj.ChargeNameEn.ToLower().Equals("automated manifest") || chargeObj.ChargeNameEn.ToLower().Equals("AMS"))
                        {
                            isOtherSell = false;
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountAutomated += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountAutomated += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains vgm admin
                        if (chargeObj.ChargeNameEn.ToLower().Equals("vgm admin"))
                        {
                            isOtherSell = false;
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountVGM += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountVGM += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains booking fee
                        if (chargeObj.ChargeNameEn.ToLower().Equals("booking fee"))
                        {
                            isOtherSell = false;
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalSellAmountBooking += charge.AmountUsd;
                            }
                            else
                            {
                                _totalSellAmountBooking += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        if(isOtherSell == true)
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
                data.TotalSellTerminal = _totalSellAmountTermial;
                data.TotalSellContainerSealFee = _totalSellAmountSealFee;
                data.TotalSellTelexRelease = _totalSellAmountTelex;
                data.TotalSellAutomated = _totalSellAmountAutomated;
                data.TotalSellVGM = _totalSellAmountVGM;
                data.TotalSellBookingFee = _totalSellAmountBooking;
                data.TotalSellOthers = _totalSellAmountOther;
                data.TotalSell = data.TotalSellFreight + data.TotalSellTerminal + data.TotalSellContainerSealFee + data.TotalSellTelexRelease + data.TotalSellAutomated + data.TotalSellVGM + data.TotalSellBookingFee + data.TotalSellOthers;
                #endregion
                #region -- Phí Buying trước thuế --
                decimal? _totalBuyAmountFreight = 0;
                decimal? _totalBuyAmountTermial = 0;
                decimal? _totalBuyAmountBillFee = 0;
                decimal? _totalBuyAmountSealFee = 0;
                decimal? _totalBuyAmountTelex = 0;
                decimal? _totalBuyAmountAutomated = 0;
                decimal? _totalBuyAmountVGM = 0;
                decimal? _totalBuyAmountBooking = 0;
                decimal? _totalBuyAmountOther = 0;
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeBuy = detailLookupSur[(Guid)item.HblId].Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE);
                    foreach (var charge in _chargeBuy)
                    {
                        var chargeObj = LookupCharge[charge.ChargeId].Select(t => t).FirstOrDefault();
                        CatChargeGroup ChargeGroupModel = new CatChargeGroup();
                        ChargeGroupModel = charge.ChargeGroup != null && charge.ChargeGroup != Guid.Empty ? ChargeGroupLookup[(Guid)charge.ChargeGroup].FirstOrDefault() : null;
                        bool isOther = true;
                        if (ChargeGroupModel == null)
                        {
                            ChargeGroupModel = chargeObj.ChargeGroup != null && chargeObj.ChargeGroup != Guid.Empty ? ChargeGroupLookup[(Guid)chargeObj.ChargeGroup].FirstOrDefault() : null;
                        }
                        // tinh total phi chargeGroup freight
                        if (ChargeGroupModel?.Name == "Freight")
                        {
                            isOther = false;
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountFreight += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountFreight += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains Terminal
                        if (chargeObj.ChargeNameEn.ToLower().Equals("terminal handling") || chargeObj.ChargeNameEn.ToLower().Equals("THC"))
                        {
                            isOther = false;
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountTermial += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountTermial += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains bill fee
                        if (chargeObj.ChargeNameEn.ToLower().Equals("bill fee"))
                        {
                            isOther = false;
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountBillFee += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountBillFee += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains seal fee
                        if (chargeObj.ChargeNameEn.ToLower().Equals("seal fee"))
                        {
                            isOther = false;
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountSealFee += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountSealFee += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains telex release
                        if (chargeObj.ChargeNameEn.ToLower().Equals("telex release"))
                        {
                            isOther = false;
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountTelex += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountTelex += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains automated manifest
                        if (chargeObj.ChargeNameEn.ToLower().Equals("automated manifest") || chargeObj.ChargeNameEn.ToLower().Equals("AMS"))
                        {
                            isOther = false;
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountAutomated += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountAutomated += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains vgm admin
                        if (chargeObj.ChargeNameEn.ToLower().Equals("vgm admin"))
                        {
                            isOther = false;
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountVGM += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountVGM += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        // ChargeName contains booking fee
                        if (chargeObj.ChargeNameEn.ToLower().Equals("booking fee"))
                        {
                            isOther = false;
                            if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                            {
                                _totalBuyAmountBooking += charge.AmountUsd;
                            }
                            else
                            {
                                _totalBuyAmountBooking += charge.AmountVnd; // Phí Selling trước thuế
                            }
                        }
                        if (isOther == true)
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
                        //END BUY
                    }
                }

                data.TotalBuyFreight = _totalBuyAmountFreight;
                data.TotalBuyTerminal = _totalBuyAmountTermial;
                data.TotalBuyContainerSealFee = _totalBuyAmountSealFee;
                data.TotalBuyTelexRelease = _totalBuyAmountTelex;
                data.TotalBuyAutomated = _totalBuyAmountAutomated;
                data.TotalBuyVGM = _totalBuyAmountVGM;
                data.TotalBuyBookingFee = _totalBuyAmountBooking;
                data.TotalBuyOthers = _totalBuyAmountOther;
                data.TotalBuy = data.TotalBuyFreight + data.TotalBuyTerminal + data.TotalBuyContainerSealFee + data.TotalBuyTelexRelease + data.TotalBuyAutomated + data.TotalBuyVGM + data.TotalBuyBookingFee + data.TotalBuyOthers;
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
                var OfficeSaleman = LookupUserLevelList[item.SalemanId].Select(t => t.OfficeId).FirstOrDefault();
                data.SalesOffice = OfficeSaleman != Guid.Empty && OfficeSaleman != null ? LookupOffice[(Guid)OfficeSaleman].Select(t => t.Code).FirstOrDefault() : string.Empty;
                data.Creator = item.TransactionType == "CL" ? LookupUser[item.PersonInCharge].Select(t => t.Username).FirstOrDefault() : LookupUser[item.UserCreated].Select(t => t.Username).FirstOrDefault();
                data.POINV = item.Pono;
                data.Commodity = item.Commodity;
                data.ProductService = item.ProductService;
                data.ServiceMode = string.Empty;//chua co thong tin
                data.PMTerm = item.PaymentTerm;
                data.ShipmentNotes = item.Notes;
                data.Created = item.DatetimeCreated;
                data.CustomerId = LookupPartner[item.CustomerId].Select(t => t.AccountNo).FirstOrDefault();
                data.CustomerName = LookupPartner[item.CustomerId].Select(t => t.ShortName).FirstOrDefault();
                string Code = item.PackageType != null ? LookupUnitList[(short)item.PackageType].Select(t => t.Code).FirstOrDefault() : string.Empty;
                data.QTy = item.PackageQty.ToString() + " " + Code;
                data.CustomNo = item.TransactionType == "CL" ? GetCustomNoOldOfShipment(item.JobNo) : string.Empty;
                data.BKRefNo = item.ReferenceNo;
                data.ReferenceNo = item.ReferenceNo;
                data.FinalDestination = item.FinalDestination;
                lstShipment.Add(data);
            }
            return lstShipment.AsQueryable();
        }

        /// <summary>
        /// Get data for shipment overview LCL
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public IQueryable<GeneralExportShipmentOverviewFCLResult> GetDataGeneralExportShipmentOverviewLCL(GeneralReportCriteria criteria)
        {
            List<GeneralExportShipmentOverviewFCLResult> lstShipment = new List<GeneralExportShipmentOverviewFCLResult>();
            var dataShipment = GetDataGeneralReport(criteria);
            if (!dataShipment.Any()) return null;
            var shipments = dataShipment.OrderBy(x => x.TransactionType).ThenBy(x => x.JobNo);
            var placesData = catPlaceRepo.Get().ToLookup(x => x.Id);
            var partnersData = catPartnerRepo.Get().ToLookup(x => x.Id);
            var usersData = sysUserRepo.Get().ToLookup(u => u.Id);
            var chargesData = surCharge.Get().ToLookup(u => u.Hblid);
            // Get id charges with filter
            var freightChargeId = catChargeGroupRepo.Get(x => x.Name.ToUpper().Contains("FREIGHT"))?.Select(x => x.Id).FirstOrDefault();
            var terminalHandling = catChargeRepo.Get(x => x.ChargeNameEn.ToUpper().Contains("TERMINAL HANDLING") || x.ChargeNameEn.ToUpper().Contains("THC"))?.Select(x => x.Id).ToList();
            var billFee = catChargeRepo.Get(x => x.ChargeNameEn.ToUpper().Contains("BILL FEE"))?.Select(x => x.Id).ToList();
            var telexRelease = catChargeRepo.Get(x => x.ChargeNameEn.ToUpper().Contains("TELEX RELEASE"))?.Select(x => x.Id).ToList();
            var cfsCharge = catChargeRepo.Get(x => x.ChargeNameEn.ToUpper().Contains("CFS"))?.Select(x => x.Id).ToList();
            var securityCharge = catChargeRepo.Get(x => x.ChargeNameEn.ToUpper().Contains("SECURITY") || x.ChargeNameEn.ToUpper().Contains("EBS"))?.Select(x => x.Id).ToList();
            var autoManCharge = catChargeRepo.Get(x => x.ChargeNameEn.ToUpper().Contains("AUTOMATED MANIFEST") || x.ChargeNameEn.ToUpper().Contains("AMS"))?.Select(x => x.Id).ToList();
            var vgmCharge = catChargeRepo.Get(x => x.ChargeNameEn.ToUpper().Contains("VGM"))?.Select(x => x.Id).ToList();
            var lclBookingFee = catChargeRepo.Get(x => x.ChargeNameEn.ToUpper().Contains("LCL BOOKING FEE"))?.Select(x => x.Id).ToList();
            var customFeeName = new string[] { "PICK-UP CHARGE", "CUSTOMS CLEARANCE", "CUSTOMS INSPECTION" };
            var customFee = catChargeRepo.Get(x => Array.Exists(customFeeName, element => x.ChargeNameEn.ToUpper().Contains(element)))?.Select(x => x.Id).ToList();

            foreach (var shipment in shipments)
            {
                var data = new GeneralExportShipmentOverviewFCLResult();
                data.ReferenceNo = shipment.ReferenceNo;
                data.ServiceName = CustomData.Services.Where(x => x.Value == shipment.TransactionType).Select(x => x.DisplayName).FirstOrDefault();
                data.JobNo = shipment.JobNo;
                data.ServiceDate = shipment.ServiceDate;
                data.etd = shipment.Etd;
                data.eta = shipment.Eta;
                data.FlightNo = shipment.FlightNo;
                data.MblMawb = shipment.Mawb;
                data.HblHawb = shipment.HwbNo;
                data.Incoterm = shipment.IncotermId == null ? string.Empty : catIncotermRepository.Get(x => x.Id == shipment.IncotermId).Select(x => x.Code).FirstOrDefault();
                if (shipment.Pol != null && shipment.Pod != null)
                {
                    data.PolPod = placesData[(Guid)shipment.Pol]?.Select(x => x.Code).FirstOrDefault() + "/" + placesData[(Guid)shipment.Pod]?.Select(x => x.Code).FirstOrDefault();
                }
                else
                {
                    if (!(shipment.Pol == null && shipment.Pod == null))
                    {
                        data.PolPod = shipment.Pol == null ? placesData[(Guid)shipment.Pod]?.Select(x => x.Code).FirstOrDefault() :
                                                                            placesData[(Guid)shipment.Pol]?.Select(x => x.Code).FirstOrDefault();
                    }
                }
                data.FinalDestination = shipment.FinalDestination;
                data.Carrier = partnersData[shipment.ColoaderId]?.Select(x => x.ShortName).FirstOrDefault();
                data.AgentName = partnersData[shipment.AgentId]?.Select(x => x.PartnerNameEn).FirstOrDefault();
                data.Shipper = shipment.ShipperDescription?.Split('\n').FirstOrDefault();
                data.Consignee = shipment.ConsigneeDescription?.Split('\n').FirstOrDefault();
                data.ShipmentType = shipment.ShipmentType;
                data.Salesman = usersData[shipment.SalemanId]?.Select(x => x.Username).FirstOrDefault();
                data.NotifyParty = shipment.NotifyPartyDescription?.Split('\n').FirstOrDefault();
                data.PackageQty = shipment.PackageQty;
                data.Cont20 = shipment.Cont20 ?? 0;
                data.Cont40 = shipment.Cont40 ?? 0;
                data.Cont40HC = shipment.Cont40HC ?? 0;
                data.Cont45 = shipment.Cont45 ?? 0;
                data.GW = shipment.GrossWeight;
                data.CW = shipment.ChargeWeight;
                data.CBM = shipment.Cbm;
                var surChargesShipment = chargesData[(Guid)shipment.HblId];
                data.TotalBuy = 0;
                data.TotalSell = 0;
                data.Profit = 0;
                if (criteria.Currency == DocumentConstants.CURRENCY_LOCAL)
                {
                    #region -- currency = VND
                    // Freight
                    var _freightCharges = surChargesShipment.Where(x => x.ChargeGroup == freightChargeId);
                    if (_freightCharges?.Count() > 0)
                    {
                        data.TotalBuyFreight = _freightCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSellFreight = _freightCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalBuy += data.TotalBuyFreight;
                        data.TotalSell += data.TotalSellFreight;
                    }
                    // Terminal Handling (THC)
                    var _terminalHandlingCharges = surChargesShipment.Where(x => terminalHandling.Contains(x.ChargeId));
                    if (_terminalHandlingCharges?.Count() > 0)
                    {
                        data.TotalBuyTerminal = _terminalHandlingCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSellTerminal = _terminalHandlingCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalBuy += data.TotalBuyTerminal;
                        data.TotalSell += data.TotalSellTerminal;
                    }
                    // Origin Doc Fee (B/L)
                    var _billFeeCharges = surChargesShipment.Where(x => billFee.Contains(x.ChargeId));
                    if (_billFeeCharges?.Count() > 0)
                    {
                        data.TotalBuyBillFee = _billFeeCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSellBillFee = _billFeeCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalBuy += data.TotalBuyBillFee;
                        data.TotalSell += data.TotalSellBillFee;
                    }
                    // Telex release Fee
                    var _telexReleaseCharges = surChargesShipment.Where(x => telexRelease.Contains(x.ChargeId));
                    if (_telexReleaseCharges?.Count() > 0)
                    {
                        data.TotalBuyTelexRelease = _telexReleaseCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSellTelexRelease = _telexReleaseCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalBuy += data.TotalBuyTelexRelease;
                        data.TotalSell += data.TotalSellTelexRelease;
                    }
                    // Origin Container Freight Station Fee (CFS)
                    var _cfsCharges = surChargesShipment.Where(x => cfsCharge.Contains(x.ChargeId));
                    if (_cfsCharges?.Count() > 0)
                    {
                        data.TotalBuyCFSFee = _cfsCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSellCFSFee = _cfsCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalBuy += data.TotalBuyCFSFee;
                        data.TotalSell += data.TotalSellCFSFee;
                    }
                    // Security (EBS)
                    var _securityCharges = surChargesShipment.Where(x => securityCharge.Contains(x.ChargeId));
                    if (_securityCharges?.Count() > 0)
                    {
                        data.TotalBuyEBSFee = _securityCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSellEBSFee = _securityCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalBuy += data.TotalBuyEBSFee;
                        data.TotalSell += data.TotalSellEBSFee;
                    }
                    // Automated Manifest System (AMS)
                    var _autoManCharges = surChargesShipment.Where(x => autoManCharge.Contains(x.ChargeId));
                    if (_autoManCharges?.Count() > 0)
                    {
                        data.TotalBuyAutomated = _autoManCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSellAutomated = _autoManCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalBuy += data.TotalBuyAutomated;
                        data.TotalSell += data.TotalSellAutomated;
                    }
                    // VGM admin Fee
                    var _vgmCharges = surChargesShipment.Where(x => vgmCharge.Contains(x.ChargeId));
                    if (_vgmCharges?.Count() > 0)
                    {
                        data.TotalBuyVGM = _vgmCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSellVGM = _vgmCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalBuy += data.TotalBuyVGM;
                        data.TotalSell += data.TotalSellVGM;
                    }
                    // Booking fee - LCL
                    var _lclBookingFeeCharges = surChargesShipment.Where(x => lclBookingFee.Contains(x.ChargeId));
                    if (_lclBookingFeeCharges?.Count() > 0)
                    {
                        data.TotalSellBookingFee = _lclBookingFeeCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSell += data.TotalSellBookingFee;
                    }
                    // Pick up charge + Customs fee
                    var _customFeeCharges = surChargesShipment.Where(x => customFee.Contains(x.ChargeId));
                    if (_customFeeCharges?.Count() > 0)
                    {
                        data.TotalBuyCustomFee = _customFeeCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalSellCustomFee = _customFeeCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                        data.TotalBuy += data.TotalBuyCustomFee;
                        data.TotalSell += data.TotalSellCustomFee;
                    }
                    // OTHERS Fee
                    var _lclOtherCharges = surChargesShipment.Except(_freightCharges)
                                                                            .Except(_terminalHandlingCharges)
                                                                            .Except(_billFeeCharges)
                                                                            .Except(_telexReleaseCharges)
                                                                            .Except(_cfsCharges)
                                                                            .Except(_securityCharges)
                                                                            .Except(_autoManCharges)
                                                                            .Except(_vgmCharges)
                                                                            .Except(_customFeeCharges);
                    // Total Others Fee Of Buying
                    data.TotalBuyOthers = _lclOtherCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                    // Total Others Fee Of Selling
                    _lclOtherCharges = _lclOtherCharges.Except(_lclBookingFeeCharges);
                    data.TotalSellOthers = _lclOtherCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountVnd ?? 0));
                    // TOTAL
                    data.TotalBuy += data.TotalBuyOthers;
                    data.TotalSell += data.TotalSellOthers;
                    data.Profit = data.TotalSell - data.TotalBuy;
                    #endregion
                }
                else
                {
                    #region -- currency = USD
                    // Freight
                    var _freightCharges = surChargesShipment.Where(x => x.ChargeGroup == freightChargeId);
                    if (_freightCharges?.Count() > 0)
                    {
                        data.TotalBuyFreight = _freightCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSellFreight = _freightCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalBuy += data.TotalBuyFreight;
                        data.TotalSell += data.TotalSellFreight;
                    }
                    // Terminal Handling (THC)
                    var _terminalHandlingCharges = surChargesShipment.Where(x => terminalHandling.Contains(x.ChargeId));
                    if (_terminalHandlingCharges?.Count() > 0)
                    {
                        data.TotalBuyTerminal = _terminalHandlingCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSellTerminal = _terminalHandlingCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalBuy += data.TotalBuyTerminal;
                        data.TotalSell += data.TotalSellTerminal;
                    }
                    // Origin Doc Fee (B/L)
                    var _billFeeCharges = surChargesShipment.Where(x => billFee.Contains(x.ChargeId));
                    if (_billFeeCharges?.Count() > 0)
                    {
                        data.TotalBuyBillFee = _billFeeCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSellBillFee = _billFeeCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalBuy += data.TotalBuyBillFee;
                        data.TotalSell += data.TotalSellBillFee;
                    }
                    // Telex release Fee
                    var _telexReleaseCharges = surChargesShipment.Where(x => telexRelease.Contains(x.ChargeId));
                    if (_telexReleaseCharges?.Count() > 0)
                    {
                        data.TotalBuyTelexRelease = _telexReleaseCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSellTelexRelease = _telexReleaseCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalBuy += data.TotalBuyTelexRelease;
                        data.TotalSell += data.TotalSellTelexRelease;
                    }
                    // Origin Container Freight Station Fee (CFS)
                    var _cfsCharges = surChargesShipment.Where(x => cfsCharge.Contains(x.ChargeId));
                    if (_cfsCharges?.Count() > 0)
                    {
                        data.TotalBuyCFSFee = _cfsCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSellCFSFee = _cfsCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalBuy += data.TotalBuyCFSFee;
                        data.TotalSell += data.TotalSellCFSFee;
                    }
                    // Security (EBS)
                    var _securityCharges = surChargesShipment.Where(x => securityCharge.Contains(x.ChargeId));
                    if (_securityCharges?.Count() > 0)
                    {
                        data.TotalBuyEBSFee = _securityCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSellEBSFee = _securityCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalBuy += data.TotalBuyEBSFee;
                        data.TotalSell += data.TotalSellEBSFee;
                    }
                    // Automated Manifest System (AMS)
                    var _autoManCharges = surChargesShipment.Where(x => autoManCharge.Contains(x.ChargeId));
                    if (_autoManCharges?.Count() > 0)
                    {
                        data.TotalBuyAutomated = _autoManCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSellAutomated = _autoManCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalBuy += data.TotalBuyAutomated;
                        data.TotalSell += data.TotalSellAutomated;
                    }
                    // VGM admin Fee
                    var _vgmCharges = surChargesShipment.Where(x => vgmCharge.Contains(x.ChargeId));
                    if (_vgmCharges?.Count() > 0)
                    {
                        data.TotalBuyVGM = _vgmCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSellVGM = _vgmCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalBuy += data.TotalBuyVGM;
                        data.TotalSell += data.TotalSellVGM;
                    }
                    // Booking fee - LCL
                    var _lclBookingFeeCharges = surChargesShipment.Where(x => lclBookingFee.Contains(x.ChargeId));
                    if (_lclBookingFeeCharges?.Count() > 0)
                    {
                        data.TotalSellBookingFee = _lclBookingFeeCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSell += data.TotalSellBookingFee;
                    }
                    // Pick up charge + Customs fee
                    var _customFeeCharges = surChargesShipment.Where(x => customFee.Contains(x.ChargeId));
                    if (_customFeeCharges?.Count() > 0)
                    {
                        data.TotalBuyCustomFee = _customFeeCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalSellCustomFee = _customFeeCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                        data.TotalBuy += data.TotalBuyCustomFee;
                        data.TotalSell += data.TotalSellCustomFee;
                    }
                    // OTHERS Fee
                    var _lclOtherCharges = surChargesShipment.Except(_freightCharges)
                                                                            .Except(_terminalHandlingCharges)
                                                                            .Except(_billFeeCharges)
                                                                            .Except(_telexReleaseCharges)
                                                                            .Except(_cfsCharges)
                                                                            .Except(_securityCharges)
                                                                            .Except(_autoManCharges)
                                                                            .Except(_vgmCharges)
                                                                            .Except(_customFeeCharges);
                    // Total Others Fee Of Buying
                    data.TotalBuyOthers = _lclOtherCharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                    // Total Others Fee Of Selling
                    _lclOtherCharges = _lclOtherCharges.Except(_lclBookingFeeCharges);
                    data.TotalSellOthers = _lclOtherCharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE)?.Sum(x => (x.AmountUsd ?? 0));
                    // TOTAL
                    data.TotalBuy += data.TotalBuyOthers;
                    data.TotalSell += data.TotalSellOthers;
                    data.Profit = data.TotalSell - data.TotalBuy;
                    #endregion
                }
                #region -- Phí OBH sau thuế --
                decimal? _obh = 0;
                var surChargesObh = surChargesShipment.Where(x => x.Type == DocumentConstants.CHARGE_OBH_TYPE);
                foreach (var charge in surChargesObh)
                {
                    _obh += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, criteria.Currency);
                }

                data.AmountOBH = _obh;
                #endregion -- Phí OBH sau thuế --
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
            //if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains(TermData.CustomLogistic))
            //{
            //    var dataOperation = GeneralReportOperation(criteria);
            //    list = dataDocumentation.Union(dataOperation);
            //}
            //else
            //{
            //    list = dataDocumentation;
            //}
            list = dataDocumentation;
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
            //if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains(TermData.CustomLogistic))
            //{
            //    var dataOperation = GeneralReportOperation(criteria);
            //    list = dataDocumentation.Union(dataOperation);
            //}
            //else
            //{
            //    list = dataDocumentation;
            //}
            list = dataDocumentation;
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
            Expression<Func<OpsTransaction, bool>> query = GetQueryOPSTransactionOperation(criteria, true);

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

        private List<sp_GetDataGeneralReport> GetDataGeneralReport(GeneralReportCriteria criteria)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@serviceDateFrom", Value = criteria.ServiceDateFrom },
                new SqlParameter(){ ParameterName = "@serviceDateTo", Value = criteria.ServiceDateTo },
                new SqlParameter(){ ParameterName = "@createdDateFrom", Value = criteria.CreatedDateFrom },
                new SqlParameter(){ ParameterName = "@createdDateTo", Value = criteria.CreatedDateTo },
                new SqlParameter(){ ParameterName = "@customerId", Value = criteria.CustomerId },
                new SqlParameter(){ ParameterName = "@service", Value = criteria.Service },
                new SqlParameter(){ ParameterName = "@currency", Value = criteria.Currency },
                new SqlParameter(){ ParameterName = "@jobId", Value = criteria.JobId },
                new SqlParameter(){ ParameterName = "@mawb", Value = criteria.Mawb },
                new SqlParameter(){ ParameterName = "@hawb", Value = criteria.Hawb },
                new SqlParameter(){ ParameterName = "@officeId", Value = criteria.OfficeId },
                new SqlParameter(){ ParameterName = "@departmentId", Value = criteria.DepartmentId },
                new SqlParameter(){ ParameterName = "@groupId", Value = criteria.GroupId },
                new SqlParameter(){ ParameterName = "@personalInCharge", Value = criteria.PersonInCharge },
                new SqlParameter(){ ParameterName = "@salesMan", Value = criteria.SalesMan },
                new SqlParameter(){ ParameterName = "@creator", Value = criteria.Creator },
                new SqlParameter(){ ParameterName = "@carrierId", Value = criteria.CarrierId },
                new SqlParameter(){ ParameterName = "@agentId", Value = criteria.AgentId },
                new SqlParameter(){ ParameterName = "@pol", Value = criteria.Pol },
                new SqlParameter(){ ParameterName = "@pod", Value = criteria.Pod }
            };
            var list = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetDataGeneralReport>(parameters);
            return list;
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
                                        Service = master.TransactionType,
                                        ChargeWeight = house.ChargeWeight
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
                                        Service = master.TransactionType,
                                        ChargeWeight = house.ChargeWeight
                                    };
                return queryShipment;
            }

        }

        private IQueryable<GeneralReportResult> GeneralReportDocumentation(GeneralReportCriteria criteria)
        {
            //var dataShipment = QueryDataDocumentation(criteria);
            var dataShipment = GetDataGeneralReport(criteria);
            List<GeneralReportResult> dataList = new List<GeneralReportResult>();
            var LstSurcharge = surCharge.Get();
            var LookupSurchage = LstSurcharge.ToLookup(x => x.Hblid);
            var PartnerList = catPartnerRepo.Get();
            var LookupPartner = PartnerList.ToLookup(x => x.Id);
            var PlaceLookup = catPlaceRepo.Get().ToLookup(q => q.Id);
            var lookupUser = sysUserRepo.Get().ToLookup(q => q.Id);
            var lookupEmployee = sysEmployeeRepo.Get().ToLookup(q => q.Id);
            foreach (var item in dataShipment)
            {
                GeneralReportResult data = new GeneralReportResult();
                data.JobId = item.JobNo;
                data.Mawb = item.Mawb;
                data.Hawb = item.HwbNo;
                data.CustomerName = LookupPartner[item.CustomerId].FirstOrDefault()?.PartnerNameEn;
                data.CarrierName = LookupPartner[item.ColoaderId].FirstOrDefault()?.PartnerNameEn;
                data.AgentName = LookupPartner[item.AgentId].FirstOrDefault()?.PartnerNameEn;
                data.ServiceDate = item.ServiceDate;
                data.VesselFlight = item.FlightNo;

                var _polCode = item.Pol != null ? PlaceLookup[(Guid)item.Pol].FirstOrDefault()?.Code : string.Empty;
                var _podCode = item.Pod != null ? PlaceLookup[(Guid)item.Pod].FirstOrDefault()?.Code : string.Empty;
                data.Route = _polCode + (!string.IsNullOrEmpty(_polCode) || !string.IsNullOrEmpty(_podCode) ? "/" : "") + _podCode;

                //Qty lấy theo Housebill
                data.Qty = item.PackageQty ?? 0;
                data.ChargeWeight = item.ChargeWeight ?? 0;

                #region -- Phí Selling trước thuế --
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeSell = LookupSurchage[(Guid)item.HblId].Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE);
                    data.Revenue = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? _chargeSell.Sum(x => x.AmountVnd ?? 0) : _chargeSell.Sum(x => x.AmountUsd ?? 0);
                }

                #endregion -- Phí Selling trước thuế --

                #region -- Phí Buying trước thuế --
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeBuy = LookupSurchage[(Guid)item.HblId].Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE);
                    data.Cost = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? _chargeBuy.Sum(x => x.AmountVnd ?? 0) : _chargeBuy.Sum(x => x.AmountUsd ?? 0);
                }

                #endregion -- Phí Buying trước thuế --

                data.Profit = data.Revenue - data.Cost;

                #region -- Phí OBH sau thuế --
                if (item.HblId != null && item.HblId != Guid.Empty)
                {
                    var _chargeObh = LookupSurchage[(Guid)item.HblId].Where(x => x.Type == DocumentConstants.CHARGE_OBH_TYPE);
                    data.Obh = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? _chargeObh.Sum(x => x.AmountVnd + x.VatAmountVnd ?? 0) : _chargeObh.Sum(x => x.AmountUsd + x.VatAmountUsd ?? 0);
                }

                #endregion -- Phí OBH sau thuế --

                var _empPic = lookupUser[item.PersonInCharge].FirstOrDefault()?.EmployeeId;
                if (!string.IsNullOrEmpty(_empPic))
                {
                    data.PersonInCharge = lookupEmployee[_empPic].FirstOrDefault()?.EmployeeNameEn;
                }

                var _empSale = lookupUser[item.SalemanId].FirstOrDefault()?.EmployeeId;
                if (!string.IsNullOrEmpty(_empSale))
                {
                    data.Salesman = lookupEmployee[_empSale].FirstOrDefault()?.EmployeeNameEn;
                }

                data.ServiceName = API.Common.Globals.CustomData.Services.Where(x => x.Value == item.TransactionType).FirstOrDefault()?.DisplayName;

                dataList.Add(data);
            }
            return dataList.AsQueryable();
        }
        #endregion -- GENERAL REPORT --     

        #region -- Export Accounting PL Sheet --

        private List<sp_GetDataExportAccountant> GetDataExportAccountant(GeneralReportCriteria criteria)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@serviceDateFrom", Value = criteria.ServiceDateFrom },
                new SqlParameter(){ ParameterName = "@serviceDateTo", Value = criteria.ServiceDateTo },
                new SqlParameter(){ ParameterName = "@createdDateFrom", Value = criteria.CreatedDateFrom },
                new SqlParameter(){ ParameterName = "@createdDateTo", Value = criteria.CreatedDateTo },
                new SqlParameter(){ ParameterName = "@customerId", Value = criteria.CustomerId },
                new SqlParameter(){ ParameterName = "@service", Value = criteria.Service },
                new SqlParameter(){ ParameterName = "@currency", Value = criteria.Currency },
                new SqlParameter(){ ParameterName = "@jobId", Value = criteria.JobId },
                new SqlParameter(){ ParameterName = "@mawb", Value = criteria.Mawb },
                new SqlParameter(){ ParameterName = "@hawb", Value = criteria.Hawb },
                new SqlParameter(){ ParameterName = "@officeId", Value = criteria.OfficeId },
                new SqlParameter(){ ParameterName = "@departmentId", Value = criteria.DepartmentId },
                new SqlParameter(){ ParameterName = "@groupId", Value = criteria.GroupId },
                new SqlParameter(){ ParameterName = "@personalInCharge", Value = criteria.PersonInCharge },
                new SqlParameter(){ ParameterName = "@salesMan", Value = criteria.SalesMan },
                new SqlParameter(){ ParameterName = "@creator", Value = criteria.Creator },
                new SqlParameter(){ ParameterName = "@carrierId", Value = criteria.CarrierId },
                new SqlParameter(){ ParameterName = "@agentId", Value = criteria.AgentId },
                new SqlParameter(){ ParameterName = "@pol", Value = criteria.Pol },
                new SqlParameter(){ ParameterName = "@pod", Value = criteria.Pod }
            };
            var list = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetDataExportAccountant>(parameters);
            return list;
        }
        public IQueryable<AccountingPlSheetExportResult> GetDataAccountingPLSheet(GeneralReportCriteria criteria)
        {
            var list = GetDataExportAccountant(criteria);
            var dataDocumentation = AcctPLSheetDocumentation(criteria, list);
            return dataDocumentation;
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

        private IQueryable<OpsTransaction> QueryDataOperationCost(GeneralReportCriteria criteria, bool? fromCost)
        {
            Expression<Func<OpsTransaction, bool>> query = GetQueryOPSTransactionOperation(criteria, null);

            var queryShipment = GetOpsTransactionWithSalesman(query, criteria);
            return queryShipment;
        }


        public List<JobProfitAnalysisExportResult> GetDataJobProfitAnalysis(GeneralReportCriteria criteria)
        {
            var dataDocumentation = JobProfitAnalysisDocumetation(criteria);
            IQueryable<JobProfitAnalysisExportResult> list;
            if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains(TermData.CustomLogistic))
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
            if (!dataShipment.Any()) return dataList.AsQueryable();
            var lstPartner = catPartnerRepo.Get();
            var lstCharge = catChargeRepo.Get();
            var lstSurchage = surCharge.Get().Where(x => !string.IsNullOrEmpty(criteria.CustomerId) ? (!string.IsNullOrEmpty(x.PaymentObjectId) && criteria.CustomerId.Contains(x.PaymentObjectId) || !string.IsNullOrEmpty(x.PayerId) && criteria.CustomerId.Contains(x.PayerId)) : true);
            var detailLookupSur = lstSurchage.ToLookup(q => q.Hblid);
            var detailLookupPartner = lstPartner.ToLookup(q => q.Id);
            var detailLookupCharge = lstCharge.ToLookup(q => q.Id);
            var DataCharge = (from d in dataShipment
                              join sur in lstSurchage on d.Hblid equals sur.Hblid
                              select new DataSurchargeResult
                              {
                                  JobId = d.JobNo,
                                  ServiceDate = d.ServiceDate,
                                  Hblid = sur.Hblid,
                                  InvoiceNo = sur.InvoiceNo,
                                  DebitNo = sur.DebitNo,
                                  AmountUsd = sur.AmountUsd,
                                  AmountVnd = sur.AmountVnd,
                                  VatAmountUsd = sur.VatAmountUsd,
                                  VatAmountVnd = sur.VatAmountVnd,
                                  VoucherId = sur.VoucherId,
                                  Type = sur.Type,
                                  KickBack = sur.KickBack,
                                  CurrencyId = sur.CurrencyId,
                                  ExchangeDate = sur.ExchangeDate,
                                  Mblno = d.Mblno,
                                  Hblno = d.Hwbno,
                                  ChargeId = sur.ChargeId,
                                  PaymentObjectId = sur.PaymentObjectId,
                                  CreditNo = sur.CreditNo,
                                  FinalExchangeRate = sur.FinalExchangeRate,
                                  ClearanceNo = sur.ClearanceNo,
                                  CustomerId = sur.Type == "OBH" ? sur.PayerId : sur.PaymentObjectId,
                                  PayerId = sur.PayerId
                              });
            foreach (var charge in DataCharge)
            {
                AccountingPlSheetExportResult data = new AccountingPlSheetExportResult();
                data.ServiceDate = charge.ServiceDate;
                data.JobId = charge.JobId;
                data.Hblid = charge.Hblid;
                var partnerId = !string.IsNullOrEmpty(charge.PayerId) && !string.IsNullOrEmpty(criteria.CustomerId) && criteria.CustomerId.Contains(charge.PayerId) ? charge.PayerId : charge.PaymentObjectId;
                data.CustomNo = !string.IsNullOrEmpty(charge.ClearanceNo) ? charge.ClearanceNo : GetCustomNoOldOfShipment(charge.JobId); //Ưu tiên: ClearanceNo of charge >> ClearanceNo of Job có ngày ClearanceDate cũ nhất
                decimal? _exchangeRate = charge.CurrencyId != criteria.Currency ? currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency) : charge.FinalExchangeRate;
                var _taxInvNoRevenue = string.Empty;
                var _voucherRevenue = string.Empty;
                decimal? _usdRevenue = 0;
                decimal? _vndRevenue = 0;
                decimal? _taxOut = 0;
                decimal? _totalRevenue = 0;
                if (charge.Type == DocumentConstants.CHARGE_SELL_TYPE)
                {
                    _taxInvNoRevenue = !string.IsNullOrEmpty(charge.InvoiceNo) ? charge.InvoiceNo : charge.DebitNo;
                    _usdRevenue = charge.AmountUsd;
                    _vndRevenue = charge.AmountVnd;
                    if (criteria.Currency == DocumentConstants.CURRENCY_USD)
                    {
                        _taxOut = charge.VatAmountUsd;
                        _totalRevenue = charge.AmountUsd + _taxOut;

                    }
                    if (criteria.Currency == DocumentConstants.CURRENCY_LOCAL)
                    {
                        _taxOut = charge.VatAmountVnd;
                        _totalRevenue = charge.AmountVnd + _taxOut;
                    }
                    data.CdNote = charge.DebitNo;
                    _voucherRevenue = charge.VoucherId;
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
                    _vndCost = charge.AmountVnd;
                    _usdCost = charge.AmountUsd;
                    if (criteria.Currency == DocumentConstants.CURRENCY_USD)
                    {
                        _taxIn = charge.VatAmountUsd;
                        _totalCost = charge.AmountUsd + _taxIn;


                    }
                    if (criteria.Currency == DocumentConstants.CURRENCY_LOCAL)
                    {
                        _taxIn = charge.VatAmountVnd;
                        _totalCost = charge.AmountVnd + _taxIn;
                    }
                    data.CdNote = charge.CreditNo;
                    _voucherCost = charge.VoucherId;
                }
                data.TaxInvNoCost = _taxInvNoCost;
                data.VoucherIdCost = _voucherCost;
                data.UsdCost = _usdCost;
                data.VndCost = _vndCost;
                data.TaxIn = _taxIn;
                data.TotalCost = _totalCost;

                if (criteria.Currency == DocumentConstants.CURRENCY_LOCAL)
                {
                    if (charge.KickBack == true)
                    {
                        data.TotalKickBack = charge.AmountVnd;
                    }
                }
                if (criteria.Currency == DocumentConstants.CURRENCY_USD)
                {
                    if (charge.KickBack == true)
                    {
                        data.TotalKickBack = charge.AmountUsd;
                    }
                }
                data.ExchangeRate = (decimal)(_exchangeRate ?? 0);
                data.Balance = _totalRevenue - _totalCost - (data.TotalKickBack ?? 0);
                data.InvNoObh = charge.Type == DocumentConstants.CHARGE_OBH_TYPE ? charge.InvoiceNo : string.Empty;

                if (charge.Type == DocumentConstants.CHARGE_OBH_TYPE)
                {
                    var _mapCharge = mapper.Map<CsShipmentSurcharge>(charge);
                    data.AmountObh = currencyExchangeService.ConvertAmountChargeToAmountObj(_mapCharge, criteria.Currency); //Amount sau thuế của phí OBH
                    data.CdNote = charge.DebitNo;
                }
                data.AcVoucherNo = string.Empty;
                data.PmVoucherNo = charge.Type == DocumentConstants.CHARGE_OBH_TYPE ? charge.VoucherId : string.Empty; //Voucher của phí OBH theo Payee
                data.Service = API.Common.Globals.CustomData.Services.Where(x => x.Value == "CL").FirstOrDefault()?.DisplayName;
                data.UserExport = currentUser.UserName;
                data.CurrencyId = charge.CurrencyId;
                data.ExchangeDate = charge.ExchangeDate;
                data.FinalExchangeRate = charge.FinalExchangeRate;
                data.Mbl = charge.Mblno;
                data.Hbl = charge.Hblno;
                data.PartnerCode = detailLookupPartner[partnerId == null ? charge.CustomerId : partnerId].FirstOrDefault()?.AccountNo;
                data.PartnerName = detailLookupPartner[partnerId == null ? charge.CustomerId : partnerId].FirstOrDefault()?.PartnerNameEn;
                data.PartnerTaxCode = detailLookupPartner[partnerId == null ? charge.CustomerId : partnerId].FirstOrDefault()?.TaxCode;
                data.ChargeCode = detailLookupCharge[charge.ChargeId].FirstOrDefault()?.Code;
                data.ChargeName = detailLookupCharge[charge.ChargeId].FirstOrDefault()?.ChargeNameEn;
                dataList.Add(data);
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
        private IQueryable<AccountingPlSheetExportResult> AcctPLSheetDocumentation(GeneralReportCriteria criteria, List<sp_GetDataExportAccountant> dataExportAccountants)
        {
            // Filter data without customerId
            var criteriaNoCustomer = (GeneralReportCriteria)criteria.Clone();
            criteriaNoCustomer.CustomerId = null;
            if (!dataExportAccountants.Any()) return null;
            var lstPartner = catPartnerRepo.Get();
            var lstCharge = catChargeRepo.Get();
            var detailLookupPartner = lstPartner.ToLookup(q => q.Id);
            var detailLookupCharge = lstCharge.ToLookup(q => q.Id);
            var dataCustom = customsDeclarationRepo.Get().ToList();
            var LookupUser = sysUserRepo.Get().ToLookup(x => x.Id);
            List<AccountingPlSheetExportResult> dataList = new List<AccountingPlSheetExportResult>();
            foreach (var charge in dataExportAccountants)
            {
                AccountingPlSheetExportResult data = new AccountingPlSheetExportResult();
                data.ServiceDate = charge.ServiceDate;
                data.JobId = charge.JobNo;
                data.CustomNo = charge.ClearanceNo;
                var _taxInvNoRevenue = string.Empty;
                var _voucherRevenue = string.Empty;
                decimal? _usdRevenue = 0;
                decimal? _vndRevenue = 0;
                decimal? _taxOut = 0;
                decimal? _totalRevenue = 0;
                if (charge.Type == DocumentConstants.CHARGE_SELL_TYPE)
                {
                    _taxInvNoRevenue = !string.IsNullOrEmpty(charge.InvoiceNo) ? charge.InvoiceNo : charge.DebitNo;
                    _usdRevenue = charge.AmountUsd;
                    _vndRevenue = charge.AmountVnd;
                    if (criteria.Currency == DocumentConstants.CURRENCY_USD)
                    {
                        _taxOut = charge.VatAmountUsd;
                        _totalRevenue = charge.AmountUsd + _taxOut;

                    }
                    if (criteria.Currency == DocumentConstants.CURRENCY_LOCAL)
                    {
                        _taxOut = charge.VatAmountVnd;
                        _totalRevenue = charge.AmountVnd + _taxOut;
                    }
                    data.CdNote = charge.DebitNo;
                    _voucherRevenue = charge.VoucherId;
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
                    _vndCost = charge.AmountVnd;
                    _usdCost = charge.AmountUsd;
                    if (criteria.Currency == DocumentConstants.CURRENCY_USD)
                    {
                        _taxIn = charge.VatAmountUsd;
                        _totalCost = charge.AmountUsd + _taxIn;


                    }
                    if (criteria.Currency == DocumentConstants.CURRENCY_LOCAL)
                    {
                        _taxIn = charge.VatAmountVnd;
                        _totalCost = charge.AmountVnd + _taxIn;
                    }
                    data.CdNote = charge.CreditNo;
                    _voucherCost = charge.VoucherId;
                }
                data.TaxInvNoCost = _taxInvNoCost;
                data.VoucherIdCost = _voucherCost;
                data.UsdCost = _usdCost;
                data.VndCost = _vndCost;
                data.TaxIn = _taxIn;
                data.TotalCost = _totalCost;

                if (criteria.Currency == DocumentConstants.CURRENCY_LOCAL)
                {
                    if (charge.KickBack == true)
                    {
                        data.TotalKickBack = charge.AmountVnd;
                    }
                }
                if (criteria.Currency == DocumentConstants.CURRENCY_USD)
                {
                    if (charge.KickBack == true)
                    {
                        data.TotalKickBack = charge.AmountUsd;
                    }
                }
                data.ExchangeRate = charge.FinalExchangeRate;
                // [CR: cột Balance chỉ tính cho total selling - total buying]
                data.Balance = _totalRevenue - _totalCost;
                data.InvNoObh = charge.Type == DocumentConstants.CHARGE_OBH_TYPE ? charge.InvoiceNo : string.Empty;

                if (charge.Type == DocumentConstants.CHARGE_OBH_TYPE)
                {
                    var _mapCharge = mapper.Map<CsShipmentSurcharge>(charge);
                    data.OBHNetAmount = currencyExchangeService.ConvertNetAmountChargeToNetAmountObj(_mapCharge, criteria.Currency); // Amount trước thuế OBH
                    data.AmountObh = currencyExchangeService.ConvertAmountChargeToAmountObj(_mapCharge, criteria.Currency); //Amount sau thuế của phí OBH
                    data.CdNote = charge.DebitNo;
                }
                data.AcVoucherNo = string.Empty;
                data.PmVoucherNo = charge.Type == DocumentConstants.CHARGE_OBH_TYPE ? charge.VoucherId : string.Empty; //Voucher của phí OBH theo Payee
                data.Service = API.Common.Globals.CustomData.Services.Where(x => x.Value == charge.Service).FirstOrDefault()?.DisplayName;
                data.UserExport = currentUser.UserName;
                data.CurrencyId = charge.CurrencyId;
                data.ExchangeDate = charge.ExchangeDate;
                data.Mbl = charge.MAWB;
                data.Hbl = charge.HWBNo;
                data.PartnerCode = detailLookupPartner[charge.PartnerId].FirstOrDefault()?.AccountNo;
                data.PartnerName = detailLookupPartner[charge.PartnerId].FirstOrDefault()?.PartnerNameEn;
                data.PartnerTaxCode = detailLookupPartner[charge.PartnerId].FirstOrDefault()?.TaxCode;
                data.ChargeCode = detailLookupCharge[charge.ChargeId].FirstOrDefault()?.Code;
                data.ChargeName = detailLookupCharge[charge.ChargeId].FirstOrDefault()?.ChargeNameEn;
                data.Creator = LookupUser[charge.UserCreated].Select(t => t.Username).FirstOrDefault();
                data.SyncedFrom = charge.SyncedFrom;
                data.VatPartnerName = detailLookupPartner[charge.VatPartnerID].FirstOrDefault()?.ShortName;
                data.BillNoSynced = getBillNoSynced(charge);
                dataList.Add(data);
            }
            return dataList.AsQueryable();
        }

        private string getBillNoSynced(sp_GetDataExportAccountant charge)
        {
            //if ((Soano == null && paySoaNo==null) && SettlementCode != null)
            //{
            //    return SettlementCode;
            //}
            //if (SettlementCode == null && Soano != null)
            //{
            //    return Soano;
            //}
            //if(SettlementCode == null && Soano == null && paySoaNo == null)
            //{
            //    return "";
            //}
            //return SettlementCode + (Soano!=null?Soano:paySoaNo);
            if(charge.SyncedFrom== "SETTLEMENT") {
                return surCharge.Get(x => x.Id == charge.SurChargeId).FirstOrDefault().SettlementCode;
            }
            else if(charge.SyncedFrom == "SOA")
            {
                if (charge.Type == "OBH")
                {
                    return surCharge.Get(x => x.Id == charge.SurChargeId).FirstOrDefault().PaySoano;
                }
                return surCharge.Get(x => x.Id == charge.SurChargeId).FirstOrDefault().Soano;
            }
            else if(charge.SyncedFrom == "VOUCHER")
            {
                return surCharge.Get(x => x.Id == charge.SurChargeId).FirstOrDefault().VoucherId;
            }else if(charge.SyncedFrom == "CDNOTE")
            {
                return surCharge.Get(x => x.Id == charge.SurChargeId).FirstOrDefault().CreditNo;
            }
            return "";
        }
        #endregion -- Export Accounting PL Sheet --


        #region -- Export Summary Of Costs Incurred
        private IQueryable<SummaryOfCostsIncurredExportResult> SummaryOfCostsIncurredOperation(GeneralReportCriteria criteria)
        {
            var dataShipment = QueryDataOperationCost(criteria, null);
            if (dataShipment == null) return null;
            var port = catPlaceRepo.Get();
            List<SummaryOfCostsIncurredExportResult> dataList = new List<SummaryOfCostsIncurredExportResult>();
            Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query = chg => criteria.CustomerId.Contains(chg.CustomerID);
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
            if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains(TermData.CustomLogistic))
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

        public SummaryOfRevenueModel GetDataCostsByPartner(GeneralReportCriteria criteria)
        {
            Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query = chg => criteria.CustomerId.Contains(chg.CustomerID);
            var chargeData = !string.IsNullOrEmpty(criteria.CustomerId) ? GetChargeOBHPayee(query, null) : GetChargeOBHPayee(null, null);
            var dataDocumentation = SummaryOfCostsByPartner(criteria, chargeData);
            SummaryOfRevenueModel obj = new SummaryOfRevenueModel();

            if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains("CL"))
            {
                var dataOperation = SummaryOfCostsByPartnerOperation(criteria, chargeData);
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
        #region Costs By Partner

        private SummaryOfRevenueModel SummaryOfCostsByPartner(GeneralReportCriteria criteria, IQueryable<SummaryOfCostsIncurredExportResult> chargeData)
        {
            var dataShipment = QueryDataSummaryOfCostsIncurred(criteria);
            if (dataShipment == null) return null;
            SummaryOfRevenueModel ObjectSummaryRevenue = new SummaryOfRevenueModel();
            List<SummaryOfRevenueExportResult> dataList = new List<SummaryOfRevenueExportResult>();
            var results = chargeData.AsEnumerable().GroupBy(x => new { x.JobId, x.HBLID });
            if (results == null) return null;
            var lookupReuslts = results.ToLookup(q => q.Key.HBLID);
            var listPartner = catPartnerRepo.Get();
            var lookupPartner = listPartner.ToLookup(q => q.Id);
            var dataCustom = customsDeclarationRepo.Get().ToList();
            var port = catPlaceRepo.Get();
            foreach (var item in dataShipment)
            {
                if (item.HBLID != Guid.Empty)
                {
                    foreach (var group in lookupReuslts[item.HBLID])
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
                            ele.SuplierName = lookupPartner[ele.CustomerID].Select(t => t.PartnerNameVn).FirstOrDefault();
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
                    if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                    {
                        it.VATAmount = it.VATAmountUSD;
                        it.NetAmount = it.AmountUSD;
                    }
                    else
                    {
                        it.VATAmount = it.VATAmountVND;
                        it.NetAmount = it.AmountVND;
                    }
                    if (it.VATRate > 0)
                    {
                        it.VATAmount = (it.VATRate * it.NetAmount) / 100;
                    }
                    else
                    {
                        it.VATAmount = it.VATRate != null ? Math.Abs(it.VATRate.Value) : 0;
                        if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                        {
                            it.VATAmount = it.VATAmountUSD;
                        }
                        else
                        {
                            it.VATAmount = it.VATAmountVND;
                        }

                    }

                }
            }
            return ObjectSummaryRevenue;
        }
        private SummaryOfRevenueModel SummaryOfCostsByPartnerOperation(GeneralReportCriteria criteria, IQueryable<SummaryOfCostsIncurredExportResult> chargeData)
        {
            var dataShipment = QueryDataOperationAcctPLSheet(criteria);
            if (dataShipment == null) return null;
            var port = catPlaceRepo.Get();
            SummaryOfRevenueModel ObjectSummaryRevenue = new SummaryOfRevenueModel();
            List<SummaryOfRevenueExportResult> dataList = new List<SummaryOfRevenueExportResult>();
            Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query = chg => criteria.CustomerId.Contains(chg.CustomerID);
            var results = chargeData.AsEnumerable().GroupBy(x => new { x.JobId, x.HBLID }).AsQueryable();

            var lookupReuslts = results.ToLookup(q => q.Key.JobId);
            var listPartner = catPartnerRepo.Get();
            var lookupPartner = listPartner.ToLookup(q => q.Id);
            var dataCustom = customsDeclarationRepo.Get().ToList();
            var commodityGroupData = catCommodityGroupRepo.Get().ToList();
            var lookupCommodityGroup = commodityGroupData.ToLookup(q => q.Id);
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
                            ele.SuplierName = lookupPartner[ele.CustomerID].Select(t => t.PartnerNameVn).FirstOrDefault();
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
                    if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                    {
                        it.VATAmount = it.VATAmountUSD;
                        it.NetAmount = it.AmountUSD;
                    }
                    else
                    {
                        it.VATAmount = it.VATAmountVND;
                        it.NetAmount = it.AmountVND;
                    }
                    if (it.VATRate > 0)
                    {
                        it.VATAmount = (it.VATRate * it.NetAmount) / 100;
                    }
                    else
                    {
                        it.VATAmount = it.VATRate != null ? Math.Abs(it.VATRate.Value) : 0;
                        if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                        {
                            it.VATAmount = it.VATAmountUSD;
                        }
                        else
                        {
                            it.VATAmount = it.VATAmountVND;
                        }

                    }

                }
            }
            return ObjectSummaryRevenue;
        }

        #endregion


        private string GetTopClearanceNoByJobNo(string JobNo, List<CustomsDeclaration> customsDeclarations)
        {
            var clearanceNo = customsDeclarations.Where(x => x.JobNo != null && x.JobNo == JobNo)
                .OrderBy(x => x.JobNo)
                .OrderByDescending(x => x.ClearanceDate)
                .FirstOrDefault()?.ClearanceNo;
            return clearanceNo;
        }

        private string GetCustomNoOldOfShipment1(string JobNo, List<CustomsDeclaration> customsDeclarations)
        {
            var clearanceNo = customsDeclarations.Where(x => x.JobNo != null && x.JobNo == JobNo)
                .OrderBy(o => o.DatetimeModified)
                .FirstOrDefault()?.ClearanceNo;
            return clearanceNo;
        }

        private IQueryable<SummaryOfCostsIncurredExportResult> SummaryOfCostsIncurred(GeneralReportCriteria criteria)
        {
            var dataShipment = QueryDataSummaryOfCostsIncurred(criteria);
            if (dataShipment == null) return null;
            var port = catPlaceRepo.Get();
            List<SummaryOfCostsIncurredExportResult> dataList = new List<SummaryOfCostsIncurredExportResult>();
            Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query = chg => criteria.CustomerId.Contains(chg.CustomerID);
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
                                          ServiceDate = cst.ServiceDate,
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

        private IQueryable<SummaryOfCostsIncurredExportResult> GetChargeOBHPayee(Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query, bool? isOBH)
        {
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
                                           Type = sur.Type,
                                           Debit = null,
                                           Credit = sur.Total,
                                           IsOBH = true,
                                           Currency = sur.CurrencyId,
                                           InvoiceNo = sur.InvoiceNo,
                                           InvoiceDate = sur.InvoiceDate,
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
                                          Type = sur.Type,
                                          Debit = null,
                                          Credit = sur.Total,
                                          IsOBH = true,
                                          Currency = sur.CurrencyId,
                                          InvoiceNo = sur.InvoiceNo,
                                          Note = sur.Notes,
                                          CustomerID = sur.Type == "OBH" ? sur.PayerId : sur.PaymentObjectId,
                                          ServiceDate = cst.ServiceDate,
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
                                          AmountVND = sur.AmountVnd,
                                          InvoiceDate = sur.InvoiceDate,
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
            Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query = chg => criteria.CustomerId.Contains(chg.CustomerID);
            var chargeData = !string.IsNullOrEmpty(criteria.CustomerId) ? GetChargeOBHSellPayerJob(query, null) : GetChargeOBHSellPayerJob(null, null);
            var results = chargeData.GroupBy(x => new { x.JobId, x.HBLID }).AsQueryable();

            var lookupReuslts = results.ToLookup(q => q.Key.JobId);
            var listPartner = catPartnerRepo.Get();
            var lookupPartner = listPartner.ToLookup(q => q.Id);
            var dataCustom = customsDeclarationRepo.Get().ToList();
            var commodityGroupData = catCommodityGroupRepo.Get().ToList();
            var lookupCommodityGroup = commodityGroupData.ToLookup(q => q.Id);
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
                    if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                    {
                        it.VATAmount = it.VATAmountUSD;
                        it.NetAmount = it.AmountUSD;
                    }
                    else
                    {
                        it.VATAmount = it.VATAmountVND;
                        it.NetAmount = it.AmountVND;
                    }
                    //if (it.VATRate > 0)
                    //{
                    //    it.VATAmount = (it.VATRate * it.NetAmount) / 100;
                    //}
                    //else
                    //{
                    //    it.VATAmount = it.VATRate != null ? Math.Abs(it.VATRate.Value) : 0;
                    //    if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                    //    {
                    //        it.VATAmount = it.VATAmountUSD;
                    //    }
                    //    else
                    //    {
                    //        it.VATAmount = it.VATAmountVND;
                    //    }

                    //}

                }
            }
            return ObjectSummaryRevenue;
        }
        public SummaryOfRevenueModel GetDataSummaryOfRevenueIncurred(GeneralReportCriteria criteria)
        {
            var dataDocumentation = SummaryOfRevenueIncurred(criteria);
            SummaryOfRevenueModel obj = new SummaryOfRevenueModel();

            if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains(TermData.CustomLogistic))
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
            Expression<Func<SummaryOfCostsIncurredExportResult, bool>> query = chg => criteria.CustomerId.Contains(chg.CustomerID);
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
                    if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                    {
                        it.VATAmount = it.VATAmountUSD;
                        it.NetAmount = it.AmountUSD;
                    }
                    else
                    {
                        it.VATAmount = it.VATAmountVND;
                        it.NetAmount = it.AmountVND;
                    }
                    //if (it.VATRate > 0)
                    //{
                    //    it.VATAmount = (it.VATRate * it.NetAmount) / 100;
                    //}
                    //else
                    //{
                    //    it.VATAmount = it.VATRate != null ? Math.Abs(it.VATRate.Value) : 0;
                    //    if (criteria.Currency != DocumentConstants.CURRENCY_LOCAL)
                    //    {
                    //        it.VATAmount = it.VATAmountUSD;
                    //    }
                    //    else
                    //    {
                    //        it.VATAmount = it.VATAmountVND;
                    //    }

                    //}

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
                                           VATAmountUSD = sur.VatAmountUsd,
                                           VATAmountVND = sur.VatAmountVnd,
                                           AmountUSD = sur.AmountUsd,
                                           AmountVND = sur.AmountVnd
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
                                          ServiceDate = cst.ServiceDate,
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
                                          InvoiceDate = sur.InvoiceDate,
                                          VATAmountUSD = sur.VatAmountUsd,
                                          VATAmountVND = sur.VatAmountVnd,
                                          AmountUSD = sur.AmountUsd,
                                          AmountVND = sur.AmountVnd
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

        #region -- COMMISSION INCENTIVE REPORT
        /// <summary>
        /// Get Department Manager
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="officeId"></param>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        private List<string> GetDeptManager(Guid? companyId, Guid? officeId, int? departmentId)
        {
            var managers = sysUserLevelRepo.Get(x => x.GroupId == 11
                                                    && x.Position == "Manager-Leader"
                                                    && x.DepartmentId == departmentId
                                                    && x.DepartmentId != null
                                                    && x.OfficeId == officeId
                                                    && x.CompanyId == companyId).Select(s => s.UserId).ToList();
            return managers;
        }

        /// <summary>
        /// Get Accoutant Manager
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="officeId"></param>
        /// <returns></returns>
        private List<string> GetAccoutantManager(Guid? companyId, Guid? officeId)
        {
            var deptAccountants = departmentRepository.Get(s => s.DeptType == "ACCOUNTANT").Select(s => s.Id).ToList();
            var accountants = sysUserLevelRepo.Get(x => x.GroupId == 11
                                                    && x.OfficeId == officeId
                                                    && x.DepartmentId != null
                                                    && x.CompanyId == companyId
                                                    && x.Position == "Manager-Leader")
                                                    .Where(x => deptAccountants.Contains(x.DepartmentId.Value))
                                                    .Select(s => s.UserId).ToList();
            return accountants;
        }

        /// <summary>
        /// Get Office Manager
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="officeId"></param>
        /// <returns></returns>
        private List<string> GetOfficeManager(Guid? companyId, Guid? officeId)
        {
            var officeManager = sysUserLevelRepo.Get(x => x.GroupId == 11
                                                    && x.DepartmentId == null
                                                    && x.OfficeId == officeId
                                                    && x.CompanyId == companyId
                                                    && x.Position == "Manager-Leader")
                                                    .Select(s => s.UserId).ToList();
            return officeManager;
        }

        /// <summary>
        /// Get Selling total not include Commission fee
        /// </summary>
        /// <param name="hblid"></param>
        /// <returns></returns>
        private decimal GetSellingRateNoCom(Guid hblid, string currency, string exceptId = null)
        {
            decimal revenue = 0;
            var chargeComId = catChargeGroupRepo.Get(x => x.Name.ToUpper() == "COM")?.Select(x => x.Id).FirstOrDefault();
            Expression<Func<CsShipmentSurcharge, bool>> query;
            if(!string.IsNullOrEmpty(exceptId))
            {
                query = x => x.Type == DocumentConstants.CHARGE_SELL_TYPE && x.Hblid == hblid
                             && !((x.KickBack == true || x.ChargeGroup == chargeComId) && x.PaymentObjectId == exceptId);
            }
            else
            {
                query = x => x.Type == DocumentConstants.CHARGE_SELL_TYPE
                             && x.Hblid == hblid
                             && (x.KickBack == false || x.KickBack == null)
                             && x.ChargeGroup != chargeComId;
            }
            var sellingCharges = surCharge.Get(query);
            if (sellingCharges != null)
            {
                foreach (var charge in sellingCharges)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    //var rate = charge.CurrencyId == currency ? 1 : currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, currency);
                    //revenue += charge.Quantity * charge.UnitPrice * rate ?? 0;
                    revenue += (currency == DocumentConstants.CURRENCY_LOCAL ? (charge.AmountVnd ?? 0) : (charge.AmountUsd ?? 0)); // Selling trước thuế
                }
            }
            return revenue;
        }

        /// <summary>
        /// Get Buying total not include Commission fee
        /// </summary>
        /// <param name="hblid"></param>
        /// <returns></returns>
        private decimal GetBuyingRateNoCom(Guid hblid, string currency, string exceptId = null)
        {
            decimal cost = 0;
            var chargeComId = catChargeGroupRepo.Get(x => x.Name.ToUpper() == "COM")?.Select(x => x.Id).FirstOrDefault();
            Expression<Func<CsShipmentSurcharge, bool>> query;
            if (!string.IsNullOrEmpty(exceptId))
            {
                query = x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && x.Hblid == hblid
                             && !((x.KickBack == true || x.ChargeGroup == chargeComId) && x.PaymentObjectId == exceptId);
            }
            else
            {
                query = x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && x.Hblid == hblid
                             && (x.KickBack == false || x.KickBack == null)
                             && x.ChargeGroup != chargeComId;
            }
            var buyingCharges = surCharge.Get(query);
            if (buyingCharges != null)
            {
                foreach (var charge in buyingCharges)
                {
                    //if (catChargeRepo.Where(c => c.Id == charge.ChargeId && c.ChargeGroup == chargeComId).Count() == 0)
                    //{
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    //var rate = charge.CurrencyId == currency ? 1 : currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, currency);
                    //cost += charge.Quantity * charge.UnitPrice * rate ?? 0; // Phí Buying trước thuế
                    cost += (currency == DocumentConstants.CURRENCY_LOCAL ? (charge.AmountVnd ?? 0) : (charge.AmountUsd ?? 0)); // Phí Buying trước thuế
                    //}
                }
            }
            return cost;
        }

        /// <summary>
        /// Get total Commission with partner id
        /// </summary>
        /// <param name="hblid"></param>
        /// <returns></returns>
        private decimal GetCommissionAmount(Guid hblid, string currency, string partnerId)
        {
            decimal com = 0;
            var chargeComId = catChargeGroupRepo.Get(x => x.Name == "Com")?.Select(x => x.Id).FirstOrDefault();
            var charges = surCharge.Get(x => (x.Type == DocumentConstants.CHARGE_BUY_TYPE
                                           || x.Type == DocumentConstants.CHARGE_SELL_TYPE)
                                           && x.Hblid == hblid
                                           && x.PaymentObjectId == partnerId);
            if (charges != null)
            {
                foreach (var charge in charges)
                {
                    var chargeHasCom = catChargeRepo.Where(c => c.Id == charge.ChargeId && c.ChargeGroup == chargeComId).Count() > 0;
                    if (charge.KickBack == true || charge.ChargeGroup == chargeComId || chargeHasCom)
                    {
                        //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                        //var rate = charge.CurrencyId == currency ? 1 : currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, currency);
                        //com += charge.Quantity * charge.UnitPrice * rate ?? 0; // Phí Selling trước thuế
                        com += (currency == DocumentConstants.CURRENCY_LOCAL ? (charge.AmountVnd ?? 0) : (charge.AmountUsd ?? 0)); // Phí Commission trước thuế
                    }
                }
            }
            return com;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobNo"></param>
        /// <returns></returns>
        private string GetPortCode(Guid HblId, string service)
        {
            var shipment = detailRepository.Get(x => x.Id == HblId)?.FirstOrDefault();
            if (service == TermData.AirExport || service == TermData.SeaConsolExport || service == TermData.SeaFCLExport || service == TermData.SeaLCLExport)
            {
                return catPlaceRepo.Get(x => x.Id == shipment.Pod).Select(x => x.Code)?.FirstOrDefault();
            }
            if (service == TermData.AirImport || service == TermData.SeaConsolImport || service == TermData.SeaFCLImport || service == TermData.SeaLCLImport)
            {
                return catPlaceRepo.Get(x => x.Id == shipment.Pol).Select(x => x.Code)?.FirstOrDefault();
            }
            return string.Empty;
        }

        /// <summary>
        /// Get data Commission Report
        /// </summary>
        /// <param name="criteria">data search</param>
        /// <param name="userId">current user</param>
        /// <returns></returns>
        public CommissionExportResult GetCommissionReport(CommissionReportCriteria criteria, string userId, string rptType)
        {
            CommissionExportResult commissionData = new CommissionExportResult();
            var forMonth = string.Empty;
            var isOPSReport = rptType == "OPS";
            // Get detail
            if (isOPSReport) // Commission OPS Report
            {
                if (!criteria.Service.Contains(TermData.CustomLogistic))
                {
                    return null;
                }
                else
                {
                    criteria.Service = TermData.CustomLogistic;
                }
                var dataShipmentOps = GetDataGeneralReport(criteria).AsQueryable(); //QueryDataOperation(criteria);
                if (!string.IsNullOrEmpty(criteria.CustomNo))
                {
                    var clearanceData = customsDeclarationRepo.Get(x => criteria.CustomNo.Contains(x.ClearanceNo)).OrderBy(x => x.ClearanceDate);
                    var clearanceDataGroup = clearanceData.GroupBy(x => x.JobNo).Select(x => x.Key).ToList();
                    dataShipmentOps = dataShipmentOps.Where(x => clearanceDataGroup.Contains(x.JobNo));
                }
                var data = dataShipmentOps.GroupBy(x => x.JobNo).AsQueryable();
                if (data.Count() == 0)
                {
                    return null;
                }
                commissionData.Details = new List<CommissionDetail>();
                var chargeComId = catChargeGroupRepo.Get(x => x.Name == "Com")?.Select(x => x.Id).FirstOrDefault();
                var listcharge = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE
                                           || x.Type == DocumentConstants.CHARGE_SELL_TYPE).ToLookup(x => x.Hblid);
                var partnerList = catPartnerRepo.Get().ToLookup(x => x.Id);
                var customsDeclaration = customsDeclarationRepo.Get().ToLookup(x => x.JobNo);
                foreach (var item in data)
                {
                    var charges = listcharge[item.Select(x => (Guid)x.HblId).FirstOrDefault()];
                    var chargeHasCom = catChargeRepo.Where(c => charges.Where(x => x.ChargeId == c.Id && x.PaymentObjectId == criteria.Beneficiary).Any() && c.ChargeGroup == chargeComId).Count() > 0;
                    if (charges.Where(x => x.KickBack == true && x.PaymentObjectId == criteria.Beneficiary).Any() || charges.Where(x => x.ChargeGroup == chargeComId && x.PaymentObjectId == criteria.Beneficiary).Any() || chargeHasCom)
                    {
                        commissionData.Details.Add(new CommissionDetail()
                        {
                            ServiceDate = item.Select(x => x.ServiceDate).FirstOrDefault(),
                            JobId = item.Select(x => x.JobNo).FirstOrDefault(),
                            HBLNo = string.Empty,
                            MBLNo = string.Empty,
                            CustomerName = partnerList[item.Select(x => x.CustomerId).FirstOrDefault()].FirstOrDefault().ShortName,
                            CustomSheet = string.IsNullOrEmpty(criteria.CustomNo) ? string.Join(';', customsDeclaration[item.Select(x => x.JobNo).FirstOrDefault()].Select(c => c.ClearanceNo).ToArray())
                                                                                  : string.Join(';', customsDeclaration[item.Select(x => x.JobNo).FirstOrDefault()].Where(c => criteria.CustomNo.Contains(c.ClearanceNo)).Select(c => c.ClearanceNo).ToArray()),
                            ChargeWeight = 0,
                            PortCode = string.Empty,
                            BuyingRate = GetBuyingRateNoCom(item.Select(x => (Guid)x.HblId).FirstOrDefault(), criteria.Currency, criteria.Beneficiary),
                            SellingRate = GetSellingRateNoCom(item.Select(x => (Guid)x.HblId).FirstOrDefault(), criteria.Currency, criteria.Beneficiary),
                            ComAmount = GetCommissionAmount(item.Select(x => (Guid)x.HblId).FirstOrDefault(), criteria.Currency, criteria.Beneficiary)
                        });
                    }
                }
            }
            else // Commission Air/Sea Report
            {
                if (!string.IsNullOrEmpty(criteria.Service))
                {
                    criteria.Service = criteria.Service.Replace(TermData.CustomLogistic, "");
                }
                var dataShipment = GetDataGeneralReport(criteria);
                if (dataShipment.Count() == 0)
                {
                    return null;
                }
                commissionData.Details = new List<CommissionDetail>();
                var chargeComId = catChargeGroupRepo.Get(x => x.Name == "Com")?.Select(x => x.Id).FirstOrDefault();
                var listcharge = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE
                                           || x.Type == DocumentConstants.CHARGE_SELL_TYPE).ToLookup(x => x.Hblid);
                var containerLst = csMawbcontainerRepo.Get().ToLookup(x => x.Hblid);
                foreach (var item in dataShipment)
                {
                    var charges = listcharge[(Guid)item.HblId];
                    var containerData = containerLst[(Guid)item.HblId];
                    var chargeHasCom = catChargeRepo.Where(c => charges.Where(x => x.ChargeId == c.Id && x.PaymentObjectId == criteria.Beneficiary).Any() && c.ChargeGroup == chargeComId).Count() > 0;
                    if ((charges.Where(x => x.KickBack == true && x.PaymentObjectId == criteria.Beneficiary).Any() || charges.Where(x => x.ChargeGroup == chargeComId && x.PaymentObjectId == criteria.Beneficiary).Any() || chargeHasCom))
                    {
                        commissionData.Details.Add(new CommissionDetail()
                        {
                            ServiceDate = item.ServiceDate,
                            JobId = item.JobNo,
                            HBLNo = item.HwbNo,
                            MBLNo = item.Mawb,
                            CustomSheet = string.Empty,
                            ChargeWeight = item.ChargeWeight,
                            TransactionType = item.TransactionType,
                            ContQty = item.TransactionType.Contains('S') ? containerData.Sum(x => x.Quantity ?? 0) : 0,
                            PackageContainer = item.PackageContainer,
                            PortCode = GetPortCode((Guid)item.HblId, item.TransactionType),
                            BuyingRate = GetBuyingRateNoCom((Guid)item.HblId, criteria.Currency, criteria.Beneficiary),
                            SellingRate = GetSellingRateNoCom((Guid)item.HblId, criteria.Currency, criteria.Beneficiary),
                            ComAmount = GetCommissionAmount((Guid)item.HblId, criteria.Currency, criteria.Beneficiary)
                        });
                    }
                }
                commissionData.CustomerName = !string.IsNullOrEmpty(criteria.CustomerId) ? string.Join("; ", catPartnerRepo.Get(x => criteria.CustomerId.ToUpper().Contains(x.Id.ToUpper())).Select(x => x.PartnerNameEn)) : string.Empty;
            }
            if (commissionData.Details.Count() == 0)
            {
                return null;
            }
            // Get header
            var listOrder = commissionData.Details.OrderBy(x => x.ServiceDate);
            if (listOrder.Select(x => x.ServiceDate?.Year).Distinct().Count() == 1)
            {
                var startMonth = listOrder.Select(x => x.ServiceDate).FirstOrDefault();
                var endMonth = listOrder.Select(x => x.ServiceDate).LastOrDefault();
                forMonth = string.Format("{0}{1}", startMonth?.Month == endMonth?.Month ? "" : (startMonth?.ToString("MMM") + " - "), listOrder.Select(x => x.ServiceDate?.ToString("MMM yyyy")).LastOrDefault());
            }
            else
            {
                var startMonth = listOrder.Select(x => x.ServiceDate).FirstOrDefault();
                var endMonth = listOrder.Select(x => x.ServiceDate).LastOrDefault();
                forMonth = string.Format("{0} - {1}", startMonth?.ToString("MMM yyyy"), endMonth?.ToString("MMM yyyy"));
            }
            commissionData.ForMonth = forMonth;
            
            commissionData.ExchangeRate = criteria.ExchangeRate;
            // Partner info
            if (!string.IsNullOrEmpty(criteria.Beneficiary))
            {
                var beneficiaryInfo = catPartnerRepo.Get(x => x.Id == criteria.Beneficiary)?.FirstOrDefault();
                commissionData.BeneficiaryName = beneficiaryInfo?.PartnerNameVn;
                commissionData.BankAccountNo = beneficiaryInfo?.BankAccountNo;
                commissionData.BankName = beneficiaryInfo?.BankAccountName;
                commissionData.TaxCode = beneficiaryInfo?.TaxCode;
            }
            // get current user
            if (string.IsNullOrEmpty(userId))
            {
                userId = currentUser.UserID;
            }
            var curUser = sysUserLevelRepo.Get(x => x.UserId == userId)?.FirstOrDefault();
            var preparedById = sysUserRepo.Get(x => x.Id == userId).FirstOrDefault()?.EmployeeId;
            commissionData.PreparedBy = sysEmployeeRepo.Get(x => x.Id == preparedById).FirstOrDefault()?.EmployeeNameEn;
            // Get Department manager
            var managers = GetDeptManager(curUser?.CompanyId, curUser?.OfficeId, curUser?.DepartmentId)?.FirstOrDefault();
            string _employeeIdDeptManager = sysUserRepo.Get(x => x.Id == managers).FirstOrDefault()?.EmployeeId;
            string _managerDept = string.Empty;
            if (_employeeIdDeptManager != null)
            {
                _managerDept = sysEmployeeRepo.Get(x => x.Id == _employeeIdDeptManager).FirstOrDefault()?.EmployeeNameVn;
            }
            commissionData.VerifiedBy = _managerDept;

            string _managerName = string.Empty, _accountantName = string.Empty;
            if (!isOPSReport)
            {
                // Get Accountant Manager
                string _userIdAccountant = GetAccoutantManager(curUser?.CompanyId, curUser?.OfficeId).FirstOrDefault();
                var _employeeIdAcountant = sysUserRepo.Get(x => x.Id == _userIdAccountant).FirstOrDefault()?.EmployeeId;
                _accountantName = sysEmployeeRepo.Get(x => x.Id == _employeeIdAcountant).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;
                // Get Office Manager
                string _userIdOfficeManager = GetOfficeManager(curUser?.CompanyId, curUser?.OfficeId).FirstOrDefault();
                var _employeeIdOfficeManager = sysUserRepo.Get(x => x.Id == _userIdOfficeManager).FirstOrDefault()?.EmployeeId;
                _managerName = sysEmployeeRepo.Get(x => x.Id == _employeeIdOfficeManager).FirstOrDefault()?.EmployeeNameEn ?? string.Empty;
            }
            commissionData.ApprovedBy = isOPSReport ? string.Empty : _managerName;
            commissionData.CrossCheckedBy = isOPSReport ? string.Empty : _accountantName;
            return commissionData;
        }

        /// <summary>
        /// Get data Incentive Report (not require Beneficiary)
        /// </summary>
        /// <param name="criteria">data search</param>
        /// <param name="userId">current user</param>
        /// <returns></returns>
        public CommissionExportResult GetIncentiveReport(CommissionReportCriteria criteria, string userId)
        {
            CommissionExportResult incentiveData = new CommissionExportResult();
            var dataShipment = GetDataGeneralReport(criteria).AsQueryable();
            // Get detail
            if (string.IsNullOrEmpty(criteria.Service) || criteria.Service.Contains(TermData.CustomLogistic))
            {
                if (!string.IsNullOrEmpty(criteria.CustomNo))
                {
                    var clearanceData = customsDeclarationRepo.Get(x => criteria.CustomNo.Contains(x.ClearanceNo)).OrderBy(x => x.ClearanceDate);
                    var clearanceDataGroup = clearanceData.GroupBy(x => x.JobNo).Select(x => x.Key).ToList();
                    dataShipment = dataShipment.Where(x => clearanceDataGroup.Contains(x.JobNo));
                }
            }
            if (dataShipment.Count() == 0)
            {
                return null;
            }
            incentiveData.Details = new List<CommissionDetail>();
            var chargeComId = catChargeGroupRepo.Get(x => x.Name == "Com")?.Select(x => x.Id).FirstOrDefault();
            var listcharge = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE
                                       || x.Type == DocumentConstants.CHARGE_SELL_TYPE).ToLookup(x => x.Hblid);
            var partnerList = catPartnerRepo.Get().ToLookup(x => x.Id);
            foreach (var item in dataShipment)
            {
                incentiveData.Details.Add(new CommissionDetail()
                {
                    ServiceDate = item.ServiceDate,
                    JobId = item.JobNo,
                    HBLNo = item.HwbNo,
                    MBLNo = item.Mawb,
                    CustomerName = partnerList[item.CustomerId].FirstOrDefault().ShortName,
                    CustomSheet = string.Empty,
                    ChargeWeight = 0,
                    PortCode = string.Empty,
                    BuyingRate = GetBuyingRateNoCom((Guid)item.HblId, criteria.Currency),
                    SellingRate = GetSellingRateNoCom((Guid)item.HblId, criteria.Currency),
                    ComAmount = 0
                });
            }
            if (incentiveData.Details.Count() == 0)
            {
                return null;
            }
            // Get header
            var forMonth = string.Empty;
            var listOrder = incentiveData.Details.OrderBy(x => x.ServiceDate);
            if (listOrder.Select(x => x.ServiceDate?.Year).Distinct().Count() == 1)
            {
                var startMonth = listOrder.Select(x => x.ServiceDate).FirstOrDefault();
                var endMonth = listOrder.Select(x => x.ServiceDate).LastOrDefault();
                forMonth = string.Format("{0}{1}", startMonth?.Month == endMonth?.Month ? "" : (startMonth?.ToString("MMM") + " - "), listOrder.Select(x => x.ServiceDate?.ToString("MMM, yyyy")).LastOrDefault());
            }
            else
            {
                var startMonth = listOrder.Select(x => x.ServiceDate).FirstOrDefault();
                var endMonth = listOrder.Select(x => x.ServiceDate).LastOrDefault();
                forMonth = string.Format("{0} - {1}", startMonth?.ToString("MMM, yyyy"), endMonth?.ToString("MMM, yyyy"));
            }
            incentiveData.ForMonth = forMonth;
            incentiveData.CustomerName = catPartnerRepo.Get(x => x.Id == criteria.CustomerId).FirstOrDefault()?.PartnerNameEn;
            incentiveData.ExchangeRate = criteria.ExchangeRate;
            // Partner info
            if (!string.IsNullOrEmpty(criteria.Beneficiary))
            {
                var beneficiaryInfo = catPartnerRepo.Get(x => x.Id == criteria.Beneficiary)?.FirstOrDefault();
                incentiveData.BeneficiaryName = beneficiaryInfo?.PartnerNameVn;
                incentiveData.BankAccountNo = beneficiaryInfo?.BankAccountNo;
                incentiveData.BankName = beneficiaryInfo?.BankAccountName;
                incentiveData.TaxCode = beneficiaryInfo?.TaxCode;
            }
            // get current user
            if (string.IsNullOrEmpty(userId))
            {
                userId = currentUser.UserID;
            }
            var curUser = sysUserLevelRepo.Get(x => x.UserId == userId)?.FirstOrDefault();
            var preparedById = sysUserRepo.Get(x => x.Id == userId).FirstOrDefault()?.EmployeeId;
            incentiveData.PreparedBy = sysEmployeeRepo.Get(x => x.Id == preparedById).FirstOrDefault()?.EmployeeNameEn;
            var managers = GetDeptManager(curUser?.CompanyId, curUser?.OfficeId, curUser?.DepartmentId)?.FirstOrDefault();
            // Get Department manager
            string _employeeIdDeptManager = sysUserRepo.Get(x => x.Id == managers).FirstOrDefault()?.EmployeeId;
            string _managerDept = string.Empty;
            if (_employeeIdDeptManager != null)
            {
                _managerDept = sysEmployeeRepo.Get(x => x.Id == _employeeIdDeptManager).FirstOrDefault()?.EmployeeNameVn;
            }
            incentiveData.VerifiedBy = _managerDept;

            return incentiveData;
        }

        public List<ShipmentAdvanceSettlementModel> GetAdvanceSettlements(Guid Id)
        {

            List<ShipmentAdvanceSettlementModel> results = new List<ShipmentAdvanceSettlementModel>();

            IQueryable<SysUser> users = sysUserRepo.Get();

            OpsTransaction opsJob = opsRepository.Get(x => x.Id == Id)?.FirstOrDefault();
            CsTransaction csJob = DataContext.Get(x => x.Id == Id)?.FirstOrDefault();

            if (opsJob == null && csJob == null)
            {
                return results;
            }

            string jobNo = opsJob == null ? csJob.JobNo : opsJob.JobNo;
            List<sp_GetAdvanceSettleOpsTransaction> dta = GetAdvanceSettleByJobNo(jobNo);
            if (dta.Count > 0)
            {
                results = dta.Select(s => new ShipmentAdvanceSettlementModel
                {
                    SettlementCode = s.SettlementCode,
                    SettlementDate = s.SettlementDate,
                    SettlementCurrency = s.SettlementCurrency,
                    SettlementAmount = s.SettlementAmount,
                    SettleStatusApproval = s.SettleStatusApproval,
                    SettleRequester = s.SettleRequester,

                    AdvanceNo = s.AdvanceNo,
                    AdvanceDate = s.AdvanceDate,
                    AdvanceCurrency = s.AdvanceCurrency,
                    AdvanceAmount = s.AdvanceAmount,
                    AdvanceStatusApproval = s.AdvanceStatusApproval,
                    Balance = s.SettlementAmount - s.AdvanceAmount,
                    AdvRequester = s.AdvRequester,
                    AdvanceSyncStatus = s.AdvanceSyncStatus,
                    SettlementSyncStatus = s.SettlementSyncStatus,
                    AdvanceVoucherNo = s.AdvanceVoucherNo,
                    SettlementVoucherNo = s.SettlementVoucherNo,
                    AdvanceVoucherDate = s.AdvanceVoucherDate,
                    SettlementVoucherDate = s.SettlementVoucherDate
                }).ToList();
            }

            return results;
        }


        private List<sp_GetAdvanceSettleOpsTransaction> GetAdvanceSettleByJobNo(string jobNo)
        {
            DbParameter[] parameters =
            {
                SqlParam.GetParameter("jobNo", jobNo)
            };
            return ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetAdvanceSettleOpsTransaction>(parameters);
        }
        #endregion

        public List<sp_GetAllShipment> GetAllShipment(string keyword)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@KEYWORD", Value = keyword }
            };
            var shipments = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetAllShipment>(parameters);
            return shipments.ToList();
        }

    }
}
