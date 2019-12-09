import { BaseModel } from "../base.model";

export class HouseBill extends BaseModel {
    id: string = '00000000-0000-0000-0000-000000000000';
    active: boolean = null;
    alsoNotifyPartyDescription: string = null;
    alsoNotifyPartyId: string = null;
    arrivalFirstNotice: string = null;
    arrivalFooter: string = null;
    arrivalHeader: string = null;
    arrivalNo: string = null;
    arrivalSecondNotice: string = null;
    cbm: number = null;
    chargeWeight: number = null;
    closingDate: string = null;
    coloaderId: string = null;
    commodity: string = null;
    consigneeDescription: string = null;
    consigneeId: string = null;
    consigneeName: string = null;
    contSealNo: string = null;
    containerNames: string = null;
    containers: string = null;
    csMawbcontainers: [];
    customerId: string = null;
    customerName: string = null;
    customerNameVn: string = null;
    customsBookingNo: string = null;
    cw: number = null;
    datetimeCreated: string = null;
    datetimeModified: string = null;
    deliveryOrderNo: string = null;
    deliveryOrderPrintedDate: string = null;
    deliveryPlace: string = null;
    desOfGoods: string = null;
    documentDate: string = null;
    documentNo: string = null;
    dofooter: string = null;
    dosentTo1: string = null;
    dosentTo2: string = null;
    eta: string = null;
    etawarehouse: string = null;
    etd: string = null;
    exportReferenceNo: string = null;
    finalDestinationPlace: string = null;
    forwardingAgentDescription: string = null;
    forwardingAgentId: string = null;
    forwardingAgentName: string = null;
    freightPayment: string = null;
    goodsDeliveryDescription: string = null;
    goodsDeliveryId: string = null;
    grossWeight: number = null;
    gw: number = null;
    hbltype: string = null;
    hwbno: string = null;
    inWord: string = null;
    inactiveOn: string = null;
    issueHbldate: string = null;
    issueHblplace: string = null;
    jobId: string = null;
    localVessel: string = null;
    localVoyNo: string = null;
    manifestRefNo: string = null;
    mawb: string = null;
    moveType: string = null;
    netWeight: number = null;
    notifyParty: string = null;
    notifyPartyDescription: string = null;
    notifyPartyId: string = null;
    oceanVessel: string = null;
    oceanVoyNo: string = null;
    onBoardStatus: string = null;
    originBlnumber: number = null;
    originCountryId: string = null;
    packageContainer: string = null;
    packageTypes: any = null;
    packageType: number = null;
    packages: string = null;
    pickupPlace: string = null;
    placeFreightPay: string = null;
    pod: string = null;
    podName: string = null;
    pol: string = null;
    polName: string = null;
    purchaseOrderNo: string = null;
    referenceNo: string = null;
    remark: string = null;
    sailingDate: string = null;
    saleManId: string = null;
    saleManName: string = null;
    saleManNameVn: string = null;
    serviceType: string = null;
    shipmentEta: string = null;
    shipperDescription: string = null;
    shipperId: string = null;
    shipperName: string = null;
    shippingMark: string = null;
    warehouseNotice: string = null;

    constructor(object?: any) {
        super();
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key.toString())) {
                self[key] = object[key];
            }
        }
    }
}