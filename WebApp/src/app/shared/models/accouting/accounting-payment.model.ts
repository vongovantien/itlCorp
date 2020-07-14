export class AccountingPaymentModel {
    refId: string = null;
    invoiceNoReal: string = null;
    soaNo: string = null;
    partnerId: string = null;
    partnerName: string = null;
    amount: number = 0;
    currency: string = null;
    issuedDate: Date = null;
    serie: string = null;
    unpaidAmount: number = null;
    paidAmount: number = null;
    dueDate: Date = null;
    overdueDays: number = 0;
    status: string = null;
    extendDays: number = 0;
    extendNote: string = null;
    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}