import { Injectable } from '@angular/core';
import { ApiService } from '../services';
import { BravoAdvance } from '@models';
import { environment } from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class PartnerAPIRepo {

    constructor(protected _api: ApiService) {
    }

    loginBravo() {
        return this._api.post(`${environment.HOST.ESB}/Accounting/api/Login`, { username: "bravo", password: "br@vopro" });
    }

    addSyncAdvanceBravo(listAdvances: BravoAdvance[]) {
        return this._api.post(`${environment.HOST.ESB}/Accounting/api?func=EFMSAdvandeSyncAdd`, listAdvances, null, { "partnerAPI": "bravo" });
    }

    updateSyncAdvanceBravo(listAdvances: BravoAdvance[]) {
        return this._api.post(`${environment.HOST.ESB}/Accounting/api?func=EFMSAdvandeSyncUpdate`, listAdvances);
    }

    addSyncVoucherBravo(body: any) {
        return this._api.post(`${environment.HOST.ESB}/Accounting/api?func=EFMSVoucherDataSyncAdd`, body);
    }

    updateSyncVoucherBravo(body: any) {
        return this._api.post(`${environment.HOST.ESB}/Accounting/api?func=EFMSVoucherDataSyncUpdate`, body);

    }

    addSyncInvoiceBravo(body: any) {
        return this._api.post(`${environment.HOST.ESB}/Accounting/api?func=EFMSInvoiceDataSyncAdd`, body);
    }

    updateSyncInvoiceBravo(body: any) {
        return this._api.post(`${environment.HOST.ESB}/Accounting/api?func=EFMSInvoiceDataSyncUpdate`, body);
    }

    // Add phiếu thu
    addSyncReceiptBravo(body: any) {
        return this._api.post(`${environment.HOST.ESB}/Accounting/api?func=EFMSReceiptDataSyncAdd`, body);
    }

    // Update phiếu thu
    updateSyncReceiptBravo(body: any) {
        return this._api.post(`${environment.HOST.ESB}/Accounting/api?func=EFMSReceiptDataSyncUpdate`, body);
    }

}
