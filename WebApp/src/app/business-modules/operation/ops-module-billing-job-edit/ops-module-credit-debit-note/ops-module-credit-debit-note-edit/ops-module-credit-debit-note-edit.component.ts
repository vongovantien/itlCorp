import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import filter from 'lodash/filter';
import map from 'lodash/map';
import concat from 'lodash/concat'
import cloneDeep from 'lodash/cloneDeep'
import groupBy from 'lodash/groupBy';
import { AcctSOA } from 'src/app/shared/models/document/acctSoa.model';
import { NgForm } from '@angular/forms';
declare var $: any;

@Component({
  selector: 'app-ops-module-credit-debit-note-edit',
  templateUrl: './ops-module-credit-debit-note-edit.component.html',
  styleUrls: ['./ops-module-credit-debit-note-edit.component.scss']
})
export class OpsModuleCreditDebitNoteEditComponent implements OnInit {

  listChargeOfPartner: any[] = []; // charges list group by housebill when choose a partner 
  listRemainingCharges: any[] = [];
  constListChargeOfPartner: any[] = [];
  isDisplay: boolean = true;
  EditingCDNote: AcctSOA = new AcctSOA();
  STORAGE_DATA: any = null;
  currentHbID:string = null;
  constructor(
    private baseServices: BaseService,
    private api_menu: API_MENU
  ) { }

  ngOnInit() {
    this.baseServices.dataStorage.subscribe(data=>{
      this.STORAGE_DATA = data;
      if(this.STORAGE_DATA.CDNoteEditing!==undefined){
        this.EditingCDNote = this.STORAGE_DATA.CDNoteEditing.soa;
        this.EditingCDNote.partnerName = this.STORAGE_DATA.CDNoteEditing.partnerNameEn;
        this.EditingCDNote.listShipmentSurcharge = this.STORAGE_DATA.CDNoteEditing.listSurcharges;
        this.getListCharges(this.EditingCDNote.partnerId);
      }
      if (this.STORAGE_DATA.CurrentOpsTransaction !== undefined) {
        this.currentHbID = this.STORAGE_DATA.CurrentOpsTransaction.hblid;
        this.getListCharges(this.EditingCDNote.partnerId);
    }
    });
  }


  async getListCharges(partnerId: String) {
    this.listChargeOfPartner = await this.baseServices.getAsync(this.api_menu.Documentation.CsShipmentSurcharge.getChargesByPartner + "?Id=" + this.currentHbID + "&partnerID=" + partnerId + "&IsHouseBillId=true");

    this.listChargeOfPartner = map(this.listChargeOfPartner, function (o) {
      for (var i = 0; i < o.listCharges.length; i++) {
        o.listCharges[i].isRemaining = true;
      }
      return o;
    });

    for (var i = 0; i < this.listChargeOfPartner.length; i++) {
      for (var k = 0; k < this.EditingCDNote.listShipmentSurcharge.length; k++) {
        if (this.listChargeOfPartner[i].hwbno === this.EditingCDNote.listShipmentSurcharge[k].hwbno) {

          this.EditingCDNote.listShipmentSurcharge[k].isRemaining = false;
          this.listChargeOfPartner[i].listCharges.push(Object.assign({}, this.EditingCDNote.listShipmentSurcharge[k]));

        }
      }

    }
    
    this.totalCreditDebitCalculate();
    this.constListChargeOfPartner = cloneDeep(this.listChargeOfPartner);
    this.baseServices.setData("listChargeOfPartner", this.listChargeOfPartner);
    // this.baseServices.setData("currentPartnerId", partnerId);
    // this.addNewRemainingCharges.emit(this.constListChargeOfPartner);
    console.log(this.listChargeOfPartner);
  }

  totalCredit: number = 0;
  totalDebit: number = 0;
  totalCreditDebitCalculate(): number {
    this.totalCredit = 0;
    this.totalDebit = 0;
    for (var i = 0; i < this.EditingCDNote.listShipmentSurcharge.length; i++) {
      const c = this.EditingCDNote.listShipmentSurcharge[i];
      if (!c["isRemaining"]) {
        if (c.type == "BUY" || c.type == "LOGISTIC" || (c.type == "OBH" && this.EditingCDNote.partnerId == c.receiverId)) {
          // calculate total credit
          this.totalCredit += (c.total * c.exchangeRate);
        }
        if (c.type == "SELL" || (c.type == "OBH" && this.EditingCDNote.partnerId == c.payerId)) {
          //calculate total debit 
          this.totalDebit += (c.total * c.exchangeRate);
        }
      }

    }
    return this.totalDebit - this.totalCredit;

  }

}
