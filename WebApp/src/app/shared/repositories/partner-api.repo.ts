import { Injectable } from '@angular/core';
import { ApiService } from '../services';
import { BravoAdvance } from '@models';
import { environment } from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class PartnerAPIRepo {

    bravoKey = { "partnerAPI": "bravo" };
    constructor(protected _api: ApiService) {
    }

    loginBravo() {
        return this._api.post(`${environment.HOST.ESB}/Accounting/api/Login`, { username: "bravo", password: "br@vopro" });
    }

    // TẠM Ứng
    addSyncAdvanceBravo(listAdvances: BravoAdvance[]) {
        return this._api.post(`${environment.HOST.ESB}/Accounting/api?func=EFMSAdvandeSyncAdd`, listAdvances, null, this.bravoKey);
    }

    updateSyncAdvanceBravo(listAdvances: BravoAdvance[]) {
        return this._api.post(`${environment.HOST.ESB}/Accounting/api?func=EFMSAdvandeSyncUpdate`, listAdvances, null, this.bravoKey);
    }

    // Hoạch toán 
    addSyncVoucherBravo(body: any) {
        return this._api.post(`${environment.HOST.ESB}/Accounting/api?func=EFMSVoucherDataSyncAdd`, body, null, this.bravoKey);
    }

    updateSyncVoucherBravo(body: any) {
        return this._api.post(`${environment.HOST.ESB}/Accounting/api?func=EFMSVoucherDataSyncUpdate`, body, null, this.bravoKey);

    }

    // Hóa đơn bán hàng - Chi phí (SOA,CD,SETTLEMENT)
    addSyncInvoiceBravo(body: any) {
        return this._api.post(`${environment.HOST.ESB}/Accounting/api?func=EFMSInvoiceDataSyncAdd`, body, null, this.bravoKey);
    }

    updateSyncInvoiceBravo(body: any) {
        return this._api.post(`${environment.HOST.ESB}/Accounting/api?func=EFMSInvoiceDataSyncUpdate`, body, null, this.bravoKey);
    }

    // Phiếu thu
    addSyncReceiptBravo(body: any) {
        return this._api.post(`${environment.HOST.ESB}/Accounting/api?func=EFMSReceiptDataSyncAdd`, body, null, this.bravoKey);
    }

    updateSyncReceiptBravo(body: any) {
        return this._api.post(`${environment.HOST.ESB}/Accounting/api?func=EFMSReceiptDataSyncUpdate`, body, null, this.bravoKey);
    }



}
