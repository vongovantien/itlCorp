import { BaseModel } from "../base.model";
import { SystemConstants } from "@constants";

export class CatChargeIncoterm extends BaseModel {
    id: string = SystemConstants.EMPTY_GUID;
    incotermId: string = SystemConstants.EMPTY_GUID;
    chargeId: string = null;
    quantityType: string = null;
    unit: any = null;
    objectTo: string = null;
    currency: string = null;
    feeType: string = null;
    type: string = null;

    constructor(object: Object) {
        super();
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
        }
    }
}

