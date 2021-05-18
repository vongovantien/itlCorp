import { CustomDeclaration, OpsTransaction } from "@models";
import { ClearanceActions, ClearanceActionTypes } from "../actions/custom-clearance.action";

export interface ICustomDeclarationState {
    customDeclarations: any;
    dataSearch: any;
    isLoading: boolean;
    isLoaded: boolean;
    pagingData: any;
};

const initialState: ICustomDeclarationState = {
    customDeclarations: { data: [], totalItems: 0 },
    dataSearch: {},
    isLoaded: false,
    isLoading: false,
    pagingData: { page: 1, pageSize: 15 }
};

export function opsReducer(state = initialState, action: ClearanceActions): ICustomDeclarationState {
    switch (action.type) {
        case ClearanceActionTypes.SEARCH_LIST: {
            return { ...state, dataSearch: action.payload, isLoading: true, isLoaded: false, pagingData: { page: 1, pageSize: 15 } };
        }

        default: {
            return state;
        }
    }
}