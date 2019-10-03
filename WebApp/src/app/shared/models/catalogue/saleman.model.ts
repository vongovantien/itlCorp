export class Saleman {
    id: string = '';
    saleman_ID: string = '';
    office: string = '';
    company: string = '';
    service: string = '';
    partnerId: string = '';
    createDate: string = '';
    status?: boolean = false;
    description: string = '';
    effectdate: any = '';

    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}