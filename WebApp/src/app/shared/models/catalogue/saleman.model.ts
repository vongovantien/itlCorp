export class Saleman {
    id: string = '';
    saleman_ID: string = '';
    office: string = '';
    company: string = '';
    service: string = '';
    partnerID: string = '';
    createDate: string = '';
    status?: boolean = false;
    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}