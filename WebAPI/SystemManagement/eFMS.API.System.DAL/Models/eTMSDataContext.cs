using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SystemManagementAPI.Service.Models
{
    public partial class eTMSDataContext : DbContext
    {
        public eTMSDataContext()
        {
        }

        public eTMSDataContext(DbContextOptions<eTMSDataContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AcctFclsoa> AcctFclsoa { get; set; }
        public virtual DbSet<AcctFuelPayment> AcctFuelPayment { get; set; }
        public virtual DbSet<AcctFuelTransaction> AcctFuelTransaction { get; set; }
        public virtual DbSet<AcctPaymentRequest> AcctPaymentRequest { get; set; }
        public virtual DbSet<AcctSoa> AcctSoa { get; set; }
        public virtual DbSet<AcctSoahistory> AcctSoahistory { get; set; }
        public virtual DbSet<AuditAction> AuditAction { get; set; }
        public virtual DbSet<AuditUpdate> AuditUpdate { get; set; }
        public virtual DbSet<AwbcodeFollowTypeBooking> AwbcodeFollowTypeBooking { get; set; }
        public virtual DbSet<BookingCodeFollowTypeBooking> BookingCodeFollowTypeBooking { get; set; }
        public virtual DbSet<CatArea> CatArea { get; set; }
        public virtual DbSet<CatBranch> CatBranch { get; set; }
        public virtual DbSet<CatCharge> CatCharge { get; set; }
        public virtual DbSet<CatChargeType> CatChargeType { get; set; }
        public virtual DbSet<CatCommodity> CatCommodity { get; set; }
        public virtual DbSet<CatCommodityGroup> CatCommodityGroup { get; set; }
        public virtual DbSet<CatContainerType> CatContainerType { get; set; }
        public virtual DbSet<CatCountry> CatCountry { get; set; }
        public virtual DbSet<CatCurrency> CatCurrency { get; set; }
        public virtual DbSet<CatCurrencyExchange> CatCurrencyExchange { get; set; }
        public virtual DbSet<CatCustomerGoodsDescription> CatCustomerGoodsDescription { get; set; }
        public virtual DbSet<CatCustomerPlace> CatCustomerPlace { get; set; }
        public virtual DbSet<CatCustomerShipmentNote> CatCustomerShipmentNote { get; set; }
        public virtual DbSet<CatDeliveryZoneCode> CatDeliveryZoneCode { get; set; }
        public virtual DbSet<CatDeliveryZoneCode1> CatDeliveryZoneCode1 { get; set; }
        public virtual DbSet<CatDepartment> CatDepartment { get; set; }
        public virtual DbSet<CatDistrict> CatDistrict { get; set; }
        public virtual DbSet<CatDriver> CatDriver { get; set; }
        public virtual DbSet<CatDriverAppHistory> CatDriverAppHistory { get; set; }
        public virtual DbSet<CatDriverManagementArea> CatDriverManagementArea { get; set; }
        public virtual DbSet<CatFclserviceType> CatFclserviceType { get; set; }
        public virtual DbSet<CatGeoCode> CatGeoCode { get; set; }
        public virtual DbSet<CatHub> CatHub { get; set; }
        public virtual DbSet<CatMobile> CatMobile { get; set; }
        public virtual DbSet<CatOrderStatusReason> CatOrderStatusReason { get; set; }
        public virtual DbSet<CatOtherPlace> CatOtherPlace { get; set; }
        public virtual DbSet<CatPartner> CatPartner { get; set; }
        public virtual DbSet<CatPartnerContact> CatPartnerContact { get; set; }
        public virtual DbSet<CatPartnerContract> CatPartnerContract { get; set; }
        public virtual DbSet<CatPartnerGroup> CatPartnerGroup { get; set; }
        public virtual DbSet<CatPickupTime> CatPickupTime { get; set; }
        public virtual DbSet<CatPickupZoneCode> CatPickupZoneCode { get; set; }
        public virtual DbSet<CatPlace> CatPlace { get; set; }
        public virtual DbSet<CatPlaceDistance> CatPlaceDistance { get; set; }
        public virtual DbSet<CatPlaceType> CatPlaceType { get; set; }
        public virtual DbSet<CatPosition> CatPosition { get; set; }
        public virtual DbSet<CatProvince> CatProvince { get; set; }
        public virtual DbSet<CatReceptacleType> CatReceptacleType { get; set; }
        public virtual DbSet<CatRoad> CatRoad { get; set; }
        public virtual DbSet<CatRoute> CatRoute { get; set; }
        public virtual DbSet<CatRouteShortTrip> CatRouteShortTrip { get; set; }
        public virtual DbSet<CatRouteSurcharge> CatRouteSurcharge { get; set; }
        public virtual DbSet<CatSaleResource> CatSaleResource { get; set; }
        public virtual DbSet<CatServiceType> CatServiceType { get; set; }
        public virtual DbSet<CatServiceTypeI> CatServiceTypeI { get; set; }
        public virtual DbSet<CatServiceTypeMapping> CatServiceTypeMapping { get; set; }
        public virtual DbSet<CatShipmentNote> CatShipmentNote { get; set; }
        public virtual DbSet<CatShipmentType> CatShipmentType { get; set; }
        public virtual DbSet<CatStandardFuelConsumption> CatStandardFuelConsumption { get; set; }
        public virtual DbSet<CatTestMobile> CatTestMobile { get; set; }
        public virtual DbSet<CatTransitRouteMiddlePlace> CatTransitRouteMiddlePlace { get; set; }
        public virtual DbSet<CatUnit> CatUnit { get; set; }
        public virtual DbSet<CatUnitExchange> CatUnitExchange { get; set; }
        public virtual DbSet<CatVehicle> CatVehicle { get; set; }
        public virtual DbSet<CatVehicleDriver> CatVehicleDriver { get; set; }
        public virtual DbSet<CatVehicleGroup> CatVehicleGroup { get; set; }
        public virtual DbSet<CatVehicleLocation> CatVehicleLocation { get; set; }
        public virtual DbSet<CatVehiclePart> CatVehiclePart { get; set; }
        public virtual DbSet<CatVehiclePartDetail> CatVehiclePartDetail { get; set; }
        public virtual DbSet<CatVehiclePartDetailHistory> CatVehiclePartDetailHistory { get; set; }
        public virtual DbSet<CatVehiclePartPrice> CatVehiclePartPrice { get; set; }
        public virtual DbSet<CatVehiclePartType> CatVehiclePartType { get; set; }
        public virtual DbSet<CatVehicleType> CatVehicleType { get; set; }
        public virtual DbSet<CatVehicleWorkPlace> CatVehicleWorkPlace { get; set; }
        public virtual DbSet<CatVolume> CatVolume { get; set; }
        public virtual DbSet<CatWard> CatWard { get; set; }
        public virtual DbSet<CatWeightRange> CatWeightRange { get; set; }
        public virtual DbSet<CatZoneCode> CatZoneCode { get; set; }
        public virtual DbSet<CsChangedSurchargeLog> CsChangedSurchargeLog { get; set; }
        public virtual DbSet<CsDocument> CsDocument { get; set; }
        public virtual DbSet<CsDtborder> CsDtborder { get; set; }
        public virtual DbSet<CsDtborderChargeBehalf> CsDtborderChargeBehalf { get; set; }
        public virtual DbSet<CsDtborderDropPoint> CsDtborderDropPoint { get; set; }
        public virtual DbSet<CsDtborderDropPointItem> CsDtborderDropPointItem { get; set; }
        public virtual DbSet<CsDtborderDropPointItemRoute> CsDtborderDropPointItemRoute { get; set; }
        public virtual DbSet<CsDtborderExpense> CsDtborderExpense { get; set; }
        public virtual DbSet<CsDtborderSurcharge> CsDtborderSurcharge { get; set; }
        public virtual DbSet<CsDtbtransportSurcharge> CsDtbtransportSurcharge { get; set; }
        public virtual DbSet<CsDtbtransportTripRecord> CsDtbtransportTripRecord { get; set; }
        public virtual DbSet<CsFclbooking> CsFclbooking { get; set; }
        public virtual DbSet<CsFclbookingDetail> CsFclbookingDetail { get; set; }
        public virtual DbSet<CsFcltransportChargeBehalf> CsFcltransportChargeBehalf { get; set; }
        public virtual DbSet<CsFcltransportSurcharge> CsFcltransportSurcharge { get; set; }
        public virtual DbSet<CsFcltransportTripRecord> CsFcltransportTripRecord { get; set; }
        public virtual DbSet<CsOrderDetail> CsOrderDetail { get; set; }
        public virtual DbSet<CsOrderDetailChargeBehalf> CsOrderDetailChargeBehalf { get; set; }
        public virtual DbSet<CsOrderDetailExpense> CsOrderDetailExpense { get; set; }
        public virtual DbSet<CsOrderDetailShipmentNote> CsOrderDetailShipmentNote { get; set; }
        public virtual DbSet<CsOrderDetailShortTrip> CsOrderDetailShortTrip { get; set; }
        public virtual DbSet<CsOrderDetailSurcharge> CsOrderDetailSurcharge { get; set; }
        public virtual DbSet<CsOrderDetailVoucher> CsOrderDetailVoucher { get; set; }
        public virtual DbSet<CsOrderHeader> CsOrderHeader { get; set; }
        public virtual DbSet<CsOrderItemDetail> CsOrderItemDetail { get; set; }
        public virtual DbSet<CsOrderSurcharge> CsOrderSurcharge { get; set; }
        public virtual DbSet<CsReceptacleChecking> CsReceptacleChecking { get; set; }
        public virtual DbSet<CsReceptacleMaster> CsReceptacleMaster { get; set; }
        public virtual DbSet<CsReceptacleOrderDetail> CsReceptacleOrderDetail { get; set; }
        public virtual DbSet<CsShipmentChecking> CsShipmentChecking { get; set; }
        public virtual DbSet<CsTransportSurcharge> CsTransportSurcharge { get; set; }
        public virtual DbSet<CsTransportTripRecord> CsTransportTripRecord { get; set; }
        public virtual DbSet<MainMaintenancePlan> MainMaintenancePlan { get; set; }
        public virtual DbSet<MainMaintenanceQuota> MainMaintenanceQuota { get; set; }
        public virtual DbSet<MainMaintenanceQuotaDetail> MainMaintenanceQuotaDetail { get; set; }
        public virtual DbSet<MainMrrequest> MainMrrequest { get; set; }
        public virtual DbSet<MainMrrequestDetail> MainMrrequestDetail { get; set; }
        public virtual DbSet<MainMrrequestPartDetail> MainMrrequestPartDetail { get; set; }
        public virtual DbSet<MainMrrequestTripRecord> MainMrrequestTripRecord { get; set; }
        public virtual DbSet<MainReplacedVehiclePartStatus> MainReplacedVehiclePartStatus { get; set; }
        public virtual DbSet<MainVehicleMaintenance> MainVehicleMaintenance { get; set; }
        public virtual DbSet<MainVehicleMaintenanceMasterPlan> MainVehicleMaintenanceMasterPlan { get; set; }
        public virtual DbSet<MainVehicleMaintenancePlace> MainVehicleMaintenancePlace { get; set; }
        public virtual DbSet<MainVehicleMaintenanceType> MainVehicleMaintenanceType { get; set; }
        public virtual DbSet<MainVehicleRepairLevel> MainVehicleRepairLevel { get; set; }
        public virtual DbSet<MainYearlyCostEstimation> MainYearlyCostEstimation { get; set; }
        public virtual DbSet<OpsDtbhireTransportRequestApproval> OpsDtbhireTransportRequestApproval { get; set; }
        public virtual DbSet<OpsDtbtransportRequest> OpsDtbtransportRequest { get; set; }
        public virtual DbSet<OpsDtbtransportRequestOrderItemRoute> OpsDtbtransportRequestOrderItemRoute { get; set; }
        public virtual DbSet<OpsFclhireTransportRequestApproval> OpsFclhireTransportRequestApproval { get; set; }
        public virtual DbSet<OpsFclmasterTransportRequest> OpsFclmasterTransportRequest { get; set; }
        public virtual DbSet<OpsFcltransportRequest> OpsFcltransportRequest { get; set; }
        public virtual DbSet<OpsHireTransportRequestApproval> OpsHireTransportRequestApproval { get; set; }
        public virtual DbSet<OpsOrderDetailTransportRequest> OpsOrderDetailTransportRequest { get; set; }
        public virtual DbSet<OpsTransportRequest> OpsTransportRequest { get; set; }
        public virtual DbSet<OpsTransportRequestImage> OpsTransportRequestImage { get; set; }
        public virtual DbSet<OpsTransportRequestReceptacle> OpsTransportRequestReceptacle { get; set; }
        public virtual DbSet<OpsTransportRequestType> OpsTransportRequestType { get; set; }
        public virtual DbSet<OpsUnlockTransportRequest> OpsUnlockTransportRequest { get; set; }
        public virtual DbSet<OpsWawePickPlan> OpsWawePickPlan { get; set; }
        public virtual DbSet<OpsWawePickPlanItem> OpsWawePickPlanItem { get; set; }
        public virtual DbSet<PriceBuying> PriceBuying { get; set; }
        public virtual DbSet<PriceBuyingCustomer> PriceBuyingCustomer { get; set; }
        public virtual DbSet<PriceBuyingDetail> PriceBuyingDetail { get; set; }
        public virtual DbSet<PriceBuyingOverWeightDetail> PriceBuyingOverWeightDetail { get; set; }
        public virtual DbSet<PriceBuyingRoute> PriceBuyingRoute { get; set; }
        public virtual DbSet<PriceBuyingRouteSurcharge> PriceBuyingRouteSurcharge { get; set; }
        public virtual DbSet<PriceCost> PriceCost { get; set; }
        public virtual DbSet<PriceCostDeliveryRoute> PriceCostDeliveryRoute { get; set; }
        public virtual DbSet<PriceCostDirectRoute> PriceCostDirectRoute { get; set; }
        public virtual DbSet<PriceCostPickupRoute> PriceCostPickupRoute { get; set; }
        public virtual DbSet<PriceCostTransitRoute> PriceCostTransitRoute { get; set; }
        public virtual DbSet<PriceCostZoneMapping> PriceCostZoneMapping { get; set; }
        public virtual DbSet<PriceCustomerRateCard> PriceCustomerRateCard { get; set; }
        public virtual DbSet<PriceDtbrateCard> PriceDtbrateCard { get; set; }
        public virtual DbSet<PriceDtbrateCardBookingSchedule> PriceDtbrateCardBookingSchedule { get; set; }
        public virtual DbSet<PriceDtbrateCardCondition> PriceDtbrateCardCondition { get; set; }
        public virtual DbSet<PriceDtbrateCardDetail> PriceDtbrateCardDetail { get; set; }
        public virtual DbSet<PriceDtbrateCardDropPoint> PriceDtbrateCardDropPoint { get; set; }
        public virtual DbSet<PriceDtbrateCardDropPointRoute> PriceDtbrateCardDropPointRoute { get; set; }
        public virtual DbSet<PriceDtbstandardCostDetail> PriceDtbstandardCostDetail { get; set; }
        public virtual DbSet<PriceFclbuying> PriceFclbuying { get; set; }
        public virtual DbSet<PriceFclbuyingSurcharge> PriceFclbuyingSurcharge { get; set; }
        public virtual DbSet<PriceRateCard> PriceRateCard { get; set; }
        public virtual DbSet<PriceRateCardCondition> PriceRateCardCondition { get; set; }
        public virtual DbSet<PriceRateCardDetail> PriceRateCardDetail { get; set; }
        public virtual DbSet<PriceRateCardOverWeightDetail> PriceRateCardOverWeightDetail { get; set; }
        public virtual DbSet<PriceRouteCost> PriceRouteCost { get; set; }
        public virtual DbSet<PriceRouteCostShortTrip> PriceRouteCostShortTrip { get; set; }
        public virtual DbSet<PriceRouteCostSurcharge> PriceRouteCostSurcharge { get; set; }
        public virtual DbSet<PriceServiceTypeWeightRange> PriceServiceTypeWeightRange { get; set; }
        public virtual DbSet<PriceServiceTypeWeightRangeDetail> PriceServiceTypeWeightRangeDetail { get; set; }
        public virtual DbSet<PriceTripBuyingDetail> PriceTripBuyingDetail { get; set; }
        public virtual DbSet<PriceTripBuyingVehicleType> PriceTripBuyingVehicleType { get; set; }
        public virtual DbSet<SaleDtbquotation> SaleDtbquotation { get; set; }
        public virtual DbSet<SaleFclquotation> SaleFclquotation { get; set; }
        public virtual DbSet<SaleFclquotationRoute> SaleFclquotationRoute { get; set; }
        public virtual DbSet<SaleFclquotationShortTrip> SaleFclquotationShortTrip { get; set; }
        public virtual DbSet<SaleFclquotationShortTripDetail> SaleFclquotationShortTripDetail { get; set; }
        public virtual DbSet<SaleFclquotationShortTripSurcharge> SaleFclquotationShortTripSurcharge { get; set; }
        public virtual DbSet<SaleQuotation> SaleQuotation { get; set; }
        public virtual DbSet<SaleQuotationRoute> SaleQuotationRoute { get; set; }
        public virtual DbSet<SaleQuotationRouteSurcharge> SaleQuotationRouteSurcharge { get; set; }
        public virtual DbSet<SaleSalesTarget> SaleSalesTarget { get; set; }
        public virtual DbSet<SysAuthorization> SysAuthorization { get; set; }
        public virtual DbSet<SysAuthorizationDetail> SysAuthorizationDetail { get; set; }
        public virtual DbSet<SysBaseEnum> SysBaseEnum { get; set; }
        public virtual DbSet<SysBaseEnumDetail> SysBaseEnumDetail { get; set; }
        public virtual DbSet<SysBu> SysBu { get; set; }
        public virtual DbSet<SysChangeBookingOverDateLog> SysChangeBookingOverDateLog { get; set; }
        public virtual DbSet<SysDriverAllowanceParameter> SysDriverAllowanceParameter { get; set; }
        public virtual DbSet<SysEmployee> SysEmployee { get; set; }
        public virtual DbSet<SysGpsprovider> SysGpsprovider { get; set; }
        public virtual DbSet<SysLogo> SysLogo { get; set; }
        public virtual DbSet<SysMenu> SysMenu { get; set; }
        public virtual DbSet<SysMenuPermissionInstruction> SysMenuPermissionInstruction { get; set; }
        public virtual DbSet<SysOneTmsbuildVersion> SysOneTmsbuildVersion { get; set; }
        public virtual DbSet<SysParameter> SysParameter { get; set; }
        public virtual DbSet<SysParameterDetail> SysParameterDetail { get; set; }
        public virtual DbSet<SysParameterVehicleType> SysParameterVehicleType { get; set; }
        public virtual DbSet<SysPermission> SysPermission { get; set; }
        public virtual DbSet<SysRole> SysRole { get; set; }
        public virtual DbSet<SysRoleMenu> SysRoleMenu { get; set; }
        public virtual DbSet<SysRolePermission> SysRolePermission { get; set; }
        public virtual DbSet<SysSentEmailHistory> SysSentEmailHistory { get; set; }
        public virtual DbSet<SysSmslog> SysSmslog { get; set; }
        public virtual DbSet<SysStatus> SysStatus { get; set; }
        public virtual DbSet<SysSynchronizationError> SysSynchronizationError { get; set; }
        public virtual DbSet<SysTemplate> SysTemplate { get; set; }
        public virtual DbSet<SysTemplateDetail> SysTemplateDetail { get; set; }
        public virtual DbSet<SysUser> SysUser { get; set; }
        public virtual DbSet<SysUserGroup> SysUserGroup { get; set; }
        public virtual DbSet<SysUserGroupRole> SysUserGroupRole { get; set; }
        public virtual DbSet<SysUserLog> SysUserLog { get; set; }
        public virtual DbSet<SysUserOtherWorkPlace> SysUserOtherWorkPlace { get; set; }
        public virtual DbSet<SysUserRole> SysUserRole { get; set; }
        public virtual DbSet<SysWebCode> SysWebCode { get; set; }

        // Unable to generate entity type for table 'dbo.catPartnerUnitExchange'. Please see the warning messages.
        // Unable to generate entity type for table 'fcl.acctSOAAction'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.ttt'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.aaa'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.InsertRoute_Temp'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.InsertRoute'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.acctSOAAction'. Please see the warning messages.
        // Unable to generate entity type for table 'fcl.csFCLBookingPlan'. Please see the warning messages.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=192.168.7.88;Database=eTMSTest;User ID=sa;Password=P@ssw0rd;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AcctFclsoa>(entity =>
            {
                entity.ToTable("acctFCLSOA", "fcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.ExportedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.PaidDate).HasColumnType("smalldatetime");

                entity.Property(e => e.SentByUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SentOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Total).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Branch)
                    .WithMany(p => p.AcctFclsoa)
                    .HasForeignKey(d => d.BranchId)
                    .HasConstraintName("FK_acctFCLSOA_Branch");
            });

            modelBuilder.Entity<AcctFuelPayment>(entity =>
            {
                entity.ToTable("acctFuelPayment");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountantDate).HasColumnType("smalldatetime");

                entity.Property(e => e.AccountantId)
                    .HasColumnName("AccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantNote).HasMaxLength(1000);

                entity.Property(e => e.AccountantStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ChiefId)
                    .HasColumnName("ChiefID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefNote).HasMaxLength(1000);

                entity.Property(e => e.ChiefStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DifferentFuelCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DifferentLiter).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DiscountFuel).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DriverId).HasColumnName("DriverID");

                entity.Property(e => e.FromDate).HasColumnType("smalldatetime");

                entity.Property(e => e.FuelAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelConsumption).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelCostConsumption).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OpsmanDate)
                    .HasColumnName("OPSManDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.OpsmanId)
                    .HasColumnName("OPSManID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanNote)
                    .HasColumnName("OPSManNote")
                    .HasMaxLength(1000);

                entity.Property(e => e.OpsmanStatus)
                    .HasColumnName("OPSManStatus")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentRefNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PriceUnit).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ToDate).HasColumnType("smalldatetime");

                entity.Property(e => e.TransactionAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransactionDate).HasColumnType("smalldatetime");

                entity.Property(e => e.TransactionId).HasColumnName("TransactionID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<AcctFuelTransaction>(entity =>
            {
                entity.ToTable("acctFuelTransaction");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountNote).HasMaxLength(1000);

                entity.Property(e => e.ActualFuelAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DriverId).HasColumnName("DriverID");

                entity.Property(e => e.FuelAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.InvoiceDate).HasColumnType("smalldatetime");

                entity.Property(e => e.InvoiceNo).HasMaxLength(100);

                entity.Property(e => e.LiterConsumption).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.PaidBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaidOn).HasColumnType("smalldatetime");

                entity.Property(e => e.PaymentDate).HasColumnType("smalldatetime");

                entity.Property(e => e.PaymentMethod)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PetrolStationId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RefNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RouteId).HasColumnName("RouteID");

                entity.Property(e => e.TotalShipmentWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalWeightFuelCs)
                    .HasColumnName("TotalWeightFuelCS")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransactionAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransactionDate).HasColumnType("smalldatetime");

                entity.Property(e => e.TransportId).HasColumnName("TransportID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleId).HasColumnName("VehicleID");

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<AcctPaymentRequest>(entity =>
            {
                entity.ToTable("acctPaymentRequest");

                entity.HasIndex(e => e.RefNo)
                    .HasName("U_RefNo")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantDate).HasColumnType("smalldatetime");

                entity.Property(e => e.AccountantId)
                    .HasColumnName("AccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BankAddress).HasMaxLength(255);

                entity.Property(e => e.BankName).HasMaxLength(255);

                entity.Property(e => e.Beneficiary).HasMaxLength(250);

                entity.Property(e => e.ChiefAccountantDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ChiefAccountantId)
                    .HasColumnName("ChiefAccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefAccountantStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCheckPayment).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DirectorDate).HasColumnType("smalldatetime");

                entity.Property(e => e.DirectorId)
                    .HasColumnName("DirectorID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DirectorStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NotPaidPriceTotal).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OpsmanDate)
                    .HasColumnName("OPSManDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.OpsmanId)
                    .HasColumnName("OPSManID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanStatus)
                    .HasColumnName("OPSManStatus")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OtherRequestor)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentMethodType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentObject)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RefNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.TotalPayment).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCheckPayment)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<AcctSoa>(entity =>
            {
                entity.ToTable("acctSOA");

                entity.HasIndex(e => e.Code)
                    .HasName("U_acctSOA_Code")
                    .IsUnique();

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

                entity.Property(e => e.CustomerConfirmDate).HasColumnType("smalldatetime");

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.ExportedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.FreightPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.InvoiceNo).HasMaxLength(100);

                entity.Property(e => e.PaidBehalfPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaidDate).HasColumnType("smalldatetime");

                entity.Property(e => e.PaidFreightPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaidPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaymentDueDate).HasColumnType("smalldatetime");

                entity.Property(e => e.SentByUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SentOn).HasColumnType("smalldatetime");

                entity.Property(e => e.StatementDate).HasColumnType("smalldatetime");

                entity.Property(e => e.Total).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TrackingTransportBill)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TrackingTransportDate).HasColumnType("smalldatetime");

                entity.Property(e => e.UnlockedDirector)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UnlockedDirectorDate).HasColumnType("smalldatetime");

                entity.Property(e => e.UnlockedDirectorStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UnlockedSaleMan)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UnlockedSaleManDate).HasColumnType("smalldatetime");

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

            modelBuilder.Entity<AcctSoahistory>(entity =>
            {
                entity.ToTable("acctSOAHistory");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.BehalfPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CustomerConfirmDate).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.FreightPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaidBehalfPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaidDate).HasColumnType("smalldatetime");

                entity.Property(e => e.PaidFreightPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaidPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Soano)
                    .IsRequired()
                    .HasColumnName("SOANo")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AuditAction>(entity =>
            {
                entity.ToTable("auditAction");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Datetime).HasColumnType("datetime");

                entity.Property(e => e.TableName).HasMaxLength(50);

                entity.Property(e => e.Type).HasMaxLength(50);

                entity.Property(e => e.UserId)
                    .HasColumnName("UserID")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkPlace).HasMaxLength(10);
            });

            modelBuilder.Entity<AuditUpdate>(entity =>
            {
                entity.HasKey(e => new { e.ActionId, e.FieldName, e.KeyValue });

                entity.ToTable("auditUpdate");

                entity.Property(e => e.ActionId).HasColumnName("ActionID");

                entity.Property(e => e.FieldName).HasMaxLength(50);

                entity.Property(e => e.KeyValue).HasMaxLength(300);
            });

            modelBuilder.Entity<AwbcodeFollowTypeBooking>(entity =>
            {
                entity.HasKey(e => new { e.TypeBookingAwb, e.DateAwb, e.BrandCode });

                entity.ToTable("AWBCodeFollowTypeBooking");

                entity.Property(e => e.TypeBookingAwb)
                    .HasColumnName("TypeBookingAWB")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DateAwb)
                    .HasColumnName("DateAWB")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.BrandCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<BookingCodeFollowTypeBooking>(entity =>
            {
                entity.HasKey(e => new { e.TypeBooking, e.DateCode, e.BrandCode });

                entity.Property(e => e.TypeBooking)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DateCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.BrandCode)
                    .HasMaxLength(10)
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

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(255);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(255);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatBranch>(entity =>
            {
                entity.HasKey(e => e.BranchId);

                entity.ToTable("catBranch");

                entity.Property(e => e.BranchId)
                    .HasColumnName("BranchID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Address).HasMaxLength(250);

                entity.Property(e => e.Bank).HasMaxLength(250);

                entity.Property(e => e.BankAccountUsd)
                    .HasColumnName("BankAccount_USD")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.BankAccountVnd)
                    .HasColumnName("BankAccount_VND")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.BankAddress).HasMaxLength(250);

                entity.Property(e => e.BillingAddress).HasMaxLength(500);

                entity.Property(e => e.ContactNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ContactPerson).HasMaxLength(150);

                entity.Property(e => e.DistrictId).HasColumnName("DistrictID");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.HotLine)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.HubId).HasColumnName("HubID");

                entity.Property(e => e.IncreasingId)
                    .HasColumnName("IncreasingID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");

                entity.Property(e => e.PublicNameEn)
                    .HasColumnName("PublicName_EN")
                    .HasMaxLength(250);

                entity.Property(e => e.PublicNameVn)
                    .HasColumnName("PublicName_VN")
                    .HasMaxLength(250);

                entity.Property(e => e.SwiftCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TaxCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Tel)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Website).HasMaxLength(250);
            });

            modelBuilder.Entity<CatCharge>(entity =>
            {
                entity.ToTable("catCharge");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.ChargeNameEn)
                    .HasColumnName("ChargeName_EN")
                    .HasMaxLength(255);

                entity.Property(e => e.ChargeNameVn)
                    .HasColumnName("ChargeName_VN")
                    .HasMaxLength(255);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.ShipmentTypeId)
                    .HasColumnName("ShipmentTypeID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatChargeType>(entity =>
            {
                entity.ToTable("catChargeType", "lcl");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(50);

                entity.Property(e => e.NameVn)
                    .IsRequired()
                    .HasColumnName("Name_VN")
                    .HasMaxLength(50);

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
                    .HasMaxLength(150);

                entity.Property(e => e.CommodityNameVn)
                    .IsRequired()
                    .HasColumnName("CommodityName_VN")
                    .HasMaxLength(150);

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
                    .HasMaxLength(150);

                entity.Property(e => e.GroupNameVn)
                    .HasColumnName("GroupName_VN")
                    .HasMaxLength(150);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Note).HasMaxLength(250);

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

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.ExtraWeight).HasColumnType("decimal(10, 4)");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Name).HasMaxLength(100);

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

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(150);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(150);

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

            modelBuilder.Entity<CatCurrencyExchange>(entity =>
            {
                entity.ToTable("catCurrencyExchange");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CurrencyFromId)
                    .IsRequired()
                    .HasColumnName("CurrencyFromID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyToId)
                    .IsRequired()
                    .HasColumnName("CurrencyToID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.EffectiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Rate).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatCustomerGoodsDescription>(entity =>
            {
                entity.ToTable("catCustomerGoodsDescription");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Brand).HasMaxLength(150);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Height).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.Length).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.Model).HasMaxLength(150);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Sku).HasColumnName("SKU");

                entity.Property(e => e.Volume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Weight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Width).HasColumnType("decimal(8, 3)");
            });

            modelBuilder.Entity<CatCustomerPlace>(entity =>
            {
                entity.ToTable("catCustomerPlace");

                entity.Property(e => e.Id).HasColumnName("ID");

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

            modelBuilder.Entity<CatCustomerShipmentNote>(entity =>
            {
                entity.ToTable("catCustomerShipmentNote", "lcl");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.ShipmentNoteId).HasColumnName("ShipmentNoteID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatDeliveryZoneCode>(entity =>
            {
                entity.ToTable("catDeliveryZoneCode", "lcl");

                entity.HasIndex(e => new { e.OriginBranchId, e.ToPlace })
                    .HasName("U_DeliveryZoneCode")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.IsRas).HasColumnName("IsRAS");

                entity.Property(e => e.OriginBranchId).HasColumnName("OriginBranchID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ZoneId).HasColumnName("ZoneID");
            });

            modelBuilder.Entity<CatDeliveryZoneCode1>(entity =>
            {
                entity.ToTable("catDeliveryZoneCode");

                entity.HasIndex(e => new { e.OriginBranchId, e.ToPlace })
                    .HasName("U_DeliveryZone")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.IsRas).HasColumnName("IsRAS");

                entity.Property(e => e.OriginBranchId).HasColumnName("OriginBranchID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ZoneId).HasColumnName("ZoneID");

                entity.HasOne(d => d.OriginBranch)
                    .WithMany(p => p.CatDeliveryZoneCode1)
                    .HasForeignKey(d => d.OriginBranchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_catDeliveryZone_catBranch");
            });

            modelBuilder.Entity<CatDepartment>(entity =>
            {
                entity.ToTable("catDepartment");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DeptName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Description).HasMaxLength(150);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatDistrict>(entity =>
            {
                entity.HasKey(e => e.DistrictId);

                entity.ToTable("catDistrict");

                entity.Property(e => e.DistrictId)
                    .HasColumnName("DistrictID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");
            });

            modelBuilder.Entity<CatDriver>(entity =>
            {
                entity.ToTable("catDriver");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AddressEn)
                    .HasColumnName("Address_EN")
                    .HasMaxLength(255);

                entity.Property(e => e.AddressVn)
                    .HasColumnName("Address_VN")
                    .HasMaxLength(255);

                entity.Property(e => e.Bank).HasMaxLength(250);

                entity.Property(e => e.BankAccountNumber)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.BankAddress).HasMaxLength(250);

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Dln)
                    .HasColumnName("DLN")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DlnexpiryDate)
                    .HasColumnName("DLNExpiryDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.DriverNameEn)
                    .HasColumnName("DriverName_EN")
                    .HasMaxLength(150);

                entity.Property(e => e.DriverNameVn)
                    .IsRequired()
                    .HasColumnName("DriverName_VN")
                    .HasMaxLength(150);

                entity.Property(e => e.EmployeeIdnumber)
                    .HasColumnName("EmployeeIDNumber")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.EmployeeNumber).HasMaxLength(50);

                entity.Property(e => e.IdentityCard)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.LivingPlace).HasMaxLength(255);

                entity.Property(e => e.Note).HasMaxLength(800);

                entity.Property(e => e.NricissuedBy)
                    .HasColumnName("NRICIssuedBy")
                    .HasMaxLength(100);

                entity.Property(e => e.NricissuedOn)
                    .HasColumnName("NRICIssuedOn")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.Password).HasMaxLength(250);

                entity.Property(e => e.PaymentBeneficiary).HasMaxLength(100);

                entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");

                entity.Property(e => e.RelativeTel)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Signature).HasColumnType("image");

                entity.Property(e => e.Team).HasMaxLength(50);

                entity.Property(e => e.TeamPosition).HasMaxLength(50);

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

                entity.Property(e => e.WorkingPhone)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatDriverAppHistory>(entity =>
            {
                entity.ToTable("catDriverAppHistory");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DriverId).HasColumnName("DriverID");

                entity.Property(e => e.Imei)
                    .IsRequired()
                    .HasColumnName("IMEI")
                    .HasMaxLength(50);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.StartedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.Token).HasMaxLength(250);

                entity.Property(e => e.Version).HasMaxLength(50);
            });

            modelBuilder.Entity<CatDriverManagementArea>(entity =>
            {
                entity.ToTable("catDriverManagementArea", "lcl");

                entity.HasIndex(e => new { e.DriverId, e.PlaceId })
                    .HasName("U_catDriverManagementArea")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DriverId).HasColumnName("DriverID");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.PlaceId).HasColumnName("PlaceID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatFclserviceType>(entity =>
            {
                entity.ToTable("catFCLServiceType", "fcl");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatGeoCode>(entity =>
            {
                entity.ToTable("catGeoCode");

                entity.HasIndex(e => e.GeoCode)
                    .HasName("U_catGeoCode")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.FuelAllowance).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelAllowanceNote).HasMaxLength(500);

                entity.Property(e => e.GeoCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatHub>(entity =>
            {
                entity.HasKey(e => e.HubId);

                entity.ToTable("catHub");

                entity.Property(e => e.HubId)
                    .HasColumnName("HubID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Address).HasMaxLength(150);

                entity.Property(e => e.Buid).HasColumnName("BUID");

                entity.Property(e => e.ContactNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ContactPerson).HasMaxLength(255);

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.DistrictId).HasColumnName("DistrictID");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.IncreasingId)
                    .HasColumnName("IncreasingID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.PostalCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.CatHub)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("FK_sysHub_catCountry");
            });

            modelBuilder.Entity<CatMobile>(entity =>
            {
                entity.HasKey(e => e.PhoneNumber);

                entity.ToTable("catMobile");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.AppVersion).HasMaxLength(50);

                entity.Property(e => e.Imei1)
                    .HasColumnName("IMEI1")
                    .HasMaxLength(100);

                entity.Property(e => e.Imei2)
                    .HasColumnName("IMEI2")
                    .HasMaxLength(100);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<CatOrderStatusReason>(entity =>
            {
                entity.ToTable("catOrderStatusReason", "lcl");

                entity.HasIndex(e => e.Code)
                    .HasName("U_catStatusReason")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerStatusEn)
                    .HasColumnName("CustomerStatus_EN")
                    .HasMaxLength(150);

                entity.Property(e => e.CustomerStatusVn)
                    .HasColumnName("CustomerStatus_VN")
                    .HasMaxLength(150);

                entity.Property(e => e.ReasonDescriptionEn)
                    .IsRequired()
                    .HasColumnName("ReasonDescription_EN")
                    .HasMaxLength(150);

                entity.Property(e => e.ReasonDescriptionVn)
                    .IsRequired()
                    .HasColumnName("ReasonDescription_VN")
                    .HasMaxLength(150);

                entity.Property(e => e.Stage).HasMaxLength(50);

                entity.Property(e => e.StatusId).HasColumnName("StatusID");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.CatOrderStatusReason)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_catStatusReason_sysStatus");
            });

            modelBuilder.Entity<CatOtherPlace>(entity =>
            {
                entity.HasKey(e => e.PlaceId);

                entity.ToTable("catOtherPlace");

                entity.Property(e => e.PlaceId)
                    .HasColumnName("PlaceID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Address).HasMaxLength(250);

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.DistrictId).HasColumnName("DistrictID");

                entity.Property(e => e.FuelAllowance).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");

                entity.Property(e => e.SupplierId)
                    .HasColumnName("SupplierID")
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
                    .HasMaxLength(255);

                entity.Property(e => e.AddressVn)
                    .HasColumnName("Address_VN")
                    .HasMaxLength(255);

                entity.Property(e => e.BankAccountAddress).HasMaxLength(255);

                entity.Property(e => e.BankAccountName).HasMaxLength(255);

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

                entity.Property(e => e.Itlcorp).HasColumnName("ITLCorp");

                entity.Property(e => e.MyFaceId)
                    .HasColumnName("myFaceID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Note).HasMaxLength(1000);

                entity.Property(e => e.ParentId)
                    .HasColumnName("ParentID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PartnerGroup)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PartnerNameEn)
                    .HasColumnName("PartnerName_EN")
                    .HasMaxLength(255);

                entity.Property(e => e.PartnerNameVn)
                    .HasColumnName("PartnerName_VN")
                    .HasMaxLength(255);

                entity.Property(e => e.PaymentBeneficiary).HasMaxLength(250);

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

                entity.Property(e => e.ShortName).HasMaxLength(250);

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

                entity.HasOne(d => d.PartnerGroupNavigation)
                    .WithMany(p => p.CatPartner)
                    .HasForeignKey(d => d.PartnerGroup)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_catPartner_catPartnerGroup");
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
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ContactNameEn)
                    .HasColumnName("ContactName_EN")
                    .HasMaxLength(255);

                entity.Property(e => e.ContactNameVn)
                    .IsRequired()
                    .HasColumnName("ContactName_VN")
                    .HasMaxLength(255);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.FieldInterested).HasMaxLength(1500);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.JobTitle).HasMaxLength(255);

                entity.Property(e => e.Notes).HasMaxLength(2000);

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

                entity.Property(e => e.Note).HasMaxLength(500);

                entity.Property(e => e.PartnerId)
                    .IsRequired()
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

            modelBuilder.Entity<CatPickupTime>(entity =>
            {
                entity.ToTable("catPickupTime", "lcl");

                entity.HasIndex(e => new { e.FromTime, e.ToTime })
                    .HasName("U_catPickupTime")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.FromTime)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.ToTime)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.UpdatedOn).HasColumnType("smalldatetime");
            });

            modelBuilder.Entity<CatPickupZoneCode>(entity =>
            {
                entity.ToTable("catPickupZoneCode");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.PickupPlaceId).HasColumnName("PickupPlaceID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ZoneId).HasColumnName("ZoneID");
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

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DisplayName).HasMaxLength(200);

                entity.Property(e => e.GeoCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(200);

                entity.Property(e => e.NameVn)
                    .IsRequired()
                    .HasColumnName("Name_VN")
                    .HasMaxLength(200);

                entity.Property(e => e.Note).HasMaxLength(500);

                entity.Property(e => e.PlaceTypeId)
                    .IsRequired()
                    .HasColumnName("PlaceTypeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatPlaceDistance>(entity =>
            {
                entity.ToTable("catPlaceDistance");

                entity.HasIndex(e => new { e.PlaceFrom, e.PlaceTo, e.ShipmentTypeId, e.Kratio })
                    .HasName("U_catPlaceDistance")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Kratio).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.Note).HasMaxLength(500);

                entity.Property(e => e.ShipmentTypeId)
                    .IsRequired()
                    .HasColumnName("ShipmentTypeID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.ShipmentType)
                    .WithMany(p => p.CatPlaceDistance)
                    .HasForeignKey(d => d.ShipmentTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_catPlaceDistance_catShipmentType");
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
                    .HasMaxLength(50);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(50);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatPosition>(entity =>
            {
                entity.ToTable("catPosition");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DepartmentId)
                    .HasColumnName("DepartmentID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.PositionName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatProvince>(entity =>
            {
                entity.ToTable("catProvince");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AreaId)
                    .HasColumnName("AreaID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.HasOne(d => d.Area)
                    .WithMany(p => p.CatProvince)
                    .HasForeignKey(d => d.AreaId)
                    .HasConstraintName("FK_catProvince_catArea");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.CatProvince)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_catProvince_catCountry");
            });

            modelBuilder.Entity<CatReceptacleType>(entity =>
            {
                entity.ToTable("catReceptacleType", "lcl");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.UnitNameEn)
                    .HasColumnName("UnitName_EN")
                    .HasMaxLength(100);

                entity.Property(e => e.UnitNameVn)
                    .IsRequired()
                    .HasColumnName("UnitName_VN")
                    .HasMaxLength(100);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatRoad>(entity =>
            {
                entity.ToTable("catRoad");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(510);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(150);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatRoute>(entity =>
            {
                entity.ToTable("catRoute");

                entity.HasIndex(e => new { e.BranchId, e.Code })
                    .HasName("U_RouteCode")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.HaulType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Kratio).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.Note).HasMaxLength(500);

                entity.Property(e => e.RoadId)
                    .IsRequired()
                    .HasColumnName("RoadID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatRouteShortTrip>(entity =>
            {
                entity.ToTable("catRouteShortTrip");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Kratio).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.RouteId).HasColumnName("RouteID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatRouteSurcharge>(entity =>
            {
                entity.ToTable("catRouteSurcharge");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ChargeId)
                    .IsRequired()
                    .HasColumnName("ChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContainerTypeId)
                    .HasColumnName("ContainerTypeID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RouteId).HasColumnName("RouteID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");

                entity.Property(e => e.WeightRangeId).HasColumnName("WeightRangeID");
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

                entity.Property(e => e.ResourceName).HasMaxLength(100);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatServiceType>(entity =>
            {
                entity.ToTable("catServiceType", "lcl");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Name)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatServiceTypeI>(entity =>
            {
                entity.ToTable("catServiceTypeI", "lcl");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.IsCod).HasColumnName("IsCOD");

                entity.Property(e => e.MaxWeight).HasColumnType("decimal(10, 3)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatServiceTypeMapping>(entity =>
            {
                entity.ToTable("catServiceTypeMapping", "lcl");

                entity.HasIndex(e => new { e.ServiceTypeId, e.ServiceTypeIId })
                    .HasName("U_catServiceTypeMapping")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.IsCod).HasColumnName("IsCOD");

                entity.Property(e => e.MappingNameEn)
                    .HasColumnName("MappingName_EN")
                    .HasMaxLength(500);

                entity.Property(e => e.MappingNameVn)
                    .IsRequired()
                    .HasColumnName("MappingName_VN")
                    .HasMaxLength(500);

                entity.Property(e => e.ServiceTypeIId).HasColumnName("ServiceTypeI_ID");

                entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceType_ID");
            });

            modelBuilder.Entity<CatShipmentNote>(entity =>
            {
                entity.ToTable("catShipmentNote", "lcl");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

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

            modelBuilder.Entity<CatShipmentType>(entity =>
            {
                entity.ToTable("catShipmentType");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactivenOn).HasColumnType("smalldatetime");

                entity.Property(e => e.TypeNameEn)
                    .HasColumnName("TypeName_EN")
                    .HasMaxLength(100);

                entity.Property(e => e.TypeNameVn)
                    .HasColumnName("TypeName_VN")
                    .HasMaxLength(100);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatStandardFuelConsumption>(entity =>
            {
                entity.ToTable("catStandardFuelConsumption");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AdditionalWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.Consumption).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.HaulType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.RouteType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");

                entity.Property(e => e.WeightRangeId).HasColumnName("WeightRangeID");
            });

            modelBuilder.Entity<CatTestMobile>(entity =>
            {
                entity.HasKey(e => e.PhoneNumber);

                entity.ToTable("catTestMobile");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.ConnectionString)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Imei1)
                    .HasColumnName("IMEI1")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Imei2)
                    .HasColumnName("IMEI2")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<CatTransitRouteMiddlePlace>(entity =>
            {
                entity.ToTable("catTransitRouteMiddlePlace", "lcl");

                entity.HasIndex(e => new { e.RouteId, e.PlaceId, e.Sequence })
                    .HasName("U_TransitRouteMiddlePlace")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.PlaceId).HasColumnName("PlaceID");

                entity.Property(e => e.RouteId).HasColumnName("RouteID");

                entity.Property(e => e.UserCreated)
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

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.ShipmentTypeId)
                    .HasColumnName("ShipmentTypeID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UnitNameEn)
                    .HasColumnName("UnitName_EN")
                    .HasMaxLength(200);

                entity.Property(e => e.UnitNameVn)
                    .HasColumnName("UnitName_VN")
                    .HasMaxLength(200);

                entity.Property(e => e.UnitType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.ShipmentType)
                    .WithMany(p => p.CatUnit)
                    .HasForeignKey(d => d.ShipmentTypeId)
                    .HasConstraintName("FK_catUnit_catShipmentType");
            });

            modelBuilder.Entity<CatUnitExchange>(entity =>
            {
                entity.HasKey(e => new { e.UnitFrom, e.UnitTo });

                entity.ToTable("catUnitExchange");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Rate).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.UnitFromNavigation)
                    .WithMany(p => p.CatUnitExchangeUnitFromNavigation)
                    .HasForeignKey(d => d.UnitFrom)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_catUnitExchange_catUnitFrom");

                entity.HasOne(d => d.UnitToNavigation)
                    .WithMany(p => p.CatUnitExchangeUnitToNavigation)
                    .HasForeignKey(d => d.UnitTo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_catUnitExchange_catUnitTo");
            });

            modelBuilder.Entity<CatVehicle>(entity =>
            {
                entity.ToTable("catVehicle");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.Color).HasMaxLength(50);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Engine).HasMaxLength(50);

                entity.Property(e => e.EngineNumber).HasMaxLength(50);

                entity.Property(e => e.FrameNumber).HasMaxLength(50);

                entity.Property(e => e.Gpsprovider).HasColumnName("GPSProvider");

                entity.Property(e => e.Gw)
                    .HasColumnName("GW")
                    .HasColumnType("decimal(10, 3)");

                entity.Property(e => e.Image).HasColumnType("image");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.InsuranceAccount).HasMaxLength(50);

                entity.Property(e => e.InsuranceDue).HasColumnType("date");

                entity.Property(e => e.InsurancePartner).HasMaxLength(50);

                entity.Property(e => e.InsurancePremium).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.LicensePlate)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Made).HasMaxLength(50);

                entity.Property(e => e.Model).HasMaxLength(200);

                entity.Property(e => e.Note).HasMaxLength(1000);

                entity.Property(e => e.OdometerType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Owner)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.PuchaseCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PurchaseDate).HasColumnType("datetime");

                entity.Property(e => e.Renewal).HasMaxLength(50);

                entity.Property(e => e.RepairStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SubsidizedEffectiveDate).HasColumnType("smalldatetime");

                entity.Property(e => e.SubsidizedFuel).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TireSize).HasMaxLength(50);

                entity.Property(e => e.Transmission).HasMaxLength(50);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");

                entity.Property(e => e.Vendor).HasMaxLength(50);

                entity.Property(e => e.Year)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.GpsproviderNavigation)
                    .WithMany(p => p.CatVehicle)
                    .HasForeignKey(d => d.Gpsprovider)
                    .HasConstraintName("FK_catVehicle_sysGPSProvider");

                entity.HasOne(d => d.VehicleType)
                    .WithMany(p => p.CatVehicle)
                    .HasForeignKey(d => d.VehicleTypeId)
                    .HasConstraintName("FK_catVehicle_catVehicleType");
            });

            modelBuilder.Entity<CatVehicleDriver>(entity =>
            {
                entity.ToTable("catVehicleDriver");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DifferenceFuel).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.DriverId).HasColumnName("DriverID");

                entity.Property(e => e.FuelPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.LockedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.Note).HasMaxLength(800);

                entity.Property(e => e.ReceiptedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ReceivedFuel).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.RemoocId).HasColumnName("RemoocID");

                entity.Property(e => e.ReturnedFuel).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleId).HasColumnName("VehicleID");

                entity.HasOne(d => d.Driver)
                    .WithMany(p => p.CatVehicleDriver)
                    .HasForeignKey(d => d.DriverId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_catVehicleDriver_catDriver");

                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.CatVehicleDriver)
                    .HasForeignKey(d => d.VehicleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_catVehicleDriver_catVehicle");
            });

            modelBuilder.Entity<CatVehicleGroup>(entity =>
            {
                entity.ToTable("catVehicleGroup");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.GroupNameEn)
                    .HasColumnName("GroupName_EN")
                    .HasMaxLength(255);

                entity.Property(e => e.GroupNameVn)
                    .IsRequired()
                    .HasColumnName("GroupName_VN")
                    .HasMaxLength(255);

                entity.Property(e => e.InactivenOn).HasColumnType("smalldatetime");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatVehicleLocation>(entity =>
            {
                entity.ToTable("catVehicleLocation");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Address)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.GeoCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("smalldatetime");

                entity.Property(e => e.VehicleId).HasColumnName("VehicleID");
            });

            modelBuilder.Entity<CatVehiclePart>(entity =>
            {
                entity.ToTable("catVehiclePart");

                entity.HasIndex(e => e.Code)
                    .HasName("U_catVehiclePart")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Description).HasMaxLength(800);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.PartNameEn)
                    .HasColumnName("PartName_EN")
                    .HasMaxLength(150);

                entity.Property(e => e.PartNameVn)
                    .IsRequired()
                    .HasColumnName("PartName_VN")
                    .HasMaxLength(150);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UseObject)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehiclePartTypeId).HasColumnName("VehiclePartTypeID");

                entity.HasOne(d => d.Unit)
                    .WithMany(p => p.CatVehiclePart)
                    .HasForeignKey(d => d.UnitId)
                    .HasConstraintName("FK_catVehiclePart_catUnit");

                entity.HasOne(d => d.VehicleGroupNavigation)
                    .WithMany(p => p.CatVehiclePart)
                    .HasForeignKey(d => d.VehicleGroup)
                    .HasConstraintName("FK_catVehiclePart_catVehicleGroup");
            });

            modelBuilder.Entity<CatVehiclePartDetail>(entity =>
            {
                entity.ToTable("catVehiclePartDetail");

                entity.HasIndex(e => new { e.VehicleId, e.VehiclePartId })
                    .HasName("U_catVehiclePartDetail")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Description).HasMaxLength(250);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleId).HasColumnName("VehicleID");

                entity.Property(e => e.VehiclePartId).HasColumnName("VehiclePartID");
            });

            modelBuilder.Entity<CatVehiclePartDetailHistory>(entity =>
            {
                entity.ToTable("catVehiclePartDetailHistory");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Description).HasMaxLength(250);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.LiquidatedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ReplacedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.Serial)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleId).HasColumnName("VehicleID");

                entity.Property(e => e.VehiclePartId).HasColumnName("VehiclePartID");

                entity.Property(e => e.VehicleWorkPlaceId).HasColumnName("VehicleWorkPlaceID");

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<CatVehiclePartPrice>(entity =>
            {
                entity.ToTable("catVehiclePartPrice");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehiclePartId).HasColumnName("VehiclePartID");

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<CatVehiclePartType>(entity =>
            {
                entity.ToTable("catVehiclePartType");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(250);

                entity.Property(e => e.NameVn)
                    .IsRequired()
                    .HasColumnName("Name_VN")
                    .HasMaxLength(250);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatVehicleType>(entity =>
            {
                entity.ToTable("catVehicleType");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.HaulType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.InactivenOn).HasColumnType("smalldatetime");

                entity.Property(e => e.MaxWeight).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.ShipmentTypeId)
                    .HasColumnName("ShipmentTypeID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ShortName).HasMaxLength(50);

                entity.Property(e => e.TypeNameEn)
                    .HasColumnName("TypeName_EN")
                    .HasMaxLength(255);

                entity.Property(e => e.TypeNameVn)
                    .IsRequired()
                    .HasColumnName("TypeName_VN")
                    .HasMaxLength(255);

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleGroupId).HasColumnName("VehicleGroupID");

                entity.HasOne(d => d.VehicleGroup)
                    .WithMany(p => p.CatVehicleType)
                    .HasForeignKey(d => d.VehicleGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_catVehicleType_catVehicleGroup");
            });

            modelBuilder.Entity<CatVehicleWorkPlace>(entity =>
            {
                entity.ToTable("catVehicleWorkPlace");

                entity.HasIndex(e => new { e.VehicleId, e.WorkPlaceId, e.FromDate })
                    .HasName("U_VehicleWorkPlace")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.FromDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ToDate).HasColumnType("smalldatetime");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleId).HasColumnName("VehicleID");

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<CatVolume>(entity =>
            {
                entity.ToTable("catVolume");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

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

            modelBuilder.Entity<CatWard>(entity =>
            {
                entity.HasKey(e => e.WardId);

                entity.ToTable("catWard");

                entity.Property(e => e.WardId)
                    .HasColumnName("WardID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DistrictId).HasColumnName("DistrictID");

                entity.Property(e => e.PostalCode)
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatWeightRange>(entity =>
            {
                entity.ToTable("catWeightRange");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.MaxWeight).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.MinWeight).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.ShipmentTypeId)
                    .HasColumnName("ShipmentTypeID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatZoneCode>(entity =>
            {
                entity.ToTable("catZoneCode");

                entity.HasIndex(e => new { e.Code, e.Type })
                    .HasName("U_ZoneCode")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Description).HasMaxLength(250);

                entity.Property(e => e.DistanceFrom).HasColumnType("decimal(10, 3)");

                entity.Property(e => e.DistanceTo).HasColumnType("decimal(10, 3)");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

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
            });

            modelBuilder.Entity<CsChangedSurchargeLog>(entity =>
            {
                entity.ToTable("csChangedSurchargeLog");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ChargeType)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.NewCurrencyId)
                    .HasColumnName("NewCurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.NewIncludedVat).HasColumnName("NewIncludedVAT");

                entity.Property(e => e.NewObjectBePaid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.NewPaymentObjectId)
                    .HasColumnName("NewPaymentObjectID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.NewPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Note).HasMaxLength(250);

                entity.Property(e => e.OldCurrencyId)
                    .HasColumnName("OldCurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.OldIncludedVat).HasColumnName("OldIncludedVAT");

                entity.Property(e => e.OldObjectBePaid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OldPaymentObjectId)
                    .HasColumnName("OldPaymentObjectID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OldPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransportSurchargeId).HasColumnName("TransportSurchargeID");

                entity.Property(e => e.UpdatedTime).HasColumnType("smalldatetime");

                entity.Property(e => e.UpdatedUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsDocument>(entity =>
            {
                entity.ToTable("csDocument");

                entity.HasIndex(e => new { e.BranchId, e.ReferenceObject, e.DocType, e.FileName })
                    .HasName("U_Document")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DocType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FileCheckSum).HasMaxLength(100);

                entity.Property(e => e.FileData).IsRequired();

                entity.Property(e => e.FileDescription).HasMaxLength(500);

                entity.Property(e => e.FileName).HasMaxLength(150);

                entity.Property(e => e.Icon).HasColumnType("image");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.ReferenceObject).HasMaxLength(100);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsDtborder>(entity =>
            {
                entity.ToTable("csDTBOrder", "dtb");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AdjustedPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.AdjustedSoadate)
                    .HasColumnName("AdjustedSOADate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.AdjustedSoauser)
                    .HasColumnName("AdjustedSOAUser")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Awb)
                    .HasColumnName("AWB")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.BillingNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CommodityId).HasColumnName("CommodityID");

                entity.Property(e => e.Contact)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.CountryFromId).HasColumnName("CountryFromID");

                entity.Property(e => e.CountryToId).HasColumnName("CountryToID");

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CurrentStatusId).HasColumnName("CurrentStatusID");

                entity.Property(e => e.CustomerBookingNo).HasMaxLength(50);

                entity.Property(e => e.CustomerDebitId)
                    .HasColumnName("CustomerDebitID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasColumnName("CustomerID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerRouteCode).HasMaxLength(50);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DeliveryStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DestinationBranchId).HasColumnName("DestinationBranchID");

                entity.Property(e => e.DestinationHubId).HasColumnName("DestinationHubID");

                entity.Property(e => e.DistrictFromId).HasColumnName("DistrictFromID");

                entity.Property(e => e.DistrictToId).HasColumnName("DistrictToID");

                entity.Property(e => e.FailedDeliveryDueTo).HasMaxLength(50);

                entity.Property(e => e.FailedDeliveryReason).HasMaxLength(250);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.OriginBranchId).HasColumnName("OriginBranchID");

                entity.Property(e => e.OriginHubId).HasColumnName("OriginHubID");

                entity.Property(e => e.PickupRequestDate).HasColumnType("smalldatetime");

                entity.Property(e => e.PlaceFromId).HasColumnName("PlaceFromID");

                entity.Property(e => e.PlaceToId).HasColumnName("PlaceToID");

                entity.Property(e => e.PodhanoverRequestDate)
                    .HasColumnName("PODHanoverRequestDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.PodleadTime).HasColumnName("PODLeadTime");

                entity.Property(e => e.PodreceivedDate)
                    .HasColumnName("PODReceivedDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.PodreturnedDate)
                    .HasColumnName("PODReturnedDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.ProvinceFromId).HasColumnName("ProvinceFromID");

                entity.Property(e => e.ProvinceToId).HasColumnName("ProvinceToID");

                entity.Property(e => e.RateCardDetailId).HasColumnName("RateCardDetailID");

                entity.Property(e => e.ReceivedBookingDate).HasColumnType("smalldatetime");

                entity.Property(e => e.Remark).HasMaxLength(500);

                entity.Property(e => e.SalePersonId)
                    .HasColumnName("SalePersonID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.SoaadjustmentReason)
                    .HasColumnName("SOAAdjustmentReason")
                    .HasMaxLength(500);

                entity.Property(e => e.SoaadjustmentRequestedDate)
                    .HasColumnName("SOAAdjustmentRequestedDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.SoaadjustmentRequestor)
                    .HasColumnName("SOAAdjustmentRequestor")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SoaadjustmentType)
                    .HasColumnName("SOAAdjustmentType")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Soaclosed).HasColumnName("SOAClosed");

                entity.Property(e => e.Soano)
                    .HasColumnName("SOANo")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Sotype)
                    .HasColumnName("SOType")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalExcludeSurcharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalExcludeVat)
                    .HasColumnName("TotalExcludeVAT")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalIncludeVat)
                    .HasColumnName("TotalIncludeVAT")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalSurcharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnlockedSoadirector)
                    .HasColumnName("UnlockedSOADirector")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UnlockedSoadirectorDate)
                    .HasColumnName("UnlockedSOADirectorDate")
                    .HasColumnType("smalldatetime");

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
                    .HasColumnType("smalldatetime");

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

                entity.Property(e => e.Vat)
                    .HasColumnName("VAT")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Vatrate).HasColumnName("VATRate");

                entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");

                entity.Property(e => e.WarehouseBookingNo).HasMaxLength(50);
            });

            modelBuilder.Entity<CsDtborderChargeBehalf>(entity =>
            {
                entity.ToTable("csDTBOrderChargeBehalf", "dtb");

                entity.HasIndex(e => new { e.OrderId, e.SupplierId, e.ChargeId, e.TransportRequestId })
                    .HasName("U_csDTBOrderChargeBehalf")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountantDate).HasColumnType("smalldatetime");

                entity.Property(e => e.AccountantId)
                    .HasColumnName("AccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantNote).HasMaxLength(500);

                entity.Property(e => e.AccountantStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BehalfType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChargeId)
                    .IsRequired()
                    .HasColumnName("ChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefAccountantId)
                    .HasColumnName("ChiefAccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ChiefNote).HasMaxLength(500);

                entity.Property(e => e.ChiefStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CsdateTripSettlement)
                    .HasColumnName("CSDateTripSettlement")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.CsidtripSettlement)
                    .HasColumnName("CSIDTripSettlement")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CsstatusTripSettlement)
                    .HasColumnName("CSStatusTripSettlement")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.InvoiceNo).HasMaxLength(50);

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.ObjectBePaid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanDate)
                    .HasColumnName("OPSManDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.OpsmanId)
                    .HasColumnName("OPSManID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanNote)
                    .HasColumnName("OPSManNote")
                    .HasMaxLength(500);

                entity.Property(e => e.OpsmanStatus)
                    .HasColumnName("OPSManStatus")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OrderId).HasColumnName("OrderID");

                entity.Property(e => e.PaymentRefNo).HasMaxLength(50);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReSignedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ReSignedUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReceivingPlace)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Remain).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.SupplierId)
                    .HasColumnName("SupplierID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TransportRequestId).HasColumnName("TransportRequestID");

                entity.Property(e => e.TripSettlementCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsDtborderDropPoint>(entity =>
            {
                entity.ToTable("csDTBOrderDropPoint", "dtb");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Address).HasMaxLength(250);

                entity.Property(e => e.ArrivedTime).HasColumnType("smalldatetime");

                entity.Property(e => e.AssignedTransportWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Codvalue)
                    .HasColumnName("CODValue")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CompanyName).HasMaxLength(250);

                entity.Property(e => e.ContactPerson).HasMaxLength(100);

                entity.Property(e => e.ContactPhone)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CurrentStatusId).HasColumnName("CurrentStatusID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DistrictId).HasColumnName("DistrictID");

                entity.Property(e => e.GeoCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.IndicatedCodvalue)
                    .HasColumnName("IndicatedCODValue")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.LeftTime).HasColumnType("smalldatetime");

                entity.Property(e => e.Note).HasMaxLength(500);

                entity.Property(e => e.OrderId).HasColumnName("OrderID");

                entity.Property(e => e.PointType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");

                entity.Property(e => e.RequestDate).HasColumnType("smalldatetime");

                entity.Property(e => e.RequestFromTime)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.RequestToTime)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.TotalActualChargedWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalActualVolume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalActualWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalEstimateChargedWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalEstimateVolume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalEstimateWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WardId).HasColumnName("WardID");
            });

            modelBuilder.Entity<CsDtborderDropPointItem>(entity =>
            {
                entity.ToTable("csDTBOrderDropPointItem", "dtb");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ActualChargeableWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ActualHeight).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.ActualLength).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.ActualVolume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ActualWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ActualWidth).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.EstimateChargeableWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.EstimateHeight).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.EstimateLength).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.EstimateVolume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.EstimateWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.EstimateWidth).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.ItemDescription).HasMaxLength(255);

                entity.Property(e => e.Model).HasMaxLength(150);

                entity.Property(e => e.OrderDropPointId).HasColumnName("OrderDropPointID");

                entity.Property(e => e.ReProtectedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ReProtectedUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReweighedBy)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ReweighedOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Sku)
                    .HasColumnName("SKU")
                    .HasMaxLength(150);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsDtborderDropPointItemRoute>(entity =>
            {
                entity.ToTable("csDTBOrderDropPointItemRoute", "dtb");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AssignedTransportWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CurrentStatusId).HasColumnName("CurrentStatusID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DeliveryArrivedTime).HasColumnType("smalldatetime");

                entity.Property(e => e.DeliveryLeftTime).HasColumnType("smalldatetime");

                entity.Property(e => e.DeliveryPointId).HasColumnName("DeliveryPointID");

                entity.Property(e => e.DeliveryRequestDate).HasColumnType("smalldatetime");

                entity.Property(e => e.DeliveryRequestFromTime)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DeliveryRequestToTime)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Note).HasMaxLength(500);

                entity.Property(e => e.OrderId).HasColumnName("OrderID");

                entity.Property(e => e.PickupArrivedTime).HasColumnType("smalldatetime");

                entity.Property(e => e.PickupLeftTime).HasColumnType("smalldatetime");

                entity.Property(e => e.PickupPointItemId).HasColumnName("PickupPointItemID");

                entity.Property(e => e.PickupRequestDate).HasColumnType("smalldatetime");

                entity.Property(e => e.PickupRequestFromTime)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.PickupRequestToTime)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Volume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Weight).HasColumnType("decimal(18, 4)");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.CsDtborderDropPointItemRoute)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_csDTBOrderDropPointItemRoute_csDTBOrder");
            });

            modelBuilder.Entity<CsDtborderExpense>(entity =>
            {
                entity.ToTable("csDTBOrderExpense", "dtb");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountantDate).HasColumnType("smalldatetime");

                entity.Property(e => e.AccountantId)
                    .HasColumnName("AccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantNote).HasMaxLength(500);

                entity.Property(e => e.AccountantStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChargeId)
                    .IsRequired()
                    .HasColumnName("ChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefAccountantId)
                    .HasColumnName("ChiefAccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ChiefNote).HasMaxLength(500);

                entity.Property(e => e.ChiefStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CsdateTripSettlement)
                    .HasColumnName("CSDateTripSettlement")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.CsidtripSettlement)
                    .HasColumnName("CSIDTripSettlement")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CsstatusTripSettlement)
                    .HasColumnName("CSStatusTripSettlement")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.InvoiceNo).HasMaxLength(250);

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.ObjectBePaid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanDate)
                    .HasColumnName("OPSManDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.OpsmanId)
                    .HasColumnName("OPSManID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanNote)
                    .HasColumnName("OPSManNote")
                    .HasMaxLength(500);

                entity.Property(e => e.OpsmanStatus)
                    .HasColumnName("OPSManStatus")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OrderId).HasColumnName("OrderID");

                entity.Property(e => e.PaymentObjectId)
                    .HasColumnName("PaymentObjectID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentRefNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReSignedDate).HasColumnType("datetime");

                entity.Property(e => e.ReSignedUser).HasMaxLength(50);

                entity.Property(e => e.Remain).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.Tariff).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransportRequestId).HasColumnName("TransportRequestID");

                entity.Property(e => e.TripSettlementCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsDtborderSurcharge>(entity =>
            {
                entity.ToTable("csDTBOrderSurcharge", "dtb");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ChargeId)
                    .IsRequired()
                    .HasColumnName("ChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.InvoiceNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.OrderId).HasColumnName("OrderID");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.Tariff).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransportRequestId).HasColumnName("TransportRequestID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsDtbtransportSurcharge>(entity =>
            {
                entity.ToTable("csDTBTransportSurcharge", "dtb");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountantDate).HasColumnType("smalldatetime");

                entity.Property(e => e.AccountantId)
                    .HasColumnName("AccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantNote).HasMaxLength(500);

                entity.Property(e => e.AccountantStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChargeId)
                    .IsRequired()
                    .HasColumnName("ChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefAccountantId)
                    .HasColumnName("ChiefAccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ChiefNote).HasMaxLength(500);

                entity.Property(e => e.ChiefStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CsdateTripSettlement)
                    .HasColumnName("CSDateTripSettlement")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.CsidtripSettlement)
                    .HasColumnName("CSIDTripSettlement")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CsstatusTripSettlement)
                    .HasColumnName("CSStatusTripSettlement")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.InvoiceNo).HasMaxLength(250);

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.ObjectBePaid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanDate)
                    .HasColumnName("OPSManDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.OpsmanId)
                    .HasColumnName("OPSManID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanNote)
                    .HasColumnName("OPSManNote")
                    .HasMaxLength(500);

                entity.Property(e => e.OpsmanStatus)
                    .HasColumnName("OPSManStatus")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentObjectId)
                    .HasColumnName("PaymentObjectID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentRefNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReSignedDate).HasColumnType("datetime");

                entity.Property(e => e.ReSignedUser).HasMaxLength(50);

                entity.Property(e => e.Remain).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.Tariff).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransportId).HasColumnName("TransportID");

                entity.Property(e => e.TripSettlementCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsDtbtransportTripRecord>(entity =>
            {
                entity.ToTable("csDTBTransportTripRecord", "dtb");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Address).HasMaxLength(300);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Description).HasMaxLength(300);

                entity.Property(e => e.FuelConsumption).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelLiter).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.GeoCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Notes).HasMaxLength(300);

                entity.Property(e => e.RequestDropPointId).HasColumnName("RequestDropPointID");

                entity.Property(e => e.RouteType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SealNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalFuelCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransportId).HasColumnName("TransportID");

                entity.Property(e => e.TripDatetime).HasColumnType("smalldatetime");

                entity.Property(e => e.TripLengthCs).HasColumnName("TripLengthCS");

                entity.Property(e => e.TripLengthGps).HasColumnName("TripLengthGPS");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Weight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.WeightReal).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<CsFclbooking>(entity =>
            {
                entity.ToTable("csFCLBooking", "fcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ApprovalStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApprovedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ApprovedNote).HasMaxLength(500);

                entity.Property(e => e.ApprovedSellingDate).HasColumnType("datetime");

                entity.Property(e => e.ApprovedSellingPriceId)
                    .HasColumnName("ApprovedSellingPriceID")
                    .HasMaxLength(50);

                entity.Property(e => e.ApprovedUserId)
                    .HasColumnName("ApprovedUserID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BookingCustomer).HasMaxLength(50);

                entity.Property(e => e.BookingDate).HasColumnType("smalldatetime");

                entity.Property(e => e.BookingStatusId)
                    .IsRequired()
                    .HasColumnName("BookingStatusID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BookingType).HasMaxLength(50);

                entity.Property(e => e.BoughtFrom)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.BuyingPriceId).HasColumnName("BuyingPriceID");

                entity.Property(e => e.CloseSoa).HasColumnName("CloseSOA");

                entity.Property(e => e.ClosingTime).HasColumnType("smalldatetime");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CommodityId).HasColumnName("CommodityID");

                entity.Property(e => e.ConsigneeId)
                    .HasColumnName("ConsigneeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContNumber).HasMaxLength(50);

                entity.Property(e => e.ContainerTypeId)
                    .HasColumnName("ContainerTypeID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerBookingNo).HasMaxLength(50);

                entity.Property(e => e.CustomerDebitId)
                    .HasColumnName("CustomerDebitID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerRouteCode).HasMaxLength(50);

                entity.Property(e => e.DatetimeClosed).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DelayedTransportByCreditDate).HasMaxLength(500);

                entity.Property(e => e.DelayedTransportByOverDueDate).HasMaxLength(500);

                entity.Property(e => e.DeliveryContact).HasMaxLength(100);

                entity.Property(e => e.DeliveryDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ImExType)
                    .HasColumnName("IM_EX_Type")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.ModeOfTransport)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Note).HasMaxLength(800);

                entity.Property(e => e.OtherRequirement).HasMaxLength(1000);

                entity.Property(e => e.PaymentTerm).HasMaxLength(100);

                entity.Property(e => e.QuotationRouteId).HasColumnName("QuotationRouteID");

                entity.Property(e => e.RealWeight).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.ReceiptContact).HasMaxLength(100);

                entity.Property(e => e.Remark).HasMaxLength(800);

                entity.Property(e => e.SaleMember)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SealNumber).HasMaxLength(50);

                entity.Property(e => e.SellingCurrencyId)
                    .HasColumnName("SellingCurrencyID")
                    .HasMaxLength(50);

                entity.Property(e => e.SellingPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ShipmentStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShipperId)
                    .HasColumnName("ShipperID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingLine).HasMaxLength(100);

                entity.Property(e => e.Sotype)
                    .HasColumnName("SOType")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ToApprover).HasMaxLength(50);

                entity.Property(e => e.UserClosed)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsFclbookingDetail>(entity =>
            {
                entity.ToTable("csFCLBookingDetail", "fcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BookingId).HasColumnName("BookingID");

                entity.Property(e => e.Codvalue)
                    .HasColumnName("CODValue")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DeliveryDate).HasColumnType("smalldatetime");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.Eta)
                    .HasColumnName("ETA")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.Etd)
                    .HasColumnName("ETD")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.ReceiptDatetime).HasColumnType("smalldatetime");

                entity.Property(e => e.ReturnDatetime).HasColumnType("smalldatetime");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsFcltransportChargeBehalf>(entity =>
            {
                entity.ToTable("csFCLTransportChargeBehalf", "fcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountantDate).HasColumnType("smalldatetime");

                entity.Property(e => e.AccountantId)
                    .HasColumnName("AccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantNote).HasMaxLength(500);

                entity.Property(e => e.AccountantStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BehalfType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChargeId)
                    .IsRequired()
                    .HasColumnName("ChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefAccountantId)
                    .HasColumnName("ChiefAccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ChiefNote).HasMaxLength(500);

                entity.Property(e => e.ChiefStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CsdateTripSettlement)
                    .HasColumnName("CSDateTripSettlement")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.CsidtripSettlement)
                    .HasColumnName("CSIDTripSettlement")
                    .HasMaxLength(50);

                entity.Property(e => e.CsstatusTripSettlement)
                    .HasColumnName("CSStatusTripSettlement")
                    .HasMaxLength(50);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.InvoiceNo).HasMaxLength(50);

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.ObjectBePaid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanDate)
                    .HasColumnName("OPSManDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.OpsmanId)
                    .HasColumnName("OPSManID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanNote)
                    .HasColumnName("OPSManNote")
                    .HasMaxLength(500);

                entity.Property(e => e.OpsmanStatus)
                    .HasColumnName("OPSManStatus")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentRefNo).HasMaxLength(50);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReSignedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ReSignedUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Remain).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasColumnName("SupplierID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TransportId).HasColumnName("TransportID");

                entity.Property(e => e.TripSettleCode).HasMaxLength(50);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsFcltransportSurcharge>(entity =>
            {
                entity.ToTable("csFCLTransportSurcharge", "fcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountantDate).HasColumnType("smalldatetime");

                entity.Property(e => e.AccountantId)
                    .HasColumnName("AccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantNote).HasMaxLength(500);

                entity.Property(e => e.AccountantStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChargeId)
                    .IsRequired()
                    .HasColumnName("ChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefAccountantId)
                    .HasColumnName("ChiefAccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ChiefNote).HasMaxLength(500);

                entity.Property(e => e.ChiefStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CsdateTripSettlement)
                    .HasColumnName("CSDateTripSettlement")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.CsidtripSettlement)
                    .HasColumnName("CSIDTripSettlement")
                    .HasMaxLength(50);

                entity.Property(e => e.CsstatusTripSettlement)
                    .HasColumnName("CSStatusTripSettlement")
                    .HasMaxLength(50);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(50);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.InvoiceNo).HasMaxLength(250);

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.ObjectBePaid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanDate)
                    .HasColumnName("OPSManDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.OpsmanId)
                    .HasColumnName("OPSManID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanNote)
                    .HasColumnName("OPSManNote")
                    .HasMaxLength(500);

                entity.Property(e => e.OpsmanStatus)
                    .HasColumnName("OPSManStatus")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentObjectId)
                    .HasColumnName("PaymentObjectID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentRefNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReSignedDate).HasColumnType("datetime");

                entity.Property(e => e.ReSignedUser).HasMaxLength(50);

                entity.Property(e => e.Remain).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.Tariff).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransportId).HasColumnName("TransportID");

                entity.Property(e => e.TripSettleCode).HasMaxLength(50);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsFcltransportTripRecord>(entity =>
            {
                entity.ToTable("csFCLTransportTripRecord", "fcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Address).HasMaxLength(300);

                entity.Property(e => e.ContainerNo).HasMaxLength(50);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.FuelConsumption).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelLiter).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.GeoCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Notes).HasMaxLength(300);

                entity.Property(e => e.PriceFuel).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RouteType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SealNo).HasMaxLength(50);

                entity.Property(e => e.TotalFuelCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransportId).HasColumnName("TransportID");

                entity.Property(e => e.TripDatetime).HasColumnType("smalldatetime");

                entity.Property(e => e.TripLengthCs).HasColumnName("TripLengthCS");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Weight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.WeightReal).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<CsOrderDetail>(entity =>
            {
                entity.ToTable("csOrderDetail", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ActualDeliveryDate).HasColumnType("smalldatetime");

                entity.Property(e => e.AdjustedPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.AdjustedSoadate)
                    .HasColumnName("AdjustedSOADate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.AdjustedSoauser)
                    .HasColumnName("AdjustedSOAUser")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AssignedTransportWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Awb)
                    .HasColumnName("AWB")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.AwbbarCode)
                    .HasColumnName("AWBBarCode")
                    .HasColumnType("image");

                entity.Property(e => e.BillingNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.BookingOverDate).HasMaxLength(50);

                entity.Property(e => e.CheckedInDestinationBranchOn).HasColumnType("smalldatetime");

                entity.Property(e => e.CheckedInOriginBranchOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CodeBarCode).HasColumnType("image");

                entity.Property(e => e.Codvalue)
                    .HasColumnName("CODValue")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CommodityId).HasColumnName("CommodityID");

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CurrentStatusId).HasColumnName("CurrentStatusID");

                entity.Property(e => e.CustomerBookingNo).HasMaxLength(50);

                entity.Property(e => e.CustomerRouteCode).HasMaxLength(50);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DelayedTransportByCreditDate).HasMaxLength(500);

                entity.Property(e => e.DelayedTransportByOverDueDate).HasMaxLength(500);

                entity.Property(e => e.DelayedTransportByPriceDate).HasMaxLength(500);

                entity.Property(e => e.DeliveryAddress).HasMaxLength(250);

                entity.Property(e => e.DeliveryCompany).HasMaxLength(250);

                entity.Property(e => e.DeliveryContactNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DeliveryContactPerson).HasMaxLength(100);

                entity.Property(e => e.DeliveryCountryId).HasColumnName("DeliveryCountryID");

                entity.Property(e => e.DeliveryDistrictId).HasColumnName("DeliveryDistrictID");

                entity.Property(e => e.DeliveryGeoCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DeliveryMobileNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DeliveryPostalCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DeliveryProvinceId).HasColumnName("DeliveryProvinceID");

                entity.Property(e => e.DeliveryStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DeliveryWardId).HasColumnName("DeliveryWardID");

                entity.Property(e => e.DestinationBranchId).HasColumnName("DestinationBranchID");

                entity.Property(e => e.DestinationHubId).HasColumnName("DestinationHubID");

                entity.Property(e => e.EstimateDeliveryDate).HasColumnType("smalldatetime");

                entity.Property(e => e.EtafromDate)
                    .HasColumnName("ETAFromDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.FailedDeliveryDueTo).HasMaxLength(50);

                entity.Property(e => e.FailedDeliveryReason).HasMaxLength(250);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.IndicatedCodvalue)
                    .HasColumnName("IndicatedCODValue")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ItemValue).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OrderId).HasColumnName("OrderID");

                entity.Property(e => e.OriginBranchId).HasColumnName("OriginBranchID");

                entity.Property(e => e.OriginHubId).HasColumnName("OriginHubID");

                entity.Property(e => e.OverwriteWeightOn).HasColumnType("smalldatetime");

                entity.Property(e => e.PickupAddress).HasMaxLength(250);

                entity.Property(e => e.PickupCompany).HasMaxLength(250);

                entity.Property(e => e.PickupConfirmDate).HasColumnType("smalldatetime");

                entity.Property(e => e.PickupContactNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.PickupContactPerson).HasMaxLength(100);

                entity.Property(e => e.PickupCountryId).HasColumnName("PickupCountryID");

                entity.Property(e => e.PickupDistrictId).HasColumnName("PickupDistrictID");

                entity.Property(e => e.PickupGeoCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PickupMobileNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.PickupPostalCode).HasMaxLength(50);

                entity.Property(e => e.PickupProvinceId).HasColumnName("PickupProvinceID");

                entity.Property(e => e.PickupRequestDate).HasColumnType("smalldatetime");

                entity.Property(e => e.PickupWardId).HasColumnName("PickupWardID");

                entity.Property(e => e.PodhanoverRequestDate)
                    .HasColumnName("PODHanoverRequestDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.PodleadTime).HasColumnName("PODLeadTime");

                entity.Property(e => e.PodreceivedDate)
                    .HasColumnName("PODReceivedDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.PodreturnedDate)
                    .HasColumnName("PODReturnedDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.QuotedRouteType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RateCardConditionId).HasColumnName("RateCardConditionID");

                entity.Property(e => e.RateCardId).HasColumnName("RateCardID");

                entity.Property(e => e.Receptacle)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Remark).HasMaxLength(500);

                entity.Property(e => e.RevenueProtectionNote).HasMaxLength(500);

                entity.Property(e => e.RoutedBy)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.SentEtaemail).HasColumnName("SentETAEmail");

                entity.Property(e => e.SentEtaemailDate)
                    .HasColumnName("SentETAEmailDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.SoaadjustmentReason)
                    .HasColumnName("SOAAdjustmentReason")
                    .HasMaxLength(500);

                entity.Property(e => e.SoaadjustmentRequestedDate)
                    .HasColumnName("SOAAdjustmentRequestedDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.SoaadjustmentRequestor)
                    .HasColumnName("SOAAdjustmentRequestor")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SoaadjustmentType)
                    .HasColumnName("SOAAdjustmentType")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Soaclosed).HasColumnName("SOAClosed");

                entity.Property(e => e.Soano)
                    .HasColumnName("SOANo")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Sotype)
                    .HasColumnName("SOType")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotaIncludeVat)
                    .HasColumnName("TotaIncludeVAT")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalActualChargedWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalActualVolume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalActualWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalEstimateChargedWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalEstimateVolume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalEstimateWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalExcludeSurcharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalExcludeVat)
                    .HasColumnName("TotalExcludeVAT")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalSurcharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnlockedSoadirector)
                    .HasColumnName("UnlockedSOADirector")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UnlockedSoadirectorDate)
                    .HasColumnName("UnlockedSOADirectorDate")
                    .HasColumnType("smalldatetime");

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
                    .HasColumnType("smalldatetime");

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

                entity.Property(e => e.Vat)
                    .HasColumnName("VAT")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Vatrate).HasColumnName("VATRate");

                entity.Property(e => e.Version)
                    .IsRequired()
                    .IsRowVersion();

                entity.Property(e => e.VolumnWeightRate).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.WarehouseBookingNo).HasMaxLength(50);

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.CsOrderDetail)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_csOrderDetail_csOrderHeader");
            });

            modelBuilder.Entity<CsOrderDetailChargeBehalf>(entity =>
            {
                entity.ToTable("csOrderDetailChargeBehalf", "lcl");

                entity.HasIndex(e => new { e.OrderDetailId, e.SupplierId, e.ChargeId, e.TransportRequestId })
                    .HasName("U_csOrderDetailChargeBehalf")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountantDate).HasColumnType("smalldatetime");

                entity.Property(e => e.AccountantId)
                    .HasColumnName("AccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantNote).HasMaxLength(500);

                entity.Property(e => e.AccountantStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BehalfType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChargeId)
                    .IsRequired()
                    .HasColumnName("ChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefAccountantId)
                    .HasColumnName("ChiefAccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ChiefNote).HasMaxLength(500);

                entity.Property(e => e.ChiefStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CsdateTripSettlement)
                    .HasColumnName("CSDateTripSettlement")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.CsidtripSettlement)
                    .HasColumnName("CSIDTripSettlement")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CsstatusTripSettlement)
                    .HasColumnName("CSStatusTripSettlement")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.InvoiceNo).HasMaxLength(50);

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.ObjectBePaid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanDate)
                    .HasColumnName("OPSManDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.OpsmanId)
                    .HasColumnName("OPSManID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanNote)
                    .HasColumnName("OPSManNote")
                    .HasMaxLength(500);

                entity.Property(e => e.OpsmanStatus)
                    .HasColumnName("OPSManStatus")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");

                entity.Property(e => e.PaymentRefNo).HasMaxLength(50);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReSignedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ReSignedUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReceivingPlace)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Remain).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.SupplierId)
                    .HasColumnName("SupplierID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TransportRequestId).HasColumnName("TransportRequestID");

                entity.Property(e => e.TripSettlementCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsOrderDetailExpense>(entity =>
            {
                entity.ToTable("csOrderDetailExpense", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountantDate).HasColumnType("smalldatetime");

                entity.Property(e => e.AccountantId)
                    .HasColumnName("AccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantNote).HasMaxLength(500);

                entity.Property(e => e.AccountantStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChargeId)
                    .IsRequired()
                    .HasColumnName("ChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefAccountantId)
                    .HasColumnName("ChiefAccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ChiefNote).HasMaxLength(500);

                entity.Property(e => e.ChiefStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CsdateTripSettlement)
                    .HasColumnName("CSDateTripSettlement")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.CsidtripSettlement)
                    .HasColumnName("CSIDTripSettlement")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CsstatusTripSettlement)
                    .HasColumnName("CSStatusTripSettlement")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.InvoiceNo).HasMaxLength(250);

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.ObjectBePaid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanDate)
                    .HasColumnName("OPSManDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.OpsmanId)
                    .HasColumnName("OPSManID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanNote)
                    .HasColumnName("OPSManNote")
                    .HasMaxLength(500);

                entity.Property(e => e.OpsmanStatus)
                    .HasColumnName("OPSManStatus")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");

                entity.Property(e => e.PaymentObjectId)
                    .HasColumnName("PaymentObjectID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentRefNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReSignedDate).HasColumnType("datetime");

                entity.Property(e => e.ReSignedUser).HasMaxLength(50);

                entity.Property(e => e.Remain).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.Tariff).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransportRequestId).HasColumnName("TransportRequestID");

                entity.Property(e => e.TripSettlementCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsOrderDetailShipmentNote>(entity =>
            {
                entity.ToTable("csOrderDetailShipmentNote", "lcl");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");

                entity.Property(e => e.ShipmentNoteId).HasColumnName("ShipmentNoteID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsOrderDetailShortTrip>(entity =>
            {
                entity.ToTable("csOrderDetailShortTrip", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");

                entity.Property(e => e.PlaceFromId).HasColumnName("PlaceFromID");

                entity.Property(e => e.PlaceToId).HasColumnName("PlaceToID");

                entity.Property(e => e.RateCardConditionId).HasColumnName("RateCardConditionID");

                entity.Property(e => e.RoadId)
                    .IsRequired()
                    .HasColumnName("RoadID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsOrderDetailSurcharge>(entity =>
            {
                entity.ToTable("csOrderDetailSurcharge", "lcl");

                entity.HasIndex(e => new { e.OrderDetailId, e.ChargeId, e.TransportRequestId })
                    .HasName("U_csOrderDetailSurcharge")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ChargeId)
                    .IsRequired()
                    .HasColumnName("ChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.InvoiceNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.TransportRequestId).HasColumnName("TransportRequestID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsOrderDetailVoucher>(entity =>
            {
                entity.ToTable("csOrderDetailVoucher", "lcl");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Description).HasMaxLength(300);

                entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsOrderHeader>(entity =>
            {
                entity.ToTable("csOrderHeader", "lcl");

                entity.HasIndex(e => e.OrderCode)
                    .HasName("U_csOrderHeader_Code")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BillingNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.CargoValue).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ChargeTypeId).HasColumnName("ChargeTypeID");

                entity.Property(e => e.ContactPersonId)
                    .HasColumnName("ContactPersonID")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CurrentStatusId).HasColumnName("CurrentStatusID");

                entity.Property(e => e.CustomerDebitId)
                    .HasColumnName("CustomerDebitID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.InvoiceAddress).HasMaxLength(50);

                entity.Property(e => e.InvoiceCountryId).HasColumnName("InvoiceCountryID");

                entity.Property(e => e.InvoiceDistrictId).HasColumnName("InvoiceDistrictID");

                entity.Property(e => e.InvoicePostalCode).HasMaxLength(50);

                entity.Property(e => e.InvoiceProvinceId).HasColumnName("InvoiceProvinceID");

                entity.Property(e => e.Mawb)
                    .HasColumnName("MAWB")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.OrderCode)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OriginBranchId).HasColumnName("OriginBranchID");

                entity.Property(e => e.OriginHubId).HasColumnName("OriginHubID");

                entity.Property(e => e.ReceivedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.RefNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Remarks).HasMaxLength(300);

                entity.Property(e => e.RoadId)
                    .IsRequired()
                    .HasColumnName("RoadID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.SalePersonId)
                    .HasColumnName("SalePersonID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceTypeMappingId).HasColumnName("ServiceTypeMappingID");

                entity.Property(e => e.TotaIncludeVat)
                    .HasColumnName("TotaIncludeVAT")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalExcludeSurcharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalExcludeVat)
                    .HasColumnName("TotalExcludeVAT")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalSurcharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Vat)
                    .HasColumnName("VAT")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Vatinclude).HasColumnName("VATInclude");

                entity.Property(e => e.Vatrate).HasColumnName("VATRate");

                entity.Property(e => e.Version)
                    .IsRequired()
                    .IsRowVersion();
            });

            modelBuilder.Entity<CsOrderItemDetail>(entity =>
            {
                entity.ToTable("csOrderItemDetail", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ActualChargeableWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ActualHeight).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.ActualLength).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.ActualVolume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ActualWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ActualWidth).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.BaggedVolume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.BaggedWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DestBranchRemark).HasMaxLength(250);

                entity.Property(e => e.DestHubRemark).HasMaxLength(250);

                entity.Property(e => e.EstimateChargeableWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.EstimateHeight).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.EstimateLength).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.EstimateVolume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.EstimateWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.EstimateWidth).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.ItemDescription).HasMaxLength(255);

                entity.Property(e => e.Model).HasMaxLength(50);

                entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");

                entity.Property(e => e.OriginHubRemark).HasMaxLength(250);

                entity.Property(e => e.ReProtectedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ReProtectedUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Remark).HasMaxLength(250);

                entity.Property(e => e.ReweighedBy)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ReweighedOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Sku)
                    .HasColumnName("SKU")
                    .HasMaxLength(50);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Version)
                    .IsRequired()
                    .IsRowVersion();
            });

            modelBuilder.Entity<CsOrderSurcharge>(entity =>
            {
                entity.ToTable("csOrderSurcharge", "dtb");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ChargeId)
                    .IsRequired()
                    .HasColumnName("ChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.InvoiceNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.OrderId).HasColumnName("OrderID");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.TransportRequestId).HasColumnName("TransportRequestID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsReceptacleChecking>(entity =>
            {
                entity.ToTable("csReceptacleChecking", "lcl");

                entity.HasIndex(e => new { e.ReceptacleMasterId, e.CheckedLocation, e.CheckingType })
                    .HasName("U_ReceptacleChecking")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CheckedOn).HasColumnType("smalldatetime");

                entity.Property(e => e.CheckedUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CheckingType)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ReceptacleMasterId).HasColumnName("ReceptacleMasterID");

                entity.HasOne(d => d.CheckedLocationNavigation)
                    .WithMany(p => p.CsReceptacleChecking)
                    .HasForeignKey(d => d.CheckedLocation)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ReceptacleChecking_Place");

                entity.HasOne(d => d.ReceptacleMaster)
                    .WithMany(p => p.CsReceptacleChecking)
                    .HasForeignKey(d => d.ReceptacleMasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ReceptacleChecking_ReceptacleMaster");
            });

            modelBuilder.Entity<CsReceptacleMaster>(entity =>
            {
                entity.ToTable("csReceptacleMaster", "lcl");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DestinationPlaceId).HasColumnName("DestinationPlaceID");

                entity.Property(e => e.Height).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Length).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.OriginPlaceId).HasColumnName("OriginPlaceID");

                entity.Property(e => e.ReceptacleParentId).HasColumnName("ReceptacleParentID");

                entity.Property(e => e.ReceptacleTypeId).HasColumnName("ReceptacleTypeID");

                entity.Property(e => e.Remark).HasMaxLength(500);

                entity.Property(e => e.SealNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UnbaggedBy).HasMaxLength(50);

                entity.Property(e => e.UnbaggedOn).HasColumnType("smalldatetime");

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Volume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.VolumeFromMeasure).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Weight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Width).HasColumnType("decimal(8, 3)");

                entity.HasOne(d => d.ReceptacleType)
                    .WithMany(p => p.CsReceptacleMaster)
                    .HasForeignKey(d => d.ReceptacleTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_csReceptacleMaster_catReceptacleType");
            });

            modelBuilder.Entity<CsReceptacleOrderDetail>(entity =>
            {
                entity.ToTable("csReceptacleOrderDetail", "lcl");

                entity.HasIndex(e => new { e.ReceptacleMasterId, e.OrderItemId, e.SortCode })
                    .HasName("U_ReceptacleOrderDetail")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.BarCode)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");

                entity.Property(e => e.OrderItemId).HasColumnName("OrderItemID");

                entity.Property(e => e.ReceptacleMasterId).HasColumnName("ReceptacleMasterID");

                entity.Property(e => e.Remark).HasMaxLength(300);

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Volume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Weight).HasColumnType("decimal(18, 4)");
            });

            modelBuilder.Entity<CsShipmentChecking>(entity =>
            {
                entity.ToTable("csShipmentChecking", "lcl");

                entity.HasIndex(e => new { e.OrderDetailId, e.CheckedLocation, e.CheckingType })
                    .HasName("U_ShipmentChecking")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CheckedOn).HasColumnType("smalldatetime");

                entity.Property(e => e.CheckedUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CheckingType)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");
            });

            modelBuilder.Entity<CsTransportSurcharge>(entity =>
            {
                entity.ToTable("csTransportSurcharge", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountantDate).HasColumnType("smalldatetime");

                entity.Property(e => e.AccountantId)
                    .HasColumnName("AccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantNote).HasMaxLength(500);

                entity.Property(e => e.AccountantStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChargeId)
                    .IsRequired()
                    .HasColumnName("ChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefAccountantId)
                    .HasColumnName("ChiefAccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ChiefNote).HasMaxLength(500);

                entity.Property(e => e.ChiefStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CsdateTripSettlement)
                    .HasColumnName("CSDateTripSettlement")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.CsidtripSettlement)
                    .HasColumnName("CSIDTripSettlement")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CsstatusTripSettlement)
                    .HasColumnName("CSStatusTripSettlement")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.InvoiceNo).HasMaxLength(250);

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.ObjectBePaid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanDate)
                    .HasColumnName("OPSManDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.OpsmanId)
                    .HasColumnName("OPSManID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanNote)
                    .HasColumnName("OPSManNote")
                    .HasMaxLength(500);

                entity.Property(e => e.OpsmanStatus)
                    .HasColumnName("OPSManStatus")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentObjectId)
                    .HasColumnName("PaymentObjectID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentRefNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReSignedDate).HasColumnType("datetime");

                entity.Property(e => e.ReSignedUser).HasMaxLength(50);

                entity.Property(e => e.Remain).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.Tariff).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransportId).HasColumnName("TransportID");

                entity.Property(e => e.TripSettlementCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CsTransportTripRecord>(entity =>
            {
                entity.ToTable("csTransportTripRecord", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Address).HasMaxLength(300);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Description).HasMaxLength(300);

                entity.Property(e => e.FuelConsumption).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelLiter).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.GeoCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Notes).HasMaxLength(300);

                entity.Property(e => e.RouteType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SealNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalFuelCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransportId).HasColumnName("TransportID");

                entity.Property(e => e.TripDatetime).HasColumnType("smalldatetime");

                entity.Property(e => e.TripLengthCs).HasColumnName("TripLengthCS");

                entity.Property(e => e.TripLengthGps).HasColumnName("TripLengthGPS");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Weight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.WeightReal).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<MainMaintenancePlan>(entity =>
            {
                entity.ToTable("mainMaintenancePlan");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ChiefApprovedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ChiefApprovedId)
                    .HasColumnName("ChiefApprovedID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefApprovedNote).HasMaxLength(500);

                entity.Property(e => e.ChiefApprovedStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.HeadApprovedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.HeadApprovedId)
                    .HasColumnName("HeadApprovedID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.HeadApprovedNote).HasMaxLength(500);

                entity.Property(e => e.HeadApprovedStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastReplacedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.LastReplacedQuantity).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.MaintenancePlanMasterId).HasColumnName("MaintenancePlanMasterID");

                entity.Property(e => e.Note).HasMaxLength(500);

                entity.Property(e => e.OpsapprovedDate)
                    .HasColumnName("OPSApprovedDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.OpsapprovedId)
                    .HasColumnName("OPSApprovedID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsapprovedNote)
                    .HasColumnName("OPSApprovedNote")
                    .HasMaxLength(500);

                entity.Property(e => e.OpsapprovedStatus)
                    .HasColumnName("OPSApprovedStatus")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PlanDatetime).HasColumnType("smalldatetime");

                entity.Property(e => e.RepairType)
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

                entity.Property(e => e.VehicleId).HasColumnName("VehicleID");

                entity.Property(e => e.VehiclePartId).HasColumnName("VehiclePartID");
            });

            modelBuilder.Entity<MainMaintenanceQuota>(entity =>
            {
                entity.ToTable("mainMaintenanceQuota");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Deviation).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehiclePartId).HasColumnName("VehiclePartID");

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<MainMaintenanceQuotaDetail>(entity =>
            {
                entity.ToTable("mainMaintenanceQuotaDetail");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.MaintenanceQuotaId).HasColumnName("MaintenanceQuotaID");

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");
            });

            modelBuilder.Entity<MainMrrequest>(entity =>
            {
                entity.ToTable("mainMRRequest");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ApprovedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ApprovedManager)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApprovedNotes).HasMaxLength(500);

                entity.Property(e => e.CheckedFuelDate).HasColumnType("datetime");

                entity.Property(e => e.CheckedFuelNote).HasMaxLength(1000);

                entity.Property(e => e.CheckedFuelUser).HasMaxLength(50);

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DriverId).HasColumnName("DriverID");

                entity.Property(e => e.FinishedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.FuelConsumption).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.MaintenancePlaceId).HasColumnName("MaintenancePlaceID");

                entity.Property(e => e.MaintenanceTypeId).HasColumnName("MaintenanceTypeID");

                entity.Property(e => e.PoEnd).HasMaxLength(250);

                entity.Property(e => e.PoStart).HasMaxLength(250);

                entity.Property(e => e.Reason).HasMaxLength(500);

                entity.Property(e => e.Remark).HasMaxLength(500);

                entity.Property(e => e.RemoocId).HasColumnName("RemoocID");

                entity.Property(e => e.RequestedDate).HasColumnType("datetime");

                entity.Property(e => e.RequestedVehicleType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SentSms).HasColumnName("SentSMS");

                entity.Property(e => e.TotalFuelLiter).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalFuelPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalLengthGps)
                    .HasColumnName("TotalLengthGPS")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated).HasMaxLength(50);

                entity.Property(e => e.UserModified).HasMaxLength(50);

                entity.Property(e => e.VehicleId).HasColumnName("VehicleID");

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<MainMrrequestDetail>(entity =>
            {
                entity.ToTable("mainMRRequestDetail");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.LastAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.LastReplacedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.LastReplacedQuantity).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.LengthKm).HasColumnName("LengthKM");

                entity.Property(e => e.MrrequestId).HasColumnName("MRRequestID");

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Remark).HasMaxLength(500);

                entity.Property(e => e.RepairType).HasMaxLength(50);

                entity.Property(e => e.Serials).HasMaxLength(500);

                entity.Property(e => e.Tariff).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehiclePartId).HasColumnName("VehiclePartID");
            });

            modelBuilder.Entity<MainMrrequestPartDetail>(entity =>
            {
                entity.ToTable("mainMRRequestPartDetail");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.MrrequestDetaiId).HasColumnName("MRRequestDetaiID");

                entity.Property(e => e.NewSerial)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Note).HasMaxLength(300);

                entity.Property(e => e.OldSerial)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<MainMrrequestTripRecord>(entity =>
            {
                entity.ToTable("mainMRRequestTripRecord");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeTripUpdate).HasColumnType("smalldatetime");

                entity.Property(e => e.FuelAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.MrrequestId).HasColumnName("MRRequestID");

                entity.Property(e => e.Notes).HasMaxLength(300);

                entity.Property(e => e.Place)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.TotalFuelPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<MainReplacedVehiclePartStatus>(entity =>
            {
                entity.ToTable("mainReplacedVehiclePartStatus");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.ReplacedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ReplacedQuatity).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleId).HasColumnName("VehicleID");

                entity.Property(e => e.VehiclePartId).HasColumnName("VehiclePartID");
            });

            modelBuilder.Entity<MainVehicleMaintenance>(entity =>
            {
                entity.ToTable("mainVehicleMaintenance");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ApprovedDatetime).HasColumnType("smalldatetime");

                entity.Property(e => e.ApprovedManager)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BillNo).HasMaxLength(50);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.FinishedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.MaintenanceTypeId).HasColumnName("MaintenanceTypeID");

                entity.Property(e => e.MrrequestId).HasColumnName("MRRequestID");

                entity.Property(e => e.Remark).HasMaxLength(500);

                entity.Property(e => e.RepairLevelId).HasColumnName("RepairLevelID");

                entity.Property(e => e.RequestedVehicleType)
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

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<MainVehicleMaintenanceMasterPlan>(entity =>
            {
                entity.ToTable("mainVehicleMaintenanceMasterPlan");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Code).HasMaxLength(50);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NamePlan)
                    .IsRequired()
                    .HasMaxLength(250);

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

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<MainVehicleMaintenancePlace>(entity =>
            {
                entity.ToTable("mainVehicleMaintenancePlace");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ContactName).HasMaxLength(50);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NameVn)
                    .IsRequired()
                    .HasColumnName("Name_VN")
                    .HasMaxLength(100);

                entity.Property(e => e.Tel).HasMaxLength(50);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<MainVehicleMaintenanceType>(entity =>
            {
                entity.ToTable("mainVehicleMaintenanceType");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code).HasMaxLength(50);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<MainVehicleRepairLevel>(entity =>
            {
                entity.ToTable("mainVehicleRepairLevel");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Note).HasMaxLength(255);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<MainYearlyCostEstimation>(entity =>
            {
                entity.ToTable("mainYearlyCostEstimation");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ChiefApprovedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ChiefApprovedId)
                    .HasColumnName("ChiefApprovedID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefApprovedNote).HasMaxLength(500);

                entity.Property(e => e.ChiefApprovedStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.HeadApprovedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.HeadApprovedId)
                    .HasColumnName("HeadApprovedID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.HeadApprovedNote).HasMaxLength(500);

                entity.Property(e => e.HeadApprovedStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.MaintenancePlanMasterId).HasColumnName("MaintenancePlanMasterID");

                entity.Property(e => e.Note).HasMaxLength(500);

                entity.Property(e => e.OpsapprovedDate)
                    .HasColumnName("OPSApprovedDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.OpsapprovedId)
                    .HasColumnName("OPSApprovedID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsapprovedNote)
                    .HasColumnName("OPSApprovedNote")
                    .HasMaxLength(500);

                entity.Property(e => e.OpsapprovedStatus)
                    .HasColumnName("OPSApprovedStatus")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehiclePartId).HasColumnName("VehiclePartID");

                entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");
            });

            modelBuilder.Entity<OpsDtbhireTransportRequestApproval>(entity =>
            {
                entity.ToTable("opsDTBHireTransportRequestApproval", "dtb");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BuyingPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.HeadDate).HasColumnType("smalldatetime");

                entity.Property(e => e.HeadId)
                    .HasColumnName("HeadID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.HeadNotes).HasMaxLength(800);

                entity.Property(e => e.HeadStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NotesCs)
                    .HasColumnName("NotesCS")
                    .HasMaxLength(800);

                entity.Property(e => e.PaymentRefNo)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RouteId).HasColumnName("RouteID");

                entity.Property(e => e.SupplierId)
                    .HasColumnName("SupplierID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalSurcharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransportId).HasColumnName("TransportID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<OpsDtbtransportRequest>(entity =>
            {
                entity.ToTable("opsDTBTransportRequest", "dtb");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AllowancePaidDatetime).HasColumnType("smalldatetime");

                entity.Property(e => e.AllowancePaidUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApprovedCancelBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApprovedCancelNote).HasMaxLength(500);

                entity.Property(e => e.ApprovedCancelStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ArisedRequestNote).HasMaxLength(500);

                entity.Property(e => e.BuyingRouteId).HasColumnName("BuyingRouteID");

                entity.Property(e => e.ClosedTime).HasColumnType("smalldatetime");

                entity.Property(e => e.ClosedUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ConfirmationStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ConfirmedOpsman)
                    .HasColumnName("ConfirmedOPSMan")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContainerTypeId)
                    .HasColumnName("ContainerTypeID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Csappoved)
                    .HasColumnName("CSAppoved")
                    .HasMaxLength(50);

                entity.Property(e => e.CsapprovedDate)
                    .HasColumnName("CSApprovedDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.CsapprovedStatus)
                    .HasColumnName("CSApprovedStatus")
                    .HasMaxLength(50);

                entity.Property(e => e.DateApprovedCancel).HasColumnType("smalldatetime");

                entity.Property(e => e.DateOpsmanConfirmed)
                    .HasColumnName("DateOPSManConfirmed")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeCheckedFuel).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DriverId).HasColumnName("DriverID");

                entity.Property(e => e.DriverRole)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.EndKm).HasColumnName("EndKM");

                entity.Property(e => e.FclbuyingId).HasColumnName("FCLBuyingID");

                entity.Property(e => e.FinishedTime).HasColumnType("smalldatetime");

                entity.Property(e => e.FixedCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelConsumption).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.FuelCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelPaymentId).HasColumnName("FuelPaymentID");

                entity.Property(e => e.Gsanote)
                    .HasColumnName("GSANote")
                    .HasMaxLength(500);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.LenghtGps)
                    .HasColumnName("LenghtGPS")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.LengthInTariff).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Mnrcost)
                    .HasColumnName("MNRCost")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Opsapproved)
                    .HasColumnName("OPSApproved")
                    .HasMaxLength(50);

                entity.Property(e => e.OpsapprovedDate)
                    .HasColumnName("OPSApprovedDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.OpsapprovedStatus)
                    .HasColumnName("OPSApprovedStatus")
                    .HasMaxLength(50);

                entity.Property(e => e.OverheadCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PrintedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.PrintedUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RefNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Remark).HasMaxLength(800);

                entity.Property(e => e.RemoocId).HasColumnName("RemoocID");

                entity.Property(e => e.RequestedForDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ResponsibleWorkPlaceId).HasColumnName("ResponsibleWorkPlaceID");

                entity.Property(e => e.RouteId).HasColumnName("RouteID");

                entity.Property(e => e.SealNumber).HasMaxLength(150);

                entity.Property(e => e.SentSms).HasColumnName("SentSMS");

                entity.Property(e => e.StartKm).HasColumnName("StartKM");

                entity.Property(e => e.SupplierId)
                    .HasColumnName("SupplierID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalActualVolume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalActualWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalCharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalCod)
                    .HasColumnName("TotalCOD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalFuelLiter).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalLenght).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.TotalLengthCs).HasColumnName("TotalLengthCS");

                entity.Property(e => e.TotalWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TripBuyingRouteId).HasColumnName("TripBuyingRouteID");

                entity.Property(e => e.TripSettlementCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCheckedFuel)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleId).HasColumnName("VehicleID");

                entity.Property(e => e.WorkingDay).HasColumnType("decimal(8, 3)");
            });

            modelBuilder.Entity<OpsDtbtransportRequestOrderItemRoute>(entity =>
            {
                entity.ToTable("opsDTBTransportRequestOrderItemRoute", "dtb");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.CheckingInRemark).HasMaxLength(500);

                entity.Property(e => e.Codvalue)
                    .HasColumnName("CODValue")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Collection).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CustomerSignature).HasColumnType("image");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DeliveredActualVolume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DeliveredActualWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DeliveryArrivedTime).HasColumnType("smalldatetime");

                entity.Property(e => e.DeliveryLeftTime).HasColumnType("smalldatetime");

                entity.Property(e => e.DeliveryStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DropDownSurcharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FailedDeliveryDueTo).HasMaxLength(50);

                entity.Property(e => e.FailedDeliveryReason).HasMaxLength(250);

                entity.Property(e => e.IndicatedCodvalue)
                    .HasColumnName("IndicatedCODValue")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Note).HasMaxLength(500);

                entity.Property(e => e.OrderDropPointItemRouteId).HasColumnName("OrderDropPointItemRouteID");

                entity.Property(e => e.PickedUpActualVolume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PickedUpActualWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PickupArrivedTime).HasColumnType("smalldatetime");

                entity.Property(e => e.PickupLeftTime).HasColumnType("smalldatetime");

                entity.Property(e => e.PodreceivedDate)
                    .HasColumnName("PODReceivedDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.PodreturnedDate)
                    .HasColumnName("PODReturnedDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.Rating).HasColumnType("decimal(5, 3)");

                entity.Property(e => e.ShipmentStatusId).HasColumnName("ShipmentStatusID");

                entity.Property(e => e.TransportRequestId).HasColumnName("TransportRequestID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Weight).HasColumnType("decimal(18, 4)");

                entity.HasOne(d => d.TransportRequest)
                    .WithMany(p => p.OpsDtbtransportRequestOrderItemRoute)
                    .HasForeignKey(d => d.TransportRequestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_opsDTBTransportRequestOrderItemRoute_opsDTBTransportRequest");
            });

            modelBuilder.Entity<OpsFclhireTransportRequestApproval>(entity =>
            {
                entity.ToTable("opsFCLHireTransportRequestApproval", "fcl");

                entity.HasIndex(e => new { e.TransportId, e.PaymentRefNo })
                    .HasName("U_opsFCLHireTransportRequestApproval")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BuyingPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.HeadDate).HasColumnType("smalldatetime");

                entity.Property(e => e.HeadId)
                    .HasColumnName("HeadID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.HeadNotes).HasMaxLength(800);

                entity.Property(e => e.HeadStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NotesCs)
                    .HasColumnName("NotesCS")
                    .HasMaxLength(800);

                entity.Property(e => e.PaymentRefNo)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RouteId).HasColumnName("RouteID");

                entity.Property(e => e.SupplierId)
                    .HasColumnName("SupplierID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalSurcharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransportId).HasColumnName("TransportID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<OpsFclmasterTransportRequest>(entity =>
            {
                entity.ToTable("opsFCLMasterTransportRequest", "fcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AdjustedNotes).HasMaxLength(800);

                entity.Property(e => e.AdjustedPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.AdjustedSoadate)
                    .HasColumnName("AdjustedSOADate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.AdjustedSoauser)
                    .HasColumnName("AdjustedSOAUser")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AdjustmentRequestDate).HasColumnType("smalldatetime");

                entity.Property(e => e.AdjustmentRequestor)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApprovedAdjustmentBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApprovedAdjustmentDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ApprovedAdjustmentNote).HasMaxLength(500);

                entity.Property(e => e.ApprovedAdjustmentStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApprovedCancelBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApprovedCancelNote).HasMaxLength(500);

                entity.Property(e => e.ApprovedCancelStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Awb)
                    .HasColumnName("AWB")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.BillingNo)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.BookingDetailId).HasColumnName("BookingDetailID");

                entity.Property(e => e.CancelReason).HasMaxLength(500);

                entity.Property(e => e.ClosedSoa).HasColumnName("ClosedSOA");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Codvalue)
                    .HasColumnName("CODValue")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ConfirmationStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ConfirmedOpsman)
                    .HasColumnName("ConfirmedOPSMan")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContNumber).HasMaxLength(50);

                entity.Property(e => e.DateApprovedCancel).HasColumnType("smalldatetime");

                entity.Property(e => e.DateOpsmanConfirmed)
                    .HasColumnName("DateOPSManConfirmed")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DeliveryStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ExportedSoa).HasColumnName("ExportedSOA");

                entity.Property(e => e.FailedDeliveryDueTo).HasMaxLength(50);

                entity.Property(e => e.FailedDeliveryReason).HasMaxLength(250);

                entity.Property(e => e.FixedCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelConsumption).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.FuelCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Gpslenght)
                    .HasColumnName("GPSLenght")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.LonghualShipmentCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Mnrcost)
                    .HasColumnName("MNRCost")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OverheadCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PlaceFromId).HasColumnName("PlaceFromID");

                entity.Property(e => e.PlaceToId).HasColumnName("PlaceToID");

                entity.Property(e => e.PodhanoverRequestDate)
                    .HasColumnName("PODHanoverRequestDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.PodleadTime).HasColumnName("PODLeadTime");

                entity.Property(e => e.PodreceivedDate)
                    .HasColumnName("PODReceivedDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.PodreturnedDate)
                    .HasColumnName("PODReturnedDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.Remark).HasMaxLength(800);

                entity.Property(e => e.RequestedForDate).HasColumnType("smalldatetime");

                entity.Property(e => e.SealNumber).HasMaxLength(50);

                entity.Property(e => e.SellingCurrencyId)
                    .HasColumnName("SellingCurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.SellingPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.SoaadjustmentReason)
                    .HasColumnName("SOAAdjustmentReason")
                    .HasMaxLength(500);

                entity.Property(e => e.SoaadjustmentRequestedDate)
                    .HasColumnName("SOAAdjustmentRequestedDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.SoaadjustmentRequestor)
                    .HasColumnName("SOAAdjustmentRequestor")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Soaclosed).HasColumnName("SOAClosed");

                entity.Property(e => e.Soaid).HasColumnName("SOAID");

                entity.Property(e => e.Soano)
                    .HasColumnName("SOANo")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalCharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalEmptyContLength).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.TotalFuelLiter).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalLenght).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.TotalProfit).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalRevenue).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalWeightReal).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnlockedSoadirector)
                    .HasColumnName("UnlockedSOADirector")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UnlockedSoadirectorDate)
                    .HasColumnName("UnlockedSOADirectorDate")
                    .HasColumnType("smalldatetime");

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
                    .HasColumnType("smalldatetime");

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

                entity.Property(e => e.WorkingDay).HasColumnType("decimal(8, 3)");
            });

            modelBuilder.Entity<OpsFcltransportRequest>(entity =>
            {
                entity.ToTable("opsFCLTransportRequest", "fcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AllowancePaidDatetime).HasColumnType("datetime");

                entity.Property(e => e.AllowancePaidUser).HasMaxLength(50);

                entity.Property(e => e.ApprovedCancelBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApprovedCancelNote).HasMaxLength(500);

                entity.Property(e => e.ApprovedCancelStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ArisedRequestNote).HasMaxLength(500);

                entity.Property(e => e.BarCode).HasColumnType("image");

                entity.Property(e => e.BillingCompanyId)
                    .HasColumnName("BillingCompanyID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CancelReason).HasMaxLength(500);

                entity.Property(e => e.ChangedRouteTypeNote).HasMaxLength(800);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Codvalue)
                    .HasColumnName("CODValue")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ConfirmationStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ConfirmedOpsman)
                    .HasColumnName("ConfirmedOPSMan")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContAssociationCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContNumber).HasMaxLength(50);

                entity.Property(e => e.ContactName).HasMaxLength(50);

                entity.Property(e => e.ContactPhone).HasMaxLength(15);

                entity.Property(e => e.Csapproved)
                    .HasColumnName("CSApproved")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CsapprovedDate)
                    .HasColumnName("CSApprovedDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.CsapprovedStatus)
                    .HasColumnName("CSApprovedStatus")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerSignature).HasColumnType("image");

                entity.Property(e => e.DateApprovedCancel).HasColumnType("smalldatetime");

                entity.Property(e => e.DateOpsmanConfirmed)
                    .HasColumnName("DateOPSManConfirmed")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeCheckedFuel).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DeliveryDate).HasColumnType("smalldatetime");

                entity.Property(e => e.DestinationWorkPlaceId).HasColumnName("DestinationWorkPlaceID");

                entity.Property(e => e.DriverId).HasColumnName("DriverID");

                entity.Property(e => e.DriverRole)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.EndKm).HasColumnName("EndKM");

                entity.Property(e => e.EstimatedLiterFuel).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.FclbuyingId).HasColumnName("FCLBuyingID");

                entity.Property(e => e.FinishedTime).HasColumnType("smalldatetime");

                entity.Property(e => e.FixedCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelConsumption).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.FuelCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Gpslength)
                    .HasColumnName("GPSLength")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Gsanote)
                    .HasColumnName("GSANote")
                    .HasMaxLength(500);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.IndicatedCodvalue)
                    .HasColumnName("IndicatedCODValue")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.LonghualShipmentCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.MasterTransportRequestId).HasColumnName("MasterTransportRequestID");

                entity.Property(e => e.Mnrcost)
                    .HasColumnName("MNRCost")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Opsapproved)
                    .HasColumnName("OPSApproved")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsapprovedDate)
                    .HasColumnName("OPSApprovedDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.OpsapprovedStatus)
                    .HasColumnName("OPSApprovedStatus")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OverheadCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PodhanoverRequestDate)
                    .HasColumnName("PODHanoverRequestDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.PodreturnedDate)
                    .HasColumnName("PODReturnedDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.PrintedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.PrintedUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.QuotationAssociationCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.QuotationShortTripId).HasColumnName("QuotationShortTripID");

                entity.Property(e => e.Rating).HasColumnType("decimal(5, 3)");

                entity.Property(e => e.ReceiptDate).HasColumnType("smalldatetime");

                entity.Property(e => e.RefNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Remark).HasMaxLength(800);

                entity.Property(e => e.RemoocId).HasColumnName("RemoocID");

                entity.Property(e => e.RequestedForDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ResponsibleWorkplaceId).HasColumnName("ResponsibleWorkplaceID");

                entity.Property(e => e.RouteId).HasColumnName("RouteID");

                entity.Property(e => e.RouteType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SealNumber).HasMaxLength(50);

                entity.Property(e => e.SentSms).HasColumnName("SentSMS");

                entity.Property(e => e.StartKm).HasColumnName("StartKM");

                entity.Property(e => e.SupplierId)
                    .HasColumnName("SupplierID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalCharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalEmptyContLength).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.TotalFuelLiter).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalLenght).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.TotalProfit).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalWeightReal).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransferDate).HasColumnType("smalldatetime");

                entity.Property(e => e.TripBuyingRouteId).HasColumnName("TripBuyingRouteID");

                entity.Property(e => e.TripSettlementCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCheckedFuel)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleId).HasColumnName("VehicleID");

                entity.Property(e => e.WorkingDay).HasColumnType("decimal(8, 3)");

                entity.HasOne(d => d.MasterTransportRequest)
                    .WithMany(p => p.OpsFcltransportRequest)
                    .HasForeignKey(d => d.MasterTransportRequestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_opsFCLTransportRequest_opsFCLMasterTransportRequest");
            });

            modelBuilder.Entity<OpsHireTransportRequestApproval>(entity =>
            {
                entity.ToTable("opsHireTransportRequestApproval", "lcl");

                entity.HasIndex(e => new { e.TransportId, e.PaymentRefNo })
                    .HasName("U_opsHireTransportRequestApproval")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BuyingPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.HeadDate).HasColumnType("smalldatetime");

                entity.Property(e => e.HeadId)
                    .HasColumnName("HeadID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.HeadNotes).HasMaxLength(800);

                entity.Property(e => e.HeadStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NotesCs)
                    .HasColumnName("NotesCS")
                    .HasMaxLength(800);

                entity.Property(e => e.PaymentRefNo)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RouteId).HasColumnName("RouteID");

                entity.Property(e => e.SupplierId)
                    .HasColumnName("SupplierID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalSurcharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TransportId).HasColumnName("TransportID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<OpsOrderDetailTransportRequest>(entity =>
            {
                entity.ToTable("opsOrderDetailTransportRequest", "lcl");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ActualVolume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ActualWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CheckingInRemark).HasMaxLength(500);

                entity.Property(e => e.Codvalue)
                    .HasColumnName("CODValue")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Collection).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CustomerSignature).HasColumnType("image");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DeliveryStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DropDownSurcharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FailedDeliveryDueTo).HasMaxLength(50);

                entity.Property(e => e.FailedDeliveryReason).HasMaxLength(250);

                entity.Property(e => e.IndicatedCodvalue)
                    .HasColumnName("IndicatedCODValue")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Note).HasMaxLength(500);

                entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");

                entity.Property(e => e.PodreceivedDate)
                    .HasColumnName("PODReceivedDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.PodreturnedDate)
                    .HasColumnName("PODReturnedDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.Rating).HasColumnType("decimal(5, 3)");

                entity.Property(e => e.ShipmentStatusId).HasColumnName("ShipmentStatusID");

                entity.Property(e => e.TransportRequestId).HasColumnName("TransportRequestID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Weight).HasColumnType("decimal(18, 4)");
            });

            modelBuilder.Entity<OpsTransportRequest>(entity =>
            {
                entity.ToTable("opsTransportRequest", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AllowancePaidDatetime).HasColumnType("smalldatetime");

                entity.Property(e => e.AllowancePaidUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApprovedCancelBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApprovedCancelNote).HasMaxLength(500);

                entity.Property(e => e.ApprovedCancelStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ArisedRequestNote).HasMaxLength(500);

                entity.Property(e => e.ArrivedDestinationDate).HasColumnType("smalldatetime");

                entity.Property(e => e.BarCode).HasColumnType("image");

                entity.Property(e => e.BuyingRouteId).HasColumnName("BuyingRouteID");

                entity.Property(e => e.ClosedTime).HasColumnType("smalldatetime");

                entity.Property(e => e.ClosedUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ConfirmationStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ConfirmedOpsman)
                    .HasColumnName("ConfirmedOPSMan")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContNumber).HasMaxLength(50);

                entity.Property(e => e.ContactName).HasMaxLength(50);

                entity.Property(e => e.ContactPhone).HasMaxLength(15);

                entity.Property(e => e.ContainerTypeId)
                    .HasColumnName("ContainerTypeID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CostPerDefaultUnit).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Csappoved)
                    .HasColumnName("CSAppoved")
                    .HasMaxLength(50);

                entity.Property(e => e.CsapprovedDate)
                    .HasColumnName("CSApprovedDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.CsapprovedStatus)
                    .HasColumnName("CSApprovedStatus")
                    .HasMaxLength(50);

                entity.Property(e => e.DateApprovedCancel).HasColumnType("smalldatetime");

                entity.Property(e => e.DateOpsmanConfirmed)
                    .HasColumnName("DateOPSManConfirmed")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeCheckedFuel).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DriverId).HasColumnName("DriverID");

                entity.Property(e => e.DriverRole)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.EndKm).HasColumnName("EndKM");

                entity.Property(e => e.FclbuyingId).HasColumnName("FCLBuyingID");

                entity.Property(e => e.FinishedTime).HasColumnType("smalldatetime");

                entity.Property(e => e.FixedCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelConsumption).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.FuelCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelPaymentId).HasColumnName("FuelPaymentID");

                entity.Property(e => e.Gsanote)
                    .HasColumnName("GSANote")
                    .HasMaxLength(500);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.IsFclrequest).HasColumnName("IsFCLRequest");

                entity.Property(e => e.LenghtGps)
                    .HasColumnName("LenghtGPS")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.LengthInTariff).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.LonghualShipmentCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Mnrcost)
                    .HasColumnName("MNRCost")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Opsapproved)
                    .HasColumnName("OPSApproved")
                    .HasMaxLength(50);

                entity.Property(e => e.OpsapprovedDate)
                    .HasColumnName("OPSApprovedDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.OpsapprovedStatus)
                    .HasColumnName("OPSApprovedStatus")
                    .HasMaxLength(50);

                entity.Property(e => e.OverheadCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PoEnd).HasMaxLength(150);

                entity.Property(e => e.PoStart).HasMaxLength(150);

                entity.Property(e => e.PrintedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.PrintedUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RefNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Remark).HasMaxLength(800);

                entity.Property(e => e.RemoocId).HasColumnName("RemoocID");

                entity.Property(e => e.RequestType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.RequestedForDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ResponsibleWorkPlaceId).HasColumnName("ResponsibleWorkPlaceID");

                entity.Property(e => e.ReturnDate).HasColumnType("smalldatetime");

                entity.Property(e => e.RouteId).HasColumnName("RouteID");

                entity.Property(e => e.SealNumber).HasMaxLength(150);

                entity.Property(e => e.SentSms).HasColumnName("SentSMS");

                entity.Property(e => e.StartKm).HasColumnName("StartKM");

                entity.Property(e => e.SupplierId)
                    .HasColumnName("SupplierID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalActualWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalCharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalChargedWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalCod)
                    .HasColumnName("TotalCOD")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalCollection).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalFuelLiter).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalLenght).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.TotalLengthCs).HasColumnName("TotalLengthCS");

                entity.Property(e => e.TotalWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TripBuyingRouteId).HasColumnName("TripBuyingRouteID");

                entity.Property(e => e.TripSettlementCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCheckedFuel)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleId).HasColumnName("VehicleID");

                entity.Property(e => e.WorkingDay).HasColumnType("decimal(8, 3)");
            });

            modelBuilder.Entity<OpsTransportRequestImage>(entity =>
            {
                entity.ToTable("opsTransportRequestImage", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.FileCheckSum)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.FileDescription).HasMaxLength(250);

                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Image).HasColumnType("image");

                entity.Property(e => e.OrderDetailTrequestId).HasColumnName("OrderDetailTRequestID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<OpsTransportRequestReceptacle>(entity =>
            {
                entity.ToTable("opsTransportRequestReceptacle", "lcl");

                entity.HasIndex(e => new { e.TransportRequestId, e.ReceptacleMasterId })
                    .HasName("U_TransportRequestReceptacle")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CheckingInOrderReasonId).HasColumnName("CheckingInOrderReasonID");

                entity.Property(e => e.CheckingInRemark).HasMaxLength(500);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Note).HasMaxLength(500);

                entity.Property(e => e.ReceptacleMasterId).HasColumnName("ReceptacleMasterID");

                entity.Property(e => e.TransportRequestId).HasColumnName("TransportRequestID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<OpsTransportRequestType>(entity =>
            {
                entity.ToTable("opsTransportRequestType", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(100);

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<OpsUnlockTransportRequest>(entity =>
            {
                entity.ToTable("opsUnlockTransportRequest");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ChiefDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ChiefId)
                    .HasColumnName("ChiefID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefNote).HasMaxLength(500);

                entity.Property(e => e.ChiefStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.OpsmanDate)
                    .HasColumnName("OPSManDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.OpsmanId)
                    .HasColumnName("OPSManID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanNote)
                    .HasColumnName("OPSManNote")
                    .HasMaxLength(500);

                entity.Property(e => e.OpsmanStatus)
                    .HasColumnName("OPSManStatus")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShipmentType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TransportId).HasColumnName("TransportID");

                entity.Property(e => e.UnlockReason)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.UnlockType).HasMaxLength(50);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<OpsWawePickPlan>(entity =>
            {
                entity.ToTable("opsWawePickPlan");

                entity.HasIndex(e => new { e.SupplierId, e.Code })
                    .HasName("U_opsWawePickPlan")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Remark).HasMaxLength(800);

                entity.Property(e => e.RequestedForDate).HasColumnType("smalldatetime");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasColumnName("SupplierID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalVolume).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<OpsWawePickPlanItem>(entity =>
            {
                entity.ToTable("opsWawePickPlanItem");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.PlaceId).HasColumnName("PlaceID");

                entity.Property(e => e.Remark).HasMaxLength(800);

                entity.Property(e => e.TransportRequestItemId).HasColumnName("TransportRequestItemID");

                entity.Property(e => e.TransportRequestItemType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WawePickPlanId).HasColumnName("WawePickPlanID");

                entity.HasOne(d => d.TransportRequestItem)
                    .WithMany(p => p.OpsWawePickPlanItem)
                    .HasForeignKey(d => d.TransportRequestItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_opsWawePickPlanItem_opsDTBTransportRequestOrderItemRoute");

                entity.HasOne(d => d.WawePickPlan)
                    .WithMany(p => p.OpsWawePickPlanItem)
                    .HasForeignKey(d => d.WawePickPlanId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_opsWawePickPlanItem_opsWawePickPlan");
            });

            modelBuilder.Entity<PriceBuying>(entity =>
            {
                entity.ToTable("priceBuying", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AppliedCustomerType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.Contract).HasMaxLength(100);

                entity.Property(e => e.ContractId).HasColumnName("ContractID");

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerId)
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Desciption).HasMaxLength(250);

                entity.Property(e => e.EffectiveOn).HasColumnType("datetime");

                entity.Property(e => e.ExpiryOn).HasColumnType("datetime");

                entity.Property(e => e.GettingPriceMethod)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.ReferCustomerId)
                    .HasColumnName("ReferCustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReferCustomerPercent).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasColumnName("SupplierID")
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

                entity.Property(e => e.VolumnWeightRate).HasColumnType("decimal(18, 4)");
            });

            modelBuilder.Entity<PriceBuyingCustomer>(entity =>
            {
                entity.ToTable("priceBuyingCustomer");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BuyingId).HasColumnName("BuyingID");

                entity.Property(e => e.BuyingType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PriceBuyingDetail>(entity =>
            {
                entity.ToTable("priceBuyingDetail", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BuyingRouteId).HasColumnName("BuyingRouteID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.FromValue).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReferCustomerFee).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ToValue).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PriceBuyingOverWeightDetail>(entity =>
            {
                entity.ToTable("priceBuyingOverWeightDetail", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BuyingRouteId).HasColumnName("BuyingRouteID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Ladder).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReferCustomerFee).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PriceBuyingRoute>(entity =>
            {
                entity.ToTable("priceBuyingRoute", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BuyingId).HasColumnName("BuyingID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.RoadId)
                    .IsRequired()
                    .HasColumnName("RoadID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceTypeMappingId).HasColumnName("ServiceTypeMappingID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PriceBuyingRouteSurcharge>(entity =>
            {
                entity.ToTable("priceBuyingRouteSurcharge", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BuyingRouteId).HasColumnName("BuyingRouteID");

                entity.Property(e => e.ChargeId)
                    .IsRequired()
                    .HasColumnName("ChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PriceCost>(entity =>
            {
                entity.ToTable("priceCost", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Days).HasColumnType("decimal(5, 2)");

                entity.Property(e => e.DestinationBranchId).HasColumnName("DestinationBranchID");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Kratio).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.Note).HasMaxLength(800);

                entity.Property(e => e.OriginBranchId).HasColumnName("OriginBranchID");

                entity.Property(e => e.PickupCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ServiceTypeMappingId).HasColumnName("ServiceTypeMappingID");

                entity.Property(e => e.TransitCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.DestinationBranch)
                    .WithMany(p => p.PriceCostDestinationBranch)
                    .HasForeignKey(d => d.DestinationBranchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_priceCost_catDestinationBranch");

                entity.HasOne(d => d.OriginBranch)
                    .WithMany(p => p.PriceCostOriginBranch)
                    .HasForeignKey(d => d.OriginBranchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_priceCost_catOriginBranch");
            });

            modelBuilder.Entity<PriceCostDeliveryRoute>(entity =>
            {
                entity.ToTable("priceCostDeliveryRoute", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.CostId).HasColumnName("CostID");

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DeliveryPlaceId).HasColumnName("DeliveryPlaceID");

                entity.Property(e => e.DeliveryZoneId).HasColumnName("DeliveryZoneID");

                entity.Property(e => e.FixedCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelConsumption).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.FuelCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Kratio).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.Mnramount)
                    .HasColumnName("MNRAmount")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Notes).HasMaxLength(2000);

                entity.Property(e => e.OverheadAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalChargedWeight).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.TotalUnitCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnitCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");

                entity.Property(e => e.WeightPerShipment).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.WeightRangeId).HasColumnName("WeightRangeID");
            });

            modelBuilder.Entity<PriceCostDirectRoute>(entity =>
            {
                entity.ToTable("priceCostDirectRoute", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BuyingRouteId).HasColumnName("BuyingRouteID");

                entity.Property(e => e.Contract).HasMaxLength(100);

                entity.Property(e => e.CostZoneMappingId).HasColumnName("CostZoneMappingID");

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.FclbuyingId).HasColumnName("FCLBuyingID");

                entity.Property(e => e.FixedCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FromPlaceId).HasColumnName("FromPlaceID");

                entity.Property(e => e.FuelConsumption).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.FuelCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Kratio).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.Mnramount)
                    .HasColumnName("MNRAmount")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Notes).HasMaxLength(800);

                entity.Property(e => e.OverheadAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RoadId)
                    .HasColumnName("RoadID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.RouteId).HasColumnName("RouteID");

                entity.Property(e => e.SupplierId)
                    .HasColumnName("SupplierID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ToPlaceId).HasColumnName("ToPlaceID");

                entity.Property(e => e.TotalChargedWeight).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.TripBuyingRouteId).HasColumnName("TripBuyingRouteID");

                entity.Property(e => e.UnitCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");

                entity.Property(e => e.WeightRangeId).HasColumnName("WeightRangeID");

                entity.Property(e => e.WeightUnitId).HasColumnName("WeightUnitID");
            });

            modelBuilder.Entity<PriceCostPickupRoute>(entity =>
            {
                entity.ToTable("priceCostPickupRoute", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.CostId).HasColumnName("CostID");

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.FixedCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelConsumption).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.FuelCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Kratio).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.Mnramount)
                    .HasColumnName("MNRAmount")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Notes).HasMaxLength(2000);

                entity.Property(e => e.OverheadAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PickupPlaceId).HasColumnName("PickupPlaceID");

                entity.Property(e => e.PickupZoneId).HasColumnName("PickupZoneID");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalChargedWeight).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.UnitCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");

                entity.Property(e => e.WeightPerShipment).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.WeightRangeId).HasColumnName("WeightRangeID");
            });

            modelBuilder.Entity<PriceCostTransitRoute>(entity =>
            {
                entity.ToTable("priceCostTransitRoute", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BuyingRouteId).HasColumnName("BuyingRouteID");

                entity.Property(e => e.Contract).HasMaxLength(100);

                entity.Property(e => e.CostId).HasColumnName("CostID");

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.FclbuyingId).HasColumnName("FCLBuyingID");

                entity.Property(e => e.FclbuyingPriceId).HasColumnName("FCLBuyingPriceID");

                entity.Property(e => e.FixedCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelConsumption).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.FuelCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Kratio).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.LclbuyingPriceId).HasColumnName("LCLBuyingPriceID");

                entity.Property(e => e.Mnramount)
                    .HasColumnName("MNRAmount")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Notes).HasMaxLength(800);

                entity.Property(e => e.OverheadAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RoadId)
                    .IsRequired()
                    .HasColumnName("RoadID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SupplierId)
                    .HasColumnName("SupplierID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalChargedWeight).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.TripBuyingRouteId).HasColumnName("TripBuyingRouteID");

                entity.Property(e => e.UnitCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");

                entity.Property(e => e.WeightRangeId).HasColumnName("WeightRangeID");

                entity.Property(e => e.WeightUnitId).HasColumnName("WeightUnitID");
            });

            modelBuilder.Entity<PriceCostZoneMapping>(entity =>
            {
                entity.ToTable("priceCostZoneMapping", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.CostId).HasColumnName("CostID");

                entity.Property(e => e.DeliveryZoneId).HasColumnName("DeliveryZoneID");

                entity.Property(e => e.PickupZoneId).HasColumnName("PickupZoneID");

                entity.Property(e => e.UnitCost).HasColumnType("decimal(18, 4)");
            });

            modelBuilder.Entity<PriceCustomerRateCard>(entity =>
            {
                entity.HasKey(e => new { e.CustomerId, e.RateCardId });

                entity.ToTable("priceCustomerRateCard", "lcl");

                entity.Property(e => e.CustomerId)
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RateCardId).HasColumnName("RateCardID");
            });

            modelBuilder.Entity<PriceDtbrateCard>(entity =>
            {
                entity.ToTable("priceDTBRateCard", "dtb");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.ChiefAccountantApprovedMethod)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefAccountantApprovedOn).HasColumnType("smalldatetime");

                entity.Property(e => e.ChiefAccountantId)
                    .HasColumnName("ChiefAccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefAccountantNote).HasMaxLength(500);

                entity.Property(e => e.ChiefAccountantStatus)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ContractId).HasColumnName("ContractID");

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerId)
                    .HasColumnName("CustomerID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.EffectiveOn).HasColumnType("datetime");

                entity.Property(e => e.ExpiryOn).HasColumnType("datetime");

                entity.Property(e => e.FuelPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.HeadBuapprovedMethod)
                    .HasColumnName("HeadBUApprovedMethod")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.HeadBuapprovedOn)
                    .HasColumnName("HeadBUApprovedOn")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.HeadBuid)
                    .HasColumnName("HeadBUID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.HeadBunote)
                    .HasColumnName("HeadBUNote")
                    .HasMaxLength(250);

                entity.Property(e => e.HeadBustatus)
                    .HasColumnName("HeadBUStatus")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Name).HasMaxLength(250);

                entity.Property(e => e.OpsmanApprovedOn)
                    .HasColumnName("OPSManApprovedOn")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.OpsmanId)
                    .HasColumnName("OPSManID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanNote)
                    .HasColumnName("OPSManNote")
                    .HasMaxLength(500);

                entity.Property(e => e.OpsmanStatus)
                    .HasColumnName("OPSManStatus")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.QuotationType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReferCustomerId)
                    .HasColumnName("ReferCustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReferCustomerPercent).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.SaleManApprovedMethod)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.SaleManApprovedOn).HasColumnType("smalldatetime");

                entity.Property(e => e.SaleManId)
                    .HasColumnName("SaleManID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SaleManNote).HasMaxLength(250);

                entity.Property(e => e.SaleManStatus)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.SaleNote).HasMaxLength(250);

                entity.Property(e => e.Type).HasMaxLength(50);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserRevokeId)
                    .HasColumnName("UserRevokeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserRevokeNote).HasMaxLength(250);

                entity.Property(e => e.UserRevokeOn).HasColumnType("smalldatetime");
            });

            modelBuilder.Entity<PriceDtbrateCardBookingSchedule>(entity =>
            {
                entity.ToTable("priceDTBRateCardBookingSchedule", "dtb");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.EveryDaysInWeek)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.RateCardDetailId).HasColumnName("RateCardDetailID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PriceDtbrateCardCondition>(entity =>
            {
                entity.ToTable("priceDTBRateCardCondition", "dtb");

                entity.HasIndex(e => new { e.RateCardId, e.VehicleTypeId })
                    .HasName("U_priceDTBRateCardCondition")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Discount).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.DiscountVolumn).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DiscountWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DropPointPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DropPointStandardPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OutsideDropPointPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RateCardId).HasColumnName("RateCardID");

                entity.Property(e => e.ReferCustomerFee).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReferCustomerOutsideFee).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");
            });

            modelBuilder.Entity<PriceDtbrateCardDetail>(entity =>
            {
                entity.ToTable("priceDTBRateCardDetail", "dtb");

                entity.HasIndex(e => new { e.RateCardConditionId, e.PlaceFrom, e.PlaceTo })
                    .HasName("U_priceDTBRateCardDetail")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Note).HasMaxLength(500);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RateCardConditionId).HasColumnName("RateCardConditionID");

                entity.Property(e => e.ReferCustomerFee).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.StandardPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PriceDtbrateCardDropPoint>(entity =>
            {
                entity.ToTable("priceDTBRateCardDropPoint", "dtb");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Address).HasMaxLength(250);

                entity.Property(e => e.CompanyName).HasMaxLength(250);

                entity.Property(e => e.ContactPerson).HasMaxLength(50);

                entity.Property(e => e.ContactPhone)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DistrictId).HasColumnName("DistrictID");

                entity.Property(e => e.GeoCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Note).HasMaxLength(500);

                entity.Property(e => e.PointType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");

                entity.Property(e => e.RateCardDetailId).HasColumnName("RateCardDetailID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WardId).HasColumnName("WardID");
            });

            modelBuilder.Entity<PriceDtbrateCardDropPointRoute>(entity =>
            {
                entity.ToTable("priceDTBRateCardDropPointRoute", "dtb");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DeliveryPointId).HasColumnName("DeliveryPointID");

                entity.Property(e => e.PickupPointId).HasColumnName("PickupPointID");

                entity.Property(e => e.RateCardDetailId).HasColumnName("RateCardDetailID");
            });

            modelBuilder.Entity<PriceDtbstandardCostDetail>(entity =>
            {
                entity.ToTable("priceDTBStandardCostDetail", "dtb");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BuyingRouteId).HasColumnName("BuyingRouteID");

                entity.Property(e => e.Contract).HasMaxLength(100);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.FclbuyingId).HasColumnName("FCLBuyingID");

                entity.Property(e => e.FixedCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelConsumption).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.FuelCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Kratio).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.Mnramount)
                    .HasColumnName("MNRAmount")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Notes).HasMaxLength(800);

                entity.Property(e => e.OverheadAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RateCardDetailId).HasColumnName("RateCardDetailID");

                entity.Property(e => e.RoadId)
                    .HasColumnName("RoadID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.RouteCostId).HasColumnName("RouteCostID");

                entity.Property(e => e.RouteId).HasColumnName("RouteID");

                entity.Property(e => e.SupplierId)
                    .HasColumnName("SupplierID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalChargedWeight).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.TripBuyingRouteId).HasColumnName("TripBuyingRouteID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WeightRangeId).HasColumnName("WeightRangeID");

                entity.Property(e => e.WeightUnitId).HasColumnName("WeightUnitID");
            });

            modelBuilder.Entity<PriceFclbuying>(entity =>
            {
                entity.ToTable("priceFCLBuying", "fcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AppliedCustomerType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.ContainerTypeId)
                    .HasColumnName("ContainerTypeID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ContractId).HasColumnName("ContractID");

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(50);

                entity.Property(e => e.CustomerId)
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Note).HasMaxLength(1000);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReferCustomerFee).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReferCustomerId)
                    .HasColumnName("ReferCustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReferCustomerPercent).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RoadId)
                    .IsRequired()
                    .HasColumnName("RoadID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasColumnName("SupplierID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WeightRangeId).HasColumnName("WeightRangeID");
            });

            modelBuilder.Entity<PriceFclbuyingSurcharge>(entity =>
            {
                entity.ToTable("priceFCLBuyingSurcharge", "fcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BuyingId).HasColumnName("BuyingID");

                entity.Property(e => e.ChargeId)
                    .IsRequired()
                    .HasColumnName("ChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PriceRateCard>(entity =>
            {
                entity.ToTable("priceRateCard", "lcl");

                entity.HasIndex(e => new { e.BranchId, e.Code })
                    .HasName("U_priceRateCard_Code")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.Calculation)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefAccountantApprovedMethod)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefAccountantApprovedOn).HasColumnType("smalldatetime");

                entity.Property(e => e.ChiefAccountantId)
                    .HasColumnName("ChiefAccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefAccountantNote).HasMaxLength(500);

                entity.Property(e => e.ChiefAccountantStatus)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.EffectiveOn).HasColumnType("datetime");

                entity.Property(e => e.ExpiryOn).HasColumnType("datetime");

                entity.Property(e => e.FuelPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.GettingPriceMethod)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.HeadBuapprovedMethod)
                    .HasColumnName("HeadBUApprovedMethod")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.HeadBuapprovedOn)
                    .HasColumnName("HeadBUApprovedOn")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.HeadBuid)
                    .HasColumnName("HeadBUID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.HeadBunote)
                    .HasColumnName("HeadBUNote")
                    .HasMaxLength(250);

                entity.Property(e => e.HeadBustatus)
                    .HasColumnName("HeadBUStatus")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Name).HasMaxLength(250);

                entity.Property(e => e.OpsmanApprovedOn)
                    .HasColumnName("OPSManApprovedOn")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.OpsmanId)
                    .HasColumnName("OPSManID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsmanNote)
                    .HasColumnName("OPSManNote")
                    .HasMaxLength(500);

                entity.Property(e => e.OpsmanStatus)
                    .HasColumnName("OPSManStatus")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.QuotationType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.QuotedRouteType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReferCustomerId)
                    .HasColumnName("ReferCustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReferCustomerPercent).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.SaleManApprovedMethod)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.SaleManApprovedOn).HasColumnType("smalldatetime");

                entity.Property(e => e.SaleManId)
                    .HasColumnName("SaleManID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SaleManNote).HasMaxLength(250);

                entity.Property(e => e.SaleManStatus)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.SaleNote).HasMaxLength(250);

                entity.Property(e => e.Type).HasMaxLength(50);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserRevokeId)
                    .HasColumnName("UserRevokeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserRevokeNote).HasMaxLength(250);

                entity.Property(e => e.UserRevokeOn).HasColumnType("smalldatetime");
            });

            modelBuilder.Entity<PriceRateCardCondition>(entity =>
            {
                entity.ToTable("priceRateCardCondition", "lcl");

                entity.HasIndex(e => new { e.RateCardId, e.PickupPlaceId, e.PickupZoneCode, e.DeliveryPlaceId, e.DeliveryZoneCode, e.RoadId, e.RouteType })
                    .HasName("U_RateCardCondition")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BuyingRouteId).HasColumnName("BuyingRouteID");

                entity.Property(e => e.CommodityId).HasColumnName("CommodityID");

                entity.Property(e => e.DeliveryPlaceId).HasColumnName("DeliveryPlaceID");

                entity.Property(e => e.FclbuyingId).HasColumnName("FCLBuyingID");

                entity.Property(e => e.PickupPlaceId).HasColumnName("PickupPlaceID");

                entity.Property(e => e.PriceSellingId).HasColumnName("PriceSellingID");

                entity.Property(e => e.RateCardId).HasColumnName("RateCardID");

                entity.Property(e => e.RoadId)
                    .HasColumnName("RoadID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.RouteType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceTypeMappingId).HasColumnName("ServiceTypeMappingID");

                entity.Property(e => e.TripBuyingRouteId).HasColumnName("TripBuyingRouteID");

                entity.HasOne(d => d.RateCard)
                    .WithMany(p => p.PriceRateCardCondition)
                    .HasForeignKey(d => d.RateCardId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_priceRateCardCondition_priceRateCard");
            });

            modelBuilder.Entity<PriceRateCardDetail>(entity =>
            {
                entity.ToTable("priceRateCardDetail", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.FromValue).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Note)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RateCardConditionId).HasColumnName("RateCardConditionID");

                entity.Property(e => e.ReferCustomerFee).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.StandardPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ToValue).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.RateCardCondition)
                    .WithMany(p => p.PriceRateCardDetail)
                    .HasForeignKey(d => d.RateCardConditionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_priceRateCardDetail_priceRateCardCondition");
            });

            modelBuilder.Entity<PriceRateCardOverWeightDetail>(entity =>
            {
                entity.ToTable("priceRateCardOverWeightDetail", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Ladder).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RateCardConditionId).HasColumnName("RateCardConditionID");

                entity.Property(e => e.ReferCustomerFee).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.StandardPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.RateCardCondition)
                    .WithMany(p => p.PriceRateCardOverWeightDetail)
                    .HasForeignKey(d => d.RateCardConditionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_priceRateCardOverWeightDetail_priceRateCardCondition");
            });

            modelBuilder.Entity<PriceRouteCost>(entity =>
            {
                entity.ToTable("priceRouteCost");

                entity.HasIndex(e => new { e.RouteId, e.VehicleTypeId, e.WeightRangeId })
                    .HasName("U_priceRouteCost")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ApprovedOn).HasColumnType("smalldatetime");

                entity.Property(e => e.ApprovedUser)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.ChargedWeight).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.ContainerTypeId)
                    .HasColumnName("ContainerTypeID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.EffectiveDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ExpiryDate).HasColumnType("smalldatetime");

                entity.Property(e => e.FixedCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelConsumption).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.FuelCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.GuaranteedDistancePerMonth).HasColumnType("decimal(10, 3)");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Kratio).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.Mnrcost)
                    .HasColumnName("MNRCost")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Note).HasMaxLength(800);

                entity.Property(e => e.OverheadCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RouteId).HasColumnName("RouteID");

                entity.Property(e => e.TotalSurcharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TripAllowance).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnitCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");

                entity.Property(e => e.WeightPerShipment).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.WeightRangeId).HasColumnName("WeightRangeID");

                entity.HasOne(d => d.Branch)
                    .WithMany(p => p.PriceRouteCost)
                    .HasForeignKey(d => d.BranchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_priceRouteCost_catBranch");
            });

            modelBuilder.Entity<PriceRouteCostShortTrip>(entity =>
            {
                entity.HasKey(e => new { e.CostId, e.PlaceFrom, e.PlaceTo });

                entity.ToTable("priceRouteCostShortTrip");

                entity.Property(e => e.CostId).HasColumnName("CostID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Kratio).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PriceRouteCostSurcharge>(entity =>
            {
                entity.HasKey(e => new { e.CostId, e.ChargeId });

                entity.ToTable("priceRouteCostSurcharge");

                entity.Property(e => e.CostId).HasColumnName("CostID");

                entity.Property(e => e.ChargeId)
                    .HasColumnName("ChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PriceServiceTypeWeightRange>(entity =>
            {
                entity.ToTable("priceServiceTypeWeightRange", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ApprovedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApprovedOn).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Ladder).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ServiceTypeMappingId).HasColumnName("ServiceTypeMappingID");

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PriceServiceTypeWeightRangeDetail>(entity =>
            {
                entity.HasKey(e => new { e.ServiceTypeWeightRangeId, e.FromValue, e.ToValue, e.UnitId });

                entity.ToTable("priceServiceTypeWeightRangeDetail", "lcl");

                entity.Property(e => e.ServiceTypeWeightRangeId).HasColumnName("ServiceTypeWeightRangeID");

                entity.Property(e => e.FromValue).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ToValue).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnitId).HasColumnName("UnitID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PriceTripBuyingDetail>(entity =>
            {
                entity.ToTable("priceTripBuyingDetail", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BuyingVehicleTypeId).HasColumnName("BuyingVehicleTypeID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DropPointPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Note).HasMaxLength(500);

                entity.Property(e => e.OutsideDropPointPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReferCustomerFee).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PriceTripBuyingVehicleType>(entity =>
            {
                entity.ToTable("priceTripBuyingVehicleType", "lcl");

                entity.HasIndex(e => new { e.BuyingId, e.VehicleTypeId })
                    .HasName("U_TripBuyingVehicleType")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BuyingId).HasColumnName("BuyingID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Discount).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.DiscountVolumn).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DiscountWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DropPointPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OutsideDropPointPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReferCustomerFee).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ReferCustomerOutsideFee).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");
            });

            modelBuilder.Entity<SaleDtbquotation>(entity =>
            {
                entity.ToTable("saleDTBQuotation", "dtb");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContractId).HasColumnName("ContractID");

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Note).HasMaxLength(1000);

                entity.Property(e => e.PaymentDeadlineUnit)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.QuotationScope)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.SaleMember)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SaleResource)
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
            });

            modelBuilder.Entity<SaleFclquotation>(entity =>
            {
                entity.ToTable("saleFCLQuotation", "fcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContractId).HasColumnName("ContractID");

                entity.Property(e => e.CustomerContact).HasMaxLength(200);

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.FromTrialDate).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Note).HasMaxLength(1000);

                entity.Property(e => e.QuotationType).HasMaxLength(50);

                entity.Property(e => e.ReferCustomerId)
                    .HasColumnName("ReferCustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReferCustomerPercent).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RejectionReason).HasMaxLength(1000);

                entity.Property(e => e.Tel)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.ToTrialDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ToUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SaleFclquotationRoute>(entity =>
            {
                entity.ToTable("saleFCLQuotationRoute", "fcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountDate).HasColumnType("smalldatetime");

                entity.Property(e => e.AccountId)
                    .HasColumnName("AccountID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountNote).HasMaxLength(1000);

                entity.Property(e => e.AccountStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountantApprovedMethod)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.BuyingPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ChargeCostAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CommodityId).HasColumnName("CommodityID");

                entity.Property(e => e.ContainerTypeId)
                    .HasColumnName("ContainerTypeID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .IsRequired()
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.ExchangeRate).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FinalPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FixedCostAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelConsumption).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelCostAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.JourneyNote).HasMaxLength(1000);

                entity.Property(e => e.Kratio).HasColumnType("decimal(8, 5)");

                entity.Property(e => e.LenthKm).HasColumnName("LenthKM");

                entity.Property(e => e.MiniumCostAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.MnrcostAmount)
                    .HasColumnName("MNRCostAmount")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Note).HasMaxLength(1000);

                entity.Property(e => e.OpsManagerDate).HasColumnType("smalldatetime");

                entity.Property(e => e.OpsManagerId)
                    .HasColumnName("OpsManagerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsManagerNote).HasMaxLength(1000);

                entity.Property(e => e.OpsManagerStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OpsapprovedMethod)
                    .HasColumnName("OPSApprovedMethod")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.OtherRevenueAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OverheadCostAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PoDelivery)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PoReceipt)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PoTransfer)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PriceBack).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PriceBackVat)
                    .HasColumnName("PriceBackVAT")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PriceCustomer).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PriceMarket).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PriceSaleRequest).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PriceSaleRequestVat)
                    .HasColumnName("PriceSaleRequestVAT")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.QuotationId).HasColumnName("QuotationID");

                entity.Property(e => e.QuotationRouteDuplicatedId).HasColumnName("QuotationRouteDuplicatedID");

                entity.Property(e => e.QuotationRouteType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RealWeight).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.ReferCustomerFee).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RoadId)
                    .IsRequired()
                    .HasColumnName("RoadID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.SaleManApprovedMethod)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.SaleManagerDate).HasColumnType("smalldatetime");

                entity.Property(e => e.SaleManagerId)
                    .HasColumnName("SaleManagerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SaleManagerNote).HasMaxLength(1000);

                entity.Property(e => e.SaleManagerStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SaleMember)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SaleResource).HasMaxLength(50);

                entity.Property(e => e.SellingCurrencyId)
                    .HasColumnName("SellingCurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Type).HasMaxLength(50);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VolumeId).HasColumnName("VolumeID");

                entity.Property(e => e.WeightRangeId).HasColumnName("WeightRangeID");
            });

            modelBuilder.Entity<SaleFclquotationShortTrip>(entity =>
            {
                entity.ToTable("saleFCLQuotationShortTrip", "fcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BuyingPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.BuyingPriceId).HasColumnName("BuyingPriceID");

                entity.Property(e => e.ChargeCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CostingPriceId).HasColumnName("CostingPriceID");

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.FixedCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelAllowance).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FuelAllowanceNote).HasMaxLength(500);

                entity.Property(e => e.FuelConsumption).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.FuelCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.JourneyNote).HasMaxLength(1000);

                entity.Property(e => e.Kratio).HasColumnType("decimal(8, 4)");

                entity.Property(e => e.LenthKm).HasColumnName("LenthKM");

                entity.Property(e => e.MinCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Mnrcost)
                    .HasColumnName("MNRCost")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OverheadCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.QuotationRouteId).HasColumnName("QuotationRouteID");

                entity.Property(e => e.Repetition).HasDefaultValueSql("((1))");

                entity.Property(e => e.RouteId).HasColumnName("RouteID");

                entity.Property(e => e.SupplierId)
                    .HasColumnName("SupplierID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.TripBuyingRouteId).HasColumnName("TripBuyingRouteID");

                entity.Property(e => e.UserCreated).HasMaxLength(50);

                entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");
            });

            modelBuilder.Entity<SaleFclquotationShortTripDetail>(entity =>
            {
                entity.HasKey(e => new { e.QuotationShortTripId, e.PlaceFrom, e.PlaceTo });

                entity.ToTable("saleFCLQuotationShortTripDetail", "fcl");

                entity.Property(e => e.QuotationShortTripId).HasColumnName("QuotationShortTripID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.LenthKm).HasColumnName("LenthKM");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SaleFclquotationShortTripSurcharge>(entity =>
            {
                entity.HasKey(e => new { e.QuotationShortTripId, e.ChargeId, e.ChargedToCustomer });

                entity.ToTable("saleFCLQuotationShortTripSurcharge", "fcl");

                entity.Property(e => e.QuotationShortTripId).HasColumnName("QuotationShortTripID");

                entity.Property(e => e.ChargeId)
                    .HasColumnName("ChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BillingCompanyId)
                    .HasColumnName("BillingCompanyID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .IsRequired()
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.OtherRevenuePrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Tariff).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SaleQuotation>(entity =>
            {
                entity.ToTable("saleQuotation", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ChiefAccountantApprovedMethod)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefAccountantDate).HasColumnType("smalldatetime");

                entity.Property(e => e.ChiefAccountantId)
                    .HasColumnName("ChiefAccountantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ChiefAccountantNote).HasMaxLength(500);

                entity.Property(e => e.ChiefAccountantStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContractId).HasColumnName("ContractID");

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.MaximumDelayTimeUnit)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Note).HasMaxLength(1000);

                entity.Property(e => e.PaymentDeadlineUnit)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.QuotationScope)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.RateCardId).HasColumnName("RateCardID");

                entity.Property(e => e.SaleManApprovedMethod)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.SaleManagerDate).HasColumnType("smalldatetime");

                entity.Property(e => e.SaleManagerId)
                    .HasColumnName("SaleManagerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SaleManagerNote).HasMaxLength(500);

                entity.Property(e => e.SaleManagerStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SaleMember)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SaleResource)
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

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");

                entity.HasOne(d => d.RateCard)
                    .WithMany(p => p.SaleQuotation)
                    .HasForeignKey(d => d.RateCardId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_saleQuotation_priceRateCard");
            });

            modelBuilder.Entity<SaleQuotationRoute>(entity =>
            {
                entity.ToTable("saleQuotationRoute", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.CommodityId).HasColumnName("CommodityID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DeliveryAddress).HasMaxLength(250);

                entity.Property(e => e.DeliveryZoneId).HasColumnName("DeliveryZoneID");

                entity.Property(e => e.DestinationBranchId).HasColumnName("DestinationBranchID");

                entity.Property(e => e.DestinationHubId).HasColumnName("DestinationHubID");

                entity.Property(e => e.EstimateWeight).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Kratio).HasColumnType("decimal(8, 5)");

                entity.Property(e => e.Note).HasMaxLength(1000);

                entity.Property(e => e.OriginBranchId).HasColumnName("OriginBranchID");

                entity.Property(e => e.OriginHubId).HasColumnName("OriginHubID");

                entity.Property(e => e.PickupAdress).HasMaxLength(250);

                entity.Property(e => e.PickupZoneId).HasColumnName("PickupZoneID");

                entity.Property(e => e.QuotationId).HasColumnName("QuotationID");

                entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Quotation)
                    .WithMany(p => p.SaleQuotationRoute)
                    .HasForeignKey(d => d.QuotationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_saleQuotationRoute_saleQuotation");
            });

            modelBuilder.Entity<SaleQuotationRouteSurcharge>(entity =>
            {
                entity.ToTable("saleQuotationRouteSurcharge", "lcl");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ChargeId)
                    .IsRequired()
                    .HasColumnName("ChargeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrencyId)
                    .IsRequired()
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.IncludedVat).HasColumnName("IncludedVAT");

                entity.Property(e => e.OtherRevenuePrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.QuotationRouteId).HasColumnName("QuotationRouteID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SaleSalesTarget>(entity =>
            {
                entity.ToTable("saleSalesTarget");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.EffectiveDate).HasColumnType("smalldatetime");

                entity.Property(e => e.SalePersonId)
                    .HasColumnName("SalePersonID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Target).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Type).HasMaxLength(50);

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

            modelBuilder.Entity<SysBaseEnum>(entity =>
            {
                entity.HasKey(e => e.Key);

                entity.ToTable("sysBaseEnum");

                entity.Property(e => e.Key)
                    .HasColumnName("key")
                    .HasMaxLength(100)
                    .ValueGeneratedNever();

                entity.Property(e => e.Api)
                    .HasColumnName("api")
                    .HasMaxLength(200);

                entity.Property(e => e.ColumnsDisplay)
                    .HasColumnName("columnsDisplay")
                    .HasMaxLength(1000);

                entity.Property(e => e.ColumnsId)
                    .HasColumnName("columnsId")
                    .HasMaxLength(100);

                entity.Property(e => e.ColumnsText)
                    .HasColumnName("columnsText")
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(200);

                entity.Property(e => e.Query)
                    .HasColumnName("query")
                    .HasMaxLength(2000);

                entity.Property(e => e.Server)
                    .HasColumnName("server")
                    .HasMaxLength(100);

                entity.Property(e => e.Version)
                    .HasColumnName("version")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<SysBaseEnumDetail>(entity =>
            {
                entity.HasKey(e => new { e.BaseEnumKey, e.Id });

                entity.ToTable("sysBaseEnumDetail");

                entity.Property(e => e.BaseEnumKey)
                    .HasColumnName("baseEnumKey")
                    .HasMaxLength(100);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasMaxLength(50);

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(100);

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasMaxLength(10);

                entity.HasOne(d => d.BaseEnumKeyNavigation)
                    .WithMany(p => p.SysBaseEnumDetail)
                    .HasForeignKey(d => d.BaseEnumKey)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysBaseEnumDetail_sysBaseEnum");
            });

            modelBuilder.Entity<SysBu>(entity =>
            {
                entity.ToTable("sysBU");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountName).HasMaxLength(150);

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
                    .HasMaxLength(150);

                entity.Property(e => e.AddressVn)
                    .HasColumnName("Address_VN")
                    .HasMaxLength(150);

                entity.Property(e => e.AreaId)
                    .HasColumnName("AreaID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BankAddress).HasMaxLength(255);

                entity.Property(e => e.BankName).HasMaxLength(150);

                entity.Property(e => e.BunameEn)
                    .HasColumnName("BUName_EN")
                    .HasMaxLength(150);

                entity.Property(e => e.BunameVn)
                    .HasColumnName("BUName_VN")
                    .HasMaxLength(150);

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DescriptionEn)
                    .HasColumnName("Description_EN")
                    .HasMaxLength(255);

                entity.Property(e => e.DescriptionVn)
                    .HasColumnName("Description_VN")
                    .HasMaxLength(255);

                entity.Property(e => e.Email).HasMaxLength(150);

                entity.Property(e => e.Fax).HasMaxLength(50);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Logo).HasColumnType("image");

                entity.Property(e => e.Notes).HasMaxLength(255);

                entity.Property(e => e.Tax)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TaxAccount).HasMaxLength(50);

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

                entity.Property(e => e.Website).HasMaxLength(50);

                entity.HasOne(d => d.Area)
                    .WithMany(p => p.SysBu)
                    .HasForeignKey(d => d.AreaId)
                    .HasConstraintName("FK_sysBU_catArea");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.SysBu)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("FK_sysBU_catCountry");
            });

            modelBuilder.Entity<SysChangeBookingOverDateLog>(entity =>
            {
                entity.ToTable("sysChangeBookingOVerDateLog");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasColumnName("CustomerID")
                    .HasMaxLength(50);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.ReChangedDate).HasColumnType("smalldatetime");

                entity.Property(e => e.UserCreated)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<SysDriverAllowanceParameter>(entity =>
            {
                entity.ToTable("sysDriverAllowanceParameter");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.CurrencyId)
                    .HasColumnName("CurrencyID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.EffectiveOn).HasColumnType("datetime");

                entity.Property(e => e.FromKm)
                    .HasColumnName("FromKM")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Note).HasMaxLength(800);

                entity.Property(e => e.ToKm)
                    .HasColumnName("ToKM")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Value).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");
            });

            modelBuilder.Entity<SysEmployee>(entity =>
            {
                entity.ToTable("sysEmployee");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.AccessDescription).HasMaxLength(50);

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

                entity.Property(e => e.EmpPhotoSize).HasMaxLength(255);

                entity.Property(e => e.EmployeeNameEn)
                    .HasColumnName("EmployeeName_EN")
                    .HasMaxLength(50);

                entity.Property(e => e.EmployeeNameVn)
                    .IsRequired()
                    .HasColumnName("EmployeeName_VN")
                    .HasMaxLength(50);

                entity.Property(e => e.ExtNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.HomeAddress).HasMaxLength(150);

                entity.Property(e => e.HomePhone)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Photo).HasColumnType("image");

                entity.Property(e => e.Position).HasMaxLength(50);

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

            modelBuilder.Entity<SysGpsprovider>(entity =>
            {
                entity.ToTable("sysGPSProvider");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Password).HasMaxLength(250);

                entity.Property(e => e.UserName).HasMaxLength(150);

                entity.Property(e => e.Wsurl)
                    .HasColumnName("WSURL")
                    .HasMaxLength(350);
            });

            modelBuilder.Entity<SysLogo>(entity =>
            {
                entity.HasKey(e => new { e.ReportName, e.Type });

                entity.ToTable("sysLogo");

                entity.Property(e => e.ReportName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Logo)
                    .IsRequired()
                    .HasColumnType("image");

                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.Property(e => e.Title).HasMaxLength(300);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<SysMenu>(entity =>
            {
                entity.ToTable("sysMenu");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Arguments).HasMaxLength(300);

                entity.Property(e => e.AssemplyName).HasMaxLength(500);

                entity.Property(e => e.Description).HasMaxLength(300);

                entity.Property(e => e.ForServiceType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ForWorkPlace)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Icon).HasMaxLength(100);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(250);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(250);

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

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(250);

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

            modelBuilder.Entity<SysOneTmsbuildVersion>(entity =>
            {
                entity.ToTable("sysOneTMSBuildVersion");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatabaseVersion)
                    .IsRequired()
                    .HasMaxLength(25);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.VersionDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<SysParameter>(entity =>
            {
                entity.ToTable("sysParameter");

                entity.HasIndex(e => new { e.Code, e.ParaGroup })
                    .HasName("U_Coe")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Note)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ParaGroup)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ParameterNameEn)
                    .HasColumnName("ParameterName_EN")
                    .HasMaxLength(250);

                entity.Property(e => e.ParameterNameVn)
                    .IsRequired()
                    .HasColumnName("ParameterName_VN")
                    .HasMaxLength(250);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysParameterDetail>(entity =>
            {
                entity.ToTable("sysParameterDetail");

                entity.HasIndex(e => new { e.BranchId, e.ParameterId, e.VehicleTypeId, e.ShipmentTypeId, e.HaulType, e.PartnerId, e.EffectiveOn })
                    .HasName("U_ParameterDetail")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.EffectiveOn).HasColumnType("datetime");

                entity.Property(e => e.ExpiryOn).HasColumnType("datetime");

                entity.Property(e => e.HaulType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Note).HasMaxLength(800);

                entity.Property(e => e.ParameterId).HasColumnName("ParameterID");

                entity.Property(e => e.PartnerId)
                    .HasColumnName("PartnerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShipmentTypeId)
                    .HasColumnName("ShipmentTypeID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Value).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");

                entity.HasOne(d => d.Parameter)
                    .WithMany(p => p.SysParameterDetail)
                    .HasForeignKey(d => d.ParameterId)
                    .HasConstraintName("FK_sysParameterDetail_sysParameter");
            });

            modelBuilder.Entity<SysParameterVehicleType>(entity =>
            {
                entity.ToTable("sysParameterVehicleType");

                entity.HasIndex(e => new { e.BranchId, e.ParameterId, e.VehicleTypeId, e.EffectiveOn })
                    .HasName("U_ParameterVehicleType")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.EffectiveOn).HasColumnType("datetime");

                entity.Property(e => e.ExpiryOn).HasColumnType("datetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Note).HasMaxLength(800);

                entity.Property(e => e.ParameterId).HasColumnName("ParameterID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Value).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");

                entity.HasOne(d => d.VehicleType)
                    .WithMany(p => p.SysParameterVehicleType)
                    .HasForeignKey(d => d.VehicleTypeId)
                    .HasConstraintName("FK_sysParameterVehicleType_catVehicleType");
            });

            modelBuilder.Entity<SysPermission>(entity =>
            {
                entity.ToTable("sysPermission");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Description).HasMaxLength(150);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Name)
                    .IsRequired()
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
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Description).HasMaxLength(250);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

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
                    .IsUnicode(false);

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Receivers).IsUnicode(false);

                entity.Property(e => e.SentDateTime).HasColumnType("smalldatetime");

                entity.Property(e => e.SentUser)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Subject).HasMaxLength(1000);

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysSmslog>(entity =>
            {
                entity.ToTable("sysSMSLog");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ActionDescription).HasMaxLength(4000);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.Property(e => e.Imei)
                    .HasColumnName("IMEI")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.LogType).HasMaxLength(50);

                entity.Property(e => e.Provider).HasMaxLength(50);

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.Tel).HasMaxLength(50);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<SysStatus>(entity =>
            {
                entity.ToTable("sysStatus");

                entity.HasIndex(e => new { e.Type, e.Code })
                    .HasName("U_StatusCode")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Description)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(250);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(250);

                entity.Property(e => e.Type)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysSynchronizationError>(entity =>
            {
                entity.ToTable("sysSynchronizationError");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.ErrorDescription).HasMaxLength(4000);

                entity.Property(e => e.OwnId)
                    .HasColumnName("OwnID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SugarId)
                    .HasColumnName("SugarID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TableCrm)
                    .HasColumnName("TableCRM")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysTemplate>(entity =>
            {
                entity.ToTable("sysTemplate");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.AddApi).HasMaxLength(100);

                entity.Property(e => e.Api)
                    .HasColumnName("API")
                    .HasMaxLength(100);

                entity.Property(e => e.DeleteApi).HasMaxLength(100);

                entity.Property(e => e.EditApi).HasMaxLength(100);

                entity.Property(e => e.Inactive).HasColumnName("INACTIVE");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("NAME")
                    .HasMaxLength(100);

                entity.Property(e => e.Server)
                    .HasColumnName("SERVER")
                    .HasMaxLength(100);

                entity.Property(e => e.TableCatalog)
                    .HasColumnName("TABLE_CATALOG")
                    .HasMaxLength(100);

                entity.Property(e => e.TableName)
                    .IsRequired()
                    .HasColumnName("TABLE_NAME")
                    .HasMaxLength(100);

                entity.Property(e => e.TableSchema)
                    .HasColumnName("TABLE_SCHEMA")
                    .HasMaxLength(100);

                entity.Property(e => e.TableType)
                    .HasColumnName("TABLE_TYPE")
                    .HasMaxLength(10);

                entity.Property(e => e.Type)
                    .HasColumnName("TYPE")
                    .HasMaxLength(10);

                entity.Property(e => e.Userid)
                    .HasColumnName("USERID")
                    .HasMaxLength(20);

                entity.Property(e => e.Version)
                    .HasColumnName("VERSION")
                    .HasMaxLength(10);
            });

            modelBuilder.Entity<SysTemplateDetail>(entity =>
            {
                entity.HasKey(e => new { e.Templateid, e.OrdinalPosition });

                entity.ToTable("sysTemplateDetail");

                entity.Property(e => e.Templateid)
                    .HasColumnName("TEMPLATEID")
                    .HasMaxLength(50);

                entity.Property(e => e.OrdinalPosition).HasColumnName("ORDINAL_POSITION");

                entity.Property(e => e.CharacterMaximumLength).HasColumnName("CHARACTER_MAXIMUM_LENGTH");

                entity.Property(e => e.Class)
                    .HasColumnName("CLASS")
                    .HasMaxLength(1000);

                entity.Property(e => e.ColumnName)
                    .IsRequired()
                    .HasColumnName("COLUMN_NAME")
                    .HasMaxLength(100);

                entity.Property(e => e.DataType)
                    .HasColumnName("DATA_TYPE")
                    .HasMaxLength(20);

                entity.Property(e => e.Description)
                    .HasColumnName("DESCRIPTION")
                    .HasMaxLength(2000);

                entity.Property(e => e.Display)
                    .HasColumnName("DISPLAY")
                    .HasMaxLength(20);

                entity.Property(e => e.Hidden).HasColumnName("HIDDEN");

                entity.Property(e => e.Invisible).HasColumnName("INVISIBLE");

                entity.Property(e => e.IsNullable)
                    .HasColumnName("IS_NULLABLE")
                    .HasMaxLength(3);

                entity.Property(e => e.Lookup)
                    .HasColumnName("LOOKUP")
                    .HasMaxLength(100);

                entity.Property(e => e.Name)
                    .HasColumnName("NAME")
                    .HasMaxLength(100);

                entity.Property(e => e.Readonly).HasColumnName("READONLY");

                entity.Property(e => e.Required).HasColumnName("REQUIRED");

                entity.Property(e => e.Stt).HasColumnName("STT");

                entity.HasOne(d => d.Template)
                    .WithMany(p => p.SysTemplateDetail)
                    .HasForeignKey(d => d.Templateid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysTemplateDetail_sysTemplate");
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

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.SugarId)
                    .HasColumnName("SugarID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserGroupId).HasColumnName("UserGroupID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Username)
                    .IsRequired()
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
                    .HasConstraintName("FK_sysUser_catPlace");
            });

            modelBuilder.Entity<SysUserGroup>(entity =>
            {
                entity.ToTable("sysUserGroup");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.Decription).HasMaxLength(500);

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysUserGroupRole>(entity =>
            {
                entity.ToTable("sysUserGroupRole");

                entity.HasIndex(e => new { e.UserGroupId, e.RoleId })
                    .HasName("U_UserGroupRole")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.UserGroupId).HasColumnName("UserGroupID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.SysUserGroupRole)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysUserGroupRole_sysRole");

                entity.HasOne(d => d.UserGroup)
                    .WithMany(p => p.SysUserGroupRole)
                    .HasForeignKey(d => d.UserGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysUserGroupRole_sysUserGroup1");
            });

            modelBuilder.Entity<SysUserLog>(entity =>
            {
                entity.ToTable("sysUserLog");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ComputerName).HasMaxLength(50);

                entity.Property(e => e.Ip)
                    .HasColumnName("IP")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LoggedInOn).HasColumnType("smalldatetime");

                entity.Property(e => e.LoggedOffOn).HasColumnType("smalldatetime");

                entity.Property(e => e.UserId)
                    .IsRequired()
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
                    .HasConstraintName("FK_sysUserOtherBranch_catPlace");
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
                    .HasConstraintName("FK_sysUserRole_catPlace");
            });

            modelBuilder.Entity<SysWebCode>(entity =>
            {
                entity.ToTable("sysWebCode");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.ObjectType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReferencedObjectId)
                    .IsRequired()
                    .HasColumnName("ReferencedObjectID")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.ToUser).HasMaxLength(50);

                entity.Property(e => e.UserCreated).HasMaxLength(50);
            });
        }
    }
}
