import { Component, OnInit, Input, Output, EventEmitter, OnDestroy, ViewChild } from '@angular/core';
import { API_MENU } from 'src/constants/api-menu.const';
import filter from 'lodash/filter';
import { PopupBase } from 'src/app/popup.base';
import { NotSelectedAlertModalComponent } from '../ops-module-credit-debit-note-addnew/not-selected-alert-modal/not-selected-alert-modal.component';
declare var $: any;
import cloneDeep from 'lodash/cloneDeep';

@Component({
  selector: 'app-ops-module-credit-debit-note-remaining-charge',
  templateUrl: './ops-module-credit-debit-note-remaining-charge.component.html'
})
export class OpsModuleCreditDebitNoteRemainingChargeComponent extends PopupBase implements OnInit {
  STORAGE_DATA: any = null;
  listChargeOfPartner: any[] = [];
  constListChargeOfPartner: any[] = [];
  @Output() newlistCharge = new EventEmitter<any>();
  @ViewChild(NotSelectedAlertModalComponent, { static: false }) popupNotSelected: NotSelectedAlertModalComponent;


  partnerId: string = null;
  checkAllCharge: boolean = false;
  constructor(
  ) {
    super();
  }

  ngOnInit() {
  }
  removeAllChargeSelected() {
    this.checkAllCharge = false;
  }
  searchCharge(search_key: string) {
    this.listChargeOfPartner = cloneDeep(this.constListChargeOfPartner);
    search_key = search_key.trim().toLowerCase();
    const listBranch: any[] = [];
    this.listChargeOfPartner = filter(cloneDeep(this.constListChargeOfPartner), function (x: any) {
      let root = false;
      let branch = false;
      if (x.hwbno == null ? "" : x.hwbno.toLowerCase().includes(search_key)) {
        root = true;
      }
      const listCharges: any[] = [];
      for (let i = 0; i < x.listCharges.length; i++) {
        if (x.listCharges[i].chargeCode.toLowerCase().includes(search_key) ||
          x.listCharges[i].quantity.toString().toLowerCase() === search_key ||
          x.listCharges[i].nameEn.toString().toLowerCase().includes(search_key) ||
          x.listCharges[i].chargeCode.toLowerCase().includes(search_key) ||
          x.listCharges[i].unit.toLowerCase().includes(search_key) ||
          x.listCharges[i].currency.toLowerCase().includes(search_key)) {
          listCharges.push(x.listCharges[i]);
          branch = true;
        }
      }
      if (listCharges.length > 0) {
        listBranch.push({
          hbId: x.id,
          list: listCharges
        });
      };

      return (root || branch);
    });
  }

  checkAllChange() {
    if (this.listChargeOfPartner[0].listCharges !== null) {
      if (this.checkAllCharge) {

        this.listChargeOfPartner[0].listCharges.forEach(element => {
          element.isSelected = true;
        });
      } else {
        this.listChargeOfPartner[0].listCharges.forEach(element => {
          element.isSelected = false;
        });
      }
    }
  }
  addCharges() {
    if (this.listChargeOfPartner[0].listCharges.filter(x => x.isSelected && x.isRemaining === true).length == 0) {
      this.popupNotSelected.show();
    } else {
      this.listChargeOfPartner[0].listCharges.forEach(o => {
        if (o.isSelected === true) {
          o.isRemaining = false;
        }
      });
      this.newlistCharge.emit(this.listChargeOfPartner);
      this.hide();
    }
  }
  close() {
    this.hide();
  }
}
