using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class eFMSDataContext : DbContext
    {
        public eFMSDataContext()
        {
        }

        public eFMSDataContext(DbContextOptions<eFMSDataContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CatArea> CatArea { get; set; }
        public virtual DbSet<CatBranch> CatBranch { get; set; }
        public virtual DbSet<CatCharge> CatCharge { get; set; }
        public virtual DbSet<CatChargeDefaultAccount> CatChargeDefaultAccount { get; set; }
        public virtual DbSet<CatCommodity> CatCommodity { get; set; }
        public virtual DbSet<CatCommodityGroup> CatCommodityGroup { get; set; }
        public virtual DbSet<CatCountry> CatCountry { get; set; }
        public virtual DbSet<CatCurrency> CatCurrency { get; set; }
        public virtual DbSet<CatCustomerPlace> CatCustomerPlace { get; set; }
        public virtual DbSet<CatDepartment> CatDepartment { get; set; }
        public virtual DbSet<CatPartner> CatPartner { get; set; }
        public virtual DbSet<CatPartnerContact> CatPartnerContact { get; set; }
        public virtual DbSet<CatPartnerContract> CatPartnerContract { get; set; }
        public virtual DbSet<CatPartnerGroup> CatPartnerGroup { get; set; }
        public virtual DbSet<CatPlace> CatPlace { get; set; }
        public virtual DbSet<CatPlaceType> CatPlaceType { get; set; }
        public virtual DbSet<CatSaleResource> CatSaleResource { get; set; }
        public virtual DbSet<CatStage> CatStage { get; set; }
        public virtual DbSet<CatTransportationMode> CatTransportationMode { get; set; }
        public virtual DbSet<CatUnit> CatUnit { get; set; }
        public virtual DbSet<CsShipment> CsShipment { get; set; }
        public virtual DbSet<CsShipmentBuyingRate> CsShipmentBuyingRate { get; set; }
        public virtual DbSet<CsShipmentDetail> CsShipmentDetail { get; set; }
        public virtual DbSet<CsShipmentHawbdetail> CsShipmentHawbdetail { get; set; }
        public virtual DbSet<CsShipmentProfitShares> CsShipmentProfitShares { get; set; }
        public virtual DbSet<CsShipmentSellingRate> CsShipmentSellingRate { get; set; }
        public virtual DbSet<SysAuthorization> SysAuthorization { get; set; }
        public virtual DbSet<SysAuthorizationDetail> SysAuthorizationDetail { get; set; }
        public virtual DbSet<SysBu> SysBu { get; set; }
        public virtual DbSet<SysEmployee> SysEmployee { get; set; }
        public virtual DbSet<SysMenu> SysMenu { get; set; }
        public virtual DbSet<SysMenuPermissionInstruction> SysMenuPermissionInstruction { get; set; }
        public virtual DbSet<SysPermission> SysPermission { get; set; }
        public virtual DbSet<SysRole> SysRole { get; set; }
        public virtual DbSet<SysRoleMenu> SysRoleMenu { get; set; }
        public virtual DbSet<SysRolePermission> SysRolePermission { get; set; }
        public virtual DbSet<SysSentEmailHistory> SysSentEmailHistory { get; set; }
        public virtual DbSet<SysUser> SysUser { get; set; }
        public virtual DbSet<SysUserGroup> SysUserGroup { get; set; }
        public virtual DbSet<SysUserLog> SysUserLog { get; set; }
        public virtual DbSet<SysUserOtherWorkPlace> SysUserOtherWorkPlace { get; set; }
        public virtual DbSet<SysUserRole> SysUserRole { get; set; }

        // Unable to generate entity type for table 'dbo.csShipmentHAWBDetailArrivalFreightCharges'. Please see the warning messages.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=192.168.7.88;Database=eFMSTest;User ID=sa;Password=P@ssw0rd;",
                    options =>
                    {
                        options.UseRowNumberForPaging();
                    });
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CatArea>(entity =>
            {
                entity.ToTable("catArea");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(510);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(510);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatBranch>(entity =>
            {
                entity.ToTable("catBranch");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AddressEn)
                    .HasColumnName("Address_EN")
                    .HasMaxLength(300);

                entity.Property(e => e.AddressVn)
                    .HasColumnName("Address_VN")
                    .HasMaxLength(300);

                entity.Property(e => e.BankAccountUsd)
                    .HasColumnName("BankAccount_USD")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.BankAccountVnd)
                    .HasColumnName("BankAccount_VND")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.BankAddress).HasMaxLength(510);

                entity.Property(e => e.BankName).HasMaxLength(300);

                entity.Property(e => e.BranchNameEn)
                    .HasColumnName("BranchName_EN")
                    .HasMaxLength(300);

                entity.Property(e => e.BranchNameVn)
                    .HasColumnName("BranchName_VN")
                    .HasMaxLength(300);

                entity.Property(e => e.Code).HasMaxLength(40);

                entity.Property(e => e.DatetimeCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Fax)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Logo).HasColumnType("image");

                entity.Property(e => e.Taxcode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Tel)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Website)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatCharge>(entity =>
            {
                entity.ToTable("catCharge");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ChargeNameEn)
                    .IsRequired()
                    .HasColumnName("ChargeName_EN")
                    .HasMaxLength(510);

                entity.Property(e => e.ChargeNameVn)
                    .IsRequired()
                    .HasColumnName("ChargeName_VN")
                    .HasMaxLength(510);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.ServiceTypeId)
                    .IsRequired()
                    .HasColumnName("ServiceTypeID")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Vat).HasColumnName("VAT");
            });

            modelBuilder.Entity<CatChargeDefaultAccount>(entity =>
            {
                entity.ToTable("catChargeDefaultAccount");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ChargeId).HasColumnName("ChargeID");

                entity.Property(e => e.CreditAccountNo).HasMaxLength(50);

                entity.Property(e => e.CreditVat)
                    .HasColumnName("CreditVAT")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DebitAccountNo).HasMaxLength(50);

                entity.Property(e => e.DebitVat)
                    .HasColumnName("DebitVAT")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Type).HasMaxLength(50);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatCommodity>(entity =>
            {
                entity.ToTable("catCommodity");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CommodityGroupId).HasColumnName("CommodityGroupID");

                entity.Property(e => e.CommodityNameEn)
                    .HasColumnName("CommodityName_EN")
                    .HasMaxLength(300);

                entity.Property(e => e.CommodityNameVn)
                    .HasColumnName("CommodityName_VN")
                    .HasMaxLength(300);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

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

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.GroupNameEn)
                    .HasColumnName("GroupName_EN")
                    .HasMaxLength(300);

                entity.Property(e => e.GroupNameVn)
                    .HasColumnName("GroupName_VN")
                    .HasMaxLength(300);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Note).HasMaxLength(500);

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

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(300);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(300);

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

                entity.Property(e => e.CurrencyName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatCustomerPlace>(entity =>
            {
                entity.ToTable("catCustomerPlace");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Address).HasMaxLength(350);

                entity.Property(e => e.ContactNo)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.ContactPerson).HasMaxLength(255);

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Note).HasMaxLength(350);

                entity.Property(e => e.PlaceId).HasColumnName("PlaceID");

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

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DeptName).HasMaxLength(100);

                entity.Property(e => e.Description).HasMaxLength(300);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

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
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AddressEn)
                    .HasColumnName("Address_EN")
                    .HasMaxLength(510);

                entity.Property(e => e.AddressShippingEn)
                    .HasColumnName("AddressShipping_EN")
                    .HasMaxLength(510);

                entity.Property(e => e.AddressShippingVn)
                    .HasColumnName("AddressShipping_VN")
                    .HasMaxLength(510);

                entity.Property(e => e.AddressVn)
                    .HasColumnName("Address_VN")
                    .HasMaxLength(510);

                entity.Property(e => e.BankAccountAddress).HasMaxLength(510);

                entity.Property(e => e.BankAccountName).HasMaxLength(510);

                entity.Property(e => e.BankAccountNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContactPerson).HasMaxLength(510);

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.CountryShippingId).HasColumnName("CountryShippingID");

                entity.Property(e => e.CreditAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DebitAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DepartmentId)
                    .HasColumnName("DepartmentID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Fax)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Note).HasMaxLength(2000);

                entity.Property(e => e.ParentId)
                    .HasColumnName("ParentID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PartnerGroup)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PartnerNameEn)
                    .HasColumnName("PartnerName_EN")
                    .HasMaxLength(510);

                entity.Property(e => e.PartnerNameVn)
                    .HasColumnName("PartnerName_VN")
                    .HasMaxLength(510);

                entity.Property(e => e.PaymentBeneficiary).HasMaxLength(500);

                entity.Property(e => e.PercentCredit).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");

                entity.Property(e => e.ProvinceShippingId).HasColumnName("ProvinceShippingID");

                entity.Property(e => e.ReceiveEtaemail).HasColumnName("ReceiveETAEmail");

                entity.Property(e => e.RoundedSoamethod)
                    .HasColumnName("RoundedSOAMethod")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SalePersonId)
                    .HasColumnName("SalePersonID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShortName).HasMaxLength(500);

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

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");

                entity.Property(e => e.ZipCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ZipCodeShipping)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatPartnerContact>(entity =>
            {
                entity.ToTable("catPartnerContact");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Birthday).HasColumnType("smalldatetime");

                entity.Property(e => e.CellPhone)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ContactNameEn)
                    .HasColumnName("ContactName_EN")
                    .HasMaxLength(510);

                entity.Property(e => e.ContactNameVn)
                    .HasColumnName("ContactName_VN")
                    .HasMaxLength(510);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.FieldInterested).HasMaxLength(3000);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.JobTitle).HasMaxLength(510);

                entity.Property(e => e.Notes).HasMaxLength(4000);

                entity.Property(e => e.PartnerId)
                    .IsRequired()
                    .HasColumnName("PartnerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SugarId)
                    .HasColumnName("SugarID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Partner)
                    .WithMany(p => p.CatPartnerContact)
                    .HasForeignKey(d => d.PartnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_catPartnerContact_catPartner");
            });

            modelBuilder.Entity<CatPartnerContract>(entity =>
            {
                entity.ToTable("catPartnerContract");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ActiveBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ActiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.ContractNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreditAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.EffectiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.ExpiryOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Note).HasMaxLength(1000);

                entity.Property(e => e.PartnerId)
                    .HasColumnName("PartnerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentDeadlineUnit)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SugarId)
                    .HasColumnName("SugarID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatPartnerGroup>(entity =>
            {
                entity.ToTable("catPartnerGroup");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.GroupNameEn)
                    .IsRequired()
                    .HasColumnName("GroupName_EN")
                    .HasMaxLength(50);

                entity.Property(e => e.GroupNameVn)
                    .HasColumnName("GroupName_VN")
                    .HasMaxLength(50);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

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

                entity.Property(e => e.Address).HasMaxLength(100);

                entity.Property(e => e.AreaId)
                    .HasColumnName("AreaID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Code)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DisplayName).HasMaxLength(400);

                entity.Property(e => e.DistrictId).HasColumnName("DistrictID");

                entity.Property(e => e.GeoCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.LocalAreaId)
                    .HasColumnName("LocalAreaID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModeOfTransport)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(400);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(400);

                entity.Property(e => e.Note).HasMaxLength(1000);

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
            });

            modelBuilder.Entity<CatPlaceType>(entity =>
            {
                entity.ToTable("catPlaceType");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(100);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(100);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatSaleResource>(entity =>
            {
                entity.ToTable("catSaleResource");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.ResourceName).HasMaxLength(200);

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

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.DescriptionEn)
                    .HasColumnName("Description_EN")
                    .HasMaxLength(500);

                entity.Property(e => e.DescriptionVn)
                    .HasColumnName("Description_VN")
                    .HasMaxLength(500);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.StageNameEn)
                    .HasColumnName("StageName_EN")
                    .HasMaxLength(250);

                entity.Property(e => e.StageNameVn)
                    .HasColumnName("StageName_VN")
                    .HasMaxLength(250);

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

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(1020);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(300);

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

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DescriptionEn).HasMaxLength(400);

                entity.Property(e => e.DescriptionVn).HasMaxLength(400);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.UnitNameEn)
                    .HasColumnName("UnitName_EN")
                    .HasMaxLength(400);

                entity.Property(e => e.UnitNameVn)
                    .HasColumnName("UnitName_VN")
                    .HasMaxLength(400);

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

            modelBuilder.Entity<CsShipment>(entity =>
            {
                entity.HasKey(e => e.JobId)
                    .ForSqlServerIsClustered(false);

                entity.ToTable("csShipment");

                entity.Property(e => e.JobId)
                    .HasColumnName("JobID")
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.AgentId)
                    .HasColumnName("AgentID")
                    .HasMaxLength(100);

                entity.Property(e => e.BranchId)
                    .HasColumnName("BranchID")
                    .HasMaxLength(100);

                entity.Property(e => e.CargoOp)
                    .HasColumnName("CargoOP")
                    .HasMaxLength(100);

                entity.Property(e => e.Cbm).HasColumnName("CBM");

                entity.Property(e => e.ColoaderId)
                    .HasColumnName("ColoaderID")
                    .HasMaxLength(100);

                entity.Property(e => e.Commodity).HasMaxLength(100);

                entity.Property(e => e.ContainerSize).HasMaxLength(100);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DeliveryPoint).HasMaxLength(100);

                entity.Property(e => e.Dimension).HasMaxLength(100);

                entity.Property(e => e.FlightVesselDateConfirm).HasColumnType("datetime");

                entity.Property(e => e.FlightVesselNo).HasMaxLength(100);

                entity.Property(e => e.InvoiceNo).HasMaxLength(100);

                entity.Property(e => e.LoadingDate).HasColumnType("datetime");

                entity.Property(e => e.Mawb)
                    .HasColumnName("MAWB")
                    .HasMaxLength(100);

                entity.Property(e => e.Mbltype)
                    .HasColumnName("MBLType")
                    .HasMaxLength(100);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.Opic)
                    .HasColumnName("OPIC")
                    .HasMaxLength(100);

                entity.Property(e => e.PaymentTerm).HasMaxLength(100);

                entity.Property(e => e.Po)
                    .HasColumnName("PO")
                    .HasMaxLength(100);

                entity.Property(e => e.PortofLadingId)
                    .HasColumnName("PortofLadingID")
                    .HasMaxLength(100);

                entity.Property(e => e.PortofUnladingId)
                    .HasColumnName("PortofUnladingID")
                    .HasMaxLength(100);

                entity.Property(e => e.RequestedDate).HasColumnType("datetime");

                entity.Property(e => e.RouteShipment).HasMaxLength(300);

                entity.Property(e => e.ServiceMode).HasMaxLength(100);

                entity.Property(e => e.ShipmentLockedDate).HasColumnType("datetime");

                entity.Property(e => e.TypeOfService).HasMaxLength(100);

                entity.Property(e => e.Unit).HasMaxLength(100);

                entity.Property(e => e.UserCreated).HasMaxLength(100);

                entity.Property(e => e.UserModified).HasMaxLength(100);

                entity.Property(e => e.Voy).HasMaxLength(100);

                entity.Property(e => e.WareHouseId)
                    .HasColumnName("WareHouseID")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<CsShipmentBuyingRate>(entity =>
            {
                entity.HasKey(e => new { e.Hawbno, e.ChagreFeeId, e.Qunit, e.Collect })
                    .ForSqlServerIsClustered(false);

                entity.ToTable("csShipmentBuyingRate");

                entity.Property(e => e.Hawbno)
                    .HasColumnName("HAWBNO")
                    .HasMaxLength(50);

                entity.Property(e => e.ChagreFeeId)
                    .HasColumnName("ChagreFeeID")
                    .HasMaxLength(50);

                entity.Property(e => e.Qunit)
                    .HasColumnName("QUnit")
                    .HasMaxLength(50);

                entity.Property(e => e.AcctantCreated).HasMaxLength(100);

                entity.Property(e => e.AcctantDate).HasMaxLength(100);

                entity.Property(e => e.Address).HasMaxLength(300);

                entity.Property(e => e.AmountNoVatusd).HasColumnName("AmountNoVATUSD");

                entity.Property(e => e.AmountNoVatvnd).HasColumnName("AmountNoVATVND");

                entity.Property(e => e.AmountVatusd).HasColumnName("AmountVATUSD");

                entity.Property(e => e.AmountVatvnd).HasColumnName("AmountVATVND");

                entity.Property(e => e.ContactCollect).HasMaxLength(300);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CurrUnit).HasMaxLength(100);

                entity.Property(e => e.CurrencyConvertRate).HasMaxLength(100);

                entity.Property(e => e.DocNo).HasMaxLength(100);

                entity.Property(e => e.ExRateInvoiceVnd).HasColumnName("ExRateInvoiceVND");

                entity.Property(e => e.ExRateSaleVnd).HasColumnName("ExRateSaleVND");

                entity.Property(e => e.Fax).HasMaxLength(100);

                entity.Property(e => e.InoiceNo).HasMaxLength(100);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.NameOfCollect).HasMaxLength(300);

                entity.Property(e => e.Notes).HasMaxLength(300);

                entity.Property(e => e.PaidDate).HasColumnType("datetime");

                entity.Property(e => e.SeriNo).HasMaxLength(100);

                entity.Property(e => e.ShipmentLockedDate).HasColumnType("datetime");

                entity.Property(e => e.Soano)
                    .HasColumnName("SOANo")
                    .HasMaxLength(100);

                entity.Property(e => e.TaxCode).HasMaxLength(100);

                entity.Property(e => e.Tel).HasMaxLength(100);

                entity.Property(e => e.UserCreated).HasMaxLength(100);

                entity.Property(e => e.UserModified).HasMaxLength(100);

                entity.Property(e => e.Vat).HasColumnName("VAT");

                entity.Property(e => e.VatinvId)
                    .HasColumnName("VATInvID")
                    .HasMaxLength(100);

                entity.Property(e => e.VoucherId)
                    .HasColumnName("VoucherID")
                    .HasMaxLength(100);

                entity.Property(e => e.VoucherIdse)
                    .HasColumnName("VoucherIDSE")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<CsShipmentDetail>(entity =>
            {
                entity.HasKey(e => new { e.JobId, e.LotNo })
                    .ForSqlServerIsClustered(false);

                entity.ToTable("csShipmentDetail");

                entity.Property(e => e.JobId)
                    .HasColumnName("JobID")
                    .HasMaxLength(50);

                entity.Property(e => e.LotNo).HasMaxLength(50);

                entity.Property(e => e.Attn).HasMaxLength(300);

                entity.Property(e => e.BookingCustomsNo)
                    .HasColumnName("BookingCustomsNO")
                    .HasMaxLength(100);

                entity.Property(e => e.Cbm).HasColumnName("CBM");

                entity.Property(e => e.ContainerSize).HasMaxLength(100);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Deliveryat).HasMaxLength(100);

                entity.Property(e => e.Eta)
                    .HasColumnName("ETA")
                    .HasColumnType("datetime");

                entity.Property(e => e.Etd)
                    .HasColumnName("ETD")
                    .HasColumnType("datetime");

                entity.Property(e => e.Hwbno)
                    .HasColumnName("HWBNO")
                    .HasMaxLength(100);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.NominationParty).HasMaxLength(510);

                entity.Property(e => e.Notes).HasMaxLength(510);

                entity.Property(e => e.ReceiptAt).HasMaxLength(100);

                entity.Property(e => e.SalesManId)
                    .HasColumnName("SalesManID")
                    .HasMaxLength(100);

                entity.Property(e => e.ShipperId)
                    .HasColumnName("ShipperID")
                    .HasMaxLength(100);

                entity.Property(e => e.Unit).HasMaxLength(100);

                entity.Property(e => e.UserCreated).HasMaxLength(100);

                entity.Property(e => e.UserModified).HasMaxLength(100);
            });

            modelBuilder.Entity<CsShipmentHawbdetail>(entity =>
            {
                entity.HasKey(e => e.Hwbno)
                    .ForSqlServerIsClustered(false);

                entity.ToTable("csShipmentHAWBDetail");

                entity.Property(e => e.Hwbno)
                    .HasColumnName("HWBNO")
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.Cbm).HasColumnName("CBM");

                entity.Property(e => e.CommodityItemNo).HasMaxLength(300);

                entity.Property(e => e.MaskNos).HasColumnType("ntext");

                entity.Property(e => e.NatureQualityOfGoods).HasColumnType("ntext");

                entity.Property(e => e.NoPieces).HasMaxLength(100);

                entity.Property(e => e.RateClass).HasMaxLength(100);

                entity.Property(e => e.Sidescription)
                    .HasColumnName("SIDescription")
                    .HasColumnType("ntext");

                entity.Property(e => e.Unit).HasMaxLength(100);

                entity.Property(e => e.Wlbs).HasMaxLength(100);
            });

            modelBuilder.Entity<CsShipmentProfitShares>(entity =>
            {
                entity.HasKey(e => new { e.Hawbno, e.PartnerId, e.ChagreFeeId, e.Qunit, e.Debit })
                    .ForSqlServerIsClustered(false);

                entity.ToTable("csShipmentProfitShares");

                entity.Property(e => e.Hawbno)
                    .HasColumnName("HAWBNO")
                    .HasMaxLength(50);

                entity.Property(e => e.PartnerId)
                    .HasColumnName("PartnerID")
                    .HasMaxLength(50);

                entity.Property(e => e.ChagreFeeId)
                    .HasColumnName("ChagreFeeID")
                    .HasMaxLength(50);

                entity.Property(e => e.Qunit)
                    .HasColumnName("QUnit")
                    .HasMaxLength(50);

                entity.Property(e => e.AcctantCreated).HasMaxLength(100);

                entity.Property(e => e.AcctantDate).HasMaxLength(100);

                entity.Property(e => e.AmountNoVatusd).HasColumnName("AmountNoVATUSD");

                entity.Property(e => e.AmountNoVatvnd).HasColumnName("AmountNoVATVND");

                entity.Property(e => e.AmountVatusd).HasColumnName("AmountVATUSD");

                entity.Property(e => e.AmountVatvnd).HasColumnName("AmountVATVND");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CurrUnit).HasMaxLength(100);

                entity.Property(e => e.CurrencyConvertRate).HasMaxLength(100);

                entity.Property(e => e.Docs).HasMaxLength(100);

                entity.Property(e => e.ExRateInvoiceVnd).HasColumnName("ExRateInvoiceVND");

                entity.Property(e => e.ExRateSaleVnd).HasColumnName("ExRateSaleVND");

                entity.Property(e => e.InoiceNo).HasMaxLength(100);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Notes).HasMaxLength(300);

                entity.Property(e => e.Obh).HasColumnName("OBH");

                entity.Property(e => e.ObhpartnerId)
                    .HasColumnName("OBHPartnerID")
                    .HasMaxLength(100);

                entity.Property(e => e.PaidDate).HasColumnType("datetime");

                entity.Property(e => e.SeriNo).HasMaxLength(100);

                entity.Property(e => e.SettlementRefNo).HasMaxLength(100);

                entity.Property(e => e.ShipmentLockedDate).HasColumnType("datetime");

                entity.Property(e => e.Soano)
                    .HasColumnName("SOANo")
                    .HasMaxLength(100);

                entity.Property(e => e.UserCreated).HasMaxLength(100);

                entity.Property(e => e.UserModified).HasMaxLength(100);

                entity.Property(e => e.Vat).HasColumnName("VAT");

                entity.Property(e => e.VatinvId)
                    .HasColumnName("VATInvID")
                    .HasMaxLength(100);

                entity.Property(e => e.Vatname)
                    .HasColumnName("VATName")
                    .HasMaxLength(510);

                entity.Property(e => e.VattaxCode)
                    .HasColumnName("VATTaxCode")
                    .HasMaxLength(100);

                entity.Property(e => e.VoucherId)
                    .HasColumnName("VoucherID")
                    .HasMaxLength(100);

                entity.Property(e => e.VoucherIdse)
                    .HasColumnName("VoucherIDSE")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<CsShipmentSellingRate>(entity =>
            {
                entity.HasKey(e => new { e.Hawbno, e.ChagreFeeId, e.Qunit, e.Collect })
                    .ForSqlServerIsClustered(false);

                entity.ToTable("csShipmentSellingRate");

                entity.Property(e => e.Hawbno)
                    .HasColumnName("HAWBNO")
                    .HasMaxLength(50);

                entity.Property(e => e.ChagreFeeId)
                    .HasColumnName("ChagreFeeID")
                    .HasMaxLength(50);

                entity.Property(e => e.Qunit)
                    .HasColumnName("QUnit")
                    .HasMaxLength(50);

                entity.Property(e => e.AcctantCreated).HasMaxLength(100);

                entity.Property(e => e.AcctantDate).HasMaxLength(100);

                entity.Property(e => e.Address).HasMaxLength(300);

                entity.Property(e => e.AmountNoVatusd).HasColumnName("AmountNoVATUSD");

                entity.Property(e => e.AmountNoVatvnd).HasColumnName("AmountNoVATVND");

                entity.Property(e => e.AmountVatusd).HasColumnName("AmountVATUSD");

                entity.Property(e => e.AmountVatvnd).HasColumnName("AmountVATVND");

                entity.Property(e => e.ContactCollect).HasMaxLength(300);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CurrUnit).HasMaxLength(100);

                entity.Property(e => e.CurrencyConvertRate).HasMaxLength(100);

                entity.Property(e => e.DocNo).HasMaxLength(100);

                entity.Property(e => e.ExRateInvoiceVnd).HasColumnName("ExRateInvoiceVND");

                entity.Property(e => e.ExRateSaleVnd).HasColumnName("ExRateSaleVND");

                entity.Property(e => e.Fax).HasMaxLength(100);

                entity.Property(e => e.InoiceNo).HasMaxLength(100);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.NameOfCollect).HasMaxLength(300);

                entity.Property(e => e.Notes).HasMaxLength(300);

                entity.Property(e => e.PaidDate).HasColumnType("datetime");

                entity.Property(e => e.SeriNo).HasMaxLength(100);

                entity.Property(e => e.ShipmentLockedDate).HasColumnType("datetime");

                entity.Property(e => e.Soano)
                    .HasColumnName("SOANo")
                    .HasMaxLength(100);

                entity.Property(e => e.TaxCode).HasMaxLength(100);

                entity.Property(e => e.Tel).HasMaxLength(100);

                entity.Property(e => e.UserCreated).HasMaxLength(100);

                entity.Property(e => e.UserModified).HasMaxLength(100);

                entity.Property(e => e.Vat).HasColumnName("VAT");

                entity.Property(e => e.VatinvId)
                    .HasColumnName("VATInvID")
                    .HasMaxLength(100);

                entity.Property(e => e.VoucherId)
                    .HasColumnName("VoucherID")
                    .HasMaxLength(100);

                entity.Property(e => e.VoucherIdse)
                    .HasColumnName("VoucherIDSE")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<SysAuthorization>(entity =>
            {
                entity.ToTable("sysAuthorization");

                entity.HasIndex(e => new { e.UserId, e.AssignTo, e.Description, e.StartDate, e.EndDate })
                    .HasName("U_Authorization")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AssignTo)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.EndDate).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.StartDate).HasColumnType("smalldatetime");

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

                entity.HasOne(d => d.AssignToNavigation)
                    .WithMany(p => p.SysAuthorizationAssignToNavigation)
                    .HasForeignKey(d => d.AssignTo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysAuthorization_AssignedUser");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SysAuthorizationUser)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysAuthorization_sysUser");
            });

            modelBuilder.Entity<SysAuthorizationDetail>(entity =>
            {
                entity.ToTable("sysAuthorizationDetail");

                entity.HasIndex(e => new { e.AuthorizationId, e.MenuId, e.WorkPlaceId, e.PermissionId, e.OtherIntructionId })
                    .HasName("U_sysAuthorizationDetail")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AuthorizationId).HasColumnName("AuthorizationID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.MenuId)
                    .IsRequired()
                    .HasColumnName("MenuID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OtherIntructionId).HasColumnName("OtherIntructionID");

                entity.Property(e => e.PermissionId).HasColumnName("PermissionID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");

                entity.HasOne(d => d.Authorization)
                    .WithMany(p => p.SysAuthorizationDetail)
                    .HasForeignKey(d => d.AuthorizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysAuthorizationDetail_sysAuthorization");
            });

            modelBuilder.Entity<SysBu>(entity =>
            {
                entity.ToTable("sysBU");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountName).HasMaxLength(300);

                entity.Property(e => e.AccountNoOverSea)
                    .HasColumnName("AccountNo_OverSea")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountNoVn)
                    .HasColumnName("AccountNo_VN")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AddressEn)
                    .HasColumnName("Address_EN")
                    .HasMaxLength(300);

                entity.Property(e => e.AddressVn)
                    .HasColumnName("Address_VN")
                    .HasMaxLength(300);

                entity.Property(e => e.AreaId)
                    .HasColumnName("AreaID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BankAddress).HasMaxLength(510);

                entity.Property(e => e.BankName).HasMaxLength(300);

                entity.Property(e => e.BunameEn)
                    .HasColumnName("BUName_EN")
                    .HasMaxLength(300);

                entity.Property(e => e.BunameVn)
                    .HasColumnName("BUName_VN")
                    .HasMaxLength(300);

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DescriptionEn)
                    .HasColumnName("Description_EN")
                    .HasMaxLength(510);

                entity.Property(e => e.DescriptionVn)
                    .HasColumnName("Description_VN")
                    .HasMaxLength(510);

                entity.Property(e => e.Email).HasMaxLength(300);

                entity.Property(e => e.Fax).HasMaxLength(100);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Logo).HasColumnType("image");

                entity.Property(e => e.Notes).HasMaxLength(510);

                entity.Property(e => e.Tax)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TaxAccount).HasMaxLength(100);

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

                entity.Property(e => e.Website).HasMaxLength(100);
            });

            modelBuilder.Entity<SysEmployee>(entity =>
            {
                entity.ToTable("sysEmployee");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.AccessDescription).HasMaxLength(100);

                entity.Property(e => e.Birthday).HasColumnType("smalldatetime");

                entity.Property(e => e.Bonus).HasColumnType("decimal(10, 4)");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DepartmentId)
                    .HasColumnName("DepartmentID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.EmpPhotoSize).HasMaxLength(510);

                entity.Property(e => e.EmployeeNameEn)
                    .HasColumnName("EmployeeName_EN")
                    .HasMaxLength(100);

                entity.Property(e => e.EmployeeNameVn)
                    .HasColumnName("EmployeeName_VN")
                    .HasMaxLength(100);

                entity.Property(e => e.ExtNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.HomeAddress).HasMaxLength(300);

                entity.Property(e => e.HomePhone)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Photo).HasColumnType("image");

                entity.Property(e => e.Position).HasMaxLength(100);

                entity.Property(e => e.SaleResource)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SaleTarget).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Signature).HasColumnType("image");

                entity.Property(e => e.Tel)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");

                entity.HasOne(d => d.SaleResourceNavigation)
                    .WithMany(p => p.SysEmployee)
                    .HasForeignKey(d => d.SaleResource)
                    .HasConstraintName("FK_sysEmployee_catSaleResource");
            });

            modelBuilder.Entity<SysMenu>(entity =>
            {
                entity.ToTable("sysMenu");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Arguments).HasMaxLength(600);

                entity.Property(e => e.AssemplyName).HasMaxLength(1000);

                entity.Property(e => e.Description).HasMaxLength(600);

                entity.Property(e => e.Icon).HasMaxLength(200);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(500);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(500);

                entity.Property(e => e.ParentId)
                    .HasColumnName("ParentID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_sysMenu_sysParentMenu");
            });

            modelBuilder.Entity<SysMenuPermissionInstruction>(entity =>
            {
                entity.ToTable("sysMenuPermissionInstruction");

                entity.HasIndex(e => new { e.MenuId, e.PermissionId, e.Code })
                    .HasName("U_Instruction")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.MenuId)
                    .IsRequired()
                    .HasColumnName("MenuID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PermissionId).HasColumnName("PermissionID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysPermission>(entity =>
            {
                entity.ToTable("sysPermission");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Description).HasMaxLength(300);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysRole>(entity =>
            {
                entity.ToTable("sysRole");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysRoleMenu>(entity =>
            {
                entity.ToTable("sysRoleMenu");

                entity.HasIndex(e => new { e.RoleId, e.MenuId })
                    .HasName("U_RoleMenu")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.MenuId)
                    .HasColumnName("MenuID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.SysRoleMenu)
                    .HasForeignKey(d => d.MenuId)
                    .HasConstraintName("FK_RoleMenu_Menu");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.SysRoleMenu)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_sysRoleMenu_sysRole");
            });

            modelBuilder.Entity<SysRolePermission>(entity =>
            {
                entity.ToTable("sysRolePermission");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.MenuId)
                    .IsRequired()
                    .HasColumnName("MenuID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OtherIntructionId).HasColumnName("OtherIntructionID");

                entity.Property(e => e.PermissionId).HasColumnName("PermissionID");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.SysRolePermission)
                    .HasForeignKey(d => d.MenuId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysRolePermission_sysMenu");

                entity.HasOne(d => d.OtherIntruction)
                    .WithMany(p => p.SysRolePermission)
                    .HasForeignKey(d => d.OtherIntructionId)
                    .HasConstraintName("FK_sysRolePermission_sysMenuPermissionInstruction");

                entity.HasOne(d => d.Permission)
                    .WithMany(p => p.SysRolePermission)
                    .HasForeignKey(d => d.PermissionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysRolePermission_sysPermission");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.SysRolePermission)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysRolePermission_sysRole");
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

                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.Property(e => e.Receivers)
                    .HasMaxLength(4000)
                    .IsUnicode(false);

                entity.Property(e => e.SentDateTime).HasColumnType("smalldatetime");

                entity.Property(e => e.SentUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Subject).HasMaxLength(2000);

                entity.Property(e => e.Type)
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

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.EmployeeId)
                    .HasColumnName("EmployeeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Password).HasMaxLength(400);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserGroupId).HasColumnName("UserGroupID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");

                entity.HasOne(d => d.UserGroup)
                    .WithMany(p => p.SysUser)
                    .HasForeignKey(d => d.UserGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysUser_sysUserGroup");

                entity.HasOne(d => d.WorkPlace)
                    .WithMany(p => p.SysUser)
                    .HasForeignKey(d => d.WorkPlaceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysUser_catBranch");
            });

            modelBuilder.Entity<SysUserGroup>(entity =>
            {
                entity.ToTable("sysUserGroup");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Decription).HasMaxLength(1000);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysUserLog>(entity =>
            {
                entity.ToTable("sysUserLog");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ComputerName).HasMaxLength(100);

                entity.Property(e => e.Ip)
                    .HasColumnName("IP")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LoggedInOn).HasColumnType("smalldatetime");

                entity.Property(e => e.LoggedOffOn).HasColumnType("smalldatetime");

                entity.Property(e => e.UserId)
                    .HasColumnName("UserID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<SysUserOtherWorkPlace>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.WorkPlaceId });

                entity.ToTable("sysUserOtherWorkPlace");

                entity.Property(e => e.UserId)
                    .HasColumnName("UserID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SysUserOtherWorkPlace)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysUserOtherBranch_User");

                entity.HasOne(d => d.WorkPlace)
                    .WithMany(p => p.SysUserOtherWorkPlace)
                    .HasForeignKey(d => d.WorkPlaceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysUserOtherBranch_catBranch");
            });

            modelBuilder.Entity<SysUserRole>(entity =>
            {
                entity.ToTable("sysUserRole");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasColumnName("UserID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.SysUserRole)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysUserRole_sysRole");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SysUserRole)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysUserRole_sysUser");

                entity.HasOne(d => d.WorkPlace)
                    .WithMany(p => p.SysUserRole)
                    .HasForeignKey(d => d.WorkPlaceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysUserRole_catBranch");
            });
        }
    }
}
