export class AddressPartner {
    id: string = "00000000-0000-0000-0000-000000000000";
    shortNameAddress: string = null;
    addressType: string = null;
    userCreated: string = null;
    datetimeCreated: string = null;
    userModified: string = null;
    datetimeModified: string = null;
    active: boolean = true;
    inactiveOn: string = null;
    userCreatedName: string = null;
    userModifiedName: string = null;

    location: string = null;
    streetAddress: string = null;
    codeCountry: string = null;
    codeCity: string = null;
    codeDistrict: string = null;
    codeWard: string = null;
    partnerId: string = null;
    
    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}
