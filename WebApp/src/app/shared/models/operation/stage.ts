
export class Stage {
    comment: string = '';
    datetimeCreated: string = '';
    deadline: string = '';
    departmentName: string = '';
    description: string = '';
    id: string = '';
    jobId: string = '';
    hblId: string = '';
    mainPersonInCharge: string = '';
    modifiedDate: string = '';
    name: string = '';
    orderNumberProcessed: number = 0;
    processTime: number = 0;
    realPersonInCharge: string = '';
    stageCode: string = '';
    stageId: number = 0;
    stageNameEN: string = '';
    status: string = '';
    userCreated: string = '';
    userModified: string = '';
    isSelected?: boolean = false;
    doneDate: string = '';
    hblno: string = '';
    type: string = '';

    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
        }
    }
}
