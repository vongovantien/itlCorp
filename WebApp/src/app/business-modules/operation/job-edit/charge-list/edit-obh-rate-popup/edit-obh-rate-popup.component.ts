import { Component, OnInit, Input, Output, EventEmitter, OnChanges } from '@angular/core';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import { PopupBase } from 'src/app/popup.base';
import { DataService, BaseService } from 'src/app/shared/services';
import { API_MENU } from 'src/constants/api-menu.const';
import { ChargeConstants } from 'src/constants/charge.const';
import cloneDeep from 'lodash/cloneDeep';
import { prepareNg2SelectData } from 'src/helper/data.helper';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { NgForm } from '@angular/forms';
import moment from 'moment/moment';
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

  constructor(private _data: DataService,
    private baseServices: BaseService,
    private api_menu: API_MENU) {
    super();
  }

  ngOnInit() {
    this._data.currentMessage.subscribe(message => {
      if (message['obhCharges'] != null) {
        this.lstOBHChargesComboBox = cloneDeep(message['obhCharges']);
        console.log('OBH charge nè');
        console.log(this.lstOBHChargesComboBox);
      } else {
        this.getListOBHCharges();
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
  ngOnChanges() {
    if (this.obhChargeToEdit) {
      this.obhChargeActive = [{ 'text': this.obhChargeToEdit.currency, 'id': this.obhChargeToEdit.currencyId }];
      if (this.obhChargeToEdit.exchangeDate != null) {
        this.exchangeRateDate = { startDate: moment(this.obhChargeToEdit.exchangeDate), endDate: moment(this.obhChargeToEdit.exchangeDate) };
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
          const res = await this.baseServices.putAsync(this.api_menu.Documentation.CsShipmentSurcharge.update, this.obhChargeToEdit);
          if (res.status) {
            // $('#' + id_form).modal('hide');
            this.outputEditOBH.emit(true);
            this.hide();
            // this.getAllSurCharges();
            // this.baseServices.setData("CurrentOpsTransaction", this.opsTransaction);
            // this.baseServices.setData("ShipmentUpdated", true);
          }
        }
      }
    }, 300);
  }
  closeChargeForm(form: NgForm) {
    form.onReset();
    this.resetDisplay();
    // $('#' + formId).modal("hide");
    this.hide();

    this.currentActiveItemDefault = [];
    // this.BuyingRateChargeToAdd = new CsShipmentSurcharge();
    // this.SellingRateChargeToAdd = new CsShipmentSurcharge();
    // this.OBHChargeToAdd = new CsShipmentSurcharge();

    // this.BuyingRateChargeToEdit = null;
    // this.SellingRateChargeToEdit = null;
    this.obhChargeToEdit = null;

  }
  public getListOBHCharges() {
    this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=20", { inactive: false, type: 'OBH', serviceTypeId: ChargeConstants.CL_CODE }).subscribe(res => {
      this.lstOBHChargesComboBox = res['data'];
    });
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
}
