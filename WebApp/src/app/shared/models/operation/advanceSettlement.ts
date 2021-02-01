export class AdvanceSettlement {
    requester: string = '';
    advanceNo: string = '';
    advanceAmount: number = 0;
    statusApproval: string = '';
    settlementNo: string = '';
    settlementAmount: number = 0;
    settleStatusApproval: string = '';
    balance: number = 0;
    advanceDate: Date = null;
    settlemenDate: Date = null;
    adCurrency: string = '';
    setCurrency: string = '';
    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
        }
    }
}