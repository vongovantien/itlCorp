import { Component, OnInit, Input, Output, EventEmitter, OnChanges } from '@angular/core';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
import { DataService, BaseService } from 'src/app/shared/services';
import cloneDeep from 'lodash/cloneDeep';
import { API_MENU } from 'src/constants/api-menu.const';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { prepareNg2SelectData } from 'src/helper/data.helper';
import { ChargeConstants } from 'src/constants/charge.const';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { PopupBase } from 'src/app/popup.base';
import moment from 'moment/moment';
import { NgForm } from '@angular/forms';
import * as dataHelper from 'src/helper/data.helper';

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
  currentActiveItemDefault: { id: null, text: null }[] = [];
  buyingRateChargeActive = [];
  exchangeRateDate: any;

  constructor(private _data: DataService,
    private baseServices: BaseService,
    private api_menu: API_MENU) {
    super();
  }
  ngOnChanges() {
    if (this.buyingRateChargeToEdit) {
      this.buyingRateChargeActive = [{ 'text': this.buyingRateChargeToEdit.currency, 'id': this.buyingRateChargeToEdit.currencyId }];
      if (this.buyingRateChargeToEdit.exchangeDate != null) {
        this.exchangeRateDate = { startDate: moment(this.buyingRateChargeToEdit.exchangeDate), endDate: moment(this.buyingRateChargeToEdit.exchangeDate) };
      }
    }
  }
  ngOnInit() {
    this._data.currentMessage.subscribe(message => {
      if (message['buyingCharges'] != null) {
        this.lstBuyingRateChargesComboBox = cloneDeep(message['buyingCharges']);
        console.log('charge buying nè');
        console.log(this.lstBuyingRateChargesComboBox);
      } else {
        this.getListBuyingRateCharges();
      }
      if (message['lstUnits'] != null) {
        this.lstUnits = cloneDeep(message['lstUnits']);
        console.log('list unit nè');
        console.log(this.lstUnits);
      } else {
        this.getUnits();
      }
      if (message['lstPartners'] != null) {
        this.lstPartners = cloneDeep(message['lstPartners']);
        console.log('list partner nè');
        console.log(this.lstPartners);
      } else {
        this.getPartners();
      }
      if (message['lstCurrencies'] != null) {
        this.lstCurrencies = prepareNg2SelectData(cloneDeep(message['lstCurrencies']), "id", "id");
        console.log('list currency nè');
        console.log(this.lstCurrencies);
      } else {
        this.getCurrencies();
      }
    });
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
    // $('#' + formId).modal("hide");
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
          const res = await this.baseServices.putAsync(this.api_menu.Documentation.CsShipmentSurcharge.update, this.buyingRateChargeToEdit);
          if (res.status) {
            // $('#' + id_form).modal('hide');
            this.outputEditBuying.emit(true);
            this.hide();
            // this.getAllSurCharges();
            // this.baseServices.setData("CurrentOpsTransaction", this.opsTransaction);
            // this.baseServices.setData("ShipmentUpdated", true);
          }
        }
      }
    }, 300);
  }
  public getListBuyingRateCharges() {
    this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=0", { inactive: false, type: 'CREDIT', serviceTypeId: ChargeConstants.CL_CODE }).subscribe(res => {
      this.lstBuyingRateChargesComboBox = res['data'];
      this._data.setData('buyingCharges', this.lstBuyingRateChargesComboBox);
    });
  }
  public getUnits() {
    this.baseServices.post(this.api_menu.Catalogue.Unit.getAllByQuery, {}).subscribe((data: any) => {
      this.lstUnits = data;
    });
  }
  public getPartners() {
    this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.ALL, inactive: false }).subscribe((res: any) => {
      this.lstPartners = res;
      console.log({ PARTNERS: this.lstPartners });
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
