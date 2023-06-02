import { Partner, WorkOrderPriceModel, WorkOrderViewUpdateModel } from "@models";
import { createAction, props } from "@ngrx/store";
import { IWorkOrderCriteria } from "../../components/form-search/form-search-work-order.component";

export enum WorkOrderActionTypes {
    SEARCH = '[WO] Insert data search',
    LOAD_LIST = '[WO] Load List',
    LOAD_LIST_SUCCESS = '[WO] Load List Success',
    LOAD_LIST_FAIL = '[WO] Load List Success',

    INIT_WORK_ORDER = '[WO] Init Work Order',
    INIT_LIST_PRICE = '[WO] Init Price List',
    INIT_LIST_PRICE_ITEM = '[WO] Init Price Item',
    INIT_LIST_PRICE_ITEM_SURCHARGE_LIST = '[WO] Init Surcharge List',

    ADD_PRICE_ITEM = '[WO] Add Price Item',
    ADD_PRICE_ITEM_SUCCESS = '[WO] Add Price Item Success',
    SELECT_PRICE_ITEM_UPDATE = '[WO] Select Price Item Update',
    UPDATE_PRICE_ITEM = '[WO] Update Price Item',
    UPDATE_PRICE_ITEM_SUCCESS = '[WO] Update Price Item Success',

    RESET_UPDATE_PRICE_ITEM = '[WO] Reset Update Price Item',

    DELETE_PRICE_ITEM = '[WO] Delete Price Item',
    DELETE_PRICE_ITEM_SUCCESS = '[WO] Delete Price Item Success',
    DELETE_PRICE_ITEM_FAIL = '[WO] Delete Price Item Fail',

    LOAD_DETAIL = '[WO] Load Detail',
    LOAD_DETAIL_SUCCESS = '[WO] Load Detail Success',
    LOAD_DETAIL_FAIL = '[WO] Load Detail Fail',

    SELECT_PARTNER_WORK_ORDER = '[WO] Select Partner',
    SELECT_AGENT_WORK_ORDER = '[WO] Select Agent',
    SELECT_PARTNER_PRICE_ITEM = '[WO] Select Partner Price Item',
}

export const SearchListWorkOrder = createAction(WorkOrderActionTypes.SEARCH, props<Partial<IWorkOrderCriteria>>());
export const LoadListWorkOrder = createAction(WorkOrderActionTypes.LOAD_LIST, props<CommonInterface.IParamPaging>());
export const LoadListWorkOrderSuccess = createAction(WorkOrderActionTypes.LOAD_LIST_SUCCESS, props<CommonInterface.IResponsePaging>());
export const LoadListWorkOrderFail = createAction(WorkOrderActionTypes.LOAD_LIST_FAIL);

export const LoadDetailWorkOrder = createAction(WorkOrderActionTypes.LOAD_DETAIL, props<{ id: string }>());
export const LoadDetailWorkOrderSuccess = createAction(WorkOrderActionTypes.LOAD_DETAIL_SUCCESS, props<WorkOrderViewUpdateModel>());
export const LoadDetailWorkOrderFail = createAction(WorkOrderActionTypes.LOAD_DETAIL_FAIL);

export const InitWorkOrder = createAction(WorkOrderActionTypes.INIT_WORK_ORDER);
export const InitPriceListWorkOrder = createAction(WorkOrderActionTypes.INIT_LIST_PRICE, props<{ data: any[] }>());
export const InitPriceItemWorkOrder = createAction(WorkOrderActionTypes.INIT_LIST_PRICE_ITEM);
export const InitPriceItemSurchargeListWorkOrder = createAction(WorkOrderActionTypes.INIT_LIST_PRICE_ITEM_SURCHARGE_LIST, props<{ data: any[] }>());

export const AddPriceItemWorkOrder = createAction(WorkOrderActionTypes.ADD_PRICE_ITEM, props<{ data: WorkOrderPriceModel }>());
export const AddPriceItemWorkOrderSuccess = createAction(WorkOrderActionTypes.ADD_PRICE_ITEM_SUCCESS, props<{ data: WorkOrderPriceModel }>());
export const SelectPriceItemWorkOrder = createAction(WorkOrderActionTypes.SELECT_PRICE_ITEM_UPDATE, props<{ index: number }>());
export const UpdatePriceItemWorkOrder = createAction(WorkOrderActionTypes.UPDATE_PRICE_ITEM, props<{ data: WorkOrderPriceModel }>());
export const UpdatePriceItemWorkOrderSuccess = createAction(WorkOrderActionTypes.UPDATE_PRICE_ITEM_SUCCESS, props<{ data: WorkOrderPriceModel }>());

export const ResetUpdatePriceItemWorkOrder = createAction(WorkOrderActionTypes.RESET_UPDATE_PRICE_ITEM);

export const DeletePriceItemWorkOrder = createAction(WorkOrderActionTypes.DELETE_PRICE_ITEM, props<{ index: number, id: string }>());
export const DeletePriceItemWorkOrderSuccess = createAction(WorkOrderActionTypes.DELETE_PRICE_ITEM_SUCCESS, props<{ index: number }>());
export const DeletePriceItemWorkOrderFail = createAction(WorkOrderActionTypes.DELETE_PRICE_ITEM_FAIL);

export const SelectPartnerPriceItemWorkOrder = createAction(WorkOrderActionTypes.SELECT_PARTNER_PRICE_ITEM, props<{ data: Partner }>());
export const SelectPartnerWorkOrder = createAction(WorkOrderActionTypes.SELECT_PARTNER_WORK_ORDER, props<{ data: Partner }>());
export const SelectAgentWorkOrder = createAction(WorkOrderActionTypes.SELECT_AGENT_WORK_ORDER, props<{ data: Partner }>());
