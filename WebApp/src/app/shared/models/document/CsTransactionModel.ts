import { TransactionTypeEnum } from "../../enums/transaction-type.enum";

export class CsTransactionModel {
    id: string = "00000000-0000-0000-0000-000000000000";
    branchId: String = "00000000-0000-0000-0000-000000000000";
    transactionTypeEnum: TransactionTypeEnum;
    jobNo: String = null;
    mawb: String = null;
    typeOfService: String = null;
    etd: Date = null;
    eta: Date = null;
    serviceDate: Date = null;
    mbltype: String = null;
    coloaderId: String = null; // supplier
    coloaderName: String = null;
    supplierName: String = null;
    subColoaderId: String = null;
    bookingNo: String = null;
    agentId: String = null; // agent 
    agentName: String = null;
    pol: String = null;
    polName: String = null;
    pod: String = null;
    podName: String = null;
    deliveryPlace: String = null;
    paymentTerm: String = null;
    flightVesselName: String = null;
    voyNo: String = null;
    shipmentType: String = null;
    commodity: String = null;
    desOfGoods: String = null;
    packageContainer: String = '';
    pono: String = null;
    personIncharge: String = null;
    personInChargeName: String = null;
    netWeight: number = 0;
    grossWeight: number = 0;
    chargeWeight: number = 0; 
    cbm: number = 0;
    notes: String = null;
    transactionType: String = null;
    isLocked: Boolean = null;
    lockedDate: Date = null;
    userCreated: String = null;
    createdDate: Date = null;
    userModified: String = null;
    modifiedDate: Date = null;
    active: Boolean = true;
    inactiveOn: Date = null;  
    
    hwbNo: String = null;
    customerId: String = null;
    notifyPartyId: string = null;
    saleManId: String = null;
    creatorName: String = null;
    sumCont: number = 0;
    sumPackage: number = 0;
    hblId: String = "00000000-0000-0000-0000-000000000000";
    constructor(object?: any) {
        const self = this;
        for (const key in object) {
            if (self.hasOwnProperty(key.toString())) {
                self[key] = object[key];
            }
        }
    }
  }