import { Container } from "src/app/shared/models/document/container.model";
import { CsTransactionDetail } from "src/app/shared/models/document/csTransactionDetail";
import { TransactionTypeEnum } from "../../enums/transaction-type.enum";
import { DIM } from "./dimension";

export class CsTransaction {
    id: string = "00000000-0000-0000-0000-000000000000";
    branchId: string = "00000000-0000-0000-0000-000000000000";
    transactionTypeEnum: TransactionTypeEnum;
    jobNo: any = null;
    mawb: string = null;
    typeOfService: string = null;
    etd: any = null;
    eta: any = null;
    serviceDate: any = null;
    mbltype: string = null;
    coloaderId: string = null; // supplier
    coloaderName: string = null; //
    supplierName: string = null; //Used
    bookingNo: string = null;
    agentId: string = null; // agent 
    agentName: string = null;
    pol: string = null;
    polName: string = null;
    pod: string = null;
    podName: string = null;
    deliveryPlace: string = null;
    paymentTerm: string = null;
    flightVesselName: string = null;
    voyNo: string = null;
    shipmentType: string = null;
    commodity: string = null;
    desOfGoods: string = null;
    packageContainer: string = '';
    pono: string = null;
    personIncharge: string = null;
    personInChargeName: string = null;
    netWeight: number = 0;
    grossWeight: number = 0;
    chargeWeight: number = 0;
    cbm: number = 0;
    notes: string = null;
    transactionType: string = null;
    isLocked: boolean = false;
    lockedDate: string = null;
    userCreated: string = null;
    datetimeCreated: string = null;
    userModified: string = null;
    datetimeModified: string = null;
    active: boolean = true;
    inactiveOn: string = null;

    csMawbcontainers: Container[] = new Array<Container>();
    csTransactionDetails: CsTransactionDetail[] = new Array<CsTransactionDetail>();

    subColoader: string = null;
    hwbNo: string = null;
    customerId: string = null;
    customerName: string = null;
    notifyPartyId: string = null;
    saleManId: string = null;
    creatorName: string = null;
    sumCont: number = 0;
    sumPackage: number = 0;
    hblId: string = "00000000-0000-0000-0000-000000000000";
    packageQty: number = null;
    packageType: string = null;
    customerName: string = null;
    supp: string = null;

    // * AIR
    flightDate: any = null;
    hw: number = null;
    hwConstant: number = 6000;
    netweight: number = null;

    dimensionDetails: DIM[] = [];

    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key.toString())) {
                self[key] = object[key];
            }
        }
    }
}
