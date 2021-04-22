import { createReducer, on, Action } from "@ngrx/store";
import { ReceiptInvoiceModel, Receipt } from "@models";
import * as ReceiptActions from "../actions";


export interface IReceiptState {
    list: Receipt[];
    invoices: ReceiptInvoiceModel[];
    debitList: any[],
    creditList: any[],
    isLoading: boolean;
    isLoaded: boolean;
}

export const initialState: IReceiptState = {
    list: [],
    debitList: [],
    creditList: [],
    invoices: [],
    isLoaded: false,
    isLoading: false
};

export const receiptManagementReducer = createReducer(
    initialState,
    on(ReceiptActions.InitInvoiceList, (state: IReceiptState) => ({
        ...state
    })),
    on(ReceiptActions.GetInvoiceList, (state: IReceiptState) => ({ ...state, isLoading: true })),
    on(ReceiptActions.GetInvoiceListSuccess, (state: IReceiptState, payload: any) => ({ ...state, invoices: payload.invoices, isLoading: false })),

    on(ReceiptActions.InsertAdvance, (state: IReceiptState, payload: any) => ({
        ...state,
        debit: [...state.debitList]
    })),
    on(ReceiptActions.RemoveInvoice, (state: IReceiptState, payload: any) => ({
        ...state, debitList: [...state.debitList.slice(0, payload.index), ...state.debitList.slice(payload.index + 1)]
    })),
    on(ReceiptActions.ProcessClear, (state: IReceiptState, payload: any) => ({
        ...state, debitList: [...state.debitList] // TODO implement
    }))
);

export function receiptReducer(state: IReceiptState | undefined, action: Action) {
    return receiptManagementReducer(state, action);
}
