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

    constructor(object?: Object) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
        }
    }
}

export class WorkOrderPrice {
    id: string = null;
    workOrderId: string = null;
    type: string = null;
    partnerId: string = null;
    unitPriceBuying: number = 0;
    unitPriceSelling: number = 0;
    vatRateBuying: number = 0;
    vatRateSelling: number = 0;
    notes: string = null;
    currencyIdBuying: string = 'VND';
    currencyIdSelling: string = 'VND';
    quantityFromRange: number = null;
    quantityToRange: number = null;
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

export class WorkOrderSurcharge {
    id: string = null;
    chargeId: string = null;
    partnerId: string = null;
    workOrderId: string = null;
    workOrderPriceId: string = null;
    partnerType: string = null;
    unitPrice: number = 0;
    vatRate: number = null;
    kickBack: boolean;

    constructor(object?: Object) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
        }
    }
}