import { Controller, Key } from "@decorators";

@Controller()
export class ReceiptInvoiceModel {
    @Key
    invoiceId: string = null;
    invoiceNo: string = null;
    currency: string = null;
    serieNo: string = null;
    invoiceDate: Date = null;
    unpaidAmount: number = null;
    receiptExcUnpaidAmount: number = null;

    type: string = null;
    paymentStatus: string = null;
    partnerName: string = null;
    billingDate: string = null;
    paidAmount: number = null;
    invoiceBalance: number = null;
    taxCode: string = null;
    refAmount: number = null;
    refCurrency: string = null;
    note: string = null;
    // * custom
    isSelected: boolean = false;


    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key.toString())) {
                self[key] = object[key];
            }
        }
    }

}
@Controller()
export class Receipt {
    @Key
    id: string;
    customerName: string;
    UserNameCreated: string;
    UserNameModified: string;
    PaymentRefNo: string;
    PaidAmount: string;
    paidDate: string;
    status: string;
    currency: string;
    syncStatus: string;
    lastSyncDate: Date;
    description: string;
    userCreated: string;
    userModified: string;
    datetimeCreate: Date;
    datetimeModified: Date;
}

