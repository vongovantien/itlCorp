export class AdvanceSettlement {
    orderNumberProcessed: number = 0;
    Requester: string = null;
    AdvanceNo: string = null;
    AdvanceAmount: number = 0;
    StatusApproval: string = null;
    SettlementNo: string = null;
    SettlementAmount: number = 0;
    SettleStatusApproval: string = null;
    Balance: number = 0;
    AdvanceDate: Date = null;
    SettlemenDate: Date = null;
    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
        }
    }
}