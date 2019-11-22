import { CsTransaction } from "src/app/shared/models";
import { SeaFCLExportActions, SeaFCLExportTypes } from "../actions/sea-fcl-export.action";

export interface ISeaFCLExportShipmentState {
    shipments: CsTransaction[];
    isLoading: boolean;
    isLoaded: boolean;
}

export const initialState: ISeaFCLExportShipmentState = {
    shipments: new Array<CsTransaction>(),
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

        default: {
            return state;
        }
    }
}

