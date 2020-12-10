import { createReducer, on, Action } from "@ngrx/store";
import { ReceiptInvoiceModel, Receipt } from "@models";
import { InitInvoiceList, GetInvoiceList, GetInvoiceListSuccess } from "../actions";


export interface IReceiptState {
    list: Receipt[];
    invoices: ReceiptInvoiceModel[];
    isLoading: boolean;
    isLoaded: boolean;
}

export const initialState: IReceiptState = {
    list: [],
    invoices: [],
    isLoaded: false,
    isLoading: false
};

export const receiptManagementReducer = createReducer(
    initialState,
    on(InitInvoiceList, (state: IReceiptState) => ({
        ...state
    })),
    on(GetInvoiceList, (state: IReceiptState) => ({ ...state, isLoading: true })),
    on(GetInvoiceListSuccess, (state: IReceiptState, payload: any) => ({ ...state, invoices: payload.invoices, isLoading: false })),
);

export function receiptReducer(state: IReceiptState | undefined, action: Action) {
    return receiptManagementReducer(state, action);
}
