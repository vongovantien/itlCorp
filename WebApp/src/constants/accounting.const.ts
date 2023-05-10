export class AccountingConstants {
    public static PAYMENT_METHOD: string[] = <string[]>['Cash', 'Bank Transfer', 'Bank Transfer / Cash', 'Other'];
    public static PAYMENT_METHOD_2: CommonInterface.INg2Select[] = <CommonInterface.INg2Select[]>[
        { id: 'Cash', text: 'Cash' },
        { id: 'Bank Transfer', text: 'Bank Transfer' },
    ];

    public static VOUCHER_TYPE: string[] = <string[]>['Cash receipt', 'Cash payment', 'Debit slip', 'Credit slip', 'Purchasing note', 'Other entry'];
    public static ISSUE_TYPE = {
        INVOICE: 'Invoice',
        VOUCHER: 'Voucher',
    };

    public static STATUS_CD: string[] = <string[]>['New', 'Issued Invoice', 'Issued Voucher'];

    public static PAYMENT_STATUS = {
        PAID: 'Paid',
        PAID_A_PART: 'Paid A Part',
        UNPAID: 'Unpaid'
    };

    public static DEFAULT_ACCOUNT_NO_CODE: string = '13111';

    public static SYNC_STATUS = {
        SYNCED: 'Synced',
        REJECTED: 'Rejected'
    };

    public static RECEIPT_STATUS = {
        DRAFT: 'Draft',
        DONE: 'Done',
        CANCEL: 'Cancel'
    };

    public static STATUS_APPROVAL = {
        NEW: 'New',
        REQUEST_APPROVAL: 'Request Approval',
        LEADER_APPROVED: 'Leader Approved',
        DEPARTMENT_MANAGER_APPROVED: 'Department Manager Approved',
        ACCOUNTANT_MANAGER_APPROVED: 'Accountant Manager Approved',
        DONE: 'Done',
        DENIED: 'Denied'
    }

    public static DATE_TYPE: string[] = <string[]>['Create Date', 'Paid Date', 'Last Sync'];
    public static PAYMENT_TYPE: string[] = <string[]>['Payment', 'Invoice'];
    public static STATUS: string[] = <string[]>['Draft', 'Done', 'Cancel'];
    public static SYNC_STATUSS: string[] = <string[]>['Synced', 'Rejected'];

    public static RECEIPT_CLASS = {
        CLEAR_DEBIT: 'Clear Debit',
        ADVANCE: 'Advance',
        COLLECT_OBH: 'Collect OBH',
        COLLECT_OBH_OTHER: 'Collect OBH - OTHER',
        PAY_OBH: 'Pay OBH',
        NET_OFF: 'Net Off',
    }

    public static RECEIPT_PAYMENT_METHOD = {
        CASH: 'Cash',
        BANK: 'Bank Transfer',
        CLEAR_ADVANCE: 'Clear-Advance',
        CLEAR_ADVANCE_BANK: 'Clear-Advance-Bank',
        CLEAR_ADVANCE_CASH: 'Clear-Advance-Cash',
        INTERNAL: 'Internal',
        COLL_INTERNAL: 'COLL - Internal',
        OBH_INTERNAL: 'OBH - Internal',
        MANAGEMENT_FEE: 'Management Fee',
        OTHER_FEE: 'Other Fee',
        EXTRA: 'COLL - Extra',
        OTHER: 'Other',
        COLLECT_OBH_AGENCY: 'Collect OBH Agency',
        PAY_OBH_AGENCY: 'Pay OBH Agency' ,
        COLLECTED_AMOUNT: 'Collected Amount',
        ADVANCE_AGENCY: 'Advance Agency',
        BANK_FEE_AGENCY: 'Bank Fee Agency',
        RECEIVE_FROM_PAY_OBH: 'Receive From Pay OBH',
        RECEIVE_FROM_COLLECT_OBH: 'Receive From Collect OBH',
        CLEAR_CREDIT_FROM_OBH: 'Clear Credit From OBH',
        CLEAR_CREDIT_FROM_PAID_AMT: 'Clear Credit From Paid AMT',
        CLEAR_DEBIT_FROM_OBH: 'Clear Debit From OBH',
        CLEAR_DEBIT_FROM_PAID_AMT: 'Clear Debit From Paid AMT',
    }

    public static RECEIPT_ADVANCE_TYPE = {
        ADVANCE: 'ADV',
        COLLECT_OBH: 'COLL_OBH',
        PAY_OBH: 'PAY_OBH',
        COLLECT_OTHER: 'COLL_OTH',
        PAY_OTHER: 'PAY_OTH'
    }

    public static RECEIPT_PAYMENT_TYPE = {
        DEBIT: 'DEBIT',
        OBH: 'OBH',
        OTHER: 'OTHER',
        CREDIT: 'CREDIT',
        CREDITNOTE: 'CREDITNOTE',
        CREDITSOA: 'CREDITSOA'

    }

    public static GENERAL_RECEIPT_PAYMENT_METHOD: CommonInterface.INg2Select[] = <CommonInterface.INg2Select[]>[
        // { id: 'Collect OBH Agency', text: 'Collect OBH Agency' },
        // { id: 'Pay OBH Agency', text: 'Pay OBH Agency' },
        // { id: 'Collected Amount', text: 'Collected Amount' },
        // { id: 'Advance Agency', text: 'Advance Agency' },
        // { id: 'Bank Fee Agency', text: 'Bank Fee Agency' },
        // { id: 'Receive From Pay OBH', text: 'Receive From Pay OBH' },
        // { id: 'Receive From Collect OBH', text: 'Receive From Collect OBH' },
        { id: 'Collect OBH Agency', text: AccountingConstants.RECEIPT_PAYMENT_METHOD.COLLECT_OBH_AGENCY },
        { id: 'Pay OBH Agency', text: AccountingConstants.RECEIPT_PAYMENT_METHOD.PAY_OBH_AGENCY },
        { id: 'Collected Amount', text: AccountingConstants.RECEIPT_PAYMENT_METHOD.COLLECTED_AMOUNT },
        { id: 'Advance Agency', text: AccountingConstants.RECEIPT_PAYMENT_METHOD.ADVANCE_AGENCY },
        { id: 'Bank Fee Agency', text: AccountingConstants.RECEIPT_PAYMENT_METHOD.BANK_FEE_AGENCY },
        { id: 'Receive From Pay OBH', text: AccountingConstants.RECEIPT_PAYMENT_METHOD.RECEIVE_FROM_PAY_OBH },
        { id: 'Receive From Collect OBH', text: AccountingConstants.RECEIPT_PAYMENT_METHOD.RECEIVE_FROM_COLLECT_OBH },
    ];
    public static CREDIT_COMBINE_RECEIPT_PAYMENT_METHOD: CommonInterface.INg2Select[] = <CommonInterface.INg2Select[]>[
        { id: 'Clear Credit From OBH', text: AccountingConstants.RECEIPT_PAYMENT_METHOD.CLEAR_CREDIT_FROM_OBH },
        { id: 'Clear Credit From Paid AMT', text: AccountingConstants.RECEIPT_PAYMENT_METHOD.CLEAR_CREDIT_FROM_PAID_AMT }
    ];
    public static DEBIT_COMBINE_RECEIPT_PAYMENT_METHOD: CommonInterface.INg2Select[] = <CommonInterface.INg2Select[]>[
        { id: 'Clear Debit From OBH', text: AccountingConstants.RECEIPT_PAYMENT_METHOD.CLEAR_DEBIT_FROM_OBH },
        { id: 'Clear Debit From Paid AMT', text: AccountingConstants.RECEIPT_PAYMENT_METHOD.CLEAR_DEBIT_FROM_PAID_AMT }
    ];
    public static MAX_NUMBER_INT: number = 2147483647;
    public static MAX_NUMBER_DECIMAL: number = 99999999999999.9999;
}
