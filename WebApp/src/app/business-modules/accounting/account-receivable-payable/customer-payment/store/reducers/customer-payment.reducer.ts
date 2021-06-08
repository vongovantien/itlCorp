import { createReducer, on, Action } from "@ngrx/store";
import { ReceiptInvoiceModel, Receipt } from "@models";
import * as ReceiptActions from "../actions";


export interface IReceiptState {
    list: Receipt[];
    invoices: ReceiptInvoiceModel[];
    debitList: ReceiptInvoiceModel[],
    creditList: ReceiptInvoiceModel[],
    debitInvoice: ReceiptInvoiceModel[];
    isLoading: boolean;
    isLoaded: boolean;
    type: string;
}

export const initialState: IReceiptState = {
    list: [],
    debitList: [],
    creditList: [],
    invoices: [],
    debitInvoice: [],
    isLoaded: false,
    isLoading: false,
    type: null
};

export const receiptManagementReducer = createReducer(
    initialState,
    on(ReceiptActions.InitInvoiceList, (state: IReceiptState) => ({
        ...state
    })),
    on(ReceiptActions.GetInvoiceList, (state: IReceiptState) => ({ ...state, isLoading: true })),
    on(ReceiptActions.GetInvoiceListSuccess, (state: IReceiptState, payload: any) => ({
        ...state,
        creditList: [...payload.invoices.filter(x => x.type === 'CREDIT'), ...state.creditList],
        debitList: [...payload.invoices.filter(x => x.type === 'DEBIT' || x.type === 'OBH' || x.type === 'ADV'), ...state.debitList]
    })),
    on(ReceiptActions.RegistTypeReceipt, (state: IReceiptState, payload: any) => ({ ...state, type: payload.data })),
    on(ReceiptActions.ResetInvoiceList, (state: IReceiptState) => ({ ...state, creditList: [], debitList: [] })),
    on(ReceiptActions.InsertAdvance, (state: IReceiptState, payload: any) => ({
        ...state,
        debitList: [...state.debitList, payload.data]
    })),
    on(ReceiptActions.RemoveInvoice, (state: IReceiptState, payload: any) => ({
        ...state, debitList: [...state.debitList.slice(0, payload.index), ...state.debitList.slice(payload.index + 1)]
    })),
    on(ReceiptActions.RemoveCredit, (state: IReceiptState, payload: any) => ({
        ...state, creditList: [...state.creditList.slice(0, payload.index), ...state.creditList.slice(payload.index + 1)]
    })),
    on(ReceiptActions.ProcessClearSuccess, (state: IReceiptState, payload: any) => {
        if (payload.data.cusAdvanceAmountVnd > 0 || payload.data.cusAdvanceAmountUsd > 0) {
            const newInvoiceWithAdv: any = {
                typeInvoice: 'ADV',
                type: 'ADV',
                paidAmountVnd: payload.data.cusAdvanceAmountVnd,
                paidAmountUsd: payload.data.cusAdvanceAmountUsd,
                refNo: null
            };
            const advData = newInvoiceWithAdv as ReceiptInvoiceModel;
            return { ...state, debitList: [...payload.data.invoices, advData] };
        }
        return { ...state, debitList: [...payload.data.invoices] }// TODO implement
    }),
    on(ReceiptActions.ClearCredit, (state: IReceiptState, payload: { invoiceNo: string, creditNo: string }) => {
        const currentIndexCredit = state.creditList.findIndex(x => x.refNo == payload.creditNo);
        if (currentIndexCredit !== -1) {
            return {
                ...state, creditList: [
                    ...state.creditList.slice(0, currentIndexCredit),
                    {
                        ...state.creditList[currentIndexCredit],
                        invoiceNo: { payload }
                    },
                    ...state.creditList.slice(currentIndexCredit + 1)
                ]
            }

        }
    })
);

export function receiptReducer(state: IReceiptState | undefined, action: Action) {
    return receiptManagementReducer(state, action);
}
