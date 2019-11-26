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
    isLoading: boolean;
    isLoaded: boolean;
}

export const initState: ITransactionState = {
    profits: [],
    isLoading: false,
    isLoaded: false
};


export function TransactionReducer(state = initState, action: TransactionActions): ITransactionState {
    switch (action.type) {
        case TransactionActionTypes.GET_PROFIT: {
            return {
                ...state,
                isLoaded: false,
                isLoading: true
            };
        }
        case TransactionActionTypes.GET_PROFIT_SUCCESS: {
            return {
                ...state,
                ...state.profits,
                profits: [...action.payload],
                isLoaded: true,
                isLoading: false

            };
        }

        default: {
            return state;
        }
    }
}