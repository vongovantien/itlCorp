import { SystemConstants } from "@constants";
import { PermissionShipment } from "../document/permissionShipment";
import { CommonEnum } from "@enums";

export class SetUnlockRequestModel {
    id: string = SystemConstants.EMPTY_GUID;
    subject: string = null;
    requester: string = null;
    unlockType: string = null;
    newServiceDate: string = null;
    generalReason: string = null;
    requestDate: string = null;
    requestUser: string = null;
    statusApproval: string = null;
    userCreated: string = null;
    datetimeCreated: string = null;
    userModified: string = null;
    datetimeModified: string = null;
    groupId: number = 0;
    departmentId: number = 0;
    officeId: string = SystemConstants.EMPTY_GUID;
    companyId: string = SystemConstants.EMPTY_GUID;
    jobs: SetUnlockRequestJobModel[] = new Array<SetUnlockRequestJobModel>();
    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key.toString())) {
                self[key] = object[key];
            }
        }
    }
}

export class SetUnlockRequestJobModel {
    id: string = SystemConstants.EMPTY_GUID;
    unlockRequestId: string = SystemConstants.EMPTY_GUID;
    unlockName: string = null;
    reason: string = null;
    job: string = null;
    unlockType: string = null;
    userCreated: string = null;
    datetimeCreated: string = null;
    // * Custom
    isSelected: boolean = false;
    constructor(object?: object) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key.toString())) {
                self[key] = object[key];
            }
        }
    }
}

export class UnlockJobCriteria {
    jobIds: string[] = [];
    mbls: string[] = [];
    customNos: string[] = [];
    advances: string[] = [];
    settlements: string[] = [];
    unlockTypeNum: CommonEnum.UnlockTypeEnum;
}

export class UnlockRequestCriteria {
    refenceNos: string[] = [];
    unlockTypeNum: CommonEnum.UnlockTypeEnum;
    requester: string = null;
    createdDate: string = null;
    statusApproval: string = null;
}
