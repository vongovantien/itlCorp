import { Component, OnInit, Input, Output, EventEmitter, OnChanges } from '@angular/core';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { PopupBase } from 'src/app/popup.base';
import { DataService, BaseService } from 'src/app/shared/services';
import { API_MENU } from 'src/constants/api-menu.const';
import { ChargeConstants } from 'src/constants/charge.const';
import { prepareNg2SelectData } from 'src/helper/data.helper';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { NgForm } from '@angular/forms';
import * as dataHelper from 'src/helper/data.helper';
import { SystemConstants } from 'src/constants/system.const';

@Component({
    selector: 'app-edit-obh-rate-popup',
    templateUrl: './edit-obh-rate-popup.component.html'
})
export class EditObhRatePopupComponent extends PopupBase implements OnInit, OnChanges {
    @Input() opsTransaction: OpsTransaction = null;
    @Input() obhChargeToEdit: CsShipmentSurcharge = null;
    @Output() outputEditOBH = new EventEmitter<any>();

    isDisplay: boolean = true;
    lstOBHChargesComboBox: any[] = [];
    lstCurrencies: any[] = [];
    lstUnits: any[] = [];
    @Input() lstPartners: any[] = [];
    currentActiveItemDefault: any[] = [];
    obhChargeActive: any[] = [];
    exchangeRateDate: any;
    invoiceDate: any;

    constructor(private _data: DataService,
        private baseServices: BaseService,
        private api_menu: API_MENU) {
        super();
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
        }
        // else {
        //     this.getPartners();
        // }

        if (!!this._data.getDataByKey("obhCharges")) {
            this.lstOBHChargesComboBox = this._data.getDataByKey('obhCharges');
        } else {
            this.getListOBHCharges();
        }
    }

    ngOnChanges() {
        if (this.obhChargeToEdit) {
            console.log(this.obhChargeToEdit);
            this.obhChargeActive = [{ 'text': this.obhChargeToEdit.currency, 'id': this.obhChargeToEdit.currencyId }];
            if (this.obhChargeToEdit.exchangeDate != null) {
                this.exchangeRateDate = { startDate: new Date(this.obhChargeToEdit.exchangeDate), endDate: new Date(this.obhChargeToEdit.exchangeDate) };
            }
            if (!!this.obhChargeToEdit.invoiceDate) {
                this.invoiceDate = { startDate: new Date(this.obhChargeToEdit.invoiceDate), endDate: new Date(this.obhChargeToEdit.invoiceDate) };
            }
        }
    }

    editCharge(id_form: string, form: NgForm) {
        setTimeout(async () => {
            if (form.submitted) {
                const error = $('#' + id_form).find('div.has-danger');
                if (error.length === 0) {
                    if (this.obhChargeToEdit.quantity != null) {
                        this.obhChargeToEdit.quantity = Number(this.obhChargeToEdit.quantity.toFixed(2));
                    }
                    if (this.exchangeRateDate != null) {
                        this.obhChargeToEdit.exchangeDate = this.exchangeRateDate.startDate != null ? dataHelper.dateTimeToUTC(this.exchangeRateDate.startDate) : null;
                    }
                    if (this.invoiceDate != null) {
                        this.obhChargeToEdit.invoiceDate = !!this.invoiceDate.startDate ? dataHelper.dateTimeToUTC(this.invoiceDate.startDate) : null;
                    }
                    const res = await this.baseServices.putAsync(this.api_menu.Documentation.CsShipmentSurcharge.update, this.obhChargeToEdit);
                    if (res.status) {
                        this.outputEditOBH.emit(true);
                        this.hide();
                    }
                }
            }
        }, 300);
    }

    closeChargeForm(form: NgForm) {
        form.onReset();
        this.resetDisplay();
        this.hide();

        this.currentActiveItemDefault = [];
        this.obhChargeToEdit = null;
    }

    getListOBHCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=20", { inactive: false, type: 'OBH', serviceTypeId: ChargeConstants.CL_CODE }).subscribe(res => {
            this.lstOBHChargesComboBox = res['data'];
        });
    }

    calculateTotalEachOBH(isEdit: boolean = false) {
        let total = 0;
        if (this.obhChargeToEdit.vatrate >= 0) {
            total = this.obhChargeToEdit.quantity * this.obhChargeToEdit.unitPrice * (1 + (this.obhChargeToEdit.vatrate / 100));
        } else {
            total = this.obhChargeToEdit.quantity * this.obhChargeToEdit.unitPrice + Math.abs(this.obhChargeToEdit.vatrate);
        }
        this.obhChargeToEdit.total = Number(total.toFixed(2));
    }

    resetDisplay() {
        this.isDisplay = false;
        setTimeout(() => {
            this.isDisplay = true;
        }, 50);
    }

    getCurrencies() {
        this.baseServices.post(this.api_menu.Catalogue.Currency.getAllByQuery, { inactive: false }).subscribe((res: any) => {
            this.lstCurrencies = prepareNg2SelectData(res, "id", "id");
        });
    }

    getUnits() {
        this.baseServices.post(this.api_menu.Catalogue.Unit.getAllByQuery, {}).subscribe((data: any) => {
            this.lstUnits = data;
        });
    }

    // getPartners() {
    //     this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.ALL }).subscribe((res: any) => {
    //         this.lstPartners = res;
    //     });
    // }

}
