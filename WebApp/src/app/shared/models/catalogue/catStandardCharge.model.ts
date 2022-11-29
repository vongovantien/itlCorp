export class StandardCharge {
    id: string = '';
    chargeId: string = '';
    code: string = '';
    quantity: number = 0;
    unitPrice: number = 0;
    currencyId: string = null;
    vatrate: number = 0;
    type: string = '';
    transactionType: string = '';
    service: string = '';
    serviceType: string = '';
    office: string = '';
    note: string = '';
    userCreated: string = '';
    datetimeCreated: string = '';
    userModified: string = '';
    datetimeModified: string = '';
    unitId: number = null;
    quantityType: any = null;
    chargeGroup: string = null;

    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}
