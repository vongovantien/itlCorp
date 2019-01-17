import { Container } from './container.model';

export class HouseBillOfLading {
    MasterBillOfLading:String = null;
    Customer:String = null;
    SaleMan:String = null;
    Shipper:String = null;
    Consignee:String = null;
    NotifyParty:String = null;
    HouseBillOfLadingNo:String = null;
    HouseBullOfLadingType:String = null;
    BookingNo:String = null;
    LocalVesselAndVoyNo:String = null;
    OceanVesselAndVoyNo:String = null;
    CountryOrigin:String = null;
    PlaceOfReceipt:String = null;
    PortOfLoading:String = null;
    PortOfDischarge:String = null;
    PlaceOfDelivery:String = null;
    FinalDestination:String = null;
    FreightPayment:String = null;
    ClosingDate:Date = null;
    SellingDate:Date = null;
    FreightPayableAt:String = null;
    ForwardingAgent:String = null;
    NumberOfOriginBL:Number = null;
    PlaceDateIssueHBL:String = null;
    ReferenceNo:String = null;
    ExportReferenceNo:String = null;
    DeliveryOfGoods:String = null;
    TypeOfMove :String = null;
    PurchaseOrderNo : String = null;
    TypeOfService : String = null;
    DescriptionOfGoods: String = null;
    ShippingMark : String = null;
    InWord:String = null;
    OnBoardStatus:String = null;
    listContainers:Container[]=[];
  }