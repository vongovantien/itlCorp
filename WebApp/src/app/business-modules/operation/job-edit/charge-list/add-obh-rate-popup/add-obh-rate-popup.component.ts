import { Component, OnInit, Output, Input, EventEmitter } from '@angular/core';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { PopupBase } from 'src/app/popup.base';
import { NgForm } from '@angular/forms';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
import { BaseService, DataService } from 'src/app/shared/services';
import { API_MENU } from 'src/constants/api-menu.const';
import { ChargeConstants } from 'src/constants/charge.const';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { prepareNg2SelectData } from 'src/helper/data.helper';
import { SystemConstants } from 'src/constants/system.const';

@Component({
    selector: 'app-add-obh-rate-popup',
    templateUrl: './add-obh-rate-popup.component.html'
})
export class AddObhRatePopupComponent extends PopupBase implements OnInit {
    @Input() opsTransaction: OpsTransaction = null;
    @Output() outputAddOBH = new EventEmitter<any>();

    currentActiveItemDefault: any[] = [];
    obhChargeToAdd: CsShipmentSurcharge = new CsShipmentSurcharge();
    lstOBHChargesComboBox: any[] = [];
    @Input() lstPartners: any[] = [];
    lstUnits: any[] = [];
    lstCurrencies: any[] = [];
    currentSelectedCharge: string = null;

    invoiceDate: any;

    constructor(private baseServices: BaseService,
        private _data: DataService,
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

    close(form: NgForm) {
        form.onReset();
        this.obhChargeToAdd = new CsShipmentSurcharge();
        this.currentActiveItemDefault = [];
        this.currentSelectedCharge = null;
    }

    calculateTotalEachOBH(isEdit: boolean = false) {
        let total = 0;
        if (this.obhChargeToAdd.vatrate >= 0) {
            total = this.obhChargeToAdd.quantity * this.obhChargeToAdd.unitPrice * (1 + (this.obhChargeToAdd.vatrate / 100));
        } else {
            total = this.obhChargeToAdd.quantity * this.obhChargeToAdd.unitPrice + Math.abs(this.obhChargeToAdd.vatrate);
        }
        this.obhChargeToAdd.total = Number(total.toFixed(2));
    }

    saveNewCharge(id_form: string, form: NgForm, isContinue: boolean) {
        setTimeout(async () => {
            const error = $('#' + id_form).find('div.has-danger');
            if (error.length === 0) {
                this.obhChargeToAdd.hblid = this.opsTransaction.hblid;
                if (this.obhChargeToAdd.quantity != null) {
                    this.obhChargeToAdd.quantity = Number(this.obhChargeToAdd.quantity.toFixed(2));
                }
                if (this.invoiceDate != null) {
                    this.obhChargeToAdd.invoiceDate = !!this.invoiceDate.startDate ? new Date(this.invoiceDate.startDate.toString() + " UTC") : null;
                }
                const res = await this.baseServices.postAsync(this.api_menu.Documentation.CsShipmentSurcharge.addNew, this.obhChargeToAdd);
                if (res.status) {
                    this.close(form);
                    this.outputAddOBH.emit(true);

                    if (!isContinue) {
                        this.hide();
                    }
                }
            }
        }, 300);
    }

    getListOBHCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=20", { inactive: false, type: 'OBH', serviceTypeId: ChargeConstants.CL_CODE }).subscribe(res => {
            this.lstOBHChargesComboBox = res['data'];
        });
    }

    getPartners() {
        this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.ALL, inactive: false }).subscribe((res: any) => {
            this.lstPartners = res;
        });
    }

    getUnits() {
        this.baseServices.post(this.api_menu.Catalogue.Unit.getAllByQuery, { inactive: false }).subscribe((data: any) => {
            this.lstUnits = data;
        });
    }

    getCurrencies() {
        this.baseServices.post(this.api_menu.Catalogue.Currency.getAllByQuery, { inactive: false }).subscribe((res: any) => {
            this.lstCurrencies = prepareNg2SelectData(res, "id", "id");
        });
    }
}
