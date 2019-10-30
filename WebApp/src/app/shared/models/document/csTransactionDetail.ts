import { Container } from './container.model';

export class CsTransactionDetail {
  id: String = "00000000-0000-0000-0000-000000000000";
  jobId: String = "00000000-0000-0000-0000-000000000000";
  jobNo: String = null;//
  mawb: String = null;
  hwbno: String = null;
  hbltype: String = null;
  customerId: String = null;
  saleManId: String = null;
  shipperDescription: String = null;
  shipperId: String = null;
  consigneeDescription: String = null;
  consigneeId: String = null;
  notifyPartyDescription: String = null;
  notifyPartyId: String = null;
  alsoNotifyPartyDescription: String = null;
  alsoNotifyPartyId: String = null;
  customsBookingNo: String = null;
  localVoyNo: String = null;
  localVessel: String = null;
  oceanVoyNo: String = null;
  oceanVessel: String = null;
  originCountryId: Number = 0;
  pickupPlace: String = null;
  etd: String = null;
  eta: String = null;
  pol: String = null;
  pod: String = null;
  deliveryPlace: String = null;
  finalDestinationPlace: String = null;
  coloaderId: String = null;
  freightPayment: String = null;
  placeFreightPay: String = null;
  closingDate: String = null;
  sailingDate: String = null;
  forwardingAgentDescription: String = null;
  forwardingAgentId: String = null;
  goodsDeliveryDescription: String = null;
  goodsDeliveryId: String = null;
  originBlnumber: Number = 0;
  issueHblplace: String = null;
  issueHbldate: String = null;
  referenceNo: String = null;
  exportReferenceNo: String = null;
  moveType: String = null;
  purchaseOrderNo: String = null;
  serviceType: String = null;
  documentDate: String = null;
  documentNo: String = null;
  etawarehouse: String = null;
  warehouseNotice: String = null;
  shippingMark: String = null;
  remark: String = null;
  commodity: String = null;
  packageContainer: String = null;
  desOfGoods: String = null;
  netWeight: Number = 0;
  grossWeight: Number = 0;
  chargeWeight: Number = 0;
  cbm: Number = 0;
  active: Boolean = true;
  inactiveOn: String = null;
  inWord: String = null;
  onBoardStatus: String = null;
  manifestRefNo: String = null;
  userCreated: String = null;
  datetimeCreated: String = null;
  userModified: String = null;
  datetimeModified: String = null;
  csMawbcontainers: Container[] = [];

  customerName: String = null;
  saleManName: String = null;
  customerNameVn: String = null;
  saleManNameVn: String = null;
  forwardingAgentName: String = null;
  notifyParty: String = null;
  podName: String = null;
  containerNames: String = null;
  packageTypes: String = null;
  cw: Number = 0;
  gw: Number = 0;
  packages: String = null;
  containers: String = null;
  constructor(object?: any) {
    const self = this;
    for (const key in object) {
      if (self.hasOwnProperty(key.toString())) {
        self[key] = object[key];
      }
    }
  }
}