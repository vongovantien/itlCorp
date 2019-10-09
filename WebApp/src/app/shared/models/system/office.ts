export class Office {
    id: string = '';
    code: string = ''
    branchNameVn: string = '';
    branchNameEn: string = '';
    shortName: string = '';
    addressVn: string = '';
    addressEn: string = '';
    taxcode: string = '';
    companyName: string = '';
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
