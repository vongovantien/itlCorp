﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace eFMS.API.ForPartner.Service.Models
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

        public virtual DbSet<AccAccountingManagement> AccAccountingManagement { get; set; }
        public virtual DbSet<AcctAdvancePayment> AcctAdvancePayment { get; set; }
        public virtual DbSet<AcctCdnote> AcctCdnote { get; set; }
        public virtual DbSet<AcctReceipt> AcctReceipt { get; set; }
        public virtual DbSet<AcctSettlementPayment> AcctSettlementPayment { get; set; }
        public virtual DbSet<AcctSoa> AcctSoa { get; set; }
        public virtual DbSet<CatCurrencyExchange> CatCurrencyExchange { get; set; }
        public virtual DbSet<CatPartner> CatPartner { get; set; }
        public virtual DbSet<CsShipmentSurcharge> CsShipmentSurcharge { get; set; }
        public virtual DbSet<SysActionFuncLog> SysActionFuncLog { get; set; }
        public virtual DbSet<SysNotifications> SysNotifications { get; set; }
        public virtual DbSet<SysPartnerApi> SysPartnerApi { get; set; }
        public virtual DbSet<SysUser> SysUser { get; set; }
        public virtual DbSet<SysUserNotification> SysUserNotification { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity<AccAccountingManagement>(entity =>
            {
                entity.ToTable("accAccountingManagement");

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

                entity.Property(e => e.ServiceType).HasMaxLength(10);

                entity.Property(e => e.SourceCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.SyncStatus).HasMaxLength(50);

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalExchangeRate).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Type).HasMaxLength(50);

                entity.Property(e => e.UnpaidAmount).HasColumnType("decimal(18, 4)");

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

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AdvanceCurrency).HasMaxLength(10);

                entity.Property(e => e.AdvanceNo)
                    .IsRequired()
                    .HasMaxLength(11)
                    .IsUnicode(false);

                entity.Property(e => e.BankAccountName).HasMaxLength(150);

                entity.Property(e => e.BankAccountNo).HasMaxLength(150);

                entity.Property(e => e.BankName).HasMaxLength(150);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DeadlinePayment).HasColumnType("date");

                entity.Property(e => e.Department)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.LastSyncDate).HasColumnType("datetime");

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

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

            modelBuilder.Entity<AcctReceipt>(entity =>
            {
                entity.ToTable("acctReceipt");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AgreementId).HasColumnName("AgreementID");

                entity.Property(e => e.Balance).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.BankAccountNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.CurrencyId)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CusAdvanceAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CustomerId)
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.ExchangeRate).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FinalPaidAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FromDate).HasColumnType("datetime");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.LastSyncDate).HasColumnType("datetime");

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaymentDate).HasColumnType("datetime");

                entity.Property(e => e.PaymentMethod).HasMaxLength(50);

                entity.Property(e => e.PaymentRefNo)
                    .HasMaxLength(50)
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

            modelBuilder.Entity<AcctSettlementPayment>(entity =>
            {
                entity.ToTable("acctSettlementPayment");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

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

                entity.Property(e => e.Id).HasColumnName("ID");

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

                entity.Property(e => e.Obh).HasColumnName("OBH");

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.PaymentDatetimeUpdated).HasColumnType("datetime");

                entity.Property(e => e.PaymentDueDate).HasColumnType("datetime");

                entity.Property(e => e.PaymentNote).HasMaxLength(500);

                entity.Property(e => e.PaymentStatus).HasMaxLength(50);

                entity.Property(e => e.ServiceTypeId)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.SoaformDate)
                    .HasColumnName("SOAFormDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Soano)
                    .HasColumnName("SOANo")
                    .HasMaxLength(7)
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

            modelBuilder.Entity<CatCurrencyExchange>(entity =>
            {
                entity.ToTable("catCurrencyExchange");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.CurrencyFromId)
                    .HasColumnName("CurrencyFromID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyToId)
                    .HasColumnName("CurrencyToID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EffectiveOn).HasColumnType("datetime");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Rate).HasColumnType("decimal(18, 4)");

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

                entity.Property(e => e.InvoiceNo).HasMaxLength(50);

                entity.Property(e => e.IsFromShipment).HasDefaultValueSql("((1))");

                entity.Property(e => e.JobNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Mblno)
                    .HasColumnName("MBLNo")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.NetAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Notes).HasMaxLength(500);

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

                entity.Property(e => e.VatAmountUsd)
                    .HasColumnName("VatAmountUSD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.VatAmountVnd)
                    .HasColumnName("VatAmountVND")
                    .HasColumnType("decimal(18, 4)");

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

            modelBuilder.Entity<SysActionFuncLog>(entity =>
            {
                entity.ToTable("sysActionFuncLog");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.FuncLocal)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.FuncPartner)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Major).HasMaxLength(100);

                entity.Property(e => e.ObjectRequest).HasColumnType("ntext");

                entity.Property(e => e.ObjectResponse).HasColumnType("ntext");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysNotifications>(entity =>
            {
                entity.ToTable("sysNotifications");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Action).HasMaxLength(200);

                entity.Property(e => e.ActionLink).IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

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

            modelBuilder.Entity<SysPartnerApi>(entity =>
            {
                entity.ToTable("sysPartnerAPI");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Company).HasMaxLength(100);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(10);

                entity.Property(e => e.Environment).HasMaxLength(150);

                entity.Property(e => e.ExpiredDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.UserId).HasColumnName("UserID");
            });

            modelBuilder.Entity<SysUser>(entity =>
            {
                entity.ToTable("sysUser");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

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

            modelBuilder.Entity<SysUserNotification>(entity =>
            {
                entity.ToTable("sysUserNotification");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.NotitficationId).HasColumnName("NotitficationID");

                entity.Property(e => e.Status).HasMaxLength(10);

                entity.Property(e => e.UserCreated).HasMaxLength(50);

                entity.Property(e => e.UserId)
                    .HasColumnName("UserID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified).HasMaxLength(50);
            });
        }
    }
}
