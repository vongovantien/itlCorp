import { CsTransaction } from "src/app/shared/models";
import { SeaFCLExportActions, SeaFCLExportTypes } from "../actions/sea-fcl-export.action";

export interface ISeaFCLExportShipmentState {
    shipments: CsTransaction[];
    detail: any;
    isLoading: boolean;
    isLoaded: boolean;
}

export const initialState: ISeaFCLExportShipmentState = {
    shipments: new Array<CsTransaction>(),
    detail: null,
    isLoaded: false,
    isLoading: false
};

export function SeaFCLExportReducer(state = initialState, action: SeaFCLExportActions): ISeaFCLExportShipmentState {
    switch (action.type) {
        case SeaFCLExportTypes.LOAD_LIST: {
            return {
                ...state,
                isLoading: true,
                isLoaded: false
            };
        }

        case SeaFCLExportTypes.LOAD_LIST_SUCCESS: {
            return {
                ...state,
                shipments: action.payload,
                isLoading: false,
                isLoaded: true
            };
        }

        case SeaFCLExportTypes.GET_DETAIL: {
            return {
                ...state,
                isLoading: true,
            };
        }

        case SeaFCLExportTypes.GET_DETAIL_SUCCESS: {
            return {
                ...state,
                ...state.detail,
                detail: action.payload,
                isLoading: false,
                isLoaded: true
            };
        }

        case SeaFCLExportTypes.UPDATE: {
            return {
                ...state,
                isLoading: true,
            };
        }

        case SeaFCLExportTypes.UPDATE_SUCCESS: {
            return { ...state, ...action.payload };
        }

        default: {
            return state;
        }
    }
}

