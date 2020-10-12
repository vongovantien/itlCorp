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

    export interface IDefaultSearchAcctMngt {
        typeOfAcctManagement: string;
        fromIssuedDate: string;
        toIssuedDate: string;
    }

    export interface IRequestGuid {
        Id: string;
        action: string;
    }

    export interface IRequestInt {
        Id: string;
        action: string;
    }
}
