export class BravoVoucher {
    stt: string;
    branchCode: string;
    office: string;
    transcode: string;
    docDate: string;
    referenceNo: string;
    customerCode: string;
    customerName: string;
    customerMode: string;
    localBranchCode: string;
    currencyCode: string;
    exchangeRate: number;
    description0: string;
    dataType: string;
    details: BravoVoucherDetail[];
}

export class BravoVoucherDetail {
    rowId: string;
    ma_SpHt: string;
    itemCode: string;
    description: string;
    unit: string;
    currencyCode: string;
    exchangeRate: number;
    billEntryNo: string;
    masterBillNo: string;
    deptCode: string;
    nganhCode: string;
    quantity9: number;
    originalUnitPrice: number;
    taxRate: number;
    originalAmount: number;
    originalAmount3: number;
    obhPartnerCode: string;
    chargeType: string;
    accountNo: string;
    contraAccount: string;
    vatAccount: string;
    atchDocNo: string;
    atchDocDate: string;
    atchDocSerialNo: string;
}