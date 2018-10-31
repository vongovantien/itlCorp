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
        public virtual DbSet<CatCommodity> CatCommodity { get; set; }
        public virtual DbSet<CatCommodityGroup> CatCommodityGroup { get; set; }
        public virtual DbSet<CatCountry> CatCountry { get; set; }
        public virtual DbSet<CatDepartment> CatDepartment { get; set; }
        public virtual DbSet<CatPartner> CatPartner { get; set; }
        public virtual DbSet<CatPartnerGroup> CatPartnerGroup { get; set; }
        public virtual DbSet<CatPlace> CatPlace { get; set; }
        public virtual DbSet<CatPlaceType> CatPlaceType { get; set; }
        public virtual DbSet<CatStage> CatStage { get; set; }

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

                entity.Property(e => e.AddressVn)
                    .HasColumnName("Address_VN")
                    .HasMaxLength(510);

                entity.Property(e => e.BankAccountAddress).HasMaxLength(510);

                entity.Property(e => e.BankAccountName).HasMaxLength(510);

                entity.Property(e => e.BankAccountNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.CreditAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DebitAmount).HasColumnType("decimal(18, 4)");

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
                    .HasMaxLength(50)
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
        }
    }
}
