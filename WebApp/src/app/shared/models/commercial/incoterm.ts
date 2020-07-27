import { CatChargeIncoterm } from "./charge-incoterm";
import { Permission } from "../system/permission";

export class Incoterm {
    id: string;
    code: string;
    nameEn: string;
    nameLocal: string;
    active: boolean;
    descriptionEn: string;
    descriptionLocal: string;
    service: string;
    datetimeCreated: string;
    datetimeModified: string;
    userCreated: string;
    userModified: string;
    userCreatedName: string;
    UserModifiedName: string;
}

export class IncotermUpdateModel {
    incoterm: Incoterm = null;
    sellings: CatChargeIncoterm[] = [];
    buyings: CatChargeIncoterm[] = [];

    constructor(object?: Object) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
        }
    }
}

