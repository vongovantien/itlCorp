namespace AccountingInterface {
    export interface IPartnerOfAccountingManagementRef {
        cdNotes: string[];
        soaNos: string[];
        jobNos: string[];
        hbls: string[];
        mbls: string[];
        settlementCodes: string[];
    }

    export interface IAccReceivableSearch {
        arType: number;
        acRefId: string;
        overDueDay: number;
        debitRateFrom?: number;
        debitRateTo?: number;
        agreementStatus: string;
        agreementExpiredDay: string;
        salesmanId: string;
        officeId: string;
    }
}
