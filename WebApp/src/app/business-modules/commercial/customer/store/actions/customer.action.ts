import { createAction, props } from '@ngrx/store';

export enum CustomerActionTypes {
    SEARCH_LIST = '[CustomerAction] Search List',
};

export const SearchList = createAction(CustomerActionTypes.SEARCH_LIST, props<{ payload: any }>());