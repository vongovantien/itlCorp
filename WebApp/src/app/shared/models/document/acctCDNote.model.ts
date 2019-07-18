import { CsShipmentSurcharge } from "./csShipmentSurcharge";

export class AcctCDNote {
  id: String = "00000000-0000-0000-0000-000000000000";
  code: String = null;
  jobId: String = null;
  branchId: String = "00000000-0000-0000-0000-000000000000";
  partnerId: String = null;
  partnerName: String = null;
  type: String = null;
  paymentDueDate: Date = null;
  customerPaid: Boolean = null;
  paidDate: Date = null;
  sentToCustomer: Boolean = null;
  sentByUser: String = null;
  sentOn: Date = null;
  total: Number = null;
  exportedDate: Date = null;
  unlockedDirector: String = null;
  unlockedDirectorStatus: String = null;
  unlockedDirectorDate: Date = null;
  unlockedSaleMan: String = null;
  unlockedSaleManStatus: String = null;
  unlockedSaleManDate: Date = null;
  invoiceNo: String = null;
  userCreated: String = null;
  datetimeCreated: Date = null;
  userModified: String = null;
  datetimeModified: Date = null;
  statementDate: Date = null;
  currencyId: String = null;
  customerConfirmDate: Date = null;
  numberDayOverDue: Number = null;
  alertNumberDayOverDueEmail: Boolean = null;
  paidPrice: Number = null;
  freightPrice: Number = null;
  behalfPrice: Number = null;
  paidFreightPrice: Number = null;
  paidBehalfPrice: Number = null;
  trackingTransportBill: String = null;
  trackingTransportDate: Date = null;
  listShipmentSurcharge: CsShipmentSurcharge[] = [];

}