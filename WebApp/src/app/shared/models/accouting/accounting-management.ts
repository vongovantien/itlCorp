export class AccAccountingManagement {
    id: string = '00000000-0000-0000-0000-000000000000';
    partnerId: string = null;
    personalName: string = null;
    partnerAddress: string = null;
    description: string = null;
    voucherId: string = null;
    date: string = null;
    invoiceNoTempt: string = null;
    invoiceNoReal: string = null;
    serie: string = null;
    paymentMethod: string = null;
    voucherType: string = null;
    accountNo: string = null;
    totalAmount: number = 0;
    currency: string = null;
    status: string = null;
    attachDocInfo: string = null;
    type: string = null;
    groupId: number = 0;
    departmentId: number = 0;
    officeId: string = '00000000-0000-0000-0000-000000000000';
    companyId: string = '00000000-0000-0000-0000-000000000000';
    userCreated: string = null;
    datetimeCreated: string = null;
    userModified: string = null;
    datetimeModified: string = null;
    // tslint:disable-next-line: no-any
    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key.toString())) {
                self[key] = object[key];
            }
        }
    }
}

export class ChargeOfAccountingManagementModel {
    surchargeId: string = '00000000-0000-0000-0000-000000000000';
    chargeId: string = '00000000-0000-0000-0000-000000000000';
    chargeCode: string = null;
    chargeName: string = null;
    jobNo: string = null;
    hbl: string = null;
    contraAccount: string = null;
    orgAmount: number = 0;
    vat: number = 0;
    orgVatAmount: number = 0;
    vatAccount: string = null;
    currency: string = null;
    exchangeDate: string = null;
    finalExchangeRate: number = 0;
    exchangeRate: number = 0;
    amountVnd: number = 0;
    vatAmountVnd: number = 0;
    vatPartnerId: string = null;
    vatPartnerCode: string = null;
    vatPartnerName: string = null;
    vatPartnerAddress: string = null;
    obhPartner: string = null;
    invoiceNo: string = null;
    serie: string = null;
    invoiceDate: string = null;
    cdNoteNo: string = null;
    qty: number = 0;
    unitName: string = null;
    unitPrice: number = 0;
    mbl: string = null;
    soaNo: string = null;
    settlementCode: string = null;
    acctManagementId: string = '00000000-0000-0000-0000-000000000000';
    // tslint:disable-next-line: no-any
    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key.toString())) {
                self[key] = object[key];
            }
        }
    }
}

export class AccAccountingManagementModel extends AccAccountingManagement {
    charges: ChargeOfAccountingManagementModel[] = new Array<ChargeOfAccountingManagementModel>();
    // tslint:disable-next-line: no-any
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

export class PartnerOfAcctManagementResult {
    partnerId: string = null;
    partnerName: string = null;
    partnerAddress: string = null;
    settlementRequester: string = null;
    inputRefNo: string = null;
    charges: ChargeOfAccountingManagementModel[] = new Array<ChargeOfAccountingManagementModel>();
    // tslint:disable-next-line: no-any
    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key.toString())) {
                self[key] = object[key];
            }
        }
    }
}

export class PartnerOfAcctManagementCriteria {
    cdNotes: string[] = [];
    soaNos: string[] = [];
    jobNos: string[] = [];
    hbls: string[] = [];
    mbls: string[] = [];
    settlementCodes: string[] = [];
    // tslint:disable-next-line: no-any
    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key.toString())) {
                self[key] = object[key];
            }
        }
    }
}