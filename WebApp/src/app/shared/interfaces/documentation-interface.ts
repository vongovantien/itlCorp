import { DIM } from "@models";

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

    export interface ISyncMAWBShipment {
        mawb: string;
        etd: string;
        eta: string;
        flightNo: string;
        flightDate: string;
        warehouseId: string;
        pol: string;
        pod: string;
        issuedBy: string;
        cbm: number;
        packageQty: number;

        hw: number;
        chargeWeight: number;
        grossWeight: number;

        dimensionDetails: DIM[];
        [name: string]: any;

    }
}
