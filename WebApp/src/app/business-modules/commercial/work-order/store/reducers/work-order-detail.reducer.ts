import { Action, createReducer, on } from "@ngrx/store";
import { IPriceWorkOrder } from "../../components/popup/price-item/price-item-work-order.component";
import * as WorkOrderActionTypes from '../actions';


export interface IWorkOrderDetailState {
    listPrice: any[]
}

const initialState: IWorkOrderDetailState = {
    listPrice: []
};

export const reducer = createReducer(
    initialState,
    on(WorkOrderActionTypes.InitPriceListWorkOrder, (state: IWorkOrderDetailState, payload: { type: string, data: any[] }) => ({
        ...state, listPrice: payload.data
    })),
    on(WorkOrderActionTypes.AddPriceItemWorkOrderSuccess, (state: IWorkOrderDetailState, payload: { data: IPriceWorkOrder }) => ({
        ...state, listPrice: [...state.listPrice, payload.data]
    })),
    on(WorkOrderActionTypes.UpdatePriceItemWorkOrder, (state: IWorkOrderDetailState, payload: { index: number }) => {
        const newArrayPriceList = [...state.listPrice];
        newArrayPriceList[payload.index].mode = 'UPDATE';

        return { ...state, listPrice: newArrayPriceList }
    }),
    on(WorkOrderActionTypes.UpdatePriceItemWorkOrderSuccess, (state: IWorkOrderDetailState, payload: { data: IPriceWorkOrder }) => {
        const newArrayPriceList = [...state.listPrice];
        const item: IPriceWorkOrder = payload.data;
        newArrayPriceList.forEach((x: IPriceWorkOrder) => {
            if (x.mode === 'UPDATE') {
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
                // x.freightCharges = item.freightCharges;
                x.mode = null
                x.chargeIdBuying = item.chargeIdBuying,
                    x.chargeIdSelling = item.chargeIdSelling
            }
        })
        return { ...state, listPrice: [...newArrayPriceList] }
    }),
    on(WorkOrderActionTypes.DeletePriceItemWorkOrder, (state: IWorkOrderDetailState, payload: { index: number }) => {
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