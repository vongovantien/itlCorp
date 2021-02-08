import { SystemConstants } from "@constants";
export class Surcharge {
    chargeId: string = '';
    chargeName: string = '';
    chargeCode: string = '';
    clearanceNo: string = '';
    contNo: string = '';
    currencyId: string = '';
    hbl: string = '';
    invoiceDate: any = null;
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
    id: string = SystemConstants.EMPTY_GUID;
    surchargeId: string = '';
    isSelected: boolean = false;
    hblid: string = '';
    type: string = '';
    typeOfFee: string = '';

    cdclosed: boolean = false;
    creditNo: string = '';
    debitNo: string = '';
    objectBePaid: string = '';
    paySoano: string = '';
    payerId: string = '';
    paymentObjectId: string = '';
    paymentRequestType: string = '';
    soaclosed: boolean = false;
    soano: string = '';

    jobNo: string = null;
    mblno: string = null;
    hblno: string = null;
    advanceNo: string = null;
    chargeGroup: string = null;

    shipmentId: string = SystemConstants.EMPTY_GUID; // * Id trong OpsTransation,CsTransation.
    typeService: string = null; // * "DOC | OPS"

    // * Custom
    obhId: string = null;
    isDuplicate: boolean = false;
    isChangeShipment: boolean = null;



    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
        }
    }
}

