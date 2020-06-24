
export class Contract {
    id: string = "00000000-0000-0000-0000-000000000000";
    saleManId: string = '';
    officeId: string = '';
    companyId: string = '';
    saleService: string = '';
    partnerId: string = '';
    username: string = '';
    officeNameEn: string = '';
    companyNameEn: string = '';
    effectiveDate: string = '';
    expiredDate: string = '';
    description: string = '';
    paymentMethod: string = '';
    contractNo: string = '';
    contractType: string = '';
    vas: string = '';
    trialCreditLimited: number = null;
    trialCreditDays: number = null;
    trialEffectDate: string = '';
    trialExpiredDate: string = '';
    paymentTerm: number = null;
    creditLimit: number = null;
    creditLimitRate: number = null;
    creditAmount: number = null;
    billingAmount: number = null;
    paidAmount: number = null;
    unpaidAmount: number = null;
    customerAdvanceAmount: number = null;
    creditRate: number = null;
    userCreated: string = '';
    datetimeCreated: string = '';
    userModified: string = '';
    datetimeModified: string = '';
    active: boolean = false;
    userModifiedName: string = '';
    userCreatedName: string = '';
    index: number = null;
    fileList: any[] = null;

    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}