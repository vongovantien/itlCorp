
export class RuleLinkFee {
    id: string = '';
    nameRule: string = '';
    effectiveDate: any = null;
    expirationDate: any = null;
    
    status: boolean = null;
    userCreated: string = '';
    datetimeCreated: string = '';
    userModified: string = '';
    datetimeModified: string = '';
    serviceBuying : string = '';
    serviceSelling : string = '';
    chargeBuying : string = '';
    chargeSelling : string = '';
    partnerBuying : string = '';
    partnerSelling : string = '';
    userNameCreated : string = '';
    userNameModified: string = '';
    partnerNameBuying : string = '';
    partnerNameSelling : string = '';
    chargeNameBuying : string = '';
    chargeNameSelling : string = '';

    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
        }
    }
}
