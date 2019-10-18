export class Tariff {
    id: string = '';
    tariffName: string = '';
    tariffType: any = null;
    effectiveDate: any = null;
    expiredDate: any = null;
    officeId: any = null;
    productService: any = null;
    cargoType: any = null;
    serviceMode: any = null;
    customerName: string = '';
    customerId: any = null;
    supplierName: string = '';
    supplierId: any = null;
    agentId: any = null;
    description: string = '';
    status: boolean = null;
    userCreated: string = '';
    datetimeCreated: string = '';
    userModified: string = '';
    datetimeModified: string = '';

    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
        }
    }
}

export class TariffAdd {
    setTariff: Tariff = new Tariff();
    setTariffDetails: TariffCharge[] = new Array<TariffCharge>();

    constructor(object?: any) {
        const self = this;
        // tslint:disable-next-line: forin
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
            if (!!self.setTariff) {
                self.setTariff = new Tariff();
            }
            if (!!self.setTariffDetails) {
                self.setTariffDetails = new Array<TariffCharge>();
            }

        }
    }

}

export class TariffCharge {
    chargeName: string = '';
    commodityName: any = null;
    payerName: string = '';
    portName: string = '';
    warehouseName: string = '';
    id: string = '';
    tariffId: string = '';
    chargeId: string = '';
    useFor: any = null;
    route: string = null;
    commodityId: any = null;
    payerId: any = null;
    portId: any = null;
    warehouseId: any = null;
    type: string = null;
    rangeType: string = null;
    rangeFrom: string = '';
    rangeTo: string = '';
    unitPrice: number = 0;
    min: number = 0;
    max: number = 0;
    nextUnit: number = 0;
    nextUnitPrice: number = 0;
    unitId: number = null;
    currencyId: any = null;
    vatrate: number = 0;
    userCreated: string = '';
    datetimeCreated: string = '';
    userModified: string = '';
    datetimeModified: string = '';

    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key)) {
                self[key] = object[key];
            }
        }
    }
}

