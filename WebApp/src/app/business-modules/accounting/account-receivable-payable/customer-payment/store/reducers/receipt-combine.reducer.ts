import { ReceiptInvoiceModel } from "@models";
import { Action, createReducer, on } from "@ngrx/store";
import { AgencyReceiptModel } from "src/app/shared/models/accouting/agency-receipt.model";
import * as ReceiptCombineActions from "../actions";
export interface IReceiptCombineState {
    creditInvoiceList: AgencyReceiptModel;
    debitInvoiceList: AgencyReceiptModel;
    generalCombineList: any[],
    debitCombineList: any[],//ReceiptInvoiceModel
    creditCombineList: any[],//ReceiptInvoiceModel[],
    isLoading: boolean;
    isLoaded: boolean;
    partnerId: string;
    paymentDate: any; // * Ngày tím kiếm -> Điều kiện search
    currency: string;
    exchangeRate: number;
    salemanId: string;
    contractId: string;
    isCombineReceipt: boolean;
    refPartnerId: string;
}

export const initialState: IReceiptCombineState = {
    creditInvoiceList: null,
    debitInvoiceList: null,
    generalCombineList: [],
    debitCombineList: [],
    creditCombineList: [],
    isLoading: false,
    isLoaded: false,
    partnerId: null,
    paymentDate: null,
    contractId: null,
    currency: 'USD',
    exchangeRate: null,
    salemanId: null,
    isCombineReceipt: false,
    refPartnerId: null,
    // listType: ''
};

export const receiptCombineManagementReducer = createReducer(
    initialState,
    on(ReceiptCombineActions.SelectPartnerReceiptCombine, (state: IReceiptCombineState, payload: {
        id: string,
        contractId: string,
    }) => ({ ...state, partnerId: payload.id, contractId: payload.contractId })),
    on(ReceiptCombineActions.RegistGetDebitType, (state: IReceiptCombineState, payload: {
        partnerId: string;
        officeId: string,
        listType: string
    }) => ({ ...state, refPartnerId: payload.partnerId, officeId: payload.officeId, listType: payload.listType })),
    on(ReceiptCombineActions.UpdateExchangeRateReceiptCombine, (state: IReceiptCombineState, payload: { exchangeRate: number }) => ({ ...state, exchangeRate: payload.exchangeRate })),
    on(ReceiptCombineActions.SelectedSalemanReceiptCombine, (state: IReceiptCombineState, payload: { salemanId: string }) => ({ ...state, salemanId: payload.salemanId })),
    on(ReceiptCombineActions.SelectedAgreementReceiptCombine, (state: IReceiptCombineState, payload: { contractId: string }) => ({ ...state, contractId: payload.contractId })),
    on(ReceiptCombineActions.IsCombineReceipt, (state: IReceiptCombineState, payload: { isCombineReceipt: boolean }) => ({ ...state, isCombineReceipt: payload.isCombineReceipt })),
    on(ReceiptCombineActions.RegistDebitListTypeReceipt, (state: IReceiptCombineState, payload: { listType: string }) => ({ ...state, listType: payload.listType })),
    on(ReceiptCombineActions.AddGeneralCombineToReceipt, (state: IReceiptCombineState, payload: any) => ({ ...state, generalCombineList: [...state.generalCombineList, ...payload.generalCombineList] })),
    on(ReceiptCombineActions.AddCreditCombineToReceipt, (state: IReceiptCombineState, payload: any) => ({ ...state, creditCombineList: [...state.creditCombineList, ...payload.creditCombineList] })),
    on(ReceiptCombineActions.AddDebitCombineToReceipt, (state: IReceiptCombineState, payload: any) => ({ ...state, debitCombineList: [...state.debitCombineList, ...payload.debitCombineList] })),
    on(ReceiptCombineActions.RegistCreditInvoiceListSuccess, (state: IReceiptCombineState, payload: any) => ({ ...state, creditInvoiceList: payload.creditInvoiceList })),
    on(ReceiptCombineActions.RegistDebitInvoiceListSuccess, (state: IReceiptCombineState, payload: any) => ({ ...state, debitInvoiceList: payload.debitInvoiceList })),
    on(ReceiptCombineActions.RemoveDebitCombine, (state: IReceiptCombineState, payload: any) => {
        if (payload._typeList === 'debit') {
            if (!state.debitCombineList[payload.indexGrp]?.status) {
                return { ...state, debitCombineList: [...state.debitCombineList.slice(0, payload.index), ...state.debitCombineList.slice(payload.index + 1)] };
            } else {
                const debitCombineListRemain = state.debitCombineList.filter((item, index) => index !== payload.indexGrp);
                const debitCombineListDelete = state.debitCombineList.filter((item, index) => index === payload.indexGrp).map(item => {
                    return { ...item, payments: [...item.payments.slice(0, payload.index), ...item.payments.slice(payload.index + 1)] }
                });
                return { ...state, debitCombineList: [...debitCombineListRemain, ...debitCombineListDelete.filter(item => item.payments.length !== 0)] }
            }
        } else if (payload._typeList === 'credit') {
            if (!state.creditCombineList[payload.indexGrp]?.status) {
                return { ...state, creditCombineList: [...state.creditCombineList.slice(0, payload.index), ...state.creditCombineList.slice(payload.index + 1)] };
            } else {
                const creditCombineListRemain = state.creditCombineList.filter((item, index) => index !== payload.indexGrp);
                const creditCombineListDelete = state.creditCombineList.filter((item, index) => index === payload.indexGrp).map(item => {
                    return { ...item, payments: [...item.payments.slice(0, payload.index), ...item.payments.slice(payload.index + 1)] }
                });
                return { ...state, creditCombineList: [...creditCombineListRemain, ...creditCombineListDelete.filter(item => item.payments.length !== 0)] }
            }
        } else if (payload._typeList === 'general') {
            return { ...state, generalCombineList: [...state.generalCombineList.slice(0, payload.index), ...state.generalCombineList.slice(payload.index + 1)] }
        }
    }),
    on(ReceiptCombineActions.ResetCombineInvoiceList, (state: IReceiptCombineState) => ({
        ...state, isLoading: false, isCombineReceipt: false, partnerId: null, agreementId: null
        , creditInvoiceList: null, debitInvoiceList: null, generalCombineList: [], creditCombineList: [], debitCombineList: []
    })),
)

export function receiptCombineReducer(state: IReceiptCombineState | undefined, action: Action) {
    return receiptCombineManagementReducer(state, action);
}