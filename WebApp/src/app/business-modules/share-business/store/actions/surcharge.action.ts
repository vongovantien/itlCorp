import { Action } from '@ngrx/store';
import { ISurcharge } from '../reducers';


export enum SurchargeActionTypes {
    ADD_BUYING = '[BUYING CHARGE] Add',
    DELETE_BUYING = '[BUYING CHARGE] Delete',
    SAVE_BUYING = '[BUYING CHARGE] SAVE'
}


export class AddBuyingSurchargeAction implements Action {
    readonly type = SurchargeActionTypes.ADD_BUYING;
    constructor(public payload: any) { }
}


export class DeleteBuyingSurchargeAction implements Action {
    readonly type = SurchargeActionTypes.DELETE_BUYING;
    constructor(public payload: number) { }
}

export class SaveBuyingSurchargeAction implements Action {
    readonly type = SurchargeActionTypes.SAVE_BUYING;
    constructor(public payload: any[]) { }
}

export type SurchargeAction = | AddBuyingSurchargeAction | DeleteBuyingSurchargeAction | SaveBuyingSurchargeAction;



