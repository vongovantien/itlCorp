import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import filter from 'lodash/filter';
import map from 'lodash/map';
import concat from 'lodash/concat'
import cloneDeep from 'lodash/cloneDeep'
import groupBy from 'lodash/groupBy';
import { ExtendData } from '../../../extend-data';
import { AcctSOA } from 'src/app/shared/models/document/acctSoa.model';
import { NgForm } from '@angular/forms';
declare var $: any;

@Component({
  selector: 'app-credit-and-debit-note-edit',
  templateUrl: './credit-and-debit-note-edit.component.html',
  styleUrls: ['./credit-and-debit-note-edit.component.scss']
})
export class CreditAndDebitNoteEditComponent implements OnInit {
  listChargeOfPartner: any[] = []; // charges list group by housebill when choose a partner 
  listRemainingCharges: any[] = [];
  constListChargeOfPartner: any[] = [];
  isDisplayAddSoaForm: boolean = true;

  @Output() addNewRemainingCharges = new EventEmitter<any>();
  EditingCDNote: AcctSOA = new AcctSOA();
  @Input() set editingCdNote(cdNote: any) {
    if (cdNote != null) {
      this.EditingCDNote = cdNote.soa;
      this.EditingCDNote.partnerName = cdNote.partnerNameEn;
      this.EditingCDNote.listShipmentSurcharge = cdNote.listSurcharges;
      console.log(this.EditingCDNote);
      this.getListCharges(this.EditingCDNote.partnerId);
    }
  }

  @Input() set chargesFromRemaining(listCharges: any) {
    this.listChargeOfPartner = cloneDeep(listCharges);
    this.constListChargeOfPartner = cloneDeep(listCharges);
  }

  async getListCharges(partnerId: String) {
    this.listChargeOfPartner = await this.baseServices.getAsync(this.api_menu.Documentation.CsShipmentSurcharge.getChargesByPartner + "?JobId=" + ExtendData.currentJobID + "&partnerID=" + partnerId);

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



    // console.log(this.EditingCDNote.listShipmentSurcharge);
    // console.log(this.listChargeOfPartner);
    this.totalCreditDebitCalculate();
    this.constListChargeOfPartner = cloneDeep(this.listChargeOfPartner);
    this.addNewRemainingCharges.emit(this.constListChargeOfPartner);
    console.log(this.listChargeOfPartner);
  }



  constructor(
    private baseServices: BaseService,
    private api_menu: API_MENU
  ) { }

  ngOnInit() {
  }

  viewDetailCDNote() {
    $('#edit-credit-debit-note-modal').modal('hide');
    $('#detail-credit-debit-note-modal').modal('show');
  }








  checkToDisplay(item: any) {
    var s = item.listCharges.map((x: any) => x.isRemaining).indexOf(false) != -1;
    return s;
  }


  selectSingleCharge(event: any, indexCharge: number) {
    var groupCheck = $(event.target).closest('tbody').find('input.group-charge-hb-select').first();
    var charges = $(event.target).closest('tbody').find('input.single-charge-select');
    var allcheck = true;
    for (var i = 0; i < charges.length; i++) {
      if ($(charges[i]).prop('checked') != true) {
        allcheck = false;
      }
    }
    $(groupCheck).prop('checked', allcheck ? true : false);
    this.checkSttAllNode();
  }


  checkSttAllNode() {
    var allNodes = $('#edit-credit-debit-note-modal').find('input.group-charge-hb-select');
    var allcheck: boolean = true;
    for (var i = 0; i < allNodes.length; i++) {
      if ($(allNodes[i]).prop('checked') != true) {
        allcheck = false;
      }
    }
    var rootCheck = $('#edit-credit-debit-note-modal').find('input.all-charges-select');
    rootCheck.prop("checked", allcheck ? true : false);
  }

  selectAllCharges(event: any) {
    const currentStt = event.target.checked;
    var nodes = $(event.target).closest('table').find('input').attr('type', "checkbox");
    for (var i = 0; i < nodes.length; i++) {
      $(nodes[i]).prop("checked", currentStt ? true : false);
    }
  }

