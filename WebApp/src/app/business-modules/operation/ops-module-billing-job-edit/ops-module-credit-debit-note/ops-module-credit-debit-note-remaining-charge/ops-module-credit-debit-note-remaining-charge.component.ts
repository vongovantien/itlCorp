import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { API_MENU } from 'src/constants/api-menu.const';
import { BaseService } from 'src/services-base/base.service';
import cloneDeep from 'lodash/cloneDeep';
declare var $: any;

@Component({
  selector: 'app-ops-module-credit-debit-note-remaining-charge',
  templateUrl: './ops-module-credit-debit-note-remaining-charge.component.html',
  styleUrls: ['./ops-module-credit-debit-note-remaining-charge.component.scss']
})
export class OpsModuleCreditDebitNoteRemainingChargeComponent implements OnInit {
  STORAGE_DATA: any = null;
  listChargeOfPartner:any[] = []
  partnerId:string = null;
  constructor(
    private baseServices: BaseService,
    private api_menu: API_MENU
  ) { }

  ngOnInit() {
    this.baseServices.dataStorage.subscribe(data => {
      this.STORAGE_DATA = data;
      if (this.STORAGE_DATA.listChargeOfPartner !== undefined) {
        this.listChargeOfPartner = cloneDeep(this.STORAGE_DATA.listChargeOfPartner);
      }

      if (this.STORAGE_DATA.currentPartnerId !== undefined) {
        this.partnerId = cloneDeep(this.STORAGE_DATA.currentPartnerId );
      }
    });
  }

  addCharges(){
    const chargesElements = $('.single-charge-select');       
    for(var i = 0; i < chargesElements.length;i ++){
      const selected : boolean = $(chargesElements[i]).prop("checked");
      if(selected){
        const indexSingle = parseInt($(chargesElements[i]).attr("data-id"));     
        var parentElement = $(chargesElements[i]).closest('tbody').find('input.group-charge-hb-select')[0];
        const indexParent =  0; //parseInt($(parentElement).attr("data-id"));
        $(parentElement).prop("checked",false);
        this.listChargeOfPartner[indexParent].listCharges[indexSingle].isRemaining = false;
      }          
    }
    this.baseServices.setData("listChargeOfPartner", this.listChargeOfPartner);

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


  checkToDisplay(item:any){
    var s = item.listCharges.map((x:any)=>x.isRemaining).indexOf(true)  !=-1;
    return s;
  }


}
