import { SystemConstants } from "src/constants/system.const";

export class DIM {
    id: string = SystemConstants.EMPTY_GUID;
    length: number = null;
    width: number = null;
    height: number = null;
    package: number = null;
    hw: number = null;
    mblId: string = SystemConstants.EMPTY_GUID;
    hblId: string = SystemConstants.EMPTY_GUID;

    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key.toString())) {
                self[key] = object[key];
            }
        }
    }
}



