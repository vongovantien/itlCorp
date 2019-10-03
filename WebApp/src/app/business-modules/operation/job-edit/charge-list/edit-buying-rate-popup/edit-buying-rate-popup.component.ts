import { Component, OnInit, Input, Output, EventEmitter, OnChanges } from '@angular/core';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
import { DataService, BaseService } from 'src/app/shared/services';
import { API_MENU } from 'src/constants/api-menu.const';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { prepareNg2SelectData } from 'src/helper/data.helper';
import { ChargeConstants } from 'src/constants/charge.const';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { PopupBase } from 'src/app/popup.base';
import { NgForm } from '@angular/forms';
import * as dataHelper from 'src/helper/data.helper';
import { SystemConstants } from 'src/constants/system.const';

@Component({
    selector: 'app-edit-buying-rate-popup',
    templateUrl: './edit-buying-rate-popup.component.html'
})
export class EditBuyingRatePopupComponent extends PopupBase implements OnInit, OnChanges {
    @Input() opsTransaction: OpsTransaction = null;
    @Input() buyingRateChargeToEdit: CsShipmentSurcharge = null;
    @Output() outputEditBuying = new EventEmitter<any>();

    isDisplay: boolean = true;
    lstBuyingRateChargesComboBox: any[] = [];
    lstPartners: any[] = [];
    lstUnits: any[] = [];
    lstCurrencies: any[] = [];
    currentActiveItemDefault: any[] = [];
    buyingRateChargeActive = [];
    exchangeRateDate: any;
    invoiceDate: any;

    constructor(private _data: DataService,
        private baseServices: BaseService,
        private api_menu: API_MENU) {
        super();
    }
    ngOnChanges() {
        if (this.buyingRateChargeToEdit) {
            this.buyingRateChargeActive = [{ 'text': this.buyingRateChargeToEdit.currency, 'id': this.buyingRateChargeToEdit.currencyId }];
            if (this.buyingRateChargeToEdit.exchangeDate != null) {
                this.exchangeRateDate = { startDate: new Date(this.buyingRateChargeToEdit.exchangeDate), endDate: new Date(this.buyingRateChargeToEdit.exchangeDate) };
            }
            if (!!this.buyingRateChargeToEdit.invoiceDate) {
                this.invoiceDate = { startDate: new Date(this.buyingRateChargeToEdit.invoiceDate), endDate: new Date(this.buyingRateChargeToEdit.invoiceDate) };
            }
        }
    }

    ngOnInit() {
        if (!!this._data.getDataByKey(SystemConstants.CSTORAGE.CURRENCY)) {
            this.lstCurrencies = prepareNg2SelectData(this._data.getDataByKey(SystemConstants.CSTORAGE.CURRENCY), "id", "id");
        } else {
            this.getCurrencies();
        }

        if (!!this._data.getDataByKey(SystemConstants.CSTORAGE.UNIT)) {
            this.lstUnits = this._data.getDataByKey(SystemConstants.CSTORAGE.UNIT);
        } else {
            this.getUnits();
        }

        if (!!this._data.getDataByKey(SystemConstants.CSTORAGE.PARTNER)) {
            this.lstPartners = this._data.getDataByKey(SystemConstants.CSTORAGE.PARTNER);
        } else {
            this.getPartners();
        }

        if (!!this._data.getDataByKey("buyingCharges")) {
            this.lstBuyingRateChargesComboBox = this._data.getDataByKey('buyingCharges');
        } else {
            this.getListBuyingRateCharges();
        }
    }

    resetDisplay() {
        this.isDisplay = false;
        setTimeout(() => {
            this.isDisplay = true;
        }, 50);
    }

    closeChargeForm(formId: string, form: NgForm) {
        form.onReset();
        this.resetDisplay();
        this.hide();

        this.currentActiveItemDefault = [];
        this.buyingRateChargeToEdit = null;
    }

    calculateTotalEachBuying() {
        let total = 0;
        if (this.buyingRateChargeToEdit.vatrate >= 0) {
            total = this.buyingRateChargeToEdit.quantity * this.buyingRateChargeToEdit.unitPrice * (1 + (this.buyingRateChargeToEdit.vatrate / 100));
        } else {
            total = this.buyingRateChargeToEdit.quantity * this.buyingRateChargeToEdit.unitPrice + Math.abs(this.buyingRateChargeToEdit.vatrate);
        }
        this.buyingRateChargeToEdit.total = Number(total.toFixed(2));
    }

    editCharge(id_form: string, form: NgForm) {
        setTimeout(async () => {
            if (form.submitted) {
                const error = $('#' + id_form).find('div.has-danger');
                if (error.length === 0) {
                    if (this.buyingRateChargeToEdit.quantity != null) {
                        this.buyingRateChargeToEdit.quantity = Number(this.buyingRateChargeToEdit.quantity.toFixed(2));
                    }
                    if (this.exchangeRateDate != null) {
                        this.buyingRateChargeToEdit.exchangeDate = this.exchangeRateDate.startDate != null ? dataHelper.dateTimeToUTC(this.exchangeRateDate.startDate) : null;
                    }
                    if (this.invoiceDate != null) {
                        this.buyingRateChargeToEdit.invoiceDate = !!this.invoiceDate.startDate ? dataHelper.dateTimeToUTC(this.invoiceDate.startDate) : null;
                    }
                    const res = await this.baseServices.putAsync(this.api_menu.Documentation.CsShipmentSurcharge.update, this.buyingRateChargeToEdit);
                    if (res.status) {
                        this.outputEditBuying.emit(true);
                        this.hide();
                    }
                }
            }
        }, 300);
    }

    getListBuyingRateCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=0", { inactive: false, type: 'CREDIT', serviceTypeId: ChargeConstants.CL_CODE }).subscribe(res => {
            this.lstBuyingRateChargesComboBox = res['data'];
        });
    }

    getUnits() {
        this.baseServices.post(this.api_menu.Catalogue.Unit.getAllByQuery, {}).subscribe((data: any) => {
            this.lstUnits = data;
        });
    }

    getPartners() {
        this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.ALL, inactive: false }).subscribe((res: any) => {
            this.lstPartners = res;
        });
    }

    getCurrencies() {
        this.baseServices.post(this.api_menu.Catalogue.Currency.getAllByQuery, { inactive: false }).subscribe((res: any) => {
            this.lstCurrencies = prepareNg2SelectData(res, "id", "id");
        });
    }

}
