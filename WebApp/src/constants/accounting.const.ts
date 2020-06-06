export class AccountingConstants {
    public static PAYMENT_METHOD: CommonInterface.INg2Select[] = <CommonInterface.INg2Select[]>[
        { id: 'Cash', text: 'Cash' },
        { id: 'Bank Transfer', text: 'Bank Transfer' },
    ];

    public static VOUCHER_TYPE: CommonInterface.INg2Select[] = <CommonInterface.INg2Select[]>[
        { id: 'Debt Vouche', text: 'Debt Vouche' },
        { id: 'Bank', text: 'Bank' },
        { id: 'Other', text: 'Other' },
    ];

    public static ISSUE_TYPE = {
        INVOICE: 'Invoice',
        VOUCHER: 'Voucher',
    }

}
