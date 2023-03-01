
export class EDocFile {
    id: string = '';
    systemFileName: string = '';
    userFileName: string = '';
    billingNo: string = '';
    billingType: string = '';
    datetimeCreated: string = '';
    datetimeModified: string = '';
    userCreated: string = '';
    expiredDate: string = '';
    groupId: string = '';
    officeId: string = '';
    departmentId: string = '';
    userModified: string = '';
    hblid: string = '';
    jobId: string = '';
    documentTypeId: string = '';
    source: string = '';
    sysImageId: string = '';
    note: string = '';

    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
        }
    }
}
