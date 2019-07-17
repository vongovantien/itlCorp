using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace eFMS.API.Operation.Service.Models
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

        public virtual DbSet<AcctCd> AcctCd { get; set; }
        public virtual DbSet<CatArea> CatArea { get; set; }
        public virtual DbSet<CatBranch> CatBranch { get; set; }
        public virtual DbSet<CatCharge> CatCharge { get; set; }
        public virtual DbSet<CatChargeDefaultAccount> CatChargeDefaultAccount { get; set; }
        public virtual DbSet<CatCommodity> CatCommodity { get; set; }
        public virtual DbSet<CatCommodityGroup> CatCommodityGroup { get; set; }
        public virtual DbSet<CatContainerType> CatContainerType { get; set; }
        public virtual DbSet<CatCountry> CatCountry { get; set; }
        public virtual DbSet<CatCurrency> CatCurrency { get; set; }
        public virtual DbSet<CatCurrencyExchange> CatCurrencyExchange { get; set; }
        public virtual DbSet<CatCustomerPlace> CatCustomerPlace { get; set; }
        public virtual DbSet<CatDepartment> CatDepartment { get; set; }
        public virtual DbSet<CatPartner> CatPartner { get; set; }
        public virtual DbSet<CatPartnerContact> CatPartnerContact { get; set; }
        public virtual DbSet<CatPartnerContract> CatPartnerContract { get; set; }
        public virtual DbSet<CatPartnerGroup> CatPartnerGroup { get; set; }
        public virtual DbSet<CatPlace> CatPlace { get; set; }
        public virtual DbSet<CatPlaceType> CatPlaceType { get; set; }
        public virtual DbSet<CatSaleResource> CatSaleResource { get; set; }
        public virtual DbSet<CatServiceType> CatServiceType { get; set; }
        public virtual DbSet<CatStage> CatStage { get; set; }
        public virtual DbSet<CatTransportationMode> CatTransportationMode { get; set; }
        public virtual DbSet<CatUnit> CatUnit { get; set; }
        public virtual DbSet<CsFcltransactionDetailContainer> CsFcltransactionDetailContainer { get; set; }
        public virtual DbSet<CsManifest> CsManifest { get; set; }
        public virtual DbSet<CsMawbcontainer> CsMawbcontainer { get; set; }
        public virtual DbSet<CsShipmentHawbdetail> CsShipmentHawbdetail { get; set; }
        public virtual DbSet<CsShipmentProfitShares> CsShipmentProfitShares { get; set; }
        public virtual DbSet<CsShipmentSellingRate> CsShipmentSellingRate { get; set; }
        public virtual DbSet<CsShipmentSurcharge> CsShipmentSurcharge { get; set; }
        public virtual DbSet<CsShippingInstruction> CsShippingInstruction { get; set; }
        public virtual DbSet<CsTransaction> CsTransaction { get; set; }
        public virtual DbSet<CsTransactionDetail> CsTransactionDetail { get; set; }
        public virtual DbSet<CustomsDeclaration> CustomsDeclaration { get; set; }
        public virtual DbSet<OpsStageAssigned> OpsStageAssigned { get; set; }
        public virtual DbSet<OpsTransaction> OpsTransaction { get; set; }
        public virtual DbSet<SetEcusconnection> SetEcusconnection { get; set; }
        public virtual DbSet<SysAuthorization> SysAuthorization { get; set; }
        public virtual DbSet<SysAuthorizationDetail> SysAuthorizationDetail { get; set; }
        public virtual DbSet<SysBu> SysBu { get; set; }
        public virtual DbSet<SysEmployee> SysEmployee { get; set; }
        public virtual DbSet<SysMenu> SysMenu { get; set; }
        public virtual DbSet<SysMenuPermissionInstruction> SysMenuPermissionInstruction { get; set; }
        public virtual DbSet<SysNotification> SysNotification { get; set; }
        public virtual DbSet<SysPermission> SysPermission { get; set; }
        public virtual DbSet<SysRole> SysRole { get; set; }
        public virtual DbSet<SysRoleMenu> SysRoleMenu { get; set; }
        public virtual DbSet<SysRolePermission> SysRolePermission { get; set; }
        public virtual DbSet<SysSentEmailHistory> SysSentEmailHistory { get; set; }
        public virtual DbSet<SysUser> SysUser { get; set; }
        public virtual DbSet<SysUserGroup> SysUserGroup { get; set; }
        public virtual DbSet<SysUserLog> SysUserLog { get; set; }
        public virtual DbSet<SysUserNotification> SysUserNotification { get; set; }
        public virtual DbSet<SysUserOtherWorkPlace> SysUserOtherWorkPlace { get; set; }
        public virtual DbSet<SysUserRole> SysUserRole { get; set; }
        public virtual DbSet<TestContainerList> TestContainerList { get; set; }
        public virtual DbSet<TestHouseBillSeaFclexport> TestHouseBillSeaFclexport { get; set; }
        public virtual DbSet<TestSeaFclexportShipment> TestSeaFclexportShipment { get; set; }

        // Unable to generate entity type for table 'dbo.csShipmentHAWBDetailArrivalFreightCharges'. Please see the warning messages.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=192.168.7.88;Database=eFMSTest;User ID=sa;Password=P@ssw0rd;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity<AcctCd>(entity =>
            {
                entity.ToTable("acctCD");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BehalfPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerConfirmDate).HasColumnType("datetime");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.ExportedDate).HasColumnType("datetime");

                entity.Property(e => e.FreightPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.InvoiceNo).HasMaxLength(100);

                entity.Property(e => e.JobId).HasColumnName("JobID");

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

            modelBuilder.Entity<CatArea>(entity =>
            {
                entity.ToTable("catArea");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

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

            modelBuilder.Entity<CatBranch>(entity =>
            {
                entity.ToTable("catBranch");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AddressEn)
                    .HasColumnName("Address_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.AddressVn)
                    .HasColumnName("Address_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.BankAccountUsd)
                    .HasColumnName("BankAccount_USD")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.BankAccountVnd)
                    .HasColumnName("BankAccount_VND")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.BankAddress).HasMaxLength(4000);

                entity.Property(e => e.BankName).HasMaxLength(4000);

                entity.Property(e => e.BranchNameEn)
                    .HasColumnName("BranchName_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.BranchNameVn)
                    .HasColumnName("BranchName_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.Code).HasMaxLength(640);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Fax)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

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

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

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

                entity.Property(e => e.ChargeId).HasColumnName("ChargeID");

                entity.Property(e => e.CreditAccountNo).HasMaxLength(800);

                entity.Property(e => e.CreditVat)
                    .HasColumnName("CreditVAT")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

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

            modelBuilder.Entity<CatCommodity>(entity =>
            {
                entity.ToTable("catCommodity");

                entity.Property(e => e.Id).HasColumnName("ID");

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

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

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

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

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

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

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

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

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

                entity.Property(e => e.CurrencyName).HasMaxLength(800);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

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

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

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

            modelBuilder.Entity<CatCustomerPlace>(entity =>
            {
                entity.ToTable("catCustomerPlace");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Address).HasMaxLength(4000);

                entity.Property(e => e.ContactNo)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.ContactPerson).HasMaxLength(4000);

                entity.Property(e => e.CustomerId)
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.Note).HasMaxLength(4000);

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

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DeptName).HasMaxLength(1600);

                entity.Property(e => e.Description).HasMaxLength(4000);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

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

                entity.Property(e => e.BankAccountAddress).HasMaxLength(4000);

                entity.Property(e => e.BankAccountName).HasMaxLength(4000);

                entity.Property(e => e.BankAccountNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContactPerson).HasMaxLength(4000);

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.CountryShippingId).HasColumnName("CountryShippingID");

                entity.Property(e => e.CreditAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

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

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Note).HasMaxLength(4000);

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

                entity.Property(e => e.Birthday).HasColumnType("datetime");

                entity.Property(e => e.CellPhone)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ContactNameEn)
                    .HasColumnName("ContactName_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.ContactNameVn)
                    .HasColumnName("ContactName_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.FieldInterested).HasMaxLength(4000);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.JobTitle).HasMaxLength(4000);

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

                entity.Property(e => e.ActiveOn).HasColumnType("datetime");

                entity.Property(e => e.ContractNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreditAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.EffectiveOn).HasColumnType("datetime");

                entity.Property(e => e.ExpiryOn).HasColumnType("datetime");

                entity.Property(e => e.Note).HasMaxLength(4000);

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

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

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

                entity.Property(e => e.Address).HasMaxLength(1600);

                entity.Property(e => e.AreaId)
                    .HasColumnName("AreaID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Code)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DisplayName).HasMaxLength(4000);

                entity.Property(e => e.DistrictId).HasColumnName("DistrictID");

                entity.Property(e => e.GeoCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

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

            modelBuilder.Entity<CatSaleResource>(entity =>
            {
                entity.ToTable("catSaleResource");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.ResourceName).HasMaxLength(3200);

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

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

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

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

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

            modelBuilder.Entity<CsFcltransactionDetailContainer>(entity =>
            {
                entity.ToTable("csFCLTransactionDetailContainer");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AppDate).HasColumnType("datetime");

                entity.Property(e => e.AppMode).HasMaxLength(4000);

                entity.Property(e => e.Apptype)
                    .HasColumnName("APPType")
                    .HasMaxLength(800);

                entity.Property(e => e.CargoContact).HasMaxLength(800);

                entity.Property(e => e.CargoContactAddress).HasMaxLength(4000);

                entity.Property(e => e.CargoContactOthers).HasMaxLength(2400);

                entity.Property(e => e.CargoContactTel).HasMaxLength(800);

                entity.Property(e => e.CargoContactTime).HasMaxLength(800);

                entity.Property(e => e.Cdsno)
                    .HasColumnName("CDSNo")
                    .HasMaxLength(4000);

                entity.Property(e => e.Cdstype)
                    .HasColumnName("CDSType")
                    .HasMaxLength(800);

                entity.Property(e => e.ClosingTime).HasMaxLength(800);

                entity.Property(e => e.Consignee).HasMaxLength(4000);

                entity.Property(e => e.ConsigneeId)
                    .HasColumnName("ConsigneeID")
                    .HasMaxLength(800);

                entity.Property(e => e.ContQty).HasMaxLength(800);

                entity.Property(e => e.CustomerId)
                    .HasColumnName("CustomerID")
                    .HasMaxLength(800);

                entity.Property(e => e.DateCreated).HasColumnType("datetime");

                entity.Property(e => e.DateModified).HasColumnType("datetime");

                entity.Property(e => e.DocsRequest).HasColumnType("ntext");

                entity.Property(e => e.EmptyReturnPickup).HasMaxLength(4000);

                entity.Property(e => e.Etd).HasColumnType("datetime");

                entity.Property(e => e.GoodsDescription).HasMaxLength(4000);

                entity.Property(e => e.GoodsNotes).HasMaxLength(4000);

                entity.Property(e => e.Hblno)
                    .HasColumnName("HBLNo")
                    .HasMaxLength(800);

                entity.Property(e => e.IdkeyShipmentDt)
                    .HasColumnName("IDKeyShipmentDT")
                    .HasColumnType("numeric(18, 0)");

                entity.Property(e => e.JobApp).HasMaxLength(800);

                entity.Property(e => e.NmpartyId)
                    .HasColumnName("NMPartyID")
                    .HasMaxLength(800);

                entity.Property(e => e.Notes).HasMaxLength(4000);

                entity.Property(e => e.OperationContact).HasMaxLength(800);

                entity.Property(e => e.Opexecutive)
                    .HasColumnName("OPExecutive")
                    .HasMaxLength(800);

                entity.Property(e => e.Packages).HasMaxLength(800);

                entity.Property(e => e.PortofDischarge).HasMaxLength(800);

                entity.Property(e => e.PortofLoading).HasMaxLength(800);

                entity.Property(e => e.RequestDate).HasColumnType("datetime");

                entity.Property(e => e.RequestNo).HasMaxLength(800);

                entity.Property(e => e.RequestService).HasMaxLength(800);

                entity.Property(e => e.RqTimes).HasMaxLength(800);

                entity.Property(e => e.Shipper).HasMaxLength(4000);

                entity.Property(e => e.ShipperId)
                    .HasColumnName("ShipperID")
                    .HasMaxLength(800);

                entity.Property(e => e.Unit).HasMaxLength(800);

                entity.Property(e => e.VesselVoy).HasMaxLength(2400);

                entity.Property(e => e.Whoismaking).HasMaxLength(800);
            });

            modelBuilder.Entity<CsManifest>(entity =>
            {
                entity.HasKey(e => e.JobId);

                entity.ToTable("csManifest");

                entity.Property(e => e.JobId)
                    .HasColumnName("JobID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Attention).HasMaxLength(250);

                entity.Property(e => e.Consolidator).HasMaxLength(250);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DeConsolidator).HasMaxLength(250);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.InvoiceDate).HasColumnType("datetime");

                entity.Property(e => e.ManifestIssuer).HasMaxLength(500);

                entity.Property(e => e.MasksOfRegistration).HasMaxLength(1000);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

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

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

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

                entity.HasOne(d => d.ContainerType)
                    .WithMany(p => p.CsMawbcontainerContainerType)
                    .HasForeignKey(d => d.ContainerTypeId)
                    .HasConstraintName("FK_csMAWBContainer_catUnit1");

                entity.HasOne(d => d.UnitOfMeasure)
                    .WithMany(p => p.CsMawbcontainerUnitOfMeasure)
                    .HasForeignKey(d => d.UnitOfMeasureId)
                    .HasConstraintName("FK_csMAWBContainer_catUnit");
            });

            modelBuilder.Entity<CsShipmentHawbdetail>(entity =>
            {
                entity.HasKey(e => e.Hwbno)
                    .HasName("csShipmentHAWBDetail_PK")
                    .ForSqlServerIsClustered(false);

                entity.ToTable("csShipmentHAWBDetail");

                entity.Property(e => e.Hwbno)
                    .HasColumnName("HWBNO")
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.Cbm).HasColumnName("CBM");

                entity.Property(e => e.CommodityItemNo).HasMaxLength(4000);

                entity.Property(e => e.MaskNos).HasColumnType("ntext");

                entity.Property(e => e.NatureQualityOfGoods).HasColumnType("ntext");

                entity.Property(e => e.NoPieces).HasMaxLength(1600);

                entity.Property(e => e.RateClass).HasMaxLength(1600);

                entity.Property(e => e.Sidescription)
                    .HasColumnName("SIDescription")
                    .HasColumnType("ntext");

                entity.Property(e => e.Unit).HasMaxLength(1600);

                entity.Property(e => e.Wlbs).HasMaxLength(1600);
            });

            modelBuilder.Entity<CsShipmentProfitShares>(entity =>
            {
                entity.HasKey(e => new { e.Hawbno, e.PartnerId, e.ChagreFeeId, e.Qunit, e.Debit })
                    .HasName("csShipmentProfitShares_PK")
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

                entity.Property(e => e.AcctantCreated).HasMaxLength(1600);

                entity.Property(e => e.AcctantDate).HasMaxLength(1600);

                entity.Property(e => e.AmountNoVatusd).HasColumnName("AmountNoVATUSD");

                entity.Property(e => e.AmountNoVatvnd).HasColumnName("AmountNoVATVND");

                entity.Property(e => e.AmountVatusd).HasColumnName("AmountVATUSD");

                entity.Property(e => e.AmountVatvnd).HasColumnName("AmountVATVND");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CurrUnit).HasMaxLength(1600);

                entity.Property(e => e.CurrencyConvertRate).HasMaxLength(1600);

                entity.Property(e => e.Docs).HasMaxLength(1600);

                entity.Property(e => e.ExRateInvoiceVnd).HasColumnName("ExRateInvoiceVND");

                entity.Property(e => e.ExRateSaleVnd).HasColumnName("ExRateSaleVND");

                entity.Property(e => e.InoiceNo).HasMaxLength(1600);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Notes).HasMaxLength(4000);

                entity.Property(e => e.Obh).HasColumnName("OBH");

                entity.Property(e => e.ObhpartnerId)
                    .HasColumnName("OBHPartnerID")
                    .HasMaxLength(1600);

                entity.Property(e => e.PaidDate).HasColumnType("datetime");

                entity.Property(e => e.SeriNo).HasMaxLength(1600);

                entity.Property(e => e.SettlementRefNo).HasMaxLength(1600);

                entity.Property(e => e.ShipmentLockedDate).HasColumnType("datetime");

                entity.Property(e => e.Soano)
                    .HasColumnName("SOANo")
                    .HasMaxLength(1600);

                entity.Property(e => e.UserCreated).HasMaxLength(1600);

                entity.Property(e => e.UserModified).HasMaxLength(1600);

                entity.Property(e => e.Vat).HasColumnName("VAT");

                entity.Property(e => e.VatinvId)
                    .HasColumnName("VATInvID")
                    .HasMaxLength(1600);

                entity.Property(e => e.Vatname)
                    .HasColumnName("VATName")
                    .HasMaxLength(4000);

                entity.Property(e => e.VattaxCode)
                    .HasColumnName("VATTaxCode")
                    .HasMaxLength(1600);

                entity.Property(e => e.VoucherId)
                    .HasColumnName("VoucherID")
                    .HasMaxLength(1600);

                entity.Property(e => e.VoucherIdse)
                    .HasColumnName("VoucherIDSE")
                    .HasMaxLength(1600);
            });

            modelBuilder.Entity<CsShipmentSellingRate>(entity =>
            {
                entity.HasKey(e => new { e.Hawbno, e.ChagreFeeId, e.Qunit, e.Collect })
                    .HasName("csShipmentSellingRate_PK")
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

                entity.Property(e => e.AcctantCreated).HasMaxLength(1600);

                entity.Property(e => e.AcctantDate).HasMaxLength(1600);

                entity.Property(e => e.Address).HasMaxLength(4000);

                entity.Property(e => e.AmountNoVatusd).HasColumnName("AmountNoVATUSD");

                entity.Property(e => e.AmountNoVatvnd).HasColumnName("AmountNoVATVND");

                entity.Property(e => e.AmountVatusd).HasColumnName("AmountVATUSD");

                entity.Property(e => e.AmountVatvnd).HasColumnName("AmountVATVND");

                entity.Property(e => e.ContactCollect).HasMaxLength(4000);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CurrUnit).HasMaxLength(1600);

                entity.Property(e => e.CurrencyConvertRate).HasMaxLength(1600);

                entity.Property(e => e.DocNo).HasMaxLength(1600);

                entity.Property(e => e.ExRateInvoiceVnd).HasColumnName("ExRateInvoiceVND");

                entity.Property(e => e.ExRateSaleVnd).HasColumnName("ExRateSaleVND");

                entity.Property(e => e.Fax).HasMaxLength(1600);

                entity.Property(e => e.InoiceNo).HasMaxLength(1600);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.NameOfCollect).HasMaxLength(4000);

                entity.Property(e => e.Notes).HasMaxLength(4000);

                entity.Property(e => e.PaidDate).HasColumnType("datetime");

                entity.Property(e => e.SeriNo).HasMaxLength(1600);

                entity.Property(e => e.ShipmentLockedDate).HasColumnType("datetime");

                entity.Property(e => e.Soano)
                    .HasColumnName("SOANo")
                    .HasMaxLength(1600);

                entity.Property(e => e.TaxCode).HasMaxLength(1600);

                entity.Property(e => e.Tel).HasMaxLength(1600);

                entity.Property(e => e.UserCreated).HasMaxLength(1600);

                entity.Property(e => e.UserModified).HasMaxLength(1600);

                entity.Property(e => e.Vat).HasColumnName("VAT");

                entity.Property(e => e.VatinvId)
                    .HasColumnName("VATInvID")
                    .HasMaxLength(1600);

                entity.Property(e => e.VoucherId)
                    .HasColumnName("VoucherID")
                    .HasMaxLength(1600);

                entity.Property(e => e.VoucherIdse)
                    .HasColumnName("VoucherIDSE")
                    .HasMaxLength(1600);
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

                entity.Property(e => e.AccountantDate).HasColumnType("datetime");

                entity.Property(e => e.AccountantId)
                    .HasColumnName("AccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantNote).HasMaxLength(500);

                entity.Property(e => e.AccountantStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Cdno)
                    .HasColumnName("CDNo")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChargeId).HasColumnName("ChargeID");

                entity.Property(e => e.ChiefAccountantDate).HasColumnType("datetime");

                entity.Property(e => e.ChiefAccountantId)
                    .HasColumnName("ChiefAccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefAccountantNote).HasMaxLength(500);

                entity.Property(e => e.ChiefAccountantStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CsdateSettlement)
                    .HasColumnName("CSDateSettlement")
                    .HasColumnType("datetime");

                entity.Property(e => e.Csidsettlement)
                    .HasColumnName("CSIDSettlement")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CsstatusSettlement)
                    .HasColumnName("CSStatusSettlement")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .IsRequired()
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DocNo).HasMaxLength(500);

                entity.Property(e => e.ExchangeDate).HasColumnType("datetime");

                entity.Property(e => e.Hblid).HasColumnName("HBLID");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.InvoiceNo).HasMaxLength(50);

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.ObjectBePaid)
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

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReceiverId)
                    .HasColumnName("ReceiverID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReceiverObject)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SettlementCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SoaadjustmentReason)
                    .HasColumnName("SOAAdjustmentReason")
                    .HasMaxLength(500);

                entity.Property(e => e.SoaadjustmentRequestedDate)
                    .HasColumnName("SOAAdjustmentRequestedDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.SoaadjustmentRequestor)
                    .HasColumnName("SOAAdjustmentRequestor")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Soaclosed).HasColumnName("SOAClosed");

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

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnlockedSoadirector)
                    .HasColumnName("UnlockedSOADirector")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UnlockedSoadirectorDate)
                    .HasColumnName("UnlockedSOADirectorDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.UnlockedSoadirectorStatus)
                    .HasColumnName("UnlockedSOADirectorStatus")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UnlockedSoasaleMan)
                    .HasColumnName("UnlockedSOASaleMan")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UnlockedSoasaleManDate)
                    .HasColumnName("UnlockedSOASaleManDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.UnlockedSoasaleManStatus)
                    .HasColumnName("UnlockedSOASaleManStatus")
                    .HasMaxLength(50)
                    .IsUnicode(false);

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

            modelBuilder.Entity<CsShippingInstruction>(entity =>
            {
                entity.HasKey(e => e.JobId);

                entity.ToTable("csShippingInstruction");

                entity.Property(e => e.JobId)
                    .HasColumnName("JobID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ActualConsigneeDescription).HasMaxLength(1000);

                entity.Property(e => e.ActualConsigneeId)
                    .HasColumnName("ActualConsigneeID")
                    .HasMaxLength(250);

                entity.Property(e => e.ActualShipperDescription).HasMaxLength(1000);

                entity.Property(e => e.ActualShipperId)
                    .HasColumnName("ActualShipperID")
                    .HasMaxLength(250);

                entity.Property(e => e.BookingNo).HasMaxLength(250);

                entity.Property(e => e.CargoNoticeRecevier).HasMaxLength(250);

                entity.Property(e => e.ConsigneeDescription).HasMaxLength(1000);

                entity.Property(e => e.ConsigneeId)
                    .HasColumnName("ConsigneeID")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.ContainerNote).HasMaxLength(250);

                entity.Property(e => e.ContainerSealNo).HasMaxLength(250);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.GoodsDescription).HasMaxLength(500);

                entity.Property(e => e.GrossWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.InvoiceDate).HasColumnType("datetime");

                entity.Property(e => e.InvoiceNoticeRecevier).HasMaxLength(250);

                entity.Property(e => e.IssuedUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LoadingDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.PackagesNote).HasMaxLength(250);

                entity.Property(e => e.PaymenType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PoDelivery).HasMaxLength(250);

                entity.Property(e => e.Pod).HasColumnName("POD");

                entity.Property(e => e.Pol).HasColumnName("POL");

                entity.Property(e => e.Remark).HasMaxLength(1000);

                entity.Property(e => e.RouteInfo).HasMaxLength(500);

                entity.Property(e => e.Shipper).HasMaxLength(500);

                entity.Property(e => e.Supplier).HasMaxLength(250);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Volume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.VoyNo).HasMaxLength(1600);
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

                entity.Property(e => e.AgentId)
                    .HasColumnName("AgentID")
                    .HasMaxLength(1600);

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

                entity.Property(e => e.ContainerSize).HasMaxLength(1600);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DeliveryPoint).HasMaxLength(1600);

                entity.Property(e => e.DesOfGoods).HasMaxLength(1600);

                entity.Property(e => e.Dimension).HasMaxLength(1600);

                entity.Property(e => e.Eta)
                    .HasColumnName("ETA")
                    .HasColumnType("datetime");

                entity.Property(e => e.Etd)
                    .HasColumnName("ETD")
                    .HasColumnType("datetime");

                entity.Property(e => e.FlightVesselConfirmedDate).HasColumnType("datetime");

                entity.Property(e => e.FlightVesselName).HasMaxLength(4000);

                entity.Property(e => e.GrossWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.InvoiceNo).HasMaxLength(1600);

                entity.Property(e => e.JobNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LoadingDate).HasColumnType("datetime");

                entity.Property(e => e.LockedDate).HasColumnType("datetime");

                entity.Property(e => e.Mawb)
                    .HasColumnName("MAWB")
                    .HasMaxLength(800);

                entity.Property(e => e.Mbltype)
                    .HasColumnName("MBLType")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.NetWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Notes).HasMaxLength(4000);

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

                entity.Property(e => e.RequestedDate).HasColumnType("datetime");

                entity.Property(e => e.RouteShipment).HasMaxLength(4000);

                entity.Property(e => e.ServiceMode).HasMaxLength(1600);

                entity.Property(e => e.ShipmentType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingServiceType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

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

                entity.Property(e => e.WareHouseId)
                    .HasColumnName("WareHouseID")
                    .HasMaxLength(1600);
            });

            modelBuilder.Entity<CsTransactionDetail>(entity =>
            {
                entity.ToTable("csTransactionDetail");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ClosingDate).HasColumnType("datetime");

                entity.Property(e => e.Commodity).HasMaxLength(1600);

                entity.Property(e => e.ConsigneeDescription).HasMaxLength(500);

                entity.Property(e => e.ConsigneeId)
                    .HasColumnName("ConsigneeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerId)
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomsBookingNo).HasMaxLength(800);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DeliveryPlace).HasMaxLength(500);

                entity.Property(e => e.DesOfGoods).HasMaxLength(1600);

                entity.Property(e => e.ExportReferenceNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FinalDestinationPlace).HasMaxLength(500);

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
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Hbltype)
                    .HasColumnName("HBLType")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Hwbno)
                    .HasColumnName("HWBNo")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InWord).HasMaxLength(4000);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.IssueHblplaceAndDate)
                    .HasColumnName("IssueHBLPlaceAndDate")
                    .HasMaxLength(4000);

                entity.Property(e => e.JobId).HasColumnName("JobID");

                entity.Property(e => e.JobNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LocalVoyNo).HasMaxLength(800);

                entity.Property(e => e.ManifestRefNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Mawb)
                    .HasColumnName("MAWB")
                    .HasMaxLength(800);

                entity.Property(e => e.MoveType).HasMaxLength(160);

                entity.Property(e => e.NotifyPartyDescription).HasMaxLength(500);

                entity.Property(e => e.NotifyPartyId)
                    .HasColumnName("NotifyPartyID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OceanVoyNo).HasMaxLength(800);

                entity.Property(e => e.OnBoardStatus).HasMaxLength(4000);

                entity.Property(e => e.OriginBlnumber).HasColumnName("OriginBLNumber");

                entity.Property(e => e.OriginCountryId).HasColumnName("OriginCountryID");

                entity.Property(e => e.PackageContainer).HasMaxLength(1600);

                entity.Property(e => e.PickupPlace).HasMaxLength(500);

                entity.Property(e => e.PlaceFreightPay).HasMaxLength(4000);

                entity.Property(e => e.Pod).HasColumnName("POD");

                entity.Property(e => e.Pol).HasColumnName("POL");

                entity.Property(e => e.PurchaseOrderNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReferenceNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SailingDate).HasColumnType("datetime");

                entity.Property(e => e.SaleManId)
                    .HasColumnName("SaleManID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceType).HasMaxLength(160);

                entity.Property(e => e.ShipperDescription).HasMaxLength(500);

                entity.Property(e => e.ShipperId)
                    .HasColumnName("ShipperID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingMark).HasMaxLength(4000);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Job)
                    .WithMany(p => p.CsTransactionDetail)
                    .HasForeignKey(d => d.JobId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_csTransactionDetail_csTransaction");
            });

            modelBuilder.Entity<CustomsDeclaration>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CargoType)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Cbm)
                    .HasColumnName("CBM")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ClearanceDate).HasColumnType("datetime");

                entity.Property(e => e.ClearanceNo)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CommodityCode)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DocumentType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ExportCountryCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FirstClearanceNo).HasMaxLength(50);

                entity.Property(e => e.Gateway)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.GrossWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Hblid)
                    .HasColumnName("HBLID")
                    .HasMaxLength(50)
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
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.NetWeight).HasColumnType("decimal(18, 4)");

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

            modelBuilder.Entity<OpsStageAssigned>(entity =>
            {
                entity.ToTable("opsStageAssigned");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Comment).HasMaxLength(200);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Deadline).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.JobId).HasColumnName("JobID");

                entity.Property(e => e.MainPersonInCharge)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.Property(e => e.ProcessTime).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.RealPersonInCharge)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StageId).HasColumnName("StageID");

                entity.Property(e => e.Status).HasMaxLength(10);

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

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CurrentStatus).HasMaxLength(10);

                entity.Property(e => e.CustomerId)
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FieldOpsId)
                    .HasColumnName("FieldOpsID")
                    .HasMaxLength(200);

                entity.Property(e => e.FinishDate).HasColumnType("datetime");

                entity.Property(e => e.FlightVessel).HasMaxLength(200);

                entity.Property(e => e.Hblid).HasColumnName("HBLID");

                entity.Property(e => e.Hwbno)
                    .HasColumnName("HWBNo")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.InvoiceNo).HasMaxLength(200);

                entity.Property(e => e.JobNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Mblno)
                    .HasColumnName("MBLNO")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

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

            modelBuilder.Entity<SetEcusconnection>(entity =>
            {
                entity.ToTable("setECUSConnection");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.Dbname)
                    .IsRequired()
                    .HasColumnName("DBName")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Dbpassword)
                    .IsRequired()
                    .HasColumnName("DBPassword")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Dbusername)
                    .IsRequired()
                    .HasColumnName("DBUsername")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

                entity.Property(e => e.InactiveOn).HasMaxLength(10);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Note).HasMaxLength(10);

                entity.Property(e => e.ServerName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
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

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

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

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

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

                entity.Property(e => e.AccountName).HasMaxLength(4000);

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

                entity.Property(e => e.Logo).HasColumnType("image");

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

                entity.Property(e => e.Birthday).HasColumnType("datetime");

                entity.Property(e => e.Bonus).HasColumnType("decimal(10, 4)");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

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

                entity.Property(e => e.Photo).HasColumnType("image");

                entity.Property(e => e.Position).HasMaxLength(1600);

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

                entity.Property(e => e.Arguments).HasMaxLength(4000);

                entity.Property(e => e.AssemplyName).HasMaxLength(4000);

                entity.Property(e => e.Description).HasMaxLength(4000);

                entity.Property(e => e.Icon).HasMaxLength(3200);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(4000);

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

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(4000);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

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

            modelBuilder.Entity<SysNotification>(entity =>
            {
                entity.ToTable("sysNotification");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.Priority).HasMaxLength(160);

                entity.Property(e => e.ReveiverUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Title).HasMaxLength(4000);

                entity.Property(e => e.Type).HasMaxLength(160);

                entity.Property(e => e.UrlReference).HasMaxLength(1600);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysPermission>(entity =>
            {
                entity.ToTable("sysPermission");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(4000);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

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

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(4000);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(3200);

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

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

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

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

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

            modelBuilder.Entity<SysUser>(entity =>
            {
                entity.ToTable("sysUser");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.EmployeeId)
                    .HasColumnName("EmployeeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Password).HasMaxLength(4000);

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

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.Decription).HasMaxLength(4000);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(3200);

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

                entity.Property(e => e.ComputerName).HasMaxLength(1600);

                entity.Property(e => e.Ip)
                    .HasColumnName("IP")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LoggedInOn).HasColumnType("datetime");

                entity.Property(e => e.LoggedOffOn).HasColumnType("datetime");

                entity.Property(e => e.UserId)
                    .HasColumnName("UserID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<SysUserNotification>(entity =>
            {
                entity.ToTable("sysUserNotification");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.IsRead).HasDefaultValueSql("((0))");

                entity.Property(e => e.NotitficationId).HasColumnName("NotitficationID");

                entity.Property(e => e.UserId)
                    .HasColumnName("UserID")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysUserOtherWorkPlace>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.WorkPlaceId })
                    .HasName("PK_sysUserOtherBranch");

                entity.ToTable("sysUserOtherWorkPlace");

                entity.Property(e => e.UserId)
                    .HasColumnName("UserID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

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

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

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

            modelBuilder.Entity<TestContainerList>(entity =>
            {
                entity.ToTable("test_ContainerList");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Cbm)
                    .HasColumnName("CBM")
                    .HasColumnType("decimal(10, 2)");

                entity.Property(e => e.ChargeAbleWeight).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.ContainerNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContainerType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.GoodsDescription).HasMaxLength(4000);

                entity.Property(e => e.GrossWeight).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.Hblno)
                    .HasColumnName("HBLNo")
                    .HasMaxLength(1600);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.JobId)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.MarkNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.NetWeight).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.PackageTypeId)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.SealNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TestHouseBillSeaFclexport>(entity =>
            {
                entity.HasKey(e => e.Hblno);

                entity.ToTable("test_HouseBillSeaFCLExport");

                entity.Property(e => e.Hblno)
                    .HasColumnName("HBLNo")
                    .HasMaxLength(100)
                    .ValueGeneratedNever();

                entity.Property(e => e.BookingNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ClosingDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ConsigneeId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DeliveryPlace).HasMaxLength(4000);

                entity.Property(e => e.ExportReferenceNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FinalDestination).HasMaxLength(4000);

                entity.Property(e => e.FowardingAgentId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FreightPayment)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.GoodsDeliveryId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Hbltype)
                    .HasColumnName("HBLType")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.InWord).HasMaxLength(4000);

                entity.Property(e => e.IssueHblplaceAndDate)
                    .HasColumnName("IssueHBLPlaceAndDate")
                    .HasMaxLength(4000);

                entity.Property(e => e.LocalVessel).HasMaxLength(1600);

                entity.Property(e => e.Mblno)
                    .HasColumnName("MBLNo")
                    .HasMaxLength(1600);

                entity.Property(e => e.MoveType).HasMaxLength(160);

                entity.Property(e => e.NotifyPartyId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OceanVessel).HasMaxLength(800);

                entity.Property(e => e.OnBoardStatus).HasMaxLength(4000);

                entity.Property(e => e.OriginBlnumber).HasColumnName("OriginBLNumber");

                entity.Property(e => e.PlaceFreightPay).HasMaxLength(4000);

                entity.Property(e => e.PolcountryId).HasColumnName("POLCountryId");

                entity.Property(e => e.PurchaseOrderNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReceiptPlace).HasMaxLength(4000);

                entity.Property(e => e.ReferenceNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SailingDate).HasColumnType("smalldatetime");

                entity.Property(e => e.SaleManId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceType).HasMaxLength(160);

                entity.Property(e => e.ShipperId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingMark).HasMaxLength(4000);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TestSeaFclexportShipment>(entity =>
            {
                entity.HasKey(e => e.JobId);

                entity.ToTable("test_SeaFCLExportShipment");

                entity.Property(e => e.JobId)
                    .HasColumnName("JobID")
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.AgentId)
                    .HasColumnName("AgentID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BookingNo).HasMaxLength(1600);

                entity.Property(e => e.ColoaderId)
                    .HasColumnName("ColoaderID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CommoditiesDescription).HasMaxLength(4000);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.EstimatedTimeofArrived).HasColumnType("datetime");

                entity.Property(e => e.EstimatedTimeofDepature).HasColumnType("datetime");

                entity.Property(e => e.GoodsDescription).HasMaxLength(4000);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.MasterBillOfLoadingType)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Mblno)
                    .HasColumnName("MBLNo")
                    .HasMaxLength(1600);

                entity.Property(e => e.Note).HasMaxLength(3200);

                entity.Property(e => e.PersonInChargeId)
                    .HasColumnName("PersonInChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PurchaseOrderNo).HasMaxLength(1600);

                entity.Property(e => e.ServiceType)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingType)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Term)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VesselName).HasMaxLength(1600);

                entity.Property(e => e.VoyNo).HasMaxLength(1600);
            });
        }
    }
}
