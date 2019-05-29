export class Warehouse { 
    id: string = "00000000-0000-0000-0000-000000000000";
    code: string;
    nameEn: string;
    nameVn: string;
    countryID?: number;
    districtID?: string;
    provinceID?:string;
    countryName: string;
    provinceName: string;
    districtName: string;
    address: string;
    placeType: number;
    inactive?: boolean;
}