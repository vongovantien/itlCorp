import { Component, OnInit, ViewChild, Output, EventEmitter, Input } from '@angular/core';
import { filter, map, findIndex, find } from 'lodash';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { ExtendData } from '../../../extend-data';
import { AcctSOA } from 'src/app/shared/models/document/acctSoa.model';

// declare var $: any;
import * as $ from 'jquery';
@Component({
  selector: 'app-credit-and-debit-note-addnew',
  templateUrl: './credit-and-debit-note-addnew.component.html',
  styleUrls: ['./credit-and-debit-note-addnew.component.scss']
})
export class CreditAndDebitNoteAddnewComponent implements OnInit {

  CDNoteWorking: AcctSOA = new AcctSOA();
  noteTypes = [
    { text: 'CREDIT', id: 'CREDIT' },
    { text: 'DEBIT', id: 'DEBIT' },
    { text: 'INVOICE', id: 'INVOICE' }
  ];
  listSubjectPartner: any[] = []; // data for combobox Subject to Partner 
  listChargeOfPartner: any[] = []; // charges list group by housebill when choose a partner 


  constructor(
    private baseServices: BaseService,
    private api_menu: API_MENU
  ) { }

  ngOnInit() {
    this.getListSubjectPartner();
  }

  getListSubjectPartner() {
    this.baseServices.get(this.api_menu.Documentation.CsShipmentSurcharge.getPartnerByJobId + "?JobId=" + ExtendData.currentJobID).subscribe((data: any[]) => {
      this.listSubjectPartner = data;
      console.log(this.listSubjectPartner);
    });
  }

  async getListCharges(partnerId: String) {
    this.listChargeOfPartner = await this.baseServices.getAsync(this.api_menu.Documentation.CsShipmentSurcharge.getChargesByPartner + "?JobId=" + ExtendData.currentJobID + "&partnerID=" + partnerId);
    console.log(this.listChargeOfPartner);
  }

  SearchPartner(key_search: string) {
    

  }

  CreateCDNote() {
    console.log({ "CURRENT_JOB_ID": ExtendData.currentJobID });
  }





  /**
    * ng2-select
    */
  public items: Array<string> = ['Option 1', 'Option 2', 'Option 3', 'Option 4',
    'Option 5', 'Option 6', 'Option 7', 'Option 8', 'Option 9', 'Option 10',];

  private value: any = {};
  private _disabledV: string = '0';
  public disabled: boolean = false;

  private get disabledV(): string {
    return this._disabledV;
  }

  private set disabledV(value: string) {
    this._disabledV = value;
    this.disabled = this._disabledV === '1';
  }

  public selected(value: any): void {
    console.log('Selected value is: ', value);
  }

  public removed(value: any): void {
    console.log('Removed value is: ', value);
  }

  public typed(value: any): void {
    console.log('New search input: ', value);
  }

  public refreshValue(value: any): void {
    this.value = value;
  }



  selectAllChargeInGroup(event: any, indexGroup: number) {
    const isSelecAll = event.target.checked;
    var charges = $(event.target).closest('tbody').find('input.single-charge-select')
    for (var i = 0; i < charges.length; i++) {
      $(charges[i]).prop('checked', isSelecAll ? true : false);
    }
  }


  selectSingleCharge(event: any, indexCharge: number) {
    var groupCheck = $(event.target).closest('tbody').find('input.group-charge-hb-select').first();
    var charges = $(event.target).closest('tbody').find('input.single-charge-select');
    var allcheck = true;
    for (var i = 0; i < charges.length; i++) {   
      if($(charges[i]).prop('checked')!=true){
        allcheck = false;
      }     
    }
    $(groupCheck).prop('checked',allcheck?true:false); 
  }

  selectAllCharges(event:any){
     var groups = $(event.target).closest('table').find('input.group-charge-hb-select');
     console.log(groups);
     for(var i=0;i<groups.length;i++){
       $(groups[i]).prop('checked',event.target.checked?true:false);
     }
  }




}
