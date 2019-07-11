import * as _ from 'lodash';

export class User {
    id: string = '';
    username: string = '';
    password: string = '';
    userGroupId: string = '';
    employeeId: string = '';
    workPlaceId: string  = '';
    refuseEmail: string = '';
    ldapObjectGuid: string = '';
    userCreated: string = '';
    datetimeCreated: string = '';
    userModified: string = '';
    datetimeModified: string = '';
    inactive: string = '';
    inactiveOn: string = '';

    constructor(data?: any) {
        let self = this;
        _.forEach(data, (val, key) => {
            if (self.hasOwnProperty(key)) {
                self[key] = val;
            }
        });
      }
}