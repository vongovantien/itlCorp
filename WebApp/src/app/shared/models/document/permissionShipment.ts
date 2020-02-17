
export class PermissionShipment {

    allowUpdate: boolean = false;
    allowDelete: boolean = false;
    allowAddCharge: boolean = false;
    allowUpdateCharge: boolean = false;
    allowLock: boolean = false;
    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key.toString())) {
                self[key] = object[key];
            }
        }
    }
}