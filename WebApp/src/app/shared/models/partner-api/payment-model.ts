export class PaymentModel {
    stt: string;
    brandCode: string;
    office: string;
    transcode: string;
    docDate: string;
    referenceNo: string;
    customerCode: string;
    customerName: string;
    currencyCode: string;
    exchangeRate: number;
    description0: string;
    dataType: string;
    details: PaymentDetailModel[];
}

export class PaymentDetailModel {
    rowId: string;
    originalAmount: number;
    description: string;
    obhPartnerCode: string;
    bankAccountNo: string;
    stt_Cd_Htt: string;
    chargeType: string;
}
