import { HBLActionTypes, HBLActions } from "../actions";
import { Container } from "src/app/shared/models/document/container.model";

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
    hbls: any[];
    profit: IHBLProfit;
    containers: Container[];
    isLoading: boolean;
    isLoaded: boolean;
    jobId: string;
}

export const initHBlState: IHBLState = {
    hbl: {},
    hbls: [],
    isLoading: false,
    isLoaded: false,
    profit: null,
    containers: [],
    jobId: null
};

export function HBLReducer(state = initHBlState, action: HBLActions): IHBLState {
    switch (action.type) {
        case HBLActionTypes.INIT_LIST: {
            if (!state.jobId || !action.payload?.jobId || state.jobId !== action.payload.jobId) {
                return { ...state, isLoaded: false, hbls: [] };
            }
            return { ...state, isLoaded: true, isLoading: false };
        }
        case HBLActionTypes.GET_DETAIL: {
            return { ...state, isLoaded: false };
        }
        case HBLActionTypes.GET_DETAIL_SUCCESS: {
            return { ...state, hbl: action.payload, isLoading: false, isLoaded: true };
        }

        case HBLActionTypes.GET_LIST: {
            if (!state.jobId || !action.payload?.jobId || state.jobId !== action.payload.jobId) {
                return { ...state, isLoaded: false, isLoading: true, jobId: action.payload?.jobId };
            }
            return { ...state, jobId: action.payload?.jobId };

        }

        case HBLActionTypes.GET_LIST_SUCCESS: {
            return { ...state, hbls: action.payload, isLoaded: true, isLoading: false };
        }

        // Profit
        case HBLActionTypes.GET_PROFIT_SUCCESS: {
            return { ...state, profit: action.payload, isLoading: false, isLoaded: true };
        }

        // Container
        case HBLActionTypes.GET_CONTAINERS_SUCCESS: {
            return { ...state, containers: action.payload, isLoading: false, isLoaded: true };
        }
        case HBLActionTypes.INIT_PROFIT: {
            return { ...state, profit: null };
        }
        default: {
            return state;
        }
    }
}
