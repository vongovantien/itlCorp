import { createAction, props } from "@ngrx/store";
import { IWorkOrderCriteria } from "../../components/form-search/form-search-work-order.component";
import { IPriceWorkOrder } from "../../components/popup/price-item/price-item-work-order.component";

export enum WorkOrderActionTypes {
    SEARCH = '[WO] Insert data search',
    LOAD_LIST = '[WO] Load List',
    LOAD_LIST_SUCCESS = '[WO] Load List Success',
    LOAD_LIST_FAIL = '[WO] Load List Success',

    INIT_LIST_PRICE = '[WO] Init Price List',
    INIT_LIST_PRICE_ITEM = '[WO] Init Price Item',
    INIT_LIST_PRICE_ITEM_SURCHARGE_LIST = '[WO] Init Surcharge List',

    ADD_PRICE_ITEM_SUCCESS = '[WO] Add Price Item',
    UPDATE_PRICE_ITEM = '[WO] Update Price Item',
    UPDATE_PRICE_ITEM_SUCCESS = '[WO] Update Price Item Success',

    DELETE_PRICE_ITEM = '[WO] Delete Price Item',
    // DELETE_PRICE_ITEM_SUCCESS = '[WO] Delete Price Item Success',
}

export const SearchListWorkOrder = createAction(WorkOrderActionTypes.SEARCH, props<Partial<IWorkOrderCriteria>>());
export const LoadListWorkOrder = createAction(WorkOrderActionTypes.LOAD_LIST, props<CommonInterface.IParamPaging>());
export const LoadListWorkOrderSuccess = createAction(WorkOrderActionTypes.LOAD_LIST_SUCCESS, props<CommonInterface.IResponsePaging>());
export const LoadListWorkOrderFail = createAction(WorkOrderActionTypes.LOAD_LIST_FAIL);

export const InitPriceListWorkOrder = createAction(WorkOrderActionTypes.INIT_LIST_PRICE, props<{ data: any[] }>());
export const InitPriceItemWorkOrder = createAction(WorkOrderActionTypes.INIT_LIST_PRICE_ITEM);
export const InitPriceItemSurchargeListWorkOrder = createAction(WorkOrderActionTypes.INIT_LIST_PRICE_ITEM_SURCHARGE_LIST, props<{ data: any[] }>());

export const AddPriceItemWorkOrderSuccess = createAction(WorkOrderActionTypes.ADD_PRICE_ITEM_SUCCESS, props<{ data: IPriceWorkOrder }>());
export const UpdatePriceItemWorkOrder = createAction(WorkOrderActionTypes.UPDATE_PRICE_ITEM, props<{ index: number }>());
export const UpdatePriceItemWorkOrderSuccess = createAction(WorkOrderActionTypes.UPDATE_PRICE_ITEM_SUCCESS, props<{ data: IPriceWorkOrder }>());

export const DeletePriceItemWorkOrder = createAction(WorkOrderActionTypes.DELETE_PRICE_ITEM, props<{ index: number }>());

