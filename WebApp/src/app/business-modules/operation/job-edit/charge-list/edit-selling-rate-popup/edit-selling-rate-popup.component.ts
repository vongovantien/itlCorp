import { Component, OnInit, Input, Output, EventEmitter, OnChanges } from '@angular/core';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
import { PopupBase } from 'src/app/popup.base';
import { NgForm } from '@angular/forms';
import { DataService, BaseService } from 'src/app/shared/services';
import { API_MENU } from 'src/constants/api-menu.const';
import { ChargeConstants } from 'src/constants/charge.const';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { prepareNg2SelectData } from 'src/helper/data.helper';
import * as dataHelper from 'src/helper/data.helper';
import { SystemConstants } from 'src/constants/system.const';

@Component({
    selector: 'app-edit-selling-rate-popup',
    templateUrl: './edit-selling-rate-popup.component.html'
})
export class EditSellingRatePopupComponent extends PopupBase implements OnInit, OnChanges {
    @Input() opsTransaction: OpsTransaction = null;
    @Input() sellingRateChargeToEdit: CsShipmentSurcharge = null;
    @Output() outputEditSelling = new EventEmitter<any>();

    isDisplay: boolean = true;
    lstSellingRateChargesComboBox: any[] = [];
    lstUnits: any[] = [];
    @Input() lstPartners: any[] = [];
    lstCurrencies: any[] = [];
    sellingRateChargeActive: any[] = [];
    exchangeRateDate: any;

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

        if (!!this._data.getDataByKey("sellingCharges")) {
            this.lstSellingRateChargesComboBox = this._data.getDataByKey('sellingCharges');
        } else {
            this.getListSellingRateCharges();
        }
    }

    ngOnChanges() {
        if (this.sellingRateChargeToEdit) {
            this.sellingRateChargeActive = [{ 'text': this.sellingRateChargeToEdit.currency, 'id': this.sellingRateChargeToEdit.currencyId }];
            if (this.sellingRateChargeToEdit.exchangeDate != null) {
                this.exchangeRateDate = { startDate: new Date(this.sellingRateChargeToEdit.exchangeDate), endDate: new Date(this.sellingRateChargeToEdit.exchangeDate) };
            }
        }
    }

    editCharge(id_form: string, form: NgForm) {
        setTimeout(async () => {
            if (form.submitted) {
                const error = $('#' + id_form).find('div.has-danger');
                if (error.length === 0) {
                    if (this.sellingRateChargeToEdit.quantity != null) {
                        this.sellingRateChargeToEdit.quantity = Number(this.sellingRateChargeToEdit.quantity.toFixed(2));
                    }
                    if (this.exchangeRateDate != null) {
                        this.sellingRateChargeToEdit.exchangeDate = this.exchangeRateDate.startDate != null ? dataHelper.dateTimeToUTC(this.exchangeRateDate.startDate) : null;
                    }
                    const res = await this.baseServices.putAsync(this.api_menu.Documentation.CsShipmentSurcharge.update, this.sellingRateChargeToEdit);
                    if (res.status) {
                        this.hide();
                        this.outputEditSelling.emit(true);
                    }
                }
            }
        }, 300);
    }

    calculateTotalEachSelling() {
        let total = 0;
        if (this.sellingRateChargeToEdit.vatrate >= 0) {
            total = this.sellingRateChargeToEdit.quantity * this.sellingRateChargeToEdit.unitPrice * (1 + (this.sellingRateChargeToEdit.vatrate / 100));
        } else {
            total = this.sellingRateChargeToEdit.quantity * this.sellingRateChargeToEdit.unitPrice + Math.abs(this.sellingRateChargeToEdit.vatrate);
        }
        this.sellingRateChargeToEdit.total = Number(total.toFixed(2));
    }

    resetDisplay() {
        this.isDisplay = false;
        setTimeout(() => {
            this.isDisplay = true;
        }, 50);
    }

    closeChargeForm(form: NgForm) {
        form.onReset();
        this.resetDisplay();
        this.hide();
        // this.sellingRateChargeToEdit = null;
    }

    getListSellingRateCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.query, { type: 'DEBIT', serviceTypeId: ChargeConstants.CL_CODE }).subscribe((res: any) => {
            this.lstSellingRateChargesComboBox = res;
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

    getCurrencies() {
        this.baseServices.post(this.api_menu.Catalogue.Currency.getAllByQuery, { active: true }).subscribe((res: any) => {
            this.lstCurrencies = prepareNg2SelectData(res, "id", "id");
        });
    }
}
