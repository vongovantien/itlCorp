import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import filter from 'lodash/filter';
import map from 'lodash/map';
import concat from 'lodash/concat'
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { ExtendData } from '../../../extend-data';
import { AcctSOA } from 'src/app/shared/models/document/acctSoa.model';

declare var $: any;
// import * as $ from 'jquery';
import { NgForm } from '@angular/forms';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
@Component({
  selector: 'app-credit-and-debit-note-addnew',
  templateUrl: './credit-and-debit-note-addnew.component.html',
  styleUrls: ['./credit-and-debit-note-addnew.component.scss']
})
export class CreditAndDebitNoteAddnewComponent implements OnInit {

  CDNoteWorking: AcctSOA = new AcctSOA();
  isDisplayAddSoaForm:boolean = true;
  noteTypes = [
    { text: 'CREDIT', id: 'CREDIT' },
    { text: 'DEBIT', id: 'DEBIT' },
    { text: 'INVOICE', id: 'INVOICE' }
  ];
  listSubjectPartner: any[] = []; // data for combobox Subject to Partner 
  listChargeOfPartner: any[] = []; // charges list group by housebill when choose a partner 
  listRemainingCharges: any[] = []; 
  constListChargeOfPartner: any[] = [];
  @Output() addNewRemainingCharges = new EventEmitter<any>();
  @Output() currentPartnerIdEmit = new EventEmitter<string>();

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
  async getListCharges(partnerId: string) {
    this.listChargeOfPartner = await this.baseServices.getAsync(this.api_menu.Documentation.CsShipmentSurcharge.getChargesByPartner + "?JobId=" + ExtendData.currentJobID + "&partnerID=" + partnerId);
    this.CDNoteWorking.listShipmentSurcharge = [];
    this.listChargeOfPartner = map(this.listChargeOfPartner,function(o){
      for(var i = 0; i< o.listCharges.length;i++){
        o.listCharges[i].isRemaining = false;
      }
      return o;
    });
    this.setChargesForCDNote();
    this.totalCreditDebitCalculate();
    this.addNewRemainingCharges.emit(this.listChargeOfPartner);
    this.currentPartnerIdEmit.emit(partnerId);
    setTimeout(() => {
      this.checkSttAllNode();
    }, 100);
    

  }


  setChargesForCDNote(){
    this.CDNoteWorking.listShipmentSurcharge = [];
    this.listChargeOfPartner.forEach(element => { 
      this.CDNoteWorking.listShipmentSurcharge = concat(this.CDNoteWorking.listShipmentSurcharge,element.listCharges);
    });
  }

  SearchPartner(key_search: string) {   

  }

  async CreateCDNote(form:NgForm) {
    if(form.submitted){
      var errors = $('#add-credit-debit-note-modal').find('div.has-danger');
      if (errors.length == 0) {
        console.log({ "CURRENT_JOB_ID": ExtendData.currentJobID });
        this.CDNoteWorking.total = this.totalDebit - this.totalCredit;
        this.CDNoteWorking.currencyId = "USD" // in the future , this id must be local currency of each country
        this.CDNoteWorking.listShipmentSurcharge = filter(this.CDNoteWorking.listShipmentSurcharge,function(o:any){
          return !o.isRemaining;
        });
         var res = await this.baseServices.postAsync(this.api_menu.Documentation.AcctSOA.addNew,this.CDNoteWorking);
         if(res.status){
          $('#add-credit-debit-note-modal').modal('hide');
            this.CDNoteWorking = new AcctSOA();
            this.resetAddSOAForm();            
         }
      }     
    }
  }

  resetAddSOAForm(){
    this.isDisplayAddSoaForm = false;
    setTimeout(() => {
      this.isDisplayAddSoaForm = true;
    }, 300);
  }

  
  totalCredit:number=0;
  totalDebit:number=0;
  totalCreditDebitCalculate(){
    this.totalCredit = 0;
    this.totalDebit = 0;
    for(var i = 0; i < this.CDNoteWorking.listShipmentSurcharge.length;i++){
      const c = this.CDNoteWorking.listShipmentSurcharge[i];
      if(c.type=="BUY"|| c.type=="LOGISTIC" || (c.type=="OBH" && this.CDNoteWorking.partnerId==c.receiverId)){
        // calculate total credit
        this.totalCredit += c.total;
      }
      if(c.type=="SELL" || (c.type=="OBH" && this.CDNoteWorking.partnerId==c.payerId)){
        //calculate total debit 
        this.totalDebit += c.total;
      }
    }

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
    this.checkSttAllNode();
  }


  selectSingleCharge(event: any, indexCharge: number) {
    const currentStt = event.target.checked;
    var groupCheck = $(event.target).closest('tbody').find('input.group-charge-hb-select').first();
    var charges = $(event.target).closest('tbody').find('input.single-charge-select');
    var allcheck = true;
    for (var i = 0; i < charges.length; i++) {   
      if($(charges[i]).prop('checked')!=true){
        allcheck = false;
      }     
    }
    $(groupCheck).prop('checked',allcheck?true:false);     
    this.checkSttAllNode();
  }

  checkSttAllNode(){
    var allNodes = $('#add-credit-debit-note-modal').find('input.group-charge-hb-select');
    var allcheck : boolean = true;
    for(var i =0 ; i< allNodes.length;i++){
      if($(allNodes[i]).prop('checked')!=true){
        allcheck = false;
      }  
    }
    var rootCheck = $('#add-credit-debit-note-modal').find('input.all-charges-select');
    rootCheck.prop("checked",allcheck?true:false);
  }

  selectAllCharges(event:any){
     const currentStt =  event.target.checked;
     var nodes = $(event.target).closest('table').find('input').attr('type',"checkbox");
    for(var i = 0 ; i < nodes.length;i++){
      $(nodes[i]).prop("checked",currentStt?true:false);
    }
  }

  removeSelectedCharges(){  
        const chargesElements = $('.single-charge-select');       
        for(var i = 0; i < chargesElements.length;i ++){
          const selected : boolean = $(chargesElements[i]).prop("checked");
          if(selected){
            const indexSingle = parseInt($(chargesElements[i]).attr("data-id"));     
            var parentElement = $(chargesElements[i]).closest('tbody').find('input.group-charge-hb-select')[0];

            const indexParent = parseInt($(parentElement).attr("data-id"));
            $(parentElement).prop("checked",false);
            this.listChargeOfPartner[indexParent].listCharges[indexSingle].isRemaining = true;
          }          
        }
        this.setChargesForCDNote()
        this.checkSttAllNode();
        this.addNewRemainingCharges.emit(this.listChargeOfPartner);
  }

  checkToDisplay(item:any){
    var s = item.listCharges.map((x:any)=>x.isRemaining).indexOf(false)  !=-1;
    return s;
  }





}