  selectAllChargeInGroup(event: any, indexGroup: number) {
    const isSelecAll = event.target.checked;
    var charges = $(event.target).closest('tbody').find('input.single-charge-select')
    for (var i = 0; i < charges.length; i++) {
      $(charges[i]).prop('checked', isSelecAll ? true : false);
    }
    this.checkSttAllNode();
  }



  removeSelectedCharges() {
    const chargesElements = $('.single-charge-select');
    for (var i = 0; i < chargesElements.length; i++) {
      const selected: boolean = $(chargesElements[i]).prop("checked");
      if (selected) {
        const indexSingle = parseInt($(chargesElements[i]).attr("data-id"));
        var parentElement = $(chargesElements[i]).closest('tbody').find('input.group-charge-hb-select')[0];

        const indexParent = parseInt($(parentElement).attr("data-id"));
        $(parentElement).prop("checked", false);

        this.listChargeOfPartner[indexParent].listCharges[indexSingle].isRemaining = true;
        const hbId = this.listChargeOfPartner[indexParent].id;
        const chargeId = this.listChargeOfPartner[indexParent].listCharges[indexSingle].id;
        const constParentInx = this.constListChargeOfPartner.map(x => x.id).indexOf(hbId);
        const constChargeInx = this.constListChargeOfPartner[constParentInx].listCharges.map((x: any) => x.id).indexOf(chargeId);
        this.constListChargeOfPartner[constParentInx].listCharges[constChargeInx].isRemaining = true;

      }
    }

    this.setChargesForCDNote()
    this.checkSttAllNode();
    this.listChargeOfPartner = this.constListChargeOfPartner;
    this.addNewRemainingCharges.emit(this.constListChargeOfPartner);
    this.totalCreditDebitCalculate();
    console.log(this.EditingCDNote);
  }

  setChargesForCDNote() {
    this.EditingCDNote.listShipmentSurcharge = [];
    this.listChargeOfPartner.forEach(element => {
      this.EditingCDNote.listShipmentSurcharge = concat(this.EditingCDNote.listShipmentSurcharge, element.listCharges);
    });
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


  @Output() updated = new EventEmitter<boolean>();
  async UpdateCDNote(form: NgForm) {
    this.setChargesForCDNote()
    this.EditingCDNote.total = this.totalDebit - this.totalCredit;
    this.EditingCDNote.currencyId = "USD" // in the future , this id must be local currency of each country
    this.EditingCDNote.listShipmentSurcharge = filter(this.EditingCDNote.listShipmentSurcharge, function (o: any) {
      return !o.isRemaining;
    });
    console.log(this.EditingCDNote);
    var res = await this.baseServices.putAsync(this.api_menu.Documentation.AcctSOA.update, this.EditingCDNote);
    if (res.status) {
      $('#edit-credit-debit-note-modal').modal('hide');
      $('#detail-credit-debit-note-modal').modal('show');
      this.updated.emit(true);
      setTimeout(() => {
        this.updated.emit(false);
      }, 1000);
      this.EditingCDNote = new AcctSOA();
    }
  }

  resetAddSOAForm() {
    this.isDisplayAddSoaForm = false;
    setTimeout(() => {
      this.isDisplayAddSoaForm = true;
    }, 300);
  }


  
  SearchCharge(search_key: string) {
    // listChargeOfPartner
    this.listChargeOfPartner = cloneDeep(this.constListChargeOfPartner);
    search_key = search_key.trim().toLowerCase();
    var listBranch: any[] = [];
    this.listChargeOfPartner = filter(cloneDeep(this.constListChargeOfPartner), function (x: any) {
      var root = false;
      var branch = false;
      if (x.hwbno == null ? "" : x.hwbno.toLowerCase().includes(search_key)) {
        root = true;
      }
      var listCharges: any[] = [];
      for (var i = 0; i < x.listCharges.length; i++) {
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

    for (var i = 0; i < this.listChargeOfPartner.length; i++) {
      for (var k = 0; k < listBranch.length; k++) {
        if (this.listChargeOfPartner[i].id === listBranch[k].hbId) {
          this.listChargeOfPartner[i].listCharges = listBranch[k].list;
        }
      }
    }

    console.log(this.listChargeOfPartner);

  }

}
