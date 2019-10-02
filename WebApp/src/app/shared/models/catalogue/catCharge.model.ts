export class Charge {
    id: string = '';
    code: string = '';
    chargeNameVn: string = '';
    chargeNameEn: string = '';
    serviceTypeId: string = '';
    type: string = '';
    currencyId: string = '';
    unitPrice: number = 0;
    unitId: number = 0;
    vatrate: number = 0;
    userCreated: string = '';
    datetimeCreated: string = '';
    userModified: string = '';
    datetimeModified: string = '';
    inactive: boolean;
    inactiveOn: string = '';

    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}

