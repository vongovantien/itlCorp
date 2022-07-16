namespace DocumentationInterface {
    export interface IDataSyncHBL {
        flightVesselName?: string;
        etd: string;
        eta: string;
        flightDate?: string;
        pol: string;
        pod: string;
        agentId?: string;
        issuedBy?: string;
        warehouseId?: string;
        [name: string]: any;
    }

    export interface ICheckPointCriteria {
        partnerIds: string[];
        settlementCode: string;
        hblId: string;
        type: number;
        /*
         1 - SHIPMENT
         2 - SOA
         3 - DEBIT
         4 - CREDIT
         5 - SURCHARGE
         6 - HBL
         7 - Preview HBL
        */
        transactionType: string;
    }
}
