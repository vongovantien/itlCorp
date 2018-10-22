namespace eFMS.API.Common.Globals
{
    public enum CatPlaceTypeEnum
    {
        BorderGate = 1,
        Branch = 2,
        Depot = 3,
        District = 4,
        Hub = 5,
        IndustrialZone = 6,
        Other = 7,
        Port = 8,
        Province = 9,
        Station = 10,
        Ward = 11,
        Warehouse = 12
    }

    public enum TemplateMaintenance
    {
        PlanReplaceOil,
        PlanReplaceAmaza,
        PlanReplaceBootAndDynamo,
        PlanReplaceGroundAndBrakeShoe,
        PlanReplaceBattery,
        PlanReplaceTractorTires,
        PlanReplaceRemoocBrakeShoe,
        PlanReplaceRemoocTires,
        PlanReplaceMaintenanceRemooc,
        PlanMaintenanceVehicleFollowMonth,
        PlanReplacePartFollowMonth,
        PlanRegister
    }

    public enum PlanType
    {
        MonthPlan,
        YearPlan,
        CostEstimation
    }

    public enum TypeReplace
    {
        Increase,
        Decrease
    }

    public enum VehiclePartID
    {
        DANGKIEMMOC = 76,
        DANGKIEMTractor = 77
    }

    public enum RepairType
    {
        Cleaning,
        Check,
        Repair,
        Replace,
        Maintenance,
        ArisingOnRoad
    }

    public enum MaintenanceType
    {
        BHTN = 1,
        CBH = 2,
        HBH = 3
    }

    public enum MaintenanceStatus
    {
        Confirmed = 67,
        Processing = 68,
        Finished = 69
    }
    public enum MaintenanceSearch
    {
        MaintenanceNeed,
        MaintenancePlan,
        MaintenanceRequest
    }

    public enum RequestStatus
    {
        NotSendRequest = 0,
        Pending = 1,
        Rejected = 2,
        Accepted = 3,
        Repairing = 4,
        AutomaticallyAccepted = 5,
        AutomaticallyReject = 6,
        Unknown = 99
    }

    public enum LCLPlaceTypeForQuotation
    {
        Province,
        OtherPlace,
        Unknown
    }

    public enum ChargeStatus
    {
        AllowEditing,
        Pending,
        Accepted,
        Rejected,
        Denied,
        AutomaticallyAccepted,
        Paid
    }

    public enum PaymentMethod
    {
        Cash,
        BankTransfer
    }

    public enum PaymentStatus
    {
        Pending,
        Suggested,
        Requested,
        Accepted,
        Rejected,
        Paidpartial,
        Completed
    }

    public enum UnitTime
    {
        Minutes,
        Hour,
        Day,
        Month,
        Week,
        Year
    }

    public enum RateCardType
    {
        Standard,
        Customer
    }

    public enum TypeCharge
    {
        FCLCharge,
        LCLCharge,
        FCLBehalf,
        LCLBehalf,
        DCharge,
        DBehalf,
        LCLWaybillExpense
    }

    //Basic Status of approval
    public enum ApprovalStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2,
        Denied = 3,
        AutomaticallyAccepted = 4,
        Unknown = 99
    }

    public enum ObjectBePaid
    {
        Driver,
        Supplier,
        Other,
        Petrol,
        AllDriver
    }

    public enum BookingSearchBoxDateType
    {
        BookingDate,
        DeliveryDate,
        DatetimeCreated
    }

    public enum RoleID
    {
        SA1 = 1,
        SA2 = 4,
        CS = 5,
        WO = 6,
        HM = 7,
        CUS = 8,
        SSV = 9,
        FIN = 14,
        SSP = 15,
        RS = 16,
        SysAdmin = 17,
        ConfirmBookingHire = 21,
        ConvertTransfer = 22,
        SMEM = 23,
        CHIEF = 24,
        Head = 25,
        BookingReceived = 26,
        CSFuel = 27,
        VehicleOPS = 28,
        AcctMember = 29,
        FuelMaintenance = 32,
        HubSup = 34,
        SaleMan = 35
    }

    public enum BehalfType
    {
        Income = 1,
        Outcome = 2,
        PT = 3,
        IncomeDifferent = 4,
        OutcomeDifferent = 5,
        ExtraFreightPrice = 6,
        IncomeThirdParty = 7,
        OutcomeThirdParty = 8
    }

    //using Approve Active Vehicle Request
    public enum StatusApproveVR
    {
        Accepted = 40,
        AutoAccepted = 41,
        AcceptedDeny = 43,
        RejectedDeny = 44,
        Rejected = 45,
        AutoDeny
    }

    public enum FCLTransportRequestStatus
    {
        DriverConfirmed = 48,
        Finished = 49,
        Processing = 50,
        BUCancel = 51,
        CustomerCancel = 52,
        RequestBUToCancel = 53,
        CustomerRequestBUToCancel = 54,
        RejectedCancelation = 60,
        ChangeDriver = 61,
        Started = 64,
        SentRequest = 80,
        DriverReceivedRequest = 81,
        DriverRejected = 83
    }
    public enum TransportRequestStatus
    {
        SentRequest = 34,
        DriverReceivedRequest = 35,
        DriverConfirmed = 36,
        DriverRejected = 37,
        Started = 62,
        Processing = 38,
        Finished = 63,
        Closed = 39,
        Canceled = 65
    }

    public enum BookingHireStatus
    {
        NotSendRequest,
        Pending,
        Rejected,
        Accepted,
        Revoke,
    }

    public enum StatusType
    {
        FCLBookingStatus,
        RateCard,
        Order,
        Booking,
        FCLTripRecordStatus,
        TransportRequest,
        ConfirmStatusVR,
        FCLTransportRequest,
        TripRecordStatus,
        MaintenanceStatus,
        RouteCost
    }

    public enum BookingStatus
    {
        BookingDenied = 24,
        CustomerDeny = 25,
        CheckingInfo = 26,
        Transfer = 27,
        Hire = 28,

        Unknown = 9999
    }

    public enum BookingType
    {
        Normal,
        VehicleForRent,
        MaintenanceRepair,
        NoneRevenue,
        OnlyInvoice
    }

    public enum IM_EX_Type
    {
        Import,
        Export,
        Domestic,
        CrossBorder
    }

    public enum Currency
    {
        VND,
        USD
    }

    public enum Branch
    {
        HAN,
        SGN,
        DAN
    }
    public enum ChargeID
    {
        BX,
        BXKH,
        LOFF,
        NX,
        OBH,
        PK,
        VSC,
        GHC,
        PVC,
        LTX,
        HH,
        CC,
        CAN,
        CD,
        MR,
        TT,
        NXKH,
        CVCH,
        LTXPC,
        LTNPT,
        TC,
        RD,
        PCC
    }

    public enum Parameter
    {
        selling_price,
        TotalFeeOffice,
        TotalWorkingDay,
        MNR,
        FixedCost,
        FuelPrice,
        TotalEmployee,
        OverheadCost,
        DriverMothlySalary,
        DriverSundayAllowance,
        MNRTyre,
        MNRMaintenance,
        ActualFixedCostPerMonth,
        ActualOverheadPerMonth,
        GuaranteedDistancePerMonth,
        DriverAllowance,
        DriverTripAllowance,
        DriverWorkingDayAllowance,
        DropPoint,
        VATRate,
        EstimateKMForMaintenance,
        TransitMinWeight,
        TransitMaxWeight,
        TransitMinVolume,
        TransitMaxVolume,
        MaxExecutionTime,
        ChangedFuelPrice,
        Maintenance,
        GuaranteedShipmentPerMonth,
        EstPickupDeliveryCost,
        KratioMinValue,
        PercentCreditFirstAlert,
        PercentCreditSecondAlert,
        PercentCreditThirdAlert,
        NumberDaySOAOverdueLevel1,
        NumberDaySOAOverdueLevel2,
        NumberDaySOAOverdueLevel3,
        MailSentFirst,
        MailSentSecond,
        DiscountFuelPrice,
        NumberDayOverdueBooking
    }

    public enum UserGroup
    {
        Admin = 1,
        HeadBU = 2,
        AccountantManager = 3,
        Accountant = 4,
        Audit = 5,
        BDManager = 6,
        BDMember = 7,
        ChiefAccountant = 8,
        CustomerService = 9,
        CSFuel = 10,
        FinanceManager = 11,
        HubManager = 12,
        ReceiveBooking = 13,
        SaleManager = 14,
        SaleClerk = 15,
        VehicleOperator = 16,
        SaleITLCorp = 17,
        SaleMem = 18
    }

    public enum Country
    {
        CAMBODIA,
        LAO,
        VN
    }

    public enum StatusVehicle
    {
        Using,
        Available,
        Complete,
        Recheking,
        Repairing,
        Checking
    }

    public enum PartnerGroupType
    {
        AGENT,
        CONSIGNEE,
        CUSTOMER,
        SHIPPER,
        SHIPPINGLINE,
        SUPPLIER,
        SUPPLIERMATERIAL,
        PETROLSTATION,
        PAYMENTOBJECT,
        OPS
    }

    public enum VehicleType
    {
        Tractor = 1,
        Remooc = 2,
        TractorM = 3,
        Remooc3 = 4,
        TractorRemooc = 5,
        FUSO = 6
    }

    public enum VehicleGroupType
    {
        Remooc = 1,
        Tractor = 2,
        Truck = 3,
        Van = 4
    }
    public enum DocumentType
    {
        Booking = 1,
        ChargeBehalf = 2,
        Maintenance = 3,
        Transit = 4,
        DeliveryRunSheet = 5,
        SOA = 6,
        RateCard = 7,
        OrderDetailTransportRequest = 8,
        Voucher = 9,
        FCLTransportRequest = 10,
        FCLVoucher = 11,
        DNTDropPointTransportRequest = 12,
        DNTVoucher = 13,
        Unknown = 9999
    }

    public enum Cont
    {
        _20DC,
        _40DC,
        _2x20,
        _45DC,
        FLATRACK
    }

    public enum VehicleSearch
    {
        TransportChargeBehalf,
        TransportTripChargeList,
        TransportRequestsofBooking,
        TripSettlement
    }

    //contermet type for vehicle
    public enum ContermetType
    {
        KM,
        Mile
    }

    public enum GPSProvider
    {
        BINHANH = 1,
        ITRACKING = 2
    }

    public enum SMSProvider
    {
        Mitek,
        NodeJS
    }

    public enum LogType
    {
        AppLog,
        MobileLog,
        WebLog
    }

    public enum RouteType
    {
        OneWay,
        TwoWay,
        OneWay_TwoWay,
        TwoWay_OneWay
    }

    public enum DriverRole
    {
        Main,
        Sub,
        ReplaceMain
    }

    public enum UserPermission
    {
        AllowAccess = 0,
        Add = 1,
        Update = 2,
        Delete = 3,
        OpenWhenStartup = 4,
        UpdateDriverTel = 5,
        UpdateGPSProvider = 6,
        SetInactive = 7,
        Approve = 8,
        ConvertTransport = 9,
        ConfirmBookingHire = 10,
        CreditAndActivePartner = 11,
        ConfirmTransportRequest = 12,
        ReceiveEmailWhenBookingDeny = 13,
        GenerateKMGPS = 14,
        CheckFuel = 15,
        PreviewTripSettlement = 16,
        PreviewTransportRequest = 17,
        SendSMS = 18,
        ConfirmTransportDeny = 19,
        CreateAccidentRequest = 20,
        CreateCopyRequest = 21,
        SwapInternalHire = 22,
        CreateSubDriverRequest = 23,
        ViewIsDefaultVehicle = 25,
        RequestRateCard = 26,
        PreviewRateCard = 27,
        ReceiveEmailChangeVehicleRequest = 28,
        UpdateFuelParameter = 29,
        UpdateFixedCostParameter = 30,
        UpdateMNRCostParameter = 31,
        UpdateOverheadCostParameter = 32,
        CheckCustomerApproveQuotation = 34,
        CancelQuotation = 35,
        ConvertBooking = 36,
        PreviewPriceSaleRequest = 37,
        Preview = 38,
        SendEmail = 39,
        ReceiveSummaryEmailTransit = 40,
        ReceiveEmailTransitPlan = 41,
        KeyBill = 42,
        PaidFuel = 43,
        ShowAll = 44,
        SignOnPetrolReceipt = 45,
        ReceiveEmail = 46,
        ReceiveDetailEmailTransit = 53,
        ReProtectRevenue = 54,
        ExportRevenueSalesReport = 56,
        CopyVehiclePartDetail = 57,
        MoveVehicle = 58,
        Pay = 59,
        ExportMaintenanceHistory = 60,
        ExportMaintenancePlan = 61,
        CreateMaintenancePlanAuto = 62,
        CopyMaintenancePlan = 63,
        ExportContermetNew = 64,
        ReceiveTransitWarningEmail = 65,
        Export = 66,
        Upload = 67,
        AutoApproved = 68,
        UpdateSubsidizedFuel = 69,
        ReUpdateSurcharge = 73,
        ViewRevenueInKPI = 74,
        SetCredit = 75,
        Import = 76,
        Payment = 77,
        CheckCharge = 78,
        Generate = 79,
        UpdateAll = 80,
        ReceiveDetailEmailTransitAll = 81,
        ReceiveSummaryEmailTransitAll = 82,
        ChangeDebitedCustomer = 83
    }

    public enum Menu
    {
        acct,
        acctCurrecy,
        acctDriverAllowance,
        acctFuelTransaction,
        acctHireTransportApproval,
        acctParameter,
        acctPartnerDebit,
        acctPaymentRequest,
        acctPaymentRequestFuel,
        acctPaymentRequestFuelRejected,
        acctRejectedSurchargeTransportRequest,
        acctReporting,
        acctSOA,
        acctSOAList,
        audit,
        cat,
        catAccountBankOfPartner,
        catArea,
        catCharge,
        catCommodity,
        catCommodityGroup,
        catContainerType,
        catCountry,
        catCustomerBookingInfo,
        catDeliveryZoneCode,
        catDistrict,
        catDriver,
        catFCLVehicleDriver,
        catGeoCode,
        catOtherPlace,
        catPartner,
        catPickupZoneCode,
        catPlace,
        catPlaceDistance,
        catProvince,
        catRoute,
        catServiceTypeWeightRange,
        catShipmentNote,
        catTransitRouteMiddlePlace,
        catUnit,
        catVehicle,
        catVehicleDriver,
        catVehicleGroup,
        catVehiclePart,
        catVehiclePartType,
        catWard,
        catWeightRange,
        catZoneCode,
        cs,
        csChangedSurchargeLog,
        csDTBOrder,
        csDTBOrderChargeBehalf,
        csDTBOrderSurcharge,
        csDTBTransportSurcharge,
        csFCLBooking,
        csFCLBookingPlan,
        csFCLMergeDraftBooking,
        csFCLShipmentManagement,
        csFCLTransportChargeBehalf,
        csFCLTransportTripChargeList,
        csLCLTransitPlanReport,
        csOrder,
        csOrderDetailChargeBehalf,
        csOrderDetailExpense,
        csShipmentManagement,
        csTemplate,
        csTransportSurcharge,
        csUpdatingLockedSurcharge,
        csVoucherShipment,
        csWaybillCommission,
        lclCat,
        main,
        mainCloseMaintenanceRequest,
        mainContermetNumberNew,
        mainMaintenancePlace,
        mainMaintenanceQuota,
        mainMaintenanceRequest,
        mainMaintenanceType,
        mainReportMaintenance,
        mainVehicleMaintenance,
        mainVehicleMaintenanceMonthlyPlan,
        mainVehicleMaintenanceYearlyCostEstimation,
        mainVehicleMaintenanceYearlyPlan,
        mainVehicleNeedMaintaining,
        man,
        manAssignment,
        material,
        ops,
        opsCheckingIn,
        opsCheckingOut,
        opsCloseTransport,
        opsCommandDeny,
        opsDeliveryPlan,
        opsDeliveryRunSheet,
        opsDistributionPlan,
        opsDistributionRequest,
        opsDropPointPlanMap,
        opsDRSClosing,
        opsDTBTransportRequestClosing,
        opsDTBTripSettlement,
        opsDTBWawePickupPlan,
        opsFCLCommandDeny,
        opsFCLTransportRequestOfBooking,
        opsFCLTripSettlement,
        opsPickupPlan,
        opsPickupRunSheet,
        opsPRSClosing,
        opsReceptacle,
        opsRevenueProtection,
        opsRouting,
        opsTransit,
        opsTransitClosing,
        opsTripSettlement,
        opsUnbagging,
        opsUnlockTransportRequest,
        opsVehicleLocation,
        priceBuying,
        priceCost,
        priceDirectRouteCost,
        priceDistributionRateCard,
        priceDistributionRateCardTool,
        priceRateCard,
        priceRateCardList,
        priceTripBuying,
        pricing,
        pricingpriceFCLBuying,
        pricingRouteCost,
        sale,
        saleDistributionQuotation,
        saleForRentQuotation,
        saleLCLCreateQuotation,
        saleQuotation,
        saleQuotationList,
        saleSalesTarget,
        sys,
        sysBranch,
        sysBU,
        sysDriverAllowanceParameter,
        sysEmployee,
        sysFunctionPolicy,
        sysHub,
        sysMenu,
        sysMenuPermissionInstruction,
        sysRole,
        sysSMSLog,
        sysUpdate,
        sysUser,
        sysUserGroupPermission,
        sysUserLog
    }

    public enum UnitType
    {
        Weight,
        Volume,
        Quantity,
        Maintenance
    }

    public enum PlaceType
    {
        Ward,
        District,
        Province,
        Branch,
        Hub,
        Port,
        Depot,
        Warehouse,
        Other,
        IndustrialZone
    }

    public enum OrderSearch_DateCriteria
    {
        PickupDate,
        DeliveryDate,
        ReceivedDate,
        CreatedDate
    }

    public enum RoadID
    {
        Road,
        Rail,
        Sea,
        Air
    }

    public enum ShipmentType
    {
        FCL,
        LCL,
        Distribution,
        Both // LCL & FCL
    }

    public enum LCLQuotationType
    {
        Normal,
        Trial
    }

    public enum QuotationType
    {
        Internal,
        Trial,
        Sunday,
        Hire,
        VehicleForRent,
        NoneRevenue,
        MaintenanceRepair,
        Service
    }

    public enum ServiceType
    {
        DD = 1,
        DH = 2,
        HD = 3,
        HH = 4
    }

    public enum RateCardStatus
    {
        Updating = 19,
        Pending = 20,
        Accepted = 21,
        Rejected = 22,
        Revoked = 23,
        AutomaticallyAccepted = 66,
        AutomaticallyRejected = 75,
        AllowBooking = 76,
        Unknown = 9999
    }

    public enum QuotationStatus
    {
        Pending,
        Rejected,
        Accepted,
        Reversed,
        ExpiredDateContract,
        AllowBooking,
        Unknown
    }

    public enum ZoneCodeType
    {
        Pickup,
        Delivery
    }

    public enum ChargeType
    {
        HW = 1,
        GW = 2,
        VW = 3
    }

    public enum OrderStatus
    {
        Processing = 1,
        Pickedup = 2,
        Sorting = 3,
        Handovered = 4,
        Delivery = 5,
        Refilled = 6,
        Delivered = 7,
        Detained = 8,
        Returned = 9,
        Canceled = 10,
        Destroyed = 11,
        CustomsHold = 12,
        CustomsCleared = 13,
        Reschedule = 14,
        NoShow = 15
    }

    public enum OrderStatusReason
    {
        Confirmed = 1,
        Verifying = 2,
        AssignedPickup = 3,
        GSAConfirmedPickup = 4,
        PickedUp = 5,
        CheckedInOrigin = 6,
        Reweighed = 7,
        Routed = 8,
        Unbagging = 9,
        Bagging = 10,
        HandoverToBroker = 11,
        ForwardToEMS = 12,
        GSAToGSA = 13,
        GSAToDEO = 14,
        DEOToGSA = 15,
        BranchToHub = 16,
        HubToBranch = 17,
        HubToHub = 18,
        OriginHubCheckin = 19,
        DestHubCheckin = 20,
        DestBranchCheckin = 21,
        DestBranchCheckedOut = 22,
        AsignedDelivery = 23,
        AssignedHandover = 24,
        GSAConfirmedHandover = 25,
        GSAConfirmedDeliver = 26,
        PartialDelivery = 27,
        Delivered = 28,
        LateDelivery = 29,
        ConsigneeCloses = 30,
        ConsigneeOfForfHoliday = 31,
        ConsigneeBankrupted = 32,
        ConsigneeMoved = 33,
        CosigneeResigned = 34,
        PendingPayment = 35,
        CosigneeNotIn = 36,
        NoAttentionName = 37,
        WrongAddress = 38,
        AddressChanged = 39,
        WaitingForShipper = 40,
        DelayBadWeather = 41,
        DelayAccident = 42,
        DelayTrafficCause = 42,
        IncorrectRoute = 44,
        ShortRecieved = 45,
        RefuseShipment = 53,
        RefuseWrongOrder = 54,
        RefuseUnknowShipment = 55,
        ReturnedCompletely = 62,
        CustomerCanceled = 63,
        ShipperDestroyed = 64,
        DestroyedOverdue = 65,
        CustomsCleared = 75,
        RedeliveryAsShipperRequest = 76,
        RedeliveryAsReceiverRequest = 77,
        FirstRedelivery = 78,
        SecondRedelivery = 79,
        ThirdRedelivery = 80,
        FirstAttemptPickup = 81,
        SecondAttemptPickup = 82,
        ThirdAttemptPickup = 83,
        CustomerCanceledInProcessing = 84
    }

    public enum ApprovalPermissionInstruction
    {
        OPSPosition,
        CSPosition,
        OPSManPosition,
        ChiefAccountantPosition,
        HeadPosition,
        AccountantPosition,
        AllPosition,
        SaleManPosition,
        Booking,
        SellingPrice
    }

    public enum ExportPermissionInstruction
    {
        VehicleActivityReport = 89,
        SalaryOfDriver = 90,
        OutSourceReport = 91,
        KPILongHaul = 92,
        KPIShortHaul = 93,
        LCLOrderHistory = 94,
        RevenueSalesReport = 95,
        MaintenanceHistory = 96,
        MaintenancePlan = 97,
        LastContermetNumber = 98,
        PickupAndDeliverByTruck = 105,
        CustomerPaid = 119,
        CustomerPaidHistory = 120,
        CustomerDebt = 122
    }

    public enum ImportPermissionInstruction
    {
        CustomerPaid = 121
    }

    public enum UploadPermissionInstruction
    {
        EmployeeSignature = 102,
        DriverSignature = 106
    }
    public enum ReceiveEmailPermissionInstruction
    {
        NotifyingToUpdatingRateCard = 104,
        ReceiveVehicleActivityReport = 78,
        RemindingClosingTransportRequest = 107
    }

    public enum UpdateInstruction
    {
        ParentPartner,
        InformationAfterClosing,//Transport request,
        BookingOverDate
    }

    public enum TransportRequestType
    {
        Pickup,
        Transit,
        Delivery,
        FCLLongHaul,
        Distribution
    }

    public enum CheckingType
    {
        CheckIn,
        CheckOut
    }

    public enum CheckingItemType
    {
        Shipment,
        Receptacle
    }

    public enum CheckoutType
    {
        Transit,
        Delivery,
        Unknown
    }

    public enum MenuWorkPlace
    {
        Branch,
        Hub,
        Both
    }

    public enum VehicleOwner
    {
        ITLCorp,
        Outsource,
        PersonalVehicle,
        DriverOwn
    }

    public enum HaulType
    {
        LongHaul,
        ShortHaul
    }

    public enum ConfirmStatusVR
    {
        Accepted,
        AutoAccepted,
        AcceptedDeny,
        RejectedDeny,
        Rejected
    }

    public enum Unit
    {
        Kilogram = 4,
        CBM = 5,
        Pallet = 6,
        PerTrip = 7,
        Ton = 8,
        Gram = 9,
        Kit = 11,
        Liter = 12,
        Container = 13,
        Packaging = 14,
        Year = 21,
        Kilomet = 22,
        Month = 23
    }

    public enum RateCardCalculation
    {
        Unit,
        WeightRange
    }

    public enum BuyingType
    {
        PerTrip,
        PerWeight,
        FCL
    }

    public enum PaymentMethodBanking
    {
        Cash,
        Card,
        Receipt,
        ExtraReceipt
    }

    public enum Resource
    {
        DASHDAN,
        DASHHAN,
        DASHSGN
    }

    public enum SpecialZoneCode
    {
        PickupBranch,
        DeliveryBranch
    }

    public enum TransportDirection
    {
        Forward,
        Return,
        Unknown
    }

    public enum CodeHubBranch
    {
        VNDAN,
        VNHCM,
        VNHAN,
        DAN,
        HCM,
        HAN
    }

    public enum TypeUnlock
    {
        FuelUnlock,
        ChargeUnlock,
        TripUnlock,
        InfoUnlock,
        Unknown
    }

    public enum ListMenu
    {
        opsUnlockTransportRequest
    }

    public enum SOAAdjustmentType
    {

        Surcharge,
        AdjustedPrice,
        Weight
    }
    public enum GettingPriceMethod
    {
        Original,
        Conversion
    }

    public enum WaybillType
    {
        LCL,
        FCL,
        DTB
    }

    public enum ShipmentNote
    {
        Loading,
        SpecialHanding
    }
    public enum RoundedSOAMethod
    {
        Unit,
        Total,
        None
    }
    public enum ParameterGroup
    {
        selling_price,
        kpi_criteria,
        waring_criteria,
        cost_criteria,
        debit_credit,
        warning_contract_expiraton,
        soa_overdue,
        customer_debit,
        discount,
        booking_overdue
    }

    public enum WebCodeType
    {
        QuotationFCL,
        RateCard
    }
    public enum QuotedRouteType
    {
        Normal,
        AllInPriceAssociateRoute,
        AssociateRoute
    }
    public enum RateCardConditionRouteType
    {
        Main,
        ShortTrip
    }
    public enum Department
    {
        Accountant,
        Admin,
        Booking,
        ICT,
        OPS,
        Sale,
    }

    public enum PointType
    {
        Pickup,
        Delivery
    }

    public enum RouteCostStatus
    {
        NotSendRequest = 77,
        Pending = 78,
        Accepted = 79
    }

    public enum CustomerDebt
    {
        Total,
        Paid
    }

    public enum PaymentType
    {
        Transport,
        Fuel,
        Maintenance
    }
    public enum CashBehalfPlace
    {
        Pickup,
        Delivery
    }
    public enum ServiceTypeMapping
    {
        DDE = 5,
        DHE = 6,
        HDE = 7,
        HHE = 8
    }

}