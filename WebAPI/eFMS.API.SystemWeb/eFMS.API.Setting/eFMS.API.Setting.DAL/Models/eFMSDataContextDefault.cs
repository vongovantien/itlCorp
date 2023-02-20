﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace eFMS.API.Setting.Service.Models
{
    public partial class eFMSDataContextDefault : DbContext
    {
        public eFMSDataContextDefault()
        {
        }

        public eFMSDataContextDefault(DbContextOptions<eFMSDataContextDefault> options)
            : base(options)
        {
        }

        public virtual DbSet<AccAccountPayable> AccAccountPayable { get; set; }
        public virtual DbSet<AccAccountingManagement> AccAccountingManagement { get; set; }
        public virtual DbSet<AcctAdvancePayment> AcctAdvancePayment { get; set; }
        public virtual DbSet<AcctApproveAdvance> AcctApproveAdvance { get; set; }
        public virtual DbSet<AcctApproveSettlement> AcctApproveSettlement { get; set; }
        public virtual DbSet<AcctCdnote> AcctCdnote { get; set; }
        public virtual DbSet<AcctDebitManagementAr> AcctDebitManagementAr { get; set; }
        public virtual DbSet<AcctReceipt> AcctReceipt { get; set; }
        public virtual DbSet<AcctReceiptSync> AcctReceiptSync { get; set; }
        public virtual DbSet<AcctSettlementPayment> AcctSettlementPayment { get; set; }
        public virtual DbSet<AcctSoa> AcctSoa { get; set; }
        public virtual DbSet<CatCharge> CatCharge { get; set; }
        public virtual DbSet<CatCommodityGroup> CatCommodityGroup { get; set; }
        public virtual DbSet<CatDepartment> CatDepartment { get; set; }
        public virtual DbSet<CatPartner> CatPartner { get; set; }
        public virtual DbSet<CatPlace> CatPlace { get; set; }
        public virtual DbSet<CsRuleLinkFee> CsRuleLinkFee { get; set; }
        public virtual DbSet<CsShipmentSurcharge> CsShipmentSurcharge { get; set; }
        public virtual DbSet<CsTransaction> CsTransaction { get; set; }
        public virtual DbSet<CsTransactionDetail> CsTransactionDetail { get; set; }
        public virtual DbSet<CustomsDeclaration> CustomsDeclaration { get; set; }
        public virtual DbSet<OpsTransaction> OpsTransaction { get; set; }
        public virtual DbSet<SetTariff> SetTariff { get; set; }
        public virtual DbSet<SetTariffDetail> SetTariffDetail { get; set; }
        public virtual DbSet<SetUnlockRequest> SetUnlockRequest { get; set; }
        public virtual DbSet<SetUnlockRequestApprove> SetUnlockRequestApprove { get; set; }
        public virtual DbSet<SetUnlockRequestJob> SetUnlockRequestJob { get; set; }
        public virtual DbSet<SysAttachFileTemplate> SysAttachFileTemplate { get; set; }
        public virtual DbSet<SysAuthorizedApproval> SysAuthorizedApproval { get; set; }
        public virtual DbSet<SysCompany> SysCompany { get; set; }
        public virtual DbSet<SysEmployee> SysEmployee { get; set; }
        public virtual DbSet<SysGroup> SysGroup { get; set; }
        public virtual DbSet<SysImage> SysImage { get; set; }
        public virtual DbSet<SysImageDetail> SysImageDetail { get; set; }
        public virtual DbSet<SysOffice> SysOffice { get; set; }
        public virtual DbSet<SysSentEmailHistory> SysSentEmailHistory { get; set; }
        public virtual DbSet<SysSettingFlow> SysSettingFlow { get; set; }
        public virtual DbSet<SysUser> SysUser { get; set; }
        public virtual DbSet<SysUserLevel> SysUserLevel { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=192.168.0.120; Database=eFMS_20220617; User ID=eFMS-Admin; Password=eFMS@dm!n20");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity<AccAccountPayable>(entity =>
            {
                entity.ToTable("accAccountPayable");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AcctManagementId)
                    .HasColumnName("AcctManagementID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BillingNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.BillingType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.Currency)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.ExchangeRate).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.InvoiceDate).HasColumnType("datetime");

                entity.Property(e => e.InvoiceNo).HasMaxLength(50);

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.Over16To30Day).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Over1To15Day).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Over30Day).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PartnerId)
                    .HasColumnName("PartnerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaymentAmountUsd)
                    .HasColumnName("PaymentAmountUSD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaymentAmountVnd)
                    .HasColumnName("PaymentAmountVND")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaymentDueDate).HasColumnType("datetime");

                entity.Property(e => e.PaymentTerm).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReferenceNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.RemainAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RemainAmountUsd)
                    .HasColumnName("RemainAmountUSD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RemainAmountVnd)
                    .HasColumnName("RemainAmountVND")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalAmountUsd)
                    .HasColumnName("TotalAmountUSD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalAmountVnd)
                    .HasColumnName("TotalAmountVND")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransactionType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VoucherDate).HasColumnType("datetime");

                entity.Property(e => e.VoucherNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AccAccountingManagement>(entity =>
            {
                entity.ToTable("accAccountingManagement");

                entity.HasIndex(e => e.PartnerId)
                    .HasName("Index_AccMngt");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountNo).HasMaxLength(250);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.ConfirmBillingDate).HasColumnType("datetime");

                entity.Property(e => e.Currency)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.LastSyncDate).HasColumnType("datetime");

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaidAmountUsd)
                    .HasColumnName("PaidAmountUSD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaidAmountVnd)
                    .HasColumnName("PaidAmountVND")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PartnerId)
                    .IsRequired()
                    .HasColumnName("PartnerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentDatetimeUpdated).HasColumnType("datetime");

                entity.Property(e => e.PaymentDueDate).HasColumnType("datetime");

                entity.Property(e => e.PaymentMethod).HasMaxLength(50);

                entity.Property(e => e.PaymentNote).HasMaxLength(500);

                entity.Property(e => e.PaymentStatus).HasMaxLength(50);

                entity.Property(e => e.PaymentTerm).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReferenceNo).HasMaxLength(100);

                entity.Property(e => e.SalesmanId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceType).HasMaxLength(100);

                entity.Property(e => e.SourceCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.SyncStatus).HasMaxLength(50);

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalAmountUsd)
                    .HasColumnName("TotalAmountUSD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalAmountVnd)
                    .HasColumnName("TotalAmountVND")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalExchangeRate).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransactionType)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Type).HasMaxLength(50);

                entity.Property(e => e.UnpaidAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnpaidAmountUsd)
                    .HasColumnName("UnpaidAmountUSD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnpaidAmountVnd)
                    .HasColumnName("UnpaidAmountVND")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VoucherId).HasColumnName("VoucherID");

                entity.Property(e => e.VoucherType).HasMaxLength(50);
            });

            modelBuilder.Entity<AccAccountPayable>(entity =>
            {
                entity.ToTable("accAccountPayable");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BillingNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.BillingType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.Currency)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.ExchangeRate).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.InvoiceDate).HasColumnType("datetime");

                entity.Property(e => e.InvoiceNo).HasMaxLength(50);

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.Over16To30Day).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Over1To15Day).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Over30Day).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PartnerId)
                    .HasColumnName("PartnerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaymentAmountUsd)
                    .HasColumnName("PaymentAmountUSD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaymentAmountVnd)
                    .HasColumnName("PaymentAmountVND")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaymentDueDate).HasColumnType("datetime");

                entity.Property(e => e.PaymentTerm).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReferenceNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.RemainAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RemainAmountUsd)
                    .HasColumnName("RemainAmountUSD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RemainAmountVnd)
                    .HasColumnName("RemainAmountVND")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalAmountUsd)
                    .HasColumnName("TotalAmountUSD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalAmountVnd)
                    .HasColumnName("TotalAmountVND")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransactionType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VoucherDate).HasColumnType("datetime");

                entity.Property(e => e.VoucherNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AccAccountingManagement>(entity =>
            {
                entity.ToTable("accAccountingManagement");

                entity.HasIndex(e => e.PartnerId)
                    .HasName("Index_AccMngt");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountNo).HasMaxLength(250);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.ConfirmBillingDate).HasColumnType("datetime");

                entity.Property(e => e.Currency)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.LastSyncDate).HasColumnType("datetime");

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaidAmountUsd)
                    .HasColumnName("PaidAmountUSD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaidAmountVnd)
                    .HasColumnName("PaidAmountVND")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PartnerId)
                    .IsRequired()
                    .HasColumnName("PartnerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentDatetimeUpdated).HasColumnType("datetime");

                entity.Property(e => e.PaymentDueDate).HasColumnType("datetime");

                entity.Property(e => e.PaymentMethod).HasMaxLength(50);

                entity.Property(e => e.PaymentNote).HasMaxLength(500);

                entity.Property(e => e.PaymentStatus).HasMaxLength(50);

                entity.Property(e => e.PaymentTerm).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReferenceNo).HasMaxLength(100);

                entity.Property(e => e.SalesmanId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceType).HasMaxLength(100);

                entity.Property(e => e.SourceCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.SyncStatus).HasMaxLength(50);

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalAmountUsd)
                    .HasColumnName("TotalAmountUSD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalAmountVnd)
                    .HasColumnName("TotalAmountVND")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalExchangeRate).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransactionType)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Type).HasMaxLength(50);

                entity.Property(e => e.UnpaidAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnpaidAmountUsd)
                    .HasColumnName("UnpaidAmountUSD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnpaidAmountVnd)
                    .HasColumnName("UnpaidAmountVND")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VoucherId).HasColumnName("VoucherID");

                entity.Property(e => e.VoucherType).HasMaxLength(50);
            });

            modelBuilder.Entity<AcctAdvancePayment>(entity =>
            {
                entity.ToTable("acctAdvancePayment");

                entity.HasIndex(e => e.AdvanceNo)
                    .HasName("Index_AccAdv")
                    .IsUnique();

                entity.HasIndex(e => e.DatetimeCreated)
                    .HasName("Idx_DatetimeCreated_acctAdvancePayment");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AdvanceCurrency).HasMaxLength(10);

                entity.Property(e => e.AdvanceFor)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.AdvanceNo)
                    .IsRequired()
                    .HasMaxLength(11)
                    .IsUnicode(false);

                entity.Property(e => e.BankAccountName).HasMaxLength(150);

                entity.Property(e => e.BankAccountNo).HasMaxLength(150);

                entity.Property(e => e.BankCode)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.BankName).HasMaxLength(150);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DeadlinePayment).HasColumnType("date");

                entity.Property(e => e.Department)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.DueDate).HasColumnType("datetime");

                entity.Property(e => e.ExcRateUsdToLocal).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.LastSyncDate).HasColumnType("datetime");

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.Payee)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentMethod)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentTerm).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.RequestDate).HasColumnType("date");

                entity.Property(e => e.Requester)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StatusApproval)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SyncStatus).HasMaxLength(50);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VoucherDate).HasColumnType("datetime");

                entity.Property(e => e.VoucherNo)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AcctApproveAdvance>(entity =>
            {
                entity.ToTable("acctApproveAdvance");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Accountant)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantApr)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantAprDate).HasColumnType("datetime");

                entity.Property(e => e.AdvanceNo)
                    .HasMaxLength(11)
                    .IsUnicode(false);

                entity.Property(e => e.Buhead)
                    .HasColumnName("BUHead")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BuheadApr)
                    .HasColumnName("BUHeadApr")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BuheadAprDate)
                    .HasColumnName("BUHeadAprDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.DateCreated).HasColumnType("datetime");

                entity.Property(e => e.DateModified).HasColumnType("datetime");

                entity.Property(e => e.Leader)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LeaderApr)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LeaderAprDate).HasColumnType("datetime");

                entity.Property(e => e.LevelApprove).HasMaxLength(50);

                entity.Property(e => e.Manager)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ManagerApr)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ManagerAprDate).HasColumnType("datetime");

                entity.Property(e => e.Requester)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RequesterAprDate).HasColumnType("datetime");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AcctApproveSettlement>(entity =>
            {
                entity.ToTable("acctApproveSettlement");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Accountant)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantApr)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantAprDate).HasColumnType("datetime");

                entity.Property(e => e.Buhead)
                    .HasColumnName("BUHead")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BuheadApr)
                    .HasColumnName("BUHeadApr")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BuheadAprDate)
                    .HasColumnName("BUHeadAprDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.DateCreated).HasColumnType("datetime");

                entity.Property(e => e.DateModified).HasColumnType("datetime");

                entity.Property(e => e.Leader)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LeaderApr)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LeaderAprDate).HasColumnType("datetime");

                entity.Property(e => e.LevelApprove).HasMaxLength(50);

                entity.Property(e => e.Manager)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ManagerApr)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ManagerAprDate).HasColumnType("datetime");

                entity.Property(e => e.Requester)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RequesterAprDate).HasColumnType("datetime");

                entity.Property(e => e.SettlementNo)
                    .HasMaxLength(11)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AcctCdnote>(entity =>
            {
                entity.ToTable("acctCDNote");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BehalfPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CombineBillingNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerConfirmDate).HasColumnType("datetime");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.ExcRateUsdToLocal).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ExchangeRate).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ExportedDate).HasColumnType("datetime");

                entity.Property(e => e.FlexId).HasColumnName("FlexID");

                entity.Property(e => e.FreightPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.InvoiceNo).HasMaxLength(100);

                entity.Property(e => e.JobId).HasColumnName("JobID");

                entity.Property(e => e.LastSyncDate).HasColumnType("datetime");

                entity.Property(e => e.NetOff).HasDefaultValueSql("((0))");

                entity.Property(e => e.Note).HasMaxLength(500);

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.PaidBehalfPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaidDate).HasColumnType("datetime");

                entity.Property(e => e.PaidFreightPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaidPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PartnerId)
                    .IsRequired()
                    .HasColumnName("PartnerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentDueDate).HasColumnType("datetime");

                entity.Property(e => e.SalemanId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SentByUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SentOn).HasColumnType("datetime");

                entity.Property(e => e.StatementDate).HasColumnType("datetime");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.SyncStatus).HasMaxLength(50);

                entity.Property(e => e.Total).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TrackingTransportBill)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TrackingTransportDate).HasColumnType("datetime");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UnlockedDirector)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UnlockedDirectorDate).HasColumnType("datetime");

                entity.Property(e => e.UnlockedDirectorStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UnlockedSaleMan)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UnlockedSaleManDate).HasColumnType("datetime");

                entity.Property(e => e.UnlockedSaleManStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AcctDebitManagementAr>(entity =>
            {
                entity.ToTable("acctDebitManagementAR");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.AcctManagementId).HasColumnName("AcctManagementID");

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.Hblid).HasColumnName("HBLID");

                entity.Property(e => e.OfficeId)
                    .HasColumnName("OfficeID")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaidAmountUsd)
                    .HasColumnName("PaidAmountUSD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaidAmountVnd)
                    .HasColumnName("PaidAmountVND")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PartnerId)
                    .HasColumnName("PartnerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentStatus).HasMaxLength(50);

                entity.Property(e => e.RefNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalAmountUsd)
                    .HasColumnName("TotalAmountUSD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalAmountVnd)
                    .HasColumnName("TotalAmountVND")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Type)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.UnpaidAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnpaidAmountUsd)
                    .HasColumnName("UnpaidAmountUSD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnpaidAmountVnd)
                    .HasColumnName("UnpaidAmountVND")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AcctReceipt>(entity =>
            {
                entity.ToTable("acctReceipt");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AgreementAdvanceAmountUsd).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.AgreementAdvanceAmountVnd).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.AgreementId).HasColumnName("AgreementID");

                entity.Property(e => e.Balance).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.BankAccountNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Class)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.CreditAmountUsd).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CreditAmountVnd).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CurrencyId)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CusAdvanceAmountUsd).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CusAdvanceAmountVnd).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CustomerId)
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.ExchangeRate).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FinalPaidAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FinalPaidAmountUsd).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FinalPaidAmountVnd).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FromDate).HasColumnType("datetime");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.LastSyncDate).HasColumnType("datetime");

                entity.Property(e => e.NotifyDepartment)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ObhpartnerId).HasColumnName("OBHPartnerID");

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaidAmountUsd).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaidAmountVnd).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaymentDate).HasColumnType("datetime");

                entity.Property(e => e.PaymentMethod).HasMaxLength(50);

                entity.Property(e => e.PaymentRefNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReferenceId).HasColumnName("ReferenceID");

                entity.Property(e => e.ReferenceNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SyncStatus).HasMaxLength(50);

                entity.Property(e => e.ToDate).HasColumnType("datetime");

                entity.Property(e => e.Type)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AcctReceiptSync>(entity =>
            {
                entity.ToTable("acctReceiptSync");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.LastSyncDate).HasColumnType("datetime");

                entity.Property(e => e.ReceiptId).HasColumnName("ReceiptID");

                entity.Property(e => e.ReceiptSyncNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SyncStatus).HasMaxLength(50);

                entity.Property(e => e.Type)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AcctSettlementPayment>(entity =>
            {
                entity.ToTable("acctSettlementPayment");

                entity.HasIndex(e => e.DatetimeCreated)
                    .HasName("Idx_DatetimeCreated_acctSettlementPayment");

                entity.HasIndex(e => e.SettlementNo)
                    .HasName("Index_Settle")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AdvanceAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.BalanceAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.BankAccountName).HasMaxLength(150);

                entity.Property(e => e.BankAccountNo).HasMaxLength(150);

                entity.Property(e => e.BankCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BankName).HasMaxLength(150);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.DueDate).HasColumnType("datetime");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.LastSyncDate).HasColumnType("datetime");

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.Payee)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentMethod)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RequestDate).HasColumnType("date");

                entity.Property(e => e.Requester)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SettlementCurrency).HasMaxLength(10);

                entity.Property(e => e.SettlementNo)
                    .IsRequired()
                    .HasMaxLength(11)
                    .IsUnicode(false);

                entity.Property(e => e.SettlementType)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.StatusApproval)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SyncStatus).HasMaxLength(50);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VoucherDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<AcctSoa>(entity =>
            {
                entity.ToTable("acctSOA");

                entity.HasIndex(e => e.DatetimeCreated)
                    .HasName("Idx_DatetimeCreated_acctSOA");

                entity.HasIndex(e => e.Soano)
                    .HasName("AcctSOA")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.CombineBillingNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CommodityGroupId).HasColumnName("CommodityGroupID");

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.CreatorShipment).IsUnicode(false);

                entity.Property(e => e.CreditAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Currency).HasMaxLength(10);

                entity.Property(e => e.Customer)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DateType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DebitAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.ExcRateUsdToLocal).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.LastSyncDate).HasColumnType("datetime");

                entity.Property(e => e.NetOff).HasDefaultValueSql("((0))");

                entity.Property(e => e.Obh).HasColumnName("OBH");

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.PaymentDatetimeUpdated).HasColumnType("datetime");

                entity.Property(e => e.PaymentDueDate).HasColumnType("datetime");

                entity.Property(e => e.PaymentNote).HasMaxLength(500);

                entity.Property(e => e.PaymentStatus).HasMaxLength(50);

                entity.Property(e => e.SalemanId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceTypeId)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.SoaformDate)
                    .HasColumnName("SOAFormDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Soano)
                    .HasColumnName("SOANo")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.SoatoDate)
                    .HasColumnName("SOAToDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.StaffType).HasMaxLength(50);

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SyncStatus).HasMaxLength(50);

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatCharge>(entity =>
            {
                entity.ToTable("catCharge");

                entity.HasIndex(e => e.Code)
                    .HasName("Index_CatCharge")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.ChargeNameEn)
                    .HasColumnName("ChargeName_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.ChargeNameVn)
                    .IsRequired()
                    .HasColumnName("ChargeName_VN")
                    .HasMaxLength(250);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.Mode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.Offices).HasMaxLength(250);

                entity.Property(e => e.ProductDept)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceTypeId)
                    .HasColumnName("ServiceTypeID")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Vatrate)
                    .HasColumnName("VATRate")
                    .HasColumnType("decimal(18, 4)");
            });

            modelBuilder.Entity<CatCommodityGroup>(entity =>
            {
                entity.ToTable("catCommodityGroup");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.GroupNameEn)
                    .HasColumnName("GroupName_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.GroupNameVn)
                    .HasColumnName("GroupName_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Note).HasMaxLength(4000);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatDepartment>(entity =>
            {
                entity.ToTable("catDepartment");

                entity.HasIndex(e => e.Code)
                    .HasName("U_catDepartment_Code")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DeptName).HasMaxLength(1600);

                entity.Property(e => e.DeptType).HasMaxLength(50);

                entity.Property(e => e.Description).HasMaxLength(4000);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.SignPath).HasMaxLength(1000);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatPartner>(entity =>
            {
                entity.ToTable("catPartner");

                entity.HasIndex(e => e.AccountNo)
                    .HasName("Index_CatPartner")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountNo)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.AddressEn).HasColumnName("Address_EN");

                entity.Property(e => e.AddressShippingEn).HasColumnName("AddressShipping_EN");

                entity.Property(e => e.AddressShippingVn).HasColumnName("AddressShipping_VN");

                entity.Property(e => e.AddressVn).HasColumnName("Address_VN");

                entity.Property(e => e.ApplyDim)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BankAccountName).HasMaxLength(4000);

                entity.Property(e => e.BankAccountNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BankCode)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.BankName).HasMaxLength(200);

                entity.Property(e => e.BillingEmail).IsUnicode(false);

                entity.Property(e => e.BillingPhone).IsUnicode(false);

                entity.Property(e => e.CoLoaderCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.ContactPerson).HasMaxLength(4000);

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.CountryShippingId).HasColumnName("CountryShippingID");

                entity.Property(e => e.CreditAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CreditPayment)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Currency)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DebitAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.Email).IsUnicode(false);

                entity.Property(e => e.Fax)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.InternalCode).HasMaxLength(250);

                entity.Property(e => e.InternalReferenceNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Note).HasMaxLength(4000);

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.ParentId)
                    .HasColumnName("ParentID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PartnerGroup)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PartnerLocation)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.PartnerMode)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.PartnerNameEn)
                    .HasColumnName("PartnerName_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.PartnerNameVn)
                    .HasColumnName("PartnerName_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.PartnerType)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentBeneficiary).HasMaxLength(4000);

                entity.Property(e => e.PercentCredit).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");

                entity.Property(e => e.ProvinceShippingId).HasColumnName("ProvinceShippingID");

                entity.Property(e => e.ReceiveEtaemail).HasColumnName("ReceiveETAEmail");

                entity.Property(e => e.RoundUpMethod)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RoundedSoamethod)
                    .HasColumnName("RoundedSOAMethod")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SalePersonId)
                    .HasColumnName("SalePersonID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShortName).HasMaxLength(4000);

                entity.Property(e => e.SugarId)
                    .HasColumnName("SugarID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SwiftCode)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.TaxCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Tel)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Website)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPhoneEx)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");

                entity.Property(e => e.ZipCode)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.ZipCodeShipping)
                    .HasMaxLength(150)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatPlace>(entity =>
            {
                entity.ToTable("catPlace");

                entity.HasIndex(e => new { e.Code, e.PlaceTypeId })
                    .HasName("U_Place_Code")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Address).HasMaxLength(1600);

                entity.Property(e => e.AreaId)
                    .HasColumnName("AreaID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Code)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.DisplayName).HasMaxLength(4000);

                entity.Property(e => e.DistrictId).HasColumnName("DistrictID");

                entity.Property(e => e.GeoCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.LocalAreaId)
                    .HasColumnName("LocalAreaID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModeOfTransport)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.Note).HasMaxLength(4000);

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.PlaceTypeId)
                    .HasColumnName("PlaceTypeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");
            });

            modelBuilder.Entity<CsRuleLinkFee>(entity =>
            {
                entity.ToTable("csRuleLinkFee");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ChargeBuying)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.ChargeSelling)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.EffectiveDate).HasColumnType("datetime");

                entity.Property(e => e.ExpiredDate).HasColumnType("datetime");

                entity.Property(e => e.PartnerBuying)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.PartnerSelling)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.RuleName).HasMaxLength(200);

                entity.Property(e => e.ServiceBuying)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceSelling)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsShipmentSurcharge>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("csShipmentBuyingRate_PK")
                    .ForSqlServerIsClustered(false);

                entity.ToTable("csShipmentSurcharge");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AcctManagementId).HasColumnName("AcctManagementID");

                entity.Property(e => e.AdvanceNo).HasMaxLength(11);

                entity.Property(e => e.AdvanceNoFor)
                    .HasMaxLength(11)
                    .IsUnicode(false);

                entity.Property(e => e.AmountUsd)
                    .HasColumnName("AmountUSD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.AmountVnd)
                    .HasColumnName("AmountVND")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Cdclosed)
                    .HasColumnName("CDClosed")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.ChargeId).HasColumnName("ChargeID");

                entity.Property(e => e.ClearanceNo).HasMaxLength(100);

                entity.Property(e => e.CombineBillingNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.ContNo).HasMaxLength(200);

                entity.Property(e => e.CreditNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .IsRequired()
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DebitNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ExchangeDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FinalExchangeRate).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Hblid).HasColumnName("HBLID");

                entity.Property(e => e.Hblno)
                    .HasColumnName("HBLNo")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.InvoiceDate).HasColumnType("datetime");

                entity.Property(e => e.InvoiceNo).HasMaxLength(50);

                entity.Property(e => e.IsFromShipment).HasDefaultValueSql("((1))");

                entity.Property(e => e.JobNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Kb)
                    .HasColumnName("KB")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.LinkChargeId).HasMaxLength(250);

                entity.Property(e => e.Mblno)
                    .HasColumnName("MBLNo")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDateLinkFee).HasColumnType("datetime");

                entity.Property(e => e.NetAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.ObhcombineBillingNo)
                    .HasColumnName("OBHCombineBillingNo")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ObjectBePaid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.PaySoano)
                    .HasColumnName("PaySOANo")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaySyncedFrom)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PayerAcctManagementId).HasColumnName("PayerAcctManagementID");

                entity.Property(e => e.PayerId)
                    .HasColumnName("PayerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentObjectId)
                    .HasColumnName("PaymentObjectID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentRefNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentRequestType)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.QuantityType)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ReferenceNo).HasMaxLength(100);

                entity.Property(e => e.SeriesNo).HasMaxLength(50);

                entity.Property(e => e.SettlementCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Soaclosed)
                    .HasColumnName("SOAClosed")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Soano)
                    .HasColumnName("SOANo")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SyncedFrom)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Total).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransactionType).HasMaxLength(10);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TypeOfFee)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserNameLinkFee)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VatAmountUsd)
                    .HasColumnName("VatAmountUSD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.VatAmountVnd)
                    .HasColumnName("VatAmountVND")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.VatPartnerId)
                    .HasColumnName("VatPartnerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Vatrate)
                    .HasColumnName("VATRate")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.VoucherId)
                    .HasColumnName("VoucherID")
                    .HasMaxLength(250);

                entity.Property(e => e.VoucherIddate)
                    .HasColumnName("VoucherIDDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.VoucherIdre)
                    .HasColumnName("VoucherIDRE")
                    .HasMaxLength(250);

                entity.Property(e => e.VoucherIdredate)
                    .HasColumnName("VoucherIDREDate")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<CsTransaction>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("csShipment_PK")
                    .ForSqlServerIsClustered(false);

                entity.ToTable("csTransaction");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.AgentId)
                    .HasColumnName("AgentID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AirlineInfo).HasMaxLength(800);

                entity.Property(e => e.Ata)
                    .HasColumnName("ATA")
                    .HasColumnType("datetime");

                entity.Property(e => e.Atd)
                    .HasColumnName("ATD")
                    .HasColumnType("datetime");

                entity.Property(e => e.BookingNo).HasMaxLength(800);

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.Cbm)
                    .HasColumnName("CBM")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ChargeWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ColoaderId)
                    .HasColumnName("ColoaderID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Commodity).HasMaxLength(1600);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.CurrentStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.Eta)
                    .HasColumnName("ETA")
                    .HasColumnType("datetime");

                entity.Property(e => e.Etd)
                    .HasColumnName("ETD")
                    .HasColumnType("datetime");

                entity.Property(e => e.FlightDate).HasColumnType("datetime");

                entity.Property(e => e.FlightVesselName).HasMaxLength(4000);

                entity.Property(e => e.GrossWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.Hw)
                    .HasColumnName("HW")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Hwconstant)
                    .HasColumnName("HWConstant")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.IsLocked).HasDefaultValueSql("((0))");

                entity.Property(e => e.IssuedBy).HasMaxLength(50);

                entity.Property(e => e.JobNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastDateUnLocked).HasColumnType("datetime");

                entity.Property(e => e.LockedDate).HasColumnType("datetime");

                entity.Property(e => e.LockedUser).HasMaxLength(50);

                entity.Property(e => e.Mawb)
                    .HasColumnName("MAWB")
                    .HasMaxLength(800);

                entity.Property(e => e.Mbltype)
                    .HasColumnName("MBLType")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.NetWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.PackageContainer).HasMaxLength(1600);

                entity.Property(e => e.PaymentTerm).HasMaxLength(1600);

                entity.Property(e => e.PersonIncharge)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Pod).HasColumnName("POD");

                entity.Property(e => e.PodDescription).HasMaxLength(150);

                entity.Property(e => e.Pol).HasColumnName("POL");

                entity.Property(e => e.PolDescription).HasMaxLength(150);

                entity.Property(e => e.Pono)
                    .HasColumnName("PONo")
                    .HasMaxLength(1600);

                entity.Property(e => e.Route).HasMaxLength(100);

                entity.Property(e => e.ServiceDate).HasColumnType("datetime");

                entity.Property(e => e.ShipmentType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SubColoader).HasMaxLength(800);

                entity.Property(e => e.TransactionType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TypeOfService)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VoyNo).HasMaxLength(1600);

                entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");
            });

            modelBuilder.Entity<CsTransactionDetail>(entity =>
            {
                entity.ToTable("csTransactionDetail");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.AlsoNotifyPartyDescription).HasMaxLength(500);

                entity.Property(e => e.AlsoNotifyPartyId)
                    .HasColumnName("AlsoNotifyPartyID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ArrivalDate).HasColumnType("datetime");

                entity.Property(e => e.ArrivalFirstNotice).HasColumnType("datetime");

                entity.Property(e => e.ArrivalNo).HasMaxLength(100);

                entity.Property(e => e.ArrivalSecondNotice).HasColumnType("datetime");

                entity.Property(e => e.Cbm)
                    .HasColumnName("CBM")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CcchargeInDrc)
                    .HasColumnName("CCChargeInDrc")
                    .IsUnicode(false);

                entity.Property(e => e.ChargeWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Chgs)
                    .HasColumnName("CHGS")
                    .HasMaxLength(250);

                entity.Property(e => e.ClosingDate).HasColumnType("datetime");

                entity.Property(e => e.ColoaderId)
                    .HasColumnName("ColoaderID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ComItemNo).HasMaxLength(250);

                entity.Property(e => e.Commodity).HasMaxLength(1600);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.ConsigneeDescription).HasMaxLength(500);

                entity.Property(e => e.ConsigneeId)
                    .HasColumnName("ConsigneeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrConvertRate).HasMaxLength(250);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerId)
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomsBookingNo).HasMaxLength(800);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Dclrca)
                    .HasColumnName("DCLRCA")
                    .HasMaxLength(250);

                entity.Property(e => e.Dclrcus)
                    .HasColumnName("DCLRCUS")
                    .HasMaxLength(250);

                entity.Property(e => e.DeliveryDate).HasColumnType("datetime");

                entity.Property(e => e.DeliveryOrderNo).HasMaxLength(100);

                entity.Property(e => e.DeliveryOrderPrintedDate).HasColumnType("datetime");

                entity.Property(e => e.DeliveryPerson).HasMaxLength(250);

                entity.Property(e => e.DeliveryPlace).HasMaxLength(500);

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.DocumentDate).HasColumnType("datetime");

                entity.Property(e => e.DocumentNo).HasMaxLength(500);

                entity.Property(e => e.Dofooter).HasColumnName("DOFooter");

                entity.Property(e => e.DosentTo1)
                    .HasColumnName("DOSentTo1")
                    .HasMaxLength(250);

                entity.Property(e => e.DosentTo2)
                    .HasColumnName("DOSentTo2")
                    .HasMaxLength(250);

                entity.Property(e => e.DueAgentCll)
                    .HasColumnName("DueAgentCLL")
                    .IsUnicode(false);

                entity.Property(e => e.DueAgentPp)
                    .HasColumnName("DueAgentPP")
                    .IsUnicode(false);

                entity.Property(e => e.DueCarrierCll)
                    .HasColumnName("DueCarrierCLL")
                    .IsUnicode(false);

                entity.Property(e => e.DueCarrierPp)
                    .HasColumnName("DueCarrierPP")
                    .IsUnicode(false);

                entity.Property(e => e.Eta)
                    .HasColumnName("ETA")
                    .HasColumnType("datetime");

                entity.Property(e => e.Etawarehouse)
                    .HasColumnName("ETAWarehouse")
                    .HasColumnType("datetime");

                entity.Property(e => e.Etd)
                    .HasColumnName("ETD")
                    .HasColumnType("datetime");

                entity.Property(e => e.ExportReferenceNo).HasMaxLength(200);

                entity.Property(e => e.FinalDestinationPlace).HasMaxLength(500);

                entity.Property(e => e.FinalPod).HasColumnName("FinalPOD");

                entity.Property(e => e.FirstCarrierBy).HasMaxLength(250);

                entity.Property(e => e.FirstCarrierTo).HasMaxLength(250);

                entity.Property(e => e.FlexId).HasColumnName("FlexID");

                entity.Property(e => e.FlightDate).HasColumnType("datetime");

                entity.Property(e => e.FlightDateOrigin).HasColumnType("datetime");

                entity.Property(e => e.FlightNo).HasMaxLength(250);

                entity.Property(e => e.FlightNoOrigin).HasMaxLength(250);

                entity.Property(e => e.ForwardingAgentDescription).HasMaxLength(500);

                entity.Property(e => e.ForwardingAgentId)
                    .HasColumnName("ForwardingAgentID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FreightPayment)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.GoodsDeliveryId)
                    .HasColumnName("GoodsDeliveryID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.GrossWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.Hbltype)
                    .HasColumnName("HBLType")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Hw)
                    .HasColumnName("HW")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.HwConstant).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Hwbno)
                    .HasColumnName("HWBNo")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.InWord).HasMaxLength(4000);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.IssueHbldate)
                    .HasColumnName("IssueHBLDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.IssueHblplace)
                    .HasColumnName("IssueHBLPlace")
                    .HasMaxLength(500);

                entity.Property(e => e.IssuedBy).HasMaxLength(250);

                entity.Property(e => e.IssuranceAmount).HasMaxLength(250);

                entity.Property(e => e.JobId).HasColumnName("JobID");

                entity.Property(e => e.KgIb).HasMaxLength(250);

                entity.Property(e => e.LocalVessel).HasMaxLength(500);

                entity.Property(e => e.LocalVoyNo).HasMaxLength(800);

                entity.Property(e => e.ManifestRefNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Mawb)
                    .HasColumnName("MAWB")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.MoveType).HasMaxLength(160);

                entity.Property(e => e.NetWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.NotifyPartyDescription).HasMaxLength(500);

                entity.Property(e => e.NotifyPartyId)
                    .HasColumnName("NotifyPartyID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OceanVessel).HasMaxLength(500);

                entity.Property(e => e.OceanVoyNo).HasMaxLength(800);

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.OnBoardStatus).HasMaxLength(4000);

                entity.Property(e => e.OriginBlnumber).HasColumnName("OriginBLNumber");

                entity.Property(e => e.OriginCountryId).HasColumnName("OriginCountryID");

                entity.Property(e => e.OtherCharge).HasMaxLength(250);

                entity.Property(e => e.OtherPayment)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.PackageContainer).HasMaxLength(1600);

                entity.Property(e => e.ParentId).HasColumnName("ParentID");

                entity.Property(e => e.PickupPlace).HasMaxLength(500);

                entity.Property(e => e.PlaceFreightPay).HasMaxLength(4000);

                entity.Property(e => e.Pod).HasColumnName("POD");

                entity.Property(e => e.PodDescription).HasMaxLength(400);

                entity.Property(e => e.PoinvoiceNo)
                    .HasColumnName("POInvoiceNo")
                    .HasMaxLength(250);

                entity.Property(e => e.Pol).HasColumnName("POL");

                entity.Property(e => e.PolDescription).HasMaxLength(400);

                entity.Property(e => e.PurchaseOrderNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RateCharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Rclass)
                    .HasColumnName("RClass")
                    .HasMaxLength(250);

                entity.Property(e => e.ReceivedBillTime).HasColumnType("datetime");

                entity.Property(e => e.ReferenceNo).HasMaxLength(200);

                entity.Property(e => e.ReferenceNoProof).HasMaxLength(200);

                entity.Property(e => e.Remark).HasMaxLength(500);

                entity.Property(e => e.Route).HasMaxLength(250);

                entity.Property(e => e.SailingDate).HasColumnType("datetime");

                entity.Property(e => e.SaleManId)
                    .HasColumnName("SaleManID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SalesCompanyId)
                    .HasColumnName("SalesCompanyID")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.SalesDepartmentId)
                    .HasColumnName("SalesDepartmentID")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.SalesGroupId)
                    .HasColumnName("SalesGroupID")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.SalesOfficeId)
                    .HasColumnName("SalesOfficeID")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Sci)
                    .HasColumnName("SCI")
                    .HasMaxLength(250);

                entity.Property(e => e.ServiceType).HasMaxLength(160);

                entity.Property(e => e.ShipmentType)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ShipperDescription).HasMaxLength(500);

                entity.Property(e => e.ShipperId)
                    .HasColumnName("ShipperID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SubAbbr).HasMaxLength(250);

                entity.Property(e => e.Taxcll)
                    .HasColumnName("TAXCLL")
                    .IsUnicode(false);

                entity.Property(e => e.Taxpp)
                    .HasColumnName("TAXPP")
                    .IsUnicode(false);

                entity.Property(e => e.Total)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalCll)
                    .HasColumnName("TotalCLL")
                    .IsUnicode(false);

                entity.Property(e => e.TotalPp)
                    .HasColumnName("TotalPP")
                    .IsUnicode(false);

                entity.Property(e => e.TransitPlaceBy1).HasMaxLength(250);

                entity.Property(e => e.TransitPlaceBy2).HasMaxLength(250);

                entity.Property(e => e.TransitPlaceTo1).HasMaxLength(250);

                entity.Property(e => e.TransitPlaceTo2).HasMaxLength(250);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Valcll)
                    .HasColumnName("VALCLL")
                    .IsUnicode(false);

                entity.Property(e => e.Valpp)
                    .HasColumnName("VALPP")
                    .IsUnicode(false);

                entity.Property(e => e.WareHouseAnDate).HasColumnType("datetime");

                entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");

                entity.Property(e => e.WarehouseNotice).HasMaxLength(500);

                entity.Property(e => e.Wtcll)
                    .HasColumnName("WTCLL")
                    .IsUnicode(false);

                entity.Property(e => e.WtorValpayment)
                    .HasColumnName("WTorVALPayment")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Wtpp)
                    .HasColumnName("WTPP")
                    .IsUnicode(false);

                entity.HasOne(d => d.Job)
                    .WithMany(p => p.CsTransactionDetail)
                    .HasForeignKey(d => d.JobId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_csTransactionDetail_csTransaction");
            });

            modelBuilder.Entity<CustomsDeclaration>(entity =>
            {
                entity.HasIndex(e => new { e.JobNo, e.ClearanceNo })
                    .HasName("Index_Clearance");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AccountNo)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.CargoType)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Cbm)
                    .HasColumnName("CBM")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ClearanceDate).HasColumnType("datetime");

                entity.Property(e => e.ClearanceNo)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.CommodityCode)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.Consignee).HasMaxLength(500);

                entity.Property(e => e.ConvertTime).HasColumnType("datetime");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.DocumentType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Eta)
                    .HasColumnName("ETA")
                    .HasColumnType("datetime");

                entity.Property(e => e.ExportCountryCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FirstClearanceNo).HasMaxLength(100);

                entity.Property(e => e.Gateway)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.GrossWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.Hblid)
                    .HasColumnName("HBLID")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.IdfromEcus)
                    .HasColumnName("IDFromECus")
                    .HasColumnType("numeric(18, 0)");

                entity.Property(e => e.ImportCountryCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.JobNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Mblid)
                    .HasColumnName("MBLID")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.NetWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.PartnerTaxCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Pcs).HasColumnName("PCS");

                entity.Property(e => e.PortCodeCk)
                    .HasColumnName("PortCodeCK")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PortCodeNn)
                    .HasColumnName("PortCodeNN")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Route)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Shipper).HasMaxLength(500);

                entity.Property(e => e.Source).HasMaxLength(200);

                entity.Property(e => e.Type).HasMaxLength(50);

                entity.Property(e => e.UnitCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<OpsTransaction>(entity =>
            {
                entity.ToTable("opsTransaction");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AgentId)
                    .HasColumnName("AgentID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BillingOpsId)
                    .HasColumnName("BillingOpsID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ClearanceDate).HasColumnType("datetime");

                entity.Property(e => e.CommodityGroupId).HasColumnName("CommodityGroupID");

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.Consignee).HasMaxLength(500);

                entity.Property(e => e.ContainerDescription).HasMaxLength(200);

                entity.Property(e => e.CurrentStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerId)
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DateCreatedLinkJob).HasColumnType("datetime");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DeliveryDate).HasColumnType("datetime");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.Eta)
                    .HasColumnName("ETA")
                    .HasColumnType("datetime");

                entity.Property(e => e.FieldOpsId)
                    .HasColumnName("FieldOpsID")
                    .HasMaxLength(200);

                entity.Property(e => e.FinishDate).HasColumnType("datetime");

                entity.Property(e => e.FlightVessel).HasMaxLength(200);

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.Hblid).HasColumnName("HBLID");

                entity.Property(e => e.Hwbno)
                    .HasColumnName("HWBNo")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.InvoiceNo).HasMaxLength(200);

                entity.Property(e => e.IsLinkFee).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsLocked).HasDefaultValueSql("((0))");

                entity.Property(e => e.JobNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastDateUnLocked).HasColumnType("datetime");

                entity.Property(e => e.LinkSource)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LockedDate).HasColumnType("datetime");

                entity.Property(e => e.LockedUser).HasMaxLength(50);

                entity.Property(e => e.Mblno)
                    .HasColumnName("MBLNO")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.PackageTypeId).HasColumnName("PackageTypeID");

                entity.Property(e => e.Pod).HasColumnName("POD");

                entity.Property(e => e.Pol).HasColumnName("POL");

                entity.Property(e => e.ProductService)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.PurchaseOrderNo)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ReplicatedId).HasColumnName("ReplicatedID");

                entity.Property(e => e.SalemanId)
                    .HasColumnName("SalemanID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SalesCompanyId)
                    .HasColumnName("SalesCompanyID")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.SalesDepartmentId)
                    .HasColumnName("SalesDepartmentID")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.SalesGroupId)
                    .HasColumnName("SalesGroupID")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.SalesOfficeId)
                    .HasColumnName("SalesOfficeID")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceDate).HasColumnType("datetime");

                entity.Property(e => e.ServiceHblId).HasColumnName("ServiceHblID");

                entity.Property(e => e.ServiceMode)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShipmentMode)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ShipmentType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Shipper).HasMaxLength(500);

                entity.Property(e => e.SumCbm)
                    .HasColumnName("SumCBM")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.SumChargeWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.SumGrossWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.SumNetWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.SupplierId)
                    .HasColumnName("SupplierID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SuspendTime).HasMaxLength(150);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreatedLinkJob)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");
            });

            modelBuilder.Entity<SetTariff>(entity =>
            {
                entity.ToTable("setTariff");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.AgentId)
                    .HasColumnName("AgentID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApplyOfficeId).HasColumnName("ApplyOfficeID");

                entity.Property(e => e.CargoType)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.CustomerId)
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.EffectiveDate).HasColumnType("datetime");

                entity.Property(e => e.ExpiredDate).HasColumnType("datetime");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.ProductService)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceMode)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.SupplierId)
                    .HasColumnName("SupplierID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TariffName).IsRequired();

                entity.Property(e => e.TariffType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SetTariffDetail>(entity =>
            {
                entity.ToTable("setTariffDetail");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ChargeId).HasColumnName("ChargeID");

                entity.Property(e => e.CommodityId).HasColumnName("CommodityID");

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.Max).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.Min).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.NextUnit).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.NextUnitPrice).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.PayerId)
                    .HasColumnName("PayerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PortId).HasColumnName("PortID");

                entity.Property(e => e.RangeFrom).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.RangeTo).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.RangeType)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Route)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TariffId).HasColumnName("TariffID");

                entity.Property(e => e.Type)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.UseFor)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Vatrate)
                    .HasColumnName("VATRate")
                    .HasColumnType("decimal(18, 3)");

                entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");
            });

            modelBuilder.Entity<SetUnlockRequest>(entity =>
            {
                entity.ToTable("setUnlockRequest");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.NewServiceDate).HasColumnType("datetime");

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.RequestDate).HasColumnType("datetime");

                entity.Property(e => e.RequestUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Requester)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StatusApproval).HasMaxLength(50);

                entity.Property(e => e.UnlockType).HasMaxLength(50);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SetUnlockRequestApprove>(entity =>
            {
                entity.ToTable("setUnlockRequestApprove");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Accountant)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantApr)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantAprDate).HasColumnType("datetime");

                entity.Property(e => e.Buhead)
                    .HasColumnName("BUHead")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BuheadApr)
                    .HasColumnName("BUHeadApr")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BuheadAprDate)
                    .HasColumnName("BUHeadAprDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.Leader)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LeaderApr)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LeaderAprDate).HasColumnType("datetime");

                entity.Property(e => e.LevelApprove).HasMaxLength(50);

                entity.Property(e => e.Manager)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ManagerApr)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ManagerAprDate).HasColumnType("datetime");

                entity.Property(e => e.UnlockRequestId).HasColumnName("UnlockRequestID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SetUnlockRequestJob>(entity =>
            {
                entity.ToTable("setUnlockRequestJob");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.Job).HasMaxLength(50);

                entity.Property(e => e.UnlockRequestId).HasColumnName("UnlockRequestID");

                entity.Property(e => e.UnlockType).HasMaxLength(50);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysAttachFileTemplate>(entity =>
            {
                entity.ToTable("sysAttachFileTemplate");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AccountingType).HasMaxLength(50);

                entity.Property(e => e.Code).HasMaxLength(150);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("NameEN")
                    .HasMaxLength(250);

                entity.Property(e => e.NameVn)
                    .HasColumnName("NameVN")
                    .HasMaxLength(250);

                entity.Property(e => e.PartnerType).HasMaxLength(50);

                entity.Property(e => e.PreFix).HasMaxLength(150);

                entity.Property(e => e.Service)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StorageFollowing)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StorageRule)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StorageType)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.SubFix).HasMaxLength(150);

                entity.Property(e => e.Tag).HasMaxLength(150);

                entity.Property(e => e.TransactionType)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Type).HasMaxLength(10);
            });

            modelBuilder.Entity<SysCompany>(entity =>
            {
                entity.ToTable("sysCompany");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountName).HasMaxLength(4000);

                entity.Property(e => e.AccountNoOverSea)
                    .HasColumnName("AccountNo_OverSea")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountNoVn)
                    .HasColumnName("AccountNo_VN")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.AddressEn)
                    .HasColumnName("Address_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.AddressVn)
                    .HasColumnName("Address_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.AreaId)
                    .HasColumnName("AreaID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BankAddress).HasMaxLength(4000);

                entity.Property(e => e.BankName).HasMaxLength(4000);

                entity.Property(e => e.BunameAbbr)
                    .HasColumnName("BUName_ABBR")
                    .HasMaxLength(50);

                entity.Property(e => e.BunameEn)
                    .HasColumnName("BUName_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.BunameVn)
                    .HasColumnName("BUName_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DescriptionEn)
                    .HasColumnName("Description_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.DescriptionVn)
                    .HasColumnName("Description_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.Email).HasMaxLength(4000);

                entity.Property(e => e.Fax).HasMaxLength(1600);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.KbExchangeRate).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.Logo).HasColumnType("image");

                entity.Property(e => e.LogoPath).IsUnicode(false);

                entity.Property(e => e.ManagerId)
                    .HasColumnName("ManagerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Notes).HasMaxLength(4000);

                entity.Property(e => e.Tax)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TaxAccount).HasMaxLength(1600);

                entity.Property(e => e.Taxcode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Tel)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Website).HasMaxLength(1600);
            });

            modelBuilder.Entity<SysEmployee>(entity =>
            {
                entity.ToTable("sysEmployee");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.AccessDescription).HasMaxLength(1600);

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.BankAccountNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BankCode)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.BankName)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Birthday).HasColumnType("datetime");

                entity.Property(e => e.Bonus).HasColumnType("decimal(10, 4)");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DepartmentId)
                    .HasColumnName("DepartmentID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.EmpPhotoSize).HasMaxLength(4000);

                entity.Property(e => e.EmployeeNameEn)
                    .HasColumnName("EmployeeName_EN")
                    .HasMaxLength(1600);

                entity.Property(e => e.EmployeeNameVn)
                    .HasColumnName("EmployeeName_VN")
                    .HasMaxLength(1600);

                entity.Property(e => e.ExtNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.HomeAddress).HasMaxLength(4000);

                entity.Property(e => e.HomePhone)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.PersonalId)
                    .HasColumnName("PersonalID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Photo).HasMaxLength(500);

                entity.Property(e => e.Position).HasMaxLength(1600);

                entity.Property(e => e.SaleResource)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SaleTarget).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Signature).HasColumnType("image");

                entity.Property(e => e.SignatureImage).HasMaxLength(500);

                entity.Property(e => e.StaffCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Tel)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Title).HasMaxLength(250);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysGroup>(entity =>
            {
                entity.ToTable("sysGroup");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.Email).HasMaxLength(150);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.ManagerId)
                    .HasColumnName("ManagerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.NameEn)
                    .HasColumnName("NameEN")
                    .HasMaxLength(250);

                entity.Property(e => e.NameVn)
                    .HasColumnName("NameVN")
                    .HasMaxLength(250);

                entity.Property(e => e.ParentId).HasColumnName("ParentID");

                entity.Property(e => e.ShortName).HasMaxLength(100);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysImage>(entity =>
            {
                entity.ToTable("sysImage");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ChildId)
                    .HasColumnName("ChildID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DateTimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.Folder).HasMaxLength(50);

                entity.Property(e => e.IsTemp).HasColumnName("isTemp");

                entity.Property(e => e.ObjectId)
                    .HasColumnName("ObjectID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SyncStatus)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated).HasMaxLength(50);

                entity.Property(e => e.UserModified).HasMaxLength(50);
            });

            modelBuilder.Entity<SysImageDetail>(entity =>
            {
                entity.ToTable("sysImageDetail");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BillingNo).HasMaxLength(50);

                entity.Property(e => e.BillingType).HasMaxLength(50);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.ExpiredDate).HasColumnType("datetime");

                entity.Property(e => e.GroupId)
                    .HasColumnName("GroupID")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Hblid).HasColumnName("HBLID");

                entity.Property(e => e.JobId).HasColumnName("JobID");

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.Source)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SystemFileName).HasColumnName("SystemFIleName");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysOffice>(entity =>
            {
                entity.ToTable("sysOffice");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.AddressEn)
                    .HasColumnName("Address_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.AddressVn)
                    .HasColumnName("Address_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.BankAccountNameEn)
                    .HasColumnName("BankAccountName_EN")
                    .HasMaxLength(1000);

                entity.Property(e => e.BankAccountNameVn)
                    .HasColumnName("BankAccountName_VN")
                    .HasMaxLength(1000);

                entity.Property(e => e.BankAccountUsd)
                    .HasColumnName("BankAccount_USD")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.BankAccountVnd)
                    .HasColumnName("BankAccount_VND")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.BankAddressEn)
                    .HasColumnName("BankAddress_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.BankAddressLocal)
                    .HasColumnName("BankAddress_Local")
                    .HasMaxLength(4000);

                entity.Property(e => e.BankNameEn)
                    .HasColumnName("BankName_EN")
                    .HasMaxLength(1000);

                entity.Property(e => e.BankNameLocal)
                    .HasColumnName("BankName_Local")
                    .HasMaxLength(1000);

                entity.Property(e => e.BranchNameEn)
                    .HasColumnName("BranchName_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.BranchNameVn)
                    .HasColumnName("BranchName_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.Buid).HasColumnName("BUID");

                entity.Property(e => e.Code).HasMaxLength(640);

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Fax)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.InternalCode)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Location).HasMaxLength(4000);

                entity.Property(e => e.Logo).HasColumnType("image");

                entity.Property(e => e.ManagerId)
                    .HasColumnName("ManagerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OfficeType)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.PartnerMapping)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShortName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SwiftCode)
                    .HasMaxLength(400)
                    .IsUnicode(false);

                entity.Property(e => e.Taxcode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Tel)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Website)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysSentEmailHistory>(entity =>
            {
                entity.ToTable("sysSentEmailHistory");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Bccs)
                    .HasColumnName("BCCs")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Ccs)
                    .HasColumnName("CCs")
                    .HasMaxLength(4000)
                    .IsUnicode(false);

                entity.Property(e => e.Description).HasMaxLength(4000);

                entity.Property(e => e.Receivers)
                    .HasMaxLength(4000)
                    .IsUnicode(false);

                entity.Property(e => e.SentDateTime).HasColumnType("datetime");

                entity.Property(e => e.SentUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Subject).HasMaxLength(4000);

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysSettingFlow>(entity =>
            {
                entity.ToTable("sysSettingFlow");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Accountant)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AlertAtd).HasColumnName("AlertATD");

                entity.Property(e => e.ApplyPartner)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApplyType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Bod)
                    .HasColumnName("BOD")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.Flow)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Leader)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Manager)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.ReplicatePrefix)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Type).HasMaxLength(100);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysUser>(entity =>
            {
                entity.ToTable("sysUser");

                entity.HasIndex(e => e.Username)
                    .HasName("Index_SysUser_ID")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.CreditLimit).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CreditRate).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(4000);

                entity.Property(e => e.EmployeeId)
                    .HasColumnName("EmployeeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.IsLdap).HasColumnName("IsLDAP");

                entity.Property(e => e.Password).HasMaxLength(4000);

                entity.Property(e => e.PasswordLdap).HasColumnName("PasswordLDAP");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserRole)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");

                entity.Property(e => e.WorkingStatus)
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysUserLevel>(entity =>
            {
                entity.ToTable("sysUserLevel");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.Position)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasColumnName("UserID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });
        }
    }
}
