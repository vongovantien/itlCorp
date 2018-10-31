export class Commodity {
    id: number;
    commodityNameVn: string;
    commodityNameEn: string;
    commodityGroupId: number;
    commonityGroupNameVn: string;
    commonityGroupNameEn: Date
    note: string;
    userCreated?: string;
    datetimeCreated?: Date;
    userModified?: string;
    datetimeModified?: Date;
    inactive: boolean;
    inactiveOn?: Date;
}