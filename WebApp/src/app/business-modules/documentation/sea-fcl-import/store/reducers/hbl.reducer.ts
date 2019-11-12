import { HBlActions, HouseBillActionTypes } from "../actions";

export interface IHBLProfit {
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
    profitLocal: number;
    profitUSD: number;
}
export interface IHBLState {
    hbl: any;
    profit: IHBLProfit;
}

export const initHBlState: IHBLState = {
    hbl: {},
    profit: null
};

export function HBLReducer(state = initHBlState, action: HBlActions): IHBLState | any {
    switch (action.type) {
        case HouseBillActionTypes.GET_DETAIL_SUCCESS: {
            return { ...state, hbl: action.payload };
        }

        case HouseBillActionTypes.GET_PROFIT_SUCCESS: {
            return { ...state, ...state.profit, profit: action.payload };
        }

        default: {
            return state;
        }
    }
}
