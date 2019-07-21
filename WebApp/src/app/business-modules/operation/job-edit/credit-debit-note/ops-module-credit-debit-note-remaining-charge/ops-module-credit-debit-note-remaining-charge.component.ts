import { Component, OnInit, Input, Output, EventEmitter, OnDestroy } from '@angular/core';
import { API_MENU } from 'src/constants/api-menu.const';
import filter from 'lodash/filter';
declare var $: any;

@Component({
  selector: 'app-ops-module-credit-debit-note-remaining-charge',
  templateUrl: './ops-module-credit-debit-note-remaining-charge.component.html'
})
export class OpsModuleCreditDebitNoteRemainingChargeComponent implements OnInit {
  STORAGE_DATA: any = null;
  @Input() listChargeOfPartner: any[] = [];
  @Output() newlistCharge = new EventEmitter<any>();
  partnerId: string = null;
  checkAllCharge: boolean = false;
  constructor(
  ) { }

  ngOnInit() {
  }
  removeAllChargeSelected() {
    this.checkAllCharge = false;
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
    this.listChargeOfPartner[0].listCharges = filter(this.listChargeOfPartner[0].listCharges, function (o: any) {
      o.isRemaining = false;
      return o.isSelected;
    });
    this.newlistCharge.emit(this.listChargeOfPartner);
  }
}
