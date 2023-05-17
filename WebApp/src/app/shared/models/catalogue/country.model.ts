export class CountryModel {
    id: number;
    code: string;
    nameVn: string;
    nameEn: string;
    userCreated: string;
    datetimeCreated?: Date
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