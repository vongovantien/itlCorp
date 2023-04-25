import { PermissionShipment, WorkOrderPriceModel, WorkOrderViewUpdateModel } from "@models";
import { Action, createReducer, on } from "@ngrx/store";
import * as WorkOrderActionTypes from '../actions';


export interface IWorkOrderDetailState {
    listPrice: WorkOrderPriceModel[]
    active: boolean;
    isLoading: boolean;
    transactionType: string;
    status: string;
    permission: Partial<PermissionShipment>;
    code: string;
    transactionTypeName: string;

}

const initialState: IWorkOrderDetailState = {
    code: null,
    transactionTypeName: null,
    listPrice: [],
    isLoading: true,
    transactionType: null,
    active: null,
    status: null,
    permission: {
        allowUpdate: null
    }
};

export const reducer = createReducer(
    initialState,
    on(WorkOrderActionTypes.LoadDetailWorkOrder, (state: IWorkOrderDetailState) => ({
        ...state, isLoading: true
    })),
    on(WorkOrderActionTypes.InitWorkOrder, (state: IWorkOrderDetailState, payload: { type: string, data: any[] }) => ({
        ...initialState
    })),
    on(WorkOrderActionTypes.LoadDetailWorkOrderSuccess, (state: IWorkOrderDetailState, payload: WorkOrderViewUpdateModel) => ({
        ...state,
        transactionType: payload.transactionType,
        listPrice: payload.listPrice,
        isLoading: false,
        active: payload.active,
        permission: payload.permission,
        transactionTypeName: payload.transactionTypeName,
        code: payload.code
    })),

    on(WorkOrderActionTypes.InitPriceListWorkOrder, (state: IWorkOrderDetailState, payload: { type: string, data: any[] }) => ({
        ...state, listPrice: payload.data, isLoading: false
    })),
    on(WorkOrderActionTypes.AddPriceItemWorkOrderSuccess, (state: IWorkOrderDetailState, payload: { data: WorkOrderPriceModel }) => ({
        ...state, listPrice: [...(state.listPrice || []), payload.data]
    })),
    on(WorkOrderActionTypes.SelectPriceItemWorkOrder, (state: IWorkOrderDetailState, payload: { index: number }) => {
        const newArrayPriceList = [...state.listPrice];
        newArrayPriceList[payload.index].mode = 'UPDATE';

        return { ...state, listPrice: newArrayPriceList }
    }),
    on(WorkOrderActionTypes.ResetUpdatePriceItemWorkOrder, (state: IWorkOrderDetailState) => {
        const newArrayPriceList = state.listPrice;
        newArrayPriceList.forEach((x: WorkOrderPriceModel) => {
            x.mode = null
        });
        return { ...state, listPrice: [...newArrayPriceList] }
    }),
    on(WorkOrderActionTypes.UpdatePriceItemWorkOrderSuccess, (state: IWorkOrderDetailState, payload: { data: WorkOrderPriceModel }) => {
        const newArrayPriceList = [...state.listPrice];
        const item: WorkOrderPriceModel = payload.data;
        newArrayPriceList.forEach((x: WorkOrderPriceModel) => {
            if (x.mode === 'UPDATE') {
                x.mode = null;
                x.id = item.id;
                x.partnerId = item.partnerId;
                x.partnerName = item.partnerName;
                x.surcharges = item.surcharges;
                x.unitCode = item.unitCode;
                x.unitId = item.unitId;
                x.quantityFromRange = item.quantityFromRange;
                x.quantityToRange = item.quantityToRange;
                x.quantityFromValue = item.quantityFromValue;
                x.quantityToValue = item.quantityToValue;
                x.chargeIdBuying = item.chargeIdBuying;
                x.chargeIdSelling = item.chargeIdSelling;
                x.unitPriceBuying = item.unitPriceBuying;
                x.currencyIdBuying = item.currencyIdBuying;
                x.currencyIdSelling = item.currencyIdSelling;
                x.vatrateBuying = item.vatrateBuying;
                x.vatrateSelling = item.vatrateSelling;
                x.notes = item.notes,
                    x.type = item.type,
                    x.unitPriceSelling = item.unitPriceSelling;

            }
        })
        return { ...state, listPrice: [...newArrayPriceList] }
    }),
    // on(WorkOrderActionTypes.DeletePriceItemWorkOrder, (state: IWorkOrderDetailState, payload: { index: number, id: string }) => {
    //     return {
    //         ...state,
    //         listPrice: [
    //             ...state.listPrice.slice(0, payload.index),
    //             ...state.listPrice.slice(payload.index + 1)
    //         ]
    //     }
    // }),

    on(WorkOrderActionTypes.DeletePriceItemWorkOrderSuccess, (state: IWorkOrderDetailState, payload: { index: number }) => {
        return {
            ...state,
            listPrice: [
                ...state.listPrice.slice(0, payload.index),
                ...state.listPrice.slice(payload.index + 1)
            ]
        }
    }),
);

export function workOrderDetailReducer(state: IWorkOrderDetailState | undefined, action: Action) {
    return reducer(state, action);
}

