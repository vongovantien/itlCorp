import { createReducer, on, Action } from "@ngrx/store";
import { ReceiptInvoiceModel, Receipt } from "@models";
import * as ReceiptActions from "../actions";
import { IAgreementReceipt } from "../../components/form-create-receipt/form-create-receipt.component";


export interface IReceiptState {
    list: { data: Receipt[], totalItems };
    debitList: ReceiptInvoiceModel[],
    creditList: ReceiptInvoiceModel[],
    isLoading: boolean;
    isLoaded: boolean;
    type: string;
    partnerId: string // * đối tượng của phiếu thu,
    date: any; // * Ngày tím kiếm -> Điều kiện search
    agreement: Partial<IAgreementReceipt>;
    dataSearch: any;
    pagingData: any;
    isAutoConvertPaid: boolean;
    currency: string;
}


export const initialState: IReceiptState = {
    //list:[],
    list: { data: [], totalItems: 0, },
    debitList: [],
    creditList: [],
    isLoaded: false,
    isLoading: false,
    partnerId: null,
    type: "CUSTOMER",
    date: null,
    agreement: {},
    dataSearch: null,
    pagingData: { page: 1, pageSize: 15 },
    isAutoConvertPaid: true,
    currency: 'VND'
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
    on(ReceiptActions.RegistTypeReceipt, (state: IReceiptState, payload: any) => ({
        ...state,
        type: payload.data,
        partnerId: !!payload.partnerId ? payload.partnerId : null
    })),
    on(ReceiptActions.ResetInvoiceList, (state: IReceiptState) => ({ ...state, creditList: [], debitList: [], agreement: {} })),
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
        if (state.currency === 'VND') {
            if (payload.data.cusAdvanceAmountVnd > 0) {
                return {
                    ...state, debitList: [...payload.data.invoices, {
                        typeInvoice: 'ADV',
                        type: 'ADV',
                        paidAmountVnd: payload.data.cusAdvanceAmountVnd,
                        paidAmountUsd: payload.data.cusAdvanceAmountUsd,
                        refNo: null
                    }]
                };
            }
        } else if (payload.data.cusAdvanceAmountVnd > 0) {
            return {
                ...state, debitList: [...payload.data.invoices, {
                    typeInvoice: 'ADV',
                    type: 'ADV',
                    paidAmountVnd: payload.data.cusAdvanceAmountVnd,
                    paidAmountUsd: payload.data.cusAdvanceAmountUsd,
                    refNo: null
                }]
            };
        }

        return { ...state, debitList: [...payload.data.invoices] }
    }),
    on(ReceiptActions.SelectPartnerReceipt, (state: IReceiptState, payload: { id: string, partnerGroup: string }) => ({
        ...state, partnerId: payload.id, type: payload.partnerGroup
    })),
    on(ReceiptActions.SelectReceiptDate, (state: IReceiptState, payload: { date: any }) => ({
        ...state, date: payload.date
    })),
    on(ReceiptActions.SelectReceiptAgreement, (state: IReceiptState, payload: { [key: string]: any }) => ({
        ...state, agreement: { ...payload }
    })),
    on(ReceiptActions.SearchListCustomerPayment, (state: IReceiptState, payload: any) => ({
        ...state, dataSearch: payload, pagingData: { page: 1, pageSize: 15 }
    })),
    on(ReceiptActions.LoadListCustomerPayment, (state: IReceiptState, payload: CommonInterface.IParamPaging) => ({
        ...state, isLoading: true, pagingData: { page: payload.page, pageSize: payload.size }
    })),
    on(ReceiptActions.LoadListCustomerPaymentSuccess, (state: IReceiptState, payload: CommonInterface.IResponsePaging) => ({
        ...state, list: payload, isLoading: false, isLoaded: true
    })),
    on(ReceiptActions.ToggleAutoConvertPaid, (state: IReceiptState, payload: { isAutoConvert: boolean }) => ({
        ...state, isAutoConvertPaid: payload.isAutoConvert
    })),
    on(ReceiptActions.SelectReceiptCurrency, (state: IReceiptState, payload: { currency: string }) => ({
        ...state, currency: payload.currency
    }))
);

export function receiptReducer(state: IReceiptState | undefined, action: Action) {
    return receiptManagementReducer(state, action);
}
