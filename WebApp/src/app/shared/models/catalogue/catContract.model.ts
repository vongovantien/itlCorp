
export class Contract {
    id: string = "00000000-0000-0000-0000-000000000000";
    saleManId: string = null;
    officeId: string = null;
    companyId: string = null;
    saleService: string = null;
    partnerId: string = null;
    username: string = null;
    officeNameEn: string = null;
    officeNameAbbr: string = null;
    companyNameEn: string = null;
    companyNameAbbr: string = null;
    effectiveDate: any = null;
    expiredDate: any = null;
    description: string = null;
    paymentMethod: string = null;
    contractNo: string = null;
    contractType: string = null;
    vas: string = null;
    trialCreditLimited: number = null;
    trialCreditDays: number = null;
    trialEffectDate: any = null;
    trialExpiredDate: any = null;
    paymentTerm: number = null;
    creditLimit: number = null;
    creditLimitRate: number = null;
    creditAmount: number = null;
    billingAmount: number = null;
    paidAmount: number = null;
    unpaidAmount: number = null;
    customerAdvanceAmount: number = null;
    creditRate: number = null;
    userCreated: string = null;
    datetimeCreated: string = null;
    userModified: string = null;
    datetimeModified: string = null;
    active: boolean = false;
    userModifiedName: string = null;
    userCreatedName: string = null;
    index: number = null;
    fileList: any = null;
    file: File = null;
    currencyId: string = null;
    isRequestApproval: boolean = false;
    partnerStatus: boolean = false;
    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}