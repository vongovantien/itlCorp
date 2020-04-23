﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace eFMS.API.Catalogue.Service.Models
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

        public virtual DbSet<CatArea> CatArea { get; set; }
        public virtual DbSet<CatCharge> CatCharge { get; set; }
        public virtual DbSet<CatChargeDefaultAccount> CatChargeDefaultAccount { get; set; }
        public virtual DbSet<CatChargeGroup> CatChargeGroup { get; set; }
        public virtual DbSet<CatCommodity> CatCommodity { get; set; }
        public virtual DbSet<CatCommodityGroup> CatCommodityGroup { get; set; }
        public virtual DbSet<CatContainerType> CatContainerType { get; set; }
        public virtual DbSet<CatCountry> CatCountry { get; set; }
        public virtual DbSet<CatCurrency> CatCurrency { get; set; }
        public virtual DbSet<CatCurrencyExchange> CatCurrencyExchange { get; set; }
        public virtual DbSet<CatDepartment> CatDepartment { get; set; }
        public virtual DbSet<CatPartner> CatPartner { get; set; }
        public virtual DbSet<CatPartnerCharge> CatPartnerCharge { get; set; }
        public virtual DbSet<CatPartnerGroup> CatPartnerGroup { get; set; }
        public virtual DbSet<CatPlace> CatPlace { get; set; }
        public virtual DbSet<CatPlaceType> CatPlaceType { get; set; }
        public virtual DbSet<CatSaleman> CatSaleman { get; set; }
        public virtual DbSet<CatServiceType> CatServiceType { get; set; }
        public virtual DbSet<CatStage> CatStage { get; set; }
        public virtual DbSet<CatTransportationMode> CatTransportationMode { get; set; }
        public virtual DbSet<CatUnit> CatUnit { get; set; }
        public virtual DbSet<CsManifest> CsManifest { get; set; }
        public virtual DbSet<CsMawbcontainer> CsMawbcontainer { get; set; }
        public virtual DbSet<CsShipmentSurcharge> CsShipmentSurcharge { get; set; }
        public virtual DbSet<CsTransaction> CsTransaction { get; set; }
        public virtual DbSet<CsTransactionDetail> CsTransactionDetail { get; set; }
        public virtual DbSet<OpsStageAssigned> OpsStageAssigned { get; set; }
        public virtual DbSet<OpsTransaction> OpsTransaction { get; set; }
        public virtual DbSet<SysOffice> SysOffice { get; set; }
        public virtual DbSet<SysUser> SysUser { get; set; }

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

            modelBuilder.Entity<CatArea>(entity =>
            {
                entity.ToTable("catArea");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(4000);

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

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

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

            modelBuilder.Entity<CatChargeDefaultAccount>(entity =>
            {
                entity.ToTable("catChargeDefaultAccount");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.ChargeId).HasColumnName("ChargeID");

                entity.Property(e => e.CreditAccountNo).HasMaxLength(800);

                entity.Property(e => e.CreditVat)
                    .HasColumnName("CreditVAT")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DebitAccountNo).HasMaxLength(800);

                entity.Property(e => e.DebitVat)
                    .HasColumnName("DebitVAT")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Type).HasMaxLength(800);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatChargeGroup>(entity =>
            {
                entity.ToTable("catChargeGroup");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatCommodity>(entity =>
            {
                entity.ToTable("catCommodity");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Code)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.CommodityGroupId).HasColumnName("CommodityGroupID");

                entity.Property(e => e.CommodityNameEn)
                    .HasColumnName("CommodityName_EN")
                    .HasMaxLength(250);

                entity.Property(e => e.CommodityNameVn)
                    .HasColumnName("CommodityName_VN")
                    .HasMaxLength(250);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Note)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
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

            modelBuilder.Entity<CatContainerType>(entity =>
            {
                entity.ToTable("catContainerType");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ExtraWeight).HasColumnType("decimal(10, 4)");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(1600);

                entity.Property(e => e.Note)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatCountry>(entity =>
            {
                entity.ToTable("catCountry");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatCurrency>(entity =>
            {
                entity.ToTable("catCurrency");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.CurrencyName).HasMaxLength(800);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

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

                entity.Property(e => e.DeptType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Description).HasMaxLength(4000);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Branch)
                    .WithMany(p => p.CatDepartment)
                    .HasForeignKey(d => d.BranchId)
                    .HasConstraintName("FK_catDepartment_sysBranch");
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
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.AddressEn)
                    .HasColumnName("Address_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.AddressShippingEn)
                    .HasColumnName("AddressShipping_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.AddressShippingVn)
                    .HasColumnName("AddressShipping_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.AddressVn)
                    .HasColumnName("Address_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.ApplyDim)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BankAccountAddress).HasMaxLength(4000);

                entity.Property(e => e.BankAccountName).HasMaxLength(4000);

                entity.Property(e => e.BankAccountNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CoLoaderCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.ContactPerson).HasMaxLength(4000);

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.CountryShippingId).HasColumnName("CountryShippingID");

                entity.Property(e => e.CreditAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DebitAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.Email)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Fax)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

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

                entity.Property(e => e.PartnerNameEn)
                    .HasColumnName("PartnerName_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.PartnerNameVn)
                    .HasColumnName("PartnerName_VN")
                    .HasMaxLength(4000);

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
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TaxCode)
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
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPhoneEx)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");

                entity.Property(e => e.ZipCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ZipCodeShipping)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatPartnerCharge>(entity =>
            {
                entity.ToTable("catPartnerCharge");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ChargeId).HasColumnName("ChargeID");

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.PartnerId)
                    .IsRequired()
                    .HasColumnName("PartnerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.QuantityType)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Vatrate)
                    .HasColumnName("VATRate")
                    .HasColumnType("decimal(18, 4)");
            });

            modelBuilder.Entity<CatPartnerGroup>(entity =>
            {
                entity.ToTable("catPartnerGroup");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.GroupNameEn)
                    .HasColumnName("GroupName_EN")
                    .HasMaxLength(800);

                entity.Property(e => e.GroupNameVn)
                    .HasColumnName("GroupName_VN")
                    .HasMaxLength(800);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
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

                entity.Property(e => e.FlightVesselNo).HasMaxLength(50);

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

            modelBuilder.Entity<CatPlaceType>(entity =>
            {
                entity.ToTable("catPlaceType");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(1600);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(1600);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatSaleman>(entity =>
            {
                entity.ToTable("catSaleman");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasColumnType("text");

                entity.Property(e => e.EffectDate).HasColumnType("datetime");

                entity.Property(e => e.FreightPayment)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.PartnerId)
                    .HasColumnName("PartnerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SaleManId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Service)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatServiceType>(entity =>
            {
                entity.ToTable("catServiceType");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(2400);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatStage>(entity =>
            {
                entity.ToTable("catStage");

                entity.HasIndex(e => e.Code)
                    .HasName("U_catStage")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.DescriptionEn)
                    .HasColumnName("Description_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.DescriptionVn)
                    .HasColumnName("Description_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.StageNameEn)
                    .HasColumnName("StageName_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.StageNameVn)
                    .HasColumnName("StageName_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatTransportationMode>(entity =>
            {
                entity.ToTable("catTransportationMode");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatUnit>(entity =>
            {
                entity.ToTable("catUnit");

                entity.HasIndex(e => e.Code)
                    .HasName("U_Unit")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DescriptionEn).HasMaxLength(4000);

                entity.Property(e => e.DescriptionVn).HasMaxLength(4000);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.UnitNameEn)
                    .HasColumnName("UnitName_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.UnitNameVn)
                    .HasColumnName("UnitName_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.UnitType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsManifest>(entity =>
            {
                entity.HasKey(e => e.JobId);

                entity.ToTable("csManifest");

                entity.Property(e => e.JobId)
                    .HasColumnName("JobID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Attention).HasMaxLength(250);

                entity.Property(e => e.Consolidator).HasMaxLength(250);

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DeConsolidator).HasMaxLength(250);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.InvoiceDate).HasColumnType("datetime");

                entity.Property(e => e.ManifestIssuer).HasMaxLength(500);

                entity.Property(e => e.MasksOfRegistration).HasMaxLength(1000);

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PaymentTerm)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Pod).HasColumnName("POD");

                entity.Property(e => e.Pol).HasColumnName("POL");

                entity.Property(e => e.RefNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Supplier).HasMaxLength(250);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Volume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.VoyNo).HasMaxLength(1600);

                entity.Property(e => e.Weight).HasColumnType("decimal(18, 4)");
            });

            modelBuilder.Entity<CsMawbcontainer>(entity =>
            {
                entity.ToTable("csMAWBContainer");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Cbm)
                    .HasColumnName("CBM")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ChargeAbleWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ContainerNo).HasMaxLength(100);

                entity.Property(e => e.ContainerTypeId).HasColumnName("ContainerTypeID");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(4000);

                entity.Property(e => e.Gw)
                    .HasColumnName("GW")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Hblid).HasColumnName("HBLID");

                entity.Property(e => e.MarkNo).HasMaxLength(100);

                entity.Property(e => e.Mblid).HasColumnName("MBLID");

                entity.Property(e => e.Nw)
                    .HasColumnName("NW")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OffHireDepot).HasMaxLength(800);

                entity.Property(e => e.OffHireRefNo).HasMaxLength(800);

                entity.Property(e => e.OwnerId)
                    .HasColumnName("OwnerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SealNo).HasMaxLength(100);

                entity.Property(e => e.UnitOfMeasureId).HasColumnName("UnitOfMeasureID");

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

                entity.Property(e => e.AdvanceNo).HasMaxLength(11);

                entity.Property(e => e.Cdclosed)
                    .HasColumnName("CDClosed")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.ChargeId).HasColumnName("ChargeID");

                entity.Property(e => e.ClearanceNo).HasMaxLength(100);

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

                entity.Property(e => e.Mblno)
                    .HasColumnName("MBLNo")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.ObjectBePaid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaySoano)
                    .HasColumnName("PaySOANo")
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

                entity.Property(e => e.Total).HasColumnType("decimal(18, 4)");

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

                entity.Property(e => e.DesOfGoods).HasMaxLength(1600);

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

                entity.Property(e => e.LockedDate).HasColumnType("datetime");

                entity.Property(e => e.Mawb)
                    .HasColumnName("MAWB")
                    .HasMaxLength(800);

                entity.Property(e => e.Mbltype)
                    .HasColumnName("MBLType")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.NetWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Notes).HasMaxLength(4000);

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.PackageContainer).HasMaxLength(1600);

                entity.Property(e => e.PaymentTerm).HasMaxLength(1600);

                entity.Property(e => e.PersonIncharge)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Pod).HasColumnName("POD");

                entity.Property(e => e.Pol).HasColumnName("POL");

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

                entity.Property(e => e.DeliveryOrderNo).HasMaxLength(100);

                entity.Property(e => e.DeliveryOrderPrintedDate).HasColumnType("datetime");

                entity.Property(e => e.DeliveryPlace).HasMaxLength(500);

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.DesOfGoods).HasMaxLength(1600);

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

                entity.Property(e => e.GoodsDeliveryDescription).HasMaxLength(500);

                entity.Property(e => e.GoodsDeliveryId)
                    .HasColumnName("GoodsDeliveryID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.GrossWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.HandingInformation).HasMaxLength(250);

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

                entity.Property(e => e.Notify).HasMaxLength(250);

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

                entity.Property(e => e.PoinvoiceNo)
                    .HasColumnName("POInvoiceNo")
                    .HasMaxLength(250);

                entity.Property(e => e.Pol).HasColumnName("POL");

                entity.Property(e => e.PurchaseOrderNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RateCharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Rclass)
                    .HasColumnName("RClass")
                    .HasMaxLength(250);

                entity.Property(e => e.ReferenceNo).HasMaxLength(200);

                entity.Property(e => e.Remark).HasMaxLength(500);

                entity.Property(e => e.Route).HasMaxLength(250);

                entity.Property(e => e.SailingDate).HasColumnType("datetime");

                entity.Property(e => e.SaleManId)
                    .HasColumnName("SaleManID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Sci)
                    .HasColumnName("SCI")
                    .HasMaxLength(250);

                entity.Property(e => e.ServiceType).HasMaxLength(160);

                entity.Property(e => e.ShipperDescription).HasMaxLength(500);

                entity.Property(e => e.ShipperId)
                    .HasColumnName("ShipperID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingMark).HasMaxLength(4000);

                entity.Property(e => e.SubAbbr).HasMaxLength(250);

                entity.Property(e => e.Taxcll)
                    .HasColumnName("TAXCLL")
                    .IsUnicode(false);

                entity.Property(e => e.Taxpp)
                    .HasColumnName("TAXPP")
                    .IsUnicode(false);

                entity.Property(e => e.Total).HasColumnType("decimal(18, 4)");

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

            modelBuilder.Entity<OpsStageAssigned>(entity =>
            {
                entity.ToTable("opsStageAssigned");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Comment).HasMaxLength(200);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Deadline).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.JobId).HasColumnName("JobID");

                entity.Property(e => e.MainPersonInCharge)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.Property(e => e.ProcessTime).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.RealPersonInCharge)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StageId).HasColumnName("StageID");

                entity.Property(e => e.Status).HasMaxLength(20);

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

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

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

                entity.Property(e => e.IsLocked).HasDefaultValueSql("((0))");

                entity.Property(e => e.JobNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

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

                entity.Property(e => e.SalemanId)
                    .HasColumnName("SalemanID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceDate).HasColumnType("datetime");

                entity.Property(e => e.ServiceMode)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ShipmentMode)
                    .HasMaxLength(20)
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

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");
            });

            modelBuilder.Entity<SysOffice>(entity =>
            {
                entity.ToTable("sysOffice");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasDefaultValueSql("(newid())");

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
                    .HasMaxLength(500);

                entity.Property(e => e.BankAccountVnd)
                    .HasColumnName("BankAccount_VND")
                    .HasMaxLength(500);

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

                entity.Property(e => e.Email).IsUnicode(false);

                entity.Property(e => e.Fax).HasMaxLength(500);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Location).HasMaxLength(4000);

                entity.Property(e => e.Logo).HasColumnType("image");

                entity.Property(e => e.ManagerId)
                    .HasColumnName("ManagerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShortName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SwiftCode).HasMaxLength(4000);

                entity.Property(e => e.Taxcode).HasMaxLength(4000);

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

            modelBuilder.Entity<SysUser>(entity =>
            {
                entity.ToTable("sysUser");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

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
        }
    }
}
