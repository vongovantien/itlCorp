export class Permission {
    name: string = 'OPS Permission HCm';
    type: string = 'Standard';
    role: string = 'Operation';
    status: boolean = true;

    constructor(data?: any) {
        const self = this;
        for (const key in data) {
            if (self.hasOwnProperty(key)) {
                self[key] = data[key];
            }
        }
    }
}
