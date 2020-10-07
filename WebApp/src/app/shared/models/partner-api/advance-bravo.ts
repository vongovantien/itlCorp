export class BravoAdvance {
    stt: string;
    branchCode: string;
    office: string;
    transcode: string;
    docDate: string;
    referenceNo: string;
    customerCode: string;
    customerName: string;
    currencyCode: string;
    exchangeDate: string;
    description0: string;
    dataType: string;
    details: BravoAdvanceDetail[];
}


export class BravoAdvanceDetail {
    rowId: string;
    ma_SpHt: string;
    billEntryNo: string;
    masterBillNo: string;
    deptCode: string;  // SEA:ITLCS, AIR:ITLAIR,CUSTOM Logistic: ITLOPS
    nganhCode: string;
    originalAmount: number;
    description: string;
    chargetype: string; // Credit
}