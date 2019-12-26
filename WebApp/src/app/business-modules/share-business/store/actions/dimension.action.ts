import { Action } from '@ngrx/store';

/**
 * For each action type in an action group, make a simple
 * enum object for all of this group's action types.
 */
export enum DimensionActionTypes {
    GET_DIMENSION = '[DIMENSION] Get',
    GET_DIMENSION_SUCESS = '[DIMENSION] Get Success',
    GET_DIMENSION_FAIL = '[DIMENSION] Get Fail',
};

/**
 * Every action is comprised of at least a type and an optional
 * payload. Expressing actions as classes enables powerful 
 * type checking in reducer functions.
 */
export class GetDimensionAction implements Action {
    readonly type = DimensionActionTypes.GET_DIMENSION;

    constructor(public payload: any) { }
}

export class GetDimensionSuccessAction implements Action {
    readonly type = DimensionActionTypes.GET_DIMENSION_SUCESS;

    constructor(public payload: any) { }
}

export class GetDimensionFailAction implements Action {
    readonly type = DimensionActionTypes.GET_DIMENSION_FAIL;

    constructor(public payload: any) { }
}

/**
 * Export a type alias of all actions in this action group
 * so that reducers can easily compose action types
 */
export type DimensionActions
    = GetDimensionAction
    | GetDimensionSuccessAction
    | GetDimensionFailAction;
