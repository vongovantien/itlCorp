export class WardModel {
    id: string;
    code: string;
    nameVn: string;
    nameEn: string;
    cityId: string;
    districtId: string;
    provinceNameEN: string;
    provinceNameVN: string;
    countryId: number;
    countryNameVN: string;
    countryNameEN: string;
    note: string;
    userCreated: string;
    datetimeCreated?: Date;
    userModified: string;
    datetimeModified?: Date;
    active?: boolean;
    inactiveOn?: Date;

    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}