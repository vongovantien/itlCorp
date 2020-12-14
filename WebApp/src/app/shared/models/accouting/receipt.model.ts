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
    id: string = null;
    customerName: string = null;
    userNameCreated: string = null;
    userNameModified: string = null;
    paymentRefNo: string = null;
    paidAmount: string = null;
    paymentDate: string = null;
    status: string = null;
    currencyId: string = null;
    syncStatus: string = null;
    lastSyncDate: Date = null;
    description: string = null;
    userCreated: string = null;
    userModified: string = null;
    datetimeCreate: Date = null;
    datetimeModified: Date = null;
    reasonReject: string = null;


    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}

