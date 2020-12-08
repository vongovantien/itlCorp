import { Controller, Key } from "@decorators";

@Controller()
export class ReceiptInvoiceModel {
    @Key
    invoiceId: string;
    invoiceNo: string;
    currency: string;
    serieNo: string;
    invoiceDate: Date;
    unpaidAmount: number;
    type: string;
    paymentStatus: string;
    partnerName: string;
    billingDate: string;
}
