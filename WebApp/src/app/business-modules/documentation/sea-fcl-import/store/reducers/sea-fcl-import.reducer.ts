import { CsTransaction } from "src/app/shared/models";
import { SeaFCLImportActionTypes, SeaFCLImportActions } from "../actions";


export function CSTransactionReducer(state = new CsTransaction(), action: SeaFCLImportActions): CsTransaction {
    switch (action.type) {
        case SeaFCLImportActionTypes.GET_DETAIL_SUCCESS: {
            return { ...state, ...action.payload }
        }

        default: {
            return state;
        }
    }
}