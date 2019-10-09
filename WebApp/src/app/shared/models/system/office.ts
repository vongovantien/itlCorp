export class Office {
    id: string = '';
    code: string = ''
    branchname_Vn: string = '';
    branchname_En: string = '';
    shortName: string = '';
    address_Vn: string = '';
    address_En: string = '';
    taxcode: string = '';
    companyname: string = '';
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
