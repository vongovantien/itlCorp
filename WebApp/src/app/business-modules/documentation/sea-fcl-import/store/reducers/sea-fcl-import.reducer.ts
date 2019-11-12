import { CsTransaction } from "src/app/shared/models";
import { SeaFCLImportActionTypes, SeaFCLImportActions } from "../actions";

export interface IProfit {
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
export interface ICsTransaction {
    cstransaction: CsTransaction;
    profits: IProfit;
}

export const initState: ICsTransaction = {
    cstransaction: new CsTransaction(),
    profits: null
};


export function CSTransactionReducer(state = initState, action: SeaFCLImportActions): CsTransaction | any {
    switch (action.type) {
        case SeaFCLImportActionTypes.GET_DETAIL_SUCCESS: {
            return { ...state, cstransaction: action.payload };
        }

        case SeaFCLImportActionTypes.UPDATE_SUCCESS: {
            return { ...state, ...action.payload };
        }

        case SeaFCLImportActionTypes.GET_PROFIT_SUCCESS: {
            return { ...state, profits: action.payload };
        }

        default: {
            return state;
        }
    }
}