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


}
