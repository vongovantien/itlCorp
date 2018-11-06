export class CountryModel {
    id: number;
    code: string;
    nameVn: string;
    nameEn: string;
    userCreated: string;
    datetimeCreated?: Date
    userModified: string;
    datetimeModified?: Date;
    inactive?: boolean;
    inactiveOn?: Date;
}