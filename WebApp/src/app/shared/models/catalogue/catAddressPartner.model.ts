export class AddressPartner {
    id: string = "00000000-0000-0000-0000-000000000000";
    shortNameAddress: string = null;
    accountNo: string = null;
    shortName: string = null;
    taxCode: string = null;
    addressType: string = null;
    userCreated: string = null;
    datetimeCreated: string = null;
    userModified: string = null;
    datetimeModified: string = null;
    active: boolean = true;
    inactiveOn: string = null;
    userCreatedName: string = null;
    userModifiedName: string = null;
    contactPerson: string = null;
    tel: string = null;
    countryName: string = null;
    cityName: string = null;
    districtName: string = null;
    wardName: string = null;

    location: string = null;
    streetAddress: string = null;
    countryId: string = null;
    cityId: string = null;
    districtId: string = null;
    wardId: string = null;
    partnerId: string = null;
    index: number = null;
    
    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}
