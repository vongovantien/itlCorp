import * as _ from 'lodash';

export class Stage {
    comment: string = '';
    createdDate: string = '';
    deadline:string = '';
    departmentName: string = '';
    description: string = '';
    id: string = '';
    jobId: string = '';
    mainPersonInCharge: string = '';
    modifiedDate: string = '';
    name: string = '';
    orderNumberProcessed: number = 0;
    processTime: number = 0;
    realPersonInCharge: string = '';
    stageCode:string = '';
    stageId: number = 0;
    stageNameEN: string = '';
    status:string = ''; 
    userCreated: string = '';
    userModified: string = '';
    doneDate: string = '';
 constructor(data?: any) {
    let self = this;
    _.forEach(data, (val, key) => {
        if (self.hasOwnProperty(key)) {
            self[key] = val;
        }
    });
  }
}