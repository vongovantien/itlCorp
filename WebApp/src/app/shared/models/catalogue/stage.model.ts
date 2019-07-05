// import { moment } from "ngx-bootstrap/chronos/test/chain";
import * as _ from 'lodash';
export class StageModel {
    
    public id: number = -1;
    public code: string = '';
    public stageNameVn: string = '';
    public stageNameEn: string = '';
    public departmentId: number = 1;
    public descriptionVn: string = '';
    public descriptionEn: string = '';
    public userCreated: string = '';
    public datetimeCreated: string = '';
    public userModified: string = '';
    public datetimeModified: string = '';
    public inactive?: boolean = false;
    public inactiveOn?: string = '';

    constructor(data?: any) {
        let self = this;
        _.forEach(data, (val, key) => {
            if (self.hasOwnProperty(key)) {
                self[key] = val;
            }
        });
      }

}