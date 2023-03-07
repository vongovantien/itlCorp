export class Bank {
    id: string = '';
    code: string = '';
    bankNameVn: string = '';
    bankNameEn: string = '';
    userCreated: string = '';
    datetimeCreated: string = '';
    userModified: string;
    datetimeModified: string = '';
    active: boolean = true;
    inactiveOn: string = '';
    userCreatedName: string = '';
    userModifiedName: string = '';
    beneficiaryAddress: string = '';
    approveStatus: string = '';
    swiftCode: string = '';
    bankAccountNo: string = '';
    bankAddress: string = '';
    partnerId: string = '';
    bankId: string = '';
    note: string = '';
    source: string = '';
    bankAccountName: string = '';
    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}
