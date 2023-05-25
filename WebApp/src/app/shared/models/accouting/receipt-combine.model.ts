import { Controller, Key } from "@decorators";
import { Receipt } from "@models";

export class GeneralCombineReceiptModel {
    id: string;
    partnerId?: string;
    agreementId?: string;
    paymentMethod?: string;
    amountVnd?: number;
    amountUsd?: number;
    obhpartnerId?: string;
    officeId?: string;
    notes?: string;
    duplicate?: boolean = false;
    duplicateOffice?: boolean = false;
    receiptNo?: string;
    userCreated?: string;
    datetimeModified?: Date;
    isModified: boolean;
    subArcbno: string;
}

export interface ICDCombine {
    id: string;
    partnerId?: string,
    partnerName?: string;
    billingNo?: string;
    // amount?: number;
    // amountVnd?: number;
    // amountUsd?: number;
    paidAmount?: number;
    paidAmountUsd?: number;
    paidAmountVnd?: number;
    unpaidAmount?: number;
    unpaidAmountVnd?: number;
    unpaidAmountUsd?: number;
    remainAmount?: number;
    remainAmountVnd?: number;
    remainAmountUsd?: number;
    hbl?: string;
    hblId?: string;
    mbl?: string;
    notes?: string;
    invoiceNo?: string;
    accMngtId?: string;
    voucherId?: string;
    refNo?: string;
    referenceNo?: string;
    [key: string]: any;
}

export interface IReceiptCombineGroup extends Receipt {
    officeName: string;
    paymentMethod: string;
    receiptNo: string;
    description: string;
    cdCombineList: ICDCombine[];
    sumTotal: {};
}