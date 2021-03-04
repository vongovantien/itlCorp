import { createAction, props } from '@ngrx/store';

export enum PartnerDataActionTypes {
    SEARCH_LIST = '[PartnerDataAction] Search List',
};

export const SearchList = createAction(PartnerDataActionTypes.SEARCH_LIST, props<{ payload: any }>());