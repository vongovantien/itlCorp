import { createAction, props } from "@ngrx/store";
import { ReceiptInvoiceModel } from "@models";

export enum ReceiptActionTypes {
    INIT_INVOICE = '[AR Receipt] Init Invoice List',
    GET_INVOICE = '[AR Receipt] Get Invoice',
    GET_INVOICE_SUCCESS = '[AR Receipt] Get Invoice Success',
    GET_INVOICE_FAIL = '[AR Receipt] Get Invoice Fail'


    // TODO another action receipt.
}

export const InitInvoiceList = createAction(ReceiptActionTypes.INIT_INVOICE);
export const GetInvoiceList = createAction(ReceiptActionTypes.GET_INVOICE);
export const GetInvoiceListSuccess = createAction(ReceiptActionTypes.GET_INVOICE_SUCCESS, props<{ invoices: ReceiptInvoiceModel[] }>());
export const GetInvoiceListFail = createAction(ReceiptActionTypes.GET_INVOICE_FAIL);
