import { Component, OnInit, Input, Output, EventEmitter, OnChanges } from '@angular/core';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
import { PopupBase } from 'src/app/popup.base';
import { NgForm } from '@angular/forms';
import { DataService, BaseService } from 'src/app/shared/services';
import { API_MENU } from 'src/constants/api-menu.const';
import cloneDeep from 'lodash/cloneDeep';
import { ChargeConstants } from 'src/constants/charge.const';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { prepareNg2SelectData } from 'src/helper/data.helper';
import * as dataHelper from 'src/helper/data.helper';

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
    lstPartners: any[] = [];
    lstCurrencies: any[] = [];
    sellingRateChargeActive: any[] = [];
    exchangeRateDate: any;

    constructor(private _data: DataService,
        private baseServices: BaseService,
        private api_menu: API_MENU) {
        super();
    }

    ngOnInit() {
        this._data.currentMessage.subscribe(message => {
            if (message['sellingCharges'] != null) {
                this.lstSellingRateChargesComboBox = cloneDeep(message['sellingCharges']);
            } else {
                this.getListSellingRateCharges();
            }
            if (message['lstUnits'] != null) {
                this.lstUnits = cloneDeep(message['lstUnits']);
            } else {
                this.getUnits();
            }
            if (message['lstPartners'] != null) {
                this.lstPartners = cloneDeep(message['lstPartners']);
            } else {
                this.getPartners();
            }
            if (message['lstCurrencies'] != null) {
                this.lstCurrencies = prepareNg2SelectData(cloneDeep(message['lstCurrencies']), "id", "id");
            } else {
                this.getCurrencies();
            }
        });
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
        this.sellingRateChargeToEdit = null;
    }

    public getListSellingRateCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.query, { type: 'DEBIT', serviceTypeId: ChargeConstants.CL_CODE }).subscribe((res: any) => {
            this.lstSellingRateChargesComboBox = res;
        });
    }

    public getUnits() {
        this.baseServices.post(this.api_menu.Catalogue.Unit.getAllByQuery, {}).subscribe((data: any) => {
            this.lstUnits = data;
        });
    }

    public getPartners() {
        this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.ALL }).subscribe((res: any) => {
            this.lstPartners = res;
        });
    }

    public getCurrencies() {
        this.baseServices.post(this.api_menu.Catalogue.Currency.getAllByQuery, { inactive: false }).subscribe((res: any) => {
            this.lstCurrencies = prepareNg2SelectData(res, "id", "id");
        });
    }

    public typed(value: any): void {
        console.log('New search input: ', value);
    }
}
