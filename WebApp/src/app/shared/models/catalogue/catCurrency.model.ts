export class Currency {
  id: string = '';
  currencyName: string = '';
  isDefault: boolean = false;
  userCreated: string = '';
  datetimeCreated: string = '';
  userModified: string;
  datetimeModified: string = '';
  inactive: boolean = false;
  inactiveOn: string = '';

  constructor(data?: any) {
    const self = this;
    for (const key in data) {
      if (self.hasOwnProperty(key)) {
        self[key] = data[key];
      }
    }
  }
}