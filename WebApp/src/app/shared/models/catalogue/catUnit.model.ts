export class Unit {
     id: any = null;
     code: string = '';
     unitNameVn: string = '';
     unitNameEn: string = '';
     unitType: string = '';
     userCreated: string = '';
     datetimeCreated: any = '';
     userModified: string = '';
     datetimeModified: any = '';
     active: boolean = true;
     inactiveOn: any = '';
     descriptionEn: string = '';
     descriptionVn: string = '';

     constructor(data?: any) {
          const self = this;
          for (const key in data) {
               if (self.hasOwnProperty(key)) {
                    self[key] = data[key];
               }
          }
     }
}