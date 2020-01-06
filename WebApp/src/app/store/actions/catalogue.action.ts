import { Action } from '@ngrx/store';


export enum CatalogueActionTypes {

    GET_PARTNER = '[Catalogue] Get Partner',

    GET_PORT = '[Catalogue] Get Port',
    GET_PORT_SUCCESS = '[Catalogue] Get Port Success',
    GET_PORT_FAIL = '[Catalogue] Get Port Fail',

    GET_CARRIER = '[Catalogue] Get Carrier',
    GET_CARRIER_SUCCESS = '[Catalogue] Get Carrier Success',
    GET_CARRIER_FAIL = '[Catalogue] Get Carrier Fail',

    GET_AGENT = '[Catalogue] Get Agent',
    GET_AGENT_SUCCESS = '[Catalogue] Get Agent Success',
    GET_AGENT_FAIL = '[Catalogue] Get Agent Fail',

    GET_UNIT = '[Catalogue] Get Unit',
    GET_UNIT_SUCCESS = '[Catalogue] Get Unit Success',
    GET_UNIT_FAIL = '[Catalogue] Get Unit Fail',

    GET_COMMODITY = '[Catalogue] Get Commodity',
    GET_COMMODITY_SUCCESS = '[Catalogue] Get Commodity Success',
    GET_COMMODITY_FAIL = '[Catalogue] Get Commodity Fail',

    GET_CUSTOMER = '[Catalogue] Get Customer',
    GET_CUSTOMER_SUCCESS = '[Catalogue] Get Customer Success',
    GET_CUSTOMER_FAIL = '[Catalogue] Get Customer Fail',

}

export class GetCataloguePartnerAction implements Action {
    readonly type = CatalogueActionTypes.GET_PARTNER;
    constructor(public payload: number) {
    }
}
//#region Port
export class GetCataloguePortAction implements Action {
    readonly type = CatalogueActionTypes.GET_PORT;
    constructor(public payload: any) { }
}
export class GetCataloguePortSuccessAction implements Action {
    readonly type = CatalogueActionTypes.GET_PORT_SUCCESS;
    constructor(public payload: any) { }
}
export class GetCataloguePortFailAction implements Action {
    readonly type = CatalogueActionTypes.GET_PORT_FAIL;
    constructor(public payload: any) { }
}
//#endregion

//#region port
export class GetCatalogueCarrierAction implements Action {
    readonly type = CatalogueActionTypes.GET_CARRIER;

    constructor(public payload: any) { }
}

export class GetCatalogueCarrierSuccessAction implements Action {
    readonly type = CatalogueActionTypes.GET_CARRIER_SUCCESS;
    constructor(public payload: any) { }
}

export class GetCatalogueCarrierFailAction implements Action {
    readonly type = CatalogueActionTypes.GET_CARRIER_FAIL;
    constructor(public payload: any) { }
}
//#endregion

//#region Agent
export class GetCatalogueAgentAction implements Action {
    readonly type = CatalogueActionTypes.GET_AGENT;
    constructor(public payload: any) { }
}
export class GetCatalogueAgentSuccessAction implements Action {
    readonly type = CatalogueActionTypes.GET_AGENT_SUCCESS;
    constructor(public payload: any) { }
}
export class GetCatalogueAgentFailAction implements Action {
    readonly type = CatalogueActionTypes.GET_AGENT_FAIL;
    constructor(public payload: any) { }
}
//#endregion

//#region Unit
export class GetCatalogueUnitAction implements Action {
    readonly type = CatalogueActionTypes.GET_UNIT;
    constructor(public payload: any) { }
}
export class GetCatalogueUnitSuccessAction implements Action {
    readonly type = CatalogueActionTypes.GET_UNIT_SUCCESS;
    constructor(public payload: any) { }
}
export class GetCatalogueUnitFailAction implements Action {
    readonly type = CatalogueActionTypes.GET_UNIT_FAIL;
    constructor(public payload: any) { }
}
//#endregion

//#region Commodity
export class GetCatalogueCommodityAction implements Action {
    readonly type = CatalogueActionTypes.GET_COMMODITY;
    constructor(public payload: any) { }
}
export class GetCatalogueCommoditySuccessAction implements Action {
    readonly type = CatalogueActionTypes.GET_COMMODITY_SUCCESS;
    constructor(public payload: any) { }
}
export class GetCatalogueCommodityFailAction implements Action {
    readonly type = CatalogueActionTypes.GET_COMMODITY_FAIL;
    constructor(public payload: any) { }
}
//#endregion

//#region Customer
export class GetCatalogueCustomerAction implements Action {
    readonly type = CatalogueActionTypes.GET_CUSTOMER;
    constructor(public payload: any) { }
}
export class GetCatalogueCustomerSuccessAction implements Action {
    readonly type = CatalogueActionTypes.GET_CUSTOMER_SUCCESS;
    constructor(public payload: any) { }
}
export class GetCatalogueCustomerFailAction implements Action {
    readonly type = CatalogueActionTypes.GET_CUSTOMER_FAIL;
    constructor(public payload: any) { }
}
//#endregion

export type CatalogueActions = GetCataloguePartnerAction
    | GetCataloguePortAction
    | GetCataloguePortSuccessAction
    | GetCataloguePortFailAction
    | GetCatalogueCarrierAction
    | GetCatalogueCarrierSuccessAction
    | GetCatalogueCarrierFailAction
    | GetCatalogueAgentAction
    | GetCatalogueAgentSuccessAction
    | GetCatalogueAgentFailAction
    | GetCatalogueUnitAction
    | GetCatalogueUnitSuccessAction
    | GetCatalogueUnitFailAction
    | GetCatalogueCommodityAction
    | GetCatalogueCommoditySuccessAction
    | GetCatalogueCommodityFailAction
    | GetCatalogueCustomerAction
    | GetCatalogueCustomerSuccessAction
    | GetCatalogueCustomerFailAction
    ;
