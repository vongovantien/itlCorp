import { PermissionShipment } from "../document/permissionShipment";

export class WorkOrder {
    id: string = null;
    transactionType: string = null;
    code: string = null;
    partnerId: string = null;
    salesmanId: string = null;
    agentId: string = null;
    shipperId: string = null;
    consigneeId: string = null;
    agentDescription: string = null;
    shipperDescription: string = null;
    consigneeDescription: string = null;
    pol: string = null;
    pod: string = null;
    polDescription: string = null;
    podDescription: string = null;
    incotermId: string = null;
    shipmentType: string = null;
    paymentMethod: string = null;
    pickupPlace: string = null;
    effectiveDate: Date = null;
    expiredDate: Date = null;
    route: string = null;
    transit: string = null;
    approvedDate: Date = null;
    approvedStatus: string = null;
    crmQuotationNo: string = null;
    sysMappingId: string = null;
    source: string = null;
    active: boolean = null;
    userCreated: string = null;
    userModified: string = null;
    datetimeCreated: Date = null;
    datetimeModified: Date = null;
    groupId: number = null;
    departmentId: number = null;
    officeId: string = null;
    companyId: string = null;
    notes: string = null;
    schedule: string = null;

    constructor(object?: Object) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
        }
    }
}
export class WorkOrderModel extends WorkOrder {
    userNameCreated: string;
    userNameModified: string;
    partnerName: string;
    transactionTypeName: string;
}
export class WorkOrderPrice {
    id: string = null;
    workOrderId: string = null;
    type: string = null;
    partnerId: string = null;
    unitPriceBuying: number = 0;
    unitPriceSelling: number = 0;
    vatrateBuying: number = 0;
    vatrateSelling: number = 0;
    notes: string = null;
    currencyIdBuying: string = 'VND';
    currencyIdSelling: string = 'VND';
    quantityType: string = null;
    // quantityFromRange: number = null;
    // quantityToRange: number = null;
    quantityFromValue: number = null;
    quantityToValue: number = null;
    unitId: string = null;
    chargeIdBuying: string = null;
    chargeIdSelling: string = null;
    surcharges: WorkOrderSurcharge[] = [];

    constructor(object?: Object) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
        }
    }
}

export class WorkOrderPriceModel extends WorkOrderPrice {
    chargeCodeBuying: string;
    chargeCodeSelling: string;
    unitCode: string;
    partnerName: string;
    surcharges: WorkOrderSurchargeModel[];
    transactionType: string

    //* custom
    mode: string;
}

export class WorkOrderSurcharge {
    id: string = null;
    chargeId: string = null;
    partnerId: string = null;
    workOrderId: string = null;
    workOrderPriceId: string = null;
    partnerType: string = null;
    unitPrice: number = 0;
    vatRate: number = null;
    kickBack: boolean = null;
    type: string = null;
    currencyId: string = null;
    datetimeCreated: Date = null;
    datetimeModified: Date = null;
    userCreated: string = null;
    userModified: string = null;

    constructor(object?: Object) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
        }
    }
}

export class WorkOrderSurchargeModel extends WorkOrderSurcharge {
    partnerName: string = null;
    chargeName: string = null;
}

export class WorkOrderViewUpdateModel extends WorkOrderModel {
    polName: string;
    podName: string;
    salesmanName: string;
    agentName: string;
    consigneeName: string;
    shipperName: string;
    permission: PermissionShipment;
    listPrice: WorkOrderPriceModel[]
}

export class WorkOrderViewModel {
    id: string;
    userCreated: string;
    userNameCreated: string;
    userNameModified: string;
    partnerName: string;
    salesmanName: string;
    polCode: string;
    podCode: string;
    approvedStatus: string;
    status: string;
    code: string;
    service: string;
    datetimeCreated: Date;
    datetimeModified: Date;
    source: string;
    active: boolean;
}