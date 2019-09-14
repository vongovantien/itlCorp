using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace eFMS.API.Accounting.Service.Models
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

        public virtual DbSet<AcctAdvancePayment> AcctAdvancePayment { get; set; }
        public virtual DbSet<AcctAdvanceRequest> AcctAdvanceRequest { get; set; }
        public virtual DbSet<AcctApproveAdvance> AcctApproveAdvance { get; set; }
        public virtual DbSet<AcctApproveSettlement> AcctApproveSettlement { get; set; }
        public virtual DbSet<AcctCdnote> AcctCdnote { get; set; }
        public virtual DbSet<AcctSettlementPayment> AcctSettlementPayment { get; set; }
        public virtual DbSet<AcctSoa> AcctSoa { get; set; }
        public virtual DbSet<SysAuthorization> SysAuthorization { get; set; }
        public virtual DbSet<SysAuthorizationDetail> SysAuthorizationDetail { get; set; }
        public virtual DbSet<SysUser> SysUser { get; set; }
        public virtual DbSet<OpsTransaction> OpsTransaction { get; set; }
        public virtual DbSet<CsTransaction> CsTransaction { get; set; }
        public virtual DbSet<CsTransactionDetail> CsTransactionDetail { get; set; }
        public virtual DbSet<SysEmployee> SysEmployee { get; set; }
        public virtual DbSet<SysUserGroup> SysUserGroup { get; set; }
        public virtual DbSet<SysGroup> SysGroup { get; set; }
        public virtual DbSet<CatDepartment> CatDepartment { get; set; }
        public virtual DbSet<SysBranch> SysBranch { get; set; }
        public virtual DbSet<OpsStageAssigned> OpsStageAssigned { get; set; }
        public virtual DbSet<CsShipmentSurcharge> CsShipmentSurcharge { get; set; }
        public virtual DbSet<CustomsDeclaration> CustomsDeclaration { get; set; }
        public virtual DbSet<CatPartner> CatPartner { get; set; }
        public virtual DbSet<CatPartnerContact> CatPartnerContact { get; set; }
        public virtual DbSet<CsMawbcontainer> CsMawbcontainer { get; set; }
        public virtual DbSet<CatCharge> CatCharge { get; set; }
        public virtual DbSet<CatUnit> CatUnit { get; set; }
        public virtual DbSet<CatCurrency> CatCurrency { get; set; }
        public virtual DbSet<CatCountry> CatCountry { get; set; }
        public virtual DbSet<CatPlace> CatPlace { get; set; }
        public virtual DbSet<CatCurrencyExchange> CatCurrencyExchange { get; set; }










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

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DeadlinePayment).HasColumnType("date");

                entity.Property(e => e.Department)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentMethod)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RequestDate).HasColumnType("date");

                entity.Property(e => e.Requester)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StatusApproval)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AcctAdvanceRequest>(entity =>
            {
                entity.ToTable("acctAdvanceRequest");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AdvanceNo)
                    .HasMaxLength(11)
                    .IsUnicode(false);

                entity.Property(e => e.AdvanceType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CustomNo).HasMaxLength(100);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(50);

                entity.Property(e => e.Hbl)
                    .HasColumnName("HBL")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.JobId)
                    .HasColumnName("JobID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Mbl)
                    .HasColumnName("MBL")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.RequestCurrency).HasMaxLength(10);

                entity.Property(e => e.StatusPayment)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
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

                entity.Property(e => e.LeaderAprDate).HasColumnType("datetime");

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

                entity.Property(e => e.LeaderAprDate).HasColumnType("datetime");

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

            modelBuilder.Entity<AcctSettlementPayment>(entity =>
            {
                entity.ToTable("acctSettlementPayment");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

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

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AcctSoa>(entity =>
            {
                entity.ToTable("acctSOA");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreatorShipment)
                    .HasMaxLength(250)
                    .IsUnicode(false);

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

                entity.Property(e => e.Obh).HasColumnName("OBH");

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

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

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

          

            modelBuilder.Entity<CatDepartment>(entity =>
            {
                entity.ToTable("catDepartment");

                entity.HasIndex(e => e.Code)
                    .HasName("U_catDepartment_Code")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

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

                entity.Property(e => e.Description).HasMaxLength(4000);

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.ManagerId)
                    .HasColumnName("ManagerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

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

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

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

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

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

                entity.Property(e => e.Cdclosed)
                    .HasColumnName("CDClosed")
                    .HasDefaultValueSql("((0))");

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

                entity.Property(e => e.ClearanceNo).HasMaxLength(100);

                entity.Property(e => e.ContNo).HasMaxLength(200);

                entity.Property(e => e.CreditNo)
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

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DebitNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ExchangeDate).HasColumnType("datetime");

                entity.Property(e => e.Hblid).HasColumnName("HBLID");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.InvoiceDate).HasColumnType("datetime");

                entity.Property(e => e.InvoiceNo).HasMaxLength(50);

                entity.Property(e => e.IsFromShipment).HasDefaultValueSql("((1))");

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

                entity.Property(e => e.SeriesNo).HasMaxLength(50);

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

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

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

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.InvoiceNo).HasMaxLength(1600);

                entity.Property(e => e.IsLocked).HasDefaultValueSql("((0))");

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

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

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

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

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
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.InWord).HasMaxLength(4000);

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

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
                    .HasMaxLength(200)
                    .IsUnicode(false);

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
                    .HasMaxLength(100);

                entity.Property(e => e.CommodityCode)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.ConvertTime).HasColumnType("datetime");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DocumentType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ExportCountryCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FirstClearanceNo).HasMaxLength(100);

                entity.Property(e => e.Gateway)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.GrossWeight).HasColumnType("decimal(18, 4)");

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

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Deadline).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.JobId).HasColumnName("JobID");

                entity.Property(e => e.MainPersonInCharge)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.Property(e => e.ProcessTime).HasColumnType("decimal(18, 3)");

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

                entity.Property(e => e.ContainerDescription).HasMaxLength(200);

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CurrentStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

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

                entity.Property(e => e.IsLocked).HasDefaultValueSql("((0))");

                entity.Property(e => e.JobNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Mblno)
                    .HasColumnName("MBLNO")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

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

            modelBuilder.Entity<SysBranch>(entity =>
            {
                entity.ToTable("sysBranch");

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

                entity.Property(e => e.Buid).HasColumnName("BUID");

                entity.Property(e => e.Code).HasMaxLength(640);

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Fax)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Logo).HasColumnType("image");

                entity.Property(e => e.ManagerId)
                    .HasColumnName("ManagerID")
                    .HasMaxLength(50)
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

                entity.Property(e => e.Website)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Bu)
                    .WithMany(p => p.SysBranch)
                    .HasForeignKey(d => d.Buid)
                    .HasConstraintName("FK_sysBranch_sysBU");
            });

            modelBuilder.Entity<SysBu>(entity =>
            {
                entity.ToTable("sysBU");

                entity.Property(e => e.Id).HasColumnName("ID");

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

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DescriptionEn)
                    .HasColumnName("Description_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.DescriptionVn)
                    .HasColumnName("Description_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.Email).HasMaxLength(4000);

                entity.Property(e => e.Fax).HasMaxLength(1600);

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Logo).HasColumnType("image");

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

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

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

            modelBuilder.Entity<SysGroup>(entity =>
            {
                entity.ToTable("sysGroup");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Decription).HasMaxLength(4000);

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.ManagerId)
                    .HasColumnName("ManagerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(3200);

                entity.Property(e => e.ParentId).HasColumnName("ParentID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.SysGroup)
                    .HasForeignKey(d => d.DepartmentId)
                    .HasConstraintName("FK_sysGroup_catDepartment");
            });

            modelBuilder.Entity<SysGroupRole>(entity =>
            {
                entity.ToTable("sysGroupRole");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.SysGroupRole)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysGroupRole_sysGroup");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.SysGroupRole)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysGroupRole_sysRole");
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

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("NameEN")
                    .HasMaxLength(4000);

                entity.Property(e => e.NameVn)
                    .HasColumnName("NameVN")
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

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

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

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DescriptionEn).HasMaxLength(4000);

                entity.Property(e => e.DescriptionVn).HasMaxLength(4000);

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

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

            modelBuilder.Entity<CatCurrency>(entity =>
            {
                entity.ToTable("catCurrency");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.CurrencyName).HasMaxLength(800);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

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

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EffectiveOn).HasColumnType("datetime");

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Rate).HasColumnType("decimal(18, 4)");

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

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

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

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DisplayName).HasMaxLength(4000);

                entity.Property(e => e.DistrictId).HasColumnName("DistrictID");

                entity.Property(e => e.GeoCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

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

            modelBuilder.Entity<SysPermission>(entity =>
            {
                entity.ToTable("sysPermission");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(4000);

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RequireAccessingForm).HasDefaultValueSql("((1))");

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

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(4000);

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

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

                entity.Property(e => e.AllowAccess).HasDefaultValueSql("((1))");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

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

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.OtherIntructionId).HasColumnName("OtherIntructionID");

                entity.Property(e => e.PermissionId).HasColumnName("PermissionID");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

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

                entity.Property(e => e.EmployeeId)
                    .HasColumnName("EmployeeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Password).HasMaxLength(4000);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<SysUserGroup>(entity =>
            {
                entity.ToTable("sysUserGroup");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

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

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.SysUserGroup)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysUserGroup_sysGroup");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SysUserGroup)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysUserGroup_sysUser");
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

                entity.HasOne(d => d.WorkPlace)
                    .WithMany(p => p.SysUserOtherWorkPlace)
                    .HasForeignKey(d => d.WorkPlaceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysUserOtherBranch_catBranch");
            });

            
        }
    }
}
