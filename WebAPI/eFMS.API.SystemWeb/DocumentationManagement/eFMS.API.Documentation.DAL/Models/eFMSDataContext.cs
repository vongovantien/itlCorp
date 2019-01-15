using System;
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

                entity.Property(e => e.Hblid)
                    .IsRequired()
                    .HasColumnName("HBLId")
                    .HasMaxLength(50);

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
