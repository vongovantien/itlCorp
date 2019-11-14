import { HBlActions, HouseBillActionTypes } from "../actions";

export interface IHBLState {
    hbl: any;
}

export const initHBlState: IHBLState = {
    hbl: {},
};

export function HBLReducer(state = initHBlState, action: HBlActions): IHBLState | any {
    switch (action.type) {
        case HouseBillActionTypes.GET_DETAIL_SUCCESS: {
            return { ...state, hbl: action.payload };
        }

        default: {
            return state;
        }
    }
}
