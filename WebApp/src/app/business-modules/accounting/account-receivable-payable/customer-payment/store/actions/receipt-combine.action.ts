import { GeneralCombineReceiptModel, ReceiptInvoiceModel } from "@models";
import { createAction, props } from "@ngrx/store";
import { AgencyReceiptModel } from "src/app/shared/models/accouting/agency-receipt.model";

export enum ReceiptCombineActionTypes {
    SELECT_PARTNER_RECEIPT_COMBINE = '[AR Receipt Combine] Select Partner Combine',
    REGIST_GET_DEBIT_DETAIL = '[AR Receipt Combine] Regist Get Debit Detail',
    UPDATE_EXCHANGE_RATE = '[AR Receipt Combine] Update Exchange Rate',
    SELECT_SALEMAN = '[AR Receipt Combine] Select Saleman',
    ADD_GENERAL_COMBINE_TO_RECEIPT = '[AR Receipt Combine] Add General Combine To Receipt',
    ADD_CREDIT_COMBINE_TO_RECEIPT = '[AR Receipt Combine] Add Credit Combine To Receipt',
    ADD_DEBIT_COMBINE_TO_RECEIPT = '[AR Receipt Combine] Add Debit Combine To Receipt',
    IS_ADD_COMBINE_RECEIPT = '[AR Receipt] Is Add Combine Receipt',
    DEBIT_LIST_TYPE_RECEIPT = '[AR Receipt] Get Debit List Type Receipt',
    GET_CREDIT_INVOICE_LIST_SUCCESS = '[AR Receipt] Get Credit Invoice List Success',
    GET_DEBIT_INVOICE_LIST_SUCCESS = '[AR Receipt] Get Debit Invoice List Success',
    REMOVE_CREDIT_COMBINE = '[AR Receipt] Remove Credit Combine List',
    REMOVE_DEBIT_COMBINE = '[AR Receipt] Remove Debit Combine List',
    RESET_INVOICE = '[AR Receipt] Reset Invoice',
}

export const SelectPartnerReceiptCombine = createAction(ReceiptCombineActionTypes.SELECT_PARTNER_RECEIPT_COMBINE, props<{
    id: string,
    shortName: string,
    accountNo: string,
    partnerNameEn: string,
    salemanId: string,
    salemanName: string,
    contractId: string
}>());

export const RegistGetDebitType = createAction(ReceiptCombineActionTypes.REGIST_GET_DEBIT_DETAIL, props<{
    partnerId: string,
    officeId: string,
    listType: string
}>());

export const UpdateExchangeRateReceiptCombine = createAction(ReceiptCombineActionTypes.UPDATE_EXCHANGE_RATE, props<{ exchangeRate: number }>());
export const SelectedSalemanReceiptCombine = createAction(ReceiptCombineActionTypes.SELECT_SALEMAN, props<{ salemanId: string }>());
export const SelectedAgreementReceiptCombine = createAction(ReceiptCombineActionTypes.SELECT_SALEMAN, props<{ contractId: string }>());
export const AddGeneralCombineToReceipt = createAction(ReceiptCombineActionTypes.ADD_GENERAL_COMBINE_TO_RECEIPT, props<{ generalCombineList: GeneralCombineReceiptModel[] }>());
export const AddCreditCombineToReceipt = createAction(ReceiptCombineActionTypes.ADD_CREDIT_COMBINE_TO_RECEIPT, props<{ creditCombineList: ReceiptInvoiceModel[] }>());
export const AddDebitCombineToReceipt = createAction(ReceiptCombineActionTypes.ADD_DEBIT_COMBINE_TO_RECEIPT, props<{ debitCombineList: ReceiptInvoiceModel[] }>());
export const IsCombineReceipt = createAction(ReceiptCombineActionTypes.IS_ADD_COMBINE_RECEIPT, props<{ isCombineReceipt: boolean }>());
export const RegistDebitListTypeReceipt = createAction(ReceiptCombineActionTypes.DEBIT_LIST_TYPE_RECEIPT, props<{ listType: string }>());
export const RegistCreditInvoiceListSuccess = createAction(ReceiptCombineActionTypes.GET_CREDIT_INVOICE_LIST_SUCCESS, props<{ creditInvoiceList: AgencyReceiptModel }>());
export const RegistDebitInvoiceListSuccess = createAction(ReceiptCombineActionTypes.GET_DEBIT_INVOICE_LIST_SUCCESS, props<{ debitInvoiceList: AgencyReceiptModel }>());
export const RemoveDebitCombine = createAction(ReceiptCombineActionTypes.REMOVE_DEBIT_COMBINE, props<{ indexGrp: number, index: number, _typeList: string }>());
export const ResetCombineInvoiceList = createAction(ReceiptCombineActionTypes.RESET_INVOICE);