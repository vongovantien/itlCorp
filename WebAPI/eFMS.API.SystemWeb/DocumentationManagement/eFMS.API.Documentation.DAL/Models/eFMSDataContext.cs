using System;
using eFMS.API.Shipment.Service.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace eFMS.API.Documentation.Service.Models
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
        public override int SaveChanges()
        {
            var entities = ChangeTracker.Entries();
            var mongoDb = Shipment.Service.Helpers.MongoDbHelper.GetDatabase();
            var modifiedList = ChangeTrackerHelper.GetChangModifield(entities);
            var addedList = ChangeTrackerHelper.GetAdded(entities);
            var deletedList = ChangeTrackerHelper.GetDeleted(entities);
            var result = base.SaveChanges();
            if (result > 0)
            {
                if (addedList != null)
                {
                    ChangeTrackerHelper.InsertToMongoDb(addedList, EntityState.Added);
                }
                if (modifiedList != null)
                {
                    ChangeTrackerHelper.InsertToMongoDb(modifiedList, EntityState.Modified);
                }
                if (deletedList != null)
                {
                    ChangeTrackerHelper.InsertToMongoDb(deletedList, EntityState.Deleted);
                }
            }
            return result;
        }
        public virtual DbSet<CatCountry> CatCountry { get; set; }
        public virtual DbSet<CatPartner> CatPartner { get; set; }
        public virtual DbSet<CatPlace> CatPlace { get; set; }
        public virtual DbSet<CatUnit> CatUnit { get; set; }
        public virtual DbSet<SysUser> SysUser { get; set; }
        public virtual DbSet<TestContainerList> TestContainerList { get; set; }
        public virtual DbSet<TestHouseBillSeaFclexport> TestHouseBillSeaFclexport { get; set; }
        public virtual DbSet<TestSeaFclexportShipment> TestSeaFclexportShipment { get; set; }

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
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.GoodsDescription).HasMaxLength(1000);

                entity.Property(e => e.GrossWeight).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.Hblno)
                    .IsRequired()
                    .HasColumnName("HBLNo")
                    .HasMaxLength(100);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.JobId)
                    .IsRequired()
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
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DeliveryPlace)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.ExportReferenceNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FinalDestination)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.FowardingAgentId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FreightPayment)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.GoodsDeliveryId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Hbltype)
                    .IsRequired()
                    .HasColumnName("HBLType")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.InWord).HasMaxLength(500);

                entity.Property(e => e.IssueHblplaceAndDate)
                    .HasColumnName("IssueHBLPlaceAndDate")
                    .HasMaxLength(500);

                entity.Property(e => e.LocalVessel).HasMaxLength(100);

                entity.Property(e => e.Mblno)
                    .IsRequired()
                    .HasColumnName("MBLNo")
                    .HasMaxLength(100);

                entity.Property(e => e.MoveType).HasMaxLength(10);

                entity.Property(e => e.NotifyPartyId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OceanVessel)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.OnBoardStatus).HasMaxLength(500);

                entity.Property(e => e.OriginBlnumber).HasColumnName("OriginBLNumber");

                entity.Property(e => e.PlaceFreightPay)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.PolcountryId).HasColumnName("POLCountryId");

                entity.Property(e => e.PurchaseOrderNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReceiptPlace)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.ReferenceNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SailingDate).HasColumnType("smalldatetime");

                entity.Property(e => e.SaleManId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceType).HasMaxLength(10);

                entity.Property(e => e.ShipperId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingMark).HasMaxLength(500);

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

                entity.Property(e => e.BookingNo).HasMaxLength(100);

                entity.Property(e => e.ColoaderId)
                    .HasColumnName("ColoaderID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.EstimatedTimeofArrived).HasColumnType("datetime");

                entity.Property(e => e.EstimatedTimeofDepature).HasColumnType("datetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.MasterBillOfLoadingType)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Mblno)
                    .IsRequired()
                    .HasColumnName("MBLNo")
                    .HasMaxLength(100);

                entity.Property(e => e.Note).HasMaxLength(200);

                entity.Property(e => e.PersonInChargeId)
                    .IsRequired()
                    .HasColumnName("PersonInChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PurchaseOrderNo).HasMaxLength(100);

                entity.Property(e => e.ServiceType)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingType)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Term)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VesselName).HasMaxLength(100);

                entity.Property(e => e.VoyNo).HasMaxLength(100);
            });
        }
    }
}
