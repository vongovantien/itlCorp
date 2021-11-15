import { CsShipmentSurcharge } from "./csShipmentSurcharge";
import { TransactionTypeEnum } from "../../enums/transaction-type.enum";

export class AcctCDNote {
  id: string = "00000000-0000-0000-0000-000000000000";
  code: string = null;
  jobId: string = null;
  branchId: string = "00000000-0000-0000-0000-000000000000";
  partnerId: string = null;
  partnerName: string = null;
  type: string = null;
  paymentDueDate: string = null;
  customerPaid: boolean = null;
  paidDate: string = null;
  sentToCustomer: boolean = null;
  sentByUser: string = null;
  sentOn: string = null;
  total: number = null;
  exportedDate: string = null;
  unlockedDirector: string = null;
  unlockedDirectorStatus: string = null;
  unlockedDirectorDate: string = null;
  unlockedSaleMan: string = null;
  unlockedSaleManStatus: string = null;
  unlockedSaleManDate: string = null;
  invoiceNo: string = null;
  userCreated: string = null;
  datetimeCreated: string = null;
  userModified: string = null;
  datetimeModified: string = null;
  statementDate: string = null;
  currencyId: string = null;
  customerConfirmDate: string = null;
  numberDayOverDue: number = null;
  alertNumberDayOverDueEmail: boolean = null;
  paidPrice: number = null;
  freightPrice: number = null;
  behalfPrice: number = null;
  paidFreightPrice: number = null;
  paidBehalfPrice: number = null;
  trackingTransportBill: string = null;
  trackingTransportDate: string = null;
  listShipmentSurcharge: CsShipmentSurcharge[] = [];
  transactionTypeEnum: TransactionTypeEnum = 0;
  flexId: string = null;
  status: string = null;
  note: string = null;
  officeId: string = null;
  companyId: string = null;
  // exchangeRate:number = null;
  excRateUsdToLocal:number = null;
}