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

        public virtual DbSet<CatCommodity> CatCommodity { get; set; }
        public virtual DbSet<CatPartner> CatPartner { get; set; }
        public virtual DbSet<CsShipmentSurcharge> CsShipmentSurcharge { get; set; }
        public virtual DbSet<CsTransaction> CsTransaction { get; set; }
        public virtual DbSet<CsTransactionDetail> CsTransactionDetail { get; set; }
        public virtual DbSet<CustomsDeclaration> CustomsDeclaration { get; set; }
        public virtual DbSet<OpsStageAssigned> OpsStageAssigned { get; set; }
        public virtual DbSet<OpsTransaction> OpsTransaction { get; set; }
        public virtual DbSet<SetEcusconnection> SetEcusconnection { get; set; }
        public virtual DbSet<SysEmployee> SysEmployee { get; set; }
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

                entity.Property(e => e.IsHawb).HasColumnName("IsHAWB");

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

                entity.Property(e => e.LastDateUnLocked).HasColumnType("datetime");

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

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Dbname)
                    .IsRequired()
                    .HasColumnName("DBName")
                    .HasMaxLength(250);

                entity.Property(e => e.Dbpassword)
                    .HasColumnName("DBPassword")
                    .HasMaxLength(100);

                entity.Property(e => e.Dbusername)
                    .IsRequired()
                    .HasColumnName("DBUsername")
                    .HasMaxLength(100);

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.InactiveOn).HasMaxLength(10);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Note).HasMaxLength(100);

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.ServerName)
                    .IsRequired()
                    .HasMaxLength(250);

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

            modelBuilder.Entity<SysEmployee>(entity =>
            {
                entity.ToTable("sysEmployee");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.AccessDescription).HasMaxLength(1600);

                entity.Property(e => e.BankAccountNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BankName)
                    .HasMaxLength(50)
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

                entity.Property(e => e.Position)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SaleResource)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SaleTarget).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Signature).HasColumnType("image");

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
        }
    }
}
