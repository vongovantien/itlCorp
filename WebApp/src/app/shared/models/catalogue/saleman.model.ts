export class Saleman {
    id: string = '';
    saleman_ID: any = '';
    office: string = '';
    company: string = '';
    service: string = '';
    partnerId: string = '';
    createDate: string = '';
    status?: boolean = false;
    description: string = '';
    effectDate: any = '';
    statusString: string = '';
    userCreated: string = '';
    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}