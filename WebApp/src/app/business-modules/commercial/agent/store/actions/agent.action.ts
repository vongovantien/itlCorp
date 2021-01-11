import { createAction, props } from '@ngrx/store';

export enum AgentActionTypes {
    SEARCH_LIST = '[AgentAction] Search List',
};

export const SearchList = createAction(AgentActionTypes.SEARCH_LIST, props<{ payload: any }>());