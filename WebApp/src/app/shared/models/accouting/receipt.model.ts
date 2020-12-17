import { Controller, Key } from "@decorators";

@Controller()
export class ReceiptInvoiceModel {
    @Key
    invoiceId: string = null;
    invoiceNo: string = null;
    currency: string = null;
    serieNo: string = null;
    invoiceDate: Date = null;
    type: string = null;
    paymentStatus: string = null;
    partnerName: string = null;
    billingDate: string = null;
    taxCode: string = null;
    refAmount: number = null;
    refCurrency: string = null;
    note: string = null;

    unpaidAmount: number = null;
    receiptExcUnpaidAmount: number = null;//*  số tiền cần thu của invoice theo tỷ giá phiếu thu

    paidAmount: number = null;
    receiptExcPaidAmount: number = null; // * Số tiền thu của invoice theo tỷ giá phiếu thu

    invoiceBalance: number = null;
    receiptExcInvoiceBalance: number = null; // * Số tiền còn lại của inoice theo tỷ giá phiếu thu

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
    id: string = null;
    customerName: string = null;
    paymentRefNo: string = null;
    paidAmount: string = null;
    paymentDate: string = null;
    status: string = null;
    type: string = null;
    currencyId: string = null;
    syncStatus: string = null;
    lastSyncDate: Date = null;
    description: string = null;
    userCreated: string = null;
    userModified: string = null;
    datetimeCreated: Date = null;
    datetimeModified: Date = null;
    fromDate: Date = null;
    toDate: Date = null;
    customerId: string = null;
    agreementId: string = null;
    reasonReject: string = null;
    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key.toString())) {
                self[key] = object[key];
            }
        }
    }
}

export class ReceiptModel extends Receipt {
    customerName: string = null;
    payments: ReceiptInvoiceModel[] = [];
    userNameCreated: string = null;
    userNameModified: string = null;
    constructor(object?: any) {
        super();
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key.toString())) {
                self[key] = object[key];
            }
        }
    }
}

