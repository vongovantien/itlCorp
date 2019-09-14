export class Surcharge {
    chargeId: string = '';
    chargeName: string = '';
    chargeCode: string = '';
    clearanceNo: string = '';
    contNo: number = 0;
    currencyId: string = '';
    hbl: string = '';
    invoiceDate: string = '';
    invoiceNo: string = '';
    isFromShipment: boolean = false;
    jobId: string = '';
    mbl: string = '';
    notes: string = '';
    obhPartnerName: string = '';
    payer: string = '';
    quantity: number = 0;
    seriesNo: string = '';
    total: number = 0;
    typeCharge: string = '';
    unitId: number = 0;
    unitName: string = '';
    unitPrice: number = 0;
    vatrate: number = 0;
    settlementCode: string = '';
    id: string = "00000000-0000-0000-0000-000000000000";
    isSelected: boolean = false;
    hblid: string = '';
    type: string = '';
    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
        }
    }
}

