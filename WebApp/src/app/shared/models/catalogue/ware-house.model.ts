import { BaseModel } from "../base.model";

export class Warehouse extends BaseModel {
    id: string = "00000000-0000-0000-0000-000000000000";
    code: string = null;
    nameEn: string = null;
    nameVn: string = null;
    countryID?: number = null;
    districtID?: string = null;
    provinceID?: string = null;
    countryName: string = null;
    provinceName: string = null;
    districtName: string = null;
    flightVesselNo: string = null;
    address: string = null;
    placeType: number = null;
    active?: boolean = null;

    constructor(data?: any) {
        super();
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}
