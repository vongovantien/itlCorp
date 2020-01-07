export class Saleman {
    id: string = '';
    saleManId: any = '';
    office: any = '';
    company: string = '';
    service: string = '';
    partnerId: string = '';
    createDate: string = '';
    status?: boolean = false;
    description: string = '';
    effectDate: any = '';
    statusString: string = '';
    userCreated: string = '';
    serviveName: string = '';
    username: string = '';
    freightPayment: string = '';
    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}