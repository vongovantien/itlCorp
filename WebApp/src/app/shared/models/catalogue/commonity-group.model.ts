export class CommodityGroup {
    id: number;
    groupNameVn: string;
    groupNameEn: string;
    note: string;
    userCreated?: string;
    datetimeCreated?: Date;
    userModified?: string;
    datetimeModified?: Date;
    inactive: boolean;
    inactiveOn?: Date;
}