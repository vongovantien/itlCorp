import { TransactionActions, TransactionActionTypes } from "../actions";

export interface ITransactionProfit {
    hblid: string;
    hblNo: string;
    houseBillTotalCharge: {
        totalBuyingUSD: number,
        totalSellingUSD: number,
        totalOBHUSD: number,
        totalBuyingLocal: number,
        totalSellingLocal: number,
        totalOBHLocal: number
    };
    profitLocal: string;
    profitUSD: string;
}

export interface ITransactionState {
    profits: ITransactionProfit[];
}

export const initState: ITransactionState = {
    profits: []
};


export function TransactionReducer(state = initState, action: TransactionActions): ITransactionState | any {
    switch (action.type) {
        case TransactionActionTypes.GET_PROFIT_SUCCESS: {
            return { ...state, ...state.profits, profits: [...action.payload] };
        }
    }
}