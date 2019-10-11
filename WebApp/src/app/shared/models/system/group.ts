export class Group {
    id: number = 0;
    code: string = '';
    nameEn: string = '';
    nameVn: string = '';
    departmentId: number = 0;
    parentId: number = 0;
    managerId: string = '';
    shortName: string = '';
    userCreated: string = '';
    datetimeCreated: Date = null;
    userModified: string = '';
    datetimeModified: Date = null;
    active: boolean = true;
    inactiveOn: Date = null;
    departmentName: string = '';
    constructor(data?: any) {
        console.log(data);
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}
