import { Component, OnInit, Input, Output, EventEmitter, OnChanges } from '@angular/core';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { PopupBase } from 'src/app/popup.base';
import { DataService, BaseService } from 'src/app/shared/services';
import { API_MENU } from 'src/constants/api-menu.const';
import { ChargeConstants } from 'src/constants/charge.const';
import cloneDeep from 'lodash/cloneDeep';
import { prepareNg2SelectData } from 'src/helper/data.helper';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { NgForm } from '@angular/forms';
import * as dataHelper from 'src/helper/data.helper';

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
    lstPartners: any[] = [];
    currentActiveItemDefault: { id: null, text: null }[] = [];
    obhChargeActive: any[] = [];
    exchangeRateDate: any;
    invoiceDate: any;

    constructor(private _data: DataService,
        private baseServices: BaseService,
        private api_menu: API_MENU) {
        super();
    }

    ngOnInit() {
        this._data.currentMessage.subscribe(message => {
            if (message['obhCharges'] != null) {
                this.lstOBHChargesComboBox = cloneDeep(message['obhCharges']);
            } else {
                this.getListOBHCharges();
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

    public getListOBHCharges() {
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

    public getCurrencies() {
        this.baseServices.post(this.api_menu.Catalogue.Currency.getAllByQuery, { inactive: false }).subscribe((res: any) => {
            this.lstCurrencies = prepareNg2SelectData(res, "id", "id");
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

    public typed(value: any): void {
        console.log('New search input: ', value);
    }
}
