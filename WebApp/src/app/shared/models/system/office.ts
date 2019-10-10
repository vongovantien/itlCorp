export class Office {
    id: string = '';
    branchNameVn: string = '';
    bankAccountName: string = '';
    branchNameEn: string = '';
    buid: string = '';
    addressVn: string = '';
    addressEn: string = '';
    tel: string = '';
    fax: string = '';
    email: string = '';
    taxcode: string = '';
    bankAccountVnd: string = '';
    bankAccountUsd: string = '';
    bankName: string = '';
    bankAddress: string = '';
    code: string = '';
    swiftCode: string = '';
    shortName: string = '';
    active: boolean = false;

    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}
