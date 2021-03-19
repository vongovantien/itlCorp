export class ProofOfDelivery {
    deliveryPerson: string = '';
    deliveryDate: any = null;
    note: string = '';
    referenceNo: string = '';
    hblid: string = '';
    constructor(object: any = {}) {
        const self = this;
        for (const key of Object.keys(object)) {
            if (self.hasOwnProperty(key.toString())) {
                self[key] = object[key];
            }
        }
    }
}
