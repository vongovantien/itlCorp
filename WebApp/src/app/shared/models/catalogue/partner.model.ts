export class Partner {
    id: string = '';
    partnerGroup: string = '';
    partnerNameVn: string = '';
    partnerNameEn: string = '';
    contactPerson: string = '';
    addressVn: string = '';
    addressEn: string = '';
    addressShippingVn: string = '';
    addressShippingEn: string = '';
    shortName: string = '';
    departmentId: string = '';
    countryId: number = 0;
    countryShippingId: number = 0;
    accountNo: string = '';
    tel: string = '';
    fax: string = '';
    taxCode: string = '';
    email: string = '';
    website: string = '';
    bankAccountNo: string = '';
    bankAccountName: string = '';
    bankAccountAddress: string = '';
    note: string = '';
    salePersonId: string = '';
    public: boolean = false;
    creditAmount: 0;
    debitAmount: 0;
    refuseEmail: true;
    receiveAttachedWaybill: true;
    roundedSoamethod: string = '';
    taxExemption: true;
    receiveEtaemail: true;
    showInDashboard: true;
    provinceId: string = '';
    provinceShippingId: string = '';
    parentId: string = '';
    percentCredit: number = 0;
    alertPercentCreditEmail: boolean = false;
    paymentBeneficiary: string = '';
    usingParrentRateCard: boolean = false;
    sugarId: string = '';
    bookingOverdueDay: number = 0;
    fixRevenueByProject: boolean = false;
    zipCode: string = '';
    zipCodeShipping: string = '';
    swiftCode: string = '';
    userCreated: string = '';
    datetimeCreated: string = '';
    userModified: string = '';
    datetimeModified: string = '';
    inactive?: boolean = false;
    inactiveOn?: string = '';
    workPlaceId: string = '';
    userCreatedName: string = '';

    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}