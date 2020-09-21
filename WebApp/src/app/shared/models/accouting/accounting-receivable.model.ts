export class TrialOfficialOtherModel {
    agreementId: string = null;
    partnerId: string = null;
    partnerCode: string = null;

    partnerNameAbbr: string = null;

    agreementNo: string = null;
    agreementType: string = null;
    agreementStatus: string = null;

    expriedDate: Date = null;
    expriedDay: number = null;
    creditLimited: number = null;

    debitAmount: number = null;
    obhAmount: number = null;
    debitRate: number = null;

    billingAmount: number = null;
    billingUnpaid: number = null;
    paidAmount: number = null;

    over1To15Day: number = null;
    over16To30Day: number = null;
    over30Day: number = null;

    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}
export class GuaranteedModel {
    salesmanId: string = null;
    salesmanNameEn: string = null;
    salesmanFullName: string = null;
    totalCreditLimited: number = null;
    totalDebitAmount: number = null;
    totalDebitRate: number = null;
    totalBillingAmount: number = null;
    totalBillingUnpaid: number = null;
    totalPaidAmount: number = null;
    totalObhAmount: number = null;
    totalOver1To15Day: number = null;
    totalOver16To30Day: number = null;
    totalOver30Day: number = null;
    arPartners: TrialOfficialOtherModel[] = [];
    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}
export class AccReceivableDetailModel {
    agreementCurrency: string = null;
    agreementId: string = null;
    agreementNo: string = null;
    agreementSalesmanId: string = null;
    agreementSalesmanName: string = null;
    agreementStatus: string = null;
    agreementType: string = null;
    arCurrency: string = null;
    arServiceCode: string = null;
    arServiceName: string = null;
    billingAmount: number = null;
    billingUnpaid: number = null;
    creditAmount: number = null;
    creditLimited: number = null;
    creditRateLimit: number = null;
    creditTerm: number = null;
    cusAdvance: number = null;
    debitAmount: number = null;
    debitRate: number = null;
    effectiveDate: Date = null;
    expriedDate: Date = null;
    expriedDay: number = null;
    obhAmount: number = null;
    officeId: string = null;
    over1To15Day: number = null;
    over16To30Day: number = null;
    over30Day: number = null;
    paidAmount: number = null;
    partnerCode: string = null;
    partnerId: string = null;
    partnerNameAbbr: string = null;
    partnerNameEn: string = null;
    partnerNameLocal: string = null;
    partnerStatus: string = null;
    saleCreditLimited: number = null;
    saleDebitAmount: number = null;
    saleDebitRate: number = null;
    taxCode: string = null;
    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}
export class AccReceivableOfficesDetailModel {
    accountReceivableGrpServices: AccReceivableServicesDetailModel[] = [];
    officeId: string = null;
    officeName: string = null;
    totalBillingAmount: number = null;
    totalBillingUnpaid: number = null;
    totalDebitAmount: number = null;
    totalObhAmount: number = null;
    totalOver1To15Day: number = null;
    totalOver15To30Day: number = null;
    totalOver30Day: number = null;
    totalPaidAmount: number = null;
    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}
export class AccReceivableServicesDetailModel {
    billingAmount: number = null;
    billingUnpaid: number = null;
    debitAmount: number = null;
    obhAmount: number = null;
    officeId: string = null;
    over1To15Day: number = null;
    over16To30Day: number = null;
    over30Day: number = null;
    paidAmount: number = null;
    serviceName: string = null;
    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}