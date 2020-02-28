import { PermissionShipment } from "./document/permissionShipment";

export class BaseModel {
    id: string = '';

    userCreated: string = '';
    datetimeCreated: string = '';
    userModified: string = '';
    datetimeModified: string = '';

    groupId: number = null;
    departmentId: number = null;
    officeId: string = '';
    companyId: string = '';
    permission: PermissionShipment = new PermissionShipment();

}

