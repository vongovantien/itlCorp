import { Injectable } from '@angular/core';
import { ApiService } from '../services';
import { environment } from 'src/environments/environment';
import { BravoAdvance } from '@models';

@Injectable({ providedIn: 'root' })
export class PartnerAPIRepo {

    constructor(protected _api: ApiService) {
    }

    addSyncAdvanceBravo(listAdvances: BravoAdvance[]) {
        return this._api.post(`${environment.HOST.ESB}/api/Sync?func=EFMSAdvandeSyncAdd`, listAdvances);
    }

    updateSyncAdvanceBravo(listAdvances: BravoAdvance[]) {
        return this._api.post(`${environment.HOST.ESB}/api/Sync?func=EFMSAdvandeSyncUpdate`, listAdvances);

    }

    addSyncVoucherBravo(body: any) {
        return this._api.post(`${environment.HOST.ESB}/api/Sync?func=EFMSVoucherDataSyncAdd`, body);
    }

    updateSyncVoucherBravo(body: any) {
        return this._api.post(`${environment.HOST.ESB}/api/Sync?func=EFMSVoucherDataSyncUpdate`, body);

    }

    addSyncInvoiceBravo(body: any) {
        return this._api.post(`${environment.HOST.ESB}/api/Sync?func=EFMSInvoiceDataSyncAdd`, body);
    }

    updateSyncInvoiceBravo(body: any) {
        return this._api.post(`${environment.HOST.ESB}/api/Sync?func=EFMSInvoiceDataSyncUpdate`, body);
    }

    // Add phiếu thu
    addSyncReceiptBravo(body: any) {
        return this._api.post(`${environment.HOST.ESB}/api/Sync?func=EFMSReceiptDataSyncAdd`, body);
    }

    // Update phiếu thu
    updateSyncReceiptBravo(body: any) {
        return this._api.post(`${environment.HOST.ESB}/api/Sync?func=EFMSReceiptDataSyncUpdate`, body);
    }

}
