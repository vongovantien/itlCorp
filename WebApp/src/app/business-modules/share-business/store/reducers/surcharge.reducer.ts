import { CsShipmentSurcharge } from "src/app/shared/models";
import { SurchargeAction, SurchargeActionTypes } from "../actions";

export interface ISurcharge {
    buyings: CsShipmentSurcharge[];
}

export const initialState: ISurcharge = {
    buyings: []
};


export function SurchargeReducer(state = initialState, action: SurchargeAction): ISurcharge {
    switch (action.type) {
        case SurchargeActionTypes.ADD_BUYING: {
            return {
                buyings: [...state.buyings, action.payload]
            };
        }
        case SurchargeActionTypes.DELETE_BUYING: {
            return { ...state, ...state.buyings.splice(action.payload, 1) };
        }

        case SurchargeActionTypes.SAVE_BUYING: {
            return {
                buyings: action.payload
            };
        }

        default: {
            return state;
        }
    }
}
