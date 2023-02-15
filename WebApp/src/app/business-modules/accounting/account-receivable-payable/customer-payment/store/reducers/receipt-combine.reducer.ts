import { Action, createReducer, on } from "@ngrx/store";
import { IAgreementReceipt } from "../../components/form-create-receipt/form-create-receipt.component";
import * as ReceiptCombineActions from "../actions";
export interface IReceiptCombineState {
    generalCombineList: any[],
    debitCombineList: any[],
    creditCombineList: any[],
    isLoading: boolean;
    isLoaded: boolean;
    partnerId: string;
    paymentDate: any; // * Ngày tím kiếm -> Điều kiện search
    agreementId: Partial<IAgreementReceipt>;
    currency: string;
    exchangeRate: number;
}

export const initialState: IReceiptCombineState = {
    generalCombineList: [],
    debitCombineList: [],
    creditCombineList: [],
    isLoading: false,
    isLoaded: false,
    partnerId: null,
    paymentDate: null,
    agreementId: null,
    currency: 'USD',
    exchangeRate: null
};

export const receiptCombineManagementReducer = createReducer(
    initialState,
    on(ReceiptCombineActions.SelectPartnerReceiptCombine, (state: IReceiptCombineState, payload: {
        id: string,
        shortName: string,
        accountNo: string,
        partnerNameEn: string
    }) => ({ ...state, partnerId: payload.id })),
    on(ReceiptCombineActions.UpdateExchangeRateReceiptCombine, (state: IReceiptCombineState, payload: { exchangeRate: number }) => ({ ...state, exchangeRate: payload.exchangeRate })),
)

export function receiptCombineReducer(state: IReceiptCombineState | undefined, action: Action) {
    return receiptCombineManagementReducer(state, action);
}