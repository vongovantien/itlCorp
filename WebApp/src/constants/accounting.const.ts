export class AccountingConstants {
    public static PAYMENT_METHOD: CommonInterface.INg2Select[] = <CommonInterface.INg2Select[]>[
        { id: 'Cash', text: 'Cash' },
        { id: 'Bank Transfer', text: 'Bank Transfer' },
    ];

    public static VOUCHER_TYPE: CommonInterface.INg2Select[] = <CommonInterface.INg2Select[]>[
        //{ id: 'Debt Voucher', text: 'Debt Voucher' },
        //{ id: 'Bank', text: 'Bank' },
        //{ id: 'Other', text: 'Other' },
        { id: 'Cash receipt', text: 'Cash receipt' },
        { id: 'Cash payment', text: 'Cash payment' },
        { id: 'Debit slip', text: 'Debit slip' },
        { id: 'Credit slip', text: 'Credit slip' },
        { id: 'Purchasing note', text: 'Purchasing note' },
        { id: 'Other entry', text: 'Other entry' },
    ];

    public static ISSUE_TYPE = {
        INVOICE: 'Invoice',
        VOUCHER: 'Voucher',
    }

    public static STATUS_CD: CommonInterface.INg2Select[] = <CommonInterface.INg2Select[]>[
        { id: 'New', text: 'New' },
        { id: 'Issued Invoice', text: 'Issued Invoice' },
        { id: 'Issued Voucher', text: 'Issued Voucher' },
    ];

}
