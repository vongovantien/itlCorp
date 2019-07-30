import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
import { NgForm } from '@angular/forms';
import { ChargeConstants } from 'src/constants/charge.const';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import { prepareNg2SelectData } from 'src/helper/data.helper';

@Component({
  selector: 'app-add-buying-rate-popup',
  templateUrl: './add-buying-rate-popup.component.html'
})
export class AddBuyingRatePopupComponent extends PopupBase implements OnInit {
  @Input() opsTransaction: OpsTransaction = null;
  @Output() outputAddBuying = new EventEmitter<any>();
  buyingRateChargeToAdd: CsShipmentSurcharge = new CsShipmentSurcharge();
  isDisplay: boolean = true;
  lstBuyingRateChargesComboBox: any[] = [];
  lstPartners: any[] = [];
  lstUnits: any[] = [];
  lstCurrencies: any[] = [];
  currentActiveItemDefault: { id: null, text: null }[] = [];

  constructor(private baseServices: BaseService,
    private api_menu: API_MENU) {
    super();
  }

  ngOnInit() {
    this.getListBuyingRateCharges();
    this.getPartners();
    this.getUnits();
    this.getCurrencies();
  }
  close(form: NgForm) {
    form.onReset();
    this.currentActiveItemDefault = [];
    this.buyingRateChargeToAdd = new CsShipmentSurcharge();
    this.resetDisplay();
    this.hide();
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
        const res = await this.baseServices.postAsync(this.api_menu.Documentation.CsShipmentSurcharge.addNew, this.buyingRateChargeToAdd);
        if (res.status) {
          form.onReset();
          this.outputAddBuying.emit(true);
          this.resetDisplay();
          // this.getAllSurCharges();
          this.buyingRateChargeToAdd = new CsShipmentSurcharge();
          this.currentActiveItemDefault = [];
          // this.baseServices.setData("CurrentOpsTransaction", this.opsTransaction);
          // this.baseServices.setData("ShipmentAdded", true);
          if (!isContinue) {
            // $('#' + id_form).modal('hide');
            this.hide();
          }
        }
      }
    }, 300);
  }
  resetDisplay() {
    this.isDisplay = false;
    setTimeout(() => {
      this.isDisplay = true;
    }, 50);
  }
  public getListBuyingRateCharges() {
    this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=0", { inactive: false, type: 'CREDIT', serviceTypeId: ChargeConstants.CL_CODE }).subscribe(res => {
      this.lstBuyingRateChargesComboBox = res['data'];
    });

  }
  public getPartners() {
    this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.ALL, inactive: false }).subscribe((res: any) => {
      this.lstPartners = res;
      console.log({ PARTNERS: this.lstPartners });
    });
  }
  public getUnits() {
    this.baseServices.post(this.api_menu.Catalogue.Unit.getAllByQuery, { inactive: false }).subscribe((data: any) => {
      this.lstUnits = data;
    });
  }
  public getCurrencies() {
    this.baseServices.post(this.api_menu.Catalogue.Currency.getAllByQuery, { inactive: false }).subscribe((res: any) => {
      this.lstCurrencies = prepareNg2SelectData(res, "id", "id");
    });
  }
}
