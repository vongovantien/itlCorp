import { CsTransaction } from "src/app/shared/models";
import { SeaFCLImportActionTypes, SeaFCLImportActions } from "../actions";

export interface ICsTransaction {
    cstransaction: CsTransaction;
}

export const initState: ICsTransaction = {
    cstransaction: new CsTransaction(),
};


export function CSTransactionReducer(state = initState, action: SeaFCLImportActions): CsTransaction | any {
    switch (action.type) {
        case SeaFCLImportActionTypes.GET_DETAIL_SUCCESS: {
            return { ...state, cstransaction: action.payload };
        }

        case SeaFCLImportActionTypes.UPDATE_SUCCESS: {
            return { ...state, ...action.payload };
        }

        default: {
            return state;
        }
    }
}