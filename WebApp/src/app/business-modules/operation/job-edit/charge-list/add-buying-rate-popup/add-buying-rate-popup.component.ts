import { Component, OnInit, Input, Output, EventEmitter, OnDestroy } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
import { NgForm } from '@angular/forms';
import { ChargeConstants } from 'src/constants/charge.const';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { prepareNg2SelectData } from 'src/helper/data.helper';
import { DataService } from 'src/app/shared/services';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { SystemConstants } from 'src/constants/system.const';

@Component({
    selector: 'app-add-buying-rate-popup',
    templateUrl: './add-buying-rate-popup.component.html'
})
export class AddBuyingRatePopupComponent extends PopupBase implements OnInit, OnDestroy {
    @Input() opsTransaction: OpsTransaction = null;
    @Output() outputAddBuying = new EventEmitter<any>();

    buyingRateChargeToAdd: CsShipmentSurcharge = new CsShipmentSurcharge();
    lstBuyingRateChargesComboBox: any[] = [];
    @Input() lstPartners: any[] = [];
    lstUnits: any[] = [];
    lstCurrencies: any[] = [];
    currentActiveItemDefault: { id: null, text: null }[] = [];
    currentSelectedCharge: string = null;
    objectBePaidActive: any[] = [];

    invoiceDate: any;

    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU,
        private _data: DataService,
        private _catalogueRepo: CatalogueRepo) {
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

        if (!!this._data.getDataByKey("buyingCharges")) {
            this.lstBuyingRateChargesComboBox = this._data.getDataByKey('buyingCharges');
        } else {
            this.getListBuyingRateCharges();
        }
    }

    calculateTotalEachBuying() {
        let total = 0;
        if (this.buyingRateChargeToAdd.vatrate >= 0) {
            total = this.buyingRateChargeToAdd.quantity * this.buyingRateChargeToAdd.unitPrice * (1 + (this.buyingRateChargeToAdd.vatrate / 100));
        } else {
            total = this.buyingRateChargeToAdd.quantity * this.buyingRateChargeToAdd.unitPrice + Math.abs(this.buyingRateChargeToAdd.vatrate);
        }
        this.buyingRateChargeToAdd.total = Number(total.toFixed(2));
    }

    saveNewCharge(id_form: string, form: NgForm, isContinue: boolean) {
        setTimeout(async () => {
            const error = $('#' + id_form).find('div.has-danger');
            if (error.length === 0) {
                this.buyingRateChargeToAdd.hblid = this.opsTransaction.hblid;
                if (this.buyingRateChargeToAdd.quantity != null) {
                    this.buyingRateChargeToAdd.quantity = Number(this.buyingRateChargeToAdd.quantity.toFixed(2));
                }
                if (this.invoiceDate != null) {
                    this.buyingRateChargeToAdd.invoiceDate = !!this.invoiceDate.startDate ? new Date(this.invoiceDate.startDate.toString() + " UTC") : null;
                }
                const res = await this.baseServices.postAsync(this.api_menu.Documentation.CsShipmentSurcharge.addNew, this.buyingRateChargeToAdd);
                if (res.status) {
                    this.close(form);
                    this.outputAddBuying.emit(true);

                    if (!isContinue) {
                        this.hide();
                    }
                }
            }
        }, 300);
    }

    close(form: NgForm) {
        form.onReset();
        this.currentSelectedCharge = null;
        this.buyingRateChargeToAdd = new CsShipmentSurcharge();
        this.currentActiveItemDefault = [];
        this.buyingRateChargeToAdd.objectBePaid = null;
        this.buyingRateChargeToAdd.paymentObjectId = null;
        this.objectBePaidActive = [];
        this.hide();
    }

    getListBuyingRateCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=0", { inactive: false, type: 'CREDIT', serviceTypeId: ChargeConstants.CL_CODE }).subscribe(res => {
            this.lstBuyingRateChargesComboBox = res['data'];
            // this._data.setData('buyingCharges', this.lstBuyingRateChargesComboBox);
        });
    }

    getPartners() {
        this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.ALL, inactive: false })
            .subscribe((res: any) => {
                this.lstPartners = res;
            });
    }

    getUnits() {
        this._catalogueRepo.getUnit({ inactive: false })
            .subscribe((data: any) => {
                this.lstUnits = data;
            });
    }

    getCurrencies() {
        this._catalogueRepo.getCurrency()
            .subscribe((res: any) => {
                this.lstCurrencies = prepareNg2SelectData(res, "id", "id");
            });
    }

}
