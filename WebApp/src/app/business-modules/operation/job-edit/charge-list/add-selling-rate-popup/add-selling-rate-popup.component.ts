import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { BaseService, DataService } from 'src/app/shared/services';
import { API_MENU } from 'src/constants/api-menu.const';
import { ChargeConstants } from 'src/constants/charge.const';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { prepareNg2SelectData } from 'src/helper/data.helper';
import { NgForm } from '@angular/forms';
import { SystemConstants } from 'src/constants/system.const';


@Component({
    selector: 'app-add-selling-rate-popup',
    templateUrl: './add-selling-rate-popup.component.html'
})
export class AddSellingRatePopupComponent extends PopupBase {
    @Input() opsTransaction: OpsTransaction = null;
    @Output() outputAddSelling = new EventEmitter<any>();

    lstSellingRateChargesComboBox: any[] = [];
    sellingRateChargeToAdd: CsShipmentSurcharge = new CsShipmentSurcharge();

    currentActiveItemDefault: any[] = [];
    @Input() lstPartners: any[] = [];
    lstUnits: any[] = [];
    lstCurrencies: any[] = [];
    currentSelectedCharge: string = null;
    objectBePaidActive: any[] = [];

    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU,
        private _data: DataService
    ) {
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

    saveNewCharge(id_form: string, form: NgForm, isContinue: boolean) {
        setTimeout(async () => {
            const error = $('#' + id_form).find('div.has-danger');
            if (error.length === 0) {
                this.sellingRateChargeToAdd.hblid = this.opsTransaction.hblid;
                if (this.sellingRateChargeToAdd.quantity != null) {
                    this.sellingRateChargeToAdd.quantity = Number(this.sellingRateChargeToAdd.quantity.toFixed(2));
                }
                const res = await this.baseServices.postAsync(this.api_menu.Documentation.CsShipmentSurcharge.addNew, this.sellingRateChargeToAdd);
                if (res.status) {
                    this.close(form);
                    this.outputAddSelling.emit(true);

                    if (!isContinue) {
                        this.hide();
                    }
                }
            }
        }, 300);
    }

    close(form: NgForm) {
        form.onReset();
        this.currentActiveItemDefault = [];
        this.sellingRateChargeToAdd = new CsShipmentSurcharge();
        this.sellingRateChargeToAdd.objectBePaid = null;
        this.sellingRateChargeToAdd.paymentObjectId = null;
        this.objectBePaidActive = [];
        this.currentSelectedCharge = null;
    }

    getListSellingRateCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=0", { inactive: false, type: 'DEBIT', serviceTypeId: ChargeConstants.CL_CODE }).subscribe(res => {
            this.lstSellingRateChargesComboBox = res['data'];
        });

    }

    calculateTotalEachSelling() {
        let total = 0;
        if (this.sellingRateChargeToAdd.vatrate >= 0) {
            total = this.sellingRateChargeToAdd.quantity * this.sellingRateChargeToAdd.unitPrice * (1 + (this.sellingRateChargeToAdd.vatrate / 100));
        } else {
            total = this.sellingRateChargeToAdd.quantity * this.sellingRateChargeToAdd.unitPrice + Math.abs(this.sellingRateChargeToAdd.vatrate);
        }
        this.sellingRateChargeToAdd.total = Number(total.toFixed(2));
    }

    // getPartners() {
    //     this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.ALL, inactive: false }).subscribe((res: any) => {
    //         this.lstPartners = res;
    //     });
    // }

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
