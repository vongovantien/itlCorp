import { SystemConstants } from "@constants";
import { BaseModel } from "../base.model";

export class CatPotentialCustomer {
    id: string = SystemConstants.EMPTY_GUID;
    nameEn: string = null;
    nameLocal: string = null;
    taxcode: string = null;
    address: string = null;
    tel: string = null;
    email: string = null;
    active?: boolean = null;
    margin: number = null;
    quotation: number = null;
    potentialType: string = null;
    datetimeCreated?: Date = null;
    datetimeModified?: Date = null;
    userCreated: string = null;
    userModified: string = null;

    // Custom


}
export class CatPotentialModel extends CatPotentialCustomer {
    userCreatedName: string = null;
    UserModifiedName: string = null;

    constructor(object: Object) {
        super();
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
        }
    }
};

export class PotentialUpdateModel {
    potential: CatPotentialModel = null;
    constructor(object: Object) {

        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
        }
    }
}