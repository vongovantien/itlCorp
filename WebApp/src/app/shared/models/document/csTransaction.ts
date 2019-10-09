import { Container } from "src/app/shared/models/document/container.model";
import { CsTransactionDetail } from "src/app/shared/models/document/csTransactionDetail";
import { TransactionTypeEnum } from "../../enums/transaction-type.enum";

export class CsTransaction {
  id: string = "00000000-0000-0000-0000-000000000000";
  branchId: String = "00000000-0000-0000-0000-000000000000";
  transactionTypeEnum: TransactionTypeEnum;
  jobNo: String = null;
  mawb: String = null;
  typeOfService: String = null;
  etd: Date = null;
  eta: Date = null;
  mbltype: String = null;
  coloaderId: String = null; // supplier
  coloaderName: String = null;
  bookingNo: String = null;
  shippingServiceType: String = null;
  agentId: String = null; // agent 
  agentName: String = null;
  pol: String = null;
  polName: String = null;
  pod: String = null;
  podName: String = null;
  paymentTerm: String = null;
  loadingDate: Date = null;
  requestedDate: Date = null;
  flightVesselName: String = null;
  voyNo: String = null;
  flightVesselConfirmedDate: Date = null;
  shipmentType: String = null;
  serviceMode: String = null;
  commodity: String = null;
  invoiceNo: String = null;
  pono: String = null;
  personIncharge: String = null;
  personInChargeName: String = null;
  deliveryPoint: String = null;
  routeShipment: String = null;
  quantity: Number = null;
  unit: Number = null;
  grossWeight: number = 0;
  chargeWeight: number = 0;
  netWeight: number = 0;
  cbm: number = 0;
  containerSize: String = null;
  dimension: String = null;
  wareHouseId: String = null;
  notes: String = null;
  locked: Boolean = null;
  lockedDate: Date = null;
  userCreated: String = null;
  createdDate: Date = null;
  userModified: String = null;
  modifiedDate: Date = null;
  active: Boolean = true;
  inactiveOn: Date = null;
  packageContainer: String = '';
  desOfGoods: String = '';
  csMawbcontainers: Container[] = null;
  csTransactionDetails: CsTransactionDetail[] = null;

}