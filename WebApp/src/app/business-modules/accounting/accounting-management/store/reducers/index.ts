
import { Action } from "@ngrx/store";
import { IAccountingManagementState, accountingManagementReducer } from "../reducers/accounting-management.reducer";

export function reducer(state: IAccountingManagementState | undefined, action: Action) {
    return accountingManagementReducer(state, action);
}
