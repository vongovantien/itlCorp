
import { Container } from "./container.model";
import { PermissionShipment } from "./permissionShipment";


export class OpsTransaction {
        userModified: string = null;
        userCreated: string = null;
        sumPackages: number = null;
        sumContainers: number = null;
        sumCbm: number = null;
        sumChargeWeight: number = null;
        sumGrossWeight: number = null;
        sumNetWeight: number = null;
        fieldOpsId: string = null;
        invoiceNo: string = null;
        warehouseId: string = null;
        finishDate: any = null;
        billingOpsId: string = null;
        datetimeCreated: Date = null;
        purchaseOrderNo: string = null;
        flightVessel: string = null;
        supplierId: string = null;
        pod: string = null;
        pol: string = null;
        clearanceLocation: string = null;
        customerId: string = null;
        customerName: string = null;
        shipmentMode: string = null;
        serviceMode: string = null;
        productService: string = null;
        serviceDate: any = null;
        hwbno: string = null;
        mblno: string = null;
        jobNo: string = null;
        id: string = "00000000-0000-0000-0000-000000000000";
        hblid: string = "00000000-0000-0000-0000-000000000000";
        agentId: string = null;
        datetimeModified: Date = null;
        salemanId: string = null;
        supplierName: string = null;
        agentName: string = null;
        podName: string = null;
        polName: string = null;
        packageTypeId: number = null;
        csMawbcontainers: Container[] = null;
        containerDescription: string = '';
        commodityGroupId: string = '';
        isLocked: boolean = false;

        permission: PermissionShipment = new PermissionShipment();


        constructor(object?: any) {
                const self = this;
                for (const key in object) {
                        if (self.hasOwnProperty(key.toString())) {
                                self[key] = object[key];
                        }
                }
        }
}